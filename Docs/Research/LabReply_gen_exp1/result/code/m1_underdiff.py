"""M1 (연구 질문 1): 속옷 몸 차분 + 해부학 prior + 프레임별 적응 팔레트. 정답 라벨 0장.

핵심: 옷 색 규칙 대신 '속옷 몸'(옷과 상관없이 같은 몸)에서 부위 기하를 얻는다.
  1) 옷 픽셀 G = 원본 ≠ 속옷 몸(색 차 > 40 또는 속옷 투명).  몸 그대로 픽셀 N = 나머지.
  2) 속옷 몸에서: 살색(팔·다리·손), 반바지 윗선(허리선, 기울기 맞춤), 다리 살 아래 끝(부츠 윗선).
  3) N: 머리(기준선과 같은 고글 규칙으로 모자/맨몸) · 허리 위 살 = 장갑 · 부츠 윗선 아래 = 신발.
  4) G: 허리 위 & 속옷 실루엣 밖 = 배낭 씨앗 → 배낭/상의/바지 팔레트(이 프레임에서 자동 수집)로 최근접 색 분류,
     허리 아래는 속옷 다리 안쪽 & 바지 팔레트면 바지, 아니면 자락(8). 배낭은 씨앗과 이어진 것만.
  5) 진한 외곽선·남은 픽셀은 가장 가까운 확정 부위(BFS).
  4') 소품: 같은 자세로 다른 옷을 전이한 그림(잎사귀)과 비교해 '옷을 바꿔도 안 변한 & 속옷 몸엔 없는' 픽셀 = 배낭·랜턴.
추가 입력: frames_underwear_base(같은 자세 속옷 몸), frames_leaf_set_transfer(같은 자세 다른 옷 1벌, 소품 판정용).
사용: python m1_underdiff.py <패키지> <출력 폴더> [--no-leaf] [--hybrid]   (--hybrid = M1h: 기준선 지도 + 위 신호로 확실한 곳만 수정)
"""
import colorsys
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import baseline_region_map as R


def hsv_img(img):
    f = img[:, :, :3].astype(float) / 255
    out = np.zeros(f.shape)
    for y in range(f.shape[0]):
        for x in range(f.shape[1]):
            out[y, x] = colorsys.rgb_to_hsv(*f[y, x])
    return out


def dil(m, r=1):
    o = m.copy()
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            o |= C.shift(m, dy, dx, False)
    return o


def cc_from(seed, allowed):
    """seed 와 8-연결로 이어진 allowed 픽셀"""
    cur = seed & allowed
    while True:
        nxt = dil(cur, 1) & allowed
        if (nxt == cur).all():
            return cur
        cur = nxt


def pal_dist(img, mask, samples, src=None):
    """img 각 픽셀과 (src 의 samples 위치 색) 팔레트 사이 최소 RGB 거리"""
    if not samples.any():
        return np.full(mask.shape, 1e9)
    cols = (img if src is None else src)[samples][:, :3].astype(float)
    cols = np.unique(cols, axis=0)
    px = img[:, :, :3].astype(float).reshape(-1, 1, 3)
    d = np.sqrt(((px - cols[None]) ** 2).sum(2)).min(1).reshape(mask.shape)
    return d


def near_diff(c, l, r=1):
    """c 각 픽셀과 l 의 ±r 이웃 중 가장 비슷한 픽셀의 색 차(전이가 1px 밀려 그린 것 허용)"""
    best = np.full(c.shape[:2], 10 ** 6)
    la = C.alpha(l)
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            ls = C.shift(l, dy, dx)
            d = C.coldiff(c, ls)
            d[~C.shift(la, dy, dx, False)] = 10 ** 6
            best = np.minimum(best, d)
    return best


