"""공격 셀 칼 점검: 초안(칼이 그려진 원본) 위에 cel.json 의 손 좌표·칼 각도 선을 겹쳐 프레임마다 비교한다.
사용: python Tools/pixelfx/angle_audit.py <출력.png> <클립,...>
빨간 점 = 손(손잡이), 노란 선 = 게임이 붙일 칼 방향(길이 20px), 숫자 = 각도.
"""
import json
import math
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(__file__))
from build_cel import CLIPS, SRC  # noqa: E402


def main():
    out, clips = sys.argv[1], sys.argv[2].split(",")
    d = json.load(open("Assets/Art/Player/PixelRig/Cel/cel.json", encoding="utf-8"))
    S = 4
    rows = []
    for c in clips:
        spec = CLIPS[c]
        srcs = [os.path.join(SRC, i + ".png") if isinstance(i, str) else os.path.join(SRC, spec[0], f"{i}.png") for i in spec[1]]
        fr = d[c]["frames"]
        row = Image.new("RGBA", (64 * S * len(fr), 64 * S + 16), (40, 44, 40, 255))
        dr = ImageDraw.Draw(row)
        for k, f in enumerate(fr):
            raw = Image.open(srcs[k]).convert("RGBA").resize((64 * S, 64 * S), Image.NEAREST)
            row.alpha_composite(raw, (64 * S * k, 16))
            hx, hy = f["hand"]
            ox, oy = 64 * S * k + hx * S, 16 + hy * S
            a = f["angle"]
            if a > -998:
                ex, ey = ox + 20 * S * math.cos(math.radians(a)), oy - 20 * S * math.sin(math.radians(a))
                dr.line((ox, oy, ex, ey), fill=(255, 230, 60), width=3)
            dr.ellipse((ox - 5, oy - 5, ox + 5, oy + 5), fill=(255, 40, 40))
            dr.text((64 * S * k + 3, 2), f"{c}{k} {'' if a < -998 else round(a)}", fill=(255, 255, 255))
        rows.append(row)
    W = max(r.width for r in rows)
    o = Image.new("RGBA", (W, sum(r.height for r in rows)))
    y = 0
    for r in rows:
        o.alpha_composite(r, (0, y))
        y += r.height
    o.save(out)


if __name__ == "__main__":
    main()
