"""2층계 ②구역 '무너진 성벽' 몬스터 도트 (Floor2Plan 5.1, TowerWorld 17절).
낮: 성벽 기사(방패 밀치며 내려베기) · 미늘창 파수병(미늘창 휩쓸기) · 가고일(돌 깃털 사격) / 밤: 가고일 + 구름 정령(번개 불꽃) / 보스: 가고일 군주.
보유 에셋에 기사·석상이 없어 코드로 그린다(SPUM 금지, 도트 필수). 오른쪽을 본다, 발은 프레임 바닥에서 4px 위.
프레임: 기사·파수병 64px · 가고일·정령 48px · 가고일 군주 128px(보스 공통 18px/유닛).
출력: Assets/Art/Monsters/<Name>/<Anim>.png (가로 시트) + 보스 판정 GargoyleTalon(64px×6)·GargoyleShard(32px×3) + 파수병 판정 HalberdCrescent(32px×5)
+ 투사체 GargoyleFeather·CloudSpark(32px×1). Tools/probes/build_rampart_mobs.cs 가 슬라이스·프리팹화한다.
기법은 gen_outskirts_mobs.py 와 같다(몸색 칠 → 위 밝게/아래 어둡게 → 1px 외곽선)."""
import math
import os

from PIL import Image, ImageDraw

from gen_forest_mobs import shade, outline
from gen_deep_forest_mobs import frame, ph_of, anim_frames, death_frames, hit_frames, write

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")

OUTLINE = (30, 32, 44)
# 기사 갑옷(은청) · 금 테 · 하늘색 겉옷
ARM, ARM_L, ARM_D = (150, 162, 184), (206, 216, 234), (92, 100, 124)
GOLD, GOLD_L = (206, 170, 84), (244, 214, 132)
TAB, TAB_D = (86, 134, 196), (56, 90, 146)
STEEL, STEEL_L = (190, 200, 216), (240, 246, 252)
WOOD = (110, 80, 54)
# 석상(푸른 회색 돌) · 눈빛
GAR, GAR_L, GAR_D = (118, 124, 140), (164, 170, 186), (74, 78, 94)
EYE, EYE_L = (120, 200, 255), (220, 246, 255)
CRACK = (58, 60, 74)
MOSS = (120, 150, 96)
# 구름 · 번개
CLOUD, CLOUD_L, CLOUD_D = (226, 232, 244), (250, 252, 255), (168, 178, 204)
SPARK, SPARK_L = (255, 226, 96), (255, 252, 214)
DUST = (214, 210, 196)


def line_at(d, x, y, ang, ln, col, w=1):
    a = math.radians(ang)
    d.line([(x, y), (x + math.cos(a) * ln, y + math.sin(a) * ln)], fill=col, width=w)
    return x + math.cos(a) * ln, y + math.sin(a) * ln


def sword(d, hx, hy, ang, ln=20, w=3):
    """칼자루(hx, hy)에서 ang(도, 0 = 오른쪽, 90 = 아래) 방향 한손검"""
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    nx, ny = -uy, ux
    gx, gy = hx + ux * 3, hy + uy * 3
    tx, ty = hx + ux * ln, hy + uy * ln
    d.line([(hx - ux * 2, hy - uy * 2), (gx, gy)], fill=WOOD, width=2)
    d.line([(gx - nx * 3, gy - ny * 3), (gx + nx * 3, gy + ny * 3)], fill=GOLD, width=2)
    hw = w / 2
    d.polygon([(gx - nx * hw, gy - ny * hw), (gx + nx * hw, gy + ny * hw), (tx, ty)], fill=STEEL)
    d.line([(gx, gy), (tx, ty)], fill=STEEL_L)


