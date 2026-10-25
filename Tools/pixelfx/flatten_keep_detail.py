"""캐릭터 시안을 담백하게 — 면 안의 저대비 잔무늬만 지우고, 대비 큰 디테일(외곽선·눈·버클·하이라이트)은 지킨다.

ArtStyle 0절 "시안 2차 피드백": 전체 색 감축(B안)은 디테일까지 뭉개져 "색만 뭉개진 것"처럼 보였다.
1) 색 병합: 색 공간(Lab 근사)에서 가까운 색끼리만 합친다(거리 < MERGE). 대비 큰 색은 수가 적어도 남긴다.
2) 잔무늬 제거: 주변 8칸 다수색과 '비슷한' 외톨이 픽셀만 다수색으로 바꾼다(거리 < SPECK). 다수색과 많이 다르면 의도된 디테일로 보고 둔다.
3) 검정 외곽선은 건드리지 않는다.
4) --head-auto: 머리(불투명 영역 위쪽 --head-frac, 기본 0.55)는 따로 약하게(--head-merge·--head-speck) 정리 —
   담백화는 몸·옷·소품에만 강하게, 머리카락은 2~3단 가닥 음영을 남긴다(CharacterPixelRules 4절).
사용: python Tools/pixelfx/flatten_keep_detail.py <in.png> <out.png> [--merge 14] [--speck 28] [--iters 3]
      [--head-auto --head-frac 0.55 --head-merge 6 --head-speck 20]
"""
import argparse
from collections import Counter

import numpy as np
from PIL import Image


def lab(c):
    r, g, b = [v / 255.0 for v in c]
    # 간단한 지각 가중 거리용 근사(sRGB 가중 유클리드)
    return np.array([r * 0.299 + g * 0.587 + b * 0.114, r - g, (r + g) / 2 - b]) * 100


def dist(a, b):
    return float(np.linalg.norm(lab(a) - lab(b)))


def is_black(c):
    return sum(c) < 90


def merge_colors(px, alpha, thr):
    cnt = Counter(tuple(int(v) for v in px[y, x]) for y, x in zip(*np.where(alpha)))
    order = [c for c, _ in cnt.most_common()]
    rep = {}
    keep = []
    for c in order:
        if is_black(c):
            rep[c] = c
            continue
        best = None
        for k in keep:
            d = dist(c, k)
            if d < thr and (best is None or d < best[0]):
                best = (d, k)
        if best:
            rep[c] = best[1]
        else:
            keep.append(c)
            rep[c] = c
    out = px.copy()
    for y, x in zip(*np.where(alpha)):
        out[y, x] = rep[tuple(int(v) for v in px[y, x])]
    return out, len(keep)


def despeckle(px, alpha, thr, iters):
    H, W = alpha.shape
    for _ in range(iters):
        out = px.copy()
        changed = 0
        for y in range(H):
            for x in range(W):
                if not alpha[y, x]:
                    continue
                c = tuple(int(v) for v in px[y, x])
                if is_black(c):
                    continue
                nb = []
                for dy in (-1, 0, 1):
                    for dx in (-1, 0, 1):
                        if (dy or dx) and 0 <= y + dy < H and 0 <= x + dx < W and alpha[y + dy, x + dx]:
                            n = tuple(int(v) for v in px[y + dy, x + dx])
                            if not is_black(n):
                                nb.append(n)
                if len(nb) < 4:
                    continue
                cnt = Counter(nb)
                best, n = cnt.most_common(1)[0]
                if cnt.get(c, 0) <= 1 and n >= 4 and best != c and dist(c, best) < thr:
                    out[y, x] = best
                    changed += 1
        px = out
        if not changed:
            break
    return px


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("src")
    ap.add_argument("dst")
    ap.add_argument("--merge", type=float, default=18)
    ap.add_argument("--speck", type=float, default=30)
    ap.add_argument("--iters", type=int, default=3)
    ap.add_argument("--head-auto", action="store_true")
    ap.add_argument("--head-frac", type=float, default=0.55)
    ap.add_argument("--head-merge", type=float, default=6)
    ap.add_argument("--head-speck", type=float, default=20)
    a = ap.parse_args()
    im = np.array(Image.open(a.src).convert("RGBA"))
    alpha = im[..., 3] > 128
    px = im[..., :3].astype(int)
    body, n = merge_colors(px, alpha, a.merge)
    body = despeckle(body, alpha, a.speck, a.iters)
    if a.head_auto:
        ys = np.where(alpha.any(1))[0]
        cut = int(ys.min() + (ys.max() - ys.min() + 1) * a.head_frac)
        head, _ = merge_colors(px, alpha, a.head_merge)
        head = despeckle(head, alpha, a.head_speck, a.iters)
        body[:cut] = head[:cut]
    px = body
    out = np.dstack([px, np.where(alpha, 255, 0)]).astype(np.uint8)
    Image.fromarray(out, "RGBA").save(a.dst)
    print("colors kept", n + 1)


if __name__ == "__main__":
    main()
