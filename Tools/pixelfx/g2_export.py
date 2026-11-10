"""한 조각 방식 내보내기(11-10, G2): 기본 코트·잎사귀 세트를 4칸(머리·옷·장갑·신발)으로 자른다.
 - 옷(top, slot body) = 머리 아래 몸+옷 전부(지도 상의·바지·자락, 기본 코트는 배낭·랜턴 포함) + 바탕(속옷 몸)의 같은 자리를 지우는 erase 마스크 → 속옷이 비치지 않는다.
 - 모자(cap): 잎사귀는 고글·두건 위로 삐져나온 머리를 지우고(erase) 이마를 채움. 기본 코트는 고글이 바탕에 있으므로 그대로.
 - 클립 이름: cel.json 의 모든 클립 중 프레임 그림이 이 44장에 속하는 것(35개)에 같은 그림을 (클립, 인덱스)로 복제.
사용: python Tools/pixelfx/g2_export.py"""
import colorsys, json, os, sys
import numpy as np
from PIL import Image
ROOT = "C:/Users/user/ProjectB"
sys.path.insert(0, ROOT + "/Docs/Research/LabReply_gen_exp1/result/code")
import common as C
import m1_underdiff as M1
G2 = ROOT + "/Docs/ArtCompare/Motion/equip/g2"
RIG = ROOT + "/Assets/Art/Player/PixelRig"
REG = ROOT + "/Docs/ArtCompare/Motion/equip/regions"
NEW = [f"ds_walk_{i}" for i in range(8)] + [f"ds_run_{i}" for i in range(8)] + ["ds_crouch_0"] + [f"ds_arc_{i}" for i in range(3)] + [f"ds_land_{i}" for i in range(3)]
FRAMES = list(C.FRAMES) + NEW
PART = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}
SLOT = {1: "head", 2: "body", 3: "gloves", 4: "legs", 5: "shoes"}

def hsv(p): return colorsys.rgb_to_hsv(*(np.asarray(p[:3], float) / 255.0))
def ld(p): return C.load(p)
def under_of(f): return ld(f"{G2}/under_head/{f}.png") if f in NEW else ld(f"{RIG}/EquipDS/under/{f}.png")
def map_of(f): return np.array(Image.open((G2 + "/regions23/" if f in NEW else REG + "/") + f + "_region.png"))
def img_of(set_id, f): return ld(f"{G2}/leaf3/{f}.png") if set_id == "leaf_set" else ld(f"{RIG}/EquipDS/base_coat/{f}.png")

def head_geom(img):
    a = img[:, :, 3] > 128; ys, xs = np.nonzero(a); pts = []
    for y, x in zip(ys, xs):
        h, s, v = hsv(img[y, x])
        if 0.44 < h < 0.55 and s > 0.55 and v > 0.45: pts.append(y)
    gy = float(np.mean(pts)) if pts else ys.min() + 8.0
    return gy, int(round(gy)) + 9

def goggle_mask(under, gy):
    H, W = under.shape[:2]; al = under[:, :, 3] > 128; lens = np.zeros((H, W), bool)
    for y in range(0, int(gy) + 4):
        for x in range(W):
            if al[y, x]:
                h, s, v = hsv(under[y, x])
                if 0.44 < h < 0.56 and s > 0.4 and v > 0.4: lens[y, x] = True
    m = np.zeros((H, W), bool)
    for y, x in zip(*np.nonzero(lens)):
        for dy in (-1, 0, 1):
            for dx in range(-3, 4):
                yy, xx = y + dy, x + dx
                if 0 <= yy < H and 0 <= xx < W and al[yy, xx]:
                    h, s, v = hsv(under[yy, xx])
                    if (0.44 < h < 0.56 and s > 0.4) or (0.04 < h < 0.13 and s > 0.45 and v > 0.3): m[yy, xx] = True
    return m

def fill_from(under, mask):
    out = {}; H, W = mask.shape; al = under[:, :, 3] > 128
    todo = mask.copy(); col = under.copy()
    for _ in range(6):
        nxt = todo.copy()
        for y, x in zip(*np.nonzero(todo)):
            c = [tuple(col[yy, xx]) for yy in range(max(0, y - 1), min(H, y + 2)) for xx in range(max(0, x - 1), min(W, x + 2)) if al[yy, xx] and not todo[yy, xx]]
            if c: col[y, x] = max(set(c), key=c.count); nxt[y, x] = False
        todo = nxt
    return col

