"""셀 프레임 전수 검사(PixelRigDesign 3.7): 잡티(몸과 떨어진 작은 조각)·갇힌 구멍·발바닥 높이를 클립별로 보고한다.
사용: python Tools/pixelfx/cel_lint.py [셀 폴더 = Assets/Art/Player/PixelRig/Cel]
기준: 잡티 0, 구멍 0(머리카락 끝 1칸 정도는 원본에도 있음), 땅 동작은 발바닥 54행(공중·사망 제외).
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from build_cel import components  # noqa: E402

DIR = sys.argv[1] if len(sys.argv) > 1 else "Assets/Art/Player/PixelRig/Cel"
AIR = {"JumpArc", "Death"}


def holes(alpha):
    h, w = alpha.shape
    empty = ~alpha
    seen = np.zeros_like(empty)
    stack = [(y, x) for y in range(h) for x in (0, w - 1) if empty[y, x]] + [(y, x) for x in range(w) for y in (0, h - 1) if empty[y, x]]
    for y, x in stack:
        seen[y, x] = True
    while stack:
        y, x = stack.pop()
        for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
            if 0 <= ny < h and 0 <= nx < w and empty[ny, nx] and not seen[ny, nx]:
                seen[ny, nx] = True
                stack.append((ny, nx))
    return int((empty & ~seen).sum())


def main():
    meta = json.load(open(os.path.join(DIR, "cel.json")))
    bad = 0
    for clip, v in meta.items():
        line = []
        for f in v["frames"]:
            a = np.array(Image.open(os.path.join(DIR, f["png"] + ".png")).convert("RGBA"))
            al = a[..., 3] > 0
            parts = sorted((len(p) for p in components(al)), reverse=True)
            specks = sum(1 for n in parts[1:] if n < 30)
            hl = holes(al)
            bottom = int(np.nonzero(al.any(axis=1))[0].max())
            flag = specks or hl > 1 or (clip not in AIR and bottom != 54 and not (clip == "Run" and bottom == 53))  # 달리기 체공은 1px 뜸 허용
            bad += bool(flag)
            line.append(f"{f['png'].split('_')[-1]}:{'!' if flag else ''}s{specks}h{hl}b{bottom}")
        print(f"{clip:11s}", " ".join(line))
    print("flagged frames:", bad)


if __name__ == "__main__":
    main()
