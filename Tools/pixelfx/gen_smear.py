"""기본 칼 3타 스미어(스컬 리틀 본식) — fx_plan.py 로 사용자 승인받은 면을 도트로 찍는다(10-28).
- 캐릭터 셀과 같은 좌표·같은 픽셀 크기(PPU 36)로 그리고, 피벗 = 셀 원점(발 가운데 41, 54)이라 셀과 정확히 겹친다.
  예전 궤적(Px_WSlashDown, 24 PPU)은 도트가 캐릭터보다 1.5배 굵고 위치를 판정 기준점에 맞춰 칼과 따로 놀았다.
- 프레임: ① 휘두르는 호 전체를 덮는 꽉 찬 초승달(안쪽 비움, 진행 끝쪽 두껍게) ② 바깥 가장자리 얇은 호 ③ 더 짧고 흐린 호. 20fps.
- 회색조 밝기(PixelSlash2D 가 무기 테마 램프로 칠한다): 진행 끝 1.0 → 꼬리 0.6, 바깥 가장자리 밝게, 안쪽 가장자리 0.7.
사용: python Tools/pixelfx/gen_smear.py   등록: Tools/probes/build_smear.cs
"""
import json
import math
import os

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "..")
OUT = os.path.join(ROOT, "Assets", "Art", "PixelFx")
KEYS = os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel", "basic_sword", "skel")
CEL = os.path.join(ROOT, "Assets", "Art", "Player", "PixelRig", "Cel", "cel.json")
W, H = 136, 80          # 셀(64)보다 넓게 — 큰 초승달·찌르기 바늘이 잘리지 않게(10-28 v7: 112 → 136, 오른쪽으로만)
OX, OY = 24, 16          # 셀 좌표 → 시트 좌표
PIV = (41 + OX, 54 + OY)  # 셀 원점(발 가운데)
BLADE = 18.5  # 칼끝까지(ShortSword 손잡이 피벗 → 끝, 24 PPU = 셀 1px 과 같은 크기: 셀 PPU 36 × 플레이어 1.5배)
# v2(10-28 사용자 "궤적이랑 칼이 안 맞는다"): 어깨 중심 원이 아니라 셀에 기록된 실제 손·칼 각도로 칼끝이 지나간 길을 따라 그리고,
# 칼이 끝에 도착한 프레임에 띄운다(스컬: 칼이 이미 끝에 있고 지나온 길이 면으로 남는다). v1 은 칼보다 먼저 앞 아래까지 그려졌다.
# 이름: (동작, 시작 프레임, 끝 프레임(=띄우는 프레임), 종류, 최대 두께)
SPECS = {
    # v5(10-28 사용자: 품질 낮음·찌르기 미사일): 세 개 모두 vfx_workbench — 베기는 swing_slash 두께 15·5단 회색(안쪽 회색 띠, 스컬처럼 두껍게),
    # 내려베기 start -150·sweep 210(뒤 위 → 머리 위 → 앞 아래). 찌르기는 zigzag_slash 를 칼 방향 직선 경로(부드러운 모서리)로 — 칼에 붙은 넓은 잔상 띠(Guard 머리처럼), 발사체 X
    "Px_WSmearDown": ("SwingDown", 2, 4, "ellipse", 0),  # v7 림버스식 타원(10-28). 이전: pixellab:pb-downslash-v3:shoulder:36
    # v4(10-28 사용자: 올려베기 꺾임·찌르기 최악): PixelLab vfx_workbench(무료) 부품으로 교체 — swing_slash(start 60·sweep -210: 앞 아래 → 머리 위 → 뒤 위, 50fps — 칼이 0.06초 만에 지나가 30fps 면 궤적이 늦었다),
    # thrust_streak(×1.6). 원본 Docs/ArtCompare/WeaponFx/pixellab_v2, My effects PB_UpSlash_v2·PB_Thrust_v2. 기준점: 올려베기 = 칼 도는 중심(어깨), 찌르기 = 손
    "Px_WSmearUp": ("SwingUp", 3, 5, "ellipse", 0),  # v7 림버스식 타원(10-28). 이전: pixellab:pb-upslash-v3:shoulder:36. 스컬 리틀 본 AttackB·Pierce RisingSlash: 양 끝 뾰족·가운데(앞 위) 두꺼운 렌즈, 칼끝보다 조금 크게
    "Px_WSmearStab": ("Stab", 3, 4, "needle", 0),  # v7 림버스식 바늘(10-28). 이전: pixellab:pb-thrust-v3:hand:30
}


