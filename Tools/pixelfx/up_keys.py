"""올려베기 졸라맨 키(11-06 사용자 지시: 졸라맨 키로 생성). 대기 그림(motions/idle/raw_0)의 PixelLab estimate-skeleton 결과(Logs/motion_review/idle_kp.json)를 기준으로
오른팔(캐릭터 오른팔 = 화면 왼쪽 팔)이 뒤 아래에서 시작해 낮은 앞을 지나 머리 위로 올라가는 8장 키를 만든다. 좌표는 64px 그림 기준, 출력은 0~1 분수.
사용: python Tools/pixelfx/up_keys.py → Logs/motion_review/up_keys.json + up_stick.gif"""
import json
import math
import os
from PIL import Image, ImageDraw

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
KP = json.load(open(os.path.join(ROOT, "Logs/motion_review/idle_kp.json")))
IDLE = {q["label"]: (q["x"] * 64, q["y"] * 64, q.get("z_index", 0)) for q in KP}
UP_LEN, FORE_LEN = 8.7, 5.8
THIGH, SHIN = 9.6, 8.0

# (골반 y 낮춤, 상체 앞기울기 px, 오른팔 위팔°, 오른팔 아래팔°, 앞발 x, 뒷발 x)
KEYS = [
    (3.0, 2.0, 245, 245, 43, 27),
    (4.0, 3.0, 230, 235, 43, 27),
    (3.5, 3.0, 275, 300, 43, 27),
    (2.5, 2.0, 305, 335, 42, 28),
    (1.5, 1.0, 340, 15, 41, 29),
    (0.5, 0.0, 15, 50, 40, 30),
    (0.0, -0.5, 50, 80, 40, 30),
    (0.0, -1.0, 80, 100, 40, 30),
]


def ik(hip, ankle, l1, l2, fwd=1):
    dx, dy = ankle[0] - hip[0], ankle[1] - hip[1]
    d = min(math.hypot(dx, dy), l1 + l2 - 0.01)
    a = math.atan2(dy, dx)
    ca = (l1 * l1 + d * d - l2 * l2) / (2 * l1 * d)
    off = math.acos(max(-1, min(1, ca)))
    k = (hip[0] + l1 * math.cos(a - fwd * off), hip[1] + l1 * math.sin(a - fwd * off))
    return k


def pose(key):
    sink, lean, a1, a2, fx, bx = key
    P = {k: [v[0], v[1]] for k, v in IDLE.items()}
    # 몸 전체: 골반 낮춤, 상체(목 위) 앞기울기
    for k in ("LEFT HIP", "RIGHT HIP"):
        P[k][1] += sink
    for k in ("NECK", "NOSE", "LEFT EYE", "RIGHT EYE", "LEFT EAR", "RIGHT EAR", "LEFT SHOULDER", "RIGHT SHOULDER"):
        P[k][1] += sink
        P[k][0] += lean
    # 왼팔(앞): 가슴 앞에 접어 고정
    ls = P["LEFT SHOULDER"]
    P["LEFT ELBOW"] = [ls[0] + 1.5, ls[1] + 7]
    P["LEFT ARM"] = [ls[0] + 5, ls[1] + 3.5]
    # 오른팔: 어깨 → 팔꿈치 → 손목 (수학 각도, 0=오른쪽 90=위)
    rs = P["RIGHT SHOULDER"]
    e = (rs[0] + UP_LEN * math.cos(math.radians(a1)), rs[1] - UP_LEN * math.sin(math.radians(a1)))
    P["RIGHT ELBOW"] = list(e)
    P["RIGHT ARM"] = [e[0] + FORE_LEN * math.cos(math.radians(a2)), e[1] - FORE_LEN * math.sin(math.radians(a2))]
    # 다리: 발은 땅(57.5)에 고정, 무릎은 앞으로
    gl, gr = (fx, 57.0), (bx, 57.0)
    P["LEFT LEG"], P["RIGHT LEG"] = list(gl), list(gr)
    P["LEFT KNEE"] = list(ik(P["LEFT HIP"], gl, THIGH, SHIN, 1))
    P["RIGHT KNEE"] = list(ik(P["RIGHT HIP"], gr, THIGH, SHIN, 1))
    return P


def to_kp(P):
    return [{"label": k, "x": round(P[k][0] / 64, 4), "y": round(P[k][1] / 64, 4), "z_index": int(IDLE[k][2])} for k in IDLE]


BONES = [("NECK", "RIGHT SHOULDER"), ("RIGHT SHOULDER", "RIGHT ELBOW"), ("RIGHT ELBOW", "RIGHT ARM"), ("NECK", "LEFT SHOULDER"),
         ("LEFT SHOULDER", "LEFT ELBOW"), ("LEFT ELBOW", "LEFT ARM"), ("NECK", "RIGHT HIP"), ("NECK", "LEFT HIP"), ("RIGHT HIP", "LEFT HIP"),
         ("RIGHT HIP", "RIGHT KNEE"), ("RIGHT KNEE", "RIGHT LEG"), ("LEFT HIP", "LEFT KNEE"), ("LEFT KNEE", "LEFT LEG"), ("NECK", "NOSE")]


def draw(P, S=8):
    im = Image.new("RGB", (64 * S, 64 * S), (70, 80, 100))
    d = ImageDraw.Draw(im)
    for a, b in BONES:
        col = (255, 90, 90) if "RIGHT" in a + b and ("SHOULDER" in a + b or "ELBOW" in a + b or "ARM" in a + b) and "LEFT" not in a + b else (200, 220, 255)
        d.line([P[a][0] * S, P[a][1] * S, P[b][0] * S, P[b][1] * S], fill=col, width=4)
    d.ellipse([P["RIGHT ARM"][0] * S - 8, P["RIGHT ARM"][1] * S - 8, P["RIGHT ARM"][0] * S + 8, P["RIGHT ARM"][1] * S + 8], fill=(255, 60, 60))
    d.ellipse([P["NOSE"][0] * S - 10, P["NOSE"][1] * S - 10, P["NOSE"][0] * S + 10, P["NOSE"][1] * S + 10], outline=(255, 255, 0), width=3)
    d.line([0, 57 * S, 64 * S, 57 * S], fill=(120, 200, 120), width=2)
    return im


if __name__ == "__main__":
    Ps = [pose(k) for k in KEYS]
    json.dump([to_kp(P) for P in Ps], open(os.path.join(ROOT, "Logs/motion_review/up_keys.json"), "w"))
    ref = Image.open(os.path.join(ROOT, "Docs/ArtCompare/CharacterGen/ds01/motions/idle/raw_0.png")).convert("RGBA")
    frames = []
    for P in Ps:
        s = draw(P)
        bg = Image.new("RGBA", (64, 64), (70, 80, 100, 255))
        bg.alpha_composite(ref)
        out = Image.new("RGB", (1024, 512))
        out.paste(bg.resize((512, 512), Image.NEAREST).convert("RGB"), (0, 0))
        out.paste(s.resize((512, 512)), (512, 0))
        frames.append(out)
    frames[0].save(os.path.join(ROOT, "Logs/motion_review/up_stick.gif"), save_all=True, append_images=frames[1:], duration=200, loop=0)
    sheet = Image.new("RGB", (256 * 8, 256))
    for i, P in enumerate(Ps):
        sheet.paste(draw(P).resize((256, 256)), (i * 256, 0))
    sheet.save(os.path.join(ROOT, "Logs/motion_review/up_stick_sheet.png"))