def build(set_id, f):
    img = img_of(set_id, f); under = under_of(f); lab = map_of(f).copy()
    a = C.alpha(img); ua = C.alpha(under)
    gy, hb = head_geom(under)
    lab[(lab == C.GLOVE) & M1.dil(lab == C.BACK, 2)] = C.TOP   # 랜턴 불빛이 장갑으로 잘못 분류된 것 → 옷
    lab_eff = lab.copy(); lab_eff[np.isin(lab_eff, [C.LEGS, C.HEM])] = C.TOP
    em = C.ext_map(lab_eff, img)
    em[em == C.HEM] = C.TOP
    em[em == C.BACK] = C.TOP   # 잎사귀 그림엔 배낭이 없다 → 배낭 자리의 픽셀은 옷(뒷모습 등판)
    Gz = M1.dil(lab == C.GLOVE, 1); Sz = M1.dil(lab == C.SHOES, 1)
    P = M1.dil(np.isin(lab, [C.TOP, C.LEGS, C.HEM, C.BACK]), 1)
    if set_id == "leaf_set":   # 잎사귀 신발: 새 포즈가 옛 코트 신발 지도와 어긋나므로 신발 지도를 넓혀 장화 전체를 신발 층으로(11-10)
        Sz6 = M1.dil(lab == C.SHOES, 5)
        Sz6 &= (np.arange(lab.shape[0])[:, None] >= (np.nonzero(lab == C.SHOES)[0].min() - 6 if (lab == C.SHOES).any() else 99))
        em[a & Sz6 & (em != 3) & (em != 1)] = 5
        Sz = Sz | Sz6
    lay = {s: np.zeros_like(img) for s in PART}
    for s in (1, 2, 3, 5):
        m = a & (em == s); lay[s][m] = img[m]
    H_, W_ = a.shape
    below = (np.arange(H_)[:, None] > hb) & np.ones((1, W_), bool)
    prot = M1.dil(np.isin(lab, [C.CAP, C.BODY]), 1)   # 얼굴·머리카락(꼬리 포함)은 지우지 않는다
    def specks(L, others, maxs=12):
        al = L[:, :, 3] > 0
        seen = np.zeros(al.shape, bool); out = L.copy(); near = M1.dil(others, 1)
        for y in range(al.shape[0]):
            for x in range(al.shape[1]):
                if al[y, x] and not seen[y, x]:
                    st = [(y, x)]; seen[y, x] = True; comp = []
                    while st:
                        cy, cx = st.pop(); comp.append((cy, cx))
                        for dy in (-1, 0, 1):
                            for dx in (-1, 0, 1):
                                yy, xx = cy + dy, cx + dx
                                if 0 <= yy < al.shape[0] and 0 <= xx < al.shape[1] and al[yy, xx] and not seen[yy, xx]:
                                    seen[yy, xx] = True; st.append((yy, xx))
                    if len(comp) <= maxs and not any(near[cy, cx] for cy, cx in comp):
                        for cy, cx in comp: out[cy, cx] = 0
        return out
    if set_id == "leaf_set":   # 한 조각: 머리 아래 속옷은 전부 지우고(얼굴·머리카락·장갑·신발 구역 제외), 어느 층에도 안 들어간 잎사귀 픽셀은 옷 층으로(11-10 검사 기반)
        P = np.ones_like(P, bool)
        cov = np.zeros(a.shape, bool)
        for s_ in (1, 2, 3, 5): cov |= lay[s_][:, :, 3] > 0
        un = a & ~cov & below
        lay[2][un] = img[un]
        un2 = a & ~cov & ~below & ~prot
        lay[2][un2] = img[un2]
    er_top = ua & P & below & ~prot & ~Gz & ~Sz
    if set_id == "leaf_set": er_top |= ua & ~a & below & ~prot   # 잎사귀 그림에 없는 자리의 속옷 잔여는 전부 지움
    lay[("erase", 2)] = er_top
    if set_id == "leaf_set":
        lay[("erase", 5)] = ua & Sz & ~M1.dil(lab == C.GLOVE, 1)
    others = ua & ~er_top
    for s_ in (1, 3, 5):
        others = others | (lay[s_][:, :, 3] > 0)
    lay[2] = specks(lay[2], others)
    if set_id == "leaf_set":
        add, erase = C.cap_extend(lab, img, under, set_id)
        lay[1][add] = img[add]
        gm = goggle_mask(under, gy)
        H, W = a.shape; hood = a & (em == 1)
        top = np.full(W, 999)
        for x in range(W):
            ys = np.nonzero(hood[:, x])[0]
            if len(ys): top[x] = ys.min()
        above = np.zeros((H, W), bool)
        for y in range(H):
            for x in range(W):
                if ua[y, x] and y < gy and min(top[max(0, x - 1):x + 2]) < 999 and y < min(top[max(0, x - 1):x + 2]): above[y, x] = True
        e = (erase | gm | above) & np.isin(lab, [C.CAP, C.BODY, 0]) & (np.arange(H)[:, None] <= hb)
        lay[("erase", 1)] = e
        fillc = fill_from(under, gm | above)
        fm = (gm & ~C.alpha(lay[1]))
        lay[1][fm] = fillc[fm]
        if f in NEW:   # 새로 생성한 프레임은 머리 위치·얼굴이 바탕(코트 원본 머리)과 어긋난다 → 얼굴·머리카락까지 모자 층으로 통째로, 바탕 머리는 지움
            hd = (np.arange(H)[:, None] <= hb) & np.ones((1, W), bool)
            take = a & hd & ~Gz & (em != 3)
            lay[1][take] = img[take]
            lay[("erase", 1)] = ua & hd
        cov = np.zeros(a.shape, bool)
        for s_ in (1, 2, 3, 5): cov |= lay[s_][:, :, 3] > 0
        rest = a & ~cov   # 어느 층에도 안 들어간 잎사귀 픽셀 → 옷 층(잘림 방지)
        lay[2][rest] = img[rest]
        if f in NEW:
            lay[1][(lay[2][:, :, 3] > 0) & (lay[1][:, :, 3] > 0)] = 0   # 같은 픽셀은 한 층에만
            lay[("erase", 1)] = (lay[("erase", 1)] | (ua & ~a & (np.arange(H)[:, None] <= hb + 4))) & ~(lay[2][:, :, 3] > 0)   # 모자 지우개가 옷 층 그림을 지우지 않게
    return lay

