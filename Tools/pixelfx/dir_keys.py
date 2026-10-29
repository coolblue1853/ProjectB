"""몸 방향이 바뀌는 프레임(정면·뒷모습·3/4)의 골격 키(10-29 밤, 회전 베기·돌려차기 — 측면 좌우 반전 폐기 후).
방향 기준 그림의 추정 골격(dirs/skel.json)에서 손목·발목·골반을 옮기고 팔꿈치·무릎은 중간점에서 꺾어 만든다(64px 좌표).
출력: keys json(동작 이름 → kp·hand·angle·times) — m41_submit <json> <이름> <문구> <seed> <방향> 으로 보낸다.
사용: python Tools/pixelfx/dir_keys.py <출력.json>
"""
import json
import math
import os
import sys

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
SKEL = json.load(open(os.path.join(ROOT, "Docs", "ArtCompare", "CharacterGen", "dirs", "skel.json"), encoding="utf-8"))

# 프레임 = (방향, {관절: (x, y)}, 무기 각도) — 0 = 화면 오른쪽, 90 = 위. 'shift' 는 몸 전체 이동
# 회전 베기(양손 무기): 측면 장전(기존 양손 수평 셀) → 3/4 뒤(무기 뒤에 끌림) → 뒷모습(무기 화면 왼쪽 수평) → 정면(무기 화면 오른쪽 수평) → 3/4 앞(앞으로 휩쓸며) → 측면 타격(기존 셀)
MOTIONS = {
    # 두 손은 한 손잡이(1~2px 차이). 짧은 팔로 몸 옆까지 쥐려면 먼 쪽 어깨를 몸 가운데로 비튼다(허리 비틀기)
    "D_SpinNE": [
        ("ne", {"RIGHT SHOULDER": (37, 37), "RIGHT ARM": (29, 44), "LEFT ARM": (28, 45), "LEFT LEG": (28, 52), "RIGHT LEG": (42, 52), "shift": (0, 1)}, 200),
        ("ne", {"RIGHT SHOULDER": (36, 37), "RIGHT ARM": (27, 42), "LEFT ARM": (26, 43), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 1)}, 185),
        ("ne", {"RIGHT SHOULDER": (35, 37), "RIGHT ARM": (26, 41), "LEFT ARM": (25, 42), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 1)}, 175),
    ],
    "D_SpinN": [
        ("north", {"RIGHT SHOULDER": (35, 37), "RIGHT ARM": (27, 40), "LEFT ARM": (26, 41), "LEFT LEG": (25, 53), "RIGHT LEG": (44, 53), "shift": (-1, 1)}, 180),
        ("north", {"RIGHT SHOULDER": (34, 37), "RIGHT ARM": (25, 39), "LEFT ARM": (24, 40), "LEFT LEG": (25, 53), "RIGHT LEG": (44, 53), "shift": (-1, 1)}, 175),
        ("north", {"RIGHT SHOULDER": (37, 36), "RIGHT ARM": (31, 36), "LEFT ARM": (30, 37), "LEFT LEG": (25, 53), "RIGHT LEG": (44, 53), "shift": (0, 1)}, 160),
    ],
    "D_SpinS": [
        ("south", {"LEFT SHOULDER": (31, 36), "RIGHT ARM": (43, 40), "LEFT ARM": (42, 41), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (1, 1)}, 0),
        ("south", {"LEFT SHOULDER": (32, 36), "RIGHT ARM": (45, 39), "LEFT ARM": (44, 40), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (1, 1)}, 5),
        ("south", {"LEFT SHOULDER": (31, 37), "RIGHT ARM": (42, 44), "LEFT ARM": (41, 45), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (1, 1)}, -20),
    ],
    "D_SpinSE": [
        ("se", {"RIGHT SHOULDER": (39, 36), "RIGHT ARM": (50, 41), "LEFT ARM": (51, 41), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 1)}, -5),
        ("se", {"RIGHT SHOULDER": (40, 36), "RIGHT ARM": (51, 40), "LEFT ARM": (52, 40), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 1)}, 0),
        ("se", {"RIGHT SHOULDER": (39, 37), "RIGHT ARM": (49, 44), "LEFT ARM": (50, 44), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 1)}, -15),
    ],
    # 돌려차기: 측면 → 3/4 뒤(뒤돌며 무릎 듦) → 뒷모습(무릎 옆으로 높이) → 측면 차기(기존 돌려차기 셀 뻗음)
    "D_KickNE": [
        ("ne", {"RIGHT ARM": (46, 40), "LEFT ARM": (24, 42), "RIGHT LEG": (44, 51), "shift": (0, 0)}, 0),
        ("ne", {"RIGHT ARM": (47, 38), "LEFT ARM": (22, 40), "RIGHT LEG": (48, 47), "shift": (-1, 0)}, 0),
        ("ne", {"RIGHT ARM": (47, 37), "LEFT ARM": (21, 39), "RIGHT LEG": (53, 43), "shift": (-1, -1)}, 0),
    ],
    "D_KickN": [
        ("north", {"RIGHT ARM": (48, 39), "LEFT ARM": (22, 41), "RIGHT LEG": (50, 47), "LEFT LEG": (33, 53), "shift": (-1, 0)}, 0),
        ("north", {"RIGHT ARM": (49, 37), "LEFT ARM": (21, 40), "RIGHT LEG": (55, 42), "LEFT LEG": (33, 53), "shift": (-2, -1)}, 0),
        ("north", {"RIGHT ARM": (48, 36), "LEFT ARM": (20, 40), "RIGHT LEG": (57, 38), "LEFT LEG": (33, 53), "shift": (-2, -1)}, 0),
    ],
    # 로프 오르기(뒷모습, 메이플식): 줄은 몸 가운데(x≈35). 손을 번갈아 머리 옆까지, 같은 쪽 무릎을 들어 디딤 — 4장 반복
    "D_RopeN": [
        ("north", {"RIGHT ARM": (37, 29), "LEFT ARM": (33, 35), "RIGHT LEG": (38, 49), "LEFT LEG": (32, 53)}, 0),
        ("north", {"RIGHT ARM": (37, 31), "LEFT ARM": (33, 32), "RIGHT LEG": (38, 51), "LEFT LEG": (32, 51), "shift": (0, -1)}, 0),
        ("north", {"RIGHT ARM": (37, 35), "LEFT ARM": (33, 29), "RIGHT LEG": (38, 53), "LEFT LEG": (32, 49)}, 0),
        ("north", {"RIGHT ARM": (37, 32), "LEFT ARM": (33, 31), "RIGHT LEG": (38, 51), "LEFT LEG": (32, 51), "shift": (0, -1)}, 0),
    ],
}
LIMB = {"ARM": ("SHOULDER", "ELBOW"), "LEG": ("HIP", "KNEE")}


