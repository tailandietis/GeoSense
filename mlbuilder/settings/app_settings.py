from dataclasses import dataclass, field


@dataclass
class EnergyThresholds:
    background: float = 150.0
    level1: float = 1000.0
    level2: float = 5000.0
    level3: float = 15000.0


@dataclass
class EmulatorSettings:
    n_background: int = 3000
    n_level1: int = 800
    n_level2: int = 150
    n_level3: int = 50
    x_range: tuple = (50, 350)
    y_range: tuple = (50, 350)
    z_range: tuple = (-400, -50)
    random_seed: int = 42


@dataclass
class AppSettings:
    output_dir: str = "output"
    thesis_figures_dir: str = ""
    energy_thresholds: EnergyThresholds = field(default_factory=EnergyThresholds)
    emulator: EmulatorSettings = field(default_factory=EmulatorSettings)
    test_size: float = 0.2
    n_estimators_xgb: int = 200
    max_depth_xgb: int = 5
    n_estimators_if: int = 100
    contamination_if: float = 0.05
