"""승인된 졸라맨 수정안(stick_gif.PROPOSAL/ANGLES)을 생성용 keys_vN.json 으로 내보낸다(skel_keys 와 같은 형식).
사용: python Tools/pixelfx/stick_export.py <출력.json> [동작...]
발은 땅에 붙이고(grounded), 칼끝은 땅 위로 보정된 각도(stick_gif.frames_of)를 그대로 쓴다.
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stick_gif as g  # noqa: E402
from skel_keys import run  # noqa: E402
from skel_pose import norm, pose  # noqa: E402
from stick_prev import grounded  # noqa: E402


def main():
    out, names = sys.argv[1], sys.argv[2:] or list(g.PROPOSAL)
    res = {}
    for m in names:
        start, main_segs, end_segs = g.PROPOSAL[m]
        n_main = len(run(start, main_segs)[0])
        frames = g.frames_of(g.PROPOSAL[m], g.ANGLES[m])
        kps = [pose(grounded(*k), *k[1:]) for k, _ in frames]
        res[m] = {"kp": [norm(d) for d in kps], "hand": [[int(round(d["RIGHT ARM"][0])), int(round(d["RIGHT ARM"][1]))] for d in kps],
                  "angle": [round(a, 1) for _, a in frames], "main": n_main, "times": g.TIMES[m]}
        print(m, len(frames), "main", n_main, res[m]["hand"])
    json.dump(res, open(out, "w"))


if __name__ == "__main__":
    main()
