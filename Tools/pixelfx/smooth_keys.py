"""보간 다듬기(10-29 사용자 "3기본공격처럼 보간 느낌이 안 산다"): 키 포즈는 그대로, 사이 프레임이 있던 구간마다 1장 더(전환 0.03~0.045초).
통과본 칼 3타처럼 전환은 촘촘히, 멈춤은 키에서만. 14장(생성 3회 상한)을 넘으면 끝 구간부터 그대로 둔다.
출력: keys json(S11_<셀>) + 옛 프레임 번호 → 새 번호 표(이펙트 타격 프레임·재생 프로파일 고칠 때 씀).
사용: python Tools/pixelfx/smooth_keys.py <출력.json> <동작 이름(stick_new.MOTIONS 키)>...
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stick_new as s  # noqa: E402
from skel_keys import IDLEK  # noqa: E402
from skel_pose import norm, pose  # noqa: E402
from stick_prev import grounded  # noqa: E402

MAX = 14


def smooth(m):
    start, main, end = m["keys"]
    times = (m["times"] + [m["times"][-1]] * 40)
    segs = [(k, n, "m") for k, n in main] + [(k, n, "e") for k, n in end]
    total = sum(n for _, n, _ in segs)
    add = [n >= 2 for _, n, _ in segs]
    # 상한을 넘으면 끝 구간부터 추가를 끈다
    i = len(segs) - 1
    while total + sum(add) > MAX and i >= 0:
        add[i] = False
        i -= 1
    new_main, new_end, new_times, mapping = [], [], [], {}
    old = new = 0
    for (k, n, where), a in zip(segs, add):
        seg_t = times[old:old + n]
        n2 = n + 1 if a else n
        inb_t = [t for t in seg_t[:-1]]
        fast = min(inb_t) if inb_t else 45
        t_in = 30 if fast <= 30 else 45
        new_seg_t = [t_in] * (n2 - 1) + [seg_t[-1]]
        for j in range(n):  # 옛 번호 → 새 번호(키는 키로, 사이 프레임은 같은 순서로)
            mapping[old + j] = new + (n2 - 1 if j == n - 1 else j)
        (new_main if where == "m" else new_end).append((k, n2))
        new_times += new_seg_t
        old += n
        new += n2
    return {"keys": [start, new_main, new_end], "times": new_times}, mapping


def main():
    out, names = sys.argv[1], sys.argv[2:]
    res = {"_idle": norm(pose(grounded(IDLEK[0][0], *IDLEK[1:10]), *IDLEK[1:10]))}
    maps = {}
    for name in names:
        m2, mp = smooth(s.MOTIONS[name])
        fr = s.frames(m2["keys"])
        kps = [pose(grounded(*k), *k[1:]) for k, _ in fr]
        key = "S11_" + name.split("_", 1)[1]
        res[key] = {"kp": [norm(d) for d in kps], "hand": [[int(round(d["RIGHT ARM"][0])), int(round(d["RIGHT ARM"][1]))] for d in kps],
                    "angle": [round(a, 1) for _, a in fr], "times": m2["times"][:len(fr)]}
        maps[key] = mp
        print(key, len(fr), mp)
    json.dump(res, open(out, "w"))
    json.dump(maps, open(out.replace(".json", "_map.json"), "w"))


if __name__ == "__main__":
    main()