def blade_path(fr, f0, f1, n=18):
    """f0 → f1 프레임 사이 손·칼 각도를 이어 붙인 경로(셀 좌표). 각 점 = (손, 각도, 진행도 0~1)."""
    pts = []
    segs = f1 - f0
    for k in range(segs):
        h0, a0 = fr[f0 + k]["hand"], fr[f0 + k]["angle"]
        h1, a1 = fr[f0 + k + 1]["hand"], fr[f0 + k + 1]["angle"]
        for j in range(n + (1 if k == segs - 1 else 0)):
            t = j / n
            pts.append(((h0[0] + (h1[0] - h0[0]) * t, h0[1] + (h1[1] - h0[1]) * t), a0 + (a1 - a0) * t, (k + t) / segs))
    return pts


def at(h, ang, dist):
    return (h[0] + dist * math.cos(math.radians(ang)) + OX, h[1] - dist * math.sin(math.radians(ang)) + OY)


def sweep_frames(pts, maxw, lens=False):
    from PIL import ImageDraw
    frames = []
    for mode in range(3):
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        if mode == 0:
            # 꽉 찬 면: 칼끝 경로 ~ 안쪽 경로(진행 끝쪽이 두껍다), 진행도에 따라 밝기 0.6 → 1.0
            for k in range(len(pts) - 1):
                (h, a, s0), (h2, a2, s1) = pts[k], pts[k + 1]
                if lens:
                    w0 = maxw * math.sin(math.pi * min(1.0, s0)) ** 0.8
                    w1 = maxw * math.sin(math.pi * min(1.0, s1)) ** 0.8
                    out = 3.0  # 바깥으로 3px 더(스컬 초승달은 칼끝 바깥까지 덮는다)
                else:
                    w0 = maxw * min(1.0, s0 * 1.25) * (1 - max(0.0, s0 - 0.95) * 10)
                    w1 = maxw * min(1.0, s1 * 1.25) * (1 - max(0.0, s1 - 0.95) * 10)
                    out = 0.5
                poly = [at(h, a, BLADE + out), at(h2, a2, BLADE + out), at(h2, a2, BLADE + out - w1), at(h, a, BLADE + out - w0)]
                g.polygon(poly, fill=int(255 * (0.6 + 0.4 * s1)))
            tip = [at(h, a, BLADE + (3.0 if lens else 0.5)) for h, a, _ in pts]
            g.line(tip, fill=255, width=1)  # 바깥 가장자리(칼날 끝) 가장 밝게
        else:
            start = 0.4 if mode == 1 else 0.7
            tip = [at(h, a, BLADE + (3.0 if lens else 0.0)) for h, a, s in pts if s >= start]
            if len(tip) > 1:
                g.line(tip, fill=230 if mode == 1 else 200, width=2 if mode == 1 else 1)
        frames.append(im)
    return frames


def streak_frames(h, ang, prev_h, maxw):
    """스컬 Guard·Cyclone 찌르기: ① 팔 위 뒤에서 칼끝 앞으로 모이는 넓은 쐐기 ② 칼끝 앞 속도선 3줄 ③ 1줄."""
    from PIL import ImageDraw
    frames = []
    ca, sa = math.cos(math.radians(ang)), math.sin(math.radians(ang))
    nx, ny = -sa, -ca  # 칼 방향의 '위'(화면 좌표, y 아래로 +)
    tip = at(h, ang, BLADE + 10)
    for mode in range(3):
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        if mode == 0:
            back = at(prev_h, ang, -2)
            top = (back[0] + nx * maxw * 2.2, back[1] + ny * maxw * 2.2)
            low = (back[0] - nx * 1.5, back[1] - ny * 1.5)
            g.polygon([top, tip, low], fill=190)
            g.polygon([(back[0] + nx * maxw, back[1] + ny * maxw), tip, low], fill=235)
            g.line([low, tip], fill=255, width=1)
        else:
            lines = [(-3, 0.5, 14), (0, 0.2, 20), (3, 0.6, 12)] if mode == 1 else [(0, 0.8, 14)]
            for off, start, ln in lines:
                p0 = at(h, ang, BLADE * start + BLADE * 0.4)
                p0 = (p0[0] + nx * off, p0[1] + ny * off)
                p1 = (p0[0] + ca * ln, p0[1] - sa * ln)
                g.line([p0, p1], fill=230 if mode == 1 else 200, width=1)
        frames.append(im)
    return frames


PL = os.path.join(ROOT, "Docs", "ArtCompare", "WeaponFx", "pixellab_v2")