def under_geometry(u):
    ua = C.alpha(u)
    hv = hsv_img(u)
    h, s, v = hv[:, :, 0], hv[:, :, 1], hv[:, :, 2]
    ys, xs = np.nonzero(ua)
    top, foot = ys.min(), ys.max()
    skin = ua & (h >= 0.02) & (h <= 0.11) & (s > 0.28) & (s < 0.7) & (v > 0.6)
    dark = ua & (v < 0.33) & (s < 0.6)
    yy, xx = np.mgrid[0:64, 0:64]
    mid = (yy > top + (foot - top) * 0.40) & (yy < top + (foot - top) * 0.80)
    shorts = dark & mid
    # 허리선: 가운데 띠에서 '어두운 픽셀이 그 줄 속옷 폭의 45% 이상'인 첫 줄(반바지 윗선). 기울기는 반바지 윗선 열별 값으로 작게만.
    b = None
    for y in range(int(top + (foot - top) * 0.40), int(top + (foot - top) * 0.80)):
        n = ua[y].sum()
        if n and shorts[y].sum() >= max(3, 0.45 * n):
            b = float(y)
            break
    if b is None:
        b = top + (foot - top) * 0.55
    a = 0.0
    sh = shorts & (yy >= b) & (yy <= b + 4)
    cols = [x for x in range(64) if sh[:, x].any()]
    if len(cols) > 5:
        tops = np.array([[x, np.nonzero(sh[:, x])[0].min()] for x in cols], float)
        a = float(np.clip(np.polyfit(tops[:, 0], tops[:, 1], 1)[0], -0.35, 0.35))
        b = float(np.median(tops[:, 1] - a * tops[:, 0]))
    waist = yy >= a * xx + b
    legskin = skin & waist
    # 부츠 윗선: 열마다 다리 살 맨 아래 + 1, 없는 열은 가까운 열 값
    line = np.full(64, -1)
    for x in range(64):
        c = np.nonzero(legskin[:, x])[0]
        c = c[c > b]
        if len(c):
            line[x] = c.max() + 1
    have = np.nonzero(line >= 0)[0]
    floor_y = foot - 9
    for x in range(64):
        if line[x] < 0:
            line[x] = line[have[np.argmin(np.abs(have - x))]] if len(have) else floor_y
        line[x] = max(line[x], floor_y)
    shoe_zone = yy >= line[None, :]
    leg_region = dil(ua & waist & ~shoe_zone & (legskin | shorts), 1)
    return dict(ua=ua, skin=skin, shorts=shorts, waist=waist, shoe_zone=shoe_zone, leg_region=leg_region)


