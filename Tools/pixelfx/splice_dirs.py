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
    "Throw": ["0", ("ThrowNE", "D_ThrowNE", 0, 110, .05, False), ("ThrowNE", "D_ThrowNE", 1, 120, .12, False),  # 15회차: 젖힘 0.12 → 릴리즈 0.04씩 → 팔로스루 0.14 + 투사체 바늘
              ("ThrowNE", "D_ThrowNE", 2, 90, .03, False), "3", "4", ("5", .03), ("6", .03), ("7", .04), ("8", .14), "9", "10", "11", "13"],
    "GreatUpper": ["0", "1", ("UpperNE", "D_UpperNE", 0, -178, .08, True), ("UpperNE", "D_UpperNE", 1, -174, .1, True),
                   ("UpperNE", "D_UpperNE", 2, -160, .04, True),
                   ("UpperE", "E_UpperE", 0, -75, .1, False), ("UpperE", "E_UpperE", 1, 35, .04, False), ("UpperE", "E_UpperE", 1, 70, .03, False),
                   ("UpperE", "E_UpperE", 2, 110, .14, False), ("UpperE", "E_UpperE", 2, 110, .1, False), ("11", .04), ("11", .04), ("11", .2), "13"],  # 17회차: 측면 아래(-75°) → 상승(35°) → 상단 멈춤(110°), 14장 유지(MotionVariants hold 12)
    # GPT 46동작 리뷰 "내미는 것처럼 보인다" → 던져 거는 앞부분은 빠르게, 정면으로 버텨 당기는 뒷부분을 길게
    "Pull": ["0", "1", ("2", .06), ("3", .04), "4", "5", ("PullS", "D_PullS", 0, 5, .12, False), ("PullS", "D_PullS", 1, 10, .06, False),
             ("PullS", "D_PullS", 2, 15, .18, False), "8", "9", "10", "11", "13"],  # 15회차: 걸림 0.12 → 당김 0.06 급가속 → 젖힘 0.14
    "Raise": ["0", "1", ("2", .1), ("3", .04), ("4", .04), ("RaiseS", "D_RaiseS", 0, 60, .04, False), ("RaiseS", "D_RaiseS", 1, 85, .04, False),
              ("RaiseS", "D_RaiseS", 2, 90, .3, False), ("12", .08), "13"],  # 15회차: 빠르게 올려 정점 버팀, 내리기 늦게
    "ShieldBash": ["0", "1", ("BashNE", "D_BashNE", 0, -100, .05, False), ("BashNE", "D_BashNE", 1, -100, .1, False),
                   ("BashNE", "D_BashNE", 2, -95, .04, False), ("4", .03), ("5", .05), "6", "7", "8", "9", "10", "11", "13"],  # 15회차: 접기 0.10 → 돌진 0.03 → 충돌 0.05
    "LowSweep": ["0", "1", ("SweepNE", "D_SweepNE", 0, -195, .05, True), ("SweepNE", "D_SweepNE", 1, -190, .15, True),
                 ("SweepNE", "D_SweepNE", 2, -160, .04, True), "3", "4", "5", "6", "7", "8", "9", "10", "12"],
    "Punch": ["0", "1", "2", "3", "4", "5", "6", "7", ("PunchNE", "D_PunchNE", 1, 0, .07, False), ("PunchSE", "D_PunchSE", 1, 0, .035, False),
              "10", "11", "12", "13"],  # 원투의 투: 3/4 뒤 장전 → 3/4 앞 통과 → 측면 스트레이트(GPT D4)
    # 검 든 채 사격: 3/4 앞으로 몸을 열고 권총(앞손)을 어깨 높이로, 검은 반대 허리 뒤(GPT 리뷰 "두 무기가 겹친다") — 사격 4·6번
    # 품질 10회차(GPT 6.0 "두 무기가 몸 앞에서 겹침"): 3/4 앞 겨눔 → 측면에서 권총손 어깨 높이 8px 앞·검손 뒤 허리(E_GunE) 사격·반동(angleB 18°)
    "SwordPistol": ["0", "1", "2", ("GunSE", "D_GunSE", 0, 200, .08, True), ("GunE", "E_GunE", 0, 200, .05, True, 0), ("GunE", "E_GunE", 1, 200, .06, True, 0),
                    ("GunE", "E_GunE", 2, 205, .05, True, 18), ("GunE", "E_GunE", 0, 200, .08, True, 4), ("GunE", "E_GunE", 1, 200, .06, True, 0),
                    ("GunE", "E_GunE", 2, 205, .12, True, 22), "12", "13"],  # 11회차: 사격 → 반동 → 재조준 → 두 번째 사격 → 큰 반동
    # 한손 베기 회전(GPT 2차 "골반·어깨 회전 부족"): 3/4 뒤 감기 → 측면 타격 → 3/4 앞 팔로스루 — 공용 프레임
    "DashSlash": ["0", "1", ("CoilNE", "D_CoilNE", 0, -170, .06, True), ("CoilNE", "D_CoilNE", 1, -165, .12, True), "3", "4", "5", "6",
                  ("FollowSE", "D_FollowSE", 0, -10, .06, True), ("FollowSE", "D_FollowSE", 1, -20, .1, True), "9", "10", "11", "13"],
    # 품질 8회차(GPT 한손 수평 6.3 "무겁지도 빠르지도 않음 — 양손 수평과 준비가 비슷"): 3/4 뒤 감기 빼고 측면 머리 옆 짧은 장전 → 내딛으며 수평 타격 → 3/4 앞 팔로스루
    "GreatSideOne": ["0", ("OneSide", "E_OneSide", 0, 170, .08, False), ("OneSide", "E_OneSide", 1, 20, .025, False), ("OneSide", "E_OneSide", 1, 0, .14, False),
                     ("FollowSE", "D_FollowSE", 1, -20, .05, True), ("OneSide", "E_OneSide", 2, -25, .08, False), "11", "12", "13"],  # 9회차: 빠른 스냅·수평 타격 유지
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
    # 품질 6회차(GPT "SE 비틀기 → E 측면 방출, 두 손 어깨보다 7~8px 앞, 뒷발로 밀기"): 3/4 앞 웅크림 → 측면 방출·유지·반동
    "PalmBlast": [("PalmSE2", "D_PalmSE2", 0, -20, .15, False), ("PalmSE2", "D_PalmSE2", 1, -20, .03, False), ("PalmSide", "E_PalmSide", 0, -20, .03, False),
                  ("PalmSide", "E_PalmSide", 1, 0, .12, False), ("PalmSide", "E_PalmSide", 2, -20, .1, False),  # 12회차: 압축 0.15 → 방출 순간 짧게 + 바늘 이펙트 → 반동 0.1
                  ("PalmRecoil", "D_PalmRecoil", 2, -20, .08, False), ("13@Pull", .1)],  # 5회차: 방출 뒤 상체 반동  # 10-30 품질 4회차: 타격 때 두 손 위아래로 크게 벌려 내밂
    "Slam": [("2@Pull", .06), ("4@Pull", .1), ("LiftS", "D_LiftS", 0, 90, .1, False), ("LiftS", "D_LiftS", 1, 90, .24, False),  # 4회차: 무릎 굽혀 버티며 머리 위로 높이
             ("3@GreatDown", .08), ("5@GreatDown", .04), ("6@GreatDown", .025), ("SlamSE", "D_SlamSE", 0, -60, .025, False), ("SlamSE2", "D_SlamSE2", 1, -75, .03, False), ("SlamSE2", "D_SlamSE2", 1, -75, .22, False),  # 6회차: 머리 위에서 더 버티다 급격히 숙임
             ("SlamSE2", "D_SlamSE2", 2, -65, .06, False), ("12@GreatDown", .06), ("13@GreatDown", .1)],  # 11회차: 골반 10px 깊게 내리꽂기(SlamSE2 0번 = 손이 얼굴을 가려 버림)  # 5회차: 3/4 앞 깊은 내리꽂기
    # 품질 6회차(GPT 앞차기 6.0 "무릎 배 앞까지, 발목 7px 앞, 즉시 회수"): 측면 준비 2장 → 무릎 장전 → 뻗음 → 회수 → 측면 복귀
    "Kick": ["0", "1", ("FrontKick2", "E_FrontKick2", 0, -20, .14, False), ("FrontKick2", "E_FrontKick2", 1, 0, .14, False),  # 7회차: 무릎 더 높이·골반 앞으로
             ("FrontKick2", "E_FrontKick2", 2, -20, .04, False), ("9", .05), "10", "11"],  # 14회차: 장전 0.14 버팀 → 뻗음 0.14 + 지지발 먼지·발끝 바늘
    # 품질 7회차(GPT 돌려차기 6.0 "앞차기처럼 읽힘"): v3(Roundhouse_v3 보관) 3/4 뒤·뒷모습·3/4 앞 타격 뒤에 측면 팔로스루(발이 골반 위로 지나감) → 흘려 내림
    "Roundhouse": ["0@Roundhouse_v3", "1@Roundhouse_v3", "2@Roundhouse_v3", "3@Roundhouse_v3", "4@Roundhouse_v3", ("5@Roundhouse_v3", .04), ("6@Roundhouse_v3", .06),
                   ("RoundFollow", "E_RoundFollow", 0, -20, .15, False), ("RoundFollow", "E_RoundFollow", 1, -20, .05, False), ("9@Roundhouse_v3", .05), "10@Roundhouse_v3", "11@Roundhouse_v3"],  # 8회차: 높은 팔로스루 유지
    # 품질 7회차(GPT 소총 6.0 "발사 반동·복귀 타이밍이 약해 총이 가벼워 보임"): 조준 0.2초 버팀 → 발사 짧게 → 반동 → 천천히 복귀(생성 0)
    "Rifle": ["0", "1", ("2", .1), "3", "4", ("5", .2), ("6", .04), ("8", .08), ("9", .06), ("10", .08), ("11", .12), "12", "13"],
    # 품질 7회차(GPT 쌍권총 6.4 "두 총이 같은 높이·같은 타이밍"): 측면 — 주손 높이 뻗어 쏨(반동) → 왼손 뻗어 쏨(반동, angleB) → 주손 한 번 더 → 둘 다 앞
    "DualPistol": ["0", "1", ("DualFire", "E_DualFire", 0, 0, .07, False, 0), ("DualFire", "E_DualFire", 0, 24, .05, False, 0),
                   ("DualFire", "E_DualFire", 1, -6, .07, False, 0), ("DualFire", "E_DualFire", 1, -6, .05, False, 22),
                   ("DualFire", "E_DualFire", 0, 0, .06, False, 0), ("DualFire", "E_DualFire", 0, 24, .05, False, 0),
                   ("DualFire", "E_DualFire", 2, 6, .12, False, 6), "12", "13"],
    # 품질 8회차(GPT 백스텝 6.4 "예비동작·이탈·착지 분리가 약해 미끄러져 보임"): 몸 낮춤 버팀 → 이탈 빠르게 → 공중 → 착지 웅크림 버팀(생성 0)
    "Backstep": ["0", "1", ("2", .12), ("3", .035), ("4", .035), ("5", .06), ("6", .06), ("7", .04), ("8", .13), ("9", .05), "10", "11", "12", "13"],
    # 품질 10회차(GPT 채찍 6.5 "끝이 앞으로 펴지는 스냅 순간이 약함"): 장전 0.12 → 전환·손목 스냅 0.04씩 → 최대 전개 0.12 → 회수(생성 0)
    "WhipSnap": ["0", "1", ("2", .12), ("3", .04), ("4", .04), ("5", .12), ("6", .06), "7", "8", "9", "10", "11", "12"],
    # 품질 15회차(GPT 도약 내려찍기 7.0 "착지 후 무게가 빨리 풀림"): 충돌 프레임 0.06 → 0.1(생성 0)
    "LeapSlam": ["0", "1", "2", "3", "4", "5", ("6", .1), "7", "8", "9", "10", "11"],
    # 10-30 품질 루프(GPT 잡기 5.0 "움켜잡는 순간이 없다"): 3/4 앞 두 팔 벌림 → 앞으로 모아 움켜쥠 → 갈비뼈로 당김(재생 프로파일 대체)
    "Grab": [("0@Pull", .045), ("GrabSE", "D_GrabSE", 0, -20, .1, False), ("GrabE", "E_GrabE", 0, -20, .04, False), ("GrabE", "E_GrabE", 1, -20, .12, False),
             ("GrabE", "E_GrabE", 2, -20, .2, False), ("13@Pull", .1)],  # 10회차: 측면 뻗음 → 움켜쥠 고정 → 골반 빼며 당김
    # 10-30 품질 루프(GPT 놓기 5.5 "물체를 놓는 접지 순간이 없다"): 3/4 앞으로 깊게 웅크려 손을 앞발 옆 땅까지 → 회수
    "Place": ["0", "1", ("PlaceSE2", "D_PlaceSE2", 0, -20, .1, False), ("PlaceSE2", "D_PlaceSE2", 1, -20, .2, False), ("PlaceSE2", "D_PlaceSE2", 2, -20, .12, False),  # 12회차: 손 땅에 붙임 → 떼어 가슴 앞(골반 낮게)
              "10", "11", "12"],
    # 10-30 품질 루프(GPT 활 5.5 "시위를 턱까지 당겼다 놓는 차이가 없다"): 3/4 뒤 — 앞손 활을 뻗고 뒷손을 턱까지 당김 → 측면에서 놓기
    "Bow": ["0", "1", ("BowNE", "D_BowNE", 0, 0, .08, False), ("BowNE", "D_BowNE", 1, 0, .1, False), ("BowNE", "D_BowNE", 2, 0, .12, False),
            ("BowE", "E_BowE", 0, 0, .16, False), ("BowE", "E_BowE", 1, 0, .05, False), ("BowE", "E_BowE", 2, -10, .1, False), "12", "13"],  # 10회차: 측면 만작 → 놓으며 시위손 뒤로 튕김
    # 10-30 품질 루프(GPT 양손 수평 6.0 "올려베기처럼 보인다, 한손 수평과 비슷"): 타격 직후 3/4 앞 양손 팔로스루(회전 베기 프레임)로 수평 통과를 보이게
    "GreatSide": ["0", "1", "2", "3", "4", ("v2SpinSE", "D_SpinSE", 1, -8, .05, False), ("v2SpinSE", "D_SpinSE", 2, -15, .1, False), "10", "11", "12", "13"],
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
# 프레임별 주무기 보이는 길이(납도: 칼이 칼집에 들어가며 짧아짐, GPT "칼 길이 변화가 안 보인다")
WEAPON_LEN = {"Sheathe": [1, 1, 1, 1, 1, 0.75, 0.4, 0.08]}


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
        if cel in WEAPON_LEN:
            out["weaponLen"] = WEAPON_LEN[cel]
        if cel in SCABBARD:
            out["angleB"] = [SCABBARD[cel]] * len(out["frames"])
        reg[cel] = out
        print(cel, len(out["frames"]), round(sum(out["times"]), 2))
    json.dump(reg, open(REG, "w", encoding="utf-8"), ensure_ascii=False, indent=1)


if __name__ == "__main__":
    main()
