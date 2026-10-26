"""장비 레이어 굽기(CelEquipmentPlan 「확정」 던파식): transfer_outfit 결과 묶음 → 클립별 덮어쓰기 레이어 + 지울 마스크.

입력: 장비 폴더(Docs/ArtCompare/Motion/equip/<장비 id>/) 아래 묶음 폴더들 — 각 묶음 폴더의 0.png, 1.png … 가 BATCHES 의 (클립, 프레임) 순서.
출력: Assets/Art/Player/PixelRig/Equip/<장비 id>/<클립>_<k>.png(레이어) + <클립>_<k>_erase.png(맨몸에서 지울 마스크) + equip.json(슬롯·프레임 목록).
레이어 = 장비 프레임에서 맨몸 셀과 색이 다른 픽셀(8px 미만 덩어리는 전이 잡티로 버림), 마스크 = 맨몸엔 있고 장비 프레임엔 없는 픽셀(삐져나온 머리카락 등).
사용: python Tools/pixelfx/build_equip.py <장비 id> <슬롯>   예) python Tools/pixelfx/build_equip.py iron_set body
"""
import json
from collections import Counter
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from extract_layer import MIN_PART, clean  # noqa: E402
from build_cel import IDLE_STAND, IDLE_STRETCH_ROW, JUMP_SET, RUN_SET, SCARF, XSHIFT, components, near  # noqa: E402

CEL = "Assets/Art/Player/PixelRig/Cel"
SRC = "Docs/ArtCompare/Motion/equip"
OUT = "Assets/Art/Player/PixelRig/Equip"
TOL = 24
# 전이 묶음(최대 15프레임 — 같은 값): 폴더 이름 → (클립, 프레임) 순서. 셀 클립을 바꾸면 여기도 맞춘다
BATCHES = {
    "walk": [("Walk", k) for k in range(8)],
    # 달리기를 7 → 6프레임(run_loop 2~7)으로 바꾼 뒤: 새 Run_k = 전이 묶음 k+1번, 예전 발 맞춤(4px)과 지금 고정 높이(3px) 차이로 앞 두 장은 1px 위로.
    # 항목: (클립, 프레임) 또는 (클립, 프레임, 묶음 안 번호, 세로 이동). None = 이 자리 전이 결과는 안 쓴다
    "batch_a": [None] + [("Run", k, k + 1, -1 if k < 2 else 0) for k in range(6)] + [("JumpCrouch", k) for k in range(3)] + [("JumpArc", k) for k in range(4)],
    "batch_b": [("Land", k) for k in range(3)] + [("Idle", k) for k in range(4)] + [("Dash", k) for k in range(3)] + [("Hurt", k) for k in range(3)],
    "batch_c": [("Death", k) for k in range(8)],
    "batch_d": [("Stab", k) for k in range(7)] + [("SwingDown", k, 7 + (3 if k == 2 else k)) for k in range(7)],
    "batch_e": [("SwingUp", k, 5 if k == 4 else k) for k in range(7)],
    "batch_f": [("RopeUp", k) for k in range(8)],
    "batch_f_stay": [("RopeStay", 0, 0, 0)],  # 매달리기 = 오르기 0번과 같은 그림
    "batch_g": [("GreatDown", k) for k in range(7)],
    "batch_h": [("DaggerFlurry", k) for k in range(7)] + [("DaggerCross", k) for k in range(3)],
    # 10-26 저녁: 똑바로 선 대기 + 다리 벌린 도약 점프(batch_i), 팔 크게 흔드는 달리기(batch_j)
    "batch_i": [("Idle", 0)] + [("JumpCrouch", k) for k in range(3)] + [("JumpArc", k) for k in range(4)] + [("Land", k) for k in range(3)],
    "batch_j": [("Run", k) for k in range(8)],
    "batch_k": [("Idle", 0), None],  # 메이플식 대기(허리에 손) — 두 번째는 전이 최소 2장 채우기용 걷기 3번
}
# 새 셀로 바뀐 클립은 예전 묶음의 전이 결과를 쓰지 않는다(몸 그림이 달라 레이어가 어긋난다)
# 대기 전이: stand_edit2 는 cand2(batch_i 0번 전이)와 상체가 같아 batch_i, maple2 는 batch_k
_IDLE_BATCH = "batch_k" if "maple" in IDLE_STAND else "batch_i"
NEW = {"batch_i": ({"Idle"} if IDLE_STAND and _IDLE_BATCH == "batch_i" else set()) | ({"JumpCrouch", "JumpArc", "Land"} if JUMP_SET == "leap" else set()),
       "batch_k": {"Idle"} if IDLE_STAND and _IDLE_BATCH == "batch_k" else set(),
       "batch_j": {"Run"} if RUN_SET == "pump" else set()}
