"""이음새 기준점 시험(11-10, 오프라인): 섞어 입을 때 모자·장갑·신발을 몸통(옷) 세트의 자기 위치로 평행이동. 사용: python Tools/pixelfx/g2_anchor_sheet.py <출력폴더>"""
import numpy as np, json, os, sys
from PIL import Image, ImageDraw
R="Assets/Art/Player/PixelRig/Equip/"; CEL="Assets/Art/Player/PixelRig/CelDS/"
cel=json.load(open(CEL+"cel.json")); out=sys.argv[1]; os.makedirs(out,exist_ok=True)
def ld(p): return np.array(Image.open(p).convert("RGBA"))
def lay(set_id,part,clip,i):
    d=f"{R}{set_id}_{part}/{clip}_{i}"
    return ld(d+".png"), np.array(Image.open(d+"_erase.png"))
def shift(a,dy,dx):
    o=np.zeros_like(a); H,W=a.shape[:2]
    ys=slice(max(0,dy),min(H,H+dy)); yd=slice(max(0,-dy),min(H,H-dy)); xs=slice(max(0,dx),min(W,W+dx)); xd=slice(max(0,-dx),min(W,W-dx))
    o[ys,xs]=a[yd,xd]; return o
def comps(m):
    H,W=m.shape; seen=np.zeros(m.shape,bool); c=[]
    for y in range(H):
        for x in range(W):
            if m[y,x] and not seen[y,x]:
                st=[(y,x)]; seen[y,x]=True; pts=[]
                while st:
                    cy,cx=st.pop(); pts.append((cy,cx))
                    for dy in(-1,0,1):
                        for dx in(-1,0,1):
                            yy,xx=cy+dy,cx+dx
                            if 0<=yy<H and 0<=xx<W and m[yy,xx] and not seen[yy,xx]: seen[yy,xx]=True; st.append((yy,xx))
                if len(pts)>=4: c.append(((np.mean([p[0] for p in pts]),np.mean([p[1] for p in pts])),len(pts)))
    return sorted(c,key=lambda t:t[0][1])
def anchors(set_id,part,clip,i):
    L,_=lay(set_id,part,clip,i); c=comps(L[:,:,3]>0)
    return [t[0] for t in c]
def delta(part,S,T,clip,i):
    """S 세트의 부위를 T 세트 몸통 위치로 옮기는 (dy,dx)"""
    if part=="cap":
        # 머리 위치 차이: 세트별 머리 맨 윗줄·가로 평균(머리 구역 rows<24)
        def head(set_id):
            L,_=lay(set_id,"cap",clip,i); top,_=lay(set_id,"top",clip,i); m=(L[:,:,3]>0)|(top[:,:,3]>0); m[24:]=False
            ys,xs=np.nonzero(m); return (ys.min() if len(ys) else 0, xs.mean() if len(xs) else 0)
        a,b=head(S),head(T); 
        return int(round(b[0]-a[0])),int(round(b[1]-a[1]))
    aS,aT=anchors(S,part,clip,i),anchors(T,part,clip,i)
    if not aS or not aT: return 0,0
    if len(aS)==len(aT): d=np.mean([(t[0]-s[0],t[1]-s[1]) for s,t in zip(aS,aT)],0)
    else: d=np.array(np.mean(aT,0))-np.array(np.mean(aS,0))
    return int(round(d[0])),int(round(d[1]))
def comp(clip,i,top,pieces,anchor):
    """top: 몸통 세트, pieces: {cap,glove,shoes: 세트}"""
    base=ld(CEL+cel[clip]["frames"][i]["png"]+".png")
    dh=delta("cap","base_coat",top,clip,i) if (anchor and top!="base_coat") else (0,0)   # 머리 위치(바탕 머리는 코트 포즈)
    c=shift(base,*dh) if anchor else base.copy()
    order=[("legs",top),("top",top),("shoes",pieces["shoes"]),("glove",pieces["glove"]),("cap",pieces["cap"])]
    for part,S in order:
        L,E=lay(S,part,clip,i)
        if anchor and part in("cap","glove","shoes") and S!=top:
            dy,dx=delta(part,S,top,clip,i); L=shift(L,dy,dx); E=shift(E,dy,dx)
        c[E>127]=0; m=L[:,:,3]>0; c[m]=L[m]
    return Image.fromarray(c)
FR=[("Walk",1),("Walk",3),("Walk",6),("Run",0),("Run",2),("Run",4),("Run",6),("JumpArc",0),("JumpArc",1),("Land",1)]
def strip(frames,label,h=190):
    w=Image.new("RGBA",(h*len(frames),h+14),(70,80,90,255)); ImageDraw.Draw(w).text((2,0),label,fill=(255,255,0,255))
    for k,f in enumerate(frames): t=f.resize((h,h),0); w.paste(t,(k*h,14),t)
    return w
rows=[]
for name,top,pc in (("leaf-top + coat pieces","leaf_set","base_coat"),("coat-top + leaf pieces","base_coat","leaf_set")):
    pieces={"cap":pc,"glove":pc,"shoes":pc}
    for anchor in (False,True):
        rows.append(strip([comp(c,i,top,pieces,anchor) for c,i in FR],f"{name} / "+("anchor ON" if anchor else "anchor OFF")))
W=max(r.width for r in rows); o=Image.new("RGBA",(W,sum(r.height for r in rows)),(70,80,90,255)); y=0
for r in rows: o.paste(r,(0,y)); y+=r.height
o.save(out+"/anchor_test.png"); print("ok")
