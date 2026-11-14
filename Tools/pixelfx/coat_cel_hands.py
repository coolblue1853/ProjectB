"""기본 코트 통짜 셀(cb_*)에 무기 손·각도 채널 넣기 (11-14, TechDebt "기본 코트 통짜 셀 연결 남은 것").

coat_cel_import.py 가 넣은 cb_* 셀은 angle -999(무기 숨김)라 평타·스킬 중 무기가 안 보였다.
빈손 3타(build_cel_ds.py HAND_OVERRIDE)와 같은 방식으로 프레임마다
 - hand  = 무기 쥔 주먹 그림 좌표(살색 덩어리 무게중심 + y 1px, 칼이 손 위로 떠 보이지 않게)
 - angle = 무기가 향하는 수학 각도(0 = 앞(오른쪽), 90 = 위, 270 = 아래)
 - <png>_fist.png = 주먹만 남긴 그림(무기 위에 한 번 더 그림: 몸 < 무기 < 주먹)
을 넣는다. 방패·너클·활·지팡이(무기 메타 upright)는 리그가 각도와 상관없이 세워 그리므로 손 좌표만 의미가 있다.
손 = 캐릭터 오른손(앞쪽 팔) 하나로 끝까지(weapon-hand-rule). 좌표는 피부색 덩어리 자동 탐지 후 5배 시트로 확인해 고쳤다.
무기 숨김 유지: 피격(Hurt)·대시(Dash) — 기존 ds 셀도 숨겼다.
CelDS 의 다른 셀·cel.json 의 다른 클립(기본 칼 3타·이동·점프)은 건드리지 않는다. build_cel_ds.py 를 다시 돌리지 않는다.
다음: Unity 메뉴 Tools/ProjectB/Pixel Rig/Build Adventurer DS.
사용: python Tools/pixelfx/coat_cel_hands.py   (coat_cel_import.py 도 가져오기 끝에 이 표를 적용한다)
"""
import json
import os
import sys

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from build_cel_ds import make_fist  # noqa: E402  주먹 추출(살색 + 둘레 어두운 외곽선)은 빈손 3타와 같은 함수(공용 모듈로 옮기는 건 TechDebt)

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DST = os.path.join(ROOT, "Assets/Art/Player/PixelRig/CelDS")
GRIP_DY = 1  # 무게중심보다 1px 아래(빈손 3타와 같음)
HIDE = ("hurt", "dash")  # 무기 숨김 유지(기존 ds 셀도 숨김)
CARRY = 340.0  # 버프·물러나기처럼 무기를 쓰지 않는 동작: 대기처럼 앞·아래로 늘어뜨림(PixelRig CelBladeAngle -20)

# 묶음 키(coat_cel_import SETS, 셀 이름 <접두>_<키>_<n> 의 키) → 프레임별 (주먹 무게중심 [x, y], 각도)
HANDS = {
    "hslash": [([10, 31], 170.0), ([11, 29], 165.0), ([54, 29], 10.0), ([51, 28], 0.0)],  # 뒤로 감았다 앞으로
    "airstab": [([56, 23], 0.0), ([56, 23], 0.0), ([55, 22], 0.0), ([54, 22], 0.0)],
    "airup": [([46, 39], 300.0), ([55, 21], 20.0), ([46, 5], 75.0), ([40, 3], 100.0)],
    "airdown": [([44, 8], 120.0), ([50, 38], 315.0), ([49, 37], 300.0)],
    "xslash": [([12, 14], 135.0), ([53, 40], 330.0), ([53, 40], 330.0), ([53, 40], 330.0)],  # 양손 머리 위 → 앞 아래 교차
    "flurry": [([41, 31], 0.0), ([53, 30], 0.0), ([42, 31], 0.0)] * 2 + [([41, 31], 0.0), ([53, 30], 0.0)],
    "smash": [([14, 12], 135.0), ([49, 48], 345.0), ([48, 46], 345.0), ([47, 44], 340.0)],  # 양손 머리 뒤 → 앞 아래
    "bow": [([54, 26], 0.0), ([50, 26], 0.0), ([49, 26], 0.0), ([47, 26], 0.0)],  # 앞으로 뻗은 활 손
    "throw": [([8, 18], 150.0), ([6, 20], 150.0), ([58, 27], 0.0), ([56, 33], 350.0)],
    "one": [([55, 26], 0.0), ([38, 25], 0.0)],
    "two": [([35, 26], 0.0), ([55, 28], 0.0)],
    "whip": [([8, 18], 120.0), ([54, 36], 330.0), ([51, 29], 0.0)],
    "claw1": [([54, 46], 330.0), ([47, 44], 330.0)],
    "claw2": [([42, 17], 70.0), ([56, 43], 330.0)],
    "cast": [([36, 28], 70.0), ([35, 27], 70.0), ([53, 24], 10.0), ([53, 24], 10.0)],  # 가슴 앞에서 들었다 앞으로 뻗음
    "dashstab": [([38, 30], 0.0), ([58, 30], 0.0), ([56, 30], 0.0), ([55, 30], 0.0)],
    "lowsweep": [([14, 38], 200.0), ([11, 41], 195.0), ([46, 47], 320.0), ([52, 58], 10.0)],  # 3칸은 손이 배낭에 가려 몸 앞 아래로 추정
    "backstep": [([44, 34], CARRY), ([43, 30], CARRY), ([42, 31], CARRY), ([43, 37], CARRY)],
    "roar": [([45, 32], CARRY), ([44, 32], CARRY), ([43, 24], 80.0)],
    "charge": [([46, 35], CARRY), ([46, 36], CARRY), ([46, 37], CARRY), ([46, 36], CARRY)],
    "bowlob": [([52, 14], 45.0), ([48, 14], 45.0), ([46, 14], 45.0), ([45, 14], 45.0)],  # 오른쪽 위 45도 조준
    "shbash": [([31, 32], 0.0), ([30, 33], 0.0), ([58, 29], 0.0), ([57, 29], 0.0)],  # 가슴 앞 가드 → 밀기
    "shblock": [([50, 21], 90.0), ([47, 21], 90.0), ([44, 21], 90.0), ([42, 21], 90.0)],
    "spin": [([11, 29], 180.0), ([8, 29], 180.0), ([24, 32], 225.0), ([57, 30], 0.0)],  # 등 → 정면 → 오른쪽 마무리
    # 재사용 그림: 올려치기 = SwingUp(ds_up) 손 그대로, 점프 내려찍기 = 점프 2칸 + 공중 내려베기 + 내려찍기 3~4칸
    "upper": [([29, 38], 250.0), ([17, 36], 235.0), ([17, 36], 230.0), ([53, 19], 50.0), ([40, 6], 120.0)],
    "leapslam": [([47, 20], 100.0), ([46, 19], 100.0), ([44, 8], 120.0), ([50, 38], 315.0), ([49, 37], 300.0),
                 ([48, 46], 345.0), ([47, 44], 340.0)],
}


