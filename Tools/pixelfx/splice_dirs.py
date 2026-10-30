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
    "GreatUpper": ["0", "1", ("UpperNE", "D_UpperNE", 0, -178, .1, True), ("UpperNE", "D_UpperNE", 1, -174, .18, True),
                   ("UpperNE", "D_UpperNE", 2, -160, .05, True), "4", "5", "6", "7", "8", "9", "10", "11", "13"],
    # GPT 46동작 리뷰 "내미는 것처럼 보인다" → 던져 거는 앞부분은 빠르게, 정면으로 버텨 당기는 뒷부분을 길게
    "Pull": ["0", "1", ("2", .06), ("3", .04), "4", "5", ("PullS", "D_PullS", 0, 5, .08, False), ("PullS", "D_PullS", 1, 10, .2, False),
             ("PullS", "D_PullS", 2, 15, .16, False), "8", "9", "10", "11", "13"],
    "Raise": ["0", "1", "2", "3", "4", ("RaiseS", "D_RaiseS", 0, 60, .05, False), ("RaiseS", "D_RaiseS", 1, 85, .05, False),
              ("RaiseS", "D_RaiseS", 2, 90, .35, False), "12", "13"],
    "ShieldBash": ["0", "1", ("BashNE", "D_BashNE", 0, -100, .05, False), ("BashNE", "D_BashNE", 1, -100, .12, False),
                   ("BashNE", "D_BashNE", 2, -95, .04, False), "4", "5", "6", "7", "8", "9", "10", "11", "13"],
    "LowSweep": ["0", "1", ("SweepNE", "D_SweepNE", 0, -195, .05, True), ("SweepNE", "D_SweepNE", 1, -190, .15, True),
                 ("SweepNE", "D_SweepNE", 2, -160, .04, True), "3", "4", "5", "6", "7", "8", "9", "10", "12"],
    "Punch": ["0", "1", "2", "3", "4", "5", "6", "7", ("PunchNE", "D_PunchNE", 1, 0, .07, False), ("PunchSE", "D_PunchSE", 1, 0, .035, False),
              "10", "11", "12", "13"],  # 원투의 투: 3/4 뒤 장전 → 3/4 앞 통과 → 측면 스트레이트(GPT D4)
    # 검 든 채 사격: 3/4 앞으로 몸을 열고 권총(앞손)을 어깨 높이로, 검은 반대 허리 뒤(GPT 리뷰 "두 무기가 겹친다") — 사격 4·6번
    "SwordPistol": ["0", "1", "2", ("GunSE", "D_GunSE", 0, 200, .08, True), ("GunSE", "D_GunSE", 1, 200, .05, True),
                    ("GunSE", "D_GunSE", 0, 200, .1, True), ("GunSE", "D_GunSE", 1, 200, .05, True), ("GunSE", "D_GunSE", 2, 205, .14, True), "12", "13"],
    # 한손 베기 회전(GPT 2차 "골반·어깨 회전 부족"): 3/4 뒤 감기 → 측면 타격 → 3/4 앞 팔로스루 — 공용 프레임
    "DashSlash": ["0", "1", ("CoilNE", "D_CoilNE", 0, -170, .06, True), ("CoilNE", "D_CoilNE", 1, -165, .12, True), "3", "4", "5", "6",
                  ("FollowSE", "D_FollowSE", 0, -10, .06, True), ("FollowSE", "D_FollowSE", 1, -20, .1, True), "9", "10", "11", "13"],
    "GreatSideOne": ["0", "1", ("CoilNE", "D_CoilNE", 0, -170, .06, True), ("CoilNE", "D_CoilNE", 1, -165, .12, True), "3", "4", "5", "6",
                     ("FollowSE", "D_FollowSE", 0, -10, .08, True), ("FollowSE", "D_FollowSE", 1, -20, .1, True), "9", "10", "11", "13"],
    # 쌍검 X 교차 정면(GPT D3) — 7번째 값 = 앞손 무기 각도(angleB, 10-30 채널)
    "DaggerCross": ["0", "1", ("CrossS", "D_CrossS", 0, 135, .14, False, 45), ("3", .03), ("CrossS", "D_CrossS", 1, -135, .04, False, -45),
                    ("CrossS", "D_CrossS", 2, -40, .22, False, -140), "9", "10", "11"],
    # 모으기(마도학자): 정면으로 두 팔 엇갈려 웅크림 → 활짝 펼침 — 시전(측면)·소환(정면 V)과 몸 자세 구별(GPT 2차)
    "Charge": ["0", "1", ("ChargeS", "D_ChargeS", 0, 120, .12, False), ("ChargeS", "D_ChargeS", 1, 120, .25, False),
               ("ChargeS", "D_ChargeS", 2, 60, .33, False), "9", "10", "11", "12", "13"],
    # 넘어짐 → 일어남(GPT D1): 누운 뒤 정면으로 몸을 틀어 한쪽 무릎·한 손 짚고 일어섬
    "Knockdown": ["0", "1", "2", "3", "4", "5", "6", "7", "8", ("GetupS", "D_GetupS", 0, -20, .12, False), ("GetupS", "D_GetupS", 1, -20, .06, False),
                  ("GetupS", "D_GetupS", 2, -20, .05, False), "12", "13"],
    # GPT 46동작 리뷰 약한 동작: 장풍(3/4 앞 두 손바닥 밀기 — 재생 프로파일 대체)·메치기(앞으로 잡기 → 정면 머리 위로 들어 → 측면 내려꽂기, 생성 0)
    "PalmBlast": [("PalmSE2", "D_PalmSE2", 0, -20, .18, False), ("PalmSE2", "D_PalmSE2", 1, -20, .04, False), ("PalmSE2", "D_PalmSE2", 2, -20, .3, False), ("13@Pull", .1)],  # 10-30 품질 루프: 더 깊게 웅크렸다 크게 내딛음
    "Slam": [("2@Pull", .06), ("4@Pull", .1), ("RaiseS", "D_RaiseS", 1, 90, .06, False), ("RaiseS", "D_RaiseS", 2, 90, .14, False),
             ("3@GreatDown", .06), ("4@GreatDown", .1), ("5@GreatDown", .03), ("6@GreatDown", .03), ("7@GreatDown", .05), ("8@GreatDown", .25), ("10@GreatDown", .06), ("12@GreatDown", .06), ("13@GreatDown", .1)],
    # 10-30 품질 루프(GPT 잡기 5.0 "움켜잡는 순간이 없다"): 3/4 앞 두 팔 벌림 → 앞으로 모아 움켜쥠 → 갈비뼈로 당김(재생 프로파일 대체)
    "Grab": [("0@Pull", .045), ("GrabSE", "D_GrabSE", 0, -20, .12, False), ("GrabSE", "D_GrabSE", 1, -20, .05, False), ("GrabSE", "D_GrabSE", 1, -20, .08, False),
             ("GrabSE", "D_GrabSE", 2, -20, .22, False), ("13@Pull", .1)],
    # 10-30 품질 루프(GPT 놓기 5.5 "물체를 놓는 접지 순간이 없다"): 3/4 앞으로 깊게 웅크려 손을 앞발 옆 땅까지 → 회수
    "Place": ["0", "1", ("PlaceSE", "D_PlaceSE", 0, -20, .1, False), ("PlaceSE", "D_PlaceSE", 1, -20, .22, False), ("PlaceSE", "D_PlaceSE", 2, -20, .12, False),
              "10", "11", "12"],
    # 10-30 품질 루프(GPT 활 5.5 "시위를 턱까지 당겼다 놓는 차이가 없다"): 3/4 뒤 — 앞손 활을 뻗고 뒷손을 턱까지 당김 → 측면에서 놓기
    "Bow": ["0", "1", ("BowNE", "D_BowNE", 0, 0, .08, False), ("BowNE", "D_BowNE", 1, 0, .1, False), ("BowNE", "D_BowNE", 2, 0, .28, False),
            "9", "10", "11", "12", "13"],
    # 새 셀: 춤(N3)·악기(N2, 생성기가 류트를 그림 — 현악기 전용)
    "Dance": [("DanceS", "D_DanceS", i, 0, .15, False) for i in range(4)],
    "Lute": [("LuteSE", "D_LuteSE", i, 30, .12, False) for i in range(3)],
    # 새 셀: 납도(발도 끝 자세 → 3/4 앞으로 돌아서며 칼집에) · 승리 포즈(정면)
    "Sheathe": [("7@Iaido", .12), ("9@Iaido", .045), ("SheatheSE", "D_SheatheSE", 0, 90, .1, False), ("SheatheSE", "D_SheatheSE", 0, 115, .04, False),
                ("SheatheSE", "D_SheatheSE", 1, 140, .04, False), ("SheatheSE", "D_SheatheSE", 1, 160, .05, True), ("SheatheSE", "D_SheatheSE", 2, 172, .06, True),
                ("SheatheSE", "D_SheatheSE", 2, 178, .3, True)],  # 10-30 품질 루프: 각도를 단계로(수직 → 칼집)
    "Victory": [("VictoryS", "D_VictoryS", 0, 45, .06, False), ("VictoryS", "D_VictoryS", 1, 80, .06, False),
                ("VictoryS", "D_VictoryS", 2, 90, .6, False)],
}

