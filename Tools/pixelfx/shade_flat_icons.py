"""평면 실루엣 아이콘에 입체 음영을 입힌다 (ArtStyle.md: 차분한 3단 명암 + 포인트, 검은 테두리 금지).
사용: python Tools/pixelfx/shade_flat_icons.py [--preview <out.png>] [이름 ...]   (이름 생략 시 SPECS 전체)
- Assets/Resources/Item/<이름>.png (32×32 도트를 8배 키운 256 PNG)을 32px로 줄여 처리한 뒤 다시 8배로 저장한다.
- 광원은 좌상단: 왼쪽·위 가장자리는 밝게, 오른쪽·아래 가장자리는 어두운 색조 외곽선(검정 아님).
- 원본 안의 명암 차이(바느질선 등)는 밝기 차로 톤 단계에 더해 보존한다.
- gem=True 는 보석·핵: 하이라이트 점과 더 강한 대비."""
import colorsys
import os
import sys
import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "Item")
SCALE = 8

# 이름: (기본 색, gem) — 기본 색은 중간 톤
SPECS = {
    "Eerie Cloth Scrap": ("#a89a6c", False),
    "Gloomy Cloth Scrap": ("#6f8a68", False),
    "Ominous Black Liquid": ("#4a3360", True),
    "Rabbit Hide": ("#cdbca2", False),
    "Round Bone": ("#d9ccad", False),
    "Small Bone": ("#d9ccad", False),
    "Ancient Bone": ("#b9ab8c", False),
    "Goblin Fang": ("#e0d5b6", False),
    "ScareHandArmor": ("#b3a689", False),
    "T0_Ore": ("#c99a2e", True),
    "T1_Ore": ("#6a46c4", True),
    "Cliff Fox Pelt": ("#c07c4e", False),
    "Cracked Shield Shard": ("#8a7398", False),
    "Death Sigil": ("#5a4870", True),
    "Abyssal Core": ("#2f3a78", True),
    "Rock": ("#8f8a78", False),
    "Enhance Stone": ("#3d6ad6", True),
}
# 너무 어두운 신발 14종: 원래 색조를 유지하고 중간 톤 명도만 끌어올린다
SHOES = ["LeafShoes", "CliffShoes", "WolfShoes", "WispShoes", "GroveShoes", "FaeShoes", "BogShoes",
         "GloomShoes", "OutlawShoes", "RaiderShoes", "StoneShoes", "RampartShoes", "SkyShoes"]
SHOE_MIN_V = 0.46


def hex_rgb(h):
    return tuple(int(h[i:i + 2], 16) / 255 for i in (1, 3, 5))


def ramp(base, gem):
    """외곽선·그림자·기본·밝음·하이라이트 5단. 어두울수록 푸른 보라로, 밝을수록 노랑으로 색조 이동."""
    h, s, v = colorsys.rgb_to_hsv(*base)
    def tone(dv, dh, ds):
        hh = (h + dh) % 1.0
        return colorsys.hsv_to_rgb(hh, min(1, max(0, s + ds)), min(1, max(0, v * dv)))
    def toward(target, amt):
        # 색조를 target 쪽으로 amt 만큼 (원형 보간)
        d = ((target - h + 0.5) % 1.0) - 0.5
        return d * amt
    lo = 0.42 if not gem else 0.36
    tones = [
        tone(lo, toward(0.72, 0.25), 0.10),
        tone(0.72, toward(0.72, 0.12), 0.06),
        tone(1.0, 0, 0),
        tone(1.18, toward(0.14, 0.10), -0.08),
        tone(1.38 if gem else 1.30, toward(0.14, 0.18), -0.25 if not gem else -0.35),
    ]
    return [tuple(int(round(c * 255)) for c in t) for t in tones]


def lum(rgb):
    return 0.299 * rgb[..., 0] + 0.587 * rgb[..., 1] + 0.114 * rgb[..., 2]


