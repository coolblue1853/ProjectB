"""1층계 폴리싱용 절차 생성 도트 이펙트 (gen_magic.py 와 같은 규칙: 밝기장 → 알파 컷 → 5색 램프).
- 연기 퍼프(회색·보라): 소환수 등장/퇴장, 몹 사망
- 흙먼지(갈색·돌 회색): 돌진·뿌리·착지 (바닥 피벗)
- 지면 충격파(흙): 내려찍기·충격파 (바닥 피벗)
- 붉은 폭발: 흡혈·포효·돌격 호출
- 구역 분위기 입자 시트(Resources/FX/Px_Ambient.png, 16x16 칸): 잎 2·포자·불씨·룬·안개
사용: python Tools/pixelfx/gen_polish_fx.py  → Assets/Art/PixelFx/Px_*.png+json, Assets/Resources/FX/Px_Ambient.png
"""
import math
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
import gen_magic as gm  # noqa: E402

gm.RAMPS.update({
    "smoke":  ["2A2633", "4A4652", "6E6A78", "9A96A3", "C4C0CC"],
    "shadow": ["1A0F2A", "3F2560", "7650A0", "B28ED6", "EADCFA"],
    "earth":  ["2E1C10", "5E3C20", "91663A", "C49C68", "EED8B0"],
    "stone":  ["24262B", "4C5058", "7E838C", "B4B8BE", "E6E8EB"],
})
rng = np.random.default_rng(11)
OUT = gm.OUT
RES_FX = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "FX")


def blob(xx, yy, cx, cy, r):
    return np.exp(-(((xx - cx) ** 2 + (yy - cy) ** 2) / max(r * r, 0.01)))


def smoke_puff(n=40, frames=10, puffs=7):
    c = (n - 1) / 2
    yy, xx = np.mgrid[0:n, 0:n]
    ang = rng.uniform(0, 2 * math.pi, puffs)
    spd = rng.uniform(0.55, 1.0, puffs)
    dither = rng.uniform(0, 0.12, (n, n))
    out = []
    for f in range(frames):
        t = f / (frames - 1)
        e = gm.ease_out(t)
        fld = np.zeros((n, n))
        for a, s in zip(ang, spd):
            d = e * s * c * 0.62
            px, py = c + math.cos(a) * d, c + math.sin(a) * d - t * 5  # 위로 피어오름
            r = 3.5 + 6 * e * s
            fld += blob(xx, yy, px, py, r) * 0.42
        fld += blob(xx, yy, c, c - t * 4, 3 + 4 * e) * (1 - t) * 0.5  # 가운데 덩어리
        fld = np.minimum(fld, 0.95) * (1 - t ** 1.4) + dither * (1 - t) - (1 - blob(xx, yy, c, c, c * 0.9)) * 0.1
        out.append(np.clip(fld, 0, 1.2))
    return out, (0.5, 0.5)


def dust_burst(w=64, h=30, frames=9, puffs=12):
    cx = (w - 1) / 2
    base = h - 3
    yy, xx = np.mgrid[0:h, 0:w]
    side = np.where(np.arange(puffs) % 2 == 0, -1, 1)
    spd = rng.uniform(0.15, 1.0, puffs)
    up = rng.uniform(0.1, 1.0, puffs)
    pebbles = rng.uniform(-1, 1, (10, 2))
    out = []
    for f in range(frames):
        t = f / (frames - 1)
        e = gm.ease_out(t)
        fld = np.zeros((h, w))
        for sd, s, u in zip(side, spd, up):
            px = cx + sd * e * s * cx * 0.85
            py = base - 2 - e * u * h * 0.45
            r = 2 + 3.5 * e * (0.5 + s * 0.5)
            fld += blob(xx, yy, px, py, r) * 0.45
        for vx, vy in pebbles:  # 튀는 흙 알갱이 (포물선)
            px = int(cx + vx * e * cx * 0.9)
            py = int(base - (abs(vy) * 14 * t - 22 * t * t))
            if 0 <= px < w and 0 <= py < h and t < 0.85:
                fld[py, px] = max(fld[py, px], 1.0 * (1 - t))
        fld = np.minimum(fld, 0.95) * (1 - t ** 1.6)
        out.append(np.clip(fld, 0, 1.2))
    return out, (0.5, 3 / h)


