#!/usr/bin/env python
"""PlayRecorder jsonl -> markdown 요약 (표준 라이브러리만).

사용: python Tools/playrec_summary.py Logs/playrec/20261115_112515.jsonl [출력.md]
기본 출력: Logs/playrec/<이름>_summary.md (입력과 같은 폴더). 봇 설계 입력 수치를 담는다(약 100줄 이내).
"""
import json
import os
import sys
from collections import Counter, defaultdict

REACT_RANGE = 6.0        # 적이 가로(|dx|) 이 안에 들어온 순간부터
REACT_WINDOW = 5.0       # 이 시간 안에 공격 입력이 없으면 반응 없음으로 본다
ATTACK_ACTIONS = ("Player/Attack",)
SKILL_PREFIX = "Player/Skill"
MAX_DT = 0.5             # 스냅샷 간격이 이보다 크면(일시정지 등) 체류 시간에 넣지 않는다


def load(path):
    rows = []
    bad = 0
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                rows.append(json.loads(line))
            except ValueError:
                bad += 1  # 비정상 종료로 잘린 마지막 줄 등
    return rows, bad


def pct(values, p):
    if not values:
        return None
    s = sorted(values)
    k = (len(s) - 1) * p / 100.0
    lo = int(k)
    hi = min(lo + 1, len(s) - 1)
    return s[lo] + (s[hi] - s[lo]) * (k - lo)


def dist(values):
    if not values:
        return "n=0"
    return "n=%d 중앙값 %.2fs p10 %.2fs p90 %.2fs" % (
        len(values), pct(values, 50), pct(values, 10), pct(values, 90))


