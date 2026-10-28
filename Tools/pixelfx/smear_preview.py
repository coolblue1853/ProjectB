"""칼 스미어 + 받침 레이어(글로우·칼끝 불꽃) 오프라인 합성 미리보기(10-28 사용자: 도트로 큰 형태, 세부는 셰이더·파티클로 감싸기).
인게임과 같은 규칙으로 흉내 낸다: 셀 프레임 시간(cel.json time) · 타격 프레임에서 스미어 시작(WeaponFx.StrikeFrame) ·
PixelSlash2D 5단 램프 + 바깥 1텍셀 테두리 · SmearGlow2D(텍셀 고정 원판 블러, 단계 양자화, 가산) · SmearAura 불꽃(호 접선 방향 버스트).
사용: python Tools/pixelfx/smear_preview.py <출력 폴더> [variant]   → preview_<동작>.gif(실속도) + _slow.gif(4배 느리게) + board.png
variant: base(스미어만) / glow / full(글로우+불꽃)
"""
import json
import math
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "..")
CELDIR = os.path.join(ROOT, "Assets", "Art", "Player", "PixelRig", "Cel")
FX = os.path.join(ROOT, "Assets", "Art", "PixelFx")
W, H, OX, OY = 112, 80, 24, 16
PX, PY = 41 + OX, 54 + OY  # 피벗(셀 원점) 시트 좌표
WHITE = [0x1E2846, 0x3C64AA, 0x8CB4F0, 0xD2E6FF, 0xFFFFFF]  # PixelRampId.Steel(기본 칼 스미어, 10-28)
BG = (38, 44, 54)
STRIKE = {"Stab": 4, "SwingDown": 4, "SwingUp": 5}
SMEAR = {"Stab": "Px_WSmearStab", "SwingDown": "Px_WSmearDown", "SwingUp": "Px_WSmearUp"}
# SmearAura.cs 와 같은 값(셀 px, 피벗 기준, y 위 +). 호: 중심·반지름·시작각→끝각 / 선: 시작점→끝점
ARC = {
    "SwingDown": ("arc", (-1.0, 11.4), 32.7, 137.0, -15.5),
    "SwingUp": ("arc", (1.9, 22.3), 24.2, -49.7, 148.9),
    "Stab": ("line", (14.0, 18.0), (46.0, 19.0)),
}
GLOW = dict(radius=5.5, intensity=2.2, steps=4, alpha=0.45, tail=0.0, env=[1.0, 0.8, 0.6, 0.45], inner=8.0)
# 바람 호: 스미어 바깥을 감싸는 1px 선 2줄(반지름 +off), 진행 끝쪽 구간이 진행 방향으로 미끄러지며 꼬리부터 줄어든다
WIND = dict(offs=(4.0, 7.0, -4.0, -8.0), los=(0.45, 0.6, 0.3, 0.55), span=(0.45, 1.0), slide=18.0, life=0.10)
SPARK = dict(count=12, speed=(70, 160), out=(10, 45), life=(0.12, 0.28), drag=6.0, grav=-40.0)


def hexc(v):
    return np.array([(v >> 16) & 255, (v >> 8) & 255, v & 255], np.float32)


RAMP = [hexc(v) for v in WHITE]


def load_sheet(name):
    meta = json.load(open(os.path.join(FX, name + ".json")))
    sh = np.array(Image.open(os.path.join(FX, name + ".png")).convert("RGBA")).astype(np.float32)
    return [sh[:, i * W:(i + 1) * W] for i in range(meta["frames"])], meta["fps"]


TONE = dict(levels={}, flash="up")  # 시트가 이미 255/150/95(gen_smear 10-28), 섬광 = 램프 2칸 위(Vfx.FlashRamp)


