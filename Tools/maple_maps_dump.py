#!/usr/bin/env python
"""maplestory.io(GMS v83) 맵 데이터 수집·요약. 표준 라이브러리만. 실행: python -I Tools/maple_maps_dump.py <fetch|build|all>

fetch : 맵 목록 -> 대상 맵 상세 -> 몹 상세(GMS 수치 + KMS 한글명) 를 Tools/cache/maple_maps/ 에 캐시(이어받기 가능).
build : 캐시로 Docs/Reference/MapleMaps/ 의 maps_summary.csv·by_region/*.md·levels_1_70.md 생성.
제외 기준·열 설명은 Docs/Reference/MapleMaps/README.md.
"""
import csv, json, math, os, re, sys, threading, time, urllib.request, statistics, collections

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CACHE = os.path.join(ROOT, "Tools", "cache", "maple_maps")
OUT = os.path.join(ROOT, "Docs", "Reference", "MapleMaps")
API = "https://maplestory.io/api"
GMS = API + "/GMS/83"
KMS = API + "/KMS/284"
INTERVAL = 0.3
WORKERS = 8

# 제외 street(이벤트·인스턴스·이동수단·도장 등). 소문자 비교·공백 정리 후.
EXCLUDE_STREETS = {
    "mu lung dojo", "goldrich's maze", "haunted house", "monster carnival", "the 2nd monster carnival",
    "party quest", "adobis's mission i", "during the ride", "on a voyage", "cruising", "in flight",
    "camp conference room", "mini dungeon", "premium road", "unique road", "opening", "warning street",
    "hidden chamber", "space gaga", "aquarium", "cave of life", "deep place of temple",
}

_lock = threading.Lock()
_last = [0.0]
fails = []


def norm(s):
    return re.sub(r"\s+", " ", (s or "").strip())


def http_json(url):
    for attempt in range(3):
        with _lock:
            wait = _last[0] + INTERVAL - time.time()
            if wait > 0:
                time.sleep(wait)
            _last[0] = time.time()
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "ProjectB-refdump/1.0"})
            with urllib.request.urlopen(req, timeout=60) as r:
                return json.loads(r.read().decode("utf-8"))
        except Exception as e:  # noqa
            err = str(e)
            if "404" in err or ("500" in err and attempt >= 1):  # 없는 id·WZ 캐스팅 오류는 재시도해도 같다
                break
            time.sleep(1.5 * (attempt + 1))
    fails.append((url, err))
    return None


def cpath(*parts):
    return os.path.join(CACHE, *parts)


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def save(path, obj):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        json.dump(obj, f, ensure_ascii=False, separators=(",", ":"))


def fetch_cached(url, path, strip=None):
    if os.path.exists(path):
        return load(path)
    d = http_json(url)
    if d is None:
        return None
    if strip:
        strip(d)
    save(path, d)
    return d


def strip_map(d):
    mm = d.get("miniMap")
    if isinstance(mm, dict):
        mm.pop("canvas", None)
    for k in ("mobs", "npcs"):
        for e in d.get(k) or []:
            if isinstance(e, dict):
                e.pop("foothold", None)
                e.pop("bounds", None)
    for p in d.get("portals") or []:
        if isinstance(p, dict):
            p.pop("bounds", None)


def run_pool(items, fn):
    q = list(items)
    qlock = threading.Lock()
    done = [0]

    def worker():
        while True:
            with qlock:
                if not q:
                    return
                it = q.pop()
                done[0] += 1
                n = done[0]
            try:
                fn(it)
            except Exception as e:  # noqa
                fails.append((str(it), "EXC " + repr(e)))
            if n % 100 == 0:
                print("  progress", n, "/", len(items), flush=True)

    ts = [threading.Thread(target=worker) for _ in range(WORKERS)]
    for t in ts:
        t.start()
    for t in ts:
        t.join()


def target_ids(lst):
    return [m["id"] for m in lst]


