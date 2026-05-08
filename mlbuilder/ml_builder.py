import argparse
import sys
import os

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
    print(f"Сгенерировано событий: {len(df_raw)}")

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


if __name__ == "__main__":
    main()
