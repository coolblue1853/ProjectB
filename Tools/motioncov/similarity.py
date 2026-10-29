"""동작 간 구별도 측정(10-29 사용자 "비슷해 보이는 동작이 많다"): 셀 실루엣 IoU 거리 + 칼 각도 차 + 손 위치 차, 순서 무시 양방향 최소 평균. 사용: python Tools/motioncov/similarity.py"""
import json, os, sys, io, itertools
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
import numpy as np
from PIL import Image
CEL='Assets/Art/Player/PixelRig/Cel'
cj=json.load(open(f'{CEL}/cel.json'))
skip={'Idle','Walk','Run','JumpCrouch','JumpArc','Land','RopeUp','RopeStay','StabEnd','SwingDownEnd','Hurt','Death','Dash'}
def masks(n):
    out=[]
    for f in cj[n]['frames']:
        a=np.array(Image.open(f"{CEL}/{f['png']}.png").convert('RGBA'))[...,3]>0
        out.append((a, f.get('angle',0), f['hand']))
    return out
M={n:masks(n) for n in cj if n not in skip}
def d(A,B):
    # 몸 실루엣(IoU 거리) + 칼 각도 차 + 손 위치 차를 합친 프레임 거리, 순서 무시 양방향 최소 평균
    def fd(x,y):
        iou=(x[0]&y[0]).sum()/max(1,(x[0]|y[0]).sum())
        da=abs((x[1]-y[1]+180)%360-180)/180
        dh=np.hypot(x[2][0]-y[2][0],x[2][1]-y[2][1])/30
        return (1-iou)+0.5*da+0.5*dh
    ab=np.mean([min(fd(a,b) for b in B) for a in A]); ba=np.mean([min(fd(b,a) for a in A) for b in B])
    return (ab+ba)/2
names=sorted(M)
pairs=sorted(((d(M[a],M[b]),a,b) for a,b in itertools.combinations(names,2)))
print(len(names),'clips')
for v,a,b in pairs[:25]: print(f'{v:.3f} {a} ~ {b}')
print('...most distinct:'); 
for v,a,b in pairs[-5:]: print(f'{v:.3f} {a} ~ {b}')
print('--- 동작별 가장 비슷한 동작')
for n in names:
    v,o=min((d(M[n],M[m]),m) for m in names if m!=n)
    print(f'{n:13s} {v:.3f} {o}')
