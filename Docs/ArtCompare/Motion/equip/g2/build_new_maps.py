"""신규 23프레임: 원본 머리를 속옷 몸에 덮어쓰고(under_head/), 영역 지도(regions23/)를 만든다. 기존 21프레임 지도는 그대로 쓴다."""
import os, sys
import numpy as np
from PIL import Image
ROOT = "C:/Users/user/ProjectB"
sys.path.insert(0, ROOT + "/Tools/pixelfx"); sys.path.insert(0, ROOT + "/Docs/Research/LabReply_gen_exp1/result/code")
import region_map as R, under_base as UB, common as C, m1_underdiff as M1
G2 = ROOT + "/Docs/ArtCompare/Motion/equip/g2"
CEL = ROOT + "/Assets/Art/Player/PixelRig/CelDS"
NEW = [f"ds_walk_{i}" for i in range(8)] + [f"ds_run_{i}" for i in range(8)] + ["ds_crouch_0"] + [f"ds_arc_{i}" for i in range(3)] + [f"ds_land_{i}" for i in range(3)]
for f in NEW:
    o = C.load(f"{CEL}/{f}.png"); u = C.load(f"{G2}/under_new/{f}.png")
    lab = R.classify(o, {})
    hm = UB.head_mask(o, lab, u)
    out = u.copy(); out[hm] = o[hm]
    Image.fromarray(out).save(f"{G2}/under_head/{f}.png")
    seg = M1.segment(o, out, None, True)
    C.save_map(seg, f"{G2}/regions23/{f}_region.png")
    print(f, "head px", int(hm.sum()), "map labels", sorted(set(np.unique(seg).tolist())))
