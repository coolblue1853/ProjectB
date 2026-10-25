"""캐릭터 도트 판정 지표 (.claude-rules/Game/CharacterPixelRules.md 3절).

투명 배경 스프라이트 PNG를 받아 기준 그림과 같은 방식으로 잰다.
- colors   : 불투명 픽셀의 색 수
- edge     : 이웃(오른쪽·아래) 픽셀과 색이 다른 비율 — 높을수록 잔무늬가 많다
- isolated : 상하좌우 이웃이 모두 다른 색인 외톨이 점 비율
- black    : 검정(합 < 90) 비율 — 외곽선 굵기·존재감
- height   : 불투명 영역 높이(px)
판정: IDEAL = 기준 그림 수준, OK = 사용자가 "적당하다"고 한 합성 시안(hero_mix_Bhair_B2body) 수준, 그 밖은 FIX.
(머리 비율은 배낭·꼬리 같은 소품 때문에 자동 추정이 틀려 지표에서 뺐다 — 비교 시트로 눈으로 본다.)

사용: python Tools/pixelfx/sprite_metrics.py a.png [b.png ...] [--json]
"""
import json
import sys

import numpy as np
from PIL import Image

# 기준 그림(Docs/ArtCompare/hero_style_anchor_64.png) 18색·43%·10%·24% → IDEAL
IDEAL = {"colors": (12, 20), "edge": (35, 48), "isolated": (0, 15), "black": (18, 32)}
# 사용자 합격 합성 시안(hero_mix_Bhair_B2body) 27색·52%·25%·21% → OK
OK = {"colors": (10, 28), "edge": (30, 55), "isolated": (0, 26), "black": (16, 34)}


def measure(path):
    a = np.array(Image.open(path).convert("RGBA")).astype(int)
    al = a[..., 3] > 128
    rgb = a[..., :3]
    n = int(al.sum())
    if n == 0:
        return {"path": path, "error": "empty"}
    keys = rgb[..., 0] * 65536 + rgb[..., 1] * 256 + rgb[..., 2]
    colors = len(np.unique(keys[al]))
    black = float(((rgb.sum(2) < 90) & al).sum()) / n
    pairs = diff = 0
    right = al[:, :-1] & al[:, 1:]
    down = al[:-1, :] & al[1:, :]
    pairs = int(right.sum() + down.sum())
    diff = int(((keys[:, :-1] != keys[:, 1:]) & right).sum() + ((keys[:-1, :] != keys[1:, :]) & down).sum())
    H, W = al.shape
    iso = 0
    for y in range(H):
        for x in range(W):
            if not al[y, x]:
                continue
            nb = [(y + dy, x + dx) for dy, dx in ((0, 1), (1, 0), (0, -1), (-1, 0))
                  if 0 <= y + dy < H and 0 <= x + dx < W and al[y + dy, x + dx]]
            if nb and all(keys[p] != keys[y, x] for p in nb):
                iso += 1
    ys = np.where(al.any(1))[0]
    return {
        "path": path,
        "colors": colors,
        "edge": round(100.0 * diff / max(pairs, 1), 1),
        "isolated": round(100.0 * iso / n, 1),
        "black": round(100.0 * black, 1),
        "height": int(ys.max() - ys.min() + 1),
    }


def _fails(m, rng):
    return [k for k, (lo, hi) in rng.items() if not (lo <= m[k] <= hi)]


def verdict(m):
    if not _fails(m, IDEAL):
        return "IDEAL"
    bad = _fails(m, OK)
    return "OK" if not bad else "FIX " + ",".join(bad)


def main():
    args = [x for x in sys.argv[1:] if not x.startswith("--")]
    rows = [measure(p) for p in args]
    if "--json" in sys.argv:
        print(json.dumps(rows, ensure_ascii=False, indent=1))
        return
    for m in rows:
        if "error" in m:
            print(m["path"], m["error"])
            continue
        print(f"{m['path']}\n  colors {m['colors']}  edge {m['edge']}%  isolated {m['isolated']}%  black {m['black']}%"
              f"  height {m['height']}px  -> {verdict(m)}")


if __name__ == "__main__":
    main()
