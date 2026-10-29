"""뒷모습 생성 프레임에 배낭 덧붙이기(10-29 밤): 두 팔이 등 앞을 지나는 회전 프레임은 생성기가 배낭을 빼먹는다(같은 seed 의 차기 프레임엔 있음).
방향 기준 그림의 배낭 영역을 스카프(주황) 위치로 맞춰 그대로 덮는다 — 뒷모습에서 팔은 몸 너머라 배낭이 위에 오는 게 맞다.
사용: python Tools/pixelfx/paste_backpack.py <기준 그림 이름(north·ne)> <프레임 png>...  (제자리 수정)
"""
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
DIRS = os.path.join(ROOT, "Docs", "ArtCompare", "CharacterGen", "dirs")
BOX = {"north": (28, 37, 41, 48), "ne": (26, 36, 38, 49)}   # 배낭 영역(x0, y0, x1, y1 포함) — 팔·다리 제외


def scarf(a):
    r, g, b, al = [a[..., i].astype(int) for i in range(4)]
    m = (al > 0) & (r > 180) & (g > 110) & (g < 190) & (b < 90)
    ys, xs = np.nonzero(m)
    return (int(round(xs.mean())), int(ys.min())) if len(xs) else None


def main():
    view = sys.argv[1]
    ref = np.array(Image.open(os.path.join(DIRS, view + ".png")).convert("RGBA"))
    rs = scarf(ref)
    x0, y0, x1, y1 = BOX[view]
    for p in sys.argv[2:]:
        a = np.array(Image.open(p).convert("RGBA"))
        fs = scarf(a)
        if not fs:
            print(p, "스카프 없음 — 건너뜀")
            continue
        dx, dy = fs[0] - rs[0], fs[1] - rs[1]
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if ref[y, x, 3] and 0 <= y + dy < 64 and 0 <= x + dx < 64:
                    a[y + dy, x + dx] = ref[y, x]
        Image.fromarray(a, "RGBA").save(p)
        print(p, "이동", dx, dy)


if __name__ == "__main__":
    main()
