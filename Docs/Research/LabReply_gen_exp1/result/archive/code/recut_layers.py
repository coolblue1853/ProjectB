"""단계 레이어 재절단(gen 실험 1 후속): '보이는 픽셀은 공용 지도, 가려진 픽셀만 단계 레이어'.

문제(gen_exp1): layered_extract 레이어는 세트마다 소속이 제각각이라
  - 다른 세트와 섞으면 같은 픽셀을 서로 다른 부위로 가져 아무도 안 그리는 자리 = 구멍(448),
  - 전이가 머리·손을 1px 다시 그린 픽셀이 상의로 들어가, 상의를 바꾸면 그 자리가 속옷 몸으로 보임 = 노출.
규칙:
  보이는 층  L_vis[s] = 최종 그림의 불투명 픽셀 중 '공용 지도(확장 3칸 + 세트 정책)'가 s 인 곳, 색 = 최종 그림.
  가려진 층  L_hid[s] = 단계 s 에서 새로 그렸지만 최종 그림에선 더 위 단계에 덮인 픽셀, 색 = 단계 s 그림.
             (예: 코트 자락 밑 바지. 다른 세트 상의를 입었을 때만 보인다)
  레이어     L[s] = L_vis[s] ∪ (L_hid[s] − 공용 지도가 맨몸(6)인 곳)
  맨몸(6)은 어느 세트도 그리지 않는다 → 머리·목 재그림 잡음은 자동으로 빠짐.
사용: python recut_layers.py <layered_extract 출력 폴더> <공용 지도 폴더> <출력 폴더> [--hem top|legs] [--back top|drop]
  공용 지도 = 모든 세트를 자르는 같은 지도(지금은 result/regions = 원본 코트 M1h).
출력: <출력>/layers/<부위>/<프레임>.png, <출력>/recut_report.json (부위별 보이는/가려진 픽셀 수)
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

NAMES = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}
SLOT_OF = {v: k for k, v in NAMES.items()}


def shared_owner(lab, final, hem_to, back_to):
    """공용 지도를 이 세트 그림 실루엣으로 3칸 확장하고 세트 정책(자락·배낭)을 적용한 슬롯 지도"""
    em = C.ext_map(lab, final)
    em[em == C.HEM] = hem_to
    em[em == C.BACK] = back_to
    return em


def recut(stage_layers, owner_stage, lab, hem_to=C.TOP, back_to=C.TOP):
    """stage_layers: {슬롯: RGBA}(layered_extract 출력), owner_stage: layered_extract 지도(보이는 픽셀의 출처 단계)"""
    final = np.zeros((64, 64, 4), np.uint8)
    for s in C.DRAW_ORDER:  # 런타임 순서로 쌓은 것 = 최종 그림
        if s in stage_layers:
            m = C.alpha(stage_layers[s]); final[m] = stage_layers[s][m]
    em = shared_owner(lab, final, hem_to, back_to)
    fa = C.alpha(final)
    out, rep = {}, {}
    for s in C.SLOTS:
        vis = fa & (em == s)
        L = np.zeros_like(final)
        L[vis] = final[vis]
        hid = np.zeros(fa.shape, bool)
        if s in stage_layers:
            sa = C.alpha(stage_layers[s])
            hid = sa & (owner_stage != s) & (lab != C.BODY) & ~vis
            L[hid] = stage_layers[s][hid]
        out[s] = L
        rep[NAMES[s]] = dict(visible=int(vis.sum()), hidden=int(hid.sum()))
    return out, rep


def main():
    src, mapdir, out = sys.argv[1:4]
    hem_to = C.LEGS if "--hem" in sys.argv and sys.argv[sys.argv.index("--hem") + 1] == "legs" else C.TOP
    back_to = 0 if "--back" in sys.argv and sys.argv[sys.argv.index("--back") + 1] == "drop" else C.TOP
    maps = C.load_maps(mapdir)
    frames = [f for f in C.FRAMES if os.path.exists(os.path.join(src, "regions", f + "_region.png"))]
    report = {}
    for f in frames:
        st = {}
        for s, n in NAMES.items():
            p = os.path.join(src, "layers", n, f + ".png")
            if os.path.exists(p):
                st[s] = C.load(p)
        owner = np.array(Image.open(os.path.join(src, "regions", f + "_region.png")))
        lay, rep = recut(st, owner, maps[f], hem_to, back_to)
        for s, L in lay.items():
            d = os.path.join(out, "layers", NAMES[s])
            os.makedirs(d, exist_ok=True)
            Image.fromarray(L).save(os.path.join(d, f + ".png"))
        # score_layers 가 프레임을 찾도록 지도도 둔다(공용 지도 그대로)
        os.makedirs(os.path.join(out, "regions"), exist_ok=True)
        C.save_map(maps[f], os.path.join(out, "regions", f + "_region.png"))
        report[f] = rep
    json.dump(report, open(os.path.join(out, "recut_report.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    print("recut", len(frames), "hem->", NAMES.get(hem_to), "back->", NAMES.get(back_to, "drop"))


if __name__ == "__main__":
    main()
