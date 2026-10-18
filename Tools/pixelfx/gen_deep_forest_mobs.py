"""1층계 ③구역 '깊은 숲' 몬스터 도트 (TowerWorld 4·13절): 숲곰 · 엔트(낮) · 유령 · 요정(밤) · 고대숲의 엔트(보스).
보유 에셋에 없는 몹이라 코드로 그린다(사용자 지시: 도트 필수, SPUM 금지). 오른쪽을 본다, 발은 프레임 바닥에서 4px 위.
프레임: 곰·엔트 64px, 유령·요정 48px, 고대숲의 엔트 96px(엔트를 1.5배 좌표로 다시 그려 도트 밀도 유지).
출력: Assets/Art/Monsters/<Name>/<Anim>.png (가로 시트) — Tools/probes/build_deep_forest_mobs.cs 가 슬라이스·프리팹화.
기법은 gen_forest_mobs.py 와 같다(몸색 칠 → 위 밝게/아래 어둡게 → 1px 외곽선)."""
import math
import os
import random
from PIL import Image, ImageDraw

from gen_forest_mobs import shade, outline, fade, tint_white

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")


def frame(f):
    return Image.new("RGBA", (f, f), (0, 0, 0, 0))


def ph_of(t, n):
    return int(t * n + 1e-6) % n


# ───────────────── 숲곰 (64px) ─────────────────
BR_BASE, BR_LIGHT, BR_DARK = (110, 72, 44), (150, 104, 64), (70, 44, 28)
BR_MUZ, BR_OUT, BR_CLAW, BR_EYE = (176, 136, 96), (30, 20, 14), (236, 230, 214), (20, 14, 10)


