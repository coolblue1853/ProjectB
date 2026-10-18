"""②구역 일반 숲 계보 아이템 아이콘 (TowerWorld 12.1): Fang Dagger + 재료 4종. 기존 아이콘 규격 256×256 (32 논리 px × 8배).
출력: Assets/Resources/Item/<Name>.png"""
import math
import os
from PIL import Image, ImageDraw

S, N = 8, 32
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "Item")
OUTL = (34, 26, 30)


def canvas():
    return Image.new("RGBA", (N, N), (0, 0, 0, 0))


def outline(im):
    px = im.load()
    src = im.copy().load()
    for y in range(N):
        for x in range(N):
            if src[x, y][3]:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < N and 0 <= ny < N and src[nx, ny][3] and src[nx, ny][:3] != OUTL:
                    px[x, y] = OUTL + (255,)
                    break
    return im


def save(name, im):
    outline(im)
    im.resize((N * S, N * S), Image.NEAREST).save(os.path.join(OUT, name + ".png"))
    print("ok", name)


def fang_dagger():
    im = canvas(); d = ImageDraw.Draw(im)
    # 칼날: 늑대 송곳니 모양으로 휜 흰 칼날 (대각선)
    blade = [(8, 24), (22, 6), (25, 4), (24, 8), (11, 26)]
    d.polygon(blade, fill=(226, 226, 232))
    d.line([(10, 24), (23, 6)], fill=(250, 250, 255))
    d.line([(12, 26), (24, 9)], fill=(160, 160, 176))
    # 가드: 늑대 털 회색
    d.polygon([(5, 22), (9, 20), (14, 25), (11, 28)], fill=(112, 108, 116))
    d.point((8, 23), fill=(156, 152, 160))
    # 손잡이 + 끝 장식(송곳니)
    d.line([(4, 28), (8, 24)], fill=(92, 62, 36), width=2)
    d.polygon([(2, 29), (4, 27), (5, 30)], fill=(236, 226, 196))
    d.point((20, 9), fill=(255, 196, 60))  # 호박색 눈빛 보석
    save("FangDagger", im)


def wolf_fang():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(12, 6), (20, 7), (19, 15), (16, 26), (13, 15)], fill=(236, 230, 214))
    d.line([(14, 8), (15, 22)], fill=(255, 252, 240))
    d.line([(18, 9), (17, 20)], fill=(196, 186, 170))
    d.rectangle((12, 5, 20, 7), fill=(150, 90, 80))  # 잇몸 뿌리
    save("Wolf Fang", im)


def boar_tusk():
    im = canvas(); d = ImageDraw.Draw(im)
    pts = [(6 + i, 24 - round(14 * math.sin(i / 20 * math.pi * 0.9))) for i in range(21)]
    for i, (x, y) in enumerate(pts):
        w = max(1, 3 - i // 8)
        d.ellipse((x - w, y - w, x + w, y + w), fill=(236, 226, 196))
    d.line(pts[2:18], fill=(255, 250, 230))
    d.rectangle((4, 22, 9, 27), fill=(122, 78, 46))
    save("Boar Tusk", im)


def wisp_ember():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((8, 10, 24, 26), fill=(80, 150, 255, 120))
    d.ellipse((11, 13, 21, 23), fill=(120, 200, 255))
    d.ellipse((13, 15, 19, 21), fill=(210, 245, 255))
    d.polygon([(14, 13), (17, 4), (19, 13)], fill=(150, 220, 255))
    save("Wisp Ember", im)


def snail_shell():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((6, 7, 26, 27), fill=(150, 96, 160))
    d.ellipse((7, 8, 24, 20), fill=(196, 140, 206))
    d.ellipse((9, 18, 25, 27), fill=(96, 56, 110))
    cx, cy = 16, 17
    pts = [(cx + round(math.cos(k * 0.45) * (9 - k * 0.2)), cy + round(math.sin(k * 0.45) * (9 - k * 0.2))) for k in range(42)]
    d.line(pts, fill=(80, 44, 96))
    save("Snail Shell", im)


def tusk_knuckles():
    im = canvas(); d = ImageDraw.Draw(im)
    d.rounded_rectangle((7, 12, 25, 24), radius=3, fill=(122, 78, 46))      # 가죽 너클
    d.rounded_rectangle((8, 13, 24, 17), radius=2, fill=(164, 112, 66))
    for x in (9, 14, 19):                                                  # 손가락 마디
        d.rectangle((x, 18, x + 3, 22), fill=(78, 46, 26))
    # 엄니 두 개가 앞으로 휘어 나온다
    d.polygon([(10, 12), (7, 4), (12, 9), (13, 12)], fill=(236, 226, 196))
    d.polygon([(19, 12), (21, 3), (23, 9), (22, 12)], fill=(236, 226, 196))
    d.point((8, 5), fill=(255, 250, 230)); d.point((21, 4), fill=(255, 250, 230))
    d.rectangle((6, 24, 26, 26), fill=(58, 36, 24))                          # 손목 끈
    save("TuskKnuckles", im)


if __name__ == "__main__":
    fang_dagger(); wolf_fang(); boar_tusk(); wisp_ember(); snail_shell(); tusk_knuckles()
