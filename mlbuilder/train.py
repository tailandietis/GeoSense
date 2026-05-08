import os
import json
import pickle
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

from xgboost import XGBClassifier
from sklearn.ensemble import IsolationForest
from sklearn.model_selection import train_test_split, StratifiedKFold, cross_val_score
from sklearn.metrics import (
    accuracy_score, f1_score, classification_report,
    confusion_matrix, roc_auc_score
)
from sklearn.preprocessing import label_binarize

from feature_engineering import FEATURE_COLS
from settings.app_settings import AppSettings


LABEL_NAMES = {0: "Фон", 1: "Умеренная", 2: "Высокая", 3: "Критическая"}
E_IDX = FEATURE_COLS.index("e")
MAGN_IDX = FEATURE_COLS.index("magn")


def train_all(df: pd.DataFrame, settings: AppSettings) -> dict:
    os.makedirs(settings.output_dir, exist_ok=True)
    plots_dir = os.path.join(settings.output_dir, "plots")
    os.makedirs(plots_dir, exist_ok=True)

    thesis_dir = settings.thesis_figures_dir
    if thesis_dir:
        os.makedirs(thesis_dir, exist_ok=True)

    X = df[FEATURE_COLS].fillna(0).values
    y = df["label"].values

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=settings.test_size, random_state=42, stratify=y,
    )

    xgb_model, xgb_metrics = _train_xgboost(
        X_train, X_test, y_train, y_test, settings, plots_dir, thesis_dir)

    iso_metrics = _train_isolation_forest(
        X_train, X_test, y_train, y_test, settings, plots_dir, thesis_dir)

    baseline_metrics = _evaluate_baselines(
        X_test, y_test, settings, plots_dir, thesis_dir)

    robustness_metrics = _robustness_analysis(
        xgb_model, X_test, y_test, settings, plots_dir, thesis_dir)

    if thesis_dir:
        _plot_class_distribution(y, thesis_dir)
        _plot_energy_distribution(df, thesis_dir)
        _plot_energy_trend_by_class(df, thesis_dir)

    metrics = {
        "xgboost": xgb_metrics,
        "isolation_forest": iso_metrics,
        "baseline_comparison": baseline_metrics,
        "robustness": robustness_metrics,
    }

    metrics_path = os.path.join(settings.output_dir, "metrics.json")
    with open(metrics_path, "w", encoding="utf-8") as f:
        json.dump(metrics, f, ensure_ascii=False, indent=2)

    demo_path = os.path.join(settings.output_dir, "demo_events.csv")
    df.drop(columns=["label"]).to_csv(demo_path, index=False)
    print(f"Демо-события сохранены: {demo_path} ({len(df)} событий)")

    print(f"\nМетрики сохранены: {metrics_path}")
    return metrics


