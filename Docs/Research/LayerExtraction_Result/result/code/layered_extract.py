"""한 겹씩 입히기(생성) 결과 → 부위 레이어 + 영역 지도 + 검사 점수.

생성 쪽 절차(GEN_RESEARCH_GUIDE.md): 속옷 몸 → 단계 A(하의) → 단계 B(상의) → [단계 C(모자·장갑)] 를 같은 21프레임으로 만든다.
각 단계 그림은 바로 앞 단계 그림을 입력으로 전이한 것이어야 한다.

레이어 = 이 단계 그림에서 '앞 단계와 달라진' 픽셀(±1px 이웃 허용 비교). 앞 단계에서 이미 끝까지 그려졌으므로 가려진 부분이 비지 않는다.
여러 부위가 한 단계에 있으면 속옷 몸 기하로 나눈다: 바지/신발 = 부츠선, 모자/장갑 = 속옷 살색 손 주변.

사용:
  python layered_extract.py <속옷 몸 폴더> <출력 폴더> legs+shoes=<단계A 폴더> top=<단계B 폴더> [cap+glove=<단계C 폴더>]
  (각 폴더에 ds_idle_0.png ... 프레임 이름 그대로. 일부 프레임만 있으면 그것만 처리)
출력:
  <출력>/layers/<부위>/<프레임>.png   부위 레이어(가려진 부분까지 완전한 것)
  <출력>/regions/<프레임>_region.png  최종 단계 그림 기준 영역 지도(0~8, 기존 형식 호환: 보이는 픽셀의 출처 단계)
  <출력>/extract_report.json          프레임별: 부위 픽셀 수, 단계가 앞 단계를 지운 픽셀(erase), 앞 단계 다시 그림(drift) 의심 픽셀
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import common as C
import m1_underdiff as M1

T_DIFF = 30      # ±1px 이웃 중 가장 비슷한 픽셀과 RGB 합 차이가 이보다 크면 '새로 그린 픽셀'
MIN_PIECE = 1    # 이보다 작은 새 픽셀 조각은 버림(모의 검증에서 1이 재구성 오차 최소: 3→19px, 1→14px)
SLOT_ID = {"cap": 1, "top": 2, "glove": 3, "legs": 4, "shoes": 5, "back": 7}


def new_pixels(cur, prev):
    a = C.alpha(cur)
    nd = M1.near_diff(cur, prev, 1)
    region = a & (nd > T_DIFF)                       # ±1px 밀림까지 허용해도 다른 곳 = 확실히 새로 그린 영역
    strict = a & ((C.coldiff(cur, prev) > T_DIFF) | ~C.alpha(prev))
    new = (strict & M1.dil(region, 2)) | (a & ~C.alpha(prev))  # 영역 근처는 정확 비교, 앞 단계 투명 자리는 무조건 새 픽셀
    # 작은 조각 제거
    keep = np.zeros_like(new)
    rem = new.copy()
    while rem.any():
        y, x = np.argwhere(rem)[0]
        sd = np.zeros_like(rem); sd[y, x] = True
        comp = M1.cc_from(sd, new)
        rem &= ~comp
        if comp.sum() >= MIN_PIECE:
            keep |= comp
    return keep


def split(slots, mask, g, img):
    """한 단계 안의 여러 부위를 속옷 몸 기하로 나눔"""
    out = {}
    if slots == ["legs", "shoes"] or slots == ["shoes", "legs"]:
        out["shoes"] = mask & g["shoe_zone"]
        out["legs"] = mask & ~g["shoe_zone"]
    elif set(slots) == {"cap", "glove"}:
        hand = M1.dil(g["skin"] & ~M1.dil(head_zone(g), 1), 2)
        out["glove"] = mask & hand
        out["cap"] = mask & ~hand
    elif len(slots) == 1:
        out[slots[0]] = mask
    else:
        raise SystemExit("지원하지 않는 묶음: %s (legs+shoes, cap+glove, 또는 한 부위)" % "+".join(slots))
    return out


def head_zone(g):
    ua = g["ua"]
    ys = np.nonzero(ua)[0]
    top = ys.min()
    zone = np.zeros_like(ua)
    zone[: top + 18] = True  # 64px 2등신: 머리 ≈ 위 18줄(속옷 몸 기준)
    return zone & ~g["waist"]


def extract(under, stages, frame):
    """stages: [(부위 목록, RGBA)] 순서대로. 반환: 레이어 dict, 지도, 보고"""
    g = M1.under_geometry(under)
    prev = under
    layers = {}
    owner = np.where(C.alpha(under), C.BODY, 0).astype(np.uint8)
    rep = {}
    for slots, img in stages:
        new = new_pixels(img, prev)
        erase = C.alpha(prev) & ~C.alpha(img)  # 이 단계가 앞 그림을 지운 곳(모자가 머리카락을 누르는 등)
        # drift: 이 단계 부위가 올 수 없는 곳(이전 단계 레이어 영역 안쪽 깊은 곳)에서 바뀐 픽셀
        deep_prev = np.zeros_like(new)
        for k, m in layers.items():
            if k.endswith("_rgba"):
                continue
            inner = m & ~M1.dil(~m, 1)
            deep_prev |= inner
        drift = new & deep_prev
        parts = split(slots, new, g, img)
        for k, m in parts.items():
            lay = np.zeros_like(img)
            lay[m] = img[m]
            layers[k] = m
            layers[k + "_rgba"] = lay
            owner[m] = SLOT_ID[k]
        owner[erase] = 0
        rep["+".join(slots)] = dict(new_px=int(new.sum()), erase_px=int(erase.sum()), drift_px=int(drift.sum()),
                                    **{k + "_px": int(m.sum()) for k, m in parts.items()})
        prev = img
    final = prev
    owner[~C.alpha(final)] = 0
    owner = M1.fill_diagonal(C.nearest_fill(owner, C.alpha(final)), C.alpha(final))
    return {k: v for k, v in layers.items() if k.endswith("_rgba")}, owner, rep


def main():
    udir, out = sys.argv[1:3]
    specs = []
    for a in sys.argv[3:]:
        k, d = a.split("=", 1)
        specs.append((k.split("+"), d))
    frames = [f for f in C.FRAMES if all(os.path.exists(os.path.join(d, f + ".png")) for _, d in specs)]
    report = {}
    os.makedirs(os.path.join(out, "regions"), exist_ok=True)
    for f in frames:
        under = C.load(os.path.join(udir, f + ".png"))
        stages = [(s, C.load(os.path.join(d, f + ".png"))) for s, d in specs]
        lay, owner, rep = extract(under, stages, f)
        for k, img in lay.items():
            d = os.path.join(out, "layers", k[:-5])
            os.makedirs(d, exist_ok=True)
            Image.fromarray(img).save(os.path.join(d, f + ".png"))
        C.save_map(owner, os.path.join(out, "regions", f + "_region.png"))
        report[f] = rep
    json.dump(report, open(os.path.join(out, "extract_report.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    print("frames", len(frames))


if __name__ == "__main__":
    main()
