#!/usr/bin/env python
"""봇 기록 vs 사람 기록 진행 곡선 비교: 분 단위 Lv·누적 EXP·골드·퀘스트 완료 (PNG 1장 + md 표). Pillow 필요(표준 외 유일).

사용: python Tools/playrec_curve.py <봇 jsonl> [--human a.jsonl b.jsonl ...] [--at 60] [--png 출력.png] [-o 출력.md]
- 사람 기본값: Logs/playrec/20261115_131248 + 20261115_133116 (이어서 한 판, 읽기 전용). 뒤 세션의 시간은 앞 세션 끝에 이어 붙인다.
- 누적 EXP = Σ LevelRules.ExpToNext(l<Lv) + 현재 레벨 EXP (st 행의 exp 는 레벨 안 값).
- 봇 기록은 마지막 meta 행(봇 시작 시각) 이후만 쓴다(시험 준비로 Lv·골드를 바꾸기 전 행 제외).
- 합격: --at(기본 60)분 지점 봇 Lv·누적 EXP 가 사람의 0.5~2배. 사람 기록이 그보다 짧으면 마지막 20% 구간 기울기로 외삽해 표시한다.
- 끝에 `CURVE_SUMMARY ...` 한 줄을 낸다(playrec_report.py 가 읽는다). 설계: .claude-rules/Game/PlayBotDesign.md 18절.
"""
import json
import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
HUMAN_DEFAULT = ["Logs/playrec/20261115_131248.jsonl", "Logs/playrec/20261115_133116.jsonl"]
EXP_TABLE = [500, 650, 800, 1200, 1450, 2900, 3500, 4200, 5000, 6000, 11500, 14000, 17000, 20500, 24500]
MAX_LEVEL = 70
PASS_LO, PASS_HI = 0.5, 2.0
_cache = []


def exp_to_next(level):
    """LevelRules.ExpToNext 와 같은 값(Lv16+ 는 Lv15 증가율 1.195 → Lv69 1.05 선형 감소)."""
    if level >= MAX_LEVEL:
        return 0
    if not _cache:
        v = float(EXP_TABLE[-1])
        for i in range(MAX_LEVEL - 1):
            if i < len(EXP_TABLE):
                _cache.append(EXP_TABLE[i])
                continue
            lv = i + 1
            t = min(1.0, max(0.0, (lv - 16.0) / (MAX_LEVEL - 1 - 16.0)))
            v = v * (1.195 + (1.05 - 1.195) * t)
            _cache.append(int(round(v)))
    return _cache[max(1, level) - 1]


def cum_exp(lv, exp):
    return sum(exp_to_next(l) for l in range(1, lv)) + exp


def load_session(path, bot):
    """-> (st 행 [(t, lv, cumExp, gold)], qdone 시각 목록, 끝 시각)"""
    rows, quests, t0, last_t = [], [], 0.0, 0.0
    with open(path, encoding="utf-8") as f:
        for ln in f:
            if '"st"' not in ln and '"ev"' not in ln and '"meta"' not in ln:
                continue
            try:
                r = json.loads(ln)
            except ValueError:
                continue
            k, t = r.get("k"), r.get("t", 0.0)
            if k == "meta":
                if bot:
                    t0 = t
                    rows, quests = [], []
            elif k == "st" and t >= t0 and r.get("lv"):
                rows.append((t - t0, r["lv"], cum_exp(r["lv"], r.get("exp", 0)), r.get("gold", 0)))
                last_t = t - t0
            elif k == "ev" and r.get("e") == "qdone" and t >= t0:
                quests.append(t - t0)
    return rows, quests, last_t


def load_series(paths, bot):
    rows, quests, off = [], [], 0.0
    for p in paths:
        r, q, end = load_session(p, bot)
        rows += [(t + off, lv, ex, g) for t, lv, ex, g in r]
        quests += [t + off for t in q]
        off += end
    return rows, quests, off


def at_minute(rows, quests, m):
    """m분 시각 이하 마지막 st 값 (lv, cumExp, gold, 퀘스트 완료 누적) — 기록이 모자라면 None"""
    if not rows or rows[-1][0] < m * 60 - 20:
        return None
    best = rows[0]
    for r in rows:
        if r[0] > m * 60:
            break
        best = r
    return best[1], best[2], best[3], sum(1 for q in quests if q <= m * 60)


