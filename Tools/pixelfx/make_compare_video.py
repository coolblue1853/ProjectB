"""기본 이동·점프 비교 영상(Q-423): Logs/playtest/cmp_<태그>_<동작>.png(0.04초 간격 256px 컷) 여러 태그를 나란히 → GIF(+ ffmpeg 있으면 MP4).
사용: python Tools/pixelfx/make_compare_video.py <출력이름> <태그1> [태그2 ...]   예) ... cmp_v1 original rig
"""
import os
import shutil
import subprocess
import sys

from PIL import Image, ImageDraw

STEP_MS = 40
SCALE = 2
LABELS = {"original": "원본(AnyPortrait)", "rig": "현재 리그", "cel": "① 셀", "rig2": "② 리그+손그림"}


def frames(tag, action):
    s = Image.open(f"Logs/playtest/cmp_{tag}_{action}.png").convert("RGB")
    return [s.crop((i * 256, 0, (i + 1) * 256, 256)) for i in range(s.size[0] // 256)]


def main():
    name, tags = sys.argv[1], sys.argv[2:]
    out = []
    for action in ("walk", "run", "jump"):
        cols = [frames(t, action) for t in tags]
        n = min(len(c) for c in cols)
        for rep in range(2):  # 동작마다 두 번
            for i in range(n):
                f = Image.new("RGB", (256 * SCALE * len(tags), 256 * SCALE + 28), (20, 20, 24))
                d = ImageDraw.Draw(f)
                for k, t in enumerate(tags):
                    f.paste(cols[k][i].resize((256 * SCALE, 256 * SCALE), Image.NEAREST), (k * 256 * SCALE, 28))
                    d.text((k * 256 * SCALE + 8, 8), f"{t} - {action}", fill=(230, 230, 230))
                out.append(f)
    os.makedirs("Docs/ArtCompare/Motion", exist_ok=True)
    gif = f"Docs/ArtCompare/Motion/{name}.gif"
    out[0].save(gif, save_all=True, append_images=out[1:], duration=STEP_MS, loop=0)
    print(gif, len(out))
    ff = shutil.which("ffmpeg")
    if ff:
        tmp = "Logs/playtest/_vid"
        os.makedirs(tmp, exist_ok=True)
        for i, f in enumerate(out):
            f.save(f"{tmp}/{i:04d}.png")
        mp4 = f"Docs/ArtCompare/Motion/{name}.mp4"
        subprocess.run([ff, "-y", "-loglevel", "error", "-framerate", str(1000 // STEP_MS), "-i", f"{tmp}/%04d.png",
                        "-pix_fmt", "yuv420p", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2:flags=neighbor", mp4], check=True)
        print(mp4)


if __name__ == "__main__":
    main()
