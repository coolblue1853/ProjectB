"""M3 (연구 질문 3 + '코트를 뜯으면 바지가 빈다' 문제): 가려진 부분 보완 층(amodal fill).

표현: 지도(0~8)는 그대로 두고, 세트·부위마다 '보완 층'을 하나 더 둔다. 보완 층은 그 부위 레이어 바로 밑에 깔리고
      자기 부위 레이어가 없는 자리(다른 부위·소품에 가려 그림이 없던 곳)만 채운다.
  - 바지 보완: 속옷 몸의 다리 영역(허리선~부츠선) 중 그 세트 바지 레이어가 없는 곳(코트 자락·랜턴 밑에 가려진 레깅스).
  - 상의 보완: 속옷 몸의 몸통·팔(허리선 위, 머리·손 제외) 중 그 세트 상의 레이어가 없는 곳(배낭 밑에 가려진 상의).
  채우기 = 패치 복사: 같은 레이어에서 빈 곳을 가장 많이 덮는 한 오프셋으로 통째 복사(질감·음영 유지), 반복.
  생성 0, 추가 입력 = 속옷 몸 프레임. 자락(8)의 소속은 hem_to 로 바꿔 끼울 수 있다(질문 3의 '선택 가능한 층').
사용: python m3_amodal.py <패키지> <지도 폴더> <출력 폴더>   # 세트별 보완 층 PNG 를 <출력>/<세트>/<부위>_fill/<프레임>.png 로 저장
"""
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import m1_underdiff as M1

_PKG = None
_GEO = {}


def set_pkg(pkg):
    global _PKG
    _PKG = pkg


def geometry(f):
    if f not in _GEO:
        pkg = _PKG or os.environ.get("LAYER_PKG")
        u = C.load(os.path.join(pkg, "frames_underwear_base", f + ".png"))
        _GEO[f] = (u, M1.under_geometry(u))
    return _GEO[f]


def extend(img, have, todo, axis):
    """todo 픽셀을 have 픽셀 색으로: axis=0 이면 같은 열 아래쪽 가장 가까운 것, 1 이면 같은 줄 가장 가까운 것, 없으면 BFS 최근접."""
    out = np.zeros_like(img)
    done = np.zeros(have.shape, bool)
    ys, xs = np.nonzero(todo)
    for y, x in zip(ys, xs):
        if axis == 0:
            col = np.nonzero(have[y:, x])[0]
            if len(col):
                out[y, x] = img[y + col[0], x]; done[y, x] = True
        else:
            row = np.nonzero(have[y])[0]
            if len(row):
                out[y, x] = img[y, row[np.argmin(np.abs(row - x))]]; done[y, x] = True
    rest = todo & ~done
    if rest.any() and have.any():
        idx = np.zeros(have.shape, np.int32)
        hy, hx = np.nonzero(have)
        idx[hy, hx] = np.arange(1, len(hy) + 1)
        filled = C.nearest_fill(idx, have | rest, seeds=have)
        ry, rx = np.nonzero(rest & (filled > 0))
        k = filled[ry, rx] - 1
        out[ry, rx] = img[hy[k], hx[k]]
    return out


def patch_copy(img, have, todo, max_d=14, prefer=(1, 0)):
    """빈 곳을 같은 레이어의 다른 자리에서 '한 오프셋으로 통째' 복사(질감·음영 유지). 남은 곳이 없어질 때까지 오프셋을 바꿔 반복.
    prefer = 오프셋 방향 가중(바지: 아래쪽에서, 상의: 옆에서)."""
    out = np.zeros_like(img)
    rest = todo.copy()
    offs = [(dy, dx) for dy in range(-max_d, max_d + 1) for dx in range(-max_d, max_d + 1) if dy or dx]
    for _ in range(6):
        if not rest.any():
            break
        best, bd = 0, None
        for dy, dx in offs:
            cov = (rest & C.shift(have, -dy, -dx, False)).sum()  # p + d 가 레이어 안
            score = cov - 0.15 * (abs(dy) + abs(dx)) - 0.3 * (prefer[0] * (dy < 0) + prefer[1] * (dx < 0))
            if score > best:
                best, bd = score, (dy, dx)
        if bd is None:
            break
        dy, dx = bd
        ok = rest & C.shift(have, -dy, -dx, False)
        src = C.shift(img, -dy, -dx)
        out[ok] = src[ok]
        rest &= ~ok
    if rest.any():
        out[rest] = extend(img, have, rest, 0)[rest]
    return out


def make_fills(f, lab, imgs, layers):
    u, g = geometry(f)
    fills = {}
    head_hand = np.isin(lab, [C.CAP, C.BODY, C.GLOVE])
    legs_zone = g["ua"] & g["waist"] & ~g["shoe_zone"] & ~head_hand
    top_zone = g["ua"] & ~g["waist"] & ~head_hand
    for sid, img in imgs.items():
        L = layers[sid]
        todo = legs_zone & ~L[C.LEGS]
        if L[C.LEGS].any() and todo.any():
            fills[(sid, C.LEGS)] = (todo, patch_copy(img, L[C.LEGS], todo, prefer=(1, 0)))
        todo = top_zone & ~L[C.TOP]
        if L[C.TOP].any() and todo.any():
            fills[(sid, C.TOP)] = (todo, patch_copy(img, L[C.TOP], todo, prefer=(0, 0)))
    return fills


def main():
    pkg, mapdir, out = sys.argv[1:4]
    set_pkg(pkg)
    maps = C.load_maps(mapdir)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    for f in C.FRAMES:
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lay = {s: C.make_layers(maps[f], imgs[s], s) for s in imgs}
        for (sid, slot), (m, fimg) in make_fills(f, maps[f], imgs, lay).items():
            d = os.path.join(out, sid, C.NAMES[slot] + "_fill")
            os.makedirs(d, exist_ok=True)
            o = np.zeros_like(fimg)
            o[m] = fimg[m]
            Image.fromarray(o).save(os.path.join(d, f + ".png"))
    print("M3 fills saved")


if __name__ == "__main__":
    main()