def themed(fr, white_flash):
    """PixelSlash2D: 밝기 → 5단 평면색, 바깥 1텍셀 = C1. TONE.levels = gen_smear 회색 3단 다시 매핑, flash = white(전부 흰색) / up(램프 2칸 위)."""
    a = fr[..., 3] > 0
    g = fr[..., 0].copy()
    for k, v in TONE["levels"].items():
        g[fr[..., 0] == k] = v
    lum = np.clip(g / 255 + 0.05, 0, 1)
    idx = (lum >= 0.25).astype(int) + (lum >= 0.5) + (lum >= 0.72) + (lum >= 0.9)
    col = np.stack([RAMP[i] for i in range(5)])[idx]
    nb = a.copy()
    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
        nb &= np.roll(a, (dy, dx), (0, 1))
    col[a & ~nb] = RAMP[1]
    if white_flash and TONE["flash"] == "white":
        col[:] = 255
    elif white_flash and TONE["flash"] == "up":
        col = np.stack([RAMP[min(4, i + 2)] for i in range(5)])[idx]
        col[a & ~nb] = RAMP[2]
    return col, a


def glow_map(fr, mask=None):
    """SmearGlow2D 와 같은 식: 텍셀 중심에서 원판 17탭(중심 1 + 반경 r/2·r 각 8방향), 밝기×알파 가중 평균 → ×세기 → 단계 양자화."""
    src = (fr[..., 3] / 255) * np.clip(fr[..., :3].max(2) / 255, 0, 1)
    r = GLOW["radius"]
    acc = src.copy()
    wsum = 1.0
    for rr, wt in ((r * 0.5, 0.7), (r, 0.4)):
        for k in range(8):
            a = k * math.pi / 4
            dx, dy = int(round(rr * math.cos(a))), int(round(rr * math.sin(a)))
            sh = np.zeros_like(src)
            ys, xs = slice(max(dy, 0), H + min(dy, 0)), slice(max(dx, 0), W + min(dx, 0))
            yd, xd = slice(max(-dy, 0), H + min(-dy, 0)), slice(max(-dx, 0), W + min(-dx, 0))
            sh[yd, xd] = src[ys, xs]
            acc += sh * wt
            wsum += wt
    g = np.clip(acc / wsum * GLOW["intensity"], 0, 1)
    if mask is not None:
        g *= mask
    s = GLOW["steps"]
    return np.floor(g * s + 0.35) / s


def sparks(clip, rng):
    kind = ARC[clip]
    out = []
    for _ in range(SPARK["count"]):
        if kind[0] == "arc":
            _, c, r, a0, a1 = kind
            t = rng.random() ** 0.6  # 진행 끝쪽에 많이
            a = math.radians(a0 + (a1 - a0) * t)
            rad = r + rng.uniform(-2, 3)
            p = (c[0] + rad * math.cos(a), c[1] + rad * math.sin(a))
            sgn = -1 if a1 < a0 else 1  # 시계 방향이면 접선 (sin, -cos)
            tang = (-math.sin(a) * sgn, math.cos(a) * sgn)
            radial = (math.cos(a), math.sin(a))
        else:
            _, p0, p1 = kind
            t = rng.random() ** 0.5
            p = (p0[0] + (p1[0] - p0[0]) * t, p0[1] + (p1[1] - p0[1]) * t + rng.uniform(-2, 2))
            tang = (1.0, 0.0)
            radial = (0.0, 1.0 if rng.random() < 0.5 else -1.0)
        sp, ou = rng.uniform(*SPARK["speed"]), rng.uniform(*SPARK["out"])
        v = (tang[0] * sp + radial[0] * ou, tang[1] * sp + radial[1] * ou)
        out.append(dict(p=p, v=v, life=rng.uniform(*SPARK["life"]), big=rng.random() < 0.3))
    return out


def spark_pos(s, t):
    k = SPARK["drag"]
    f = (1 - math.exp(-k * t)) / k
    return (s["p"][0] + s["v"][0] * f, s["p"][1] + s["v"][1] * f + 0.5 * SPARK["grav"] * t * t)


def arc_mask(clip):
    """글로우는 호 바깥쪽만(반지름 - inner 부터 4px 에 걸쳐 켜짐) — 안쪽 머리·몸을 덮어 뿌옇게 하지 않는다."""
    k = ARC[clip]
    if k[0] != "arc":
        return None
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.hypot(xx + 0.5 - (PX + k[1][0]), (PY - k[1][1]) - (yy + 0.5))
    return np.clip((d - (k[2] - GLOW["inner"])) / 4.0, 0, 1)


