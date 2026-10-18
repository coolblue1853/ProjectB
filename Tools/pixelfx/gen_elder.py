"""마을 장로 NPC 도트 — 기존 마을 NPC(BlackSmith/BalckSmith.png)와 같은 양식: 24×32 논리 픽셀, 큰 머리(2등신), 8배 확대, PPU 100.
2프레임 대기(숨쉬기 1px). 출력: Assets/Art/NPC/Elder.png (가로 시트 384×256)"""
import os
from PIL import Image

S = 8
W, H = 24, 32
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "NPC", "Elder.png")

PAL = {
    ".": None,
    "o": (40, 30, 34),      # 외곽선
    "h": (232, 228, 222),   # 흰 머리
    "H": (196, 190, 184),   # 머리 그림자
    "s": (246, 214, 190),   # 피부
    "S": (222, 178, 152),   # 피부 그림자
    "e": (40, 60, 70),      # 눈
    "r": (74, 104, 82),     # 로브
    "R": (52, 76, 60),      # 로브 그림자
    "g": (196, 160, 72),    # 금장 띠
    "w": (128, 90, 54),     # 지팡이
    "W": (92, 62, 36),
    "j": (130, 200, 255),   # 지팡이 보석
    "b": (70, 52, 40),      # 신발
}

BASE = [
    "........................",
    "......oooooooo..........",
    ".....ohhhhhhhhoo........",
    "....ohhhhhhhhhhho.......",
    "...ohhhhhhhhhhhhho......",
    "...ohhHhhhhhhhhHho......",
    "..ohhHsssssssssHhho.....",
    "..ohHsssssssssssHho.....",
    "..ohHsssssssssssHho.....",
    "..ohHssessssesssHho.....",
    "..ohHssessssesssHho.....",
    "..ohHSsssssssssSHho..o..",
    "..ohhhhhhhsShhhhhho.ojo.",
    "...ohhhhhhhhhhhhho..ojo.",
    "...ohhhhhhhhhhhhho...o..",
    "....ohhhhhhhhhhho....w..",
    ".....ohhhhhhhhho.....w..",
    ".....orrohhhhorro....w..",
    "....orrrrohhorrrro..ow..",
    "....orrrrrhhrrrrrrosso..",
    "....orrrrrrrrrrrrrosso..",
    "....oggggggggggggo.ow...",
    "....orrrrrrrrrrrRo..w...",
    "....orrrrrrrrrrRRo..w...",
    "....orrrrrrrrrrRRo..W...",
    "....orRrrrrrrrrRRo..W...",
    "....orRrrrrrrrRRRo..W...",
    "....orRRrrrrrRRRRo..W...",
    "....oRRRRRRRRRRRRo..W...",
    ".....obbbo...obbbo..W...",
    ".....obbbo...obbbo..W...",
    "......ooo.....ooo...o...",
]


def draw(rows, dy):
    im = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    px = im.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            c = PAL.get(ch)
            if c is None:
                continue
            yy = y + (dy if y < 21 else 0)  # 숨쉬기: 상체만 1px
            if yy < 0 or yy >= H:
                continue
            for oy in range(S):
                for ox in range(S):
                    px[x * S + ox, yy * S + oy] = c + (255,)
    return im


def main():
    assert all(len(r) == W for r in BASE), [len(r) for r in BASE]
    frames = [draw(BASE, 0), draw(BASE, 1)]
    sheet = Image.new("RGBA", (W * S * 2, H * S), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (i * W * S, 0))
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    sheet.save(OUT)
    print("ok", sheet.size)


if __name__ == "__main__":
    main()
