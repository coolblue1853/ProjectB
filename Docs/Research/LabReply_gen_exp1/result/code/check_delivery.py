"""전달 전 점검(생성 0). result/ 폴더에서: python code/check_delivery.py <패키지> [--full]

  1. regions/ 21장 형식(L, 64×64, 값 0~8, 불투명 픽셀 = 라벨>0) + M1h 재생성 결과와 픽셀 단위 일치
  2. metrics.json 의 recommended·baseline 수치를 다시 계산해 일치(--full 일 때, 약 2분)
  3. export_runtime 출력(PNG·_erase·equip.json) 구조 검사 + 런타임 규칙(속옷 몸 → 지울 마스크 → 바지·신발·상의·장갑·모자)으로
     다시 합성한 결과가 평가기(common.compose) 합성과 픽셀 단위로 같은지(16조합 × 21프레임)
  4. 문서(*.md)에 백틱으로 적힌 result 내부 경로가 실제로 있는지
"""
import glob
import json
import os
import re
import sys
import tempfile

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

CLIP = {"ds_idle": "Idle", "ds_stab": "Stab", "ds_down": "SwingDown", "ds_up": "SwingUp"}
ok_all = True


def report(name, ok, detail=""):
    global ok_all
    ok_all &= bool(ok)
    print(("PASS " if ok else "FAIL ") + name + (("  " + detail) if detail else ""))


def check_regions(pkg):
    import m1_underdiff as M1
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    bad, diff = [], 0
    for f in C.FRAMES:
        im = Image.open(os.path.join("regions", f + "_region.png"))
        a = np.array(im)
        if im.mode != "L" or a.shape != (64, 64) or a.max() > 8 or not ((a > 0) == C.alpha(coat[f])).all():
            bad.append(f)
        diff += int((M1.segment(coat[f], under[f], leaf[f], hybrid=True) != a).sum())
    report("regions 형식 21장", not bad, str(bad) if bad else "")
    report("regions = M1h 재생성", diff == 0, "다른 픽셀 %d" % diff)


def check_metrics(pkg):
    import study4_slots as S4
    import m3_amodal as M3
    M3.set_pkg(pkg)
    m = json.load(open("metrics.json", encoding="utf-8"))
    maps, gt = C.load_maps("regions"), C.load_maps("labels")
    r = S4.run(pkg, maps, C.load_dir("under_fixed"), True, True, gt)
    keys = ("holes", "exposure", "strays", "intrusion", "cut", "head_mismatch_all_leaf")
    same = all(r[k] == m["recommended"][k] for k in keys)
    report("metrics.json recommended 재계산 일치", same, str({k: (r[k], m["recommended"][k]) for k in keys}))
    base = C.load_maps(os.path.join(pkg, "regions_current"))
    rb = S4.run(pkg, base, C.load_dir(os.path.join(pkg, "frames_underwear_base")), True, False, gt)
    same = all(rb[k] == m["baseline"][k] for k in keys)
    report("metrics.json baseline 재계산 일치", same, str({k: (rb[k], m["baseline"][k]) for k in keys}))


def runtime_compose(root, wear, f, under):
    """CelComposer.ComposeFrame 그대로: 슬롯 순서(Legs10 Shoes15 Body20 Gloves30 Head40)로, 각 레이어 차례에
    erase(r>127)로 지금까지의 그림을 투명하게 지운 뒤 레이어(a>0)를 덮어쓴다."""
    clip, k = CLIP[f.rsplit("_", 1)[0]], f.rsplit("_", 1)[1]
    out = under.copy()
    part = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}
    for s in C.DRAW_ORDER:
        d = os.path.join(root, "%s_%s" % (wear[s], part[s]))
        e = np.array(Image.open(os.path.join(d, "%s_%s_erase.png" % (clip, k))).convert("RGBA"))[:, :, 0] > 127
        out[e] = 0
        L = np.array(Image.open(os.path.join(d, "%s_%s.png" % (clip, k))).convert("RGBA"))
        m = L[:, :, 3] > 0
        out[m] = L[m]
    return out