REPLACED = set().union(*NEW.values())


# 맨몸 셔츠(초록) 색 — 갑옷 전이가 일부 프레임(대검 내려찍기)에서 셔츠를 덜 덮어 초록이 드러났다
SHIRT = [(106, 110, 63), (66, 62, 38)]


def cover_shirt(done):
    """다른 프레임에서 셔츠 색이 어떤 장비 색으로 덮였는지 투표해, 덮이지 않고 남은 셔츠 픽셀을 그 색으로 채운다."""
    votes = {c: Counter() for c in SHIRT}
    masks = []
    for _, _, b, layer, lm, erase in done:
        ms = {c: np.all(b[..., :3] == c, axis=2) & (b[..., 3] > 0) for c in SHIRT}
        masks.append(ms)
        for c, m in ms.items():
            votes[c].update(tuple(int(v) for v in px) for px in layer[m & lm][:, :3])
    to = {c: v.most_common(1)[0][0] for c, v in votes.items() if v}
    if len(to) < len(SHIRT):
        return  # 셔츠를 덮지 않는 장비(투구만 등)
    for (_, _, b, layer, lm, erase), ms in zip(done, masks):
        for c, m in ms.items():
            fill = m & ~lm & ~erase
            layer[fill, :3] = to[c]
            layer[fill, 3] = 255
            lm |= fill


OUTLINE = (6, 2, 2)  # 장비 레이어 외곽선 색


def seal_outline(b, layer, lm, erase):
    """장비의 밝은 픽셀이 바로 바깥(합성 후 빈칸)과 맞닿으면 그 빈칸에 외곽선을 한 겹 — 전이가 투구 테두리 외곽선을 군데군데 빼먹었다."""
    solid = (b[..., 3] > 0) & ~erase | lm
    light = lm & (layer[..., :3].sum(axis=2) > 180)
    edge = np.zeros_like(solid)
    edge[1:] |= light[:-1]; edge[:-1] |= light[1:]; edge[:, 1:] |= light[:, :-1]; edge[:, :-1] |= light[:, 1:]
    add = edge & ~solid
    layer[add, :3] = OUTLINE
    layer[add, 3] = 255
    lm |= add


def trim_spikes(b, lm, erase):
    """장비 테두리 밖으로 1px 삐죽 나온 맨몸 외곽선(투구 아래 머리카락 끝)을 지운다 — 합성 후 이웃 불투명이 하나뿐인 어두운 픽셀."""
    near_layer = np.zeros_like(lm)
    near_layer[1:] |= lm[:-1]; near_layer[:-1] |= lm[1:]; near_layer[:, 1:] |= lm[:, :-1]; near_layer[:, :-1] |= lm[:, 1:]
    for _ in range(2):
        solid = (b[..., 3] > 0) & ~erase | lm
        n = np.zeros(solid.shape, int)
        n[1:] += solid[:-1]; n[:-1] += solid[1:]; n[:, 1:] += solid[:, :-1]; n[:, :-1] += solid[:, 1:]
        dark = b[..., :3].sum(axis=2) < 120
        spike = solid & ~lm & dark & (n <= 1) & near_layer
        erase |= spike


