"""관절 길이를 대기와 똑같이 고정한 COCO-18 자세 생성기(10-27 사용자 규칙: 신체 부위가 길어지거나 커지면 안 된다).
자세 = 골반 위치 + 상체 기울기 + 팔·다리 관절 각도(도, 0 = 오른쪽, 90 = 위). 길이는 대기 그림에서 잰 값 고정.
사용: python Tools/pixelfx/skel_pose.py <출력.json>  → {"first": 대기 관절, "<동작>": [프레임 관절...], "hand": {"<동작>": [[x,y],...]}}
손(칼 손 = RIGHT ARM 손목) 좌표는 그대로 cel.json 손 좌표로 쓴다.
"""
import json
import math
import sys

W = 64.0
THIGH, SHIN, UPPER, FORE = 5.0, 4.5, 5.2, 5.0
NECK_UP, SHOULDER_W, HIP_W = 13.0, 3.5, 2.5  # 골반→목, 어깨·골반 반폭
IDLE = {"NOSE": (44, 23), "RIGHT EYE": (45, 21), "LEFT EYE": (41, 21), "RIGHT EAR": (38, 21), "LEFT EAR": (36, 21), "NECK": (40, 31),
        "RIGHT SHOULDER": (37, 33), "RIGHT ELBOW": (35, 38), "RIGHT ARM": (34, 43), "LEFT SHOULDER": (44, 33), "LEFT ELBOW": (47, 38), "LEFT ARM": (49, 43),
        "RIGHT HIP": (38, 44), "RIGHT KNEE": (37, 49), "RIGHT LEG": (37, 53), "LEFT HIP": (43, 44), "LEFT KNEE": (44, 49), "LEFT LEG": (45, 53)}


def p(o, ang, ln):
    return (o[0] + ln * math.cos(math.radians(ang)), o[1] - ln * math.sin(math.radians(ang)))


def pose(hip, lean, ra, rf, la, lf, rt, rs, lt, ls):
    """hip = 골반 가운데, lean = 상체 기울기(도, + 앞), ra/rf = 칼 팔 위팔·아래팔 각도, la/lf = 반대 팔, rt/rs·lt/ls = 다리 허벅지·정강이 각도."""
    neck = p(hip, 90 - lean, NECK_UP)
    rsh, lsh = (neck[0] - SHOULDER_W, neck[1] + 2), (neck[0] + SHOULDER_W, neck[1] + 2)
    rh, lh = (hip[0] - HIP_W, hip[1]), (hip[0] + HIP_W, hip[1])
    re = p(rsh, ra, UPPER)
    le = p(lsh, la, UPPER)
    rk, lk = p(rh, rt, THIGH), p(lh, lt, THIGH)
    d = {"NECK": neck, "RIGHT SHOULDER": rsh, "RIGHT ELBOW": re, "RIGHT ARM": p(re, rf, FORE), "LEFT SHOULDER": lsh, "LEFT ELBOW": le,
         "LEFT ARM": p(le, lf, FORE), "RIGHT HIP": rh, "RIGHT KNEE": rk, "RIGHT LEG": p(rk, rs, SHIN), "LEFT HIP": lh, "LEFT KNEE": lk, "LEFT LEG": p(lk, ls, SHIN)}
    hx, hy = neck[0] - IDLE["NECK"][0], neck[1] - IDLE["NECK"][1]
    for k in ("NOSE", "RIGHT EYE", "LEFT EYE", "RIGHT EAR", "LEFT EAR"):
        d[k] = (IDLE[k][0] + hx, IDLE[k][1] + hy)
    return d


# 다리: 뒷다리(RIGHT) 뒤로, 앞다리(LEFT) 앞으로. 각도는 아래가 -90
STAND = dict(rt=-95, rs=-90, lt=-85, ls=-90)
MOTIONS = {
    # 1타 찌르기: 허리로 당김 → 뒷발 밀며 출발 → 찌름(팔 끝까지) → 유지 → 거둠
    "Stab": [
        pose((40.5, 44), 0, -110, -40, -80, -100, -100, -90, -80, -90),
        pose((40, 45), -5, -150, -20, -70, -100, -110, -80, -70, -95),
        pose((41, 45), 5, -120, 0, -90, -110, -120, -100, -60, -100),
        pose((43, 46), 15, -10, -5, -120, -120, -125, -110, -55, -95),
        pose((43, 46), 15, -5, -5, -125, -120, -125, -110, -55, -95),
        pose((42, 45), 8, -40, -20, -110, -110, -115, -100, -65, -95),
    ],
    # 2타 내려베기: 들어 올림 → 머리 위 → 내리침(앞 아래) → 낮게 받아냄
    "SwingDown": [
        pose((42, 45), 5, 30, 60, -100, -100, -115, -100, -65, -95),
        pose((41, 45), -5, 80, 110, -110, -100, -110, -95, -70, -95),
        pose((41, 45), -10, 110, 150, -110, -100, -110, -95, -70, -95),
        pose((42, 46), 15, 10, -30, -120, -120, -125, -105, -55, -95),
        pose((43, 47), 22, -35, -60, -125, -120, -130, -110, -50, -100),
        pose((43, 47), 22, -40, -65, -125, -120, -130, -110, -50, -100),
    ],
    # 3타 올려베기: 낮은 압축(칼 아래 뒤) → 솟구치며 앞으로 → 머리 위 → 머리 뒤 → 회복
    "SwingUp": [
        pose((42, 47), 20, -130, -150, -110, -100, -130, -105, -55, -100),
        pose((42, 46), 10, -90, -60, -110, -100, -120, -100, -60, -95),
        pose((42, 45), 0, 20, 70, -100, -100, -110, -95, -70, -92),
        pose((41, 44), -8, 70, 95, -100, -100, -100, -92, -78, -90),
        pose((41, 44), -12, 100, 130, -100, -100, -100, -92, -78, -90),
        pose((40.5, 44), -5, 60, 140, -95, -95, -98, -90, -82, -90),
        pose((40.5, 44), 0, -80, -100, -85, -95, -95, -90, -85, -90),
    ],
}
# 칼 각도(도) — 손목 관절에 붙인다. 준비·타격·마무리 흐름을 사람이 정한다
ANGLES = {
    "Stab": [-15, 5, 0, 0, 0, -30],
    "SwingDown": [80, 115, 150, 20, -35, -40],
    "SwingUp": [-160, -110, 60, 95, 130, 150, -40],
}


def norm(d):
    return [{"label": k, "x": round(v[0] / W, 4), "y": round(v[1] / W, 4)} for k, v in d.items()]


def main():
    out = {"first": norm(IDLE), "angles": ANGLES, "hand": {}}
    for m, fr in MOTIONS.items():
        out[m] = [norm(f) for f in fr]
        out["hand"][m] = [[int(round(f["RIGHT ARM"][0])), int(round(f["RIGHT ARM"][1]))] for f in fr]
        for f in fr:
            lens = [math.dist(f["RIGHT HIP"], f["RIGHT KNEE"]), math.dist(f["RIGHT KNEE"], f["RIGHT LEG"]), math.dist(f["RIGHT SHOULDER"], f["RIGHT ELBOW"]), math.dist(f["RIGHT ELBOW"], f["RIGHT ARM"])]
            assert all(abs(a - b) < 0.01 for a, b in zip(lens, (THIGH, SHIN, UPPER, FORE)))
    json.dump(out, open(sys.argv[1], "w"), indent=1)
    print({m: out["hand"][m] for m in MOTIONS})


if __name__ == "__main__":
    main()
