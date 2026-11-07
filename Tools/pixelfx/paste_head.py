"""합격한 방향 그림의 머리를 모션 프레임 머리 자리에 덮어 붙인다(생성 0, 11-07 올려베기 M1-a).
사용: python Tools/pixelfx/paste_head.py <머리 기준 png> <대상 png> <출력 png> [dx dy] [x0 y0 x1 y1]
기본 영역: 열 14~37, 행 4~27(오른팔 열≥38 보존), 오프셋은 머리 실루엣이 가장 겹치는 값을 자동 탐색(±4)."""
import sys
import numpy as np
from PIL import Image


def main(a):
    ref = Image.open(a[0]).convert("RGBA")
    dst = Image.open(a[1]).convert("RGBA")
    box = [14, 4, 37, 27]
    if len(a) >= 8:
        box = [int(v) for v in a[-4:]]
    x0, y0, x1, y1 = box
    ra, da = np.array(ref), np.array(dst)
    tgt = da[y0:y1 + 1, x0:x1 + 1, 3] > 0
    tm = np.zeros((64, 64), bool)
    tm[y0:y1 + 1, x0:x1 + 1] = tgt
    best = None
    if len(a) >= 5 and a[3].lstrip("-").isdigit() and a[4].lstrip("-").isdigit():
        best = (0, int(a[3]), int(a[4]))
    else:
        for dx in range(-4, 5):
            for dy in range(-4, 5):
                m = np.zeros((64, 64), bool)
                for y in range(y0, y1 + 1):
                    for x in range(x0, x1 + 1):
                        sx, sy = x - dx, y - dy
                        if 0 <= sx < 64 and 0 <= sy < 64 and ra[sy, sx, 3] > 0:
                            m[y, x] = True
                sc = (m & tm).sum() - 0.5 * (m ^ tm).sum()
                if best is None or sc > best[0]:
                    best = (sc, dx, dy)
    _, dx, dy = best
    px, sp = dst.load(), ref.load()
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            px[x, y] = (0, 0, 0, 0)
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            sx, sy = x - dx, y - dy
            if 0 <= sx < 64 and 0 <= sy < 64 and sp[sx, sy][3] > 0:
                px[x, y] = sp[sx, sy]
    dst.save(a[2])
    print("offset", dx, dy)


if __name__ == "__main__":
    main(sys.argv[1:])
