#!/usr/bin/env python
"""PlayRecorder jsonl(여러 개) -> 밸런스 조사 markdown (표준 라이브러리만).

사용: python Tools/playrec_balance.py <a.jsonl> [b.jsonl ...] [-o Docs/Balance/PlayBalance_1115.md]
기본 출력: Docs/Balance/PlayBalance_<날짜>.md. 출력 파일에 `<!-- ANALYSIS -->` 마커가 있으면
마커 아래(손으로 쓴 분석·수정안)는 보존하고 위쪽 자동 표만 다시 쓴다.

기록 형식은 .claude-rules/Game/PlayBotDesign.md. 처치·EXP·골드·드롭은 log(Player/Item/Combat) 미러에서 읽는다
(같은 시각의 `Player EXP +N (몹)` 줄 = 처치 1건, 같은 시각 `Item 드롭 이름 (확률 N%)`·`골드 드롭 NG (몹, LvN)` 줄 = 그 처치의 드롭).
한글 로그 문구는 기록 단계에서 깨져 있으므로(U+FFFD) 영문 토큰·숫자만 정규식에 쓴다.
낮/밤은 cx 행의 night 필드(1초 주기 carry-forward)다. 없으면 '미기록'.
"""
import bisect
import csv
import io
import json
import os
import re
import sys
from collections import Counter, defaultdict

MARKER = "<!-- ANALYSIS -->"
MAX_DT = 0.5          # st 간격이 이보다 크면 일시정지로 보고 체류 시간에 넣지 않는다
SAME_T = 0.06         # 같은 시각으로 보는 오차(초)
CHAIN_IDLE = 30.0     # 이 시간 동안 안 맞은 몹 인스턴스는 사슬에서 버린다
HIT_RESULT = "hit"
RE_EXP = re.compile(r"EXP \+(\d+) \((\w+?)\(Clone\)\)")  # 몹 처치만(퀘스트 EXP 는 (Q101) 형태)
RE_DROP = re.compile(r"^\S+ (.+?) \(\S+ ([\d.]+)%\)\s*$")
RE_GOLD = re.compile(r"(\d+)G x(\d+) \((\w+), Lv(\d+)\)")
RE_HIT = re.compile(r"^(\w+?)\(Clone\) \S+ (\d+)(?: \+\S+ (\d+))? .*?HP (\d+)[^\d-]+(-?\d+)\s*$")
RE_SURE = re.compile(r"^[^\x00-\x7f]\S* \S+ ([A-Za-z0-9_' \-]+?)\s*$")  # 확정 드롭(정예·채집물): 확률 표기 없음


def load(path):
    rows = []
    with open(path, encoding="utf-8", errors="replace") as f:
        for line in f:
            line = line.strip()
            if line:
                try:
                    rows.append(json.loads(line))
                except ValueError:
                    pass  # 비정상 종료로 잘린 줄
    return rows


# ---------- 순수 파서 ----------
def parse_exp(msg):
    m = RE_EXP.search(msg)
    return (int(m.group(1)), m.group(2)) if m else None


def parse_drop(msg):
    if "%" not in msg:
        return None
    m = RE_DROP.match(msg)
    return (m.group(1), float(m.group(2))) if m else None


def parse_gold(msg):
    m = RE_GOLD.search(msg)
    return (int(m.group(1)) * int(m.group(2)), m.group(3), int(m.group(4))) if m else None


def parse_sure(msg):
    """'확정 드롭 이름' 줄(영문 이름만). 획득 로그 'Wood x1' 은 제외."""
    m = RE_SURE.match(msg)
    if not m or re.search(r" x\d+$", m.group(1)):
        return None
    return m.group(1)


def parse_hit(msg):
    """(몹, HP전, HP후). 로그의 HP후는 기본 피해만 뺀 값이고 '+추가' 피해는 그 뒤에 빠지므로 추가분을 더 뺀다."""
    m = RE_HIT.match(msg)
    if not m:
        return None
    return m.group(1), int(m.group(4)), int(m.group(5)) - int(m.group(3) or 0)


