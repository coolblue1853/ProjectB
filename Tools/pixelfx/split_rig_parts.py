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
# 팔 = 소매 + 손 (10-25 수정: 손만 떼면 팔을 돌릴 때 손이 공중에 떠서 "팔이 뜯어져" 보였다)
# 몸통은 x16·x26 의 진한 선이 자체 외곽선이라 팔을 떼도 구멍이 생기지 않는다
ARM_L = (12, 16, 38, 49)       # x0, x1(미포함), y0, y1 — 화면 왼쪽 팔(반전 후 캐릭터의 앞팔)
ARM_R = (27, 32, 39, 49)       # 화면 오른쪽 팔(배낭 쪽, 뒷팔)
ARM_L_INNER = (14, 15)          # 팔에서 몸통 쪽 2열 — 몸통에도 겹쳐 그린다
ARM_R_INNER = (27, 28)
LEG_TOP = 50                   # 이 행부터 다리
LEG_SPLIT_X = 22               # 이보다 왼쪽 = 왼다리
LEG_MAX_X = 29
LEG_OVERLAP = 2                # 몸통에 겹쳐 그릴 다리 윗줄 수
HEAD_BOTTOM = 35               # 이 행까지 머리
PACK_X = 31                    # 이 열부터(31~50행) 배낭
PACK_ROWS = (31, 51)
PACK_SHADOW = (55, 26, 32, 255)  # 배낭 가장 어두운 갈색 — 뒷팔 뒤 메움색

# 피벗(관절점, 원본 좌표 픽셀 중심)
PIVOTS = {
    "hips": (22, 49), "pack": (33, 37), "head": (22, 35),
    "shoulder_l": (14, 38), "hand_l": (13, 46),
    "shoulder_r": (29, 38), "hand_r": (29, 46),
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
            if ARM_L[0] <= x < ARM_L[1] and ARM_L[2] <= y < ARM_L[3]:
                lab[y, x] = "arm_l"
            elif ARM_R[0] <= x < ARM_R[1] and ARM_R[2] <= y < ARM_R[3] and not (x == 31 and y == 39):
                lab[y, x] = "arm_r"
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
    parts = ["pack", "torso", "head", "arm_l", "arm_r", "leg_l", "leg_r"]
    w = a.shape[1]
    for p in parts:
        img = np.zeros_like(a)
        m = lab == p
        img[m] = a[m]
        # 머리 아래 1행은 몸통 위로 겹쳐 그려 둔다(머리가 1px 들썩여도 목에 틈이 안 보이게)
        # 뒷팔이 움직이면 팔 자리가 비어 몸과 배낭 사이로 배경이 보였다 → 배낭 그림자색으로 메운다(정지 땐 팔에 가려 안 보임)
        if p == "pack":
            img[lab == "arm_r"] = PACK_SHADOW
            # 겹침: 배낭 맨 윗줄을 한 줄 위에도 그린다(정지 땐 머리카락 뒤에 가려짐) — 배낭이 2차 움직임으로 1px 내려가면
            # 머리카락과 배낭 사이로 배경이 비쳤다
            packm = lab == "pack"
            for xx in range(packm.shape[1]):
                ys_ = np.where(packm[:, xx])[0]
                if len(ys_) and ys_[0] > 0 and img[ys_[0] - 1, xx, 3] == 0 and lab[ys_[0] - 1, xx] == "head":
                    img[ys_[0] - 1, xx] = a[ys_[0], xx]
        if p == "torso":
            img[HEAD_BOTTOM] = np.where((lab[HEAD_BOTTOM + 1] == "torso")[:, None], a[HEAD_BOTTOM + 1], img[HEAD_BOTTOM])
            # 겹침(원본 AnyPortrait 리깅 방식: 맞닿는 줄을 양쪽 부위에 다 그린다): 다리 윗부분 2줄을 몸통에도 그려 둔다.
            # 몸통이 다리 위에 그려지므로 정지 자세는 같고, 다리가 1~2px 움직여도 반바지 아래로 배경이 비치지 않는다
            # 팔 안쪽 2줄도 몸통에 그려 둔다 — 팔이 1px 옆으로 움직여도 팔과 몸 사이로 배경이 비치지 않게(10-25 "몸에 빈칸")
            for arm, cols in (("arm_l", ARM_L_INNER), ("arm_r", ARM_R_INNER)):
                for xx in cols:
                    col = lab[:, xx] == arm
                    img[col, xx] = a[col, xx]
            for yy in range(LEG_TOP, LEG_TOP + LEG_OVERLAP):
                legs = (lab[yy] == "leg_l") | (lab[yy] == "leg_r")
                img[yy][legs] = a[yy][legs]
        if mirror:
            img = img[:, ::-1]
        Image.fromarray(img).save(os.path.join(out, f"{p}.png"))
        print(p, int(m.sum()), "px")
    piv = {k: ([w - 1 - x, y] if mirror else [x, y]) for k, (x, y) in PIVOTS.items()}
    with open(os.path.join(out, "pivots.json"), "w", encoding="utf-8") as f:
        json.dump({"size": [w, a.shape[0]], "mirror": mirror, "pivots": piv}, f, indent=1)
    # 검증: 부위 합성 = 원본
    comp = np.zeros_like(a)
    for p in ["pack", "arm_r", "leg_r", "leg_l", "torso", "head", "arm_l"]:
        part = np.array(Image.open(os.path.join(out, f"{p}.png")))
        comp = np.where(part[..., 3:4] > 0, part, comp)
    ref = a[:, ::-1] if mirror else a
    print("recompose diff px", int((np.abs(comp.astype(int) - ref.astype(int)).sum(2) > 0).sum()))


if __name__ == "__main__":
    main()
