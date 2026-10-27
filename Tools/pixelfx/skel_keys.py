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
    # v7(10-27): 리틀 본 AttackA·B 자세를 따른다 — 몸은 거의 세운 채, 준비 높이와 칼 궤적으로 보여 준다(사용자: 올려베기 허우적댐)
    "SwingDown": [((42, 45), 5, 60, 100, -100, -100, -100, -92, -80, -92, 80), [
        (((40.5, 45.5), -8, 120, 170, -90, -95, -105, -95, -75, -92, 165), 3),   # 칼을 어깨 뒤로 높이(준비)
        (((43, 47), 18, -30, -60, -100, -100, -115, -100, -62, -96, -40), 2),     # 앞 아래로 내리침 + 앞발 디딤
        (((43, 47), 17, -33, -62, -100, -100, -115, -100, -62, -96, -42), 1),
     ], [(IDLEK, 3)]],
    "SwingUp": [((42.5, 47), 18, -140, -160, -100, -100, -115, -100, -62, -96, -160), [
        (((41.5, 45.5), 6, -40, 10, -95, -100, -100, -92, -80, -92, -40), 2),      # 몸을 세우며 칼이 앞으로
        (((40.5, 44), 0, 75, 95, -90, -100, -95, -90, -85, -90, 95), 2),            # 머리 위 정면(뒤로 넘기지 않음)
        (((40.5, 44), 0, 78, 98, -90, -100, -95, -90, -85, -90, 100), 2),
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
