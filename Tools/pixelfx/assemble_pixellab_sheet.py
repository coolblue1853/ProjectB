# PixelLab animate_image 결과(프레임 PNG들)를 64px 가로 시트로 합친다.
# 사용: python assemble_pixellab_sheet.py <out.png> <dy> <frame1.png> [frame2.png ...]
# dy: 발끝을 시트 바닥(y=62 부근)에 맞추기 위한 세로 이동량. 모든 프레임에 같은 값을 써야 흔들리지 않는다.
import sys
from PIL import Image

def main():
    out_path, dy, frames = sys.argv[1], int(sys.argv[2]), sys.argv[3:]
    size = 64
    sheet = Image.new("RGBA", (size * len(frames), size), (0, 0, 0, 0))
    for i, path in enumerate(frames):
        im = Image.open(path).convert("RGBA")
        if im.size != (size, size):
            raise SystemExit(f"{path}: {im.size} (64x64 아님)")
        sheet.alpha_composite(im, (i * size, dy))
    sheet.save(out_path)
    print(out_path, sheet.size)

if __name__ == "__main__":
    main()
