"""확대 전후 비교: python closeup.py <패키지> <출력.png> 이름=지도폴더[:fill][:hem=N] ...   (행 = 방법, 열 = 조합×프레임)"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import m3_amodal

CASES = [("top=leaf", "clccc", "ds_idle_0"), ("top=leaf", "clccc", "ds_stab_1"), ("legs=coat", "lllcl", "ds_down_2"),
         ("top=coat,legs=leaf", "cccll", "ds_idle_0"), ("all leaf", "lllll", "ds_up_4"), ("top=leaf", "clccc", "ds_up_2")]


def main():
    pkg, out = sys.argv[1:3]
    m3_amodal.set_pkg(pkg)
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    rows = []
    for arg in sys.argv[3:]:
        name, spec = arg.split("=", 1)
        parts = spec.split(":")
        if len(parts[0]) == 1:
            parts = [parts[0] + ":" + parts[1]] + parts[2:]
        maps = C.load_maps(parts[0])
        fill = "fill" in parts
        hem = next((int(p[4:]) for p in parts if p.startswith("hem=")), None)
        tiles = []
        for _, code, f in CASES:
            wear = {s: ("base_coat" if ch == "c" else "leaf_set") for s, ch in zip(C.SLOTS, code)}
            imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
            lay = {s: C.make_layers(maps[f], imgs[s], s, hem) for s in imgs}
            fl = m3_amodal.make_fills(f, maps[f], imgs, lay) if fill else None
            img, _ = C.compose(under[f], wear, imgs, lay, fl)
            b = Image.new("RGBA", (64, 64), (70, 70, 70, 255))
            b.alpha_composite(Image.fromarray(img))
            tiles.append(b)
        rows.append((name, tiles))
    W = 64 * len(CASES)
    sheet = Image.new("RGB", (W * 5, (64 * 5 + 20) * len(rows) + 20), (20, 20, 20))
    d = ImageDraw.Draw(sheet)
    for i, (lab, code, f) in enumerate(CASES):
        d.text((i * 320 + 4, 4), "%s %s" % (lab, f), fill=(255, 255, 255))
    for r, (name, tiles) in enumerate(rows):
        y = 20 + r * (64 * 5 + 20)
        d.text((4, y + 2), name, fill=(255, 255, 0))
        for i, t in enumerate(tiles):
            sheet.paste(t.convert("RGB").resize((320, 320), Image.NEAREST), (i * 320, y + 18))
    sheet.save(out)


if __name__ == "__main__":
    main()
