"""마스터 모션 이펙트(10-29) — 기본 3타 v7 스미어(gen_smear.py)와 같은 림버스 문법으로, 새 동작마다 셀에 기록된 실제 칼끝 궤적에 맞춘 타원 고리·바늘을 절차 생성한다.
기본 3타(Stab·SwingDown·SwingUp)는 건드리지 않는다(보존 태그 keep/basic-motion-fx-v7).
출력: Assets/Art/PixelFx/Px_MSmear<슬롯>.png/json + Assets/Script/Combat/Vfx/MotionFxTable.cs(동작 → 타격 프레임·스미어 슬롯·받침 모양, 지면 충격 먼지).
사용: python Tools/pixelfx/gen_motion_fx.py → Unity 에서 Tools/probes/build_motion_fx.cs
"""
import json
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gen_smear import BLADE, CEL, ELL_FRAMES, H, NEEDLE_FRAMES, OUT, PIV, W, teeth  # noqa: E402

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
TABLE = os.path.join(ROOT, "Assets", "Script", "Combat", "Vfx", "MotionFxTable.cs")
CX, CY = 41, 54  # 셀 원점(발 가운데) — 셀 좌표(y 아래) → 피벗 기준(y 위)

# (동작, 시작 프레임, 도착 프레임(= 띄우는 프레임), 종류, 방향 +1 반시계 / -1 시계, 머리 두께)
# 방향: 각도 0 앞·90 위 기준. 위 → 앞 아래로 내려치면 시계(-1), 뒤 → 아래 → 앞으로 휘두르면 반시계(+1)
STRIKES = [
    ("GreatDown", 3, 5, "arc", -1, 9.0),  # 10-29 규격 시험 v2: 세움(3) → 사이(4) → 타격(5)
    ("GreatSide", 1, 3, "arc", +1, 8.0),  # 규격 2묶음: 뒤(1) → 옆(2) → 앞(3)
    ("GreatSideOne", 1, 3, "arc", +1, 7.0),
    ("DaggerCross", 1, 3, "arc", -1, 6.0),  # 규격 6묶음: 머리 위(1) → X 교차(3)
    ("DaggerCross", 3, 5, "arc", -1, 5.0),  # 끝까지 흘림(5)
    ("Spin", 1, 3, "arc", +1, 7.0),  # 규격 4묶음: 1타 뒤 → 아래 → 앞
    ("Spin", 3, 8, "arc", +1, 7.0),  # 2타 앞 → 위 → 뒤 → 아래 → 앞(한 바퀴)
    ("LeapSlam", 4, 5, "arc", -1, 9.0),
    ("Iaido", 3, 4, "arc", +1, 7.0),
    ("AirSlash", 1, 3, "arc", -1, 7.0),  # 규격 5묶음: 머리 위(1) → 사이(2) → 벰(3)
    ("Slam", 4, 5, "arc", -1, 8.0),
    ("Lunge", 1, 2, "needle", 0, 6.0),  # 규격 5묶음: 몸째 뻗는 순간(2)
    ("DaggerFlurry", 1, 2, "needle", 0, 4.0),  # 규격 4묶음: 찌르기 3번(2·4·7)
    ("DaggerFlurry", 3, 4, "needle", 0, 4.0),
    ("DaggerFlurry", 6, 7, "needle", 0, 5.5),
    ("Punch", 2, 3, "needle", 0, 4.0),  # 규격 6묶음: 뻗은 순간(3)
]
# 지면 충격(기존 도트 VfxId, 발 앞 거리 px): (동작, 프레임, VfxId, 앞 거리, 크기)
IMPACTS = [
    ("GreatDown", 5, "GroundShock", 22, 0.8),
    ("LeapSlam", 5, "GroundShock", 20, 1.0),
    ("LeapSlam", 5, "DustBrown", 6, 0.9),
    ("Slam", 5, "GroundShock", 18, 0.9),
    ("ShieldBash", 3, "DustBrown", -4, 0.7),
    ("Iaido", 4, "StepDust", -6, 1.0),
    ("Lunge", 2, "StepDust", -6, 0.9),
    ("Backstep", 4, "StepDust", 0, 0.9),
]


def unwrap(a0, a1, sgn):
    d = a1 - a0
    if sgn > 0:
        while d <= 0:
            d += 360
    elif sgn < 0:
        while d >= 0:
            d -= 360
    return a0 + d


def tip_path(fr, f0, f1, sgn, n=12):
    pts, ang = [], fr[f0]["angle"]
    for k in range(f0, f1):
        h0, h1 = fr[k]["hand"], fr[k + 1]["hand"]
        a0 = ang
        a1 = unwrap(a0, fr[k + 1]["angle"], sgn)
        for j in range(n + 1):
            t = j / n
            h = (h0[0] + (h1[0] - h0[0]) * t, h0[1] + (h1[1] - h0[1]) * t)
            a = math.radians(a0 + (a1 - a0) * t)
            pts.append((h[0] + BLADE * math.cos(a) - CX, CY - (h[1] - BLADE * math.sin(a))))  # 피벗 기준(y 위)
        ang = a1
    return pts, ang - fr[f0]["angle"]


