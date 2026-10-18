"""1층계 ④구역 '늪지' 몬스터 도트 (TowerWorld 14절, LineageGear 1절).
낮: 개구리(점프·혀) · 꽃게(집게) · 악어(큰 근접) / 밤(습지 안개): 모기(비행 흡혈) · 슬라임(분열) / 보스: 거대 슬라임.
보유 에셋에 없는 몹이라 코드로 그린다(SPUM 금지, 도트 필수). 오른쪽을 본다, 발은 프레임 바닥에서 4px 위.
프레임: 개구리·꽃게·모기·슬라임 48px, 악어 64px, 거대 슬라임 96px(슬라임을 2배 좌표로 다시 그려 도트 밀도 유지).
출력: Assets/Art/Monsters/<Name>/<Anim>.png (가로 시트) — Tools/probes/build_swamp_mobs.cs 가 슬라이스·프리팹화.
기법은 gen_deep_forest_mobs.py 와 같다(몸색 칠 → 위 밝게/아래 어둡게 → 1px 외곽선)."""
import math
import os

from PIL import Image, ImageDraw

from gen_forest_mobs import shade, outline, fade
from gen_deep_forest_mobs import frame, ph_of, anim_frames, death_frames, hit_frames, write

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")


# ───────────────── 개구리 (48px) ─────────────────
FR_BASE, FR_LIGHT, FR_DARK = (92, 150, 64), (132, 190, 86), (54, 98, 40)
FR_BELLY, FR_OUT, FR_TONGUE = (220, 214, 140), (24, 36, 18), (230, 96, 120)


def frog(pose, t):
    F, G = 48, 44
    im = frame(F)
    d = ImageDraw.Draw(im)
    hop, lunge, tongue, stretch = 0, 0, 0, 0
    if pose == "idle":
        stretch = round(math.sin(t * math.tau))  # 목 울대 숨쉬기
    elif pose == "run":  # 깡충 뛰기: 웅크림 → 도약 → 착지
        k = ph_of(t, 6)
        hop = [0, 3, 8, 9, 5, 0][k]
        stretch = [-2, 1, 2, 2, 0, -1][k]
    elif pose == "attack":  # 몸을 낮췄다가 혀를 쭉 뻗어 당긴다
        k = ph_of(t, 6)
        lunge = [0, -2, 2, 3, 2, 0][k]
        tongue = [0, 0, 10, 16, 8, 0][k]
    elif pose == "hit":
        lunge = -3
    cx = 20 + lunge
    by = G - 6 - hop
    # 뒷다리(접힌 허벅지) — 도약 중에는 뒤로 뻗는다
    if hop > 4:
        d.polygon([(cx - 8, by + 1), (cx - 16, by + 6), (cx - 18, by + 8), (cx - 10, by + 5)], fill=FR_DARK)
    else:
        d.ellipse((cx - 13, by - 4, cx - 1, by + 6), fill=FR_BASE)
        d.rectangle((cx - 14, G - 1, cx - 4, G), fill=FR_DARK)
    # 몸통 (납작한 타원) + 배
    h = 9 + stretch
    d.ellipse((cx - 12, by - h, cx + 12, by + 5), fill=FR_BASE)
    d.ellipse((cx - 4, by - 3, cx + 11, by + 5), fill=FR_BELLY)
    # 앞다리
    fy = G if hop == 0 else by + 8
    d.line([(cx + 6, by + 2), (cx + 8, fy)], fill=FR_BASE, width=2)
    shade(im, FR_LIGHT, FR_DARK, FR_BASE)
    # 튀어나온 눈 2개
    for ex in (cx + 2, cx + 8):
        d.ellipse((ex - 3, by - h - 4, ex + 3, by - h + 2), fill=FR_LIGHT)
        d.rectangle((ex, by - h - 2, ex + 1, by - h - 1), fill=(20, 20, 10))
    # 등 무늬(짙은 반점)
    for (x, y) in ((cx - 6, by - 4), (cx - 1, by - 6), (cx - 8, by)):
        d.rectangle((x, y, x + 1, y + 1), fill=FR_DARK)
    # 입선 + 혀
    my = by - 1
    d.line([(cx + 6, my), (cx + 12, my)], fill=FR_OUT)
    if tongue:
        d.line([(cx + 12, my), (cx + 12 + tongue, my - 1)], fill=FR_TONGUE, width=2)
        d.ellipse((cx + 11 + tongue, my - 3, cx + 15 + tongue, my + 1), fill=FR_TONGUE)
    outline(im, FR_OUT)
    return im


