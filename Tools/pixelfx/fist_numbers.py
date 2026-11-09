"""손 후보 번호 이미지: 살색 덩어리를 찾아 번호를 붙인다. 사용: python fist_numbers.py <폴더> <프레임수> <출력.png>"""
import sys
from PIL import Image, ImageDraw, ImageFont
FONT = ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf', 56)
from collections import deque

d, n, out = sys.argv[1], int(sys.argv[2]), sys.argv[3]
S = 12
COLS = 3
ROWS = (n + COLS - 1) // COLS
sh = Image.new("RGB", (COLS * 64 * S, ROWS * 64 * S), (60, 70, 90))
for i in range(n):
    im = Image.open("%s/g_%d.png" % (d, i)).convert("RGBA")
    px = im.load()
    skin = {(x, y) for y in range(64) for x in range(64)
            if px[x, y][3] and px[x, y][0] > 215 and 160 < px[x, y][1] < 235 and 120 < px[x, y][2] < 205 and px[x, y][0] - px[x, y][2] > 40}
    seen, blobs = set(), []
    for p in skin:
        if p in seen:
            continue
        q, comp = deque([p]), []
        seen.add(p)
        while q:
            c = q.popleft(); comp.append(c)
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    r = (c[0] + dx, c[1] + dy)
                    if r in skin and r not in seen:
                        seen.add(r); q.append(r)
        if len(comp) >= 3:
            blobs.append(comp)
    bg = Image.new("RGBA", im.size, (60, 70, 90, 255)); bg.alpha_composite(im)
    big = bg.resize((64 * S, 64 * S), Image.NEAREST).convert("RGB")
    dr = ImageDraw.Draw(big)
    blobs.sort(key=lambda c: -len(c))
    for k, comp in enumerate(blobs[:6], 1):
        cx = sum(c[0] for c in comp) / len(comp); cy = sum(c[1] for c in comp) / len(comp)
        dr.ellipse([cx * S - 18, cy * S - 18, cx * S + 18, cy * S + 18], outline=(255, 40, 40), width=3)
        dr.text((cx * S + 22, cy * S - 34), str(k), fill=(255, 255, 0), font=FONT, stroke_width=3, stroke_fill=(0, 0, 0))
        print("frame %d blob %d center (%d,%d) size %d" % (i, k, round(cx), round(cy), len(comp)))
    dr.text((6, 6), "frame %d" % i, fill=(255, 255, 255), font=FONT)
    sh.paste(big, ((i % COLS) * 64 * S, (i // COLS) * 64 * S))
sh.save(out)
