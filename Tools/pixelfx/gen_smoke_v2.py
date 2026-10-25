"""연기 퍼프·지면 충격파 v2 — 상업 도트 이펙트 기준(GameFeelPlan 10.7, PixelLab 없이 직접 채색).

v1(gen_polish_fx.py)은 밝기장을 잘라 만든 흐릿한 덩어리 하나라 형태가 약했다.
v2 규칙:
- 연기: 둥근 덩어리 여러 개가 각자 1px 외곽선 + 3단 명암(왼쪽 위 빛) — 뒤 덩어리부터 칠해 앞 덩어리가 겹쳐 보인다.
  퍼지며 커졌다가 점점 작아져 사라진다(노이즈 페이드 없음).
- 충격파: 흙 가시(빛 받는 면·그늘 면·외곽선)가 바깥으로 차례로 솟았다 가라앉고, 돌 조각이 포물선으로 튄다.
결과는 기존과 같은 이름·형식(Assets/Art/PixelFx/Px_*.png + json)이라 PixelVfxBaker.ImportSheet 로 다시 들이면 된다.
사용: python Tools/pixelfx/gen_smoke_v2.py
"""
import json
import math
import os

import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "PixelFx")
RAMPS = {  # 어둠(외곽선) → 밝음
    "smoke": ["2A2633", "4A4652", "6E6A78", "9A96A3", "C4C0CC"],
    "shadow": ["1A0F2A", "3F2560", "7650A0", "B28ED6", "EADCFA"],
    "earth": ["2E1C10", "5E3C20", "91663A", "C49C68", "EED8B0"],
}
LIGHT = np.array([-0.6, -0.8])  # 왼쪽 위에서 오는 빛(화면 좌표, y 아래 +)
rng = np.random.default_rng(23)


def cols(ramp):
    return [tuple(int(c[i:i + 2], 16) for i in (0, 2, 4)) + (255,) for c in RAMPS[ramp]]


def ease_out(t):
    return 1 - (1 - t) ** 3


def draw_puff(img, cx, cy, r, pal):
    """한 덩어리: 외곽선 → 명암 3단(빛 쪽 밝게, 반대쪽 어둡게, 가운데 중간)"""
    if r < 0.8:
        return
    h, w = img.shape[:2]
    x0, x1 = int(cx - r - 2), int(cx + r + 2)
    y0, y1 = int(cy - r - 2), int(cy + r + 2)
    for y in range(max(0, y0), min(h, y1 + 1)):
        for x in range(max(0, x0), min(w, x1 + 1)):
            dx, dy = x + 0.5 - cx, y + 0.5 - cy
            d = math.hypot(dx, dy)
            if d > r + 1:
                continue
            if d > r:
                img[y, x] = pal[0]  # 외곽선
                continue
            n = np.array([dx, dy]) / max(r, 0.001)
            lit = -float(n @ LIGHT)  # 빛 쪽 +
            # 3단(빛·중간·그늘) — 4단은 줄무늬처럼 보였다. 가장 밝은 색은 빛 쪽 작은 하이라이트에만
            if lit > 0.7 and d < r * 0.75:
                img[y, x] = pal[4]
            elif lit > 0.25:
                img[y, x] = pal[3]
            elif lit > -0.5:
                img[y, x] = pal[2]
            else:
                img[y, x] = pal[1]


def smoke(ramp, n=48, frames=10, puffs=7):
    pal = cols(ramp)
    c = (n - 1) / 2
    ang = np.linspace(0, 2 * math.pi, puffs, endpoint=False) + rng.uniform(-0.3, 0.3, puffs)
    spd = rng.uniform(0.6, 1.0, puffs)
    rmax = rng.uniform(5.0, 7.5, puffs)
    delay = rng.uniform(0.0, 0.15, puffs)
    order = np.argsort(np.sin(ang))  # 아래쪽(화면 앞) 덩어리를 나중에 칠한다
    out = []
    for f in range(frames):
        t = f / (frames - 1)
        img = np.zeros((n, n, 4), np.uint8)
        # 가운데 큰 덩어리(처음에 크고 빨리 줄어든다)
        # 가운데 덩어리: 작게 시작해 빠르게 커졌다가 줄어든다(펑). 초반엔 맨 위에 그린다(주변 덩어리가 안쪽에 비치지 않게)
        core_r = 9 * math.sin(math.pi * min(1, 0.12 + t * 1.3)) + 0.5
        core_on_top = t < 0.35
        if not core_on_top:
            draw_puff(img, c, c - t * 3, core_r, pal)
        for i in order:
            tt = min(1, max(0, (t - delay[i]) / (1 - delay[i])))
            e = ease_out(tt)
            d = e * spd[i] * c * 0.55
            px, py = c + math.cos(ang[i]) * d, c + math.sin(ang[i]) * d * 0.8 - tt * 6  # 위로 피어오름
            r = rmax[i] * math.sin(math.pi * min(1, tt * 0.95))  # 0에서 커졌다 줄어듦(처음부터 크면 가운데와 겹쳐 도넛처럼 보였다)
            draw_puff(img, px, py, r, pal)
        if core_on_top:
            draw_puff(img, c, c - t * 3, core_r, pal)
        out.append(img)
    return out, (0.5, 0.5)


