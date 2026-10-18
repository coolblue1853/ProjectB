"""2층계 ②구역 '무너진 성벽' 계보 아이템 아이콘 (Floor2Plan 5.1, TowerWorld 17절).
무기: Rampart Sword(성벽 기사) · Sentry Halberd(미늘창 파수병) · Gargoyle Wing(가고일) · Skyfall Talon(가고일 군주 유니크)
재료: Rampart Shard · Halberd Fragment · Gargoyle Feather · Storm Wisp.
기존 아이콘 규격 256×256 (32 논리 px × 8배). 팔레트는 gen_rampart_mobs.py 를 재사용한다.
출력: Assets/Resources/Item/<Name>.png"""
import math

from PIL import ImageDraw

from gen_forest_items import canvas, save
from gen_rampart_mobs import (ARM, ARM_L, ARM_D, GOLD, GOLD_L, TAB, TAB_D, STEEL, STEEL_L, WOOD,
                              GAR, GAR_L, GAR_D, EYE, EYE_L, CRACK, CLOUD, CLOUD_L, CLOUD_D, SPARK, SPARK_L)


def diag_blade(d, hx, hy, ln, w, ang=-45):
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    nx, ny = -uy, ux
    gx, gy = hx + ux * 6, hy + uy * 6
    tx, ty = hx + ux * ln, hy + uy * ln
    d.polygon([(gx - nx * w, gy - ny * w), (gx + nx * w, gy + ny * w), (tx, ty)], fill=STEEL)
    d.line([(gx, gy), (tx, ty)], fill=STEEL_L)
    d.line([(gx - nx * 5, gy - ny * 5), (gx + nx * 5, gy + ny * 5)], fill=GOLD, width=2)
    d.line([(hx, hy), (gx, gy)], fill=WOOD, width=2)
    d.ellipse((hx - 2, hy - 2, hx + 2, hy + 2), fill=GOLD)
    return gx, gy


def rampart_sword():
    im = canvas(); d = ImageDraw.Draw(im)
    diag_blade(d, 5, 27, 30, 2.5)
    d.point((5, 27), fill=GOLD_L)
    # 손잡이 옆 작은 연 방패 문장
    d.polygon([(18, 18), (26, 18), (26, 25), (22, 30), (18, 25)], fill=TAB)
    d.line([(22, 19), (22, 28)], fill=GOLD_L)
    d.line([(19, 22), (25, 22)], fill=GOLD_L)
    save("RampartSword", im)


def sentry_halberd():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(4, 30), (24, 6)], fill=WOOD, width=2)
    d.polygon([(19, 9), (27, 13), (28, 19), (22, 17)], fill=STEEL)  # 도끼날
    d.line([(20, 10), (27, 15)], fill=STEEL_L)
    d.polygon([(23, 4), (29, 1), (26, 7)], fill=STEEL_L)  # 창끝
    d.polygon([(18, 8), (14, 6), (17, 10)], fill=STEEL)  # 갈고리
    d.rectangle((16, 12, 18, 14), fill=GOLD)
    d.line([(6, 25), (9, 28)], fill=TAB, width=2)  # 술
    save("SentryHalberd", im)


def gargoyle_wing():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(6, 30), (18, 10)], fill=GAR_D, width=2)  # 돌 지팡이
    d.polygon([(18, 10), (4, 4), (8, 10), (5, 14), (12, 14), (10, 19)], fill=GAR)  # 박쥐 날개
    d.polygon([(18, 10), (30, 3), (27, 9), (30, 13), (23, 14), (25, 19)], fill=GAR_L)
    d.line([(18, 10), (4, 4)], fill=GAR_D)
    d.line([(18, 10), (30, 3)], fill=GAR_D)
    d.ellipse((15, 7, 21, 13), fill=GAR_D)
    d.rectangle((17, 9, 19, 10), fill=EYE_L)  # 석상 눈
    save("GargoyleWing", im)


def skyfall_talon():
    im = canvas(); d = ImageDraw.Draw(im)
    d.rectangle((5, 18, 14, 28), fill=ARM_D)  # 손목 보호대
    d.line([(5, 21), (14, 21)], fill=GOLD)
    d.ellipse((10, 10, 22, 22), fill=GAR)
    for k, (x, y) in enumerate(((22, 5), (27, 10), (28, 17))):  # 세 발톱
        d.line([(17, 15), (x, y)], fill=GAR_L, width=2)
        d.point((x, y), fill=STEEL_L)
    d.rectangle((14, 14, 16, 15), fill=EYE_L)
    for k in range(3):
        d.point((4 + k * 3, 6 + k), fill=EYE)  # 하늘에서 떨어지는 빛
    save("SkyfallTalon", im)


def rampart_shard():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(6, 12), (14, 6), (26, 9), (27, 24), (8, 26)], fill=(206, 200, 184))
    d.polygon([(6, 12), (14, 6), (26, 9), (24, 12), (9, 14)], fill=(236, 232, 220))
    for x in (9, 17):  # 총안(성벽 윗단)
        d.rectangle((x, 3, x + 4, 7), fill=(206, 200, 184))
    d.line([(14, 14), (17, 19), (15, 24)], fill=CRACK)
    d.rectangle((19, 16, 22, 18), fill=TAB)  # 옛 문장 조각
    save("Rampart Shard", im)


def halberd_fragment():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(8, 8), (22, 12), (24, 22), (14, 24)], fill=STEEL)
    d.line([(9, 9), (22, 13)], fill=STEEL_L)
    d.line([(16, 16), (20, 23)], fill=CRACK)
    d.rectangle((4, 22, 10, 24), fill=WOOD)
    save("Halberd Fragment", im)


def gargoyle_feather():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(5, 27), (12, 14), (24, 4), (20, 16)], fill=GAR)
    d.polygon([(12, 14), (24, 4), (17, 13)], fill=GAR_L)
    d.line([(5, 27), (22, 6)], fill=GAR_D)
    for k in range(3):
        d.point((10 + k * 4, 22 - k * 5), fill=EYE)
    save("Gargoyle Feather", im)


def storm_wisp():
    im = canvas(); d = ImageDraw.Draw(im)
    for (x, y, r) in ((10, 15, 6), (17, 11, 7), (23, 15, 6), (16, 17, 6)):
        d.ellipse((x - r, y - r, x + r, y + r), fill=CLOUD)
    d.ellipse((10, 6, 20, 12), fill=CLOUD_L)
    d.line([(10, 21), (21, 21)], fill=CLOUD_D)
    d.line([(16, 22), (13, 26), (17, 26), (14, 31)], fill=SPARK, width=2)
    d.point((14, 31), fill=SPARK_L)
    save("Storm Wisp", im)


if __name__ == "__main__":
    rampart_sword(); sentry_halberd(); gargoyle_wing(); skyfall_talon()
    rampart_shard(); halberd_fragment(); gargoyle_feather(); storm_wisp()