# ───────────────── 성벽 기사 (64px): 방패 + 한손검 ─────────────────
def knight(pose, t):
    F, G = 64, 60
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, step = 0, 0, 0.0
    sw = (38, 34, -60, 20)  # 칼자루 x, y, 각도, 길이 — 기본: 어깨에 기대 세움
    shield_dx = 0
    fx = 0
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        step = math.sin(t * math.tau)
        bob = -1 if ph_of(t, 6) in (0, 3) else 0
        sw = (38, 36, -30, 20)
    elif pose == "attack":  # 방패를 앞으로 밀치고 → 검을 치켜들었다가 → 내려벤다
        n = ph_of(t, 6)
        shield_dx = [2, 4, 2, 0, 0, 0][n]
        sw = [(36, 28, -100, 20), (34, 22, -130, 20), (36, 20, -160, 20), (42, 30, 20, 22), (44, 36, 60, 22), (42, 38, 80, 20)][n]
        lunge = [1, 2, 0, 3, 5, 3][n]
        fx = [0, 0, 0, 1, 2, 0][n]
    elif pose == "hit":
        lunge = -2
        sw = (36, 34, -80, 20)
    bx, by = 28 + lunge, 0 + bob
    # 다리(그리브) — 뒤 다리 어둡게
    for i, lx in enumerate((bx - 6, bx + 2)):
        off = round(step * 3) * (1 if i else -1)
        d.rectangle((lx + off, by + 46, lx + off + 4, G - 2), fill=ARM_D if i == 0 else ARM)
        d.rectangle((lx + off - 1, G - 2, lx + off + 6, G), fill=ARM_D)
    # 겉옷(하늘색 태바드) + 흉갑
    d.polygon([(bx - 8, by + 26), (bx + 8, by + 26), (bx + 9, by + 47), (bx - 9, by + 47)], fill=TAB)
    d.rectangle((bx - 7, by + 26, bx + 7, by + 34), fill=ARM)
    # 뒷팔
    d.rectangle((bx - 11, by + 27, bx - 8, by + 38), fill=ARM_D)
    # 투구 + 깃털 장식
    d.rectangle((bx - 6, by + 14, bx + 6, by + 26), fill=ARM)
    d.polygon([(bx - 6, by + 14), (bx - 2, by + 6), (bx + 2, by + 6), (bx + 6, by + 14)], fill=ARM)
    d.polygon([(bx - 4, by + 7), (bx - 12, by + 4), (bx - 14, by + 10), (bx - 6, by + 12)], fill=TAB)
    shade(im, ARM_L, ARM_D, ARM)
    shade(im, (120, 170, 226), TAB_D, TAB)
    d.rectangle((bx - 1, by + 18, bx + 6, by + 19), fill=(24, 26, 36))  # 바이저
    d.rectangle((bx + 2, by + 18, bx + 5, by + 18), fill=EYE_L)
    d.line([(bx - 8, by + 34), (bx + 8, by + 34)], fill=GOLD)  # 허리띠
    d.line([(bx, by + 36), (bx, by + 46)], fill=GOLD)  # 겉옷 문장선
    d.point((bx, by + 40), fill=GOLD_L)
    # 앞팔 + 검
    hx, hy, ang, ln = sw
    hx += lunge
    hy += bob
    d.polygon([(bx + 4, by + 27), (bx + 8, by + 27), (hx + 1, hy + 1), (hx - 2, hy - 1)], fill=ARM)
    sword(d, hx, hy, ang, ln)
    # 방패(앞, 연 모양) — 금 테 + 하늘색 문장
    sx, sy = bx + 9 + shield_dx, by + 30
    d.polygon([(sx - 3, sy - 8), (sx + 6, sy - 8), (sx + 6, sy + 4), (sx + 1, sy + 11), (sx - 3, sy + 4)], fill=STEEL)
    d.polygon([(sx - 1, sy - 6), (sx + 4, sy - 6), (sx + 4, sy + 3), (sx + 1, sy + 7), (sx - 1, sy + 3)], fill=TAB)
    d.line([(sx + 1, sy - 5), (sx + 1, sy + 5)], fill=GOLD_L)
    d.line([(sx - 1, sy - 2), (sx + 3, sy - 2)], fill=GOLD_L)
    outline(im, OUTLINE)
    if fx:  # 내려베기 섬광
        for k in range(4 + fx * 2):
            a = math.radians(-40 + k * (110 / (3 + fx * 2)))
            r = 16 + fx * 2
            d.point((min(F - 1, hx + round(math.cos(a) * r)), min(G, hy + round(math.sin(a) * r))), fill=STEEL_L + (220,))
    return im


