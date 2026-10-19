"""실루엣이 어색했던 재료 아이콘 7종을 어비스 유물풍으로 다시 그린다 (ArtStyle.md 머리말·평면 아이콘 음영 절).
Round Bone · Small Bone · Ancient Bone · Goblin Fang · Death Sigil · Abyssal Core · Ominous Black Liquid
상아 뼈는 라벤더 그림자, 유물은 청록·라벤더 생물발광 포인트. 32 논리 px × 8배 = 256 PNG.
사용: python Tools/pixelfx/gen_abyss_relic_icons.py   (Tools/pixelfx 에서 gen_forest_items 를 가져온다)"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
from PIL import ImageDraw  # noqa: E402

from gen_forest_items import canvas, save  # noqa: E402

BONE_HI, BONE, BONE_MID, BONE_SH = (244, 236, 214), (218, 206, 178), (184, 170, 150), (132, 120, 140)
ROOT = (122, 86, 70)
STONE_HI, STONE, STONE_SH = (96, 92, 112), (64, 60, 80), (40, 36, 54)
GLOW_HI, GLOW, GLOW_D = (220, 255, 250), (120, 232, 220), (60, 150, 160)
LAV_HI, LAV, LAV_D = (236, 214, 255), (170, 140, 220), (98, 76, 150)
MOSS = (96, 128, 88)


def capsule(d, p0, p1, r, fill):
    """두 점 사이 둥근 막대(뼈 몸통)."""
    n = max(1, int(math.dist(p0, p1) * 2))
    for i in range(n + 1):
        t = i / n
        x, y = p0[0] + (p1[0] - p0[0]) * t, p0[1] + (p1[1] - p0[1]) * t
        d.ellipse((x - r, y - r, x + r, y + r), fill=fill)


def bone(d, p0, p1, shaft, knob):
    """좌상단 광원: 그림자 → 기본 → 밝음 순으로 1px 어긋나게 겹친다."""
    for (dx, dy, col) in ((1, 1, BONE_SH), (0, 0, BONE), (-0.6, -0.6, BONE_HI)):
        r_s = shaft - (0.9 if col == BONE_HI else 0)
        r_k = knob - (1.2 if col == BONE_HI else 0)
        a = (p0[0] + dx, p0[1] + dy); b = (p1[0] + dx, p1[1] + dy)
        capsule(d, a, b, max(r_s, 0.5), col)
        ang = math.atan2(b[1] - a[1], b[0] - a[0]) + math.pi / 2
        ox, oy = math.cos(ang) * knob * 0.6, math.sin(ang) * knob * 0.6
        for (cx, cy) in (a, b):
            for s in (-1, 1):
                k = r_k * 0.75
                d.ellipse((cx + s * ox - k, cy + s * oy - k, cx + s * ox + k, cy + s * oy + k), fill=col)


def round_bone():
    im = canvas(); d = ImageDraw.Draw(im)
    bone(d, (8, 23), (23, 8), 2.2, 4.2)
    d.line([(12, 18), (17, 13)], fill=BONE_MID)  # 결
    save("Round Bone", im)


def small_bone():
    im = canvas(); d = ImageDraw.Draw(im)
    bone(d, (5, 15), (14, 6), 1.5, 2.8)
    bone(d, (15, 27), (26, 16), 1.5, 2.8)
    save("Small Bone", im)


def ancient_bone():
    im = canvas(); d = ImageDraw.Draw(im)
    bone(d, (6, 24), (25, 7), 2.6, 4.6)
    # 오래된 유물: 이끼 얼룩과 청록 룬 균열
    for (x, y) in ((9, 21), (10, 22), (11, 20), (21, 9), (22, 10)):
        d.point((x, y), fill=MOSS)
    d.line([(13, 17), (15, 16), (16, 14), (18, 13)], fill=GLOW_D)
    d.point((15, 16), fill=GLOW); d.point((16, 14), fill=GLOW_HI)
    save("Ancient Bone", im)


def goblin_fang():
    im = canvas(); d = ImageDraw.Draw(im)
    # 휘어진 송곳니: 뿌리(위) → 끝(아래 왼쪽)
    d.polygon([(14, 4), (22, 5), (21, 12), (17, 21), (11, 28), (12, 19), (13, 11)], fill=BONE_SH)
    d.polygon([(14, 4), (21, 5), (20, 12), (16, 20), (11, 27), (12, 18), (13, 11)], fill=BONE)
    d.line([(15, 8), (14, 14), (13, 20)], fill=BONE_HI)
    d.polygon([(14, 3), (22, 4), (21, 8), (14, 8)], fill=ROOT)
    d.line([(15, 5), (20, 5)], fill=(156, 112, 92))
    save("Goblin Fang", im)


def death_sigil():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((4, 4, 28, 28), fill=STONE_SH)
    d.ellipse((4, 4, 27, 27), fill=STONE)
    d.arc((6, 6, 25, 25), 200, 290, fill=STONE_HI)
    # 라벤더 빛의 눈 문양 + 테두리 룬 점
    d.polygon([(9, 16), (16, 11), (23, 16), (16, 21)], fill=LAV_D)
    d.polygon([(11, 16), (16, 12), (21, 16), (16, 20)], fill=LAV)
    d.ellipse((14, 14, 18, 18), fill=STONE_SH)
    d.point((16, 15), fill=LAV_HI)
    for a in range(0, 360, 45):
        x = 16 + 10 * math.cos(math.radians(a)); y = 16 + 10 * math.sin(math.radians(a))
        d.point((round(x), round(y)), fill=LAV)
    save("Death Sigil", im)


def abyssal_core():
    im = canvas(); d = ImageDraw.Draw(im)
    d.ellipse((6, 6, 26, 26), fill=(30, 28, 56))
    d.ellipse((7, 7, 24, 24), fill=(44, 42, 82))
    # 안쪽에서 빛나는 청록 소용돌이
    for i in range(26):
        t = i / 25
        r = 1 + t * 7
        a = t * 4.2 * math.pi
        x, y = 16 + r * math.cos(a), 16 + r * math.sin(a)
        d.point((round(x), round(y)), fill=GLOW if t < 0.6 else GLOW_D)
    d.ellipse((14, 14, 18, 18), fill=GLOW)
    d.point((15, 15), fill=GLOW_HI)
    d.arc((8, 8, 22, 22), 200, 260, fill=(110, 106, 160))  # 유리 반사
    for (x, y) in ((4, 12), (27, 20), (21, 4)):  # 떠도는 입자
        d.point((x, y), fill=GLOW)
    save("Abyssal Core", im)


def ominous_black_liquid():
    im = canvas(); d = ImageDraw.Draw(im)
    glass, glass_hi = (150, 160, 182), (210, 220, 236)
    d.rectangle((13, 3, 18, 6), fill=ROOT)  # 코르크
    d.rectangle((14, 7, 17, 10), fill=glass)  # 목
    d.ellipse((7, 10, 24, 28), fill=glass)
    d.ellipse((8, 14, 23, 27), fill=(22, 16, 34))  # 검보라 액체
    d.ellipse((9, 15, 21, 22), fill=(46, 30, 66))
    for (x, y, c) in ((12, 20, LAV), (17, 18, LAV_HI), (15, 23, LAV_D), (19, 22, LAV)):  # 떠오르는 빛 거품
        d.point((x, y), fill=c)
    d.line([(10, 13), (9, 18)], fill=glass_hi)
    save("Ominous Black Liquid", im)


def main():
    for f in (round_bone, small_bone, ancient_bone, goblin_fang, death_sigil, abyssal_core, ominous_black_liquid):
        f()


if __name__ == "__main__":
    main()
