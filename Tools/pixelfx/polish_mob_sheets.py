"""몹 시트 도트 폴리싱 (ArtStyle.md 2절 3단계 '팔레트 감축(색 16 이하)' 보강).
사용: python Tools/pixelfx/polish_mob_sheets.py <몹 이름> [<몹 이름> ...] [--colors 16] [--dry]
- 알파 정리: 반투명 경계를 0/255 로 (반투명 날개 몹은 170 단계를 남긴다)
- 몹 단위 공용 팔레트: 모든 애니메이션 픽셀을 모아 k-means(멀리 떨어진 색부터 씨앗) → 눈·불꽃 같은 포인트 색이 묻히지 않는다
- 점 노이즈 정리: 주변 8칸 중 5칸 이상이 같은 색이고 그 색과 가까운 외톨이 픽셀만 바꾼다(하이라이트는 거리 조건으로 남김)
- 부스러기 제거: 프레임마다 3픽셀 이하로 떨어진 덩어리를 지운다(Wisp 처럼 입자가 디자인인 몹은 --keep-bits)
원본은 덮어쓴다. git 으로 되돌릴 수 있게 커밋 전에 비교 시트를 확인한다."""
import os
import sys
import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")
ANIMS = ["Idle", "Run", "Attack1", "Attack2", "Take Hit", "Death"]
SEMI_ALPHA = 170
NOISE_DIST = 70.0  # 이 거리보다 멀면 의도된 포인트 색으로 보고 남긴다
MIN_BIT = 3


def load_sheets(mob):
    sheets = {}
    for anim in ANIMS:
        path = os.path.join(ROOT, mob, anim + ".png")
        if os.path.exists(path):
            sheets[anim] = np.array(Image.open(path).convert("RGBA"), dtype=np.int32)
    return sheets


def clean_alpha(arr, keep_semi):
    # Death 시트는 프레임마다 전체 알파를 낮춰 사라지므로 프레임 최대 알파를 기준으로 정리한다
    fs = arr.shape[0]
    for i in range(arr.shape[1] // fs):
        a = arr[:, i * fs:(i + 1) * fs, 3]
        top = int(a.max())
        if top == 0:
            continue
        r = a / top
        if keep_semi:
            semi = (r >= 40 / 255) & (r < 220 / 255)
            a[r < 40 / 255] = 0
            a[semi] = SEMI_ALPHA * top // 255
            a[r >= 220 / 255] = top
        else:
            a[r < 0.5] = 0
            a[r >= 0.5] = top


def build_palette(sheets, k):
    px = np.concatenate([s[s[..., 3] > 0][:, :3] for s in sheets.values()]).astype(np.float64)
    uniq, counts = np.unique(px, axis=0, return_counts=True)
    if len(uniq) <= k:
        return uniq
    # 씨앗: 가장 흔한 색 → 이후 기존 씨앗에서 가장 먼 색(포인트 색 보존)
    seeds = [uniq[np.argmax(counts)]]
    d = np.linalg.norm(uniq - seeds[0], axis=1)
    while len(seeds) < k:
        seeds.append(uniq[np.argmax(d)])
        d = np.minimum(d, np.linalg.norm(uniq - seeds[-1], axis=1))
    centers = np.array(seeds)
    for _ in range(20):
        lab = np.argmin(((uniq[:, None, :] - centers[None]) ** 2).sum(-1), axis=1)
        for i in range(k):
            m = lab == i
            if m.any():
                centers[i] = (uniq[m] * counts[m, None]).sum(0) / counts[m].sum()
    return np.rint(centers)


def apply_palette(arr, pal):
    mask = arr[..., 3] > 0
    rgb = arr[mask][:, :3].astype(np.float64)
    idx = np.argmin(((rgb[:, None, :] - pal[None]) ** 2).sum(-1), axis=1)
    out = arr.copy()
    out[mask, :3] = pal[idx].astype(np.int32)
    return out


def despeckle(arr):
    h, w = arr.shape[:2]
    src = arr.copy()
    changed = 0
    for y in range(1, h - 1):
        for x in range(1, w - 1):
            if src[y, x, 3] == 0:
                continue
            c = tuple(src[y, x, :3])
            neigh = [tuple(src[y + dy, x + dx, :3]) for dy in (-1, 0, 1) for dx in (-1, 0, 1)
                     if (dy or dx) and src[y + dy, x + dx, 3] > 0]
            if len(neigh) < 8 or c in neigh:
                continue
            best = max(set(neigh), key=neigh.count)
            if neigh.count(best) >= 5 and np.linalg.norm(np.subtract(c, best)) < NOISE_DIST:
                arr[y, x, :3] = best
                changed += 1
    return changed


def drop_bits(arr, fs):
    h, w = arr.shape[:2]
    seen = np.zeros((h, w), bool)
    removed = 0
    for y in range(h):
        for x in range(w):
            if seen[y, x] or arr[y, x, 3] == 0:
                continue
            comp, stack = [], [(y, x)]
            seen[y, x] = True
            fx = x // fs
            while stack:
                cy, cx = stack.pop()
                comp.append((cy, cx))
                for dy in (-1, 0, 1):
                    for dx in (-1, 0, 1):
                        ny, nx = cy + dy, cx + dx
                        if 0 <= ny < h and fx * fs <= nx < (fx + 1) * fs and not seen[ny, nx] and arr[ny, nx, 3] > 0:
                            seen[ny, nx] = True
                            stack.append((ny, nx))
            if len(comp) <= MIN_BIT:
                for cy, cx in comp:
                    arr[cy, cx, 3] = 0
                removed += len(comp)
    return removed


def polish(mob, k, dry, keep_bits):
    sheets = load_sheets(mob)
    idle = sheets.get("Idle")
    keep_semi = idle is not None and ((idle[..., 3] > 40) & (idle[..., 3] < 220)).sum() > 50
    for s in sheets.values():
        clean_alpha(s, keep_semi)
    before = len(np.unique(np.concatenate([s[s[..., 3] > 0][:, :3] for s in sheets.values()]), axis=0))
    pal = build_palette(sheets, k)
    stats = []
    for anim, s in sheets.items():
        q = apply_palette(s, pal)
        n_sp = despeckle(q)
        n_bit = 0 if keep_bits else drop_bits(q, s.shape[0])
        stats.append(f"{anim}:speck{n_sp}/bit{n_bit}")
        if not dry:
            Image.fromarray(q.astype(np.uint8), "RGBA").save(os.path.join(ROOT, mob, anim + ".png"))
    print(f"{mob}: {before}색 -> {len(pal)}색 semi={keep_semi} " + " ".join(stats))


def main():
    args = sys.argv[1:]
    k = int(args[args.index("--colors") + 1]) if "--colors" in args else 16
    dry = "--dry" in args
    keep_bits = "--keep-bits" in args
    mobs = [a for i, a in enumerate(args) if not a.startswith("--") and (i == 0 or args[i - 1] != "--colors")]
    for mob in mobs:
        polish(mob, k, dry, keep_bits)


if __name__ == "__main__":
    main()
