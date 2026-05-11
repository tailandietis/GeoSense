import argparse
import sys
import os

import numpy as np
import pandas as pd

from settings.settings_service import load_settings
from emulator import generate_events
from feature_engineering import build_features
from train import train_all
from predict import predict


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="GeoSense ML Builder — обучение и классификация сейсмических событий"
    )
    parser.add_argument("--mode", choices=["train", "predict"], default="train",
                        help="Режим: train (обучение) или predict (классификация)")
    parser.add_argument("--settings", default="settings.json",
                        help="Путь к JSON-файлу настроек")
    parser.add_argument("--output", default=None,
                        help="Директория для результатов")
    parser.add_argument("--thesis_figures", default=None,
                        help="Директория для графиков ПЗ (только в режиме train)")
    parser.add_argument("--real_events", default=None,
                        help="[train] Путь к CSV реальных событий для смешанного обучения")
    # Аргументы predict режима
    parser.add_argument("--events", default=None,
                        help="[predict] Путь к CSV с событиями")
    parser.add_argument("--model", default="output/xgb_model.json",
                        help="[predict] Путь к XGBoost модели")
    parser.add_argument("--iso_model", default="output/iso_forest.pkl",
                        help="[predict] Путь к Isolation Forest модели")
    parser.add_argument("--out", default="output/predictions.json",
                        help="[predict] Путь для сохранения predictions.json")
    return parser.parse_args()


def main():
    args = parse_args()

    if args.mode == "predict":
        if not args.events:
            print("Ошибка: для режима predict нужен --events <path>")
            sys.exit(1)
        predict(args.events, args.model, args.iso_model, args.out)
        return

    # Режим train
    print("GeoSense mlbuilder v1.1")
    print("=" * 50)

    settings = load_settings(args.settings)
    if args.output:
        settings.output_dir = args.output
    if args.thesis_figures:
        settings.thesis_figures_dir = args.thesis_figures

    print(f"Настройки:          {args.settings}")
    print(f"Директория вывода:  {settings.output_dir}")
    if settings.thesis_figures_dir:
        print(f"Графики для ПЗ:     {settings.thesis_figures_dir}")

    total = (settings.emulator.n_background + settings.emulator.n_level1 +
             settings.emulator.n_level2 + settings.emulator.n_level3)
    print(f"\nГенерация синтетических событий: {total} шт.")

    df_raw = generate_events(settings, random_seed=settings.emulator.random_seed)
    print(f"Сгенерировано синтетических событий: {len(df_raw)}")

    if args.real_events and os.path.exists(args.real_events):
        df_real = _load_and_label_real(args.real_events)
        print(f"Реальных событий загружено и размечено: {len(df_real)}")
        df_raw = pd.concat([df_raw, df_real], ignore_index=True).sample(
            frac=1, random_state=42).reset_index(drop=True)
        print(f"Итого событий в обучающей выборке: {len(df_raw)}")
    else:
        print("Реальные данные не переданы — обучение только на синтетике")

    print("\nРасчёт признаков...")
    df = build_features(df_raw)
    print(f"Признаков: {len([c for c in df.columns if c != 'label'])}, событий: {len(df)}")

    os.makedirs(settings.output_dir, exist_ok=True)
    dataset_path = os.path.join(settings.output_dir, "dataset.csv")
    df.to_csv(dataset_path, index=False)

    metrics = train_all(df, settings)

    print("\n" + "=" * 50)
    print("ИТОГОВЫЕ МЕТРИКИ")
    print("=" * 50)
    xgb = metrics["xgboost"]
    iso = metrics["isolation_forest"]
    bl = metrics.get("baseline_comparison", {})

    print(f"XGBoost:          Accuracy={xgb['accuracy']:.4f}  F1={xgb['f1_macro']:.4f}  ROC-AUC={xgb['roc_auc_macro']:.4f}")
    print(f"Isolation Forest: Accuracy={iso['accuracy_binary']:.4f}  F1={iso['f1_anomaly']:.4f}")
    if bl:
        print(f"\nBaseline сравнение (F1-macro):")
        print(f"  Порог энергии (GeoDa): {bl['energy_threshold']['f1_macro']:.4f}")
        print(f"  b-value:               {bl['b_value']['f1_macro']:.4f}")
        print(f"  Energy Index:          {bl['energy_index']['f1_macro']:.4f}")
        print(f"  XGBoost:               {xgb['f1_macro']:.4f}")
    print("\nГотово.")


def _load_and_label_real(path: str) -> pd.DataFrame:
    """
    Загружает реальные события и размечает их по перцентилям энергии.
    Разбивка 60/20/12/8% соответствует физически обоснованному
    соотношению классов опасности в горном массиве.
    """
    df = pd.read_csv(path)
    for col in ["ampl", "magn", "proc", "np_actual", "rq_min", "rq_max"]:
        if col not in df.columns:
            df[col] = np.nan
    if "obj" not in df.columns:
        df["obj"] = 1

    df = df[df["e"] > 0].dropna(subset=["e"]).copy()

    log_e = np.log(df["e"].values)
    q60 = np.percentile(log_e, 60)
    q80 = np.percentile(log_e, 80)
    q92 = np.percentile(log_e, 92)

    labels = np.zeros(len(df), dtype=int)
    labels[(log_e >= q60) & (log_e < q80)] = 1
    labels[(log_e >= q80) & (log_e < q92)] = 2
    labels[log_e >= q92] = 3
    df["label"] = labels

    return df


if __name__ == "__main__":
    main()