def segment(c, u, l=None, hybrid=False):
    """c 원본(분할 대상), u 속옷 몸, l 같은 자세 다른 옷 전이 그림(있으면 소품 판정에 씀)."""
    ca = C.alpha(c)
    g = under_geometry(u)
    G = ca & (~g["ua"] | (C.coldiff(c, u) > 40))
    N = ca & ~G
    base = R.classify(c, {})  # 머리 모자/맨몸 관례(고글 위 = 모자)만 빌림
    headzone = np.isin(base, [C.CAP, C.BODY]) & ~g["waist"]
    lab = np.zeros((64, 64), np.uint8)
    head = N & headzone
    if l is not None:  # 머리 구역에서 옷을 바꿔도 안 변한 픽셀(머리카락·고글)도 머리
        head |= ca & headzone & C.alpha(l) & (C.coldiff(c, l) <= 40)
    lab[head] = base[head]
    lab[N & g["shoe_zone"]] = C.SHOES
    # 살색 범위는 속옷 몸의 살 픽셀 HSV 분포(5~95%)에서 얻는다(몸 고유, 옷과 무관)
    hu = hsv_img(u)[g["skin"]]
    lo, hi = np.percentile(hu, 5, axis=0) - [0.03, 0.10, 0.08], np.percentile(hu, 95, axis=0) + [0.03, 0.05, 0.05]  # 살 밝은 면(채도 낮음) 허용
    hc = hsv_img(c)
    skin_c = ca & np.all((hc >= lo) & (hc <= hi), axis=2)
    hand = skin_c & ~dil(headzone, 1) & ~g["shoe_zone"]
    # 가죽(배낭)과 살색이 겹치므로: 2~20px 덩어리이면서 소품 후보(세트 불변) 픽셀과 안 닿는 것만 손
    if l is not None:
        accn = dil(G & (near_diff(c, l) <= 30) & ~headzone & ~skin_c, 1)
    else:
        accn = np.zeros_like(hand)
    keep = np.zeros_like(hand)
    rem = hand.copy()
    while rem.any():
        y, x = np.argwhere(rem)[0]
        sd = np.zeros_like(rem); sd[y, x] = True
        comp = cc_from(sd, hand)
        if 2 <= comp.sum() <= 20 and (comp & accn).sum() <= comp.sum() // 3:
            keep |= comp
        rem &= ~comp
    hand = keep
    # 손 덩어리(살색 + 바로 옆 진한 외곽선 1칸)
    # 11-10: 머리 구역 안의 손(뒷모습에서 머리 위로 든 주먹 등). 머리 구역 살색 2~20px 덩어리 중
    # 둘레(불투명)의 30% 이상이 옷(G, 살색 아님)이고 머리카락(N, 살색 아님)은 30% 미만이면 얼굴이 아니라 손.
    # 소품(배낭 가죽은 살색과 비슷) 과 얼굴(머리 구역 최대 살색 덩어리 ±2칸) 근처는 제외.
    hz_skin = skin_c & dil(headzone, 1) & ~g["shoe_zone"] & ~hand
    hair_n = N & headzone & ~skin_c
    acc_raw = (G & (near_diff(c, l) <= 30) & ~skin_c) if l is not None else np.zeros_like(G)
    comps = []
    rem = hz_skin.copy()
    while rem.any():
        y, x = np.argwhere(rem)[0]
        sd = np.zeros_like(rem); sd[y, x] = True
        comp = cc_from(sd, hz_skin)
        rem &= ~comp
        comps.append(comp)
    face = max(comps, key=lambda m: m.sum()) if comps else np.zeros_like(hand)
    near_face = dil(face, 2)
    for comp in comps:
        if comp is face:
            continue
        ring = dil(comp, 1) & ~comp & ca
        if 2 <= comp.sum() <= 20 and ring.sum() >= 3 and not (comp & near_face).any():
            cloth = ring & G & ~skin_c & ~acc_raw
            if cloth.sum() >= 0.3 * ring.sum() and (ring & hair_n).sum() < 0.3 * ring.sum() and not (ring & acc_raw).any():
                hand |= comp
    hand = hand | (dil(hand, 1) & ca & (hc[:, :, 2] < 0.3) & ~headzone)
    lab[hand] = C.GLOVE
    G = G & ~head
    above = G & ~g["waist"] & ~hand
    below = G & g["waist"] & ~g["shoe_zone"]
    lab[G & g["shoe_zone"]] = C.SHOES
    if l is not None:
        # 소품 = 옷을 갈아입혀도 안 변한 픽셀 중 속옷 몸에는 없는 것(배낭·랜턴은 전이에서 보존됨)
        acc = G & (near_diff(c, l) <= 30) & ~g["shoe_zone"] & ~headzone
        # 작은 조각 제거 + 소품에 둘러싸인 옷 픽셀 흡수
        keep = np.zeros_like(acc)
        rem = acc.copy()
        while rem.any():
            y, x = np.argwhere(rem)[0]
            seed = np.zeros_like(rem); seed[y, x] = True
            comp = cc_from(seed, acc)
            if comp.sum() >= 6 and (comp & ~g["waist"]).sum() >= 0.5 * comp.sum():
                keep |= comp
            rem &= ~comp
        nb = sum(C.shift(keep, dy, dx, False).astype(int) for dy in (-1, 0, 1) for dx in (-1, 0, 1) if dy or dx)
        back = keep | (G & (nb >= 5) & ~g["shoe_zone"] & ~g["leg_region"])
        # 배낭 팔레트(세트 불변으로 얻은 깨끗한 표본)로 이어진 옷 픽셀 흡수(전이가 배낭을 살짝 다시 그린 곳)
        if back.any():
            d_b = pal_dist(c, ca, back)
            d_t = pal_dist(c, ca, G & ~back & ~g["waist"] & ~dil(back, 2))
            cand = G & ~head & ~hand & ~g["shoe_zone"] & ~g["leg_region"] & (d_b + 8 < d_t)
            back = cc_from(back, back | cand)
    else:
        seed_back = above & ~dil(g["ua"], 1)
        back = seed_back
    top_s = above & ~back
    leg_s = below & g["leg_region"] & C.shift(g["shoe_zone"], -3, 0, False) & ~back
    d_top = pal_dist(c, ca, top_s)
    d_leg = pal_dist(c, ca, leg_s)
    lab[above & ~back] = C.TOP
    lab[below & ~back] = C.HEM
    lab[below & g["leg_region"] & (d_leg + 4 < d_top) & ~back] = C.LEGS
    lab[back] = C.BACK
    hv = hsv_img(c)
    outline = ca & (hv[:, :, 2] < 0.11) & ~head & ~back & ~hand
    lab[outline] = 0
    lab = C.nearest_fill(lab, ca)
    lab[~ca] = 0
    if hybrid:
        return hybridize(c, lab, base, g, hand, back, head, G)
    return lab


