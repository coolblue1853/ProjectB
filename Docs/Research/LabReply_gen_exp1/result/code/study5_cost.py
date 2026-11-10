"""비용 절감 연구(생성 0, 11-10): 던파·메이플식 부위 교체를 PixelLab 으로 싸게 여러 동작에 적용하려면 몇 장을 생성해야 하나.

실험(정답 = 잎사귀 세트 21프레임 실물, frames_leaf_set_transfer):
  anchor : 모자·신발이 '한 장 + 프레임별 위치 이동(메이플식 앵커)'으로 되는가. 키 프레임 부위 그림을 ±8px 이동(좌우 반전 포함)해
           각 프레임의 실제 부위와 비교. 틀린 픽셀 / 부위 픽셀.
  warp   : 키 프레임만 생성하고 나머지 프레임은 '원본 캐릭터(코트)끼리의 대응'으로 세트 그림을 옮기기.
           클립마다 키 1장(첫 프레임) / 2장(첫·가운데) / 전부. 옮긴 그림 vs 실제 잎사귀 그림: 실루엣 IoU, 색 틀린 픽셀 비율.
사용: python study5_cost.py <패키지> <지도 폴더> <속옷 몸 폴더(under_fixed)> <출력 json> [시트 png]
"""
import json
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import m2_propagate as M2

CLIPS = {"ds_idle": 8, "ds_stab": 4, "ds_down": 4, "ds_up": 5}


def part_mask(lab, img, under, slot):
    lay = C.make_layers(lab, img, "leaf_set", 2, under, armor=True)
    return lay[slot]


def best_shift_match(tmpl_img, tmpl_m, img, m, r=8):
    """템플릿(부위 그림)을 이동·반전해 대상 부위와 비교: 틀린 픽셀(색 차>30 또는 덮음 불일치) 최소값"""
    best = (10 ** 9, None)
    for flip in (False, True):
        ti = tmpl_img[:, ::-1] if flip else tmpl_img
        tm = tmpl_m[:, ::-1] if flip else tmpl_m
        for dy in range(-r, r + 1):
            for dx in range(-r - 20, r + 21) if flip else range(-r, r + 1):
                si, sm = C.shift(ti, dy, dx), C.shift(tm, dy, dx, False)
                wrong = (sm != m) | (sm & m & (C.coldiff(si, img) > 30))
                k = int(wrong.sum())
                if k < best[0]:
                    best = (k, (dy, dx, flip))
    return best


def anchor_study(pkg, maps, unders):
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    res = {}
    for slot, name in ((C.CAP, "cap"), (C.SHOES, "shoes")):
        per = {}
        for clip, n in CLIPS.items():
            k0 = clip + "_0"
            tm = part_mask(maps[k0], leaf[k0], unders[k0], slot)
            for i in range(n):
                f = "%s_%d" % (clip, i)
                m = part_mask(maps[f], leaf[f], unders[f], slot)
                wrong, mv = best_shift_match(leaf[k0], tm, leaf[f], m)
                per[f] = dict(part_px=int(m.sum()), wrong_px=wrong, wrong_ratio=round(wrong / max(1, m.sum()), 3), move=mv and list(map(lambda v: v if not isinstance(v, np.bool_) else bool(v), mv)))
        ratios = [v["wrong_ratio"] for f, v in per.items() if not f.endswith("_0")]
        res[name] = dict(mean_wrong_ratio=round(float(np.mean(ratios)), 3), max_wrong_ratio=round(float(np.max(ratios)), 3), per_frame=per)
    return res


def warp_set(coat_src, set_src, coat_dst):
    """원본(코트) 두 장의 패치 정합으로 세트 그림을 옮김(study2.warp_under 와 같은 방식)"""
    S, T = M2.feat(coat_src), M2.feat(coat_dst)
    d0 = np.round(M2.anchor(coat_dst) - M2.anchor(coat_src)).astype(int)
    best = np.full((64, 64), 1e18)
    out = np.zeros((64, 64, 4), np.uint8)
    for dy in range(d0[0] - 12, d0[0] + 13):
        for dx in range(d0[1] - 12, d0[1] + 13):
            Ss = C.shift(S, dy, dx)
            diff = np.abs(T - Ss).sum(2)
            cost = np.zeros((64, 64))
            for ky in range(-2, 3):
                for kx in range(-2, 3):
                    cost += C.shift(diff, ky, kx)
            cost += 6.0 * 25 * (abs(dy - d0[0]) + abs(dx - d0[1]))
            upd = cost < best
            best[upd] = cost[upd]
            out[upd] = C.shift(set_src, dy, dx)[upd]
    out[~C.alpha(coat_dst)] = 0
    return out


def warp_study(pkg):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    res, made = {}, {}
    for keys_name, keyfn in (("1 key/clip", lambda n: [0]), ("2 keys/clip", lambda n: sorted({0, n // 2})),
                             ("every other", lambda n: list(range(0, n, 2)))):
        sil, wrong, gen = [], [], 0
        made[keys_name] = {}
        for clip, n in CLIPS.items():
            keys = keyfn(n)
            gen += len(keys)
            for i in range(n):
                f = "%s_%d" % (clip, i)
                if i in keys:
                    made[keys_name][f] = leaf[f]
                    continue
                k = min(keys, key=lambda j: abs(j - i))
                kf = "%s_%d" % (clip, k)
                w = warp_set(coat[kf], leaf[kf], coat[f])
                made[keys_name][f] = w
                a, b = C.alpha(w), C.alpha(leaf[f])
                sil.append(float((a & b).sum() / max(1, (a | b).sum())))
                both = a & b
                wrong.append(float((both & (C.coldiff(w, leaf[f]) > 60)).sum() / max(1, both.sum())))
        res[keys_name] = dict(generated_frames=gen, of=21, warped_frames=len(sil), sil_iou=round(float(np.mean(sil)), 3),
                              color_wrong_ratio=round(float(np.mean(wrong)), 3),
                              worst_sil=round(float(np.min(sil)), 3))
    return res, made


def sheet(pkg, made, path):
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    fr = ["ds_idle_3", "ds_idle_6", "ds_stab_2", "ds_down_2", "ds_up_3"]
    rows = [("real transfer", leaf)] + list(made.items())
    sh = Image.new("RGB", (110 + 130 * len(fr), 130 * len(rows) + 16), (20, 20, 20))
    d = ImageDraw.Draw(sh)
    for c, f in enumerate(fr):
        d.text((110 + c * 130, 2), f, fill=(255, 255, 255))
    for r, (n, imgs) in enumerate(rows):
        d.text((2, 16 + r * 130 + 60), n, fill=(255, 255, 0))
        for c, f in enumerate(fr):
            b = Image.new("RGBA", (64, 64), (70, 70, 70, 255)); b.alpha_composite(Image.fromarray(imgs[f]))
            sh.paste(b.convert("RGB").resize((128, 128), Image.NEAREST), (110 + c * 130, 16 + r * 130))
    sh.save(path)


def main():
    pkg, mapdir, udir, out = sys.argv[1:5]
    maps = C.load_maps(mapdir)
    unders = C.load_dir(udir)
    R = {"anchor": anchor_study(pkg, maps, unders)}
    print("anchor", {k: (v["mean_wrong_ratio"], v["max_wrong_ratio"]) for k, v in R["anchor"].items()})
    R["warp"], made = warp_study(pkg)
    print("warp", R["warp"])
    json.dump(R, open(out, "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    if len(sys.argv) > 5:
        sheet(pkg, made, sys.argv[5])


if __name__ == "__main__":
    main()
