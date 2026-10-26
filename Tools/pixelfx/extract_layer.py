"""장비 레이어 추출(CelEquipmentPlan B안 시험): 맨몸 셀 프레임과 장비 입은 프레임(transfer_outfit 결과)의 픽셀 차이 → 덮어쓰기 레이어.

레이어 = 장비 프레임에서 맨몸과 색이 다른(또는 맨몸엔 없던) 불투명 픽셀. 맨몸 위에 겹치면 장비 프레임이 되어야 한다.
수치: layer(잡티 걸러 낸 레이어 픽셀), erase(맨몸에서 지울 마스크 — <i>_erase.png), mismatch(맨몸−마스크+레이어 와 장비 프레임 색 차이 — 전이 중 몸을 다시 그린 차이, 몸은 맨몸 그대로 쓰는 게 의도), extra(튀어나온 픽셀).
사용: python Tools/pixelfx/extract_layer.py <맨몸 폴더> <이름 패턴(예 Walk_{})> <장비 폴더> <프레임 수> <출력 폴더> [색 허용 오차=24]
"""
import os
import sys

import numpy as np
from PIL import Image


MIN_PART = 8  # 이보다 작은 덩어리는 장비가 아니라 전이 중 몸을 미세하게 다시 그린 잡티(다리·팔·스카프)


def clean(mask, min_size):
    """상하좌우로 이어진 덩어리 중 min_size 픽셀 미만은 버린다"""
    h, w = mask.shape
    seen = np.zeros_like(mask)
    out = np.zeros_like(mask)
    for y in range(h):
        for x in range(w):
            if not mask[y, x] or seen[y, x]:
                continue
            stack, part = [(y, x)], []
            seen[y, x] = True
            while stack:
                cy, cx = stack.pop()
                part.append((cy, cx))
                for ny, nx in ((cy + 1, cx), (cy - 1, cx), (cy, cx + 1), (cy, cx - 1)):
                    if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = True
                        stack.append((ny, nx))
            if len(part) >= min_size:
                for cy, cx in part:
                    out[cy, cx] = True
    return out


def main():
    base_dir, pattern, eq_dir, n, out = sys.argv[1], sys.argv[2], sys.argv[3], int(sys.argv[4]), sys.argv[5]
    tol = int(sys.argv[6]) if len(sys.argv) > 6 else 24
    os.makedirs(out, exist_ok=True)
    for i in range(n):
        b = np.array(Image.open(os.path.join(base_dir, pattern.format(i) + ".png")).convert("RGBA")).astype(int)
        e = np.array(Image.open(os.path.join(eq_dir, f"{i}.png")).convert("RGBA")).astype(int)
        bo, eo = b[..., 3] > 0, e[..., 3] > 0
        differ = np.abs(b[..., :3] - e[..., :3]).sum(axis=2) > tol
        layer_mask = clean(eo & (~bo | differ), MIN_PART)
        layer = np.zeros_like(e)
        layer[layer_mask] = e[layer_mask]
        layer[..., 3] = np.where(layer_mask, 255, 0)
        Image.fromarray(layer.astype(np.uint8), "RGBA").save(os.path.join(out, f"{i}.png"))
        erase = clean(bo & ~eo, 2)  # 맨몸에선 지워야 하는 픽셀(투구 밖으로 삐져나오는 머리카락 등) — 레이어와 함께 저장
        Image.fromarray(np.where(erase, 255, 0).astype(np.uint8), "L").save(os.path.join(out, f"{i}_erase.png"))
        comp = b.copy()
        comp[erase] = 0
        comp[layer_mask] = layer[layer_mask]
        removed = int(erase.sum())
        mism = int((np.abs(comp[eo][:, :3] - e[eo][:, :3]).sum(axis=1) > tol).sum())
        extra = int(((comp[..., 3] > 0) & ~eo).sum())  # 지운 뒤에도 장비 프레임보다 튀어나온 픽셀
        print(f"frame {i}: layer {int(layer_mask.sum()):4d} erase {removed:3d} mismatch(body kept) {mism:3d} extra {extra:2d}")


if __name__ == "__main__":
    main()
