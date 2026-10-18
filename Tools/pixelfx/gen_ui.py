"""코드 생성 UI 공용 도트 프레임 (9-slice). 어두운 목재+금테 톤(Quest Machine 나무 UI와 어울리게).
출력: Assets/Art/UI/Frame.png(24x24, border 8), FrameLight.png(행 강조용), Divider.png(1x4 금선)"""
import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "UI")
os.makedirs(OUT, exist_ok=True)

def frame(name, fill, edge_dark, edge_gold, inner):
    s = 24
    im = Image.new("RGBA", (s, s), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.rounded_rectangle((1, 1, s - 2, s - 2), radius=4, fill=fill, outline=edge_dark)   # 바깥 어두운 외곽
    d.rounded_rectangle((2, 2, s - 3, s - 3), radius=3, outline=edge_gold)              # 금테
    d.rounded_rectangle((4, 4, s - 5, s - 5), radius=2, outline=inner)                  # 안쪽 하이라이트
    for (x, y) in ((2, 2), (s - 3, 2), (2, s - 3), (s - 3, s - 3)):                     # 모서리 리벳
        d.point((x, y), fill=(255, 230, 150, 255))
    im.save(os.path.join(OUT, name + ".png"))

frame("Frame", (22, 18, 28, 235), (10, 8, 14, 255), (190, 150, 70, 255), (60, 50, 70, 255))
frame("FrameLight", (70, 56, 40, 235), (20, 14, 10, 255), (230, 190, 90, 255), (120, 100, 70, 255))

div = Image.new("RGBA", (64, 4), (0, 0, 0, 0)); dd = ImageDraw.Draw(div)
for x in range(64):
    a = int(255 * (1 - abs(x - 31.5) / 32))
    dd.point((x, 1), fill=(190, 150, 70, a)); dd.point((x, 2), fill=(120, 90, 40, a))
div.save(os.path.join(OUT, "Divider.png"))
print("ok")
