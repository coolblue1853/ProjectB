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
    # v9(10-27): 웹 레퍼런스(rvros·SLYNYRD·itch 튜토리얼) 몸 전체 자세 — 주저앉음/솟음 + 반대손 가드·반작용. 막대 인형 `stick_prev.py` 로 사용자 승인
    # 칼이 지나가는 사이 프레임(베기 중간)은 나중에 스미어로 덮는다. 골반 y 는 GROUNDED 로 매 프레임 발을 땅에 붙인다
    "SwingDown": [IDLEK, [
        (((40, 44), -8, 110, 150, -10, 20, -110, -95, -70, -90, 140), 2),           # 준비: 칼 어깨 위, 반대손 앞 가드
        (((40, 44), -9, 112, 152, -10, 22, -110, -95, -70, -90, 142), 1),           # 준비 유지
        (((43, 46), 28, -5, -15, -140, -120, -150, -100, -25, -95, -15), 2),        # 베기: 주저앉고 앞발 딛고 숙임, 칼 앞 수평, 반대손 뒤로
        (((43, 46), 30, -8, -20, -145, -125, -150, -100, -25, -95, -20), 1),        # 유지
     ], [(IDLEK, 3)]],
    "SwingUp": [IDLEK, [
        (((42, 45.7), 25, -130, -150, -10, 10, -145, -100, -25, -95, -165), 2),     # 준비: 웅크림, 칼 뒤 아래, 반대손 앞 가드
        (((42, 45.7), 26, -132, -152, -10, 12, -145, -100, -25, -95, -165), 1),     # 준비 유지
        (((42, 44), 5, 20, 50, -130, -100, -120, -120, -70, -90, 80), 2),           # 올려베기: 뒷다리 펴며 솟음, 칼 앞 위, 반대손 뒤로
        (((41, 44), -8, 100, 130, -120, -100, -115, -115, -75, -90, 130), 2),       # 끝: 칼 머리 위 뒤, 몸 세움
        (((41, 44), -8, 102, 132, -120, -100, -115, -115, -75, -90, 132), 1),       # 유지
        ((IDLEK), 3),
     ], []],
}

# 보간 각도가 칼끝을 바닥 아래로 보내는 프레임만 손으로 고친다(칼은 코드로 붙이므로 재생성 불필요)
ANGLE_FIX = {"SwingUp": {0: -140}}  # v9 프레임 0: 대기(-20)→준비(-165) 보간이 칼끝을 바닥으로 보냄
GROUNDED = {"SwingDown", "SwingUp"}  # 발을 땅(y=53)에 붙이는 동작


def ground(f):
    """낮은 쪽 발이 땅(y=53)에 닿도록 골반 y 만 옮긴다(다리 길이 고정이라 주저앉으면 골반이 내려간다)."""
    d = pose(*f[:10])
    low = max(d["RIGHT LEG"][1], d["LEFT LEG"][1])
    return ((f[0][0], f[0][1] + 53.0 - low),) + tuple(f[1:])


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
        if m in GROUNDED:
            frames = [ground(f) for f in frames]
        kps = [pose(*f[:10]) for f in frames]
        res[m] = {"kp": [norm(k) for k in kps], "hand": [[int(round(k["RIGHT ARM"][0])), int(round(k["RIGHT ARM"][1]))] for k in kps],
                  "angle": [round(ANGLE_FIX.get(m, {}).get(i, f[10]), 1) for i, f in enumerate(frames)], "main": len(a)}
        print(m, len(frames), "main", len(a), res[m]["hand"])
    json.dump(res, open(sys.argv[1], "w"))


if __name__ == "__main__":
    main()
