"""maple_maps_dump.py 의 build 2단계: 퀘스트·몹·NPC·아이템 요약. 캐시(Tools/cache/maple_maps/)만 읽는다."""
import collections, csv, json, os, re, statistics

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CACHE = os.path.join(ROOT, "Tools", "cache", "maple_maps")
REF = os.path.join(ROOT, "Docs", "Reference")


def load(p):
    with open(p, encoding="utf-8") as f:
        return json.load(f)


def loaddir(name):
    d = os.path.join(CACHE, name)
    out = {}
    if os.path.isdir(d):
        for fn in os.listdir(d):
            try:
                x = load(os.path.join(d, fn))
                out[int(fn[:-5])] = x
            except Exception:  # noqa
                pass
    return out


def w_csv(path, cols, rows):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f)
        w.writerow(cols)
        for r in rows:
            w.writerow([r.get(c, "") for c in cols])


def w_md(path, lines):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")


def med(v):
    v = [x for x in v if x is not None]
    return statistics.median(v) if v else 0


def p90(v):
    v = sorted(v)
    return v[int(len(v) * 0.9)] if v else 0


def norm(s):
    return re.sub(r"\s+", " ", (s or "").strip())


def names(lst_en, lst_ko):
    en = {x["id"]: x.get("name", "") for x in lst_en}
    ko = {x["id"]: x.get("name", "") for x in lst_ko}
    return en, ko


# ---------- NPC·아이템 목록 ----------

def build_lists(npc_en, npc_ko):
    L = os.path.join(CACHE, "lists")
    rows = [{"id": i, "name": n, "name_ko": npc_ko.get(i, "")} for i, n in sorted(npc_en.items())]
    w_csv(os.path.join(REF, "MapleMaps", "npcs_list.csv"), ["id", "name", "name_ko"], rows)
    it = load(os.path.join(L, "item.json"))
    kit = {x["id"]: x.get("name", "") for x in load(os.path.join(L, "kitem.json"))}
    rows = []
    for x in it:
        t = x.get("typeInfo") or {}
        rows.append({"id": x["id"], "name": x.get("name", ""), "name_ko": kit.get(x["id"], ""), "category": t.get("overallCategory", ""),
                     "sub": t.get("category", ""), "subsub": t.get("subCategory", ""), "req_level": x.get("requiredLevel", ""), "cash": int(bool(x.get("isCash")))})
    w_csv(os.path.join(REF, "MapleMaps", "items_list.csv"), ["id", "name", "name_ko", "category", "sub", "subsub", "req_level", "cash"], rows)
    return len(npc_en), len(it)


# ---------- 몹 ----------

