"""한 동작의 칼 든 팔만 바꾼 안 여러 개를 졸라맨 GIF로 나란히(생성 0).
사용: python Tools/pixelfx/stick_variants.py <출력.gif>  — 기준은 stick_gif.PROPOSAL["SwingUp"], 아래 VARIANTS 가 구간별 칼팔(ra, rf)과 프레임별 칼 각도를 바꾼다.
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stick_gif as g  # noqa: E402

M = "SwingUp"
# 구간 순서: 0 웅크림 1 유지 2 아래 3 앞 위 4 끝 5 끝 유지 6 회복1 7 회복2 8 대기
VARIANTS = {
    "v16 지금": ({}, g.ANGLES[M]),
    "①안 고른 호": ({2: (-95, -90), 3: (10, 15), 4: (105, 112), 5: (106, 113)},
                 [-160, -165, -165, -70, 20, 115, 118, 119, 180, -150, -60, -20]),
    "②안 팔꿈치 굽힘": ({3: (30, 60), 4: (95, 140), 5: (96, 141), 6: (150, 190)},
                    [-160, -165, -165, -60, 60, 145, 148, 149, 185, -150, -60, -20]),
    "③안 수직 끊기": ({3: (30, 45), 4: (75, 85), 5: (76, 86), 6: (40, 20), 7: (-60, -40)},
                   [-160, -165, -165, -60, 50, 90, 92, 93, 40, -10, -30, -20]),
}


def keys_with(arm):
    start, segs, end = g.PROPOSAL[M]
    out = []
    for i, (k, n) in enumerate(segs):
        if i in arm:
            k = (k[0], k[1]) + arm[i] + tuple(k[4:])
        out.append((k, n))
    return [start, out, end]


def main():
    out = sys.argv[1]
    cols = []
    for name, (arm, ang) in VARIANTS.items():
        fr = g.frames_of(keys_with(arm), ang)
        cols.append((name, fr))
    n = len(cols[0][1])
    gif = []
    for i in range(n):
        ims = []
        for name, fr in cols:
            k, a = fr[i]
            im, _, _ = g.draw(k, a, scale=4)
            ImageDraw.Draw(im).text((6, 4), f"{name}  {i}", fill=(180, 30, 30), font=g.FONT)
            ims.append(im)
        c = Image.new("RGB", (sum(x.width + 6 for x in ims), ims[0].height), (255, 255, 255))
        x = 0
        for im in ims:
            c.paste(im, (x, 0))
            x += im.width + 6
        gif.append(c)
    gif[0].save(out, save_all=True, append_images=gif[1:], duration=g.TIMES[M], loop=0)
    s = Image.new("RGB", (gif[0].width, gif[0].height * 4), (255, 255, 255))
    for j, i in enumerate([2, 3, 4, 6]):
        s.paste(gif[i], (0, j * gif[0].height))
    s.resize((s.width * 2 // 5, s.height * 2 // 5)).save(out.replace(".gif", "_keys.png"))


if __name__ == "__main__":
    main()
