"""PixelLab vfx_workbench 내보내기(시트 PNG + JSON)를 게임 도트 이펙트 시트로 바꾼다 (GameFeelPlan 10.7).

- 여러 줄 격자 → 한 줄 시트(PixelVfxBaker.ImportSheet 형식: Px_X.png + Px_X.json)
- 끝의 빈 프레임 제거
- 명암만 남김: RGB = 밝기(회색조) — PixelSlash2D 셰이더가 밝기를 무기 테마 색 램프로 칠한다
- 피벗: PixelLab 은 위쪽 기준 → Unity 아래쪽 기준으로 뒤집는다

사용: python Tools/pixelfx/import_pixellab_vfx.py <pixellab.json> <out_dir> <Px_이름>
"""
import json
import os
import sys

import numpy as np
from PIL import Image


def main():
    src_json, out_dir, name = sys.argv[1], sys.argv[2], sys.argv[3]
    meta = json.load(open(src_json, encoding="utf-8"))
    sheet = Image.open(os.path.splitext(src_json)[0] + ".png").convert("RGBA")
    frames = meta["frames"]
    w, h = frames[0]["frame"]["w"], frames[0]["frame"]["h"]
    cells = []
    for f in frames:
        r = f["frame"]
        cells.append(np.array(sheet.crop((r["x"], r["y"], r["x"] + w, r["y"] + h))))
    while len(cells) > 1 and cells[-1][..., 3].max() == 0:
        cells.pop()
    out = np.zeros((h, w * len(cells), 4), np.uint8)
    for i, c in enumerate(cells):
        lum = (0.299 * c[..., 0] + 0.587 * c[..., 1] + 0.114 * c[..., 2]).round().astype(np.uint8)
        g = np.stack([lum, lum, lum, c[..., 3]], -1)
        out[:, i * w:(i + 1) * w] = g
    os.makedirs(out_dir, exist_ok=True)
    Image.fromarray(out).save(os.path.join(out_dir, name + ".png"))
    piv = frames[0].get("pivot", {"x": 0.5, "y": 0.5})
    fps = round(1000.0 / max(frames[0].get("duration", 62.5), 1))
    with open(os.path.join(out_dir, name + ".json"), "w", encoding="utf-8") as fp:
        json.dump({"frames": len(cells), "w": w, "h": h, "fps": fps,
                   "pivot": [round(piv["x"], 4), round(1.0 - piv["y"], 4)],
                   "ramp": "gray", "source": "pixellab vfx_workbench: " + os.path.basename(src_json)}, fp)
    print(name, len(cells), "frames", w, "x", h, "fps", fps)


if __name__ == "__main__":
    main()