def do_fetch():
    """우선순위: 맵 > 퀘스트 > 몹 > NPC·아이템(목록만). 캐시가 있으면 건너뛴다(이어받기)."""
    os.makedirs(CACHE, exist_ok=True)
    lst = fetch_cached(GMS + "/map", cpath("_list.json"))
    fetch_cached(KMS + "/map", cpath("_list_kms.json"))
    for e in ("mob", "npc", "item", "quest"):
        fetch_cached("%s/%s" % (GMS, e), cpath("lists", e + ".json"))
        fetch_cached("%s/%s" % (KMS, e), cpath("lists", "k" + e + ".json"))
    ids = target_ids(lst)
    print("maps", len(ids), flush=True)
    run_pool(ids, lambda i: fetch_cached("%s/map/%d" % (GMS, i), cpath("map", "%d.json" % i), strip_map))
    save(cpath("_fails.json"), fails)
    qids = [q["id"] for q in load(cpath("lists", "quest.json"))]
    print("quests", len(qids), flush=True)
    run_pool(qids, lambda i: fetch_cached("%s/quest/%d" % (GMS, i), cpath("quest", "%d.json" % i)))
    save(cpath("_fails.json"), fails)
    mids = [m["id"] for m in load(cpath("lists", "mob.json"))]
    print("mobs", len(mids), flush=True)

    def mob_strip(d):
        for k in list(d.keys()):
            if k not in ("id", "meta", "name", "foundAt"):
                d.pop(k)

    run_pool(mids, lambda i: fetch_cached("%s/mob/%d" % (GMS, i), cpath("mob", "%d.json" % i), mob_strip))
    save(cpath("_fails.json"), fails)
    print("fails", len(fails))
    for f in fails[:20]:
        print(" ", f)


# ---------------- 요약 ----------------

def cluster_floors(segs, tol=40):
    """수평에 가까운 발판 y(길이 가중)를 tol 이내로 묶어 층 목록 [(y,len)] 반환."""
    pts = sorted(((s[2], s[3]) for s in segs), key=lambda t: t[0])  # (ymid, len)
    groups = []
    for y, ln in pts:
        if groups and y - groups[-1]["ymax"] <= tol:
            g = groups[-1]
            g["len"] += ln
            g["ymax"] = y
            g["ysum"] += y * ln
        else:
            groups.append({"ymax": y, "len": ln, "ysum": y * ln, "ymin": y})
    return [(g["ysum"] / g["len"] if g["len"] else g["ymin"], g["len"]) for g in groups]


def summarize(d, mobinfo, mobko, kname):
    fh = [f for f in (d.get("footholds") or {}).values() if isinstance(f, dict)]
    segs = []  # (x1,x2,ymid,len)
    total = 0.0
    xs, ys = [], []
    for f in fh:
        x1, x2, y1, y2 = f["x1"], f["x2"], f["y1"], f["y2"]
        xs += [x1, x2]
        ys += [y1, y2]
        if x1 == x2:
            continue
        ln = math.hypot(x2 - x1, y2 - y1)
        total += ln
        segs.append((x1, x2, (y1 + y2) / 2, ln))
    vr = d.get("vrBounds") or {}
    if vr.get("width", 0) > 0 and vr.get("height", 0) > 0:
        w, h = vr["width"], vr["height"]
        wsrc = "vr"
    else:
        w = (max(xs) - min(xs)) if xs else 0
        h = (max(ys) - min(ys)) if ys else 0
        wsrc = "fh"
    floors = [f for f in cluster_floors(segs) if f[1] >= 150]
    # 연결 발판 묶음(플랫폼) 수: next/prev 연결 요소
    byid = {f["id"]: f for f in fh}
    seen, platforms = set(), 0
    for f in fh:
        if f["id"] in seen:
            continue
        stack = [f["id"]]
        comp_len = 0.0
        while stack:
            i = stack.pop()
            if i in seen or i not in byid:
                continue
            seen.add(i)
            g = byid[i]
            if g["x1"] != g["x2"]:
                comp_len += math.hypot(g["x2"] - g["x1"], g["y2"] - g["y1"])
            for n in (g.get("next"), g.get("prev")):
                if n and n in byid and n not in seen:
                    stack.append(n)
        if comp_len >= 100:
            platforms += 1
    lr = [l for l in (d.get("ladderRopes") or []) if isinstance(l, dict)]
    ropes = [l for l in lr if not l.get("isLadder")]
    ladders = [l for l in lr if l.get("isLadder")]
    lens = [abs(l["y2"] - l["y1"]) for l in lr]
    raw = d.get("portals") or []
    nones = sum(1 for p in raw if p is None)  # API 가 대상 맵을 못 풀면 null(대개 다른 맵으로 가는 출구)
    portals = [p for p in raw if isinstance(p, dict) and not (p.get("type") == 0 and p.get("portalName") == "sp")]
    own = d.get("id")
    exits = [p for p in portals if p.get("toMap") not in (None, 999999999) and p.get("toMap") != own]
    selfp = [p for p in portals if p.get("toMap") == own]
    hidden = [p for p in portals if p.get("type") == 1]
    script = [p for p in portals if p.get("toMap") == 999999999 and p.get("type") != 1]
    n_portals = len(portals) + nones
    n_exits = len(exits) + nones
    mobs = [m for m in (d.get("mobs") or []) if isinstance(m, dict)]
    species = collections.OrderedDict()
    for m in mobs:
        species[m["id"]] = species.get(m["id"], 0) + 1
    lv = [mobinfo[i]["level"] for i in species if i in mobinfo]
    mobs_txt = []
    for i, n in sorted(species.items(), key=lambda t: mobinfo.get(t[0], {}).get("level", 0)):
        mi = mobinfo.get(i, {})
        mobs_txt.append("%s(Lv%s,x%d)" % (mi.get("name", i), mi.get("level", "?"), n))
    mobs_ko = [mobko.get(i, "") for i in species if mobko.get(i)]
    exp_sum = sum(mobinfo[i]["exp"] * n for i, n in species.items() if i in mobinfo)
    hp_sum = sum(mobinfo[i]["hp"] * n for i, n in species.items() if i in mobinfo)
    dens = (len(mobs) / total * 1000) if total else 0
    return {
        "id": own, "region": norm(d.get("streetName")), "name": d.get("name"), "name_ko": kname.get(own, ""),
        "width": int(w), "height": int(h), "wsrc": wsrc,
        "floors": len(floors), "platforms": platforms, "fh_count": len(fh), "fh_len": int(total),
        "ropes": len(ropes), "ladders": len(ladders), "lr_avg": int(statistics.mean(lens)) if lens else 0,
        "portals": n_portals, "exits": n_exits, "self_portals": len(selfp), "hidden": len(hidden), "script": len(script),
        "spawns": len(mobs), "species": len(species), "lv_min": min(lv) if lv else "", "lv_max": max(lv) if lv else "",
        "mob_rate": round(d.get("mobRate") or 0, 2), "dens": round(dens, 2),
        "mobs": "; ".join(mobs_txt), "mobs_ko": "; ".join(mobs_ko),
        "exp_sum": exp_sum, "hp_sum": hp_sum, "bgm": d.get("backgroundMusic") or "",
        "return": d.get("returnMap"), "town": bool(d.get("isTown")),
        "_floors": floors, "_lr": lr, "_fh": fh, "_exits": exits, "_species": species, "_mobs": mobs,
    }


