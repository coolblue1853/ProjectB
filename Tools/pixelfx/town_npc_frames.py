"""마을 NPC 2프레임 숨쉬기 시트 + 건물 정리(바닥선 크롭).
NPC: 64 폭 그대로, 발끝(bbox 아래)까지 자르고 [기본, 상체 1px 내림] 2칸 가로 시트(128 x H).
건물: bbox 로 자른다(바닥선 = 아래 끝).
사용: python Tools/pixelfx/town_npc_frames.py <src_dir> <dst_dir>
"""
import sys
from pathlib import Path
from PIL import Image

NPCS = ["merchant", "weaponsmith", "armorsmith", "elder"]
BUILDINGS = {"raw_forge": "forge", "raw_stall_empty": "stall"}
LEG_ROWS = 7  # 발에서 위로 이 줄까지는 고정(다리)


def npc_sheet(src: Path, dst: Path) -> None:
    im = Image.open(src).convert("RGBA")
    bottom = im.getbbox()[3]
    base = im.crop((0, 0, 64, bottom))
    h = bottom
    split = h - LEG_ROWS
    breath = base.copy()
    upper = base.crop((0, 0, 64, split))
    breath.paste((0, 0, 0, 0), (0, 0, 64, split))
    breath.alpha_composite(upper, (0, 1))
    sheet = Image.new("RGBA", (128, h))
    sheet.alpha_composite(base, (0, 0))
    sheet.alpha_composite(breath, (64, 0))
    sheet.save(dst)
    print(dst, sheet.size)


def main() -> None:
    src, dst = Path(sys.argv[1]), Path(sys.argv[2])
    for n in NPCS:
        npc_sheet(src / f"{n}.png", dst / f"Npc_{n}.png")
    for raw, name in BUILDINGS.items():
        im = Image.open(src / f"{raw}.png").convert("RGBA")
        out = im.crop(im.getbbox())
        out.save(dst / f"Bld_{name}.png")
        print(name, out.size)


if __name__ == "__main__":
    main()
