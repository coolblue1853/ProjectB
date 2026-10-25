"""기본 공격 찌르기 궤적 v2(Px_WThrust 덮어씀) — PixelLab 없이 직접 그리는 회색조 밝기 시트(GameFeelPlan 10.7).

v1(vfx_workbench 줄기)은 가늘고 짧아 찌르기의 힘이 안 읽혔다. v2 6프레임(피벗 = 왼쪽 손 쪽):
  0 손 앞 짧은 원뿔 → 1 길게 뻗은 원뿔(뿌리 넓고 끝 날카로움) + 밝은 심선 + 위아래 속도선
  → 2 최대 길이 + 끝 섬광(작은 마름모 별) → 3 원뿔이 가늘어지며 속도선이 앞으로 → 4 선 조각만 → 5 끝 점
밝기만 그린다(심 1.0 / 원뿔 0.75 / 속도선 0.55). 색은 PixelSlash2D 테마 램프 + 1텍셀 테두리.
결과: Assets/Art/PixelFx/Px_WThrust.png + json. 등록: Tools/probes/build_hit_impact.cs 와 같은 방식(build_thrust.cs).
사용: python Tools/pixelfx/gen_thrust.py
"""
import json
import os

import numpy as np
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "PixelFx")
W, H = 72, 20
X0 = 4  # 손 쪽 시작 x(피벗)
CY = H // 2


def cone(d, x0, x1, half, v):
    """x0 에서 폭 2*half, x1 에서 뾰족"""
    d.polygon([(x0, CY - half), (x1, CY), (x0, CY + half)], fill=v)


def frames():
    out = []
    for f in range(6):
        im = Image.new("L", (W, H), 0)
        d = ImageDraw.Draw(im)
        if f == 0:
            cone(d, X0, X0 + 18, 4, 190)
            d.line([(X0, CY), (X0 + 14, CY)], fill=255)
        elif f == 1:
            cone(d, X0, X0 + 52, 5, 190)
            d.line([(X0 + 6, CY - 7), (X0 + 30, CY - 7)], fill=140)
            d.line([(X0 + 10, CY + 7), (X0 + 34, CY + 7)], fill=140)
            d.line([(X0, CY), (X0 + 46, CY)], fill=255)
        elif f == 2:
            cone(d, X0 + 4, X0 + 62, 4, 190)
            d.line([(X0 + 16, CY - 7), (X0 + 40, CY - 7)], fill=140)
            d.line([(X0 + 20, CY + 7), (X0 + 46, CY + 7)], fill=140)
            d.line([(X0 + 6, CY), (X0 + 58, CY)], fill=255)
            tx = X0 + 62  # 끝 섬광
            d.polygon([(tx - 3, CY), (tx, CY - 4), (tx + 3, CY), (tx, CY + 4)], fill=255)
        elif f == 3:
            cone(d, X0 + 18, X0 + 64, 2, 170)
            d.line([(X0 + 30, CY - 6), (X0 + 50, CY - 6)], fill=120)
            d.line([(X0 + 34, CY + 6), (X0 + 56, CY + 6)], fill=120)
        elif f == 4:
            d.line([(X0 + 40, CY), (X0 + 64, CY)], fill=150)
            d.line([(X0 + 48, CY - 5), (X0 + 58, CY - 5)], fill=100)
            d.line([(X0 + 52, CY + 5), (X0 + 62, CY + 5)], fill=100)
        else:
            d.line([(X0 + 58, CY), (X0 + 64, CY)], fill=110)
        g = np.array(im)
        rgba = np.zeros((H, W, 4), np.uint8)
        rgba[..., 0] = rgba[..., 1] = rgba[..., 2] = g
        rgba[..., 3] = np.where(g > 0, 255, 0)
        out.append(rgba)
    return out


if __name__ == "__main__":
    fr = frames()
    Image.fromarray(np.concatenate(fr, axis=1), "RGBA").save(os.path.join(OUT, "Px_WThrust.png"))
    with open(os.path.join(OUT, "Px_WThrust.json"), "w") as fp:
        json.dump({"frames": len(fr), "w": W, "h": H, "fps": 20, "pivot": [X0 / W, 0.5], "ramp": "gray",
                   "source": "Tools/pixelfx/gen_thrust.py"}, fp)
    print("Px_WThrust", len(fr), (W, H))
