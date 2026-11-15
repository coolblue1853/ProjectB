#!/usr/bin/env python
"""사람 플레이 기록(jsonl) -> 봇 버릇 모델 human_profile.json (표준 라이브러리만).

사용: python Tools/playrec_fit.py <기록.jsonl>... [-o Assets/Resources/Bot/human_profile.json]
- 여러 기록을 합쳐 분포(p10/p25/p50/p75/p90 + n)와 비율(값 + n)을 낸다.
- 표본이 MIN_N 보다 적은 칸은 값을 null 로 두고 n 만 남긴다(추측 기본값 금지).
- 스키마·정의는 .claude-rules/Game/PlayBotDesign.md 9절. 원본 jsonl 은 읽기만 한다.
"""
import bisect
import json
import os
import sys
from collections import Counter

VERSION = 1
MIN_N = 5                 # 이보다 적으면 값 null
PCTS = (10, 25, 50, 75, 90)
REACT_RANGE = 6.0         # playrec_summary 와 같은 정의(적 |dx| 진입 -> 첫 공격)
REACT_WINDOW = 5.0
BURST_GAP = 0.35          # 평타 간격 분포가 0.1~0.3 / 0.4+ 로 갈린다(11-15 두 기록)
INTERVAL_CAP = 10.0
NEAR_ENEMY = 2.5          # 점프 직전 "적이 가까움"
EDGE_NEAR = 1.0           # 점프 직전 "발판 끝"
HUNT_GAP = 15.0           # 전투 이벤트가 이 시간 넘게 끊기면 사냥 세그먼트 종료
FOCUS_CAP = 5.0
TOWN = "Town"
KEEP_KINDS = ("in", "st", "tk", "ev", "av", "hs", "fo", "cx")


# ───────────── 순수 함수(시험 대상) ─────────────

def pct(values, p):
    """선형 보간 백분위(values 는 정렬돼 있어야 한다)."""
    if not values:
        return None
    k = (len(values) - 1) * p / 100.0
    lo = int(k)
    hi = min(lo + 1, len(values) - 1)
    return values[lo] + (values[hi] - values[lo]) * (k - lo)


def dist(values, unit="s", digits=3):
    """분포 칸. n < MIN_N 이면 백분위는 null."""
    vals = sorted(float(v) for v in values)
    out = {"n": len(vals), "unit": unit}
    for p in PCTS:
        v = pct(vals, p) if len(vals) >= MIN_N else None
        out["p%d" % p] = round(v, digits) if v is not None else None
    return out


def ratio(hit, total):
    """비율 칸. total < MIN_N 이면 값 null."""
    return {"n": total, "k": hit, "value": round(hit / float(total), 4) if total >= MIN_N else None}


def rate(key, count, minutes):
    """분당 빈도 칸. 사건 수가 MIN_N 보다 적거나 시간이 1분 미만이면 값 null."""
    ok = count >= MIN_N and minutes >= 1.0
    return {"key": key, "n": count, "minutes": round(minutes, 2), "value": round(count / minutes, 3) if ok else None}


def press_durations(inputs):
    """액션별 누름 시간(S -> C). 같은 액션의 다음 S 가 먼저 오면 앞의 S 는 버린다."""
    open_at, out = {}, {}
    for r in inputs:
        a, ph = r.get("a"), r.get("ph")
        if ph == "S":
            open_at[a] = r["t"]
        elif ph == "C" and a in open_at:
            out.setdefault(a, []).append(r["t"] - open_at.pop(a))
    return out


def starts(inputs, name):
    return sorted(r["t"] for r in inputs if r.get("a") == name and r.get("ph") == "S")


def intervals(times, cap=INTERVAL_CAP):
    return [b - a for a, b in zip(times, times[1:]) if 0 <= b - a <= cap]


def bursts(times, gap=BURST_GAP):
    """연타 묶음: 간격 gap 이하로 이어진 누름. (묶음 안 간격, 묶음 길이, 묶음 사이 간격)."""
    within, lengths, between = [], [], []
    if not times:
        return within, lengths, between
    n = 1
    for a, b in zip(times, times[1:]):
        d = b - a
        if d <= gap:
            within.append(d)
            n += 1
        else:
            lengths.append(n)
            if d <= INTERVAL_CAP:
                between.append(d)
            n = 1
    lengths.append(n)
    return within, lengths, between


def move_sign(r):
    """Player/Move 행의 방향(-1/0/1). C 는 0."""
    if r.get("ph") == "C":
        return 0
    v = r.get("v", 0) or 0
    return 1 if v > 0.3 else -1 if v < -0.3 else 0


