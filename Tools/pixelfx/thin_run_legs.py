"""달리기 다리 두꺼움 정리 v2(11-07, PixelLab 없음). 바지색 덩어리가 MAXW 보다 넓은 줄에서 **가운데** 칸을 빼고 오른쪽을 왼쪽으로 붙여 좁힌다(양쪽 외곽선 유지 → 다리가 끊기지 않는다).
v1(가장자리 깎기)은 다리가 조각나 폐기. 줄마다 빼는 칸 수가 위아래로 1 이상 튀지 않게 부드럽게 한다. 연결 덩어리 수가 늘면 그 프레임은 원본 유지.
입력 ds01/run_R1 → 출력 ds01/run_R1_thin."""
import glob, os
import numpy as np
from PIL import Image
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "Docs/ArtCompare/CharacterGen/ds01/run_R1")
DST = SRC + "_thin"
MAXW = 4
Y0, Y1 = 40, 56


def dark(p):
    return p[3] > 0 and p[0] < 60 and p[1] < 60 and p[2] < 60


def comps(a):
    m = a[..., 3] > 0
    H, W = m.shape
    seen = np.zeros_like(m)
    n = 0
    for y in range(H):
        for x in range(W):
            if m[y, x] and not seen[y, x]:
                n += 1
                st = [(x, y)]
                seen[y, x] = 1
                while st:
                    cx, cy = st.pop()
                    for dx in (-1, 0, 1):
                        for dy in (-1, 0, 1):
                            nx, ny = cx + dx, cy + dy
                            if 0 <= nx < W and 0 <= ny < H and m[ny, nx] and not seen[ny, nx]:
                                seen[ny, nx] = 1
                                st.append((nx, ny))
    return n


def coat(p):
    return p[3] > 0 and p[2] > p[0] + 15


def thin(im):
    """v3: 오른쪽(바깥) 가장자리는 그대로 두고 **왼쪽(안쪽)** 을 깎는다 → 부츠로 이어지는 오른쪽 선이 안 끊긴다. 깎은 자리 왼쪽이 코트면 코트색으로, 투명이면 투명으로."""
    px = im.load()
    W, H = im.size
    from collections import Counter
    cc = Counter(px[x, y] for y in range(36, 52) for x in range(W) if coat(px[x, y]))
    base = cc.most_common(1)[0][0] if cc else (0, 0, 0, 0)  # 코트 기본색(하이라이트 회색이 섞이지 않게)
    for y in range(Y0, Y1):
        runs = []
        x = 0
        while x < W:
            if dark(px[x, y]):
                s = x
                while x < W and dark(px[x, y]):
                    x += 1
                runs.append((s, x))
            else:
                x += 1
        if not runs:
            continue
        s, e = max(runs, key=lambda r: r[1] - r[0])
        if e - s <= MAXW:
            continue
        left = px[s - 1, y] if s > 0 else (0, 0, 0, 0)
        fill = base if coat(left) else (0, 0, 0, 0)
        for x in range(s, e - MAXW):
            px[x, y] = fill
    return im


os.makedirs(DST, exist_ok=True)
for f in sorted(glob.glob(SRC + "/raw_*.png")):
    org = Image.open(f).convert("RGBA")
    out = thin(org.copy())
    c0, c1 = comps(np.array(org)), comps(np.array(out))
    if c1 > c0:
        print("조각남 → 원본 유지", os.path.basename(f), c0, c1)
        out = org
    out.save(os.path.join(DST, os.path.basename(f)))
print("ok")
