"""보간 매끄러움 검사(10-29 밤 사용자 "3타 품질이 나오는지"): 셀마다 프레임 사이 무기 끝 이동량(px)과 속도(px/초)를 재서 통과본 3타와 비교한다.
무기 끝 = 손 + 각도 방향 20px. 큰 점프(한 프레임에 18px 넘게) = 뚝 끊김 후보, 긴 멈춤 = 0.15초 넘는 프레임.
사용: python Tools/motioncov/smoothness.py [셀 이름 ...]
"""
import json
import math
import os
import sys

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
CEL = os.path.join(ROOT, "Assets", "Art", "Player", "PixelRig", "Cel", "cel.json")
BASIC = ["SwingDown", "SwingDownEnd", "SwingUp", "Stab", "StabEnd"]
SKIP = {"Walk", "Run", "JumpCrouch", "JumpArc", "Land", "Dash", "Hurt", "Death", "Idle", "RopeStay"}


def tip(f):
    a = math.radians(f["angle"])
    return f["hand"][0] + 20 * math.cos(a), f["hand"][1] - 20 * math.sin(a)


def stats(frames):
    steps = []
    for p, q in zip(frames, frames[1:]):
        (x0, y0), (x1, y1) = tip(p), tip(q)
        hand = math.dist(p["hand"], q["hand"])
        steps.append(max(math.hypot(x1 - x0, y1 - y0), hand))
    jumps = sum(1 for s in steps if s > 18)
    holds = sum(1 for f in frames if f["time"] > 0.15)
    total = sum(f["time"] for f in frames)
    path = sum(steps)
    return {"n": len(frames), "t": round(total, 2), "path": round(path), "max": round(max(steps) if steps else 0),
            "jumps": jumps, "holds": holds, "even": round((max(steps) / (path / len(steps))) if steps and path else 0, 1)}


def main():
    c = json.load(open(CEL, encoding="utf-8"))
    names = sys.argv[1:] or [k for k in c if k not in SKIP]
    print("셀            장  초    경로  최대  점프 멈춤 고르기(최대/평균)")
    for k in names:
        s = stats(c[k]["frames"])
        mark = "  기준" if k in BASIC else ("  ← 끊김" if s["jumps"] >= 2 or s["even"] > 3.2 else "")
        print(f"{k:13} {s['n']:3} {s['t']:4} {s['path']:5} {s['max']:4} {s['jumps']:4} {s['holds']:4} {s['even']:5}{mark}")


if __name__ == "__main__":
    main()
