"""1층계 ⑤구역 '산적 성채' 계보 아이템 아이콘 (TowerWorld 15절, LineageGear 2절).
무기: Cutthroat Dagger(단검병) · Bandit Shortbow(궁수) · Raider Buckler(방패병) · Chief's War Banner(보스 고유)
재료: Bandit Bandana · Broken Arrow · Iron Rivet · Pitch Torch.
기존 아이콘 규격 256×256 (32 논리 px × 8배). 팔레트는 gen_bandit_mobs.py 의 산적 옷 색을 재사용한다.
출력: Assets/Resources/Item/<Name>.png"""
import math

from PIL import ImageDraw

from gen_forest_items import canvas, save
from gen_bandit_mobs import KINDS, STEEL, STEEL_L, STEEL_D, WOOD, WOOD_D, FLAME, FUR

CUT, ARC, SHD, CHF = KINDS["cutthroat"], KINDS["archer"], KINDS["shield"], KINDS["chief"]


def cutthroat_dagger():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(10, 22), (25, 5), (28, 3), (27, 7), (12, 24)], fill=STEEL)  # 휜 칼날
    d.line([(12, 22), (26, 5)], fill=STEEL_L)
    d.line([(13, 24), (27, 8)], fill=STEEL_D)
    d.line([(7, 19), (14, 26)], fill=CUT["hat_d"], width=2)  # 가드
    d.line([(3, 29), (9, 23)], fill=(60, 44, 40), width=3)  # 손잡이
    d.polygon([(2, 27), (0, 31), (5, 30)], fill=CUT["hat"])  # 붉은 두건 천 매듭
    d.point((20, 12), fill=(150, 30, 34))  # 핏자국
    save("CutthroatDagger", im)


def bandit_shortbow():
    im = canvas(); d = ImageDraw.Draw(im)
    d.arc((6, 2, 22, 30), 270, 90, fill=WOOD, width=3)  # 활대
    d.point((14, 2), fill=WOOD_D); d.point((14, 29), fill=WOOD_D)
    d.line([(14, 3), (11, 16), (14, 28)], fill=(226, 220, 200))  # 당긴 시위
    d.line([(11, 16), (29, 16)], fill=(196, 160, 104))  # 메긴 화살
    d.polygon([(27, 14), (31, 16), (27, 18)], fill=STEEL)
    d.line([(9, 14), (12, 16)], fill=(220, 70, 60)); d.line([(9, 18), (12, 16)], fill=(170, 50, 44))
    d.rectangle((19, 13, 21, 19), fill=ARC["hat"])  # 녹색 손잡이 감개
    save("BanditShortbow", im)


def raider_buckler():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((4, 4, 28, 28), fill=STEEL_D)
    d.ellipse((6, 6, 26, 26), fill=WOOD)
    for a in range(0, 360, 45):  # 테두리 리벳
        x, y = 16 + round(math.cos(math.radians(a)) * 11), 16 + round(math.sin(math.radians(a)) * 11)
        d.point((x, y), fill=STEEL_L)
    d.ellipse((12, 12, 20, 20), fill=SHD["accent"])  # 방패 돌기
    d.point((14, 14), fill=(250, 230, 160))
    d.line([(8, 16), (11, 16)], fill=WOOD_D); d.line([(21, 16), (24, 16)], fill=WOOD_D)
    save("RaiderBuckler", im)


def chiefs_war_banner():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(6, 31), (6, 2)], fill=WOOD_D, width=2)  # 깃대
    d.polygon([(5, 2), (7, 0), (9, 2), (7, 4)], fill=STEEL_L)  # 창끝
    d.polygon([(8, 4), (27, 6), (24, 12), (28, 18), (8, 17)], fill=CHF["body"])  # 붉은 깃발
    d.line([(8, 4), (27, 6)], fill=(190, 60, 58))
    d.line([(11, 6), (22, 15)], fill=(232, 222, 196), width=2)  # 엇갈린 뼈
    d.line([(22, 6), (11, 15)], fill=(232, 222, 196), width=2)
    d.ellipse((13, 6, 20, 12), fill=(232, 222, 196))  # 해골
    d.rectangle((14, 11, 19, 13), fill=(232, 222, 196))
    d.rectangle((14, 8, 15, 9), fill=(40, 20, 20)); d.rectangle((18, 8, 19, 9), fill=(40, 20, 20))
    d.point((15, 13), fill=(40, 20, 20)); d.point((17, 13), fill=(40, 20, 20))
    d.rectangle((4, 18, 8, 20), fill=FUR)  # 모피 장식
    save("ChiefsWarBanner", im)


def bandit_bandana():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(4, 10), (28, 10), (16, 26)], fill=CUT["hat"])
    d.line([(4, 10), (28, 10)], fill=(200, 70, 70))
    d.polygon([(4, 10), (0, 6), (2, 14)], fill=CUT["hat_d"]); d.polygon([(28, 10), (31, 5), (31, 13)], fill=CUT["hat_d"])
    for (x, y) in ((12, 13), (19, 14), (16, 19)):
        d.point((x, y), fill=(240, 220, 200))
    save("Bandit Bandana", im)


def broken_arrow():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(4, 26), (14, 16)], fill=(196, 160, 104), width=2)
    d.line([(19, 14), (26, 5)], fill=(196, 160, 104), width=2)  # 꺾여 어긋난 앞부분
    d.polygon([(25, 3), (30, 1), (28, 7)], fill=STEEL)
    for c in range(3):
        d.line([(2 + c, 24 + c), (4 + c, 29)], fill=(220, 70, 60))
    d.point((16, 14), fill=(240, 220, 170)); d.point((17, 15), fill=(240, 220, 170))  # 부러진 단면
    save("Broken Arrow", im)


def iron_rivet():
    im = canvas(); d = ImageDraw.Draw(im)
    for (x, y) in ((10, 12), (20, 18), (13, 22)):
        d.ellipse((x - 5, y - 4, x + 5, y + 4), fill=STEEL_D)
        d.ellipse((x - 4, y - 4, x + 3, y + 2), fill=STEEL)
        d.point((x - 2, y - 2), fill=STEEL_L)
    save("Iron Rivet", im)


def pitch_torch():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(10, 30), (19, 12)], fill=WOOD, width=3)
    d.rectangle((16, 11, 21, 15), fill=(50, 40, 34))  # 송진 감은 천
    d.ellipse((14, 2, 24, 13), fill=FLAME[2])
    d.ellipse((16, 4, 22, 12), fill=FLAME[1])
    d.ellipse((18, 7, 20, 11), fill=FLAME[0])
    save("Pitch Torch", im)


if __name__ == "__main__":
    cutthroat_dagger(); bandit_shortbow(); raider_buckler(); chiefs_war_banner()
    bandit_bandana(); broken_arrow(); iron_rivet(); pitch_torch()
