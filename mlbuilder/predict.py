import argparse
import json
import os
import pickle
import numpy as np
import pandas as pd
from xgboost import XGBClassifier

from feature_engineering import build_features, FEATURE_COLS


LABEL_NAMES = {0: "Фон", 1: "Умеренная", 2: "Высокая", 3: "Критическая"}
LABEL_COLORS = {0: "green", 1: "yellow", 2: "orange", 3: "red"}


def predict(events_path: str, model_path: str, iso_path: str, output_path: str):
    print("GeoSense mlbuilder — классификация событий")
    print("=" * 50)

    df_raw = _load_events(events_path)
    print(f"Загружено событий: {len(df_raw)}")

    df = build_features(df_raw)
    X = df[FEATURE_COLS].fillna(0).values

    xgb = XGBClassifier()
    xgb.load_model(model_path)
    print(f"XGBoost модель загружена: {model_path}")

    iso = None
    if iso_path and os.path.exists(iso_path):
        with open(iso_path, "rb") as f:
            iso = pickle.load(f)
        print(f"Isolation Forest загружен: {iso_path}")

    labels = xgb.predict(X).tolist()
    probs = xgb.predict_proba(X).tolist()

    # Уровень 1: точечные аномалии (быстрые процессы)
    anomaly_scores = None
    if iso is not None:
        anomaly_scores = iso.score_samples(X).tolist()

    # Уровень 2: протяжённые аномалии (медленные процессы)
    slow_anomaly_flags = _detect_slow_anomalies(df_raw, window_hours=6, zscore_threshold=2.0)
    slow_anomaly_count = int(slow_anomaly_flags.sum())
    if slow_anomaly_count > 0:
        print(f"Протяжённых аномалий (медленные процессы): {slow_anomaly_count} событий")

    results = []
    for i in range(len(df_raw)):
        row = df_raw.iloc[i]
        entry = {
            "index": i,
            "obj": int(row.get("obj", 1)),
            "idat": int(row["idat"]),
            "itim": int(row["itim"]),
            "x": float(row["x"]),
            "y": float(row["y"]),
            "z": float(row["z"]),
            "e": float(row["e"]),
            "label": labels[i],
            "label_name": LABEL_NAMES[labels[i]],
            "label_color": LABEL_COLORS[labels[i]],
            "probabilities": {
                "class_0": round(probs[i][0], 4),
                "class_1": round(probs[i][1], 4),
                "class_2": round(probs[i][2], 4),
                "class_3": round(probs[i][3], 4),
            },
            "is_slow_anomaly": bool(slow_anomaly_flags.iloc[i]),
        }
        if anomaly_scores is not None:
            entry["anomaly_score"] = round(anomaly_scores[i], 4)
            entry["is_anomaly"] = bool(anomaly_scores[i] < np.percentile(anomaly_scores, 5))
        else:
            entry["is_anomaly"] = False

        results.append(entry)

    summary = {
        "total_events": len(results),
        "class_counts": {
            LABEL_NAMES[i]: int(np.sum(np.array(labels) == i))
            for i in range(4)
        },
        "anomaly_count": int(sum(1 for r in results if r.get("is_anomaly", False))),
        "slow_anomaly_count": slow_anomaly_count,
    }

    output = {"summary": summary, "events": results}

    os.makedirs(os.path.dirname(output_path) or ".", exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print(f"\nРезультаты сохранены: {output_path}")
    print(f"Всего событий: {summary['total_events']}")
    for name, count in summary["class_counts"].items():
        print(f"  {name}: {count}")
    if summary["anomaly_count"] > 0:
        print(f"Точечных аномалий (IF): {summary['anomaly_count']}")
    if slow_anomaly_count > 0:
        print(f"Протяжённых аномалий: {slow_anomaly_count}")


def _detect_slow_anomalies(df_raw: pd.DataFrame,
                            window_hours: int = 6,
                            zscore_threshold: float = 2.0) -> pd.Series:
    """
    Детектирование протяжённых аномалий — медленных геофизических процессов.

    Алгоритм:
    1. Группируем события в временные окна (по умолчанию 6 часов).
    2. Для каждого окна вычисляем агрегаты: число событий, суммарная энергия.
    3. Окно считается аномальным, если его показатели отклоняются от среднего
       более чем на zscore_threshold стандартных отклонений (z-score > threshold).
    4. Все события внутри аномального окна помечаются как is_slow_anomaly=True.

    Физический смысл: медленные процессы (накопление напряжений, активизация зоны)
    проявляются не отдельными событиями, а нарастанием активности за часы/сутки.
    """
    df = df_raw.copy()
    df['_orig_idx'] = np.arange(len(df))

    # Извлекаем час из itim (формат HHMMSS)
    df['_hour'] = (df['itim'].astype(int) // 10000)

    # Окно идентифицируется по idat + номер окна внутри суток
    df['_window_id'] = (
        df['idat'].astype(str) + '_' +
        (df['_hour'] // window_hours).astype(str)
    )

    # Агрегаты по окну
    window_stats = df.groupby('_window_id').agg(
        count=('e', 'count'),
        total_energy=('e', 'sum'),
    ).reset_index()

    # Z-score для числа событий и суммарной энергии
    def zscore_col(series):
        std = series.std()
        if std == 0 or len(series) < 3:
            return pd.Series(np.zeros(len(series)), index=series.index)
        return (series - series.mean()) / std

    window_stats['z_count'] = zscore_col(window_stats['count'])
    window_stats['z_energy'] = zscore_col(window_stats['total_energy'])

    window_stats['is_slow_anomaly'] = (
        (window_stats['z_count'] > zscore_threshold) |
        (window_stats['z_energy'] > zscore_threshold)
    )

    # Переносим метку обратно на каждое событие
    anomalous_windows = set(
        window_stats.loc[window_stats['is_slow_anomaly'], '_window_id']
    )
    result = df['_window_id'].isin(anomalous_windows)
    result.index = df['_orig_idx']

    return result.sort_index()


def _load_events(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)

    required = ["idat", "itim", "x", "y", "z", "e"]
    missing = [c for c in required if c not in df.columns]
    if missing:
        raise ValueError(f"В файле отсутствуют обязательные колонки: {missing}")

    for col in ["ampl", "magn", "proc", "np_actual", "rq_min", "rq_max"]:
        if col not in df.columns:
            df[col] = np.nan

    if "obj" not in df.columns:
        df["obj"] = 1

    return df


def parse_args():
    parser = argparse.ArgumentParser(
        description="GeoSense — классификация сейсмических событий"
    )
    parser.add_argument("--events", required=True,
                        help="Путь к CSV с событиями (peleng.events формат)")
    parser.add_argument("--model", default="output/xgb_model.json",
                        help="Путь к обученной XGBoost модели")
    parser.add_argument("--iso_model", default="output/iso_forest.pkl",
                        help="Путь к Isolation Forest модели")
    parser.add_argument("--out", default="output/predictions.json",
                        help="Путь для сохранения результатов")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    predict(args.events, args.model, args.iso_model, args.out)
