"""1층계 ⑥구역 '탑 근교' 몬스터 도트 (TowerWorld 16절, LineageGear 3절).
낮: 소형 골렘(구르기 돌진) · 중형 골렘(지면 강타) · 대형 골렘(슈퍼아머, 바위 투척) / 밤: 골렘 + 유적 정령(작은 빛) / 보스: 탑의 문지기(거대 갑옷 수호자).
보유 에셋에 골렘이 없어 코드로 그린다(SPUM 금지, 도트 필수). 오른쪽을 본다, 발은 프레임 바닥에서 4px 위.
프레임: 소형 48px · 중형 64px · 대형 80px · 정령 48px · 문지기 128px(보스 공통 18px/유닛 → 약 7유닛 높이 프레임).
출력: Assets/Art/Monsters/<Name>/<Anim>.png (가로 시트) + 보스 판정 GateCleave(64px×6)·GateShockwave(32px×3) + 투사체 Boulder·RuinBolt(32px×1).
Tools/probes/build_outskirts_mobs.cs 가 슬라이스·프리팹화한다. 기법은 gen_deep_forest_mobs.py 와 같다(몸색 칠 → 위 밝게/아래 어둡게 → 1px 외곽선)."""
import math
import os

from PIL import Image, ImageDraw

from gen_forest_mobs import shade, outline
from gen_deep_forest_mobs import frame, ph_of, anim_frames, death_frames, hit_frames, write

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")

OUTLINE = (28, 28, 32)
STONE, STONE_L, STONE_D = (128, 126, 120), (170, 168, 158), (84, 84, 86)
MOSS, MOSS_L = (88, 122, 62), (122, 156, 82)
RUNE, RUNE_L = (110, 214, 250), (210, 248, 255)
CRACK = (58, 58, 62)
DUST = (196, 186, 160)
# 문지기 갑옷
ARM, ARM_L, ARM_D = (96, 102, 116), (150, 158, 174), (56, 60, 72)
GOLD, GOLD_L = (196, 160, 72), (240, 210, 120)
CAPE, CAPE_D = (66, 52, 78), (44, 34, 54)
STEEL, STEEL_L = (186, 194, 208), (236, 242, 250)


def sh(im, base, light=None, dark=None):
    shade(im, light or STONE_L, dark or STONE_D, base)


def eye(d, x, y, w=2):
    d.rectangle((x, y, x + w - 1, y), fill=RUNE_L)
    d.point((x - 1, y), fill=RUNE)


def dust(d, x, y, n, spread=8):
    """땅 먼지 튐: n 단계(0~3)"""
    for c in range(4 + n * 2):
        a = math.radians(200 + c * (140 / (3 + n * 2)))
        r = 2 + n * 2 + (c % 3)
        px, py = x + round(math.cos(a) * r * spread / 8), y + round(math.sin(a) * r * 0.6)
        d.point((px, py), fill=DUST + (230 - n * 40,))


