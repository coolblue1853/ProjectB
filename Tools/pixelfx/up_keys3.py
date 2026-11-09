"""올려베기 졸라맨 키 v3(11-06 사용자 지시: 반대 손·다리도 같이 움직여야 한다, 올려베기는 3타 중 가장 크게).
오른팔 = 칼 든 팔(빨강), 왼팔은 반대로 휘두르며 균형, 다리는 체중 이동·뒷발 들림. up_keys2.py 의 IDLE·ik·draw 를 쓴다.
사용: python Tools/pixelfx/up_keys3.py → Logs/motion_review/up_keys6.json, up_stick6_sheet.png, up_stick6.gif"""
import json
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import up_keys2 as u  # noqa: E402
from PIL import Image  # noqa: E402

ROOT = u.ROOT
UL, FL = 6.4, 5.6  # 왼팔 길이(팔을 반대로 휘두름)
# (골반 낮춤, 상체 앞기울기, 오른팔 위°, 오른팔 아래°, 왼팔 위°, 왼팔 아래°, 앞발 x, 앞발 들림, 뒷발 x, 뒷발 들림)
KEYS = [
    (4.5, 3.0, 300, 320, 280, 320, 45, 0.0, 21, 0.0),   # 1 앞 보며 낮게, 칼 앞 아래(285°)
    (4.5, 3.0, 320, 350, 275, 315, 45, 0.0, 21, 0.0),   # 2 칼 앞으로 올라옴(340°)
    (4.0, 3.0, 335, 40, 270, 305, 45, 0.0, 21, 0.0),    # 3 칼 앞 위(35°)
    (3.0, 1.5, 60, 95, 265, 300, 44, 0.0, 22, 0.0),     # 4 머리 위 수직 넘어감(105°)
    (2.0, -2.5, 95, 140, 285, 330, 44, 0.0, 22, 0.0),   # 5 몸 젖혀 칼이 등 뒤로 넘어가며 끝(145°) — 새 던파 레퍼런스(앞 보다가 등으로 올려치기)
]


def pose(key):
    sink, lean, a1, a2, l1, l2, fx, fl, bx, bl = key
    P = u.pose((sink, lean, a1, a2, fx, bx))  # 몸·오른팔·다리 기본
    ls = P["LEFT SHOULDER"]
    e = (ls[0] + UL * math.cos(math.radians(l1)), ls[1] - UL * math.sin(math.radians(l1)))
    P["LEFT ELBOW"] = list(e)
    P["LEFT ARM"] = [e[0] + FL * math.cos(math.radians(l2)), e[1] - FL * math.sin(math.radians(l2))]
    gl, gr = (fx, 57.0 - fl), (bx, 57.0 - bl)
    P["LEFT LEG"], P["RIGHT LEG"] = list(gl), list(gr)
    P["LEFT KNEE"] = list(u.ik(P["LEFT HIP"], gl, u.THIGH, u.SHIN, 1))
    P["RIGHT KNEE"] = list(u.ik(P["RIGHT HIP"], gr, u.THIGH, u.SHIN, 1))
    return P


BLADE = [285, 340, 35, 105, 145]  # 칼 각도(수학 각도, 0 = 오른쪽 90 = 위). 칼 길이 = 손에서 16px
BL = 16.0


def draw_blade(P, ang, S=8):
    im = u.draw(P, S)
    from PIL import ImageDraw
    d = ImageDraw.Draw(im)
    w = P["RIGHT ARM"]
    tip = (w[0] + BL * math.cos(math.radians(ang)), w[1] - BL * math.sin(math.radians(ang)))
    d.line([w[0] * S, w[1] * S, tip[0] * S, tip[1] * S], fill=(255, 215, 40), width=7)
    d.ellipse([tip[0] * S - 5, tip[1] * S - 5, tip[0] * S + 5, tip[1] * S + 5], fill=(255, 245, 160))
    return im


if __name__ == "__main__":
    Ps = [pose(k) for k in KEYS]
    json.dump([u.to_kp(P) for P in Ps], open(os.path.join(ROOT, "Logs/motion_review/up_keys6.json"), "w"))
    n = len(Ps)
    sheet = Image.new("RGB", (n * 300, 300))
    for i, P in enumerate(Ps):
        sheet.paste(u.draw(P).resize((300, 300)), (i * 300, 0))
    sheet.save(os.path.join(ROOT, "Logs/motion_review/up_stick6_sheet.png"))
    fr = [u.draw(P).resize((384, 384)) for P in Ps]
    fr[0].save(os.path.join(ROOT, "Logs/motion_review/up_stick6.gif"), save_all=True, append_images=fr[1:], duration=200, loop=0)
    print("ok", n)
