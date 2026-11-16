#!/usr/bin/env python
"""막힘 경고 직전 N초 tk 행을 상태(걷기·점프·낙하·로프)·입력·속도로 집계한다 (표준 라이브러리만).
사용: python Tools/playrec_stuck_tk.py <jsonl...> [--sec 5] [--xmin -410 --xmax -370]   (봇 기록: "막힘" warn 기준)
      python Tools/playrec_stuck_tk.py <jsonl> --pass --xmin -401 --xmax -386      (사람/봇 기록: 구간 통과 시간·점프 수)
      python Tools/playrec_stuck_tk.py <jsonl...> --edges [--sec 5]                (봇 기록: 막힘 직전 tk 의 be(간선 종류)·bf(목표) 별 집계, 맵·x 전체)
"""
import json, sys, re, argparse
from collections import Counter

def load(p):
    out = []
    with open(p, encoding="utf-8") as f:
        for l in f:
            try: out.append(json.loads(l))
            except ValueError: pass
    return out

def state(r):
    s = r.get("s", 0); vy = r["p"][3]
    if s & 2: return "로프"
    if s & 1: return "접지"
    return "상승" if vy > 0.5 else ("하강" if vy < -0.5 else "공중")

def rises(rows, bit):
    n, prev = 0, 0
    for r in rows:
        b = (r.get("a", 0) >> bit) & 1
        if b and not prev: n += 1
        prev = b
    return n

def stuck_report(path, sec, xmin, xmax):
    rows = load(path)
    tk = [r for r in rows if r.get("k") == "tk" and "p" in r]
    for w in rows:
        if w.get("k") != "warn" or "막힘" not in w.get("msg", ""): continue
        m = re.search(r"x=(-?[\d.]+) y=(-?[\d.]+)", w["msg"])
        if not m: continue
        x, y = float(m[1]), float(m[2])
        if not (xmin <= x <= xmax): continue
        seg = [r for r in tk if w["t"] - sec <= r["t"] <= w["t"]]
        if not seg: continue
        st = Counter(state(r) for r in seg)
        n = len(seg)
        xs = [r["p"][0] for r in seg]; ys = [r["p"][1] for r in seg]
        mxs = Counter("L" if r["mx"] < -0.3 else "R" if r["mx"] > 0.3 else "0" for r in seg)
        mys = Counter("U" if r["my"] > 0.3 else "D" if r["my"] < -0.3 else "0" for r in seg)
        atk = sum(1 for r in seg if (r.get("a", 0) >> 3) & 1)
        en = [r["en"][0] for r in seg if r.get("en")]
        spd = sum(abs(r["p"][2]) for r in seg) / n
        print(f"{path.split('/')[-1][:15]} t={w['t']:.0f} x={x:.1f} y={y:.1f} | 상태 " +
              " ".join(f"{k}{v*100//n}%" for k, v in st.most_common()) +
              f" | mx {dict(mxs)} my {dict(mys)} | 점프입력 {rises(seg,0)}회 공격%{atk*100//n}"
              f" | x범위 {min(xs):.1f}~{max(xs):.1f} y범위 {min(ys):.1f}~{max(ys):.1f} |vx|평균 {spd:.2f}"
              f" | 근접몹 {en[-1][0] if en else '-'}")

def focus_class(bf):
    return {0: "없음", -1: "배회", -2: "드롭", -3: "창"}.get(bf, "몹")   # 몹 Uid 는 인스턴스 ID 라 음수일 수 있다

def edge_report(paths, sec):
    """막힘 warn 직전 sec 초 tk 행의 be·bf 를 센다. be 필드가 없는 옛 기록은 건너뛴다"""
    by_edge, by_pair, total, nofield = Counter(), Counter(), 0, 0
    for path in paths:
        rows = load(path)
        tk = [r for r in rows if r.get("k") == "tk" and "p" in r]
        for w in rows:
            if w.get("k") != "warn" or "막힘" not in w.get("msg", ""): continue
            total += 1
            seg = [r for r in tk if w["t"] - sec <= r["t"] <= w["t"] and "be" in r]
            if not seg: nofield += 1; continue
            be = Counter(r["be"] for r in seg).most_common(1)[0][0]   # 막힘 직전 구간에서 가장 오래 따라간 간선
            fc = focus_class(Counter(r.get("bf", 0) for r in seg).most_common(1)[0][0])
            by_edge[be] += 1; by_pair[(be, fc)] += 1
    print(f"막힘 경고 {total}건 (be 필드 없음 {nofield}) — 직전 {sec:g}초 간선별:")
    for k, v in by_edge.most_common(): print(f"  {k:9s} {v}")
    for (k, fc), v in by_pair.most_common(): print(f"  {k}/{fc} {v}")

def pass_report(path, xmin, xmax):
    rows = load(path)
    tk = [r for r in rows if r.get("k") == "tk" and "p" in r]
    gd = [r for r in rows if r.get("k") == "gd"]
    # 같은 맵(SideRoad) 구간 점유: x 가 범위 안인 연속 구간(간격 2초 이상이면 분리), y 0~3 로 한정하지 않고 범위 안 모두
    runs, cur = [], []
    for r in tk:
        x = r["p"][0]
        if xmin <= x <= xmax and r["p"][1] < 4:
            if cur and r["t"] - cur[-1]["t"] > 2: runs.append(cur); cur = []
            cur.append(r)
    if cur: runs.append(cur)
    for c in runs:
        dur = c[-1]["t"] - c[0]["t"]
        if dur < 0.3: continue
        net = c[-1]["p"][0] - c[0]["p"][0]
        print(f"{path.split('/')[-1][:15]} t={c[0]['t']:.0f}~{c[-1]['t']:.0f} {dur:.1f}초 x {c[0]['p'][0]:.0f}->{c[-1]['p'][0]:.0f} "
              f"y {min(r['p'][1] for r in c):.1f}~{max(r['p'][1] for r in c):.1f} 점프 {rises(c,0)} 방향 {'<-' if net<0 else '->'}")

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+"); ap.add_argument("--sec", type=float, default=5)
    ap.add_argument("--xmin", type=float, default=-410); ap.add_argument("--xmax", type=float, default=-370)
    ap.add_argument("--pass", dest="passmode", action="store_true")
    ap.add_argument("--edges", action="store_true")
    a = ap.parse_args()
    if a.edges:
        edge_report(a.files, a.sec); sys.exit(0)
    for f in a.files:
        (pass_report(f, a.xmin, a.xmax) if a.passmode else stuck_report(f, a.sec, a.xmin, a.xmax))