COLS = ["id", "kind", "region", "name", "name_ko", "width", "height", "floors", "platforms", "fh_len", "ropes", "ladders", "lr_avg",
        "portals", "exits", "self_portals", "hidden", "script", "spawns", "species", "lv_min", "lv_max", "mob_rate",
        "dens", "exp_sum", "hp_sum", "mobs", "mobs_ko", "return", "bgm", "is_town"]


def medrange(vals):
    vals = sorted(v for v in vals if v != "")
    if not vals:
        return "-"
    return "%g (%g~%g)" % (statistics.median(vals), vals[0], vals[-1])


def terrain_desc(s):
    """footholds·ladderRopes 좌표로 짧은 지형 서술(모양 복제용 아님, 구조 패턴만)."""
    fl = sorted(s["_floors"], key=lambda t: t[0])
    parts = []
    if fl:
        parts.append("층 %d개(y=%s)" % (len(fl), ",".join("%d" % round(y) for y, _ in fl)))
        top, bot = fl[0], max(fl, key=lambda t: t[1])
        parts.append("최장 층 y≈%d 길이 %d" % (round(bot[0]), round(bot[1])))
    lr = s["_lr"]
    if lr:
        xs = sorted(l["x"] for l in lr)
        parts.append("로프/사다리 %d개 x=%s" % (len(lr), ",".join(str(x) for x in xs[:8])))
    return " / ".join(parts)


def region_file(name):
    n = re.sub(r"[^0-9A-Za-z가-힣_\-]+", "_", name.strip()).strip("_")
    return n or "unknown"