def main():
    cel = json.load(open(RIG + "/CelDS/cel.json"))
    use = {}
    for name, c in cel.items():
        for i, fr in enumerate(c["frames"]):
            if fr["png"] in FRAMES: use.setdefault(fr["png"], []).append((name, i))
    root = RIG + "/Equip"
    for set_id in ("base_coat", "leaf_set"):
        lays = {f: build(set_id, f) for f in FRAMES}
        for s, part in PART.items():
            lid = f"{set_id}_{part}"; d = os.path.join(root, lid)
            os.makedirs(d, exist_ok=True)
            for fn in os.listdir(d):
                if fn.endswith(".png"): os.remove(os.path.join(d, fn))
            frames = []
            for f in FRAMES:
                L = lays[f][s]; er = lays[f].get(("erase", s))
                for clip, i in use.get(f, []):
                    Image.fromarray(L).save(f"{d}/{clip}_{i}.png")
                    e8 = (er.astype(np.uint8) * 255) if er is not None else np.zeros((64, 64), np.uint8)
                    Image.fromarray(e8, "L").save(f"{d}/{clip}_{i}_erase.png")
                    frames.append({"clip": clip, "k": i})
            json.dump({"id": lid, "slot": SLOT[s], "ds": True, "frames": frames}, open(d + "/equip.json", "w"), indent=1)
            print(lid, len(frames))
    # 바탕(속옷 몸) 44장 → CelDS
    for f in FRAMES:
        Image.fromarray(under_of(f)).save(f"{RIG}/CelDS/{f}.png")
    print("CelDS 바탕 교체", len(FRAMES))

if __name__ == "__main__":
    main()
