"""자세 수정안을 막대 인형(졸라맨) GIF 로 먼저 보여 준다(생성 0). 왼쪽 = 지금 키(skel_keys.KEYS), 오른쪽 = 수정안(PROPOSAL).
사용: python Tools/pixelfx/stick_gif.py <출력 폴더>  → <동작>_stick.gif
수정안은 키 자세 + 프레임별 칼 각도(ANGLES)로 적는다. 칼은 손목 회전(스냅)이 팔보다 빨라 보간과 다르게 직접 준다.
"""
import math
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from skel_keys import ANGLE_FIX, IDLEK, KEYS, run  # noqa: E402
from skel_pose import p, pose  # noqa: E402
from stick_prev import BLADE, FONT, GROUND, draw, grounded  # noqa: E402

# 레퍼런스 따라 그리기: rvros Animated Pixel Adventurer 프레임을 6배 격자로 확대해 읽은 관절(픽셀, 땅 y=59). 참고만, 그림은 저장소 밖.
REF = {
    "D_prep": dict(head=(45, 15), neck=(43, 22), hip=(42, 40), sh=(42, 23), el=(45, 27), hand=(50, 18), osh=(45, 23), oel=(47, 27), ohand=(48, 20),
                   bk=(33, 50), bf=(25, 58), fk=(52, 48), ff=(55, 58), sword=-150),
    "D_end": dict(head=(45, 25), neck=(42, 31), hip=(35, 40), sh=(40, 31), el=(32, 36), hand=(22, 40), osh=(44, 31), oel=(48, 36), ohand=(53, 40),
                  bk=(27, 49), bf=(20, 58), fk=(48, 48), ff=(50, 58), sword=180),
    "U_crouch": dict(head=(45, 28), neck=(43, 34), hip=(40, 45), sh=(40, 36), el=(33, 42), hand=(24, 45), osh=(46, 36), oel=(52, 40), ohand=(57, 40),
                     bk=(30, 51), bf=(22, 59), fk=(50, 50), ff=(54, 59), sword=-145),
    "U_rise": dict(head=(54, 22), neck=(52, 28), hip=(52, 42), sh=(51, 30), el=(56, 22), hand=(56, 10), osh=(54, 30), oel=(58, 35), ohand=(60, 31),
                   bk=(33, 52), bf=(17, 59), fk=(58, 50), ff=(54, 59), sword=100),
    "U_end": dict(head=(55, 20), neck=(54, 27), hip=(52, 42), sh=(51, 28), el=(55, 20), hand=(55, 12), osh=(55, 28), oel=(58, 33), ohand=(59, 25),
                  bk=(35, 52), bf=(18, 59), fk=(58, 50), ff=(57, 59), sword=180),
}
REF_MAP = {
    "SwingDown": ["D_prep", "D_prep", "D_prep", "D_end", "D_end", "D_end", "D_end", "D_end", "D_end", "D_end"],
    "SwingUp": ["U_crouch", "U_crouch", "U_crouch", "U_rise", "U_rise", "U_end", "U_end", "U_end", "U_end", "U_end", "U_end"],
}

# v11 수정안(10-27 사용자: v10 내려베기 팔 어깨 탈골·오다리, 올려베기 칼손은 앞으로 가며 올려베기·반대손·다리 부자연 → 레퍼런스 졸라맨과 비교하며 고치기)
# 다리: 뒷다리는 뒤로 곧게 뻗은 사선, 앞다리는 무릎이 앞(정강이 거의 수직) — 무릎이 양쪽으로 벌어지는 오다리 금지
# 반대손: 뒤로 던지지 않고 가슴 앞 가드 유지(올려벨 때는 아래팔을 세워 주먹이 가슴~얼굴 앞)
STANCE = (-128, -128, -45, -85)
PROPOSAL = {
    "SwingDown": [IDLEK, [
        (((40.5, 44), -4, 60, 150, -40, 15, -118, -122, -62, -82, 0), 2),          # 준비: 칼 어깨에 멤, 반대손 앞 가드, 다리 살짝 벌림
        (((40.5, 44), -5, 62, 152, -40, 17, -118, -122, -62, -82, 0), 1),
        (((43, 44), 22, -20, -45, -45, -10) + STANCE + (0,), 1),                    # 베기: 숙이며 앞발 딛음, 칼손 앞 가슴~허리
        (((43, 44), 28, -80, -95, -45, -15) + STANCE + (0,), 1),                    # 베어 지나감: 칼손이 몸 아래로
        (((43, 44), 32, -140, -165, -45, -20) + STANCE + (0,), 1),                  # 끝(rvros): 칼손 뒤 허리, 칼 뒤로 수평, 반대손 앞
        (((43, 44), 33, -142, -167, -45, -22) + STANCE + (0,), 1),
     ], [(IDLEK, 3)]],
    "SwingUp": [IDLEK, [
        (((41, 44), 15, -140, -160, -35, 0) + STANCE + (0,), 2),                    # 준비: 웅크림, 칼손 뒤 허리, 반대손 가슴 앞 주먹
        (((41, 44), 16, -142, -162, -35, 2) + STANCE + (0,), 1),
        (((43.5, 44), 2, 15, 70, -50, 70, -135, -135, -62, -105, 0), 2),            # 올려베기: 몸이 앞으로 나가며 솟음, 칼손이 앞으로 돌아 머리 앞 위
        (((44, 44), -4, 63, 92, -55, 80, -135, -135, -62, -105, 0), 2),            # 끝: 칼손 머리 위, 칼 머리 뒤 수평, 반대손 아래팔 세워 가드
        (((44, 44), -5, 64, 93, -55, 82, -135, -135, -62, -105, 0), 1),
        ((IDLEK), 3),
     ], []],
}
ANGLES = {
    # 내려베기: 어깨에 멘 칼(뒤 아래) → 칼이 손보다 앞서 앞 위로 넘어옴 → 앞 아래 → 따라감은 낮고 수평에 가깝게 빠짐
    "SwingDown": [80, -165, -165, -40, -140, 180, 180, 120, 50, -20],
    # 올려베기: 뒤 아래 → 앞 아래로 퍼 올림 → 수직을 지나 머리 위 → 머리 뒤 수평 → 회복은 등 뒤로 떨어뜨려 내림
    "SwingUp": [-140, -160, -160, -45, 105, 150, 180, 180, 215, 260, 340],
}
TIMES = {
    "SwingDown": [50, 40, 100, 30, 30, 60, 150, 50, 60, 90],
    "SwingUp": [50, 40, 100, 30, 50, 40, 60, 160, 50, 60, 90],
}


