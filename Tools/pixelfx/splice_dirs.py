"""측면 셀 + 방향 프레임 이어 붙이기(10-29 밤, MotionSpec39 §18~19): register.json 의 측면 원본(<셀>_side 로 한 번 보관)에서 프레임을 골라
방향 프레임(m41/dirs/<폴더>/<번호>, 손 = dir_keys 의 RIGHT/LEFT ARM)과 섞어 같은 이름 셀로 다시 등록한다. 회전 베기(사용자 합격) 방식.
사용: python Tools/pixelfx/splice_dirs.py  → 이어서 build_cel.py
항목: 숫자 = 측면 원본 프레임(시간 생략 시 원본 시간), (폴더, keys 동작, 번호, 무기 각도, 시간, 몸 뒤) = 방향 프레임
"""
import json
import os

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
M41 = os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel", "m41")
REG = os.path.join(M41, "register.json")
KEYS = os.path.join(M41, "keys_d4.json")

SPLICE = {
    "Iaido": ["0", "1", ("IaidoNE", "D_IaidoNE", 0, -120, .08, True), ("IaidoNE", "D_IaidoNE", 1, -125, .16, True),
              ("IaidoNE", "D_IaidoNE", 2, -60, .03, True), "4", "5", "6", "7", "8", "9", "10", "11", ("13", .08)],
    "Throw": ["0", ("ThrowNE", "D_ThrowNE", 0, 110, .05, False), ("ThrowNE", "D_ThrowNE", 1, 120, .18, False),
              ("ThrowNE", "D_ThrowNE", 2, 90, .03, False), "3", "4", "5", "6", "7", "8", "9", "10", "11", "13"],
    "GreatUpper": ["0", "1", ("UpperNE", "D_UpperNE", 0, 182, .1, True), ("UpperNE", "D_UpperNE", 1, 186, .18, True),
                   ("UpperNE", "D_UpperNE", 2, 200, .05, True), "4", "5", "6", "7", "8", "9", "10", "11", "13"],
    "Pull": ["0", "1", "2", "3", "4", "5", ("PullS", "D_PullS", 0, 5, .06, False), ("PullS", "D_PullS", 1, 10, .15, False),
             ("PullS", "D_PullS", 2, 15, .1, False), "8", "9", "10", "11", "13"],
    "Raise": ["0", "1", "2", "3", "4", ("RaiseS", "D_RaiseS", 0, 60, .05, False), ("RaiseS", "D_RaiseS", 1, 85, .05, False),
              ("RaiseS", "D_RaiseS", 2, 90, .35, False), "12", "13"],
    "ShieldBash": ["0", "1", ("BashNE", "D_BashNE", 0, -100, .05, False), ("BashNE", "D_BashNE", 1, -100, .12, False),
                   ("BashNE", "D_BashNE", 2, -95, .04, False), "4", "5", "6", "7", "8", "9", "10", "11", "13"],
    "LowSweep": ["0", "1", ("SweepNE", "D_SweepNE", 0, 165, .05, True), ("SweepNE", "D_SweepNE", 1, 170, .15, True),
                 ("SweepNE", "D_SweepNE", 2, 200, .04, True), "3", "4", "5", "6", "7", "8", "9", "10", "12"],
    # 새 셀: 납도(발도 끝 자세 → 3/4 앞으로 돌아서며 칼집에) · 승리 포즈(정면)
    "Sheathe": [("7@Iaido", .12), ("9@Iaido", .045), ("SheatheSE", "D_SheatheSE", 0, 90, .08, False),
                ("SheatheSE", "D_SheatheSE", 1, 160, .06, False), ("SheatheSE", "D_SheatheSE", 2, 175, .35, False)],
    "Victory": [("VictoryS", "D_VictoryS", 0, 45, .06, False), ("VictoryS", "D_VictoryS", 1, 80, .06, False),
                ("VictoryS", "D_VictoryS", 2, 90, .6, False)],
}


def joint(keys, name, i, label):
    q = next(p for p in keys[name]["kp"][i] if p["label"] == label)
    return [int(round(q["x"] * 64)), int(round(q["y"] * 64))]


def main():
    reg = json.load(open(REG, encoding="utf-8"))
    keys = json.load(open(KEYS, encoding="utf-8"))
    for cel, items in SPLICE.items():
        if cel in reg and cel + "_side" not in reg and "dirs/" not in "".join(reg[cel]["frames"]):
            reg[cel + "_side"] = reg[cel]
        out = {"frames": [], "hand": [], "handB": [], "angle": [], "times": [], "behind": []}
        for it in items:
            if isinstance(it, tuple) and len(it) == 6:
                folder, kname, i, ang, t, back = it
                out["frames"].append(f"m41/dirs/{folder}/{i}")
                out["hand"].append(joint(keys, kname, i, "RIGHT ARM"))
                out["handB"].append(joint(keys, kname, i, "LEFT ARM"))
                out["angle"].append(ang)
                out["times"].append(t)
                out["behind"].append(back)
                continue
            ref, t = (it if isinstance(it, tuple) else (it, None))
            idx, _, src = ref.partition("@")
            base = reg[(src or cel) + "_side"] if (src or cel) + "_side" in reg else reg[src or cel]
            k = int(idx)
            out["frames"].append(base["frames"][k])
            out["hand"].append(base["hand"][k])
            out["handB"].append(base.get("handB", base["hand"])[k])
            out["angle"].append(base["angle"][k])
            out["times"].append(base["times"][k] if t is None else t)
            out["behind"].append(False)
        out["motion"] = f"splice_dirs({cel})"
        reg[cel] = out
        print(cel, len(out["frames"]), round(sum(out["times"]), 2))
    json.dump(reg, open(REG, "w", encoding="utf-8"), ensure_ascii=False, indent=1)


if __name__ == "__main__":
    main()
