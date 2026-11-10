"""공통: 프레임 입출력, 확장 지도, 런타임과 같은 합성, 평가 지표.

합성 규칙(00_README A절 / equip_ds.py export 와 같음):
  속옷 몸 위에 바지 → 신발 → 상의 → 장갑 → 모자 순으로 덧그림(지우기 없음).
  부위 레이어 = 그 세트 그림 중 '확장 지도'(지도 + 몸 밖 3칸)가 그 부위인 불투명 픽셀.
  세트 정책: base_coat 는 7(배낭·랜턴)·8(자락) → 상의, 다른 세트는 8 → 바지, 7 은 버림.
"""
import itertools
import os

import numpy as np
from PIL import Image

FRAMES = (["ds_idle_%d" % i for i in range(8)] + ["ds_stab_%d" % i for i in range(4)]
          + ["ds_down_%d" % i for i in range(4)] + ["ds_up_%d" % i for i in range(5)])
CAP, TOP, GLOVE, LEGS, SHOES, BODY, BACK, HEM = 1, 2, 3, 4, 5, 6, 7, 8
NAMES = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes", 6: "body", 7: "back", 8: "hem"}
SLOTS = [1, 2, 3, 4, 5]
DRAW_ORDER = [4, 5, 2, 3, 1]
COLORS = {1: (80, 200, 80), 2: (60, 120, 230), 3: (250, 230, 40), 4: (170, 80, 220), 5: (230, 100, 40),
          6: (150, 150, 150), 7: (120, 80, 40), 8: (230, 60, 170)}
N4 = ((1, 0), (-1, 0), (0, 1), (0, -1))


def load(path):
    return np.array(Image.open(path).convert("RGBA"))


def load_dir(d, frames=FRAMES):
    return {f: load(os.path.join(d, f + ".png")) for f in frames}


def load_maps(d, frames=FRAMES):
    return {f: np.array(Image.open(os.path.join(d, f + "_region.png"))) for f in frames
            if os.path.exists(os.path.join(d, f + "_region.png"))}


def save_map(lab, path):
    Image.fromarray(lab.astype(np.uint8), "L").save(path)


def alpha(img):
    return img[:, :, 3] > 128


def shift(a, dy, dx, fill=0):
    out = np.full_like(a, fill)
    h, w = a.shape[:2]
    ys, yd = (slice(0, h - dy), slice(dy, h)) if dy >= 0 else (slice(-dy, h), slice(0, h + dy))
    xs, xd = (slice(0, w - dx), slice(dx, w)) if dx >= 0 else (slice(-dx, w), slice(0, w + dx))
    out[yd, xd] = a[ys, xs]
    return out


def nearest_fill(lab, mask, seeds=None, max_steps=200):
    """mask 안의 0 픽셀을 4-이웃 BFS(층별)로 가장 가까운 seed 라벨로 채운다."""
    lab = lab.copy()
    known = (lab > 0) if seeds is None else seeds.copy()
    lab[~known & mask] = 0
    for _ in range(max_steps):
        todo = mask & ~known
        if not todo.any():
            break
        new = lab.copy()
        got = np.zeros_like(known)
        for dy, dx in N4:
            nl = shift(lab, dy, dx)
            nk = shift(known, dy, dx, False)
            take = todo & nk & ~got
            new[take] = nl[take]
            got |= take
        if not got.any():
            break
        lab, known = new, known | got
    return lab


def ext_map(lab, img, steps=3):
    """기준 지도 + 세트 그림이 지도 밖으로 나온 픽셀을 가까운 비(非)맨몸 부위로 3칸까지 확장(equip_ds.ext_map 과 같은 뜻)."""
    seeds = np.isin(lab, [1, 2, 3, 4, 5, 7, 8])
    cover = alpha(img) & (lab == 0)
    out = np.where(seeds, lab, 0).astype(np.uint8)
    known = seeds.copy()
    for _ in range(steps):
        todo = cover & ~known
        new = out.copy()
        got = np.zeros_like(known)
        for dy, dx in N4:
            nl = shift(out, dy, dx)
            nk = shift(known, dy, dx, False)
            take = todo & nk & ~got
            new[take] = nl[take]
            got |= take
        out, known = new, known | got
    out[lab == BODY] = BODY
    return out


def policy(emap, set_id, hem_to=None):
    """세트 정책으로 7·8 을 슬롯에 붙인다. hem_to 를 주면 자락(8)을 그 슬롯에 강제(질문 3 실험용)."""
    m = emap.copy()
    if set_id == "base_coat":
        m[m == BACK] = TOP
        m[m == HEM] = TOP if hem_to is None else hem_to
    else:
        m[m == BACK] = 0
        m[m == HEM] = LEGS if hem_to is None else hem_to
    return m


def cap_extend(lab, img, under, set_id):
    """세트별 모자(11-10): 머리 구역에서 그 세트 그림이 속옷 몸(=원본 머리)과 ±1px 로도 다른 픽셀 중
    지도가 맨몸(6)·바깥(0)이고 모자 덩어리와 이어진 것을 모자에 더한다(두건이 원본 모자 영역보다 클 때 잘림 방지).
    맨몸은 어느 세트도 그리지 않으므로 다른 부위와 소속이 겹치지 않는다(구멍 위험 없음).
    erase = 그 모자를 쓰면 숨어야 하는 원본 머리카락(속옷 몸엔 있는데 세트 그림 머리 구역은 투명인 곳, 모자 둘레 3칸 안)."""
    import m1_underdiff as M1
    import layered_extract as LE
    if set_id == "base_coat" or under is None:
        return np.zeros(lab.shape, bool), np.zeros(lab.shape, bool)
    g = M1.under_geometry(under)
    hz = LE.head_zone(g) | M1.dil(lab == CAP, 2)
    nd = M1.near_diff(img, under, 1)
    capm = alpha(img) & (lab == CAP)
    diff = alpha(img) & (nd > 30) & hz & np.isin(lab, [0, BODY])
    add = M1.cc_from(capm, capm | diff) & ~capm
    cap = capm | add
    erase = alpha(under) & ~alpha(img) & hz & M1.dil(cap, 3)
    # 11-10 점검: 런타임(CelComposer)은 모자 차례에 '지금까지 그린 그림 전체'를 지운다. 지도의 머리 영역(모자·맨몸) 밖을 지우면
    # 먼저 그려진 갑옷·장갑·신발까지 지워져 구멍이 된다 → 머리 영역으로 제한(이 영역은 어느 세트의 다른 부위도 그리지 않음).
    erase &= np.isin(lab, [CAP, BODY])
    return add, erase


