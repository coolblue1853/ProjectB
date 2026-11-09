"""한 겹씩 입히기 레이어 채점(GEN_RESEARCH_GUIDE.md 1-5).

같은 프레임·같은 32조합(새 세트 ↔ 잎사귀)으로 두 줄을 같은 규칙으로 계산해 나란히 낸다.
  reference : 1차 권장안 = 원본 코트를 M1h 지도로 자르고 보완 층(M3)·자락 상의 통일
  gen       : layered_extract.py 가 뽑은 단계 레이어(자르지 않음, 보완 층 없음)
잎사귀 쪽은 두 줄 모두 M1h 지도 + 보완 층(1차 권장안과 같은 조건).
사용: python score_layers.py <패키지> <layered_extract 출력 폴더> <M1h 지도 폴더(result/regions)>
"""
import json
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import m3_amodal as M3

NAMES = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}


def score(under, new_img, new_lay, new_fill, leaf, lab, f):
    imgs = {"new": new_img, "leaf_set": leaf}
    leaf_lay = C.make_layers(lab, leaf, "leaf_set", 2)
    lay = {"new": new_lay, "leaf_set": leaf_lay}
    fl = {k: v for k, v in M3.make_fills(f, lab, {"leaf_set": leaf}, {"leaf_set": leaf_lay}).items()}
    if new_fill:
        fl.update(new_fill)
    t = dict(holes=0, exposure=0, strays=0)
    for w in C.patterns(["new", "leaf_set"]):
        img, src = C.compose(under, w, imgs, lay, fl)
        t["holes"] += int(((src == 0) & C.alpha(new_img) & C.alpha(leaf) & (lab != C.BACK)).sum())
        t["exposure"] += int((C.alpha(new_img) & C.alpha(leaf) & (src == 1) & (C.coldiff(under, new_img) > 30)).sum())
        t["strays"] += C.strays(img)
    return t


def main():
    pkg, out, regdir = sys.argv[1:4]
    M3.set_pkg(pkg)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    maps = C.load_maps(regdir)
    frames = [f for f in C.FRAMES if os.path.exists(os.path.join(out, "layers", "top", f + ".png"))]
    res = {"frames": frames, "reference": dict(holes=0, exposure=0, strays=0), "gen": dict(holes=0, exposure=0, strays=0)}
    for f in frames:
        # 1차 권장안(원본 코트 자르기)
        cl = C.make_layers(maps[f], coat[f], "base_coat", 2)
        cf = {k: v for k, v in M3.make_fills(f, maps[f], {"base_coat": coat[f]}, {"base_coat": cl}).items()}
        cf = {("new", k[1]): v for k, v in cf.items()}
        r = score(under[f], coat[f], cl, cf, leaf[f], maps[f], f)
        # 단계 레이어
        new = {}
        for s, n in NAMES.items():
            p = os.path.join(out, "layers", n, f + ".png")
            new[s] = C.load(p) if os.path.exists(p) else np.zeros_like(coat[f])
        img = under[f].copy()
        for s in C.DRAW_ORDER:
            m = C.alpha(new[s]); img[m] = new[s][m]
        g = score(under[f], img, {s: C.alpha(new[s]) for s in C.SLOTS}, None, leaf[f], maps[f], f)
        for k in r:
            res["reference"][k] += r[k]
            res["gen"][k] += g[k]
    print(json.dumps(res, ensure_ascii=False))


if __name__ == "__main__":
    main()