def do_build():
    lst = load(cpath("_list.json"))
    klst = load(cpath("_list_kms.json")) if os.path.exists(cpath("_list_kms.json")) else []
    kname = {m["id"]: m.get("name", "") for m in klst}
    mobinfo, mobko = {}, {}
    mdir = cpath("mob")
    if os.path.isdir(mdir):
        for fn in os.listdir(mdir):
            d = load(os.path.join(mdir, fn))
            m = d.get("meta") or {}
            mobinfo[d["id"]] = {"name": d.get("name", ""), "level": m.get("level", 0), "hp": m.get("maxHP", 0), "exp": m.get("exp", 0)}
    kdir = cpath("mobko")
    if os.path.isdir(kdir):
        for fn in os.listdir(kdir):
            d = load(os.path.join(kdir, fn))
            mobko[d["id"]] = d.get("name", "")
    rows = []
    skipped = collections.Counter()
    ids = target_ids(lst)
    for i in ids:
        p = cpath("map", "%d.json" % i)
        if not os.path.exists(p):
            skipped["missing"] += 1
            continue
        d = load(p)
        if not d.get("footholds"):
            skipped["nofh"] += 1
        r = summarize(d, mobinfo, mobko, kname)
        if r["town"]:
            r["kind"] = "town"
        elif norm(d.get("streetName")).lower() in EXCLUDE_STREETS:
            r["kind"] = "event"  # 이벤트·인스턴스·도장 등
        elif r["spawns"] > 0:
            r["kind"] = "hunt"
        elif r["width"] < 1500 and r["floors"] <= 2:
            r["kind"] = "indoor"  # 몹 없는 소형(상점·집·퀘스트방 등)
        else:
            r["kind"] = "other"
        rows.append(r)
    rows.sort(key=lambda r: (r["region"], r["id"]))
    for r in rows:
        r["is_town"] = int(r["town"])
    os.makedirs(os.path.join(OUT, "by_region"), exist_ok=True)
    with open(os.path.join(OUT, "maps_summary.csv"), "w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f)
        w.writerow(COLS)
        for r in rows:
            w.writerow([r[c] for c in COLS])
    # 지역별
    byreg = collections.OrderedDict()
    for r in rows:
        byreg.setdefault(r["region"], []).append(r)
    # 이전 산출물 정리
    for fn in os.listdir(os.path.join(OUT, "by_region")):
        if fn.endswith(".md"):
            os.remove(os.path.join(OUT, "by_region", fn))
    for reg, rs in byreg.items():
        kc = collections.Counter(r["kind"] for r in rs)
        allrs = rs
        rs = [r for r in rs if r["kind"] == "town" or (r["kind"] == "hunt" and r["spawns"] >= 5)]
        lines = ["# %s (전체 %d맵: %s)" % (reg or "(지역 없음)", len(allrs), ", ".join("%s %d" % kv for kv in kc.items())), "",
                 "표는 마을(town)·사냥터(hunt, 스폰 5 이상)만. 나머지(소형 hunt·indoor/event/other)는 maps_summary.csv.", "",
                 "출처 maplestory.io GMS v83. 폭·높이 px(발판 범위 기준), 층=발판 y 근접 묶음(150px 이상), 밀도=발판 1000px당 스폰 지점.", "",
                 "| id | 이름 | 한글 | 폭 | 높이 | 층 | 로프 | 포털(출구) | 스폰 | 밀도 | Lv | 몹 |",
                 "|---|---|---|---|---|---|---|---|---|---|---|---|"]
        for r in rs:
            lvs = "%s~%s" % (r["lv_min"], r["lv_max"]) if r["lv_min"] != "" else "-"
            lines.append("| %d | %s | %s | %d | %d | %d | %d | %d(%d) | %d | %.1f | %s | %s |" % (
                r["id"], r["name"], r["name_ko"], r["width"], r["height"], r["floors"], r["ropes"] + r["ladders"],
                r["portals"], r["exits"], r["spawns"], r["dens"], lvs, r["mobs"].replace("|", "/")))
        lines += ["", "## 레벨대별 대표 지형 서술", ""]
        # 대표: 레벨 구간(10단위)별로 스폰 많은 맵 1~2개
        bands = collections.defaultdict(list)
        for r in rs:
            if r["lv_max"] != "":
                bands[(int(r["lv_min"]) // 10) * 10].append(r)
        for b in sorted(bands):
            pick = sorted(bands[b], key=lambda r: -r["spawns"])[:2]
            for r in pick:
                lines.append("- Lv%d대 %s(%d): 폭 %d·높이 %d. %s. 스폰 %d(밀도 %.1f)." % (b, r["name"], r["id"], r["width"], r["height"], terrain_desc(r), r["spawns"], r["dens"]))
        with open(os.path.join(OUT, "by_region", region_file(reg) + ".md"), "w", encoding="utf-8", newline="\n") as f:
            f.write("\n".join(lines) + "\n")
    # Lv1~70
    sel = [r for r in rows if r["kind"] == "hunt" and r["lv_max"] != "" and r["lv_min"] != "" and r["lv_min"] <= 70 and r["lv_max"] <= r["lv_min"] + 30 and r["spawns"] >= 5 and r["width"] >= 800 and r["height"] >= 300 and r["exits"] >= 1 and r["floors"] >= 2]
    sel.sort(key=lambda r: (r["lv_min"], r["lv_max"], r["id"]))
    L = ["# Lv1~70 사냥터 (레벨순)", "",
         "대상: kind=hunt, 몹 최저 레벨 ≤70(최고−최저 ≤30), 스폰 ≥5, 폭 ≥800, 높이 ≥300, 출구 포털 ≥1, 층 ≥2(퀘스트 인스턴스·조각 맵 제외 목적). 총 %d맵. 값은 maplestory.io GMS v83 실측(px)." % len(sel), "",
         "## 통계 (중앙값 (최소~최대))", "",
         "| 구간 | 맵 수 | 폭 | 높이 | 층 | 로프 | 스폰 | 밀도 | 포털 | 종 수 |", "|---|---|---|---|---|---|---|---|---|---|"]
    for a, b in [(1, 10), (11, 20), (21, 30), (31, 40), (41, 50), (51, 60), (61, 70)]:
        g = [r for r in sel if a <= r["lv_min"] <= b]
        L.append("| Lv%d~%d | %d | %s | %s | %s | %s | %s | %s | %s | %s |" % (a, b, len(g), medrange([r["width"] for r in g]), medrange([r["height"] for r in g]),
                 medrange([r["floors"] for r in g]), medrange([r["ropes"] + r["ladders"] for r in g]), medrange([r["spawns"] for r in g]),
                 medrange([r["dens"] for r in g]), medrange([r["portals"] for r in g]), medrange([r["species"] for r in g])))
    g = sel
    L.append("| 전체 | %d | %s | %s | %s | %s | %s | %s | %s | %s |" % (len(g), medrange([r["width"] for r in g]), medrange([r["height"] for r in g]),
             medrange([r["floors"] for r in g]), medrange([r["ropes"] + r["ladders"] for r in g]), medrange([r["spawns"] for r in g]),
             medrange([r["dens"] for r in g]), medrange([r["portals"] for r in g]), medrange([r["species"] for r in g])))
    L += ["", "## 맵 목록", "", "| Lv | 지역 | id | 이름 | 한글 | 폭 | 높이 | 층 | 로프 | 포털 | 스폰 | 밀도 | 몹 |", "|---|---|---|---|---|---|---|---|---|---|---|---|---|"]
    for r in sel:
        L.append("| %s~%s | %s | %d | %s | %s | %d | %d | %d | %d | %d | %d | %.1f | %s |" % (r["lv_min"], r["lv_max"], r["region"], r["id"], r["name"], r["name_ko"], r["width"],
                 r["height"], r["floors"], r["ropes"] + r["ladders"], r["portals"], r["spawns"], r["dens"], r["mobs"].replace("|", "/")))
    with open(os.path.join(OUT, "levels_1_70.md"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(L) + "\n")
    print("kinds", dict(collections.Counter(r["kind"] for r in rows)))
    print("rows", len(rows), "regions", len(byreg), "lv1-70", len(sel), "skipped", dict(skipped))


def do_fetch_ko():
    """퀘스트 한글 본문(KMS) — 말투 통계용. 원문은 캐시에만 둔다."""
    qids = [q["id"] for q in load(cpath("lists", "quest.json"))]
    have = set(int(f[:-5]) for f in os.listdir(cpath("quest")))
    qids = sorted((i for i in qids if i in have), reverse=True)  # pop() 이 낮은 id(기본 지역 퀘스트)부터 꺼내도록

    def strip(d):
        for k in list(d):
            if k not in ("id", "messages", "name"):
                d.pop(k)

    run_pool(qids, lambda i: fetch_cached("%s/quest/%d" % (KMS, i), cpath("questko", "%d.json" % i), strip))
    save(cpath("_fails_ko.json"), fails)
    print("ko fails", len(fails))


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else "all"
    if cmd in ("fetch", "all"):
        do_fetch()
    if cmd == "fetchko":
        do_fetch_ko()
    if cmd in ("build", "all"):
        do_build()
    if cmd in ("build", "all", "buildref"):
        sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
        import maple_ref_build  # 퀘스트·몹·NPC·아이템 요약(같은 폴더)
        maple_ref_build.main()
