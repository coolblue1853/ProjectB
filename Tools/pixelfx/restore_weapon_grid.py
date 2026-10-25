"""확대 저장된 무기 도트 그림(512px·100 PPU)을 원래 도트 격자로 되돌린다 — 픽셀 리그 주인공용 (PixelRigDesign 3.5절 무기 쥐기).

원본 무기 그림은 도트를 몇 배로 키워 큰 캔버스에 둔 것이라, 같은 색이 이어지는 최소 길이(= 확대 배율)를 찾아
그 칸의 가운데 색으로 줄이면 원래 도트가 손실 없이 돌아온다(새로 그리거나 변환하지 않는다).

결과: Assets/Resources/PixelRig/Weapons/<이름>.png + .json
  grip  = 손이 쥐는 픽셀(손잡이)
  angle = 그림 속 칼날 방향(도, 0 = 오른쪽, 90 = 위) — 리그가 칼날이 위를 향하게 돌린다
사용: python Tools/pixelfx/restore_weapon_grid.py [이름 ...]
"""
import json
import math
import os
import sys
from collections import Counter

import numpy as np
from PIL import Image

SRC = "Assets/Sprite/Weapon"
OUT = "Assets/Resources/PixelRig/Weapons"

# 무기 종류별 쥐는 법: "blade" = 손잡이 끝에서 칼날 쪽으로 grip_frac 지점(긴 축의 아래·왼쪽 끝 기준),
# "center" = 그림 가운데(방패·너클·책), "left" = 왼쪽 끝 가운데(손등에 끼우는 발톱)
KIND = {
    "ScareStaff": ("staff", 0.22), "SacreGreatSword": ("blade", 0.12), "ScareSide": ("blade", 0.25),
    "Wood Spear": ("blade", 0.30), "RatCheifDagger": ("blade", 0.12), "LeafStaff": ("staff", 0.25),
    "PoisonSword": ("blade", 0.12), "ChiefsWarBanner": ("pole", 0),
    "WoodShield": ("center", 0), "Lleatherknuckles": ("center", 0), "Rleatherknuckles": ("center", 0),
    "LeafAdae": ("center", 0), "BanditShortbow": ("center", 0), "RatWhistle": ("center", 0),
    "ScareClowRight": ("left", 0), "ScareClowLeft": ("left", 0),
}


# 자동 각도가 맞지 않는 그림(낫: 휜 자루라 주축이 기운다) — 자루가 세로라 90
# 낫(ScareSide): 큰 날이 자루 위에서 아래로 늘어진 그림이라 비스듬히 들면 날 끝이 땅을 뚫었다
# → 대기 칼날 각도(45°)에서 그림이 똑바로 서도록 45, 손잡이는 자루 맨 아래 끝에서 4줄 위
ANGLE_OVERRIDE = {"ScareSide": 45.0}
GRIP_FROM_BOTTOM = {"ScareSide": 4}


def block_size(a):
    """같은 색이 가로·세로로 이어지는 길이들의 최빈 최소 단위 = 확대 배율"""
    rgba = a.astype(int)
    key = rgba[..., 0] * 1_000_000 + rgba[..., 1] * 1000 + rgba[..., 2] + (rgba[..., 3] > 127) * 7
    runs = Counter()
    for line in list(key) + list(key.T):
        start = 0
        for i in range(1, len(line) + 1):
            if i == len(line) or line[i] != line[start]:
                runs[i - start] += 1
                start = i
    cands = [r for r, n in runs.items() if r >= 2 and n >= 20]
    best = None
    for s in range(2, 33):
        score = sum(n for r, n in runs.items() if r % s == 0 and r >= s)
        tot = sum(n for r, n in runs.items() if r >= 2)
        if tot and score / tot > 0.85:
            best = s
    return best or (min(cands) if cands else 1)


