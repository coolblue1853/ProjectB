"""Lv2 아이템 임시 아이콘/텍스처 생성: 기존 아이콘을 색조 회전해서 만든다 (정식 아트 전 임시, Q-107 Q-L3).
사용: python Tools/make_lv2_icons.py
"""
import colorsys
import os
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources")
ITEM = os.path.join(ROOT, "Item")

# 새 아이템 이름 -> (원본 아이콘 이름, 색조 회전(0~1), 채도 배율, 밝기 배율)
ICONS = {
    "T1_Ore": ("T0_Ore", 0.55, 1.2, 1.1),
    "Cliff Fox Pelt": ("Rabbit Hide", 0.05, 1.4, 1.0),
    "Fox Meat": ("Rabbit Meat", 0.03, 1.1, 1.0),
    "Glowing Spore": ("GreenHurb", 0.45, 1.3, 1.25),
    "Spongy Mushroom Cap": ("Small Tuber", 0.9, 1.3, 1.0),
    "Goblin Fang": ("Small Bone", 0.25, 0.6, 1.0),
    "Tarnished Goblin Coin": ("Rat Thief's Stash", 0.12, 1.2, 1.0),
    "Membrane Wing": ("Fresh Green Leaves", 0.75, 1.0, 0.9),
    "Bloodshot Lens": ("Sharp Petal", 0.95, 1.3, 1.0),
    "Ancient Bone": ("Round Bone", 0.08, 0.5, 0.9),
    "Cracked Shield Shard": ("T0_Ore", 0.62, 0.4, 0.8),
    "Death Sigil": ("Ominous Black Liquid", 0.8, 1.4, 1.1),
    "Abyssal Core": ("Ominous Black Liquid", 0.6, 1.6, 1.3),
    "Roasted Fox Skewer": ("Rabbit Meat Stew", 0.05, 1.1, 1.0),
    "CliffCap": ("LeafCap", 0.45, 1.0, 1.0),
    "CliffArmor": ("LeafArmor", 0.45, 1.0, 1.0),
    "CliffLegArmor": ("LeafLegArmor", 0.45, 1.0, 1.0),
    "CliffHandArmor": ("LeafHandArmor", 0.45, 1.0, 1.0),
    "CliffShoes": ("LeafShoes", 0.45, 1.0, 1.0),
    "CliffStaff": ("ScareStaff", 0.5, 1.0, 1.2),
    "CliffBlade": ("ScareSide", 0.5, 1.0, 1.2),
    "AbyssStaff": ("ScareStaff", 0.72, 1.5, 1.1),
    "Eye of the Cliffs": ("Green Wreath", 0.5, 1.2, 1.1),
    "Bringer's Sigil Ring": ("Nasty Rat Ring", 0.78, 1.4, 1.0),
}

# 기존에 아이콘이 없던 아이템(검증 리포트) 임시 아이콘
ICONS.update({
    "Rock": ("T0_Ore", 0.0, 0.3, 0.9),
    "PickAxe": ("RustedSword", 0.08, 0.8, 1.0),
    "ToxicPickAxe": ("PoisonSword", 0.0, 1.0, 1.0),
    "Potion": ("Herbal Salad", 0.9, 1.3, 1.1),
    "Food": ("Rabbit Meat Stew", 0.1, 1.0, 1.0),
    "HighArmor": ("Armor", 0.6, 1.0, 1.0),
    "Necklace": ("Green Wreath", 0.15, 0.8, 1.0),
    "Ring": ("Shabby Straw Ring", 0.1, 1.0, 1.0),
    "test4": ("Wood", 0.5, 1.0, 1.0),
    "Enhance Stone": ("T0_Ore", 0.45, 1.5, 1.35),  # 강화석 — 하늘색 크리스털 (마젠타는 셰이더 오류처럼 보여서 피함)
})

# 방어구 AnyPortrait 텍스처
TEXTURES = {"CliffArmor": ("LeafArmor", 0.45, 1.0, 1.0)}


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


def find(folder, name):
    for ext in (".png", ".PNG"):
        p = os.path.join(folder, name + ext)
        if os.path.exists(p):
            return p
    return None


made, missing = [], []
for new, (base, h, s, v) in ICONS.items():
    src = find(ITEM, base)
    if src is None:
        missing.append(base)
        continue
    shift(src, os.path.join(ITEM, new + ".png"), h, s, v)
    made.append(new)
for new, (base, h, s, v) in TEXTURES.items():
    src = find(os.path.join(ROOT, "Sprite"), base)
    if src is None:
        missing.append("Sprite/" + base)
        continue
    shift(src, os.path.join(ROOT, "Sprite", new + ".png"), h, s, v)
    made.append("Sprite/" + new)
print("made", len(made), "missing", missing)