def check_export(pkg):
    import export_runtime as E
    coat_dir = os.path.join(pkg, "frames_coat_original")
    leaf_dir = os.path.join(pkg, "frames_leaf_set_transfer")
    tmp = tempfile.mkdtemp(prefix="exportcheck_")
    E.write(tmp, "base_coat", E.from_map(coat_dir, "regions", "base_coat", C.TOP, C.TOP, True, None, "under_fixed"))
    E.write(tmp, "leaf_set", E.from_map(leaf_dir, "regions", "leaf_set", C.TOP, 0, True, None, "under_fixed"))
    # 구조
    probs = []
    for d in sorted(glob.glob(os.path.join(tmp, "*"))):
        j = json.load(open(os.path.join(d, "equip.json")))
        if j["id"] != os.path.basename(d) or not j["ds"] or len(j["frames"]) != 21:
            probs.append(os.path.basename(d))
        for fr in j["frames"]:
            p = os.path.join(d, "%s_%d.png" % (fr["clip"], fr["k"]))
            e = os.path.join(d, "%s_%d_erase.png" % (fr["clip"], fr["k"]))
            if not (os.path.exists(p) and os.path.exists(e)):
                probs.append(p)
            elif Image.open(e).mode != "L" or Image.open(p).size != (64, 64):
                probs.append(e)
    report("export 구조(폴더·이름·equip.json·erase)", not probs, str(probs[:5]))
    legs_empty = all(not C.alpha(C.load(p)).any() for p in glob.glob(os.path.join(tmp, "*_legs", "*.png")) if not p.endswith("_erase.png"))
    report("4칸: 바지 레이어가 전부 비어 있음", legs_empty)
    # 런타임 재합성 == 평가기 합성
    under = C.load_dir("under_fixed")
    coat, leaf = C.load_dir(coat_dir), C.load_dir(leaf_dir)
    maps = C.load_maps("regions")
    diff = 0
    for f in C.FRAMES:
        imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
        lay = {s: C.make_layers(maps[f], imgs[s], s, C.TOP, under[f], armor=True) for s in imgs}
        for w in C.patterns(["base_coat", "leaf_set"]):
            if w[C.LEGS] != w[C.TOP]:
                continue
            a, _ = C.compose(under[f], w, imgs, lay)
            b = runtime_compose(tmp, w, f, under[f])
            diff += int(((C.coldiff(a, b) > 0) | (C.alpha(a) != C.alpha(b))).sum())
    report("export 런타임 재합성 = 평가기 합성(16조합×21)", diff == 0, "다른 픽셀 %d" % diff)


def check_docs():
    missing = []
    for md in glob.glob("*.md") + glob.glob("*/*.md"):
        if md.startswith("archive"):
            continue
        for ref in re.findall(r"`([^`\s]+)`", open(md, encoding="utf-8").read()):
            ref = ref.strip()
            if ref.startswith("result/"):
                ref = ref[len("result/"):]
            if not ref or ref.startswith(("gen_exp", "_")) or ref in ("regions_current/",):
                continue
            if "<" in ref or "*" in ref or ref.startswith("-") or "=" in ref or ref.startswith("ds_"):
                continue
            if not re.search(r"\.(md|json|png|py)$|/$", ref):
                continue
            cands = [ref, os.path.join(os.path.dirname(md), ref), os.path.join("code", ref), os.path.join("archive", "code", ref)]
            base = os.path.basename(ref.rstrip("/"))
            in_archive = bool(glob.glob(os.path.join("archive", "**", base), recursive=True))
            if not any(os.path.exists(c) for c in cands) and not in_archive and not ref.startswith(("frames_", "Assets", "Tools", "Docs")):
                missing.append("%s: %s" % (md, ref))
    known_external = ("ref_b.png", "equip.json", "RESULT.md", "keys.json", "keys98.json", "under_base.py", "A_ref.png",
                      "out.json", "out.png", "region_map.py", "equip_ds.py", "cel.json", "gt_ops.json", "frames_underwear_base/")
    missing = [m for m in missing if not m.split(": ")[1].endswith(known_external) and not os.path.basename(m.split(": ")[1]) in known_external]
    report("문서에 적힌 경로가 존재", not missing, "; ".join(missing[:12]))


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    pkg = sys.argv[1]
    check_regions(pkg)
    check_export(pkg)
    check_docs()
    if "--full" in sys.argv:
        check_metrics(pkg)
    print("ALL PASS" if ok_all else "SOME FAILED")


if __name__ == "__main__":
    main()