HELM_CANON = ("Walk", 3)  # 모든 프레임 투구를 이 프레임 투구로 통일(전이가 프레임마다 흠집·땜빵을 달리 그렸다 — 사용자 "가만히 있으면 땜빵이 생기고 걸으면 사라진다")
HELM_SKIP = {"RopeUp", "RopeStay", "Death"}  # 뒷모습·눕기는 모양이 달라 그대로
HELM_IOU = 0.55


def _cut(b):
    rows = np.nonzero(near(b.astype(np.uint8), SCARF).any(axis=1))[0]
    return int(rows.min()) if len(rows) else 40


def _roll2(a, dy, dx):
    return np.roll(np.roll(a, dy, axis=0), dx, axis=1)


def canon_helm(done):
    """프레임마다 투구(스카프 윗줄 위 레이어)를 기준 투구와 가장 잘 겹치는 위치를 찾아 기준 투구로 바꾼다."""
    ref = next((d for d in done if (d[0], d[1]) == HELM_CANON), None)
    if ref is None:
        return
    rc = _cut(ref[2])
    cm = ref[4].copy()
    cm[rc:] = False
    if cm.sum() < 30:
        return  # 투구 없는 장비
    cl = ref[3].copy()
    n = 0
    for clip, k, b, layer, lm, erase in done:
        if clip in HELM_SKIP:
            continue
        c = _cut(b)
        hm = lm.copy()
        hm[c:] = False
        if hm.sum() < 30:
            continue
        best, bd = 0.0, (0, 0)
        for dy in range(-8, 9):
            for dx in range(-8, 9):
                m = _roll2(cm, dy, dx)
                iou = (m & hm).sum() / max(1, (m | hm).sum())
                if iou > best:
                    best, bd = iou, (dy, dx)
        if best < HELM_IOU:
            continue
        m = _roll2(cm, *bd)
        m[c + 2:] = False
        lm[:c] = False
        layer[:c] = 0
        layer[m] = _roll2(cl, *bd)[m]
        lm |= m
        n += 1
    print("투구 통일", n, "프레임")


IDLE_LEG_ROW = 46


def idle_breath(done):
    """대기 1~3번 레이어는 0번 레이어로 만든다: 몸과 같이 튜닉 줄 위를 1px 올림(대기 몸 = 0·올림·올림·0) — 프레임마다 전이하면 투구가 들썩였다."""
    src = next((d for d in done if d[0] == "Idle" and d[1] == 0), None)
    if src is None or not IDLE_STAND:
        return
    done[:] = [d for d in done if d[0] != "Idle" or d[1] == 0]
    _, _, _, layer, lm, erase = src
    # 대기 다리는 전이 뒤에 손으로 다시 그렸다 — 다리 줄(46~)의 전이 결과는 버린다(옛 다리를 덧그리거나 새 다리를 지웠다)
    layer[IDLE_LEG_ROW:] = 0
    lm[IDLE_LEG_ROW:] = False
    erase[IDLE_LEG_ROW:] = False
    for k, up in ((1, True), (2, True), (3, False)):
        L, M, E = layer.copy(), lm.copy(), erase.copy()
        if up:
            r = IDLE_STRETCH_ROW
            for a in (L, M, E):
                a[:r] = a[1:r + 1].copy()
        b = np.array(Image.open(os.path.join(CEL, f"Idle_{k}.png")).convert("RGBA")).astype(int)
        done.append(("Idle", k, b, L, M, E))


def keep_scarf(b, layer, lm, erase):
    """스카프는 늘 갑옷 위에 보이게 — 전이가 프레임마다 스카프를 덮기도 하고 두기도 해서(걷기엔 보이고 대기·점프엔 사라짐) 깜빡였다."""
    sc = near(b.astype(np.uint8), SCARF) & (b[..., 3] > 0)
    lm &= ~sc
    layer[sc] = 0
    erase &= ~sc


# 전이 없이 다른 프레임 레이어를 그대로 쓰는 프레임(몸 그림이 거의 같다): 착지 끝 = 걷기 3번(23px 차이)
ALIAS = {("Land", 3): ("Walk", 3)}