def wind_lines(clip, ts):
    """바람 호 점 목록(시트 좌표, 밝기)."""
    k = ARC[clip]
    life = WIND["life"]
    if ts < 0 or ts >= life:
        return []
    u = ts / life
    pts = []
    for n, off in enumerate(WIND["offs"]):
        lo, hi = WIND.get("los", [WIND["span"][0]] * len(WIND["offs"]))[n], WIND["span"][1]
        lo = lo + (hi - lo) * u  # 꼬리부터 줄어든다
        if k[0] == "arc":
            _, c, r, a0, a1 = k
            sgn = 1 if a1 > a0 else -1
            for j in range(60):
                t = lo + (hi - lo) * j / 59
                a = math.radians(a0 + (a1 - a0) * t + sgn * WIND["slide"] * u)
                rr = r + off
                pts.append((PX + c[0] + rr * math.cos(a), PY - (c[1] + rr * math.sin(a)), 1 - u))
        else:
            _, p0, p1 = k
            for j in range(60):
                t = lo + (hi - lo) * j / 59
                pts.append((PX + p0[0] + (p1[0] - p0[0]) * t + 14 * u, PY - (p0[1] + (off if n == 0 else -off * 0.8)), 1 - u))
    return pts


CUT = dict(life=0.24, lo=0.05, delay=0.0)  # SmearCut


def cut_line(clip, ts):
    """칼선 점 목록(시트 좌표, 밝기 0~1). 앞쪽(진행 끝) 밝고 꼬리 흐림."""
    life = CUT["life"]
    t = ts - CUT["delay"]
    if life <= 0 or t < 0 or t >= life:
        return []
    u = t / life
    lo = CUT["lo"] + (1 - CUT["lo"]) * u ** 1.4
    k = ARC[clip]
    pts = []
    n = 120
    for j in range(n):
        q = lo + (1 - lo) * j / (n - 1)
        b = (0.35 + 0.65 * (j / (n - 1))) * (1 - u * 0.5)
        if k[0] == "arc":
            _, c, r, a0, a1 = k
            a = math.radians(a0 + (a1 - a0) * q)
            pts.append((PX + c[0] + r * math.cos(a), PY - (c[1] + r * math.sin(a)), b))
        else:
            _, p0, p1 = k
            pts.append((PX + p0[0] + (p1[0] + 6 - p0[0]) * q, PY - (p0[1] + 1), b))
    return pts


def render(clip, variant, step=0.02):
    cel = json.load(open(os.path.join(CELDIR, "cel.json")))[clip]["frames"]
    times = [f["time"] for f in cel]
    starts = np.cumsum([0] + times)
    frames, fps = load_sheet(SMEAR[clip])
    t_smear = starts[STRIKE[clip]]
    dur = len(frames) / fps
    rng = np.random.default_rng(7)
    sp = sparks(clip, rng) if variant == "full" else []
    total = starts[-1] + 0.05
    out = []
    t = 0.0
    while t < total:
        ci = int(np.searchsorted(starts, t, side="right") - 1)
        ci = min(ci, len(cel) - 1)
        can = np.zeros((H, W, 3), np.float32) + np.array(BG, np.float32)
        celim = np.array(Image.open(os.path.join(CELDIR, cel[ci]["png"] + ".png")).convert("RGBA")).astype(np.float32)
        a = celim[..., 3:] / 255
        can[OY:OY + 64, OX:OX + 64] = can[OY:OY + 64, OX:OX + 64] * (1 - a) + celim[..., :3] * a
        ts = t - t_smear
        if 0 <= ts < dur + (GLOW["tail"] if variant != "base" else 0):
            fi = min(len(frames) - 1, int(ts * fps))
            fr = frames[fi]
            if variant != "base":
                env = GLOW["env"][min(fi, len(GLOW["env"]) - 1)]
                if ts >= dur:
                    env *= 1 - (ts - dur) / GLOW["tail"]
                g = glow_map(fr, arc_mask(clip))[..., None] * GLOW["alpha"] * env
                can = can + RAMP[2] * g  # 가산
            if ts < dur:
                col, m = themed(fr, fi == 0)
                can[m] = col[m]
        if variant == "full":
            for x, y, b in wind_lines(clip, ts):
                xi, yi = int(x), int(y)
                if 0 <= xi < W and 0 <= yi < H:
                    can[yi, xi] = np.maximum(can[yi, xi], can[yi, xi] + RAMP[3] * 0.7 * b)
        seen = set()
        for x, y, b in cut_line(clip, ts):
            xi, yi = int(x), int(y)
            if (xi, yi) in seen or not (0 <= xi < W and 0 <= yi < H):
                continue
            seen.add((xi, yi))
            c = RAMP[4] if b > 0.6 else RAMP[3]
            can[yi, xi] = can[yi, xi] * (1 - b) + c * b
        for s in sp:
            if 0 <= ts < s["life"]:
                x, y = spark_pos(s, ts)
                fade = 1 - ts / s["life"]
                sx, sy = int(round(PX + x)), int(round(PY - y))
                c = RAMP[4] if fade > 0.5 else RAMP[3] if fade > 0.25 else RAMP[2]
                for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1)) if s["big"] and fade > 0.5 else ((0, 0),):
                    if 0 <= sx + dx < W and 0 <= sy + dy < H:
                        can[sy + dy, sx + dx] = can[sy + dy, sx + dx] + c * (0.6 + 0.4 * fade)
        out.append(Image.fromarray(np.clip(can, 0, 255).astype(np.uint8)).resize((W * 4, H * 4), Image.NEAREST))
        t += step
    return out