# ───────────────── 꽃게 (48px) ─────────────────
CR_BASE, CR_LIGHT, CR_DARK = (196, 84, 56), (232, 130, 90), (130, 48, 34)
CR_OUT, CR_EYE = (40, 16, 12), (20, 14, 10)


def crab(pose, t):
    F, G = 48, 44
    im = frame(F)
    d = ImageDraw.Draw(im)
    leg_ph, lunge, claw_up, snap = 0.0, 0, 0, 0
    if pose == "idle":
        claw_up = round(math.sin(t * math.tau) * 1)
    elif pose == "run":  # 옆걸음 — 다리를 번갈아 든다
        leg_ph = t * math.tau
    elif pose == "attack":  # 집게를 들었다가 앞으로 연타
        k = ph_of(t, 6)
        claw_up = [0, 4, 6, 0, 5, 0][k]
        snap = [0, 0, 0, 1, 0, 1][k]
        lunge = [0, -1, -1, 3, 1, 3][k]
    elif pose == "hit":
        lunge = -3
    cx, by = 22 + lunge, G - 8
    # 다리 (양쪽 3개씩)
    for i in range(3):
        for side in (-1, 1):
            lift = round(max(0.0, math.sin(leg_ph + i * 1.3 + (0 if side < 0 else math.pi))) * 3) if pose == "run" else 0
            x0 = cx + side * (6 + i * 3)
            d.line([(x0, by + 2), (x0 + side * 4, by + 4 - lift), (x0 + side * 5, G - lift)], fill=CR_DARK, width=1)
    # 등딱지
    d.ellipse((cx - 13, by - 9, cx + 13, by + 5), fill=CR_BASE)
    shade(im, CR_LIGHT, CR_DARK, CR_BASE)
    for x in range(cx - 8, cx + 9, 4):  # 등딱지 돌기
        d.point((x, by - 7), fill=CR_LIGHT)
    # 눈자루
    for ex in (cx + 3, cx + 8):
        d.line([(ex, by - 9), (ex, by - 13)], fill=CR_DARK)
        d.rectangle((ex - 1, by - 15, ex + 1, by - 13), fill=CR_EYE)
    # 집게 2개 (앞쪽이 크다)
    for i, (px, sz) in enumerate(((cx + 17, 7), (cx + 12, 5))):
        py = by - 11 - claw_up + i * 4
        d.line([(cx + 9, by - 2), (px - 3, py + 3)], fill=CR_DARK, width=2)
        d.ellipse((px - sz // 2, py - sz // 2, px + sz, py + sz // 2 + 1), fill=CR_BASE if i == 0 else CR_DARK)
        gap = 0 if snap else 2
        d.polygon([(px + sz - 1, py - 1 - gap), (px + sz + 4, py - 2 - gap), (px + sz + 2, py)], fill=CR_LIGHT)
        d.polygon([(px + sz - 1, py + 1 + gap), (px + sz + 4, py + 2 + gap), (px + sz + 2, py)], fill=CR_DARK)
    if snap:  # 집게 연타 섬광
        sx, sy = cx + 26, by - 6 - claw_up
        d.line([(sx - 2, sy), (sx + 2, sy)], fill=(255, 240, 220, 220))
        d.line([(sx, sy - 2), (sx, sy + 2)], fill=(255, 240, 220, 220))
    outline(im, CR_OUT)
    return im


# ───────────────── 악어 (64px) ─────────────────
CC_BASE, CC_LIGHT, CC_DARK = (82, 104, 60), (118, 142, 80), (48, 64, 36)
CC_BELLY, CC_OUT, CC_TEETH, CC_EYE = (176, 170, 112), (18, 24, 14), (240, 236, 214), (230, 200, 60)


def croc(pose, t):
    F, G = 64, 60
    im = frame(F)
    d = ImageDraw.Draw(im)
    legs = [0, 0, 0, 0]
    jaw, lunge, tail = 0, 0, 0
    if pose == "idle":
        tail = round(math.sin(t * math.tau) * 2)
    elif pose == "run":
        ph = t * math.tau
        legs = [round(3 * math.sin(ph + i * math.pi / 2)) for i in range(4)]
        tail = round(math.sin(ph) * 3)
    elif pose == "attack":  # 입을 크게 벌렸다가 앞으로 덥석 물고 흔든다
        k = ph_of(t, 6)
        jaw = [0, 4, 8, 1, 0, 3][k]
        lunge = [0, -1, -2, 3, 4, 2][k]
        tail = [0, 2, 3, -3, -2, 0][k]
    elif pose == "hit":
        lunge = -3
    bx, by = 2 + lunge, G - 10
    # 꼬리 (왼쪽으로 가늘게)
    d.polygon([(bx + 14, by - 6), (bx, by - 2 + tail), (bx - 4, by + tail), (bx + 14, by + 2)], fill=CC_BASE)
    # 다리 (짧고 굵게)
    for i, lx in enumerate((bx + 14, bx + 21, bx + 29, bx + 36)):
        x = lx + legs[i]
        d.rectangle((x, by + 1, x + 4, G), fill=CC_DARK if i % 2 == 0 else CC_BASE)
    # 몸통 (낮고 길게)
    d.rounded_rectangle((bx + 10, by - 8, bx + 42, by + 4), radius=4, fill=CC_BASE)
    d.rectangle((bx + 14, by + 1, bx + 40, by + 4), fill=CC_BELLY)
    # 머리 + 위턱/아래턱 (jaw 만큼 벌어짐)
    hx, hy = bx + 40, by - 4
    d.polygon([(hx, hy - 4), (hx + 6, hy - 6), (hx + 16, hy - 4 - jaw // 2), (hx + 17, hy - 1 - jaw // 2), (hx, hy)], fill=CC_BASE)
    d.polygon([(hx, hy + 1), (hx + 16, hy + 1 + jaw // 2), (hx + 15, hy + 4 + jaw // 2), (hx, hy + 5)], fill=CC_BASE)
    shade(im, CC_LIGHT, CC_DARK, CC_BASE)
    # 등 비늘 돌기
    for x in range(bx + 12, bx + 40, 4):
        d.polygon([(x, by - 8), (x + 2, by - 11), (x + 3, by - 8)], fill=CC_DARK)
    # 눈 (튀어나온 노란 눈)
    d.ellipse((hx + 3, hy - 9, hx + 7, hy - 5), fill=CC_LIGHT)
    d.rectangle((hx + 5, hy - 8, hx + 6, hy - 7), fill=CC_EYE)
    # 이빨
    for x in range(hx + 4, hx + 16, 3):
        d.point((x, hy - 1 - (jaw // 2 if jaw else 0) + 1), fill=CC_TEETH)
        d.point((x + 1, hy + 1 + (jaw // 2 if jaw else 0)), fill=CC_TEETH)
    if jaw >= 4:  # 벌린 입 안쪽
        d.polygon([(hx + 2, hy - 1), (hx + 14, hy - 1 - jaw // 2 + 2), (hx + 14, hy + jaw // 2), (hx + 2, hy + 1)], fill=(150, 50, 50))
    outline(im, CC_OUT)
    if pose == "attack" and lunge >= 3:  # 물어 흔들기 궤적
        d2 = ImageDraw.Draw(im)
        for c in range(2):
            d2.arc((hx + 4 + c * 3, hy - 10, hx + 18 + c * 3, hy + 10), 300, 60, fill=(255, 250, 230, 200))
    return im


# ───────────────── 모기 (밤, 48px, 비행) ─────────────────
MQ_BASE, MQ_LIGHT, MQ_DARK = (96, 84, 76), (140, 126, 112), (56, 48, 44)
MQ_OUT, MQ_EYE, MQ_BLOOD = (24, 20, 20), (210, 40, 40), (200, 30, 40)


def mosquito(pose, t):
    F = 48
    im = frame(F)
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    cx, cy = 22, 20 + round(math.sin(ph) * 2)
    dive = 0
    if pose == "attack":  # 뒤로 움찔했다가 주둥이로 돌진
        k = ph_of(t, 6)
        dive = [0, -3, -4, 6, 9, 3][k]
        cy += [0, -1, -2, 2, 3, 1][k]
    if pose == "hit":
        cx -= 3
    cx += dive
    flap = (int(t * 12) % 2) if pose != "hit" else 0
    # 날개(반투명, 빠른 날갯짓 2프레임)
    wing = (210, 226, 236, 140)
    if flap:
        d.polygon([(cx - 1, cy - 3), (cx - 9, cy - 13), (cx - 3, cy - 12), (cx + 2, cy - 3)], fill=wing)
    else:
        d.polygon([(cx - 1, cy - 3), (cx - 13, cy - 7), (cx - 11, cy - 3), (cx + 1, cy - 1)], fill=wing)
    # 배(부풀어 오른 붉은 배 — 흡혈) + 가슴 + 머리
    d.ellipse((cx - 12, cy - 2, cx - 1, cy + 4), fill=MQ_BASE)
    d.ellipse((cx - 10, cy, cx - 4, cy + 4), fill=MQ_BLOOD)
    d.ellipse((cx - 2, cy - 4, cx + 5, cy + 2), fill=MQ_BASE)
    d.ellipse((cx + 4, cy - 4, cx + 9, cy + 1), fill=MQ_BASE)
    shade(im, MQ_LIGHT, MQ_DARK, MQ_BASE)
    for x in range(cx - 10, cx - 2, 3):  # 배 줄무늬
        d.line([(x, cy - 1), (x, cy + 3)], fill=MQ_DARK)
    d.rectangle((cx + 6, cy - 3, cx + 7, cy - 2), fill=MQ_EYE)
    # 긴 주둥이(침)
    plen = 9 + (3 if pose == "attack" and dive > 4 else 0)
    d.line([(cx + 9, cy - 1), (cx + 9 + plen, cy + 2)], fill=MQ_DARK)
    # 늘어진 다리 6개
    for i in range(3):
        x = cx - 2 + i * 3
        sw = round(math.sin(ph + i) * 1)
        d.line([(x, cy + 2), (x - 2 + sw, cy + 8), (x - 3 + sw, cy + 12)], fill=MQ_DARK)
    outline(im, MQ_OUT)
    return im


# ───────────────── 슬라임 / 거대 슬라임 ─────────────────
SLIME = dict(base=(96, 168, 120), light=(150, 216, 160), dark=(52, 110, 80), core=(200, 240, 200), out=(20, 46, 32), eye=(16, 30, 22))
GIANT = dict(base=(84, 120, 70), light=(128, 162, 96), dark=(48, 74, 42), core=(196, 214, 120), out=(16, 26, 14), eye=(240, 210, 70))


def slime(pose, t, k=1.0, pal=SLIME):
    F = round(48 * k)
    G = F - 4
    im = frame(F)
    d = ImageDraw.Draw(im)

    def P(x, y):
        return (round(x * k), round(y * k))

    sq, jump, lunge, splash = 0, 0, 0, 0
    if pose == "idle":
        sq = round(math.sin(t * math.tau) * 1.5)
    elif pose == "run":  # 출렁이며 미끄러진다
        n = ph_of(t, 6)
        sq = [2, 0, -2, -3, -1, 1][n]
        jump = [0, 1, 3, 3, 1, 0][n]
    elif pose == "attack":  # 웅크렸다가 뛰어올라 덮친다
        n = ph_of(t, 6)
        sq = [3, 4, -2, -2, 5, 1][n]
        jump = [0, 0, 6, 8, 0, 0][n]
        lunge = [0, -1, 3, 6, 7, 3][n]
        splash = [0, 0, 0, 0, 2, 1][n]
    elif pose == "attack2":  # (보스) 높이 뛰어 내려찍으면 양옆으로 점액 파도
        n = ph_of(t, 8)
        sq = [3, 5, -3, -2, -2, 6, 3, 1][n]
        jump = [0, 0, 4, 7, 5, 0, 0, 0][n]
        splash = [0, 0, 0, 0, 0, 3, 4, 2][n]
    elif pose == "hit":
        lunge, sq = -2, 2
    cx = 22 + lunge
    w = 13 + sq // 2   # 눌리면 넓고 낮게
    h = 14 - sq
    bottom = 44 - jump
    # 몸 (아래가 평평한 물방울)
    d.ellipse([P(cx - w, bottom - h * 2), P(cx + w, bottom + 2)], fill=pal["base"])
    d.rectangle([P(cx - w + 1, bottom - 3), P(cx + w - 1, bottom)], fill=pal["base"])
    shade(im, pal["light"], pal["dark"], pal["base"])
    # 속에 비치는 핵·거품
    d.ellipse([P(cx - 3, bottom - h - 2), P(cx + 4, bottom - h + 4)], fill=pal["core"] + (170,))
    for (bx, by) in ((cx - 7, bottom - 6), (cx + 6, bottom - h), (cx - 4, bottom - h * 2 + 5)):
        d.rectangle([P(bx, by), P(bx + 1, by + 1)], fill=pal["light"])
    # 하이라이트
    d.line([P(cx - w + 4, bottom - h * 2 + 6), P(cx - w + 7, bottom - h * 2 + 3)], fill=(240, 255, 240), width=max(1, round(k)))
    # 눈·입
    ey = bottom - h - 1
    d.rectangle([P(cx + 3, ey), P(cx + 4, ey + 2)], fill=pal["eye"])
    d.rectangle([P(cx + 8, ey), P(cx + 9, ey + 2)], fill=pal["eye"])
    if pose in ("attack", "attack2") and jump > 0:
        d.ellipse([P(cx + 4, ey + 4), P(cx + 9, ey + 7)], fill=pal["out"])
    if k > 1.5:  # 거대 슬라임: 왕관처럼 솟은 점액 뿔 + 삼켜진 뼈·잎 잔해
        for c in (-6, 0, 6):
            tx = cx + c
            d.polygon([P(tx - 2, bottom - h * 2 + 1), P(tx, bottom - h * 2 - 4), P(tx + 2, bottom - h * 2 + 1)], fill=pal["base"])
        d.line([P(cx - 8, bottom - 8), P(cx - 3, bottom - 10)], fill=(220, 214, 190), width=round(k))
        d.ellipse([P(cx + 2, bottom - 7), P(cx + 5, bottom - 5)], fill=(150, 120, 60))
    outline(im, pal["out"])
    if splash:  # 착지 점액 튐
        d2 = ImageDraw.Draw(im)
        for s in (-1, 1):
            for c in range(splash):
                x = cx + s * (w + 3 + c * 4)
                d2.ellipse([P(x - 1, 42 - c * 2), P(x + 1, 44 - c * 2)], fill=pal["light"])
    return im


def giant_slime(pose, t):
    return slime(pose, t, 2.0, GIANT)


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
    build("SwampFrog", frog)
    build("MarshCrab", crab)
    build("BogCroc", croc)
    build("SwampMosquito", mosquito)
    build("BogSlime", slime)
    build("GiantSlime", giant_slime, attack2=True)
    # 미리보기(검수용, 커밋 안 함): Run·Attack1 시트를 3배로
    rows = [(n, a) for n in ("SwampFrog", "MarshCrab", "BogCroc", "SwampMosquito", "BogSlime", "GiantSlime") for a in ("Run", "Attack1")]
    sheets = [Image.open(os.path.join(OUT, n, a + ".png")) for n, a in rows]
    W = max(s.width for s in sheets) * 3
    H = sum(s.height for s in sheets) * 3
    prev = Image.new("RGBA", (W, H), (52, 58, 44, 255))
    y = 0
    for s in sheets:
        prev.alpha_composite(s.resize((s.width * 3, s.height * 3), Image.NEAREST), (0, y))
        y += s.height * 3
    prev.save(os.path.join(os.path.dirname(__file__), "..", "..", "Logs", "swamp_mobs_preview.png"))
