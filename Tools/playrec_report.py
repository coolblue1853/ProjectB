#!/usr/bin/env python
"""봇 기록 1개 -> 자동 플레이테스트 보고서 한 장(<세션>_report.md, 300줄 이내). 표준 라이브러리만.

사용: python Tools/playrec_report.py <봇 jsonl> [--status "<봇 상태 줄>"] [--prep "fresh=1 time=day level=-"] [--cond "<시작 조건 한 줄>"] [-o 출력.md]
- 같은 폴더의 <세션>_summary.md · <세션>_balance.md 를 읽는다(없으면 playrec_summary.py·playrec_balance.py 를 돌려 만든다).
- 사람 기준값: Logs/playrec/20261115_131248·133116(읽기 전용)을 playrec_balance 로 돌린 표(Logs/playrec_bot/human_1115_balance.md, 없으면 생성)와
  맵·낮밤별 처치/분·EXP/분·받은피해/분, 몹별 처치시간·피해/처치를 비교해 2배 초과(▲)·절반 미만(▼)을 표시한다.
설계: .claude-rules/Game/PlayBotDesign.md 13절.
"""
import json
import os
import re
import subprocess
import sys
from collections import Counter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
HUMAN_JSONL = ["Logs/playrec/20261115_131248.jsonl", "Logs/playrec/20261115_133116.jsonl"]
HUMAN_MD = "Logs/playrec_bot/human_1115_balance.md"
MAX_LINES = 300


def read(path):
    with open(path, encoding="utf-8") as f:
        return f.read().split("\n")


def run_tool(script, jsonl, out=None):
    cmd = [sys.executable, "-X", "utf8", os.path.join(ROOT, "Tools", script)] + jsonl
    if out:
        cmd += ["-o", out]
    subprocess.run(cmd, cwd=ROOT, stdout=subprocess.DEVNULL, check=False)


def section(lines, title_start):
    """'## 제목' 으로 시작하는 절의 본문(다음 '## ' 전까지)"""
    out, on = [], False
    for ln in lines:
        if ln.startswith("## "):
            if on:
                break
            on = ln.startswith("## " + title_start)
            continue
        if on:
            out.append(ln)
    return out


def table_rows(lines):
    rows = []
    for ln in lines:
        if ln.startswith("|") and not ln.startswith("|---"):
            rows.append([c.strip() for c in ln.strip().strip("|").split("|")])
    return rows[1:] if rows else []


def num(s):
    m = re.match(r"-?\d+(\.\d+)?", s.strip()) if s else None
    return float(m.group(0)) if m else None


def ratio_flag(bot, human):
    if bot is None or human is None or human <= 0:
        return ""
    r = bot / human
    if r > 2.0:
        return "▲%.1f배" % r
    if r < 0.5:
        return "▼%.1f배" % r
    return ""


def parse_status(text):
    d = {}
    for k, v in re.findall(r"(\w+)=([^\s]+)", text or ""):
        d[k] = v
    return d


def jsonl_digest(path):
    c, errs, warns, deaths, buys, sells, equips, kills = Counter(), Counter(), Counter(), [], 0, 0, 0, 0
    last = {}
    for ln in open(path, encoding="utf-8"):
        try:
            r = json.loads(ln)
        except ValueError:
            continue
        k = r.get("k")
        if k == "warn":
            msg = re.sub(r"\d+(\.\d+)?", "#", r.get("msg", ""))[:150]
            if r.get("ty") in ("Error", "Exception", "Assert"):
                errs[(r.get("ty"), msg)] += 1
            elif "[PB][Bot]" in r.get("msg", ""):
                warns[msg] += 1
            if "[PB][Bot] 사망" in r.get("msg", ""):
                deaths.append((r.get("t", 0), r["msg"][:100]))
        elif k == "ev":
            e = r.get("e")
            if e == "buy":
                buys += 1
            elif e == "sell":
                sells += 1
            elif e == "equip":
                equips += 1
            elif e == "kill":
                kills += 1
        elif k == "meta":
            last = r
    return errs, warns, deaths, buys, sells, equips, kills, last


def progress_digest(path):
    """st 행에서 끝 Lv·골드와 맵 방문 순서(맵, 들어간 초)를 뽑는다."""
    order, lv0, lv, gold = [], None, None, None
    for ln in open(path, encoding="utf-8"):
        if '"k":"st"' not in ln:
            continue
        try:
            r = json.loads(ln)
        except ValueError:
            continue
        m = r.get("map")
        if m and (not order or order[-1][0] != m):
            order.append((m, int(r.get("t", 0))))
        if lv0 is None and r.get("lv"):
            lv0 = r["lv"]
        lv, gold = r.get("lv", lv), r.get("gold", gold)
    return order, lv0, lv, gold


