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


# 10-29 사용자 "우리 무기가 이상하다 — 직접 만들어 씌워라": 무기 종류별 임시 무기 전부(같은 외곽선·팔레트로 통일)
L1, L2 = (120, 132, 150, 255), (84, 92, 108, 255)   # 날 어두운 쪽(크기 큰 날 음영)


def blade(im, x, y0, y1, w=2):
    for y in range(y0, y1 + 1):
        for k in range(w):
            im.putpixel((x + k, y), S1 if k < w - 1 or w == 1 else S2)


def sword():
    im = canvas(9, 26)
    blade(im, 3, 2, 17, 3)
    im.putpixel((4, 1), S1)
    rect(im, 1, 18, 7, 18, G)        # 가드
    rect(im, 4, 19, 4, 23, W2)       # 손잡이
    im.putpixel((4, 24), G)
    return outline(im), (4, 21), False


def greatsword():
    im = canvas(13, 48)
    rect(im, 4, 3, 8, 34, S1)
    rect(im, 7, 3, 8, 34, S2)
    rect(im, 6, 5, 6, 32, L1)        # 홈
    for x in range(5, 8):
        im.putpixel((x, 2), S1)
    im.putpixel((6, 1), S1)
    rect(im, 1, 35, 11, 36, G)
    rect(im, 5, 37, 7, 45, W2)
    rect(im, 5, 46, 7, 46, G)
    return outline(im), (6, 41), False


def dagger():
    im = canvas(7, 16)
    blade(im, 2, 2, 9, 2)
    im.putpixel((2, 1), S1)
    rect(im, 1, 10, 5, 10, G)
    rect(im, 3, 11, 3, 14, W2)
    return outline(im), (3, 12), False


def spear():
    im = canvas(7, 62)
    rect(im, 3, 9, 3, 60, W1)        # 자루
    for y in range(12, 60, 6):
        im.putpixel((3, y), W2)
    for y, w in ((1, 0), (2, 1), (3, 1), (4, 2), (5, 2), (6, 2), (7, 1), (8, 1)):  # 창날
        rect(im, 3 - w, y, 3 + w, y, S1)
        im.putpixel((3 + w, y), S2)
    rect(im, 2, 9, 4, 9, G)
    return outline(im), (3, 46), False


def staff():
    im = canvas(11, 46)
    rect(im, 5, 9, 5, 44, W1)
    rect(im, 4, 2, 6, 8, (110, 170, 230, 255))   # 보석
    rect(im, 5, 3, 5, 4, (220, 240, 255, 255))
    rect(im, 3, 8, 7, 9, G)
    im.putpixel((3, 3), G)
    im.putpixel((7, 3), G)
    return outline(im), (5, 34), False


def bow():
    im = canvas(10, 32)
    pts = [(2, 1), (3, 2), (4, 4), (5, 7), (5, 10), (6, 13), (6, 16), (6, 19), (5, 22), (5, 25), (4, 28), (3, 30), (2, 31)]
    for x, y in pts:
        im.putpixel((x, y), W1)
        im.putpixel((x + 1, y), W2)
    for y in range(2, 31):           # 시위
        im.putpixel((1, y), (230, 225, 210, 255))
    rect(im, 6, 14, 7, 18, W2)       # 손잡이 감개
    return outline(im), (7, 16), True


def shield():
    im = canvas(18, 22)
    for y in range(1, 21):
        half = 8 if y < 13 else max(1, 8 - (y - 12))
        rect(im, 9 - half, y, 8 + half, y, (110, 70, 45, 255))
    for y in range(1, 21):           # 테두리 쇠
        half = 8 if y < 13 else max(1, 8 - (y - 12))
        im.putpixel((9 - half, y), I1)
        im.putpixel((8 + half, y), I1)
    rect(im, 1, 1, 16, 1, I1)
    rect(im, 8, 3, 9, 18, G)         # 문장 세로
    rect(im, 4, 8, 13, 9, G)         # 가로
    return outline(im), (9, 10), True


def knuckle():
    im = canvas(8, 8)
    rect(im, 1, 1, 6, 3, I1)
    rect(im, 1, 3, 6, 3, I2)
    for x in (1, 3, 5):
        im.putpixel((x, 1), S1)
    rect(im, 2, 4, 5, 6, W2)
    return outline(im), (3, 5), True


def whip_handle():
    im = canvas(5, 14)
    rect(im, 2, 1, 2, 9, (90, 60, 40, 255))   # 감긴 가죽 끈(늘어나는 끈은 T1 줄 그리기에서)
    for y in range(2, 9, 2):
        im.putpixel((2, y), W2)
    rect(im, 1, 10, 3, 12, W2)
    return outline(im), (2, 11), False


