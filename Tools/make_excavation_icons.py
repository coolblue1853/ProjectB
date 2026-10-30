# 고대 유물 발굴(MiniGames.md ③) 보상 아이콘(코드 도트): 32x32 를 8배(256x256)로 키워 Assets/Resources/Item/{이름}.png 로 저장.
# make_gather_icons.py 와 같은 방식(도형 + 3단 명암 + 외곽선). 실행: python Tools/make_excavation_icons.py
from PIL import Image
import math
import os

OUT = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Resources', 'Item')
OUTLINE = (24, 20, 28, 255)
W = 32


def blank():
    return [[None] * W for _ in range(W)]


def poly_fill(px, pts, light, base, dark):
    """볼록 다각형 채우기(위쪽 = 밝게, 아래 = 어둡게)"""
    ys = [p[1] for p in pts]
    y0, y1 = min(ys), max(ys)
    for y in range(W):
        for x in range(W):
            inside = True
            n = len(pts)
            for i in range(n):
                ax, ay = pts[i]
                bx, by = pts[(i + 1) % n]
                if (bx - ax) * (y + 0.5 - ay) - (by - ay) * (x + 0.5 - ax) < 0:
                    inside = False
                    break
            if inside:
                t = (y - y0) / max(1, (y1 - y0))
                px[y][x] = light if t < 0.3 else (base if t < 0.72 else dark)


def relic_shard():
    px = blank()
    poly_fill(px, [(12, 4), (24, 9), (27, 20), (17, 28), (6, 22), (5, 11)], (196, 186, 160), (150, 138, 116), (98, 88, 76))
    glyph = (226, 196, 110)
    for y in range(10, 22):
        px[y][15] = glyph
    for x in range(11, 20):
        px[15][x] = glyph
    for d in range(3):
        px[12 - d][12 + d] = glyph
    return px


def blueprint():
    px = blank()
    light, base, dark = (120, 160, 214), (76, 116, 176), (52, 82, 132)
    for y in range(5, 28):
        for x in range(5, 27):
            tear = (x + y) % 5 == 0 and (x > 23 or y > 25)
            if not tear:
                px[y][x] = light if y < 9 else (base if y < 22 else dark)
    line = (226, 236, 250)
    for x in range(8, 24):
        px[11][x] = line
    for y in range(11, 24):
        px[y][16] = line
    for a in range(0, 360, 20):
        r = math.radians(a)
        px[int(18 + 4 * math.sin(r))][int(11 + 4 * math.cos(r))] = line
    return px


def gear_core():
    px = blank()
    light, base, dark = (226, 176, 104), (176, 122, 62), (112, 74, 40)
    cx = cy = 15.5
    for y in range(W):
        for x in range(W):
            dx, dy = x - cx, y - cy
            r = math.hypot(dx, dy)
            ang = math.atan2(dy, dx)
            tooth = math.cos(ang * 8) > 0.35
            if r <= 10 or (tooth and r <= 13):
                px[y][x] = light if dy < -4 else (base if dy < 5 else dark)
    core_l, core = (200, 252, 246), (90, 200, 204)
    for y in range(W):
        for x in range(W):
            r = math.hypot(x - cx, y - cy)
            if r <= 4:
                px[y][x] = core_l if r <= 1.6 else core
    return px


def runestone():
    px = blank()
    poly_fill(px, [(10, 4), (22, 4), (27, 14), (24, 28), (8, 28), (5, 14)], (110, 104, 124), (76, 72, 90), (50, 48, 62))
    rune = (150, 226, 255)
    for y in range(9, 24):
        px[y][16] = rune
    for d in range(5):
        px[10 + d][16 - d - 1] = rune
        px[10 + d][16 + d + 1] = rune
        px[18 + d][16 + d] = rune if d < 4 else rune
    return px


ICONS = {
    'Ancient Relic Shard': relic_shard,
    'Ancient Blueprint Fragment': blueprint,
    'Ancient Gear Core': gear_core,
    'Forgotten Runestone': runestone,
}


def save(name, px):
    out = Image.new('RGBA', (W, W), (0, 0, 0, 0))
    for y in range(W):
        for x in range(W):
            if px[y][x] is not None:
                out.putpixel((x, y), px[y][x] + (255,))
            else:
                for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + ddx, y + ddy
                    if 0 <= nx < W and 0 <= ny < W and px[ny][nx] is not None:
                        out.putpixel((x, y), OUTLINE)
                        break
    out.resize((256, 256), Image.NEAREST).save(os.path.join(OUT, name + '.png'))


for n, f in ICONS.items():
    save(n, f())
print('ok', list(ICONS))
