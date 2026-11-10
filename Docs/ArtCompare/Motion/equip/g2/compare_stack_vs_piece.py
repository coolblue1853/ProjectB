"""비교: A 몸 위에 쌓기(현재 방식) vs B 몸+옷 한 조각(머리·손·발 제외). 새 생성 없음. 사용: python compare_stack_vs_piece.py"""
import os, sys
import numpy as np
from PIL import Image
ROOT = "C:/Users/user/ProjectB"
CODE = ROOT + "/Docs/Research/LabReply_gen_exp1/result/code"
sys.path.insert(0, CODE)
import common as C
import export_runtime as E
REG = ROOT + "/Docs/ArtCompare/Motion/equip/regions"
LEAF = ROOT + "/Docs/ArtCompare/Motion/equip/g2/leaf_new"
UND = ROOT + "/Assets/Art/Player/PixelRig/EquipDS/under"
OUT = ROOT + "/Docs/ArtCompare/Motion/equip/g2"
frames = ["ds_idle_0", "ds_idle_4", "ds_stab_1", "ds_down_2", "ds_up_3", "ds_up_4"]
maps = C.load_maps(REG)

def over(dst, src):
    m = src[:, :, 3] > 0
    dst[m] = src[m]

def head_rows(img):
    import colorsys
    a = img[:, :, 3] > 128
    ys, xs = np.nonzero(a)
    pts = []
    for y, x in zip(ys, xs):
        h, s, v = colorsys.rgb_to_hsv(*(img[y, x, :3] / 255.0))
        if 0.44 < h < 0.55 and s > 0.55 and v > 0.45: pts.append(y)
    gy = np.mean(pts) if pts else ys.min() + 8
    return int(round(gy)) + 9

res = {}
for f in frames:
    leaf = C.load(f"{LEAF}/{f}.png"); under = C.load(f"{UND}/{f}.png"); lab = maps[f].copy()
    lab[np.isin(lab, [C.LEGS, C.HEM])] = C.TOP
    em = C.ext_map(lab, leaf)
    em[em == C.BACK] = 0
    a = C.alpha(leaf)
    # 조각(레이어) 만들기
    lay = {}
    for s in C.SLOTS:
        L = np.zeros_like(leaf); m = a & (em == s); L[m] = leaf[m]; lay[s] = L
    add, erase = C.cap_extend(lab, leaf, under, "leaf_set")
    lay[C.CAP][add] = leaf[add]
    # A: 속옷 몸 위에 쌓기
    A = under.copy()
    for s in [4, 5, 2, 3]: over(A, lay[s])
    A[erase] = 0
    over(A, lay[C.CAP])
    # B: 머리(속옷 몸의 머리) + 한 조각(몸+옷 = 잎사귀 그림에서 머리 아래 전부) + 손·발 = 같은 그림
    hb = head_rows(under)
    B = np.zeros_like(under)
    below = np.zeros(a.shape, bool); below[hb + 1:, :] = True
    body_piece = a & below
    B[body_piece] = leaf[body_piece]
    headpx = (under[:, :, 3] > 128) & ~below
    B[headpx] = under[headpx]
    B[erase] = 0
    over(B, lay[C.CAP])
    res[f] = (under, A, B, leaf)
    sil = lambda x: x[:, :, 3] > 128
    print(f, "A 알파", int(sil(A).sum()), "B 알파", int(sil(B).sum()), "A-B 다른 픽셀", int((np.abs(A.astype(int) - B.astype(int)).sum(2) > 30).sum()))

K = 6
sheet = Image.new("RGBA", (64 * K * 4, 64 * K * len(frames)), (120, 130, 140, 255))
for r, f in enumerate(frames):
    under, A, B, leaf = res[f]
    for c, im in enumerate((leaf, A, B, np.abs(A.astype(int) - B.astype(int)).sum(2) > 30)):
        if c == 3:
            d = np.zeros((64, 64, 4), np.uint8); d[im] = (255, 60, 60, 255); im = d
        sheet.alpha_composite(Image.fromarray(im.astype(np.uint8)).resize((64 * K, 64 * K), Image.NEAREST), (c * 64 * K, r * 64 * K))
sheet.save(OUT + "/compare_stack_vs_piece.png")

# ---- 고글 빼기: 잎사귀 모자(두건) 그림에서 고글(청록 렌즈 + 갈색 띠)을 두건 색으로 덮음 ----
import colorsys
def goggle_mask(img, hb):
    h_, w_ = img.shape[:2]
    lens = np.zeros((h_, w_), bool)
    for y in range(0, hb):
        for x in range(w_):
            if img[y, x, 3] > 128:
                h, s_, v = colorsys.rgb_to_hsv(*(img[y, x, :3] / 255.0))
                if 0.44 < h < 0.56 and s_ > 0.4 and v > 0.4: lens[y, x] = True
    ys, xs = np.nonzero(lens)
    m = np.zeros_like(lens)
    if len(ys) == 0: return m
    for y, x in zip(ys, xs):
        for dy in (-1, 0, 1):
            for dx in range(-3, 4):
                yy, xx = y + dy, x + dx
                if 0 <= yy < h_ and 0 <= xx < w_ and img[yy, xx, 3] > 128:
                    h, s_, v = colorsys.rgb_to_hsv(*(img[yy, xx, :3] / 255.0))
                    if (0.44 < h < 0.56 and s_ > 0.4) or (0.04 < h < 0.13 and s_ > 0.45 and v > 0.3): m[yy, xx] = True
    return m
def fill_masked(img, m, keep):
    out = img.copy(); todo = m.copy()
    for _ in range(6):
        nxt = todo.copy()
        for y, x in zip(*np.nonzero(todo)):
            cols = []
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    yy, xx = y + dy, x + dx
                    if 0 <= yy < 64 and 0 <= xx < 64 and not todo[yy, xx] and keep[yy, xx] and out[yy, xx, 3] > 128: cols.append(tuple(out[yy, xx]))
            if cols:
                out[y, x] = max(set(cols), key=cols.count); nxt[y, x] = False
        todo = nxt
    return out
gf = ["ds_idle_0", "ds_stab_1", "ds_down_2", "ds_idle_4"]
sh2 = Image.new("RGBA", (64 * K * 3, 64 * K * len(gf)), (120, 130, 140, 255))
for r, f in enumerate(gf):
    under, A, B, leaf = res[f]
    hb = head_rows(under)
    gm = goggle_mask(leaf, hb)
    leaf2 = fill_masked(leaf, gm, C.alpha(leaf) & ~gm)
    # 기본 코트 머리(고글 있음) vs 잎사귀 모자 + 고글 있음 vs 고글 없음
    coat = C.load(f"{ROOT}/Assets/Art/Player/PixelRig/EquipDS/base_coat/{f}.png")
    for c, im in enumerate((coat, leaf, leaf2)):
        sh2.alpha_composite(Image.fromarray(im).resize((64 * K, 64 * K), Image.NEAREST), (c * 64 * K, r * 64 * K))
    print(f, "고글 픽셀", int(gm.sum()))
sh2.save(OUT + "/compare_goggles.png")
