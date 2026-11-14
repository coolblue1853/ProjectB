"""의상 한 벌 통짜 셀(11-14 밤 PixelLab) → CelDS/Outfits/<의상>/ 로 가져오기(11-15).

원본: Docs/ArtCompare/Motion/outfit/<의상>/{idle,walk,run,jump}/f0~f8.png (f0 = 기준 그림, 셀은 f1~f8)
      Docs/ArtCompare/Motion/outfit/<의상>/attack/<코트 셀 이름>.png (136장, 쥐는 없음)
결과: Assets/Art/Player/PixelRig/CelDS/Outfits/<의상>/<코트 셀 PNG 이름>.png
 - 파일 이름 = 코트 셀 PNG 이름(ds_idle_0, cb_hslash_2 …). 리그(PixelRig.SetEquipmentLayers + OutfitCels.Swap)가
   클립 프레임 그림이 전부 있으면 의상 그림으로, 하나라도 없으면 코트 셀로 폴백한다.
 - 타이밍·타격 프레임·손·각도·주먹은 코트 cel.json 값을 그대로 쓴다(cel.json 은 건드리지 않는다).
 - 점프는 8칸(웅크림→도약→공중→착지)을 코트 ds_crouch 1·ds_arc 3·ds_land 3 자리에 의상별 표(JUMP)로 나눈다.
 - ds_stabend_0·ds_downend_0(3타 회복 1칸)은 의상 그림이 없어 찌르기·내려베기 마지막 칸으로 대신한다.
 - 원본 코트 셀(CelDS 루트)은 그대로 둔다.
다음: Unity 메뉴 Tools/ProjectB/Pixel Rig/Build Adventurer DS (Outfits 폴더를 읽어 _outfitCels 로 굽는다).
사용: python Tools/pixelfx/outfit_cel_import.py
"""
import os
import shutil
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "Docs/ArtCompare/Motion/outfit")
DST = os.path.join(ROOT, "Assets/Art/Player/PixelRig/CelDS/Outfits")
OUTFITS = ["scarecrow", "leaf", "wisp", "wolf", "rat"]
LOOPS = {"idle": "ds_idle", "walk": "ds_walk", "run": "ds_run"}
# 점프 8칸 중 코트 자리(crouch 1, arc 3 = 오름·꼭대기·내림, land 3 = 눌림→일어섬). 의상마다 공중 칸이 달라 그림 바닥선으로 골랐다
JUMP = {
    "scarecrow": ([2], [4, 5, 6], [7, 8, 8]),
    "leaf": ([1], [3, 4, 5], [6, 7, 8]),
    "wisp": ([2], [4, 5, 6], [7, 8, 8]),
    "wolf": ([2], [3, 4, 5], [6, 7, 8]),
    "rat": ([2], [4, 6, 7], [8, 8, 8]),
}
ALIAS = {"ds_stabend_0": "ds_stab_3", "ds_downend_0": "ds_down_3"}


def copy(src, dst_dir, name):
    from PIL import Image
    im = Image.open(src)
    if im.size != (64, 64):
        raise ValueError("64px 아님: %s %s" % (src, im.size))
    shutil.copyfile(src, os.path.join(dst_dir, name + ".png"))


def main():
    for o in OUTFITS:
        src = os.path.join(SRC, o)
        out = os.path.join(DST, o)
        os.makedirs(out, exist_ok=True)
        n = 0
        for folder, prefix in LOOPS.items():
            for i in range(8):
                copy(os.path.join(src, folder, "f%d.png" % (i + 1)), out, "%s_%d" % (prefix, i))
                n += 1
        crouch, arc, land = JUMP[o]
        for prefix, idx in (("ds_crouch", crouch), ("ds_arc", arc), ("ds_land", land)):
            for k, f in enumerate(idx):
                copy(os.path.join(src, "jump", "f%d.png" % f), out, "%s_%d" % (prefix, k))
                n += 1
        attack = os.path.join(src, "attack")
        if os.path.isdir(attack):
            for f in sorted(os.listdir(attack)):
                if f.endswith(".png") and not f.endswith("_fist.png"):
                    copy(os.path.join(attack, f), out, f[:-4])
                    n += 1
            for name, frm in ALIAS.items():
                copy(os.path.join(attack, frm + ".png"), out, name)
                n += 1
        print(o, n)


if __name__ == "__main__":
    sys.exit(main())
