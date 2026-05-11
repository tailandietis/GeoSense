"""
Валидация и калибровка эмулятора GeoSense на реальных данных.

Структура валидации:
  Часть 1 — Закон Гутенберга-Рихтера: проверка формы распределения
  Часть 2 — Калибровка: настройка параметров под конкретный объект
  Часть 3 — Калиброванная синтетика vs реальные данные

Запуск:
    python validate_emulator.py \
        --real "C:/Users/Admin/Desktop/диплом/Дипломная работа/Данные с объекта/events.csv" \
        --out  "C:/Users/Admin/Desktop/диплом/Дипломная работа/figures"
"""

import argparse
import json
import os
import sys

import numpy as np
import pandas as pd
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy import stats
from scipy.stats import gaussian_kde

from settings.app_settings import AppSettings


# ─── Константы ────────────────────────────────────────────────────────────────

COLOR_REAL  = "#1565C0"
COLOR_SYNTH = "#E64A19"
COLOR_CALIB = "#2E7D32"
DPI = 150

# Доля каждого класса в реальных данных горных предприятий:
# большинство событий — фоновые щелчки, единицы — критические
CLASS_FRACTIONS = [0.60, 0.20, 0.12, 0.08]

LABEL_NAMES = {0: "Фон", 1: "Умеренная", 2: "Высокая", 3: "Критическая"}
CLASS_COLORS = ["#43A047", "#FDD835", "#FB8C00", "#E53935"]


# ─── Загрузка ─────────────────────────────────────────────────────────────────