def fit(pts):
    p = np.array(pts)
    A = np.c_[2 * p[:, 0], 2 * p[:, 1], np.ones(len(p))]
    c = np.linalg.lstsq(A, (p ** 2).sum(1), rcond=None)[0]
    return c[0], c[1], math.sqrt(c[2] + c[0] ** 2 + c[1] ** 2)


def arc_shape(fr, f0, f1, sgn):
    pts, swept = tip_path(fr, f0, f1, sgn)
    cx, cy, r = fit(pts)
    if f1 - f0 == 1:
        # 키 포즈 2장 사이(규격 시험 10-29): 손이 크게 옮겨 가 원 맞춤이 몸 밖으로 튄다 → 두 손의 가운데를 중심, 칼끝까지 평균 거리를 반지름
        hx = sum(fr[k]["hand"][0] for k in (f0, f1)) / 2 - CX
        hy = CY - sum(fr[k]["hand"][1] for k in (f0, f1)) / 2
        cx, cy = hx, hy
        r = sum(math.hypot(x - cx, y - cy) for x, y in pts) / len(pts)
    r = min(max(r, 16.0), 34.0)  # 손 위치 흔들림으로 원이 너무 작거나 커지지 않게
    tx, ty = pts[-1]
    head = math.degrees(math.atan2(ty - cy, tx - cx))
    tail = min(300.0, max(150.0, abs(swept) * 1.15))
    return dict(cx=round(cx, 1), cy=round(cy, 1), ea=round(r + 4, 1), eb=round((r + 4) * 0.8, 1), tilt=0.0, head=round(head, 1), sgn=sgn, tail=round(tail, 1))


def to_rgba(ims):
    out = []
    for im in ims:
        v = np.array(im)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(v > 0, 255, 0)
        out.append(img)
    return out


def sheet(p):
    return (PIV[0] + p[0], PIV[1] - p[1])


def arc_frames(s, wmax):
    """gen_smear.ellipse_frames 와 같은 그림(머리 두껍고 꼬리 바늘·안쪽 톱니·바깥 날 1px)."""
    ct, st = math.cos(math.radians(s["tilt"])), math.sin(math.radians(s["tilt"]))

    def pt(phi):
        x, y = s["ea"] * math.cos(phi), s["eb"] * math.sin(phi)
        return s["cx"] + x * ct - y * st, s["cy"] + x * st + y * ct

    def normal(phi):
        nx, ny = math.cos(phi) / s["ea"], math.sin(phi) / s["eb"]
        n = math.hypot(nx, ny)
        return (nx / n) * ct - (ny / n) * st, (nx / n) * st + (ny / n) * ct

    ims = []
    for k, (tl, wk) in enumerate(ELL_FRAMES):
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        h0 = math.radians(s["head"] + s["sgn"] * 6 * k)
        n = 90
        outer, inner, mid = [], [], []
        for j in range(n + 1):
            u = j / n
            phi = h0 - s["sgn"] * math.radians(s["tail"] * tl) * (1 - u)
            w = wmax * wk * (u ** 1.5) * (1 - max(0.0, u - 0.94) / 0.06 * 0.8) * teeth(u)
            p, nn = pt(phi), normal(phi)
            outer.append(sheet(p))
            inner.append(sheet((p[0] - nn[0] * w, p[1] - nn[1] * w)))
            mid.append(sheet((p[0] - nn[0] * w * 0.72, p[1] - nn[1] * w * 0.72)))
        g.polygon(outer + inner[::-1], fill=95)
        g.polygon(outer + mid[::-1], fill=150)
        g.line(outer[int(n * 0.25):], fill=255, width=1)
        ims.append(im)
    return to_rgba(ims)


def needle_shape(fr, f1):
    h, a = fr[f1]["hand"], math.radians(fr[f1]["angle"])
    hx, hy = h[0] - CX, CY - h[1]
    return dict(x0=round(hx - 10 * math.cos(a), 1), y0=round(hy - 10 * math.sin(a), 1),
                x1=round(hx + (BLADE + 26) * math.cos(a), 1), y1=round(hy + (BLADE + 26) * math.sin(a), 1))


