import sys
from PIL import Image
import numpy as np
def x5(path,name):
    im=Image.open(path).convert('RGBA')
    bg=Image.new('RGBA',im.size,(255,255,255,255));bg.alpha_composite(im)
    bg.convert('RGB').resize((320,320),Image.NEAREST).save(f'check/{name}_x5.png')
    a=np.array(im)[:,:,3]>0
    ys,xs=np.where(a);iso=[]
    p=np.pad(a,1)
    for y,x in zip(ys,xs):
        if p[y:y+3,x:x+3].sum()==1: iso.append((x,y))
    print(name,'bbox x',xs.min(),xs.max(),'y',ys.min(),ys.max(),'isolated',iso)
x5(sys.argv[1],sys.argv[2])
