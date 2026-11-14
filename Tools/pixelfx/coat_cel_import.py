"""기본 코트 새 셀(11-14) → CelDS 통짜 의상 셀로 가져오기.

원본: Docs/ArtCompare/Motion/coat_basic/<폴더>/c1~cN.png (64px, 빈손, 이펙트 제거, c0 = 대기)
      재사용은 Assets/Art/Player/PixelRig/EquipDS/base_coat/<이름>.png (코트 기준 그림)
결과: Assets/Art/Player/PixelRig/CelDS/cb_<키>_<n>.png + cel.json 항목 추가·교체("outfit": true)
 - outfit = 의상 한 벌 통짜 그림. 리그가 장비 레이어(g2_export 부위 분리)를 합성하지 않는다.
 - 무기 손·각도 채널은 coat_cel_hands.py 표로 넣는다(가져온 뒤 자동 적용, 피격·대시는 숨김 유지).
 - cel.json 의 다른 항목(기본 칼 3타·이동·점프 등)은 건드리지 않는다.
다음: Unity 메뉴 Tools/ProjectB/Pixel Rig/Build Adventurer DS.
사용: python Tools/pixelfx/coat_cel_import.py
"""
import json
import os
import shutil
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from coat_cel_hands import apply  # noqa: E402  무기 손·각도·주먹(11-14)

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "Docs/ArtCompare/Motion/coat_basic")
BASE = os.path.join(ROOT, "Assets/Art/Player/PixelRig/EquipDS/base_coat")
DST = os.path.join(ROOT, "Assets/Art/Player/PixelRig/CelDS")
HAND = [40, 42]
HAND_B = [26, 42]
NO_ANGLE = -999.0
A4 = [0.1, 0.07, 0.1, 0.15]   # 준비 → 빠른 전환 → 타격 유지(완급)


def c(folder, *idx):
    return [(folder, i) for i in idx]


def b(*names):
    return [("@base", n) for n in names]


