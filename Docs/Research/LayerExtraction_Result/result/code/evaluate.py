"""평가: 부위 IoU(정답 라벨 프레임) + 조합 시트 지표(32조합 × 21프레임).

사용: python evaluate.py <패키지 폴더> <labels 폴더> <metrics.json> 이름=지도폴더 [이름=지도폴더 ...]
  패키지 폴더 = frames_coat_original / frames_underwear_base / frames_leaf_set_transfer 가 있는 곳.
지표:
  iou_per_class   : 정답 라벨 시험 프레임(키 프레임 ds_idle_0 제외 7장) 합산 IoU, 클래스 1~8
  holes           : 투명 구멍(코트·잎사귀 모두 불투명인데 결과 투명) — 라벨 불필요, 21프레임 × 32조합 합계
  exposure        : 두 옷이 모두 덮는 자리에 속옷 몸이 드러난 픽셀(뜯은 자리 빈칸) — 라벨 불필요, 21×32 합계
  strays          : 몸에서 떨어져 떠 있는 픽셀(라벨 불필요 침범 지표) — 21×32 합계
  intrusion / cut : 정답 지도로 합성한 '이상 결과'와 비교한 침범·잘림 픽셀 — 시험 7프레임 × 32조합 합계(이상 합성은 평가 대상과 같은 자락 정책)
"""
import json
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

KEY = "ds_idle_0"


def evaluate(pkg, gt_maps, maps, fills=None, hem_to=None, with_none=False):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    sets = ["base_coat", "leaf_set"]
    pats = C.patterns(sets)
    none_pats = C.patterns(sets + [None]) if with_none else []
    test = [f for f in gt_maps if f != KEY]
    acc = {k: [0, 0] for k in range(1, 9)}
    for f in test:
        for k, (i, u) in C.iou(maps[f], gt_maps[f]).items():
            acc[k][0] += i
            acc[k][1] += u
    ious = {C.NAMES[k]: (round(i / u, 4) if u else None) for k, (i, u) in acc.items()}
    slot_ious = [ious[C.NAMES[k]] for k in (1, 2, 3, 4, 5, 6) if ious[C.NAMES[k]] is not None]
    tot = dict(holes=0, exposure=0, intrusion=0, cut=0, strays=0)
    per_frame = {}
    ebl = [0] * 9
    hbl = [0] * 9
    for f in C.FRAMES:
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lay = {s: C.make_layers(maps[f], imgs[s], s, hem_to) for s in sets}
        fl = fills(f, maps[f], imgs, lay) if fills else None
        glay = {s: C.make_layers(gt_maps[f], imgs[s], s, hem_to) for s in sets} if f in test else None  # 이상 합성도 같은 자락 정책
        pf = dict(holes=0, exposure=0, intrusion=0, cut=0, strays=0, exp_by_label=[0] * 9, hole_by_label=[0] * 9)
        for w in pats:
            img, src = C.compose(under[f], w, imgs, lay, fl)
            pf["holes"] += C.holes(src, coat[f], leaf[f])
            hm = (src == 0) & C.alpha(coat[f]) & C.alpha(leaf[f])
            for k in range(9):
                pf["hole_by_label"][k] += int((hm & (maps[f] == k)).sum())
            em = C.exposure_mask(src, under[f], coat[f], leaf[f])
            for k in range(9):
                pf["exp_by_label"][k] += int((em & (maps[f] == k)).sum())
            pf["exposure"] += C.exposure(src, under[f], coat[f], leaf[f])
            pf["strays"] += C.strays(img)
            if glay:
                ideal, isrc = C.compose(under[f], w, imgs, glay)
                a, b = C.gt_compare(img, src, ideal, isrc)
                pf["intrusion"] += a
                pf["cut"] += b
        pf["filled"] = 0
        if fl:
            for w in pats:
                img, src = C.compose(under[f], w, imgs, lay, fl)
                pf["filled"] += int((src >= 100).sum())
        if with_none:
            pf["holes_243"] = 0
            for w in none_pats:
                img, src = C.compose(under[f], w, imgs, lay, fl)
                pf["holes_243"] += C.holes(src, coat[f], leaf[f]) if all(v is not None for v in w.values()) else int(((src == 0) & C.alpha(under[f])).sum())
        if glay:
            pf["miou"] = round(float(np.mean([i / u for k, (i, u) in C.iou(maps[f], gt_maps[f], range(1, 9)).items() if u])), 4)
        per_frame[f] = pf
        for k in tot:
            tot[k] += pf[k]
        for k in range(9):
            ebl[k] += pf["exp_by_label"][k]
            hbl[k] += pf["hole_by_label"][k]
    return {"iou_per_class": ious, "miou_slots_1to6": round(float(np.mean(slot_ious)), 4), **tot, "holes_by_map_label": {C.NAMES.get(k, "none"): hbl[k] for k in range(9) if hbl[k]}, "exposure_by_map_label": {C.NAMES.get(k, "none"): ebl[k] for k in range(9) if ebl[k]},
            "frames": len(C.FRAMES), "patterns": len(pats), "test_frames": test, "per_frame": per_frame}


def main():
    pkg, lab_dir, out = sys.argv[1:4]
    gt = C.load_maps(lab_dir)
    res = json.load(open(out, encoding="utf-8")) if os.path.exists(out) else {}
    for arg in sys.argv[4:]:
        # 이름=지도폴더[:fill][:hem=N][:none]   fill = M3 보완 층, hem = 자락 소속 강제, none = 미착용 포함 243조합 구멍
        name, d = arg.split("=", 1)
        opts = d.split(":")
        d = opts[0] if not (len(opts[0]) == 1 and len(opts) > 1) else opts[0] + ":" + opts[1]
        opts = [o for o in opts if o not in d.split(":")]
        fills = None
        if "fill" in opts:
            import m3_amodal
            m3_amodal.set_pkg(pkg)
            fills = m3_amodal.make_fills
        hem = next((int(o[4:]) for o in opts if o.startswith("hem=")), None)
        r = evaluate(pkg, gt, C.load_maps(d), fills, hem, "none" in opts)
        if "none" in opts:
            r["holes_243_patterns"] = sum(v["holes_243"] for v in r["per_frame"].values())
        r["filled_px"] = sum(v["filled"] for v in r["per_frame"].values())
        r["holes_excl_backpack"] = r["holes"] - r["holes_by_map_label"].get("back", 0)  # 배낭 제거 자리는 의도된 투명
        r["options"] = opts
        res[name] = r
        print(name, "mIoU", r["miou_slots_1to6"], {k: r[k] for k in ("holes", "holes_excl_backpack", "exposure", "strays", "intrusion", "cut", "filled_px")}, r.get("holes_243_patterns", ""), r["iou_per_class"])
    json.dump(res, open(out, "w", encoding="utf-8"), indent=1, ensure_ascii=False)


if __name__ == "__main__":
    main()
