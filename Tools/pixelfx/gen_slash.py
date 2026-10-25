"""기본 공격 베기 궤적 v2(Px_WSlashDown·Px_WSlashSide 덮어씀) — PixelLab 없이 직접 그리는 회색조 밝기 시트(GameFeelPlan 10.7).

v1(vfx_workbench 초승달)은 평평한 2색에 6프레임 중 3프레임이 거의 같아 정지 그림처럼 보였다. v2:
- 머리(진행 끝)가 빠르게 호를 그리며 나아가고(35% → 75% → 100%) 꼬리가 뒤따라 지워진다(0 → 15% → 50% → 80% → 95%).
- 두께는 머리 쪽으로 치우친 초승달(양 끝 뾰족), 밝기는 꼬리 0.45 → 머리 1.0 + 바깥 가장자리 더 밝게(쓸고 가는 날 끝).
- 1~2프레임에 안쪽 속도선(가는 호). 각도는 시계 방향(캐릭터가 오른쪽을 볼 때 위 → 앞 → 아래).
시트 크기·피벗은 v1 과 같게(Weapon 의 이펙트 위치·배율을 그대로 쓴다). 색은 PixelSlash2D 테마 램프 + 1텍셀 테두리.
사용: python Tools/pixelfx/gen_slash.py   등록: Tools/probes/build_slash.cs
"""
import json
import math
import os

import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "PixelFx")
HEAD = [0.35, 0.75, 1.0, 1.0, 1.0, 1.0]
TAIL = [0.0, 0.0, 0.15, 0.5, 0.8, 0.95]
THICK = [0.8, 1.0, 1.0, 0.8, 0.55, 0.35]
SPEEDLINE = [False, True, True, False, False, False]

# 이름: (폭, 높이, 피벗(v1 json 과 같게), 중심(위 기준 px), 반지름(x, y), 시작·끝 각도(도, 위가 +), 최대 두께 px)
SPECS = {
    "Px_WSlashDown": (48, 57, (0.3542, 0.5263), (17, 28), (28, 26), 95, -65, 12),  # 끝을 -95° 까지 그리면 땅 아래로 파고들었다
    "Px_WSlashSide": (58, 35, (0.4655, 0.4571), (29, 29), (27, 24), 175, -10, 10),
}


def render(w, h, center, radii, a0, a1, tmax, f):
    cx, cy = center
    rx, ry = radii
    head, tail = HEAD[f], TAIL[f]
    img = np.zeros((h, w), np.float32)
    span = a0 - a1  # 시계 방향(각도 감소)
    for y in range(h):
        for x in range(w):
            dx, dy = (x + 0.5 - cx) / rx, -(y + 0.5 - cy) / ry
            rho = math.hypot(dx, dy)
            ang = math.degrees(math.atan2(dy, dx))
            # 진행도 s: 시작각 0 → 끝각 1
            s = (a0 - ang) / span
            if s < -0.5:
                s += 360 / span
            if not (tail <= s <= head):
                continue
            u = (s - tail) / max(1e-4, head - tail)  # 보이는 구간 안 위치: 꼬리 0 → 머리 1
            prof = math.sin(math.pi * min(1, u ** 0.6)) ** 0.7  # 머리 쪽으로 치우친 초승달, 양 끝 뾰족
            th = tmax * THICK[f] * prof / ((rx + ry) / 2)
            if th <= 0 or not (1 - th <= rho <= 1):
                if SPEEDLINE[f] and 0.55 < u < 0.95 and abs(rho - (1 - tmax / ((rx + ry) / 2) - 0.12)) < 0.035:
                    img[y, x] = max(img[y, x], 0.6)
                continue
            edge = (rho - (1 - th)) / th  # 안쪽 0 → 바깥 1
            lum = 0.45 + 0.45 * u + (0.15 if edge > 0.7 else 0)
            img[y, x] = min(1.0, max(img[y, x], lum))
    return img


def build(name, spec):
    w, h, pivot, center, radii, a0, a1, tmax = spec
    frames = []
    for f in range(len(HEAD)):
        g = render(w, h, center, radii, a0, a1, tmax, f)
        rgba = np.zeros((h, w, 4), np.uint8)
        v = (g * 255).astype(np.uint8)
        rgba[..., 0] = rgba[..., 1] = rgba[..., 2] = v
        rgba[..., 3] = np.where(g > 0, 255, 0)
        frames.append(rgba)
    Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
    with open(os.path.join(OUT, name + ".json"), "w") as fp:
        json.dump({"frames": len(frames), "w": w, "h": h, "fps": 20, "pivot": list(pivot), "ramp": "gray",
                   "source": "Tools/pixelfx/gen_slash.py"}, fp)
    print(name, len(frames), (w, h))


if __name__ == "__main__":
    for n, sp in SPECS.items():
        build(n, sp)
