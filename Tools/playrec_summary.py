#!/usr/bin/env python
"""PlayRecorder jsonl -> markdown 요약 (표준 라이브러리만, R2).

사용: python Tools/playrec_summary.py Logs/playrec/<세션>.jsonl [출력.md] [--frames]
기본 출력: Logs/playrec/<세션>_summary.md (150줄 이내). 봇 설계 입력 수치 + 플레이테스트 문제 목록.
--frames: 북마크·막힘·사망 시각마다 ffmpeg -ss 로 영상 프레임 1장을 <세션>_frames/ 에 뽑는다(ffmpeg: PATH 또는 WinGet 설치).
형식표는 .claude-rules/Game/PlayBotDesign.md.
"""
import bisect
import json
import os
import shutil
import subprocess
import sys
from collections import Counter, defaultdict

REACT_RANGE = 6.0        # 적이 가로(|dx|) 이 안에 들어온 순간부터
REACT_WINDOW = 5.0       # 이 시간 안에 공격 입력이 없으면 반응 없음으로 본다
ATTACK_ACTIONS = ("Player/Attack",)
SKILL_PREFIX = "Player/Skill"
MAX_DT = 0.5             # 스냅샷 간격이 이보다 크면(일시정지 등) 체류 시간에 넣지 않는다
VIDEO_FPS = 15
STATE_NAMES = ["대기", "추적", "예고", "공격", "피격", "사망"]
STAT_NAMES = ["기본피해", "방어", "치명률", "치명피해", "피해증가", "이속", "공속", "최대HP"]


def load(path):
    rows, bad = [], 0
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


def dist(values, unit="s"):
    if not values:
        return "n=0"
    return "n=%d 중앙값 %.2f%s p10 %.2f%s p90 %.2f%s" % (
        len(values), pct(values, 50), unit, pct(values, 10), unit, pct(values, 90), unit)


