"""슬롯 설계 비교(SLOT_DECISION.md): 5칸 vs 4칸(갑옷 한 벌), 그리고 4칸 개선(세트별 모자 + 지울 마스크, 속옷 몸 목·턱 보정).

사용: python study4_slots.py <패키지> <지도 폴더> <labels> <보정 속옷 몸 폴더(under_fixed)> <출력 json>
지표(21프레임, 세트 2벌의 조합 전부 — 5칸 32개 / 4칸 16개):
  holes, exposure, strays (evaluate.py 와 같은 정의; 지울 마스크 자리는 구멍에서 뺌 — 의도된 투명)
  intrusion / cut        : 시험 7장에서 정답 지도로 같은 규칙(같은 모자 확장·같은 속옷 몸)으로 합성한 이상 결과와 비교
  head_mismatch_all_leaf : 전부 잎사귀일 때 머리 구역이 잎사귀 원본 그림과 다른 픽셀(전이 얼굴 잡음 제외)
  miou_5 / miou_4        : 시험 7장 IoU(4칸은 상의·바지·자락·배낭을 갑옷 하나로)
"""
import json
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import layered_extract as LE
import m1_underdiff as M1
import m3_amodal as M3

SETS = ["base_coat", "leaf_set"]


def fills(f, lab, imgs, lay, under, armor):
    if not armor:
        return M3.make_fills(f, lab, imgs, lay)
    g = M1.under_geometry(under)
    zone = g["ua"] & ~g["shoe_zone"] & ~np.isin(lab, [C.CAP, C.BODY, C.GLOVE])
    out = {}
    for s, img in imgs.items():
        todo = zone & ~lay[s][C.TOP]
        if lay[s][C.TOP].any() and todo.any():
            out[(s, C.TOP)] = (todo, M3.patch_copy(img, lay[s][C.TOP], todo, prefer=(0, 0)))
    return out


def run(pkg, maps, unders, armor, cap, gt=None):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    t = dict(holes=0, exposure=0, strays=0, head_mismatch_all_leaf=0, intrusion=0, cut=0, combos=0)
    for f in C.FRAMES:
        u = unders[f]
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lab = maps[f]
        lay = {s: C.make_layers(lab, imgs[s], s, 2, u if cap else None, armor=armor) for s in SETS}
        fl = fills(f, lab, imgs, lay, u, armor)
        glay = ({s: C.make_layers(gt[f], imgs[s], s, 2, u if cap else None, armor=armor) for s in SETS}
                if gt is not None and f in gt and f != "ds_idle_0" else None)
        g = M1.under_geometry(u)
        hz = LE.head_zone(g) | M1.dil(lab == C.CAP, 2)
        for w in C.patterns(SETS):
            if armor and w[C.LEGS] != w[C.TOP]:
                continue
            img, src = C.compose(u, w, imgs, lay, fl)
            er = lay[w[C.CAP]].get("erase_cap", np.zeros((64, 64), bool))
            t["holes"] += int(((src == 0) & C.alpha(coat[f]) & C.alpha(leaf[f]) & (lab != C.BACK) & ~er).sum())
            t["exposure"] += C.exposure(src, u, coat[f], leaf[f])
            t["strays"] += C.strays(img)
            t["combos"] += 1
            if glay:  # 정답 지도로 같은 규칙(같은 모자 확장)으로 합성한 이상 결과와 비교
                ideal, isrc = C.compose(u, w, imgs, glay)
                a, b = C.gt_compare(img, src, ideal, isrc)
                t["intrusion"] += a
                t["cut"] += b
            if all(v == "leaf_set" for v in w.values()):
                mm = hz & ((C.coldiff(img, leaf[f]) > 30) | (C.alpha(img) != C.alpha(leaf[f])))
                mm &= ~(C.alpha(leaf[f]) & (M1.near_diff(leaf[f], u, 1) <= 30))
                t["head_mismatch_all_leaf"] += int(mm.sum())
    return t


def miou(maps, gt, armor):
    acc = {}
    for f in gt:
        if f == "ds_idle_0":
            continue
        a, b = maps[f].copy(), gt[f].copy()
        if armor:
            for x in (a, b):
                x[np.isin(x, [C.TOP, C.LEGS, C.HEM, C.BACK])] = C.TOP
        cls = [1, 2, 3, 5, 6] if armor else [1, 2, 3, 4, 5, 6]
        for k, (i, u) in C.iou(a, b, cls).items():
            acc.setdefault(k, [0, 0]); acc[k][0] += i; acc[k][1] += u
    per = {("armor" if armor and k == 2 else C.NAMES[k]): round(i / u, 3) for k, (i, u) in acc.items()}
    return round(float(np.mean(list(per.values()))), 4), per


def main():
    pkg, mapdir, labdir, ufix, out = sys.argv[1:6]
    M3.set_pkg(pkg)
    maps, gt = C.load_maps(mapdir), C.load_maps(labdir)
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    fixed = C.load_dir(ufix)
    res = {}
    base = C.load_maps(os.path.join(pkg, "regions_current"))
    rb = run(pkg, base, under, True, False, gt)
    rb["per_combo"] = {k: round(v / rb["combos"], 2) for k, v in rb.items() if k not in ("combos", "head_mismatch_all_leaf")}
    rb["miou"], rb["iou"] = miou(base, gt, True)
    res["baseline map, 4slot"] = rb
    print("baseline map, 4slot", rb)
    for name, armor, cap, U in [("5slot", False, False, under), ("4slot", True, False, under),
                                ("4slot + set cap + erase", True, True, under),
                                ("4slot + set cap + erase + under neck fix", True, True, fixed)]:
        r = run(pkg, maps, U, armor, cap, gt)
        r["per_combo"] = {k: round(v / r["combos"], 2) for k, v in r.items() if k not in ("combos", "head_mismatch_all_leaf")}
        r["miou"], r["iou"] = miou(maps, gt, armor)
        res[name] = r
        print(name, r)
    json.dump(res, open(out, "w", encoding="utf-8"), indent=1, ensure_ascii=False)


if __name__ == "__main__":
    main()
