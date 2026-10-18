"""절차 생성 도트 마법 이펙트 (비숍 스킬 레퍼런스 구조: 방사형 폭발 · 빛기둥 · 룬 마법진).
밝기장(0..1)을 numpy로 그린 뒤 PixelVfxBaker 와 같은 규칙(알파 컷 → 5색 램프)으로 양자화해 가로 시트 PNG + JSON 메타를 만든다.
Unity 쪽은 PixelVfxBaker.ImportSheet 가 JSON 을 읽어 슬라이스·PixelFxClip 을 만든다.
사용: python Tools/pixelfx/gen_magic.py   (출력: Assets/Art/PixelFx/Px_*.png + .json)
"""
import json, math, os
import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "PixelFx")
RAMPS = {  # PixelVfxBaker.Ramps 와 동일 (어두움 → 밝음)
    "gold":   ["5A1E0A", "C8501A", "F5A032", "FADF9F", "FFFAEB"],
    "blue":   ["141E5A", "285AC8", "50AAFF", "AAE1FF", "F0FAFF"],
    "purple": ["28104A", "6A28B4", "B45AF0", "E1AAFF", "FAF0FF"],
    "green":  ["0A3214", "1E8228", "5AD250", "BEFFA0", "F5FFEB"],
    "red":    ["460A14", "AA1E28", "F0503C", "FFAA8C", "FFF0E6"],
}
CUT = 0.2
rng = np.random.default_rng(7)


def ease_out(t):
    return 1 - (1 - t) ** 3


def quantize(field, ramp):
    h, w = field.shape
    img = np.zeros((h, w, 4), np.uint8)
    cols = [tuple(int(c[i:i + 2], 16) for i in (0, 2, 4)) for c in RAMPS[ramp]]
    k = np.clip((field - CUT) / (1 - CUT), 0, 0.9999)
    idx = (k * len(cols)).astype(int)
    on = field >= CUT
    for i, c in enumerate(cols):
        m = on & (idx == i)
        img[m, :3] = c
        img[m, 3] = 255
    return img


def radial_burst(n=64, frames=10, rays=8):
    c = (n - 1) / 2
    yy, xx = np.mgrid[0:n, 0:n]
    dx, dy = xx - c, yy - c
    d = np.hypot(dx, dy)
    ang = np.arctan2(dy, dx)
    sparks = rng.uniform(-c * 0.8, c * 0.8, (10, 2))
    out = []
    for f in range(frames):
        t = f / (frames - 1)
        fld = np.zeros((n, n))
        r0 = 2 + 9 * ease_out(min(1, t * 2))
        fld += np.exp(-(d / max(r0, 0.1)) ** 2) * 1.3 * (1 - t) ** 1.5  # 코어 번쩍
        R = 4 + (c - 3) * ease_out(t)
        fld += np.clip(1 - np.abs(d - R) / 1.6, 0, 1) * (1 - t) ** 0.7  # 충격 고리
        L = 6 + (c - 4) * ease_out(min(1, t * 1.7))
        rot = t * 0.35
        for k in range(rays):
            a = 2 * math.pi * k / rays + rot
            da = np.angle(np.exp(1j * (ang - a)))
            perp = np.abs(np.sin(da)) * d
            along = np.cos(da) * d
            thick = 1.8 * (1 - along / L)
            m = (along > 0) & (along < L) & (perp < thick)
            fld += np.where(m, (1 - along / L) * 1.1 * (1 - t) ** 0.8, 0)
        if t > 0.25:  # 십자 별빛
            tw = math.sin(t * 20) * 0.5 + 0.5
            for sx, sy in sparks:
                px, py = int(c + sx * (0.6 + t * 0.5)), int(c + sy * (0.6 + t * 0.5))
                for o in range(-2, 3):
                    for (x, y) in ((px + o, py), (px, py + o)):
                        if 0 <= x < n and 0 <= y < n:
                            fld[y, x] = max(fld[y, x], (1 - abs(o) / 3) * (1 - t) * (0.6 + 0.4 * tw))
        out.append(fld)
    return out, (0.5, 0.5)