def strike_index(clip, step=0.02):
    cel = json.load(open(os.path.join(CELDIR, "cel.json")))[clip]["frames"]
    return int(round(sum(f["time"] for f in cel[:STRIKE[clip]]) / step))


def main():
    """variants: 쉼표로 여러 개(base,full …). 'full+{json}' 꼴이면 GLOW·SPARK 값을 덮어쓴다. board.png = 동작×변형 행, 타격 직전부터 0.02초 간격 8칸."""
    od = sys.argv[1]
    variants = sys.argv[2].split(";") if len(sys.argv) > 2 else ["base", "full"]
    os.makedirs(od, exist_ok=True)
    g0, s0, w0, t0, c0 = dict(GLOW), dict(SPARK), dict(WIND), dict(TONE), dict(CUT)
    rows = []
    for clip in ("Stab", "SwingDown", "SwingUp"):
        for vi, v in enumerate(variants):
            name, _, over = v.partition("+")
            GLOW.clear(); GLOW.update(g0); SPARK.clear(); SPARK.update(s0); WIND.clear(); WIND.update(w0)
            TONE.clear(); TONE.update(t0); CUT.clear(); CUT.update(c0)
            RAMP[:] = [hexc(c) for c in WHITE]
            if over:
                o = json.loads(over)
                GLOW.update(o.get("glow", {})); SPARK.update(o.get("spark", {})); WIND.update(o.get("wind", {}))
                CUT.update(o.get("cut", {}))
                if "levels" in o:
                    TONE["levels"] = {int(k): v for k, v in o["levels"].items()}
                if "flash" in o:
                    TONE["flash"] = o["flash"]
                if "ramp" in o:
                    RAMP[:] = [hexc(int(c, 16)) for c in o["ramp"]]
            fr = render(clip, name)
            tag = f"{name}{vi}"
            fr[0].save(f"{od}/preview_{clip}_{tag}.gif", save_all=True, append_images=fr[1:], duration=20, loop=0)
            fr[0].save(f"{od}/preview_{clip}_{tag}_slow.gif", save_all=True, append_images=fr[1:], duration=120, loop=0)
            k = strike_index(clip)
            pick = [min(len(fr) - 1, k + d) for d in range(-1, 7)]
            w, h = fr[0].width // 2, fr[0].height // 2
            row = Image.new("RGB", (len(pick) * w, h))
            for j, i in enumerate(pick):
                row.paste(fr[i].resize((w, h), Image.NEAREST), (j * w, 0))
            rows.append(row)
    board = Image.new("RGB", (rows[0].width, sum(r.height + 4 for r in rows)), (255, 255, 255))
    y = 0
    for r in rows:
        board.paste(r, (0, y))
        y += r.height + 4
    board.save(f"{od}/board.png")
    print("ok", board.size)


if __name__ == "__main__":
    main()