# ───────────────── 소형 골렘 (48px): 구르는 바위 ─────────────────
def small_golem(pose, t):
    F, G = 48, 44
    im = frame(F)
    d = ImageDraw.Draw(im)
    roll, lunge, bob, crouch = False, 0, 0, 0
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        roll = True
        bob = -1 if ph_of(t, 6) % 3 == 0 else 0
    elif pose == "attack":  # 몸을 말았다가 앞으로 굴러 들이받는다
        n = ph_of(t, 6)
        crouch = [1, 2, 0, 0, 0, 0][n]
        roll = n >= 2
        lunge = [0, -1, 2, 6, 9, 4][n]
    elif pose == "hit":
        lunge = -2
    if roll:
        cx, cy, r = 24 + lunge, G - 9 + bob, 9
        d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=STONE)
        sh(im, STONE)
        ang = t * math.tau * 2 + lunge * 0.4
        for k in range(3):  # 도는 금
            a = ang + k * math.tau / 3
            d.line([(cx, cy), (cx + round(math.cos(a) * (r - 1)), cy + round(math.sin(a) * (r - 1)))], fill=CRACK)
        a = ang + math.pi / 3
        d.point((cx + round(math.cos(a) * 5), cy + round(math.sin(a) * 5)), fill=RUNE_L)  # 굴러가는 눈빛
        d.arc((cx - r + 2, cy - r + 1, cx + r - 2, cy - 2), 200, 320, fill=MOSS)
        if pose == "attack" or pose == "run":
            dust(d, cx - r - 1, G, 1 if pose == "run" else 2)
    else:
        by = G - 2 + crouch
        bx = 24 + lunge
        d.rectangle((bx - 7, by - 3, bx - 3, G), fill=STONE_D)  # 다리
        d.rectangle((bx + 3, by - 3, bx + 7, G), fill=STONE_D)
        d.ellipse((bx - 10, by - 20 + bob + crouch, bx + 10, by - 1), fill=STONE)  # 바위 몸
        d.ellipse((bx - 14, by - 13 + bob, bx - 8, by - 6 + bob), fill=STONE)  # 뒷팔
        d.ellipse((bx + 8, by - 12 + bob, bx + 14, by - 5 + bob), fill=STONE)  # 앞팔
        sh(im, STONE)
        d.rectangle((bx - 6, by - 21 + bob + crouch, bx + 4, by - 18 + bob + crouch), fill=MOSS)  # 이끼 모자
        d.point((bx - 3, by - 22 + bob + crouch), fill=MOSS_L)
        d.line([(bx - 4, by - 8), (bx - 1, by - 5), (bx + 2, by - 8)], fill=CRACK)  # 금
        eye(d, bx + 3, by - 13 + bob + crouch)
        eye(d, bx + 7, by - 13 + bob + crouch, 1)
    outline(im, OUTLINE)
    return im


# ───────────────── 중형 골렘 (64px): 지면 강타 ─────────────────
def medium_golem(pose, t):
    F, G = 64, 60
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, lift, slam, step = 0, 0, 0, 0, 0.0
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        step = math.sin(t * math.tau)
        bob = 1 if ph_of(t, 6) in (1, 4) else 0
    elif pose == "attack":  # 두 주먹을 들어 올렸다가 땅을 내려친다 → 충격 먼지
        n = ph_of(t, 6)
        lift = [4, 10, 14, 0, 0, 2][n]
        slam = [0, 0, 0, 1, 2, 3][n]
        lunge = [0, -1, -2, 3, 3, 1][n]
        bob = [0, -1, -2, 2, 2, 1][n]
    elif pose == "hit":
        lunge = -2
    bx, by = 30 + lunge, 50 + bob
    # 다리
    for i, lx in enumerate((bx - 8, bx + 4)):
        off = round(step * 3) * (1 if i else -1)
        d.rectangle((lx + off, by - 2, lx + off + 6, G), fill=STONE_D if i == 0 else STONE)
    # 몸통 (어깨가 넓은 돌덩이)
    d.polygon([(bx - 13, by), (bx - 15, by - 18), (bx - 10, by - 26), (bx + 10, by - 27), (bx + 16, by - 19), (bx + 13, by)], fill=STONE)
    # 머리 (몸에 파묻힌 작은 돌)
    d.rectangle((bx - 3, by - 33, bx + 8, by - 25), fill=STONE)
    # 팔 + 주먹: 내려칠 때는 앞으로 뻗어 땅에
    if slam:
        fx, fy = bx + 20, G - 6
        d.polygon([(bx + 8, by - 22), (bx + 15, by - 24), (fx + 4, fy - 2), (fx - 2, fy + 1)], fill=STONE)
        d.ellipse((fx - 5, fy - 6, fx + 7, fy + 5), fill=STONE)
    else:
        fy = by - 8 - lift * 2
        d.polygon([(bx + 8, by - 22), (bx + 15, by - 24), (bx + 19, fy), (bx + 12, fy + 2)], fill=STONE)
        d.ellipse((bx + 11, fy - 5, bx + 22, fy + 6), fill=STONE)
    d.polygon([(bx - 10, by - 22), (bx - 15, by - 23), (bx - 20, by - 6 - lift), (bx - 14, by - 4 - lift)], fill=STONE)
    d.ellipse((bx - 22, by - 9 - lift, bx - 12, by + 1 - lift), fill=STONE)
    sh(im, STONE)
    # 어깨 이끼 · 금 · 룬
    d.rectangle((bx - 12, by - 27, bx - 2, by - 25), fill=MOSS)
    d.rectangle((bx + 6, by - 28, bx + 13, by - 26), fill=MOSS)
    d.point((bx - 8, by - 28), fill=MOSS_L)
    d.line([(bx - 6, by - 18), (bx - 2, by - 12), (bx - 5, by - 6)], fill=CRACK)
    d.rectangle((bx + 1, by - 16, bx + 4, by - 13), fill=RUNE)  # 가슴 룬
    d.point((bx + 2, by - 15), fill=RUNE_L)
    eye(d, bx + 3, by - 30, 4)
    if slam:  # 충격 먼지 · 균열
        dust(d, bx + 22, G, slam + 1, 10)
        d.line([(bx + 14, G), (bx + 30, G)], fill=CRACK)
    outline(im, OUTLINE)
    return im


