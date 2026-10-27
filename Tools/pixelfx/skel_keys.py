"""v6 자세표: 스컬처럼 크게 쓰는 키 자세 + 사이 보간(완급). 길이 고정은 skel_pose.pose 그대로.
사용: python Tools/pixelfx/skel_keys.py <출력.json>  → {동작: {"kp": [...], "hand": [...], "angle": [...], "main": n}}
키 = (hip, lean, ra, rf, la, lf, rt, rs, lt, ls, 칼각도). 구간 = (다음 키, 프레임 수) — 보간은 앞뒤를 부드럽게(smoothstep).
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
from skel_pose import norm, pose  # noqa: E402

IDLEK = ((40.5, 44), 0, -110, -80, -80, -100, -95, -90, -85, -90, -20)
KEYS = {
    "Stab": [IDLEK, [  # 뒤로 감기 → 끝까지 찌름 → 유지 | 회복
        (((38, 46), -12, -170, -15, -60, -60, -120, -95, -70, -95, 0), 3),
        (((44.5, 47), 25, -5, -3, -150, -120, -138, -118, -48, -98, 0), 2),
        (((44.5, 47), 24, -8, -6, -148, -118, -138, -118, -48, -98, -5), 1),
     ], [(IDLEK, 3)]],
    "SwingDown": [((43, 46), 10, 40, 80, -120, -110, -115, -100, -65, -95, 60), [
        (((40.5, 45), -18, 115, 165, -100, -90, -112, -95, -72, -92, 160), 3),
        (((45, 48), 30, -25, -55, -140, -100, -138, -116, -45, -100, -40), 2),
        (((45, 48), 28, -30, -60, -138, -100, -138, -116, -45, -100, -45), 1),
     ], [(IDLEK, 3)]],
    "SwingUp": [((44.5, 48), 30, -140, -165, -110, -100, -138, -116, -45, -100, -160), [
        (((43, 46), 10, -60, -25, -110, -100, -120, -100, -60, -95, -80), 2),
        (((42, 44), -12, 80, 100, -100, -100, -100, -92, -78, -90, 95), 2),
        (((41, 44), -20, 125, 155, -95, -95, -98, -90, -82, -90, 150), 2),
        ((IDLEK), 4),
     ], []],
}


def ss(t):
    return t * t * (3 - 2 * t)


def lerp(a, b, t):
    if isinstance(a, tuple):
        return tuple(lerp(x, y, t) for x, y in zip(a, b))
    return a + (b - a) * t


def run(start, segs):
    out, cur = [], start
    for key, n in segs:
        for i in range(1, n + 1):
            out.append(lerp(cur, key, ss(i / n)))
        cur = key
    return out, cur


def main():
    res = {}
    for m, (start, main_segs, end_segs) in KEYS.items():
        a, last = run(start, main_segs)
        b, _ = run(last, end_segs)
        frames = a + b
        kps = [pose(*f[:10]) for f in frames]
        res[m] = {"kp": [norm(k) for k in kps], "hand": [[int(round(k["RIGHT ARM"][0])), int(round(k["RIGHT ARM"][1]))] for k in kps],
                  "angle": [round(f[10], 1) for f in frames], "main": len(a)}
        print(m, len(frames), "main", len(a), res[m]["hand"])
    json.dump(res, open(sys.argv[1], "w"))


if __name__ == "__main__":
    main()
