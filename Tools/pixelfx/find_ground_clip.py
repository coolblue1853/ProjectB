"""무기 칼끝 땅 뚫림 자동 검사(PixelRigDesign 3.5절).

weapon_ground_sheet.cs 의 자홍 배경 시트(열 = 무기, 행 = 대기 / 내려베기)에서 칸마다 가장 아래 불투명 줄을 찾아,
무기 없는 발바닥 줄(가장 많은 칸의 최저선 = 발)보다 아래로 내려간 칸을 보고한다.
사용: python Tools/pixelfx/find_ground_clip.py [시트] [칸] [무기 이름 쉼표]
"""
import sys
from collections import Counter

import numpy as np
from PIL import Image

sheet = sys.argv[1] if len(sys.argv) > 1 else "Logs/playtest/weapon_ground.png"
cell = int(sys.argv[2]) if len(sys.argv) > 2 else 150
names = sys.argv[3].split(",") if len(sys.argv) > 3 else None
a = np.array(Image.open(sheet).convert("RGB")).astype(int)
bg = (np.abs(a[..., 0] - 255) < 40) & (a[..., 1] < 40) & (np.abs(a[..., 2] - 255) < 40)
rows, cols = a.shape[0] // cell, a.shape[1] // cell
low = np.zeros((rows, cols), int)
for r in range(rows):
    for c in range(cols):
        m = ~bg[r * cell:(r + 1) * cell, c * cell:(c + 1) * cell]
        ys = np.where(m.any(1))[0]
        low[r, c] = ys.max() if len(ys) else -1
feet = Counter(low.flatten().tolist()).most_common(1)[0][0]
print("feet line", feet)
bad = 0
for c in range(cols):
    for r, tag in ((0, "idle"), (1, "swing")):
        if low[r, c] > feet + 1:
            n = names[c] if names and c < len(names) else str(c)
            print(f"{n:22s} {tag:5s} +{low[r, c] - feet}px below feet")
            bad += 1
print("clipping cells", bad)
