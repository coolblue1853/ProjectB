"""하이브리드 레이어(gen 실험 1 후속, 권장): 보이는 픽셀 = 한 번에 입힌 고품질 그림, 가려진 픽셀 = 단계 생성(하의만).

배경: 부위별로 따로 생성하면 품질이 급감(사용자 확인, 11-10). 그래서 단계 생성은 '가려진 부분'을 얻는 데만 쓴다.
  보이는 층  L[s] = 완성 그림(원본 코트 또는 세트 전이 그림)의 불투명 픽셀 중 공용 지도(확장 3칸, 자락·배낭 → 상의)가 s 인 곳.
  가려진 층  하의 = 단계 A(속옷 몸 + 하의만 생성) 바지 픽셀 중
            ① 완성 그림 실루엣 안(정말 가려진 곳)  ② 보이는 하의가 아닌 곳  ③ 맨몸·신발·장갑 자리가 아닌 곳.
            색은 보이는 하의 팔레트에 밝기 순위로 맞춤(--no-recolor 로 끔). 생성 바지 색이 원본과 달라도 이음새가 안 생김.
  상의의 가려진 부분(배낭 밑)은 아직 단계 생성이 없으므로 m3_amodal 보완(선택, --fill-top).
사용:
  python hybrid_layers.py <완성 그림 폴더> <공용 지도 폴더> <단계A 하의 레이어 폴더(layered_extract 출력의 layers/legs)> <출력 폴더>
        [--no-recolor] [--fill-top <패키지>]
출력: <출력>/layers/<부위>/<프레임>.png, <출력>/regions/<프레임>_region.png(공용 지도 복사), <출력>/hybrid_report.json
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

NAMES = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}


def lum(a):
    f = a[..., :3].astype(float)
    return 0.299 * f[..., 0] + 0.587 * f[..., 1] + 0.114 * f[..., 2]


def recolor(src_px, ref_px):
    """src 색들을 밝기 분위수로 ref 팔레트에 대응(같은 순위 → 같은 분위의 ref 색)"""
    if len(ref_px) == 0 or len(src_px) == 0:
        return src_px
    med = np.median(ref_px[:, :3].astype(float), axis=0)
    keep = np.abs(ref_px[:, :3].astype(float) - med).sum(1) < 90  # 자락 끝 청록·버클 같은 튀는 색은 팔레트에서 뺌
    if keep.sum() >= 3:
        ref_px = ref_px[keep]
    ref = ref_px[np.argsort(lum(ref_px[None])[0])]
    sl = lum(src_px[None])[0]
    rank = sl.argsort().argsort() / max(1, len(sl) - 1)
    idx = np.clip(np.round(rank * (len(ref) - 1)).astype(int), 0, len(ref) - 1)
    out = src_px.copy()
    out[:, :3] = ref[idx][:, :3]
    return out


def build(full, lab, stage_legs, do_recolor=True):
    em = C.ext_map(lab, full)
    em[(em == C.HEM) | (em == C.BACK)] = C.TOP
    fa = C.alpha(full)
    lay = {}
    for s in C.SLOTS:
        L = np.zeros_like(full)
        vis = fa & (em == s)
        L[vis] = full[vis]
        lay[s] = L
    vis_legs = fa & (em == C.LEGS)
    gl = C.alpha(stage_legs)
    hid = gl & fa & ~vis_legs & ~np.isin(lab, [C.BODY, C.SHOES, C.GLOVE]) & ~np.isin(em, [C.SHOES, C.GLOVE])
    px = stage_legs[hid].copy()
    if do_recolor:
        px = recolor(px, full[vis_legs])
    lay[C.LEGS][hid] = px
    cover = float((gl & vis_legs).sum() / max(1, vis_legs.sum()))
    return lay, dict(visible_legs=int(vis_legs.sum()), hidden_legs=int(hid.sum()), stage_covers_visible=round(cover, 3))


def main():
    fdir, mapdir, ldir, out = sys.argv[1:5]
    do_rc = "--no-recolor" not in sys.argv
    maps = C.load_maps(mapdir)
    frames = [f for f in C.FRAMES if os.path.exists(os.path.join(ldir, f + ".png")) and f in maps]
    rep = {}
    for f in frames:
        full = C.load(os.path.join(fdir, f + ".png"))
        lay, r = build(full, maps[f], C.load(os.path.join(ldir, f + ".png")), do_rc)
        if "--fill-top" in sys.argv:
            import m3_amodal as M3
            M3.set_pkg(sys.argv[sys.argv.index("--fill-top") + 1])
            fl = M3.make_fills(f, maps[f], {"x": full}, {"x": {s: C.alpha(lay[s]) for s in C.SLOTS}})
            if ("x", C.TOP) in fl:
                m, img = fl[("x", C.TOP)]
                m = m & ~C.alpha(lay[C.TOP])
                lay[C.TOP][m] = img[m]  # 상의 레이어에 보완을 합침(보완은 자기 레이어 빈 곳만)
                r["top_fill"] = int(m.sum())
        for s, L in lay.items():
            d = os.path.join(out, "layers", NAMES[s])
            os.makedirs(d, exist_ok=True)
            Image.fromarray(L).save(os.path.join(d, f + ".png"))
        os.makedirs(os.path.join(out, "regions"), exist_ok=True)
        C.save_map(maps[f], os.path.join(out, "regions", f + "_region.png"))
        rep[f] = r
    json.dump(rep, open(os.path.join(out, "hybrid_report.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    print("hybrid", len(frames), "recolor" if do_rc else "no-recolor")


if __name__ == "__main__":
    main()