def bear(pose, t):
    F, G = 64, 60
    im = frame(F)
    d = ImageDraw.Draw(im)
    bob, lunge, rear, swipe = 0, 0, 0, 0
    legs = [0, 0, 0, 0]
    if pose == "idle":
        bob = round(math.sin(t * math.tau))
    elif pose == "run":
        ph = t * math.tau
        bob = -round(abs(math.sin(ph)) * 2)
        legs = [round(4 * math.sin(ph)), round(4 * math.sin(ph + math.pi)), round(4 * math.sin(ph + math.pi)), round(4 * math.sin(ph))]
    elif pose == "attack":  # 일어섰다가 앞발로 내려찍는 할퀴기
        k = ph_of(t, 6)
        rear = [0, 5, 9, 2, 0, 0][k]
        lunge = [0, -2, -3, 5, 6, 2][k]
        swipe = [0, 0, 0, 1, 2, 0][k]
    elif pose == "hit":
        lunge = -3
    by = 40 + bob
    bx = 8 + lunge
    # 뒷다리(먼 쪽, 어둡게)
    for i, lx in enumerate((bx + 34, bx + 8)):
        x = lx + legs[i * 2 + 1]
        d.rectangle((x, by + 4, x + 5, G), fill=BR_DARK)
    # 몸통 (어깨 혹이 솟은 덩치) — rear 만큼 앞쪽이 들린다
    d.polygon([(bx, by + 2), (bx + 4, by - 10), (bx + 18, by - 16), (bx + 30, by - 18 - rear), (bx + 40, by - 12 - rear),
               (bx + 44, by - 2 - rear // 2), (bx + 40, by + 9), (bx + 6, by + 10)], fill=BR_BASE)
    # 앞다리(가까운 쪽) — 일어서면 앞발이 들리고 할퀴기 때 앞으로 뻗는다
    fx = bx + 36 + legs[0]
    if rear > 0 or swipe > 0:
        px_, py_ = bx + 42 + swipe * 4, by - 6 - rear + swipe * 6
        d.polygon([(bx + 34, by - 6 - rear), (bx + 40, by - 8 - rear), (px_ + 4, py_), (px_, py_ + 4)], fill=BR_BASE)
        d.ellipse((px_ - 2, py_ - 2, px_ + 5, py_ + 5), fill=BR_BASE)
        for c in range(3):  # 발톱
            d.line((px_ + 4, py_ + c * 2, px_ + 7, py_ + c * 2 + 1), fill=BR_CLAW)
        d.rectangle((bx + 30, by + 4, bx + 35, G), fill=BR_BASE)  # 버티는 다른 앞다리
    else:
        d.rectangle((fx, by + 4, fx + 5, G), fill=BR_BASE)
        d.rectangle((fx - 1, G - 1, fx + 6, G), fill=BR_DARK)
        for c in range(3):
            d.point((fx + 2 + c * 2, G), fill=BR_CLAW)
    x = bx + 10 + legs[2]
    d.rectangle((x, by + 4, x + 6, G), fill=BR_BASE)
    d.rectangle((x - 1, G - 1, x + 7, G), fill=BR_DARK)
    # 머리
    hx, hy = bx + 42, by - 14 - rear
    d.ellipse((hx - 6, hy - 7, hx + 8, hy + 7), fill=BR_BASE)
    d.ellipse((hx - 5, hy - 10, hx - 1, hy - 5), fill=BR_BASE)  # 귀
    d.polygon([(hx + 4, hy - 2), (hx + 13, hy), (hx + 13, hy + 5), (hx + 4, hy + 6)], fill=BR_MUZ)
    shade(im, BR_LIGHT, BR_DARK, BR_BASE)
    d.point((hx + 13, hy), fill=BR_OUT)  # 코
    d.point((hx + 13, hy + 1), fill=BR_OUT)
    d.rectangle((hx + 3, hy - 3, hx + 4, hy - 2), fill=BR_EYE)
    if pose == "attack" and rear > 0:  # 포효 — 입을 벌린다
        d.line((hx + 7, hy + 4, hx + 12, hy + 4), fill=(150, 40, 40))
        d.point((hx + 9, hy + 3), fill=BR_CLAW)
    # 할퀴기 궤적
    if swipe:
        sx, sy = bx + 48 + swipe * 3, by - 10
        for c in range(3):
            d.arc((sx - 8 + c * 2, sy - 6 + c * 3, sx + 10 + c * 2, sy + 14 + c * 3), 300, 60, fill=(255, 250, 235, 220))
    outline(im, BR_OUT)
    return im


# ───────────────── 엔트 / 고대숲의 엔트 ─────────────────
ENT = dict(bark=(104, 78, 52), light=(140, 108, 72), dark=(66, 48, 32), leaf=(70, 128, 58), leaf_l=(110, 168, 80),
           leaf_d=(42, 88, 40), eye=(220, 255, 120), out=(26, 22, 16), moss=None, flower=None)
ANCIENT = dict(bark=(92, 84, 72), light=(130, 122, 104), dark=(56, 50, 44), leaf=(46, 104, 92), leaf_l=(84, 150, 128),
               leaf_d=(28, 70, 64), eye=(255, 170, 60), out=(18, 20, 18), moss=(96, 140, 70), flower=(230, 150, 200))


def ent(pose, t, k=1.0, pal=ENT):
    F = round(64 * k)
    G = F - 4
    im = frame(F)
    d = ImageDraw.Draw(im)

    def P(x, y):
        return (round(x * k), round(y * k))

    def R(x0, y0, x1, y1):
        return (round(x0 * k), round(y0 * k), round(x1 * k), round(y1 * k))

    sway, arm, lunge, roots = 0, 0, 0, 0
    step = [0, 0]
    if pose == "idle":
        sway = round(math.sin(t * math.tau) * 1)
    elif pose == "run":
        ph = t * math.tau
        step = [round(3 * math.sin(ph)), round(3 * math.sin(ph + math.pi))]
        sway = round(math.sin(ph) * 1)
    elif pose == "attack":  # 가지 팔을 들었다 내려찍으면 앞쪽 땅에서 뿌리가 솟는다
        n = ph_of(t, 6)
        arm = [0, -8, -12, 6, 10, 4][n]
        lunge = [0, -1, -2, 2, 3, 1][n]
        roots = [0, 0, 0, 1, 3, 2][n]
    elif pose == "attack2":  # 두 팔을 치켜들고 넓게 뿌리 폭발 (보스 전용)
        n = ph_of(t, 8)
        arm = [0, -10, -14, -14, 8, 10, 8, 2][n]
        roots = [0, 0, 0, 0, 2, 4, 4, 2][n]
    elif pose == "hit":
        lunge = -2
    bx = 18 + lunge
    top = 14
    # 다리(뿌리 발) 2개
    for i, lx in enumerate((bx + 4, bx + 16)):
        x = lx + step[i]
        d.polygon([P(x, 44), P(x + 7, 44), P(x + 8, 60), P(x + 9, 60), P(x - 2, 60)], fill=pal["dark"] if i == 0 else pal["bark"])
    # 몸통(줄기)
    d.polygon([P(bx + 2, 46), P(bx + 1, top + 10 + sway), P(bx + 6, top + 6), P(bx + 22, top + 6), P(bx + 26, top + 12 + sway), P(bx + 25, 46)], fill=pal["bark"])
    # 뒷팔(먼 쪽)
    ax, ay = bx + 2, top + 16
    d.line([P(ax, ay), P(ax - 8, ay + 10 + arm // 2), P(ax - 10, ay + 18 + arm // 2)], fill=pal["dark"], width=max(2, round(3 * k)))
    shade(im, pal["light"], pal["dark"], pal["bark"])
    # 나무껍질 결
    for c in range(4):
        x = bx + 6 + c * 5
        d.line([P(x, top + 14 + (c % 2) * 4), P(x + (c % 2), 42)], fill=pal["dark"])
    # 얼굴: 옹이 구멍 눈 + 입
    ex, ey = bx + 16, top + 18 + sway
    d.rectangle(R(ex, ey, ex + 2, ey + 2), fill=(20, 16, 10))
    d.rectangle(R(ex + 6, ey, ex + 8, ey + 2), fill=(20, 16, 10))
    d.point(P(ex + 1, ey + 1), fill=pal["eye"])
    d.point(P(ex + 7, ey + 1), fill=pal["eye"])
    if k > 1.2:
        d.rectangle(R(ex, ey, ex + 2, ey + 2), fill=pal["eye"])
        d.rectangle(R(ex + 6, ey, ex + 8, ey + 2), fill=pal["eye"])
    d.line([P(ex + 1, ey + 7), P(ex + 7, ey + 7 + (2 if pose in ("attack", "attack2") and arm > 0 else 0))], fill=(30, 22, 14), width=max(1, round(k)))
    # 앞팔(가까운 쪽) — arm<0 이면 들어올림, >0 이면 앞으로 내려찍음
    sx, sy = bx + 22, top + 16
    ex2, ey2 = sx + 10 + max(0, arm), sy + 10 + arm
    d.line([P(sx, sy), P(sx + 6, sy + 4 + arm // 2), P(ex2, ey2)], fill=pal["bark"], width=max(2, round(3 * k)))
    for c in (-3, 0, 3):  # 손가락 잔가지
        d.line([P(ex2, ey2), P(ex2 + 3, ey2 + c)], fill=pal["dark"])
    # 수관(잎) — 몸 위
    random.seed(7)
    blobs = [(bx - 4, top - 4, 14), (bx + 8, top - 10, 16), (bx + 20, top - 4, 14), (bx + 2, top + 2, 12), (bx + 16, top + 2, 12)]
    for (x, y, r) in blobs:
        d.ellipse(R(x, y + sway, x + r, y + r + sway), fill=pal["leaf"])
    shade(im, pal["leaf_l"], pal["leaf_d"], pal["leaf"])
    for _ in range(round(14 * k)):  # 잎 점묘
        x = random.randint(round((bx - 2) * k), round((bx + 32) * k))
        y = random.randint(round((top - 8) * k), round((top + 10) * k))
        if im.getpixel((x, y))[3]:
            d.point((x, y), fill=pal["leaf_l"] if random.random() < 0.5 else pal["leaf_d"])
    if pal["moss"]:  # 고대 엔트: 이끼·돌 무늬·꽃
        for c in range(6):
            x, y = bx + 3 + c * 4, 30 + (c * 7) % 12
            d.rectangle(R(x, y, x + 2, y + 1), fill=pal["moss"])
        for (x, y) in ((bx + 2, top - 2), (bx + 14, top - 8), (bx + 26, top), (bx + 8, top + 6)):
            d.rectangle(R(x, y, x + 1, y + 1), fill=pal["flower"])
    # 뿌리 공격: 앞쪽 땅에서 가시 뿌리가 솟는다
    if roots:
        base_x = bx + 30
        count = 3 if pose == "attack" else 5
        for c in range(count):
            rx = base_x + c * 6
            h = roots * (3 + (c % 2) * 2)
            if pose == "attack2":
                rx = bx - 24 + c * 16 if c != 2 else bx + 40
                h = roots * 3 + 2
            d.polygon([P(rx, 60), P(rx + 2, 60 - h), P(rx + 4, 60)], fill=pal["bark"])
            d.line([P(rx + 2, 60 - h), P(rx + 2, 59)], fill=pal["light"])
    outline(im, pal["out"])
    return im


def ancient_ent(pose, t):
    return ent(pose, t, 1.5, ANCIENT)


# ───────────────── 유령 (밤, 48px, 반투명) ─────────────────
def ghost(pose, t):
    F = 48
    im = frame(F)
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    cy = 22 + round(math.sin(ph) * 2)
    cx = 22
    mouth = 0
    if pose == "attack":
        n = ph_of(t, 6)
        cx += [0, -2, -3, 5, 7, 2][n]
        mouth = [0, 1, 2, 3, 3, 1][n]
    if pose == "hit":
        cx -= 3
    body, light, dark = (200, 220, 236), (236, 246, 255), (140, 160, 190)
    # 머리 + 몸 (아래로 갈수록 가늘고 흔들리는 꼬리)
    d.ellipse((cx - 9, cy - 10, cx + 9, cy + 8), fill=body)
    for k in range(6):
        w = 9 - k
        y = cy + 4 + k * 3
        off = round(math.sin(ph * 2 + k * 0.9) * 2) - k
        d.ellipse((cx - w + off, y - 2, cx + w + off, y + 2), fill=body)
    # 늘어진 팔(앞쪽으로)
    reach = 3 if pose == "attack" and mouth >= 2 else 0
    d.polygon([(cx + 5, cy + 3), (cx + 10 + reach, cy + 8), (cx + 8 + reach, cy + 10), (cx + 4, cy + 6)], fill=body)
    shade(im, light, dark, body)
    # 눈·입 구멍
    d.rectangle((cx + 1, cy - 4, cx + 3, cy - 1), fill=(30, 40, 70))
    d.rectangle((cx + 6, cy - 4, cx + 8, cy - 1), fill=(30, 40, 70))
    d.ellipse((cx + 3, cy + 1, cx + 6, cy + 2 + mouth * 2), fill=(30, 40, 70))
    outline(im, (60, 70, 100))
    fade(im, 0.72)  # 반투명
    return im


# ───────────────── 요정 (밤, 48px 프레임 안의 작은 몸) ─────────────────
def fairy(pose, t):
    F = 48
    im = frame(F)
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    cy = 22 + round(math.sin(ph) * 3)
    cx = 24
    if pose == "attack":
        n = ph_of(t, 6)
        cx += [0, -3, -4, 6, 8, 3][n]
    if pose == "hit":
        cx -= 3
    flap = math.sin(ph * 2 if pose != "run" else ph * 3)
    # 날개(반투명 하늘색) — 위아래로 펄럭
    wy = round(flap * 4)
    wing = (180, 236, 255, 150)
    d.polygon([(cx - 1, cy - 2), (cx - 10, cy - 9 + wy), (cx - 11, cy - 3 + wy), (cx - 2, cy + 1)], fill=wing)
    d.polygon([(cx - 1, cy + 1), (cx - 8, cy + 6 - wy // 2), (cx - 4, cy + 7 - wy // 2)], fill=wing)
    # 빛무리는 프리팹의 Light2D 가 맡는다(반투명 원은 어두운 배경에서 탁해 보임)
    # 몸(초록 옷) + 머리(살구) + 머리카락(분홍)
    d.polygon([(cx - 2, cy), (cx + 2, cy), (cx + 3, cy + 6), (cx - 3, cy + 6)], fill=(110, 190, 110, 255))
    d.ellipse((cx - 3, cy - 7, cx + 3, cy - 1), fill=(252, 214, 186, 255))
    d.polygon([(cx - 4, cy - 6), (cx + 2, cy - 9), (cx + 4, cy - 6), (cx - 3, cy - 3)], fill=(244, 140, 190, 255))
    d.point((cx + 2, cy - 4), fill=(40, 30, 60, 255))
    # 지팡이 끝 별빛
    d.line((cx + 2, cy + 2, cx + 6, cy - 1), fill=(200, 160, 90, 255))
    sp = 1 + (1 if pose == "attack" and ph_of(t, 6) in (3, 4) else 0)
    d.rectangle((cx + 6 - sp, cy - 1, cx + 6 + sp, cy - 1), fill=(255, 255, 210, 255))
    d.rectangle((cx + 6, cy - 1 - sp, cx + 6, cy - 1 + sp), fill=(255, 255, 210, 255))
    # 반짝이 꼬리
    for k in range(4):
        x = cx - 6 - k * 3
        y = cy + 4 + round(math.sin(ph * 2 + k) * 2)
        d.point((x, y), fill=(255, 250, 190, 220 - k * 45))
    return im


# ───────────────── 공통 ─────────────────
def anim_frames(fn, pose, n):
    return [fn(pose, i / n) for i in range(n)]


def death_frames(fn, n=5):
    base = fn("idle", 0)
    f = base.size[0]
    out = []
    for i in range(n):
        k = i / (n - 1)
        im = base.rotate(-k * 80, resample=Image.NEAREST, center=(f // 2, f - 4))
        out.append(fade(im, 1 - k * 0.85))
    return out


def hit_frames(fn):
    return [tint_white(fn("hit", 0), 0.7), fn("hit", 0.5), fn("idle", 0)]


def write(name, anim, frames):
    f = frames[0].size[0]
    sheet = Image.new("RGBA", (f * len(frames), f), (0, 0, 0, 0))
    for i, fr in enumerate(frames):
        sheet.paste(fr, (i * f, 0))
    folder = os.path.join(OUT, name)
    os.makedirs(folder, exist_ok=True)
    sheet.save(os.path.join(folder, anim + ".png"))


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
    build("ForestBear", bear)
    build("Ent", ent)
    build("Ghost", ghost)
    build("Fairy", fairy)
    build("AncientEnt", ancient_ent, attack2=True)
    # 미리보기(검수용, 커밋 안 함): Attack1 시트를 3배로
    rows = ("ForestBear", "Ent", "Ghost", "Fairy", "AncientEnt")
    sheets = [Image.open(os.path.join(OUT, n, "Attack1.png")) for n in rows]
    W = max(s.width for s in sheets) * 3
    H = sum(s.height for s in sheets) * 3
    prev = Image.new("RGBA", (W, H), (40, 52, 44, 255))
    y = 0
    for s in sheets:
        prev.alpha_composite(s.resize((s.width * 3, s.height * 3), Image.NEAREST), (0, y))
        y += s.height * 3
    prev.save(os.path.join(os.path.dirname(__file__), "..", "..", "Logs", "deep_forest_mobs_preview.png"))
