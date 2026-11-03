"""1층계 최단 시간 모델 (Floor1Completion §9, 큐 L9).
T = 퀘스트·이동·보스(비사냥) + 반복 사냥(그라인드) EXP / (구역 분당 EXP x k)
입력: Assets/Resources/QuestsFloor1.csv, Logs/playtest/hunt_estimates.csv (Tools/probes/f1b_audit.cs 출력)
사용: python Tools/floor1_fastest.py   (모든 시간은 모델 추정, 실플레이 미검증)
성채(Q5xx)·밤 전용 Q104·Lv15 이후 Q7xx 는 최단 경로에서 제외한다.
"""
import csv
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
EXP = [500, 650, 800, 1200, 1450, 2900, 3500, 4200, 5000, 6000, 11500, 14000, 17000, 20500, 24500]
MAPS = ["Lv1_NearForest_SideRoad", "Lv1_NormalForest", "Lv1_DeepForest", "Lv1_Swamp", "Lv1_BanditFortress", "Lv1_TowerOutskirts"]
BOSS = {"AncientEnt_BD": 6, "GiantSlime_BD": 6, "AlphaWolf_BD": 5, "OldBear_BD": 5, "ElderGolem_BD": 6}
TRAVEL = 12  # 구역 이동 6회 x 2분
GATE = 7     # 문지기 입장·전투


def load_quests():
    return list(csv.DictReader(open(ROOT / "Assets/Resources/QuestsFloor1.csv", encoding="utf-8-sig")))


def load_rates():
    rows = list(csv.DictReader(open(ROOT / "Logs/playtest/hunt_estimates.csv")))
    rate = {}
    for m in MAPS:
        v = [float(r["expMinAfter"]) for r in rows if r["map"] == m]
        rate[m] = sum(v) / len(v)  # 낮/밤 평균
    return rate


def zone(lv):
    # 성채 제외 경로: Lv9~12 늪, Lv13+ 탑 근교
    return MAPS[5] if lv >= 13 else MAPS[3] if lv >= 9 else MAPS[2] if lv >= 6 else MAPS[1] if lv >= 3 else MAPS[0]


def qtime(q, k):
    o, c, t = q["objective"], int(q["count"]), q["target"]
    if o == "talk":
        return 1.5
    if o == "visit":
        return 2.5
    if o == "kill":
        return BOSS.get(t, 1.5 + c / 8 / k)
    if o == "collect":
        return 2.5 + c * 0.5 / k
    if o == "craft":
        return 5
    if o == "equip":
        return 3
    if o == "killElite":
        return 5
    if o == "enhance":
        return 2.5
    if o == "bestiary":
        return 5
    return 3


def skip(q):
    return q["id"].startswith(("Q5", "Q7")) or q["id"] == "Q104"


def run(quests, rate, k, target=15):
    lv, exp, grind, qt = 1, 0, 0.0, 0.0
    done = set()
    per_lv = {}

    def add(g):
        nonlocal lv, exp
        t = exp + g
        while lv < 15 and t >= EXP[lv - 1]:
            t -= EXP[lv - 1]
            lv += 1
        exp = t if lv < 15 else 0

    while lv < target:
        got = False
        for q in quests:
            if (q["id"] in done or skip(q) or int(q["minLevel"]) >= target or int(q["minLevel"]) > lv
                    or (q["requires"] and q["requires"] not in done)):
                continue
            done.add(q["id"])
            qt += qtime(q, k)
            add(int(q["exp"]))
            got = True
            if lv >= target:
                break
        if got or lv >= target:
            continue
        need = EXP[lv - 1] - exp
        m = need / (rate[zone(lv)] * k)
        grind += m
        per_lv[lv] = (round(need), round(m, 1))
        lv += 1
        exp = 0
    return qt + TRAVEL + GATE, grind, per_lv


if __name__ == "__main__":
    qs, rt = load_quests(), load_rates()
    for k in (1.2, 1.5, 2.0):
        non, g, per = run(qs, rt, k)
        print(f"k={k}: 비사냥 {non:.0f}분 + 그라인드 {g:.0f}분 = {non + g:.0f}분, 그라인드 {g / (non + g) * 100:.1f}%  구간={per}")