def build_mobs(mobs, mlist, ko, maps_region):
    rows = []
    lv_of = {m["id"]: m for m in mlist}
    for mid, d in sorted(mobs.items()):
        m = d.get("meta") or {}
        found = d.get("foundAt") or []
        regs = collections.Counter(maps_region.get(i, "?") for i in found)
        rows.append({
            "id": mid, "name": d.get("name", ""), "name_ko": ko.get(mid, ""), "level": m.get("level", 0), "boss": int(bool(lv_of.get(mid, {}).get("isBoss"))),
            "hp": m.get("maxHP", 0), "mp": m.get("maxMP", 0), "exp": m.get("exp", 0), "patk": m.get("physicalDamage", 0), "pdef": m.get("physicalDefense", 0),
            "matk": m.get("magicDamage", 0), "mdef": m.get("magicDefense", 0), "acc": m.get("accuracy", 0), "eva": m.get("evasion", 0), "speed": m.get("speed", 0),
            "undead": int(bool(m.get("isUndead"))), "body_attack": int(bool(m.get("isBodyAttack"))), "map_count": len(found),
            "regions": "; ".join("%s(%d)" % kv for kv in regs.most_common(4)),
            "maps": " ".join(str(i) for i in found[:25]) + (" …+%d" % (len(found) - 25) if len(found) > 25 else "")})
    cols = ["id", "name", "name_ko", "level", "boss", "hp", "mp", "exp", "patk", "pdef", "matk", "mdef", "acc", "eva", "speed", "undead", "body_attack", "map_count", "regions", "maps"]
    w_csv(os.path.join(REF, "MapleMobs", "mobs_summary.csv"), cols, rows)
    # 레벨대 통계
    L = ["# 몹 레벨대별 HP·EXP 곡선 (밸런스 기준용)", "", "출처 maplestory.io GMS v83. 보스 제외·HP 100만 미만·EXP>0(훈련용 허수아비·이벤트 몹 제외) 기준. HP·EXP 는 몹 1마리 값. 비율 열 = EXP/HP(처치 효율 지표), 필요 처치 수 = 다음 레벨 EXP 가 없어 생략.", "",
         "| Lv 구간 | 몹 수 | HP 중앙값 | HP 90% | HP 최대 | EXP 중앙값 | EXP 90% | EXP/HP 중앙값 | 공격 중앙값 | 방어 중앙값 | 회피 중앙값 |", "|---|---|---|---|---|---|---|---|---|---|---|"]
    norm_rows = [r for r in rows if not r["boss"] and r["level"] > 0 and r["hp"] < 1000000 and r["exp"] > 0]
    for a in range(1, 200, 10):
        g = [r for r in norm_rows if a <= r["level"] < a + 10]
        if not g:
            continue
        L.append("| %d~%d | %d | %d | %d | %d | %d | %d | %.2f | %d | %d | %d |" % (a, a + 9, len(g), med([r["hp"] for r in g]), p90([r["hp"] for r in g]), max(r["hp"] for r in g),
                 med([r["exp"] for r in g]), p90([r["exp"] for r in g]), med([r["exp"] / r["hp"] for r in g if r["hp"]]), med([r["patk"] for r in g]), med([r["pdef"] for r in g]), med([r["eva"] for r in g])))
    L += ["", "## 보스(isBoss)", "", "| Lv | 이름 | HP | EXP | 공격 | 방어 | 출현 맵 수 |", "|---|---|---|---|---|---|---|"]
    for r in sorted([r for r in rows if r["boss"]], key=lambda r: r["level"]):
        L.append("| %d | %s | %d | %d | %d | %d | %d |" % (r["level"], r["name"], r["hp"], r["exp"], r["patk"], r["pdef"], r["map_count"]))
    L += ["", "## 레벨 1~40 몹 목록(초반 설계용)", "", "| Lv | 이름 | 한글 | HP | EXP | 공격 | 방어 | 속도 | 대표 지역 |", "|---|---|---|---|---|---|---|---|---|"]
    for r in sorted([r for r in rows if 0 < r["level"] <= 40 and not r["boss"] and r["map_count"] > 0], key=lambda r: (r["level"], r["id"])):
        L.append("| %d | %s | %s | %d | %d | %d | %d | %d | %s |" % (r["level"], r["name"], r["name_ko"], r["hp"], r["exp"], r["patk"], r["pdef"], r["speed"], r["regions"].split(";")[0]))
    w_md(os.path.join(REF, "MapleMobs", "mobs_by_level.md"), L)
    return len(rows)


# ---------- 퀘스트 ----------

POLITE = re.compile(r"(요|니다|세요|습니까|죠|까요|네요)[.!?~… ]*$")
CASUAL = re.compile(r"(다|야|어|지|자|라|냐|군|걸)[.!?~… ]*$")
MOTIF = collections.OrderedDict([
    ("부탁·도움", ["부탁", "도와", "도움", "도와줄"]), ("심부름·전달", ["전해", "전달", "찾아가", "건네", "가져다"]),
    ("수집·구하기", ["구해", "모아", "수집", "찾아", "가져"]), ("조사·확인", ["조사", "알아", "확인", "수상", "정찰", "살펴"]),
    ("위협·퇴치", ["위협", "퇴치", "막아", "쓰러", "없애", "물리", "위험", "공격"]), ("걱정·사연", ["걱정", "고민", "곤란", "슬프", "그리", "사연"]),
])
STOP = set("그리고 하지만 그러나 이것 저것 있는 없는 하는 되는 같은 위해 대한 통해 모두 정말 아주 매우 이제 다시 먼저 하고 에게 에서 으로 라고 이라고 싶어 하여".split())


