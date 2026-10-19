"""주인공 AnyPortrait 아틀라스 외형 변경 A단계 (사용자 2026-10-19: 외형이 바뀌고 디테일이 들어가도 된다).
리그·UV는 그대로 두고 파츠 영역별로 팔레트를 바꾼다 → 38개 클립에 자동 반영, 무기 소켓·방어구 교체 그대로.
- 머리카락(붉은 장발) → 옅은 금발 포니테일, 모자(갈색 고글 모자) → 숯빛 라벤더 후드 + 어두운 청록 유리 고글, 깃털 → 회색
- 피부는 채도를 낮춰 차분하게. 밝기 순위로 새 팔레트에 옮겨 원래 명암 구조를 유지한다.
원본: git 의 PlayerBaseTempt_0.png. 재실행하면 이미 바뀐 색을 다시 매핑하므로 원본에서 한 번만 돌린다.
사용: python Tools/pixelfx/recolor_player.py [--preview <out.png>]"""
import colorsys
import os
import sys
import numpy as np
from PIL import Image

P = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Sprite", "Player", "Atlas", "PlayerBaseTempt_0.png")
HAIR_BOX = (116, 618, 143, 647)   # x0, y0, x1, y1 (이미지 좌표, 위가 0)
HAT_BOX = (114, 178, 141, 192)
WING_MAX_X = 123                  # 모자 왼쪽 흰 깃털
BLONDE = [(70, 52, 44), (104, 80, 56), (152, 118, 72), (196, 160, 98), (226, 198, 132), (246, 230, 176)]
HOOD = [(30, 28, 40), (44, 40, 56), (62, 58, 78), (84, 80, 102), (110, 106, 128)]
LENS, LENS_HI = (44, 66, 78), (150, 222, 214)
FEATHER = [(120, 118, 132), (168, 166, 178), (204, 202, 212)]


def lum(c):
    return 0.299 * c[..., 0] + 0.587 * c[..., 1] + 0.114 * c[..., 2]


def ramp_map(pixels, ramp):
    """고유색을 밝기 순위로 ramp 단계에 배정."""
    u = np.unique(pixels.reshape(-1, 3), axis=0)
    order = np.argsort(lum(u.astype(float)))
    table = {}
    for rank, idx in enumerate(order):
        k = int(round(rank / max(len(u) - 1, 1) * (len(ramp) - 1)))
        table[tuple(u[idx])] = ramp[k]
    return table


def apply_box(a, box, fn):
    x0, y0, x1, y1 = box
    sub = a[y0:y1 + 1, x0:x1 + 1]
    fn(sub, x0, y0)


def recolor(a):
    a = a.copy()
    op = a[..., 3] > 0

    def hair(sub, x0, y0):
        m = sub[..., 3] > 0
        t = ramp_map(sub[m][:, :3], BLONDE)
        for yy, xx in zip(*np.nonzero(m)):
            sub[yy, xx, :3] = t[tuple(sub[yy, xx, :3])]

    def hat(sub, x0, y0):
        m = sub[..., 3] > 0
        L = lum(sub[..., :3].astype(float))
        white = m & (L > 200)
        body = m & ~white
        t = ramp_map(sub[body][:, :3], HOOD)
        for yy, xx in zip(*np.nonzero(body)):
            sub[yy, xx, :3] = t[tuple(sub[yy, xx, :3])]
        for yy, xx in zip(*np.nonzero(white)):
            if x0 + xx <= WING_MAX_X:
                sub[yy, xx, :3] = FEATHER[2] if L[yy, xx] > 240 else FEATHER[1]
            else:
                sub[yy, xx, :3] = LENS_HI if L[yy, xx] > 250 and (yy + xx) % 3 == 0 else LENS

    apply_box(a, HAIR_BOX, hair)
    apply_box(a, HAT_BOX, hat)
    # 피부(주황 기 h 10~25, 밝음): 채도 ×0.78, 명도 ×0.97
    rgb = a[..., :3].astype(float) / 255
    for yy, xx in zip(*np.nonzero(op)):
        r, g, b = rgb[yy, xx]
        h, s, v = colorsys.rgb_to_hsv(r, g, b)
        if 0.025 < h < 0.075 and v > 0.85 and s > 0.25:
            r, g, b = colorsys.hsv_to_rgb(h, s * 0.78, v * 0.97)
            a[yy, xx, :3] = (int(r * 255), int(g * 255), int(b * 255))
    return a


def main():
    a = np.array(Image.open(P).convert("RGBA"))
    res = recolor(a)
    if "--preview" in sys.argv:
        out = sys.argv[sys.argv.index("--preview") + 1]
        T = 8
        tiles = []
        for box in (HAT_BOX, HAIR_BOX, (386, 976, 401, 990)):
            x0, y0, x1, y1 = box
            for arr in (a, res):
                tiles.append(Image.fromarray(arr[y0:y1 + 1, x0:x1 + 1]).resize(((x1 - x0 + 1) * T, (y1 - y0 + 1) * T), Image.NEAREST))
        W = sum(t.width for t in tiles) + 10 * len(tiles)
        sheet = Image.new("RGBA", (W, max(t.height for t in tiles)), (58, 62, 70, 255))
        x = 0
        for t in tiles:
            sheet.alpha_composite(t, (x, 0)); x += t.width + 10
        sheet.save(out); print("preview", out); return
    Image.fromarray(res).save(P)
    print("player recolored")


if __name__ == "__main__":
    main()
