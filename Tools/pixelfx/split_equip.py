"""세트 장비 레이어를 부위별로 나눈다(생성 0회, CelEquipmentPlan 던파식): 프레임마다 맨몸 셀의 스카프 윗줄을 경계로
위 = 투구(레이어 위쪽 + 지울 마스크 전부 — 삐져나온 머리카락은 투구가 가린다), 아래 = 갑옷(레이어 아래쪽, 마스크 없음).
투구·흉갑을 한 번에 입혀 전이한 세트(예 iron_set)에서 iron_helm(Head) · iron_plate(Body) 를 따로 만들어 자유롭게 조합.
사용: python Tools/pixelfx/split_equip.py <세트 id> <투구 id> <갑옷 id>
"""
import json
import os
import shutil
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from build_cel import SCARF, near  # noqa: E402

CEL = "Assets/Art/Player/PixelRig/Cel"
EQ = "Assets/Art/Player/PixelRig/Equip"


def main():
    src, helm, plate = sys.argv[1], sys.argv[2], sys.argv[3]
    meta = json.load(open(os.path.join(EQ, src, "equip.json")))
    for d in (helm, plate):
        os.makedirs(os.path.join(EQ, d), exist_ok=True)
    out = {helm: [], plate: []}
    for f in meta["frames"]:
        name = f"{f['clip']}_{f['k']}"
        base = np.array(Image.open(os.path.join(CEL, name + ".png")).convert("RGBA"))
        rows = np.nonzero(near(base, SCARF).any(axis=1))[0]
        cut = int(rows.min()) if len(rows) else 40  # 스카프 윗줄 — 여기부터 아래는 몸
        layer = np.array(Image.open(os.path.join(EQ, src, name + ".png")).convert("RGBA"))
        erase = np.array(Image.open(os.path.join(EQ, src, name + "_erase.png")))
        top, bottom = layer.copy(), layer.copy()
        top[cut:] = 0
        bottom[:cut] = 0
        Image.fromarray(top, "RGBA").save(os.path.join(EQ, helm, name + ".png"))
        Image.fromarray(erase, "L").save(os.path.join(EQ, helm, name + "_erase.png"))
        Image.fromarray(bottom, "RGBA").save(os.path.join(EQ, plate, name + ".png"))
        Image.fromarray(np.zeros_like(erase), "L").save(os.path.join(EQ, plate, name + "_erase.png"))
        for d in (helm, plate):
            out[d].append({"clip": f["clip"], "k": f["k"]})
    json.dump({"id": helm, "slot": "head", "frames": out[helm]}, open(os.path.join(EQ, helm, "equip.json"), "w"), indent=1)
    json.dump({"id": plate, "slot": "body", "frames": out[plate]}, open(os.path.join(EQ, plate, "equip.json"), "w"), indent=1)
    print(helm, plate, len(meta["frames"]))


if __name__ == "__main__":
    main()
