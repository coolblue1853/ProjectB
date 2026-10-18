"""1층계 ②구역 계보 세트(TowerWorld 12.1) 아이콘·착용 텍스처: Leaf 세트를 색 변환.
Pack Hunter(WolfArmor) = 늑대 회색 / Night Lantern(WispArmor) = 도깨비불 파랑.
③구역: Ancient Grove(GroveArmor) = 갈색·이끼 / Will-o'-Fae(FaeArmor) = 보라·분홍.
④구역: Bog Stalker(BogArmor) = 올리브·갈색 / Gloom Swarm(GloomArmor) = 핏빛 보라.  사용: python Tools/make_forest_sets.py"""
import colorsys
import os
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources")
PARTS = ["Cap", "Armor", "LegArmor", "HandArmor", "Shoes"]
# 세트 키 -> (색조 회전, 채도 배율, 밝기 배율)
SETS = {"Wolf": (0.0, 0.15, 0.95), "Wisp": (0.28, 1.15, 1.1),
        # ③구역 깊은 숲 (TowerWorld 12.2): Ancient Grove = 갈색·이끼(초록→황갈), Will-o'-Fae = 보라·분홍
        "Grove": (-0.2, 0.75, 0.78), "Fae": (0.5, 0.85, 1.12),
        # ④구역 늪지 (TowerWorld 14절): Bog Stalker = 탁한 올리브·갈색, Gloom Swarm = 어두운 핏빛 보라
        "Bog": (-0.1, 0.6, 0.68), "Gloom": (0.62, 0.7, 0.62)}


def shift(src, dst, h, s, v):
    im = Image.open(src).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            hh, ss, vv = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            hh = (hh + h) % 1.0
            ss = min(1.0, ss * s)
            vv = min(1.0, vv * v)
            rr, gg, bb = colorsys.hsv_to_rgb(hh, ss, vv)
            px[x, y] = (int(rr * 255), int(gg * 255), int(bb * 255), a)
    im.save(dst)



import sys
only = set(sys.argv[1:])  # 인자로 세트 키를 주면 그것만 다시 만든다
for key, (h, s, v) in SETS.items():
    if only and key not in only:
        continue
    for part in PARTS:
        shift(os.path.join(ROOT, "Item", "Leaf" + part + ".png"), os.path.join(ROOT, "Item", key + part + ".png"), h, s, v)
    shift(os.path.join(ROOT, "Sprite", "LeafArmor.png"), os.path.join(ROOT, "Sprite", key + "Armor.png"), h, s, v)
    print("ok", key)