def needle_frames(s, wmax):
    """gen_smear.needle_frames 를 임의 방향으로: 70% 지점이 가장 두껍고 앞이 바늘처럼 뾰족, 뒤에서부터 줄며 앞으로."""
    dx, dy = s["x1"] - s["x0"], s["y1"] - s["y0"]
    L = math.hypot(dx, dy)
    ux, uy = dx / L, dy / L
    nx, ny = -uy, ux
    ims = []
    for cut, wk, adv in NEEDLE_FRAMES:
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        top, bot, mid = [], [], []
        for j in range(61):
            u = j / 60
            if u < cut:
                continue
            bx, by = s["x0"] + dx * u + ux * adv, s["y0"] + dy * u + uy * adv
            prof = (u / 0.7) ** 1.3 if u < 0.7 else ((1 - u) / 0.3) ** 0.8
            w = wmax * wk * prof
            top.append(sheet((bx + nx * w * 0.55, by + ny * w * 0.55)))
            bot.append(sheet((bx - nx * w * 0.45, by - ny * w * 0.45)))
            mid.append(sheet((bx + nx * w * 0.15, by + ny * w * 0.15)))
        if len(top) >= 2:
            g.polygon(top + bot[::-1], fill=95)
            g.polygon(top + mid[::-1], fill=150)
            g.line(top[len(top) // 4:], fill=255, width=1)
        ims.append(im)
    return to_rgba(ims)


def cs_float(v):
    return f"{v:g}f"


def main():
    cel = json.load(open(CEL))
    rows, imp = [], []
    for i, (anim, f0, f1, kind, sgn, wmax) in enumerate(STRIKES):
        fr = cel[anim]["frames"]
        name = f"Px_MSmear{i}"
        if kind == "arc":
            s = arc_shape(fr, f0, f1, sgn)
            frames = arc_frames(s, wmax)
            a1 = s["head"]
            a0 = a1 - sgn * s["tail"]
            shape = (f"new SmearAura.Shape {{ arc = true, c = new Vector2({cs_float(s['cx'])}, {cs_float(s['cy'])}), ea = {cs_float(s['ea'])}, "
                     f"eb = {cs_float(s['eb'])}, tilt = {cs_float(s['tilt'])}, a0 = {cs_float(a0)}, a1 = {cs_float(a1)} }}")
        else:
            s = needle_shape(fr, f1)
            frames = needle_frames(s, wmax)
            shape = (f"new SmearAura.Shape {{ arc = false, p0 = new Vector2({cs_float(s['x0'])}, {cs_float(s['y0'])}), "
                     f"p1 = new Vector2({cs_float(s['x1'])}, {cs_float(s['y1'])}) }}")
        Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
        with open(os.path.join(OUT, name + ".json"), "w") as fp:
            json.dump({"frames": len(frames), "w": W, "h": H, "fps": 36, "pivot": [PIV[0] / W, (H - (PIV[1] + 1)) / H], "ramp": "gray",
                       "source": "Tools/pixelfx/gen_motion_fx.py"}, fp)
        rows.append(f'        new Strike("{anim}", {f1}, VfxId.MSmear{i}, {shape}),')
        print(name, anim, f0, f1, kind, s)
    for anim, f, vid, ahead, scale in IMPACTS:
        imp.append(f'        new Impact("{anim}", {f}, VfxId.{vid}, {cs_float(ahead)}, {cs_float(scale)}),')
    src = TABLE_CS.replace("{STRIKES}", "\n".join(rows)).replace("{IMPACTS}", "\n".join(imp)).replace("{N}", str(len(STRIKES)))
    with open(TABLE, "w", encoding="utf-8-sig", newline="\r\n") as fp:
        fp.write(src)
    print(TABLE)


TABLE_CS = """using UnityEngine;

/// <summary>
/// 마스터 모션 이펙트 표 — Tools/pixelfx/gen_motion_fx.py 가 생성한다(손으로 고치지 말 것).
/// 셀 동작이 재생되면 타격 프레임에 스미어(셀 좌표 타원 고리·바늘, 림버스 문법)와 지면 충격을 띄운다. 기본 3타는 Weapon 이 따로 띄우므로 여기 없다.
/// </summary>
public static class MotionFxTable
{
    public readonly struct Strike
    {
        public readonly string anim;
        public readonly int frame;
        public readonly VfxId id;
        public readonly SmearAura.Shape shape;

        public Strike(string anim, int frame, VfxId id, SmearAura.Shape shape)
        {
            this.anim = anim;
            this.frame = frame;
            this.id = id;
            this.shape = shape;
        }
    }

    public readonly struct Impact
    {
        public readonly string anim;
        public readonly int frame;
        public readonly VfxId id;
        /// <summary>발 앞 거리(셀 px, 바라보는 쪽 +)</summary>
        public readonly float ahead;
        public readonly float scale;

        public Impact(string anim, int frame, VfxId id, float ahead, float scale)
        {
            this.anim = anim;
            this.frame = frame;
            this.id = id;
            this.ahead = ahead;
            this.scale = scale;
        }
    }

    public const int SlotCount = {N};

    public static readonly Strike[] Strikes =
    {
{STRIKES}
    };

    public static readonly Impact[] Impacts =
    {
{IMPACTS}
    };

    public static bool IsMotionSmear(VfxId id)
    {
        return id >= VfxId.MSmear0 && id < VfxId.MSmear0 + SlotCount;
    }

    public static bool TryShape(VfxId id, out SmearAura.Shape shape)
    {
        foreach (Strike s in Strikes)
        {
            if (s.id == id)
            {
                shape = s.shape;
                return true;
            }
        }
        shape = default;
        return false;
    }
}
"""

if __name__ == "__main__":
    main()
