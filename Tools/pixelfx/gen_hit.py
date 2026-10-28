"""타격 이펙트 v7(림버스 타격 문법, 10-28) — 절차 생성 회색조 도트. PixelSlash2D 가 테마 램프로 칠한다.
문법(LimbusReference.md): ① 불규칙 바늘 별(갈래 길이가 서로 다르고 바늘처럼 뾰족) ② 맞은 방향 찢긴 쐐기(뒤는 톱니) ③ 안쪽 섬광은 작게.
프레임: 0 별 크게 + 쐐기 → 1 갈래가 더 길고 가늘게 → 2 갈래 끝만 남아 끊김 → 3 점 몇 개. 기존 Px_WHitStar 는 보존하고 새 이름 Px_WHit7.
회색 3단: 255(심·날) / 150(몸) / 95(가장자리). 사용: python Tools/pixelfx/gen_hit.py [미리보기 폴더]
"""
import json
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.join(os.path.dirname(__file__), "..", "..")
OUT = os.path.join(ROOT, "Assets", "Art", "PixelFx")
W, H = 64, 48
CX, CY = 30, 24  # 중심(쐐기가 오른쪽으로 뻗어 중심을 왼쪽에)
SEED = 11
# 별 갈래: (각도°, 길이 배율) — 길이가 들쭉날쭉해야 림버스 같다(hit_06). 오른쪽(맞은 방향)이 길다
SPIKES = [(0, 1.0), (38, 0.55), (82, 0.7), (128, 0.4), (170, 0.62), (212, 0.45), (262, 0.78), (318, 0.5)]
FRAMES = [  # (별 반지름, 갈래 두께, 쐐기 길이, 남김 비율)
    (13.0, 3.2, 22.0, 1.0),
    (17.0, 2.2, 28.0, 1.0),
    (19.0, 1.4, 30.0, 0.45),
    (20.0, 1.0, 0.0, 0.2),
]


def spike_poly(cx, cy, ang, length, width, start=0.0):
    a = math.radians(ang)
    dx, dy = math.cos(a), -math.sin(a)
    nx, ny = -dy, dx
    b0 = (cx + dx * length * start, cy + dy * length * start)
    tip = (cx + dx * length, cy + dy * length)
    w = width * (1 - start)
    return [(b0[0] + nx * w, b0[1] + ny * w), tip, (b0[0] - nx * w, b0[1] - ny * w)]


def frame(r, wid, wedge, keep):
    im = Image.new("L", (W, H), 0)
    g = ImageDraw.Draw(im)
    start = 1 - keep  # 남김 비율이 줄면 갈래 안쪽부터 사라져 끝만 남는다
    # ② 맞은 방향 쐐기(오른쪽): 위 가장자리 곧고, 아래는 톱니
    if wedge > 0:
        top = [(CX - 4, CY - 3), (CX + wedge, CY + 1)]
        teeth = []
        n = 4
        for k in range(n):
            x0 = CX + wedge * (1 - (k + 1) / n)
            teeth += [(x0 + wedge / n * 0.6, CY + 1 + 4 * (1 - k / n) * 0.4), (x0, CY + 4 * (1 - k / n))]
        g.polygon(top + teeth + [(CX - 4, CY + 4)], fill=95)
        g.polygon([(CX - 2, CY - 2), (CX + wedge * 0.95, CY + 1), (CX - 2, CY + 2)], fill=150)
    # ① 불규칙 바늘 별
    for ang, mul in SPIKES:
        poly = spike_poly(CX, CY, ang, r * mul, wid, start)
        g.polygon(poly, fill=150)
        core = spike_poly(CX, CY, ang, r * mul * 0.75, max(0.6, wid * 0.45), start)
        g.polygon(core, fill=255)
    # ③ 가운데 섬광(작게, 첫 두 프레임만)
    if keep >= 1.0:
        rr = 3 if wid > 3 else 2
        g.ellipse([CX - rr, CY - rr, CX + rr, CY + rr], fill=255)
    if keep < 0.3:  # 마지막: 갈래 끝 점 몇 개
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        for ang, mul in SPIKES[::2]:
            a = math.radians(ang)
            x, y = CX + math.cos(a) * r * mul, CY - math.sin(a) * r * mul
            g.point((round(x), round(y)), fill=255)
    return np.array(im)


def build():
    frames = []
    for r, wid, wedge, keep in FRAMES:
        v = frame(r, wid, wedge, keep)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(v > 0, 255, 0)
        frames.append(img)
    return frames


def main():
    frames = build()
    name = "Px_WHit7"
    Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
    with open(os.path.join(OUT, name + ".json"), "w") as fp:
        json.dump({"frames": len(frames), "w": W, "h": H, "fps": 30, "pivot": [CX / W, (H - CY) / H], "ramp": "gray",
                   "source": "Tools/pixelfx/gen_hit.py"}, fp)
    if len(sys.argv) > 1:
        # 미리보기: 강철 램프로 칠해 4배
        ramp = [(0x1E, 0x28, 0x46), (0x3C, 0x64, 0xAA), (0x8C, 0xB4, 0xF0), (0xD2, 0xE6, 0xFF), (255, 255, 255)]
        row = Image.new("RGB", (W * len(frames), H), (38, 44, 54))
        px = row.load()
        for k, f in enumerate(frames):
            for y in range(H):
                for x in range(W):
                    if f[y, x, 3]:
                        v = f[y, x, 0]
                        px[k * W + x, y] = ramp[4] if v >= 230 else ramp[2] if v >= 130 else ramp[1]
        row.resize((row.width * 5, row.height * 5), Image.NEAREST).save(os.path.join(sys.argv[1], "hit7.png"))
    print(name, len(frames))


if __name__ == "__main__":
    main()