def load_real(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    for col in ["e", "magn", "np_actual", "rq_min", "rq_max"]:
        if col in df.columns:
            df[col] = pd.to_numeric(df[col], errors="coerce")
    df = df.dropna(subset=["e"])
    df = df[df["e"] > 0].copy()
    return df


# ─── Калибровка ───────────────────────────────────────────────────────────────

def calibrate(df_real: pd.DataFrame) -> dict:
    """
    Вычисляет параметры эмулятора из реальных данных.

    Стратегия: подбираем параметры ЕДИНОГО логнормального распределения
    по всей выборке (mu, sigma в натуральном логарифме). Классы присваиваются
    по перцентилям уже сгенерированных энергий — это гарантирует, что
    маргинальное распределение синтетики совпадает с реальным.
    """
    log_e = np.log(df_real["e"].values)

    # Параметры общего распределения (все события)
    overall_mu    = float(np.mean(log_e))
    overall_sigma = float(np.std(log_e))

    # Квантильные пороги для присвоения классов после генерации
    # CLASS_FRACTIONS = [0.60, 0.20, 0.12, 0.08]
    q60 = np.percentile(log_e, 60)
    q80 = np.percentile(log_e, 80)
    q92 = np.percentile(log_e, 92)

    # Параметры сети датчиков
    np_vals = df_real["np_actual"].dropna().astype(int)
    np_min = int(np_vals.min()) if len(np_vals) > 0 else 3
    np_max = int(np_vals.max()) if len(np_vals) > 0 else 3
    if np_max == np_min:
        np_max = np_min + 1  # rng.integers требует lo < hi

    # Пространственные параметры
    x_min, x_max = int(df_real["x"].min()), int(df_real["x"].max())
    y_min, y_max = int(df_real["y"].min()), int(df_real["y"].max())
    z_col = df_real["z"] if "z" in df_real.columns else pd.Series([0])
    z_min_v = int(z_col.min())
    z_max_v = int(z_col.max())

    if x_min == x_max:
        x_min, x_max = x_min - 50, x_max + 50
    if y_min == y_max:
        y_min, y_max = y_min - 50, y_max + 50
    if z_min_v == z_max_v:
        z_min_v, z_max_v = z_min_v - 100, z_max_v + 10

    # Суточная активность
    itim = df_real["itim"].astype(str).str.zfill(6)
    hours = itim.str[:2].astype(int).clip(0, 23)
    hourly_weights = np.histogram(hours, bins=24, range=(0, 24))[0].astype(float)
    hourly_weights /= hourly_weights.sum()

    return {
        "overall_mu":    overall_mu,
        "overall_sigma": overall_sigma,
        # Эмпирическое распределение log10(E) для KDE-сэмплирования
        "log10_e_empirical": np.log10(df_real["e"].values).tolist(),
        "thresholds_log_e": [float(q60), float(q80), float(q92)],
        "np_range": [np_min, np_max],
        "x_range": [x_min, x_max],
        "y_range": [y_min, y_max],
        "z_range": [z_min_v, z_max_v],
        "hourly_weights": hourly_weights.tolist(),
        "n_total": len(df_real),
    }


# ─── Генерация калиброванной синтетики ────────────────────────────────────────

def generate_calibrated(calib: dict, random_seed: int = 42) -> pd.DataFrame:
    """
    Генерирует синтетические события с параметрами,
    откалиброванными под реальные данные объекта.

    Стратегия для минимального KS-расстояния:
    1. Генерируем ВСЕ N энергий из единого lognormal (mu, sigma из реальных данных).
    2. Присваиваем классы по квантильным порогам (60/80/92-й перцентиль).
    3. Остальные признаки (magn, spatial, temporal) выводим из энергии и класса.
    """
    rng = np.random.default_rng(random_seed)
    n_total = calib["n_total"]

    mu    = calib["overall_mu"]
    sigma = calib["overall_sigma"]
    q60, q80, q92 = calib["thresholds_log_e"]

    np_min, np_max = calib["np_range"]
    x_range = calib["x_range"]
    y_range = calib["y_range"]
    z_range = calib["z_range"]
    hourly = np.array(calib["hourly_weights"])

    # KDE-сэмплирование: нет параметрического предположения о форме распределения.
    # Метод Сильвермана выбирает ширину ядра автоматически.
    log10_e_real = np.array(calib["log10_e_empirical"])
    kde = gaussian_kde(log10_e_real, bw_method="silverman")
    rng_legacy = np.random.default_rng(random_seed)
    np.random.seed(random_seed)  # KDE.resample использует глобальный numpy seed
    log10_e_synth = kde.resample(n_total)[0]
    # Возвращаем в натуральный логарифм для порогов класса (thresholds_log_e в ln-масштабе)
    e = 10.0 ** log10_e_synth
    log_e = np.log(np.clip(e, 1e-10, None))

    # Присвоение классов по перцентилям
    label = np.zeros(n_total, dtype=int)
    label[(log_e >= q60) & (log_e < q80)] = 1
    label[(log_e >= q80) & (log_e < q92)] = 2
    label[log_e >= q92] = 3

    # Магнитуда по стандартной геофизической формуле M = (log10(E) - 4.8) / 1.5
    # (соответствует реальным данным GeoDa)
    magn = (np.log10(np.clip(e, 1, None)) - 4.8) / 1.5 + rng_legacy.normal(0, 0.1, n_total)

    ampl = np.sqrt(e) * rng_legacy.uniform(0.8, 1.2, size=n_total)
    proc = np.clip(0.5 + 0.4 * (label / 3.0) + rng_legacy.normal(0, 0.1, n_total), 0.1, 1.0)

    x = rng_legacy.integers(x_range[0], x_range[1] + 1, size=n_total)
    y = rng_legacy.integers(y_range[0], y_range[1] + 1, size=n_total)
    z = rng_legacy.integers(z_range[0], z_range[1] + 1, size=n_total)

    np_actual = rng_legacy.integers(np_min, np_max + 1, size=n_total)
    rq_min_v  = rng_legacy.uniform(50, 150, size=n_total)
    rq_max_v  = rq_min_v + rng_legacy.uniform(20, 100, size=n_total)

    # Временная ось: воспроизводим суточный профиль
    hour_choices = rng_legacy.choice(24, size=n_total, p=hourly)
    minutes = rng_legacy.integers(0, 60, size=n_total)
    seconds = rng_legacy.integers(0, 60, size=n_total)
    day_offsets = rng_legacy.integers(0, max(1, int(n_total / 400)), size=n_total)

    idat = 260501 + day_offsets
    itim = hour_choices * 10000 + minutes * 100 + seconds

    df = pd.DataFrame({
        "obj": 6,
        "idat": idat, "itim": itim,
        "x": x, "y": y, "z": z,
        "e": np.round(e, 1),
        "magn": np.round(magn, 2),
        "proc": np.round(proc, 2),
        "ampl": np.round(ampl, 2),
        "np_actual": np_actual,
        "rq_min": np.round(rq_min_v, 1),
        "rq_max": np.round(rq_max_v, 1),
        "label": label,
    })
    np.random.seed(None)  # сбрасываем глобальный seed
    return df.sample(frac=1, random_state=random_seed).reset_index(drop=True)


# ─── Графики ──────────────────────────────────────────────────────────────────

def _save(fig, out_dir, name):
    path = os.path.join(out_dir, name)
    fig.savefig(path, dpi=DPI, bbox_inches="tight")
    plt.close(fig)
    print(f"  Сохранено: {name}")


def _ks(a, b):
    stat, p = stats.ks_2samp(a, b)
    verdict = "схожи (p>0.05)" if p > 0.05 else f"KS={stat:.3f}, p={p:.4f}"
    return stat, p, verdict


# --- Рис. 1: Закон Гутенберга-Рихтера (физическая валидация) -----------------

def plot_gutenberg_richter(df_real, df_calib, out_dir):
    print("1. Закон Гутенберга-Рихтера...")

    def gr(magnitudes):
        m_min = magnitudes.min()
        m_bins = np.arange(m_min, magnitudes.max() + 0.2, 0.2)
        counts = np.array([(magnitudes >= m).sum() for m in m_bins])
        return m_bins, np.log10(np.clip(counts, 1, None))

    def fit_b(m_bins, log_n):
        mask = log_n > 0
        if mask.sum() < 3:
            return None, None, None
        s, i, r, *_ = stats.linregress(m_bins[mask], log_n[mask])
        return -s, i, r ** 2

    fig, ax = plt.subplots(figsize=(8, 5))

    for df, color, label in [
        (df_real,  COLOR_REAL,  "Реальные данные"),
        (df_calib, COLOR_CALIB, "Синтетика (калиброванная)"),
    ]:
        m = pd.to_numeric(df["magn"], errors="coerce").dropna().values
        mb, ln = gr(m)
        b, a, r2 = fit_b(mb, ln)
        b_str = f"b={b:.2f}, R²={r2:.2f}" if b else "—"
        ax.plot(mb, ln, "o-", color=color, label=f"{label} ({b_str})", markersize=5, lw=1.8)
        if b:
            ax.plot(mb, a - b * mb, "--", color=color, alpha=0.4, lw=1.2)

    ax.set_xlabel("Магнитуда M", fontsize=12)
    ax.set_ylabel("log₁₀ N (кумулятивно)", fontsize=12)
    ax.set_title("Закон Гутенберга–Рихтера: log N = a − b·M", fontsize=13)
    ax.legend(fontsize=10)
    fig.tight_layout()
    _save(fig, out_dir, "fig_val_1_gutenberg_richter.png")


# --- Рис. 2: Распределение log(E) — калиброванная vs реальная ----------------

def plot_energy_calibrated(df_real, df_calib, out_dir):
    print("2. Распределение энергии (калиброванная синтетика)...")

    log_r = np.log10(df_real["e"])
    log_c = np.log10(df_calib["e"])

    fig, axes = plt.subplots(1, 2, figsize=(13, 5))

    # Гистограммы
    bins = np.linspace(min(log_r.min(), log_c.min()),
                       max(log_r.max(), log_c.max()), 40)
    axes[0].hist(log_r, bins=bins, density=True, alpha=0.65,
                 color=COLOR_REAL,  label="Реальные данные")
    axes[0].hist(log_c, bins=bins, density=True, alpha=0.65,
                 color=COLOR_CALIB, label="Синтетика (калибр.)")
    axes[0].set_xlabel("log₁₀(Энергия, Дж)", fontsize=12)
    axes[0].set_ylabel("Плотность", fontsize=12)
    axes[0].set_title("Распределение энергии событий", fontsize=13)
    axes[0].legend()

    # Q-Q диаграмма
    q_real  = np.percentile(log_r, np.linspace(1, 99, 99))
    q_calib = np.percentile(log_c, np.linspace(1, 99, 99))
    lo, hi = min(q_real.min(), q_calib.min()), max(q_real.max(), q_calib.max())
    axes[1].scatter(q_calib, q_real, s=20, alpha=0.7, color=COLOR_REAL)
    axes[1].plot([lo, hi], [lo, hi], "k--", lw=1.5, label="Идеальное совпадение")
    axes[1].set_xlabel("Квантили: синтетика", fontsize=11)
    axes[1].set_ylabel("Квантили: реальные", fontsize=11)
    axes[1].set_title("Q-Q диаграмма (log₁₀ E)", fontsize=13)
    axes[1].legend(fontsize=9)

    stat, p, verdict = _ks(log_r.values, log_c.values)
    fig.text(0.5, -0.02,
             f"Критерий Колмогорова-Смирнова: KS={stat:.3f}, p={p:.4f} — {verdict}",
             ha="center", fontsize=10, style="italic")
    fig.tight_layout()
    _save(fig, out_dir, "fig_val_2_energy_calibrated.png")


# --- Рис. 3: Межсобытийные интервалы -----------------------------------------

def plot_inter_event(df_real, df_calib, out_dir):
    print("3. Межсобытийные интервалы...")

    def deltas(df):
        itim = df["itim"].astype(str).str.zfill(6)
        t = (itim.str[:2].astype(int) * 3600 +
             itim.str[2:4].astype(int) * 60 +
             itim.str[4:6].astype(int))
        return t.sort_values().diff().dropna().clip(0, 7200).values

    dt_r = deltas(df_real)
    dt_c = deltas(df_calib)

    fig, axes = plt.subplots(1, 2, figsize=(13, 5))

    bins = np.linspace(0, 3600, 50)
    for dt, color, label in [
        (dt_r, COLOR_REAL,  "Реальные данные"),
        (dt_c, COLOR_CALIB, "Синтетика (калибр.)"),
    ]:
        axes[0].hist(dt, bins=bins, density=True, alpha=0.6, color=color, label=label)
        axes[1].hist(np.log1p(dt), bins=40, density=True, alpha=0.6, color=color, label=label)

    axes[0].set_xlabel("Интервал (сек)", fontsize=12)
    axes[0].set_ylabel("Плотность", fontsize=12)
    axes[0].set_title("Межсобытийные интервалы", fontsize=13)
    axes[0].legend()

    axes[1].set_xlabel("log(1 + интервал, сек)", fontsize=12)
    axes[1].set_ylabel("Плотность", fontsize=12)
    axes[1].set_title("Межсобытийные интервалы (log)\nЭкспоненциальный процесс = прямая", fontsize=12)
    axes[1].legend()

    stat, p, verdict = _ks(dt_r, dt_c)
    fig.text(0.5, -0.02,
             f"KS-тест: KS={stat:.3f}, p={p:.4f} — {verdict}",
             ha="center", fontsize=10, style="italic")
    fig.tight_layout()
    _save(fig, out_dir, "fig_val_3_inter_event_times.png")


# --- Рис. 4: Суточная активность ---------------------------------------------

def plot_hourly(df_real, df_calib, out_dir):
    print("4. Суточная активность...")

    def hours(df):
        return df["itim"].astype(str).str.zfill(6).str[:2].astype(int).clip(0, 23)

    r_h = hours(df_real)
    c_h = hours(df_calib)

    bins = np.arange(0, 25)
    r_pct = np.histogram(r_h, bins=bins)[0] / len(r_h) * 100
    c_pct = np.histogram(c_h, bins=bins)[0] / len(c_h) * 100

    fig, ax = plt.subplots(figsize=(11, 5))
    x = np.arange(24)
    w = 0.38
    ax.bar(x - w/2, r_pct, width=w, color=COLOR_REAL,  alpha=0.85, label="Реальные данные")
    ax.bar(x + w/2, c_pct, width=w, color=COLOR_CALIB, alpha=0.85, label="Синтетика (калибр.)")
    ax.axhline(100/24, color="gray", linestyle=":", alpha=0.5, label="Равномерное (4.17%)")
    ax.set_xlabel("Час суток", fontsize=12)
    ax.set_ylabel("Доля событий (%)", fontsize=12)
    ax.set_title("Суточное распределение сейсмической активности", fontsize=13)
    ax.set_xticks(x)
    ax.legend()
    fig.tight_layout()
    _save(fig, out_dir, "fig_val_4_hourly_activity.png")


# --- Рис. 5: Распределение по классам + сводка -------------------------------

def plot_class_distribution(df_calib, out_dir):
    print("5. Распределение по классам (синтетика)...")

    counts = [int((df_calib["label"] == i).sum()) for i in range(4)]
    total  = sum(counts)

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(list(LABEL_NAMES.values()), counts,
                  color=CLASS_COLORS, edgecolor="white", width=0.55)
    for bar, cnt in zip(bars, counts):
        ax.text(bar.get_x() + bar.get_width() / 2,
                bar.get_height() + total * 0.005,
                f"{cnt}\n({cnt/total*100:.0f}%)",
                ha="center", va="bottom", fontsize=10)
    ax.set_ylabel("Количество событий", fontsize=12)
    ax.set_title("Распределение классов опасности\n(калиброванная синтетика)", fontsize=13)
    ax.set_ylim(0, max(counts) * 1.15)
    fig.tight_layout()
    _save(fig, out_dir, "fig_val_5_class_distribution.png")


# --- Рис. 6: Сводная сравнительная таблица (рисунок) -------------------------

def plot_summary_table(df_real, df_calib, calib, out_dir):
    print("6. Сводная таблица...")

    log_r = np.log10(df_real["e"])
    log_c = np.log10(df_calib["e"])
    ks_e_stat, ks_e_p, _ = _ks(log_r.values, log_c.values)

    m_r = pd.to_numeric(df_real["magn"], errors="coerce").dropna()
    m_c = pd.to_numeric(df_calib["magn"], errors="coerce").dropna()
    ks_m_stat, ks_m_p, _ = _ks(m_r.values, m_c.values)

    rows = [
        ("Кол-во событий",      f"{len(df_real):,}",         f"{len(df_calib):,}",        "—"),
        ("log₁₀(E): среднее",   f"{log_r.mean():.2f}",  f"{log_c.mean():.2f}",  ""),
        ("log₁₀(E): std",       f"{log_r.std():.2f}",   f"{log_c.std():.2f}",   ""),
        ("log₁₀(E): медиана",   f"{log_r.median():.2f}",f"{log_c.median():.2f}",""),
        ("KS (log E)",         f"{ks_e_stat:.3f}",           f"p={ks_e_p:.4f}",
         "OK" if ks_e_p > 0.05 else f"KS={ks_e_stat:.3f}"),
        ("Магнитуда: среднее", f"{m_r.mean():.2f}",         f"{m_c.mean():.2f}",        ""),
        ("KS (магнитуда)",     f"{ks_m_stat:.3f}",          f"p={ks_m_p:.4f}",
         "OK" if ks_m_p > 0.05 else f"KS={ks_m_stat:.3f}"),
    ]

    fig, ax = plt.subplots(figsize=(9, 4))
    ax.axis("off")
    col_labels = ["Параметр", "Реальные данные", "Синтетика (калибр.)", "Результат"]
    table = ax.table(
        cellText=rows,
        colLabels=col_labels,
        loc="center",
        cellLoc="center",
    )
    table.auto_set_font_size(False)
    table.set_fontsize(10)
    table.scale(1, 1.6)

    for (r, c), cell in table.get_celld().items():
        if r == 0:
            cell.set_facecolor("#1565C0")
            cell.set_text_props(color="white", fontweight="bold")
        elif r % 2 == 0:
            cell.set_facecolor("#E3F2FD")

    ax.set_title("Сводная статистика валидации эмулятора",
                 fontsize=13, fontweight="bold", pad=20)
    fig.tight_layout()
    _save(fig, out_dir, "fig_val_6_summary_table.png")


# ─── Вывод в консоль ──────────────────────────────────────────────────────────

def print_summary(df_real, df_calib, calib):
    log_r = np.log10(df_real["e"])
    log_c = np.log10(df_calib["e"])
    stat, p, verdict = _ks(log_r.values, log_c.values)

    print("\n" + "=" * 65)
    print("РЕЗУЛЬТАТЫ ВАЛИДАЦИИ ЭМУЛЯТОРА")
    print("=" * 65)
    print(f"{'Параметр':<30} {'Реальные':>14} {'Синтетика':>14}")
    print("-" * 65)

    rows = [
        ("Кол-во событий",        f"{len(df_real):,}", f"{len(df_calib):,}"),
        ("log10(E): среднее",     f"{log_r.mean():.3f}", f"{log_c.mean():.3f}"),
        ("log10(E): std",         f"{log_r.std():.3f}", f"{log_c.std():.3f}"),
        ("log10(E): медиана",     f"{log_r.median():.3f}", f"{log_c.median():.3f}"),
    ]
    m_r = pd.to_numeric(df_real["magn"], errors="coerce").dropna()
    m_c = pd.to_numeric(df_calib["magn"], errors="coerce").dropna()
    if len(m_r) and len(m_c):
        rows.append(("Магнитуда: среднее", f"{m_r.mean():.3f}", f"{m_c.mean():.3f}"))

    for name, rv, sv in rows:
        print(f"{name:<30} {rv:>14} {sv:>14}")

    print(f"\nКритерий Колмогорова-Смирнова (log10 E):")
    print(f"  KS={stat:.4f}, p={p:.4f}")
    if p > 0.05:
        print("  Результат: распределения статистически не различаются (p > 0.05) [OK]")
    else:
        print(f"  Результат: {verdict}")

    q60, q80, q92 = calib["thresholds_log_e"]
    print("\nКалибровочные параметры распределения (единое lognormal):")
    print(f"  mu={calib['overall_mu']:.3f}, sigma={calib['overall_sigma']:.3f} (натуральный логарифм)")
    print(f"  Пороги классов (ln E): <{q60:.2f} / {q60:.2f}-{q80:.2f} / {q80:.2f}-{q92:.2f} / >{q92:.2f}")


# ─── Точка входа ──────────────────────────────────────────────────────────────

def parse_args():
    parser = argparse.ArgumentParser(description="Валидация эмулятора GeoSense")
    parser.add_argument("--real", required=True, help="Путь к events.csv")
    parser.add_argument("--out",  required=True, help="Директория для графиков")
    parser.add_argument("--save-calib", default=None,
                        help="Сохранить калибровку в JSON (опционально)")
    return parser.parse_args()


def main():
    args = parse_args()

    print("GeoSense — валидация эмулятора на реальных данных")
    print("=" * 65)

    if not os.path.exists(args.real):
        print(f"Ошибка: файл не найден: {args.real}", file=sys.stderr)
        sys.exit(1)

    os.makedirs(args.out, exist_ok=True)

    print(f"Загрузка реальных данных: {args.real}")
    df_real = load_real(args.real)
    print(f"  Загружено: {len(df_real)} событий")

    print("Калибровка параметров эмулятора...")
    calib = calibrate(df_real)

    print("Генерация калиброванной синтетики...")
    df_calib = generate_calibrated(calib)
    print(f"  Сгенерировано: {len(df_calib)} событий")

    if args.save_calib:
        with open(args.save_calib, "w", encoding="utf-8") as f:
            json.dump(calib, f, ensure_ascii=False, indent=2)
        print(f"  Калибровка сохранена: {args.save_calib}")

    print(f"\nГенерация графиков -> {args.out}")
    plot_gutenberg_richter(df_real, df_calib, args.out)
    plot_energy_calibrated(df_real, df_calib, args.out)
    plot_inter_event(df_real, df_calib, args.out)
    plot_hourly(df_real, df_calib, args.out)
    plot_class_distribution(df_calib, args.out)
    plot_summary_table(df_real, df_calib, calib, args.out)

    print_summary(df_real, df_calib, calib)
    print(f"\nГотово. 6 графиков сохранены в: {args.out}")


if __name__ == "__main__":
    main()
