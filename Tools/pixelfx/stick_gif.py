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
REF.update({
    # 던파M 여귀검사 평타(유튜브 jnK1RA-192g 5.53초): 칼을 머리 위로 들어 뒤로 수평, 다리는 곧게 넓게(A자)
    "D_dnf_prep": dict(head=(44, 12), neck=(44, 19), hip=(44, 36), sh=(43, 21), el=(42, 13), hand=(44, 6), osh=(46, 21), oel=(47, 14), ohand=(46, 7),
                       bk=(40, 47), bf=(36, 59), fk=(49, 47), ff=(53, 59), sword=175),
    # SLYNYRD Pixelblog 9 칼 공격 HIT 프레임: 깊게 내딛고 숙이며 팔을 가슴 높이로 앞으로 쭉, 칼 거의 수평
    "D_sly_hit": dict(head=(44, 22), neck=(42, 28), hip=(36, 40), sh=(42, 29), el=(49, 31), hand=(55, 33), osh=(43, 29), oel=(50, 32), ohand=(55, 34),
                      bk=(30, 49), bf=(24, 59), fk=(44, 48), ff=(47, 59), sword=-12),
    # 던파M 5.90초: 낮게 웅크려 칼을 앞 아래로 빼낸 끝(= 올려베기 시작)
    "D_dnf_end": dict(head=(49, 22), neck=(46, 28), hip=(40, 42), sh=(45, 30), el=(49, 37), hand=(52, 42), osh=(47, 30), oel=(50, 38), ohand=(52, 43),
                      bk=(35, 51), bf=(32, 59), fk=(47, 50), ff=(50, 59), sword=-60),
    # 던파M 6.10~6.23초: 일어서며 칼이 앞 아래에서 머리 위 뒤로, 다리 A자
    "U_dnf_rise": dict(head=(47, 13), neck=(45, 20), hip=(44, 37), sh=(44, 22), el=(46, 13), hand=(45, 6), osh=(46, 22), oel=(49, 28), ohand=(51, 24),
                       bk=(39, 48), bf=(35, 59), fk=(49, 48), ff=(53, 59), sword=110),
    "U_dnf_end": dict(head=(47, 13), neck=(45, 20), hip=(44, 37), sh=(44, 22), el=(40, 14), hand=(36, 9), osh=(46, 22), oel=(49, 28), ohand=(51, 24),
                      bk=(39, 48), bf=(35, 59), fk=(49, 48), ff=(53, 59), sword=160),
})

# Pinterest 픽셀 한손검+방패 3연타(9b10b0cd…gif, 20프레임 60ms, 오른쪽 보기). 9b_grid.png(0.75배 칸 좌표)에서 읽어 땅 59·키 ~47 로 환산
def _pin(d, sword):
    k = 0.207 / 0.75
    f = lambda q: (40 + (q[0] - 118 * 0.75 / 0.75) * k, 59 - (185 - q[1]) * k)
    out = {n: f(v) for n, v in d.items()}
    out["sword"] = sword
    return out


REF.update({
    "P1_prep": _pin(dict(head=(130, 30), neck=(128, 48), hip=(118, 112), sh=(130, 55), el=(150, 35), hand=(165, 12), osh=(122, 55), oel=(118, 80), ohand=(130, 88),
                         bk=(85, 150), bf=(55, 182), fk=(140, 145), ff=(148, 185)), 180),
    "P2_strike": _pin(dict(head=(148, 40), neck=(140, 55), hip=(118, 120), sh=(140, 60), el=(100, 85), hand=(72, 105), osh=(145, 60), oel=(155, 85), ohand=(150, 95),
                           bk=(70, 160), bf=(45, 185), fk=(150, 150), ff=(150, 185)), 200),
    "P3_end": _pin(dict(head=(155, 40), neck=(145, 60), hip=(120, 125), sh=(140, 65), el=(110, 95), hand=(80, 110), osh=(146, 65), oel=(150, 90), ohand=(145, 95),
                        bk=(75, 165), bf=(50, 185), fk=(150, 155), ff=(155, 185)), 195),
    "P14_lunge": _pin(dict(head=(153, 58), neck=(140, 75), hip=(110, 135), sh=(150, 80), el=(180, 120), hand=(205, 145), osh=(130, 75), oel=(110, 58), ohand=(100, 50),
                           bk=(70, 170), bf=(40, 185), fk=(150, 160), ff=(155, 188)), -30),
    "P0_ready": _pin(dict(head=(124, 38), neck=(122, 55), hip=(115, 120), sh=(110, 60), el=(90, 50), hand=(60, 40), osh=(130, 62), oel=(145, 85), ohand=(150, 80),
                          bk=(75, 170), bf=(55, 185), fk=(140, 160), ff=(148, 185)), 165),
    "P7_upend": _pin(dict(head=(120, 72), neck=(118, 88), hip=(110, 140), sh=(105, 85), el=(70, 55), hand=(40, 35), osh=(125, 90), oel=(140, 115), ohand=(145, 105),
                          bk=(70, 170), bf=(45, 185), fk=(140, 160), ff=(150, 185)), 175),
})

