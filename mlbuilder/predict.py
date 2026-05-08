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

    anomaly_scores = None
    if iso is not None:
        anomaly_scores = iso.score_samples(X).tolist()

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
        }
        if anomaly_scores is not None:
            entry["anomaly_score"] = round(anomaly_scores[i], 4)
            entry["is_anomaly"] = anomaly_scores[i] < np.percentile(anomaly_scores, 5)

        results.append(entry)

    summary = {
        "total_events": len(results),
        "class_counts": {
            LABEL_NAMES[i]: int(np.sum(np.array(labels) == i))
            for i in range(4)
        },
        "anomaly_count": int(sum(1 for r in results if r.get("is_anomaly", False))),
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
        print(f"Аномалий (IF): {summary['anomaly_count']}")


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
