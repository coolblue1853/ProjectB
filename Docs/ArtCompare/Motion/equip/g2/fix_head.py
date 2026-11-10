"""머리 보정(한 조각 방식 B): 두건 위로 삐져나온 머리 조각 제거, 초록으로 칠해진 뒷머리(포니테일) 복원, 떨어진 작은 조각 제거."""
import colorsys
import numpy as np

def hsv(px):
    return colorsys.rgb_to_hsv(*(np.asarray(px[:3], float) / 255.0))

def comps(mask):
    h, w = mask.shape; lab = np.zeros((h, w), int); n = 0; out = []
    for y in range(h):
        for x in range(w):
            if mask[y, x] and lab[y, x] == 0:
                n += 1; st = [(y, x)]; lab[y, x] = n; pix = []
                while st:
                    cy, cx = st.pop(); pix.append((cy, cx))
                    for dy in (-1, 0, 1):
                        for dx in (-1, 0, 1):
                            yy, xx = cy + dy, cx + dx
                            if 0 <= yy < h and 0 <= xx < w and mask[yy, xx] and lab[yy, xx] == 0:
                                lab[yy, xx] = n; st.append((yy, xx))
                out.append(pix)
    return out

def remove_specks(img, minsize=14):
    a = img[:, :, 3] > 128; cs = comps(a); out = img.copy()
    if not cs: return out
    big = max(len(c) for c in cs)
    for c in cs:
        if len(c) < minsize and len(c) < big:
            for y, x in c: out[y, x] = 0
    return out

def is_green(px):
    h, s, v = hsv(px); return 0.18 < h < 0.45 and s > 0.25

def is_skin(px):
    h, s, v = hsv(px); return 0.02 < h < 0.12 and 0.15 < s < 0.6 and v > 0.7

def fix(B, under, leaf, gy, hb):
    out = B.copy(); H, W = B.shape[:2]
    ua = under[:, :, 3] > 128; la = leaf[:, :, 3] > 128
    # 1) 두건 위로 삐져나온 머리(속옷 몸 머리 픽셀 중 같은 열의 두건 맨 위보다 위): 제거
    top = np.full(W, 999)
    for x in range(W):
        ys = [y for y in range(H) if la[y, x] and is_green(leaf[y, x]) and y < hb]
        if ys: top[x] = min(ys)
    for y in range(H):
        for x in range(W):
            if y < hb and ua[y, x] and out[y, x, 3] > 0:
                t = min(top[max(0, x - 1):x + 2])
                if t < 999 and y < t and not is_green(out[y, x]):
                    out[y, x] = 0
    # 2) 초록으로 칠해진 뒷머리 복원: 두건 영역 밖(얼굴 살색에서 4칸 이상) 아래쪽에서 속옷 몸이 머리카락인데 B 가 초록인 곳
    skin = np.zeros((H, W), bool)
    for y in range(H):
        for x in range(W):
            if ua[y, x] and is_skin(under[y, x]): skin[y, x] = True
    ys, xs = np.nonzero(skin)
    for y in range(int(gy) + 2, hb + 2):
        for x in range(W):
            if y < H and out[y, x, 3] > 0 and is_green(out[y, x]) and ua[y, x]:
                d = np.min(np.abs(ys - y) + np.abs(xs - x)) if len(ys) else 99
                h_, s_, v_ = hsv(under[y, x])
                if d >= 5 and s_ < 0.4 and v_ > 0.4:
                    out[y, x] = under[y, x]
    return remove_specks(out)
