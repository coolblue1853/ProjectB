"""합성 프레임(맨몸 셀 + 장비 레이어 + 칼 + 주먹, 게임과 같은 규칙) 도트 깨짐 자동 검사(생성 0).
검사 항목:
  dot   = 몸에서 떨어져 떠 있는 3px 이하 조각
  hole  = 실루엣 안 투명 구멍(바깥과 이어지지 않은 빈칸)
  edge  = 외곽선 없이 밝은 픽셀이 바로 바깥과 닿는 칸 수(장비 테두리 빠짐)
  spike = 이웃 불투명이 하나뿐인 어두운 픽셀(1px 삐침)
사용: python Tools/pixelfx/comp_lint.py [장비 id 쉼표(빈칸=맨몸)] [클립 쉼표]   예) python Tools/pixelfx/comp_lint.py ,iron_helm,iron_set Idle,Walk,Run
"""
import json
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, __file__.rsplit("\\", 1)[0].rsplit("/", 1)[0])
from build_cel import components  # noqa: E402

P = "Assets/Art/Player/PixelRig/"
BASE = True  # 장비 검사는 맨몸 대비 증가분


def body(clip, i, eq):
    b = np.array(Image.open(f"{P}Cel/{clip}_{i}.png").convert("RGBA")).copy()
    if eq:
        try:
            er = np.array(Image.open(f"{P}Equip/{eq}/{clip}_{i}_erase.png").convert("L")) > 0
            L = np.array(Image.open(f"{P}Equip/{eq}/{clip}_{i}.png").convert("RGBA"))
        except FileNotFoundError:
            return b
        b[er] = 0
        m = L[..., 3] > 0
        b[m] = L[m]
    return b


def lint(a):
    op = a[..., 3] > 0
    h, w = op.shape
    dots = sum(len(p) for p in components(op) if len(p) <= 3)
    out = np.zeros_like(op)
    stack = [(y, x) for y in range(h) for x in (0, w - 1) if not op[y, x]] + [(y, x) for x in range(w) for y in (0, h - 1) if not op[y, x]]
    for y, x in stack:
        out[y, x] = True
    while stack:
        y, x = stack.pop()
        for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
            if 0 <= ny < h and 0 <= nx < w and not op[ny, nx] and not out[ny, nx]:
                out[ny, nx] = True
                stack.append((ny, nx))
    holes = int((~op & ~out).sum())
    lum = a[..., :3].astype(int).sum(axis=2)
    light = op & (lum > 200)
    nb_out = np.zeros_like(op)
    nb_out[1:] |= out[:-1]; nb_out[:-1] |= out[1:]; nb_out[:, 1:] |= out[:, :-1]; nb_out[:, :-1] |= out[:, 1:]
    edge = int((light & nb_out).sum())
    n = np.zeros(op.shape, int)
    n[1:] += op[:-1]; n[:-1] += op[1:]; n[:, 1:] += op[:, :-1]; n[:, :-1] += op[:, 1:]
    spike = int((op & (lum < 120) & (n <= 1)).sum())
    return dots, holes, edge, spike


def main():
    eqs = (sys.argv[1] if len(sys.argv) > 1 else ",iron_helm,iron_set").split(",")
    d = json.load(open(P + "Cel/cel.json", encoding="utf-8"))
    clips = sys.argv[2].split(",") if len(sys.argv) > 2 else ["Idle", "Walk", "Run", "JumpCrouch", "JumpArc", "Land", "Dash", "Hurt"]
    bad = 0
    for eq in eqs:
        for c in clips:
            row = []
            for i in range(len(d[c]["frames"])):
                dots, holes, edge, spike = lint(body(c, i, eq))
                if eq and BASE:
                    # 장비가 새로 만든 깨짐만: 맨몸 그림 자체의 삐침·테두리(원래 그림)는 뺀다
                    b0 = lint(body(c, i, ""))
                    dots, holes, edge, spike = (max(0, v - v0) for v, v0 in zip((dots, holes, edge, spike), b0))
                flag = dots or holes or edge > 2 or spike
                bad += bool(flag)
                row.append(f"{i}:{'!' if flag else ''}d{dots}h{holes}e{edge}s{spike}")
            print(f"{eq or 'none':9s} {c:10s} " + " ".join(row))
    print("flagged", bad)


if __name__ == "__main__":
    main()
