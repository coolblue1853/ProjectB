"""배경이 남은 스프라이트에서 배경 제거 — 테두리에 닿은 영역을 색 유사도로 채워 투명하게 한다.

외곽선이 있는 캐릭터 도트용: 테두리부터 플러드필하고, 외곽선(짙은 색)에서 멈춘다.
사용: python Tools/pixelfx/remove_flat_bg.py in.png out.png [--tol 60] [--dark 160]
  --tol  : 이웃 배경 픽셀과의 색 거리 허용치(배경이 여러 색이면 크게)
  --dark : 밝기 합이 이 값보다 어두우면 외곽선으로 보고 채우지 않는다
"""
import argparse
from collections import deque

import numpy as np
from PIL import Image


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("src")
    ap.add_argument("dst")
    ap.add_argument("--tol", type=float, default=60)
    ap.add_argument("--dark", type=int, default=160)
    a = ap.parse_args()
    im = np.array(Image.open(a.src).convert("RGBA")).astype(int)
    H, W = im.shape[:2]
    rgb = im[..., :3]
    bg = np.zeros((H, W), bool)
    q = deque()
    for y in range(H):
        for x in (0, W - 1):
            q.append((y, x))
    for x in range(W):
        for y in (0, H - 1):
            q.append((y, x))
    for y, x in q:
        bg[y, x] = rgb[y, x].sum() >= a.dark
    q = deque([(y, x) for y, x in q if bg[y, x]])
    while q:
        y, x = q.popleft()
        for dy, dx in ((0, 1), (1, 0), (0, -1), (-1, 0)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < H and 0 <= nx < W and not bg[ny, nx]:
                c = rgb[ny, nx]
                if c.sum() >= a.dark and np.abs(c - rgb[y, x]).sum() <= a.tol:
                    bg[ny, nx] = True
                    q.append((ny, nx))
    im[..., 3] = np.where(bg, 0, im[..., 3])
    Image.fromarray(im.astype(np.uint8), "RGBA").save(a.dst)
    print(a.dst, "bg px", int(bg.sum()))


if __name__ == "__main__":
    main()