# ───────────────── 미늘창 파수병 (64px): 무거운 갑옷 + 미늘창 ─────────────────
def halberd(d, x, y, ang, ln=34):
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    nx, ny = -uy, ux
    tx, ty = x + ux * ln, y + uy * ln
    d.line([(x - ux * 8, y - uy * 8), (tx, ty)], fill=WOOD, width=2)
    # 도끼날(초승달) + 창끝
    bx, by = x + ux * (ln - 6), y + uy * (ln - 6)
    d.polygon([(bx - nx * 1, by - ny * 1), (bx + nx * 7 - ux * 3, by + ny * 7 - uy * 3), (bx + nx * 8 + ux * 3, by + ny * 8 + uy * 3), (bx + ux * 4, by + uy * 4)], fill=STEEL)
    d.polygon([(tx - nx * 2, ty - ny * 2), (tx + nx * 2, ty + ny * 2), (tx + ux * 7, ty + uy * 7)], fill=STEEL_L)
    d.polygon([(bx - nx * 1, by - ny * 1), (bx - nx * 4, by - ny * 4), (bx - nx * 2 + ux * 2, by - ny * 2 + uy * 2)], fill=STEEL)


def sentry(pose, t):
    F, G = 64, 60
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, step = 0, 0, 0.0
    hb = (40, 30, -84, 34)  # 손 x, y, 각도, 길이 — 기본: 세워 든 미늘창
    fx = 0
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        step = math.sin(t * math.tau)
        bob = 1 if ph_of(t, 6) in (1, 4) else 0
        hb = (40, 32, -60, 34)
    elif pose == "attack":  # 머리 위로 들었다가 앞으로 크게 휩쓸어 내린다
        n = ph_of(t, 6)
        hb = [(36, 26, -110, 34), (32, 20, -140, 34), (32, 18, -170, 34), (42, 30, -10, 36), (44, 36, 20, 36), (42, 36, 30, 34)][n]
        lunge = [0, -1, -2, 3, 5, 3][n]
        fx = [0, 0, 0, 1, 2, 1][n]
    elif pose == "hit":
        lunge = -2
    bx, by = 27 + lunge, 0 + bob
    for i, lx in enumerate((bx - 8, bx + 2)):
        off = round(step * 2) * (1 if i else -1)
        d.rectangle((lx + off, by + 44, lx + off + 6, G - 2), fill=ARM_D if i == 0 else ARM)
        d.rectangle((lx + off - 1, G - 2, lx + off + 8, G), fill=ARM_D)
    # 두꺼운 흉갑 + 사슬 치마
    d.polygon([(bx - 11, by + 22), (bx + 11, by + 22), (bx + 12, by + 40), (bx - 12, by + 40)], fill=ARM)
    d.rectangle((bx - 11, by + 40, bx + 11, by + 46), fill=GAR)
    # 큰 견갑
    d.ellipse((bx - 16, by + 18, bx - 4, by + 30), fill=ARM)
    d.ellipse((bx + 3, by + 18, bx + 15, by + 30), fill=ARM)
    # 투구(바렐 헬름)
    d.rectangle((bx - 6, by + 8, bx + 7, by + 22), fill=ARM)
    d.rectangle((bx - 7, by + 6, bx + 8, by + 8), fill=ARM)
    shade(im, ARM_L, ARM_D, ARM)
    shade(im, GAR_L, GAR_D, GAR)
    for k in range(4):  # 사슬 무늬
        d.line([(bx - 10 + k * 6, by + 41), (bx - 8 + k * 6, by + 45)], fill=GAR_D)
    d.rectangle((bx - 5, by + 13, bx + 7, by + 14), fill=(24, 26, 36))  # 눈 틈
    d.rectangle((bx + 2, by + 13, bx + 6, by + 13), fill=EYE_L)
    for k in range(3):
        d.point((bx + 1 + k * 2, by + 17), fill=(24, 26, 36))  # 숨구멍
    d.line([(bx - 11, by + 22), (bx + 11, by + 22)], fill=GOLD)
    d.rectangle((bx - 3, by + 28, bx + 3, by + 34), fill=TAB)  # 가슴 문장
    d.point((bx, by + 31), fill=GOLD_L)
    # 앞팔 + 미늘창
    hx, hy, ang, ln = hb
    hx += lunge
    hy += bob
    d.polygon([(bx + 6, by + 24), (bx + 11, by + 25), (hx + 2, hy + 2), (hx - 2, hy)], fill=ARM_D)
    halberd(d, hx, hy, ang, ln)
    d.ellipse((hx - 2, hy - 2, hx + 2, hy + 2), fill=ARM_D)
    outline(im, OUTLINE)
    if fx:
        for k in range(3 + fx * 2):
            x = 40 + k * 3
            d.point((min(F - 1, x + lunge), G - (k % 3)), fill=DUST + (220,))
    return im


