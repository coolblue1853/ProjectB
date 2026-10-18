"""1층계 ②구역 '일반 숲' 몬스터 도트 (TowerWorld 4절): 숲늑대 · 멧돼지 · 도깨비불(밤) · 숲달팽이(밤).
보유 에셋에 없는 짐승이라 코드로 그린다(사용자 지시: 도트 필수). 48×48 프레임, 오른쪽을 본다, 발은 바닥 행(44) 기준.
출력: Assets/Art/Monsters/<Name>/<Anim>.png (가로 시트) — Tools/probes/build_forest_mobs.cs 가 슬라이스·프리팹화.
기법: 도형을 '몸색'으로 칠한 뒤 위쪽 밝게/아래쪽 어둡게 명암, 마지막에 알파 경계 1px 외곽선."""
import math
import os
from PIL import Image, ImageDraw

F = 48                     # 프레임 크기(px)
GROUND = 44                # 발 바닥 행
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")


def frame():
    return Image.new("RGBA", (F, F), (0, 0, 0, 0))


def shade(im, light, dark, base):
    """base 색 픽셀만 골라 위쪽 이웃이 비어 있으면 밝게, 아래쪽 이웃이 비어 있으면 어둡게"""
    px = im.load()
    w, h = im.size
    src = im.copy().load()
    for y in range(h):
        for x in range(w):
            if src[x, y][:3] != base:
                continue
            up = src[x, y - 1][3] if y > 0 else 0
            dn = src[x, y + 1][3] if y < h - 1 else 0
            if up == 0:
                px[x, y] = light + (255,)
            elif dn == 0 or (y < h - 2 and src[x, y + 2][3] == 0):
                px[x, y] = dark + (255,)


def outline(im, col):
    px = im.load()
    w, h = im.size
    src = im.copy().load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] != 0:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 0 and src[nx, ny][:3] != col:
                    px[x, y] = col + (255,)
                    break
    return im


def fade(im, a):
    px = im.load()
    for y in range(im.size[1]):
        for x in range(im.size[0]):
            r, g, b, al = px[x, y]
            if al:
                px[x, y] = (r, g, b, int(al * a))
    return im


def tint_white(im, k):
    px = im.load()
    for y in range(im.size[1]):
        for x in range(im.size[0]):
            r, g, b, al = px[x, y]
            if al:
                px[x, y] = (int(r + (255 - r) * k), int(g + (255 - g) * k), int(b + (255 - b) * k), al)
    return im


