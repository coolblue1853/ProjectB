"""기본 방어구 몸 프레임 영역 지도(CelEquipmentPlan 「부위 합성 설계 원칙」): 모든 불투명 픽셀을 정확히 한 부위에 배정한다.

부위: 0 투명, 1 모자, 2 상의, 3 장갑, 4 바지, 5 신발, 6 맨몸(얼굴·머리카락·고글·배낭 — 장비가 바꾸지 않는 부분)
방법: (1) 색·위치로 확실한 픽셀을 먼저 분류(코트=청록 계열, 장갑=머리 아래 살색 덩어리, 신발=아래쪽 갈색, 모자=고글 위 머리카락, 바지=허리 아래 어두운 무채색) (2) 나머지(외곽선·음영)는 가장 가까운 확정 픽셀의 부위를 따른다 → 겹침·빈칸 0.
프레임별 보정은 region_overrides.json({프레임: {"cap_y": n, ...}})으로 한다.
사용: python Tools/pixelfx/region_map.py <셀 폴더> <출력 폴더> ds_idle_0 ds_idle_1 ...
출력: <프레임>_region.png(L, 부위 번호) + 확인용 <프레임>_region_view.png + 전체 시트 region_sheet.png
"""
import colorsys
import json
import os
import sys
from collections import deque

import numpy as np
from PIL import Image

CAP, TOP, GLOVE, LEGS, SHOES, BODY = 1, 2, 3, 4, 5, 6
COLORS = {CAP: (80, 200, 80), TOP: (60, 120, 230), GLOVE: (240, 220, 60), LEGS: (170, 80, 220), SHOES: (230, 100, 40), BODY: (150, 150, 150)}


def hsv(px):
    return colorsys.rgb_to_hsv(px[0] / 255, px[1] / 255, px[2] / 255)


def classify(arr, ov):
    h, w = arr.shape[:2]
    alpha = arr[:, :, 3] > 128
    lab = np.zeros((h, w), np.uint8)
    ys, xs = np.nonzero(alpha)
    top, foot = ys.min(), ys.max()
    # 고글(청록 밝은색) = 머리 기준선
    gy = None
    gp = [(y, x) for y, x in zip(ys, xs) if 0.44 < hsv(arr[y, x])[0] < 0.55 and hsv(arr[y, x])[1] > 0.55 and hsv(arr[y, x])[2] > 0.45]
    if gp:
        gy = float(np.mean([p[0] for p in gp]))
        gx = float(np.mean([p[1] for p in gp]))
    else:
        gy, gx = top + 8.0, float(xs.mean())
    cap_y = ov.get("cap_y", int(round(gy)) + 1)  # 이 줄 위(포함)의 머리 색 = 모자
    head_bottom = ov.get("head_bottom", int(round(gy)) + 9)  # 턱 아래 = 머리 아님
    waist = ov.get("waist", None)
    height = foot - top
    shoe_y = ov.get("shoe_y", foot - max(6, int(height * 0.14)))
    skin_hue = lambda p: 0.0 <= hsv(p)[0] <= 0.12 and 0.33 < hsv(p)[1] < 0.55 and hsv(p)[2] > 0.6
    # 1차 확정
    for y, x in zip(ys, xs):
        p = arr[y, x]
        hh, ss, vv = hsv(p)
        if y <= head_bottom and abs(x - gx) < 16:
            # 머리 영역: 모자(고글 위) / 맨몸
            if y <= cap_y and not (0.44 < hh < 0.55 and ss > 0.5) and not (ss < 0.45 and vv < 0.2):
                lab[y, x] = CAP
            else:
                lab[y, x] = BODY
            continue
        if 0.5 <= hh <= 0.62 and ss > 0.3:
            lab[y, x] = TOP
        elif skin_hue(p) and (y > head_bottom + 8 or abs(x - gx) > 10):
            lab[y, x] = GLOVE
        elif 0.02 <= hh <= 0.1 and ss > 0.45 and 0.25 < vv < 0.62 and y >= shoe_y:
            lab[y, x] = SHOES
        elif ss < 0.3 and vv < 0.3 and y > head_bottom + 12 and (waist is None or y >= waist):
            lab[y, x] = LEGS
        elif 0.02 <= hh <= 0.1 and ss > 0.45:
            lab[y, x] = BODY  # 배낭(갈색)
    # 2차: 나머지는 가장 가까운 확정 픽셀(BFS)
    q = deque()
    dist = -np.ones((h, w), int)
    for y, x in zip(*np.nonzero(lab)):
        q.append((y, x))
        dist[y, x] = 0
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w and alpha[ny, nx] and lab[ny, nx] == 0 and dist[ny, nx] < 0:
                lab[ny, nx] = lab[y, x]
                dist[ny, nx] = dist[y, x] + 1
                q.append((ny, nx))
    lab[~alpha] = 0
    return lab


def view(arr, lab, scale=6):
    h, w = lab.shape
    base = Image.new("RGBA", (w, h), (30, 30, 30, 255))
    rgb = np.array(base)
    for k, c in COLORS.items():
        m = lab == k
        rgb[m, :3] = c
    return Image.fromarray(rgb).resize((w * scale, h * scale), Image.NEAREST)


def main():
    cel, out = sys.argv[1], sys.argv[2]
    names = sys.argv[3:]
    os.makedirs(out, exist_ok=True)
    ovp = os.path.join(os.path.dirname(os.path.abspath(__file__)), "region_overrides.json")
    overrides = json.load(open(ovp)) if os.path.exists(ovp) else {}
    tiles = []
    for n in names:
        arr = np.array(Image.open(os.path.join(cel, n + ".png")).convert("RGBA"))
        lab = classify(arr, overrides.get(n, {}))
        Image.fromarray(lab, "L").save(os.path.join(out, n + "_region.png"))
        v = view(arr, lab, 4)
        o = Image.fromarray(arr).resize((arr.shape[1] * 4, arr.shape[0] * 4), Image.NEAREST)
        t = Image.new("RGBA", (v.width * 2, v.height), (60, 60, 60, 255))
        t.alpha_composite(o, (0, 0))
        t.alpha_composite(v, (v.width, 0))
        tiles.append(t)
    cols = 3
    rows = (len(tiles) + cols - 1) // cols
    sheet = Image.new("RGBA", (tiles[0].width * cols, tiles[0].height * rows), (60, 60, 60, 255))
    for i, t in enumerate(tiles):
        sheet.alpha_composite(t, ((i % cols) * t.width, (i // cols) * t.height))
    sheet.save(os.path.join(out, "region_sheet.png"))
    print("saved", len(tiles))


if __name__ == "__main__":
    main()
