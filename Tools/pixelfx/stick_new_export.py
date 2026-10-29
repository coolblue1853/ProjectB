"""stick_new.MOTIONS(승인된 졸라맨) → 생성용 keys json(stick_export 와 같은 형식) + PixelLab 첫 프레임 키포인트.
사용: python Tools/pixelfx/stick_new_export.py <출력.json> <접두사...>
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stick_new as s  # noqa: E402
from skel_keys import IDLEK  # noqa: E402
from skel_pose import norm, pose  # noqa: E402
from stick_prev import grounded  # noqa: E402


def main():
    out, pre = sys.argv[1], sys.argv[2:]
    res = {"_idle": norm(pose(grounded(IDLEK[0][0], *IDLEK[1:10]), *IDLEK[1:10]))}
    for name, m in s.MOTIONS.items():
        if pre and not any(name.startswith(p) for p in pre):
            continue
        fr = s.frames(m["keys"])
        kps = [pose(grounded(*k), *k[1:]) for k, _ in fr]
        times = (m["times"] + [m["times"][-1]] * len(fr))[:len(fr)]
        res[name] = {"kp": [norm(d) for d in kps], "hand": [[int(round(d["RIGHT ARM"][0])), int(round(d["RIGHT ARM"][1]))] for d in kps],
                     "angle": [round(a, 1) for _, a in fr], "times": times}
        print(name, len(fr))
    json.dump(res, open(out, "w"))


if __name__ == "__main__":
    main()
