"""1층계 ⑥구역 '탑 근교' 계보 아이템 아이콘 (TowerWorld 16절, LineageGear 3절).
무기: Pebble Gauntlet(소형 골렘) · Quakestone Maul(중형 골렘) · Monolith Shield(대형 골렘) · Gatekeeper's Oath(문지기 레전더리)
재료: Golem Pebble · Quake Core · Monolith Shard · Ruin Mote.
기존 아이콘 규격 256×256 (32 논리 px × 8배). 팔레트는 gen_outskirts_mobs.py(돌·이끼·룬·문지기 갑옷)를 재사용한다.
출력: Assets/Resources/Item/<Name>.png"""
import math

from PIL import ImageDraw

from gen_forest_items import canvas, save
from gen_outskirts_mobs import STONE, STONE_L, STONE_D, MOSS, MOSS_L, RUNE, RUNE_L, CRACK, ARM, ARM_L, ARM_D, GOLD, GOLD_L, STEEL, STEEL_L


def pebble_gauntlet():
    im = canvas(); d = ImageDraw.Draw(im)
    d.rectangle((6, 18, 16, 28), fill=(92, 70, 52))  # 가죽 손목
    d.line([(6, 20), (16, 20)], fill=(70, 52, 38))
    for i, (x, y) in enumerate(((14, 8), (20, 6), (25, 10), (13, 15), (20, 14), (25, 17))):  # 주먹에 박힌 조약돌
        d.ellipse((x - 3, y - 3, x + 3, y + 3), fill=STONE if i % 2 == 0 else STONE_D)
        d.point((x - 1, y - 1), fill=STONE_L)
    d.rectangle((19, 10, 20, 11), fill=RUNE_L)
    d.line([(10, 17), (16, 13)], fill=MOSS)
    save("PebbleGauntlet", im)


def quakestone_maul():
    im = canvas(); d = ImageDraw.Draw(im)
    d.line([(4, 30), (17, 13)], fill=(96, 70, 48), width=3)  # 자루
    d.polygon([(11, 6), (24, 2), (30, 13), (17, 18)], fill=STONE)  # 돌 망치 머리
    d.polygon([(11, 6), (24, 2), (26, 6), (13, 10)], fill=STONE_L)
    d.polygon([(17, 18), (30, 13), (29, 16), (18, 20)], fill=STONE_D)
    d.line([(16, 8), (20, 12), (18, 15)], fill=CRACK)
    d.rectangle((22, 8, 24, 10), fill=RUNE)  # 진동 룬
    d.point((23, 9), fill=RUNE_L)
    d.rectangle((12, 4, 16, 5), fill=MOSS)
    for c in range(3):  # 충격 선
        d.point((3 + c * 2, 12 - c), fill=RUNE_L)
    save("QuakestoneMaul", im)


def monolith_shield():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(8, 3), (24, 3), (28, 9), (26, 26), (16, 31), (6, 26), (4, 9)], fill=STONE)  # 비석 방패
    d.polygon([(8, 3), (24, 3), (26, 6), (6, 6)], fill=STONE_L)
    d.line([(16, 8), (16, 26)], fill=CRACK)
    for (x, y) in ((11, 12), (21, 12), (16, 20)):  # 새긴 룬
        d.rectangle((x - 1, y - 1, x + 1, y + 1), fill=RUNE)
    d.point((16, 20), fill=RUNE_L)
    d.rectangle((6, 3, 14, 4), fill=MOSS)
    d.point((9, 2), fill=MOSS_L)
    save("MonolithShield", im)


def gatekeepers_oath():
    im = canvas(); d = ImageDraw.Draw(im)
    a = math.radians(-45)
    ux, uy = math.cos(a), math.sin(a)
    hx, hy = 5, 27
    tx, ty = hx + ux * 34, hy + uy * 34
    nx, ny = -uy, ux
    d.polygon([(hx + ux * 7 - nx * 3, hy + uy * 7 - ny * 3), (hx + ux * 7 + nx * 3, hy + uy * 7 + ny * 3), (tx + nx, ty + ny), (tx - nx, ty - ny)], fill=STEEL)  # 대검 날
    d.line([(hx + ux * 8, hy + uy * 8), (tx, ty)], fill=RUNE_L)  # 룬 빛 홈
    gx, gy = hx + ux * 7, hy + uy * 7
    d.line([(gx - nx * 6, gy - ny * 6), (gx + nx * 6, gy + ny * 6)], fill=GOLD, width=2)  # 가드
    d.line([(hx, hy), (gx, gy)], fill=(70, 52, 40), width=2)
    d.ellipse((hx - 3, hy - 1, hx + 1, hy + 3), fill=GOLD)  # 폼멜 — 브로치와 같은 문장
    d.point((hx - 1, hy + 1), fill=RUNE_L)
    d.ellipse((gx - 2, gy - 2, gx + 2, gy + 2), fill=(70, 120, 150))
    d.point((round(gx), round(gy)), fill=RUNE_L)
    save("GatekeepersOath", im)


def golem_pebble():
    im = canvas(); d = ImageDraw.Draw(im)
    for (x, y, r) in ((11, 18, 6), (21, 14, 5), (18, 23, 4)):
        d.ellipse((x - r, y - r, x + r, y + r), fill=STONE)
        d.point((x - 2, y - 2), fill=STONE_L)
    d.rectangle((8, 12, 12, 13), fill=MOSS)
    save("Golem Pebble", im)


def quake_core():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(16, 4), (26, 12), (22, 27), (10, 27), (6, 12)], fill=STONE_D)
    d.polygon([(16, 8), (22, 13), (19, 23), (13, 23), (10, 13)], fill=RUNE)
    d.polygon([(16, 10), (19, 14), (16, 20), (13, 14)], fill=RUNE_L)
    d.line([(6, 12), (10, 13)], fill=CRACK)
    save("Quake Core", im)


def monolith_shard():
    im = canvas(); d = ImageDraw.Draw(im)
    d.polygon([(12, 3), (20, 5), (23, 28), (9, 28)], fill=STONE)
    d.polygon([(12, 3), (20, 5), (19, 9), (12, 8)], fill=STONE_L)
    d.rectangle((14, 14, 16, 16), fill=RUNE)
    d.line([(13, 20), (18, 24)], fill=CRACK)
    d.rectangle((9, 26, 14, 27), fill=MOSS)
    save("Monolith Shard", im)


def ruin_mote():
    im = canvas(); d = ImageDraw.Draw(im)
    for k, a in ((11, 70), (8, 130)):
        d.ellipse((16 - k, 16 - k, 16 + k, 16 + k), fill=RUNE + (a,))
    d.polygon([(16, 9), (21, 16), (16, 23), (11, 16)], fill=STONE)
    d.polygon([(16, 9), (21, 16), (16, 16)], fill=STONE_L)
    d.line([(15, 14), (17, 18)], fill=RUNE_L)
    save("Ruin Mote", im)


if __name__ == "__main__":
    pebble_gauntlet(); quakestone_maul(); monolith_shield(); gatekeepers_oath()
    golem_pebble(); quake_core(); monolith_shard(); ruin_mote()