def qtype(d):
    rc = d.get("requirementToComplete") or {}
    rs = d.get("requirementToStart") or {}
    kill = bool(rc.get("mobs"))
    col = any((x.get("count") or 1) > 0 for x in rc.get("items") or [])
    if kill and col:
        return "처치+수집"
    if kill:
        return "처치"
    if col:
        return "수집"
    if rc.get("npcId") and rc.get("npcId") != rs.get("npcId"):
        return "전달"
    return "대화"


def build_quests(quests, qko, qlist_en, qlist_ko, npc_en, npc_ko, item_en, mob_en, npc_region):
    qname = {q["id"]: q.get("name", "") for q in qlist_en}
    qname_ko = {q["id"]: q.get("name", "") for q in qlist_ko}
    rows = {}
    for qid, d in quests.items():
        rs = d.get("requirementToStart") or {}
        rc = d.get("requirementToComplete") or {}
        rw = d.get("rewardOnComplete") or {}
        sn, cn = rs.get("npcId"), rc.get("npcId")
        reg = npc_region.get(sn) or npc_region.get(cn) or ("area %s" % d.get("area"))
        goal = []
        for m in rc.get("mobs") or []:
            goal.append("%s x%d" % (mob_en.get(m["id"], m["id"]), m.get("count", 0)))
        for it in rc.get("items") or []:
            if (it.get("count") or 1) > 0:
                goal.append("%s x%d" % (item_en.get(it["id"], it["id"]), it.get("count") or 1))
        rit = ["%s x%d" % (item_en.get(it["id"], it["id"]), it.get("count") or 1) for it in rw.get("items") or [] if (it.get("count") or 1) > 0]
        pre = [x["id"] for x in rs.get("quests") or [] if x.get("state") == 2]
        nxt = [x["id"] for x in d.get("questsAvailableOnComplete") or []]
        rows[qid] = {"id": qid, "name": d.get("name", ""), "name_ko": qname_ko.get(qid, ""), "region": reg, "area": d.get("area", ""),
                     "min_level": rs.get("levelMinimum", ""), "max_level": rs.get("levelMaximum", ""), "start_npc": npc_en.get(sn, sn or ""), "start_npc_ko": npc_ko.get(sn, ""),
                     "end_npc": npc_en.get(cn, cn or ""), "type": qtype(d), "goal": "; ".join(goal), "reward_exp": rw.get("exp", 0), "reward_meso": rw.get("meso", 0),
                     "reward_items": "; ".join(rit), "prereq": " ".join(str(i) for i in pre), "next": " ".join(str(i) for i in nxt), "stages": len(d.get("messages") or []),
                     "repeat": int(bool(rs.get("repeatInterval"))), "auto_start": int(bool(d.get("autoStart")))}
    cols = ["id", "name", "name_ko", "region", "area", "min_level", "max_level", "start_npc", "start_npc_ko", "end_npc", "type", "goal", "reward_exp", "reward_meso", "reward_items",
            "prereq", "next", "stages", "repeat", "auto_start"]
    srt = sorted(rows.values(), key=lambda r: (r["region"], r["min_level"] if r["min_level"] != "" else 999, r["id"]))
    w_csv(os.path.join(REF, "MapleQuests", "quests_summary.csv"), cols, srt)
    # by_level
    def lvb(r):
        return "미지정" if r["min_level"] == "" else "%d~%d" % ((r["min_level"] // 10) * 10, (r["min_level"] // 10) * 10 + 9)
    bl = collections.defaultdict(list)
    for r in rows.values():
        bl[lvb(r)].append(r)
    L = ["# 퀘스트 레벨대별 통계 (참고용, 보상은 우리가 더 강하게 줄 것이므로 곡선 모양만 본다)", "", "레벨 = 시작 요구 레벨(levelMinimum). 총 %d개." % len(rows), "",
         "| 최소 레벨 | 수 | 처치 | 수집 | 처치+수집 | 전달 | 대화 | 보상 EXP 중앙값 | EXP 최대 | 메소 중앙값 | 아이템 보상 비율 | 평균 단계 수 | 반복형 |", "|---|---|---|---|---|---|---|---|---|---|---|---|---|"]
    order = sorted(bl, key=lambda k: (k == "미지정", int(k.split("~")[0]) if k != "미지정" else 0))
    for k in order:
        g = bl[k]
        n = len(g)
        tc = collections.Counter(r["type"] for r in g)
        pc = lambda t: "%d%%" % round(100 * tc[t] / n)
        expv = [r["reward_exp"] for r in g if r["reward_exp"]]
        mes = [r["reward_meso"] for r in g if r["reward_meso"]]
        L.append("| %s | %d | %s | %s | %s | %s | %s | %d | %d | %d | %d%% | %.1f | %d |" % (k, n, pc("처치"), pc("수집"), pc("처치+수집"), pc("전달"), pc("대화"), med(expv), max(expv or [0]), med(mes),
                 round(100 * sum(1 for r in g if r["reward_items"]) / n), statistics.mean(r["stages"] for r in g), sum(r["repeat"] for r in g)))
    L += ["", "## 지역별 퀘스트 수·유형 (시작 NPC 가 있는 지역)", "", "| 지역 | 수 | 최소~최대 레벨 | 처치 | 수집 | 전달 | 대화 |", "|---|---|---|---|---|---|---|"]
    br = collections.defaultdict(list)
    for r in rows.values():
        br[r["region"]].append(r)
    for reg, g in sorted(br.items(), key=lambda kv: -len(kv[1])):
        lv = [r["min_level"] for r in g if r["min_level"] != ""]
        tc = collections.Counter(r["type"] for r in g)
        L.append("| %s | %d | %s | %d | %d | %d | %d |" % (reg, len(g), "%d~%d" % (min(lv), max(lv)) if lv else "-", tc["처치"] + tc["처치+수집"], tc["수집"], tc["전달"], tc["대화"]))
    w_md(os.path.join(REF, "MapleQuests", "by_level.md"), L)
    # chains
    pre_of = {r["id"]: [int(x) for x in r["prereq"].split()] for r in rows.values()}
    nexts = collections.defaultdict(set)
    for q, pres in pre_of.items():
        for p in pres:
            nexts[p].add(q)
    for r in rows.values():
        for n in r["next"].split():
            if int(n) in rows:
                nexts[r["id"]].add(int(n))
    roots = [q for q in rows if not [p for p in pre_of[q] if p in rows] and nexts.get(q)]
    chains = []

    def walk(q, path, seen, budget):
        path = path + [q]
        ns = sorted(n for n in nexts.get(q, ()) if n in rows and n not in seen and len(path) < 40)
        if not ns:
            chains.append(path)
            budget[0] -= 1
            return
        for n in ns[:3]:
            if budget[0] <= 0:
                return
            walk(n, path, seen | {q}, budget)

    for r0 in roots:
        walk(r0, [], set(), [4])  # 루트당 최대 4줄
    bychreg = collections.defaultdict(list)
    for c in chains:
        if len(c) >= 3:
            bychreg[rows[c[0]]["region"]].append(c)

    def lvtxt(q):
        v = rows[q]["min_level"]
        return "Lv%s" % v if v != "" else "Lv-"

    L = ["# 퀘스트 연계 사슬 (3단 이상, 지역·레벨순)", "", "표기: `이름(최소레벨)` → 다음 퀘스트. 분기는 갈래마다 한 줄(최대 3갈래). 선행(prereq)·후속(questsAvailableOnComplete)으로 그래프를 만들어 루트에서 걸었다.", ""]
    for reg in sorted(bychreg, key=lambda k: -len(bychreg[k])):
        cs = sorted(bychreg[reg], key=lambda c: (rows[c[0]]["min_level"] if rows[c[0]]["min_level"] != "" else 999, -len(c)))
        L += ["## %s (%d줄)" % (reg, len(cs)), ""]
        for c in cs[:60]:
            L.append("- " + " → ".join("%s(%s)" % (rows[q]["name"], lvtxt(q)) for q in c[:14]) + (" … (+%d)" % (len(c) - 14) if len(c) > 14 else ""))
        if len(cs) > 60:
            L.append("- … 외 %d줄(quests_summary.csv 의 prereq/next 참조)" % (len(cs) - 60))
        L.append("")
    w_md(os.path.join(REF, "MapleQuests", "chains.md"), L)
    # tone
    tone(rows, quests, qko, br, npc_ko, chains)
    return len(rows), len(chains)


def tone(rows, quests, qko, br, npc_ko, chains):
    L = ["# 지역별 퀘스트 말투·소재 (우리 퀘스트 문장 새로 쓸 때의 분위기 참고)", "",
         "한글 본문(KMS) 통계. 원문은 캐시에만 있고 여기엔 **통계·키워드·구조 요약**만 있다(원문 인용 없음). 줄거리 요약은 구조 데이터(NPC·목표·보상)로 우리 말로 새로 쓴 것이다.",
         "말투 지표: 문장 끝이 해요/합니다체(존댓말)인 문장 비율. 퀘스트 로그 문장은 대체로 서술체라 낮을 수 있다. 소재 지표: 소재 키워드가 한 번이라도 들어간 퀘스트 비율.", ""]
    for reg, g in sorted(br.items(), key=lambda kv: -len(kv[1])):
        if len(g) < 8:
            continue
        sents, polite, casual, stages, motifs, words = [], 0, 0, [], collections.Counter(), collections.Counter()
        npcs = collections.Counter(r["start_npc_ko"] or r["start_npc"] for r in g)
        n_ko = 0
        for r in g:
            k = qko.get(r["id"])
            if not k:
                continue
            n_ko += 1
            msgs = [re.sub(r"#\w|#p\d+#|#t\d+#|#o\d+#|#m\d+#|#k|#e|#n", "", m) for m in k.get("messages") or []]
            stages.append(len(msgs))
            text = " ".join(msgs)
            for sent in re.split(r"(?<=[.!?])\s+", text):
                sent = sent.strip()
                if len(sent) < 3:
                    continue
                sents.append(len(sent))
                if POLITE.search(sent):
                    polite += 1
                elif CASUAL.search(sent):
                    casual += 1
            for mname, keys in MOTIF.items():
                if any(kw in text for kw in keys):
                    motifs[mname] += 1
            for wd in re.findall(r"[가-힣]{2,}", text):
                if wd not in STOP and len(wd) <= 5:
                    words[wd] += 1
        if not n_ko:
            continue
        tot = max(1, polite + casual)
        lv = [r["min_level"] for r in g if r["min_level"] != ""]
        tc = collections.Counter(r["type"] for r in g)
        L += ["## %s (%d개, Lv %s)" % (reg, len(g), "%d~%d" % (min(lv), max(lv)) if lv else "-"), "",
              "- 유형: 처치 %d·수집 %d·전달 %d·대화 %d. 로그 단계 평균 %.1f, 문장 길이 평균 %d자, 존댓말 비율 %d%%." % (tc["처치"] + tc["처치+수집"], tc["수집"], tc["전달"], tc["대화"], statistics.mean(stages), statistics.mean(sents) if sents else 0, round(100 * polite / tot)),
              "- 자주 나오는 소재: " + ", ".join("%s %d%%" % (m, round(100 * c / n_ko)) for m, c in motifs.most_common(4)) + ".",
              "- 분위기 키워드(빈도순): " + ", ".join(w for w, _ in words.most_common(12)) + ".",
              "- 퀘스트를 많이 주는 NPC: " + ", ".join("%s(%d)" % kv for kv in npcs.most_common(5)) + "."]
        pick, seen_t = [], set()
        cand = sorted([r for r in g if r["stages"] >= 2 and r["min_level"] != ""], key=lambda r: r["min_level"])
        step = max(1, len(cand) // 6)
        for r in cand[::step]:
            if r["type"] in seen_t and len(pick) >= 3:
                continue
            seen_t.add(r["type"])
            pick.append(r)
            if len(pick) >= 4:
                break
        L.append("- 대표 줄거리(구조 요약):")
        for r in pick:
            who = r["start_npc_ko"] or r["start_npc"]
            goal = r["goal"] or "(물건·몹 없음)"
            end = r["end_npc"] or who
            if r["type"] == "처치":
                s = "%s 가 마을 주변 위협을 줄여 달라고 맡기는 토벌형. 목표 %s." % (who, goal)
            elif r["type"] == "수집":
                s = "%s 가 필요한 재료를 모아 오라고 맡기는 수집형. 목표 %s." % (who, goal)
            elif r["type"] == "처치+수집":
                s = "%s 의 부탁으로 몹을 잡아 물건까지 모으는 복합형. 목표 %s." % (who, goal)
            elif r["type"] == "전달":
                s = "%s 에서 시작해 %s 에게 소식·물건을 이어 주는 심부름형." % (who, end)
            else:
                s = "%s 와 대화로 진행되는 안내형." % who
            L.append("  - Lv%s %s: %s 단계 %d, 보상 EXP %s." % (r["min_level"], r["name"], s, r["stages"], r["reward_exp"] or "-"))
        L.append("")
    w_md(os.path.join(REF, "MapleQuests", "tone_by_region.md"), L)


# ---------- main ----------

def main():
    maps = loaddir("map")
    mobs = loaddir("mob")
    quests = loaddir("quest")
    qko = loaddir("questko")
    Lp = os.path.join(CACHE, "lists")
    mob_en, mob_ko = names(load(os.path.join(Lp, "mob.json")), load(os.path.join(Lp, "kmob.json")))
    npc_en, npc_ko = names(load(os.path.join(Lp, "npc.json")), load(os.path.join(Lp, "knpc.json")))
    item_en = {x["id"]: x.get("name", "") for x in load(os.path.join(Lp, "item.json"))}
    qlist_en = load(os.path.join(Lp, "quest.json"))
    qlist_ko = load(os.path.join(Lp, "kquest.json"))
    maps_region = {i: norm(d.get("streetName")) for i, d in maps.items()}
    # NPC -> 지역(그 NPC 가 서 있는 맵 streetName 최빈값, 마을 우선)
    cnt = collections.defaultdict(collections.Counter)
    for i, d in maps.items():
        w = 3 if d.get("isTown") else 1
        for n in d.get("npcs") or []:
            if isinstance(n, dict):
                cnt[n["id"]][maps_region[i]] += w
    npc_region = {k: v.most_common(1)[0][0] for k, v in cnt.items()}
    nn, ni = build_lists(npc_en, npc_ko)
    nm = build_mobs(mobs, load(os.path.join(Lp, "mob.json")), mob_ko, maps_region)
    nq, nc = build_quests(quests, qko, qlist_en, qlist_ko, npc_en, npc_ko, item_en, mob_en, npc_region)
    print("npcs", nn, "items", ni, "mobs", nm, "quests", nq, "chains", nc, "questko", len(qko))
