"""원본 앵커(빨간 머리 여우 소녀)에서 머리카락·귀·꼬리·옷을 지워 '기본 체형'을 만든다 — AI 재생성 없이 원본 픽셀에서 직접.

사용자: "원본 사진에서 머리카락이랑 옷, 꼬리를 날린다고 생각하는 게 빠를 것 같다."
(PixelLab 편집은 원본을 새로 그려 머리 모양·얼굴 위치·눈·몸 폭이 바뀌었다 — gen02/overlay_check.png)

- 얼굴(눈·입·볼·피부)과 다리는 원본 픽셀을 그대로 쓴다.
- 머리카락 자리: 얼굴 중심에 맞춘 둥근 맨머리(피부색 + 1px 검정 외곽선) — 2등신 머리 크기 유지.
- 옷 자리: 같은 폭의 몸통(회색 민소매·반바지)과 소매 자리의 팔·손.
- 귀·꼬리: 삭제.
좌표는 원본 64×64 캔버스 기준. 사용: python Tools/pixelfx/make_base_body.py [out.png]
"""
import sys

import numpy as np
from PIL import Image

SRC = "Docs/ArtCompare/hero_style_anchor_64.png"
SKETCH = "Docs/ArtCompare/CharacterGen/gen03/body_sketch_user.png"
_args = [x for x in sys.argv[1:] if not x.startswith("--")]
OUT = _args[0] if _args else "Docs/ArtCompare/CharacterGen/gen03/base_body_pixel.png"

BLACK = (0, 0, 0)
SKIN = (252, 231, 221)      # 원본 얼굴 밝은 피부(j)
SKIN_MID = (251, 206, 192)  # 원본 피부 중간(m)
SKIN_DARK = (219, 139, 133)  # 원본 피부 그늘(l) — 다리에 쓰인 색
TOP = (254, 255, 251)       # 원본 블라우스 흰색(d)
TOP_D = (203, 172, 172)     # 원본 소매 그늘(g)
PANTS = (77, 33, 49)        # 원본 원피스 자주(c)
PANTS_D = (33, 15, 25)      # 원본 원피스 그늘(h)

FACE_COLORS = {(252, 231, 221), (251, 206, 192), (250, 182, 165), (188, 97, 102), (219, 139, 133),
               (151, 67, 10), (189, 114, 12), (245, 177, 76), (254, 255, 251), (255, 255, 255)}


def main():
    src = np.array(Image.open(SRC).convert("RGBA")).astype(int)
    out = np.zeros_like(src)

    def put(x, y, c, a=255):
        if 0 <= x < 64 and 0 <= y < 64:
            out[y, x, :3] = c
            out[y, x, 3] = a

    # 1) 맨머리: 얼굴 중심(x≈21.5)에 맞춘 타원(위 13행~), 31행 아래는 원본 턱선(35행)까지만
    cx, cy, rx, ry = 21.5, 24.0, 11.0, 11.0
    head = np.zeros((64, 64), bool)
    for y in range(64):
        for x in range(64):
            if y <= 31 and ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 <= 1.0:
                head[y, x] = True
    if "--sketch-head" in sys.argv:  # 사용자 스케치의 머리 실루엣(36행 위 불투명 영역)을 그대로 쓴다
        skh = np.array(Image.open(SKETCH).convert("RGBA"))
        head[:] = False
        head[:36] = skh[:36, :, 3] >= 128
    else:
        for y, (x0, x1) in ((32, (14, 29)), (33, (15, 28)), (34, (16, 27))):  # 턱: 좌우 대칭으로 한 칸씩 좁힘
            head[y, x0:x1 + 1] = True
        for y, x in ((30, 10), (31, 10), (30, 33), (31, 33)):  # 사람 귀
            head[y, x] = True
    for y, x in zip(*np.where(head)):
        shade = ((x - cx) / rx) * 0.6 + ((y - cy) / ry) * 0.6 > 0.7  # 위·왼쪽 광원
        if 29 <= y <= 34 and x <= 28:  # 볼: 볼터치와 겹쳐 얼룩지지 않게 음영은 가장자리 한 칸만
            shade = not head[y, x + 1]
        put(x, y, SKIN_MID if shade else SKIN)

    # 2) 원본 얼굴 픽셀 그대로 — 눈썹·눈·볼·입(입 안 빨강 a 포함)·턱선
    for y in range(27, 36):
        for x in range(15, 29):
            if src[y, x, 3] < 128 or not head[y, x] and y < 35:
                continue
            c = tuple(src[y, x, :3])
            mouth = c == (224, 66, 80) and 32 <= y <= 34 and 18 <= x <= 23
            if c in FACE_COLORS or (c == BLACK and 28 <= y <= 31) or mouth:
                put(x, y, c)

    # 3) 몸: 사용자 스케치(gen03/body_sketch_user.png) 실루엣 — 머리보다 훨씬 작은 A자 티셔츠, 아래 모서리로 손, 좁은 반바지
    sk = np.array(Image.open(SKETCH).convert("RGBA")).astype(int)
    SK_SHIRT, SK_HAND, SK_PANTS = (0, 183, 192), (219, 139, 133), {(77, 33, 49), (33, 15, 25)}
    for y in range(36, 51):
        for x in range(64):
            if sk[y, x, 3] < 128:
                continue
            c = tuple(sk[y, x, :3])
            if c == SK_SHIRT:
                put(x, y, TOP_D if x >= 27 or y == 43 else TOP)
            elif c == SK_HAND and y <= 44:
                put(x, y, SKIN_MID if x >= 29 else SKIN)
            elif c in SK_PANTS:
                put(x, y, PANTS_D if x >= 26 else PANTS)
    put(22, 36, SKIN)  # 목 V
    put(23, 36, SKIN_MID)
    put(22, 37, SKIN_MID)
    # 팔 안쪽 선(어깨→손): 팔이 몸통 옆으로 내려온 것처럼
    for (x, y) in ((19, 38), (18, 39), (17, 40), (17, 41), (26, 38), (27, 39), (28, 40), (28, 41)):
        put(x, y, TOP_D)
    # 손 사이 셔츠 밑단 그늘선
    for x in range(18, 28):
        if out[43, x, 3] and tuple(out[43, x, :3]) in (TOP, TOP_D):
            put(x, 43, TOP_D)

    # 5) 다리: 원본 51~54행 그대로(살구색 l·검정)
    for y in range(51, 55):
        for x in range(17, 30):
            if src[y, x, 3] >= 128 and tuple(src[y, x, :3]) == (219, 139, 133):
                put(x, y, tuple(src[y, x, :3]))

    # 6) 외곽선: 불투명 영역 바깥 테두리를 1px 검정으로
    al = out[..., 3] > 0
    edge = np.zeros_like(al)
    for y in range(64):
        for x in range(64):
            if al[y, x]:
                continue
            for dy, dx in ((0, 1), (1, 0), (0, -1), (-1, 0)):
                ny, nx = y + dy, x + dx
                if 0 <= ny < 64 and 0 <= nx < 64 and al[ny, nx]:
                    edge[y, x] = True
                    break
    for y, x in zip(*np.where(edge)):
        put(x, y, BLACK)
    Image.fromarray(out.astype(np.uint8), "RGBA").save(OUT)
    print(OUT)


if __name__ == "__main__":
    main()
