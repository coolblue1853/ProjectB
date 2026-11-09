"""M2 (연구 질문 2): 키 프레임 1장 라벨을 다음 프레임으로 전파(패치 정합 = 블록 매칭 광학 흐름).

  각 대상 픽셀 p 에 대해 출처 프레임에서 ±R 범위 이동 d 중 5×5 패치(RGB+알파) 차이 + λ·|d - d0| 가 최소인 것을 고르고
  출처 라벨 L[p-d] 를 가져온다(d0 = 발 기준 바운딩박스 정렬). 그 뒤 3×3 최빈값 1회, 빈 곳은 가장 가까운 라벨.
  전파 순서: 같은 클립 안에서는 바로 앞 프레임에서(연쇄), 각 클립 첫 프레임은 키 프레임에서.
추가 입력: 손 라벨 1장(labels/ds_idle_0_region.png).
사용: python m2_propagate.py <패키지> <키 라벨 폴더> <출력 폴더> [--direct]   (--direct = 모든 프레임을 키에서 바로)
"""
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

KEY = "ds_idle_0"
R = 12
LAM = 6.0


def feat(img):
    f = img.astype(float).copy()
    f[:, :, 3] = np.where(img[:, :, 3] > 128, 255, 0)
    f[img[:, :, 3] <= 128, :3] = 0
    return f


def anchor(img):
    ys, xs = np.nonzero(C.alpha(img))
    return np.array([ys.max(), (xs.min() + xs.max()) / 2.0])


def propagate(src_img, src_lab, dst_img):
    S, T = feat(src_img), feat(dst_img)
    d0 = np.round(anchor(dst_img) - anchor(src_img)).astype(int)
    best = np.full((64, 64), 1e18)
    lab = np.zeros((64, 64), np.uint8)
    for dy in range(d0[0] - R, d0[0] + R + 1):
        for dx in range(d0[1] - R, d0[1] + R + 1):
            Ss = C.shift(S, dy, dx)           # 출처를 d 만큼 옮겨 대상 좌표에 놓음
            Ls = C.shift(src_lab, dy, dx)
            diff = np.abs(T - Ss).sum(2)
            cost = np.zeros((64, 64))
            for ky in range(-2, 3):
                for kx in range(-2, 3):
                    cost += C.shift(diff, ky, kx)
            cost += LAM * 25 * (abs(dy - d0[0]) + abs(dx - d0[1]))
            upd = cost < best
            best[upd] = cost[upd]
            lab[upd] = Ls[upd]
    a = C.alpha(dst_img)
    lab[~a] = 0
    # 3×3 최빈값
    votes = np.zeros((9, 64, 64), int)
    for ky in range(-1, 2):
        for kx in range(-1, 2):
            sl = C.shift(lab, ky, kx)
            for k in range(1, 9):
                votes[k] += sl == k
    mode = votes[1:].argmax(0).astype(np.uint8) + 1
    lab = np.where(a & (votes[1:].max(0) >= 5), mode, lab).astype(np.uint8)
    lab = C.nearest_fill(lab, a)
    lab[~a] = 0
    return lab


def main():
    pkg, labdir, out = sys.argv[1:4]
    direct = "--direct" in sys.argv
    os.makedirs(out, exist_ok=True)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    key = C.load_maps(labdir)[KEY]
    res = {KEY: key}
    prev = None
    for f in C.FRAMES:
        if f == KEY:
            prev = f
            continue
        clip = f.rsplit("_", 1)[0]
        src = KEY if (direct or prev is None or prev.rsplit("_", 1)[0] != clip) else prev
        res[f] = propagate(coat[src], res[src], coat[f])
        prev = f
    for f, lab in res.items():
        C.save_map(lab, os.path.join(out, f + "_region.png"))
    print("M2 saved", len(res), "direct" if direct else "chain")


if __name__ == "__main__":
    main()
