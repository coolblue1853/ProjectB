"""이펙트(스미어)가 들어갈 면을 졸라맨 위에 선으로 먼저 그려 검토받는다(도트 찍기 전, 10-28 사용자).
면 = 앞 프레임 → 이 프레임 사이에 칼날이 실제로 쓸고 간 영역(손 경로 ~ 칼끝 경로). full = 꽉 찬 면, thin = 얇은 호(칼끝 경로만).
사용: python Tools/pixelfx/fx_plan.py <출력 폴더>  → fx_plan_<동작>.gif + fx_plan_board.png(스컬 리틀 본 비교)
"""
import json
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

FONT = ImageFont.truetype("C:/Windows/Fonts/malgun.ttf", 18)
S = "Docs/ArtCompare/Motion/cel/basic_sword/skel/"
BLADE = 15.0
SC = 5
# v2(스컬 리틀 본식): 쓸고 간 부채꼴이 아니라 휘두르는 호 전체를 한 프레임에 덮는 초승달(바깥 반지름 = 팔+칼, 안쪽은 비워 몸이 보이게, 끝쪽이 두껍다) → 다음 프레임 바깥 가장자리 얇은 호
# (중심 어깨, 시작각 → 끝각 반시계 기준 범위, 바깥 반지름, 두께)
CRESCENT = {
    "SwingDown": {3: ("full", -40, 175, 23, 14), 4: ("thin", -40, 175, 22, 2)},
    "SwingUp": {4: ("full", -100, 165, 23, 14), 5: ("thin", -100, 165, 22, 2)},
    "Stab": {4: ("streak", 0, 0, 26, 5), 5: ("streak_thin", 0, 0, 26, 1)},
}
MOTIONS = {
    # 동작: (keys 파일, 동작 키, 프레임 수, {프레임: 면 종류})
    "찌르기": ("keys_v6.json", "Stab", None, {4: "full", 5: "thin"}),
    "내려베기": ("keys_v13.json", "SwingDown", None, {3: "full", 4: "thin"}),
    "올려베기": ("keys_v18.json", "SwingUp", None, {4: "full", 5: "thin"}),
}
BONES = [("NECK", "RIGHT HIP"), ("RIGHT SHOULDER", "RIGHT ELBOW"), ("RIGHT ELBOW", "RIGHT ARM"), ("LEFT SHOULDER", "LEFT ELBOW"), ("LEFT ELBOW", "LEFT ARM"),
         ("RIGHT HIP", "RIGHT KNEE"), ("RIGHT KNEE", "RIGHT LEG"), ("LEFT HIP", "LEFT KNEE"), ("LEFT KNEE", "LEFT LEG")]


def tip(hand, ang):
    return (hand[0] + BLADE * math.cos(math.radians(ang)), hand[1] - BLADE * math.sin(math.radians(ang)))


def swept(h0, a0, h1, a1, n=24):
    """앞 프레임 → 이 프레임 칼날이 쓸고 간 면(손 경로 + 칼끝 경로)."""
    tips, hands = [], []
    for k in range(n + 1):
        t = k / n
        h = (h0[0] + (h1[0] - h0[0]) * t, h0[1] + (h1[1] - h0[1]) * t)
        hands.append(h)
        tips.append(tip(h, a0 + (a1 - a0) * t))
    return tips, hands


def draw_frame(d, i, kind):
    kp = {j["label"]: (j["x"] * 64, j["y"] * 64) for j in d["kp"][i]}
    im = Image.new("RGBA", (64 * SC, 64 * SC), (236, 232, 222, 255))
    ov = Image.new("RGBA", im.size, (0, 0, 0, 0))
    g = ImageDraw.Draw(ov)
    s = lambda q: (q[0] * SC, q[1] * SC)
    if kind and i > 0:
        tips, hands = swept(d["hand"][i - 1], d["angle"][i - 1], d["hand"][i], d["angle"][i])
        if kind == "full":
            g.polygon([s(p) for p in tips + hands[::-1]], fill=(120, 170, 255, 110), outline=(30, 80, 220, 255))
        else:
            g.line([s(p) for p in tips], fill=(30, 80, 220, 255), width=4)
    gb = ImageDraw.Draw(im)
    gb.line([s((0, 54)), s((64, 54))], fill=(150, 140, 120), width=2)
    for a, b in BONES:
        far = a.startswith("LEFT")
        gb.line([s(kp[a]), s(kp[b])], fill=(150, 150, 170) if far else (40, 40, 60), width=SC)
    hc = (kp["NOSE"][0] - 4, kp["NOSE"][1] - 3)
    gb.ellipse([s((hc[0] - 8, hc[1] - 8)), s((hc[0] + 8, hc[1] + 8))], outline=(40, 40, 60), width=3)
    h = d["hand"][i]
    gb.line([s(h), s(tip(h, d["angle"][i]))], fill=(90, 120, 200), width=SC - 2)
    im.alpha_composite(ov)
    return im


