import argparse
import os
import numpy as np
import pandas as pd
import plotly.graph_objects as go
from PIL import Image


TEXTURE_SIZE = 512
PALETTE_COLORS = 64


def _image_surface(img_path: str, x_min, x_max, y_min, y_max, z_val: float, opacity: float = 0.6):
    img = Image.open(img_path).convert("RGB")
    h_resized = int((img.height * TEXTURE_SIZE) / img.width)
    img_resized = img.resize((TEXTURE_SIZE, h_resized), Image.BILINEAR)
    img_quantized = img_resized.quantize(colors=PALETTE_COLORS)
    palette = img_quantized.getpalette()

    colorscale = [
        [i / (PALETTE_COLORS - 1), "rgb({},{},{})".format(
            palette[i * 3], palette[i * 3 + 1], palette[i * 3 + 2]
        )]
        for i in range(PALETTE_COLORS)
    ]

    surfacecolor = np.array(img_quantized, dtype=float) / (PALETTE_COLORS - 1)
    H, W = surfacecolor.shape
    xs = np.linspace(x_min, x_max, W)
    ys = np.linspace(y_min, y_max, H)
    xg, yg = np.meshgrid(xs, ys)
    zg = np.full_like(xg, z_val)

    return go.Surface(
        x=xg,
        y=yg,
        z=zg,
        surfacecolor=surfacecolor,
        colorscale=colorscale,
        cmin=0,
        cmax=1,
        showscale=False,
        opacity=opacity,
        hoverinfo="skip",
        name="Подложка",
    )


def generate_map(
    events_csv: str,
    output_html: str,
    z_scale: float = 4.0,
    layouts: list = None,
):
    df = pd.read_csv(events_csv)

    required = {"x", "y", "z", "e"}
    if not required.issubset(df.columns):
        raise ValueError(f"CSV должен содержать колонки: {required}")

    df = df[df["e"] > 0].dropna(subset=["x", "y", "z", "e"]).copy()

    # Убираем события с placeholder-координатами (x<=100 и y<=100 одновременно)
    df = df[~((df["x"] <= 100) & (df["y"] <= 100))].copy()

    if df.empty:
        raise ValueError("Нет событий для визуализации")

    log_e = np.log10(df["e"].values)
    p5  = np.percentile(log_e, 5)
    p95 = np.percentile(log_e, 95)

    size = np.clip((log_e - p5) / (p95 - p5 + 1e-9), 0.05, 1.0) * 12 + 3

    hover_text = [
        f"X={row.x:.0f}, Y={row.y:.0f}, Z={row.z:.0f}<br>"
        f"E={row.e:.1f} Дж<br>"
        f"log₁₀E={le:.2f}"
        for row, le in zip(df.itertuples(), log_e)
    ]

    x_min, x_max = df["x"].min(), df["x"].max()
    y_min, y_max = df["y"].min(), df["y"].max()
    z_min = df["z"].min()

    traces = []

    if layouts:
        sorted_layouts = sorted(layouts, key=lambda l: l.get("z", z_min))
        for i, layout in enumerate(sorted_layouts):
            img_path = layout.get("file")
            z_coor = layout.get("z", z_min)
            opacity = 1.0 if i == 0 else layout.get("opacity", 0.5)
            if img_path and os.path.exists(img_path):
                surf = _image_surface(img_path, x_min, x_max, y_min, y_max, z_coor * z_scale, opacity)
                traces.append(surf)

    scatter = go.Scatter3d(
        x=df["x"],
        y=df["y"],
        z=df["z"] * z_scale,
        mode="markers",
        marker=dict(
            size=size,
            color=log_e,
            colorscale="Jet",
            cmin=p5,
            cmax=p95,
            opacity=0.85,
            colorbar=dict(title="log₁₀(E), Дж", thickness=16, len=0.6),
            line=dict(width=0),
        ),
        text=hover_text,
        hoverinfo="text",
        name="События",
    )
    traces.append(scatter)

    z_tick_vals = np.linspace(df["z"].min() * z_scale, df["z"].max() * z_scale, 6)
    z_tick_text = [f"{v / z_scale:.0f}" for v in z_tick_vals]

    layout = go.Layout(
        scene=dict(
            xaxis=dict(title="X, м", showgrid=True, gridcolor="#334"),
            yaxis=dict(title="Y, м", showgrid=True, gridcolor="#334"),
            zaxis=dict(
                title="Z, м",
                tickvals=z_tick_vals.tolist(),
                ticktext=z_tick_text,
                showgrid=True,
                gridcolor="#334",
            ),
            bgcolor="#0d0d1a",
            aspectmode="manual",
            aspectratio=dict(x=1, y=1, z=0.6),
            camera=dict(eye=dict(x=1.5, y=1.5, z=0.8)),
        ),
        margin=dict(l=0, r=0, b=0, t=40),
        title=dict(text=f"3D-карта сейсмических событий (n={len(df)})", x=0.5),
        paper_bgcolor="#1a1a2e",
        font=dict(color="#e0e0e0"),
    )

    fig = go.Figure(data=traces, layout=layout)
    fig.write_html(output_html, include_plotlyjs=True, full_html=True)
    print(f"Карта сохранена: {output_html} ({len(df)} событий)")


def main():
    parser = argparse.ArgumentParser(description="Генерация 3D-карты сейсмических событий")
    parser.add_argument("--events", required=True, help="CSV с событиями")
    parser.add_argument("--output", default="output/events_3d_map.html", help="Выходной HTML")
    parser.add_argument("--z_scale", type=float, default=4.0, help="Коэффициент растяжения оси Z")
    parser.add_argument("--layout", action="append", default=[],
                        help="Подложка: path:z:opacity (можно несколько раз)")
    args = parser.parse_args()

    layouts = []
    for l in args.layout:
        parts = l.split("|")
        entry = {"file": parts[0]}
        if len(parts) > 1:
            entry["z"] = float(parts[1])
        if len(parts) > 2:
            entry["opacity"] = float(parts[2])
        layouts.append(entry)

    os.makedirs(os.path.dirname(os.path.abspath(args.output)), exist_ok=True)
    generate_map(args.events, args.output, args.z_scale, layouts)


if __name__ == "__main__":
    main()
