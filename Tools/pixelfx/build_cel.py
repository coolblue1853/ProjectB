"""셀 애니메이션 정리 → 게임용 프레임(Q-423 셀 방식, PixelRigDesign 3.7).

PixelLab PixMiniMax 초안(Docs/ArtCompare/Motion/cel/<폴더>/<i>.png, 64x64)을 골라
1) 팔레트 고정: 모든 색을 원본 주인공 그림(hero_base.png)의 색 중 가장 가까운 색으로 — 프레임마다 색이 미세하게 흔들리지 않게
2) 정렬: 땅 동작은 발바닥(가장 아래 불투명 줄)을 FeetRow-1 에, 공중 동작은 스카프(주황) 윗줄을 서 있을 때 높이에 — 초안에 박힌 점프 높이를 빼고 무릎 접기만 남긴다
3) 손 위치: 스카프 윗줄+2 ~ 엉덩이 사이 피부색 덩어리 중 가장 앞(오른쪽) = 앞손(무기), 가장 뒤 = 뒷손. 없으면 이전 프레임 값
결과: Assets/Art/Player/PixelRig/Cel/<클립>_<k>.png + cel.json {클립: {frame: 초, loop, frames: [{png, hand:[x,y], handB:[x,y]}]}}
좌표는 64 캔버스 픽셀(왼쪽 위 0). 리그 원점(엉덩이 x 41, 발바닥 55행)은 PixelRigBuilder 와 같다.
사용: python Tools/pixelfx/build_cel.py
"""
import json
import os

import numpy as np
from PIL import Image

SRC = "Docs/ArtCompare/Motion/cel"
OUT = "Assets/Art/Player/PixelRig/Cel"
FEET_ROW = 55
HIP_X = 41  # 리그 원점 x(엉덩이)
# 클립: (초안 폴더, 프레임 번호들, 프레임 시간(초), 반복, 정렬 'feet' | 'scarf')
CLIPS = {
    "Walk": ("walk_loop", list(range(0, 8)), 0.1, True, "feet"),
    "Run": ("run_loop", list(range(1, 8)), 0.07, True, "feet"),
    "JumpCrouch": ("jump_side", [2, 3, 4], 0.04, False, "feet"),  # 측면 기준 재생성(첫 점프 초안은 3/4 정면이라 측면 걷기와 시점이 달랐다)
    "JumpArc": ("jump_side", [5, 6, 7, 8], 0.25, False, "scarf"),  # 리그가 세로 속도로 시점을 고른다(0 상승 → 1 낙하)
    "Land": ("jump_side", [9, 10, 11], 0.05, False, "feet"),
    "Dash": ("dash_side", [2, 3, 4], 0.05, False, "feet"),     # 1번은 거의 서 있는 자세라 뺐다
    "Hurt": ("hurt_side", [2, 3, 4], 0.07, False, "feet"),     # 1번엔 흰 베기 효과가 그림에 섞여 뺐다
    "Death": ("death_side", list(range(1, 9)), 0.1, False, "feet"),
}
SKIN = [(0xFD, 0xE0, 0xC6), (0xF3, 0xB4, 0x9A), (0xD7, 0x8E, 0x7A)]
SCARF = [(0xF1, 0xAB, 0x36), (0xB8, 0x70, 0x2C)]


def palette():
    a = np.array(Image.open(os.path.join(SRC, "hero_base.png")).convert("RGBA"))
    cols = {tuple(p) for p in a[a[..., 3] > 0][:, :3]}
    return np.array(sorted(cols), dtype=int)


def lock(img, pal):
    a = np.array(img.convert("RGBA")).astype(int)
    m = a[..., 3] > 0
    px = a[m][:, :3]
    d = ((px[:, None, :] - pal[None, :, :]) ** 2).sum(axis=2)
    a[m, :3] = pal[d.argmin(axis=1)]
    a[..., 3] = np.where(m, 255, 0)
    return a.astype(np.uint8)


def near(a, cols, tol=40):
    m = np.zeros(a.shape[:2], bool)
    for c in cols:
        m |= (np.abs(a[..., :3].astype(int) - np.array(c)).sum(axis=2) < tol) & (a[..., 3] > 0)
    return m


def shift(a, dy):
    out = np.zeros_like(a)
    if dy >= 0:
        out[dy:] = a[:a.shape[0] - dy]
    else:
        out[:dy] = a[-dy:]
    return out


