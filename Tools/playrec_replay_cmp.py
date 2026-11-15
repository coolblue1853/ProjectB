#!/usr/bin/env python
"""B0 입력 재생 검증: 원본 기록 구간의 in 행과 재생 중 새로 기록된 in 행의 순서·간격을 비교한다(표준 라이브러리만).

사용: python Tools/playrec_replay_cmp.py <원본.jsonl> <시작초> <길이초> <재생.jsonl>
- 비교 대상 액션: 이동·점프·공격·대시·스킬·UI 열기(인벤)·포털.
- 순서: (액션, 단계) 열의 최장 공통 부분열(LCS) 일치율.
- 시각은 실제 시간(일시정지 중 게임 시간이 멈추므로). 간격: 짝지은 사건끼리 앞 사건과의 간격 차 |dt_원본 - dt_재생| 분포.
"""
import json
import sys

ACTIONS = ("Player/Move", "Player/Jump", "Player/Attack", "Player/Dash", "Player/Run", "Player/SkillA", "Player/SkillS",
           "Player/SkillD", "Player/SkillF", "UI/OpenInventory", "Portal/PortalOn", "UI/Select")


def events(path, t0=None, t1=None):
    """비교 대상 in 행 [(실제 시각, 액션, 단계)]. 실제 시각 = 앞 tk 행의 u + (t - t_tk)(0~0.05 로 자름) — BotReplay 와 같은 정의."""
    out, tk_t, tk_u = [], 0.0, 0.0
    with open(path, encoding="utf-8") as f:
        for line in f:
            if '"k":"tk"' in line:
                r = json.loads(line)
                tk_t, tk_u = r["t"], r["u"]
                continue
            if '"k":"in"' not in line:
                continue
            r = json.loads(line)
            if t1 is not None and r["t"] >= t1:
                break
            if t0 is not None and r["t"] < t0:
                continue
            if r.get("a") in ACTIONS and r.get("ph") in ("S", "C"):
                out.append((tk_u + min(max(r["t"] - tk_t, 0.0), 0.05), r["a"], r["ph"]))
    if out:
        base = out[0][0]
        out = [(t - base, a, ph) for t, a, ph in out]
    return out


def lcs_pairs(a, b):
    """두 토큰 열의 LCS 짝 목록 [(i, j)]."""
    n, m = len(a), len(b)
    dp = [[0] * (m + 1) for _ in range(n + 1)]
    for i in range(n - 1, -1, -1):
        for j in range(m - 1, -1, -1):
            dp[i][j] = dp[i + 1][j + 1] + 1 if a[i] == b[j] else max(dp[i + 1][j], dp[i][j + 1])
    pairs, i, j = [], 0, 0
    while i < n and j < m:
        if a[i] == b[j]:
            pairs.append((i, j))
            i, j = i + 1, j + 1
        elif dp[i + 1][j] >= dp[i][j + 1]:
            i += 1
        else:
            j += 1
    return pairs


def compare(orig, rep):
    ta = [(a, ph) for _, a, ph in orig]
    tb = [(a, ph) for _, a, ph in rep]
    pairs = lcs_pairs(ta, tb)
    errs = []
    for (i0, j0), (i1, j1) in zip(pairs, pairs[1:]):
        errs.append(abs((orig[i1][0] - orig[i0][0]) - (rep[j1][0] - rep[j0][0])))
    abs_err = [abs(orig[i][0] - rep[j][0]) for i, j in pairs]
    return pairs, sorted(errs), sorted(abs_err)


def q(v, p):
    return v[min(len(v) - 1, int(len(v) * p))] if v else None


def main(argv):
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    if len(argv) < 5:
        print(__doc__)
        return 2
    t0, dur = float(argv[2]), float(argv[3])
    orig = events(argv[1], t0, t0 + dur)
    rep = events(argv[4])
    rep = [e for e in rep if e[0] <= (orig[-1][0] + 5.0 if orig else 0)]
    pairs, errs, abs_err = compare(orig, rep)
    print("원본 %d · 재생 %d · 순서 일치(LCS) %d (원본 대비 %.1f%%)" % (
        len(orig), len(rep), len(pairs), 100.0 * len(pairs) / max(len(orig), 1)))
    print("간격 오차 |dt| 중앙 %.3f · p90 %.3f · 최대 %.3f 초 (n=%d)" % (q(errs, .5) or 0, q(errs, .9) or 0, errs[-1] if errs else 0, len(errs)))
    print("누적 시각 오차 중앙 %.3f · p90 %.3f · 끝 %.3f 초" % (q(abs_err, .5) or 0, q(abs_err, .9) or 0, abs_err[-1] if abs_err else 0))
    by = {}
    for _, a, ph in orig:
        by.setdefault(a, [0, 0])[0] += 1
    for _, a, ph in rep:
        by.setdefault(a, [0, 0])[1] += 1
    print("액션별 원본/재생: " + ", ".join("%s %d/%d" % (k.split("/")[-1], v[0], v[1]) for k, v in sorted(by.items())))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
