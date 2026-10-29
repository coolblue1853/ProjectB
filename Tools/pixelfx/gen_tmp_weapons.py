"""모션 확인용 임시 도트 무기(10-29 사용자: 모션에 맞는 임시 무기로 합본 영상). 칼날·자루가 위, 손잡이 피벗, 24 PPU — Resources/PixelRig/Weapons 규약.
기존 도트 무기(대검·단검·활·지팡이·너클·창·방패)에 없는 것만 그린다: 카타나·해머·대포·투척 단검·사슬낫·권총·소총.
사용: python Tools/pixelfx/gen_tmp_weapons.py → Unity 에서 Tools/probes/import_tmp_weapons.cs(스프라이트 피벗·PPU 설정)
"""
import json
import os

from PIL import Image

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Resources", "PixelRig", "Weapons")
O = (27, 25, 32, 255)        # 외곽선
S1, S2 = (214, 222, 230, 255), (138, 150, 163, 255)   # 강철 밝음·어둠
W1, W2 = (140, 98, 57, 255), (98, 68, 44, 255)        # 나무
G = (201, 162, 58, 255)      # 금속 장식
I1, I2 = (96, 102, 112, 255), (62, 66, 75, 255)       # 쇠


def canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def rect(im, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            im.putpixel((x, y), c)


def outline(im):
    w, h = im.size
    src = im.copy()
    for y in range(h):
        for x in range(w):
            if src.getpixel((x, y))[3]:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h and src.getpixel((nx, ny))[3] and src.getpixel((nx, ny)) != O:
                    im.putpixel((x, y), O)
                    break
    return im


def katana():
    im = canvas(7, 42)
    for y in range(2, 30):  # 살짝 휜 칼날(위로 갈수록 뒤로 1px)
        x = 3 if y > 12 else 2
        im.putpixel((x, y), S1)
        im.putpixel((x + 1, y), S2)
    rect(im, 1, 30, 5, 30, G)        # 코등이
    rect(im, 3, 31, 3, 39, W2)       # 자루
    for y in range(32, 40, 2):
        im.putpixel((3, y), O)
    return outline(im), (3, 36), False


def hammer():
    im = canvas(16, 36)
    rect(im, 7, 9, 8, 34, W1)
    rect(im, 8, 9, 8, 34, W2)
    rect(im, 2, 2, 13, 9, I1)        # 머리
    rect(im, 2, 8, 13, 9, I2)
    rect(im, 2, 2, 3, 9, I2)
    return outline(im), (7, 30), False


def cannon():
    im = canvas(10, 34)
    rect(im, 2, 1, 7, 30, I1)        # 포신
    rect(im, 6, 1, 7, 30, I2)
    rect(im, 1, 1, 8, 3, I2)         # 포구
    rect(im, 2, 12, 7, 13, G)        # 띠
    rect(im, 2, 24, 7, 25, G)
    rect(im, 3, 31, 6, 32, W2)       # 손잡이
    return outline(im), (4, 22), False


def knife():
    im = canvas(5, 14)
    rect(im, 2, 1, 2, 8, S1)
    rect(im, 3, 2, 3, 8, S2)
    rect(im, 1, 9, 3, 9, G)
    rect(im, 2, 10, 2, 12, W2)
    return outline(im), (2, 11), False


def hook():
    im = canvas(14, 30)
    rect(im, 6, 6, 7, 28, W1)
    rect(im, 7, 6, 7, 28, W2)
    for x, y in ((6, 5), (5, 4), (4, 3), (3, 3), (2, 4), (2, 5), (1, 6), (8, 5), (9, 4), (10, 4), (11, 5)):  # 낫 날
        im.putpixel((x, y), S1)
    for x, y in ((12, 6), (12, 7), (11, 8)):
        im.putpixel((x, y), S2)
    for y in range(18, 28, 3):       # 사슬 감은 자국
        im.putpixel((6, y), I2)
    return outline(im), (6, 24), False


def pistol():
    im = canvas(7, 13)
    rect(im, 2, 1, 4, 7, I1)         # 총신(위)
    rect(im, 4, 1, 4, 7, I2)
    rect(im, 2, 8, 4, 11, W2)        # 손잡이
    im.putpixel((5, 8), I2)          # 방아쇠울
    return outline(im), (3, 9), False


def rifle():
    im = canvas(7, 40)
    rect(im, 3, 1, 3, 20, I2)        # 총열
    rect(im, 2, 21, 4, 29, I1)       # 몸통
    rect(im, 2, 12, 4, 20, W1)       # 앞 손잡이(받침손)
    rect(im, 2, 30, 4, 38, W2)       # 개머리판
    im.putpixel((5, 27), I2)
    return outline(im), (3, 28), False


def main():
    for name, fn in (("TmpKatana", katana), ("TmpHammer", hammer), ("TmpCannon", cannon), ("TmpKnife", knife), ("TmpHook", hook), ("TmpPistol", pistol), ("TmpRifle", rifle)):
        im, grip, upright = fn()
        im.save(os.path.join(OUT, name + ".png"))
        with open(os.path.join(OUT, name + ".json"), "w") as fp:
            json.dump({"grip": list(grip), "size": list(im.size), "angle": 90.0, "kind": "blade", "upright": upright, "tmp": True}, fp)
        print(name, im.size, grip)


if __name__ == "__main__":
    main()
