"""비교 촬영(cmp_<태그>_<동작>.png, 0.04초 간격 256px 컷)에서 프레임마다 발(가장 아래 어두운 덩어리 두 개) 위치와 몸 윗선 높이를 뽑는다(Q-423 프레임 분석).
사용: python Tools/pixelfx/track_feet.py <태그> <동작>   출력: 프레임, 뒷발 x·y, 앞발 x·y, 발 간격, 윗선 y (화면 px, 256 셀 기준 — 아트 1px ≈ 4.6 화면 px)
"""
import sys

import numpy as np
from PIL import Image


def feet(cell):
    band = cell[170:250, 30:226]
    dark = band.sum(axis=2) < 70
    ys, xs = np.nonzero(dark)
    if len(xs) < 4:
        return None
    # 가장 아래 12줄 안의 어두운 픽셀만(발바닥 근처)
    low = ys >= ys.max() - 12
    xs, ys = xs[low], ys[low]
    c = np.array([xs.min(), xs.max()], float)
    for _ in range(10):
        g = np.abs(xs[:, None] - c[None, :]).argmin(axis=1)
        c = np.array([xs[g == k].mean() if np.any(g == k) else c[k] for k in range(2)])
    out = []
    for k in range(2):
        m = g == k
        out.append((xs[m].mean() + 30, ys[m].mean() + 170, ys[m].max() + 170))
    return sorted(out)


def top(cell, bg):
    diff = np.abs(cell.astype(int) - bg.astype(int)).sum(axis=2) > 60
    rows = np.nonzero(diff[:, 60:200].any(axis=1))[0]
    return rows.min() if len(rows) else -1


def main():
    tag, action = sys.argv[1], sys.argv[2]
    s = np.array(Image.open(f"Logs/playtest/cmp_{tag}_{action}.png").convert("RGB")).astype(int)
    n = s.shape[1] // 256
    for i in range(n):
        c = s[:, i * 256:(i + 1) * 256]
        f = feet(c)
        if f is None:
            print(i, "-")
            continue
        (bx, by, bmax), (fx, fy, fmax) = f
        print(f"{i:2d} back({bx:5.1f},{bmax:4.0f}) front({fx:5.1f},{fmax:4.0f}) gap {fx - bx:5.1f} lift {abs(bmax - fmax):3.0f}")


if __name__ == "__main__":
    main()
