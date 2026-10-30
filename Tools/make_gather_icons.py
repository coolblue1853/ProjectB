# 채집·채굴(MiniGames.md ④⑤) 아이템 아이콘(코드 도트): 32x32 를 8배(256x256)로 키워 Assets/Resources/Item/{이름}.png 로 저장.
# 새 아트 생성 없이 도형 + 팔레트 3단 명암 + 외곽선. make_fish_icons.py 와 같은 방식. 실행: python Tools/make_gather_icons.py
from PIL import Image
import os

OUT = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Resources', 'Item')
OUTLINE = (24, 20, 28, 255)
W = 32


def blank():
    return [[None] * W for _ in range(W)]


def ellipse(px, cx, cy, rx, ry, light, base, dark):
    for y in range(W):
        for x in range(W):
            dx, dy = (x - cx) / rx, (y - cy) / ry
            if dx * dx + dy * dy <= 1.0:
                px[y][x] = light if dy < -0.35 else (base if dy < 0.4 else dark)


def rect(px, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            px[y][x] = c


def mushroom():
    px = blank()
    rect(px, 13, 16, 18, 26, (214, 200, 170))           # 대
    rect(px, 17, 16, 18, 26, (170, 156, 128))
    ellipse(px, 15.5, 13, 11, 6.5, (176, 120, 88), (140, 86, 60), (96, 58, 42))  # 갓
    for x, y in ((10, 11), (15, 9), (20, 12), (13, 14)):
        px[y][x] = (230, 214, 186)                       # 점무늬
    return px


def moth_wing():
    px = blank()
    ellipse(px, 11, 13, 8, 9, (236, 226, 150), (200, 182, 96), (140, 120, 60))   # 앞날개
    ellipse(px, 20, 19, 6, 6, (210, 200, 130), (170, 150, 80), (120, 100, 50))   # 뒷날개
    for x, y in ((9, 10), (12, 14), (20, 18)):
        px[y][x] = (250, 250, 210)                       # 발광점
    px[13][7] = (120, 90, 50)
    return px


def moonpetal():
    px = blank()
    for cx, cy in ((16, 9), (9, 15), (23, 15), (12, 22), (20, 22)):
        ellipse(px, cx, cy, 4.5, 4.5, (236, 236, 252), (190, 196, 236), (130, 136, 190))
    ellipse(px, 16, 16, 3, 3, (250, 236, 160), (230, 206, 110), (200, 170, 80))  # 꽃술
    return px


def ore(light, base, dark, gem=None):
    px = blank()
    ellipse(px, 16, 18, 11, 8.5, (150, 142, 138), (110, 102, 100), (74, 68, 70))  # 바위
    ellipse(px, 13, 16, 5, 4, light, base, dark)          # 광물 덩어리
    ellipse(px, 21, 20, 3.5, 3, light, base, dark)
    if gem:
        rect(px, 18, 11, 19, 15, gem)
        px[10][18] = (255, 255, 255)
    return px


ICONS = {
    'Forest Mushroom': mushroom,
    'Glow Moth Wing': moth_wing,
    'Moonpetal': moonpetal,
    'Bog Copper Ore': lambda: ore((226, 160, 110), (184, 112, 70), (120, 70, 44)),
    'Starfall Ore': lambda: ore((200, 210, 255), (130, 140, 220), (80, 84, 150), gem=(250, 236, 140)),
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
