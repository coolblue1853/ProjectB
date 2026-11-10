"""잎사귀 옷 프레임의 이마 높이 짙은 초록 띠(고글 자리에 생긴 두건 챙)를 모든 프레임에서 지운다. 띠 = 청록 계열 초록(hue .30~.45) + 바로 붙은 어두운 외곽선. 빈 곳은 이웃 얼굴·머리 색으로 채움."""
import colorsys, os, sys, glob
import numpy as np
from PIL import Image
SRC, DST = sys.argv[1], sys.argv[2]
os.makedirs(DST, exist_ok=True)
def hsv(p): return colorsys.rgb_to_hsv(*(np.asarray(p[:3], float) / 255.0))
tot = {}
for p in sorted(glob.glob(SRC + "/ds_*.png")):
    n = os.path.basename(p); a = np.array(Image.open(p).convert("RGBA")); H, W = a.shape[:2]
    al = a[:, :, 3] > 128
    band = np.zeros((H, W), bool)
    for y in range(0, 36):
        for x in range(W):
            if al[y, x]:
                h, s, v = hsv(a[y, x])
                if 0.30 < h < 0.385 and s > 0.28 and 0.25 < v < 0.5: band[y, x] = True
    if band.sum() == 0:
        Image.fromarray(a).save(os.path.join(DST, n)); continue
    # 띠에 붙은 어두운 외곽선(v<0.2)도 포함, 띠 행 범위 안에서만
    ys, xs = np.nonzero(band); y0, y1, x0, x1 = ys.min() - 1, ys.max() + 1, xs.min() - 1, xs.max() + 1
    m = band.copy()
    for _ in range(2):
        for y in range(max(0, y0), min(H, y1 + 1)):
            for x in range(max(0, x0), min(W, x1 + 1)):
                if al[y, x] and not m[y, x] and hsv(a[y, x])[2] < 0.2:
                    if m[max(0, y - 1):y + 2, max(0, x - 1):x + 2].any(): m[y, x] = True
    out = a.copy(); todo = m.copy()
    def ok(y, x):
        if not al[y, x] or todo[y, x]: return False
        h, s, v = hsv(out[y, x])
        return not (0.18 < h < 0.30 and s > 0.25) and v > 0.35   # 올리브 두건·어두운 외곽선은 채움 재료에서 제외
    for _ in range(8):
        nxt = todo.copy()
        for y, x in zip(*np.nonzero(todo)):
            cols = [tuple(out[yy, xx]) for yy in range(max(0, y - 1), min(H, y + 2)) for xx in range(max(0, x - 1), min(W, x + 2)) if ok(yy, xx)]
            if cols:
                out[y, x] = max(set(cols), key=cols.count); nxt[y, x] = False
        todo = nxt
    # 그래도 남은 것은 두건 올리브색으로
    for y, x in zip(*np.nonzero(todo)): out[y, x] = (96, 120, 62, 255)
    tot[n] = int(m.sum())
    Image.fromarray(out).save(os.path.join(DST, n))
print("띠 지운 프레임", len(tot), "픽셀", sum(tot.values()))
print(tot)