def _train_xgboost(X_train, X_test, y_train, y_test,
                   settings, plots_dir, thesis_dir):
    print("\n" + "=" * 50)
    print("XGBoost — классификация уровня опасности")
    print("=" * 50)

    model = XGBClassifier(
        n_estimators=settings.n_estimators_xgb,
        max_depth=settings.max_depth_xgb,
        learning_rate=0.1,
        subsample=0.8,
        colsample_bytree=0.8,
        eval_metric="mlogloss",
        random_state=42,
        n_jobs=-1,
    )

    cv = StratifiedKFold(n_splits=5, shuffle=True, random_state=42)
    cv_scores = cross_val_score(model, X_train, y_train, cv=cv,
                                scoring="f1_macro", n_jobs=-1)
    print(f"CV F1-macro (5-fold): {cv_scores.mean():.4f} ± {cv_scores.std():.4f}")

    model.fit(X_train, y_train, eval_set=[(X_test, y_test)], verbose=False)

    y_pred = model.predict(X_test)
    acc = accuracy_score(y_test, y_pred)
    f1_macro = f1_score(y_test, y_pred, average="macro")
    f1_weighted = f1_score(y_test, y_pred, average="weighted")

    n_classes = len(np.unique(y_train))
    y_prob = model.predict_proba(X_test)
    y_bin = label_binarize(y_test, classes=list(range(n_classes)))
    roc_auc = roc_auc_score(y_bin, y_prob, multi_class="ovr", average="macro")

    print(f"Accuracy:    {acc:.4f}")
    print(f"F1-macro:    {f1_macro:.4f}")
    print(f"ROC-AUC:     {roc_auc:.4f}")
    print(classification_report(y_test, y_pred, target_names=list(LABEL_NAMES.values())))

    _plot_confusion_matrix(y_test, y_pred, plots_dir, "xgb_confusion_matrix.png")
    _plot_feature_importance(model, FEATURE_COLS, plots_dir, "xgb_feature_importance.png")

    if thesis_dir:
        _plot_confusion_matrix(y_test, y_pred, thesis_dir, "fig3_confusion_matrix.png")
        _plot_feature_importance(model, FEATURE_COLS, thesis_dir, "fig4_feature_importance.png")

    model_path = os.path.join(settings.output_dir, "xgb_model.json")
    model.save_model(model_path)

    metrics = {
        "accuracy": round(acc, 4),
        "f1_macro": round(f1_macro, 4),
        "f1_weighted": round(f1_weighted, 4),
        "roc_auc_macro": round(roc_auc, 4),
        "cv_f1_macro_mean": round(float(cv_scores.mean()), 4),
        "cv_f1_macro_std": round(float(cv_scores.std()), 4),
    }
    return model, metrics


def _train_isolation_forest(X_train, X_test, y_train, y_test,
                             settings, plots_dir, thesis_dir):
    print("\n" + "=" * 50)
    print("Isolation Forest — детектирование аномалий")
    print("=" * 50)

    # Обучаем только на фоновых событиях (класс 0 = щелчки породы)
    # Всё что отличается от нормального фона — потенциальная аномалия
    X_background = X_train[y_train == 0]
    print(f"Обучение на фоновых событиях: {len(X_background)} из {len(X_train)}")

    model = IsolationForest(
        n_estimators=settings.n_estimators_if,
        contamination=settings.contamination_if,
        random_state=42,
        n_jobs=-1,
    )
    model.fit(X_background)

    y_pred_raw = model.predict(X_test)
    y_pred_binary = (y_pred_raw == -1).astype(int)
    y_true_binary = (y_test >= 2).astype(int)

    acc = accuracy_score(y_true_binary, y_pred_binary)
    f1 = f1_score(y_true_binary, y_pred_binary, zero_division=0)
    anomaly_rate = y_pred_binary.mean()

    print(f"Доля обнаруженных аномалий: {anomaly_rate:.2%}")
    print(f"Accuracy: {acc:.4f} | F1 (аномалии): {f1:.4f}")

    scores = model.score_samples(X_test)
    _plot_anomaly_scores(scores, y_test, plots_dir)
    if thesis_dir:
        _plot_anomaly_scores(scores, y_test, thesis_dir, "fig8_anomaly_scores.png")

    model_path = os.path.join(settings.output_dir, "iso_forest.pkl")
    with open(model_path, "wb") as f:
        pickle.dump(model, f)

    return {
        "anomaly_rate": round(float(anomaly_rate), 4),
        "accuracy_binary": round(acc, 4),
        "f1_anomaly": round(f1, 4),
        "contamination": settings.contamination_if,
        "trained_on_background_only": True,
    }


