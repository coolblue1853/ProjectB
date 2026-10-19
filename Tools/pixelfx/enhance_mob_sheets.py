"""기존 몹 시트 강화 패스 (사용자 2026-10-19: 새로 만들지 말고 기존 몹·캐릭터 강화, 어비스 감성).
디자인은 그대로 두고 읽기 쉬움·입체감·색 통일만 올린다.
사용: python Tools/pixelfx/enhance_mob_sheets.py [--preview <out.png>] <몹 이름> ...   (--all 이면 Assets/Art/Monsters 전체)
1) 색 외곽선(sel-out): 실루엣 바깥 1px 을 '인접한 안쪽 색을 어둡게·보라 쪽으로' 칠한다. 검정 테두리 금지(ArtStyle).
   안쪽 가장자리가 이미 충분히 어두우면(기존 외곽선) 건너뛴다.
2) 림 라이트: 위·왼쪽이 비어 있는 가장자리 픽셀을 한 단계 밝고 따뜻하게(좌상단 광원).
3) 색조 통일: 어두운 면은 라벤더 쪽, 밝은 면은 호박 쪽으로 살짝(AbyssGrade 와 같은 방향).
같은 시트를 두 번 돌리면 외곽선이 겹치므로 원본에서 한 번만 실행한다(재실행 방지 표식: 시트 (0,0) 픽셀 미사용 확인 대신 git 기준)."""
import os
import sys
import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")
ANIMS = ["Idle", "Run", "Attack1", "Attack2", "Take Hit", "Death"]
LAV = np.array([92, 78, 132], float)
WARM = np.array([255, 232, 196], float)
OUTLINE_SKIP_LUM = 58  # 안쪽 가장자리가 이보다 어두우면 이미 외곽선이 있다
DARKEN = 0.42
CONTRAST = 1.2
RIM = 1.25


def lum(c):
    return 0.299 * c[..., 0] + 0.587 * c[..., 1] + 0.114 * c[..., 2]


def enhance_frame(fr):
    rgb = fr[..., :3].astype(float)
    a = fr[..., 3]
    op = a > 0
    h, w = op.shape
    L = lum(rgb)
    out = rgb.copy()
    # 0) 명암 대비 확장: 탁한 GPT 질감을 또렷한 면으로 (몹 평균 밝기 기준)
    if op.any():
        m = L[op].mean()
        scale = (m + (L - m) * CONTRAST) / np.maximum(L, 1)
        out = np.where(op[..., None], np.clip(out * scale[..., None], 0, 255), out)
        L = lum(out)
    # 3) 색조 통일 (먼저, 외곽선 색이 이 결과를 따르도록)
    dark = op & (L < 90)
    bright = op & (L > 170)
    out[dark] = out[dark] * 0.86 + LAV * 0.14
    out[bright] = out[bright] * 0.9 + WARM * 0.1
    pad = np.pad(op, 1)
    up, down = pad[:-2, 1:-1], pad[2:, 1:-1]
    left, right = pad[1:-1, :-2], pad[1:-1, 2:]
    # 2) 림 라이트: 위나 왼쪽이 빈 가장자리
    rim = op & (~up | ~left) & (L > 40)
    out[rim] = np.minimum(255, out[rim] * RIM + WARM * 0.1)
    # 2-1) 그림자 쪽 안쪽 1px: 아래·오른쪽이 빈 가장자리를 어둡게 → 부피감
    shade = op & (~down | ~right) & ~rim
    out[shade] = out[shade] * 0.78 + LAV * 0.08
    # 1) 색 외곽선: 빈 칸 중 4방향에 불투명 이웃이 있는 곳
    res = np.zeros_like(fr)
    res[..., :3] = np.clip(out, 0, 255).astype(np.uint8)
    res[..., 3] = a
    empty = ~op
    for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        src_y = np.clip(np.arange(h)[:, None] + dy, 0, h - 1)
        src_x = np.clip(np.arange(w)[None, :] + dx, 0, w - 1)
        nb_op = op[src_y, src_x]
        valid = (0 <= np.arange(h)[:, None] + dy) & (np.arange(h)[:, None] + dy < h) & \
                (0 <= np.arange(w)[None, :] + dx) & (np.arange(w)[None, :] + dx < w)
        cand = empty & nb_op & valid & (res[..., 3] == 0)
        nb_rgb = out[src_y, src_x]
        nb_L = L[src_y, src_x]
        cand &= nb_L > OUTLINE_SKIP_LUM
        col = nb_rgb * DARKEN * 0.7 + LAV * 0.3 * DARKEN * 1.4
        res[cand, :3] = np.clip(col[cand], 0, 255).astype(np.uint8)
        res[cand, 3] = a[src_y, src_x][cand]
    return res


def enhance_sheet(arr):
    fs = arr.shape[0]
    out = arr.copy()
    for i in range(arr.shape[1] // fs):
        out[:, i * fs:(i + 1) * fs] = enhance_frame(arr[:, i * fs:(i + 1) * fs])
    return out


def mob_names():
    return sorted(d for d in os.listdir(ROOT) if os.path.isdir(os.path.join(ROOT, d)))


def main():
    args = sys.argv[1:]
    preview = None
    if "--preview" in args:
        i = args.index("--preview"); preview = args[i + 1]; del args[i:i + 2]
    mobs = mob_names() if "--all" in args else [a for a in args if not a.startswith("--")]
    rows = []
    for mob in mobs:
        for anim in ANIMS:
            p = os.path.join(ROOT, mob, anim + ".png")
            if not os.path.exists(p):
                continue
            arr = np.array(Image.open(p).convert("RGBA"))
            res = enhance_sheet(arr)
            if preview:
                if anim == "Idle":
                    rows.append((mob, arr, res))
            else:
                Image.fromarray(res, "RGBA").save(p)
        if not preview:
            print("enhanced", mob)
    if preview:
        S = 3
        W = max(r[1].shape[0] for r in rows) * S
        sheet = Image.new("RGBA", (W * 2 + 30, sum(r[1].shape[0] * S + 6 for r in rows)), (70, 66, 92, 255))
        y = 0
        for mob, a, b in rows:
            fs = a.shape[0]
            for j, arr in enumerate((a, b)):
                fr = Image.fromarray(arr[:, :fs], "RGBA").resize((fs * S, fs * S), Image.NEAREST)
                sheet.alpha_composite(fr, (j * (W + 30), y))
            y += fs * S + 6
        sheet.save(preview)
        print("preview", preview)


if __name__ == "__main__":
    main()
