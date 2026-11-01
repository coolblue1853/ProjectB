"""던전 슬래셔 그림체 주인공(ds03) 셀 세트 만들기 (11-01, DungeonSlasherReference.md 확정 레시피).

PixelLab pixminimax 원본(Docs/ArtCompare/CharacterGen/ds01/...)을 Assets/Art/Player/PixelRig/CelDS/ 로 옮기고 cel.json 을 쓴다.
보간 없음(원작처럼 키 프레임만). 피벗 = (32, 60) — 발바닥 행 60, 몸 가운데 x 32. 무기는 그림에 그려져 있다(리그 무기 숨김).
PixelRigBuilder "Build Adventurer DS" 가 읽는다.
"""
import json
import os
import shutil
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "Docs/ArtCompare/CharacterGen/ds01")
DST = os.path.join(ROOT, "Assets/Art/Player/PixelRig/CelDS")
HAND = [40, 42]   # 앞손(그림 좌표, 무기는 숨기지만 손 소켓 이펙트 위치용)
HAND_B = [26, 42]

# 원본 묶음: 이름 → (폴더, 프레임 번호들)
SETS = {
    "idle": ("motions/idle", range(0, 8)),
    "walk": ("walk_6101", range(0, 8)),
    "run": ("motions/run", range(0, 8)),
    "crouch": ("motions/jump", [1]),
    "arc": ("motions/jump", [2, 3, 4]),
    "land": ("motions/jump", [5, 6, 7]),
    "hurt": ("motions/hurt", [1, 2, 3]),
    "death": ("motions/death", range(1, 9)),
    "stab": ("motions/stab", [1, 2, 3, 4]),
    "down": ("motions/down", [1, 2, 3, 4]),
    "up": ("motions/up", [1, 2, 3, 4]),
    "side": ("motions/sweep", [1, 2, 3, 4]),  # 11-01 수평 베기 새로(예전 atk2 1·2·3·7 은 7번이 어색)
    "stabend": ("motions/stab", [3]),  # 공격 끝 → 땅 동작 전환 1장(예전 StabEnd 처럼 전체를 다시 틀지 않게)
    "downend": ("motions/down", [3]),
    "airstab": ("motions/airstab", [1, 2, 3, 4]),  # 공중 평타(점프 꼭대기 프레임에서 시작, 다리 접음)
    "airdown": ("motions/airdown", [1, 2, 3, 4]),
    "airup": ("motions/airup", [1, 2, 3, 4]),
    "flurry": ("motions/flurry", range(1, 8)),
    "cross": ("motions/cross", [1, 2, 3, 4, 8]),
    "charge": ("motions/charge", range(1, 8)),
    "rope": ("motions/rope", range(3, 9)),
    "ropestay": ("motions/rope", [5]),
}

# 묶음별 재생: (frame 초, loop, 프레임별 시간 or None, 타격 프레임 -1 = 없음)
ATK = [0.07, 0.06, 0.09, 0.1]
PLAY = {
    "idle": (0.12, True, None, -1),
    "walk": (0.1, True, None, -1),
    "run": (0.08, True, None, -1),
    "crouch": (0.08, False, None, -1),
    "arc": (0.1, False, None, -1),
    "land": (0.06, False, None, -1),
    "hurt": (0.08, False, None, -1),
    "death": (0.1, False, None, -1),
    "stab": (0.083, False, ATK, 1),
    "down": (0.083, False, ATK, 2),
    "up": (0.083, False, ATK, 2),
    "side": (0.083, False, ATK, 2),
    "flurry": (0.06, True, None, 1),
    "cross": (0.083, False, [0.07, 0.06, 0.09, 0.09, 0.1], 2),
    "charge": (0.083, False, None, 3),
    "rope": (0.1, True, None, -1),
    "stabend": (0.08, False, None, -1),
    "downend": (0.08, False, None, -1),
    "airstab": (0.083, False, ATK, 1),
    "airdown": (0.083, False, ATK, 1),
    "airup": (0.083, False, ATK, 1),
    "ropestay": (0.1, True, None, -1),
}

# 리그 클립 이름 → 묶음(PixelRigBuilder.BuildCel 이름 목록 전부. 없는 동작은 가장 비슷한 묶음)
CLIPS = {
    "Idle": "idle", "Walk": "walk", "Run": "run", "JumpCrouch": "crouch", "JumpArc": "arc", "Land": "land",
    "Dash": "run", "Hurt": "hurt", "Death": "death", "Stab": "stab", "SwingDown": "down", "SwingUp": "up",
    "RopeUp": "rope", "RopeStay": "ropestay", "GreatDown": "down", "GreatSide": "side",
    "DaggerFlurry": "flurry", "DaggerCross": "cross", "AirSlash": "down", "Lunge": "stab", "Spin": "side",
    "Cast": "charge", "Throw": "stab", "Guard": "idle", "Charge": "charge", "LeapSlam": "down", "Backstep": "hurt",
    "Punch": "flurry", "Iaido": "side", "ShoulderAim": "stab", "Raise": "up", "Knockdown": "death", "Place": "crouch",
    "Kick": "stab", "Grab": "stab", "Ride": "idle", "StabEnd": "stabend", "SwingDownEnd": "downend", "Bow": "stab",
    "GreatSideOne": "side", "ShieldBash": "stab", "Slam": "down", "Pull": "flurry", "LowSweep": "down",
    "GreatUpper": "up", "DashSlash": "side", "WhipSnap": "down", "Roundhouse": "side", "DualPistol": "stab",
    "Rifle": "stab", "SwordPistol": "stab", "Sheathe": "idle", "Victory": "charge", "Dance": "idle", "Lute": "idle",
    "PalmBlast": "stab",
    "AirStab": "airstab", "AirSwingDown": "airdown", "AirSwingUp": "airup", "AirGreatDown": "airdown", "AirGreatSide": "airdown",
    "AirGreatSideOne": "airdown",
}


def clean(im, key):
    """피격 그림의 피 튀김(빨강) 지우기 — 초반은 소박하게"""
    if key != "hurt":
        return im
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a and r > 110 and g < 60 and b < 60:
                px[x, y] = (0, 0, 0, 0)
    return im


def main():
    if os.path.isdir(DST):
        for f in os.listdir(DST):
            if f.endswith(".png") or f == "cel.json":
                os.remove(os.path.join(DST, f))
    os.makedirs(DST, exist_ok=True)
    pngs = {}
    for key, (folder, idx) in SETS.items():
        names = []
        for n, i in enumerate(idx):
            im = Image.open(os.path.join(SRC, folder, "raw_%d.png" % i)).convert("RGBA")
            assert im.size == (64, 64), (folder, i, im.size)
            name = "ds_%s_%d" % (key, n)
            clean(im, key).save(os.path.join(DST, name + ".png"))
            names.append(name)
        pngs[key] = names
    out = {}
    for clip, key in CLIPS.items():
        frame, loop, times, strike = PLAY[key]
        frames = []
        for n, png in enumerate(pngs[key]):
            frames.append({"png": png, "hand": HAND, "handB": HAND_B, "time": times[n] if times else 0.0, "angle": -999.0})
        out[clip] = {"frame": frame, "loop": loop, "strike": strike, "frames": frames}
    with open(os.path.join(DST, "cel.json"), "w", encoding="utf-8") as f:
        json.dump(out, f, indent=1)
    print("clips", len(out), "pngs", sum(len(v) for v in pngs.values()))


if __name__ == "__main__":
    sys.exit(main())