# 의상 접두별 손 표(체형·자세가 기본 코트와 다른 의상만): {"sc": {"hslash": [...]}}. 없는 키는 HANDS(기본 코트 cb) 로 폴백하고 로그에 남긴다
HANDS_BY_PREFIX = {}


def split_name(png):
    """셀 이름 <의상 접두>_<키>_<n> → (접두, 키)"""
    prefix, rest = png.split("_", 1)
    return prefix, rest.rsplit("_", 1)[0]


def table_for(prefix, key):
    """(표, 폴백 여부). 접두 표 → 기본 표 순서. 둘 다 없으면 (None, False)"""
    own = HANDS_BY_PREFIX.get(prefix, {}).get(key)
    if own is not None:
        return own, False
    return HANDS.get(key), prefix != "cb"


def apply(cel):
    """cel.json 사전의 통짜 의상 클립(outfit)에 손·각도를 넣고 주먹 PNG 를 쓴다. 고친 클립 수를 돌려준다.
    표 없음·프레임 수 다름은 그 클립만 건너뛰고(무기 숨김 유지) 이유를 찍는다. 주먹이 비면(살색 없음) 경고한다."""
    done = 0
    notes = []
    for name, clip in cel.items():
        frames = clip.get("frames", [])
        if not clip.get("outfit") or not frames:
            continue
        prefix, key = split_name(frames[0]["png"])
        if key in HIDE:
            continue
        table, fallback = table_for(prefix, key)
        if table is None:
            notes.append("표 없음(무기 숨김): %s" % name)
            continue
        if len(table) != len(frames):
            notes.append("프레임 수 다름, 건너뜀: %s %d != %d" % (name, len(table), len(frames)))
            continue
        if fallback:
            notes.append("기본 코트 표 재사용(5배 시트로 눈 확인 필요): %s" % name)
        for f, (hand, angle) in zip(frames, table):
            grip = [hand[0], hand[1] + GRIP_DY]
            f["hand"] = grip
            f["angle"] = angle
            im = Image.open(os.path.join(DST, f["png"] + ".png")).convert("RGBA")
            fist = make_fist(im, grip)
            if fist.getbbox() is None:
                notes.append("주먹 빈 그림(살색 없음: 손이 가려졌거나 피부색 다름): %s" % f["png"])
            fist.save(os.path.join(DST, f["png"] + "_fist.png"))
        done += 1
    for n in notes:
        print(n)
    return done


def main():
    cel_path = os.path.join(DST, "cel.json")
    with open(cel_path, encoding="utf-8") as f:
        cel = json.load(f)
    n = apply(cel)
    with open(cel_path, "w", encoding="utf-8") as f:
        json.dump(cel, f, indent=1)
    print("hand clips", n)


if __name__ == "__main__":
    sys.exit(main())
