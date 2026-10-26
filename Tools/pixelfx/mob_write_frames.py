"""새로 그린 몹 셀(64칸) → 기존 몹 프레임 PNG(8배 확대, 원래 캔버스) 덮어쓰기 — 애니메이터·프리팹은 그대로 새 그림을 쓴다.
새 프레임을 원래 그림의 원본 캔버스(예 토끼 38×32)에 놓는다: 기준 그림을 64칸에 놓았던 위치(x,y)만큼 잘라 낸 뒤 8배 확대.
프레임 수가 다르면 균등하게 골라 채운다. 원본은 Docs/ArtCompare/Mobs/<몹>/orig 에 이미 보관돼 있어야 한다(mob_recolor 또는 첫 실행 때 복사).
사용: python Tools/pixelfx/mob_write_frames.py <몹> <x,y> <새 프레임 폴더> <대상 파일1,대상 파일2,...> [시작 번호 끝 번호]
예)  python Tools/pixelfx/mob_write_frames.py Rabbit 13,24 Docs/ArtCompare/Mobs/Rabbit/idle Rabbit1.png,Rabbit2.png,Rabbit3.png,Rabbit4.png,Rabbit5.png 0 3
"""
import os
import shutil
import sys

from PIL import Image

SCALE = 8


def main():
    mob, pos, src, targets = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4].split(",")
    a = int(sys.argv[5]) if len(sys.argv) > 6 else 0
    b = int(sys.argv[6]) if len(sys.argv) > 6 else None
    ox, oy = (int(v) for v in pos.split(","))
    d = f"Assets/Sprite/Enemy/{mob}"
    keep = f"Docs/ArtCompare/Mobs/{mob}/orig"
    os.makedirs(keep, exist_ok=True)
    fs = sorted([f for f in os.listdir(src) if f.endswith(".png") and f[:-4].isdigit()], key=lambda x: int(x[:-4]))
    if b is not None:
        fs = [f for f in fs if a <= int(f[:-4]) <= b]
    for i, t in enumerate(targets):
        if not os.path.exists(os.path.join(keep, t)):
            shutil.copy(os.path.join(d, t), os.path.join(keep, t))
        orig = Image.open(os.path.join(keep, t))
        w, h = orig.size[0] // SCALE, orig.size[1] // SCALE
        f = fs[min(len(fs) - 1, i * len(fs) // len(targets))]
        im = Image.open(os.path.join(src, f)).convert("RGBA")
        crop = Image.new("RGBA", (w, h))
        crop.alpha_composite(im, (-ox, -oy))
        crop.resize((w * SCALE, h * SCALE), Image.NEAREST).save(os.path.join(d, t))
        print(t, "<-", f)


if __name__ == "__main__":
    main()
