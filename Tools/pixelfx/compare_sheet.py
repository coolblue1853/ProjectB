"""캐릭터 시안 비교 시트 — 기준 그림과 같은 배율로 나란히, 위에 판정 지표를 적는다.

사용: python Tools/pixelfx/compare_sheet.py <out.png> REF=Docs/ArtCompare/hero_style_anchor_64.png "A 설명=a.png" "B 설명=b.png" ... [--scale 6] [--cols 4]
"""
import sys

import os

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, __file__.rsplit("\\", 1)[0].rsplit("/", 1)[0])
from sprite_metrics import measure, verdict  # noqa: E402


def _font(size):
    for f in ("C:/Windows/Fonts/malgun.ttf", "C:/Windows/Fonts/gulim.ttc"):
        if os.path.exists(f):
            return ImageFont.truetype(f, size)
    return ImageFont.load_default()


def main():
    out = sys.argv[1]
    font = _font(14)
    scale, cols = 6, 4
    items = []
    rest = sys.argv[2:]
    i = 0
    while i < len(rest):
        if rest[i] == "--scale":
            scale = int(rest[i + 1]); i += 2; continue
        if rest[i] == "--cols":
            cols = int(rest[i + 1]); i += 2; continue
        label, path = rest[i].split("=", 1)
        items.append((label, path)); i += 1
    tiles = []
    for label, path in items:
        im = Image.open(path).convert("RGBA")
        m = measure(path)
        big = im.resize((im.width * scale, im.height * scale), Image.NEAREST)
        t = Image.new("RGB", (big.width, big.height + 46), (170, 170, 170))
        t.paste(big, (0, 46), big)
        d = ImageDraw.Draw(t)
        d.rectangle((0, 0, t.width, 44), fill=(25, 25, 25))
        d.text((6, 3), label, fill=(255, 230, 80), font=font)
        d.text((6, 25), f"c{m['colors']} e{m['edge']}% i{m['isolated']}% k{m['black']}% h{m['height']}  {verdict(m)}",
               fill=(255, 255, 255), font=font)
        tiles.append(t)
    W = max(t.width for t in tiles)
    H = max(t.height for t in tiles)
    rows = (len(tiles) + cols - 1) // cols
    sheet = Image.new("RGB", (min(cols, len(tiles)) * W, rows * H), (120, 120, 120))
    for k, t in enumerate(tiles):
        sheet.paste(t, ((k % cols) * W, (k // cols) * H))
    sheet.save(out)
    print(out, sheet.size)


if __name__ == "__main__":
    main()