# ───────────────── 가고일 (48px): 날개 달린 작은 석상 ─────────────────
def wing(d, rx, ry, spread, col, flip=False):
    """어깨(rx, ry)에서 뒤(왼쪽)로 펼친 박쥐 날개. spread 0(접힘)~1(활짝)"""
    s = -1 if not flip else 1
    tipx, tipy = rx + s * round(6 + 12 * spread), ry - round(4 + 8 * spread)
    midx, midy = rx + s * round(4 + 8 * spread), ry + round(6 - 4 * spread)
    lowx, lowy = rx + s * round(2 + 5 * spread), ry + 10
    d.polygon([(rx, ry), (tipx, tipy), (midx, midy), (lowx, lowy), (rx + s * -1, ry + 8)], fill=col)
    d.line([(rx, ry), (tipx, tipy)], fill=GAR_D)


def gargoyle(pose, t):
    F, G = 48, 44
    im = frame(F)
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    hover = round(math.sin(ph) * 2)
    spread, lunge, mouth = 0.5 + 0.5 * math.sin(ph * 2), 0, False
    if pose == "run":
        spread = 0.5 + 0.5 * math.sin(ph * 3)
    elif pose == "attack":  # 날개를 활짝 펴고 몸을 젖혔다가 돌 깃털을 뿌린다
        n = ph_of(t, 6)
        spread = [0.6, 1.0, 1.0, 0.3, 0.2, 0.5][n]
        lunge = [0, -2, -3, 2, 3, 1][n]
        mouth = n in (3, 4)
    elif pose == "hit":
        lunge, spread = -2, 0.2
    cx, cy = 22 + lunge, 24 + hover
    wing(d, cx - 2, cy - 4, spread, GAR_D)  # 먼 날개
    # 몸(웅크린 몸통) · 다리 · 꼬리
    d.ellipse((cx - 7, cy - 6, cx + 6, cy + 8), fill=GAR)
    d.rectangle((cx - 5, cy + 6, cx - 2, cy + 12), fill=GAR)
    d.rectangle((cx + 1, cy + 6, cx + 4, cy + 12), fill=GAR)
    d.line([(cx - 6, cy + 6), (cx - 13, cy + 10), (cx - 15, cy + 7)], fill=GAR_D, width=2)
    d.polygon([(cx - 15, cy + 7), (cx - 18, cy + 6), (cx - 16, cy + 9)], fill=GAR_D)
    # 머리 + 뿔
    d.ellipse((cx + 1, cy - 14, cx + 13, cy - 3), fill=GAR)
    d.polygon([(cx + 3, cy - 12), (cx + 1, cy - 19), (cx + 6, cy - 13)], fill=GAR_L)
    d.polygon([(cx + 8, cy - 13), (cx + 10, cy - 20), (cx + 11, cy - 12)], fill=GAR_L)
    # 앞팔(발톱)
    d.polygon([(cx + 3, cy - 1), (cx + 9, cy + 2), (cx + 10, cy + 6), (cx + 6, cy + 5)], fill=GAR)
    shade(im, GAR_L, GAR_D, GAR)
    wing(d, cx, cy - 5, min(1.0, spread + 0.1), GAR)  # 가까운 날개
    d.line([(cx - 3, cy - 1), (cx, cy + 3), (cx - 2, cy + 6)], fill=CRACK)
    d.rectangle((cx + 8, cy - 10, cx + 10, cy - 9), fill=EYE_L)  # 눈
    d.point((cx + 7, cy - 10), fill=EYE)
    if mouth:
        d.rectangle((cx + 9, cy - 6, cx + 12, cy - 5), fill=(30, 30, 40))
    for k in range(2):
        d.point((cx + 10 + k, cy + 6), fill=GAR_L)  # 발톱 끝
    d.rectangle((cx - 4, cy - 5, cx - 2, cy - 5), fill=MOSS)
    outline(im, OUTLINE)
    return im


