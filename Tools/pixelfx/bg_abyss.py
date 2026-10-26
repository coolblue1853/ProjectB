"""공용 원경(LV1_Field) 어비스풍 정리 + 먼 탑 실루엣 생성.
1) 팔레트 교체: 레이어마다 색 4~7개를 1:1로 바꾼다(형태·도트는 그대로). 먼 층일수록 밝고 흐리게(공기원근),
   구름은 흰색 대신 옅은 안개·라벤더. 맵별 틴트(곱셈)는 씬에 그대로 남는다.
2) far-tower.png: 구름 위로 솟은 먼 탑(TowerWorld). 논리 픽셀로 그려 8배 확대(다른 원경과 같은 도트 크기).
사용: python Tools/pixelfx/bg_abyss.py <src_dir> <dst_dir>   (원본은 Docs/ArtCompare/Background/orig 에 보관)
"""
import random
import sys
from pathlib import Path
from PIL import Image

REMAP = {
    "far-clouds": {
        (228, 240, 244): (204, 212, 216), (201, 211, 211): (178, 188, 196), (171, 185, 180): (152, 162, 176),
    },
    "near-clouds": {
        (117, 151, 169): (110, 132, 146), (156, 171, 200): (142, 150, 176), (156, 193, 200): (140, 168, 172),
        (191, 206, 221): (172, 182, 200), (228, 239, 244): (200, 208, 214),
    },
    "far-mountains": {
        (23, 26, 25): (60, 74, 82), (38, 48, 43): (70, 86, 92), (46, 67, 63): (78, 98, 102),
        (54, 85, 75): (90, 112, 112), (38, 38, 50): (66, 70, 90),
    },
    "mountains": {
        (33, 36, 35): (40, 50, 54), (55, 68, 62): (56, 72, 74), (151, 148, 135): (122, 130, 136),
        (66, 95, 76): (62, 90, 84), (106, 102, 93): (92, 98, 106), (50, 47, 60): (52, 54, 72),
    },
    "trees": {
        (34, 36, 33): (28, 34, 35), (62, 68, 55): (48, 62, 58), (85, 95, 66): (66, 86, 74), (50, 47, 60): (44, 44, 60),
    },
}

TOWER = {"hi": (150, 160, 174), "mid": (124, 134, 150), "lo": (102, 110, 128), "edge": (86, 92, 112),
         "win": (70, 76, 96), "glow": (168, 232, 226), "glow2": (120, 190, 196)}


def remap(src: Path, dst: Path, table: dict) -> None:
    im = Image.open(src).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a and (r, g, b) in table:
                px[x, y] = table[(r, g, b)] + (a,)
    im.save(dst)


def tower(dst: Path) -> None:
    W, H = 64, 240  # 논리 픽셀 → ×8 = 512 x 1920 (다른 원경과 같은 높이)
    im = Image.new("RGBA", (W, H))
    px = im.load()
    rnd = random.Random(7)
    cx, top, base = 32, 20, 239
    for y in range(top, H):
        t = (y - top) / (base - top)
        half = int(4 + t * 7)  # 위 4 → 아래 11 (위로 갈수록 가늘다)
        if y < top + 26:
            half = 3 if y > top + 6 else 2  # 첨탑
        for x in range(cx - half, cx + half + 1):
            rel = (x - (cx - half)) / max(1, 2 * half)
            col = TOWER["hi"] if rel < 0.25 else TOWER["mid"] if rel < 0.7 else TOWER["lo"]
            if x in (cx - half, cx + half):
                col = TOWER["edge"] if x == cx + half else TOWER["mid"]
            px[x, y] = col + (255,)
        if (y - top) % 16 == 0 and y > top + 26:  # 층 띠: 밝은 줄 + 어두운 줄, 양옆으로 1px 돌출
            for x in range(cx - half - 1, cx + half + 2):
                px[x, y] = TOWER["hi"] + (255,)
                if y + 1 < H:
                    px[x, y + 1] = TOWER["edge"] + (255,)
        if (y - top) % 16 == 8 and y > top + 30:  # 창: 대부분 어둡고 드물게 청록 불빛
            for x in range(cx - half + 2, cx + half - 1, 3):
                glow = rnd.random() < 0.12
                px[x, y] = (TOWER["glow"] if glow else TOWER["win"]) + (255,)
    # 꼭대기 불빛과 떠 있는 고리(어비스풍 기묘함)
    for y in range(top - 6, top):
        px[cx, y] = TOWER["edge"] + (255,)
    px[cx, top - 7] = TOWER["glow"] + (255,)
    for dx in (-1, 1):
        px[cx + dx, top - 7] = TOWER["glow2"] + (255,)
    px[cx, top - 8] = TOWER["glow2"] + (255,)
    ring_y = top + 34
    for dx in range(-14, 15):
        if abs(dx) > 5:
            px[cx + dx, ring_y + (1 if abs(dx) > 11 else 0)] = (TOWER["mid"] if dx < 0 else TOWER["lo"]) + (255,)
    # 무너진 틈: 몸통 가운데 몇 칸을 빼서 실루엣에 상처를 낸다
    for (gx, gy) in [(cx + 5, 120), (cx + 6, 121), (cx + 6, 122), (cx - 7, 170), (cx - 8, 171)]:
        if 0 <= gx < W:
            px[gx, gy] = (0, 0, 0, 0)
    im.resize((W * 8, H * 8), Image.NEAREST).save(dst)


def main() -> None:
    src, dst = Path(sys.argv[1]), Path(sys.argv[2])
    dst.mkdir(parents=True, exist_ok=True)
    for name, table in REMAP.items():
        remap(src / f"{name}.png", dst / f"{name}.png", table)
        print("remap", name)
    tower(dst / "far-tower.png")
    print("tower")


if __name__ == "__main__":
    main()