def shade(small, base, gem):
    a = small[..., 3] > 0
    h, w = a.shape
    pal = ramp(base, gem)
    inside = lambda y, x: 0 <= y < h and 0 <= x < w and a[y, x]
    L = lum(small[..., :3].astype(float))
    m = L[a].mean() if a.any() else 0
    spread = max(L[a].std(), 1e-6) if a.any() else 1
    ys, xs = np.nonzero(a)
    cy, cx = ys.mean(), xs.mean()
    span = max(ys.max() - ys.min(), 1)
    out = np.zeros_like(small)
    for y, x in zip(ys, xs):
        t = 2.0
        # 좌상단 광원: 위·왼쪽이 비면 밝게, 아래·오른쪽이 비면 어둡게
        if not inside(y - 1, x) or not inside(y, x - 1):
            t += 1
        if not inside(y + 1, x) or not inside(y, x + 1):
            t -= 1
        if not inside(y + 1, x + 1) and not inside(y + 1, x) and not inside(y, x + 1):
            t -= 1
        # 위쪽이 조금 더 밝게
        t += (cy - y) / span * 0.8
        # 원본 내부 명암 보존(선·무늬)
        if spread > 3:
            t += np.clip((L[y, x] - m) / spread, -1.2, 1.2) * 0.7
        k = int(np.clip(round(t), 0, 4))
        out[y, x, :3] = pal[k]
        out[y, x, 3] = 255
    if gem:
        add_sparkle(out, a, pal)
    return out


def add_sparkle(out, a, pal):
    # 좌상단 1/3 지점 안쪽에 하이라이트 2픽셀
    ys, xs = np.nonzero(a)
    y0 = int(ys.min() + (ys.max() - ys.min()) * 0.28)
    x0 = int(xs.min() + (xs.max() - xs.min()) * 0.32)
    for dy, dx in ((0, 0), (1, 1), (0, 1)):
        y, x = y0 + dy, x0 + dx
        if a[y, x]:
            out[y, x, :3] = (250, 248, 240) if (dy, dx) == (0, 0) else pal[4]


def shoe_base(small):
    a = small[..., 3] > 0
    rgb = small[a][:, :3].astype(float) / 255
    mid = np.median(rgb, axis=0)
    h, s, v = colorsys.rgb_to_hsv(*mid)
    return colorsys.hsv_to_rgb(h, min(0.45, s + 0.05), max(v, SHOE_MIN_V))


def process(name, preview_rows):
    path = os.path.join(ROOT, name + ".png")
    big = Image.open(path).convert("RGBA")
    small = np.array(big.resize((big.width // SCALE, big.height // SCALE), Image.NEAREST))
    if name in SHOES:
        base, gem = shoe_base(small), False
    else:
        base, gem = hex_rgb(SPECS[name][0]), SPECS[name][1]
    out = shade(small, base, gem)
    res = Image.fromarray(out.astype(np.uint8), "RGBA").resize(big.size, Image.NEAREST)
    preview_rows.append((name, big, res))
    return path, res


def main():
    args = sys.argv[1:]
    preview = None
    if "--preview" in args:
        i = args.index("--preview")
        preview = args[i + 1]
        del args[i:i + 2]
    names = args or list(SPECS) + SHOES
    rows = [process(n, []) + (n,) for n in names]
    if preview:
        T = 96
        sheet = Image.new("RGBA", (T * 2 * 6, T * ((len(rows) + 5) // 6)), (58, 62, 70, 255))
        for i, (path, res, n) in enumerate(rows):
            before = Image.open(path).convert("RGBA").resize((T - 8, T - 8), Image.NEAREST)
            x, y = (i % 6) * T * 2, (i // 6) * T
            sheet.alpha_composite(before, (x + 4, y + 4))
            sheet.alpha_composite(res.resize((T - 8, T - 8), Image.NEAREST), (x + T + 4, y + 4))
        sheet.save(preview)
        print("preview", preview)
        return
    for path, res, n in rows:
        res.save(path)
        print("shaded", n)


if __name__ == "__main__":
    main()
