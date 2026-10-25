"""픽셀 리그 주인공용 도트 무기 그림 (PixelRigDesign 3.5절 '무기 쥐기'). PixelLab 없이 직접 그린다.

기존 무기 스프라이트는 AnyPortrait 캐릭터용 고해상도(512px·100 PPU)라 도트 격자에서 가는 막대로 뭉개졌다.
캐릭터와 같은 도트 밀도(24 PPU)로 칼날이 위를 향하게 그리고, 손잡이 가운데를 피벗으로 쓴다(손이 손잡이를 덮음).
결과: Assets/Resources/PixelRig/Weapons/<원본 스프라이트 이름>.png + 같은 이름 .json(피벗 픽셀)

사용: python Tools/pixelfx/draw_weapon_pixel.py
"""
import json
import os

from PIL import Image

OUT = "Assets/Resources/PixelRig/Weapons"

# 팔레트: 외곽선 / 칼날 밝음·중간·어두움 / 녹(어두움·밝음) / 코등이·칼자루 끝(놋쇠) / 손잡이 가죽
PAL = {
    "O": (28, 22, 30), "W": (226, 230, 236), "L": (176, 184, 198), "M": (122, 128, 146), "D": (78, 80, 98),
    "r": (122, 66, 44), "R": (170, 98, 58), "g": (150, 104, 54), "G": (212, 160, 84), "h": (70, 42, 32), "H": (112, 70, 46),
    ".": None,
}

# 녹슨 소검(ShortSword) — 칼날 위, 7x22. 한 줄 = 위→아래, 왼쪽이 칼등(빛 받는 면은 왼쪽 위)
SHORT_SWORD = [
    "...O...",
    "..OWO..",
    "..OWLO.",
    ".OWLMO.",
    ".OWLMO.",
    ".OWrMO.",
    ".OWLMO.",
    ".OWLDO.",
    ".OWRMO.",
    ".OWLrO.",
    ".OWLMO.",
    ".OWLDO.",
    ".OWLMO.",
    ".OLMDO.",
    "OOOOOOO",
    "OGGgGgO",
    "OOOOOOO",
    "..OHO..",
    "..OhO..",
    "..OHO..",
    ".OGgGO.",
    "..OOO..",
]
GRIP = (3, 18)  # 손잡이 가운데(가로 3, 세로 18행) = 피벗

# 단검(DoubleDagger) — 원본은 X자 교차 아이콘이라 한 자루를 직접 그린다. 5x14, 짧고 넓은 날
DAGGER = [
    "..O..",
    ".OWO.",
    ".OLMO",
    "OWMDO",
    "OWLDO",
    "OLMDO",
    "OWMDO",
    "OLDDO",
    "OOOOO",
    "OGgGO",
    ".OHO.",
    ".OhO.",
    ".OgO.",
    "..O..",
]
DAGGER_GRIP = (2, 11)


def draw(name, rows, grip):
    h, w = len(rows), len(rows[0])
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        assert len(row) == w, (name, y, row)
        for x, ch in enumerate(row):
            c = PAL[ch]
            if c is not None:
                img.putpixel((x, y), c + (255,))
    os.makedirs(OUT, exist_ok=True)
    img.save(os.path.join(OUT, name + ".png"))
    with open(os.path.join(OUT, name + ".json"), "w", encoding="utf-8") as f:
        json.dump({"grip": list(grip), "size": [w, h], "angle": 90.0, "kind": "blade", "upright": False}, f)
    print(name, w, "x", h, "grip", grip)


if __name__ == "__main__":
    draw("ShortSword", SHORT_SWORD, GRIP)
    draw("DoubleDagger", DAGGER, DAGGER_GRIP)