def cel_shift(clip, i, keys_hand):
    """셀은 프레임마다 sx·dy 로 정렬돼 있다 — 손 좌표 차이로 같은 이동을 준다."""
    cj = json.load(open(CEL))[clip]["frames"][i]
    return cj["hand"][0] - keys_hand[0], cj["hand"][1] - keys_hand[1]


# 붓 → 칼(10-28 사용자: "칼로 벤다기보다 붓으로 그리는 느낌"): 머리가 호를 따라 그려 나가는 성장 프레임을 빼고,
# 꽉 찬 프레임부터 바로 보여 준 뒤 가늘어지며 사라지게(스컬: 한 프레임에 호 전체 → 얇은 호 → 없음). 0부터 센 원본 프레임 번호
# 10-28 사용자: 동작이 큰 내려·올려베기는 중간 프레임이 한두 개 더 있으면 좋겠다 → 원본 3번(꽉 찬 호 다음)을 넣고, 끝의 둥근 뭉치(원본 6번)는 빼서 4프레임 36fps(10-28 사용자: 마지막이 둥글다)
PICK = {"pb-upslash-v3": [2, 3, 4, 5], "pb-downslash-v3": [2, 3, 4, 5], "pb-thrust-v3": [3, 4, 5, 6]}


def fit_tip_circle(fr, f0, f1, n=12):
    """칼끝이 f0 → f1 동안 지나간 길에 원을 맞춘다(중심 x, y, 반지름) — 이펙트 호를 칼 범위에 맞추는 기준(10-28 사용자: 칼 범위와 이펙트 범위가 안 맞음)."""
    pts = []
    for k in range(f0, f1):
        h0, a0 = fr[k]["hand"], fr[k]["angle"]
        h1, a1 = fr[k + 1]["hand"], fr[k + 1]["angle"]
        for j in range(n + 1):
            t = j / n
            h = (h0[0] + (h1[0] - h0[0]) * t, h0[1] + (h1[1] - h0[1]) * t)
            a = math.radians(a0 + (a1 - a0) * t)
            pts.append((h[0] + BLADE * math.cos(a), h[1] - BLADE * math.sin(a)))
    p = np.array(pts)
    A = np.c_[2 * p[:, 0], 2 * p[:, 1], np.ones(len(p))]
    c = np.linalg.lstsq(A, (p ** 2).sum(1), rcond=None)[0]
    return c[0], c[1], math.sqrt(c[2] + c[0] ** 2 + c[1] ** 2)


