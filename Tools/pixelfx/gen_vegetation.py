"""1층계 얕은 숲 식생 장식 도트 (구글 문서: 풀·버섯·잡다한 풀·예쁜 풀·이상한 나무), 24 PPU, 바닥 중앙 피벗.
출력: Assets/Art/Vegetation/<Name>.png (단일 스프라이트). 배경 장식 전용(콜라이더 없음)."""
import os, random
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Vegetation")
os.makedirs(OUT, exist_ok=True)
rng = random.Random(3)
G_D, G, G_L = (34, 92, 44), (62, 140, 58), (120, 196, 86)
OUTL = (18, 40, 24)

def grass(name, w=18, h=12, blades=7, light=False):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for i in range(blades):
        x = 2 + int(i * (w - 4) / max(1, blades - 1)); top = rng.randint(1, 5); lean = rng.choice((-2, -1, 1, 2))
        col = G_L if (light and i % 2 == 0) else (G if i % 2 else G_D)
        d.line((x, h - 1, x + lean, top), fill=col, width=2)
        d.point((x + lean, top), fill=G_L)
    im.save(os.path.join(OUT, name + ".png"))

def mushroom(name, cap, spots, w=14, h=14):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.rectangle((5, 7, 8, 13), fill=(226, 212, 180), outline=OUTL)                 # 줄기
    d.ellipse((1, 1, 12, 9), fill=cap, outline=OUTL)                              # 갓
    for (x, y) in spots:
        d.point((x, y), fill=(250, 240, 220))
    im.save(os.path.join(OUT, name + ".png"))

def flower(name, petal, w=12, h=16):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.line((6, 15, 6, 6), fill=G, width=2); d.line((6, 11, 3, 9), fill=G_L); d.line((6, 12, 9, 10), fill=G_L)
    for (x, y) in ((6, 2), (3, 5), (9, 5), (6, 8), (4, 3), (8, 3), (4, 7), (8, 7)):
        d.point((x, y), fill=petal)
    d.rectangle((5, 4, 7, 6), fill=(255, 230, 120), outline=OUTL)
    im.save(os.path.join(OUT, name + ".png"))

def odd_tree(name, w=34, h=48):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.rectangle((15, 26, 19, 47), fill=(96, 64, 40), outline=OUTL)                 # 굵은 줄기
    d.line((17, 30, 11, 22), fill=(96, 64, 40), width=3); d.line((17, 32, 24, 24), fill=(96, 64, 40), width=3)
    for (cx, cy, r, c) in ((17, 14, 11, (40, 110, 150)), (9, 20, 7, (60, 140, 170)), (25, 19, 7, (50, 120, 160)), (17, 9, 6, (90, 180, 200))):
        d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=c, outline=OUTL)          # 푸른 잎 덩어리 (이상한 나무)
    for (x, y) in ((12, 12), (20, 8), (26, 17), (8, 23), (16, 20)):
        d.point((x, y), fill=(200, 240, 255))
    im.save(os.path.join(OUT, name + ".png"))

grass("Grass1"); grass("Grass2", w=22, blades=9); grass("GrassLight", light=True)
mushroom("MushroomRed", (200, 60, 50), ((4, 4), (8, 3), (6, 6)))
mushroom("MushroomBrown", (150, 110, 70), ((5, 4), (8, 5)))
flower("FlowerWhite", (240, 240, 250)); flower("FlowerPink", (240, 150, 190))
odd_tree("OddTree")
print("ok")
