"""PixelLab 애니 잡 결과(첫 프레임 포함 5장)를 받아 폴더에 0.png~4.png 로 저장하고, 여러 구간을 이어 번호 띠를 만든다.
사용: python Tools/pixelfx/segfetch.py <모션 폴더> <구간이름=job_id>... [--strip 출력.png]
"""
import os
import sys
import urllib.request

from PIL import Image, ImageDraw


def fetch(folder, name, job, n=5):
    os.makedirs(os.path.join(folder, name), exist_ok=True)
    for i in range(n):
        urllib.request.urlretrieve(f"https://api.pixellab.ai/mcp/images/{job}/download?index={i}", os.path.join(folder, name, f"{i}.png"))


def strip(folder, segs, out):
    seq = [f"{segs[0]}/0"] + [f"{s}/{i}" for s in segs for i in range(1, 5)]
    o = Image.new("RGBA", (64 * len(seq), 76), (60, 70, 60, 255))
    d = ImageDraw.Draw(o)
    for k, s in enumerate(seq):
        o.alpha_composite(Image.open(os.path.join(folder, s + ".png")).convert("RGBA"), (64 * k, 12))
        d.text((64 * k + 2, 0), f"{k} {s}", fill=(255, 230, 120))
    o.resize((o.width * 2, o.height * 2), Image.NEAREST).save(out)
    return seq


def main():
    folder, args = sys.argv[1], sys.argv[2:]
    out = None
    if "--strip" in args:
        out = args[args.index("--strip") + 1]
        args = args[:args.index("--strip")]
    segs = []
    for a in args:
        name, job = a.split("=") if "=" in a else (a, None)
        if job:
            fetch(folder, name, job)
        segs.append(name)
    if out:
        print(strip(folder, segs, out))


if __name__ == "__main__":
    main()