def outer_radius(name):
    """PixelLab 호의 바깥 반지름(피벗 기준, 꽉 찬 프레임)."""
    meta = json.load(open(os.path.join(PL, name + ".json")))
    f = meta["frames"][2]
    r = f["frame"]
    a = np.array(Image.open(os.path.join(PL, name + ".png")).convert("RGBA").crop((r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"])))[..., 3] > 0
    ys, xs = np.nonzero(a)
    return float(np.hypot(xs + 0.5 - f["pivot"]["x"] * r["w"], ys + 0.5 - f["pivot"]["y"] * r["h"]).max())


def pixellab_frames(name, anchor, scale=1.0):
    """PixelLab 시트를 셀 좌표 캔버스로: 효과 원점(anchor, 셀 좌표)에 시트의 피벗을 맞춘다. 회색 4단 → 밝기."""
    meta = json.load(open(os.path.join(PL, name + ".json")))
    sheet = Image.open(os.path.join(PL, name + ".png")).convert("RGBA")
    out = []
    for f in [meta["frames"][k] for k in PICK.get(name, range(len(meta["frames"])))]:
        r = f["frame"]
        im = sheet.crop((r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"]))
        if abs(scale - 1.0) > 0.02:
            im = im.resize((max(1, round(r["w"] * scale)), max(1, round(r["h"] * scale))), Image.NEAREST)
        px, py = round(f["pivot"]["x"] * im.width), round(f["pivot"]["y"] * im.height)
        can = Image.new("RGBA", (W, H))
        can.alpha_composite(im, (int(round(anchor[0] + OX - px)), int(round(anchor[1] + OY - py))))
        a = np.array(can)
        v = a[..., :3].max(axis=2)
        # 날카롭게(10-28 사용자: 너무 부드럽다): 회색 단계를 흰 몸통·밝은 회색 가장자리 2단으로 눌러 대비를 세운다
        v = np.where(v >= 235, 255, np.where(v >= 150, 150, 95)).astype(np.uint8)  # 10-28 "너무 밝기만": 몸 210→150(테마 중간색), 안쪽 160→95(진한 색). 흰색은 날 끝만  # 3단: 흰 날 · 밝은 회색 몸 · 안쪽 회색 띠(2단은 통째로 하얘 둥글게 보였다)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(a[..., 3] > 0, 255, 0)
        out.append(img)
    return out


# v7(10-28 림버스 기준): 원이 아니라 몸을 감싸는 기울어진 타원 고리. 머리(칼이 지금 있는 곳)가 두껍고 지나온 길로 긴 꼬리가 바늘처럼 가늘어진다.
# 프레임마다 꼬리가 머리 쪽으로 줄어들며 가늘어진다(림버스: 2~3프레임 보이고 사라짐). 회색 3단: 바깥 1px 255(날) · 몸 150 · 안쪽 95.
# ELLIPSE[동작] = (중심 x, y(셀 px, 피벗 기준 y 위 +), 가로 반지름, 세로 반지름, 기울기°, 머리 각°, 진행 방향 +1 반시계/-1 시계, 꼬리 길이°, 머리 두께)
ELLIPSE = {
    "SwingDown": (-1.0, 14.0, 38.0, 27.0, -12.0, -18.0, -1, 230.0, 8.0),
    "SwingUp": (2.0, 20.0, 31.0, 24.0, 10.0, 150.0, +1, 220.0, 7.0),
}
ELL_FRAMES = [(1.0, 1.0), (0.62, 0.85), (0.34, 0.6), (0.14, 0.35)]  # (꼬리 길이 비율, 두께 비율)


TEETH = (4, 0.6)  # 안쪽 가장자리 톱니(개수, 깊이) — 림버스 Fx_T_Shape_Slash_05·09·14: 바깥(날)은 곧고 안쪽은 꼬리 쪽을 향한 찢긴 톱니


def teeth(u):
    """머리 쪽 65% 구간에 꼬리 방향 톱니: 톱니 안에서 두께가 1 → 1-깊이 로 줄었다가 다음 톱니에서 다시 1."""
    n, depth = TEETH
    if u < 0.35 or u > 0.97:
        return 1.0
    f = ((u - 0.35) / 0.62 * n) % 1.0
    return 1.0 - depth * (1.0 - f) ** 2


def ellipse_frames(clip):
    from PIL import ImageDraw
    cx, cy, ea, eb, tilt, head, sgn, tail, wmax = ELLIPSE[clip]
    ct, st = math.cos(math.radians(tilt)), math.sin(math.radians(tilt))

    def pt(phi):
        x, y = ea * math.cos(phi), eb * math.sin(phi)
        return cx + x * ct - y * st, cy + x * st + y * ct

    def normal(phi):
        nx, ny = math.cos(phi) / ea, math.sin(phi) / eb  # 타원 바깥 법선(회전 전)
        n = math.hypot(nx, ny)
        nx, ny = nx / n, ny / n
        return nx * ct - ny * st, nx * st + ny * ct

    def sheet(p):
        return (PIV[0] + p[0], PIV[1] - p[1])

    out = []
    for k, (tl, wk) in enumerate(ELL_FRAMES):
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        h0 = math.radians(head + sgn * 6 * k)  # 머리가 조금씩 더 나아간다
        n = 90
        outer, inner, mid = [], [], []
        for j in range(n + 1):
            u = j / n  # 0 꼬리 → 1 머리
            phi = h0 - sgn * math.radians(tail * tl) * (1 - u)
            w = wmax * wk * (u ** 1.5) * (1 - max(0.0, u - 0.94) / 0.06 * 0.8)
            w *= teeth(u)
            p, nn = pt(phi), normal(phi)
            outer.append(sheet(p))
            inner.append(sheet((p[0] - nn[0] * w, p[1] - nn[1] * w)))
            mid.append(sheet((p[0] - nn[0] * w * 0.72, p[1] - nn[1] * w * 0.72)))
        g.polygon(outer + inner[::-1], fill=95)
        g.polygon(outer + mid[::-1], fill=150)
        edge = outer[int(n * 0.25):]
        g.line(edge, fill=255, width=1)
        out.append(im)
    res = []
    for im in out:
        v = np.array(im)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(v > 0, 255, 0)
        res.append(img)
    return res


# v7 찌르기(10-28 림버스식): 칼 방향으로 길게 뻗은 바늘 쐐기. 앞(칼끝 너머)이 바늘처럼 뾰족하고 몸통 70% 지점이 가장 두껍고 뒤로 가늘어진다.
# 프레임마다 뒤에서부터 줄어들며 앞으로 조금 더 나아간다. NEEDLE = (시작 x, 끝 x, y(셀 px, 피벗 기준 y 위 +), 최대 두께)
NEEDLE = (12.0, 58.0, 19.0, 5.0)
NEEDLE_FRAMES = [(0.0, 1.0, 0.0), (0.35, 0.85, 3.0), (0.62, 0.6, 5.0), (0.82, 0.35, 6.0)]  # (뒤에서 잘라낸 비율, 두께 비율, 앞으로 이동 px)


def needle_frames():
    from PIL import ImageDraw
    x0, x1, y, wmax = NEEDLE
    res = []
    for cut, wk, adv in NEEDLE_FRAMES:
        im = Image.new("L", (W, H), 0)
        g = ImageDraw.Draw(im)
        top, bot, mid = [], [], []
        n = 60
        for j in range(n + 1):
            u = j / n  # 0 뒤 → 1 앞
            if u < cut:
                continue
            x = x0 + (x1 - x0) * u + adv
            prof = (u / 0.7) ** 1.3 if u < 0.7 else ((1 - u) / 0.3) ** 0.8  # 70% 지점이 가장 두껍다
            w = wmax * wk * prof
            top.append((PIV[0] + x, PIV[1] - (y + w * 0.55)))
            bot.append((PIV[0] + x, PIV[1] - (y - w * 0.45)))
            mid.append((PIV[0] + x, PIV[1] - (y + w * 0.15)))
        if len(top) < 2:
            res.append(im)
            continue
        g.polygon(top + bot[::-1], fill=95)
        g.polygon(top + mid[::-1], fill=150)
        g.line(top[len(top) // 4:], fill=255, width=1)  # 위 가장자리 = 날
        res.append(im)
    out = []
    for im in res:
        v = np.array(im)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(v > 0, 255, 0)
        out.append(img)
    return out


def render(spec):
    clip, f0, f1, kind, maxw = spec
    fr = json.load(open(CEL))[clip]["frames"]
    if kind == "ellipse":
        return ellipse_frames(clip)
    if kind == "needle":
        return needle_frames()
    if kind.startswith("pixellab:"):
        _, name, where, _fps = kind.split(":")
        if where == "hand":
            anchor = fr[f1]["hand"]  # 찌르기 띠 가운데 = 뻗은 손(띠가 손 뒤 22px ~ 칼끝 너머까지)
            # 10-28 인게임: 손 뒤 띠가 얼굴을 덮었다 → 손 4px 뒤부터만(레퍼런스 찌르기 줄기는 손에서 시작)
            out = pixellab_frames(name, anchor)
            cut = int(anchor[0] + OX - 4)
            cy = anchor[1] + OY
            for im in out:
                im[:, :cut] = 0
                # 잘린 끝을 뾰족하게: 8px 에 걸쳐 띠 두께가 0 → 전부로 벌어진다(뭉툭한 사각 끝 X)
                rows = np.nonzero(im[..., 3].any(1))[0]
                half = (rows.max() - rows.min() + 1) / 2 if len(rows) else 0
                for x in range(cut, min(W, cut + 8)):
                    keep = half * (x - cut + 1) / 8
                    for y in range(H):
                        if abs(y + 0.5 - cy) > keep:
                            im[y, x] = 0
            return out
        else:
            # 호 중심 = 칼끝 경로에 맞춘 원의 중심, 크기 = 바깥 반지름이 칼끝보다 2px 바깥(10-28: 어깨 기준은 올려베기 호가 칼보다 8px 컸다)
            cx, cy, rad = fit_tip_circle(fr, f0, f1)
            anchor = (cx, cy)
            return pixellab_frames(name, anchor, (rad + 2) / outer_radius(name))
        return pixellab_frames(name, anchor)
    if kind in ("sweep", "lens"):
        frames = sweep_frames(blade_path(fr, f0, f1), maxw, lens=kind == "lens")
    else:
        frames = streak_frames(fr[f1]["hand"], fr[f1]["angle"], fr[f0]["hand"], maxw)
    out = []
    for im in frames:
        v = np.array(im)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(v > 0, 255, 0)
        out.append(img)
    return out


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, spec in SPECS.items():
        frames = render(spec)
        fps = int(spec[3].split(":")[3]) if spec[3].startswith("pixellab:") else 36 if spec[3] in ("ellipse", "needle") else 20
        Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
        pivot = [PIV[0] / W, (H - (PIV[1] + 1)) / H]  # 셀 임포트와 같은 식(PixelRigBuilder.ImportPart)
        with open(os.path.join(OUT, name + ".json"), "w") as fp:
            json.dump({"frames": len(frames), "w": W, "h": H, "fps": fps, "pivot": pivot, "ramp": "gray",
                       "source": "Tools/pixelfx/gen_smear.py"}, fp)
        print(name, pivot)


if __name__ == "__main__":
    main()
