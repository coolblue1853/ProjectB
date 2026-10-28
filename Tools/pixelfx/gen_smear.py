"""기본 칼 3타 스미어(스컬 리틀 본식) — fx_plan.py 로 사용자 승인받은 면을 도트로 찍는다(10-28).
- 캐릭터 셀과 같은 좌표·같은 픽셀 크기(PPU 36)로 그리고, 피벗 = 셀 원점(발 가운데 41, 54)이라 셀과 정확히 겹친다.
  예전 궤적(Px_WSlashDown, 24 PPU)은 도트가 캐릭터보다 1.5배 굵고 위치를 판정 기준점에 맞춰 칼과 따로 놀았다.
- 프레임: ① 휘두르는 호 전체를 덮는 꽉 찬 초승달(안쪽 비움, 진행 끝쪽 두껍게) ② 바깥 가장자리 얇은 호 ③ 더 짧고 흐린 호. 20fps.
- 회색조 밝기(PixelSlash2D 가 무기 테마 램프로 칠한다): 진행 끝 1.0 → 꼬리 0.6, 바깥 가장자리 밝게, 안쪽 가장자리 0.7.
사용: python Tools/pixelfx/gen_smear.py   등록: Tools/probes/build_smear.cs
"""
import json
import math
import os

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "..")
OUT = os.path.join(ROOT, "Assets", "Art", "PixelFx")
KEYS = os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel", "basic_sword", "skel")
CEL = os.path.join(ROOT, "Assets", "Art", "Player", "PixelRig", "Cel", "cel.json")
W, H = 112, 80          # 셀(64)보다 넓게 — 큰 초승달이 잘리지 않게
OX, OY = 24, 16          # 셀 좌표 → 시트 좌표
PIV = (41 + OX, 54 + OY)  # 셀 원점(발 가운데)
# 이름: (keys, 동작, 면 프레임, 종류, 시작각, 끝각, 반지름, 두께) — fx_plan.py CRESCENT 와 같다
SPECS = {
    "Px_WSmearDown": ("keys_v13.json", "SwingDown", 3, "arc_cw", -40, 175, 23, 14),  # 시계 방향(뒤 위 → 앞 아래): 두꺼운 끝이 앞 아래
    "Px_WSmearUp": ("keys_v18.json", "SwingUp", 4, "arc", -100, 165, 23, 14),
    "Px_WSmearStab": ("keys_v6.json", "Stab", 4, "streak", 0, 0, 26, 5),
}


def cel_shift(clip, i, keys_hand):
    """셀은 프레임마다 sx·dy 로 정렬돼 있다 — 손 좌표 차이로 같은 이동을 스미어에도 준다."""
    cj = json.load(open(CEL))[clip]["frames"][i]
    return cj["hand"][0] - keys_hand[0], cj["hand"][1] - keys_hand[1]


def arc_frame(c, a0, a1, r, thick, mode, cw=False):
    yy, xx = np.mgrid[0:H, 0:W]
    x = xx + 0.5 - c[0]
    y = c[1] - (yy + 0.5)
    d = np.hypot(x, y)
    span = (a1 - a0) % 360 or 360
    t = ((np.degrees(np.arctan2(y, x)) - a0) % 360) / span
    on = t <= 1.0
    if cw:
        t = 1.0 - t  # 진행 방향 기준(0 = 시작, 1 = 칼이 있는 끝)
    if mode == 0:      # 꽉 찬 초승달: 끝쪽 두껍고 시작 쪽 0, 맨 끝은 둥글게 닫힘
        w = thick * np.clip(t * 1.3, 0, 1) * (1 - np.clip((t - 0.93) * 12, 0, 1))
        rr = r
    elif mode == 1:    # 얇은 호(뒤쪽 40%는 지워짐)
        w = 2.4 * np.clip((t - 0.4) * 3, 0, 1)
        rr = r + 0.5
    else:              # 더 짧고 얇은 호
        w = 1.4 * np.clip((t - 0.7) * 5, 0, 1)
        rr = r + 1
    m = on & (d <= rr) & (d >= rr - w) & (w > 0.3)
    b = np.where(d >= rr - 1.2, 1.0, np.where(d <= rr - w + 1.2, 0.7, 0.88))
    b = b * (0.6 + 0.4 * np.clip(t, 0, 1)) if mode == 0 else b * (0.75 + 0.25 * t)
    return m, b


def streak_frame(h, ang, r, thick, mode):
    yy, xx = np.mgrid[0:H, 0:W]
    ca, sa = math.cos(math.radians(ang)), math.sin(math.radians(ang))
    x = xx + 0.5 - h[0]
    y = h[1] - (yy + 0.5)
    along = (x * ca + y * sa) / r
    side = -x * sa + y * ca
    if mode == 0:
        half = 0.6 + thick * 0.5 * np.clip(1 - along, 0, 1) * np.clip(along * 4, 0, 1)
        m = (along >= 0) & (along <= 1.15) & (np.abs(side) <= half)
    elif mode == 1:
        m = (along >= 0.35) & (along <= 1.25) & (np.abs(side) <= 0.6)
    else:
        m = (along >= 0.8) & (along <= 1.3) & (np.abs(side) <= 0.5)
    b = 0.7 + 0.3 * np.clip(along, 0, 1)
    return m, b


def render(spec):
    kf, clip, i, kind, a0, a1, r, thick = spec
    k = json.load(open(os.path.join(KEYS, kf)))[clip]
    sx, dy = cel_shift(clip, i, k["hand"][i])
    kp = {j["label"]: (j["x"] * 64 + sx + OX, j["y"] * 64 + dy + OY) for j in k["kp"][i]}
    frames = []
    for mode in range(3):
        if kind.startswith("arc"):
            m, b = arc_frame(kp["RIGHT SHOULDER"], a0, a1, r, thick, mode, cw=kind == "arc_cw")
        else:
            h = (k["hand"][i][0] + sx + OX, k["hand"][i][1] + dy + OY)
            m, b = streak_frame(h, k["angle"][i], r, thick, mode)
        img = np.zeros((H, W, 4), np.uint8)
        v = (np.clip(b, 0, 1) * 255).astype(np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(m, 255, 0)
        frames.append(img)
    return frames


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, spec in SPECS.items():
        frames = render(spec)
        Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
        pivot = [PIV[0] / W, (H - (PIV[1] + 1)) / H]  # 셀 임포트와 같은 식(PixelRigBuilder.ImportPart)
        with open(os.path.join(OUT, name + ".json"), "w") as fp:
            json.dump({"frames": len(frames), "w": W, "h": H, "fps": 20, "pivot": pivot, "ramp": "gray",
                       "source": "Tools/pixelfx/gen_smear.py"}, fp)
        print(name, pivot)


if __name__ == "__main__":
    main()
