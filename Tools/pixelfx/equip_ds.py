"""CelDS 부위 합성 도구(CelEquipmentPlan 「부위 합성 설계 원칙」).

기본 몸 영역 지도(region_map.py) → 확장 지도(세트 전이 그림이 몸 밖으로 나온 픽셀도 가까운 부위로) → 세트별 부위 레이어.
합성 규칙: 확장 지도의 부위 s 가 장비되어 있으면 그 픽셀 = 장비 그림(없으면 투명), 아니면 기본 몸 그림. 부위 6(맨몸)은 항상 기본.
사용:
  python Tools/pixelfx/equip_ds.py build <세트 id> <전이 결과 폴더들...>   # 프레임 순서는 FRAMES
  python Tools/pixelfx/equip_ds.py combos <세트 a> <세트 b> <출력 시트>      # 부위별 섞어 입기 시트(+검사)
"""
import itertools
import json
import os
import sys
from collections import deque

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
CEL = os.path.join(ROOT, "Assets/Art/Player/PixelRig/CelDS")
REG = os.path.join(ROOT, "Docs/ArtCompare/Motion/equip/regions")
OUT = os.path.join(ROOT, "Assets/Art/Player/PixelRig/EquipDS")
SLOTS = {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}
FRAMES = ["ds_idle_%d" % i for i in range(8)] + ["ds_stab_%d" % i for i in range(4)] + ["ds_down_%d" % i for i in range(4)] + ["ds_up_%d" % i for i in range(5)]
UNDER = os.path.join(ROOT, "Assets/Art/Player/PixelRig/EquipDS/under")  # 속옷 기본 몸(G1-6): 모든 장비는 이 위에 쌓는다
BASE = "base"  # 기본 방어구 = 원본 몸 프레임


def load(path):
    return np.array(Image.open(path).convert("RGBA"))


def ext_map(frame, armors):
    """기본 지도 + 장비 그림 픽셀 중 지도 밖(몸 밖)인 것을 가까운 비(非)맨몸 부위로 확장"""
    lab = np.array(Image.open(os.path.join(REG, frame + "_region.png")))
    h, w = lab.shape
    cover = np.zeros((h, w), bool)
    for a in armors:
        cover |= a[:, :, 3] > 128
    out = lab.copy()
    q = deque((y, x) for y, x in zip(*np.nonzero(((lab >= 1) & (lab <= 5)) | (lab == 7))))
    dist = np.full((h, w), 99)
    for y, x in q:
        dist[y, x] = 0
    while q:
        y, x = q.popleft()
        if dist[y, x] >= 3:
            continue
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w and out[ny, nx] == 0 and cover[ny, nx] and dist[ny, nx] == 99:
                out[ny, nx] = out[y, x]
                dist[ny, nx] = dist[y, x] + 1
                q.append((ny, nx))
    return out


def frame_armor_files(set_id):
    d = os.path.join(OUT, set_id)
    return {f: load(os.path.join(d, f + ".png")) for f in FRAMES if os.path.exists(os.path.join(d, f + ".png"))}


def build(set_id, folders):
    imgs = []
    for fo in folders:
        files = sorted(os.listdir(fo), key=lambda n: int(os.path.splitext(n)[0]))
        imgs += [load(os.path.join(fo, f)) for f in files if f.endswith(".png")]
    assert len(imgs) == len(FRAMES), (len(imgs), len(FRAMES))
    d = os.path.join(OUT, set_id)
    os.makedirs(d, exist_ok=True)
    for f, im in zip(FRAMES, imgs):
        Image.fromarray(im).save(os.path.join(d, f + ".png"))  # 세트 전체 전이 그림(원본)
    mp = os.path.join(OUT, "_maps")
    os.makedirs(mp, exist_ok=True)
    return len(imgs)


CLIP = {"ds_idle": "Idle", "ds_stab": "Stab", "ds_down": "SwingDown", "ds_up": "SwingUp"}
SLOT_NAME = {1: "head", 2: "body", 3: "gloves", 4: "legs", 5: "shoes"}  # CelEquipLayer.Slot 이름


