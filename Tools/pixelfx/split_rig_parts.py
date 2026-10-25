"""캐릭터 도트(64×64)를 픽셀 리그 부위로 나눈다 — PixelRigDesign.md 1절.

부위마다 64×64 투명 캔버스 PNG(그 부위 픽셀만) + 피벗(관절점) JSON을 만든다. 모든 부위를 피벗 위치에 겹치면 원본과 픽셀 단위로 같다.
좌표 규칙은 원본(왼쪽을 보는 그림) 기준으로 적고, --mirror 면 저장 직전에 좌우 반전한다(게임은 오른쪽을 보는 것이 기본).

사용: python Tools/pixelfx/split_rig_parts.py <src.png> <out_dir> [--mirror]
"""
import json
import os
import sys

import numpy as np
from PIL import Image

# 기본 모험가(clean_5102) 기준 경계 — 원본 좌표(x 오른쪽, y 아래)
HAND_L = (12, 16, 43, 49)      # x0, x1(미포함), y0, y1 — 화면 왼쪽 손(캐릭터의 앞손)
HAND_R = (28, 33, 42, 49)      # 화면 오른쪽 손(배낭 쪽, 캐릭터의 뒷손)
LEG_TOP = 50                   # 이 행부터 다리
LEG_SPLIT_X = 22               # 이보다 왼쪽 = 왼다리
LEG_MAX_X = 29
HEAD_BOTTOM = 35               # 이 행까지 머리
PACK_X = 31                    # 이 열부터(31~50행) 배낭
PACK_ROWS = (31, 51)

# 피벗(관절점, 원본 좌표 픽셀 중심)
PIVOTS = {
    "hips": (22, 49), "pack": (33, 37), "head": (22, 35),
    "shoulder_l": (15, 39), "hand_l": (14, 46),
    "shoulder_r": (28, 39), "hand_r": (29, 46),
    "leg_l": (18, 50), "leg_r": (25, 50),
}


def label(a):
    h, w = a.shape[:2]
    lab = np.full((h, w), "", dtype=object)
    op = a[..., 3] >= 128
    for y in range(h):
        for x in range(w):
            if not op[y, x]:
                continue
            if HAND_L[0] <= x < HAND_L[1] and HAND_L[2] <= y < HAND_L[3]:
                lab[y, x] = "hand_l"
            elif HAND_R[0] <= x < HAND_R[1] and HAND_R[2] <= y < HAND_R[3]:
                lab[y, x] = "hand_r"
            elif x >= PACK_X and PACK_ROWS[0] <= y < PACK_ROWS[1] or (x >= LEG_MAX_X + 1 and y >= LEG_TOP - 1):
                lab[y, x] = "pack"
            elif y >= LEG_TOP and x <= LEG_MAX_X:
                lab[y, x] = "leg_l" if x < LEG_SPLIT_X else "leg_r"
            elif y <= HEAD_BOTTOM:
                lab[y, x] = "head"
            else:
                lab[y, x] = "torso"
    return lab


def main():
    src, out = sys.argv[1], sys.argv[2]
    mirror = "--mirror" in sys.argv
    a = np.array(Image.open(src).convert("RGBA"))
    lab = label(a)
    os.makedirs(out, exist_ok=True)
    parts = ["pack", "torso", "head", "hand_l", "hand_r", "leg_l", "leg_r"]
    w = a.shape[1]
    for p in parts:
        img = np.zeros_like(a)
        m = lab == p
        img[m] = a[m]
        # 머리 아래 1행은 몸통 위로 겹쳐 그려 둔다(머리가 1px 들썩여도 목에 틈이 안 보이게)
        if p == "torso":
            img[HEAD_BOTTOM] = np.where((lab[HEAD_BOTTOM + 1] == "torso")[:, None], a[HEAD_BOTTOM + 1], img[HEAD_BOTTOM])
        if mirror:
            img = img[:, ::-1]
        Image.fromarray(img).save(os.path.join(out, f"{p}.png"))
        print(p, int(m.sum()), "px")
    piv = {k: ([w - 1 - x, y] if mirror else [x, y]) for k, (x, y) in PIVOTS.items()}
    with open(os.path.join(out, "pivots.json"), "w", encoding="utf-8") as f:
        json.dump({"size": [w, a.shape[0]], "mirror": mirror, "pivots": piv}, f, indent=1)
    # 검증: 부위 합성 = 원본
    comp = np.zeros_like(a)
    for p in parts:
        part = np.array(Image.open(os.path.join(out, f"{p}.png")))
        comp = np.where(part[..., 3:4] > 0, part, comp)
    ref = a[:, ::-1] if mirror else a
    print("recompose diff px", int((np.abs(comp.astype(int) - ref.astype(int)).sum(2) > 0).sum()))


if __name__ == "__main__":
    main()