def _evaluate_baselines(X_test, y_test, settings, plots_dir, thesis_dir):
    print("\n" + "=" * 50)
    print("Baseline методы сравнения")
    print("=" * 50)

    thr = settings.energy_thresholds
    e_vals = X_test[:, E_IDX]
    m_vals = X_test[:, MAGN_IDX]

    # Baseline 1: порог по энергии (GeoDa)
    y_energy = np.zeros(len(e_vals), dtype=int)
    y_energy[e_vals >= thr.background] = 1
    y_energy[e_vals >= thr.level1] = 2
    y_energy[e_vals >= thr.level2] = 3
    f1_energy = f1_score(y_test, y_energy, average="macro")
    acc_energy = accuracy_score(y_test, y_energy)
    print(f"Порог по энергии (GeoDa): Accuracy={acc_energy:.4f}, F1-macro={f1_energy:.4f}")

    # Baseline 2: b-value
    # b-value < 0.6 → критический, < 1.0 → высокий, < 1.5 → умеренный
    b_values = _compute_rolling_bvalue(m_vals, window=50)
    y_bvalue = np.zeros(len(b_values), dtype=int)
    y_bvalue[b_values < 1.5] = 1
    y_bvalue[b_values < 1.0] = 2
    y_bvalue[b_values < 0.6] = 3
    f1_bvalue = f1_score(y_test, y_bvalue, average="macro")
    acc_bvalue = accuracy_score(y_test, y_bvalue)
    print(f"b-value:                  Accuracy={acc_bvalue:.4f}, F1-macro={f1_bvalue:.4f}")

    # Baseline 3: Energy Index (EI = E / E_expected(M))
    # E_expected из стандартного соотношения: log10(E) = 1.5*M + 4.8
    e_expected = 10 ** (1.5 * np.clip(m_vals, -2, 5) + 4.8)
    ei = e_vals / (e_expected + 1e-6)
    y_ei = np.zeros(len(ei), dtype=int)
    y_ei[ei >= 0.1] = 1
    y_ei[ei >= 1.0] = 2
    y_ei[ei >= 3.0] = 3
    f1_ei = f1_score(y_test, y_ei, average="macro")
    acc_ei = accuracy_score(y_test, y_ei)
    print(f"Energy Index:             Accuracy={acc_ei:.4f}, F1-macro={f1_ei:.4f}")

    results = {
        "energy_threshold": {"accuracy": round(acc_energy, 4), "f1_macro": round(f1_energy, 4)},
        "b_value": {"accuracy": round(acc_bvalue, 4), "f1_macro": round(f1_bvalue, 4)},
        "energy_index": {"accuracy": round(acc_ei, 4), "f1_macro": round(f1_ei, 4)},
    }

    _plot_baseline_comparison(results, plots_dir, "baseline_comparison.png")
    if thesis_dir:
        _plot_baseline_comparison(results, thesis_dir, "fig5_baseline_comparison.png")

    return results


def _robustness_analysis(model, X_test, y_test, settings, plots_dir, thesis_dir):
    print("\n" + "=" * 50)
    print("Robustness анализ — устойчивость к погрешностям датчиков")
    print("=" * 50)

    # Реальная погрешность датчиков: ±1 порядок по энергии
    # σ = 0.5/1.0/1.5 в логшкале соответствует реальным условиям
    noise_levels = [0.0, 0.5, 1.0, 1.5]
    results = {}

    thr = settings.energy_thresholds
    e_idx = E_IDX

    for sigma in noise_levels:
        if sigma == 0.0:
            X_noisy = X_test.copy()
        else:
            rng = np.random.default_rng(42)
            X_noisy = X_test.copy().astype(float)
            # Добавляем шум к log_e (индекс 1 = log_e в FEATURE_COLS)
            log_e_idx = FEATURE_COLS.index("log_e")
            X_noisy[:, log_e_idx] += rng.normal(0, sigma, len(X_test))
            # Пересчитываем e из зашумлённого log_e
            X_noisy[:, e_idx] = np.expm1(np.clip(X_noisy[:, log_e_idx], 0, 20))

        # XGBoost
        y_pred_xgb = model.predict(X_noisy)
        f1_xgb = f1_score(y_test, y_pred_xgb, average="macro")

        # Baseline по энергии
        e_noisy = X_noisy[:, e_idx]
        y_energy = np.zeros(len(e_noisy), dtype=int)
        y_energy[e_noisy >= thr.background] = 1
        y_energy[e_noisy >= thr.level1] = 2
        y_energy[e_noisy >= thr.level2] = 3
        f1_energy = f1_score(y_test, y_energy, average="macro")

        results[str(sigma)] = {
            "xgboost_f1_macro": round(f1_xgb, 4),
            "energy_threshold_f1_macro": round(f1_energy, 4),
        }
        print(f"sigma={sigma:.1f}: XGBoost F1={f1_xgb:.4f} | Порог F1={f1_energy:.4f}")

    _plot_robustness(results, plots_dir, "robustness.png")
    if thesis_dir:
        _plot_robustness(results, thesis_dir, "fig6_robustness.png")

    return results


