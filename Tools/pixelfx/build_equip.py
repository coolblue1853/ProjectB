"""장비 레이어 굽기(CelEquipmentPlan 「확정」 던파식): transfer_outfit 결과 묶음 → 클립별 덮어쓰기 레이어 + 지울 마스크.

입력: 장비 폴더(Docs/ArtCompare/Motion/equip/<장비 id>/) 아래 묶음 폴더들 — 각 묶음 폴더의 0.png, 1.png … 가 BATCHES 의 (클립, 프레임) 순서.
출력: Assets/Art/Player/PixelRig/Equip/<장비 id>/<클립>_<k>.png(레이어) + <클립>_<k>_erase.png(맨몸에서 지울 마스크) + equip.json(슬롯·프레임 목록).
레이어 = 장비 프레임에서 맨몸 셀과 색이 다른 픽셀(8px 미만 덩어리는 전이 잡티로 버림), 마스크 = 맨몸엔 있고 장비 프레임엔 없는 픽셀(삐져나온 머리카락 등).
사용: python Tools/pixelfx/build_equip.py <장비 id> <슬롯>   예) python Tools/pixelfx/build_equip.py iron_set body
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from extract_layer import MIN_PART, clean  # noqa: E402

CEL = "Assets/Art/Player/PixelRig/Cel"
SRC = "Docs/ArtCompare/Motion/equip"
OUT = "Assets/Art/Player/PixelRig/Equip"
TOL = 24
# 전이 묶음(최대 15프레임 — 같은 값): 폴더 이름 → (클립, 프레임) 순서. 셀 클립을 바꾸면 여기도 맞춘다
BATCHES = {
    "walk": [("Walk", k) for k in range(8)],
    "batch_a": [("Run", k) for k in range(7)] + [("JumpCrouch", k) for k in range(3)] + [("JumpArc", k) for k in range(4)],
    "batch_b": [("Land", k) for k in range(3)] + [("Idle", k) for k in range(4)] + [("Dash", k) for k in range(3)] + [("Hurt", k) for k in range(3)],
    "batch_c": [("Death", k) for k in range(8)],
}


def main():
    eid, slot = sys.argv[1], sys.argv[2]
    out = os.path.join(OUT, eid)
    os.makedirs(out, exist_ok=True)
    frames = []
    for folder, items in BATCHES.items():
        src = os.path.join(SRC, eid, folder)
        if not os.path.isdir(src):
            print("없음:", src)
            continue
        for i, (clip, k) in enumerate(items):
            b = np.array(Image.open(os.path.join(CEL, f"{clip}_{k}.png")).convert("RGBA")).astype(int)
            e = np.array(Image.open(os.path.join(src, f"{i}.png")).convert("RGBA")).astype(int)
            bo, eo = b[..., 3] > 0, e[..., 3] > 0
            differ = np.abs(b[..., :3] - e[..., :3]).sum(axis=2) > TOL
            lm = clean(eo & (~bo | differ), MIN_PART)
            layer = np.zeros_like(e)
            layer[lm] = e[lm]
            layer[..., 3] = np.where(lm, 255, 0)
            erase = clean(bo & ~eo, 2)
            name = f"{clip}_{k}"
            Image.fromarray(layer.astype(np.uint8), "RGBA").save(os.path.join(out, name + ".png"))
            Image.fromarray(np.where(erase, 255, 0).astype(np.uint8), "L").save(os.path.join(out, name + "_erase.png"))
            frames.append({"clip": clip, "k": k, "layer": int(lm.sum()), "erase": int(erase.sum())})
    with open(os.path.join(out, "equip.json"), "w") as fp:
        json.dump({"id": eid, "slot": slot, "frames": frames}, fp, indent=1)
    print(eid, slot, len(frames), "프레임")


if __name__ == "__main__":
    main()
