# 탐험가 요리(MiniGames.md ②) 아이템 아이콘: 기존 음식 도트(Resources/Item)를 색조 이동 + 몇 칸 점 찍기로 가공한 임시 아이콘.
# 32x32 도트를 8배(256x256)로 키운 PNG 라서 8픽셀 블록 단위로 칠한다. 새 AI 생성 없음. 실행: python Tools/make_cooking_icons.py
from PIL import Image
import colorsys
import os
import random

DIR = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Resources', 'Item')
B = 8  # 도트 한 칸 = 8px


def load(name):
    return Image.open(os.path.join(DIR, name + '.png')).convert('RGBA')


def tint(img, hue_shift, sat=1.0, val=1.0):
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            h = (h + hue_shift) % 1.0
            s = min(1.0, s * sat)
            v = min(1.0, v * val)
            r2, g2, b2 = colorsys.hsv_to_rgb(h, s, v)
            px[x, y] = (int(r2 * 255), int(g2 * 255), int(b2 * 255), a)
    return out


def specks(img, color, count, seed):
    """불투명한 도트 칸 중 count 칸을 color 로 칠한다(가장자리 외곽선 칸은 피함)."""
    rnd = random.Random(seed)
    px = img.load()
    cells = []
    for cy in range(1, img.height // B - 1):
        for cx in range(1, img.width // B - 1):
            x, y = cx * B + B // 2, cy * B + B // 2
            if px[x, y][3] == 0:
                continue
            # 사방이 모두 불투명이어야 안쪽 칸
            if all(px[x + dx * B, y + dy * B][3] > 0 for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                cells.append((cx, cy))
    rnd.shuffle(cells)
    for cx, cy in cells[:count]:
        for yy in range(cy * B, cy * B + B):
            for xx in range(cx * B, cx * B + B):
                px[xx, yy] = color + (255,)
    return img


def save(name, img):
    img.save(os.path.join(DIR, name + '.png'))


GREEN = (96, 170, 82)
EMBER = (255, 150, 56)
GLOW = (255, 240, 150)
GOLD = (255, 214, 96)
LAVENDER = (214, 200, 255)

save('Herb Roasted Rabbit', specks(tint(load('Grilled Forest Carp'), -0.04, 0.9, 0.95), GREEN, 5, 1))
save('Moss Loach Broth', specks(tint(load('Forest Mushroom Soup'), 0.22, 1.1, 0.95), GREEN, 5, 2))
save('Ember Fox Skewer', specks(load('Roasted Fox Skewer'), EMBER, 4, 3))
save('Glowwing Carp Soup', specks(tint(load('Amber Trout Soup'), 0.45, 0.8, 1.0), GLOW, 5, 4))
save('Amber Herb Stew', specks(tint(load('Rabbit Meat Stew'), -0.02, 1.25, 1.05), GREEN, 5, 5))
save('Spirit Carp Moon Roast', specks(tint(load('Grilled Forest Carp'), 0.62, 0.55, 1.15), GOLD, 4, 6))
save('Burnt Meal', specks(tint(load('Grilled Forest Carp'), 0.0, 0.25, 0.32), LAVENDER, 0, 7))
print('ok')
