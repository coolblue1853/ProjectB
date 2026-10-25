"""정리 후 남은 색을 N색 이하로 줄인다(디더링 없음, 투명도 유지) — flatten_keep_detail 다음 단계.

flatten_keep_detail은 고대비 디테일을 보호하느라 27~39색이 남을 수 있다(원본 18색).
검정 외곽선은 그대로 지키고, 나머지 색만 median cut으로 묶는다.
사용: python Tools/pixelfx/cap_colors.py in.png out.png [--colors 20]
"""
import argparse

import numpy as np
from PIL import Image


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("src")
    ap.add_argument("dst")
    ap.add_argument("--colors", type=int, default=20)
    a = ap.parse_args()
    im = np.array(Image.open(a.src).convert("RGBA"))
    op = im[..., 3] >= 128
    black = op & (im[..., :3].astype(int).sum(2) <= 30)
    rest = op & ~black
    px = im[..., :3][rest].reshape(-1, 1, 3)
    q = Image.fromarray(px.astype(np.uint8), "RGB").quantize(colors=a.colors - 1, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
    im[..., :3][rest] = np.array(q.convert("RGB")).reshape(-1, 3)
    im[..., :3][black] = 0
    im[..., 3] = np.where(op, 255, 0)
    Image.fromarray(im, "RGBA").save(a.dst)
    print(a.dst, "colors", len(np.unique(im[op][:, :3], axis=0)))


if __name__ == "__main__":
    main()