def track_hits(hits):
    """[(t, 몹, HP전, HP후)] -> 닫힌(처치된) 인스턴스 [{mob,t0,t1,hits,dmg,hp}].
    HP전 == 어떤 열린 인스턴스의 마지막 HP후 인 것에 이어 붙이고(가장 최근), 없으면 새 인스턴스로 본다."""
    open_by_mob = defaultdict(list)
    closed = []
    # 몹별 풀피 기준 = 사슬 시작 HP 의 최빈값(정예·스케일 HP 는 소수라 무시, 근사). 1차로 시작 HP 를 모은다.
    starts = defaultdict(Counter)
    probe = defaultdict(list)
    for t, mob, before, after in hits:
        lst = probe[mob]
        cur = next((i for i in reversed(lst) if i[0] == before and t - i[1] <= CHAIN_IDLE), None)
        if cur is None:
            starts[mob][before] += 1
            cur = [after, t]
            lst.append(cur)
        cur[0], cur[1] = after, t
        if after <= 0:
            lst.remove(cur)
    top = {m: c.most_common(1)[0][0] for m, c in starts.items()}
    for t, mob, before, after in hits:
        lst = open_by_mob[mob]
        lst[:] = [i for i in lst if t - i["last"] <= CHAIN_IDLE]
        inst = None
        for i in reversed(lst):
            if i["hp_now"] == before:
                inst = i
                break
        if inst is None:
            inst = {"mob": mob, "t0": t, "hits": 0, "dmg": 0, "hp": before, "full": before >= top[mob] * 0.95}
            lst.append(inst)
        inst["hits"] += 1
        inst["dmg"] += before - max(after, 0)
        inst["hp_now"] = after
        inst["last"] = t
        if after <= 0:
            lst.remove(inst)
            closed.append({"mob": mob, "t0": inst["t0"], "t1": t, "hits": inst["hits"], "dmg": inst["dmg"],
                           "hp": inst["hp"], "full": inst["full"]})
    return closed


def pct(values, p):
    if not values:
        return None
    s = sorted(values)
    k = (len(s) - 1) * p / 100.0
    lo = int(k)
    hi = min(lo + 1, len(s) - 1)
    return s[lo] + (s[hi] - s[lo]) * (k - lo)


def mean(values):
    return sum(values) / len(values) if values else None


def per_min(count, minutes):
    return count / minutes if minutes and minutes > 0 else None


