import numpy as np
import pandas as pd


FEATURE_COLS = [
    # Энергетические
    "e", "log_e", "magn", "ampl", "log_ampl", "proc",
    # Пространственные (только относительные — работают на любом объекте)
    "depth_normalized", "dist_to_centroid",
    # Сетевые
    "np_actual", "rq_min", "rq_max", "rq_range",
    # Временные — счётчики и интервал
    "hour", "delta_t_prev", "count_1h", "count_6h", "count_24h",
    # Временные — нарастание (ключевые для коллективных аномалий)
    "energy_sum_1h", "energy_sum_6h", "energy_sum_24h",
    "energy_trend_6h", "count_trend_6h",
    # Нормированные счётчики (устойчивы к разбросу 10-1000 событий/день)
    "count_1h_norm", "count_6h_norm",
]


def build_features(df: pd.DataFrame) -> pd.DataFrame:
    df = df.copy()
    df = _add_energy_features(df)
    df = _add_spatial_features(df)
    df = _add_network_features(df)
    df = _add_temporal_features(df)
    return df


def _add_energy_features(df: pd.DataFrame) -> pd.DataFrame:
    df["log_e"] = np.log1p(df["e"])
    df["log_ampl"] = np.log1p(df["ampl"].fillna(0))
    df["magn"] = df["magn"].fillna(df["magn"].median())
    df["ampl"] = df["ampl"].fillna(df["ampl"].median())
    df["proc"] = df["proc"].fillna(0.5)
    return df


def _add_spatial_features(df: pd.DataFrame) -> pd.DataFrame:
    centroid_x = df["x"].mean()
    centroid_y = df["y"].mean()
    centroid_z = df["z"].mean()

    df["dist_to_centroid"] = np.sqrt(
        (df["x"] - centroid_x) ** 2 +
        (df["y"] - centroid_y) ** 2 +
        (df["z"] - centroid_z) ** 2
    )

    z_min, z_max = df["z"].min(), df["z"].max()
    z_range = z_max - z_min if z_max != z_min else 1
    df["depth_normalized"] = (df["z"] - z_min) / z_range

    return df


def _add_network_features(df: pd.DataFrame) -> pd.DataFrame:
    df["np_actual"] = df["np_actual"].fillna(df["np_actual"].median())
    df["rq_min"] = df["rq_min"].fillna(df["rq_min"].median())
    df["rq_max"] = df["rq_max"].fillna(df["rq_max"].median())
    df["rq_range"] = df["rq_max"] - df["rq_min"]
    return df


def _add_temporal_features(df: pd.DataFrame) -> pd.DataFrame:
    df["hour"] = (df["itim"] // 10000).clip(0, 23)

    df["_t_sec"] = _to_seconds(df)
    df_sorted = df.sort_values("_t_sec").copy()

    df_sorted["delta_t_prev"] = df_sorted["_t_sec"].diff().fillna(0).clip(0, 86400)

    t = df_sorted["_t_sec"]
    e = df_sorted["log_e"]

    df_sorted["count_1h"] = _rolling_count(t, window_sec=3600)
    df_sorted["count_6h"] = _rolling_count(t, window_sec=21600)
    df_sorted["count_24h"] = _rolling_count(t, window_sec=86400)

    df_sorted["energy_sum_1h"] = _rolling_sum(t, e, window_sec=3600)
    df_sorted["energy_sum_6h"] = _rolling_sum(t, e, window_sec=21600)
    df_sorted["energy_sum_24h"] = _rolling_sum(t, e, window_sec=86400)

    # Тренд энергии: последние 3ч vs предыдущие 3ч (>1 = нарастание)
    e_last_3h = _rolling_sum(t, e, window_sec=10800)
    e_prev_3h = _rolling_sum(t, e, window_sec=21600) - e_last_3h
    df_sorted["energy_trend_6h"] = e_last_3h / (e_prev_3h + 1e-6)

    # Тренд частоты: последние 3ч vs предыдущие 3ч (>1 = ускорение)
    c_last_3h = _rolling_count(t, window_sec=10800)
    c_prev_3h = df_sorted["count_6h"] - c_last_3h
    df_sorted["count_trend_6h"] = c_last_3h / (c_prev_3h + 1e-6)

    # Нормировка на фоновый уровень объекта (медиана за 24ч)
    median_24h = df_sorted["count_24h"].median()
    if median_24h > 0:
        df_sorted["count_1h_norm"] = df_sorted["count_1h"] / (median_24h / 24)
        df_sorted["count_6h_norm"] = df_sorted["count_6h"] / (median_24h / 4)
    else:
        df_sorted["count_1h_norm"] = df_sorted["count_1h"]
        df_sorted["count_6h_norm"] = df_sorted["count_6h"]

    df_sorted = df_sorted.drop(columns=["_t_sec"])
    return df_sorted.reindex(df.index)


def _to_seconds(df: pd.DataFrame) -> pd.Series:
    idat = df["idat"].astype(str).str.zfill(6)
    itim = df["itim"].astype(str).str.zfill(6)

    days = (
        (idat.str[:2].astype(int) + 2000 - 2024) * 365 +
        (idat.str[2:4].astype(int) - 1) * 30 +
        idat.str[4:6].astype(int)
    )
    secs = (
        itim.str[:2].astype(int) * 3600 +
        itim.str[2:4].astype(int) * 60 +
        itim.str[4:6].astype(int)
    )
    return days * 86400 + secs


def _rolling_count(t: pd.Series, window_sec: int) -> pd.Series:
    t_vals = t.values
    counts = np.zeros(len(t_vals), dtype=int)
    for i in range(len(t_vals)):
        counts[i] = np.searchsorted(t_vals, t_vals[i] - window_sec, side="left")
        counts[i] = i + 1 - counts[i]
    return pd.Series(counts, index=t.index)


def _rolling_sum(t: pd.Series, values: pd.Series, window_sec: int) -> pd.Series:
    t_vals = t.values
    v_vals = values.values
    result = np.zeros(len(t_vals))
    for i in range(len(t_vals)):
        left = np.searchsorted(t_vals, t_vals[i] - window_sec, side="left")
        result[i] = np.sum(v_vals[left:i + 1])
    return pd.Series(result, index=t.index)