def cutlass():
    """해적 커틀러스(10-29 밤 직업 최종본): 넓고 휜 칼날 + 금색 손 보호 테."""
    im = canvas(11, 28)
    for y in range(2, 19):  # 위로 갈수록 앞(오른쪽)으로 휘고 넓어지는 칼날
        x = 3 + (19 - y) // 6
        w = 3 if y > 6 else 2
        for k in range(w):
            im.putpixel((x + k, y), S1 if k < w - 1 else S2)
    rect(im, 1, 19, 8, 19, G)
    for y in range(20, 25):          # 손 보호 테(앞쪽 고리)
        im.putpixel((8, y), G)
    rect(im, 4, 20, 4, 24, W2)
    rect(im, 4, 25, 8, 25, G)
    return outline(im), (4, 22), False


def anchor():
    """심해 닻(양손 둔기): 긴 쇠 자루 + 위쪽 고리, 아래(타격쪽)가 아니라 위가 머리 — 칼날 쪽 = 갈고리 두 개."""
    im = canvas(19, 46)
    A1, A2 = (88, 110, 112, 255), (52, 70, 74, 255)   # 녹슨 청동
    R = (150, 90, 60, 255)                             # 녹
    rect(im, 8, 6, 10, 44, A1)
    rect(im, 10, 6, 10, 44, A2)
    for x in range(1, 18):           # 갈고리 가로대(위쪽 = 타격 머리)
        dy = abs(x - 9) // 3
        rect(im, x, 6 - dy, x, 7 - dy, A1 if x < 9 else A2)
    for x, y in ((1, 2), (1, 3), (2, 1), (17, 2), (17, 3), (16, 1)):   # 갈고리 끝
        im.putpixel((x, y), A1)
    rect(im, 6, 14, 12, 15, A2)      # 가로 막대(스톡)
    for y in (20, 27, 33):
        im.putpixel((9, y), R)
    rect(im, 7, 40, 11, 44, A2)      # 아래 고리
    rect(im, 8, 41, 10, 43, (0, 0, 0, 0))
    return outline(im), (9, 34), False


def eye_staff():
    """마도학자 지팡이: 비틀린 나무 + 위에 눈알 보석(크툴루풍)."""
    im = canvas(11, 46)
    for y in range(10, 45):
        im.putpixel((5 + (1 if (y // 4) % 2 else 0), y), W2)
    rect(im, 3, 2, 7, 7, (60, 140, 120, 255))      # 녹색 눈알
    rect(im, 4, 3, 6, 6, (210, 230, 190, 255))
    rect(im, 5, 4, 5, 5, (30, 20, 40, 255))        # 눈동자
    for x, y in ((2, 8), (8, 8), (3, 9), (7, 9), (1, 6), (9, 6)):   # 촉수 받침
        im.putpixel((x, y), (110, 70, 130, 255))
    return outline(im), (5, 34), False


def tome():
    """마도서(앞손, 세움): 보라 표지 + 금 장식."""
    im = canvas(10, 12)
    rect(im, 1, 1, 8, 10, (88, 52, 110, 255))
    rect(im, 8, 1, 8, 10, (60, 34, 78, 255))
    rect(im, 1, 10, 7, 10, (230, 220, 200, 255))  # 종이 모서리
    rect(im, 4, 4, 5, 6, G)
    return outline(im), (5, 6), True


def scabbard():
    """카타나 칼집(앞손, 10-30 납도 품질 — GPT "칼집에 들어가는 게 안 보인다"): 검은 옻칠 + 금 입구 고리. 피벗 = 입구(손이 잡는 곳), 칼집은 위(각도 방향)로 뻗는다."""
    im = canvas(6, 30)
    rect(im, 2, 2, 3, 27, (40, 30, 42, 255))
    rect(im, 3, 2, 3, 27, (24, 18, 26, 255))
    rect(im, 1, 26, 4, 27, G)        # 입구 고리
    rect(im, 2, 2, 3, 3, G)          # 끝 장식
    return outline(im), (2, 27), False


def main():
    for name, fn in (("TmpKatana", katana), ("TmpHammer", hammer), ("TmpCannon", cannon), ("TmpKnife", knife), ("TmpHook", hook), ("TmpPistol", pistol), ("TmpRifle", rifle), ("TmpSword", sword), ("TmpGreatSword", greatsword), ("TmpDagger", dagger), ("TmpSpear", spear), ("TmpStaff", staff), ("TmpBow", bow), ("TmpShield", shield), ("TmpKnuckle", knuckle), ("TmpWhip", whip_handle), ("TmpCutlass", cutlass), ("TmpAnchor", anchor), ("TmpEyeStaff", eye_staff), ("TmpTome", tome), ("TmpScabbard", scabbard)):
        im, grip, upright = fn()
        im.save(os.path.join(OUT, name + ".png"))
        with open(os.path.join(OUT, name + ".json"), "w") as fp:
            json.dump({"grip": list(grip), "size": list(im.size), "angle": 90.0, "kind": "blade", "upright": upright, "tmp": True}, fp)
        print(name, im.size, grip)


if __name__ == "__main__":
    main()
