"""속옷 기본 몸 만들기(G1-6): 코트를 벗긴 transfer 그림(under/t1·t2)에 원본 머리(모자 부위+머리 맨몸)를 덮어 얼굴·머리카락·고글을 그대로 유지하고
EquipDS/under/ 에 쓴다. 비율 검사(바운딩 박스·머리 위치) 출력. 사용: python Tools/pixelfx/under_base.py [apply]
apply: CelDS 의 21프레임을 속옷 몸으로 교체(원본은 EquipDS/base_coat 에 이미 있음)."""
import os, sys
import numpy as np
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import equip_ds as e
import region_map as R

ROOT = e.ROOT
SRC = os.path.join(ROOT, "Docs/ArtCompare/Motion/equip/under")
COAT = os.path.join(ROOT, "Assets/Art/Player/PixelRig/EquipDS/base_coat")
UNDER = os.path.join(ROOT, "Assets/Art/Player/PixelRig/EquipDS/under")


def head_mask(o, lab, u=None):
    h, w = lab.shape
    al = o[:, :, 3] > 128
    ys, xs = np.nonzero(al)
    gp = [(y, x) for y, x in zip(ys, xs) if 0.44 < R.hsv(o[y, x])[0] < 0.55 and R.hsv(o[y, x])[1] > 0.55 and R.hsv(o[y, x])[2] > 0.45]
    gy, gx = (np.mean([p[0] for p in gp]), np.mean([p[1] for p in gp])) if gp else (ys.min() + 8.0, xs.mean())
    hb = int(round(gy)) + 9
    m = np.zeros((h, w), bool)
    for y, x in zip(ys, xs):
        if lab[y, x] == R.CAP:
            hh, ss, vv = R.hsv(o[y, x])
            if not (0.5 <= hh <= 0.62 and ss > 0.3 and not (0.44 < hh < 0.55 and ss > 0.5)):
                m[y, x] = True
        elif lab[y, x] == R.BODY and y <= hb and abs(x - gx) < 16:
            hh, ss, vv = R.hsv(o[y, x])
            if lab[y, x] == R.BODY and (0.02 <= hh <= 0.1 and ss > 0.45 and 0.25 < vv < 0.62 or 0.5 <= hh <= 0.62 and ss > 0.3):  # 갈색 배낭·청록 코트 제외
                continue
            if True:
                m[y, x] = True
    if u is not None:  # 눈 아래에서 원본이 속옷 생성본과 ±1px 로도 다른 픽셀 = 코트 목도리 → 가져오지 않는다
        import importlib.util
        sp = importlib.util.spec_from_file_location("m1", os.path.join(ROOT, "Docs/Research/LayerExtraction_Result/result/code/m1_underdiff.py"))
        sys.path.insert(0, os.path.join(ROOT, "Docs/Research/LayerExtraction_Result/result/code"))
        m1 = importlib.util.module_from_spec(sp); sp.loader.exec_module(m1)
        d = m1.near_diff(o, u, 1)
        rows = np.arange(h)[:, None] >= int(round(gy)) + 2
        m &= ~((d > 40) & rows)
    return m


def main():
    os.makedirs(UNDER, exist_ok=True)
    srcs = [os.path.join(SRC, "t1/%d.png" % i) for i in range(15)] + [os.path.join(SRC, "t2/%d.png" % i) for i in range(6)]
    for f, s in zip(e.FRAMES, srcs):
        o = e.load(os.path.join(COAT, f + ".png"))
        u = e.load(s)
        lab = np.array(Image.open(os.path.join(e.REG, f + "_region.png")))
        hm = head_mask(o, lab, u)
        out = u.copy()
        out[hm] = o[hm]
        Image.fromarray(out).save(os.path.join(UNDER, f + ".png"))
        ub = np.nonzero(u[:, :, 3] > 128); ob = np.nonzero(o[:, :, 3] > 128)
        print(f, "높이", ub[0].min(), ub[0].max(), "/", ob[0].min(), ob[0].max())
    if len(sys.argv) > 1 and sys.argv[1] == "apply":
        for f in e.FRAMES:
            Image.open(os.path.join(UNDER, f + ".png")).save(os.path.join(e.CEL, f + ".png"))
        print("CelDS 교체", len(e.FRAMES))


if __name__ == "__main__":
    main()
