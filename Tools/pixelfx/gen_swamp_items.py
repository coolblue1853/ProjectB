"""1층계 ④구역 '늪지' 계보 아이템 아이콘 (TowerWorld 14절, LineageGear 1절).
무기: Frog Tongue Whip · Bloodsting Rapier · Slime Mace · Croc Jaw Axe · Petal Pincer · Primordial Gel(보스 고유)
재료: Frog Skin · Crab Pincer · Croc Tooth · Blood Sting · Slime Gel.
기존 아이콘 규격 256×256 (32 논리 px × 8배). 팔레트는 gen_swamp_mobs.py 의 몹 색을 재사용한다.
출력: Assets/Resources/Item/<Name>.png"""
import math
from PIL import ImageDraw

from gen_forest_items import canvas, save
from gen_swamp_mobs import (FR_BASE, FR_LIGHT, FR_DARK, FR_BELLY, FR_TONGUE, CR_BASE, CR_LIGHT, CR_DARK,
                            CC_BASE, CC_LIGHT, CC_DARK, CC_TEETH, MQ_BASE, MQ_DARK, MQ_BLOOD, SLIME, GIANT)

WOOD, WOOD_D = (120, 86, 54), (78, 54, 34)


def frog_tongue_whip():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(3, 29), (12, 20)], fill=WOOD, width=3)  # 손잡이
    d.ellipse((10, 15, 17, 22), fill=FR_BASE)  # 개구리 머리 장식
    d.point((12, 16), fill=(20, 20, 10)); d.point((15, 16), fill=(20, 20, 10))
    pts = [(16 + i, 17 - round(math.sin(i * 0.45) * 5) - i // 2) for i in range(0, 13)]  # 휘어진 분홍 혀 채찍
    d.line(pts, fill=FR_TONGUE, width=2)
    d.ellipse((26, 3, 31, 8), fill=FR_TONGUE)
    d.point((28, 4), fill=(255, 190, 200))
    save("FrogTongueWhip", im)


def bloodsting_rapier():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(9, 23), (30, 2)], fill=(200, 196, 190), width=1)  # 가는 찌르기 칼날
    d.line([(10, 23), (30, 3)], fill=(150, 30, 40))  # 피 홈
    d.arc((4, 18, 14, 28), 200, 20, fill=MQ_BASE)  # 컵 가드
    d.line([(6, 20), (12, 26)], fill=MQ_DARK, width=2)
    d.line([(2, 30), (7, 25)], fill=WOOD_D, width=2)
    d.ellipse((7, 22, 10, 25), fill=MQ_BLOOD)  # 붉은 보석(부푼 배)
    for (x, y) in ((20, 14), (24, 9)):  # 맺힌 핏방울
        d.point((x + 1, y + 1), fill=MQ_BLOOD)
    save("BloodstingRapier", im)


def slime_mace(pal=SLIME, name="SlimeMace"):
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(4, 29), (16, 15)], fill=WOOD, width=3)
    d.ellipse((13, 3, 29, 19), fill=pal["base"])  # 점액 덩어리 머리
    d.ellipse((15, 4, 25, 11), fill=pal["light"])
    d.ellipse((19, 9, 24, 14), fill=pal["core"] + (200,))
    for (x, y) in ((14, 18), (27, 17), (21, 20)):  # 흘러내리는 방울
        d.ellipse((x - 1, y, x + 1, y + 3), fill=pal["base"])
    d.point((17, 6), fill=(240, 255, 240))
    save(name, im)


def primordial_gel():
    im = canvas(); d = ImageDraw.Draw(im)
    # 거대 슬라임의 핵을 박은 대검: 탁한 점액 칼날 속에 노란 핵이 떠 있다
    d.polygon([(5, 26), (23, 5), (29, 2), (27, 9), (8, 29)], fill=GIANT["base"])
    d.line([(7, 26), (25, 6)], fill=GIANT["light"])
    d.ellipse((15, 11, 21, 17), fill=GIANT["core"])
    d.point((17, 13), fill=GIANT["eye"]); d.point((19, 13), fill=GIANT["eye"])
    d.polygon([(2, 22), (8, 20), (12, 26), (9, 30)], fill=GIANT["dark"])
    d.line([(1, 31), (5, 27)], fill=WOOD_D, width=2)
    for (x, y) in ((10, 20), (24, 4)):
        d.point((x, y), fill=(220, 240, 180))
    save("PrimordialGel", im)


