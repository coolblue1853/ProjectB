"""키 자세 후보를 대기 셀 옆에 나란히 놓고 머리 폭·발 위치를 잰다(자체 점검용).
사용: python Tools/pixelfx/keyprev.py <출력.png> <png...>
머리 폭 = 위에서 22줄 안의 최대 불투명 폭, 발 = 맨 아래 3줄의 불투명 x 범위.
"""
import sys

import numpy as np
from PIL import Image, ImageDraw

IDLE = "Assets/Art/Player/PixelRig/Cel/Idle_0.png"


def measure(im):
    a = np.array(im)[:, :, 3] > 0
    ys = np.nonzero(a.any(1))[0]
    top, bot = ys.min(), ys.max()
    head = max(a[y].sum() for y in range(top, min(top + 22, bot)))
    xs = np.nonzero(a[bot - 2:bot + 1].any(0))[0]
    return head, (int(xs.min()), int(xs.max())), int(top), int(bot)


def main():
    out, paths = sys.argv[1], [IDLE] + sys.argv[2:]
    ims = [Image.open(p).convert("RGBA") for p in paths]
    o = Image.new("RGBA", (64 * len(ims), 76), (60, 70, 60, 255))
    d = ImageDraw.Draw(o)
    for k, im in enumerate(ims):
        o.alpha_composite(im, (64 * k, 12))
        h, f, t, b = measure(im)
        d.text((64 * k + 1, 0), f"h{h} f{f[0]}-{f[1]}", fill=(255, 230, 120))
        print(paths[k].split("/")[-1], "head", h, "feet", f, "top", t, "bot", b)
    o.resize((o.width * 4, o.height * 4), Image.NEAREST).save(out)


if __name__ == "__main__":
    main()