def fmt_time(sec):
    return "%d분 %02d초" % (int(sec) // 60, int(sec) % 60)


def input_stats(inputs):
    """액션별 (S 횟수, 평균 누름 시간). S -> C 를 이름 단위로 짝짓는다."""
    count = Counter()
    holds = defaultdict(list)
    open_at = {}
    dev = defaultdict(Counter)
    for r in inputs:
        a = r.get("a", "?")
        ph = r.get("ph")
        dev[a][r.get("dev", "?")] += 1
        if ph == "S":
            count[a] += 1
            open_at[a] = r["t"]
        elif ph == "C" and a in open_at:
            holds[a].append(r["t"] - open_at.pop(a))
    return count, holds, dev


def press_times(inputs, names=None, prefix=None):
    out = []
    for r in inputs:
        if r.get("ph") != "S":
            continue
        a = r.get("a", "")
        if (names and a in names) or (prefix and a.startswith(prefix)):
            out.append(r["t"])
    return sorted(out)


def intervals(times, cap=10.0):
    return [b - a for a, b in zip(times, times[1:]) if b - a <= cap]


def reaction_times(states, action_times):
    """적이 가로 REACT_RANGE 안에 새로 들어온 순간 -> 다음 공격(평타·스킬) 입력까지."""
    out = []
    missed = 0
    prev_near = False
    j = 0
    for s in states:
        near = any(abs(e.get("dx", 99)) <= REACT_RANGE for e in s.get("en", []))
        if near and not prev_near:
            while j < len(action_times) and action_times[j] < s["t"]:
                j += 1
            if j < len(action_times) and action_times[j] - s["t"] <= REACT_WINDOW:
                out.append(action_times[j] - s["t"])
            else:
                missed += 1
        prev_near = near
    return out, missed


def dwell(states, key_fn):
    res = defaultdict(float)
    for a, b in zip(states, states[1:]):
        dt = b["t"] - a["t"]
        if 0 < dt <= MAX_DT:
            for k in key_fn(a):
                res[k] += dt
    return res


def ui_opens(states):
    opens = Counter()
    prev = set()
    for s in states:
        cur = set(s.get("ui", []))
        for w in cur - prev:
            opens[w] += 1
        prev = cur
    return opens


def direction_changes(inputs):
    last = 0
    n = 0
    for r in inputs:
        if r.get("a") != "Player/Move" or r.get("ph") == "C":
            continue
        v = r.get("v", 0)
        sign = 1 if v > 0.3 else -1 if v < -0.3 else 0
        if sign != 0:
            if last != 0 and sign != last:
                n += 1
            last = sign
    return n


def group_warns(warns):
    groups = {}
    for w in warns:
        key = (w.get("ty", "?"), (w.get("msg") or "")[:80] if w.get("ty") != "stuck" else "")
        g = groups.setdefault(key, {"n": 0, "first": w})
        g["n"] = max(g["n"] + 1, w.get("n", 0))
    return groups


def build(rows, bad, src_name):
    L = []
    ins = [r for r in rows if r.get("k") == "in"]
    sts = [r for r in rows if r.get("k") == "st"]
    evs = [r for r in rows if r.get("k") == "ev"]
    logs = [r for r in rows if r.get("k") == "log"]
    warns = [r for r in rows if r.get("k") == "warn"]
    total = (rows[-1]["t"] - rows[0]["t"]) if len(rows) > 1 else 0.0
    minutes = max(total / 60.0, 1e-6)
    srcs = Counter(r.get("src", "?") for r in rows)

    L.append("# 플레이 기록 요약: %s" % src_name)
    L.append("")
    L.append("- 총 시간 %s (게임 시간) · 줄 %d (in %d / st %d / ev %d / log %d / warn %d)%s" % (
        fmt_time(total), len(rows), len(ins), len(sts), len(evs), len(logs), len(warns),
        " · 깨진 줄 %d" % bad if bad else ""))
    L.append("- 출처 %s" % ", ".join("%s %d" % kv for kv in srcs.items()))
    if len(sts) > 1:
        span = sts[-1]["t"] - sts[0]["t"]
        L.append("- 스냅샷 %.1fHz" % (len(sts) / span if span > 0 else 0))

    # 맵 체류
    md = dwell(sts, lambda s: [s.get("map") or "(미상)"])
    L += ["", "## 맵별 체류"]
    for k, v in sorted(md.items(), key=lambda kv: -kv[1]):
        L.append("- %s: %s (%.0f%%)" % (k, fmt_time(v), 100.0 * v / max(sum(md.values()), 1e-6)))
    if not md:
        L.append("- (스냅샷 없음)")

    # 입력
    count, holds, dev = input_stats(ins)
    L += ["", "## 입력 (액션: 횟수 / 평균 누름 / 장치)"]
    shown = 0
    for a, n in count.most_common():
        if a.startswith("Debug Menu"):
            continue
        h = holds.get(a)
        L.append("- %s: %d회 / %s / %s" % (
            a, n, "%.2fs" % (sum(h) / len(h)) if h else "-",
            ",".join("%s%d" % kv for kv in dev[a].items())))
        shown += 1
        if shown >= 24:
            L.append("- ... (상위 24개만)")
            break
    if not count:
        L.append("- (입력 없음)")

    # 공격 간격·반응·점프·방향 전환
    atk = press_times(ins, names=ATTACK_ACTIONS)
    skl = press_times(ins, prefix=SKILL_PREFIX)
    L += ["", "## 전투 리듬"]
    L.append("- 평타 간격: %s" % dist(intervals(atk)))
    L.append("- 스킬 사용 %d회 (%.1f/분)" % (len(skl), len(skl) / minutes))
    rt, missed = reaction_times(sts, sorted(atk + skl))
    L.append("- 반응 시간(적 |dx|<=%.0f 진입 -> 첫 공격): %s · 무반응 %d" % (REACT_RANGE, dist(rt), missed))
    jumps = press_times(ins, names=("Player/Jump",))
    L.append("- 점프 %d회 (%.1f/분)" % (len(jumps), len(jumps) / minutes))
    dash = press_times(ins, names=("Player/Dash",))
    L.append("- 대시 %d회 (%.1f/분)" % (len(dash), len(dash) / minutes))
    dc = direction_changes(ins)
    L.append("- 이동 방향 전환 %d회 (%.1f/분)" % (dc, dc / minutes))

    # 이벤트
    evc = Counter(e.get("e") for e in evs)
    kills = evc.get("kill", 0)
    dealt = [e.get("v", 0) for e in evs if e.get("e") == "dmg"]
    hurt = [e.get("v", 0) for e in evs if e.get("e") == "hurt"]
    L += ["", "## 전투 결과"]
    L.append("- 처치 %d (%.1f/분) · 준 피해 %d회 합 %d · 받은 피해 %d회 합 %d · 사망 %d · 부활 %d" % (
        kills, kills / minutes, len(dealt), sum(dealt), len(hurt), sum(hurt),
        evc.get("death", 0), evc.get("revive", 0)))
    if sts:
        L.append("- 레벨 %d -> %d · 골드 %d -> %d" % (
            sts[0].get("lv", 0), sts[-1].get("lv", 0), sts[0].get("gold", 0), sts[-1].get("gold", 0)))
    other = ["%s %d" % (k, v) for k, v in evc.most_common() if k not in ("kill", "dmg", "hurt", "death", "revive")]
    if other:
        L.append("- 기타 이벤트: " + ", ".join(other[:16]))

    # UI
    uo = ui_opens(sts)
    ud = dwell(sts, lambda s: s.get("ui", []))
    L += ["", "## UI 창 (열기 횟수 / 체류)"]
    for w, n in uo.most_common(12):
        L.append("- %s: %d회 / %s" % (w, n, fmt_time(ud.get(w, 0.0))))
    if not uo:
        L.append("- (없음)")

    # 막힘·경고·예외
    groups = group_warns(warns)
    L += ["", "## 막힘 · 경고 · 예외 (중복 묶음)"]
    stuck = [w for w in warns if w.get("ty") == "stuck"]
    if stuck:
        spots = Counter("%s (%.0f,%.0f)" % (w.get("map", ""), w.get("x", 0), w.get("y", 0)) for w in stuck)
        L.append("- 막힘 %d건: %s" % (len(stuck), "; ".join("%s x%d" % kv for kv in spots.most_common(5))))
    n_lines = 0
    for (ty, _), g in sorted(groups.items(), key=lambda kv: -kv[1]["n"]):
        if ty == "stuck":
            continue
        w = g["first"]
        L.append("- [%s] x%d t=%.0f %s" % (ty, g["n"], w["t"], (w.get("msg") or "").replace("\n", " ")[:110]))
        n_lines += 1
        if n_lines >= 15:
            L.append("- ... (상위 15종만)")
            break
    if not warns:
        L.append("- (없음)")
    return "\n".join(L) + "\n"


def main(argv):
    if len(argv) < 2:
        print("usage: playrec_summary.py <file.jsonl> [out.md]")
        return 2
    path = argv[1]
    rows, bad = load(path)
    if not rows:
        print("empty log")
        return 1
    name = os.path.splitext(os.path.basename(path))[0]
    out = argv[2] if len(argv) > 2 else os.path.join(os.path.dirname(os.path.abspath(path)), name + "_summary.md")
    text = build(rows, bad, name)
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    print(out)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