def curve_line(jsonl):
    """Tools/playrec_curve.py 를 돌려 CURVE_SUMMARY 한 줄을 받는다(실패하면 빈 문자열)."""
    try:
        out = subprocess.run([sys.executable, "-X", "utf8", os.path.join(ROOT, "Tools", "playrec_curve.py"), jsonl],
                             cwd=ROOT, capture_output=True, text=True, encoding="utf-8", check=False).stdout
    except OSError:
        return ""
    for ln in out.splitlines():
        if ln.startswith("CURVE_SUMMARY "):
            return ln[len("CURVE_SUMMARY "):]
    return ""


def human_start():
    """사람 기록 첫 세션 meta 의 시작 조건 한 줄(없으면 빈 문자열)."""
    path = os.path.join(ROOT, HUMAN_JSONL[0])
    if not os.path.exists(path):
        return ""
    for ln in open(path, encoding="utf-8"):
        if '"k":"meta"' not in ln:
            continue
        try:
            m = json.loads(ln)
        except ValueError:
            return ""
        return "사람 %s: %s" % (m.get("session", "?"), start_text(m))
    return ""


def start_text(m):
    eq = [e for e in (m.get("eq") or []) if e]
    return ("Lv%s · EXP %s%% · 골드 %s · 장비 %s · 가방 빈칸 %s · 가방 %s · 스킬 투자 %s · 진행 퀘스트 %d · 완료 퀘스트 %s · 낮/밤 %s"
            % (m.get("lv", "?"), m.get("expp", "?"), m.get("gold", "?"), ",".join(eq) or "없음", m.get("bag", "?"), m.get("bagItems", "미기록"),
               m.get("skillSpent", "미기록"), len(m.get("q") or []), m.get("doneQ", "미기록"), {0: "낮", 1: "밤"}.get(m.get("night"), "?")))


def start_warnings(m, cond, prep):
    """PREP_FRESH 인데 새 게임 조건이 아니면 경고 문장들. meta(첫 틱)와 시작 직전 조건(cond) 둘 다 본다."""
    warns = []
    if prep.get("fresh") != "1":
        return warns
    eq = [e for e in (m.get("eq") or []) if e]
    if eq != ["RustedSword"]:
        warns.append("meta 장비가 기본 무기 1개가 아님: %s" % (",".join(eq) or "없음"))
    if m.get("skillSpent", 0) != 0:
        warns.append("meta 스킬 투자 %s (0 이어야 함)" % m.get("skillSpent"))
    if m.get("doneQ", 0) != 0:
        warns.append("meta 완료 퀘스트 %s (0 이어야 함)" % m.get("doneQ"))
    if m.get("lv") != 1:
        warns.append("meta 시작 레벨 %s (1 이어야 함)" % m.get("lv"))
    if "skillSpent" not in m:
        warns.append("meta 에 시작 조건 필드(skillSpent·doneQ)가 없음 — 기록기가 옛 판")
    if cond:
        if "eq=-,-,-,-,-,-,-,RustedSword,-,-" not in cond:
            warns.append("시작 직전 장비가 기본 무기 1개가 아님: " + cond)
        if "skillSpent=0 " not in cond + " ":
            warns.append("시작 직전 스킬 투자가 0 이 아님: " + cond)
        if "doneQ=0 " not in cond + " ":
            warns.append("시작 직전 완료 퀘스트가 0 이 아님: " + cond)
    else:
        warns.append("시작 직전 조건(--cond)이 비어 있음")
    return warns