def draw_ref(r, scale=4, w=72, h=64):
    """레퍼런스 관절을 같은 크기 막대 인형으로(땅 59 → 53 으로 맞춤)."""
    im = Image.new("RGB", (w * scale, h * scale), (226, 236, 240))
    g = ImageDraw.Draw(im)
    s = lambda q: ((q[0] - 4) * scale, (q[1] - 6) * scale)
    g.line([s((0, 59)), s((w + 8, 59))], fill=(150, 140, 120), width=2)
    far, near = (150, 150, 170), (40, 40, 60)
    hip = r["hip"]
    for a, b in [(hip, r["fk"]), (r["fk"], r["ff"]), (r["osh"], r["oel"]), (r["oel"], r["ohand"])]:
        g.line([s(a), s(b)], fill=far, width=scale)
    g.line([s(hip), s(r["neck"])], fill=near, width=scale + 2)
    hc = r["head"]
    g.ellipse([s((hc[0] - 7, hc[1] - 7)), s((hc[0] + 7, hc[1] + 7))], outline=near, width=3)
    for a, b in [(hip, r["bk"]), (r["bk"], r["bf"]), (r["sh"], r["el"]), (r["el"], r["hand"])]:
        g.line([s(a), s(b)], fill=near, width=scale)
    tip = p(r["hand"], r["sword"], 16)
    g.line([s(r["hand"]), s(tip)], fill=(90, 120, 200), width=scale - 2)
    for q, c in [(r["hand"], (200, 60, 40)), (r["ohand"], (60, 160, 60))]:
        g.ellipse([s((q[0] - 1.2, q[1] - 1.2)), s((q[0] + 1.2, q[1] + 1.2))], fill=c)
    return im


def keep_above_ground(k, ang):
    """칼끝이 땅(GROUND-1) 아래로 가면 칼을 수평 쪽으로 세운다(칼이 바닥을 뚫으면 안 된다)."""
    d = pose(grounded(*k), *k[1:])
    hand = d["RIGHT ARM"]
    a = ang
    for _ in range(90):
        if p(hand, a, BLADE)[1] <= GROUND - 1:
            return a
        # 아래쪽(-90 / 270) 에서 멀어지는 방향으로 1도씩
        a += 1 if math.cos(math.radians(a)) > 0 else -1
    return a


def frames_of(keys, angles=None, fix=None):
    start, main_segs, end_segs = keys
    a, last = run(start, main_segs)
    b, _ = run(last, end_segs)
    out = []
    for i, f in enumerate(a + b):
        ang = angles[i] if angles else (fix or {}).get(i, f[10])
        k = (f[0][0],) + tuple(f[1:10])
        out.append((k, keep_above_ground(k, ang) if angles else ang))
    return out


def main():
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    for m in PROPOSAL:
        cur = frames_of(KEYS[m], fix=ANGLE_FIX.get(m))
        new = frames_of(PROPOSAL[m], ANGLES[m])
        assert len(new) == len(TIMES[m]), (m, len(new))
        cur = (cur + [cur[-1]] * len(new))[:len(new)]
        gif = []
        for i, ((kc, ac), (kn, an)) in enumerate(zip(cur, new)):
            r = draw_ref(REF[REF_MAP[m][i]])
            a, _, _ = draw(kc, ac, scale=4)
            b, _, _ = draw(kn, an, scale=4)
            c = Image.new("RGB", (a.width * 3 + 16, a.height + 30), (255, 255, 255))
            c.paste(r, (0, 30))
            c.paste(a, (a.width + 8, 30))
            c.paste(b, (a.width * 2 + 16, 30))
            g = ImageDraw.Draw(c)
            g.text((6, 2), f"레퍼런스 rvros ({REF_MAP[m][i]})", fill=(0, 90, 160), font=FONT)
            g.text((a.width + 14, 2), f"지금 v9  {m} {i}", fill=(0, 0, 0), font=FONT)
            g.text((a.width * 2 + 22, 2), f"수정안 v11  {i}", fill=(180, 30, 30), font=FONT)
            gif.append(c)
        gif[0].save(f"{out}/{m}_stick.gif", save_all=True, append_images=gif[1:], duration=TIMES[m], loop=0)
        print(m, len(gif))


if __name__ == "__main__":
    main()
