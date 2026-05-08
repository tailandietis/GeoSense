import json
from settings.app_settings import AppSettings, EnergyThresholds, EmulatorSettings


def load_settings(path: str) -> AppSettings:
    try:
        with open(path, encoding="utf-8") as f:
            data = json.load(f)
    except FileNotFoundError:
        return AppSettings()

    thresholds_data = data.get("energy_thresholds", {})
    thresholds = EnergyThresholds(
        background=thresholds_data.get("background", 150.0),
        level1=thresholds_data.get("level1", 1000.0),
        level2=thresholds_data.get("level2", 5000.0),
        level3=thresholds_data.get("level3", 15000.0),
    )

    emulator_data = data.get("emulator", {})
    emulator = EmulatorSettings(
        n_background=emulator_data.get("n_background", 3000),
        n_level1=emulator_data.get("n_level1", 800),
        n_level2=emulator_data.get("n_level2", 150),
        n_level3=emulator_data.get("n_level3", 50),
        random_seed=emulator_data.get("random_seed", 42),
    )

    return AppSettings(
        output_dir=data.get("output_dir", "output"),
        thesis_figures_dir=data.get("thesis_figures_dir", ""),
        energy_thresholds=thresholds,
        emulator=emulator,
        test_size=data.get("test_size", 0.2),
        n_estimators_xgb=data.get("n_estimators_xgb", 200),
        max_depth_xgb=data.get("max_depth_xgb", 5),
        n_estimators_if=data.get("n_estimators_if", 100),
        contamination_if=data.get("contamination_if", 0.05),
    )
