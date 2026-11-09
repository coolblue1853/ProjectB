"""layered_extract.py 모의 검증(생성 0): 정답을 아는 가짜 '단계 그림'을 만들어 추출이 맞는지 본다.

  단계 A(가짜) = 속옷 몸 + 코트 레깅스(보완 층으로 자락 밑까지 완성) + 신발
  단계 B(가짜) = 단계 A 위에 원본 코트 그림 전체(상의·배낭·자락)
  교란(전이가 다시 그린 것 흉내): B 에서 다리 쪽 8×8 블록을 1px 밀기 + 픽셀 10% 에 색 ±8 잡음
측정: 레이어만으로 단계 그림 재구성 오차(틀린 픽셀), 상의 레이어로 샌 하의 픽셀, drift 검출 수.
사용: python sim_layered.py <패키지> <지도 폴더> <출력 json>
"""
import json
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import layered_extract as LE
import m3_amodal as M3


def iou(a, b):
    u = (a | b).sum()
    return float((a & b).sum() / u) if u else 1.0


def main():
    pkg, mapdir, out = sys.argv[1:4]
    M3.set_pkg(pkg)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    maps = C.load_maps(mapdir)
    rng = np.random.default_rng(0)
    res = {"clean": {}, "perturbed": {}}
    for f in C.FRAMES:
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lay = {s: C.make_layers(maps[f], imgs[s], s, 2) for s in imgs}
        fl = M3.make_fills(f, maps[f], imgs, lay)
        legs = lay["base_coat"][C.LEGS].copy()
        A = under[f].copy()
        legs_rgba = np.zeros_like(A)
        legs_rgba[legs] = coat[f][legs]
        if ("base_coat", C.LEGS) in fl:
            fm, fimg = fl[("base_coat", C.LEGS)]
            legs_rgba[fm & ~legs] = fimg[fm & ~legs]
            legs = legs | fm
        shoes = lay["base_coat"][C.SHOES]
        A[legs] = legs_rgba[legs]
        A[shoes] = coat[f][shoes]
        B = A.copy()
        ca = C.alpha(coat[f])
        B[ca] = coat[f][ca]
        # 정답: 각 단계에서 실제로 새로 그려진 픽셀(같은 색이면 '새로 그림'이 아님 → 추출 정의와 같게)
        trueA = C.alpha(A) & ((C.coldiff(A, under[f]) > LE.T_DIFF) | ~C.alpha(under[f]))
        trueB = C.alpha(B) & ((C.coldiff(B, A) > LE.T_DIFF) | ~C.alpha(A))
        for mode in ("clean", "perturbed"):
            Bm = B.copy()
            drift_true = 0
            if mode == "perturbed":
                ys, xs = np.nonzero(legs & ~shoes)
                if len(ys):
                    y0, x0 = int(np.median(ys)) - 4, int(np.median(xs)) - 4
                    blk = (slice(max(0, y0), y0 + 8), slice(max(0, x0), x0 + 8))
                    moved = C.shift(Bm, 0, 1)
                    before = Bm[blk].copy()
                    Bm[blk] = moved[blk]
                    drift_true = int((C.coldiff(Bm, B) > LE.T_DIFF)[blk].sum())
                noise = rng.integers(-8, 9, size=Bm[:, :, :3].shape)
                pick = rng.random(Bm.shape[:2]) < 0.10
                Bm[:, :, :3] = np.where(pick[:, :, None], np.clip(Bm[:, :, :3].astype(int) + noise, 0, 255), Bm[:, :, :3]).astype(np.uint8)
            layers, owner, rep = LE.extract(under[f], [(["legs", "shoes"], A), (["top"], Bm)], f)
            al = lambda k: C.alpha(layers[k + "_rgba"]) if k + "_rgba" in layers else np.zeros((64, 64), bool)
            recA = under[f].copy()
            for k in ("legs", "shoes"):
                m = al(k); recA[m] = layers[k + "_rgba"][m]
            recB = recA.copy()
            m = al("top"); recB[m] = layers["top_rgba"][m]
            errA = int(((C.coldiff(recA, A) > LE.T_DIFF) | (C.alpha(recA) != C.alpha(A))).sum())
            errB = int(((C.coldiff(recB, Bm) > LE.T_DIFF) | (C.alpha(recB) != C.alpha(Bm))).sum())
            leak = int((al("top") & ~trueB).sum())  # 상의 레이어로 샌 하의 픽셀(밀림·잡음)
            r = dict(recon_err_stageA=errA, recon_err_stageB=errB, top_leak_px=leak, top_iou=iou(al("top"), trueB),
                     drift_flagged=rep["top"]["drift_px"], drift_injected=drift_true,
                     legs_layer_px=int(al("legs").sum()), legs_visible_in_original_px=int(lay["base_coat"][C.LEGS].sum()))
            res[mode][f] = r
    summ = {}
    for mode, d in res.items():
        summ[mode] = {k: round(float(np.mean([v[k] for v in d.values()])), 4) for k in next(iter(d.values()))}
    res["summary"] = summ
    json.dump(res, open(out, "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    print(json.dumps(summ, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    main()