def ground_shock(ramp="earth", w=96, h=32, frames=9):
    pal = cols(ramp)
    cx = (w - 1) / 2
    base = h - 5
    out = []
    pebbles = [(rng.uniform(-1, 1), rng.uniform(0.6, 1.0)) for _ in range(8)]
    for f in range(frames):
        t = f / (frames - 1)
        img = np.zeros((h, w, 4), np.uint8)
        # 지면 고리(납작한 타원 테두리, 2px 두께)
        rx, ry = 6 + (cx - 6) * ease_out(t), 1.5 + 2.5 * ease_out(t)
        if t < 0.85:
            for y in range(h):
                for x in range(w):
                    q = math.hypot((x + 0.5 - cx) / rx, (y + 0.5 - base) / ry)
                    if 0.82 < q < 1.0:
                        img[y, x] = pal[3] if y < base else pal[2]
                    elif 1.0 <= q < 1.12:
                        img[y, x] = pal[0]
        # 흙 가시: 바깥으로 차례로 솟았다 가라앉음
        for k in range(1, 7):
            for side in (-1, 1):
                px = cx + side * k * (cx / 7.2)
                phase = t * 1.6 - k * 0.09
                hgt = max(0.0, math.sin(min(1, max(0, phase)) * math.pi)) * (13 - k * 1.2)
                if hgt < 1.5:
                    continue
                half = 2.5 + (7 - k) * 0.25
                for y in range(int(base - hgt), base + 1):
                    wy = half * (y - (base - hgt)) / max(hgt, 1)  # 위로 갈수록 뾰족
                    for x in range(int(px - wy - 1), int(px + wy + 2)):
                        if not (0 <= x < w and 0 <= y < h):
                            continue
                        dx = x + 0.5 - px
                        if abs(dx) > wy + 0.6:
                            continue
                        if abs(dx) > wy - 0.6 or y == int(base - hgt):
                            img[y, x] = pal[0]
                        else:
                            img[y, x] = pal[3] if dx < 0 else pal[1]  # 왼쪽(빛) 면 밝게
        # 돌 조각: 포물선
        for vx, vy in pebbles:
            if t > 0.9:
                continue
            px = int(cx + vx * ease_out(t) * cx * 0.8)
            py = int(base - (vy * 26 * t - 30 * t * t))
            for dy in (0, 1):
                for dx in (0, 1):
                    x, y = px + dx, py + dy
                    if 0 <= x < w and 0 <= y < h:
                        img[y, x] = pal[2] if (dx + dy) else pal[0]
        out.append(img)
    return out, (0.5, 5 / h)


def write(name, frames, pivot, ramp, fps):
    h, w = frames[0].shape[:2]
    Image.fromarray(np.concatenate(frames, axis=1), "RGBA").save(os.path.join(OUT, name + ".png"))
    with open(os.path.join(OUT, name + ".json"), "w") as fp:
        json.dump({"frames": len(frames), "w": w, "h": h, "fps": fps, "pivot": list(pivot), "ramp": ramp, "source": "Tools/pixelfx/gen_smoke_v2.py"}, fp)
    print(name, len(frames), "x", (w, h))


if __name__ == "__main__":
    f, p = smoke("smoke"); write("Px_SmokeGray", f, p, "smoke", 16)
    f, p = smoke("shadow"); write("Px_SmokePurple", f, p, "shadow", 16)
    f, p = ground_shock(); write("Px_GroundShock", f, p, "earth", 18)