LOOP = {"Dance", "Lute"}
# 앞손에 칼집을 쥐는 셀: 앞손 무기 각도(뒤·아래로 허리에 찬 칼집, 10-30 GPT "납도에 칼집이 안 보인다")
SCABBARD = {"Iaido": 200.0, "Sheathe": 200.0}
# 방향 프레임에서 무기를 앞손(LEFT ARM)에 쥐는 셀 — 활: 앞손이 활, 뒷손이 시위를 턱까지 당긴다
SWAP_HANDS = {"Bow"}


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
            if isinstance(it, tuple) and len(it) >= 6:
                folder, kname, i, ang, t, back = it[:6]
                out.setdefault("angleB", [-999.0] * len(out["frames"])).append(it[6] if len(it) > 6 else -999.0)
                out["frames"].append(f"m41/dirs/{folder}/{i}")
                main, off = ("LEFT ARM", "RIGHT ARM") if cel in SWAP_HANDS else ("RIGHT ARM", "LEFT ARM")
                out["hand"].append(joint(keys, kname, i, main))
                out["handB"].append(joint(keys, kname, i, off))
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
            if "angleB" in out:
                out["angleB"].append(-999.0)
        out["motion"] = f"splice_dirs({cel})"
        if cel in LOOP:
            out["loop"] = True
        if cel in SCABBARD:
            out["angleB"] = [SCABBARD[cel]] * len(out["frames"])
        reg[cel] = out
        print(cel, len(out["frames"]), round(sum(out["times"]), 2))
    json.dump(reg, open(REG, "w", encoding="utf-8"), ensure_ascii=False, indent=1)


if __name__ == "__main__":
    main()
