"""셀 + 장비 레이어 + 칼 + 주먹을 게임과 같은 규칙으로 합성해 동작별 시트로 본다(생성 0, 촬영 없이 전 프레임 점검).
칼 각도 규칙은 PixelRig.ApplyCel 과 맞춘다: 공격 셀은 cel.json angle, 땅 반복(대기·걷기·달리기)은 -20° + 손 x 흔들림,
그 밖엔 손이 엉덩이보다 7px 넘게 뒤면 210°(공중 JumpArc 는 220°→185°), 아니면 -20°.
사용: python Tools/pixelfx/cel_weapon_sheet.py <출력.png> [장비 id=iron_set] [클립,...]
"""
import json
import sys

from PIL import Image, ImageDraw

P = "Assets/Art/Player/PixelRig/"
HIP_X = 41
SWORD = "Assets/Resources/PixelRig/Weapons/ShortSword.png"
PIVOT_Y = 3.5  # 칼 스프라이트 아래에서 손잡이 가운데까지(px)


def blade_angle(clip, i, n, fr):
    if fr.get("angle", -999) > -998:
        return fr["angle"]
    hx = fr["hand"][0] - HIP_X
    if clip in ("Idle", "Walk", "Run"):
        return -20 + max(-10, min(10, (hx + 5) * 2.5))
    if hx < -7:
        if clip == "JumpArc":
            return 220 + (185 - 220) * (i / max(1, n - 1))
        return 210
    return -20


def frame(clip, i, d, eq, sw):
    fr = d[clip]["frames"][i]
    img = Image.new("RGBA", (96, 80), (60, 70, 60, 255))
    b = Image.open(f"{P}Cel/{clip}_{i}.png").convert("RGBA")
    try:
        er = Image.open(f"{P}Equip/{eq}/{clip}_{i}_erase.png").convert("L")
        al = b.getchannel("A")
        al.paste(0, mask=er)
        b.putalpha(al)
        img.alpha_composite(b, (16, 8))
        img.alpha_composite(Image.open(f"{P}Equip/{eq}/{clip}_{i}.png").convert("RGBA"), (16, 8))
    except FileNotFoundError:
        img.alpha_composite(b, (16, 8))
    if not clip.startswith("Rope") and clip != "Death":
        ang = blade_angle(clip, i, len(d[clip]["frames"]), fr)
        big = Image.new("RGBA", (64, 64))
        big.alpha_composite(sw, (29, int(round(32 - (sw.size[1] - PIVOT_Y)))))
        rot = big.rotate(ang - 90, resample=Image.NEAREST, center=(32.5, 32))
        hx, hy = fr["hand"]
        img.alpha_composite(rot, (16 + hx - 32, 8 + hy - 32))
        img.alpha_composite(Image.open(f"{P}Cel/{clip}_{i}_fist.png").convert("RGBA"), (16, 8))
    return img


def main():
    out = sys.argv[1]
    eq = sys.argv[2] if len(sys.argv) > 2 else "iron_set"
    d = json.load(open(P + "Cel/cel.json", encoding="utf-8"))
    clips = sys.argv[3].split(",") if len(sys.argv) > 3 else [c for c in d]
    sw = Image.open(SWORD).convert("RGBA")
    S = 3
    W = max(len(d[c]["frames"]) for c in clips)
    sheet = Image.new("RGBA", (96 * S * W + 120, 80 * S * len(clips)), (30, 30, 30, 255))
    dr = ImageDraw.Draw(sheet)
    for r, c in enumerate(clips):
        dr.text((4, r * 80 * S + 4), c, fill=(255, 255, 255, 255))
        for i in range(len(d[c]["frames"])):
            sheet.paste(frame(c, i, d, eq, sw).resize((96 * S, 80 * S), Image.NEAREST), (120 + i * 96 * S, r * 80 * S))
            dr.text((120 + i * 96 * S + 4, r * 80 * S + 4), str(i), fill=(255, 255, 0, 255))
    sheet.save(out)
    print(out)


if __name__ == "__main__":
    main()
