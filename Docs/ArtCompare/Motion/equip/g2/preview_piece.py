import sys, numpy as np
sys.path.insert(0, "C:/Users/user/ProjectB/Tools/pixelfx")
import g2_export as G
from PIL import Image, ImageDraw
C = G.C
ORDER = [4, 5, 2, 3, 1]
def compose(under, layers):
    px = under.copy()
    for s in ORDER:
        lay = layers[s]
        er = lay.get(("erase", s))
        if er is not None: px[er] = 0
        L = lay[s]; m = L[:, :, 3] > 0; px[m] = L[m]
    return px
frames = G.FRAMES
base = {f: G.build("base_coat", f) for f in frames}
leaf = {f: G.build("leaf_set", f) for f in frames}
K = 4
panels = []
names = ["기본 코트", "잎사귀 전부", "잎사귀 옷+기본 모자·장갑·신발", "기본 옷+잎사귀 모자"]
order = [f for f in frames if f.startswith(("ds_idle", "ds_walk", "ds_run", "ds_crouch", "ds_arc", "ds_land", "ds_stab", "ds_down", "ds_up"))]
for f in order:
    u = G.under_of(f)
    pm = []
    pm.append(compose(u, {s: {s: base[f][s], ("erase", s): base[f].get(("erase", s))} for s in G.PART}))
    pm.append(compose(u, {s: {s: leaf[f][s], ("erase", s): leaf[f].get(("erase", s))} for s in G.PART}))
    mix = {s: {s: base[f][s], ("erase", s): base[f].get(("erase", s))} for s in G.PART}
    mix[2] = {2: leaf[f][2], ("erase", 2): leaf[f].get(("erase", 2))}
    pm.append(compose(u, mix))
    mix2 = {s: {s: base[f][s], ("erase", s): base[f].get(("erase", s))} for s in G.PART}
    mix2[1] = {1: leaf[f][1], ("erase", 1): leaf[f].get(("erase", 1))}
    pm.append(compose(u, mix2))
    sh = Image.new("RGBA", (64 * K * 4, 64 * K + 14), (120, 130, 140, 255))
    for c, im in enumerate(pm):
        sh.alpha_composite(Image.fromarray(im).resize((64 * K, 64 * K), Image.NEAREST), (c * 64 * K, 14))
    ImageDraw.Draw(sh).text((4, 1), f, fill=(255, 255, 255, 255))
    panels.append(sh)
sheet = Image.new("RGBA", (64 * K * 4, (64 * K + 14) * 0 + 1))
gif = [p.convert("P", palette=Image.ADAPTIVE) for p in panels]
out = "C:/Users/user/ProjectB/Docs/ArtCompare/Motion/equip/g2/preview_piece.gif"
gif[0].save(out, save_all=True, append_images=gif[1:], duration=130, loop=0)
# 정지 시트: 대표 12프레임
pick = ["ds_idle_0", "ds_walk_2", "ds_walk_5", "ds_run_1", "ds_run_5", "ds_crouch_0", "ds_arc_1", "ds_land_1", "ds_stab_1", "ds_down_2", "ds_up_3", "ds_up_4"]
S = Image.new("RGBA", (64 * K * 4 * 2, (64 * K + 14) * 6), (120, 130, 140, 255))
for i, f in enumerate(pick):
    S.alpha_composite(panels[order.index(f)], ((i % 2) * 64 * K * 4, (i // 2) * (64 * K + 14)))
S.save("C:/Users/user/ProjectB/Docs/ArtCompare/Motion/equip/g2/preview_piece.png")
print(len(panels))