def export(set_id):
    """세트의 부위별 레이어(그림 = 장비 픽셀 중 확장 지도가 그 부위인 곳, 지울 마스크 = 기본 지도가 그 부위인 곳)를
    Assets/Art/Player/PixelRig/Equip/<세트>_<부위>/ 에 PixelRigBuilder 형식(equip.json + <클립>_<k>.png·_erase.png)으로 쓴다"""
    sets = {set_id: frame_armor_files(set_id)}
    root = os.path.join(ROOT, "Assets/Art/Player/PixelRig/Equip")
    for slot, sname in SLOT_NAME.items():
        lid = "%s_%s" % (set_id, {1: "cap", 2: "top", 3: "glove", 4: "legs", 5: "shoes"}[slot])
        d = os.path.join(root, lid)
        os.makedirs(d, exist_ok=True)
        frames = []
        for f in FRAMES:
            arm = sets[set_id][f]
            emap = ext_map(f, [arm])
            if set_id == "base_coat":
                emap = np.where(emap == 7, 2, emap)  # 기본 코트 상의 = 배낭·랜턴 포함. 다른 세트는 배낭 없음
            base = np.array(Image.open(os.path.join(REG, f + "_region.png")))
            layer = np.zeros_like(arm)
            m = (emap == slot) & (arm[:, :, 3] > 128)
            layer[m] = arm[m]
            erase = np.zeros(base.shape, np.uint8)  # 속옷 몸 위에 쌓으므로 지울 영역 없음(G1-6)
            clip = CLIP[f.rsplit("_", 1)[0]]
            k = int(f.rsplit("_", 1)[1])
            Image.fromarray(layer).save(os.path.join(d, "%s_%d.png" % (clip, k)))
            Image.fromarray(erase, "L").save(os.path.join(d, "%s_%d_erase.png" % (clip, k)))
            frames.append({"clip": clip, "k": k})
        json.dump({"id": lid, "slot": sname, "ds": True, "frames": frames}, open(os.path.join(d, "equip.json"), "w"), indent=1)
        print(lid, len(frames))


def compose(frame, wear, sets):
    """wear: {부위 번호: 세트 id 또는 BASE}. 기본 몸 + 장비 그림을 확장 지도로 합성"""
    base = load(os.path.join(UNDER, frame + ".png"))
    armors = [sets[s][frame] for s in set(wear.values()) if s != BASE and s in sets]
    emap = ext_map(frame, armors)
    out = base.copy()
    h, w = emap.shape
    for slot, sid in wear.items():
        if sid == BASE or sid not in sets:
            continue
        m = emap == slot
        out[m] = sets[sid][frame][m]
    # 장비가 바뀐 부위 밖에서 기본 몸이 지도 밖으로 삐져나온 곳은 없다(지도는 기본 몸을 전부 덮음)
    return out, emap


def issues(img, emap, wear):
    """합성 결과 검사: 확장 지도 안인데 비어 있는 곳 중 이웃이 둘 다 불투명한 1px 틈 개수"""
    a = img[:, :, 3] > 128
    h, w = a.shape
    gaps = 0
    for y in range(1, h - 1):
        for x in range(1, w - 1):
            if not a[y, x]:
                if (a[y, x - 1] and a[y, x + 1]) or (a[y - 1, x] and a[y + 1, x]):
                    gaps += 1
    return gaps


def combos(set_a, set_b, outpath):
    sets = {s: frame_armors for s, frame_armors in ((set_a, frame_armor_files(set_a)), (set_b, frame_armor_files(set_b)))}
    probe = ["ds_idle_0", "ds_stab_1", "ds_down_2", "ds_up_3"]
    rows = []
    # 부위마다 a 또는 b: 2^5 = 32 조합 중 대표 + 기본
    patterns = [dict.fromkeys(SLOTS, BASE), dict.fromkeys(SLOTS, set_a), dict.fromkeys(SLOTS, set_b)]
    for k in SLOTS:  # 한 부위만 b, 나머지 a
        w = dict.fromkeys(SLOTS, set_a)
        w[k] = set_b
        patterns.append(w)
    for k in SLOTS:  # 한 부위만 a, 나머지 기본
        w = dict.fromkeys(SLOTS, BASE)
        w[k] = set_a
        patterns.append(w)
    total_gaps = 0
    for wear in patterns:
        row = []
        for f in probe:
            im, em = compose(f, wear, sets)
            g = issues(im, em, wear)
            total_gaps += g
            row.append(Image.fromarray(im))
        rows.append(row)
    W, H = 64, 64
    sheet = Image.new("RGBA", (W * len(probe), H * len(rows)), (90, 90, 90, 255))
    for r, row in enumerate(rows):
        for c, im in enumerate(row):
            sheet.alpha_composite(im, (c * W, r * H))
    sheet = sheet.resize((sheet.width * 3, sheet.height * 3), Image.NEAREST)
    sheet.save(outpath)
    print("patterns", len(patterns), "gap pixels total", total_gaps)


if __name__ == "__main__":
    cmd = sys.argv[1]
    if cmd == "build":
        print("frames", build(sys.argv[2], sys.argv[3:]))
    elif cmd == "export":
        export(sys.argv[2])
    elif cmd == "combos":
        combos(sys.argv[2], sys.argv[3], sys.argv[4])