def restore(name):
    path = os.path.join(SRC, name + ".png")
    if not os.path.exists(path):
        path = os.path.join("Assets/Resources/Item", name + ".png")  # 깃발·활은 아이템 그림만 있다
    a = np.array(Image.open(path).convert("RGBA"))
    ys, xs = np.where(a[..., 3] > 127)
    a = a[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    s = block_size(a)
    # 격자 위상: 불투명 경계가 칸 경계에 맞도록 왼쪽 위 원점 사용, 칸 가운데 색을 샘플
    h, w = a.shape[0] // s + (1 if a.shape[0] % s else 0), a.shape[1] // s + (1 if a.shape[1] % s else 0)
    out = np.zeros((h, w, 4), np.uint8)
    for y in range(h):
        for x in range(w):
            cy, cx = min(y * s + s // 2, a.shape[0] - 1), min(x * s + s // 2, a.shape[1] - 1)
            px = a[cy, cx]
            out[y, x] = px if px[3] > 127 else (0, 0, 0, 0)
            if px[3] > 127:
                out[y, x, 3] = 255
    kind, frac = KIND.get(name, ("blade", 0.15))
    oy, ox = np.where(out[..., 3] > 0)
    pts = np.stack([ox, -oy], 1).astype(float)  # 위가 +y
    c = pts.mean(0)
    if kind == "pole":
        # 깃발: 왼쪽 깃대를 아래 3/4 지점에서 쥔다, 깃대는 세로
        px_ = int(np.median(ox[oy > h * 0.8]))
        gx, gy, ang = px_, int(h * 0.75), 90.0
    elif kind == "center":
        gx, gy, ang = int(round(c[0])), int(round(-c[1])), 90.0
    elif kind == "left":
        lx = ox.min()
        col = oy[ox <= lx + 1]
        gx, gy, ang = int(lx + 1), int(round(col.mean())), 0.0
    else:
        # 주축(PCA) — 끝점 두 개 중 아래쪽(왼쪽 아래) 끝이 손잡이 (blade·staff 공통)
        u, sv, vt = np.linalg.svd(pts - c, full_matrices=False)
        d = vt[0]
        proj = (pts - c) @ d
        lo, hi = proj.min(), proj.max()
        end_lo, end_hi = c + d * lo, c + d * hi
        if end_lo[1] > end_hi[1]:
            d, lo, hi = -d, -hi, -lo
            end_lo, end_hi = c + d * lo, c + d * hi
        # 손잡이 아래로 남는 자루가 10px 이하가 되게(손이 발에서 8px 위라 길면 비스듬히 들 때 자루 끝이 땅을 뚫었다)
        frac = min(frac, 10.0 / max(hi - lo, 1.0))
        g = end_lo + d * (hi - lo) * frac
        gx, gy = int(round(g[0])), int(round(-g[1]))
        ang = math.degrees(math.atan2(d[1], d[0]))
        # 손잡이 픽셀이 투명이면 가장 가까운 불투명 픽셀로
        if out[min(max(gy, 0), h - 1), min(max(gx, 0), w - 1), 3] == 0:
            k = np.argmin((ox - gx) ** 2 + (oy - gy) ** 2)
            gx, gy = int(ox[k]), int(oy[k])
    # 거의 세로·가로·대각선이면 정확한 각도로 — 도트 그림을 몇 도만 돌리면 외곽이 지글거린다
    snapped = round(ang / 45.0) * 45.0
    if abs(snapped - ang) <= 15:
        ang = snapped
    ang = ANGLE_OVERRIDE.get(name, ang)
    if name in GRIP_FROM_BOTTOM:
        by = int(oy.max()) - GRIP_FROM_BOTTOM[name]
        row = ox[oy == by]
        gx, gy = int(row.min()) + 1, by  # 그 줄의 왼쪽(자루) 픽셀
    upright = kind in ("center", "pole", "staff")  # 방패·너클·활·책·깃발·지팡이: 손목 각도와 상관없이 똑바로 선다(지팡이는 옆에 세워 짚는다)
    os.makedirs(OUT, exist_ok=True)
    Image.fromarray(out).save(os.path.join(OUT, name + ".png"))
    with open(os.path.join(OUT, name + ".json"), "w", encoding="utf-8") as f:
        json.dump({"grip": [gx, gy], "size": [w, h], "angle": round(ang, 1), "scale": s, "kind": kind, "upright": upright}, f)
    print(f"{name}: x{s} -> {w}x{h} grip=({gx},{gy}) angle={ang:.0f} {kind}")


if __name__ == "__main__":
    names = sys.argv[1:] or list(KIND.keys())
    for n in names:
        restore(n)
