"""아티팩트 아이콘 도트 16x16 (Resources/Artifacts/<ArtifactId>.png): HealingDew(물방울) · GuardianShell(방패) · Hourglass(모래시계) · StarBurst(별)"""
import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "Artifacts")
os.makedirs(OUT, exist_ok=True)
OUTL = (20, 14, 24, 255)

def canvas():
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0)); return im, ImageDraw.Draw(im)

im, d = canvas()  # 물방울
d.polygon([(8, 1), (3, 9), (8, 15), (13, 9)], fill=(90, 220, 140, 255), outline=OUTL)
d.ellipse((5, 7, 10, 12), fill=(150, 255, 190, 255)); d.point((6, 8), fill=(240, 255, 245, 255))
im.save(os.path.join(OUT, "HealingDew.png"))

im, d = canvas()  # 방패
d.polygon([(2, 2), (14, 2), (14, 8), (8, 15), (2, 8)], fill=(90, 150, 220, 255), outline=OUTL)
d.polygon([(4, 4), (12, 4), (12, 8), (8, 12), (4, 8)], fill=(150, 200, 255, 255))
d.line((8, 4, 8, 11), fill=(240, 250, 255, 255))
im.save(os.path.join(OUT, "GuardianShell.png"))

im, d = canvas()  # 모래시계
d.rectangle((3, 1, 12, 2), fill=(180, 140, 60, 255), outline=OUTL); d.rectangle((3, 13, 12, 14), fill=(180, 140, 60, 255), outline=OUTL)
d.polygon([(4, 3), (11, 3), (8, 8), (11, 12), (4, 12), (8, 8)], fill=(250, 230, 150, 255), outline=OUTL)
d.polygon([(6, 4), (9, 4), (8, 7)], fill=(240, 200, 90, 255)); d.polygon([(5, 12), (10, 12), (8, 9)], fill=(240, 200, 90, 255))
im.save(os.path.join(OUT, "Hourglass.png"))

im, d = canvas()  # 별
pts = [(8, 1), (10, 6), (15, 6), (11, 9), (12, 14), (8, 11), (4, 14), (5, 9), (1, 6), (6, 6)]
d.polygon(pts, fill=(255, 170, 80, 255), outline=OUTL); d.point((8, 7), fill=(255, 240, 200, 255))
im.save(os.path.join(OUT, "StarBurst.png"))
print("ok")
