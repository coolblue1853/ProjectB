"""비교: A 몸 위에 쌓기(현재 방식) vs B 몸+옷 한 조각(머리·손·발 제외). 새 생성 없음. 사용: python compare_stack_vs_piece.py"""
import os, sys
import numpy as np
from PIL import Image
ROOT = "C:/Users/user/ProjectB"
CODE = ROOT + "/Docs/Research/LabReply_gen_exp1/result/code"
sys.path.insert(0, CODE)
import common as C
import export_runtime as E
REG = ROOT + "/Docs/ArtCompare/Motion/equip/regions"
LEAF = ROOT + "/Docs/ArtCompare/Motion/equip/g2/leaf_new"
UND = ROOT + "/Assets/Art/Player/PixelRig/EquipDS/under"
OUT = ROOT + "/Docs/ArtCompare/Motion/equip/g2"
frames = C.FRAMES
maps = C.load_maps(REG)
def over(dst, src):
    m = src[:, :, 3] > 0
    dst[m] = src[m]

def head_rows(img):
    import colorsys
    a = img[:, :, 3] > 128
    ys, xs = np.nonzero(a)
    pts = []
    for y, x in zip(ys, xs):
        h, s, v = colorsys.rgb_to_hsv(*(img[y, x, :3] / 255.0))
        if 0.44 < h < 0.55 and s > 0.55 and v > 0.45: pts.append(y)
    gy = np.mean(pts) if pts else ys.min() + 8
    return int(round(gy)) + 9

res = {}
for f in frames:
    leaf = C.load(f"{LEAF}/{f}.png"); under = C.load(f"{UND}/{f}.png"); lab = maps[f].copy()
    lab[np.isin(lab, [C.LEGS, C.HEM])] = C.TOP
    em = C.ext_map(lab, leaf)
    em[em == C.BACK] = 0
    a = C.alpha(leaf)
    # 조각(레이어) 만들기
    lay = {}
    for s in C.SLOTS:
        L = np.zeros_like(leaf); m = a & (em == s); L[m] = leaf[m]; lay[s] = L
    add, erase = C.cap_extend(lab, leaf, under, "leaf_set")
    lay[C.CAP][add] = leaf[add]
    # A: 속옷 몸 위에 쌓기
    A = under.copy()
    for s in [4, 5, 2, 3]: over(A, lay[s])
    A[erase] = 0
    over(A, lay[C.CAP])
    # B: 머리(속옷 몸의 머리) + 한 조각(몸+옷 = 잎사귀 그림에서 머리 아래 전부) + 손·발 = 같은 그림
    hb = head_rows(under)
    B = np.zeros_like(under)
    below = np.zeros(a.shape, bool); below[hb + 1:, :] = True
    body_piece = a & below
    B[body_piece] = leaf[body_piece]
    headpx = (under[:, :, 3] > 128) & ~below
    B[headpx] = under[headpx]
    B[erase] = 0
    over(B, lay[C.CAP])
    res[f] = (under, A, B, leaf)
    sil = lambda x: x[:, :, 3] > 128
    #print(f, "A 알파", int(sil(A).sum()), "B 알파", int(sil(B).sum()), "A-B 다른 픽셀", int((np.abs(A.astype(int) - B.astype(int)).sum(2) > 30).sum()))


from PIL import ImageDraw
K=5
order=C.FRAMES
panels=[]
for f in order:
    under,A,B,leaf=res[f]
    sh=Image.new("RGBA",(64*K*3,64*K+16),(120,130,140,255))
    for c,im in enumerate((leaf,A,B)):
        sh.alpha_composite(Image.fromarray(im).resize((64*K,64*K),Image.NEAREST),(c*64*K,16))
    d=ImageDraw.Draw(sh)
    for c,t in enumerate(("original","A stack","B piece")): d.text((c*64*K+4,2),t,fill=(255,255,255,255))
    panels.append(sh.convert("P",palette=Image.ADAPTIVE))
out=OUT+"/compare_stack_vs_piece.gif"
panels[0].save(out,save_all=True,append_images=panels[1:],duration=160,loop=0)
print(os.path.getsize(out),len(panels))
