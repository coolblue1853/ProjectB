"""퀘스트 게시판 도트 (32x40, 24 PPU). 나무 기둥·판자·종이 쪽지 3장·붉은 핀 + 위쪽 노란 '!' (퀘스트 있음 표시용 별도 스프라이트)
사용: python Tools/pixelfx/gen_questboard.py → Assets/Art/NPC/QuestBoard.png, QuestMark.png
"""
import os
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "NPC")
W, H = 32, 40
WOOD_D, WOOD, WOOD_L = (74, 44, 28), (122, 76, 44), (160, 108, 64)
PAPER, PAPER_D = (236, 226, 196), (196, 180, 140)
INK, PIN = (90, 70, 60), (200, 40, 40)

img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
px = img.load()

def rect(x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            px[x, y] = c + (255,)

# 기둥 2개 (아래쪽)
for x0 in (6, 23):
    rect(x0, 18, x0 + 2, 39, WOOD_D)
    rect(x0 + 1, 18, x0 + 1, 39, WOOD)
# 판자 + 테두리 + 지붕 판
rect(2, 6, 29, 27, WOOD_D)
rect(3, 7, 28, 26, WOOD)
for y in (12, 18, 24):
    rect(3, y, 28, y, WOOD_D)  # 판자 이음새
rect(1, 3, 30, 5, WOOD_D)
rect(2, 3, 29, 4, WOOD_L)
# 종이 쪽지 3장
for (x0, y0, w, h) in ((5, 9, 8, 9), (15, 8, 7, 7), (16, 16, 9, 8)):
    rect(x0, y0, x0 + w, y0 + h, PAPER_D)
    rect(x0, y0, x0 + w - 1, y0 + h - 1, PAPER)
    for ly in range(y0 + 3, y0 + h - 1, 2):
        rect(x0 + 1, ly, x0 + w - 3, ly, INK)
    px[x0 + w // 2, y0] = PIN + (255,)
os.makedirs(OUT, exist_ok=True)
img.save(os.path.join(OUT, "QuestBoard.png"))

# 퀘스트 표시 '!' (6x10, 노랑 + 어두운 테두리)
m = Image.new("RGBA", (6, 10), (0, 0, 0, 0))
mp = m.load()
for y in range(10):
    for x in range(6):
        if (1 <= x <= 4 and (0 <= y <= 6 or 8 <= y <= 9)):
            mp[x, y] = (90, 40, 0, 255)
for y in list(range(1, 6)) + [8]:
    for x in (2, 3):
        mp[x, y] = (255, 220, 60, 255)
m.save(os.path.join(OUT, "QuestMark.png"))
print("ok")
