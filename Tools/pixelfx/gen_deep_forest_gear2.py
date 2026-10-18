"""1층계 ③구역 '깊은 숲' 계보 장비 2차 (TowerWorld 12.2·13절) 코드 도트.
- 아이콘(256×256, 32 논리 px × 8): Phantom Scythe(유령) · Fae Wand(요정)
- 요정 가루 유도탄 프레임: Assets/Art/Weapons/FaeDust.png (16px × 4프레임)
- 고대숲의 엔트 보스 공격 판정: Assets/Art/Monsters/AncientEnt/RootSwipe.png (64px × 6, 땅에서 솟는 뿌리 휩쓸기)
                               Assets/Art/Monsters/AncientEnt/RootThorn.png (32px × 3, 날아가는 뿌리 가시)
팔레트는 gen_deep_forest_mobs.py(유령·요정·고대 엔트)를 재사용한다. 사용: python Tools/pixelfx/gen_deep_forest_gear2.py"""
import math
import os
from PIL import Image, ImageDraw

from gen_forest_items import canvas, save

HERE = os.path.dirname(__file__)
ART = os.path.join(HERE, "..", "..", "Assets", "Art")

GH, GH_L, GH_D = (200, 220, 236), (236, 246, 255), (140, 160, 190)
VIO, VIO_L, VIO_D = (150, 110, 200), (196, 160, 240), (90, 64, 130)
PINK, PINK_L, CREAM = (244, 140, 190), (255, 200, 230), (255, 250, 210)
ANC, ANC_L, ANC_D, ANC_LEAF, ANC_EYE = (92, 84, 72), (130, 122, 104), (56, 50, 44), (46, 104, 92), (255, 170, 60)
BARK, BARK_L, BARK_D, MOSS = (104, 78, 52), (140, 108, 72), (66, 48, 32), (96, 140, 70)


def phantom_scythe():
    im = canvas(); d = ImageDraw.Draw(im)
    # 자루: 보라빛 유령 나무 (대각선)
    d.line([(6, 30), (22, 4)], fill=VIO_D, width=2)
    d.line([(7, 29), (22, 5)], fill=VIO)
    # 낫날: 반투명 유령빛으로 크게 휜 초승달
    blade = []
    for i in range(0, 15):
        a = math.radians(200 + i * 9)
        blade.append((22 + round(math.cos(a) * -11), 6 + round(math.sin(a) * 6)))
    d.polygon([(22, 4)] + blade + [(12, 10)], fill=GH + (220,))
    d.line(blade, fill=GH_L)
    d.line([(14, 9), (21, 6)], fill=GH_D)
    # 손잡이 끝 유령 혼 장식
    d.ellipse((3, 26, 8, 31), fill=GH + (200,))
    d.point((5, 28), fill=(60, 70, 100)); d.point((7, 28), fill=(60, 70, 100))
    for (x, y) in ((27, 15), (29, 20), (25, 22)):  # 흩날리는 혼불
        d.point((x, y), fill=VIO_L)
    save("PhantomScythe", im)


def fae_wand():
    im = canvas(); d = ImageDraw.Draw(im)
    # 가는 은빛 나뭇가지 지팡이 + 끝에 분홍 별꽃, 날개 장식
    d.line([(5, 30), (20, 10)], fill=BARK_L, width=2)
    d.line([(6, 30), (20, 11)], fill=(214, 200, 176))
    wing = (180, 236, 255, 170)
    d.polygon([(19, 12), (11, 5), (12, 11)], fill=wing)
    d.polygon([(21, 13), (28, 18), (22, 18)], fill=wing)
    # 별꽃
    cx, cy = 23, 8
    pts = []
    for k in range(10):
        r = 6 if k % 2 == 0 else 2.6
        a = math.radians(-90 + k * 36)
        pts.append((cx + round(math.cos(a) * r), cy + round(math.sin(a) * r)))
    d.polygon(pts, fill=PINK)
    d.ellipse((cx - 2, cy - 2, cx + 2, cy + 2), fill=PINK_L)
    d.point((cx, cy), fill=CREAM)
    for (x, y) in ((28, 4), (15, 4), (29, 11)):  # 요정 가루
        d.line([(x - 1, y), (x + 1, y)], fill=CREAM); d.line([(x, y - 1), (x, y + 1)], fill=CREAM)
    save("FaeWand", im)


def fae_dust():
    """유도탄: 분홍 빛가루 덩이 + 반짝이(4프레임 회전)"""
    n, fs = 4, 16
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for f in range(n):
        ox = f * fs; c = 8
        d.ellipse((ox + c - 4, c - 4, ox + c + 4, c + 4), fill=PINK + (230,))
        d.ellipse((ox + c - 2, c - 2, ox + c + 2, c + 2), fill=PINK_L + (255,))
        d.point((ox + c, c), fill=CREAM + (255,))
        for k in range(4):  # 도는 반짝이
            a = math.radians(f * 22 + k * 90)
            x, y = ox + c + round(math.cos(a) * 6.5), c + round(math.sin(a) * 6.5)
            d.point((x, y), fill=CREAM + (255,))
    path = os.path.join(ART, "Weapons", "FaeDust.png")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    im.save(path); print("ok", path)