def build(jsonl, status_text, prep=None, cond=""):
    prep = prep or {}
    base = jsonl[:-6]
    name = os.path.basename(base)
    sum_path, bal_path = base + "_summary.md", base + "_balance.md"
    if not os.path.exists(sum_path):
        run_tool("playrec_summary.py", [jsonl])
    if not os.path.exists(bal_path):
        run_tool("playrec_balance.py", [jsonl], bal_path)
    hum_path = os.path.join(ROOT, HUMAN_MD)
    if not os.path.exists(hum_path):
        run_tool("playrec_balance.py", [os.path.join(ROOT, p) for p in HUMAN_JSONL], hum_path)
    summ = read(sum_path)
    bal = read(bal_path)
    hum = read(hum_path) if os.path.exists(hum_path) else []
    errs, warns, deaths, buys, sells, equips, kills, meta = jsonl_digest(jsonl)
    st = parse_status(status_text)
    L = ["# 봇 자동 플레이테스트 보고: " + name, ""]
    for w in start_warnings(meta, cond, prep):
        L.append("**경고: 새 게임 시작 조건 위반 — %s**" % w)
    if any(ln.startswith("**경고") for ln in L):
        L.append("**이 보고서의 레벨·EXP·처치·사망·곡선 수치는 신뢰하지 않는다.**")
        L.append("")

    # 1. 한눈에
    head = summ[:8]
    dur = next((ln for ln in head if ln.startswith("- 총")), "")
    L.append("## 1. 한눈에")
    L.append(dur)
    battle = next((ln for ln in section(summ, "전투 결과") if ln.startswith("- 처치")), "")
    L.append(battle)
    L.append("- 마을 업무(상태 줄): 왕복 %s · 판매 %s · 구매(물약) %s · 장착 %s · 귀환 주문서 %s · 업무 중단 %s · 막힘(stucks) %s · 진척없음 경고 %s · 사망 %s"
             % (st.get("trips", "?"), st.get("sold", "?"), st.get("bought", "?"), st.get("equipped", "?"), st.get("recalls", "?"),
                st.get("errandAborts", "?"), st.get("stucks", "?"), st.get("warns", "?"), st.get("deaths", "?")))
    L.append("- UI 복구(창이 열린 채 ESC 로 닫음): %d회" % sum(n for m, n in warns.items() if "UI 복구" in m))
    L.append("- 퀘스트(상태 줄): 완료 %s · 받음 %s · 보고 %s · 중단 %s · 사냥 맵 %s"
             % (st.get("questsDone", "?"), st.get("questAccepted", "?"), st.get("questReported", "?"), st.get("questAborts", "?"), st.get("huntMap", "?")))
    L.append("- 기록 이벤트: 판매 %d · 장착 %d · 구매 %d · 처치 %d (jsonl ev)" % (sells, equips, buys, kills))
    order, lv0, lv1, gold1 = progress_digest(jsonl)
    L.append("- 진행: Lv %s → %s · 끝 골드 %s · 맵 순서 %s" % (meta.get("lv", lv0), lv1, gold1, " → ".join("%s(%ds)" % (m.replace("Lv1_", "").replace("Lv2_", ""), t) for m, t in order[:30])))
    cs = curve_line(jsonl)
    if cs:
        L.append("- 곡선 비교(사람 11-15 기록 대비, `%s_curve.md`·`_curve.png`): %s" % (name, cs))
    L.append("- 메타(봇 %s, 준비 %s, git %s): %s" % (name, " ".join("%s=%s" % kv for kv in prep.items()) or "없음", meta.get("git", "?"), start_text(meta)))
    if cond:
        L.append("- 시작 직전(봇 시작 프로브): %s" % cond)
    hs = human_start()
    if hs:
        L.append("- 메타(%s)" % hs)
    L.append("")

    # 2. 막힘
    L.append("## 2. 막힘 (위치 · 북마크 PNG)")
    bms = [ln for ln in section(summ, "북마크") if ln.startswith("- #")]
    L.extend(bms[:20] or ["- 없음"])
    L.append("- PNG 는 `Logs/playrec_bot/` 아래 위 경로. 진척 없음·길 없음 경고:")
    for ln in section(summ, "경고")[:60]:
        if "[PB][Bot]" in ln and ("진척" in ln or "경로" in ln or "막힘" in ln or "시간 초과" in ln or "중단" in ln or "닿지" in ln):
            L.append(ln[:230])
    L.append("")

    # 3. 예외·error · 사망
    L.append("## 3. 예외·error 상위")
    if errs:
        for (ty, msg), n in errs.most_common(10):
            L.append("- [%s] x%d %s" % (ty, n, msg))
    else:
        L.append("- Error/Exception 없음")
    L.append("")
    L.append("## 4. 사망")
    if deaths:
        for t, m in deaths:
            L.append("- t=%.0f %s" % (t, m))
    else:
        L.append("- 없음")
    L.append("")

    # 5. 봇 경고 상위
    L.append("## 5. 봇 경고 상위 (숫자 묶음)")
    for msg, n in warns.most_common(10):
        L.append("- x%d %s" % (n, msg))
    if not warns:
        L.append("- 없음")
    L.append("")

    # 6. 사람 대비
    L.append("## 6. 사람 기준값 대비 (사람 = 11-15 기록 2개, `Docs/Handoff/playtest_1115.md`; ▲ 2배 초과 · ▼ 절반 미만)")
    hmap = {r[0]: r for r in table_rows(section(hum, "2."))}
    hlv = table_rows(section(hum, "3."))
    L.append("")
    L.append("| 맵·낮밤 | 처치/분 봇(사람) | EXP/분 봇(사람) | 받은피해/분 봇(사람) | 표시 |")
    L.append("|---|---|---|---|---|")
    flagged = 0
    for r in table_rows(section(bal, "2.")):
        h = hmap.get(r[0])
        if h is None:
            L.append("| %s (분 %s) | %s (-) | %s (-) | %s (-) | 사람 기록 없음 |" % (r[0], r[1], r[3], r[4], r[6]))
            continue
        fl = []
        for idx, nm in ((3, "처치/분"), (4, "EXP/분"), (6, "받은피해/분")):
            f = ratio_flag(num(r[idx]), num(h[idx]))
            if f:
                fl.append(nm + f)
        flagged += bool(fl)
        L.append("| %s (분 %s) | %s (%s) | %s (%s) | %s (%s) | %s |" % (r[0], r[1], r[3], h[3], r[4], h[4], r[6], h[6], " ".join(fl)))
    L.append("")
    L.append("몹별(사람 n 5 이상만): 처치시간 중앙·피해/처치")
    L.append("")
    L.append("| 몹 | n | 처치시간 봇(사람) | 피해/처치 봇(사람) | 표시 |")
    L.append("|---|---|---|---|---|")
    hmob = {r[0]: r for r in table_rows(section(hum, "1."))}
    for r in table_rows(section(bal, "1.")):
        h = hmob.get(r[0])
        if h is None or (num(h[3]) or 0) < 5:
            continue
        fl = []
        for idx, nm in ((5, "처치시간"), (7, "피해/처치")):
            f = ratio_flag(num(r[idx]), num(h[idx]))
            if f:
                fl.append(nm + f)
        L.append("| %s | %s | %s (%s) | %s (%s) | %s |" % (r[0], r[3], r[5].split(" ")[0], h[5].split(" ")[0], r[7], h[7], " ".join(fl)))
    L.append("")
    L.append("레벨 구간(사람, 참고): " + " · ".join("%s %s처치/분 %sEXP/분" % (r[0], r[3], r[4]) for r in hlv))
    L.append("")

    # 7. 밸런스 표 원문
    L.append("## 7. 밸런스 표 (맵·몹별 처치시간·피해·EXP·드롭/분)")
    L.append("원문 `" + os.path.basename(bal_path) + "`. 몹별:")
    L.append("")
    L.extend(ln for ln in section(bal, "1.") if ln.startswith("|"))
    L.append("")
    L.append("맵·낮밤별:")
    L.append("")
    L.extend(ln for ln in section(bal, "2.") if ln.startswith("|"))
    L.append("")
    L.append("## 8. 후속")
    L.append("- 요약 전체 `%s_summary.md`, 밸런스 전체 `%s_balance.md`. 사람 기준과 2배 넘게 다른 칸 %d개." % (name, name, flagged))
    if len(L) > MAX_LINES:
        L = L[:MAX_LINES - 1] + ["- (300줄 초과로 잘림)"]
    return name, L


def main(argv):
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    jsonl, status, out, i = None, "", None, 1
    prep, cond = {}, ""
    while i < len(argv):
        if argv[i] == "--status":
            status, i = argv[i + 1], i + 2
        elif argv[i] == "--prep":
            prep, i = dict(kv.split("=", 1) for kv in argv[i + 1].split() if "=" in kv), i + 2
        elif argv[i] == "--cond":
            cond, i = argv[i + 1], i + 2
        elif argv[i] == "-o":
            out, i = argv[i + 1], i + 2
        else:
            jsonl, i = argv[i], i + 1
    if not jsonl or not jsonl.endswith(".jsonl"):
        print(__doc__)
        return 2
    name, lines = build(jsonl, status, prep, cond)
    out = out or jsonl[:-6] + "_report.md"
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    print("wrote %s (%d lines)" % (out, len(lines)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
