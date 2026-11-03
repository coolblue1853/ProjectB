import sys
sys.stdout.reconfigure(encoding="utf-8")
# PT-BOT 보고서 생성: Logs/ptbot/{summary.txt,run.log,console_errors.json,config.txt} -> Markdown (stdout)
import sys, json, re, os, collections
d = sys.argv[1]
def rd(n):
    p = os.path.join(d, n)
    return open(p, encoding="utf-8", errors="replace").read() if os.path.exists(p) else ""
sm = rd("summary.txt").splitlines(); log = rd("run.log").splitlines(); cfg = rd("config.txt").splitlines()
print("# PtBot 런 보고서")
print("\n## 설정\n`" + " ".join(sorted(cfg)) + "`")
if not sm: print("\n**summary 없음 — 봇이 끝나지 않았다.**")
print("\n## 요약")
for l in sm:
    if not l.startswith(("ZONE ", "ERR ")): print("- " + l)
print("\n## 구역별\n| 구역 | 게임초 | 처치 | 정예 | 보스 | 첫 처치(초) | 사망 | 막힘 | 물약 | 대화 | 레벨 | EXP | 골드 |\n|---|---|---|---|---|---|---|---|---|---|---|---|---|")
for l in sm:
    if l.startswith("ZONE "):
        name, rest = l[5:].split(" | ", 1)
        kv = dict(re.findall(r"(\w+)=(\S+)", rest))
        g = lambda k: kv.get(k, "-")
        print("| %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s |" % (name, g("gameSec"), g("kills"), g("elite"), g("boss"), g("firstKillSec"), g("deaths"), g("stucks"), g("potions"), g("talks"), g("lv"), g("exp"), g("gold")))
ev = [l for l in log if re.search(r"\] (DEATH|STUCK|UNSTUCK_TP|WAYPOINT_TP|NO_POTION|CONSOLE_ERROR|TALK_|SKILL_ERR)", l)]
print("\n## 이벤트 (사망·막힘·에러, 최대 40줄)")
for l in ev[:40]: print("- " + l[:240])
if not ev: print("- 없음")
errs = [l for l in sm if l.startswith("ERR ")]
print("\n## 콘솔 에러 (봇 수집)")
for l in errs: print("- " + l[4:])
if not errs: print("- 없음")
try:
    items = json.loads(rd("console_errors.json") or "{}").get("data") or []
    c = collections.Counter(i.strip().split("\n")[0][:200] for i in items if isinstance(i, str))
    print("\n## 에디터 콘솔 에러 (런 종료 시점)\n- 합계 %d (종류 %d)" % (len(items), len(c)))
    for k, v in c.most_common(10): print("  - %dx %s" % (v, k))
except Exception as e: print("\n콘솔 파싱 실패", e)
