"""무기 = 직업(10-29 사용자). 무기마다 요구 동작 2~4개가 다른 무기와 똑같은 세트인지(다양성) + 합친 동작을 재생 프로파일로 바꾼 뒤의 세트.
사용: python Tools/motioncov/weapon_diversity.py
"""
import collections
import io
import os
import sys

# stdout UTF-8 은 merge_check 가 설정
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from data import W  # noqa: E402
from merge_check import MERGE  # noqa: E402

sig = collections.defaultdict(list)
for w, (cat, m, t) in W.items():
    s = tuple(sorted({MERGE.get(x, x) for x in m.split()}))
    sig[s].append(w)
same = [(s, ws) for s, ws in sig.items() if len(ws) > 1]
print(f"무기 {len(W)}종, 서로 다른 동작 세트 {len(sig)}개, 세트를 공유하는 무기 묶음 {len(same)}개")
for s, ws in sorted(same, key=lambda x: -len(x[1]))[:25]:
    print(f"{len(ws):2d} | {' '.join(s)} | {', '.join(ws)}")
