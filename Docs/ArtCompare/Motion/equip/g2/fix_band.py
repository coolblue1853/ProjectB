"""잎사귀 옷 프레임의 이마 높이 청록 초록 띠(고글 자리에 생긴 챙)를 모든 프레임에서 두건 올리브 무늬로 바꾼다.
띠 픽셀(hue .30~.385, v .25~.5)만 칠하고 어두운 외곽선·얼굴은 건드리지 않는다. 색은 같은 열 위쪽의 가장 가까운 두건 픽셀에서 가져온다(체크 무늬 유지)."""
import colorsys, os, sys, glob
import numpy as np
from PIL import Image
SRC, DST = sys.argv[1], sys.argv[2]
os.makedirs(DST, exist_ok=True)
def hsv(p): return colorsys.rgb_to_hsv(*(np.asarray(p[:3], float) / 255.0))
def olive(p):
    h, s, v = hsv(p); return 0.18 < h < 0.30 and s > 0.25 and v > 0.3
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
    out = a.copy()
    if band.any():
        # 띠 열별 맨 위 y: 그 위의 어두운 선은 두건 가장자리(유지), 띠 안·아래 어두운 선은 챙 외곽선(제거)
        M = band.copy()
        for x in range(W):
            ys = np.nonzero(band[:, x])[0]
            if len(ys) == 0: continue
            yt, yb = ys.min(), ys.max()
            for y in range(yt, min(H, yb + 2)):
                if al[y, x] and not band[y, x] and hsv(a[y, x])[2] < 0.2: M[y, x] = True
        def face(p):
            h, s, v = hsv(p); return v > 0.4 and not (0.18 < h < 0.40 and s > 0.25)
        for x in range(W):
            ys = np.nonzero(M[:, x])[0]
            if len(ys) == 0: continue
            yb = ys.max(); col = None
            for yy in range(yb + 1, min(H, yb + 8)):
                if al[yy, x] and not M[yy, x] and face(a[yy, x]): col = a[yy, x]; break
            if col is None:
                for dx in (-1, 1, -2, 2):
                    xx = x + dx
                    if 0 <= xx < W:
                        for yy in range(yb + 1, min(H, yb + 8)):
                            if al[yy, xx] and not M[yy, xx] and face(a[yy, xx]): col = a[yy, xx]; break
                    if col is not None: break
            for y in ys:
                out[y, x] = col if col is not None else a[y, x]
        band = M
    if band.any(): tot[n] = int(band.sum())
    Image.fromarray(out).save(os.path.join(DST, n))
print("띠 지운 프레임", len(tot), "픽셀", sum(tot.values()))
