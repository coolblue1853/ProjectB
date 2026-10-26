"""셀 애니메이션 정리 → 게임용 프레임(Q-423 셀 방식, PixelRigDesign 3.7).

PixelLab PixMiniMax 초안(Docs/ArtCompare/Motion/cel/<폴더>/<i>.png, 64x64)을 골라
1) 팔레트 고정: 모든 색을 원본 주인공 그림(hero_base.png)의 색 중 가장 가까운 색으로 — 프레임마다 색이 미세하게 흔들리지 않게
2) 정렬: 땅 동작은 발바닥(가장 아래 불투명 줄)을 FeetRow-1 에, 공중 동작은 스카프(주황) 윗줄을 서 있을 때 높이에 — 초안에 박힌 점프 높이를 빼고 무릎 접기만 남긴다
3) 손 위치: 스카프 윗줄+2 ~ 엉덩이 사이 피부색 덩어리 중 가장 앞(오른쪽) = 앞손(무기), 가장 뒤 = 뒷손. 없으면 이전 프레임 값
결과: Assets/Art/Player/PixelRig/Cel/<클립>_<k>.png(+ _fist.png: 무기 위에 다시 그릴 주먹) + cel.json {클립: {frame: 초, loop, frames: [{png, hand:[x,y], handB:[x,y]}]}}
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
    "Walk": ("walk_loop", list(range(0, 8)), 0.08, True, "feet"),  # 걷기는 늘 한 발이 땅에 — 프레임마다 발 맞춤  # 0.1 은 디딘 발이 땅보다 느려(3 대 7.2px/프레임) 미끄러졌다
    "Run": ("run_loop", list(range(2, 8)), 0.06, True, "const"),  # 2~7: 프레임 간 변화가 가장 고른 6장(이음새 531 ≈ 평균), 리그 달리기 6포즈와 같은 수
    "JumpCrouch": ("jump_side", [2, 3, 4], 0.04, False, "feet"),  # 측면 기준 재생성(첫 점프 초안은 3/4 정면이라 측면 걷기와 시점이 달랐다)
    "JumpArc": ("jump_side", [5, 6, 7, 8], 0.25, False, "scarf"),  # 리그가 세로 속도로 시점을 고른다(0 상승 → 1 낙하)
    "Land": ("jump_side", [9, 10, 11], 0.05, False, "feet"),
    "Dash": ("dash_side", [2, 3, 4], 0.05, False, "feet"),     # 1번은 거의 서 있는 자세라 뺐다
    "Hurt": ("hurt_side", [2, 3, 4], 0.07, False, "feet"),     # 1번엔 흰 베기 효과가 그림에 섞여 뺐다
    "Death": ("death_side", list(range(1, 9)), 0.1, False, "feet"),
    "RopeUp": ("rope_loop", list(range(0, 8)), 0.08, True, "const"),   # 뒷모습 오르기(로프에선 무기 숨김)
    "RopeStay": ("rope_loop", [0], 0.3, True, "const"),
    # 공격(칼을 쥔 채 생성 → 칼·잔상을 지우고 손잡이·칼날 각도를 읽어 실제 장착 무기를 붙인다). 0·8번은 기준 그림이라 뺀다
    "Stab": ("attack/stab", list(range(1, 8)), 0.04, False, "feet", True),
    "SwingDown": ("attack/down", [1, 2, 4, 4, 5, 6, 7], 0.04, False, "feet", True),  # 3번은 몸이 쪼그라든 뒷모습이라(투구 레이어도 빈 칸) 4번을 두 번 쓴다
    "SwingUp": ("attack/up", [1, 2, 3, 4, 6, 6, 7], 0.04, False, "feet", True),  # 5번은 칼 지우기가 얼굴 오른쪽까지 깎아 6번을 두 번 쓴다
    # 대검·둔기 양손(늘린 강철 대검을 합성한 기준 그림에서 생성, 칼 길이 36). 묵직하게 0.05초
    "GreatDown": ("attack/great_down", list(range(1, 8)), 0.05, False, "feet", 36),
    # 단검(쌍단검 계열): 연속 찌르기·X 베기, 칼 길이 14
    "DaggerFlurry": ("attack/dagger_flurry", list(range(1, 8)), 0.035, False, "feet", 14),
    "DaggerCross": ("attack/dagger_cross", [1, 4, 5], 0.07, False, "feet", 14),  # 2·3·6·7번은 생성 그림의 큰 베기 잔상이 몸을 덮어 지우면 몸이 날아가 뺐다
    # GreatSide: 첫 생성은 2~6번에서 왼쪽으로 돌아서 기각(great_side), 재생성 결과를 확인 후 넣는다
}
CONST_REF = {"rope_loop": list(range(0, 8))}
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
        # 무기 손 = 온전히 보이는 손(피부 픽셀이 가장 많은 덩어리) = 카메라 쪽 손. 예전엔 x 가 가장 앞인 손을 골라
        # 프레임마다 손이 바뀌어 무기가 몸 앞뒤로 들락날락했다(10-26 사용자 지적)
        sizes = [len(gx) for gx, gy in groups]
        near_i = int(np.argmax(sizes))
        return pts[near_i], pts[1 - near_i]
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
        Image.fromarray(fist(img, h), "RGBA").save(os.path.join(OUT, name + "_fist.png"))
        frames.append({"png": name, "hand": h, "handB": src["handB"], "dy": dy})
    print("Idle (synth)", len(frames))
    return {"frame": 0.3, "loop": True, "frames": frames}


BLADE_LEN = 24  # 칼 길이(px) — 손잡이에서 이 안의 가장 먼 강철 픽셀이 칼끝


def steel_mask(a):
    """칼·베기 잔상: 채도 낮은 회색·강철 계열(주인공 팔레트엔 없다). 검정 외곽선(어두움)은 빼고, 칼 외곽선(78,80,98)은 포함"""
    rgb = a[..., :3].astype(int)
    mx, mn = rgb.max(axis=2), rgb.min(axis=2)
    gray = (mx - mn < 34) & (mx > 70) & (a[..., 3] > 0)
    bluish = (rgb[..., 2] >= rgb[..., 0]) & (rgb[..., 2] >= rgb[..., 1] - 4)
    pale = (mx - mn < 48) & (mx > 185) & (a[..., 3] > 0)  # 밝은 베이지·흰 베기 잔상(올려베기에서 남았다)
    return (gray & bluish) | pale


MAIN_MIN = 40  # 몸 본체와 떨어진 이보다 작은 조각은 칼 손잡이·잔상 찌꺼기


def components(mask):
    h, w = mask.shape
    seen = np.zeros_like(mask)
    parts = []
    for y in range(h):
        for x in range(w):
            if mask[y, x] and not seen[y, x]:
                stack, part = [(y, x)], []
                seen[y, x] = True
                while stack:
                    cy, cx = stack.pop()
                    part.append((cy, cx))
                    for ny, nx in ((cy + 1, cx), (cy - 1, cx), (cy, cx + 1), (cy, cx - 1), (cy + 1, cx + 1), (cy - 1, cx - 1), (cy + 1, cx - 1), (cy - 1, cx + 1)):
                        if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                            seen[ny, nx] = True
                            stack.append((ny, nx))
                parts.append(part)
    return parts


def keep_main(a):
    out = a.copy()
    parts = components(a[..., 3] > 0)
    biggest = max((len(p) for p in parts), default=0)
    for part in parts:
        # 몸 본체(가장 큰 덩어리)의 25% 미만은 버린다 — 잔상 안쪽이 팔레트 고정으로 갈색 호 조각(60px)이 되어 남았다
        if len(part) < max(MAIN_MIN, biggest * 0.25):
            for y, x in part:
                out[y, x] = 0
    return out


def fill_holes(a):
    """바깥(테두리)과 이어지지 않은 투명 픽셀 = 칼이 몸 위를 지나던 자리의 구멍 → 위·왼쪽 이웃 색으로 채운다"""
    out = a.copy()
    h, w = a.shape[:2]
    empty = out[..., 3] == 0
    outside = np.zeros_like(empty)
    stack = [(y, x) for y in range(h) for x in (0, w - 1) if empty[y, x]] + [(y, x) for x in range(w) for y in (0, h - 1) if empty[y, x]]
    for y, x in stack:
        outside[y, x] = True
    while stack:
        y, x = stack.pop()
        for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
            if 0 <= ny < h and 0 <= nx < w and empty[ny, nx] and not outside[ny, nx]:
                outside[ny, nx] = True
                stack.append((ny, nx))
    holes = empty & ~outside
    for _ in range(6):
        ys, xs = np.nonzero(holes)
        if not len(ys):
            break
        for y, x in zip(ys, xs):
            nbs = [(ny, nx) for ny, nx in ((y - 1, x), (y, x - 1), (y + 1, x), (y, x + 1)) if 0 <= ny < h and 0 <= nx < w and out[ny, nx, 3] > 0]
            if not nbs:
                continue
            # 어둡지 않은 이웃 색 우선 — 외곽선 색으로 메우면 머리카락 속에 검은 줄이 생겼다
            bright = [n for n in nbs if int(out[n][:3].astype(int).sum()) >= 150]
            out[y, x] = out[(bright or nbs)[0]]
            holes[y, x] = False
    return out


def _shift_or(m, r):
    o = m.copy()
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            o |= np.roll(np.roll(m, dy, axis=0), dx, axis=1)
    return o


def detach_thin(a, steel):
    """칼·잔상 자리 둘레(2px)에서 본체에 가늘게(≤2px) 붙은 부분을 떼어 낸다 — 잔상 테두리를 머리카락 어두운 색으로 그려
    색으로 안 걸리고 머리카락과 한 점으로 이어져 남았다(내려베기 타격). 1px 깎기 → 가장 큰 덩어리 → 다시 1px 늘리기, 칼 자리 밖은 건드리지 않는다"""
    m = a[..., 3] > 0
    env = _shift_or(steel, 2)
    er = m & ~_shift_or(~m, 1)
    parts = components(er)
    if not parts:
        return a
    big = max(parts, key=len)
    core = np.zeros_like(m)
    for y, x in big:
        core[y, x] = True
    keep = _shift_or(core, 1) & m
    out = a.copy()
    out[env & m & ~keep] = 0
    return out


def recolor_inner_outline(a, steel):
    """칼 자리 바로 옆(1px)의 어두운 픽셀 중 몸 안쪽(이웃 3면 이상 불투명)에 있는 것 = 머리카락·몸 위에 그려졌던 칼 외곽선 → 밝은 이웃 색으로
    (올려베기에서 머리카락 속에 검은 줄로 남았다). 실루엣 가장자리의 몸 외곽선은 이웃이 2면 이하라 그대로"""
    out = a.copy()
    env = _shift_or(steel, 1)
    h, w = out.shape[:2]
    for _ in range(2):
        alpha = out[..., 3] > 0
        dark = alpha & (out[..., :3].astype(int).sum(axis=2) < 150)
        ys, xs = np.nonzero(dark & env)
        for y, x in zip(ys, xs):
            nbs = [(ny, nx) for ny, nx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)) if 0 <= ny < h and 0 <= nx < w and alpha[ny, nx]]
            bright = [n for n in nbs if int(out[n][:3].astype(int).sum()) >= 150]
            if len(nbs) >= 3 and bright:
                out[y, x] = out[bright[0]]
    return out


def strip_weapon(a, blade_len=None):
    """칼·잔상 픽셀을 지우고(몸에 난 구멍은 주변 색으로 메움) 손잡이(칼에 가장 가까운 주먹)와 칼날 각도(도, 0 = 앞, 90 = 위)를 돌려준다"""
    st = steel_mask(a)
    ys, xs = np.nonzero(st)
    grip, angle = None, None
    sk = near(a, SKIN)
    if len(xs):
        cy, cx = ys.mean(), xs.mean()
        sy, sx = np.nonzero(sk)
        if len(sx):
            d = (sy - cy) ** 2 + (sx - cx) ** 2
            # 칼 픽셀과 맞닿은 피부 = 주먹
            best, gi = 1e9, 0
            for k in range(len(sx)):
                dd = ((ys - sy[k]) ** 2 + (xs - sx[k]) ** 2).min()
                if dd < best:
                    best, gi = dd, k
            grip = [int(sx[gi]), int(sy[gi])]
            dist = np.sqrt((ys - grip[1]) ** 2 + (xs - grip[0]) ** 2)
            inside = dist <= (blade_len or BLADE_LEN)
            if inside.any():
                k = int(np.argmax(np.where(inside, dist, -1)))
                angle = float(np.degrees(np.arctan2(grip[1] - ys[k], xs[k] - grip[0])))
    out = a.copy()
    out[st] = 0
    # 칼 외곽선 잔여물: 8방향 이웃에 색 있는(어둡지 않은) 몸 픽셀이 하나도 없는 어두운 픽셀을 반복해서 지운다 — 몸 외곽선은 늘 몸 색에 닿아 있다
    for _ in range(8):
        alpha = out[..., 3] > 0
        dark = alpha & (out[..., :3].astype(int).sum(axis=2) < 150)
        colored = alpha & ~dark
        cnt = np.zeros(alpha.shape, int)
        for dy2 in (-1, 0, 1):
            for dx2 in (-1, 0, 1):
                if dy2 or dx2:
                    cnt += np.roll(np.roll(colored, dy2, axis=0), dx2, axis=1)
        lone = dark & (cnt == 0)
        if not lone.any():
            break
        out[lone] = 0
    out = detach_thin(out, st)
    out = keep_main(out)
    out = fill_holes(out)
    out = recolor_inner_outline(out, st)
    # 구멍 메우기: 지운 픽셀 중 상하좌우 이웃 3개 이상이 불투명이면 몸 안쪽 → 이웃 색으로
    for _ in range(3):
        alpha = out[..., 3] > 0
        fill = []
        for y, x in zip(*np.nonzero(st & ~alpha)):
            nb = [(y + dy2, x + dx2) for dy2, dx2 in ((1, 0), (-1, 0), (0, 1), (0, -1)) if 0 <= y + dy2 < 64 and 0 <= x + dx2 < 64 and alpha[y + dy2, x + dx2]]
            if len(nb) >= 3:
                fill.append((y, x, out[nb[0]]))
        for y, x, c in fill:
            out[y, x] = c
    return out, grip, angle


FIST_R = 2  # 주먹 레이어: 손 좌표 둘레 이 반경 안의 피부색 + 그 테두리 외곽선


def fist(a, hand):
    """무기 위에 다시 그릴 주먹만 뽑는다 — 무기는 몸·옷·투구 위, 주먹 아래(10-26 사용자: 무기가 가장 잘 보여야)"""
    out = np.zeros_like(a)
    if hand is None:
        return out
    hx, hy = hand
    sk = near(a, SKIN)
    h, w = sk.shape
    keep = np.zeros_like(sk)
    for y in range(max(0, hy - FIST_R - 1), min(h, hy + 2)):
        for x in range(max(0, hx - FIST_R), min(w, hx + FIST_R + 1)):
            if sk[y, x]:
                keep[y, x] = True
    # 주먹 둘레의 검은 외곽선도 같이(무기 위에 주먹 테두리가 보이게)
    edge = np.zeros_like(keep)
    ys, xs = np.nonzero(keep)
    for y, x in zip(ys, xs):
        for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
            if 0 <= ny < h and 0 <= nx < w and not keep[ny, nx] and a[ny, nx, 3] > 0 and a[ny, nx, :3].sum() < 120:
                edge[ny, nx] = True
    m = keep | edge
    out[m] = a[m]
    return out


def main():
    os.makedirs(OUT, exist_ok=True)
    pal = palette()
    ref = lock(Image.open(os.path.join(SRC, "walk_loop", "0.png")), pal)
    sy = np.nonzero(near(ref, SCARF).any(axis=1))[0]
    scarf_top_ref = int(sy.min())
    meta = {}
    for clip, spec in CLIPS.items():
        folder, idx, frame, loop, align = spec[:5]
        weapon = len(spec) > 5 and spec[5]
        blade_len = spec[5] if len(spec) > 5 and not isinstance(spec[5], bool) else BLADE_LEN
        frames = []
        last_h, last_b = [44, 46], [36, 46]
        for k, i in enumerate(idx):
            raw = np.array(Image.open(os.path.join(SRC, folder, f"{i}.png")).convert("RGBA"))
            angle, grip = None, None
            if weapon:
                # 칼은 팔레트 고정 전에 지운다 — 고정이 먼저면 강철 색이 주인공 색으로 바뀌어 칼을 못 찾았다
                raw, grip, angle = strip_weapon(raw, blade_len)
            a = lock(Image.fromarray(raw, "RGBA"), pal)
            if align == "const" and k == 0:
                # 루프 전체에서 가장 낮은 발을 땅(54행)에 — 첫 프레임 기준이면 다른 프레임 발이 땅을 1px 파고들었다
                ref_idx = CONST_REF.get(folder, idx)  # 같은 초안 폴더를 쓰는 클립끼리는 같은 높이(매달리기 ↔ 오르기 전환 때 5px 튀었다)
                lows = [int(np.nonzero(np.array(Image.open(os.path.join(SRC, folder, f"{j}.png")).convert("RGBA"))[..., 3].any(axis=1))[0].max()) for j in ref_idx]
                const_dy = (FEET_ROW - 1) - max(lows)
                dy = const_dy
            elif align == "feet":
                ys = np.nonzero(a[..., 3].any(axis=1))[0]
                dy = (FEET_ROW - 1) - int(ys.max())
            elif align == "const":
                # 반복 동작은 첫 프레임 높이로 고정 — 프레임마다 발을 맞추면 원래 그림의 체공 출렁임이 사라지고 엉뚱한 데서 1px 튀었다
                dy = const_dy
            else:
                s = np.nonzero(near(a, SCARF).any(axis=1))[0]
                dy = scarf_top_ref - int(s.min())
            a = shift(a, dy)
            h, b = hands(a)
            if weapon and grip is not None:
                h = [grip[0], grip[1] + dy]
            h = list(h) if h else last_h
            b = list(b) if b else last_b
            last_h, last_b = h, b
            name = f"{clip}_{k}"
            Image.fromarray(a, "RGBA").save(os.path.join(OUT, name + ".png"))
            Image.fromarray(fist(a, h), "RGBA").save(os.path.join(OUT, name + "_fist.png"))
            fr = {"png": name, "hand": h, "handB": b, "dy": dy, "angle": angle if angle is not None else -999.0}  # -999 = 리그 규칙(손 위치로)
            frames.append(fr)
        meta[clip] = {"frame": frame, "loop": loop, "frames": frames}
        print(clip, len(frames), [f["dy"] for f in frames], [f["hand"] for f in frames])
    meta["Idle"] = synth_idle(meta)
    with open(os.path.join(OUT, "cel.json"), "w") as fp:
        json.dump(meta, fp, indent=1)


if __name__ == "__main__":
    main()