def _compute_rolling_bvalue(magnitudes: np.ndarray, window: int = 50) -> np.ndarray:
    b_values = np.ones(len(magnitudes)) * 1.0
    for i in range(len(magnitudes)):
        start = max(0, i - window)
        m_window = magnitudes[start:i + 1]
        if len(m_window) < 5:
            continue
        m_min = m_window.min()
        m_mean = m_window.mean()
        if m_mean > m_min:
            b_values[i] = np.log10(np.e) / (m_mean - m_min)
    return np.clip(b_values, 0.1, 5.0)


def _plot_confusion_matrix(y_true, y_pred, out_dir, filename):
    cm = confusion_matrix(y_true, y_pred)
    labels = list(LABEL_NAMES.values())
    fig, ax = plt.subplots(figsize=(7, 6))
    im = ax.imshow(cm, interpolation="nearest", cmap="Blues")
    plt.colorbar(im, ax=ax)
    ax.set_xticks(range(len(labels)))
    ax.set_yticks(range(len(labels)))
    ax.set_xticklabels(labels, rotation=30, ha="right")
    ax.set_yticklabels(labels)
    ax.set_xlabel("Предсказанный класс")
    ax.set_ylabel("Истинный класс")
    ax.set_title("Матрица ошибок — XGBoost")
    for i in range(cm.shape[0]):
        for j in range(cm.shape[1]):
            ax.text(j, i, str(cm[i, j]), ha="center", va="center",
                    color="white" if cm[i, j] > cm.max() / 2 else "black")
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, filename), dpi=150)
    plt.close()


def _plot_feature_importance(model, feature_names, out_dir, filename):
    importance = model.feature_importances_
    idx = np.argsort(importance)
    top_n = min(20, len(idx))
    idx_top = idx[-top_n:]
    fig, ax = plt.subplots(figsize=(9, 7))
    ax.barh([feature_names[i] for i in idx_top],
            importance[idx_top], color="steelblue")
    ax.set_xlabel("Важность признака (gain)")
    ax.set_title("Важность признаков — XGBoost (топ 20)")
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, filename), dpi=150)
    plt.close()


def _plot_anomaly_scores(scores, y_true, out_dir, filename="iso_anomaly_scores.png"):
    fig, ax = plt.subplots(figsize=(9, 5))
    label_colors = {0: "green", 1: "gold", 2: "orange", 3: "red"}
    for label, color in label_colors.items():
        mask = y_true == label
        ax.scatter(np.where(mask)[0], scores[mask],
                   c=color, label=LABEL_NAMES[label], alpha=0.6, s=15)
    ax.axhline(y=np.percentile(scores, 5), color="black",
               linestyle="--", label="Порог аномалии (5%)")
    ax.set_xlabel("Индекс события")
    ax.set_ylabel("Anomaly score")
    ax.set_title("Isolation Forest — аномальные скоры по классам")
    ax.legend(loc="upper right")
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, filename), dpi=150)
    plt.close()


