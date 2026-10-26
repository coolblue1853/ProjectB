"""몹 재채색(생성 0회): 원본 대표 프레임과 PixelLab 으로 색만 바꾼 같은 자세 그림을 픽셀끼리 짝지어 색 대응표를 만들고,
그 몹의 모든 프레임 PNG(8배 확대 도트)에 적용해 같은 이름으로 덮어쓴다 — 애니메이터·프리팹은 그대로 새 색을 쓴다.
원본은 Docs/ArtCompare/Mobs/<몹>/orig/ 에 보관. 대응표에 없는 색은 가장 가까운 원본 색의 대응을 쓴다.
사용: python Tools/pixelfx/mob_recolor.py <몹 폴더 이름> <대표 프레임 파일> <편집 그림(64칸)> <놓인 위치 x,y> [확대 배율=8]
예)  python Tools/pixelfx/mob_recolor.py Rabbit Rabbit1.png Docs/ArtCompare/Mobs/Rabbit/edit2.png 13,24
"""
import os
import shutil
import sys
from collections import Counter, defaultdict

import numpy as np
from PIL import Image


SKIP = ("VenomVesh", "FlowerAttck")  # 공격 이펙트 그림은 건드리지 않는다(사용자: 이펙트는 예시를 준 뒤에)


def main():
    mob, ref_name, edit_path, pos = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
    scale = int(sys.argv[5]) if len(sys.argv) > 5 else 8
    ox, oy = (int(v) for v in pos.split(","))
    src_dir = f"Assets/Sprite/Enemy/{mob}"
    keep = f"Docs/ArtCompare/Mobs/{mob}/orig"
    os.makedirs(keep, exist_ok=True)
    ref = np.array(Image.open(os.path.join(keep if os.path.exists(os.path.join(keep, ref_name)) else src_dir, ref_name)).convert("RGBA"))
    ref = ref[::scale, ::scale]
    edit = np.array(Image.open(edit_path).convert("RGBA"))[oy:oy + ref.shape[0], ox:ox + ref.shape[1]]
    votes = defaultdict(Counter)
    for y in range(ref.shape[0]):
        for x in range(ref.shape[1]):
            if ref[y, x, 3] > 0 and edit[y, x, 3] > 0:
                votes[tuple(ref[y, x, :3])][tuple(edit[y, x, :3])] += 1
    table = {k: v.most_common(1)[0][0] for k, v in votes.items()}
    keys = np.array(list(table.keys()), int)
    print(mob, "colors mapped", len(table))
    n = 0
    for f in sorted(os.listdir(src_dir)):
        if not f.endswith(".png") or any(f.startswith(x) for x in SKIP):
            continue
        orig_path = os.path.join(keep, f)
        if not os.path.exists(orig_path):
            shutil.copy(os.path.join(src_dir, f), orig_path)
        a = np.array(Image.open(orig_path).convert("RGBA"))
        out = a.copy()
        m = a[..., 3] > 0
        cols = {tuple(c) for c in a[m][:, :3]}
        for c in cols:
            if c in table:
                to = table[c]
            else:
                d = ((keys - np.array(c)) ** 2).sum(axis=1)
                near = tuple(keys[int(d.argmin())])
                to = table[near]
            sel = m & np.all(a[..., :3] == c, axis=2)
            out[sel, :3] = to
        Image.fromarray(out, "RGBA").save(os.path.join(src_dir, f))
        n += 1
    print("frames recolored", n)


if __name__ == "__main__":
    main()
