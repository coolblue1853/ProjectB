"""공용 원경(LV1_Field) 레이어를 1/8(논리 픽셀)로 겹쳐 보는 목업. 팔레트 교체 전후 비교용.
사용: python Tools/pixelfx/bg_mock.py <out.png> [layer_dir] [sky_hex]
"""
import sys
from PIL import Image

ORDER = ["far-clouds", "near-clouds", "far-mountains", "mountains", "trees"]


def mock(layer_dir: str, sky: tuple) -> Image.Image:
    w = 320
    canvas = Image.new("RGBA", (w, 240), sky + (255,))
    for name in ORDER:
        im = Image.open(f"{layer_dir}/{name}.png").convert("RGBA")
        im = im.resize((im.width // 8, im.height // 8), Image.NEAREST)
        x = 0
        while x < w:
            canvas.alpha_composite(im, (x, 0))
            x += im.width
    return canvas


if __name__ == "__main__":
    out = sys.argv[1]
    d = sys.argv[2] if len(sys.argv) > 2 else "Assets/Sprite/LV1_Field"
    sky = tuple(int(sys.argv[3][i:i + 2], 16) for i in (0, 2, 4)) if len(sys.argv) > 3 else (70, 120, 180)
    mock(d, sky).resize((960, 720), Image.NEAREST).save(out)