# ───────────────── 대형 골렘 (80px): 슈퍼아머, 바위 투척 ─────────────────
def large_golem(pose, t):
    F, G = 80, 76
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, step, rock = 0, 0, 0.0, None
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        step = math.sin(t * math.tau)
        bob = 1 if ph_of(t, 6) in (1, 4) else 0
    elif pose == "attack":  # 등 뒤에서 바위를 집어 머리 위로 → 앞으로 던진다(바위는 투사체가 이어받음)
        n = ph_of(t, 6)
        rock = [(-20, -16), (-16, -38), (-4, -48), (6, -46), (22, -30), None][n]
        lunge = [-1, -2, -2, 0, 4, 3][n]
    elif pose == "hit":
        lunge = -1
    bx, by = 38 + lunge, 60 + bob
    for i, lx in enumerate((bx - 13, bx + 5)):
        off = round(step * 3) * (1 if i else -1)
        d.rectangle((lx + off, by - 2, lx + off + 9, G), fill=STONE_D if i == 0 else STONE)
        d.rectangle((lx + off - 1, G - 2, lx + off + 10, G), fill=STONE_D)
    # 몸통: 거대한 돌기둥(어깨 바위가 솟음)
    d.polygon([(bx - 18, by), (bx - 21, by - 26), (bx - 14, by - 40), (bx + 12, by - 42), (bx + 21, by - 30), (bx + 18, by)], fill=STONE)
    d.rectangle((bx - 4, by - 50, bx + 10, by - 39), fill=STONE)  # 머리
    d.polygon([(bx - 20, by - 38), (bx - 26, by - 46), (bx - 12, by - 48), (bx - 8, by - 40)], fill=STONE)  # 어깨 돌
    d.polygon([(bx + 10, by - 40), (bx + 14, by - 50), (bx + 26, by - 44), (bx + 22, by - 34)], fill=STONE)
    # 팔
    if rock is None or rock[0] > 0:
        d.polygon([(bx + 12, by - 34), (bx + 20, by - 36), (bx + 28, by - 14), (bx + 20, by - 12)], fill=STONE)
        d.ellipse((bx + 18, by - 18, bx + 30, by - 6), fill=STONE)
    else:  # 바위를 든 팔은 위로
        d.polygon([(bx + 12, by - 34), (bx + 20, by - 36), (bx + rock[0] + 6, by + rock[1] + 6), (bx + rock[0], by + rock[1] + 8)], fill=STONE)
    d.polygon([(bx - 14, by - 34), (bx - 22, by - 34), (bx - 28, by - 12), (bx - 20, by - 10)], fill=STONE)
    d.ellipse((bx - 30, by - 16, bx - 18, by - 4), fill=STONE)
    sh(im, STONE)
    for (x0, y0, x1) in ((bx - 24, by - 48, bx - 12), (bx + 12, by - 50, bx + 24), (bx - 4, by - 52, bx + 8)):
        d.rectangle((x0, y0, x1, y0 + 2), fill=MOSS)
        d.point((x0 + 3, y0 - 1), fill=MOSS_L)
    d.line([(bx - 10, by - 30), (bx - 4, by - 20), (bx - 9, by - 10), (bx - 5, by - 2)], fill=CRACK)
    d.line([(bx + 8, by - 24), (bx + 13, by - 16)], fill=CRACK)
    for (x, y) in ((bx + 2, by - 26), (bx + 6, by - 18)):  # 몸에 박힌 룬
        d.rectangle((x, y, x + 2, y + 2), fill=RUNE)
    eye(d, bx + 4, by - 46, 5)
    if rock is not None:  # 들고 있는 바위
        rx, ry = bx + rock[0], by + rock[1]
        d.ellipse((rx - 7, ry - 6, rx + 7, ry + 6), fill=STONE_D)
        d.ellipse((rx - 6, ry - 6, rx + 4, ry + 2), fill=STONE)
        d.point((rx - 2, ry - 3), fill=STONE_L)
    if pose == "run" and ph_of(t, 6) in (1, 4):
        dust(d, bx - 10, G, 0)
    outline(im, OUTLINE)
    return im