def hybridize(c, m1, base, g, hand, back, head, G):
    """M1h: 기준선 지도를 출발점으로 두고, 옷 종류와 무관한 신호로 확실한 곳만 고친다.
    신발 = 속옷 부츠선 아래, 장갑 = 속옷 살색 팔레트, 배낭 = 세트 불변 소품, 머리 구역의 옷 픽셀(들어 올린 팔 등) = M1."""
    ca = C.alpha(c)
    lab = base.copy()
    lab[(lab == C.GLOVE) & ~hand] = 0
    back = (back | (base == C.BACK)) & ~g["shoe_zone"] & ~hand  # 기준선 배낭은 유지하고 세트 불변 소품을 더함(지우지 않음)
    lab[(lab == C.SHOES) & ~g["shoe_zone"]] = 0
    lab[np.isin(lab, [C.CAP, C.BODY]) & G & ~head] = 0
    lab = C.nearest_fill(lab, ca)
    lab[np.isin(base, [C.CAP, C.BODY]) & G & ~head & ca] = m1[np.isin(base, [C.CAP, C.BODY]) & G & ~head & ca]
    lab[ca & g["shoe_zone"]] = C.SHOES
    lab[back] = C.BACK
    lab[hand] = C.GLOVE
    lab[~ca] = 0
    return fill_diagonal(lab, ca)


def fill_diagonal(lab, ca):
    """대각선으로만 이어진 외딴 불투명 픽셀(4-이웃 BFS 가 못 닿음 — 기준선에도 10px 있음)을 8-이웃 최빈 라벨로."""
    for _ in range(3):
        miss = ca & (lab == 0)
        if not miss.any():
            break
        for y, x in np.argwhere(miss):
            nb = [int(lab[yy, xx]) for yy in range(max(0, y - 1), min(64, y + 2)) for xx in range(max(0, x - 1), min(64, x + 2)) if lab[yy, xx]]
            if nb:
                lab[y, x] = max(set(nb), key=nb.count)
    return lab


def main():
    pkg, out = sys.argv[1:3]
    os.makedirs(out, exist_ok=True)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    other = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer")) if "--no-leaf" not in sys.argv else {}
    for f in C.FRAMES:
        C.save_map(segment(coat[f], under[f], other.get(f), "--hybrid" in sys.argv), os.path.join(out, f + "_region.png"))
    print("M1 saved", len(C.FRAMES))


if __name__ == "__main__":
    main()
