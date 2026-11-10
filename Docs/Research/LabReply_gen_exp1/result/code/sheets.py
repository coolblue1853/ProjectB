"""확인 시트.
  python sheets.py regions <패키지> <지도폴더> <출력.png>                         # 원본 | 부위색 (21장)
  python sheets.py combos  <패키지> <지도폴더> <labels> <출력.png> [fill=m3] [hem=2]  # 5칸 조합 시트(구멍 빨강·빈칸 분홍·침범 노랑)
  python sheets.py combos  <패키지> <지도폴더> <labels> <출력.png> armor=1 [under=<속옷 몸 폴더>] [cap=1] [title=..]
      # 4칸(머리·갑옷·장갑·신발) 조합 시트. under=보정 속옷 몸, cap=1 세트별 모자 확장+지울 마스크, 보완 층은 4칸용(study4_slots.fills)
"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C

PROBE = ["ds_idle_0", "ds_stab_1", "ds_stab_3", "ds_down_2", "ds_up_2", "ds_up_4"]
# 같은 조합을 기준선·새 방법 시트에 똑같이 쓴다: (이름, 모자, 상의, 장갑, 바지, 신발)  c=코트 l=잎사귀
COMBOS = [("all coat", "ccccc"), ("all leaf", "lllll"), ("top=leaf", "clccc"), ("legs=leaf", "cccl c".replace(" ", "")),
          ("top=coat", "lclll"), ("legs=coat", "lllcl"), ("cap+glove=leaf", "lclcc"), ("top+shoes=leaf", "clccl")]
# 4칸 조합(모자, 갑옷, 장갑, 신발) — 바지 칸은 갑옷을 따름
COMBOS4 = [("all coat", "cccc"), ("all leaf", "llll"), ("armor=leaf", "clcc"), ("armor=coat", "lcll"),
           ("cap=leaf", "lccc"), ("cap+glove=leaf", "lclc"), ("shoes=leaf", "cccl"), ("armor+shoes=leaf", "clcl")]
SC = 4


def bg(img):
    b = Image.new("RGBA", (64, 64), (70, 70, 70, 255))
    b.alpha_composite(Image.fromarray(img))
    return np.array(b)


def regions_sheet(pkg, mapdir, out):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    maps = C.load_maps(mapdir)
    cols = 3
    tw = 64 * 2 + 4
    sheet = Image.new("RGB", (cols * tw, 7 * 76), (20, 20, 20))
    d = ImageDraw.Draw(sheet)
    for i, f in enumerate(C.FRAMES):
        x, y = (i % cols) * tw, (i // cols) * 76
        sheet.paste(Image.fromarray(bg(coat[f])[:, :, :3]), (x, y + 12))
        sheet.paste(Image.fromarray(C.colorize(maps[f])), (x + 66, y + 12))
        d.text((x + 2, y), f, fill=(255, 255, 255))
    sheet.resize((sheet.width * SC, sheet.height * SC), Image.NEAREST).save(out)


def combos_sheet(pkg, mapdir, labdir, out, fills=None, hem_to=None, title="", armor=False, under_dir=None, cap=False):
    coat = C.load_dir(os.path.join(pkg, "frames_coat_original"))
    under = C.load_dir(under_dir) if under_dir else C.load_dir(os.path.join(pkg, "frames_underwear_base"))
    leaf = C.load_dir(os.path.join(pkg, "frames_leaf_set_transfer"))
    maps = C.load_maps(mapdir)
    gt = C.load_maps(labdir)
    sets = ["base_coat", "leaf_set"]
    W = 66
    combos = COMBOS4 if armor else COMBOS
    if armor:
        import study4_slots as S4
        fills = lambda f, lab, imgs, lay: S4.fills(f, lab, imgs, lay, under[f], True)
        hem_to = C.TOP
    sheet = Image.new("RGB", (90 + W * len(PROBE) * 2, 14 + 66 * len(combos)), (20, 20, 20))
    d = ImageDraw.Draw(sheet)
    d.text((2, 1), title + "  (each frame: composite | defects: red=hole (dark red=backpack removed, intended), pink=exposed under-body, yellow=intrusion vs GT, cyan=amodal fill)", fill=(255, 255, 255))
    totals = [0, 0, 0]
    for r, (name, code) in enumerate(combos):
        if armor:
            code = code[0] + code[1] + code[2] + code[1] + code[3]  # 모자, 갑옷, 장갑, 바지(=갑옷), 신발
        wear = {s: ("base_coat" if ch == "c" else "leaf_set") for s, ch in zip(C.SLOTS, code)}
        d.text((2, 14 + r * 66 + 24), name, fill=(255, 255, 255))
        for c, f in enumerate(PROBE):
            imgs = {"base_coat": coat[f], "leaf_set": leaf[f]}
            lay = {s: C.make_layers(maps[f], imgs[s], s, hem_to, under[f] if cap else None, armor=armor) for s in sets}
            fl = fills(f, maps[f], imgs, lay) if fills else None
            img, src = C.compose(under[f], wear, imgs, lay, fl)
            er = lay[wear[C.CAP]].get("erase_cap", np.zeros((64, 64), bool))
            gap = (src == 0) & C.alpha(coat[f]) & C.alpha(leaf[f]) & ~er
            expo = C.exposure_mask(src, under[f], coat[f], leaf[f])
            mark = bg(img).copy()
            mark[:, :, :3] = (mark[:, :, :3] * 0.35).astype(np.uint8)
            mark[expo] = (255, 120, 200, 255)
            mark[(src >= 100)] = (60, 200, 255, 255)  # 보완 층이 채운 곳(파랑)
            mark[gap & (maps[f] == C.BACK)] = (110, 0, 0, 255)  # 배낭 제거 자리(의도된 투명)
            mark[gap & (maps[f] != C.BACK)] = (255, 0, 0, 255)
            totals[0] += int((gap & (maps[f] != C.BACK)).sum()); totals[1] += int(expo.sum())
            if f in gt:
                glay = {s: C.make_layers(gt[f], imgs[s], s, hem_to, under[f] if cap else None, armor=armor) for s in sets}
                ideal, isrc = C.compose(under[f], wear, imgs, glay)
                diff = (C.coldiff(img, ideal) > 30) | (C.alpha(img) != C.alpha(ideal))
                intr = (src >= 10) & (src < 100) & (src != isrc) & diff
                mark[intr] = (255, 230, 0, 255)
                totals[2] += int(intr.sum())
            x = 90 + c * W * 2
            sheet.paste(Image.fromarray(bg(img)[:, :, :3]), (x, 14 + r * 66))
            sheet.paste(Image.fromarray(mark[:, :, :3]), (x + W - 2, 14 + r * 66))
    d.text((2, sheet.height - 12), "sheet totals (8 combos x 6 frames): holes(excl. backpack, cap erase) %d  exposed under-body %d  intrusion vs GT %d" % tuple(totals), fill=(255, 255, 0))
    sheet.resize((sheet.width * SC, sheet.height * SC), Image.NEAREST).save(out)
    return totals


if __name__ == "__main__":
    cmd = sys.argv[1]
    if cmd == "regions":
        regions_sheet(*sys.argv[2:5])
    else:
        kw = dict(a.split("=", 1) for a in sys.argv[6:])
        fills = None
        if kw.get("fill") == "m3":
            import m3_amodal
            m3_amodal.set_pkg(sys.argv[2])
            fills = m3_amodal.make_fills
        print(combos_sheet(*sys.argv[2:6], fills=fills, hem_to=int(kw["hem"]) if "hem" in kw else None, title=kw.get("title", ""),
                           armor=kw.get("armor") == "1", under_dir=kw.get("under"), cap=kw.get("cap") == "1"))