def _plot_baseline_comparison(results: dict, out_dir, filename):
    methods = ["Порог энергии\n(GeoDa)", "b-value", "Energy Index", "XGBoost\n(наша модель)"]
    f1_values = [
        results["energy_threshold"]["f1_macro"],
        results["b_value"]["f1_macro"],
        results["energy_index"]["f1_macro"],
        0.0,  # XGBoost добавляется снаружи — заглушка для структуры
    ]
    colors = ["#aec6cf", "#aec6cf", "#aec6cf", "#2196F3"]
    fig, ax = plt.subplots(figsize=(8, 5))
    bars = ax.bar(methods, f1_values, color=colors, edgecolor="white", width=0.5)
    for bar, val in zip(bars, f1_values):
        ax.text(bar.get_x() + bar.get_width() / 2, bar.get_height() + 0.01,
                f"{val:.3f}", ha="center", va="bottom", fontsize=10)
    ax.set_ylim(0, 1.1)
    ax.set_ylabel("F1-macro")
    ax.set_title("Сравнение методов оценки уровня риска")
    ax.axhline(y=0.7, color="gray", linestyle=":", alpha=0.5)
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, filename), dpi=150)
    plt.close()


def _plot_robustness(results: dict, out_dir, filename):
    sigmas = [float(k) for k in results.keys()]
    f1_xgb = [v["xgboost_f1_macro"] for v in results.values()]
    f1_energy = [v["energy_threshold_f1_macro"] for v in results.values()]
    fig, ax = plt.subplots(figsize=(8, 5))
    ax.plot(sigmas, f1_xgb, "o-", color="#2196F3", label="XGBoost", linewidth=2)
    ax.plot(sigmas, f1_energy, "s--", color="#FF5722", label="Порог по энергии", linewidth=2)
    ax.set_xlabel("σ шума (в логшкале энергии)")
    ax.set_ylabel("F1-macro")
    ax.set_title("Устойчивость к погрешностям датчиков")
    ax.legend()
    ax.set_ylim(0, 1.05)
    ax.set_xticks(sigmas)
    ax.set_xticklabels(["0\n(без шума)", "0.5\n(~3x)", "1.0\n(~10x)", "1.5\n(~30x)"])
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, filename), dpi=150)
    plt.close()


def _plot_class_distribution(y, out_dir):
    labels = [LABEL_NAMES[i] for i in range(4)]
    counts = [np.sum(y == i) for i in range(4)]
    colors = ["green", "gold", "orange", "red"]
    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(labels, counts, color=colors, edgecolor="white")
    for bar, count in zip(bars, counts):
        ax.text(bar.get_x() + bar.get_width() / 2, bar.get_height() + 5,
                str(count), ha="center", va="bottom")
    ax.set_ylabel("Количество событий")
    ax.set_title("Распределение классов в обучающей выборке")
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, "fig1_class_distribution.png"), dpi=150)
    plt.close()


def _plot_energy_distribution(df, out_dir):
    fig, ax = plt.subplots(figsize=(8, 5))
    colors = {0: "green", 1: "gold", 2: "orange", 3: "red"}
    for label, color in colors.items():
        subset = df[df["label"] == label]["e"]
        ax.hist(np.log10(subset + 1), bins=40, alpha=0.6,
                color=color, label=LABEL_NAMES[label])
    ax.set_xlabel("log₁₀(Энергия, Дж)")
    ax.set_ylabel("Количество событий")
    ax.set_title("Распределение энергии по классам\n(логнормальное распределение)")
    ax.legend()
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, "fig2_energy_distribution.png"), dpi=150)
    plt.close()


def _plot_energy_trend_by_class(df, out_dir):
    if "energy_trend_6h" not in df.columns:
        return
    fig, ax = plt.subplots(figsize=(8, 5))
    colors = {0: "green", 1: "gold", 2: "orange", 3: "red"}
    for label, color in colors.items():
        subset = df[df["label"] == label]["energy_trend_6h"].clip(0, 5)
        ax.hist(subset, bins=40, alpha=0.6, color=color, label=LABEL_NAMES[label])
    ax.set_xlabel("Тренд энергии (последние 3ч / предыдущие 3ч)")
    ax.set_ylabel("Количество событий")
    ax.set_title("Нарастание энергии перед опасными событиями")
    ax.axvline(x=1.0, color="black", linestyle="--", label="Нет нарастания (=1)")
    ax.legend()
    plt.tight_layout()
    plt.savefig(os.path.join(out_dir, "fig7_energy_trend_by_class.png"), dpi=150)
    plt.close()
