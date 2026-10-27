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

# v10 수정안(10-27 사용자: "칼로 벤다기보다 내려친다 느낌", 올려베기 더 다듬기)
# 벤다 = 칼날이 손보다 앞서 호를 그리며 지나가고, 맞은 뒤에도 멈추지 않고 끝까지 빠져나간다(따라감).
PROPOSAL = {
    "SwingDown": [IDLEK, [
        (((40, 44), -8, 110, 150, -10, 20, -110, -95, -70, -90, 0), 2),            # 준비: 칼 어깨 위, 반대손 앞 가드
        (((40, 44), -9, 112, 152, -10, 22, -110, -95, -70, -90, 0), 1),
        (((43, 46), 28, -5, -15, -140, -120, -150, -100, -25, -95, 0), 2),         # 베기: 주저앉고 앞발 딛고 숙임
        (((43, 46.5), 32, -40, -70, -150, -125, -150, -100, -25, -95, 0), 1),      # 따라감: 팔이 몸 앞 아래로 빠져나감(새 키)
     ], [(IDLEK, 3)]],
    "SwingUp": [IDLEK, [
        (((42, 45.7), 25, -130, -150, -10, 10, -145, -100, -25, -95, 0), 2),      # 준비: 웅크림, 반대손 앞 가드
        (((42, 45.7), 26, -132, -152, -10, 12, -145, -100, -25, -95, 0), 1),
        (((42, 44), 0, 45, 80, -130, -100, -120, -120, -70, -90, 0), 2),           # 올려베기: 손이 머리 앞 위까지(얼굴 가리지 않게 더 높이)
        (((41, 44), -10, 100, 135, -120, -100, -115, -115, -75, -90, 0), 2),       # 끝: 칼 머리 뒤로 빠짐, 몸 살짝 젖힘
        (((41, 44), -10, 102, 137, -120, -100, -115, -115, -75, -90, 0), 1),
        ((IDLEK), 3),
     ], []],
}
# 프레임별 칼 각도(도). 내려베기: 준비 때 칼끝을 등 뒤로 더 떨어뜨려(손목 꺾음) → 중간엔 칼이 손보다 앞서 앞 위 → 맞을 때 앞 아래 → 계속 빠져나가 아래
ANGLES = {
    "SwingDown": [70, 165, 168, 25, -20, -35, -32, -26, -20],
    # 올려베기: 뒤 아래(바닥 위) → 앞 아래로 퍼 올림 → 손보다 앞서 위로 지나감(수직에서 멈추지 않음) → 머리 뒤 → 회복은 칼을 등 뒤로 떨어뜨려 내림
    "SwingUp": [-140, -165, -165, -30, 110, 140, 160, 162, 200, 245, 340],
}
TIMES = {
    "SwingDown": [50, 40, 100, 30, 60, 180, 45, 60, 90],
    "SwingUp": [50, 40, 100, 30, 50, 40, 60, 160, 50, 60, 90],
}


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
        assert len(cur) == len(new) == len(TIMES[m]), (m, len(cur), len(new))
        gif = []
        for i, ((kc, ac), (kn, an)) in enumerate(zip(cur, new)):
            a, _, _ = draw(kc, ac, scale=4)
            b, _, _ = draw(kn, an, scale=4)
            c = Image.new("RGB", (a.width * 2 + 8, a.height + 30), (255, 255, 255))
            c.paste(a, (0, 30))
            c.paste(b, (a.width + 8, 30))
            g = ImageDraw.Draw(c)
            g.text((6, 2), f"지금 v9  {m}  {i}", fill=(0, 0, 0), font=FONT)
            g.text((a.width + 14, 2), f"수정안 v10  {i}", fill=(180, 30, 30), font=FONT)
            gif.append(c)
        gif[0].save(f"{out}/{m}_stick.gif", save_all=True, append_images=gif[1:], duration=TIMES[m], loop=0)
        print(m, len(gif))


if __name__ == "__main__":
    main()