# ───────────────── 구름 정령 (48px): 번개 품은 뭉게구름 ─────────────────
def cloud_wisp(pose, t):
    F = 48
    im = frame(F)
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    cx, cy = 24, 24 + round(math.sin(ph) * 2)
    charge = 0
    if pose == "attack":
        n = ph_of(t, 6)
        charge = [0, 1, 2, 3, 2, 0][n]
        cx += [0, -1, -1, 2, 1, 0][n]
    elif pose == "hit":
        cx -= 2
    puff = 1 if ph_of(t, 4) in (1, 2) else 0
    for (dx, dy, r) in ((-8, 2, 7 + puff), (0, -3, 9), (8, 1, 7), (3, 5, 7), (-4, 6, 6)):
        d.ellipse((cx + dx - r, cy + dy - r, cx + dx + r, cy + dy + r), fill=CLOUD)
    shade(im, CLOUD_L, CLOUD_D, CLOUD)
    # 눈(작고 동그란) + 볼
    d.rectangle((cx + 2, cy - 1, cx + 3, cy + 1), fill=(40, 46, 70))
    d.rectangle((cx + 7, cy - 1, cx + 8, cy + 1), fill=(40, 46, 70))
    d.point((cx + 2, cy - 1), fill=CLOUD_L)
    d.point((cx + 7, cy - 1), fill=CLOUD_L)
    # 아래로 떨어지는 빗방울/번개
    if charge >= 1:
        x0 = cx + 2
        pts = [(x0, cy + 10), (x0 - 2, cy + 14), (x0 + 1, cy + 14), (x0 - 1, cy + 18 + charge)]
        d.line(pts, fill=SPARK, width=1)
    for k in range(2 + charge * 2):  # 몸에 튀는 불꽃
        a = ph * 2 + k * math.tau / (2 + charge * 2)
        x, y = cx + round(math.cos(a) * (11 + charge)), cy + round(math.sin(a) * (8 + charge))
        d.point((x, y), fill=SPARK_L if k % 2 else SPARK)
    outline(im, (110, 118, 150))
    return im


