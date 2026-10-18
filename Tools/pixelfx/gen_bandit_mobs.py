"""1층계 ⑤구역 '산적 성채' 몬스터 도트 (TowerWorld 15절, LineageGear 2절).
낮: 산적 단검병(빠른 근접) · 산적 궁수(직선 화살) · 산적 방패병(방어·슈퍼아머) / 밤: 같은 종 + 횃불 순찰병(Light2D) / 보스: 산적 대장.
SPUM 금지(사용자) → 마을 NPC(gen_elder.py) 양식을 따른 코드 도트: 큰 머리 2등신, 짙은 외곽선, 단순한 면 + 위 밝게/아래 어둡게.
몬스터 규격(MonsterBuilder): 48px 프레임(보스 96px = 같은 그림을 2배 좌표로 다시 그림), 오른쪽을 본다, 발은 바닥에서 4px 위.
키는 약 32px(머리 13 + 몸 10 + 다리 7) = 24px/유닛에서 1.35유닛 → 플레이어와 비슷.
출력: Assets/Art/Monsters/<Name>/<Anim>.png (가로 시트) + 보스 판정 ChiefCleave(64px×6)·ChiefAxe(32px×3) + 화살 Arrow(32px×1).
Tools/probes/build_bandit_mobs.cs 가 슬라이스·프리팹화한다."""
import math
import os

from PIL import Image, ImageDraw

from gen_forest_mobs import outline
from gen_deep_forest_mobs import frame, ph_of, anim_frames, death_frames, hit_frames, write

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")

OUTLINE = (34, 22, 22)
SKIN, SKIN_D = (230, 182, 142), (190, 138, 104)
EYE = (30, 20, 20)
STEEL, STEEL_L, STEEL_D = (190, 196, 206), (236, 240, 246), (120, 126, 140)
WOOD, WOOD_D = (138, 94, 52), (92, 60, 34)
BOOT = (52, 38, 30)
FLAME = ((255, 236, 140), (255, 176, 64), (226, 92, 40))

# 종류별 옷 색: hat(머리 장비), hat_d, body(상의), body_d, pants, accent
KINDS = {
    "cutthroat": dict(hat=(156, 40, 46), hat_d=(108, 26, 32), body=(92, 70, 58), body_d=(62, 46, 40), pants=(58, 54, 70), accent=(156, 40, 46)),
    "archer": dict(hat=(88, 104, 60), hat_d=(58, 72, 40), body=(124, 94, 60), body_d=(86, 64, 42), pants=(70, 62, 52), accent=(170, 140, 80)),
    "shield": dict(hat=(150, 156, 166), hat_d=(98, 102, 114), body=(116, 62, 50), body_d=(80, 42, 36), pants=(64, 58, 56), accent=(196, 160, 72)),
    "torch": dict(hat=(74, 56, 44), hat_d=(50, 38, 30), body=(100, 66, 44), body_d=(68, 44, 30), pants=(56, 50, 46), accent=(226, 120, 50)),
    "chief": dict(hat=(150, 156, 166), hat_d=(98, 102, 114), body=(146, 38, 38), body_d=(98, 24, 26), pants=(60, 50, 50), accent=(208, 168, 74)),
}
FUR, FUR_D = (164, 140, 106), (118, 98, 72)
BEARD = (84, 54, 34)