def light_pillar(w=40, h=96, frames=10):
    cx = (w - 1) / 2
    yy, xx = np.mgrid[0:h, 0:w]  # y=0 이 위
    out = []
    motes = rng.uniform(0, 1, (14, 2))
    for f in range(frames):
        t = f / (frames - 1)
        fld = np.zeros((h, w))
        reach = ease_out(min(1, t / 0.3)) * h  # 위에서 아래로 내리꽂힘
        width = (9 if t < 0.5 else 9 * (1 - (t - 0.5) / 0.5) + 1.5)
        prof = np.clip(1 - np.abs(xx - cx) / max(width, 0.5), 0, 1)
        core = np.clip(1 - np.abs(xx - cx) / max(width * 0.35, 0.5), 0, 1)
        m = yy <= reach
        fld += np.where(m, prof * 0.75 + core * 0.6, 0) * (1 - max(0, t - 0.6) / 0.4)
        if t > 0.2:  # 착탄 충격 (바닥 타원)
            tt = (t - 0.2) / 0.8
            ex, ey = (xx - cx) / (cx * (0.4 + 0.6 * ease_out(tt))), (yy - (h - 6)) / (3.5 + 3.5 * tt)
            r = np.hypot(ex, ey)
            fld += np.clip(1 - np.abs(r - 1) / 0.35, 0, 1) * (1 - tt) * 1.1
            fld += np.exp(-r ** 2 * 3) * (1 - tt) ** 2
        for mx, my in motes:  # 떠오르는 빛 입자
            y = int(h - 4 - (my * 0.5 + t) * h * 0.8) % h
            x = int(cx + (mx - 0.5) * w * 0.9)
            if 0 <= x < w and 0 <= y < h and t > 0.15:
                fld[y, x] = max(fld[y, x], 0.8 * (1 - t))
        out.append(fld)
    return out, (0.5, 6 / h)  # 피벗 = 바닥 착탄점


def rune_circle(w=96, h=36, frames=10, ticks=16):
    cx, cy = (w - 1) / 2, (h - 1) / 2
    yy, xx = np.mgrid[0:h, 0:w]
    out = []
    for f in range(frames):
        t = f / (frames - 1)
        g = ease_out(min(1, t / 0.35))
        fade = 1 - max(0, t - 0.6) / 0.4
        fld = np.zeros((h, w))
        for rr, amp in ((1.0, 1.0), (0.72, 0.7)):
            rx, ry = (cx - 1) * rr * g + 0.01, (cy - 1) * rr * g + 0.01
            r = np.hypot((xx - cx) / rx, (yy - cy) / ry)
            fld += np.clip(1 - np.abs(r - 1) * rx / 1.3, 0, 1) * amp * fade
        ang = np.arctan2((yy - cy) / max((cy - 1) * g, 0.01), (xx - cx) / max((cx - 1) * g, 0.01))
        rr = np.hypot((xx - cx) / max((cx - 1) * g * 0.86, 0.01), (yy - cy) / max((cy - 1) * g * 0.86, 0.01))
        band = np.clip(1 - np.abs(rr - 1) * 6, 0, 1)
        tick = (np.cos(ang * ticks + t * 6) > 0.55)  # 회전하는 룬 눈금
        fld += band * tick * 0.95 * fade
        if 0.25 < t < 0.85:  # 마법진에서 솟는 빛
            # 가운데서 솟는 빛 (어두운 램프 단계로 떨어지지 않게 밝게, 가장자리는 컷 아래로)
            fld += np.exp(-((xx - cx) / (cx * 0.22)) ** 2) * np.clip(1 - np.abs(yy - cy) / (cy * 1.1), 0, 1) * 0.95 * math.sin((t - 0.25) / 0.6 * math.pi)
        out.append(fld)
    return out, (0.5, 0.5)


def write(name, frames, pivot, ramp, fps=20):
    imgs = [quantize(f, ramp) for f in frames]
    h, w = imgs[0].shape[:2]
    sheet = np.concatenate(imgs, axis=1)
    Image.fromarray(sheet, "RGBA").save(os.path.join(OUT, name + ".png"))
    with open(os.path.join(OUT, name + ".json"), "w") as fp:
        json.dump({"frames": len(imgs), "w": w, "h": h, "fps": fps, "pivot": pivot, "ramp": ramp, "source": "Tools/pixelfx/gen_magic.py"}, fp)
    print(name, len(imgs), "x", (w, h))


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for ramp, tag in (("blue", "Blue"), ("purple", "Purple"), ("gold", "Gold"), ("green", "Green")):
        f, p = radial_burst(); write("Px_Burst" + tag, f, p, ramp)
        f, p = light_pillar(); write("Px_Pillar" + tag, f, p, ramp)
        f, p = rune_circle(); write("Px_Rune" + tag, f, p, ramp)
