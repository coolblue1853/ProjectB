# 층계 낚시 물고기 아이콘(코드 도트) 생성: 32x32 픽셀을 8배(256x256)로 키워 Assets/Resources/Item/{이름}.png 로 저장.
# 새 아트 생성 없이 타원 몸통 + 꼬리 + 눈 + 무늬를 팔레트로 칠한다. 실행: python Tools/make_fish_icons.py
from PIL import Image
import math, os

OUT = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Resources', 'Item')
OUTLINE = (24, 20, 28, 255)

FISH = {
    # 이름: (몸 길이 반경, 몸 높이 반경, 밝은색, 기본색, 어두운색, 무늬 함수 이름)
    'Forest Carp': (10, 6.5, (176, 168, 104), (128, 120, 72), (84, 78, 48), 'scale'),
    'Moss Loach': (12, 3.6, (128, 160, 96), (86, 116, 64), (56, 78, 44), 'stripe'),
    'Amber Trout': (11, 5.2, (240, 186, 96), (204, 132, 52), (140, 84, 36), 'spot'),
    'Spirit Koi': (10.5, 5.8, (244, 240, 232), (214, 208, 200), (160, 150, 150), 'koi'),
}

def draw(name, rx, ry, light, base, dark, pattern):
    W = 32
    px = [[None] * W for _ in range(W)]
    cx, cy = 14, 16
    # 몸통(타원)
    for y in range(W):
        for x in range(W):
            dx, dy = (x - cx) / rx, (y - cy) / ry
            if dx * dx + dy * dy <= 1.0:
                c = light if y < cy - ry * 0.25 else (base if y < cy + ry * 0.45 else dark)
                px[y][x] = c
    # 꼬리(삼각형 두 갈래)
    tx = int(cx + rx) - 1
    for i in range(7):
        h = 1 + i
        for y in range(cy - h, cy + h + 1):
            x = tx + i
            if 0 <= x < W and 0 <= y < W and abs(y - cy) >= i // 2:
                px[y][x] = dark if abs(y - cy) > h - 2 else base
    # 등지느러미
    for i in range(5):
        x = cx - 2 + i
        y = int(cy - ry) - (1 if i < 3 else 0)
        if 0 <= y < W:
            px[y][x] = dark
    # 무늬
    for y in range(W):
        for x in range(W):
            if px[y][x] is None or x > cx + rx - 2:
                continue
            if pattern == 'scale' and (x + y) % 4 == 0 and abs(y - cy) < ry - 1 and x > cx - rx + 4:
                px[y][x] = dark
            elif pattern == 'stripe' and y == cy and x > cx - rx + 3:
                px[y][x] = dark
            elif pattern == 'spot' and (x * 7 + y * 13) % 11 == 0 and y < cy + 2:
                px[y][x] = (110, 60, 30)
            elif pattern == 'koi' and ((x - 10) ** 2 + (y - 14) ** 2 < 7 or (x - 18) ** 2 + (y - 17) ** 2 < 6):
                px[y][x] = (214, 70, 52)
    # 눈
    ex, ey = int(cx - rx * 0.62), cy - 1
    px[ey][ex] = (16, 16, 20)
    px[ey - 1][ex] = (250, 250, 250)
    # 외곽선
    out = Image.new('RGBA', (W, W), (0, 0, 0, 0))
    for y in range(W):
        for x in range(W):
            if px[y][x] is not None:
                out.putpixel((x, y), px[y][x] + (255,) if len(px[y][x]) == 3 else px[y][x])
            else:
                for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + ddx, y + ddy
                    if 0 <= nx < W and 0 <= ny < W and px[ny][nx] is not None:
                        out.putpixel((x, y), OUTLINE)
                        break
    out = out.resize((256, 256), Image.NEAREST)
    out.save(os.path.join(OUT, name + '.png'))

for n, args in FISH.items():
    draw(n, *args)
print('ok', list(FISH))