def bandit(kind, pose, t, k=1.0):
    """kind 별 인간형 산적. 논리 좌표(48px 기준)를 k 배로 그린다."""
    pal = KINDS[kind]
    F = round(48 * k)
    im = frame(F)
    d = ImageDraw.Draw(im)
    W = max(1, round(k))

    def P(x, y):
        return (round(x * k), round(y * k))

    def R(x0, y0, x1, y1):
        return (round(x0 * k), round(y0 * k), round(x1 * k + k - 1), round(y1 * k + k - 1))

    def line(pts, col, w=1):
        d.line([P(x, y) for x, y in pts], fill=col, width=max(1, round(w * k)))

    G = 44
    bob, lean, step, crouch = 0, 0, 0.0, 0
    arm = "idle"      # 앞팔 자세 키
    fx = 0            # 효과 단계
    n = 0
    if pose == "idle":
        bob = 1 if ph_of(t, 4) in (1, 2) else 0
    elif pose == "run":
        ph = t * math.tau
        step = math.sin(ph)
        bob = 1 if ph_of(t, 6) in (1, 4) else 0
        lean = 1
        arm = "run"
    elif pose == "attack":
        n = ph_of(t, 6)
        arm = "atk%d" % n
        lean = {"cutthroat": [-1, -1, 3, 1, 3, 0], "archer": [0, 0, -1, -1, 0, 0], "shield": [-1, -2, 4, 3, 1, 0],
                "torch": [-1, -2, 2, 3, 1, 0], "chief": [-1, -2, 2, 3, 1, 0]}[kind][n]
        crouch = {"cutthroat": [1, 2, 0, 0, 0, 0], "shield": [1, 2, 0, 0, 0, 0]}.get(kind, [0, 1, 0, 0, 0, 0])[n]
        fx = n
    elif pose == "attack2":  # 보스: 칼을 치켜들고 부하를 부른다(외침 파동)
        n = ph_of(t, 8)
        arm = "call%d" % n
        crouch = [0, 1, 0, 0, 0, 0, 1, 0][n]
        fx = n
    elif pose == "hit":
        lean = -2
        arm = "hit"
    cx = 22 + lean
    top = crouch + bob  # 상체가 내려가는 양

    # ── 다리 (뒤 → 앞) ──
    s = round(step * 2.5)
    for i, off in enumerate((-s, s)):
        lx = cx - 3 + i * 4 + off
        lift = 1 if pose == "run" and (off > 0) == (i == 1) and abs(off) >= 2 else 0
        col = pal["pants"] if i == 1 else tuple(max(0, c - 18) for c in pal["pants"])
        d.rectangle(R(lx, G - 7 + crouch, lx + 2, G - 2 - lift), fill=col)
        d.rectangle(R(lx, G - 2 - lift, lx + 3, G - lift), fill=BOOT)

    # ── 뒤팔 (몸 뒤, 어둡게) ──
    sx, sy = cx - 3, G - 15 + top
    back_hand = (cx - 6, G - 9 + top)
    if pose == "run":
        back_hand = (cx - 4 - round(step * 3), G - 9 + top)
    if kind == "archer" and pose == "attack":
        back_hand = [(cx + 6, G - 15), (cx + 5, G - 15), (cx + 1, G - 15), (cx - 1, G - 15), (cx - 3, G - 13), (cx - 5, G - 10)][n]
    if kind == "cutthroat" and pose == "attack":
        back_hand = [(cx - 6, G - 12), (cx - 7, G - 14), (cx - 5, G - 10), (cx + 2, G - 12), (cx + 9, G - 12), (cx - 2, G - 10)][n]
    line([(sx, sy), back_hand], pal["body_d"], 2)
    d.rectangle(R(back_hand[0], back_hand[1], back_hand[0] + 1, back_hand[1] + 1), fill=SKIN_D)
    if kind == "cutthroat":  # 역수로 쥔 두 번째 단검
        bx, by = back_hand
        line([(bx + 1, by + 1), (bx + 4 if n == 4 and pose == "attack" else bx - 1, by + 4)], STEEL_D, 1)

    # ── 몸통 ──
    bw = 6 if kind != "chief" else 7
    d.rectangle(R(cx - bw + 1, G - 17 + top, cx + bw - 1, G - 7 + crouch), fill=pal["body"])
    d.rectangle(R(cx - bw + 1, G - 17 + top, cx + bw - 1, G - 16 + top), fill=tuple(min(255, c + 24) for c in pal["body"]))
    d.rectangle(R(cx - bw + 1, G - 8 + crouch, cx + bw - 1, G - 7 + crouch), fill=pal["body_d"])
    d.rectangle(R(cx - bw + 1, G - 10 + top, cx + bw - 1, G - 10 + top), fill=BOOT)  # 허리띠
    d.rectangle(R(cx + 1, G - 10 + top, cx + 2, G - 10 + top), fill=pal["accent"])     # 버클
    if kind == "archer":  # 등에 멘 화살통
        d.rectangle(R(cx - 8, G - 20 + top, cx - 6, G - 11 + top), fill=WOOD_D)
        for c in range(3):
            d.point(P(cx - 8 + c, G - 21 + top), fill=(230, 230, 220))
    if kind == "chief":  # 모피 망토 깃
        d.rectangle(R(cx - 7, G - 18 + top, cx + 6, G - 16 + top), fill=FUR)
        for c in range(-6, 6, 2):
            d.point(P(cx + c, G - 15 + top), fill=FUR_D)
        d.polygon([P(cx - 7, G - 17 + top), P(cx - 10, G - 6), P(cx - 6, G - 7 + crouch)], fill=pal["body_d"])  # 망토 자락
    if kind == "shield":  # 쇠사슬 조끼 점
        for yy in range(G - 15 + top, G - 10 + top, 2):
            for xx in range(cx - 4, cx + 5, 2):
                d.point(P(xx, yy), fill=pal["hat_d"])

    # ── 머리 (큰 머리 2등신) ──
    hx, hy = cx + 1, G - 30 + top
    d.ellipse(R(hx - 7, hy, hx + 6, hy + 12), fill=SKIN)
    d.rectangle(R(hx - 6, hy + 9, hx + 5, hy + 11), fill=SKIN_D)  # 턱 그림자
    ey = hy + 6
    d.rectangle(R(hx + 1, ey, hx + 1, ey + 1), fill=EYE)
    d.rectangle(R(hx + 4, ey, hx + 4, ey + 1), fill=EYE)
    d.rectangle(R(hx, ey - 1, hx + 2, ey - 1), fill=(90, 56, 40))  # 찌푸린 눈썹
    d.rectangle(R(hx + 3, ey - 1, hx + 5, ey - 1), fill=(90, 56, 40))
    if kind == "cutthroat":  # 두건 + 코까지 올린 복면
        d.chord(R(hx - 7, hy - 1, hx + 6, hy + 9), 180, 360, fill=pal["hat"])
        d.rectangle(R(hx - 7, hy + 3, hx + 6, hy + 4), fill=pal["hat"])
        d.polygon([P(hx - 7, hy + 3), P(hx - 11, hy + 2 + bob), P(hx - 10, hy + 6)], fill=pal["hat_d"])  # 매듭 꼬리
        d.rectangle(R(hx - 6, ey + 2, hx + 6, hy + 11), fill=(48, 40, 44))
    elif kind == "archer":  # 뾰족 두건
        d.polygon([P(hx - 8, hy + 8), P(hx - 6, hy - 1), P(hx + 1, hy - 3), P(hx + 7, hy + 2), P(hx + 7, hy + 4), P(hx - 3, hy + 4), P(hx - 4, hy + 11)], fill=pal["hat"])
        d.polygon([P(hx - 6, hy - 1), P(hx - 10, hy + 1), P(hx - 8, hy + 4)], fill=pal["hat_d"])
        d.rectangle(R(hx + 3, hy + 9, hx + 4, hy + 9), fill=SKIN_D)
    elif kind == "shield":  # 둥근 철모 + 코 가리개
        d.chord(R(hx - 8, hy - 2, hx + 7, hy + 10), 180, 360, fill=pal["hat"])
        d.rectangle(R(hx - 8, hy + 3, hx + 7, hy + 4), fill=pal["hat_d"])
        d.rectangle(R(hx + 3, hy + 4, hx + 3, hy + 8), fill=pal["hat_d"])
        d.point(P(hx - 3, hy), fill=STEEL_L)
        d.rectangle(R(hx - 2, hy + 10, hx + 5, hy + 11), fill=BEARD)  # 짧은 수염
    elif kind == "torch":  # 짙은 두건 + 목도리
        d.chord(R(hx - 8, hy - 1, hx + 7, hy + 11), 180, 360, fill=pal["hat"])
        d.rectangle(R(hx - 8, hy + 4, hx - 4, hy + 11), fill=pal["hat"])
        d.rectangle(R(hx - 6, hy + 11, hx + 5, hy + 13), fill=pal["accent"])
    elif kind == "chief":  # 뿔 달린 철투구 + 안대 + 수염
        d.chord(R(hx - 8, hy - 2, hx + 7, hy + 10), 180, 360, fill=pal["hat"])
        d.rectangle(R(hx - 8, hy + 3, hx + 7, hy + 4), fill=pal["accent"])
        d.polygon([P(hx - 7, hy + 1), P(hx - 11, hy - 4), P(hx - 10, hy - 7), P(hx - 8, hy - 2)], fill=(232, 222, 192))
        d.polygon([P(hx + 5, hy + 1), P(hx + 9, hy - 4), P(hx + 8, hy - 7), P(hx + 6, hy - 2)], fill=(232, 222, 192))
        d.rectangle(R(hx + 3, ey - 1, hx + 5, ey + 1), fill=(20, 16, 16))  # 안대
        d.line([P(hx - 6, ey - 2), P(hx + 6, ey - 3)], fill=(20, 16, 16), width=W)
        d.polygon([P(hx - 5, hy + 9), P(hx + 6, hy + 9), P(hx + 4, hy + 14), P(hx + 1, hy + 15), P(hx - 3, hy + 13)], fill=BEARD)
        d.rectangle(R(hx + 1, hy + 10, hx + 3, hy + 10), fill=(120, 40, 40))  # 입
        d.line([P(hx - 4, ey + 1), P(hx - 2, ey + 4)], fill=(170, 100, 90), width=W)  # 흉터

    # ── 앞팔 + 무기 ──
    shoulder = (cx + 2, G - 15 + top)
    hand = (cx + 6, G - 10 + top)
    if pose == "run":
        hand = (cx + 4 + round(step * 3), G - 10 + top)
    if pose == "hit":
        hand = (cx - 1, G - 14 + top)
    weapon = None  # (끝점, 색) 등은 종류별로
    if kind == "cutthroat":
        if pose == "attack":
            hand = [(cx - 2, G - 16), (cx - 3, G - 18), (cx + 10, G - 13), (cx + 6, G - 12), (cx + 3, G - 13), (cx + 4, G - 11)][n]
        line([shoulder, hand], pal["body"], 2)
        tip = (hand[0] + (5 if pose != "attack" or n in (2, 3, 4, 5) else 3), hand[1] - (0 if pose == "attack" and n == 2 else 2))
        line([hand, tip], STEEL, 1)
        d.point(P(*tip), fill=STEEL_L)
        if pose == "attack" and n in (2, 4):  # 찌르기 섬광
            ox = hand[0] if n == 2 else back_hand[0]
            oy = hand[1] if n == 2 else back_hand[1]
            for c in range(3):
                d.point(P(ox + 7 + c * 2, oy - 1 + (c % 2)), fill=(255, 245, 230, 220))
    elif kind == "archer":
        if pose == "attack":
            hand = (cx + 8, G - 15)
        else:
            hand = (cx + 6, G - 13 + top)
        line([shoulder, hand], pal["body"], 2)
        bx, by = hand
        pull = [0, 1, 3, 5, 0, 0][n] if pose == "attack" else 0
        d.arc(R(bx - 2, by - 8, bx + 3, by + 8), 280, 80, fill=WOOD, width=W)  # 활대
        d.point(P(bx + 2, by - 8), fill=WOOD_D); d.point(P(bx + 2, by + 8), fill=WOOD_D)
        string_x = bx + 1 - pull
        line([(bx + 2, by - 7), (string_x, by), (bx + 2, by + 7)], (226, 220, 200), 0.5)
        if pose == "attack" and n in (1, 2, 3):  # 화살 메김
            line([(string_x, by), (string_x + 9, by)], (200, 170, 120), 1)
            d.polygon([P(string_x + 9, by - 1), P(string_x + 11, by), P(string_x + 9, by + 1)], fill=STEEL)
            d.point(P(string_x, by - 1), fill=(220, 60, 60))
        if pose == "attack" and n == 4:  # 발사 궤적
            for c in range(4):
                d.point(P(bx + 5 + c * 3, by), fill=(255, 250, 230, 200))
    elif kind == "shield":
        if pose == "attack":
            hand = [(cx + 5, G - 11), (cx + 4, G - 14), (cx + 9, G - 12), (cx + 9, G - 12), (cx + 7, G - 11), (cx + 6, G - 11)][n]
        line([shoulder, hand], pal["body"], 2)
        sx0, sy0 = hand[0] - 1, hand[1] - 8
        d.rounded_rectangle(R(sx0, sy0, sx0 + 5, sy0 + 13), radius=max(1, round(2 * k)), fill=WOOD)  # 둥근 방패(옆모습)
        d.rectangle(R(sx0, sy0, sx0 + 5, sy0), fill=STEEL)
        d.rectangle(R(sx0, sy0 + 13, sx0 + 5, sy0 + 13), fill=STEEL_D)
        d.rectangle(R(sx0 + 5, sy0 + 1, sx0 + 5, sy0 + 12), fill=STEEL)
        d.rectangle(R(sx0 + 2, sy0 + 5, sx0 + 3, sy0 + 7), fill=pal["accent"])  # 방패 돌기
        for yy in (sy0 + 2, sy0 + 11):
            d.point(P(sx0 + 1, yy), fill=STEEL_L)
        if pose == "attack" and n in (2, 3):  # 밀치기 충격선
            for c in range(3):
                line([(sx0 + 8, sy0 + 2 + c * 4), (sx0 + 11, sy0 + 2 + c * 4)], (255, 245, 220, 220), 0.5)
    elif kind == "torch":
        if pose == "attack":
            hand = [(cx + 5, G - 12), (cx + 1, G - 20), (cx + 6, G - 19), (cx + 10, G - 13), (cx + 9, G - 10), (cx + 6, G - 11)][n]
        line([shoulder, hand], pal["body"], 2)
        ang = [-70, -110, -60, -10, 10, -40][n] if pose == "attack" else -75
        a = math.radians(ang)
        tx, ty = hand[0] + math.cos(a) * 6, hand[1] + math.sin(a) * 6
        line([hand, (tx, ty)], WOOD, 1)
        fl = ph_of(t, 4) if pose != "attack" else n
        r = 2 + (fl % 2)
        d.ellipse(R(tx - r, ty - r - 1, tx + r, ty + r - 1), fill=FLAME[2])
        d.ellipse(R(tx - r + 1, ty - r, tx + r - 1, ty + r - 2), fill=FLAME[1])
        d.point(P(tx, ty - 1), fill=FLAME[0])
        d.point(P(tx + (fl % 3) - 1, ty - r - 2), fill=FLAME[1])
        if pose == "attack" and n in (2, 3, 4):  # 불꽃 궤적
            for c in range(3):
                d.point(P(tx - 2 - c * 2, ty - 2 + c * 2), fill=FLAME[1] + (200,))
    elif kind == "chief":
        if pose == "attack":
            hand = [(cx + 4, G - 12), (cx - 1, G - 21), (cx + 2, G - 23), (cx + 10, G - 14), (cx + 9, G - 9), (cx + 6, G - 11)][n]
            ang = [-60, -150, -120, -15, 25, -30][n]
        elif pose == "attack2":
            hand = [(cx + 5, G - 12), (cx + 3, G - 18), (cx + 3, G - 22), (cx + 3, G - 23), (cx + 3, G - 23), (cx + 3, G - 22), (cx + 4, G - 16), (cx + 5, G - 12)][n]
            ang = [-60, -80, -90, -90, -90, -90, -70, -60][n]
        else:
            ang = -55 if pose != "hit" else -100
        line([shoulder, hand], pal["body"], 2)
        a = math.radians(ang)
        ux, uy = math.cos(a), math.sin(a)
        tip = (hand[0] + ux * 11, hand[1] + uy * 11)
        # 넓은 곡도: 칼등 + 칼날 두 줄
        line([hand, tip], STEEL, 2)
        line([(hand[0] - uy * 1, hand[1] + ux * 1), (tip[0] - uy * 2, tip[1] + ux * 2)], STEEL_L, 1)
        line([(hand[0] - ux * 1.5, hand[1] - uy * 1.5), (hand[0] + uy * 2, hand[1] - ux * 2)], pal["accent"], 1)  # 가드
        if pose == "attack" and n in (3, 4):  # 큰 베기 궤적
            for c in range(5):
                aa = math.radians(-150 + c * 35)
                d.point(P(hand[0] + math.cos(aa) * 13, hand[1] + math.sin(aa) * 13 + 2), fill=(255, 240, 220, 230))
        if pose == "attack2" and 2 <= n <= 5:  # 외침 파동
            rr = 4 + (n - 2) * 3
            d.arc(R(hx + 7 - rr, hy + 9 - rr, hx + 7 + rr, hy + 9 + rr), 300, 60, fill=(255, 220, 160, 220), width=W)
            d.arc(R(hx - 9 - rr, hy + 9 - rr, hx - 9 + rr, hy + 9 + rr), 120, 240, fill=(255, 220, 160, 220), width=W)
            d.rectangle(R(hx + 1, hy + 10, hx + 4, hy + 12), fill=(110, 30, 30))  # 벌린 입
    d.rectangle(R(hand[0], hand[1], hand[0] + 1, hand[1] + 1), fill=SKIN)
    outline(im, OUTLINE)
    return im


