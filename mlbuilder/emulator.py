import numpy as np
import pandas as pd
from settings.app_settings import AppSettings


LABEL_BACKGROUND = 0  # щелчки породы — фоновая активность
LABEL_LEVEL1 = 1      # умеренная активность — пучение почвы
LABEL_LEVEL2 = 2      # высокая активность — давление на борт выработки
LABEL_LEVEL3 = 3      # критическая — опасное состояние массива


def generate_events(settings: AppSettings, random_seed: int = 42) -> pd.DataFrame:
    """
    Генерация синтетических сейсмических событий.

    Физические обоснования:
    - Энергия: логнормальное распределение — подтверждено эмпирически
      для горно-тектонических событий (Gibowicz & Kijko, 1994)
    - Магнитуда: соотношение Гутенберга-Рихтера M = 0.67*log10(E) - 1.2
    - Временная динамика: сценарий нарастания моделирует реальный каскад
      накопления напряжений перед макроразрушением

    Ограничения модели:
    - Координаты равномерные, без привязки к геологии объекта
    - Нет афтершоков после крупных событий
    - Нет пространственной кластеризации вдоль разломов
    """
    rng = np.random.default_rng(random_seed)
    em = settings.emulator

    # Независимые события по классам
    independent = [
        _generate_class(rng, em.n_background, LABEL_BACKGROUND,
                        e_mean=50.0, e_std=0.8, em=em),
        _generate_class(rng, em.n_level1, LABEL_LEVEL1,
                        e_mean=400.0, e_std=0.6, em=em),
        _generate_class(rng, em.n_level2, LABEL_LEVEL2,
                        e_mean=2500.0, e_std=0.5, em=em),
        _generate_class(rng, em.n_level3, LABEL_LEVEL3,
                        e_mean=10000.0, e_std=0.4, em=em),
    ]

    # Сценарии нарастания: фон → пучение → давление → опасно
    # Моделирует реальный каскад накопления напряжений
    buildup_sequences = [
        _generate_buildup_sequence(rng, em, start_day_offset=i * 60)
        for i in range(5)
    ]

    df = pd.concat(independent + buildup_sequences, ignore_index=True)
    df = df.sort_values("_t_sec").drop(columns=["_t_sec"]).reset_index(drop=True)
    return df


def _generate_class(rng: np.random.Generator, n: int, label: int,
                    e_mean: float, e_std: float, em) -> pd.DataFrame:
    mu = np.log(e_mean)
    e = rng.lognormal(mean=mu, sigma=e_std, size=n)

    x = rng.uniform(*em.x_range, size=n)
    y = rng.uniform(*em.y_range, size=n)
    z = rng.uniform(*em.z_range, size=n)

    magn = 0.67 * np.log10(np.clip(e, 1, None)) - 1.2 + rng.normal(0, 0.15, n)
    ampl = np.sqrt(e) * rng.uniform(0.8, 1.2, size=n)
    proc_base = 0.5 + 0.4 * (label / 3.0)
    proc = np.clip(proc_base + rng.normal(0, 0.1, n), 0.1, 1.0)
    np_actual = rng.integers(4, 13, size=n)
    rq_min = rng.uniform(50, 150, size=n)
    rq_max = rq_min + rng.uniform(50, 200, size=n)

    days_total = 730
    day_offsets = rng.integers(0, days_total, size=n)
    t_sec = day_offsets * 86400 + rng.integers(0, 86400, size=n)
    idat, itim = _t_sec_to_idat_itim(t_sec)

    return pd.DataFrame({
        "obj": 1, "idat": idat, "itim": itim,
        "x": x.astype(int), "y": y.astype(int), "z": z.astype(int),
        "e": np.round(e, 2), "magn": np.round(magn, 2),
        "proc": np.round(proc, 2), "ampl": np.round(ampl, 2),
        "np_actual": np_actual,
        "rq_min": np.round(rq_min, 1), "rq_max": np.round(rq_max, 1),
        "label": label, "_t_sec": t_sec,
    })