# ───────────────── 유적 정령 (48px): 작은 빛 ─────────────────
def ruin_spirit(pose, t):
    F = 48
    im = frame(F)
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    cy = 26 + round(math.sin(ph) * 2)
    cx = 24
    glow = 0
    if pose == "attack":
        n = ph_of(t, 6)
        glow = [0, 1, 2, 3, 1, 0][n]
        cx += [0, -1, -1, 2, 1, 0][n]
    elif pose == "hit":
        cx -= 2
    r = 9 + glow
    for k, a in ((r, 60), (r - 3, 110), (r - 6, 170)):  # 빛 무리
        d.ellipse((cx - k, cy - k, cx + k, cy + k), fill=RUNE + (a,))
    # 룬 돌 조각(다이아몬드) 핵
    d.polygon([(cx, cy - 6), (cx + 4, cy), (cx, cy + 6), (cx - 4, cy)], fill=STONE)
    d.polygon([(cx, cy - 6), (cx + 4, cy), (cx, cy)], fill=STONE_L)
    d.line([(cx - 1, cy - 2), (cx + 1, cy + 2)], fill=RUNE_L)
    d.point((cx + 1, cy - 1), fill=RUNE_L)
    for k in range(3):  # 공전하는 작은 돌 조각
        a = ph + k * math.tau / 3
        x, y = cx + round(math.cos(a) * 11), cy + round(math.sin(a) * 5)
        d.rectangle((x, y, x + 1, y + 1), fill=STONE_L)
    if glow >= 2:
        for k in range(6):
            a = k * math.tau / 6 + ph
            d.point((cx + round(math.cos(a) * (12 + glow)), cy + round(math.sin(a) * (12 + glow))), fill=RUNE_L)
    return im


# ───────────────── 탑의 문지기 (128px) ─────────────────
def blade(d, hx, hy, ang, length, width, glow=False):
    """칼자루 끝(hx, hy)에서 ang(도, 0 = 오른쪽, 90 = 아래) 방향으로 대검을 그린다"""
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    nx, ny = -uy, ux
    grip = 8
    gx, gy = hx + ux * grip, hy + uy * grip
    tx, ty = hx + ux * length, hy + uy * length
    w = width / 2
    d.line([(hx, hy), (gx, gy)], fill=(70, 52, 40), width=3)  # 손잡이
    d.line([(gx - nx * 7, gy - ny * 7), (gx + nx * 7, gy + ny * 7)], fill=GOLD, width=3)  # 가드
    d.polygon([(gx - nx * w, gy - ny * w), (gx + nx * w, gy + ny * w), (tx + nx * 1, ty + ny * 1), (tx + ux * 4, ty + uy * 4), (tx - nx * 1, ty - ny * 1)], fill=STEEL)
    d.line([(gx + ux * 2, gy + uy * 2), (tx, ty)], fill=STEEL_L if not glow else RUNE_L)  # 날 중심선
    d.point((round(hx - ux * 2), round(hy - uy * 2)), fill=GOLD_L)  # 폼멜