def cutthroat(pose, t):
    return bandit("cutthroat", pose, t)


def archer(pose, t):
    return bandit("archer", pose, t)


def shieldman(pose, t):
    return bandit("shield", pose, t)


def torchman(pose, t):
    return bandit("torch", pose, t)


def chief(pose, t):
    return bandit("chief", pose, t, 2.0)


# ───────────────── 보스 판정·화살 ─────────────────
def chief_cleave():
    """보스 근접(구 갈퀴 ClowAttack 자리): 곡도 큰 베기 초승달 궤적. 64px × 6프레임(16ppu = 4유닛, 원본과 같은 크기)"""
    n, fs = 6, 64
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    sweep = [(200, 230), (190, 290), (185, 345), (190, 360), (230, 360), (290, 360)]
    alpha = [200, 240, 255, 230, 160, 90]
    for f in range(n):
        ox = f * fs
        a0, a1 = sweep[f]
        for w, col in ((7, (150, 40, 40)), (5, (236, 236, 244)), (2, (255, 255, 255))):
            d.arc((ox + 6, 8, ox + 58, 60), a0, a1, fill=col + (alpha[f],), width=w)
        if 2 <= f <= 4:  # 튀는 불똥
            for c in range(4):
                aa = math.radians(a1 - c * 25)
                d.point((ox + 32 + round(math.cos(aa) * 30), 34 + round(math.sin(aa) * 30)), fill=(255, 200, 120, 220))
    path = os.path.join(OUT, "BanditChief", "ChiefCleave.png")
    im.save(path)
    print("ok", path)


