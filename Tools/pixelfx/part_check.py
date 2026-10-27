"""부위 크기 검사(10-27 사용자 1·4번 규칙): 셀마다 머리(스카프 윗줄 위) 면적·폭·높이와 몸(스카프 아래) 면적을 대기와 비교한다.
사용: python Tools/pixelfx/part_check.py 클립,...   ±8% 넘으면 * 표시
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from build_cel import SCARF, near  # noqa: E402

P = "Assets/Art/Player/PixelRig/Cel/"


def parts(name):
    a = np.array(Image.open(P + name + ".png").convert("RGBA"))
    m = a[..., 3] > 0
    sc = np.nonzero(near(a, SCARF).any(axis=1))[0]
    top = sc.min() if len(sc) else 30
    head = m[:top]
    ys, xs = np.nonzero(head)
    return int(head.sum()), int(xs.max() - xs.min() + 1) if len(xs) else 0, int(top - ys.min()) if len(ys) else 0


def main():
    d = json.load(open(P + "cel.json", encoding="utf-8"))
    ref = parts("Idle_0")
    print("Idle", ref)
    for c in sys.argv[1].split(","):
        out = []
        for k in range(len(d[c]["frames"])):
            v = parts(f"{c}_{k}")
            bad = abs(v[0] - ref[0]) / ref[0] > 0.08
            out.append(f"{v}{'*' if bad else ''}")
        print(c, " ".join(out))


if __name__ == "__main__":
    main()
