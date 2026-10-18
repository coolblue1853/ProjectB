"""1층계 파괴 가능 오브젝트 도트 (24 PPU): 나무통(Barrel) · 상자(Crate), 각 3프레임(온전/금감/파편 터짐).
출력: Assets/Art/Props/<Name>.png (가로 시트) + .json (PixelVfxBaker.ImportSheet 규격)"""
import json, os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Props")
WOOD_D, WOOD, WOOD_L, BAND, BAND_L = (62, 36, 22), (118, 72, 40), (156, 104, 60), (70, 70, 80), (120, 120, 135)
OUTL = (30, 18, 10)


def barrel(frame):
    w, h = 22, 26
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    if frame == 2:  # 파편
        for (x, y, ww, hh, c) in ((2, 20, 6, 4, WOOD), (12, 21, 7, 3, WOOD_L), (6, 16, 4, 4, WOOD_D), (15, 15, 4, 5, WOOD), (9, 22, 3, 3, BAND)):
            d.rectangle((x, y, x + ww, y + hh), fill=c, outline=OUTL)
        return im
    d.rounded_rectangle((2, 1, 19, 24), radius=4, fill=WOOD, outline=OUTL)
    for x in (6, 10, 14):  # 널빤지 선
        d.line((x, 3, x, 22), fill=WOOD_D)
    d.line((17, 3, 17, 22), fill=WOOD_L)
    for y in (5, 19):  # 쇠띠
        d.rectangle((2, y, 19, y + 2), fill=BAND, outline=OUTL)
        d.line((3, y + 1, 18, y + 1), fill=BAND_L)
    if frame == 1:  # 금
        d.line((8, 6, 11, 12), fill=OUTL); d.line((11, 12, 9, 18), fill=OUTL); d.line((13, 9, 15, 15), fill=OUTL)
    return im


def crate(frame):
    w, h = 24, 22
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    if frame == 2:
        for (x, y, ww, hh, c) in ((1, 17, 7, 4, WOOD), (10, 18, 6, 3, WOOD_L), (17, 16, 5, 5, WOOD_D), (6, 13, 4, 3, WOOD)):
            d.rectangle((x, y, x + ww, y + hh), fill=c, outline=OUTL)
        return im
    d.rectangle((1, 2, 22, 20), fill=WOOD, outline=OUTL)
    d.rectangle((3, 4, 20, 18), fill=WOOD_L, outline=WOOD_D)
    d.line((3, 4, 20, 18), fill=WOOD_D); d.line((20, 4, 3, 18), fill=WOOD_D)  # X 보강
    for y in (3, 19):
        d.line((2, y, 21, y), fill=WOOD_D)
    if frame == 1:
        d.line((6, 5, 9, 11), fill=OUTL); d.line((9, 11, 14, 16), fill=OUTL)
    return im


def write(name, fn):
    frames = [fn(i) for i in range(3)]
    w, h = frames[0].size
    sheet = Image.new("RGBA", (w * 3, h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (i * w, 0))
    os.makedirs(OUT, exist_ok=True)
    sheet.save(os.path.join(OUT, name + ".png"))
    json.dump({"frames": 3, "w": w, "h": h, "fps": 0, "pivot": [0.5, 0.0], "source": "Tools/pixelfx/gen_props.py"}, open(os.path.join(OUT, name + ".json"), "w"))
    print(name, w, h)


write("Barrel", barrel)
write("Crate", crate)
