"""한 조각 장비 검사 시트(11-10): 게임과 같은 합성 순서(legs→top→shoes→glove→cap, 빈 칸은 base_coat)로 오프라인 합성.
 1) 코트(장비 없음) 2) 잎사귀 한 칸씩·전체 3) 코트 vs 잎사귀. 사용: python Tools/pixelfx/g2_sheet.py <출력폴더>"""
import numpy as np, json, os, sys
from PIL import Image, ImageDraw
R="Assets/Art/Player/PixelRig/Equip/"; CEL="Assets/Art/Player/PixelRig/CelDS/"
cel=json.load(open(CEL+"cel.json")); out=sys.argv[1]; os.makedirs(out,exist_ok=True)
ORDER=[("legs","legs"),("top","top"),("shoes","shoes"),("glove","glove"),("cap","cap")]
def comp(clip,i,pick):  # pick: part -> set_id
    c=np.array(Image.open(CEL+cel[clip]["frames"][i]["png"]+".png").convert("RGBA"))
    for part,_ in ORDER:
        d=f"{R}{pick[part]}_{part}/{clip}_{i}"
        if not os.path.exists(d+".png"): continue
        L=np.array(Image.open(d+".png").convert("RGBA")); E=np.array(Image.open(d+"_erase.png"))
        c[E>127]=0; m=L[:,:,3]>0; c[m]=L[m]
    return Image.fromarray(c)
def strip(frames,label,h=200):
    w=Image.new("RGBA",(h*len(frames),h+14),(70,80,90,255)); ImageDraw.Draw(w).text((2,0),label,fill=(255,255,0,255))
    for k,f in enumerate(frames):
        t=f.resize((h,h),0); w.paste(t,(k*h,14),t)
    return w
def chunks(frames,label,n=10):
    return [strip(frames[i:i+n],label+(f" ({i//n+1})" if len(frames)>n else "")) for i in range(0,len(frames),n)]
def stack(rows,name):
    W=max(r.width for r in rows); o=Image.new("RGBA",(W,sum(r.height for r in rows)),(70,80,90,255)); y=0
    for r in rows: o.paste(r,(0,y)); y+=r.height
    o.save(f"{out}/{name}")
COAT={p:"base_coat" for p,_ in ORDER}; LEAF={p:"leaf_set" for p,_ in ORDER}
ACT={"Idle":[0,2,4,6],"Walk":[0,2,4,6],"Run":[0,2,4,6],"JumpCrouch":[0],"JumpArc":[0,1,2],"Land":[0,1,2],"Stab":[1,3],"SwingDown":[1,3],"SwingUp":[1,3]}
fr=lambda pick:[comp(c,i,pick) for c,ids in ACT.items() if c in cel for i in ids if i<len(cel[c]["frames"])]
stack(chunks(fr(COAT),"1) 코트(장비 없음)"),"1_coat.png")
rows=[]
for part in ("cap","top","glove","shoes"):
    pk=dict(COAT); pk[part]="leaf_set"; rows+=chunks(fr(pk),"2) 잎사귀 "+part+" 하나만")
rows+=chunks(fr(LEAF),"2) 잎사귀 전체")
stack(rows,"2_pieces.png")
stack(chunks(fr(COAT),"3) 코트")+chunks(fr(LEAF),"3) 잎사귀"),"3_compare.png")
print("ok",len(fr(COAT)))
