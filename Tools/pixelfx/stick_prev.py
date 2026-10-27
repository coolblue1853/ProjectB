"""키 자세를 막대 인형 + 칼로 그려 레퍼런스 옆에서 비교한다(생성 0, PixelLab 돌리기 전 점검용).
사용: python Tools/pixelfx/stick_prev.py <출력.png>  — 아래 KEYS 를 고쳐 가며 본다.
발은 땅(y=53)에 붙도록 골반 높이를 자동으로 맞춘다(다리 길이는 skel_pose 고정값 그대로).
"""
import math
import sys

from PIL import Image, ImageDraw, ImageFont

FONT = ImageFont.truetype("C:/Windows/Fonts/malgun.ttf", 20)

sys.path.insert(0, __file__.rsplit("\\", 1)[0].rsplit("/", 1)[0])
from skel_pose import IDLE, p, pose  # noqa: E402

GROUND = 53.0
BLADE = 15.0  # 손목에서 칼끝까지(ShortSword 피벗 18.5 - 날 시작 기준 근사)


def grounded(hx, lean, ra, rf, la, lf, rt, rs, lt, ls):
    """골반 x 와 관절 각도만 주면 낮은 쪽 발이 땅에 닿게 골반 y 를 정한다."""
    d = pose((hx, 44.0), lean, ra, rf, la, lf, rt, rs, lt, ls)
    low = max(d["RIGHT LEG"][1], d["LEFT LEG"][1])
    return (hx, 44.0 + GROUND - low)


def draw(k, sword, scale=6, w=72, h=64):
    hip = grounded(*k)
    d = pose(hip, *k[1:])
    im = Image.new("RGB", (w * scale, h * scale), (236, 232, 222))
    g = ImageDraw.Draw(im)
    s = lambda q: (q[0] * scale, q[1] * scale)
    g.line([s((0, GROUND)), s((w, GROUND))], fill=(150, 140, 120), width=2)
    far = (150, 150, 170)
    near = (40, 40, 60)
    for a, b, c in [("LEFT SHOULDER", "LEFT ELBOW", far), ("LEFT ELBOW", "LEFT ARM", far), ("LEFT HIP", "LEFT KNEE", far), ("LEFT KNEE", "LEFT LEG", far)]:
        g.line([s(d[a]), s(d[b])], fill=c, width=scale)
    neck = d["NECK"]
    g.line([s(hip), s(neck)], fill=near, width=scale + 2)
    hc = (d["NOSE"][0] - 4, d["NOSE"][1] - 3)  # 머리 중심(대기 기준 코에서 뒤 위)
    g.ellipse([s((hc[0] - 8, hc[1] - 8)), s((hc[0] + 8, hc[1] + 8))], outline=near, width=3)
    g.ellipse([s((d["NOSE"][0] - 1, d["NOSE"][1] - 1)), s((d["NOSE"][0] + 1, d["NOSE"][1] + 1))], fill=near)
    for a, b in [("RIGHT HIP", "RIGHT KNEE"), ("RIGHT KNEE", "RIGHT LEG"), ("RIGHT SHOULDER", "RIGHT ELBOW"), ("RIGHT ELBOW", "RIGHT ARM")]:
        g.line([s(d[a]), s(d[b])], fill=near, width=scale)
    hand = d["RIGHT ARM"]
    tip = p(hand, sword, BLADE)
    g.line([s(hand), s(tip)], fill=(90, 120, 200), width=scale - 2)
    g.ellipse([s((hand[0] - 1.2, hand[1] - 1.2)), s((hand[0] + 1.2, hand[1] + 1.2))], fill=(200, 60, 40))
    off = d["LEFT ARM"]
    g.ellipse([s((off[0] - 1.2, off[1] - 1.2)), s((off[0] + 1.2, off[1] + 1.2))], fill=(60, 160, 60))
    return im, hip, tip


# (골반x, 기울기, 칼팔 위·아래, 반대팔 위·아래, 뒷다리 허벅지·정강이, 앞다리 허벅지·정강이), 칼 각도
KEYS = {
    "down": [
        ("대기", (40.5, 0, -110, -80, -80, -100, -95, -90, -85, -90), -20),
        ("준비: 칼 어깨 위로, 반대손 앞 가드", (40, -8, 110, 150, -10, 20, -110, -95, -70, -90), 140),
        ("베기: 주저앉으며 앞발 딛고 숙임", (43, 28, -5, -15, -140, -120, -150, -100, -25, -95), -15),
        ("유지: 칼 앞 수평, 반대손 뒤로", (43, 30, -8, -20, -145, -125, -150, -100, -25, -95), -20),
    ],
    "up": [
        ("대기", (40.5, 0, -110, -80, -80, -100, -95, -90, -85, -90), -20),
        ("준비: 낮게 웅크림, 칼 뒤 아래, 반대손 앞 가드", (42, 25, -130, -150, -10, 10, -145, -100, -25, -95), -165),
        ("올려베기: 뒷다리 펴며 솟음, 칼 앞 위", (42, 5, 20, 50, -130, -100, -120, -120, -70, -90), 80),
        ("끝: 칼 머리 위 뒤, 몸 세움", (41, -8, 100, 130, -120, -100, -115, -115, -75, -90), 130),
    ],
}


def main():
    out = sys.argv[1]
    rows = []
    for m, ks in KEYS.items():
        ims = []
        for label, k, sw in ks:
            im, hip, tip = draw(k, sw)
            ImageDraw.Draw(im).text((6, 4), f"{m} {label}", fill=(0, 0, 0), font=FONT)
            ims.append(im)
            print(m, label, "골반", tuple(round(v, 1) for v in hip), "칼끝 y", round(tip[1], 1))
        row = Image.new("RGB", (sum(i.width for i in ims), ims[0].height))
        x = 0
        for i in ims:
            row.paste(i, (x, 0))
            x += i.width
        rows.append(row)
    sheet = Image.new("RGB", (max(r.width for r in rows), sum(r.height for r in rows)))
    y = 0
    for r in rows:
        sheet.paste(r, (0, y))
        y += r.height
    sheet.resize((sheet.width // 2, sheet.height // 2)).save(out)


if __name__ == "__main__":
    main()