def gatekeeper(pose, t, attack2=False):
    F, G = 128, 124
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, step = 0, 0, 0.0
    sword = (100, 40, 90, 80)      # 칼자루 끝 x, y, 각도, 길이 — 기본: 앞에 꽂아 세운 대검(두 손을 폼멜에)
    arm_to = (100, 42)
    fx = 0
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        step = math.sin(t * math.tau)
        bob = 1 if ph_of(t, 6) in (1, 4) else 0
        sword = (88, 70, 20, 80)
        arm_to = (88, 70)
    elif pose == "attack":  # 대검을 머리 위로 들었다가 앞으로 내려찍는다
        n = ph_of(t, 6)
        sword = [(78, 40, -100, 84), (70, 26, -120, 84), (70, 22, -150, 84), (88, 52, 30, 86), (92, 62, 60, 86), (96, 52, 80, 86)][n]
        arm_to = sword[:2]
        lunge = [0, -2, -3, 4, 6, 3][n]
        bob = [0, -1, -2, 2, 3, 1][n]
        fx = [0, 0, 0, 1, 2, 3][n]
    elif pose == "attack2":  # 대검을 땅에 꽂아 룬 충격파 + 부하 소환
        n = ph_of(t, 8)
        sword = [(90, 36, 90, 86), (84, 18, 90, 86), (84, 10, 90, 86), (92, 44, 90, 90), (94, 50, 90, 90), (94, 50, 90, 90), (94, 50, 90, 90), (92, 44, 90, 86)][n]
        arm_to = sword[:2]
        bob = [0, -2, -3, 3, 3, 2, 1, 0][n]
        fx = [0, 0, 0, 1, 2, 3, 4, 0][n] + 10  # 10+ = 룬 고리
    elif pose == "hit":
        lunge = -3
    bx, by = 60 + lunge, 0 + bob
    # 망토(뒤, 너덜너덜)
    d.polygon([(bx - 18, by + 46), (bx - 4, by + 44), (bx - 6, by + 104), (bx - 14, by + 112), (bx - 20, by + 104), (bx - 26, by + 114), (bx - 28, by + 70)], fill=CAPE)
    # 다리 (그리브)
    for i, lx in enumerate((bx - 14, bx + 6)):
        off = round(step * 4) * (1 if i else -1)
        col = ARM_D if i == 0 else ARM
        d.rectangle((lx + off, by + 94, lx + off + 12, G - 4), fill=col)
        d.rectangle((lx + off - 1, G - 5, lx + off + 15, G), fill=col)  # 사바톤
        d.rectangle((lx + off + 2, by + 100, lx + off + 10, by + 103), fill=ARM_L if i else ARM)  # 무릎 판
    # 허리 갑주 (태싯)
    d.polygon([(bx - 20, by + 80), (bx + 24, by + 80), (bx + 26, by + 98), (bx - 22, by + 98)], fill=ARM)
    # 흉갑
    d.polygon([(bx - 22, by + 44), (bx + 24, by + 44), (bx + 22, by + 82), (bx - 20, by + 82)], fill=ARM)
    # 뒷팔(먼 쪽)
    d.polygon([(bx - 20, by + 50), (bx - 30, by + 52), (bx - 30, by + 82), (bx - 22, by + 84)], fill=ARM_D)
    # 어깨 갑주
    d.ellipse((bx - 34, by + 36, bx - 10, by + 58), fill=ARM)
    d.ellipse((bx + 10, by + 36, bx + 36, by + 58), fill=ARM)
    # 투구
    d.polygon([(bx - 10, by + 44), (bx - 12, by + 22), (bx - 4, by + 12), (bx + 10, by + 12), (bx + 16, by + 22), (bx + 14, by + 44)], fill=ARM)
    shade(im, ARM_L, ARM_D, ARM)
    shade(im, (88, 70, 100), CAPE_D, CAPE)
    # 금 테두리·문장(브로치와 같은 문양: 원 + 네 갈래 별) — 스토리 암시(TowerWorld 2절)
    d.line([(bx - 20, by + 80), (bx + 22, by + 80)], fill=GOLD)
    d.line([(bx - 21, by + 45), (bx + 23, by + 45)], fill=GOLD)
    cx, cy = bx + 2, by + 60
    d.ellipse((cx - 7, cy - 7, cx + 7, cy + 7), fill=GOLD)
    d.ellipse((cx - 5, cy - 5, cx + 5, cy + 5), fill=(70, 120, 150))
    d.polygon([(cx, cy - 5), (cx + 1, cy - 1), (cx + 5, cy), (cx + 1, cy + 1), (cx, cy + 5), (cx - 1, cy + 1), (cx - 5, cy), (cx - 1, cy - 1)], fill=RUNE_L)
    # 이끼(오래 서 있던 수호자)
    for (x0, y0, w0) in ((bx - 30, by + 37, 10), (bx + 16, by + 37, 12), (bx - 16, by + 82, 8), (bx + 6, by + 96, 6)):
        d.rectangle((x0, y0, x0 + w0, y0 + 1), fill=MOSS)
        d.point((x0 + 2, y0 - 1), fill=MOSS_L)
    # 투구 바이저 빛 · 뿔 장식
    d.rectangle((bx - 6, by + 27, bx + 13, by + 29), fill=(20, 22, 30))
    d.rectangle((bx + 2, by + 27, bx + 11, by + 28), fill=RUNE_L)
    d.polygon([(bx - 2, by + 12), (bx + 2, by + 2), (bx + 6, by + 12)], fill=GOLD)
    d.polygon([(bx - 6, by + 14), (bx - 16, by + 4), (bx - 12, by + 16)], fill=ARM_D)
    # 앞팔 + 대검
    sx, sy, ang, ln = sword
    sx += lunge
    sy += bob
    ax, ay = arm_to[0] + lunge, arm_to[1] + bob
    d.polygon([(bx + 16, by + 50), (bx + 26, by + 52), (ax + 3, ay + 3), (ax - 4, ay - 2)], fill=ARM)
    d.ellipse((ax - 5, ay - 5, ax + 5, ay + 5), fill=ARM_D)  # 건틀릿
    blade(d, sx, sy, ang, ln, 9, glow=fx > 0)
    outline(im, OUTLINE)
    # 효과
    if 1 <= fx <= 3:  # 내려찍기 충격
        for k in range(3 + fx * 2):
            a = math.radians(190 + k * (160 / (2 + fx * 2)))
            r = 6 + fx * 5
            d.point((min(F - 1, 112 + round(math.cos(a) * r)), G - 1 + round(math.sin(a) * r * 0.5)), fill=DUST + (230,))
        d.line([(96, G), (min(F - 1, 104 + fx * 8), G)], fill=RUNE)
    if fx >= 11:  # 땅에 꽂힌 검에서 퍼지는 룬 고리
        k = fx - 10
        r = 8 + k * 9
        d.arc((94 + lunge - r, G - 4 - r // 3, 94 + lunge + r, G + r // 3), 180, 360, fill=RUNE + (240 - k * 40,), width=2)
        d.arc((94 + lunge - r // 2, G - 3 - r // 6, 94 + lunge + r // 2, G + r // 6), 180, 360, fill=RUNE_L + (220,))
    return im


def gatekeeper_any(pose, t):
    return gatekeeper(pose, t)


# ───────────────── 보스 판정·투사체 ─────────────────
def gate_cleave():
    """보스 근접(구 갈퀴 ClowAttack 자리): 대검 내려찍기 궤적 + 룬 섬광. 64px × 6프레임(16ppu = 4유닛, 원본과 같은 크기)"""
    n, fs = 6, 64
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    sweep = [(240, 270), (230, 320), (225, 370), (230, 400), (270, 405), (330, 405)]
    alpha = [190, 240, 255, 230, 160, 80]
    for f in range(n):
        ox = f * fs
        a0, a1 = sweep[f]
        for w, col in ((8, (40, 70, 110)), (6, (150, 200, 240)), (2, (240, 252, 255))):
            d.arc((ox + 4, 6, ox + 60, 62), a0, a1, fill=col + (alpha[f],), width=w)
        if f >= 3:  # 땅 충격 파편
            for c in range(5):
                x = ox + 30 + c * 6
                y = 58 - ((c * 7 + f * 3) % 9)
                d.rectangle((x, y, x + 1, y + 1), fill=(200, 196, 180, 230 - (f - 3) * 50))
    path = os.path.join(OUT, "Gatekeeper", "GateCleave.png")
    im.save(path)
    print("ok", path)


def gate_shockwave():
    """보스 원거리(구 불꽃 자리): 땅을 타고 미는 룬 충격파 초승달 + 돌 파편. 32px × 3프레임(12.5ppu)"""
    n, fs = 3, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    for f in range(n):
        one = Image.new("RGBA", (fs, fs), (0, 0, 0, 0))
        d = ImageDraw.Draw(one)
        d.arc((4, 4, 28, 28), 300 - f * 6, 60 + f * 6, fill=RUNE + (230,), width=4)
        d.arc((6, 6, 26, 26), 310, 50, fill=RUNE_L + (255,), width=2)
        for c in range(4):  # 앞에 튀는 돌 조각
            x, y = 22 + ((c * 3 + f * 2) % 7), 8 + c * 5
            d.rectangle((x, y, x + 1, y + 1), fill=STONE_L + (255,))
        im.alpha_composite(one, (f * fs, 0))
    path = os.path.join(OUT, "Gatekeeper", "GateShockwave.png")
    im.save(path)
    print("ok", path)


def quake_burst():
    """중형 골렘 지면 강타(구 LeafAttackPrefab 뿌리 자리): 땅에서 솟는 돌 가시 + 먼지. 32px × 5프레임(12.5ppu = 2.56유닛, 원본과 같은 크기)"""
    n, fs = 5, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    hts = [4, 12, 18, 14, 6]
    for f in range(n):
        one = Image.new("RGBA", (fs, fs), (0, 0, 0, 0))
        d = ImageDraw.Draw(one)
        h = hts[f]
        for (x, k) in ((9, 0.7), (16, 1.0), (23, 0.8)):
            hh = max(2, round(h * k))
            d.polygon([(x - 4, 28), (x, 28 - hh), (x + 4, 28)], fill=STONE)
        shade(one, STONE_L, STONE_D, STONE)
        if f >= 1:
            for c in range(6):
                a = math.radians(190 + c * 30)
                r = 6 + f * 2
                d.point((16 + round(math.cos(a) * r), 27 + round(math.sin(a) * r * 0.5)), fill=DUST + (220,))
        d.line([(3, 29), (29, 29)], fill=CRACK + (200,))
        outline(one, OUTLINE)
        im.alpha_composite(one, (f * fs, 0))
    path = os.path.join(OUT, "GolemMedium", "QuakeBurst.png")
    im.save(path)
    print("ok", path)


def boulder():
    """대형 골렘 투척 바위(+ Monolith Shield 바위). 32×32 한 장"""
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse((5, 6, 27, 26), fill=STONE)
    shade(im, STONE_L, STONE_D, STONE)
    d.line([(10, 12), (15, 17), (13, 22)], fill=CRACK)
    d.rectangle((9, 6, 18, 7), fill=MOSS)
    d.rectangle((19, 15, 20, 16), fill=RUNE)
    outline(im, OUTLINE)
    path = os.path.join(OUT, "GolemLarge", "Boulder.png")
    im.save(path)
    print("ok", path)


def ruin_bolt():
    """유적 정령 탄: 작은 룬 빛 조각(+x 방향). 32×32 한 장"""
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for k, a in ((8, 70), (5, 140)):
        d.ellipse((16 - k, 16 - k, 16 + k, 16 + k), fill=RUNE + (a,))
    d.polygon([(12, 16), (16, 12), (24, 16), (16, 20)], fill=RUNE_L + (255,))
    for c in range(3):
        d.point((9 - c * 2, 16 + (c % 2)), fill=RUNE + (200 - c * 50,))
    path = os.path.join(OUT, "RuinSpirit", "RuinBolt.png")
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
    build("GolemSmall", small_golem)
    build("GolemMedium", medium_golem)
    build("GolemLarge", large_golem)
    build("RuinSpirit", ruin_spirit)
    build("Gatekeeper", gatekeeper_any, attack2=True)
    gate_cleave()
    gate_shockwave()
    quake_burst()
    boulder()
    ruin_bolt()
    # 미리보기(검수용, 커밋 안 함)
    rows = [(n, a) for n in ("GolemSmall", "GolemMedium", "GolemLarge", "RuinSpirit", "Gatekeeper") for a in ("Idle", "Run", "Attack1")]
    rows.append(("Gatekeeper", "Attack2"))
    sheets = [Image.open(os.path.join(OUT, n, a + ".png")) for n, a in rows]
    sheets.append(Image.open(os.path.join(OUT, "Gatekeeper", "GateCleave.png")))
    sheets.append(Image.open(os.path.join(OUT, "Gatekeeper", "GateShockwave.png")))
    Z = 2
    Wd = max(s.width for s in sheets) * Z
    Ht = sum(s.height for s in sheets) * Z
    prev = Image.new("RGBA", (Wd, Ht), (70, 74, 80, 255))
    y = 0
    for s in sheets:
        prev.alpha_composite(s.resize((s.width * Z, s.height * Z), Image.NEAREST), (0, y))
        y += s.height * Z
    prev.save(os.path.join(os.path.dirname(__file__), "..", "..", "Logs", "outskirts_mobs_preview.png"))
