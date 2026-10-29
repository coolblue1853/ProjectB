"""동작 합치기 판정(10-29 사용자: 정말 필요한 동작만·겹치지 않게 → 그 구성으로 직업이 문제없는지).
MERGE = {없앨 동작: 대신 쓸 동작}. 직업 요구 동작을 바꿔 쓴 뒤 coverage 와 같은 엄격 판정을 다시 돌린다.
사용: python Tools/motioncov/merge_check.py
"""
import io
import os
import sys

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from coverage import jobs  # noqa: E402
from data import CANDIDATES, MOTIONS  # noqa: E402

# 근거: Tools/motioncov/similarity.py 측정(가장 비슷한 동작 거리 0.19~0.22 무리) + 몸 동작이 같고 무기·이펙트만 다른 것
MERGE = {
    "R9": "A4",    # 메치기 = 머리 위로 들어 내리꽂기 — 양손 내려찍기와 몸이 같다(거리 0.199·0.224). 잡는 손은 이펙트·대상 연출로
    "A16": "A8",   # 도약 내려찍기 = 점프(이동) + 공중 베기 + 착지 — 공중 베기와 점프로 조합
    "A6": "A3",    # 연속 찌르기 = 찌르기를 빠르게 반복 재생(메이플 stabOF 도 찌르기 셀 재조합)
    "R3": "R10",   # 잡기 = 앞으로 뻗어 끌어당기기 — 당기기와 같은 몸(거리 0.198~0.236)
    "A15": "A11",  # 모으기 = 시전의 준비 구간을 반복 재생(시전 셀 앞부분), 차이는 이펙트
}


def main():
    have = set(MOTIONS) - set(MERGE)
    js = jobs()
    before = sum(1 for j in js if j[3] <= set(MOTIONS) and not j[4])
    after = 0
    lost = []
    for cat, name, ws, need, tech in js:
        need2 = {MERGE.get(m, m) for m in need}
        ok_before = need <= set(MOTIONS)
        ok_after = need2 <= have
        after += ok_after and not tech
        if ok_before and not ok_after:
            lost.append(name)
    allneed = set().union(*[{MERGE.get(m, m) for m in j[3]} for j in js])
    print(f"동작 {len(MOTIONS)} → {len(have)} (합침 {len(MERGE)}), 후보 포함 전체 필요 {len(allneed)}")
    print(f"완전 대응(기술 제외): 합치기 전 {before} / 후 {after} / {len(js)}")
    print("합쳐서 잃는 직업:", lost or "없음")
    print("남는 동작:", " ".join(sorted(have, key=lambda x: (x[0], int(''.join(c for c in x[1:] if c.isdigit()) or 0), x))))
    print("후보:", " ".join(sorted(set(CANDIDATES) & allneed)))


if __name__ == "__main__":
    main()
