"""셀 애니메이션 루프·흔들림 판정(Q-423 셀 방식): 프레임 폴더(0.png, 1.png …)를 읽어 프레임별 수치를 낸다.

- diff: 이전 프레임과 다른 픽셀 수(움직임 크기). seam: 마지막 → 첫 프레임 diff — 다른 프레임 diff 평균보다 크게 튀면 루프 이음새가 끊긴다.
- top/bottom/left/right: 불투명 영역 경계. bottom(발바닥) 이 프레임마다 바뀌면 땅에서 떠 보이거나 출렁인다(걷기·달리기는 0~1px 이내 권장).
- h: 키(머리 위 ~ 발). 4px 넘게 바뀌면 캐릭터가 커졌다 작아지는 것처럼 보인다.
- colors: 불투명 픽셀 색 수(원본 팔레트 ≈ 20).
사용: python Tools/pixelfx/cel_check.py <폴더> [시작 끝]   (끝 포함, 기본 = 전부)
"""
import os
import sys

import numpy as np
from PIL import Image


def load(folder, a=None, b=None):
    fs = sorted([f for f in os.listdir(folder) if f.endswith(".png") and f[:-4].isdigit()], key=lambda x: int(x[:-4]))
    if a is not None:
        fs = [f for f in fs if a <= int(f[:-4]) <= b]
    return fs, [np.array(Image.open(os.path.join(folder, f)).convert("RGBA")) for f in fs]


def bbox(a):
    ys, xs = np.nonzero(a[..., 3] > 0)
    return ys.min(), ys.max(), xs.min(), xs.max()


def diff(a, b):
    return int(np.any(a != b, axis=2).sum())


def main():
    folder = sys.argv[1]
    a = int(sys.argv[2]) if len(sys.argv) > 3 else None
    b = int(sys.argv[3]) if len(sys.argv) > 3 else None
    names, ims = load(folder, a, b)
    ds = []
    for i, im in enumerate(ims):
        t, bt, l, r = bbox(im)
        d = diff(ims[i - 1], im) if i else 0
        if i:
            ds.append(d)
        cols = len({tuple(p) for p in im[im[..., 3] > 0][:, :3]})
        print(f"{names[i]:>7} diff {d:4d}  top {t:2d} bottom {bt:2d} left {l:2d} right {r:2d} h {bt - t:2d} colors {cols}")
    seam = diff(ims[-1], ims[0])
    avg = sum(ds) / max(1, len(ds))
    bots = [bbox(im)[1] for im in ims]
    hs = [bbox(im)[1] - bbox(im)[0] for im in ims]
    print(f"seam {seam} vs avg diff {avg:.0f} → {'OK' if seam <= avg * 1.5 else '이음새 튐'};"
          f" bottom range {min(bots)}~{max(bots)}; height range {min(hs)}~{max(hs)}")


if __name__ == "__main__":
    main()