def _generate_buildup_sequence(rng: np.random.Generator, em,
                                start_day_offset: int) -> pd.DataFrame:
    """
    Сценарий нарастания напряжений:
    Фаза 1 (24ч): фоновые щелчки породы — низкая энергия, высокая частота
    Фаза 2 (12ч): нарастание — пучение почвы, энергия растёт
    Фаза 3 (6ч):  высокая активность — давление на борт, кластеризация
    Фаза 4 (1ч):  критическое событие — макроразрушение
    """
    parts = []
    t0 = start_day_offset * 86400

    # Фаза 1 — фон: 40 событий за 24 часа
    n1 = 40
    t1 = t0 + rng.integers(0, 86400, size=n1)
    parts.append(_build_phase(rng, t1, LABEL_BACKGROUND, e_mean=50.0, e_std=0.8, em=em))

    # Фаза 2 — нарастание: 20 событий за следующие 12 часов, энергия растёт
    n2 = 20
    t2 = t0 + 86400 + rng.integers(0, 43200, size=n2)
    parts.append(_build_phase(rng, t2, LABEL_LEVEL1, e_mean=400.0, e_std=0.6, em=em))

    # Фаза 3 — высокая активность: 10 событий за 6 часов
    n3 = 10
    t3 = t0 + 86400 + 43200 + rng.integers(0, 21600, size=n3)
    parts.append(_build_phase(rng, t3, LABEL_LEVEL2, e_mean=2500.0, e_std=0.5, em=em))

    # Фаза 4 — критическое событие
    t4 = np.array([t0 + 86400 + 43200 + 21600 + rng.integers(0, 3600)])
    parts.append(_build_phase(rng, t4, LABEL_LEVEL3, e_mean=10000.0, e_std=0.4, em=em))

    return pd.concat(parts, ignore_index=True)


def _build_phase(rng: np.random.Generator, t_sec: np.ndarray, label: int,
                 e_mean: float, e_std: float, em) -> pd.DataFrame:
    n = len(t_sec)
    mu = np.log(e_mean)
    e = rng.lognormal(mean=mu, sigma=e_std, size=n)

    x = rng.uniform(*em.x_range, size=n)
    y = rng.uniform(*em.y_range, size=n)
    z = rng.uniform(*em.z_range, size=n)

    magn = 0.67 * np.log10(np.clip(e, 1, None)) - 1.2 + rng.normal(0, 0.15, n)
    ampl = np.sqrt(e) * rng.uniform(0.8, 1.2, size=n)
    proc_base = 0.5 + 0.4 * (label / 3.0)
    proc = np.clip(proc_base + rng.normal(0, 0.1, n), 0.1, 1.0)
    np_actual = rng.integers(4, 13, size=n)
    rq_min = rng.uniform(50, 150, size=n)
    rq_max = rq_min + rng.uniform(50, 200, size=n)
    idat, itim = _t_sec_to_idat_itim(t_sec)

    return pd.DataFrame({
        "obj": 1, "idat": idat, "itim": itim,
        "x": x.astype(int), "y": y.astype(int), "z": z.astype(int),
        "e": np.round(e, 2), "magn": np.round(magn, 2),
        "proc": np.round(proc, 2), "ampl": np.round(ampl, 2),
        "np_actual": np_actual,
        "rq_min": np.round(rq_min, 1), "rq_max": np.round(rq_max, 1),
        "label": label, "_t_sec": t_sec,
    })


def _t_sec_to_idat_itim(t_sec: np.ndarray):
    days = t_sec // 86400
    secs = t_sec % 86400
    year = 24 + days // 365
    day_of_year = days % 365
    month = day_of_year // 30 + 1
    day = day_of_year % 30 + 1
    idat = year * 10000 + np.clip(month, 1, 12) * 100 + np.clip(day, 1, 28)
    h = secs // 3600
    m = (secs % 3600) // 60
    s = secs % 60
    itim = h * 10000 + m * 100 + s
    return idat.astype(int), itim.astype(int)
