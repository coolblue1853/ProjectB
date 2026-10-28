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
W, H = 112, 80          # 셀(64)보다 넓게 — 큰 초승달이 잘리지 않게
OX, OY = 24, 16          # 셀 좌표 → 시트 좌표
PIV = (41 + OX, 54 + OY)  # 셀 원점(발 가운데)
BLADE = 18.5  # 칼끝까지(ShortSword 손잡이 피벗 → 끝, 24 PPU = 셀 1px 과 같은 크기: 셀 PPU 36 × 플레이어 1.5배)
# v2(10-28 사용자 "궤적이랑 칼이 안 맞는다"): 어깨 중심 원이 아니라 셀에 기록된 실제 손·칼 각도로 칼끝이 지나간 길을 따라 그리고,
# 칼이 끝에 도착한 프레임에 띄운다(스컬: 칼이 이미 끝에 있고 지나온 길이 면으로 남는다). v1 은 칼보다 먼저 앞 아래까지 그려졌다.
# 이름: (동작, 시작 프레임, 끝 프레임(=띄우는 프레임), 종류, 최대 두께)
SPECS = {
    # v5(10-28 사용자: 품질 낮음·찌르기 미사일): 세 개 모두 vfx_workbench — 베기는 swing_slash 두께 15·5단 회색(안쪽 회색 띠, 스컬처럼 두껍게),
    # 내려베기 start -150·sweep 210(뒤 위 → 머리 위 → 앞 아래). 찌르기는 zigzag_slash 를 칼 방향 직선 경로(부드러운 모서리)로 — 칼에 붙은 넓은 잔상 띠(Guard 머리처럼), 발사체 X
    "Px_WSmearDown": ("SwingDown", 2, 4, "pixellab:pb-downslash-v3:shoulder:50", 0),
    # v4(10-28 사용자: 올려베기 꺾임·찌르기 최악): PixelLab vfx_workbench(무료) 부품으로 교체 — swing_slash(start 60·sweep -210: 앞 아래 → 머리 위 → 뒤 위, 50fps — 칼이 0.06초 만에 지나가 30fps 면 궤적이 늦었다),
    # thrust_streak(×1.6). 원본 Docs/ArtCompare/WeaponFx/pixellab_v2, My effects PB_UpSlash_v2·PB_Thrust_v2. 기준점: 올려베기 = 칼 도는 중심(어깨), 찌르기 = 손
    "Px_WSmearUp": ("SwingUp", 3, 5, "pixellab:pb-upslash-v3:shoulder:50", 0),  # 스컬 리틀 본 AttackB·Pierce RisingSlash: 양 끝 뾰족·가운데(앞 위) 두꺼운 렌즈, 칼끝보다 조금 크게
    "Px_WSmearStab": ("Stab", 3, 4, "pixellab:pb-thrust-v3:hand:40", 0),
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


def pixellab_frames(name, anchor):
    """PixelLab 시트를 셀 좌표 캔버스로: 효과 원점(anchor, 셀 좌표)에 시트의 피벗을 맞춘다. 회색 4단 → 밝기."""
    meta = json.load(open(os.path.join(PL, name + ".json")))
    sheet = Image.open(os.path.join(PL, name + ".png")).convert("RGBA")
    out = []
    for f in meta["frames"]:
        r = f["frame"]
        im = sheet.crop((r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"]))
        px, py = round(f["pivot"]["x"] * r["w"]), round(f["pivot"]["y"] * r["h"])
        can = Image.new("RGBA", (W, H))
        can.alpha_composite(im, (int(round(anchor[0] + OX - px)), int(round(anchor[1] + OY - py))))
        a = np.array(can)
        v = a[..., :3].max(axis=2)
        img = np.zeros((H, W, 4), np.uint8)
        img[..., 0] = img[..., 1] = img[..., 2] = v
        img[..., 3] = np.where(a[..., 3] > 0, 255, 0)
        out.append(img)
    return out


def render(spec):
    clip, f0, f1, kind, maxw = spec
    fr = json.load(open(CEL))[clip]["frames"]
    if kind.startswith("pixellab:"):
        _, name, where, _fps = kind.split(":")
        if where == "hand":
            anchor = fr[f1]["hand"]  # 찌르기 띠 가운데 = 뻗은 손(띠가 손 뒤 22px ~ 칼끝 너머까지)
        else:
            k = json.load(open(os.path.join(KEYS, {"SwingUp": "keys_v18.json", "SwingDown": "keys_v13.json"}[clip])))[clip]
            sx, dy = cel_shift(clip, f1, k["hand"][f1])
            kp = {j["label"]: (j["x"] * 64 + sx, j["y"] * 64 + dy) for j in k["kp"][f1]}
            anchor = kp["RIGHT SHOULDER"]
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
        fps = int(spec[3].split(":")[3]) if spec[3].startswith("pixellab:") else 20
        Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
        pivot = [PIV[0] / W, (H - (PIV[1] + 1)) / H]  # 셀 임포트와 같은 식(PixelRigBuilder.ImportPart)
        with open(os.path.join(OUT, name + ".json"), "w") as fp:
            json.dump({"frames": len(frames), "w": W, "h": H, "fps": fps, "pivot": pivot, "ramp": "gray",
                       "source": "Tools/pixelfx/gen_smear.py"}, fp)
        print(name, pivot)


if __name__ == "__main__":
    main()
