"""픽셀 리그 빈칸 자동 검출(PixelRigDesign 3.5절 '빈칸' 기준).

rig_holes_sheet.cs 가 찍은 자홍 배경 시트에서, 칸마다 테두리부터 배경을 채워 나가고 남는 자홍 픽셀 = 몸 안에 갇힌 구멍.
사용: python Tools/pixelfx/find_rig_holes.py [시트] [칸크기] [행이름 쉼표]
"""
import sys
from collections import deque

import numpy as np
from PIL import Image

sheet = sys.argv[1] if len(sys.argv) > 1 else "Logs/playtest/rig_holes.png"
cell = int(sys.argv[2]) if len(sys.argv) > 2 else 160
names = sys.argv[3].split(",") if len(sys.argv) > 3 else None
a = np.array(Image.open(sheet).convert("RGB")).astype(int)
bg = (np.abs(a[..., 0] - 255) < 40) & (a[..., 1] < 40) & (np.abs(a[..., 2] - 255) < 40)
rows, cols = a.shape[0] // cell, a.shape[1] // cell
total = 0
for r in range(rows):
    found = []
    for c in range(cols):
        m = bg[r * cell:(r + 1) * cell, c * cell:(c + 1) * cell].copy()
        seen = np.zeros_like(m)
        q = deque()
        h, w = m.shape
        for y in range(h):
            for x in (0, w - 1):
                if m[y, x] and not seen[y, x]:
                    seen[y, x] = True
                    q.append((y, x))
        for x in range(w):
            for y in (0, h - 1):
                if m[y, x] and not seen[y, x]:
                    seen[y, x] = True
                    q.append((y, x))
        while q:
            y, x = q.popleft()
            for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                ny, nx = y + dy, x + dx
                if 0 <= ny < h and 0 <= nx < w and m[ny, nx] and not seen[ny, nx]:
                    seen[ny, nx] = True
                    q.append((ny, nx))
        holes = int((m & ~seen).sum())
        if holes:
            found.append(f"t{c}:{holes}")
            total += holes
    name = names[r] if names and r < len(names) else str(r)
    print(f"{name:12s} " + (" ".join(found) if found else "OK"))
print("total hole px", total)