def fmt_time(sec):
    return "%d분 %02d초" % (int(sec) // 60, int(sec) % 60)


def kinds(rows, k):
    return [r for r in rows if r.get("k") == k]


def input_stats(inputs):
    """액션별 (S 횟수, 평균 누름 시간). S -> C 를 이름 단위로 짝짓는다."""
    count, holds, open_at, dev = Counter(), defaultdict(list), {}, defaultdict(Counter)
    for r in inputs:
        a, ph = r.get("a", "?"), r.get("ph")
        dev[a][r.get("dev", "?")] += 1
        if ph == "S":
            count[a] += 1
            open_at[a] = r["t"]
        elif ph == "C" and a in open_at:
            holds[a].append(r["t"] - open_at.pop(a))
    return count, holds, dev


def press_times(inputs, names=None, prefix=None):
    return sorted(r["t"] for r in inputs if r.get("ph") == "S" and (
        (names and r.get("a") in names) or (prefix and r.get("a", "").startswith(prefix))))


def intervals(times, cap=10.0):
    return [b - a for a, b in zip(times, times[1:]) if b - a <= cap]


def reaction_times(states, action_times):
    """적이 가로 REACT_RANGE 안에 새로 들어온 순간 -> 다음 공격(평타·스킬) 입력까지."""
    out, missed, prev_near, j = [], 0, False, 0
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


def direction_changes(inputs):
    last, n = 0, 0
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


class Clock:
    """게임 시간 t -> 실제 시간 u (tk 행), 실제 시간 u -> 영상 초 (vf 행)."""

    def __init__(self, tks, vfs):
        self.t = [r["t"] for r in tks]
        self.u = [r["u"] for r in tks]
        pts = sorted((r["fu"], r["f"]) for r in vfs if r.get("f", -1) >= 0 and r.get("fu", -1) >= 0)
        self.vu = [p[0] for p in pts]
        self.vf = [p[1] for p in pts]

    def t2u(self, t):
        if not self.t:
            return None
        i = min(bisect.bisect_left(self.t, t), len(self.t) - 1)
        return self.u[i] + (t - self.t[i])

    def u2video(self, u):
        if u is None or not self.vu:
            return None
        i = bisect.bisect_right(self.vu, u) - 1
        if i < 0:
            i = 0
        f = self.vf[i] + (u - self.vu[i]) * VIDEO_FPS
        return max(0.0, f / VIDEO_FPS)


def section_header(L, rows, bad, name, tks, sts):
    total = (rows[-1]["t"] - rows[0]["t"]) if len(rows) > 1 else 0.0
    c = Counter(r.get("k") for r in rows)
    L.append("# 플레이 기록 요약: %s" % name)
    L.append("")
    L.append("- 총 %s (게임 시간) · 줄 %d (%s)%s" % (fmt_time(total), len(rows),
             " ".join("%s %d" % kv for kv in c.most_common()), " · 깨진 줄 %d" % bad if bad else ""))
    srcs = Counter(r.get("src", "?") for r in rows)
    L.append("- 출처: " + " ".join("src=%s %d" % kv for kv in srcs.most_common()))
    meta = next((r for r in reversed(rows) if r.get("k") == "meta"), None)  # 마지막 meta = 봇 시작 직전 상태
    if meta:
        L.append("- 메타: %s git %s · %dx%d · %s · 시작 Lv%s %s · 영상 %s" % (
            meta.get("ver"), meta.get("git"), meta.get("sw", 0), meta.get("sh", 0), meta.get("dev", ""),
            meta.get("lv"), meta.get("map") or "?", meta.get("video")))
    if len(tks) > 1:
        span = tks[-1]["u"] - tks[0]["u"]
        gaps = [b["u"] - a["u"] for a, b in zip(tks, tks[1:]) if b["u"] - a["u"] > 0.2]
        live = span - sum(gaps)
        L.append("- tk %.1fHz (실제 %s) · 0.2초 넘는 끊김 %d회 %.0f초 제외 %.1fHz · st/tk %.2f" % (
            len(tks) / span if span > 0 else 0, fmt_time(span), len(gaps), sum(gaps),
            (len(tks) - 1 - len(gaps)) / live if live > 0 else 0, len(sts) / float(len(tks))))
    return max(total / 60.0, 1e-6)


def section_video(L, path, vfs, clock):
    mp4 = os.path.splitext(path)[0] + ".mp4"
    L += ["", "## 영상"]
    if not os.path.exists(mp4):
        L.append("- 없음" + (" (" + vfs[-1].get("err") + ")" if vfs and vfs[-1].get("err") else ""))
        return
    size = os.path.getsize(mp4) / 1048576.0
    last = vfs[-1] if vfs else {}
    frames = last.get("f", -1) + 1
    L.append("- %s %.1fMB · 프레임 %d (%.0f초, %dfps) · 버린 프레임 %d%s" % (
        os.path.basename(mp4), size, frames, frames / float(VIDEO_FPS), VIDEO_FPS, last.get("drop", 0),
        " · 오류 " + last["err"] if last.get("err") else ""))


def section_bookmarks(L, bms, clock, frames_out):
    L += ["", "## 북마크 (F8 · 패드 SELECT+R3)"]
    for b in bms[:20]:
        vs = clock.u2video(b.get("u"))
        L.append("- #%d t=%.0f %s (%.1f,%.1f) %s · 영상 %s · %s%s" % (
            b.get("n", 0), b["t"], b.get("map", ""), b.get("x", 0), b.get("y", 0), b.get("fsm", ""),
            "%.1fs" % vs if vs is not None else "-", b.get("png", ""), frames_out.get(("bm", b.get("n")), "")))
    if not bms:
        L.append("- (없음)")


def section_combat(L, ins, sts, evs, minutes):
    atk = press_times(ins, names=ATTACK_ACTIONS)
    skl = press_times(ins, prefix=SKILL_PREFIX)
    L += ["", "## 전투 리듬"]
    L.append("- 평타 간격: %s" % dist(intervals(atk)))
    rt, missed = reaction_times(sts, sorted(atk + skl))
    L.append("- 스킬 %d회 (%.1f/분) · 반응(적 |dx|<=%.0f 진입->첫 공격) %s · 무반응 %d" % (
        len(skl), len(skl) / minutes, REACT_RANGE, dist(rt), missed))
    jumps = press_times(ins, names=("Player/Jump",))
    dash = press_times(ins, names=("Player/Dash",))
    dc = direction_changes(ins)
    L.append("- 점프 %.1f/분 · 대시 %.1f/분 · 방향 전환 %.1f/분" % (len(jumps) / minutes, len(dash) / minutes, dc / minutes))
    evc = Counter(e.get("e") for e in evs)
    dmg = [e for e in evs if e.get("e") == "dmg"]
    hurt = [e.get("v", 0) for e in evs if e.get("e") == "hurt"]
    crit = sum(1 for e in dmg if "crit" in (e.get("b") or ""))
    L += ["", "## 전투 결과"]
    L.append("- 처치 %d (%.1f/분) · 준 피해 %d회 합 %d (치명 %d%% · 넉백 있음 %d · 최대 콤보 %d) · 받은 피해 %d회 합 %d · 사망 %d" % (
        evc.get("kill", 0), evc.get("kill", 0) / minutes, len(dmg), sum(e.get("v", 0) for e in dmg),
        100 * crit // max(len(dmg), 1), sum(1 for e in dmg if e.get("kb", 0) > 0), max([e.get("cb", 0) for e in dmg] or [0]),
        len(hurt), sum(hurt), evc.get("death", 0)))
    if sts:
        L.append("- 레벨 %d -> %d · 골드 %d -> %d" % (
            sts[0].get("lv", 0), sts[-1].get("lv", 0), sts[0].get("gold", 0), sts[-1].get("gold", 0)))
    other = ["%s %d" % kv for kv in evc.most_common() if kv[0] not in ("kill", "dmg", "hurt", "death")]
    if other:
        L.append("- 기타 이벤트: " + ", ".join(other[:14]))


def section_avoid(L, avs, hits, tks):
    L += ["", "## 공격 예고 -> 회피 반응 (av)"]
    hc = Counter(h.get("r") for h in hits)
    L.append("- 피격 시도 %d: %s" % (len(hits), ", ".join("%s %d" % kv for kv in hc.most_common()) or "-"))
    if avs:
        res = Counter(a.get("res") for a in avs)
        rk = Counter(a.get("r") for a in avs)
        rts = [a["rt"] for a in avs if a.get("rt", -1) >= 0]
        L.append("- 예고 %d건 · 결과 %s · 첫 반응 %s · 반응 시간 %s" % (
            len(avs), ", ".join("%s %d" % kv for kv in res.most_common()),
            ", ".join("%s %d" % kv for kv in rk.most_common()), dist(rts)))
        by = defaultdict(list)
        for a in avs:
            by[a.get("a", "?")].append(a)
        for sp, lst in sorted(by.items(), key=lambda kv: -len(kv[1]))[:6]:
            L.append("  - %s: %d건 맞음 %d%% · 주 반응 %s" % (sp, len(lst), 100 * sum(1 for a in lst if a.get("res") == "hit") // len(lst),
                     Counter(a.get("r") for a in lst).most_common(1)[0][0]))
    else:
        L.append("- 예고 0건")
    # 몹 종류별 신호(Sig)와 관측된 상태: 예고 신호가 없는 종은 회피 학습에서 빠진다
    sig, states = defaultdict(int), defaultdict(Counter)
    for r in tks:
        for e in r.get("en", []):
            if len(e) >= 10:
                sig[e[0]] |= e[9]
                states[e[0]][e[7]] += 1
    if sig:
        L.append("- 종별 신호(1 애니 2 돌진 4 사격)·상태: " + "; ".join(
            "%s sig%d %s" % (sp, sig[sp], "/".join("%s%d" % (STATE_NAMES[k], v) for k, v in sorted(states[sp].items())))
            for sp in sorted(sig)[:10]))


def section_ui(L, sts, decs, fos):
    opens, prev = Counter(), set()
    for s in sts:
        cur = set(s.get("ui", []))
        for w in cur - prev:
            opens[w] += 1
        prev = cur
    ud = dwell(sts, lambda s: s.get("ui", []))
    L += ["", "## UI 창 (열기 횟수 / 체류)"]
    L.append("- " + (", ".join("%s %d회/%s" % (w, n, fmt_time(ud.get(w, 0.0))) for w, n in opens.most_common(10)) or "(없음)"))
    looks = Counter(f["p"][5:] for f in fos if f.get("p", "").startswith("shop/"))
    L.append("- 포커스 이동 %d회 · 상점에서 본 품목 %d종: %s" % (len(fos), len(looks), ", ".join("%s %d" % kv for kv in looks.most_common(8))))
    L += ["", "## UI 결정 (dec: 산·판·장착·사용·버림·퀵슬롯·스킬투자)"]
    for d in decs[:15]:
        delta = ["%s %+d" % (STAT_NAMES[i], b - a) for i, (a, b) in enumerate(zip(d.get("s0", []), d.get("s1", []))) if a != b]
        L.append("- t=%.0f %s %s%s 골드 %+d %s · 포커스 %s" % (
            d["t"], d.get("d"), d.get("a", ""), " x%d" % d["v"] if "v" in d else "", d.get("g1", 0) - d.get("g0", 0),
            ("(" + ", ".join(delta) + ")") if delta else "", d.get("foc") or "-"))
    if len(decs) > 15:
        L.append("- ... (총 %d)" % len(decs))
    if not decs:
        L.append("- (없음)")


def section_human(L, hss, warns, clock, frames_out):
    L += ["", "## 사람 신호 · 막힘"]
    c = Counter(h.get("s") for h in hss)
    L.append("- " + (", ".join("%s %d" % kv for kv in c.most_common()) or "(없음)") +
             " · 무입력 합 %s · 일시정지 합 %s" % (
                 fmt_time(sum(h.get("dur", 0) for h in hss if h.get("s") == "idle")),
                 fmt_time(sum(h.get("dur", 0) for h in hss if h.get("s") == "pause"))))
    mash = Counter(h.get("a") for h in hss if h.get("s") == "mash")
    qc = Counter(h.get("a") for h in hss if h.get("s") == "qclose")
    if mash:
        L.append("- 연타: " + ", ".join("%s %d" % kv for kv in mash.most_common(5)))
    if qc:
        L.append("- 창 바로 닫기: " + ", ".join("%s %d" % kv for kv in qc.most_common(5)))
    stuck = [w for w in warns if w.get("ty") == "stuck"]
    for w in stuck[:8]:
        vs = clock.u2video(clock.t2u(w["t"]))
        L.append("- 막힘 t=%.0f %s (%.1f,%.1f) %s · 영상 %s%s" % (w["t"], w.get("map", ""), w.get("x", 0), w.get("y", 0),
                 w.get("fsm", ""), "%.1fs" % vs if vs is not None else "-", frames_out.get(("stuck", w["t"]), "")))


def section_perf(L, pfs, hitches, gds):
    L += ["", "## 성능 · 지형"]
    if pfs:
        avg = [p["avg"] for p in pfs[2:]] or [p["avg"] for p in pfs]
        L.append("- 프레임(초당 평균의 분포) 중앙값 %.2fms 평균 %.2fms p95 %.2fms · 최악 %.0fms · 50ms 끊김 %d · GC %d회 · 할당 급증 %d초 · 힙 %.0fMB" % (
            pct(avg, 50), sum(avg) / len(avg), pct(avg, 95), max(p["max"] for p in pfs), len(hitches), sum(p.get("gc", 0) for p in pfs),
            sum(1 for p in pfs if p.get("spike")), pfs[-1].get("heap", 0)))
    L.append("- 지형 격자 %d종(gd)" % len(gds))


def section_warns(L, warns):
    L += ["", "## 경고 · 예외 (중복 묶음)"]
    groups = {}
    for w in warns:
        if w.get("ty") == "stuck":
            continue
        key = (w.get("ty", "?"), (w.get("msg") or "")[:80])
        g = groups.setdefault(key, {"n": 0, "first": w})
        g["n"] = max(g["n"] + 1, w.get("n", 0))
    for (ty, _), g in sorted(groups.items(), key=lambda kv: -kv[1]["n"])[:10]:
        w = g["first"]
        L.append("- [%s] x%d t=%.0f %s" % (ty, g["n"], w["t"], (w.get("msg") or "").replace("\n", " ")[:110]))
    if not groups:
        L.append("- (없음)")


def find_ffmpeg():
    """PATH -> WinGet 설치 폴더(기록기 PlayRecVideo.FindFfmpeg 와 같은 순서)."""
    ff = shutil.which("ffmpeg")
    if ff:
        return ff
    root = os.path.join(os.environ.get("LOCALAPPDATA", ""), "Microsoft", "WinGet", "Packages")
    if os.path.isdir(root):
        for d in sorted(os.listdir(root)):
            if "ffmpeg" not in d.lower():
                continue
            for base, _, files in os.walk(os.path.join(root, d)):
                if "ffmpeg.exe" in files:
                    return os.path.join(base, "ffmpeg.exe")
    return None


def extract_frames(path, rows, clock):
    """북마크·막힘·사망 시각의 영상 프레임 1장씩 -> <세션>_frames/. {(종류,키): ' · 프레임 파일'}"""
    mp4 = os.path.splitext(path)[0] + ".mp4"
    ff = find_ffmpeg()
    if not ff or not os.path.exists(mp4):
        print("frames: ffmpeg 또는 mp4 없음")
        return {}
    out_dir = os.path.splitext(path)[0] + "_frames"
    os.makedirs(out_dir, exist_ok=True)
    jobs = []
    for b in kinds(rows, "bm"):
        jobs.append((("bm", b.get("n")), "bm%02d" % b.get("n", 0), b.get("u")))
    for w in kinds(rows, "warn"):
        if w.get("ty") == "stuck":
            jobs.append((("stuck", w["t"]), "stuck_t%d" % int(w["t"]), clock.t2u(w["t"])))
    for e in kinds(rows, "ev"):
        if e.get("e") == "death":
            jobs.append((("death", e["t"]), "death_t%d" % int(e["t"]), clock.t2u(e["t"])))
    res = {}
    for key, stem, u in jobs[:40]:
        vs = clock.u2video(u)
        if vs is None:
            continue
        dst = os.path.join(out_dir, "%s_v%.1f.png" % (stem, vs))
        r = subprocess.run([ff, "-v", "error", "-y", "-ss", "%.2f" % vs, "-i", mp4, "-frames:v", "1", dst],
                           capture_output=True, text=True)
        if r.returncode == 0 and os.path.exists(dst):
            res[key] = " · 프레임 " + os.path.basename(out_dir) + "/" + os.path.basename(dst)
    print("frames: %d/%d -> %s" % (len(res), len(jobs), out_dir))
    return res


def build(rows, bad, name, path, frames=False):
    L = []
    ins, sts, evs, tks = kinds(rows, "in"), kinds(rows, "st"), kinds(rows, "ev"), kinds(rows, "tk")
    warns, vfs = kinds(rows, "warn"), kinds(rows, "vf")
    clock = Clock(tks, vfs)
    frames_out = extract_frames(path, rows, clock) if frames else {}
    minutes = section_header(L, rows, bad, name, tks, sts)
    md = dwell(sts, lambda s: [s.get("map") or "(미상)"])
    L.append("- 맵 체류: " + (", ".join("%s %s" % (k, fmt_time(v)) for k, v in sorted(md.items(), key=lambda kv: -kv[1])[:8]) or "-"))
    section_video(L, path, vfs, clock)
    section_bookmarks(L, kinds(rows, "bm"), clock, frames_out)
    count, holds, dev = input_stats(ins)
    L += ["", "## 입력 (액션: 횟수 / 평균 누름 / 장치, 상위 16)"]
    top = [(a, n) for a, n in count.most_common() if not a.startswith("Debug Menu")][:16]
    for a, n in top:
        h = holds.get(a)
        L.append("- %s: %d회 / %s / %s" % (a, n, "%.2fs" % (sum(h) / len(h)) if h else "-",
                                             ",".join("%s%d" % kv for kv in dev[a].items())))
    if not top:
        L.append("- (입력 없음)")
    section_combat(L, ins, sts, evs, minutes)
    section_avoid(L, kinds(rows, "av"), kinds(rows, "hitat"), tks)
    section_ui(L, sts, kinds(rows, "dec"), kinds(rows, "fo"))
    section_human(L, kinds(rows, "hs"), warns, clock, frames_out)
    section_perf(L, kinds(rows, "pf"), kinds(rows, "hitch"), kinds(rows, "gd"))
    section_warns(L, warns)
    return "\n".join(L) + "\n"


def main(argv):
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")  # 윈도우 콘솔 cp949 로 한글이 깨져 보이는 것 방지
    args = [a for a in argv[1:] if not a.startswith("--")]
    frames = "--frames" in argv
    if not args:
        print("usage: playrec_summary.py <file.jsonl> [out.md] [--frames]")
        return 2
    path = args[0]
    rows, bad = load(path)
    if not rows:
        print("empty log")
        return 1
    name = os.path.splitext(os.path.basename(path))[0]
    out = args[1] if len(args) > 1 else os.path.join(os.path.dirname(os.path.abspath(path)), name + "_summary.md")
    text = build(rows, bad, name, path, frames)
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    print(out)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
