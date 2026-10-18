"""1층계 ④구역 '늪지' 거대 슬라임·모기 공격 판정 코드 도트 (B-124, TowerWorld 14절).
- Assets/Art/Monsters/GiantSlime/SlimeSlam.png (64px × 6, 내려찍기 점액 기둥이 솟았다 퍼진다 — 구 갈퀴 ClowAttack 자리)
- Assets/Art/Monsters/GiantSlime/GelShot.png   (32px × 3, 날아가는 점액 덩이 — 구 불꽃 PassiveTerrorAttackPrefab 자리)
- Assets/Art/Monsters/SwampMosquito/MosquitoBite.png (32px × 3, 돌진 물기 침 궤적 — 구 FlowerMole 유도탄 자리)
팔레트는 gen_swamp_mobs.py 의 GIANT·모기 색을 따른다. 사용: python Tools/pixelfx/gen_swamp_boss_fx.py"""
import math
import os
from PIL import Image, ImageDraw

HERE = os.path.dirname(__file__)
ART = os.path.join(HERE, "..", "..", "Assets", "Art", "Monsters")

# GIANT 팔레트(gen_swamp_mobs.py) + 점액 하이라이트
G_BASE, G_LIGHT, G_DARK, G_CORE, G_OUT = (84, 120, 70), (128, 162, 96), (48, 74, 42), (196, 214, 120), (16, 26, 14)
G_SHINE = (230, 244, 190)
MUD = (62, 52, 38)
# 모기: 핏빛 침
MQ_RED, MQ_RED_L, MQ_DARK = (190, 60, 60), (240, 120, 110), (90, 30, 34)


def save(im, *parts):
    path = os.path.join(ART, *parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    im.save(path)
    print("ok", path)


def blob(d, cx, cy, rx, ry, outline=True):
    """테두리 있는 점액 덩이 한 개: 어두운 아래·밝은 위·반짝임"""
    if rx < 1 or ry < 1:
        return
    if outline:
        d.ellipse((cx - rx - 1, cy - ry - 1, cx + rx + 1, cy + ry + 1), fill=G_OUT + (255,))
    d.ellipse((cx - rx, cy - ry, cx + rx, cy + ry), fill=G_BASE + (255,))
    d.ellipse((cx - rx, cy, cx + rx, cy + ry), fill=G_DARK + (255,))
    d.ellipse((cx - rx + 1, cy - ry + 1, cx + rx - 1, cy + ry * 0.2), fill=G_LIGHT + (255,))
    d.point((cx - rx * 0.4, cy - ry * 0.5), fill=G_SHINE + (255,))


def slime_slam():
    """보스 근접: 앞 땅에 점액이 내려찍혀 기둥처럼 솟았다가 옆으로 퍼지며 꺼진다. 64px × 6프레임, 오른쪽이 앞"""
    n, fs = 6, 64
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    rise = [0.25, 0.7, 1.0, 0.85, 0.5, 0.2]   # 기둥 높이 비율
    spread = [0.3, 0.55, 0.8, 1.0, 1.0, 0.9]  # 바닥 웅덩이 폭 비율
    for f in range(n):
        ox = f * fs; base = fs - 4; cx = ox + 34
        # 바닥 웅덩이(흙탕 + 점액)
        w = round(28 * spread[f])
        d.ellipse((cx - w - 1, base - 3, cx + w + 1, base + 3), fill=G_OUT + (255,))
        d.ellipse((cx - w, base - 2, cx + w, base + 2), fill=G_DARK + (255,))
        d.line([(cx - w + 2, base - 1), (cx + w - 2, base - 1)], fill=G_BASE + (255,))
        # 가운데 기둥: 아래 굵고 위로 갈수록 방울
        h = round(44 * rise[f])
        for k in range(0, h, 6):
            t = k / max(1, h)
            r = max(2, round(9 * (1 - t) + 2))
            blob(d, cx + round(math.sin(k * 0.3 + f) * 1.5), base - 3 - k, r, min(r, 5), outline=(k == 0))
        if h > 8:
            blob(d, cx, base - 3 - h, 4, 4)  # 꼭대기 방울
        # 튀는 점액 방울(퍼질 때)
        if f >= 2:
            for (dx, dy, r) in ((-22, -10, 2), (20, -14, 3), (-12, -22, 2), (26, -6, 2)):
                fall = (f - 2) * 4
                bx = cx + dx + (2 if dx > 0 else -2) * (f - 2)
                bx = min(max(bx, ox + r + 2), ox + fs - r - 3)  # 프레임 밖(옆 프레임)으로 번지지 않게
                blob(d, bx, base + dy + fall, r, r)
        # 흙 튐
        if 1 <= f <= 3:
            for (x, y) in ((-26, -4), (24, -5), (-16, -8)):
                d.point((cx + x, base + y - f), fill=MUD + (255,))
    save(im, "GiantSlime", "SlimeSlam.png")


def gel_shot():
    """보스 원거리: 꼬리 끄는 점액 덩이(+x 방향). 32px × 3프레임, 흔들리며 날아간다"""
    n, fs = 3, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    wob = [(8, 6), (9, 5), (7, 7)]
    for f in range(n):
        ox = f * fs; cy = 16
        rx, ry = wob[f]
        # 꼬리 방울
        for i, (tx, tr) in enumerate(((4, 2), (9, 3))):
            d.ellipse((ox + tx - tr, cy - tr + (f - 1), ox + tx + tr, cy + tr + (f - 1)), fill=G_DARK + (255,))
        blob(d, ox + 20, cy, rx, ry)
        d.point((ox + 20, cy), fill=G_CORE + (255,))  # 삼킨 뼈 대신 핵
        d.point((ox + 21, cy + 1), fill=G_CORE + (255,))
    save(im, "GiantSlime", "GelShot.png")


def mosquito_bite():
    """모기 돌진 물기: 앞으로 찌르는 침 궤적 + 핏방울. 32px × 3프레임, 오른쪽이 앞"""
    n, fs = 3, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    reach = [14, 26, 20]
    for f in range(n):
        ox = f * fs; cy = 16
        x1 = ox + 3 + reach[f]
        # 잔상 궤적
        for k in range(3):
            a = 120 - k * 35
            d.line([(ox + 3 + k * 3, cy - 2 + k), (x1 - 6, cy)], fill=MQ_RED_L + (a,))
        d.line([(ox + 4, cy + 1), (x1 - 1, cy + 1)], fill=MQ_DARK + (255,))
        d.line([(ox + 4, cy), (x1, cy)], fill=MQ_DARK + (255,))
        d.line([(ox + 6, cy - 1), (x1 - 2, cy - 1)], fill=MQ_RED + (255,))
        d.point((x1, cy), fill=(255, 230, 230, 255))
        if f >= 1:  # 핏방울
            d.point((x1 - 3, cy + 3), fill=MQ_RED + (255,)); d.point((x1 - 6, cy - 4), fill=MQ_RED_L + (255,))
    save(im, "SwampMosquito", "MosquitoBite.png")


if __name__ == "__main__":
    slime_slam(); gel_shot(); mosquito_bite()
