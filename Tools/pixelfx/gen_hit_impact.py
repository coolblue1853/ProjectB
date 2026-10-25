"""기본 공격 타격 이펙트 v2(Px_WHitStar 덮어씀) — PixelLab 없이 직접 그리는 회색조 밝기 시트(GameFeelPlan 10.7).

v1(vfx_workbench 별)은 작은 십자 별 + 흩날리는 점이라 손맛이 약했다. v2 는 상업 도트 액션식 6프레임:
  0 꽉 찬 섬광 원 → 1 방사 가시 + 얇은 충격 고리 → 2 가시 길어지며 가늘어짐·고리 퍼짐(끊김)
  → 3 가시 끝 조각이 바깥으로 → 4 파편만 → 5 마지막 점
밝기만 그린다(1.0 심 / 0.8 가시 / 0.6 고리 / 0.35 파편). 색은 PixelSlash2D 가 무기 테마 램프 5단으로 칠하고 1텍셀 테두리를 두른다.
가로 가시(휘두른 방향)를 길게, 대각 가시는 짧게 — 방향감. 결과: Assets/Art/PixelFx/Px_WHitStar.png + json (PixelVfxBaker.ImportSheet 로 다시 슬라이스).
사용: python Tools/pixelfx/gen_hit_impact.py
"""
import json
import math
import os

import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "PixelFx")
N = 48
C = (N - 1) / 2
# (각도, 길이 배율, 굵기 배율): 가로가 주 가시
SPIKES = [(0, 1.0, 1.0), (180, 0.85, 1.0), (90, 0.6, 0.8), (270, 0.6, 0.8), (40, 0.5, 0.6), (140, 0.45, 0.6), (220, 0.5, 0.6), (320, 0.45, 0.6)]


def poly_spike(draw, ang, r0, r1, half, v):
    """r0~r1 마름모 가시(가운데 폭 2*half). 다각형으로 채운다(점으로 굵기를 찍으면 평행선·십자 막대가 생겼다)"""
    a = math.radians(ang)
    dx, dy = math.cos(a), -math.sin(a)
    rm = r0 + (r1 - r0) * 0.35  # 굵은 곳은 안쪽(심 쪽)으로 치우침 — 바깥으로 뻗는 느낌
    pts = [(C + dx * r0, C + dy * r0), (C + dx * rm - dy * half, C + dy * rm + dx * half),
           (C + dx * r1, C + dy * r1), (C + dx * rm + dy * half, C + dy * rm - dx * half)]
    draw.polygon([(round(x), round(y)) for x, y in pts], fill=v)


def ring_arc(draw, r, v, gaps=0, width=1):
    box = [round(C - r), round(C - r), round(C + r), round(C + r)]
    if not gaps:
        draw.ellipse(box, outline=v, width=width)
        return
    step = 360 / gaps
    for i in range(0, gaps, 2):
        draw.arc(box, i * step, i * step + step * 0.8, fill=v, width=width)


def dot(draw, a, r, v, size=1):
    x, y = C + math.cos(math.radians(a)) * r, C - math.sin(math.radians(a)) * r
    draw.rectangle([round(x), round(y), round(x) + size - 1, round(y) + size - 1], fill=v)


def frames():
    from PIL import ImageDraw
    rng = np.random.default_rng(7)
    shards = [(rng.uniform(0, 360), rng.uniform(0.7, 1.0)) for _ in range(9)]
    out = []
    for f in range(6):
        im = Image.new("L", (N, N), 0)
        d = ImageDraw.Draw(im)
        if f == 0:
            # 첫 프레임 섬광은 작게(8px 원은 치명타 2배에서 몹을 통째로 가리는 공처럼 보였다)
            d.ellipse([C - 6, C - 6, C + 6, C + 6], fill=200)
            d.ellipse([C - 4, C - 4, C + 4, C + 4], fill=255)
        elif f == 1:
            ring_arc(d, 12, 150)
            for ang, ln, th in SPIKES:
                poly_spike(d, ang, 2, 6 + 15 * ln, 3.2 * th, 210)
            d.ellipse([C - 4, C - 4, C + 4, C + 4], fill=255)
        elif f == 2:
            ring_arc(d, 16, 150, gaps=12)
            for ang, ln, th in SPIKES:
                poly_spike(d, ang, 6, 10 + 16 * ln, 1.8 * th, 200)
            d.ellipse([C - 2, C - 2, C + 2, C + 2], fill=255)
        elif f == 3:
            for ang, ln, th in SPIKES:
                poly_spike(d, ang, 13 + 5 * ln, 17 + 9 * ln, 1.0 * th, 170)
            for a, sc in shards:
                dot(d, a, 15 * sc, 150, 2)
        elif f == 4:
            for ang, ln, th in SPIKES[:4]:
                poly_spike(d, ang, 19 + 3 * ln, 21 + 5 * ln, 0.6, 140)
            for a, sc in shards:
                dot(d, a, 19 * sc, 110)
        else:
            for a, sc in shards[:5]:
                dot(d, a, 21 * sc, 90)
        g = np.array(im)
        rgba = np.zeros((N, N, 4), np.uint8)
        rgba[..., 0] = rgba[..., 1] = rgba[..., 2] = g
        rgba[..., 3] = np.where(g > 0, 255, 0)
        out.append(rgba)
    return out


if __name__ == "__main__":
    fr = frames()
    Image.fromarray(np.concatenate(fr, axis=1), "RGBA").save(os.path.join(OUT, "Px_WHitStar.png"))
    with open(os.path.join(OUT, "Px_WHitStar.json"), "w") as fp:
        json.dump({"frames": len(fr), "w": N, "h": N, "fps": 20, "pivot": [0.5, 0.5], "ramp": "gray",
                   "source": "Tools/pixelfx/gen_hit_impact.py"}, fp)
    print("Px_WHitStar", len(fr), N)
