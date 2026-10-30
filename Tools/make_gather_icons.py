# 채집·채굴(MiniGames.md ④⑤) 아이템 + 그 재료로 만드는 자가 제작 결과물(포션·수프·합금) 아이콘(코드 도트): 32x32 를 8배(256x256)로 키워 Assets/Resources/Item/{이름}.png 로 저장.
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


def bottle(light, base, dark, cork=(150, 110, 70)):
    px = blank()
    ellipse(px, 16, 20, 8, 8, light, base, dark)          # 둥근 병 몸
    rect(px, 14, 8, 17, 12, (200, 214, 220))              # 목
    rect(px, 13, 5, 18, 7, cork)                          # 코르크
    px[17][12] = (250, 250, 250)                          # 반사
    px[18][12] = (250, 250, 250)
    return px


def soup():
    px = blank()
    ellipse(px, 16, 15, 11, 3, (196, 150, 100), (170, 120, 80), (140, 96, 64))   # 수프 면
    for y in range(16, 25):
        half = 11 - (y - 16)
        rect(px, 16 - half, y, 16 + half - 1, y, (120, 84, 60) if y < 22 else (90, 62, 46))  # 그릇
    for x, y in ((11, 14), (19, 15), (15, 13)):
        px[y][x] = (110, 70, 50)                          # 버섯 조각
    return px


def ingot(light, base, dark):
    px = blank()
    for y in range(12, 22):
        inset = (21 - y) // 3
        rect(px, 6 + inset, y, 25 - inset, y, light if y < 15 else (base if y < 19 else dark))
    rect(px, 9, 13, 14, 13, (255, 255, 255))
    return px


ICONS = {
    'Forest Mushroom': mushroom,
    'Glow Moth Wing': moth_wing,
    'Moonpetal': moonpetal,
    'Bog Copper Ore': lambda: ore((226, 160, 110), (184, 112, 70), (120, 70, 44)),
    'Starfall Ore': lambda: ore((200, 210, 255), (130, 140, 220), (80, 84, 150), gem=(250, 236, 140)),
    'Minor Herbal Potion': lambda: bottle((150, 210, 120), (90, 160, 80), (56, 110, 56)),
    'Glow Stamina Potion': lambda: bottle((250, 236, 150), (220, 190, 80), (160, 130, 50)),
    'Forest Mushroom Soup': soup,
    'Moonpetal Elixir': lambda: bottle((236, 236, 255), (180, 186, 240), (120, 126, 190), cork=(220, 200, 120)),
    'Bog Copper Alloy': lambda: ingot((236, 170, 120), (190, 120, 76), (130, 78, 50)),
    'Starfall Alloy': lambda: ingot((214, 222, 255), (150, 160, 230), (96, 100, 170)),
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
