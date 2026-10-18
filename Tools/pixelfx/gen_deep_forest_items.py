"""1층계 ③구역 '깊은 숲' 계보 아이템 아이콘 (TowerWorld 12·13절): Bear Claw · Root Staff · Heartwood Greatsword(보스 고유) + 재료 4종.
기존 아이콘 규격 256×256 (32 논리 px × 8배). 팔레트는 gen_deep_forest_mobs.py 의 몹 색을 재사용한다.
출력: Assets/Resources/Item/<Name>.png"""
import math
import os
import random
from PIL import Image, ImageDraw

from gen_forest_items import canvas, save

BEAR, BEAR_L, BEAR_D, CLAW = (110, 72, 44), (150, 104, 64), (70, 44, 28), (236, 230, 214)
BARK, BARK_L, BARK_D = (104, 78, 52), (140, 108, 72), (66, 48, 32)
LEAF, LEAF_L = (70, 128, 58), (110, 168, 80)
ANC, ANC_L, ANC_D, ANC_LEAF, ANC_EYE = (92, 84, 72), (130, 122, 104), (56, 50, 44), (46, 104, 92), (255, 170, 60)


def bear_claw():
    im = canvas(); d = ImageDraw.Draw(im)
    # 곰 앞발 장갑 + 앞으로 휜 발톱 3개
    d.rounded_rectangle((6, 14, 22, 28), radius=4, fill=BEAR)
    d.rounded_rectangle((7, 15, 21, 19), radius=3, fill=BEAR_L)
    d.rectangle((6, 25, 22, 28), fill=BEAR_D)
    for y in (15, 20, 25):  # 앞으로 휘어 내려오는 갈고리 발톱
        d.polygon([(21, y - 2), (26, y - 4), (30, y - 1), (29, y + 1), (26, y - 1), (21, y + 1)], fill=CLAW)
        d.line([(22, y - 2), (26, y - 3)], fill=(255, 255, 245))
    d.ellipse((9, 18, 13, 22), fill=(176, 136, 96))  # 발바닥 패드
    save("BearClaw", im)


def root_staff():
    im = canvas(); d = ImageDraw.Draw(im)
    # 비틀린 뿌리 지팡이 (대각선) — 끝에 잎과 초록 빛
    pts = [(4 + i, 29 - i + round(math.sin(i * 0.7) * 1.2)) for i in range(0, 20)]
    d.line(pts, fill=BARK, width=3)
    d.line(pts[::2], fill=BARK_L)
    for x, y in ((8, 26), (12, 20), (16, 17)):  # 잔뿌리
        d.line([(x, y), (x - 3, y - 2)], fill=BARK_D)
    # 머리: 뿌리가 감싼 구슬
    d.ellipse((19, 3, 29, 13), fill=(120, 210, 110))
    d.ellipse((21, 5, 26, 10), fill=(210, 255, 190))
    for a in range(0, 360, 72):
        r = math.radians(a)
        d.line([(24, 8), (24 + round(math.cos(r) * 6), 8 + round(math.sin(r) * 6))], fill=BARK_D)
    d.polygon([(17, 12), (13, 9), (16, 7)], fill=LEAF)
    d.polygon([(28, 14), (31, 17), (27, 17)], fill=LEAF_L)
    save("RootStaff", im)


def heartwood_greatsword():
    im = canvas(); d = ImageDraw.Draw(im)
    # 이끼 낀 돌빛 대검 — 칼날 가운데로 주황 수액(고대 엔트의 눈빛)이 흐른다
    d.polygon([(5, 26), (24, 4), (29, 2), (27, 8), (8, 29)], fill=ANC_L)
    d.line([(7, 27), (26, 5)], fill=ANC_EYE)
    d.line([(9, 28), (27, 7)], fill=ANC_D)
    random.seed(3)
    for _ in range(6):  # 이끼 점
        t = random.random()
        d.point((round(6 + t * 19), round(26 - t * 20)), fill=(96, 140, 70))
    d.polygon([(2, 22), (8, 20), (12, 26), (9, 30)], fill=ANC_LEAF)  # 잎 가드
    d.point((7, 23), fill=(230, 150, 200))
    d.line([(1, 31), (5, 27)], fill=BARK_D, width=2)  # 손잡이
    save("HeartwoodGreatsword", im)


def bear_pelt():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(6, 8), (12, 5), (20, 5), (26, 8), (27, 18), (24, 27), (16, 25), (8, 27), (5, 18)], fill=BEAR)
    d.polygon([(8, 9), (14, 7), (20, 7), (24, 9), (22, 13), (10, 13)], fill=BEAR_L)
    for x in (6, 26):
        d.rectangle((x - 1, 18, x + 1, 22), fill=BEAR_D)
    save("Bear Pelt", im)


def heartwood_chip():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(8, 10), (22, 6), (26, 18), (12, 26), (6, 20)], fill=BARK)
    d.polygon([(11, 12), (20, 10), (22, 17), (13, 21)], fill=(196, 150, 96))  # 속살
    for r in (2, 4):
        d.ellipse((17 - r, 15 - r, 17 + r, 15 + r), outline=(150, 104, 60))
    d.point((17, 15), fill=(255, 210, 120))
    save("Heartwood Chip", im)


def ectoplasm():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((8, 10, 24, 26), fill=(200, 220, 236, 200))
    d.ellipse((10, 12, 20, 20), fill=(236, 246, 255, 230))
    d.polygon([(14, 10), (17, 3), (19, 10)], fill=(200, 220, 236, 200))
    d.rectangle((13, 16, 14, 17), fill=(60, 70, 100))
    d.rectangle((18, 16, 19, 17), fill=(60, 70, 100))
    save("Ectoplasm", im)


def fairy_dust():
    im = canvas(); d = ImageDraw.Draw(im)
    d.rounded_rectangle((10, 12, 22, 27), radius=3, fill=(170, 210, 230, 200))  # 작은 유리병
    d.rectangle((13, 9, 19, 12), fill=(160, 110, 70))
    d.rectangle((11, 18, 21, 26), fill=(255, 236, 150))
    for (x, y) in ((13, 20), (17, 22), (19, 19), (15, 24)):
        d.point((x, y), fill=(255, 255, 230))
    for (x, y) in ((6, 8), (25, 6), (26, 14)):  # 반짝이
        d.line([(x - 1, y), (x + 1, y)], fill=(255, 250, 190)); d.line([(x, y - 1), (x, y + 1)], fill=(255, 250, 190))
    save("Fairy Dust", im)


if __name__ == "__main__":
    bear_claw(); root_staff(); heartwood_greatsword()
    bear_pelt(); heartwood_chip(); ectoplasm(); fairy_dust()
