"""무기·직업 × 마스터 모션 엄격 판정 → .claude-rules/Game/MotionCoverage.md
사용: python Tools/motioncov/coverage.py
"""
import collections
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from data import CANDIDATES, MOTIONS, TECH, J, W  # noqa: E402

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", ".claude-rules", "Game", "MotionCoverage.md")


def jobs():
    out = []
    for line in J.splitlines():
        if not line.strip():
            continue
        cat, name, ws, extra = line.split("|")
        ws = [w for w in ws.split(",") if w]
        need, tech = set(), set()
        for w in ws:
            _, m, t = W[w]
            need |= set(m.split())
            tech |= set(t.split())
        for e in extra.split():
            (tech if e.startswith("T") else need).add(e)
        tech.discard("T3")  # 투사체 그리기는 이펙트 쪽(모든 원거리 공통) — 판정에서 뺀다
        out.append((cat, name, ws, need, tech))
    return out


def main():
    js = jobs()
    have = set(MOTIONS)
    rows, miss_m, miss_t = [], collections.Counter(), collections.Counter()
    ok = 0
    for cat, name, ws, need, tech in js:
        lack = sorted(need - have)
        for m in lack:
            miss_m[m] += 1
        for t in tech:
            miss_t[t] += 1
        st = "✔" if not lack and not tech else ("기술" if not lack else "✖")
        ok += st == "✔"
        rows.append((cat, name, "·".join(ws), " ".join(sorted(need, key=lambda x: (x[0], len(x), x))), " ".join(lack), " ".join(sorted(tech)), st))
    n = len(js)
    # 사용 빈도(동작마다 그 동작이 필요한 직업 수), 단독 의존(이 동작만 빠지면 무너지는 직업 수)
    freq = collections.Counter(m for *_, need, _ in [(j[0], j[1], j[2], j[3], j[4]) for j in js] for m in need)
    # 최소 모션: 기술 문제는 해결된다고 보고, 동작만으로 직업을 완전히 덮는 탐욕 선택
    allm = sorted(set(MOTIONS) | set(CANDIDATES))
    chosen, curve = [], []
    left = [j[3] for j in js]
    while True:
        cov = lambda s: sum(1 for need in left if need <= s)  # noqa: E731
        base = cov(set(chosen))
        best, gain = None, -1
        for m in allm:
            if m in chosen:
                continue
            g = cov(set(chosen) | {m}) - base
            # 동점이면 많이 쓰이는 동작 먼저(다음 단계에서 덮는 직업이 늘어난다)
            if g > gain or (g == gain and best is not None and freq[m] > freq[best]):
                best, gain = m, g
        if best is None:
            break
        chosen.append(best)
        curve.append((len(chosen), best, cov(set(chosen))))
        if cov(set(chosen)) == n:
            break
    cats = collections.Counter(j[0] for j in js)
    wcats = collections.Counter(v[0] for v in W.values())
    L = []
    L.append("# 무기·직업 × 마스터 모션 엄격 재판정 (10-29 사용자 재조사)\n")
    L.append("생성: `python Tools/motioncov/coverage.py`(데이터 `Tools/motioncov/data.py`). 이전 표(`JobCoverage.md`)는 \"비슷한 팔 동작이 있으면 ✔\" 라 너그러웠다 → 이번엔 **엄격 기준**.\n")
    L.append("- 무기마다 기본 동작(평타·대표기 2~4개)을 정하고, 직업 = 무기 조합 + 고유 행동. 필요한 동작이 **전부** 마스터 모션에 있어야 ✔.")
    L.append("- 동작은 있는데 무기 표현 기술(줄·사슬 그리기 T1, 변신형 T2)이 필요하면 **기술**, 동작이 빠지면 **✖**. 투사체 그리기(T3)는 모든 원거리 공통 이펙트라 판정에서 뺀다.")
    L.append("- 이동(대기·걷기·달리기·대시·점프·착지·로프·피격·사망)은 전 직업 공통이라 빼고 공격·행동 동작만 본다.\n")
    L.append(f"## 요약\n\n- 무기 **{len(W)}종**({', '.join(f'{k} {v}' for k, v in wcats.most_common())})")
    L.append(f"- 직업 **{n}종**({', '.join(f'{k} {v}' for k, v in cats.most_common())})")
    L.append(f"- 현재 마스터 모션 {len(MOTIONS)}개(공격·행동)로: ✔ **{ok}** / 기술만 필요 **{sum(1 for r in rows if r[6] == '기술')}** / ✖ **{sum(1 for r in rows if r[6] == '✖')}**")
    L.append("- 빠진 동작(필요 직업 수): " + ", ".join(f"{m} {CANDIDATES.get(m, m)} **{c}**" for m, c in miss_m.most_common()))
    L.append("- 필요한 무기 표현 기술(직업 수): " + ", ".join(f"{t} {c}" for t, c in miss_t.most_common()) + "\n")
    for k, v in TECH.items():
        L.append(f"  - {k}: {v}")
    # 지금에서 무엇을 더하면 얼마나 늘어나나(효과 큰 것부터)
    L.append("\n## 지금 30개에서 더하면\n")
    L.append("| 단계 | 더하는 것 | 완전 대응 직업 | 비율 |\n|---|---|---|---|")

    def full(ms, ts):
        return sum(1 for j in js if j[3] <= ms and j[4] <= ts)

    ms, ts = set(MOTIONS), set()
    L.append(f"| 0 | 현재 | {full(ms, ts)} | {full(ms, ts) * 100 // n}% |")
    pool = list(CANDIDATES) + list(k for k in TECH if k != "T3")
    step = 0
    while pool:
        best = max(pool, key=lambda x: full(ms | ({x} if x[0] != "T" else set()), ts | ({x} if x[0] == "T" else set())))
        pool.remove(best)
        (ts if best[0] == "T" else ms).add(best)
        step += 1
        c = full(ms, ts)
        L.append(f"| {step} | {best} {CANDIDATES.get(best) or TECH.get(best)} | {c} | {c * 100 // n}% |")
    L.append("\n## 판정 밖 — 이동 쪽에서 따로 볼 것(10-29 2차)\n")
    L.append("- 이 표는 공격·행동만 본다. 직업 스킬 중 **이동형**은 이동 모션 쪽 문제다: 회피 구르기(지금은 대시만), 활공·비행(날개·제트팩), 벽 타기·벽 차기, 그래플 스윙(갈고리로 매달려 이동), 수영, 엎드려 기기.")
    L.append("- 이 중 회피 구르기는 거의 모든 액션 직업이 쓰는 공용이라 이동 기본에 넣을지 기획 결정이 필요하다. 나머지는 해당 직업·지형이 생길 때 변신형·전용으로.")
    L.append("\n## 최소 모션 곡선(기술은 해결된다고 보고 동작만, 탐욕 선택)\n")
    L.append("| 동작 수 | 추가한 동작 | 완전히 덮는 직업 | 비율 |\n|---|---|---|---|")
    for k, m, c in curve:
        L.append(f"| {k} | {m} {MOTIONS.get(m) or CANDIDATES.get(m)} | {c} | {c * 100 // n}% |")
    L.append("\n## 동작별 사용 직업 수\n")
    L.append("| 동작 | 이름 | 필요 직업 수 |\n|---|---|---|")
    for m, c in freq.most_common():
        L.append(f"| {m} | {MOTIONS.get(m) or CANDIDATES.get(m) + ' **(없음)**'} | {c} |")
    unused = [m for m in MOTIONS if freq[m] == 0]
    if unused:
        L.append(f"\n쓰는 직업이 없는 동작: {', '.join(unused)}")
    L.append("\n## 무기별 기본 동작\n")
    L.append("| 분류 | 무기 | 기본 동작 | 기술 |\n|---|---|---|---|")
    for k, (c, m, t) in sorted(W.items(), key=lambda x: x[1][0]):
        L.append(f"| {c} | {k} | {m} | {t} |")
    L.append("\n## 직업별 판정\n")
    L.append("| 분류 | 직업 | 무기 | 필요 동작 | 빠진 동작 | 필요 기술 | 판정 |\n|---|---|---|---|---|---|---|")
    for r in rows:
        L.append("| " + " | ".join(r) + " |")
    open(OUT, "w", encoding="utf-8", newline="\n").write("\n".join(L) + "\n")
    print(n, "jobs", len(W), "weapons ok", ok, miss_m.most_common(), miss_t.most_common())
    print(curve[:12], len(curve))


if __name__ == "__main__":
    main()
