"""같은 스켈레톤 프레임으로 연출 방식 여러 개를 비교하는 GIF(생성 0). 칼은 게임 규칙대로 손목에 붙인다.
사용: python Tools/pixelfx/style_test.py <프레임 폴더> <keys_v6.json> <동작> <출력 폴더>
A 보간형(전 프레임, 고른 완급) / B 데드셀식 포즈 대 포즈(키만, 준비 짧게·타격 유지) / C SLYNYRD 정석(준비 1·타격 직후 유지 180ms·복귀 2) / D B + 단순 초승달 스미어
"""
import json
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(__file__))
import cel_weapon_sheet as ws  # noqa: E402
from smear import thin_arc  # noqa: E402


FEET_Y = 54  # 게임 셀(build_cel) 발 높이


def frame(img, hand, ang, sword, smear=None):
    c = Image.new("RGBA", (96, 80), (60, 70, 60, 255))
    if smear is not None:
        c.alpha_composite(smear)
    big = Image.new("RGBA", (128, 128))
    wim, (pvx, pvy), _ = sword
    big.alpha_composite(wim, (int(round(64 - pvx)), int(round(64 - pvy))))
    rot = big.rotate(ang - 90, resample=Image.NEAREST, center=(64, 64))
    # 발 맞춤(10-28 사용자: 발 높이가 프레임마다 달라 보임) — 생성 그림은 1~3px 들쭉날쭉하므로 게임 셀처럼 맨 아래 픽셀을 FEET_Y 에 맞춘다
    dy = FEET_Y - (img.getbbox()[3] - 1)
    c.alpha_composite(img, (16, 8 + dy))
    c.paste(rot, (16 + hand[0] - 64, 8 + dy + hand[1] - 64), rot)
    return c.resize((96 * 4, 80 * 4), Image.NEAREST)


def main():
    src, kj, m, out = sys.argv[1:5]
    d = json.load(open(kj))[m]
    n = len(d["hand"])
    ims = [Image.open(os.path.join(src, f"{i}.png")).convert("RGBA") for i in range(n)]
    sword = ws.load_weapon("ShortSword")
    os.makedirs(out, exist_ok=True)
    main_n = d["main"]
    peak = max(range(main_n), key=lambda i: d["hand"][i][0])  # 손이 가장 앞
    styles = {
        "A_interp": [(i, 50) for i in range(n)],
        "B_pose": [(0, 60), (min(1, peak - 1), 60), (peak, 200), (n - 1, 100)],
        "C_slynyrd": [(max(0, peak - 2), 100), (peak - 1, 50), (peak, 180), (min(n - 1, peak + 2), 60), (n - 1, 100)],
    }
    for name, seq in styles.items():
        fr = [frame(ims[i], d["hand"][i], d["angle"][i], sword) for i, _ in seq]
        fr[0].save(f"{out}/{m}_{name}.gif", save_all=True, append_images=fr[1:], duration=[t for _, t in seq], loop=0)
    # D: B + 스미어(타격 프레임에 손목 궤적 호)
    hx, hy = d["hand"][peak]
    sm = thin_arc((96, 80), 16 + hx - 14, 8 + hy + 4, 18, -40, 60, width=3)
    seq = styles["B_pose"]
    fr = [frame(ims[i], d["hand"][i], d["angle"][i], sword, sm if i == peak else None) for i, _ in seq]
    fr[0].save(f"{out}/{m}_D_pose_smear.gif", save_all=True, append_images=fr[1:], duration=[t for _, t in seq], loop=0)
    print(out, "peak", peak)


if __name__ == "__main__":
    main()