def lv_bin(lv):
    lo = ((max(lv, 1) - 1) // 2) * 2 + 1
    return "Lv%d-%d" % (lo, lo + 1)


class Timeline:
    """(t, 값) 목록에서 시각 t 의 직전 값을 돌려준다(carry-forward)."""

    def __init__(self, pairs, default=None):
        pairs = sorted(pairs, key=lambda p: p[0])
        self.ts = [p[0] for p in pairs]
        self.vs = [p[1] for p in pairs]
        self.default = default

    def at(self, t):
        i = bisect.bisect_right(self.ts, t) - 1
        return self.vs[i] if i >= 0 else (self.vs[0] if self.vs else self.default)


def build_session(rows):
    """한 세션 -> dict(kills, ttks, hurts, dwell, picks, equips, sells, deaths, has_night)."""
    st = [r for r in rows if r.get("k") == "st" and r.get("pl", 1) != 0 and "map" in r]
    cx = [r for r in rows if r.get("k") == "cx" and "night" in r]
    has_night = bool(cx)
    map_tl = Timeline([(r["t"], r["map"]) for r in st], "")
    lv_tl = Timeline([(r["t"], r.get("lv", 0)) for r in st], 0)
    night_tl = Timeline([(r["t"], r["night"]) for r in cx], None)
    # 체류 시간(UI 열림 제외)
    dwell = defaultdict(float)
    for a, b in zip(st, st[1:]):
        dt = b["t"] - a["t"]
        if 0 < dt <= MAX_DT and not a.get("ui"):
            dwell[(a["map"], night_tl.at(a["t"]), lv_bin(a.get("lv", 0)))] += dt
    # 처치·드롭
    kills, hits = [], []
    for r in rows:
        if r.get("k") != "log":
            continue
        cat, msg, t = r.get("cat"), r.get("msg", ""), r["t"]
        if cat == "Player":
            e = parse_exp(msg)
            if e:
                kills.append({"t": t, "mob": e[1], "exp": e[0], "gold": 0, "items": [], "sure": [], "prof": None,
                              "map": map_tl.at(t), "night": night_tl.at(t), "lv": lv_tl.at(t), "ttk": None})
        elif cat == "Item" and kills and t - kills[-1]["t"] <= SAME_T:
            d = parse_drop(msg)
            if d:
                kills[-1]["items"].append(d)
                continue
            g = parse_gold(msg)
            if g:
                kills[-1]["gold"] += g[0]
                kills[-1]["prof"] = (g[1], g[2])  # 스포너 profile 이 몹 프리팹과 다를 수 있다(예: SporeShroom -> ForestSpore)
                continue
            sure = parse_sure(msg)
            if sure:
                kills[-1]["sure"].append(sure)
        elif cat == "Combat":
            h = parse_hit(msg)
            if h:
                hits.append((t, h[0], h[1], h[2]))
    ttks = track_hits(hits)
    for c in ttks:
        c["map"], c["night"], c["lv"] = map_tl.at(c["t1"]), night_tl.at(c["t1"]), lv_tl.at(c["t1"])
        for k in reversed(kills):
            if c["t1"] - k["t"] > 0.1:
                break
            if k["mob"] == c["mob"] and k["ttk"] is None and abs(k["t"] - c["t1"]) <= 0.1:
                if c["full"]:
                    k["ttk"] = (c["t1"] - c["t0"], c["hits"], c["hp"])
                break
    hurts = [{"t": r["t"], "mob": r.get("a", "?"), "v": r.get("v", 0), "res": r.get("r", ""),
              "map": map_tl.at(r["t"]), "night": night_tl.at(r["t"]), "lv": lv_tl.at(r["t"])}
             for r in rows if r.get("k") == "hitat"]
    picks, equips = Counter(), Counter()
    sells = defaultdict(lambda: [0, 0])  # 이름 -> [판 횟수, 받은 골드]
    sell_dec = [r for r in rows if r.get("k") == "dec" and r.get("d") == "sell"]
    for r in rows:
        if r.get("k") != "ev":
            continue
        e, a = r.get("e"), r.get("a", "")
        if e == "pick" and "�" not in a and ":" not in a:
            picks[a] += 1
        elif e == "equip":
            equips[a] += 1
        elif e == "sell":
            sells[a][0] += r.get("v", 1) or 1
    for r in sell_dec:
        sells[r.get("a", "?")][1] += r.get("g1", 0) - r.get("g0", 0)
    deaths = [r["t"] for r in rows if r.get("k") == "ev" and r.get("e") == "death"]
    lvups = [(r["t"], r.get("v", 0)) for r in rows if r.get("k") == "ev" and r.get("e") == "lvup"]
    for k in kills:
        k["sess_lv"] = None
    return {"lvups": lvups, "kills": kills, "ttks": ttks, "hurts": hurts, "dwell": dwell, "picks": picks, "equips": equips,
            "sells": sells, "deaths": [(t, map_tl.at(t)) for t in deaths], "has_night": has_night}


def level_spans(lvups, kills):
    """연속한 레벨업 사이 [(레벨, 처치 수, 걸린 초)] (한 세션 안에서 시작·끝이 다 보이는 레벨만)."""
    out = []
    for (t0, v0), (t1, v1) in zip(lvups, lvups[1:]):
        if v1 == v0 + 1:
            out.append((v0, sum(1 for k in kills if t0 < k["t"] <= t1), t1 - t0))
    return out


def merge(sessions):
    out = {"lvup_spans": [], "kills": [], "ttks": [], "hurts": [], "dwell": defaultdict(float), "picks": Counter(),
           "equips": Counter(), "sells": defaultdict(lambda: [0, 0]), "deaths": [], "has_night": True}
    for s in sessions:
        out["lvup_spans"] += level_spans(s["lvups"], s["kills"])
        for k in ("kills", "ttks", "hurts", "deaths"):
            out[k] += s[k]
        for k, v in s["dwell"].items():
            out["dwell"][k] += v
        out["picks"].update(s["picks"])
        out["equips"].update(s["equips"])
        for k, v in s["sells"].items():
            out["sells"][k][0] += v[0]
            out["sells"][k][1] += v[1]
        out["has_night"] = out["has_night"] and s["has_night"]
    return out


# ---------- CSV ----------
def read_csv(path, key=None):
    """CSV -> dict 목록. key 가 있으면 그 칼럼명이 든 행을 헤더로 본다(ItemSheet 는 앞에 타입 행이 있다).
    ItemSheet 는 레코드 중간에 단독 CR 이 섞여 있어 csv 가 행을 쪼개므로 먼저 지운다."""
    if not os.path.exists(path):
        return []
    with open(path, encoding="utf-8-sig", newline="") as f:
        text = re.sub(r"\r(?!\n)", "", f.read())
    rd = csv.reader(io.StringIO(text, newline=""))
    head = next(rd, None)
    while key and head is not None and key not in head:
        head = next(rd, None)
    return [dict(zip(head, r)) for r in rd if r] if head else []


def load_monsters(root):
    return {r["id"]: r for r in read_csv(os.path.join(root, "Assets/Resources/MonsterSheet.csv"))}


def load_items(root):
    items = {}
    for r in read_csv(os.path.join(root, "Assets/Resources/ItemSheet.csv"), key="name"):
        info = {"rar": r.get("rarity", "?"), "type": r.get("type", ""), "cat": r.get("category", ""),
                "price": r.get("price", ""), "equip": r.get("type") == "Equip"}
        items[r["name"]] = info
        if r.get("itemNameT"):
            items.setdefault(r["itemNameT"], info)
    return items


def load_maple(root):
    """레벨 -> (HP 중앙값, EXP 중앙값). 보스·EXP 0·HP 1e6 초과 제외, 레벨 ±1 창."""
    rows = read_csv(os.path.join(root, "Docs/Reference/MapleMobs/mobs_summary.csv"))
    pool = defaultdict(list)
    for r in rows:
        try:
            lv, hp, ex = int(r["level"]), int(r["hp"]), int(r["exp"])
        except (ValueError, KeyError):
            continue
        if r.get("boss") not in ("0", "False", "false", "", None) or ex <= 0 or hp > 1000000:
            continue
        pool[lv].append((hp, ex))
    res = {}
    for lv in range(1, 21):
        win = [p for l in (lv - 1, lv, lv + 1) for p in pool.get(l, [])]
        if win:
            res[lv] = (pct([p[0] for p in win], 50), pct([p[1] for p in win], 50), len(win))
    return res


def flag(ratio, lo=0.5, hi=2.0):
    if ratio is None:
        return ""
    return "▲" if ratio > hi else "▼" if ratio < lo else ""


def expected_gold(level, trait=None):
    """GoldDropRules: 일반 몹 40% x (레벨 x 4 + 2) 평균. 정예(스폰 승격)·보스 미반영."""
    return 0.4 * (level * 4 + 2)


def f1(v, nd=1):
    return "-" if v is None else ("%." + str(nd) + "f") % v


def md_table(head, rows):
    out = ["| " + " | ".join(head) + " |", "|" + "|".join(["---"] * len(head)) + "|"]
    out += ["| " + " | ".join(str(c) for c in r) + " |" for r in rows]
    return out


def night_label(n):
    return "미기록" if n is None else ("밤" if n else "낮")


# ---------- 보고서 ----------
def mark_elite(kills):
    """같은 몹의 최빈 EXP 대비 2배 이상 받은 처치 = 정예(스폰 승격)로 본다. kill['elite'] 를 채운다."""
    by = defaultdict(list)
    for k in kills:
        by[k["mob"]].append(k["exp"])
    base = {m: Counter(v).most_common(1)[0][0] for m, v in by.items()}
    for k in kills:
        k["elite"] = k["exp"] >= 2 * base[k["mob"]]
    return base


def resolve_row(mob, kills, monsters):
    """몹 프리팹 id 의 MonsterSheet 행. 골드 줄의 profile(스포너 매핑)이 다르면 그 행을 쓴다."""
    profs = Counter(k["prof"][0] for k in kills if k["prof"])
    prof = profs.most_common(1)[0][0] if profs else mob
    return prof, monsters.get(prof) or monsters.get(mob) or {}


def mob_rows(data, monsters, items):
    kills = data["kills"]
    mark_elite(kills)
    by = defaultdict(list)
    for k in kills:
        by[k["mob"]].append(k)
    hurt_by = defaultdict(list)
    for h in data["hurts"]:
        hurt_by[h["mob"]].append(h)
    rows = []
    for mob, ks in sorted(by.items(), key=lambda kv: -len(kv[1])):
        prof, m = resolve_row(mob, ks, monsters)
        n = len(ks)
        norm = [k for k in ks if not k["elite"]]
        ttk = [k["ttk"] for k in norm if k["ttk"]]
        hurts = [h for h in hurt_by.get(mob, []) if h["res"] == HIT_RESULT]
        night = Counter(k["night"] for k in ks)
        n_night = night.get(1, 0)
        nl = "미기록" if None in night else ("밤" if n_night == n else "낮" if n_night == 0 else "혼재 %d/%d" % (n_night, n))
        lv = int(m["level"]) if m.get("level") else 0
        gold_hit = sum(1 for k in norm if k["gold"] > 0)
        nn = max(len(norm), 1)
        items_n = sum(len(k["items"]) for k in norm)
        rare = sum(1 for k in norm for d, _ in k["items"] if items.get(d, {}).get("rar", "C") not in ("C", "?"))
        name = mob.replace("_BD", "") + ("" if prof == mob else "→" + prof.replace("_BD", ""))
        rows.append([name, lv or "-", nl, "%d(정예%d)" % (n, n - len(norm)),
                     "%s/%s" % (m.get("maxHP", "-"), f1(mean([t[2] for t in ttk]), 0)),
                     "%ss/%s타 (n%d)" % (f1(pct([t[0] for t in ttk], 50)), f1(pct([t[1] for t in ttk], 50), 0), len(ttk)) if ttk else "-",
                     f1(len(hurts) / n, 2), f1(sum(h["v"] for h in hurts) / n, 1),
                     "%s/%s" % (m.get("exp", "-"), f1(mean([k["exp"] for k in norm]), 1)),
                     "%s/%s" % (f1(mean([k["gold"] for k in norm]), 1), f1(expected_gold(lv, m.get("trait")), 1) if lv else "-"),
                     "%d%%" % round(100.0 * gold_hit / nn), f1(items_n / nn, 2), rare])
    return rows


def elite_rows(data, items):
    """일반 처치 대 정예(승격) 처치의 보상·드롭 비교 + 등급별 드롭 줄 수."""
    rows = []
    for label, flt in (("일반", False), ("정예", True)):
        ks = [k for k in data["kills"] if k["elite"] == flt]
        n = max(len(ks), 1)
        every = [d for k in ks for d in [x for x, _ in k["items"]] + k["sure"]]
        grade = Counter(items.get(d, {}).get("rar", "?") for d in every)
        junk = sum(1 for d in every if "Junk" in items.get(d, {}).get("cat", ""))
        rows.append([label, len(ks), f1(sum(k["exp"] for k in ks) / n, 1), f1(sum(k["gold"] for k in ks) / n, 1),
                     f1(sum(len(k["items"]) for k in ks) / n, 2), f1(sum(len(k["sure"]) for k in ks) / n, 2),
                     f1(sum(1 for d in every if items.get(d, {}).get("equip")) / n, 2),
                     " ".join("%s%d" % (g, c) for g, c in sorted(grade.items())), "%d%%" % round(100.0 * junk / max(len(every), 1))])
    return rows


def source_rows(data, items, limit=8):
    """(몹, 아이템)별 확률 드롭: 표시 확률(로그) 대 실측 비율. 정예 처치는 제외."""
    kills_of = Counter(k["mob"] for k in data["kills"] if not k["elite"])
    cnt, prob = Counter(), {}
    for k in data["kills"]:
        if k["elite"]:
            continue
        for d, pr in k["items"]:
            cnt[(k["mob"], d)] += 1
            prob[(k["mob"], d)] = pr
    rows = []
    for (mob, d), c in sorted(cnt.items(), key=lambda kv: -kv[1])[:limit]:
        n = kills_of[mob]
        obs = 100.0 * c / n
        rows.append([mob.replace("_BD", ""), d, items.get(d, {}).get("rar", "?"), n, c, "%.1f%%" % prob[(mob, d)],
                     "%.1f%%" % obs, "%.1f" % (obs / prob[(mob, d)]) if prob[(mob, d)] else "-"])
    return rows


def group_rates(data):
    """(map, night) 별 지표 + 레벨 구간별 지표."""
    def blank():
        return {"min": 0.0, "kills": 0, "exp": 0, "gold": 0, "dmg": 0, "hits": 0, "drops": 0}

    seg, lvl = defaultdict(blank), defaultdict(blank)
    for (mp, night, lb), sec in data["dwell"].items():
        seg[(mp, night)]["min"] += sec / 60.0
        lvl[lb]["min"] += sec / 60.0
    for k in data["kills"]:
        for g, key in ((seg, (k["map"], k["night"])), (lvl, lv_bin(k["lv"]))):
            g[key]["kills"] += 1
            g[key]["exp"] += k["exp"]
            g[key]["gold"] += k["gold"]
            g[key]["drops"] += len(k["items"])
    for h in data["hurts"]:
        if h["res"] != HIT_RESULT:
            continue
        for g, key in ((seg, (h["map"], h["night"])), (lvl, lv_bin(h["lv"]))):
            g[key]["dmg"] += h["v"]
            g[key]["hits"] += 1
    return seg, lvl


def rate_rows(groups, label_fn, min_minutes=0.5):
    groups = {k: g for k, g in groups.items() if g["kills"] > 0}
    rows = []
    for key in sorted(groups, key=lambda k: str(k)):
        g = groups[key]
        if g["min"] < min_minutes and g["kills"] == 0:
            continue
        m = g["min"]
        rows.append([label_fn(key), f1(m), g["kills"], f1(per_min(g["kills"], m)), f1(per_min(g["exp"], m), 0),
                     f1(per_min(g["gold"], m), 0), f1(per_min(g["dmg"], m), 0), f1(per_min(g["hits"], m)),
                     f1(g["drops"] / g["kills"], 2) if g["kills"] else "-"])
    return rows


def item_rows(data, items, limit=10):
    drops = Counter()
    for k in data["kills"]:
        for d in [d for d, _ in k["items"]] + k["sure"]:
            drops[d] += 1
    names = set(data["picks"]) | set(data["sells"]) | set(data["equips"])
    rows = []
    for n in sorted(names, key=lambda x: -(data["picks"][x] + data["sells"][x][0])):
        info = items.get(n, {})
        p, s, e = data["picks"][n], data["sells"][n][0], data["equips"][n]
        junk = "Junk" in info.get("cat", "")
        if info.get("equip"):
            verdict = "쓸모없음 후보(판매만)" if s > 0 and e == 0 else ("착용" if e else "")
        else:
            verdict = "잡템" if junk else ""
        rows.append([n, info.get("rar", "미등록"), info.get("type", "-"), p, drops.get(n, 0), s, e, verdict, info.get("equip", False)])
    gear = [r[:-1] for r in rows if r[-1]]
    other = [r[:-1] for r in rows if not r[-1]]
    return other[:limit // 2] + gear[:limit - limit // 2]


def baseline_rows(monsters, maple, kills):
    by = defaultdict(list)
    for k in kills:
        by[k["mob"]].append(k)
    rows = []
    for mob, ks in sorted(by.items()):
        prof, m = resolve_row(mob, ks, monsters)
        if not m.get("level") or m.get("isBoss") == "1":
            continue
        lv, hp, ex = int(m["level"]), float(m["maxHP"] or 0), float(m["exp"] or 0)
        mp = maple.get(lv)
        if not mp or not hp:
            continue
        hr, er = hp / mp[0], ex / mp[1]
        rows.append([prof.replace("_BD", ""), lv, "%d/%d" % (hp, mp[0]), "%d/%d" % (ex, mp[1]),
                     "%.2f%s" % (hr, flag(hr)), "%.2f%s" % (er, flag(er)),
                     "%.2f/%.2f" % (ex / hp, mp[1] / mp[0]), mp[2]])
    return rows


def night_ratio_rows(monsters):
    """같은 층계 일반 몹에서 밤 전용(zone 에 (night)) 대 낮 몹의 레벨별 중앙값 비."""
    day, night = defaultdict(list), defaultdict(list)
    for m in monsters.values():
        z = m.get("zone", "")
        if not m.get("level") or m.get("isBoss") == "1" or "boss" in z or "miniboss" in z or not m.get("maxHP"):
            continue
        (night if "(night)" in z else day)[int(m["level"])].append(m)
    rows = []
    for lv in sorted(night):
        d = [x for l in (lv - 1, lv, lv + 1) for x in day.get(l, [])]
        if not d:
            continue
        r = [lv, ", ".join(x["id"].replace("_BD", "") for x in night[lv])]
        for fld in ("maxHP", "contactDamage", "exp"):
            nv = pct([float(x[fld]) for x in night[lv] if x.get(fld)], 50)
            dv = pct([float(x[fld]) for x in d if x.get(fld)], 50)
            r.append("%s/%s=%s" % (f1(nv, 0), f1(dv, 0), f1(nv / dv, 2) if dv else "-"))
        r.append(len(d))
        rows.append(r)
    return rows


def render(data, inputs, monsters, items, maple):
    kills = data["kills"]
    total_min = sum(data["dwell"].values()) / 60.0
    out = ["# 플레이 기록 기반 밸런스 조사", "",
           "**한계: 사람 첫 판(Lv1→8) 기록 %d개(표본 작음, 표마다 n 표기). 몹 종·맵당 n 이 10 미만이면 방향만 본다. 수치는 사람 플레이 1회 평균이며 봇 반복으로 확장 예정.**" % len(inputs), "",
           "- 입력: " + ", ".join(os.path.basename(p) for p in inputs),
           "- 처치 n=%d, 전투 체류 %.1f분(UI 열림·일시정지 제외), 받은 피격 %d회, 사망 %d회, 낮/밤 %s" % (
               len(kills), total_min, sum(1 for h in data["hurts"] if h["res"] == HIT_RESULT), len(data["deaths"]),
               "cx.night 사용" if data["has_night"] else "미기록(PlayBotDesign 에 R2 추가 필요)"),
           "- 도구: `python Tools/playrec_balance.py <jsonl...>`. 처치=log `EXP +N (몹)`, 드롭=같은 시각 `Item 드롭` 줄, 처치 시간=Combat `HP a→b` 사슬(몹 인스턴스 id 가 없어 HP 연속성으로 이음, 근사).",
           "", "## 1. 몹별 (CSV값/실측)", ""]
    out += md_table(["몹", "Lv", "낮밤", "n", "HP(CSV/관측)", "처치시간 중앙(n)", "피격/처치", "피해/처치",
                     "EXP(CSV/실측)", "골드 실측/기대", "골드드롭률", "아이템/처치", "희귀+"], mob_rows(data, monsters, items))
    seg, lvl = group_rates(data)
    head = ["구분", "분", "처치", "처치/분", "EXP/분", "골드/분", "받은피해/분", "피격/분", "드롭/처치"]
    out += ["", "## 1b. 일반 대 정예 처치(스폰 승격: EXP 2배 이상으로 판별)", ""]
    out += md_table(["구분", "n", "EXP/처치", "골드/처치", "확률드롭/처치", "확정드롭/처치", "장비 드롭/처치", "등급별 드롭 줄", "잡템%"], elite_rows(data, items))
    out += ["", "## 1c. 확률 드롭 출처: 표시 확률 대 실측(일반 처치, 상위 8)", ""]
    out += md_table(["몹", "아이템", "등급", "몹 처치n", "드롭n", "표시확률", "실측", "배율"], source_rows(data, items))
    out += ["", "## 2. 맵·낮밤별", ""]
    out += md_table(head, rate_rows(seg, lambda k: "%s %s" % (k[0].replace("Lv1_", ""), night_label(k[1]))))
    out += ["", "## 3. 레벨 구간별", ""]
    out += md_table(head, rate_rows(lvl, lambda k: k))
    spans = defaultdict(list)
    for lv, n, sec in data["lvup_spans"]:
        spans[lv].append((n, sec))
    out += ["", "- 레벨당 처치·시간(완전 관측 레벨, 퀘스트 EXP 포함): " + " / ".join(
        "Lv%d %d킬 %d:%02d" % (lv, v[0][0], v[0][1] // 60, v[0][1] % 60) for lv, v in sorted(spans.items()))]
    out += ["", "## 4. 아이템 (주움은 상점 구매·보상 포함, 드롭=처치 드롭 줄)", ""]
    out += md_table(["아이템", "등급", "종류", "주움", "드롭", "판매", "장착", "판정"], item_rows(data, items))
    out += ["", "## 5. 메이플 초반 기준선 대비 (관측 몹, HP/EXP = 우리/메이플 레벨±1 중앙값; ▲▼ = 2배 초과·절반 미만)", ""]
    out += md_table(["몹", "Lv", "HP", "EXP", "HP비", "EXP비", "EXP/HP", "메이플n"], baseline_rows(monsters, maple, kills))
    out += ["", "## 6. 밤 전용 몹 대 낮 몹 (CSV, 레벨±1 중앙값 우리 밤/낮=배율)", ""]
    out += md_table(["Lv", "밤 몹", "HP", "공격(접촉)", "EXP", "낮 n"], night_ratio_rows(monsters)[:6])
    return out


def keep_analysis(path):
    if not os.path.exists(path):
        return []
    with open(path, encoding="utf-8") as f:
        text = f.read()
    i = text.find(MARKER)
    return text[i:].splitlines() if i >= 0 else []


def main(argv):
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
    sessions = [build_session(load(p)) for p in paths]
    data = merge(sessions)
    lines = render(data, paths, load_monsters(root), load_items(root), load_maple(root))
    out_path = out_path or os.path.join(root, "Docs", "Balance", "PlayBalance_1115.md")
    tail = keep_analysis(out_path) or [MARKER, "", "## 분석·수정안", "", "(손으로 작성)"]
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines + [""] + tail) + "\n")
    print("wrote %s (%d lines)" % (out_path, len(lines) + len(tail) + 1))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