def croc_jaw_axe():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(4, 30), (20, 6)], fill=WOOD, width=3)
    # 악어 턱 도끼날: 위아래 턱 + 이빨
    d.polygon([(16, 4), (29, 6), (30, 11), (18, 12)], fill=CC_BASE)
    d.polygon([(17, 13), (30, 13), (28, 18), (16, 18)], fill=CC_DARK)
    d.line([(17, 5), (29, 7)], fill=CC_LIGHT)
    for x in range(19, 30, 3):
        d.point((x, 12), fill=CC_TEETH); d.point((x + 1, 13), fill=CC_TEETH)
    d.point((20, 7), fill=(230, 200, 60))
    save("CrocJawAxe", im)


def petal_pincer():
    im = canvas(); d = ImageDraw.Draw(im)
    # 꽃게 집게 너클: 손등 껍데기 + 앞으로 벌린 집게 + 꽃잎 장식
    d.rounded_rectangle((5, 14, 18, 27), radius=3, fill=CR_BASE)
    d.rounded_rectangle((6, 15, 17, 18), radius=2, fill=CR_LIGHT)
    d.polygon([(17, 14), (29, 9), (30, 13), (19, 18)], fill=CR_BASE)
    d.polygon([(17, 20), (29, 24), (28, 27), (18, 24)], fill=CR_DARK)
    d.line([(20, 13), (28, 10)], fill=CR_LIGHT)
    for (x, y) in ((8, 11), (11, 9), (14, 11)):  # 꽃잎
        d.ellipse((x - 2, y - 2, x + 2, y + 2), fill=(240, 160, 190))
    d.point((11, 11), fill=(255, 230, 120))
    save("PetalPincer", im)


def frog_skin():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(6, 10), (14, 6), (24, 8), (27, 18), (20, 27), (9, 25), (5, 17)], fill=FR_BASE)
    d.polygon([(10, 12), (20, 10), (22, 18), (12, 21)], fill=FR_BELLY)
    for (x, y) in ((9, 9), (23, 12), (8, 21), (21, 23)):
        d.rectangle((x, y, x + 1, y + 1), fill=FR_DARK)
    d.line([(7, 11), (13, 7)], fill=FR_LIGHT)
    save("Frog Skin", im)


def crab_pincer():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((6, 12, 20, 24), fill=CR_BASE)
    d.polygon([(18, 14), (29, 8), (28, 13), (20, 17)], fill=CR_LIGHT)
    d.polygon([(18, 20), (28, 24), (26, 27), (17, 23)], fill=CR_DARK)
    d.line([(8, 14), (15, 13)], fill=CR_LIGHT)
    save("Crab Pincer", im)


def croc_tooth():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(12, 4), (20, 4), (22, 12), (17, 28), (15, 28), (10, 12)], fill=CC_TEETH)
    d.line([(14, 6), (13, 13)], fill=(255, 255, 250))
    d.polygon([(11, 4), (21, 4), (20, 8), (12, 8)], fill=(176, 150, 120))  # 뿌리
    save("Croc Tooth", im)


def blood_sting():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(6, 26), (26, 6)], fill=MQ_DARK, width=2)  # 모기 침
    d.ellipse((4, 22, 11, 29), fill=MQ_BASE)
    d.ellipse((20, 14, 26, 21), fill=MQ_BLOOD)  # 핏방울
    d.point((22, 15), fill=(255, 140, 140))
    save("Blood Sting", im)


def slime_gel():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((7, 10, 25, 27), fill=SLIME["base"])
    d.rectangle((8, 22, 24, 26), fill=SLIME["base"])
    d.ellipse((9, 11, 19, 18), fill=SLIME["light"])
    d.ellipse((14, 16, 19, 21), fill=SLIME["core"] + (200,))
    d.point((11, 13), fill=(240, 255, 240))
    save("Slime Gel", im)


if __name__ == "__main__":
    frog_tongue_whip(); bloodsting_rapier(); slime_mace(); primordial_gel(); croc_jaw_axe(); petal_pincer()
    frog_skin(); crab_pincer(); croc_tooth(); blood_sting(); slime_gel()