def root_shape(d, x0, base, h, w, lean, col):
    """땅(base)에서 솟는 뿌리 한 줄기: 아래가 굵고 끝이 뾰족, lean 만큼 휜다"""
    if h <= 0:
        return
    left, right = [], []
    steps = max(2, h)
    for i in range(steps + 1):
        t = i / steps
        y = base - t * h
        x = x0 + lean * t * t
        half = w * (1 - t) + 0.4
        left.append((x - half, y)); right.append((x + half, y))
    d.polygon(left + right[::-1], fill=col[0])
    d.line([(p[0] + 0.5, p[1]) for p in left[: steps * 3 // 4]], fill=col[1])  # 밝은 면
    for i in range(2, steps - 2, 4):  # 잔가시
        p = right[i]
        d.line([p, (p[0] + 3, p[1] - 2)], fill=col[2])


def root_swipe():
    """보스 근접(구 갈퀴): 앞쪽 땅에서 뿌리 다섯 가닥이 차례로 솟았다가 꺼진다. 64px × 6프레임, 오른쪽이 앞"""
    n, fs = 6, 64
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    roots = [(10, 30, 5, 6), (22, 44, 6, 8), (34, 54, 7, 6), (46, 44, 6, 4), (56, 30, 5, 2)]  # x, 최대 높이, 굵기, 휨
    grow = [0.2, 0.55, 1.0, 1.0, 0.7, 0.3]
    col = (BARK, BARK_L, BARK_D)
    for f in range(n):
        ox = f * fs; base = fs - 3
        # 갈라진 땅 + 흙 튐
        d.rectangle((ox + 2, base, ox + fs - 3, base + 2), fill=(70, 54, 40, 255))
        for k, (x, h, w, lean) in enumerate(roots):
            g = grow[f] if k % 2 == 0 else grow[max(0, f - 1)]
            root_shape(d, ox + x, base, round(h * g), w, lean, col)
            if g >= 0.9:  # 다 솟은 뿌리 끝에 이끼·잎
                tip = (ox + x + lean, base - h)
                d.point((tip[0] - 1, tip[1] + 4), fill=MOSS + (255,))
                d.polygon([(tip[0] + 1, tip[1] + 6), (tip[0] + 5, tip[1] + 4), (tip[0] + 2, tip[1] + 9)], fill=ANC_LEAF + (255,))
        if 1 <= f <= 3:
            for (x, y) in ((8, -6), (30, -10), (50, -5)):
                d.point((ox + x + f, base + y - f * 2), fill=(120, 96, 70, 255))
    path = os.path.join(ART, "Monsters", "AncientEnt", "RootSwipe.png")
    im.save(path); print("ok", path)


def root_thorn():
    """보스 원거리(구 불꽃): 이끼 낀 뿌리 가시가 날아간다(+x 방향). 끝에 주황 수액 빛. 32px × 3프레임"""
    n, fs = 3, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for f in range(n):
        ox = f * fs; cy = 16
        d.polygon([(ox + 4, cy - 3), (ox + 22, cy - 2), (ox + 29, cy), (ox + 22, cy + 2), (ox + 4, cy + 3)], fill=ANC + (255,))
        d.line([(ox + 5, cy - 2), (ox + 24, cy - 1)], fill=ANC_L + (255,))
        d.line([(ox + 5, cy + 2), (ox + 22, cy + 2)], fill=ANC_D + (255,))
        for x in (8, 13, 18):  # 가시
            d.line([(ox + x, cy - 3), (ox + x - 2, cy - 6)], fill=ANC_D + (255,))
            d.line([(ox + x + 2, cy + 3), (ox + x, cy + 6)], fill=ANC_D + (255,))
        d.point((ox + 10, cy - 1), fill=MOSS + (255,)); d.point((ox + 16, cy + 1), fill=MOSS + (255,))
        glow = [ANC_EYE, (255, 210, 120), ANC_EYE][f]
        d.ellipse((ox + 25, cy - 2, ox + 29, cy + 2), fill=glow + (255,))
        tail = [(0, -1), (1, 1), (-1, 0)][f]  # 흩날리는 흙·잎 꼬리
        d.point((ox + 2, cy + tail[0]), fill=ANC_LEAF + (255,)); d.point((ox + 1, cy + tail[1] + 2), fill=(120, 96, 70, 255))
    path = os.path.join(ART, "Monsters", "AncientEnt", "RootThorn.png")
    im.save(path); print("ok", path)


if __name__ == "__main__":
    phantom_scythe(); fae_wand(); fae_dust(); root_swipe(); root_thorn()