def hands(a):
    """스카프 아래 피부색 픽셀을 x 로 둘로 나눠 (앞손, 뒷손) — 각 덩어리의 가장 아래·바깥 픽셀(주먹 끝)"""
    sc = near(a, SCARF)
    sy = np.nonzero(sc.any(axis=1))[0]
    # 얼굴은 스카프 윗줄보다 위에만 있다 — 손은 스카프 옆 같은 높이에도 온다(달리기·웅크림), 그래서 스카프 아랫줄이 아니라 윗줄 + 2 부터
    top = sy.min() + 2 if len(sy) else 40
    sk = near(a, SKIN)
    sk[:top] = False
    # 무릎 맨살도 피부색이라 발에서 6줄 위(엉덩이)보다 아래는 뺀다 — 처음엔 무릎을 손으로 잡았다
    hips = np.nonzero(a[..., 3].any(axis=1))[0].max() - 5
    sk[hips:] = False
    ys, xs = np.nonzero(sk)
    if len(xs) == 0:
        return None, None
    order = np.argsort(xs)
    xs, ys = xs[order], ys[order]
    gaps = np.diff(xs)
    if len(gaps) and gaps.max() >= 3:
        cut = gaps.argmax() + 1
        groups = [(xs[:cut], ys[:cut]), (xs[cut:], ys[cut:])]
    else:
        groups = [(xs, ys)]
    pts = [(int(round(gx.mean())), int(gy.max())) for gx, gy in groups]
    if len(pts) > 1:
        return max(pts), min(pts)
    # 하나만 보이면 그게 가까운 팔(무기 손) — 측면 그림에서 먼 팔은 몸에 가려진다. 몸 뒤쪽이면 리그가 칼을 위·뒤로 세운다(최대 100°).
    # (3/4 정면 초안에선 뒷손을 앞손으로 잡아 칼이 얼굴을 가로질렀지만, 측면으로 통일한 뒤엔 보이는 손이 곧 가까운 손)
    return pts[0], pts[0]


IDLE_BASE = ("Walk", 3)  # 걷기 루프에서 두 발이 모인 프레임 — 걷기와 같은 시점(착지 회복 프레임은 정면 쪽이라 어긋났다)
IDLE_STRETCH_ROW = 44   # 튜닉 가운데 줄 — 복제하면 허리 위(상체·머리)가 1px 오른다


def synth_idle(meta):
    """측면 대기 숨쉬기(생성 없이): 착지 회복 프레임에서 튜닉 한 줄을 복제해 상체를 1px 올렸다 내림.
    PixMiniMax 로 만든 대기는 1~3프레임에서 3/4 정면 그림으로 되돌아가 이음새가 끊겼다(10-26)."""
    clip, k = IDLE_BASE
    src = meta[clip]["frames"][k]
    a = np.array(Image.open(os.path.join(OUT, src["png"] + ".png")).convert("RGBA"))
    up = np.zeros_like(a)
    up[:IDLE_STRETCH_ROW] = a[1:IDLE_STRETCH_ROW + 1]
    up[IDLE_STRETCH_ROW:] = a[IDLE_STRETCH_ROW:]
    frames = []
    for i, img in enumerate([a, up, up, a]):
        name = f"Idle_{i}"
        Image.fromarray(img, "RGBA").save(os.path.join(OUT, name + ".png"))
        dy = -1 if img is up else 0
        h = [src["hand"][0], src["hand"][1] + (dy if src["hand"][1] < IDLE_STRETCH_ROW else 0)]
        frames.append({"png": name, "hand": h, "handB": src["handB"], "dy": dy})
    print("Idle (synth)", len(frames))
    return {"frame": 0.3, "loop": True, "frames": frames}


def main():
    os.makedirs(OUT, exist_ok=True)
    pal = palette()
    ref = lock(Image.open(os.path.join(SRC, "walk_loop", "0.png")), pal)
    sy = np.nonzero(near(ref, SCARF).any(axis=1))[0]
    scarf_top_ref = int(sy.min())
    meta = {}
    for clip, (folder, idx, frame, loop, align) in CLIPS.items():
        frames = []
        last_h, last_b = [44, 46], [36, 46]
        for k, i in enumerate(idx):
            a = lock(Image.open(os.path.join(SRC, folder, f"{i}.png")), pal)
            if align == "feet":
                ys = np.nonzero(a[..., 3].any(axis=1))[0]
                dy = (FEET_ROW - 1) - int(ys.max())
            else:
                s = np.nonzero(near(a, SCARF).any(axis=1))[0]
                dy = scarf_top_ref - int(s.min())
            a = shift(a, dy)
            h, b = hands(a)
            h = list(h) if h else last_h
            b = list(b) if b else last_b
            last_h, last_b = h, b
            name = f"{clip}_{k}"
            Image.fromarray(a, "RGBA").save(os.path.join(OUT, name + ".png"))
            frames.append({"png": name, "hand": h, "handB": b, "dy": dy})
        meta[clip] = {"frame": frame, "loop": loop, "frames": frames}
        print(clip, len(frames), [f["dy"] for f in frames], [f["hand"] for f in frames])
    meta["Idle"] = synth_idle(meta)
    with open(os.path.join(OUT, "cel.json"), "w") as fp:
        json.dump(meta, fp, indent=1)


if __name__ == "__main__":
    main()