def build(view, edits):
    base = {q["label"]: [q["x"] * 64, q["y"] * 64] for q in SKEL[view]}
    dx, dy = edits.get("shift", (0, 0))
    upper = {"NOSE", "NECK", "RIGHT EYE", "LEFT EYE", "RIGHT EAR", "LEFT EAR", "RIGHT SHOULDER", "LEFT SHOULDER"}
    for k in base:  # 상체만 기울이고(이동량 전부), 골반은 반만
        f = 1.0 if k in upper else (0.5 if "HIP" in k else 0)
        base[k][0] += dx * f
        base[k][1] += dy * f
    for k, v in edits.items():
        if k == "shift":
            continue
        base[k] = list(v)
    # 팔·다리 길이는 기준 그림 골격 그대로(10-29 밤 사용자 "팔이 고무인간이냐" — 손목을 원래 팔 길이 두 배로 잡아 생성기가 팔을 늘렸다).
    # 목표가 닿는 거리보다 멀면 그 방향으로 최대 거리까지만, 팔꿈치·무릎은 두 마디 길이로 IK(팔은 아래로, 무릎은 앞으로 굽힘)
    orig = {q["label"]: (q["x"] * 64, q["y"] * 64) for q in SKEL[view]}
    for side in ("RIGHT", "LEFT"):
        for end, (root, mid) in LIMB.items():
            r = base[f"{side} {root}"]
            o_r, o_m, o_e = orig[f"{side} {root}"], orig[f"{side} {mid}"], orig[f"{side} {end}"]
            l1 = max(2.5, math.dist(o_r, o_m))
            l2 = max(2.5, math.dist(o_m, o_e))
            e = base[f"{side} {end}"]
            d = math.dist(r, e)
            reach = (l1 + l2) * 0.97
            if d > reach:
                e = [r[0] + (e[0] - r[0]) * reach / d, r[1] + (e[1] - r[1]) * reach / d]
                base[f"{side} {end}"] = e
                d = reach
            if f"{side} {mid}" in edits:
                continue
            d = max(d, 0.5)
            a = (l1 * l1 - l2 * l2 + d * d) / (2 * d)
            h = math.sqrt(max(0.0, l1 * l1 - a * a))
            ux, uy = (e[0] - r[0]) / d, (e[1] - r[1]) / d
            px, py = r[0] + ux * a, r[1] + uy * a
            nx, ny = -uy, ux  # 두 굽힘 후보 중 팔은 아래(y 큰 쪽), 다리는 무릎이 몸 앞(x 큰 쪽)
            c1, c2 = (px + nx * h, py + ny * h), (px - nx * h, py - ny * h)
            if end == "ARM":
                base[f"{side} {mid}"] = list(max(c1, c2, key=lambda c: c[1]))
            else:
                base[f"{side} {mid}"] = list(max(c1, c2, key=lambda c: c[0]))
    return [{"label": k, "x": round(min(max(v[0], 0), 63) / 64, 3), "y": round(min(max(v[1], 0), 63) / 64, 3)} for k, v in base.items()]


def main():
    out = {}
    for name, fr in MOTIONS.items():
        kps = [build(v, e) for v, e, _ in fr]
        out[name] = {"kp": kps, "view": fr[0][0], "angle": [a for _, _, a in fr], "times": [40] * len(fr),
                     "hand": [[int(round(p["x"] * 64)), int(round(p["y"] * 64))] for k in kps for p in k if p["label"] == "RIGHT ARM"]}
    json.dump(out, open(sys.argv[1], "w"), indent=0)
    print(list(out))


if __name__ == "__main__":
    main()
