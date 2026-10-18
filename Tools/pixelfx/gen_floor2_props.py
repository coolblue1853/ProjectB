"""2층계 공용 소품 도트 (Floor2Plan 6절): 무너진 성벽 총안·부서진 기둥·찢긴 깃발·구름.
1층계 식생(gen_vegetation.py)과 같은 규격: 24 PPU · 아래 가운데 피벗 · 콜라이더 없음. 맵 빌더(build_broken_rampart.cs)가 지면 레이캐스트로 배치한다.
출력: Assets/Art/Floor2/<Name>.png"""
import math
import os
import random

from PIL import Image, ImageDraw

from gen_forest_mobs import shade, outline

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Floor2")
OUTLINE = (52, 54, 70)
WALL, WALL_L, WALL_D = (196, 192, 178), (230, 226, 212), (140, 136, 130)
MORTAR = (160, 154, 142)
MOSS, MOSS_L = (120, 150, 96), (156, 184, 120)
TAB, TAB_D = (86, 134, 196), (56, 90, 146)
GOLD = (206, 170, 84)
WOOD = (110, 80, 54)
CLOUD, CLOUD_L, CLOUD_D = (236, 240, 250), (252, 253, 255), (196, 206, 226)


def save(name, im):
    os.makedirs(OUT, exist_ok=True)
    im.save(os.path.join(OUT, name + ".png"))
    print("ok", name)


def bricks(d, x0, y0, x1, y1, rnd):
    for y in range(y0 + 5, y1, 6):
        d.line([(x0, y), (x1, y)], fill=MORTAR)
        off = 0 if (y // 6) % 2 else 6
        for x in range(x0 + off, x1, 12):
            d.line([(x, y - 5), (x, y)], fill=MORTAR)


def rampart_wall(broken=False, seed=1):
    """총안(凹凸) 성벽 조각 96×64. broken 이면 오른쪽이 무너져 내림"""
    rnd = random.Random(seed)
    im = Image.new("RGBA", (96, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    top = 18
    if broken:
        pts = [(2, 63), (2, top), (60, top), (66, top + 8), (72, top + 6), (80, top + 20), (88, top + 30), (94, 63)]
        d.polygon(pts, fill=WALL)
    else:
        d.rectangle((2, top, 93, 63), fill=WALL)
    for x in range(2, 92 if not broken else 58, 16):  # 총안 머리
        d.rectangle((x, top - 10, x + 8, top), fill=WALL)
    shade(im, WALL_L, WALL_D, WALL)
    bricks(d, 3, top, 93 if not broken else 70, 63, rnd)
    for k in range(4):  # 금
        x = rnd.randint(8, 80 if not broken else 56)
        y = rnd.randint(top + 4, 50)
        d.line([(x, y), (x + rnd.randint(-3, 3), y + 5), (x + rnd.randint(-2, 4), y + 9)], fill=WALL_D)
    for k in range(5):  # 이끼
        x = rnd.randint(4, 84 if not broken else 60)
        d.rectangle((x, top - 1, x + rnd.randint(3, 7), top), fill=MOSS)
        d.point((x + 1, top - 2), fill=MOSS_L)
    if broken:  # 굴러떨어진 돌
        for (x, y, r) in ((78, 58, 4), (86, 60, 3), (70, 60, 3)):
            d.ellipse((x - r, y - r, x + r, y + r), fill=WALL_D)
    outline(im, OUTLINE)
    return im


def broken_pillar(seed=2):
    """부서진 기둥 32×80: 받침 + 홈 파인 기둥 + 사선으로 꺾인 윗단"""
    rnd = random.Random(seed)
    im = Image.new("RGBA", (32, 80), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle((4, 70, 27, 79), fill=WALL)
    d.rectangle((6, 66, 25, 70), fill=WALL)
    d.polygon([(8, 66), (8, 20), (23, 12), (23, 66)], fill=WALL)
    shade(im, WALL_L, WALL_D, WALL)
    for x in (11, 15, 19):
        d.line([(x, 64), (x, 22 - (x - 8) // 2)], fill=MORTAR)
    d.line([(9, 40), (14, 44), (12, 50)], fill=WALL_D)
    d.rectangle((6, 65, 12, 66), fill=MOSS)
    d.ellipse((24, 74, 30, 79), fill=WALL_D)  # 떨어진 조각
    outline(im, OUTLINE)
    return im


def banner(seed=3):
    """찢긴 하늘색 깃발 24×72(장대 + 금 테 + 찢긴 끝)"""
    im = Image.new("RGBA", (24, 72), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.line([(5, 71), (5, 2)], fill=WOOD, width=2)
    d.ellipse((3, 0, 7, 4), fill=GOLD)
    d.polygon([(6, 6), (21, 6), (21, 34), (17, 30), (15, 38), (12, 32), (9, 40), (6, 36)], fill=TAB)
    shade(im, (120, 170, 226), TAB_D, TAB)
    d.line([(6, 7), (21, 7)], fill=GOLD)
    d.polygon([(13, 14), (15, 18), (13, 22), (11, 18)], fill=GOLD)  # 옛 왕국 문장
    outline(im, OUTLINE)
    return im


def cloud(w, h, seed):
    """뭉게구름(배경, 반투명으로 배치): 둥근 덩어리만 겹쳐 아래도 둥글게(사각 바닥 없음)"""
    rnd = random.Random(seed)
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    n = max(3, w // 14)
    for k in range(n):
        m = h * 0.55
        x = m + (w - 2 * m) * k / (n - 1)
        mid = 1 - abs(k - (n - 1) / 2) / ((n - 1) / 2)  # 가운데가 높게
        r = round(h * (0.22 + 0.2 * mid)) + rnd.randint(0, 2)
        cy = h - 3 - r
        d.ellipse((x - r * 1.3, cy - r, x + r * 1.3, cy + r), fill=CLOUD)
    shade(im, CLOUD_L, CLOUD_D, CLOUD)
    return im


if __name__ == "__main__":
    save("RampartWall", rampart_wall(False, 1))
    save("RampartWallBroken", rampart_wall(True, 4))
    save("BrokenPillar", broken_pillar())
    save("TornBanner", banner())
    save("Cloud1", cloud(96, 40, 5))
    save("Cloud2", cloud(64, 28, 6))
