"""생성 스프라이트가 원본(목표)과 얼마나 비슷한지 점수 매기기 — 세대별 보정 학습용(CharacterPixelRules 11절).

두 투명 PNG를 받아 항목별 점수(0~100)와 종합 점수를 낸다. 낮은 항목이 다음 세대에서 고칠 곳이다.
- silhouette : 불투명 영역을 같은 크기 상자로 맞춘 뒤 겹침(IoU) — 전체 형태·자세
- head       : 참고값만 출력(종합에서 제외) — 긴 머리카락·꼬리가 목을 가려 자동 추정이 틀린다(원본 0.96). 머리 크기는 비교 시트로 눈으로 본다.
- palette    : 주요 색 8개끼리 가장 가까운 색 거리 평균 — 색감
- detail     : sprite_metrics 지표(colors·edge·isolated·black) 차이 — 담백함·외곽선
- size       : 키·폭 비율 차
종합 = silhouette 40 + palette 20 + detail 25 + size 15.

사용: python Tools/pixelfx/sprite_similarity.py <target.png> <candidate.png> [<candidate2.png> ...] [--json]
"""
import json
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sprite_metrics import measure  # noqa: E402

WEIGHTS = {"silhouette": 40, "palette": 20, "detail": 25, "size": 15}


def load(path):
    a = np.array(Image.open(path).convert("RGBA")).astype(int)
    al = a[..., 3] > 128
    ys, xs = np.where(al)
    box = (xs.min(), ys.min(), xs.max() + 1, ys.max() + 1)
    return a, al, box


def silhouette(al_a, box_a, al_b, box_b, n=48):
    def norm(al, box):
        crop = Image.fromarray((al[box[1]:box[3], box[0]:box[2]] * 255).astype(np.uint8))
        return np.array(crop.resize((n, n), Image.NEAREST)) > 127
    A, B = norm(al_a, box_a), norm(al_b, box_b)
    inter = (A & B).sum()
    union = (A | B).sum()
    return 100.0 * inter / max(union, 1)


def head_ratio(al, box):
    widths = al.sum(1)
    top, bottom = box[1], box[3] - 1
    h = bottom - top + 1
    upper = range(top, top + int(h * 0.65) + 1)
    widest = max(upper, key=lambda y: widths[y])
    neck = bottom
    for y in range(widest, bottom + 1):
        if widths[y] < widths[widest] * 0.75:
            neck = y
            break
    return (neck - top) / h


def main_colors(a, al, k=8):
    px = a[..., :3][al]
    keys, counts = np.unique(px, axis=0, return_counts=True)
    order = np.argsort(-counts)
    return keys[order][:k].astype(float)


def palette(a1, al1, a2, al2):
    p, q = main_colors(a1, al1), main_colors(a2, al2)

    def side(x, y):
        return np.mean([np.min(np.linalg.norm(y - c, axis=1)) for c in x])
    d = (side(p, q) + side(q, p)) / 2  # 0(같음) ~ 441(최대)
    return max(0.0, 100.0 - d / 1.5)


def detail(m1, m2):
    diffs = [abs(m1["colors"] - m2["colors"]) / 20, abs(m1["edge"] - m2["edge"]) / 30,
             abs(m1["isolated"] - m2["isolated"]) / 25, abs(m1["black"] - m2["black"]) / 15]
    return max(0.0, 100.0 * (1 - sum(min(d, 1) for d in diffs) / len(diffs)))


def compare(target, cand):
    a1, al1, b1 = load(target)
    a2, al2, b2 = load(cand)
    h1, h2 = head_ratio(al1, b1), head_ratio(al2, b2)
    w1, w2 = b1[2] - b1[0], b2[2] - b2[0]
    hh1, hh2 = b1[3] - b1[1], b2[3] - b2[1]
    s = {
        "silhouette": silhouette(al1, b1, al2, b2),
        "head": max(0.0, 100.0 - abs(h1 - h2) * 250),
        "palette": palette(a1, al1, a2, al2),
        "detail": detail(measure(target), measure(cand)),
        "size": max(0.0, 100.0 - (abs(hh1 - hh2) / hh1 + abs(w1 - w2) / w1) * 100),
    }
    s["total"] = sum(s[k] * w for k, w in WEIGHTS.items()) / sum(WEIGHTS.values())
    s = {k: round(v, 1) for k, v in s.items()}
    s.update({"path": cand, "head_ratio": round(h2, 2), "target_head_ratio": round(h1, 2), "weakest": min(WEIGHTS, key=lambda k: s[k])})
    return s


def main():
    args = [x for x in sys.argv[1:] if not x.startswith("--")]
    target, cands = args[0], args[1:]
    rows = [compare(target, c) for c in cands]
    if "--json" in sys.argv:
        print(json.dumps(rows, ensure_ascii=False, indent=1))
        return
    for r in rows:
        print(f"{r['path']}\n  total {r['total']}  silhouette {r['silhouette']}  head {r['head']} ({r['head_ratio']} vs {r['target_head_ratio']})"
              f"  palette {r['palette']}  detail {r['detail']}  size {r['size']}  -> weakest: {r['weakest']}")


if __name__ == "__main__":
    main()
