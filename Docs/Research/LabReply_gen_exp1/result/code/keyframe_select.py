"""생성할 키 프레임 고르기(생성 0): 원본 캐릭터끼리 옮겨 보기로 '옮기기 쉬운 정도'를 재고, 최소 키 집합을 고른다.

  proxy(k→f) = 원본 코트 k 를 패치 정합으로 f 자리에 옮겼을 때 원본 f 와 틀린 픽셀 비율(색 차>60 또는 덮음 불일치).
               세트 그림도 같은 대응으로 옮기므로 세트와 무관하게 미리 계산할 수 있다.
  선택 = 모든 프레임이 '어떤 키에서 proxy ≤ τ' 가 되도록 하는 탐욕적 최소 집합(같은 동작 클립 안에서만 옮김).
  검증(잎사귀 실물이 있을 때): 고른 키에서 잎사귀를 옮겨 실제 잎사귀와 실루엣 IoU·색 틀림 비율.
사용: python keyframe_select.py <패키지> <출력 json> [τ 목록, 기본 0.15,0.25,0.35]
새 동작에 쓰려면: frames_coat_original 에 새 동작 원본 프레임(ds_<동작>_<k>.png)만 넣으면 된다(속옷 몸·세트 그림 불필요).
"""
import json
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import study5_cost as S5


def frames_by_clip(names):
    clips = {}
    for f in names:
        clips.setdefault(f.rsplit("_", 1)[0], []).append(f)
    for c in clips:
        clips[c].sort(key=lambda f: int(f.rsplit("_", 1)[1]))
    return clips


def proxy_err(a, b_true, warped):
    u = C.alpha(warped) | C.alpha(b_true)
    wrong = (C.alpha(warped) != C.alpha(b_true)) | (C.alpha(warped) & C.alpha(b_true) & (C.coldiff(warped, b_true) > 60))
    return float((wrong & u).sum() / max(1, u.sum()))


def select(errs, frames, tau):
    """errs[(k,f)] → 같은 클립 프레임들에서 τ 로 덮는 최소 키(탐욕)"""
    uncovered = set(frames)
    keys = []
    while uncovered:
        best = max(frames, key=lambda k: (len({f for f in uncovered if f == k or errs.get((k, f), 1) <= tau}), -frames.index(k)))
        keys.append(best)
        uncovered -= {f for f in uncovered if f == best or errs.get((best, f), 1) <= tau}
    return sorted(keys, key=frames.index)


def main():
    pkg, out = sys.argv[1:3]
    taus = [float(t) for t in (sys.argv[3].split(",") if len(sys.argv) > 3 else ["0.15", "0.25", "0.35"])]
    cdir = os.path.join(pkg, "frames_coat_original")
    names = sorted(n[:-4] for n in os.listdir(cdir) if n.endswith(".png"))
    coat = {n: C.load(os.path.join(cdir, n + ".png")) for n in names}
    ldir = os.path.join(pkg, "frames_leaf_set_transfer")
    leaf = {n: C.load(os.path.join(ldir, n + ".png")) for n in names if os.path.exists(os.path.join(ldir, n + ".png"))}
    clips = frames_by_clip(names)
    errs = {}
    for c, fr in clips.items():
        for k in fr:
            for f in fr:
                if k != f:
                    errs[(k, f)] = proxy_err(coat[k], coat[f], S5.warp_set(coat[k], coat[k], coat[f]))
    res = {"proxy": {"%s->%s" % kf: round(v, 3) for kf, v in errs.items()}, "by_tau": {}}
    for tau in taus:
        keys, val = {}, dict(sil=[], wrong=[])
        for c, fr in clips.items():
            ks = select(errs, fr, tau)
            keys[c] = ks
            for f in fr:
                if f in ks or f not in leaf:
                    continue
                k = min(ks, key=lambda k: errs[(k, f)])
                w = S5.warp_set(coat[k], leaf[k], coat[f])
                a, b = C.alpha(w), C.alpha(leaf[f])
                val["sil"].append(float((a & b).sum() / max(1, (a | b).sum())))
                both = a & b
                val["wrong"].append(float((both & (C.coldiff(w, leaf[f]) > 60)).sum() / max(1, both.sum())))
        nk = sum(len(v) for v in keys.values())
        res["by_tau"][str(tau)] = dict(keys=keys, n_keys=nk, n_frames=len(names),
                                       leaf_sil_iou=round(float(np.mean(val["sil"])), 3) if val["sil"] else None,
                                       leaf_worst_sil=round(float(np.min(val["sil"])), 3) if val["sil"] else None,
                                       leaf_color_wrong=round(float(np.mean(val["wrong"])), 3) if val["wrong"] else None)
        print("tau", tau, "keys", nk, "/", len(names), {c: len(v) for c, v in keys.items()},
              "leaf sil", res["by_tau"][str(tau)]["leaf_sil_iou"], "worst", res["by_tau"][str(tau)]["leaf_worst_sil"])
    json.dump(res, open(out, "w", encoding="utf-8"), indent=1, ensure_ascii=False)


if __name__ == "__main__":
    main()