def alias_layers(done):
    for (clip, k), (sc, sk) in ALIAS.items():
        src = next((d for d in done if (d[0], d[1]) == (sc, sk)), None)
        path = os.path.join(CEL, f"{clip}_{k}.png")
        if src is None or not os.path.exists(path):
            continue
        b = np.array(Image.open(path).convert("RGBA")).astype(int)
        done.append((clip, k, b, src[3].copy(), src[4].copy(), src[5].copy()))


def main():
    eid, slot = sys.argv[1], sys.argv[2]
    out = os.path.join(OUT, eid)
    os.makedirs(out, exist_ok=True)
    frames, done = [], []
    for folder, items in BATCHES.items():
        src = os.path.join(SRC, eid, folder)
        if not os.path.isdir(src):
            print("없음:", src)
            continue
        for i, item in enumerate(items):
            if item is None:
                continue
            clip, k = item[0], item[1]
            if folder in NEW and clip not in NEW[folder] or folder not in NEW and clip in REPLACED:
                continue
            src_i = item[2] if len(item) > 2 else i
            shift = item[3] if len(item) > 3 else 0
            b = np.array(Image.open(os.path.join(CEL, f"{clip}_{k}.png")).convert("RGBA")).astype(int)
            e = np.array(Image.open(os.path.join(src, f"{src_i}.png")).convert("RGBA")).astype(int)
            if shift:
                e = np.roll(e, shift, axis=0)
            if clip in XSHIFT:
                e = np.roll(e, XSHIFT[clip][k], axis=1)  # 셀과 같은 가로 맞춤
            bo, eo = b[..., 3] > 0, e[..., 3] > 0
            differ = np.abs(b[..., :3] - e[..., :3]).sum(axis=2) > TOL
            lm = clean(eo & (~bo | differ), MIN_PART)
            layer = np.zeros_like(e)
            layer[lm] = e[lm]
            layer[..., 3] = np.where(lm, 255, 0)
            erase = clean(bo & ~eo, 2)
            # 장비는 몸에 붙어 있어야 한다: 맨몸 그림에서 4px 밖의 레이어 픽셀은 버린다(공격 프레임 전이가 잔상 자리에 투구를 그렸다)
            near_body = bo.copy()
            for _ in range(4):
                g = near_body.copy()
                g[1:] |= near_body[:-1]; g[:-1] |= near_body[1:]; g[:, 1:] |= near_body[:, :-1]; g[:, :-1] |= near_body[:, 1:]
                near_body = g
            lm &= near_body
            # 합성했을 때 몸 본체와 떨어진 레이어 조각은 버린다(공격 타격 프레임에서 투구가 허공에 따로 떴다)
            comp = bo & ~erase | lm
            parts = components(comp)
            if parts:
                big = max(len(p) for p in parts)
                for part in parts:
                    if len(part) < big * 0.25:
                        for y, x in part:
                            lm[y, x] = False
                layer = np.zeros_like(e)
                layer[lm] = e[lm]
                layer[..., 3] = np.where(lm, 255, 0)
            done.append((clip, k, b, layer, lm, erase))
    idle_breath(done)
    alias_layers(done)
    canon_helm(done)
    cover_shirt(done)
    for _, _, b, layer, lm, erase in done:
        keep_scarf(b, layer, lm, erase)
    for _, _, b, layer, lm, erase in done:
        seal_outline(b, layer, lm, erase)
        trim_spikes(b, lm, erase)
    for clip, k, b, layer, lm, erase in done:
        name = f"{clip}_{k}"
        Image.fromarray(layer.astype(np.uint8), "RGBA").save(os.path.join(out, name + ".png"))
        Image.fromarray(np.where(erase, 255, 0).astype(np.uint8), "L").save(os.path.join(out, name + "_erase.png"))
        frames.append({"clip": clip, "k": k, "layer": int(lm.sum()), "erase": int(erase.sum())})
    with open(os.path.join(out, "equip.json"), "w") as fp:
        json.dump({"id": eid, "slot": slot, "frames": frames}, fp, indent=1)
    print(eid, slot, len(frames), "프레임")


if __name__ == "__main__":
    main()