def chief_axe():
    """보스 원거리(구 불꽃 자리): 회전하며 날아가는 손도끼. 32px × 3프레임(12.5ppu)"""
    n, fs = 3, 32
    im = Image.new("RGBA", (fs * n, fs), (0, 0, 0, 0))
    for f in range(n):
        one = Image.new("RGBA", (fs, fs), (0, 0, 0, 0))
        d = ImageDraw.Draw(one)
        d.line([(9, 22), (22, 9)], fill=WOOD + (255,), width=2)
        d.polygon([(19, 6), (27, 8), (26, 15), (21, 12)], fill=STEEL + (255,))
        d.line([(27, 8), (26, 15)], fill=STEEL_L + (255,))
        d.point((21, 11), fill=STEEL_D + (255,))
        for c in range(3):  # 회전 잔상
            d.point((6 - c, 25 + c), fill=(255, 230, 200, 160 - c * 40))
        outline(one, OUTLINE)
        im.alpha_composite(one.rotate(-f * 120, resample=Image.NEAREST, center=(16, 16)), (f * fs, 0))
    path = os.path.join(OUT, "BanditChief", "ChiefAxe.png")
    im.save(path)
    print("ok", path)


def arrow():
    """산적 궁수 화살(+x 방향) — 몹 화살과 Bandit Shortbow 화살이 같이 쓴다. 32×32 한 장(가로 화살)"""
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.line([(4, 16), (25, 16)], fill=(196, 160, 104, 255))
    d.line([(5, 17), (24, 17)], fill=(140, 104, 60, 255))
    d.polygon([(25, 13), (30, 16), (30, 17), (25, 20)], fill=STEEL + (255,))
    d.point((28, 16), fill=STEEL_L + (255,))
    for c in range(3):  # 깃
        d.line([(3 + c, 13 + c), (6 + c, 16)], fill=(220, 70, 60, 255))
        d.line([(3 + c, 20 - c), (6 + c, 17)], fill=(170, 50, 44, 255))
    outline(im, OUTLINE)
    folder = os.path.join(OUT, "BanditArcher")
    os.makedirs(folder, exist_ok=True)
    path = os.path.join(folder, "Arrow.png")
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
    build("BanditCutthroat", cutthroat)
    build("BanditArcher", archer)
    build("BanditShieldman", shieldman)
    build("BanditTorchbearer", torchman)
    build("BanditChief", chief, attack2=True)
    chief_cleave()
    chief_axe()
    arrow()
    # 미리보기(검수용, 커밋 안 함): Idle·Run·Attack1(+보스 Attack2) 시트를 4배로
    rows = [(n, a) for n in ("BanditCutthroat", "BanditArcher", "BanditShieldman", "BanditTorchbearer", "BanditChief") for a in ("Idle", "Run", "Attack1")]
    rows.append(("BanditChief", "Attack2"))
    sheets = [Image.open(os.path.join(OUT, n, a + ".png")) for n, a in rows]
    sheets.append(Image.open(os.path.join(OUT, "BanditChief", "ChiefCleave.png")))
    Z = 3
    Wd = max(s.width for s in sheets) * Z
    Ht = sum(s.height for s in sheets) * Z
    prev = Image.new("RGBA", (Wd, Ht), (70, 60, 52, 255))
    y = 0
    for s in sheets:
        prev.alpha_composite(s.resize((s.width * Z, s.height * Z), Image.NEAREST), (0, y))
        y += s.height * Z
    prev.save(os.path.join(os.path.dirname(__file__), "..", "..", "Logs", "bandit_mobs_preview.png"))