def make_layers(lab, img, set_id, hem_to=None, under=None, armor=False):
    if armor:
        lab = lab.copy()
        lab[np.isin(lab, [LEGS, HEM])] = TOP
    pm = policy(ext_map(lab, img), set_id, hem_to)
    a = alpha(img)
    lay = {s: (pm == s) & a for s in SLOTS}
    if under is not None:
        add, erase = cap_extend(lab, img, under, set_id)
        lay[CAP] = lay[CAP] | add
        lay["erase_cap"] = erase
    return lay


def compose(under, wear, imgs, layers, fills=None):
    """wear: {슬롯: 세트 id}. layers: {세트 id: {슬롯: 마스크}}. fills: {(세트, 슬롯): (마스크, RGBA)} = 그 레이어 밑에 깔리는 가려진 부분 보완.
    반환: 합성 RGBA, 출처(0 투명, 1 속옷 몸, 10*세트번호+슬롯, 100+...=보완)."""
    out = under.copy()
    src = np.where(alpha(under), 1, 0).astype(np.int32)
    ids = sorted(imgs)
    for s in DRAW_ORDER:
        sid = wear[s]
        if sid is None:  # 미착용 = 그 부위는 속옷 몸 그대로
            continue
        # 지울 마스크(CelComposer 와 같음): 그 레이어 차례에 '지금까지 그린 그림 전체'에서 지운 뒤 덮어쓴다
        em = layers[sid].get("erase_%s" % NAMES[s])
        if em is not None:
            out[em] = 0
            src[em] = 0
        if fills and (sid, s) in fills:
            fm, fimg = fills[(sid, s)]
            out[fm] = fimg[fm]
            src[fm] = 100 + 10 * ids.index(sid) + s
        m = layers[sid][s]
        out[m] = imgs[sid][m]
        src[m] = 10 * ids.index(sid) + s
    return out, src


def patterns(set_ids):
    """5부위 각각 세트 a/b → 2^5 = 32 조합. set_ids 에 None 을 넣으면 '미착용'까지 3^5 = 243."""
    return [dict(zip(SLOTS, p)) for p in itertools.product(set_ids, repeat=5)]


def holes(src, coat, leaf):
    """투명 구멍: 두 옷 그림(코트·잎사귀)이 모두 불투명한 자리인데 합성 결과가 투명."""
    return int(((src == 0) & alpha(coat) & alpha(leaf)).sum())


def strays(img):
    """떠 있는 조각: 합성 결과에서 가장 큰 8-연결 덩어리에 속하지 않은 불투명 픽셀 수(라벨 불필요 침범 지표)."""
    a = alpha(img)
    seen = np.zeros_like(a)
    sizes = []
    for y, x in np.argwhere(a):
        if seen[y, x]:
            continue
        cur = np.zeros_like(a); cur[y, x] = True
        while True:
            nxt = cur.copy()
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    nxt |= shift(cur, dy, dx, False)
            nxt &= a
            if (nxt == cur).all():
                break
            cur = nxt
        seen |= cur
        sizes.append(int(cur.sum()))
    return int(sum(sizes) - max(sizes)) if sizes else 0


def coldiff(a, b):
    return np.abs(a[:, :, :3].astype(int) - b[:, :, :3].astype(int)).sum(2)


def exposure_mask(src, under, coat, leaf):
    """빈칸(속옷 노출): 두 옷 그림 모두 덮는 픽셀인데 합성 결과가 '드러난 속옷 몸'(원본과 색이 다른 속옷·맨살)."""
    return alpha(coat) & alpha(leaf) & (src == 1) & (coldiff(under, coat) > 30)


def exposure(src, under, coat, leaf):
    return int(exposure_mask(src, under, coat, leaf).sum())


def gt_compare(img, src, ideal, isrc):
    """정답 지도로 만든 이상 합성과 비교. 침범 = 레이어 픽셀인데 이상 합성과 출처가 다르고 색이 다름(+이상이 투명인데 칠함).
    잘림 = 이상 합성은 레이어인데 결과는 속옷 몸/투명이고 색이 다름."""
    diff = (coldiff(img, ideal) > 30) | (alpha(img) != alpha(ideal))
    lay, ilay = (src >= 10) & (src < 100), isrc >= 10  # 보완 층(100+) 픽셀은 따로 셈
    intr = lay & (src != isrc) & diff
    cut = ilay & ~lay & diff
    return int(intr.sum()), int(cut.sum())


def iou(pred, gt, classes=range(1, 9)):
    res = {}
    for k in classes:
        i = int(((pred == k) & (gt == k)).sum())
        u = int(((pred == k) | (gt == k)).sum())
        res[k] = (i, u)
    return res


def colorize(lab):
    rgb = np.full(lab.shape + (3,), 30, np.uint8)
    for k, c in COLORS.items():
        rgb[lab == k] = c
    return rgb