# ───────────────── 가고일 군주 (128px): 거대한 날개 석상 ─────────────────
def lord(pose, t):
    F, G = 128, 124
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, step, spread = 0, 0, 0.0, 0.25
    claw = (88, 74)  # 앞발 끝
    fx = 0
    roar = False
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
        spread = 0.25 + (0.05 if ph_of(t, 4) in (1, 2) else 0)
    elif pose == "run":
        step = math.sin(t * math.tau)
        bob = -2 if ph_of(t, 6) in (0, 3) else 0
        spread = 0.4 + 0.3 * abs(math.sin(t * math.tau))
    elif pose == "attack":  # 앞발을 치켜들었다가 크게 할퀸다
        n = ph_of(t, 6)
        claw = [(80, 60), (76, 40), (78, 34), (100, 70), (110, 92), (102, 96)][n]
        lunge = [0, -2, -3, 4, 7, 4][n]
        spread = [0.3, 0.6, 0.7, 0.4, 0.3, 0.3][n]
        fx = [0, 0, 0, 1, 2, 3][n]
    elif pose == "attack2":  # 날개를 활짝 펴고 포효 → 돌 깃털 폭풍 + 석상 깨우기
        n = ph_of(t, 8)
        spread = [0.3, 0.6, 0.9, 1.0, 1.0, 1.0, 0.8, 0.4][n]
        bob = [0, -2, -4, -5, -5, -4, -2, 0][n]
        roar = 3 <= n <= 5
        fx = 10 + n if 3 <= n <= 6 else 0
    elif pose == "hit":
        lunge = -3
    bx, by = 54 + lunge, 0 + bob
    # 먼 날개(뒤)
    s = spread
    tip = (bx - 18 - round(40 * s), by + 40 - round(36 * s))
    d.polygon([(bx - 6, by + 52), tip, (bx - 14 - round(30 * s), by + 62 - round(10 * s)), (bx - 10 - round(22 * s), by + 74), (bx - 4, by + 72)], fill=GAR_D)
    # 뒷다리(웅크림)
    for i, lx in enumerate((bx - 16, bx + 4)):
        off = round(step * 4) * (1 if i else -1)
        col = GAR_D if i == 0 else GAR
        d.ellipse((lx + off - 4, by + 86, lx + off + 16, by + 110), fill=col)
        d.rectangle((lx + off, by + 104, lx + off + 10, G - 2), fill=col)
        d.rectangle((lx + off - 2, G - 3, lx + off + 14, G), fill=col)
        for k in range(3):
            d.point((lx + off + 12 + k, G - 1), fill=GAR_L)
    # 꼬리
    d.line([(bx - 18, by + 100), (bx - 34, by + 110), (bx - 44, by + 102), (bx - 50, by + 106)], fill=GAR, width=4)
    d.polygon([(bx - 50, by + 100), (bx - 58, by + 104), (bx - 50, by + 110)], fill=GAR)
    # 몸통(웅크린 가슴)
    d.ellipse((bx - 22, by + 50, bx + 24, by + 102), fill=GAR)
    # 머리 + 뿔 + 갈기 바위
    hx, hy = bx + 22, by + 40
    d.ellipse((hx - 14, hy - 14, hx + 14, hy + 12), fill=GAR)
    d.polygon([(hx + 6, hy + 2), (hx + 22, hy + 4), (hx + 20, hy + 12), (hx + 6, hy + 12)], fill=GAR)  # 주둥이
    d.polygon([(hx - 8, hy - 10), (hx - 20, hy - 34), (hx - 2, hy - 14)], fill=GAR_L)  # 뿔
    d.polygon([(hx + 2, hy - 13), (hx + 4, hy - 34), (hx + 10, hy - 12)], fill=GAR_L)
    d.polygon([(hx - 14, hy - 6), (hx - 24, hy - 2), (hx - 16, hy + 6)], fill=GAR)  # 뺨 가시
    # 앞팔 + 발톱
    cx, cy = claw[0] + lunge, claw[1] + bob
    d.polygon([(bx + 10, by + 60), (bx + 22, by + 58), (cx + 4, cy - 2), (cx - 4, cy + 4)], fill=GAR)
    d.ellipse((cx - 7, cy - 6, cx + 7, cy + 6), fill=GAR)
    shade(im, GAR_L, GAR_D, GAR)
    for k in range(3):  # 발톱
        a = math.radians(20 + k * 30)
        line_at(d, cx + 4, cy, math.degrees(a), 7, (230, 234, 244), 2)
    # 가까운 날개(앞)
    tip2 = (bx - 6 - round(30 * s), by + 30 - round(30 * s))
    d.polygon([(bx + 4, by + 54), tip2, (bx - 6 - round(22 * s), by + 60 - round(6 * s)), (bx - 2 - round(16 * s), by + 76), (bx + 6, by + 72)], fill=GAR_L)
    d.line([(bx + 4, by + 54), tip2], fill=GAR_D, width=2)
    for k in range(3):  # 날개 막 뼈
        fx2 = bx + 4 + (tip2[0] - bx - 4) * (k + 1) / 4
        fy2 = by + 54 + (tip2[1] - by - 54) * (k + 1) / 4
        d.line([(fx2, fy2), (bx - 2 - round(12 * s) + k * 2, by + 72)], fill=GAR)
    # 금·이끼·문장 조각(옛 성의 석상)
    d.line([(bx - 6, by + 64), (bx - 1, by + 74), (bx - 6, by + 86)], fill=CRACK)
    d.line([(bx + 8, by + 80), (bx + 14, by + 90)], fill=CRACK)
    for (x0, y0, w0) in ((hx - 10, hy - 14, 8), (bx - 18, by + 52, 10), (bx + 2, by + 100, 8)):
        d.rectangle((x0, y0, x0 + w0, y0 + 1), fill=MOSS)
    d.ellipse((bx - 4, by + 70, bx + 6, by + 80), fill=GOLD)  # 가슴에 박힌 옛 성 문장
    d.point((bx + 1, by + 75), fill=GOLD_L)
    # 눈 · 입
    d.rectangle((hx + 4, hy - 4, hx + 9, hy - 2), fill=EYE_L)
    d.point((hx + 3, hy - 3), fill=EYE)
    if roar:
        d.polygon([(hx + 8, hy + 6), (hx + 22, hy + 6), (hx + 20, hy + 14), (hx + 8, hy + 12)], fill=(36, 30, 44))
        for k in range(3):
            d.point((hx + 11 + k * 3, hy + 7), fill=(230, 234, 244))
    outline(im, OUTLINE)
    if 1 <= fx <= 3:  # 할퀸 자국(세 줄)
        for k in range(3):
            y = cy - 6 + k * 6
            d.line([(cx + 4, y), (min(F - 1, cx + 10 + fx * 4), y + 4 + fx)], fill=(220, 236, 255, 230 - fx * 40), width=1)
    if fx >= 10:  # 포효 충격 고리 + 흩날리는 돌 깃털
        k = fx - 10
        r = 10 + k * 10
        d.arc((hx + 10 - r, hy + 6 - r, hx + 10 + r, hy + 6 + r), -60, 60, fill=EYE + (220 - k * 40,), width=2)
        for c in range(4 + k):
            x = min(F - 2, hx + 18 + ((c * 13 + k * 7) % 30))
            y = hy - 20 + ((c * 17) % 50)
            d.line([(x, y), (x + 3, y + 1)], fill=GAR_L + (230,))
    return im