def ground_shock(w=96, h=26, frames=8):
    cx = (w - 1) / 2
    base = h - 4
    yy, xx = np.mgrid[0:h, 0:w]
    out = []
    for f in range(frames):
        t = f / (frames - 1)
        e = gm.ease_out(t)
        fld = np.zeros((h, w))
        rx, ry = 4 + (cx - 4) * e, 2 + 4 * e
        r = np.hypot((xx - cx) / rx, (yy - base) / ry)
        fld += np.clip(1 - np.abs(r - 1) * rx / 2.2, 0, 1) * (1 - t) * 1.1  # 지면 고리
        for k in range(-5, 6):  # 솟는 흙 가시
            px = cx + k * rx / 5.5
            hgt = (1 - abs(k) / 6) * 14 * math.sin(min(1, t * 1.6) * math.pi)
            m = (np.abs(xx - px) < 1.4) & (yy > base - hgt) & (yy <= base)
            fld += np.where(m, 0.9 * (1 - t * 0.6), 0)
        fld += blob(xx, yy, cx, base, 3 + 6 * e) * (1 - t) ** 2
        out.append(np.clip(fld, 0, 1.2))
    return out, (0.5, 4 / h)


def ambient_sheet():
    """16x16 칸 6개: 잎A·잎B·포자·불씨·룬·안개 (RGBA 직접 채색)."""
    cells = []

    def cell():
        return np.zeros((16, 16, 4), np.uint8)

    def put(img, pts, col):
        for x, y in pts:
            img[y, x] = (*col, 255)

    leaf_dark, leaf_mid, leaf_hi = (58, 92, 40), (98, 140, 52), (164, 196, 86)
    a = cell()
    put(a, [(x, y) for y in range(5, 11) for x in range(4, 12) if abs(x - 7.5) + abs(y - 7.5) * 1.6 < 5.2], leaf_mid)
    put(a, [(x, 7 + (x - 4) // 4) for x in range(4, 12)], leaf_dark)
    put(a, [(6, 6), (7, 6), (8, 6)], leaf_hi)
    cells.append(a)
    b = cell()
    put(b, [(x, y) for y in range(4, 12) for x in range(5, 11) if abs(x - 7.5) * 1.6 + abs(y - 7.5) < 5.2], (150, 104, 44))
    put(b, [(7, y) for y in range(4, 12)], (96, 62, 26))
    put(b, [(6, 6), (6, 7)], (210, 156, 70))
    cells.append(b)
    s = cell()  # 포자: 연두 빛 점
    put(s, [(7, 7), (8, 7), (7, 8), (8, 8)], (210, 255, 150))
    put(s, [(6, 7), (9, 8), (7, 6), (8, 9)], (130, 200, 90))
    cells.append(s)
    e = cell()  # 불씨
    put(e, [(7, 7), (8, 7), (7, 8)], (255, 236, 160))
    put(e, [(8, 8), (6, 7), (7, 9)], (250, 140, 40))
    cells.append(e)
    r = cell()  # 룬 조각 (청록)
    put(r, [(7, y) for y in range(4, 12)] + [(x, 5) for x in range(5, 10)] + [(5, 9), (6, 10), (9, 9), (8, 10)], (110, 240, 230))
    put(r, [(7, 7), (7, 8)], (230, 255, 255))
    cells.append(r)
    f = cell()  # 안개 덩어리 (가장자리 디더)
    yy, xx = np.mgrid[0:16, 0:16]
    d = np.hypot((xx - 7.5) / 7.5, (yy - 7.5) / 5.0)
    on = (d < 0.75) | ((d < 1.0) & ((xx + yy) % 2 == 0))
    f[on] = (205, 215, 200, 255)
    cells.append(f)
    sheet = np.concatenate(cells, axis=1)
    os.makedirs(RES_FX, exist_ok=True)
    Image.fromarray(sheet, "RGBA").save(os.path.join(RES_FX, "Px_Ambient.png"))
    print("Px_Ambient", len(cells))


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    f, p = smoke_puff(); gm.write("Px_SmokeGray", f, p, "smoke", fps=18)
    f, p = smoke_puff(); gm.write("Px_SmokePurple", f, p, "shadow", fps=18)
    f, p = dust_burst(); gm.write("Px_DustBrown", f, p, "earth", fps=20)
    f, p = dust_burst(); gm.write("Px_DustStone", f, p, "stone", fps=20)
    f, p = ground_shock(); gm.write("Px_GroundShock", f, p, "earth", fps=20)
    f, p = gm.radial_burst(); gm.write("Px_BurstRed", f, p, "red")
    ambient_sheet()
