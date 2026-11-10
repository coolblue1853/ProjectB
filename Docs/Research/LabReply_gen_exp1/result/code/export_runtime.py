"""레이어 → 런타임 형식(equip_ds.export 와 같은 폴더·이름·equip.json). 소속 정책을 인자로 받는다.

출력 형식(equip_ds.export 와 동일):
  <루트>/<세트>_<cap|top|glove|legs|shoes>/<Idle|Stab|SwingDown|SwingUp>_<k>.png      부위 레이어(RGBA)
  <루트>/<세트>_<부위>/<클립>_<k>_erase.png                                            지울 마스크(L, 속옷 몸 위에 쌓으므로 전부 0)
  <루트>/<세트>_<부위>/equip.json   {"id", "slot": head|body|gloves|legs|shoes, "ds": true, "frames": [{"clip","k"}]}

두 가지 입력:
  A) 이미 만든 레이어 폴더(layers/<부위>/<프레임>.png 형식. 예: layered_extract 출력)
     python export_runtime.py layers <레이어 폴더> <세트 id> <루트>
  B) 완성 그림을 공용 지도로 자르기(= equip_ds.export 대체, 정책을 명시)
     python export_runtime.py map <세트 그림 폴더(ds_*.png)> <공용 지도 폴더> <세트 id> <루트> [--hem top|legs] [--back top|drop]
     기본 --hem top(모든 세트 자락 → 상의: 1차 연구에서 구멍 5,280 → 0), --back 은 세트 id 에 'coat' 가 있으면 top, 아니면 drop(기존 정책과 같음).
     --armor                 4칸 설계(SLOT_DECISION.md): 상의·바지·자락을 갑옷(body) 하나로. 바지 레이어는 빈 것.
     --cap-under <속옷 몸 폴더>  세트별 모자 확장(두건이 원본 모자 영역보다 클 때 잘림 방지) + 모자 _erase.png(가려질 원본 머리카락).
                             런타임 CelComposer 가 erase 를 이미 지원(레이어 전에 지움). 기본 코트(base_coat)는 확장·지우기 없음.
     --split-back <패키지>   배낭을 '<세트>_backpack'(슬롯 back) 별도 레이어로, 갑옷의 배낭 밑은 생성 0 보완(배낭 끄기용).
     equip_ds.export 와 달리 세트마다 자락 정책이 갈리지 않는다(갈리면 섞어 입을 때 구멍).
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

CLIP = {"ds_idle": "Idle", "ds_stab": "Stab", "ds_down": "SwingDown", "ds_up": "SwingUp"}
PART = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes", 7: "backpack"}
SLOT_NAME = {1: "head", 2: "body", 3: "gloves", 4: "legs", 5: "shoes", 7: "back"}  # CelEquipLayer.Slot 이름. "back"(배낭)은 C# enum 에 추가 필요(갑옷 다음에 그림)


def write(root, set_id, layers_by_frame):
    """layers_by_frame: {프레임: {슬롯: RGBA}}"""
    present = set(k for lay in layers_by_frame.values() for k in lay if not isinstance(k, tuple))
    for s, part in PART.items():
        if s == 7 and 7 not in present:
            continue
        lid = "%s_%s" % (set_id, part)
        d = os.path.join(root, lid)
        os.makedirs(d, exist_ok=True)
        frames = []
        for f in C.FRAMES:
            if f not in layers_by_frame:
                continue
            L = layers_by_frame[f].get(s)
            if L is None:
                L = np.zeros((64, 64, 4), np.uint8)
            clip, k = CLIP[f.rsplit("_", 1)[0]], int(f.rsplit("_", 1)[1])
            Image.fromarray(L).save(os.path.join(d, "%s_%d.png" % (clip, k)))
            er = layers_by_frame[f].get(("erase", s))
            er = (er.astype(np.uint8) * 255) if er is not None else np.zeros((64, 64), np.uint8)
            Image.fromarray(er, "L").save(os.path.join(d, "%s_%d_erase.png" % (clip, k)))
            frames.append({"clip": clip, "k": k})
        json.dump({"id": lid, "slot": SLOT_NAME[s], "ds": True, "frames": frames},
                  open(os.path.join(d, "equip.json"), "w"), indent=1)
        print(lid, len(frames))


def from_layers(ldir):
    out = {}
    for f in C.FRAMES:
        lay = {}
        for s, part in PART.items():
            p = os.path.join(ldir, part, f + ".png")
            if os.path.exists(p):
                lay[s] = C.load(p)
        if lay:
            out[f] = lay
    return out


def from_map(sdir, mapdir, set_id, hem_to, back_to, armor=False, split_back=None, under_dir=None):
    """armor: 4칸(상의·바지·자락 → 갑옷 = body 슬롯, 바지 레이어는 빈 것).
    split_back=<패키지>: 배낭(7)을 별도 레이어 '<세트>_backpack'(슬롯 back)로 떼고, 갑옷의 배낭 밑 몸 안쪽은 생성 0 보완으로 채움."""
    maps = C.load_maps(mapdir)
    if split_back:
        import m3_amodal as M3
        M3.set_pkg(split_back)
    out = {}
    for f in C.FRAMES:
        p = os.path.join(sdir, f + ".png")
        if not os.path.exists(p) or f not in maps:
            continue
        img = C.load(p)
        lab = maps[f].copy()
        if armor:
            lab[np.isin(lab, [C.LEGS, C.HEM])] = C.TOP
        em = C.ext_map(lab, img)
        em[em == C.HEM] = hem_to
        back_mask = C.alpha(img) & (em == C.BACK)
        em[em == C.BACK] = C.BACK if split_back else back_to
        a = C.alpha(img)
        lay = {}
        for s in C.SLOTS:
            L = np.zeros_like(img)
            m = a & (em == s)
            L[m] = img[m]
            lay[s] = L
        if under_dir:  # 세트별 모자 확장 + 모자 지울 마스크(속옷 몸의 원본 머리카락 중 이 모자가 가리는 곳)
            u = C.load(os.path.join(under_dir, f + ".png"))
            add, erase = C.cap_extend(lab, img, u, set_id)
            lay[C.CAP][add] = img[add]
            lay[("erase", C.CAP)] = erase
        if split_back:
            B = np.zeros_like(img)
            B[back_mask] = img[back_mask]
            lay[7] = B
            u, g = M3.geometry(f)
            todo = back_mask & g["ua"] & ~C.alpha(lay[C.TOP])
            if todo.any() and C.alpha(lay[C.TOP]).any():
                fill = M3.patch_copy(img, C.alpha(lay[C.TOP]), todo, prefer=(0, 0))
                lay[C.TOP][todo] = fill[todo]
        out[f] = lay
    return out


def main():
    mode = sys.argv[1]
    if mode == "layers":
        ldir, set_id, root = sys.argv[2:5]
        write(root, set_id, from_layers(ldir))
    elif mode == "map":
        sdir, mapdir, set_id, root = sys.argv[2:6]
        hem_to = C.LEGS if "--hem" in sys.argv and sys.argv[sys.argv.index("--hem") + 1] == "legs" else C.TOP
        if "--back" in sys.argv:
            back_to = C.TOP if sys.argv[sys.argv.index("--back") + 1] == "top" else 0
        else:
            back_to = C.TOP if "coat" in set_id else 0
        armor = "--armor" in sys.argv
        sb = sys.argv[sys.argv.index("--split-back") + 1] if "--split-back" in sys.argv else None
        ud = sys.argv[sys.argv.index("--cap-under") + 1] if "--cap-under" in sys.argv else None
        write(root, set_id, from_map(sdir, mapdir, set_id, hem_to, back_to, armor, sb, ud))
    else:
        raise SystemExit(__doc__)


if __name__ == "__main__":
    main()
