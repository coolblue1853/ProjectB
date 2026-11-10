"""첫 프레임 검은 외곽선 완화(11-10): 바깥과 닿은 어두운 픽셀을 이웃 옷 색 x0.62 로. 사용: python soften.py in.png out.png"""
import sys, numpy as np
from PIL import Image
im=np.array(Image.open(sys.argv[1]).convert("RGBA")); H,W=im.shape[:2]; a=im[:,:,3]>128
out=im.copy()
for y in range(H):
    for x in range(W):
        if not a[y,x]: continue
        if im[y,x,:3].max()>=60: continue
        nb=[(yy,xx) for yy in range(max(0,y-1),min(H,y+2)) for xx in range(max(0,x-1),min(W,x+2))]
        if all(a[yy,xx] for yy,xx in nb): continue
        c=[im[yy,xx,:3].astype(float) for yy,xx in nb if a[yy,xx] and im[yy,xx,:3].max()>=60]
        if c: out[y,x,:3]=(np.mean(c,0)*0.62).astype(np.uint8)
Image.fromarray(out).save(sys.argv[2])
