"""무기 각도를 팔뚝 방향에 묶는다(10-29 사용자 "무기 움직임이 부자연스럽다"): 졸라맨 키에서 칼 각도를 따로 보간해 사이 프레임에서 칼이 팔뚝과 반대로 돌았다.
각 프레임 무기 각도 = 팔뚝(팔꿈치 → 손목) 각도 + 손목 꺾임(±LIMIT 안으로 자름). register.json 의 m41 동작 각도를 고친다(생성 0).
사용: python Tools/pixelfx/fix_weapon_angles.py
"""
import glob
import json
import math
import os

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
M41 = os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel", "m41")
LIMIT = 60.0


def wrap(a):
    return (a + 180.0) % 360.0 - 180.0


def main():
    keys = {}
    for p in glob.glob(os.path.join(M41, "keys_*.json")):
        if p.endswith("_map.json"):
            continue
        for k, v in json.load(open(p, encoding="utf-8")).items():
            if not k.startswith("_"):
                keys[k] = v
    reg_p = os.path.join(M41, "register.json")
    reg = json.load(open(reg_p, encoding="utf-8"))
    for name, r in reg.items():
        k = keys.get(r.get("motion", ""))
        if not k or len(k["kp"]) != len(r["angle"]):
            continue
        out, moved = [], 0
        for f, a in zip(k["kp"], k["angle"]):
            d = {q["label"]: (q["x"], q["y"]) for q in f}
            e, w = d["RIGHT ELBOW"], d["RIGHT ARM"]
            fore = math.degrees(math.atan2(-(w[1] - e[1]), w[0] - e[0]))
            wr = wrap(a - fore)
            wr2 = max(-LIMIT, min(LIMIT, wr))
            moved += abs(wr2 - wr) > 1
            out.append(round(fore + wr2, 1))
        r["angle"] = out
        print(f"{name:13s} 고친 프레임 {moved}/{len(out)}")
    json.dump(reg, open(reg_p, "w", encoding="utf-8"), ensure_ascii=False, indent=1)


if __name__ == "__main__":
    main()
