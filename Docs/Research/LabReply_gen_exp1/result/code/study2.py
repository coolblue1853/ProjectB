"""2차 연구(생성 0): 경계 이음새, 프레임 간 깜빡임, 세트별 지도 보정·조각 제거, 키 프레임 속옷 몸 전파.

사용: python study2.py <패키지> <지도 폴더> <labels> <출력 폴더>
출력: <출력>/study2.json, <출력>/sheets/*.png, <출력>/regions_stable/ (깜빡임 안정화 지도)
"""
import json
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import m1_underdiff as M1
import m2_propagate as M2
import m3_amodal as M3

SETS = ["base_coat", "leaf_set"]
CLIPS = {"ds_idle": 8, "ds_stab": 4, "ds_down": 4, "ds_up": 5}


def lum(img):
    f = img[:, :, :3].astype(float)
    return 0.299 * f[:, :, 0] + 0.587 * f[:, :, 1] + 0.114 * f[:, :, 2]


# ---------- 1. 경계 이음새 ----------
def seam_pairs(img, src, pures=None):
    """다른 세트(또는 속옷 몸)에서 온 두 픽셀이 맞닿는 4-이웃 쌍 중 밝기 차 > 60 이고 둘 다 외곽선(밝기<45)이 아닌 것 = 이음새 끊김.
    pures(원본 세트 그림 목록)를 주면, 그 자리가 원본 어느 쪽에서든 이미 강한 경계(>60)였던 쌍은 빼고 '합성에서만 생긴' 끊김만 센다."""
    L = lum(img)
    natural = np.zeros((2,) + img.shape[:2], bool)
    if pures:
        for i, (dy, dx) in enumerate(((0, 1), (1, 0))):
            for p in pures:
                Lp = lum(p)
                natural[i] |= C.alpha(p) & C.shift(C.alpha(p), -dy, -dx, False) & (np.abs(Lp - C.shift(Lp, -dy, -dx)) > 60)
    a = C.alpha(img)
    setid = np.where(src >= 100, (src - 100) // 10, np.where(src >= 10, src // 10, -1 - src))  # 세트 번호 / 속옷 -2 / 투명 -1
    n = 0
    mask = np.zeros(a.shape, bool)
    for i, (dy, dx) in enumerate(((0, 1), (1, 0))):
        L2, a2, s2 = C.shift(L, -dy, -dx), C.shift(a, -dy, -dx, False), C.shift(setid, -dy, -dx, -9)
        bad = a & a2 & (setid != s2) & (np.abs(L - L2) > 60) & (L > 45) & (L2 > 45) & ~natural[i]
        n += int(bad.sum())
        mask |= bad | C.shift(bad, dy, dx, False)
    return n, mask


def outline_seams(img, src, pures=None):
    """원칙 5: 다른 세트끼리 맞닿은 경계에서 '위에 그려진 쪽' 1px 을 그 픽셀 주변 가장 어두운 색으로(외곽선 통일)."""
    out = img.copy()
    L = lum(img)
    a = C.alpha(img)
    setid = np.where(src >= 100, (src - 100) // 10, np.where(src >= 10, src // 10, -1 - src))
    order = np.where(src >= 10, src % 10, 0)  # 슬롯 번호
    rank = {4: 1, 5: 2, 2: 3, 3: 4, 1: 5, 0: 0}
    rk = np.vectorize(rank.get)(order)
    edge = np.zeros(a.shape, bool)
    for dy, dx in C.N4:
        s2, a2, r2 = C.shift(setid, dy, dx, -9), C.shift(a, dy, dx, False), C.shift(rk, dy, dx, -1)
        L2 = C.shift(L, dy, dx)
        nat = np.zeros(a.shape, bool)
        for p in (pures or []):
            Lp = lum(p)
            nat |= C.alpha(p) & C.shift(C.alpha(p), dy, dx, False) & (np.abs(Lp - C.shift(Lp, dy, dx)) > 60)
        edge |= a & a2 & (setid != s2) & (rk > r2) & (np.abs(L - L2) > 60) & (src >= 10) & ~nat
    for y, x in np.argwhere(edge):
        ys, xs = slice(max(0, y - 1), y + 2), slice(max(0, x - 1), x + 2)
        cand = img[ys, xs][C.alpha(img[ys, xs]) & (src[ys, xs] == src[y, x])]
        if len(cand):
            out[y, x, :3] = cand[np.argmin(lum(cand[None])[0])][:3]
            out[y, x, :3] = (out[y, x, :3] * 0.8).astype(np.uint8)
    return out, int(edge.sum())


# ---------- 2. 깜빡임 ----------
def best_shift(a, b, r=6):
    """b 를 (dy,dx) 옮겼을 때 a 와 불투명·색이 가장 많이 맞는 이동"""
    best, bd = -1, (0, 0)
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            bs = C.shift(b, dy, dx)
            m = C.alpha(a) & C.alpha(bs) & (C.coldiff(a, bs) <= 20)
            if m.sum() > best:
                best, bd = m.sum(), (dy, dx)
    return bd


def flicker(coat, maps):
    """연속 프레임에서 '같은 그림(같은 색, 전역 이동 맞춤)인데 라벨이 바뀐' 픽셀 수. 동작이 아니라 분할이 떨리는 정도."""
    tot, per = 0, {}
    for clip, n in CLIPS.items():
        for i in range(1, n):
            f0, f1 = "%s_%d" % (clip, i - 1), "%s_%d" % (clip, i)
            dy, dx = best_shift(coat[f1], coat[f0])
            c0 = C.shift(coat[f0], dy, dx)
            l0 = C.shift(maps[f0], dy, dx)
            same = C.alpha(coat[f1]) & C.alpha(c0) & (C.coldiff(coat[f1], c0) <= 20)
            k = int((same & (l0 != maps[f1])).sum())
            per[f1] = k
            tot += k
    return tot, per


def stabilize(coat, maps, keep=None):
    """앞·뒤 프레임 지도를 패치 정합(±4px)으로 현재 프레임에 옮겨 3표 다수결. 확실한 신호(keep 마스크)는 건드리지 않음."""
    out = {}
    for clip, n in CLIPS.items():
        for i in range(n):
            f = "%s_%d" % (clip, i)
            votes = [maps[f]]
            for j in (i - 1, i + 1):
                if 0 <= j < n:
                    g = "%s_%d" % (clip, j)
                    votes.append(prop_small(coat[g], maps[g], coat[f]))
            if len(votes) < 3:
                out[f] = maps[f]
                continue
            v = np.stack(votes)
            maj = np.where((v[1] == v[2]) & (v[1] != v[0]) & (v[1] > 0), v[1], v[0])
            if keep is not None:
                maj = np.where(keep[f], maps[f], maj)
            maj[~C.alpha(coat[f])] = 0
            out[f] = maj.astype(np.uint8)
    return out


def prop_small(src_img, src_lab, dst_img):
    oldR = M2.R
    M2.R = 4
    try:
        return M2.propagate(src_img, src_lab, dst_img)
    finally:
        M2.R = oldR


# ---------- 3. 세트별 보정 ----------
def set_map(lab, coat, other):
    """다른 세트 그림 전용 지도: 코트 배낭 픽셀과 ±2px 안에서 색이 같은(전이가 밀어 그린 배낭) 픽셀은 배낭(7)으로 → 그 세트 상의에서 빠짐."""
    back = lab == C.BACK
    if not back.any():
        return lab
    m = lab.copy()
    oa = C.alpha(other)
    match = np.zeros(lab.shape, bool)
    for dy in range(-2, 3):
        for dx in range(-2, 3):
            cs = C.shift(coat, dy, dx)
            bs = C.shift(back, dy, dx, False)
            match |= oa & bs & (C.coldiff(other, cs) <= 24)
    m[match & np.isin(lab, [C.TOP, C.HEM, C.LEGS, 0])] = C.BACK
    return m


def drop_strays(layer_mask, support, min_keep=4):
    """레이어 픽셀 중, 그 세트 그림의 큰 덩어리(support)와 이어지지 않는 4px 미만 조각은 버림."""
    keep = layer_mask.copy()
    rem = layer_mask.copy()
    while rem.any():
        y, x = np.argwhere(rem)[0]
        sd = np.zeros_like(rem); sd[y, x] = True
        comp = M1.cc_from(sd, layer_mask)
        rem &= ~comp
        if comp.sum() < min_keep and not (M1.dil(comp, 1) & support & ~comp).any():
            keep &= ~comp
    return keep


def layers_for(f, lab, imgs, under, refine, hem_to=2):
    lay = {}
    for s in SETS:
        L = set_map(lab, imgs["base_coat"], imgs[s]) if (refine in (True, "setmap") and s != "base_coat") else lab
        ly = C.make_layers(L, imgs[s], s, hem_to)
        if refine in (True, "strays"):
            support = C.alpha(under)
            for k in C.SLOTS:
                support = support | ly[k]
            big = np.zeros_like(support)
            # 지지 = 속옷 몸 + 다른 부위 레이어(이 부위 제외)
            for k in C.SLOTS:
                others = C.alpha(under).copy()
                for k2 in C.SLOTS:
                    if k2 != k:
                        others |= ly[k2]
                ly[k] = drop_strays(ly[k], others)
        lay[s] = ly
    return lay


def combo_scores(pkg, maps, refine, fill=True, outline=False, gt=None):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    M3.set_pkg(pkg)
    tot = dict(holes_excl_backpack=0, exposure=0, strays=0, seam_breaks=0, outlined_px=0, backpack_residue=0, intrusion=0)
    for f in C.FRAMES:
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lay = layers_for(f, maps[f], imgs, under[f], refine)
        fl = M3.make_fills(f, maps[f], imgs, lay) if fill else None
        glay = {s: C.make_layers(gt[f], imgs[s], s, 2) for s in SETS} if gt and f in gt and f != "ds_idle_0" else None
        # 배낭 잔점 = 잎사귀 상의 레이어 중 코트 배낭 팔레트(±12)에 드는 픽셀
        bk = coat[f][maps[f] == C.BACK][:, :3].astype(float)
        if len(bk):
            px = leaf[f][lay["leaf_set"][C.TOP]][:, :3].astype(float)
            if len(px):
                d = np.sqrt(((px[:, None] - np.unique(bk, axis=0)[None]) ** 2).sum(2)).min(1)
                tot["backpack_residue"] += int((d < 12).sum()) * 16  # 상의=잎사귀인 조합 16개
        for w in C.patterns(SETS):
            img, src = C.compose(under[f], w, imgs, lay, fl)
            hm = (src == 0) & C.alpha(coat[f]) & C.alpha(leaf[f]) & (maps[f] != C.BACK)
            tot["holes_excl_backpack"] += int(hm.sum())
            tot["exposure"] += C.exposure(src, under[f], coat[f], leaf[f])
            tot["strays"] += C.strays(img)
            if outline:
                img, k = outline_seams(img, src, [coat[f], leaf[f], under[f]])
                tot["outlined_px"] += k
            tot["seam_breaks"] += seam_pairs(img, src, [coat[f], leaf[f], under[f]])[0]
            if glay:
                ideal, isrc = C.compose(under[f], w, imgs, glay)
                tot["intrusion"] += C.gt_compare(img, src, ideal, isrc)[0]
    return tot


# ---------- 4. 키 프레임 속옷 몸 전파(질문 5) ----------
def warp_under(coat_src, under_src, coat_dst, R=12, lam=6.0):
    """원본(코트) 두 장 사이 패치 정합으로 대응을 구하고, 출처 속옷 몸 색을 대상 좌표로 가져온다."""
    S, T = M2.feat(coat_src), M2.feat(coat_dst)
    d0 = np.round(M2.anchor(coat_dst) - M2.anchor(coat_src)).astype(int)
    best = np.full((64, 64), 1e18)
    out = np.zeros((64, 64, 4), np.uint8)
    for dy in range(d0[0] - R, d0[0] + R + 1):
        for dx in range(d0[1] - R, d0[1] + R + 1):
            Ss = C.shift(S, dy, dx)
            diff = np.abs(T - Ss).sum(2)
            cost = np.zeros((64, 64))
            for ky in range(-2, 3):
                for kx in range(-2, 3):
                    cost += C.shift(diff, ky, kx)
            cost += lam * 25 * (abs(dy - d0[0]) + abs(dx - d0[1]))
            upd = cost < best
            best[upd] = cost[upd]
            out[upd] = C.shift(under_src, dy, dx)[upd]
    out[~C.alpha(coat_dst)] = 0
    return out


def under_study(pkg, maps):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    res = {}
    made = {}
    for mode in ("key_only_naive", "warp_from_clip_first", "warp_chain"):
        r = dict(sil_iou=[], color_err=[], exposure=0, holes=0)
        made[mode] = {}
        for clip, n in CLIPS.items():
            for i in range(n):
                f = "%s_%d" % (clip, i)
                k0 = clip + "_0"
                if i == 0:
                    u = under[f]  # 클립 첫 프레임만 생성본 사용
                elif mode == "key_only_naive":
                    u = under[k0]
                elif mode == "warp_from_clip_first":
                    u = warp_under(coat[k0], under[k0], coat[f])
                else:
                    p = "%s_%d" % (clip, i - 1)
                    u = warp_under(coat[p], made[mode][p], coat[f])
                made[mode][f] = u
                if i == 0:
                    continue
                a, b = C.alpha(u), C.alpha(under[f])
                r["sil_iou"].append(float((a & b).sum() / max(1, (a | b).sum())))
                both = a & b
                r["color_err"].append(float(C.coldiff(u, under[f])[both].mean() / 3) if both.any() else 999)
                imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
                lay = {s: C.make_layers(maps[f], imgs[s], s, 2) for s in SETS}
                for w in C.patterns(SETS):
                    img, src = C.compose(u, w, imgs, lay)
                    r["holes"] += int(((src == 0) & C.alpha(coat[f]) & C.alpha(leaf[f]) & (maps[f] != C.BACK)).sum())
                    r["exposure"] += int((C.alpha(coat[f]) & C.alpha(leaf[f]) & (src == 1) & (C.coldiff(u, coat[f]) > 30)).sum())
        res[mode] = dict(sil_iou=round(float(np.mean(r["sil_iou"])), 4), color_err=round(float(np.mean(r["color_err"])), 2),
                         holes_excl_backpack=r["holes"], exposure=r["exposure"], frames=len(r["sil_iou"]))
    # 기준: 생성본 속옷 몸 그대로(같은 17프레임)
    ref = dict(holes=0, exposure=0)
    for clip, n in CLIPS.items():
        for i in range(1, n):
            f = "%s_%d" % (clip, i)
            imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
            lay = {s: C.make_layers(maps[f], imgs[s], s, 2) for s in SETS}
            for w in C.patterns(SETS):
                img, src = C.compose(under[f], w, imgs, lay)
                ref["holes"] += int(((src == 0) & C.alpha(coat[f]) & C.alpha(leaf[f]) & (maps[f] != C.BACK)).sum())
                ref["exposure"] += C.exposure(src, under[f], coat[f], leaf[f])
    res["generated_under_reference"] = dict(sil_iou=1.0, color_err=0.0, holes_excl_backpack=ref["holes"], exposure=ref["exposure"], frames=17)
    return res, made


def main():
    pkg, mapdir, labdir, out = sys.argv[1:5]
    os.makedirs(os.path.join(out, "sheets"), exist_ok=True)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    maps = C.load_maps(mapdir)
    gt = C.load_maps(labdir)
    base = C.load_maps(os.path.join(pkg, "regions_current"))
    R = {}
    # 깜빡임
    R["flicker"] = {"baseline": flicker(coat, base)[0], "m1h": flicker(coat, maps)[0]}
    st = stabilize(coat, maps)
    R["flicker"]["m1h_stabilized"] = flicker(coat, st)[0]
    test = [f for f in gt if f != "ds_idle_0"]

    def miou(m):
        acc = {k: [0, 0] for k in range(1, 7)}
        for f in test:
            for k, (i, u) in C.iou(m[f], gt[f], range(1, 7)).items():
                acc[k][0] += i; acc[k][1] += u
        return round(float(np.mean([i / u for i, u in acc.values() if u])), 4)
    R["flicker"]["miou"] = {"baseline": miou(base), "m1h": miou(maps), "m1h_stabilized": miou(st)}
    os.makedirs(os.path.join(out, "regions_stable"), exist_ok=True)
    for f, m in st.items():
        C.save_map(m, os.path.join(out, "regions_stable", f + "_region.png"))
    print("flicker", R["flicker"])
    # 이음새·세트별 보정·조각
    R["combos"] = {
        "best_v1 (M1h+hemTop+fill)": combo_scores(pkg, maps, refine=False, gt=gt),
        "+ stray drop only": combo_scores(pkg, maps, refine="strays", gt=gt),
        "+ per-set map only": combo_scores(pkg, maps, refine="setmap", gt=gt),
        "+ per-set map & stray drop": combo_scores(pkg, maps, refine=True, gt=gt),
        "+ stray drop + seam outline": combo_scores(pkg, maps, refine="strays", outline=True, gt=gt),
        "+ per-set & strays + seam outline": combo_scores(pkg, maps, refine=True, outline=True, gt=gt),
    }
    for k, v in R["combos"].items():
        print(k, v)
    # 속옷 몸 전파
    R["under_propagation"], made = under_study(pkg, maps)
    for k, v in R["under_propagation"].items():
        print("under", k, v)
    json.dump(R, open(os.path.join(out, "study2.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    sheet_under(pkg, made, os.path.join(out, "sheets", "under_propagation.png"))
    sheet_seam(pkg, maps, os.path.join(out, "sheets", "seam_outline.png"))


def sheet_under(pkg, made, path):
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    fr = ["ds_idle_4", "ds_stab_2", "ds_down_2", "ds_up_2", "ds_up_4"]
    rows = [("generated", under)] + [(k, v) for k, v in made.items()]
    sh = Image.new("RGB", (100 + 66 * len(fr), 66 * len(rows) + 14), (20, 20, 20))
    d = ImageDraw.Draw(sh)
    for c, f in enumerate(fr):
        d.text((100 + c * 66, 1), f, fill=(255, 255, 255))
    for r, (n, imgs) in enumerate(rows):
        d.text((2, 14 + r * 66 + 28), n[:16], fill=(255, 255, 0))
        for c, f in enumerate(fr):
            b = Image.new("RGBA", (64, 64), (70, 70, 70, 255)); b.alpha_composite(Image.fromarray(imgs[f]))
            sh.paste(b.convert("RGB"), (100 + c * 66, 14 + r * 66))
    sh.resize((sh.width * 4, sh.height * 4), Image.NEAREST).save(path)


def sheet_seam(pkg, maps, path):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    M3.set_pkg(pkg)
    cases = [("cccll", "ds_idle_0"), ("clccc", "ds_stab_1"), ("lclll", "ds_down_2"), ("llcll", "ds_up_2")]
    sh = Image.new("RGB", (66 * 3 * len(cases), 80), (20, 20, 20))
    d = ImageDraw.Draw(sh)
    for c, (code, f) in enumerate(cases):
        wear = {s: ("base_coat" if ch == "c" else "leaf_set") for s, ch in zip(C.SLOTS, code)}
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lay = layers_for(f, maps[f], imgs, under[f], "strays")
        fl = M3.make_fills(f, maps[f], imgs, lay)
        img, src = C.compose(under[f], wear, imgs, lay, fl)
        _, mk = seam_pairs(img, src, [coat[f], leaf[f], under[f]])
        ol, _ = outline_seams(img, src, [coat[f], leaf[f], under[f]])
        mark = (img.copy() * 0.35).astype(np.uint8); mark[:, :, 3] = 255; mark[mk] = (255, 60, 60, 255)
        for k, im in enumerate([img, mark, ol]):
            b = Image.new("RGBA", (64, 64), (70, 70, 70, 255)); b.alpha_composite(Image.fromarray(im))
            sh.paste(b.convert("RGB"), (c * 198 + k * 66, 14))
        d.text((c * 198 + 2, 1), "%s %s | breaks | outlined" % (code, f[3:]), fill=(255, 255, 255))
    sh.resize((sh.width * 4, sh.height * 4), Image.NEAREST).save(path)


if __name__ == "__main__":
    main()
