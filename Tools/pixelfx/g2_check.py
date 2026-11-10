"""한 조각 내보내기 검사(11-10): 원본 잎사귀 그림 vs 게임 합성(바탕+층). miss=잘림(원본에 있는데 합성에 없음), col=색 다름, extra=원본에 없는 잔여(속옷 등). 사용: python Tools/pixelfx/g2_check.py"""
import numpy as np, json, os
from PIL import Image
R="Assets/Art/Player/PixelRig/Equip/"; G="Docs/ArtCompare/Motion/equip/g2/leaf3/"
cel=json.load(open("Assets/Art/Player/PixelRig/CelDS/cel.json")); seen={}
for clip,c in cel.items():
    for i,fr in enumerate(c["frames"]): seen.setdefault(fr["png"],(clip,i))
tm=te=0
for png,(clip,i) in seen.items():
    if not os.path.exists(G+png+".png"): continue
    src=np.array(Image.open(G+png+".png").convert("RGBA")); c=np.array(Image.open(f"Assets/Art/Player/PixelRig/CelDS/{png}.png").convert("RGBA"))
    for part in ("legs","top","shoes","glove","cap"):
        L=np.array(Image.open(f"{R}leaf_set_{part}/{clip}_{i}.png").convert("RGBA"));E=np.array(Image.open(f"{R}leaf_set_{part}/{clip}_{i}_erase.png"))
        c[E>127]=0;m=L[:,:,3]>0;c[m]=L[m]
    sa=src[:,:,3]>128; ca=c[:,:,3]>128
    miss=int((sa&~ca).sum()); extra=int((ca&~sa).sum()); tm+=miss; te+=extra
    print(png,"miss",miss,"extra",extra)
print("TOTAL miss",tm,"extra",te)
