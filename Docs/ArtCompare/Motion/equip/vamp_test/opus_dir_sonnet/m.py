import numpy as np
from PIL import Image
def A(i): return np.array(Image.open(f'raw_{i}.png').convert('RGBA'))[:,:,3]>0
def xor(a,b): return (a^b).sum()/max((a|b).sum(),1)
def head(a):
    ys,xs=np.where(a);y0=ys.min();h=np.zeros_like(a);h[y0:y0+22]=a[y0:y0+22];return h
for i in range(9):
    a=A(i);ys,xs=np.where(a);p=np.pad(a,1)
    iso=[(x,y) for y,x in zip(ys,xs) if p[y:y+3,x:x+3].sum()==1]
    print(i,'bbox',xs.min(),xs.max(),ys.min(),ys.max(),'cnt',a.sum(),'iso',iso)
for i in range(8): print(i,i+1,'%.3f'%xor(A(i),A(i+1)),'head %.3f'%xor(head(A(i)),head(A(i+1))))
print('seam 7-0 %.3f  8-1 %.3f'%(xor(A(7),A(0)),xor(A(8),A(1))))