def extrapolate(rows, quests, m):
    """기록 끝 20% 구간의 EXP·골드 기울기로 m분까지 이은 값(Lv 는 누적 EXP 로 역산)."""
    end = rows[-1][0]
    start = max((r for r in rows if r[0] <= end * 0.8), key=lambda r: r[0], default=rows[0])
    dt = max(1.0, end - start[0])
    rate = (rows[-1][2] - start[2]) / dt
    cum = rows[-1][2] + rate * (m * 60 - end)
    lv, rest = 1, cum
    while lv < MAX_LEVEL and rest >= exp_to_next(lv):
        rest -= exp_to_next(lv)
        lv += 1
    grate = (rows[-1][3] - start[3]) / dt
    return lv, cum, max(0.0, rows[-1][3] + grate * (m * 60 - end)), sum(1 for q in quests if q <= end)


def ratio(b, h):
    return b / h if h else float("inf")


def font(size):
    from PIL import ImageFont
    for p in ("C:/Windows/Fonts/malgun.ttf", "C:/Windows/Fonts/arial.ttf"):
        try:
            return ImageFont.truetype(p, size)
        except OSError:
            pass
    return ImageFont.load_default()


def draw_png(path, bot, human, at, title):
    from PIL import Image, ImageDraw
    W, H = 1400, 900
    img = Image.new("RGB", (W, H), "white")
    d = ImageDraw.Draw(img)
    f12, f16 = font(14), font(20)
    d.text((20, 10), title, fill="black", font=f16)
    d.text((W - 330, 14), "파랑=봇   주황=사람(이어서 한 판)", fill="black", font=f12)
    panels = [("Lv", 1), ("누적 EXP", 2), ("골드", 3), ("퀘스트 완료(누적)", 4)]
    xmax = max([bot[0][-1][0] if bot[0] else 0, human[0][-1][0] if human[0] else 0, at * 60]) / 60.0
    for i, (name, col) in enumerate(panels):
        x0, y0 = 70 + (i % 2) * 690, 60 + (i // 2) * 420
        x1, y1 = x0 + 610, y0 + 340
        series = []
        for (rows, quests, _), color in ((bot, (30, 100, 220)), (human, (240, 140, 20))):
            if col == 4:
                pts = [(0.0, 0)]
                for n, q in enumerate(sorted(quests)):
                    pts += [(q / 60.0, n), (q / 60.0, n + 1)]
                pts.append((xmax, len(quests)))
            else:
                pts = [(r[0] / 60.0, r[col]) for r in rows]
            series.append((pts, color))
        ymax = max([p[1] for s, _ in series for p in s] + [1]) * 1.08
        d.rectangle([x0, y0, x1, y1], outline="black")
        d.text((x0, y0 - 22), name, fill="black", font=f16)
        for g in range(0, 5):
            gy = y1 - (y1 - y0) * g / 4
            d.line([x0, gy, x1, gy], fill=(225, 225, 225))
            d.text((x0 - 62, gy - 8), "%d" % (ymax * g / 4), fill="black", font=f12)
        step = 10 if xmax > 40 else 5
        for gm in range(0, int(xmax) + 1, step):
            gx = x0 + (x1 - x0) * gm / xmax
            d.line([gx, y1, gx, y1 + 5], fill="black")
            d.text((gx - 8, y1 + 7), str(gm), fill="black", font=f12)
        ax = x0 + (x1 - x0) * at / xmax
        for yy in range(int(y0), int(y1), 10):
            d.line([ax, yy, ax, yy + 4], fill=(120, 120, 120))
        d.text((ax - 20, y0 + 2), "%d분" % at, fill=(90, 90, 90), font=f12)
        for pts, color in series:
            xy = [(x0 + (x1 - x0) * p[0] / xmax, y1 - (y1 - y0) * p[1] / ymax) for p in pts]
            if len(xy) > 1:
                d.line(xy, fill=color, width=3)
    img.save(path)


def build(bot_path, human_paths, at):
    hp = [p if os.path.isabs(p) else os.path.join(ROOT, p) for p in human_paths]
    bot = load_series([bot_path], True)
    human = load_series(hp, False)
    L = ["# 진행 곡선: 봇 %s vs 사람 %d세션" % (os.path.basename(bot_path)[:-6], len(hp)), ""]
    L.append("- 봇 %.1f분 · 사람 %.1f분 (두 세션을 이어 붙임). 누적 EXP = 레벨 표 합 + 현재 레벨 EXP." % (bot[2] / 60, human[2] / 60))
    b_at = at_minute(bot[0], bot[1], at)
    h_at = at_minute(human[0], human[1], at)
    cp, note, extra = at, "", ""
    verdict, summary = "판정불가", "CURVE_SUMMARY 판정불가(" + ("봇 기록이 %d분보다 짧다" % at) + ")"
    if b_at is not None and h_at is None and human[0]:
        # 사람 기록이 짧다: 실제로 겹치는 마지막 분에서 판정하고, at 분 외삽값은 참고로만 적는다
        cp = int(min(bot[2], human[2]) / 60)
        ex = extrapolate(human[0], human[1], at)
        note = "사람 기록이 %.0f분뿐이라 %d분 판정은 실제 겹친 %d분 지점으로 대신한다. %d분 사람 값(끝 20%% 구간 기울기 외삽, 참고): Lv %d · EXP %d · 골드 %d" % (
            human[2] / 60, at, cp, at, ex[0], round(ex[1]), round(ex[2]))
        extra = " 외삽참고 %d분 Lv %d/%d EXP %d/%d" % (at, b_at[0], ex[0], b_at[1], round(ex[1]))
        h_at = ex
    b_cp, h_cp = at_minute(bot[0], bot[1], cp), at_minute(human[0], human[1], cp)
    if b_at is not None and b_cp and h_cp:
        rl, re_ = ratio(b_cp[0], h_cp[0]), ratio(b_cp[1], h_cp[1])
        verdict = "합격" if PASS_LO <= rl <= PASS_HI and PASS_LO <= re_ <= PASS_HI else "불합격"
        summary = "CURVE_SUMMARY %s %d분 Lv %d/%d(%.2f배) EXP %d/%d(%.2f배) 골드 %d/%d 퀘스트 %d/%d%s" % (
            verdict, cp, b_cp[0], h_cp[0], rl, b_cp[1], h_cp[1], re_, b_cp[2], h_cp[2], b_cp[3], h_cp[3], extra)
    L.append("- **%d분 지점 판정: %s** (합격선 사람의 %.1f~%.1f배: Lv·누적 EXP 둘 다)" % (cp, verdict, PASS_LO, PASS_HI))
    if note:
        L.append("- " + note)
    L += ["", "| 분 | Lv 봇 | Lv 사람 | EXP 봇 | EXP 사람 | EXP 배율 | 골드 봇 | 골드 사람 | 퀘완 봇 | 퀘완 사람 |", "|---|---|---|---|---|---|---|---|---|---|"]
    top = int(max(bot[2], human[2], at * 60) / 60)
    for m in sorted(set(list(range(5, top + 1, 5)) + [at])):
        b, h = at_minute(bot[0], bot[1], m), at_minute(human[0], human[1], m)
        hx = ""
        if h is None and human[0] and m == at:
            h, hx = extrapolate(human[0], human[1], m), "(외삽)"

        def cell(v, i):
            return ("%d" % round(v[i])) if v else "-"
        L.append("| %d%s | %s | %s | %s | %s%s | %s | %s | %s | %s | %s |" % (
            m, " **←합격선**" if m == at else "", cell(b, 0), cell(h, 0), cell(b, 1), cell(h, 1), hx,
            ("%.2f" % ratio(b[1], h[1])) if b and h else "-", cell(b, 2), cell(h, 2), cell(b, 3), cell(h, 3)))
    L += ["", summary]
    return bot, human, L, summary


def main(argv):
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    bot_path, human, png, out, at, i = None, [], None, None, 60, 1
    while i < len(argv):
        a = argv[i]
        if a == "--human":
            i += 1
            while i < len(argv) and not argv[i].startswith("-"):
                human.append(argv[i])
                i += 1
            continue
        if a == "--png":
            png, i = argv[i + 1], i + 2
        elif a == "-o":
            out, i = argv[i + 1], i + 2
        elif a == "--at":
            at, i = int(argv[i + 1]), i + 2
        else:
            bot_path, i = a, i + 1
    if not bot_path or not bot_path.endswith(".jsonl"):
        print(__doc__)
        return 2
    bot, hum, lines, summary = build(bot_path, human or HUMAN_DEFAULT, at)
    base = bot_path[:-6]
    out = out or base + "_curve.md"
    png = png or base + "_curve.png"
    try:
        draw_png(png, bot, hum, at, "진행 곡선 %s" % os.path.basename(base))
        lines.insert(1, "![곡선](%s)" % os.path.basename(png))
    except ImportError:
        lines.insert(1, "(Pillow 없음 — PNG 생략)")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    print(summary)
    print("wrote %s %s" % (out, png))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