# 키: (원본 프레임, 클립 이름들, frame 초, 프레임별 시간 or None, 타격 프레임, loop, 상태)
# 상태: 합격 = 사용자 합격, 임시 = 판정 대기(미검증)
SETS = {
    "hurt": (c("hurt_g2", 1, 2, 3), ["Hurt"], 0.075, None, -1, False, "합격"),
    "dash": (c("dash_g2", 1, 2, 3, 4), ["Dash"], 0.05, None, -1, False, "합격"),
    "hslash": (c("hslash_g4", 1, 2, 3, 4), ["CoatHSlash"], 0.083, A4, 2, False, "합격"),
    "airstab": (c("airstab_g1", 1, 2, 3, 4), ["AirStab"], 0.083, [0.07, 0.06, 0.09, 0.1], 1, False, "임시"),
    "airup": (c("airup_g1", 1, 2, 3, 4), ["AirSwingUp"], 0.083, [0.07, 0.06, 0.09, 0.1], 1, False, "임시"),
    "airdown": (c("airdown_g2", 2, 3, 4), ["AirSwingDown"], 0.083, [0.08, 0.09, 0.12], 1, False, "임시"),
    "xslash": (c("cross_g3", 1, 2, 3, 4), ["CoatXSlash"], 0.083, A4, 2, False, "임시"),
    "flurry": (c("flurry_g1", 1, 2, 3, 4, 5, 6, 7, 8), ["CoatFlurry"], 0.05, None, 1, False, "임시"),
    "smash": (c("smash_g1", 1, 2, 3, 4), ["CoatSmash"], 0.083, [0.12, 0.07, 0.1, 0.16], 2, False, "임시"),
    "bow": (c("bow_g2", 1, 2, 3, 4), ["CoatBow"], 0.083, A4, 2, False, "임시"),
    "throw": (c("throw_g2", 1, 2, 3, 4), ["CoatThrow"], 0.083, A4, 2, False, "임시"),
    "one": (c("onetwo_g1", 1, 2), ["CoatOne"], 0.083, [0.1, 0.16], 0, False, "임시"),
    "two": (c("onetwo_g1", 3, 4), ["CoatTwo"], 0.083, [0.08, 0.18], 1, False, "임시"),
    "whip": (c("whip_use", 1, 2, 3), ["CoatWhip"], 0.083, [0.12, 0.08, 0.18], 1, False, "임시"),
    "claw1": (c("claw_g1", 1, 2), ["CoatClaw1"], 0.083, [0.1, 0.16], 0, False, "임시"),
    "claw2": (c("claw_g1", 3, 4), ["CoatClaw2"], 0.083, [0.1, 0.18], 1, False, "임시"),
    "cast": (c("cast_g1", 1, 2, 3, 4), ["CoatCast"], 0.083, A4, 2, False, "임시"),
    "dashstab": (c("dashstab_g1", 1, 2, 3, 4), ["CoatDashStab"], 0.083, A4, 2, False, "임시"),
    "lowsweep": (c("lowsweep_g1", 1, 2, 3, 4), ["CoatLowSweep"], 0.083, A4, 2, False, "임시"),
    "backstep": (c("backstep_g1", 1, 2, 3, 4), ["CoatBackstep"], 0.07, None, -1, False, "임시"),
    "roar": (c("roar_use", 1, 2, 3), ["CoatRoar"], 0.083, [0.1, 0.1, 0.3], 2, False, "임시"),
    "charge": (c("charge_g1", 1, 2, 3, 4), ["CoatCharge"], 0.083, None, 3, False, "임시"),
    "bowlob": (c("bowlob_g1", 1, 2, 3, 4), ["CoatBowLob"], 0.083, A4, 2, False, "임시"),
    "shbash": (c("shbash_g1", 1, 2, 3, 4), ["CoatShBash"], 0.083, A4, 2, False, "임시"),
    "shblock": (c("shblock_g1", 1, 2, 3, 4), ["CoatShBlock"], 0.083, None, -1, False, "임시"),
    "spin": (c("spin_g1", 1, 2, 3, 4), ["CoatSpin"], 0.083, [0.08, 0.07, 0.07, 0.14], 2, False, "임시"),
    # 재사용: 양손 올려치기 = 코트 올려베기 그림, 점프 내려찍기 = 점프 + 공중 내려베기 + 내려찍기 3~4칸
    "upper": (b("ds_up_0", "ds_up_1", "ds_up_2", "ds_up_3", "ds_up_4"), ["CoatUpper"], 0.083, [0.1, 0.07, 0.07, 0.08, 0.3], 3, False, "임시"),
    "leapslam": (b("ds_arc_0", "ds_arc_1") + c("airdown_g2", 2, 3, 4) + c("smash_g1", 3, 4), ["CoatLeapSlam"], 0.083,
                 [0.08, 0.08, 0.06, 0.06, 0.06, 0.1, 0.2], 5, False, "임시"),
}


def src_path(folder, i):
    if folder == "@base":
        return os.path.join(BASE, i + ".png")
    return os.path.join(SRC, folder, "c%d.png" % i)


def main():
    from PIL import Image
    cel_path = os.path.join(DST, "cel.json")
    with open(cel_path, encoding="utf-8") as f:
        cel = json.load(f)
    for key, (frames, clips, frame, times, strike, loop, _) in SETS.items():
        out = []
        for n, (folder, i) in enumerate(frames):
            p = src_path(folder, i)
            im = Image.open(p)
            if im.size != (64, 64):
                raise ValueError("64px 아님: %s %s" % (p, im.size))
            name = "cb_%s_%d" % (key, n)
            shutil.copyfile(p, os.path.join(DST, name + ".png"))
            out.append({"png": name, "hand": HAND, "handB": HAND_B, "time": times[n] if times else 0.0, "angle": NO_ANGLE})
        for clip in clips:
            cel[clip] = {"frame": frame, "loop": loop, "strike": strike, "outfit": True, "frames": out}
    print("hand clips", apply(cel))
    with open(cel_path, "w", encoding="utf-8") as f:
        json.dump(cel, f, indent=1)
    print("clips", len(cel), "coat sets", len(SETS))


if __name__ == "__main__":
    sys.exit(main())