def direction_change_intervals(inputs):
    """좌우 이동 방향이 뒤집힌 시각들 사이 간격."""
    last, flips = 0, []
    for r in inputs:
        if r.get("a") != "Player/Move":
            continue
        s = move_sign(r)
        if s != 0:
            if last != 0 and s != last:
                flips.append(r["t"])
            last = s
    return intervals(flips)


def engage_reactions(states, action_times):
    """적이 |dx|<=REACT_RANGE 안에 새로 들어온 순간 -> 다음 공격 입력(summary 와 같은 정의)."""
    out, prev, j = [], False, 0
    for s in states:
        near = any(abs(e.get("dx", 99)) <= REACT_RANGE for e in s.get("en", []))
        if near and not prev:
            while j < len(action_times) and action_times[j] < s["t"]:
                j += 1
            if j < len(action_times) and action_times[j] - s["t"] <= REACT_WINDOW:
                out.append(action_times[j] - s["t"])
        prev = near
    return out


class TickIndex:
    """t -> 그 시각 직전 tk 행."""

    def __init__(self, tks):
        self.tks = [r for r in tks if "p" in r]
        self.ts = [r["t"] for r in self.tks]

    def at(self, t):
        i = bisect.bisect_right(self.ts, t) - 1
        return self.tks[i] if i >= 0 else None


def jump_context(tk):
    """점프 직전 상황 하나(우선순위 순)."""
    if tk is None:
        return "unknown"
    en = tk.get("en") or []
    if any(e[5] <= NEAR_ENEMY and e[7] != 5 for e in en):
        return "enemy_near"
    if (tk.get("s", 0) & 4) or (tk.get("s", 0) & 2):
        return "rope"
    ed = tk.get("ed") or [-1, -1, -1, -1]
    left = (tk.get("s", 0) & 8) != 0
    face_edge = ed[0] if left else ed[1]
    if (tk.get("s", 0) & 1) and 0 <= face_edge <= EDGE_NEAR:
        return "edge"
    if abs(tk.get("mx", 0)) > 0.3:
        return "moving"
    return "standing"


def idle_reactions(avs, tick, inputs):
    """예고 창이 열릴 때 아무것도 안 누르고 있었으면 -> 첫 입력(S)까지. 순수 반응 시간."""
    st = sorted(r["t"] for r in inputs if r.get("ph") == "S" and not r.get("a", "").startswith("Debug"))
    out = []
    for av in avs:
        t0 = av.get("t0", av["t"])
        tk = tick.at(t0)
        if tk is None or tk.get("a", 0) != 0 or abs(tk.get("mx", 0)) > 0.01:
            continue
        i = bisect.bisect_left(st, t0)
        if i < len(st) and st[i] - t0 <= 3.0:
            out.append(st[i] - t0)
    return out


def segments(times, gap):
    """정렬된 시각들을 gap 이하 간격으로 묶은 [시작, 끝] 목록."""
    out = []
    for t in times:
        if out and t - out[-1][1] <= gap:
            out[-1][1] = t
        else:
            out.append([t, t])
    return out


def target_choices(evs, tick):
    """처치 직후 다음 첫 타격 대상을 처치 순간의 후보와 대조한다.
    반환: (최근접 맞음, 최근접 판정 수, HP 최저 맞음, HP 판정 수, 드롭 먼저, 드롭 판정 수)."""
    kills = [e["t"] for e in evs if e.get("e") == "kill"]
    dmgs = [e for e in evs if e.get("e") == "dmg"]
    picks = sorted(e["t"] for e in evs if e.get("e") == "pick")
    dts = [e["t"] for e in dmgs]
    near_hit = near_n = hp_hit = hp_n = drop_hit = drop_n = 0
    for kt in kills:
        tk0 = tick.at(kt)
        i = bisect.bisect_right(dts, kt + 0.05)
        if tk0 is None or i >= len(dmgs) or dmgs[i]["t"] - kt > 8.0:
            continue
        d = dmgs[i]
        cands = [e for e in (tk0.get("en") or []) if e[7] != 5 and e[6] > 0]
        tk1 = tick.at(d["t"])
        if tk1 is None or len(cands) < 2:
            continue
        # 맞은 몹 = 첫 타격 순간 같은 종 중 가장 가까운 것 -> 절대 위치로 처치 순간 후보와 대조
        same = [e for e in (tk1.get("en") or []) if e[0] == d.get("a")]
        if not same:
            continue
        hit = min(same, key=lambda e: e[5])
        hx = tk1["p"][0] + hit[1]
        hy = tk1["p"][1] + hit[2]
        px, py = tk0["p"][0], tk0["p"][1]
        chosen = min(cands, key=lambda e: (px + e[1] - hx) ** 2 + (py + e[2] - hy) ** 2)
        near_n += 1
        if chosen[5] <= min(e[5] for e in cands) + 0.01:
            near_hit += 1
        hps = [e[6] for e in cands]
        if max(hps) != min(hps):
            hp_n += 1
            if chosen[6] == min(hps):
                hp_hit += 1
        if "dr" in tk0:
            drop_n += 1
            j = bisect.bisect_right(picks, kt)
            if j < len(picks) and picks[j] < d["t"]:
                drop_hit += 1
    return near_hit, near_n, hp_hit, hp_n, drop_hit, drop_n