REF_MAP = {
    "SwingDown": ["P1_prep", "P1_prep", "P1_prep", "P2_strike", "P14_lunge", "P14_lunge", "P14_lunge", "P14_lunge", "P14_lunge", "P14_lunge", "P14_lunge"],
    "SwingUp": ["D_dnf_end", "D_dnf_end", "D_dnf_end", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise", "U_dnf_rise"],
}

# v13 수정안(10-27 사용자: v12 초록손(반대손) 팔이 이상함 → 여러 횡스크롤 레퍼런스를 졸라맨으로 비교)
# 반대손: 주먹을 앞으로 내뻗지 않는다. 준비·올려베기 땐 팔꿈치 굽혀 가슴 앞에 붙이고(핀 0·7, 던파M),
#         앞으로 깊게 내딛어 벨 땐 뒤 위로 젖혀 균형(핀 14). 다리: 벨 때 뒷다리 뒤로 길게 곧게, 앞 무릎 앞.
TUCK = (-85, -40)         # 반대손 배 앞에 자연스럽게(우리 반대 어깨는 몸 앞쪽이라 팔을 들면 갈고리처럼 꺾여 보임)
BALANCE = (-125, -135)    # 반대손 뒤 아래로 흔들어 균형(몸 앞 어깨에서 뒤 위로 올리면 가슴을 가로질러 이상함)
A_STANCE = (-112, -112, -68, -68)
LUNGE = (-140, -140, -40, -95)
# 회복(v13c, 사용자: 마지막에 자세가 갑자기 바뀜): ① 골반을 뒤로 끌지 않고 그 자리(43)에서 선다 ② 중간 자세를 하나 거쳐 몸통→팔→다리 순으로 풀린다 ③ 4프레임 ease-out
IDLE_HERE = ((43, 44),) + IDLEK[1:]
DOWN_MID = ((43, 44), 12, -80, -60) + TUCK + (-115, -115, -65, -88, 0)
UP_MID = ((43, 44), -2, 130, 175) + TUCK + (-118, -118, -65, -88, 0)
UP_MID2 = ((43, 44), 0, 10, 35) + TUCK + (-115, -115, -65, -88, 0)   # 올려베기 회복 중간: 칼손 가슴 앞
PROPOSAL = {
    "SwingDown": [IDLEK, [
        (((40.5, 44), -5, 80, 110) + TUCK + A_STANCE + (0,), 2),                     # 준비: 칼손 머리 위, 칼 뒤로 수평(핀 1·던파M)
        (((40.5, 44), -6, 82, 112) + TUCK + A_STANCE + (0,), 1),
        (((42, 44), 15, 20, -10) + TUCK + (-125, -125, -55, -90, 0), 1),            # 베기 시작: 칼손 머리 앞으로, 반대손은 배 앞에 그대로
        (((43, 44), 30, -30, -35) + TUCK + LUNGE + (0,), 1),                      # 내딛어 벰: 팔을 앞 아래로 쭉, 반대손은 배 앞에 고정(사용자: 흔들지 말 것)
        (((43, 44), 32, -40, -45) + TUCK + LUNGE + (0,), 1),                      # 끝: 칼 앞 아래로 빠짐
        (((43, 44), 33, -41, -46) + TUCK + LUNGE + (0,), 1),
     ], [(DOWN_MID, 2), (IDLE_HERE, 2)]],
    # v15(10-28): 올려베기 레퍼런스 7종(핀 사무라이·광선검·기사·닌자·한손검 2타, rvros, 던파M) 공통 —
    # 팔을 쭉 편 채 어깨를 축으로 크게 휘두르고(칼날 = 아래팔 연장선), 몸은 낮게 남은 채 비튼다. 끝은 팔 곧게 위 뒤 사선.
    # v14 는 손을 가슴 앞 → 얼굴 앞으로 끌어올리고 몸을 똑바로 세워 '국자로 뜨는' 모양이었다.
    "SwingUp": [IDLEK, [
        (((42, 44), 22, -140, -160, 0, 0) + LUNGE + (0,), 2),                         # v16: 칼을 뒤 아래로 당겨 감음(핀 사무라이·rvros·닌자) — 앞에서 시작하면 들어 올리기(국자)가 됨
        (((42, 44), 23, -142, -162, 0, 0) + LUNGE + (0,), 1),
        (((43, 44), 24, -95, -90, 0, 0, -138, -138, -42, -93, 0), 1),                 # 팔 곧게 아래로 — 칼이 몸 아래를 지나 앞으로
        (((43, 44), 14, 45, 50, 0, 0, -135, -135, -48, -92, 0), 1),                   # 팔 곧게 앞 위로 휘두름 — 몸 조금만 일어남
        (((43, 44), 6, 115, 125, 0, 0, -132, -132, -52, -90, 0), 2),                  # 끝: 팔 곧게 위 뒤 사선(칼 = 팔 연장선), 다리 넓고 낮게, 얼굴 정면
        (((43, 44), 6, 116, 126, 0, 0, -132, -132, -52, -90, 0), 1),
        (((43, 44), 4, 170, 185, 0, 0, -125, -125, -58, -88, 0), 1),                   # 회복: 팔이 뒤로 내려감(핀 2타 9→11)
        (((43, 44), 2, -140, -130, 0, 0, -118, -118, -65, -88, 0), 1),                 # 칼손 뒤 허리
        (IDLE_HERE, 2),
     ], []],
}


def body_tuck(key):
    """반대손을 몸통에 붙여 고정: 각도는 월드 기준이라 몸을 숙이면(lean) 팔이 그대로 수직으로 남아 흔들려 보인다.
    레퍼런스(핀 방패팔·던파M)처럼 몸통과 함께 돌도록 TUCK 에서 lean 만큼 뺀다(10-27 사용자: 손 흔들림)."""
    (hip, lean, ra, rf, la, lf, rt, rs, lt, ls, sw) = key
    return (hip, lean, ra, rf, TUCK[0] - lean, TUCK[1] - lean, rt, rs, lt, ls, sw)


for _m in ("SwingUp",):
    _u = PROPOSAL[_m]
    PROPOSAL[_m] = [_u[0], [(body_tuck(k) if k is not IDLE_HERE else k, n) for k, n in _u[1]], _u[2]]
_d = PROPOSAL["SwingDown"]
PROPOSAL["SwingDown"] = [_d[0], [(body_tuck(k), n) for k, n in _d[1]], [(body_tuck(k) if k is not IDLE_HERE else k, n) for k, n in _d[2]]]

ANGLES = {
    "SwingDown": [100, 172, 174, 60, -30, -35, -35, -40, -35, -28, -20],
    "SwingUp": [-160, -165, -165, -60, 50, 125, 128, 129, 180, -150, -60, -20],
}
TIMES = {
    "SwingDown": [50, 40, 100, 30, 30, 60, 150, 50, 60, 80, 100],
    "SwingUp": [50, 40, 90, 30, 30, 50, 60, 150, 50, 60, 80, 100],
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
            g.text((6, 2), f"레퍼런스 {REF_MAP[m][i]}", fill=(0, 90, 160), font=FONT)
            g.text((a.width + 14, 2), f"지금 v9  {m} {i}", fill=(0, 0, 0), font=FONT)
            g.text((a.width * 2 + 22, 2), f"수정안 v16  {i}", fill=(180, 30, 30), font=FONT)
            gif.append(c)
        gif[0].save(f"{out}/{m}_stick.gif", save_all=True, append_images=gif[1:], duration=TIMES[m], loop=0)
        print(m, len(gif))


if __name__ == "__main__":
    main()


GALLERY = {
    "내려베기 레퍼런스": [("rvros 준비", "D_prep"), ("rvros 끝", "D_end"), ("SLYNYRD 타격", "D_sly_hit"), ("던파M 준비", "D_dnf_prep"), ("던파M 끝", "D_dnf_end"),
                     ("핀 1타 준비", "P1_prep"), ("핀 1타 벰", "P2_strike"), ("핀 1타 끝", "P3_end"), ("핀 3타 내딛어 벰", "P14_lunge")],
    "올려베기 레퍼런스": [("rvros 웅크림", "U_crouch"), ("rvros 솟음", "U_rise"), ("rvros 끝", "U_end"), ("던파M 솟음", "U_dnf_rise"), ("던파M 끝", "U_dnf_end"),
                     ("핀 준비", "P0_ready"), ("핀 2타 끝", "P7_upend")],
}


def gallery(out):
    """레퍼런스 졸라맨 모음(동작별 한 줄)."""
    rows = []
    for title, items in GALLERY.items():
        ims = []
        for label, key in items:
            im = draw_ref(REF[key], scale=3)
            ImageDraw.Draw(im).text((4, 2), label, fill=(0, 60, 140), font=FONT)
            ims.append(im)
        row = Image.new("RGB", (sum(i.width + 4 for i in ims), ims[0].height + 30), (255, 255, 255))
        ImageDraw.Draw(row).text((4, 2), title, fill=(0, 0, 0), font=FONT)
        x = 0
        for i in ims:
            row.paste(i, (x, 30))
            x += i.width + 4
        rows.append(row)
    sheet = Image.new("RGB", (max(r.width for r in rows), sum(r.height for r in rows)), (255, 255, 255))
    y = 0
    for r in rows:
        sheet.paste(r, (0, y))
        y += r.height
    sheet.save(out)