def lord_any(pose, t):
    return lord(pose, t)


# ───────────────── 판정·투사체 ─────────────────
def gargoyle_talon():
    """보스 근접(구 갈퀴 ClowAttack 자리): 세 줄 발톱 할퀴기 궤적. 64px × 6프레임(16ppu = 4유닛, 원본과 같은 크기)"""
    n, fs = 6, 64
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    sweep = [(250, 280), (240, 320), (235, 360), (240, 390), (280, 400), (330, 400)]
    alpha = [180, 240, 255, 230, 150, 70]
    for f in range(n):
        ox = f * fs
        a0, a1 = sweep[f]
        for k, off in enumerate((-8, 0, 8)):
            box = (ox + 6 + off // 2, 8 + off, ox + 58 + off // 2, 60 + off)
            d.arc(box, a0, a1, fill=(60, 80, 120) + (alpha[f],), width=4)
            d.arc(box, a0, a1, fill=(214, 232, 255) + (alpha[f],), width=2)
        if f >= 3:
            for c in range(4):  # 돌 부스러기
                x = ox + 34 + c * 6
                y = 52 - ((c * 7 + f * 3) % 10)
                d.rectangle((x, y, x + 1, y + 1), fill=GAR_L + (230 - (f - 3) * 50,))
    path = os.path.join(OUT, "GargoyleLord", "GargoyleTalon.png")
    im.save(path)
    print("ok", path)


def feather(d, x, y, ln=10, col=GAR, light=GAR_L):
    """+x 방향 돌 깃털(끝이 뾰족한 잎 모양)"""
    d.polygon([(x - ln // 2, y), (x - 1, y - 2), (x + ln // 2, y), (x - 1, y + 2)], fill=col)
    d.line([(x - ln // 2, y), (x + ln // 2, y)], fill=light)


def gargoyle_shard():
    """보스 원거리(구 불꽃 자리): 돌 깃털 세 개 + 하늘색 꼬리. 32px × 3프레임(12.5ppu)"""
    n, fs = 3, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    for f in range(n):
        one = Image.new("RGBA", (fs, fs), (0, 0, 0, 0))
        d = ImageDraw.Draw(one)
        for k, (x, y) in enumerate(((20, 10), (24, 16), (20, 22))):
            for c in range(3):
                d.point((x - 8 - c * 3 - f, y + (c % 2)), fill=EYE + (200 - c * 60,))
            feather(d, x, y, 12)
        outline(one, OUTLINE)
        im.alpha_composite(one, (f * fs, 0))
    path = os.path.join(OUT, "GargoyleLord", "GargoyleShard.png")
    im.save(path)
    print("ok", path)


def halberd_crescent():
    """미늘창 파수병 휩쓸기(구 LeafAttackPrefab 뿌리 자리): 땅에서 솟는 초승달 베기 + 먼지. 32px × 5프레임(12.5ppu = 2.56유닛)"""
    n, fs = 5, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    rs = [6, 11, 14, 13, 10]
    al = [160, 240, 255, 180, 90]
    for f in range(n):
        ox = f * fs
        r = rs[f]
        box = (ox + 16 - r, 28 - r * 2, ox + 16 + r, 28)
        d.arc(box, 200, 340, fill=(70, 90, 130, al[f]), width=4)
        d.arc(box, 205, 335, fill=STEEL_L + (al[f],), width=2)
        if f >= 1:
            for c in range(5):
                a = math.radians(190 + c * 40)
                rr = 5 + f * 2
                d.point((ox + 16 + round(math.cos(a) * rr), 28 + round(math.sin(a) * rr * 0.4)), fill=DUST + (220,))
        d.line([(ox + 3, 29), (ox + 29, 29)], fill=CRACK + (180,))
    path = os.path.join(OUT, "HalberdSentry", "HalberdCrescent.png")
    im.save(path)
    print("ok", path)


def gargoyle_feather():
    """가고일 탄: 돌 깃털 한 개(+x 방향). 32×32 한 장"""
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for c in range(4):
        d.point((8 - c * 2, 16 + (c % 2)), fill=EYE + (200 - c * 45,))
    feather(d, 18, 16, 16)
    outline(im, OUTLINE)
    path = os.path.join(OUT, "Gargoyle", "GargoyleFeather.png")
    im.save(path)
    print("ok", path)


def cloud_spark():
    """구름 정령 탄: 작은 번개 구슬. 32×32 한 장"""
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for k, a in ((9, 60), (6, 130)):
        d.ellipse((16 - k, 16 - k, 16 + k, 16 + k), fill=SPARK + (a,))
    d.line([(12, 11), (17, 15), (14, 17), (20, 22)], fill=SPARK_L, width=2)
    for c in range(3):
        d.point((8 - c * 2, 16 + (c % 2)), fill=SPARK + (200 - c * 50,))
    path = os.path.join(OUT, "CloudWisp", "CloudSpark.png")
    im.save(path)
    print("ok", path)


def build(name, fn, attack2=False):
    write(name, "Idle", anim_frames(fn, "idle", 4))
    write(name, "Run", anim_frames(fn, "run", 6))
    write(name, "Attack1", anim_frames(fn, "attack", 6))
    if attack2:
        write(name, "Attack2", anim_frames(fn, "attack2", 8))
    write(name, "Take Hit", hit_frames(fn))
    write(name, "Death", death_frames(fn))
    print("ok", name)


if __name__ == "__main__":
    build("RampartKnight", knight)
    build("HalberdSentry", sentry)
    build("Gargoyle", gargoyle)
    build("CloudWisp", cloud_wisp)
    build("GargoyleLord", lord_any, attack2=True)
    gargoyle_talon()
    gargoyle_shard()
    halberd_crescent()
    gargoyle_feather()
    cloud_spark()
    # 미리보기(검수용, 커밋 안 함)
    rows = [(n, a) for n in ("RampartKnight", "HalberdSentry", "Gargoyle", "CloudWisp", "GargoyleLord") for a in ("Idle", "Run", "Attack1")]
    rows.append(("GargoyleLord", "Attack2"))
    sheets = [Image.open(os.path.join(OUT, n, a + ".png")) for n, a in rows]
    sheets.append(Image.open(os.path.join(OUT, "GargoyleLord", "GargoyleTalon.png")))
    sheets.append(Image.open(os.path.join(OUT, "GargoyleLord", "GargoyleShard.png")))
    sheets.append(Image.open(os.path.join(OUT, "HalberdSentry", "HalberdCrescent.png")))
    Z = 2
    Wd = max(s.width for s in sheets) * Z
    Ht = sum(s.height for s in sheets) * Z
    prev = Image.new("RGBA", (Wd, Ht), (120, 150, 190, 255))
    y = 0
    for s in sheets:
        prev.alpha_composite(s.resize((s.width * Z, s.height * Z), Image.NEAREST), (0, y))
        y += s.height * Z
    prev.save(os.path.join(os.path.dirname(__file__), "..", "..", "Logs", "rampart_mobs_preview.png"))