def town_returns(evs, cxs):
    """다른 맵 -> 마을 이동 직전 cx(가방 빈 칸·포션 수)."""
    bags, pots, prev = [], [], None
    cts = [c["t"] for c in cxs]
    for e in evs:
        if e.get("e") != "map":
            continue
        cur = e.get("a")
        if cur == TOWN and prev not in (None, TOWN):
            i = bisect.bisect_right(cts, e["t"] - 0.5) - 1
            if i >= 0:
                bags.append(cxs[i].get("bag", -1))
                pots.append(cxs[i].get("pot", -1))
        prev = cur
    return bags, pots


def potion_hp(evs, tick):
    out = []
    for e in evs:
        if e.get("e") == "use" and "Potion" in (e.get("a") or ""):
            tk = tick.at(e["t"] - 0.02)
            if tk and tk.get("hp") and tk["hp"][1] > 0:
                out.append(100.0 * tk["hp"][0] / tk["hp"][1])
    return out


def focus_intervals(fos):
    out = []
    for a, b in zip(fos, fos[1:]):
        pa, pb = a.get("p") or "", b.get("p") or ""
        if pa and pb and pa.split("/")[0] == pb.split("/")[0] and 0 < b["t"] - a["t"] <= FOCUS_CAP:
            out.append(b["t"] - a["t"])
    return out


# ───────────── 조립 ─────────────

def load(path):
    rows = {k: [] for k in KEEP_KINDS}
    with open(path, encoding="utf-8") as f:
        for line in f:
            try:
                r = json.loads(line)
            except ValueError:
                continue
            k = r.get("k")
            if k in rows:
                rows[k].append(r)
    return rows


def fit_session(rows):
    """한 세션의 표본 목록(합치기 전 원재료)."""
    ins = [r for r in rows["in"] if r.get("dev") in ("kb", "pad")]
    tick = TickIndex(rows["tk"])
    s = {"press": press_durations(ins), "minutes": 0.0, "field_minutes": 0.0}
    if rows["tk"]:
        s["minutes"] = (rows["tk"][-1]["t"] - rows["tk"][0]["t"]) / 60.0
    atk = starts(ins, "Player/Attack")
    skl = sorted(r["t"] for r in ins if r.get("ph") == "S" and r.get("a", "").startswith("Player/Skill"))
    s["attack_interval"] = [d for d in intervals(atk) if d <= 2.0]
    s["burst_within"], s["burst_len"], s["burst_gap"] = bursts(atk)
    s["dir_change"] = direction_change_intervals(ins)
    s["engage"] = engage_reactions(rows["st"], sorted(atk + skl))
    s["react_idle"] = idle_reactions(rows["av"], tick, ins)
    jumps = starts(ins, "Player/Jump")
    s["jumps"] = len(jumps)
    s["jump_ctx"] = Counter(jump_context(tick.at(t)) for t in jumps)
    field = [r for r in rows["st"] if r.get("map") and r.get("map") != TOWN]
    s["field_minutes"] = 0.1 * len(field) / 60.0       # st 10Hz
    s["field_jumps"] = sum(1 for t in jumps if _map_at(rows["st"], t) not in ("", TOWN))
    s["avoid_r"] = Counter(a.get("r", "none") for a in rows["av"])
    s["avoid_res"] = Counter(a.get("res", "none") for a in rows["av"])
    s["avoid_rt"] = [a["rt"] for a in rows["av"] if a.get("rt", -1) >= 0 and a.get("r") not in (None, "none")]
    s["target"] = target_choices(rows["ev"], tick)
    s["potion_hp"] = potion_hp(rows["ev"], tick)
    s["ret_bag"], s["ret_pot"] = town_returns(rows["ev"], rows["cx"])
    combat = sorted(e["t"] for e in rows["ev"] if e.get("e") in ("dmg", "kill", "hurt"))
    s["hunt"] = [b - a for a, b in segments(combat, HUNT_GAP) if b > a]
    s["idle"] = [h.get("dur", 0) for h in rows["hs"] if h.get("s") == "idle"]
    s["focus"] = focus_intervals(rows["fo"])
    s["kills"] = sum(1 for e in rows["ev"] if e.get("e") == "kill")
    return s


