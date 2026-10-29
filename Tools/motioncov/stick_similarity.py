"""졸라맨(키포인트) 단계 구별도: 동작 키 json 들에서 동작마다 가장 비슷한 동작까지 거리. 관절 위치(px)·칼 각도, 순서 무시 양방향 최소 평균.
사용: python Tools/motioncov/stick_similarity.py <keys json>... (뒤에 오는 같은 셀 이름이 앞을 덮는다)
"""
import io
import json
import sys

import numpy as np

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
CEL = {"A4_GreatDown": "GreatDown", "A12_Throw": "Throw", "A5b_OneHandSide": "GreatSideOne", "A5_GreatSide": "GreatSide", "R2_Raise": "Raise",
       "A11_Cast": "Cast", "R6_Place": "Place", "A10_Spin": "Spin", "A6_Flurry": "DaggerFlurry", "A15_Charge": "Charge", "A17_Backstep": "Backstep",
       "A14_Guard": "Guard", "R3_Grab": "Grab", "A9_Lunge": "Lunge", "A8_AirSlash": "AirSlash", "R10_Pull": "Pull", "R7_Kick": "Kick",
       "A18_Punch": "Punch", "A7_Cross": "DaggerCross", "R4_Ride": "Ride", "R8_ShieldBash": "ShieldBash", "A19_Iaido": "Iaido",
       "A16_LeapSlam": "LeapSlam", "R9_Slam": "Slam", "R1_ShoulderAim": "ShoulderAim", "A13_Bow": "Bow"}
MERGED = {"Slam", "LeapSlam", "DaggerFlurry", "Grab", "Charge"}


def load(paths):
    m = {}
    for p in paths:
        for k, v in json.load(open(p, encoding="utf-8")).items():
            if k.startswith("_"):
                continue
            cel = CEL.get(k.split("_", 1)[1])
            if cel and cel not in MERGED:
                fr = []
                for f, a in zip(v["kp"], v["angle"]):
                    d = {q["label"]: (q["x"] * 64, q["y"] * 64) for q in f}
                    fr.append((np.array([d[l] for l in sorted(d)]), a))
                m[cel] = fr
    return m


def fd(x, y):
    return np.mean(np.hypot(*(x[0] - y[0]).T)) / 10 + 0.5 * abs((x[1] - y[1] + 180) % 360 - 180) / 180


def dist(A, B):
    return (np.mean([min(fd(a, b) for b in B) for a in A]) + np.mean([min(fd(b, a) for a in A) for b in B])) / 2


def main():
    m = load(sys.argv[1:])
    for n in sorted(m):
        v, o = min((dist(m[n], m[k]), k) for k in m if k != n)
        print(f"{n:13s} {v:.3f} {o}")


if __name__ == "__main__":
    main()