def rotate_down(im, deg):
    """바닥 중앙을 축으로 기울인다(쓰러짐)"""
    big = im.rotate(deg, resample=Image.NEAREST, center=(F // 2, GROUND), translate=(0, 0))
    return big


# ───────────────── 숲늑대 ─────────────────
W_BASE, W_LIGHT, W_DARK = (112, 108, 116), (156, 152, 160), (70, 66, 78)
W_BELLY, W_OUT, W_EYE = (196, 186, 170), (28, 24, 34), (255, 196, 60)


def wolf(pose, t):
    """pose: idle/run/attack/hit/death, t: 0..1 진행"""
    im = frame()
    d = ImageDraw.Draw(im)
    bob = 0
    lunge = 0
    jaw = 0
    legs = [0, 0, 0, 0]  # 앞앞, 앞뒤, 뒤앞, 뒤뒤 x 오프셋
    lift = [0, 0, 0, 0]
    if pose == "idle":
        bob = round(math.sin(t * math.tau))
    elif pose == "run":
        ph = t * math.tau
        bob = -round(abs(math.sin(ph)) * 2)
        legs = [round(4 * math.sin(ph)), round(4 * math.sin(ph + math.pi)), round(4 * math.sin(ph + math.pi)), round(4 * math.sin(ph))]
        lift = [max(0, round(2 * math.cos(ph))), max(0, round(2 * math.cos(ph + math.pi))), max(0, round(2 * math.cos(ph + math.pi))), max(0, round(2 * math.cos(ph)))]
    elif pose == "attack":
        lunge = round([0, -2, -3, 6, 7, 3][int(t * 6) % 6])
        jaw = [0, 0, 1, 3, 3, 1][int(t * 6) % 6]
        bob = [0, 1, 1, -2, -1, 0][int(t * 6) % 6]
    elif pose == "hit":
        lunge = -3
    by = 30 + bob
    bx = 8 + lunge
    # 꼬리
    d.polygon([(bx + 2, by - 3), (bx - 6, by - 7 + (bob if pose != "run" else round(2 * math.sin(t * math.tau)))), (bx - 4, by - 2), (bx + 2, by + 1)], fill=W_BASE)
    # 다리(뒤쪽 두 개 먼저, 어둡게)
    for i, lx in enumerate((bx + 24, bx + 6)):
        x = lx + legs[i * 2 + 1]
        d.rectangle((x, by + 4, x + 2, GROUND - 1 - lift[i * 2 + 1]), fill=W_DARK)
        d.rectangle((x - 1, GROUND - 1 - lift[i * 2 + 1], x + 2, GROUND - lift[i * 2 + 1]), fill=W_DARK)
    # 몸통
    d.ellipse((bx, by - 7, bx + 28, by + 7), fill=W_BASE)
    d.ellipse((bx + 6, by - 1, bx + 24, by + 7), fill=W_BELLY)
    # 앞쪽 다리
    for i, lx in enumerate((bx + 22, bx + 4)):
        x = lx + legs[i * 2]
        d.rectangle((x, by + 4, x + 2, GROUND - 1 - lift[i * 2]), fill=W_BASE)
        d.rectangle((x - 1, GROUND - 1 - lift[i * 2], x + 3, GROUND - lift[i * 2]), fill=W_BASE)
    # 목·머리
    hx, hy = bx + 26, by - 9
    d.polygon([(bx + 20, by - 6), (hx + 2, hy - 1), (hx + 6, hy + 8), (bx + 22, by + 3)], fill=W_BASE)
    d.ellipse((hx - 1, hy - 4, hx + 10, hy + 6), fill=W_BASE)
    # 귀
    d.polygon([(hx + 1, hy - 3), (hx + 3, hy - 9), (hx + 5, hy - 3)], fill=W_BASE)
    d.polygon([(hx + 5, hy - 3), (hx + 7, hy - 8), (hx + 9, hy - 2)], fill=W_DARK)
    # 주둥이 + 턱
    d.polygon([(hx + 8, hy - 1), (hx + 16, hy + 1), (hx + 16, hy + 3), (hx + 8, hy + 4)], fill=W_BASE)
    d.polygon([(hx + 8, hy + 4 + jaw), (hx + 15, hy + 4 + jaw), (hx + 14, hy + 6 + jaw), (hx + 8, hy + 7 + jaw)], fill=W_BELLY)
    if jaw >= 2:
        d.line((hx + 10, hy + 4, hx + 14, hy + 4), fill=(240, 240, 240))  # 이빨
    d.point((hx + 16, hy + 1), fill=(20, 16, 20))  # 코
    shade(im, W_LIGHT, W_DARK, W_BASE)
    d.point((hx + 8, hy), fill=W_EYE)
    d.point((hx + 9, hy), fill=W_EYE)
    outline(im, W_OUT)
    return im


# ───────────────── 멧돼지 ─────────────────
B_BASE, B_LIGHT, B_DARK = (122, 78, 46), (164, 112, 66), (78, 46, 26)
B_MANE, B_TUSK, B_OUT, B_SNOUT = (58, 36, 24), (236, 226, 196), (30, 18, 10), (196, 128, 110)


def boar(pose, t):
    im = frame()
    d = ImageDraw.Draw(im)
    bob, lunge = 0, 0
    legs = [0, 0, 0, 0]
    if pose == "idle":
        bob = round(math.sin(t * math.tau) * 0.8)
    elif pose == "run":
        ph = t * math.tau
        bob = -round(abs(math.sin(ph)) * 2)
        legs = [round(3 * math.sin(ph)), round(3 * math.sin(ph + math.pi)), round(3 * math.sin(ph + math.pi)), round(3 * math.sin(ph))]
    elif pose == "attack":  # 돌진 박치기
        lunge = [0, -3, -4, 4, 8, 3][int(t * 6) % 6]
        bob = [0, 1, 2, -1, 0, 0][int(t * 6) % 6]
    elif pose == "hit":
        lunge = -3
    by = 31 + bob
    bx = 7 + lunge
    for i, lx in enumerate((bx + 25, bx + 7)):
        x = lx + legs[i * 2 + 1]
        d.rectangle((x, by + 4, x + 3, GROUND), fill=B_DARK)
    d.ellipse((bx, by - 10, bx + 31, by + 8), fill=B_BASE)
    # 등 갈기
    for k in range(7):
        x = bx + 6 + k * 3
        d.polygon([(x, by - 9), (x + 2, by - 13 - (k % 2)), (x + 3, by - 9)], fill=B_MANE)
    for i, lx in enumerate((bx + 23, bx + 5)):
        x = lx + legs[i * 2]
        d.rectangle((x, by + 4, x + 3, GROUND), fill=B_BASE)
        d.rectangle((x, GROUND - 1, x + 3, GROUND), fill=B_OUT)
    hx, hy = bx + 27, by - 4
    d.ellipse((hx - 4, hy - 7, hx + 9, hy + 7), fill=B_BASE)
    d.polygon([(hx + 6, hy - 3), (hx + 14, hy - 1), (hx + 14, hy + 5), (hx + 6, hy + 6)], fill=B_BASE)
    d.rectangle((hx + 13, hy - 1, hx + 15, hy + 5), fill=B_SNOUT)
    d.polygon([(hx - 2, hy - 6), (hx, hy - 11), (hx + 3, hy - 6)], fill=B_DARK)  # 귀
    shade(im, B_LIGHT, B_DARK, B_BASE)
    # 엄니
    d.polygon([(hx + 9, hy + 4), (hx + 13, hy + 1), (hx + 12, hy + 4)], fill=B_TUSK)
    d.point((hx + 5, hy - 2), fill=(250, 80, 50))
    d.point((hx + 15, hy + 2), fill=B_OUT)
    outline(im, B_OUT)
    return im


# ───────────────── 도깨비불 (밤) ─────────────────
def wisp(pose, t):
    im = frame()
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    cy = 26 + round(math.sin(ph) * 2)
    cx = 24
    if pose == "attack":
        cx += [0, -2, -3, 5, 6, 2][int(t * 6) % 6]
    if pose == "hit":
        cx -= 3
    r = 7 if pose != "attack" else 7 + [0, 0, 1, 2, 1, 0][int(t * 6) % 6]
    # 꼬리 불꽃 (뒤쪽으로 흔들림)
    for k in range(5):
        tx = cx - 6 - k * 3
        ty = cy + round(math.sin(ph * 2 + k) * 2) - k
        rr = max(1, 5 - k)
        d.ellipse((tx - rr, ty - rr, tx + rr, ty + rr), fill=(70, 140, 255, 150 - k * 25))
    d.ellipse((cx - r - 3, cy - r - 3, cx + r + 3, cy + r + 3), fill=(80, 150, 255, 90))  # 후광
    d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=(120, 200, 255, 255))
    d.ellipse((cx - r + 3, cy - r + 2, cx + r - 3, cy + r - 4), fill=(210, 245, 255, 255))
    # 불꽃 끝
    d.polygon([(cx - 3, cy - r + 1), (cx + 1 + round(math.sin(ph * 3)), cy - r - 6), (cx + 3, cy - r + 1)], fill=(150, 220, 255, 255))
    # 눈
    d.rectangle((cx + 1, cy - 1, cx + 2, cy + 1), fill=(20, 40, 90, 255))
    d.rectangle((cx + 5, cy - 1, cx + 6, cy + 1), fill=(20, 40, 90, 255))
    return im


# ───────────────── 숲달팽이 (밤) ─────────────────
S_SHELL, S_SHELL_L, S_SHELL_D = (150, 96, 160), (196, 140, 206), (96, 56, 110)
S_BODY, S_BODY_L, S_BODY_D, S_OUT = (126, 170, 96), (170, 210, 130), (82, 120, 62), (30, 34, 24)


def snail(pose, t):
    im = frame()
    d = ImageDraw.Draw(im)
    ph = t * math.tau
    stretch = 0
    if pose == "run":
        stretch = round((math.sin(ph) + 1) * 2)
    if pose == "attack":
        stretch = [0, 1, 3, 6, 5, 2][int(t * 6) % 6]
    if pose == "hit":
        stretch = -2
    # 몸 (발)
    d.rounded_rectangle((8, GROUND - 6, 34 + stretch, GROUND), radius=3, fill=S_BODY)
    # 머리·더듬이
    hx = 31 + stretch
    d.ellipse((hx - 3, GROUND - 12, hx + 6, GROUND - 2), fill=S_BODY)
    sway = round(math.sin(ph) * 1.5)
    d.line((hx + 1, GROUND - 11, hx + sway, GROUND - 18), fill=S_BODY_D, width=1)
    d.line((hx + 4, GROUND - 11, hx + 5 + sway, GROUND - 17), fill=S_BODY_D, width=1)
    d.rectangle((hx + sway - 1, GROUND - 20, hx + sway, GROUND - 19), fill=(240, 230, 120))
    d.rectangle((hx + 5 + sway, GROUND - 19, hx + 6 + sway, GROUND - 18), fill=(240, 230, 120))
    shade(im, S_BODY_L, S_BODY_D, S_BODY)
    # 껍데기 (나선)
    sy = GROUND - 18 + (1 if pose == "run" and math.sin(ph) > 0 else 0)
    d.ellipse((9, sy - 2, 29, sy + 16), fill=S_SHELL)
    shade(im, S_SHELL_L, S_SHELL_D, S_SHELL)
    cx, cy = 19, sy + 7
    pts = []
    for k in range(40):
        a = k * 0.42
        rr = 8 - k * 0.19
        pts.append((cx + round(math.cos(a) * rr), cy + round(math.sin(a) * rr)))
    d.line(pts, fill=S_SHELL_D)
    d.point((hx + 3, GROUND - 8), fill=(20, 20, 20))
    outline(im, S_OUT)
    return im


def anim_frames(fn, pose, n):
    return [fn(pose, i / n) for i in range(n)]


def death_frames(fn, n=5):
    base = fn("idle", 0)
    out = []
    for i in range(n):
        k = i / (n - 1)
        im = rotate_down(base, -k * 80)
        out.append(fade(im, 1 - k * 0.85))
    return out


def hit_frames(fn):
    a = tint_white(fn("hit", 0), 0.7)
    b = fn("hit", 0.5)
    return [a, b, fn("idle", 0)]


def write(name, anim, frames):
    sheet = Image.new("RGBA", (F * len(frames), F), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (i * F, 0))
    d = os.path.join(OUT, name)
    os.makedirs(d, exist_ok=True)
    sheet.save(os.path.join(d, anim + ".png"))


def build(name, fn):
    write(name, "Idle", anim_frames(fn, "idle", 4))
    write(name, "Run", anim_frames(fn, "run", 6))
    write(name, "Attack1", anim_frames(fn, "attack", 6))
    write(name, "Take Hit", hit_frames(fn))
    write(name, "Death", death_frames(fn))
    print("ok", name)


if __name__ == "__main__":
    build("ForestWolf", wolf)
    build("WildBoar", boar)
    build("Wisp", wisp)
    build("ForestSnail", snail)
    # 미리보기(검수용, 커밋 안 함): 각 시트를 4배로
    prev = Image.new("RGBA", (F * 6 * 4, F * 4 * 4), (40, 52, 44, 255))
    for row, name in enumerate(("ForestWolf", "WildBoar", "Wisp", "ForestSnail")):
        sh = Image.open(os.path.join(OUT, name, "Run.png"))
        prev.alpha_composite(sh.resize((sh.width * 4, sh.height * 4), Image.NEAREST), (0, row * F * 4))
    prev.save(os.path.join(os.path.dirname(__file__), "..", "..", "Logs", "forest_mobs_preview.png"))