_MAP_CACHE = {}


def _map_at(sts, t):
    key = id(sts)
    if key not in _MAP_CACHE:
        _MAP_CACHE[key] = ([r["t"] for r in sts], [r.get("map", "") for r in sts])
    ts, ms = _MAP_CACHE[key]
    i = bisect.bisect_right(ts, t) - 1
    return ms[i] if i >= 0 else ""


def short(action):
    return action.split("/", 1)[1] if action.startswith("Player/") else action


def build_profile(sessions, sources):
    def cat(key):
        out = []
        for s in sessions:
            out += s[key]
        return out

    def csum(key):
        c = Counter()
        for s in sessions:
            c.update(s[key])
        return c

    press = {}
    for s in sessions:
        for a, v in s["press"].items():
            if a.startswith("Player/") or a in ("UI/OpenInventory", "Portal/PortalOn", "UI/Select", "UI/Back"):
                press.setdefault(short(a), []).extend(v)
    minutes = sum(s["minutes"] for s in sessions)
    fmin = sum(s["field_minutes"] for s in sessions)
    jumps = sum(s["jumps"] for s in sessions)
    fjumps = sum(s["field_jumps"] for s in sessions)
    jctx = csum("jump_ctx")
    ar, ares = csum("avoid_r"), csum("avoid_res")
    avn = sum(ar.values())
    t = [sum(s["target"][i] for s in sessions) for i in range(6)]
    prof = {
        "version": VERSION,
        "minN": MIN_N,
        "sources": sources,
        "minutes": round(minutes, 2),
        "fieldMinutes": round(fmin, 2),
        "kills": sum(s["kills"] for s in sessions),
        "dists": [],
        "ratios": [],
    }
    D = prof["dists"]
    R = prof["ratios"]

    def add_d(key, values, unit="s"):
        d = dist(values, unit)
        d["key"] = key
        D.append(d)

    def add_r(key, hit, total):
        r = ratio(hit, total)
        r["key"] = key
        R.append(r)

    add_d("react.engage", cat("engage"))
    add_d("react.idle", cat("react_idle"))
    for a in sorted(press):
        add_d("press." + a, press[a])
    add_d("attack.interval", cat("attack_interval"))
    add_d("attack.burstWithin", cat("burst_within"))
    add_d("attack.burstLen", cat("burst_len"), "count")
    add_d("attack.burstGap", cat("burst_gap"))
    add_d("move.dirChange", cat("dir_change"))
    add_d("avoid.rt", cat("avoid_rt"))
    add_d("potion.hpPct", cat("potion_hp"), "%")
    add_d("return.bagFree", cat("ret_bag"), "slots")
    add_d("return.potions", cat("ret_pot"), "count")
    add_d("hunt.segment", cat("hunt"))
    add_d("rest.idle", cat("idle"))
    add_d("menu.focusInterval", cat("focus"))
    prof["rates"] = [rate("jump.perMinute", jumps, minutes), rate("jump.fieldPerMinute", fjumps, fmin),
                     rate("kill.perMinute", prof["kills"], minutes)]
    for c in ("enemy_near", "rope", "edge", "moving", "standing", "unknown"):
        add_r("jump.ctx." + c, jctx.get(c, 0), jumps)
    for c in ("jump", "dash", "back", "fwd", "guard", "atk", "none"):
        add_r("avoid.r." + c, ar.get(c, 0), avn)
    for c in ("hit", "guard", "parry", "immune", "iframe", "miss", "none"):
        add_r("avoid.res." + c, ares.get(c, 0), avn)
    add_r("target.nearest", t[0], t[1])
    add_r("target.lowestHp", t[2], t[3])
    add_r("target.dropFirst", t[4], t[5])
    return prof


def main(argv):
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    out_path, paths, i = None, [], 1
    while i < len(argv):
        if argv[i] == "-o":
            out_path, i = argv[i + 1], i + 2
        else:
            paths.append(argv[i])
            i += 1
    if not paths:
        print(__doc__)
        return 2
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out_path = out_path or os.path.join(root, "Assets", "Resources", "Bot", "human_profile.json")
    sessions = [fit_session(load(p)) for p in paths]
    prof = build_profile(sessions, [os.path.basename(p) for p in paths])
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(prof, f, ensure_ascii=False, indent=1)
        f.write("\n")
    nulls = [d["key"] for d in prof["dists"] if d["p50"] is None] + [r["key"] for r in prof["ratios"] + prof["rates"] if r["value"] is None]
    print("wrote %s (dists %d, ratios %d, null %d: %s)" % (out_path, len(prof["dists"]), len(prof["ratios"]), len(nulls), ", ".join(nulls)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
