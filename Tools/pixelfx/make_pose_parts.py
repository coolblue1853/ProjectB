"""픽셀 리그 포즈 변형 그림(PixelRigDesign 4절 걷기·달리기): 다리·팔 PNG 를 마디별 정수 이동으로 바꿔 굽는다.

통째 이동(팔·다리가 몸에서 떨어져 보임)·선형 시어(외곽선이 계단으로 끊김) 대신:
- 다리(50 밑단 / 51 무릎 / 52~53 신발 / 54 밑창): 밑단 고정, 무릎 = 발 이동의 절반, 신발·밑창 = dx. 들기 1 = 무릎 줄을 접어 1 짧게(발 1 앞), 2 = 무릎·신발 윗줄을 접어 2 짧게.
- 팔: 소매(위 4줄) 고정, 팔뚝 = 손 이동의 절반, 손(아래 줄) = dx.
- 옮긴 뒤 원본에선 막혀 있던 면이 비면 검정으로 메워 외곽선을 닫는다(원래 열린 면은 그대로).
- 머리: 머리카락 돔 가운데 줄을 복제/삭제해 정수리만 1px 늘임(head_p1)·눌림(head_m1) — 몸이 움직일 때 머리카락이 한 박자 늦는 2차 움직임.
이름: leg_l_p1_u0(앞 1·들기 0), leg_r_m2_u1, arm_l_p2, arm_r_z0, head_p1 … (p 앞·위, m 뒤·아래, z 0). 원본 이름(leg_l.png)은 그대로 둔다.
사용: python Tools/pixelfx/make_pose_parts.py [부위 폴더]
"""
import os
import sys

from PIL import Image

DIR = sys.argv[1] if len(sys.argv) > 1 else "Assets/Art/Player/PixelRig/Adventurer"
ARM_TOP = {"arm_l": 38, "arm_r": 39}  # 팔 그림 첫 줄(어깨)
LEG_KNEE = 51


def joints(img, rows, fold=()):
    """rows = [(시작 줄, dx)] 계단식 이동. fold 의 줄은 지우고 그 아래 줄을 지운 수만큼 올리고 1 앞으로.
    외곽 닫기: 옮긴 색 픽셀의 한 면이 원본에선 이웃과 닿아 있었는데 지금 비었으면 그 빈칸을 검정으로 메운다
    (원래 열려 있던 면 — 몸통과 겹치는 바지 윗단 등 — 은 그대로 둔다)."""
    w, h = img.size
    out = Image.new("RGBA", (w, h))
    src, dst = img.load(), out.load()
    moved = []  # (새 x, 새 y, 원본 x, 원본 y)
    for y in range(h):
        if y in fold:
            continue
        dx = 0
        for y0, d in rows:
            if y >= y0:
                dx = d
        up = sum(1 for f in fold if f < y)
        if up:
            dx += 1
        for x in range(w):
            if src[x, y][3] and 0 <= x + dx < w:
                dst[x + dx, y - up] = src[x, y]
                moved.append((x + dx, y - up, x, y))
    fill = []
    for nx, ny, ox, oy in moved:
        if src[ox, oy][:3] == (0, 0, 0):
            continue
        for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            tx, ty, sx, sy = nx + ddx, ny + ddy, ox + ddx, oy + ddy
            if not (0 <= tx < w and 0 <= ty < h) or dst[tx, ty][3]:
                continue
            if 0 <= sx < w and 0 <= sy < h and src[sx, sy][3]:
                fill.append((tx, ty))
    for tx, ty in fill:
        dst[tx, ty] = (0, 0, 0, 255)
    return out


def leg(img, dx, lift):
    knee = int(dx / 2) if abs(dx) > 1 else 0
    fold = (LEG_KNEE, LEG_KNEE + 1)[:lift]
    return joints(img, [(LEG_KNEE, knee), (LEG_KNEE + 1, dx)], fold)


def arm(img, top, dx):
    fore = int(dx / 2) if abs(dx) > 1 else dx
    return joints(img, [(top + 4, fore), (top + 7, dx)])


HAIR_ROW = 20  # 머리카락 돔 가운데(거의 단색 줄) — 이 줄을 복제/삭제해 정수리를 1px 늘이고 누른다


def hair(img, dy):
    """dy +1 = 정수리 1px 위로 늘임(HAIR_ROW 복제), -1 = 1px 눌림(HAIR_ROW 삭제). 그 아래(얼굴·목)는 그대로"""
    w, h = img.size
    out = Image.new("RGBA", (w, h))
    top = img.crop((0, 0, w, HAIR_ROW + (1 if dy > 0 else 0)))
    out.alpha_composite(img.crop((0, HAIR_ROW, w, h)), (0, HAIR_ROW))
    if dy > 0:
        out.alpha_composite(top, (0, -1))
    else:
        out.alpha_composite(img.crop((0, 0, w, HAIR_ROW)), (0, 1))
        out.alpha_composite(img.crop((0, HAIR_ROW + 1, w, h)), (0, HAIR_ROW + 1))
    return out


def tag(dx):
    return ("p" if dx > 0 else "m" if dx < 0 else "z") + str(abs(dx))


def main():
    n = 0
    for name in ("leg_l", "leg_r"):
        base = Image.open(os.path.join(DIR, name + ".png")).convert("RGBA")
        for dx in range(-2, 3):
            for lift in range(3):
                leg(base, dx, lift).save(os.path.join(DIR, f"{name}_{tag(dx)}_u{lift}.png"))
                n += 1
    for name, top in ARM_TOP.items():
        base = Image.open(os.path.join(DIR, name + ".png")).convert("RGBA")
        for dx in range(-2, 3):
            arm(base, top, dx).save(os.path.join(DIR, f"{name}_{tag(dx)}.png"))
            n += 1
    head = Image.open(os.path.join(DIR, "head.png")).convert("RGBA")
    for dy in (-1, 1):
        hair(head, dy).save(os.path.join(DIR, f"head_{tag(dy)}.png"))
        n += 1
    print("pose parts", n, "->", DIR)


if __name__ == "__main__":
    main()