def crescent_mask(center, a0, a1, r, thick, lead_at_end=True):
    """어깨 중심 초승달 면(64격자 좌표 마스크, SC 배율). 진행 끝쪽(a1)이 가장 두껍고 시작 쪽은 0으로 가늘다."""
    H = 64 * SC
    yy, xx = np.mgrid[0:H, 0:H]
    x = xx / SC - center[0]
    y = center[1] - yy / SC
    d = np.hypot(x, y)
    ang = np.degrees(np.arctan2(y, x))
    t = ((ang - a0) % 360) / ((a1 - a0) % 360 or 360)
    inside = t <= 1.0
    w = thick * np.clip(t * 1.4, 0, 1) * (1 - np.clip((t - 0.92) * 10, 0, 1))
    return inside & (d <= r) & (d >= r - w)


def draw_frame_v2(d, i, spec):
    im = draw_frame(d, i, None)
    if not spec:
        return im
    kind, a0, a1, r, thick = spec
    kp = {j["label"]: (j["x"] * 64, j["y"] * 64) for j in d["kp"][i]}
    sh = kp["RIGHT SHOULDER"]
    if kind.startswith("streak"):
        h = d["hand"][i]
        H = 64 * SC
        yy, xx = np.mgrid[0:H, 0:H]
        x = xx / SC - h[0]
        y = yy / SC - h[1]
        along = x / r
        m = (along >= 0) & (along <= 1) & (np.abs(y) <= np.maximum(0.5, thick * (1 - along) * 0.5 + (0.6 if kind == "streak" else 0)))
    else:
        m = crescent_mask(sh, a0, a1, r, thick)
    ov = np.zeros((64 * SC, 64 * SC, 4), np.uint8)
    ov[m] = (120, 170, 255, 120)
    edge = m & ~(np.roll(m, 1, 0) & np.roll(m, -1, 0) & np.roll(m, 1, 1) & np.roll(m, -1, 1))
    ov[edge] = (30, 80, 220, 255)
    im.alpha_composite(Image.fromarray(ov, "RGBA"))
    return im


def skul_frames(an):
    path = f"C:/Users/user/SkulRef/lib/character/player_head/LittleBone_57c1fb1632/06_attack_basic__{an}.png"
    im = Image.open(path).convert("RGBA")
    col = (np.array(im)[:, :, 3] > 0).any(0)
    runs, s0 = [], None
    for x, c in enumerate(col):
        if c and s0 is None:
            s0 = x
        if not c and s0 is not None:
            runs.append((s0, x))
            s0 = None
    if s0 is not None:
        runs.append((s0, len(col)))
    return [im.crop((a, 0, b, im.height)) for a, b in runs]


def main():
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    rows = []
    for name, (f, key, _, smear) in MOTIONS.items():
        d = json.load(open(S + f))[key]
        n = len(d["hand"])
        frames = []
        for i in range(n):
            spec = CRESCENT.get(key, {}).get(i)
            im = draw_frame_v2(d, i, spec) if spec else draw_frame(d, i, None)
            ImageDraw.Draw(im).text((6, 4), f"{name} {i}" + (" 면" if smear.get(i) == "full" else " 얇은 호" if smear.get(i) == "thin" else ""), fill=(0, 0, 0), font=FONT)
            frames.append(im.convert("RGB"))
        times = d.get("times") or [50, 40, 70, 30, 20, 180, 45, 60, 90][:n]
        frames[0].save(f"{out}/fx_plan_{key}.gif", save_all=True, append_images=frames[1:], duration=times[:n], loop=0)
        keys = sorted(set([max(0, min(smear) - 1)] + list(smear)))
        row = Image.new("RGB", (len(keys) * frames[0].width // 2, frames[0].height // 2), (255, 255, 255))
        for j, i in enumerate(keys):
            row.paste(frames[i].resize((frames[i].width // 2, frames[i].height // 2)), (j * frames[0].width // 2, 0))
        rows.append(row)
    for an, label in [("AttackA", "스컬 리틀 본 내려베기(AttackA 5프레임)"), ("AttackB", "스컬 리틀 본 올려베기(AttackB 4프레임)")]:
        fs = skul_frames(an)
        hgt = 140
        fs = [x.resize((int(x.width * hgt / x.height), hgt), Image.NEAREST) for x in fs]
        row = Image.new("RGB", (sum(x.width + 10 for x in fs) + 10, hgt + 30), (60, 60, 70))
        ImageDraw.Draw(row).text((6, 2), label, fill=(255, 255, 120), font=FONT)
        x0 = 10
        for x in fs:
            row.paste(x, (x0, 30), x)
            x0 += x.width + 10
        rows.append(row)
    W = max(r.width for r in rows)
    board = Image.new("RGB", (W, sum(r.height + 8 for r in rows)), (255, 255, 255))
    y = 0
    for r in rows:
        board.paste(r, (0, y))
        y += r.height + 8
    board.save(f"{out}/fx_plan_board.png")
    print(board.size)


if __name__ == "__main__":
    main()
