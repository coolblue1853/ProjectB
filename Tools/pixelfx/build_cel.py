"""셀 애니메이션 정리 → 게임용 프레임(Q-423 셀 방식, PixelRigDesign 3.7).

PixelLab PixMiniMax 초안(Docs/ArtCompare/Motion/cel/<폴더>/<i>.png, 64x64)을 골라
1) 팔레트 고정: 모든 색을 원본 주인공 그림(hero_base.png)의 색 중 가장 가까운 색으로 — 프레임마다 색이 미세하게 흔들리지 않게
2) 정렬: 땅 동작은 발바닥(가장 아래 불투명 줄)을 FeetRow-1 에, 공중 동작은 스카프(주황) 윗줄을 서 있을 때 높이에 — 초안에 박힌 점프 높이를 빼고 무릎 접기만 남긴다
3) 손 위치: 스카프 윗줄+2 ~ 엉덩이 사이 피부색 덩어리 중 가장 앞(오른쪽) = 앞손(무기), 가장 뒤 = 뒷손. 없으면 이전 프레임 값
결과: Assets/Art/Player/PixelRig/Cel/<클립>_<k>.png(+ _fist.png: 무기 위에 다시 그릴 주먹) + cel.json {클립: {frame: 초, loop, frames: [{png, hand:[x,y], handB:[x,y]}]}}
좌표는 64 캔버스 픽셀(왼쪽 위 0). 리그 원점(엉덩이 x 41, 발바닥 55행)은 PixelRigBuilder 와 같다.
사용: python Tools/pixelfx/build_cel.py
"""
import json
import os

import numpy as np
from PIL import Image

SRC = "Docs/ArtCompare/Motion/cel"
OUT = "Assets/Art/Player/PixelRig/Cel"
FEET_ROW = 55
HIP_X = 41  # 리그 원점 x(엉덩이)
# 클립: (초안 폴더, 프레임 번호들, 프레임 시간(초), 반복, 정렬 'feet' | 'scarf')
# 가로 맞춤(아트 px, +앞): 피격 초안은 몸이 15px 뒤로 밀린 채 그려져 맞는 순간 뒤로 튀고 대기로 돌아올 때 앞으로 튀었다 — 넉백은 물리가 한다. 2px 젖혔다 돌아오는 정도만 남김
XSHIFT = {}  # 10-27 새 피격 셀은 이동 보정 없음(예전: {"Hurt": [13, 11, 5]})

# 점프 세트: "side" = 다리 모은 점프(10-26, 사용자 "동작은 완벽한데 다소곳하다" — 지우지 말고 보관), "leap" = 다리 앞뒤로 벌린 도약(10-26 저녁)
JUMPS = {
    "side": (("jump_side", [2, 3, 4], 0.04, False, "feet"), ("jump_side", [5, 6, 7, 8], 0.25, False, "scarf"), ("jump_side", [9, 10, 11], 0.05, False, "feet")),
    "leap": (("jump_leap2", [1, 2, 3], 0.04, False, "feet"), ("jump_leap2", [4, 5, 7, 8], 0.25, False, "scarf"), ("jump_leap2", [9, 10, 11, 12], 0.05, False, "feet")),
}
JUMP_SET = "leap"  # 착지 끝에 12번(걷기 3번과 23px 차이 — 걷기·대기로 이어지는 선 자세)을 붙여 착지 → 대기가 덜 튄다

# 달리기 세트: "loop" = 예전 6장(사용자 "거의 완벽한데 팔 움직임이 적다"), "pump" = 같은 시작 그림에서 팔을 크게 흔들도록 재생성 8장(10-26 저녁)
RUNS = {
    "loop": ("run_loop", list(range(2, 8)), 0.06, True, "const"),  # 2~7: 프레임 간 변화가 가장 고른 6장(이음새 531 ≈ 평균), 리그 달리기 6포즈와 같은 수,
    "pump": ("run_pump2", list(range(0, 8)), 0.055, True, "const"),  # 8 × 0.055 = 0.44초 — 0.045(0.36초)는 사용자 "재생이 빨라 부자연스럽다"
}
RUN_SET = "pump"

CLIPS = {
    "Walk": ("walk_loop", list(range(0, 8)), 0.08, True, "feet"),  # 걷기는 늘 한 발이 땅에 — 프레임마다 발 맞춤  # 0.1 은 디딘 발이 땅보다 느려(3 대 7.2px/프레임) 미끄러졌다
    "Run": RUNS[RUN_SET],
    "JumpCrouch": JUMPS[JUMP_SET][0],
    "JumpArc": JUMPS[JUMP_SET][1],  # 리그가 세로 속도로 시점을 고른다(0 상승 → 1 낙하)
    "Land": JUMPS[JUMP_SET][2],
    "Dash": ("dash_side", [2, 3, 4], 0.05, False, "feet"),     # 1번은 거의 서 있는 자세라 뺐다
    "Hurt": ("basic_sword", ['basic_sword/hurt/H1/4', 'basic_sword/hurt/G1/4', 'basic_sword/hurt/G2/1', 'basic_sword/hurt/G2/2', 'basic_sword/hurt/G2/3'], 0.044, False, "feet"),  # 10-27: 맞는 순간 바로 젖히는 키(대기에서 생성) + 회복 3장 — 생성기가 넣은 베기·피 이펙트 프레임은 뺐다(이펙트는 별도). 예전: ("hurt_side", [2, 3, 4], 0.07, ...)
    "Death": ("death_side", list(range(1, 9)), 0.1, False, "feet"),
    "RopeUp": ("rope_loop", list(range(0, 8)), 0.08, True, "const"),   # 뒷모습 오르기(로프에선 무기 숨김)
    "RopeStay": ("rope_loop", [0], 0.3, True, "const"),
    # 공격(칼을 쥔 채 생성 → 칼·잔상을 지우고 손잡이·칼날 각도를 읽어 실제 장착 무기를 붙인다). 0·8번은 기준 그림이라 뺀다
    "Stab": ("basic_sword", ['basic_sword/skel/stab/0', 'basic_sword/skel/stab/1', 'basic_sword/skel/stab/2', 'basic_sword/skel/stab/3', 'basic_sword/skel/stab/4'], 0.05, False, "feet"),  # 10-27 스켈레톤 v3(관절 길이 대기와 고정, skel_pose.py) — 맨손 생성, 손 = 손목 관절
    "SwingDown": ("basic_sword", ['basic_sword/skel/down2/0', 'basic_sword/skel/down2/1', 'basic_sword/skel/down2/2', 'basic_sword/skel/down2/3', 'basic_sword/skel/down2/4'], 0.05, False, "feet"),  # 스켈레톤 v3(칼 없이 다시 생성한 down2)
    # 공격 끝 회복(10-27 GPT 토론 합의): 콤보가 이어지면 안 보이고, 대기로 돌아갈 때만 리그가 잠깐 보여 준다(PixelRig "<셀>End")
    "StabEnd": ("basic_sword", ['basic_sword/skel/stab/5'], 0.05, False, "feet"),
    "SwingDownEnd": ("basic_sword", ['basic_sword/skel/down2/5'], 0.05, False, "feet"),
    "SwingUp": ("basic_sword", ['basic_sword/skel/up/0', 'basic_sword/skel/up/1', 'basic_sword/skel/up/2', 'basic_sword/skel/up/3', 'basic_sword/skel/up/4', 'basic_sword/skel/up/5', 'basic_sword/skel/up/6'], 0.05, False, "feet"),  # 스켈레톤 v3
    # 대검·둔기 양손(늘린 강철 대검을 합성한 기준 그림에서 생성, 칼 길이 36). 묵직하게 0.05초
    "GreatDown": ("attack/great_down", list(range(1, 8)), 0.05, False, "feet", 36),
    # 단검(쌍단검 계열): 연속 찌르기·X 베기, 칼 길이 14
    # 마스터 모션 38개(MotionSet.md) 중 10-27 추가분 — PixMiniMax 초안 Docs/ArtCompare/Motion/cel/m38, 0·8번은 기준 그림이라 뺀다
    "GreatSide": ("m38/A5_sweep", [1, 2, 4, 4, 5, 6, 7], 0.05, False, "feet", 36),   # A5 양손 수평 휘두르기(세 번째 생성에서 몸이 안 돌았다), 3번은 칼 지우기로 몸이 잘려 4번 두 번
    "AirSlash": ("m38", ['m38/v2_air_a/1', 'm38/v2_air_a/2', 'm38/v2_air_a/3', 'm38/v2_air_b/1', 'm38/v2_air_b/1', 'm38/v2_air_b/2', 'm38/v2_air_b/3'], 0.045, False, "feet", True),  # A8 v2 — 스컬 기준 재작업(10-27), 예전: "AirSlash": ("m38/A8_air", list(range(1, 8)), 0.045, False, 
    "Lunge": ("m38/A9_lunge", list(range(1, 8)), 0.045, False, "feet", True),    # A9 돌진 찌르기
    "Spin": ("m38/A10_spin", list(range(1, 8)), 0.045, False, "feet", True),     # A10 회전 베기(뒷모습 포함)
    "Cast": ("m38/A11_cast", list(range(1, 8)), 0.06, False, "feet"),            # A11 시전(맨손 기준, 무기는 손에)
    "Throw": ("m38/A12_throw", list(range(1, 8)), 0.05, False, "feet"),          # A12 던지기
    "Guard": ("m38", ['m38/v2_guard_a/1', 'm38/v2_guard_a/2', 'm38/v2_guard_a/3', 'm38/v2_guard_a/4', 'm38/v2_guard_b/1', 'm38/v2_guard_b/2', 'm38/v2_guard_b/3'], 0.05, False, "feet", True),  # A14 v2 — 스컬 기준 재작업(10-27), 예전: "Guard": ("m38/A14_guard", list(range(1, 8)), 0.05, False, "
    "Charge": ("m38", ['m38/v2_charge_a/1', 'm38/v2_charge_a/2', 'm38/v2_charge_a/3', 'm38/v2_charge_a/4', 'm38/v2_charge_b/1', 'm38/v2_charge_b/2', 'm38/v2_charge_b/3'], 0.07, False, "feet", True),  # A15 v2 — 스컬 기준 재작업(10-27), 예전: "Charge": ("m38/A15_charge", list(range(1, 8)), 0.07, False,
    "LeapSlam": ("m38", ['m38/v2_slam_a/1', 'm38/v2_slam_a/2', 'm38/v2_slam_a/3', 'm38/v2_slam_a/4', 'm38/v2_slam_b/1', 'm38/v2_slam_b/2', 'm38/v2_slam_b/3'], 0.055, False, "feet", True),  # A16 v2 — 스컬 기준 재작업(10-27), 예전: "LeapSlam": ("m38/A16_leapslam", list(range(1, 8)), 0.05, Fa
    "Backstep": ("m38/A17_backstep", list(range(1, 8)), 0.05, False, "feet", True),  # A17 백스텝(이동은 물리)
    "Punch": ("m38", ['m38/v2_punch_a/1', 'm38/v2_punch_a/2', 'm38/v2_punch_a/3', 'm38/v2_punch_a/4', 'm38/v2_punch_b/1', 'm38/v2_punch_b/2', 'm38/v2_punch_b/3'], 0.05, False, "feet", True),  # A18 v2(칼을 든 채 빈손 주먹) — 스컬 기준 재작업(10-27), 예전: "Punch": ("m38/A18_punch", list(range(1, 8)), 0.05, False, "
    "Iaido": ("m38", ['m38/v2_iaido_a/1', 'm38/v2_iaido_a/2', 'm38/v2_iaido_a/3', 'm38/v2_iaido_a/4', 'm38/v2_iaido_b/1', 'm38/v2_iaido_b/2', 'm38/v2_iaido_b/3'], 0.045, False, "feet", True),  # A19 v2 — 스컬 기준 재작업(10-27), 예전: "Iaido": ("m38/A19_iaido", [1, 2, 3, 4, 4, 6, 7], 0.045, Fal
    "ShoulderAim": ("m38/R1_shoulder", list(range(1, 8)), 0.06, False, "feet"),  # R1 어깨 겨눔
    "Raise": ("m38/R2_raise", list(range(1, 8)), 0.07, False, "feet"),           # R2 양손 들기
    "Knockdown": ("m38/R5_knockdown", list(range(1, 12)), 0.08, False, "feet"),  # R5 넘어짐·일어남
    "Place": ("m38/R6_place", list(range(1, 8)), 0.07, False, "feet"),           # R6 웅크려 놓기
    "Kick": ("m38/R7_kick", list(range(1, 8)), 0.05, False, "feet"),             # R7 발차기
    "Grab": ("m38/R3_grab2", list(range(1, 8)), 0.06, False, "feet"),          # R3 잡기(재생성: 소품 없이)
    "Ride": ("m38/R4_sit2", list(range(1, 9)), 0.12, True, "const"),           # R4 앉기(탈것) 반복
    # A13 활: 두 번 생성 모두 활을 그리거나 뒤를 겨눠 기각 → 찌르기 셀 + 활 무기 각도로 대체(PixelRig Shoot→Bow 셀 없으면 Stab)
    "DaggerFlurry": ("attack/dagger_flurry", list(range(1, 8)), 0.035, False, "feet", 14),
    "DaggerCross": ("attack/dagger_cross", [1, 4, 5], 0.07, False, "feet", 14),  # 2·3·6·7번은 생성 그림의 큰 베기 잔상이 몸을 덮어 지우면 몸이 날아가 뺐다
    # GreatSide: 첫 생성은 2~6번에서 왼쪽으로 돌아서 기각(great_side), 재생성 결과를 확인 후 넣는다
}
CONST_REF = {"rope_loop": list(range(0, 8))}
SKIN = [(0xFD, 0xE0, 0xC6), (0xF3, 0xB4, 0x9A), (0xD7, 0x8E, 0x7A)]
SCARF = [(0xF1, 0xAB, 0x36), (0xB8, 0x70, 0x2C)]


def palette():
    a = np.array(Image.open(os.path.join(SRC, "hero_base.png")).convert("RGBA"))
    cols = {tuple(p) for p in a[a[..., 3] > 0][:, :3]}
    return np.array(sorted(cols), dtype=int)


def lock(img, pal):
    a = np.array(img.convert("RGBA")).astype(int)
    m = a[..., 3] > 0
    px = a[m][:, :3]
    d = ((px[:, None, :] - pal[None, :, :]) ** 2).sum(axis=2)
    a[m, :3] = pal[d.argmin(axis=1)]
    a[..., 3] = np.where(m, 255, 0)
    return a.astype(np.uint8)


def near(a, cols, tol=40):
    m = np.zeros(a.shape[:2], bool)
    for c in cols:
        m |= (np.abs(a[..., :3].astype(int) - np.array(c)).sum(axis=2) < tol) & (a[..., 3] > 0)
    return m


def shift(a, dy):
    out = np.zeros_like(a)
    if dy >= 0:
        out[dy:] = a[:a.shape[0] - dy]
    else:
        out[:dy] = a[-dy:]
    return out


GRIP_UP = 2


def hand_near(a, last, r=9):
    """직전 무기 손(last) 근처 r px 안의 피부 덩어리 → 주먹 점(가운데 x, 맨 아래 - GRIP_UP). 없으면 None."""
    sc = near(a, SCARF)
    sy = np.nonzero(sc.any(axis=1))[0]
    sk = near(a, SKIN)
    sk[:(sy.min() + 2 if len(sy) else 40)] = False
    ys, xs = np.nonzero(sk)
    if len(xs) == 0:
        return None
    dd = (xs - last[0]) ** 2 + (ys - last[1]) ** 2
    i = int(dd.argmin())
    if dd[i] > r * r:
        return None
    blob = (np.abs(xs - xs[i]) <= 2) & (np.abs(ys - ys[i]) <= 3)
    return [int(round(xs[blob].mean())), int(ys[blob].max()) - GRIP_UP]


def hands(a):
    """스카프 아래 피부색 픽셀을 x 로 둘로 나눠 (앞손, 뒷손) — 각 덩어리의 가장 아래·바깥 픽셀(주먹 끝)"""
    sc = near(a, SCARF)
    sy = np.nonzero(sc.any(axis=1))[0]
    # 얼굴은 스카프 윗줄보다 위에만 있다 — 손은 스카프 옆 같은 높이에도 온다(달리기·웅크림), 그래서 스카프 아랫줄이 아니라 윗줄 + 2 부터
    top = sy.min() + 2 if len(sy) else 40
    sk = near(a, SKIN)
    sk[:top] = False
    # 무릎 맨살도 피부색이라 발에서 6줄 위(엉덩이)보다 아래는 뺀다 — 처음엔 무릎을 손으로 잡았다
    hips = np.nonzero(a[..., 3].any(axis=1))[0].max() - 5
    sk[hips:] = False
    ys, xs = np.nonzero(sk)
    if len(xs) == 0:
        return None, None
    order = np.argsort(xs)
    xs, ys = xs[order], ys[order]
    gaps = np.diff(xs)
    if len(gaps) and gaps.max() >= 3:
        cut = gaps.argmax() + 1
        groups = [(xs[:cut], ys[:cut]), (xs[cut:], ys[cut:])]
    else:
        groups = [(xs, ys)]
    # 주먹 가운데(맨 아래 픽셀 - GRIP_UP) — 맨 아래에 손잡이 가운데를 두면 칼이 주먹 아래에 매달려 보였다(10-26 저녁)
    pts = [(int(round(gx.mean())), int(gy.max()) - GRIP_UP) for gx, gy in groups]
    if len(pts) > 1:
        # 무기 손 = 온전히 보이는 손(피부 픽셀이 가장 많은 덩어리) = 카메라 쪽 손. 예전엔 x 가 가장 앞인 손을 골라
        # 프레임마다 손이 바뀌어 무기가 몸 앞뒤로 들락날락했다(10-26 사용자 지적)
        sizes = [len(gx) for gx, gy in groups]
        near_i = int(np.argmax(sizes))
        return pts[near_i], pts[1 - near_i]
    # 하나만 보이면 그게 가까운 팔(무기 손) — 측면 그림에서 먼 팔은 몸에 가려진다. 몸 뒤쪽이면 리그가 칼을 위·뒤로 세운다(최대 100°).
    # (3/4 정면 초안에선 뒷손을 앞손으로 잡아 칼이 얼굴을 가로질렀지만, 측면으로 통일한 뒤엔 보이는 손이 곧 가까운 손)
    return pts[0], pts[0]


IDLE_BASE = ("Walk", 3)  # 걷기 루프에서 두 발이 모인 프레임 — 걷기와 같은 시점(착지 회복 프레임은 정면 쪽이라 어긋났다)
IDLE_STAND = "idle_stand/arm_clean.png"  # 임시(10-26 밤): 사용자가 왼다리·왼팔을 직접 고친 그림(arm_user.png)에서 소매 끝·손끝·배낭 밑단 픽셀만 정리  # 빈 문자열이면 걷기 3번
# 손 검출이 무릎 맨살을 잡거나 못 찾는 프레임(다리 벌린 도약 점프) — 눈으로 잡은 뒷손(무기 손) 주먹 가운데, 64 캔버스 좌표(정렬 후)
# 칼 각도 고치기: 초안 칼의 손잡이 쪽을 잘못 읽은 프레임(10-27 기본 칼) — 그림에 그려진 칼 방향으로 맞춘다
# 프레임별 시간(초, 10-27 GPT 토론: 준비는 길게·타격은 짧게·끝은 잠깐 멈춤). 없으면 클립 frame
TIMES = {
    "Stab": [0.07, 0.06, 0.05, 0.03, 0.29],
    "SwingDown": [0.06, 0.06, 0.08, 0.03, 0.27],
    "SwingUp": [0.07, 0.05, 0.03, 0.04, 0.06, 0.07, 0.08],
    "Hurt": [0.03, 0.05, 0.05, 0.045, 0.045],
}
# 스켈레톤 셀(10-27): 손 = 자세표 손목 관절(가로·세로 맞춤만 더함), 칼 각도 = 자세표 설계값
SKEL_HAND = {'Stab': [[39, 41], [36, 38], [41, 39], [53, 37], [53, 36]], 'StabEnd': [[49, 39]], 'SwingDown': [[47, 27], [36, 24], [29, 27], [51, 37], [51, 44]], 'SwingDownEnd': [[50, 45]], 'SwingUp': [[35, 43], [43, 45], [45, 28], [37, 23], [31, 24], [35, 25], [37, 43]]}
SKEL_ANGLE = {'Stab': [-15, 5, 0, 0, 0], 'StabEnd': [-30], 'SwingDown': [80, 115, 150, 20, -35], 'SwingDownEnd': [-40], 'SwingUp': [-160, -110, 60, 95, 130, 150, -40]}
ANGLE_FIX = {}
HAND_FIX = {("JumpCrouch", 2): (24, 33), ("JumpArc", 0): (24, 33), ("JumpArc", 1): (25, 33), ("JumpArc", 2): (27, 33), ("JumpArc", 3): (28, 34),
            ("Land", 0): (27, 31), ("Land", 1): (28, 36)}
IDLE_HAND, IDLE_HAND_B = (34, 44), (49, 43)  # 대기 셀의 무기 손·반대 손(cel.json Idle) — 동작 손 추적 시작점
IDLE_STRETCH_ROW = 44   # 튜닉 가운데 줄 — 복제하면 허리 위(상체·머리)가 1px 오른다


# 숨쉬기 3단(10-26 밤 사용자: 2단은 딱딱 끊긴다): 0 = 기본, 1 = 튜닉 줄 위(상체·머리) 1px 올림, 2 = 거기서 머리(스카프 윗줄 위)만 1px 더
IDLE_SEQ = [0, 1, 2, 2, 1, 0]
IDLE_FRAME = 0.2  # 6 × 0.2 = 1.2초(예전 4 × 0.3 과 같은 호흡 길이)


def idle_head_row(a):
    rows = np.nonzero(near(a, SCARF).any(axis=1))[0]
    return int(rows.min()) if len(rows) else 36


def breath(a, stage, head):
    """숨쉬기 단계 그림: 줄 복제로 위쪽을 올린다(장비 레이어에도 같은 변환을 쓴다)."""
    out = a.copy()
    if stage >= 1:
        out[:IDLE_STRETCH_ROW] = a[1:IDLE_STRETCH_ROW + 1]
    if stage >= 2:
        prev = out.copy()
        out[:head] = prev[1:head + 1]
    return out


def synth_idle(meta):
    """측면 대기 숨쉬기(생성 없이): 착지 회복 프레임에서 튜닉 한 줄을 복제해 상체를 1px 올렸다 내림.
    PixMiniMax 로 만든 대기는 1~3프레임에서 3/4 정면 그림으로 되돌아가 이음새가 끊겼다(10-26)."""
    clip, k = IDLE_BASE
    src = meta[clip]["frames"][k]
    a = np.array(Image.open(os.path.join(OUT, src["png"] + ".png")).convert("RGBA"))
    if IDLE_STAND:
        # 똑바로 선 대기(10-26 저녁 사용자: 걷기 3번은 한 다리를 꼰 채 서 있어 부자연스럽다) — 걷기 3번을 다리만 편집(5 gen)
        a = lock(Image.open(os.path.join(SRC, IDLE_STAND)).convert("RGBA"), palette())
        ys = np.nonzero(a[..., 3].any(axis=1))[0]
        a = shift(a, (FEET_ROW - 1) - int(ys.max()))
        h, b = hands(a)
        src = {"hand": list(h) if h else src["hand"], "handB": list(b) if b else src["handB"]}
    head = idle_head_row(a)
    frames = []
    for i, st in enumerate(IDLE_SEQ):
        img = breath(a, st, head)
        name = f"Idle_{i}"
        Image.fromarray(img, "RGBA").save(os.path.join(OUT, name + ".png"))
        dy = -st
        h = list(src["hand"])  # 손은 튜닉 줄(44) 아래라 숨쉬기에 안 움직인다
        Image.fromarray(fist(img, h), "RGBA").save(os.path.join(OUT, name + "_fist.png"))
        frames.append({"png": name, "hand": h, "handB": src["handB"], "dy": dy})
    print("Idle (synth)", len(frames))
    return {"frame": IDLE_FRAME, "loop": True, "frames": frames}


BLADE_LEN = 24  # 칼 길이(px) — 손잡이에서 이 안의 가장 먼 강철 픽셀이 칼끝


def steel_mask(a):
    """칼·베기 잔상: 채도 낮은 회색·강철 계열(주인공 팔레트엔 없다). 검정 외곽선(어두움)은 빼고, 칼 외곽선(78,80,98)은 포함"""
    rgb = a[..., :3].astype(int)
    mx, mn = rgb.max(axis=2), rgb.min(axis=2)
    gray = (mx - mn < 34) & (mx > 70) & (a[..., 3] > 0)
    bluish = (rgb[..., 2] >= rgb[..., 0]) & (rgb[..., 2] >= rgb[..., 1] - 4)
    pale = (mx - mn < 48) & (mx > 185) & (a[..., 3] > 0)  # 밝은 베이지·흰 베기 잔상(올려베기에서 남았다)
    # 어두운 청회색 칼 외곽선(47,59,71 등): 칼날이 1~2px 로 가늘게 그려지면 몸통 없이 외곽선만 남아 칼을 못 읽었다(10-27 찌르기)
    dark_steel = (rgb[..., 2] >= rgb[..., 0] + 12) & (rgb[..., 2] >= rgb[..., 1]) & (mx > 40) & (mx < 120) & (a[..., 3] > 0)
    return (gray & bluish) | pale | dark_steel


def blade_fit(a, st, blade_len):
    """그려진 칼에서 손잡이 끝과 칼날 각도를 읽는다(10-27): 가장 큰 강철 덩어리(칼)에 주성분 직선을 맞추고, 양 끝 중 피부(주먹)에
    가까운 끝 = 손잡이. 예전엔 '칼 무게중심에 가장 가까운 피부'를 손으로 잡아 손이 팔꿈치·가슴으로 튀고 각도가 프레임마다 흔들렸다."""
    parts = [q for q in components(st) if len(q) >= 10]  # 10px 미만 = 칼끝 조각·잡티(손 위치를 엉뚱한 데로 끌었다)
    if not parts:
        return None, None
    pts = np.array(max(parts, key=len), dtype=float)  # (y, x)
    if len(pts) > blade_len * 4:
        return None, None  # 칼보다 훨씬 큰 덩어리 = 베기 잔상(스미어) — 칼 방향을 읽을 수 없다
    c = pts.mean(axis=0)
    u, sv, vt = np.linalg.svd(pts - c, full_matrices=False)
    axis = vt[0]
    proj = (pts - c) @ axis
    e1, e2 = c + axis * proj.min(), c + axis * proj.max()
    sk = np.argwhere(near(a, SKIN))
    if len(sk):
        d1 = ((sk - e1) ** 2).sum(axis=1).min()
        d2 = ((sk - e2) ** 2).sum(axis=1).min()
        g, t = (e1, e2) if d1 <= d2 else (e2, e1)
        if min(d1, d2) > 9 ** 2:  # 손잡이(어두운 색)는 칼로 안 잡혀 칼날 끝이 주먹에서 6~8px 떨어진다
            return None, None  # 손에서 떨어진 칼 그림(생성 잡티) — 손잡이로 쓰면 칼이 몸 밖에 떠 있었다
    else:
        g, t = e1, e2
    angle = float(np.degrees(np.arctan2(g[0] - t[0], t[1] - g[1])))
    return [int(round(g[1])), int(round(g[0]))], angle


MAIN_MIN = 40  # 몸 본체와 떨어진 이보다 작은 조각은 칼 손잡이·잔상 찌꺼기


def components(mask):
    h, w = mask.shape
    seen = np.zeros_like(mask)
    parts = []
    for y in range(h):
        for x in range(w):
            if mask[y, x] and not seen[y, x]:
                stack, part = [(y, x)], []
                seen[y, x] = True
                while stack:
                    cy, cx = stack.pop()
                    part.append((cy, cx))
                    for ny, nx in ((cy + 1, cx), (cy - 1, cx), (cy, cx + 1), (cy, cx - 1), (cy + 1, cx + 1), (cy - 1, cx - 1), (cy + 1, cx - 1), (cy - 1, cx + 1)):
                        if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                            seen[ny, nx] = True
                            stack.append((ny, nx))
                parts.append(part)
    return parts


def keep_main(a):
    out = a.copy()
    parts = components(a[..., 3] > 0)
    biggest = max((len(p) for p in parts), default=0)
    for part in parts:
        # 몸 본체(가장 큰 덩어리)의 25% 미만은 버린다 — 잔상 안쪽이 팔레트 고정으로 갈색 호 조각(60px)이 되어 남았다
        if len(part) < max(MAIN_MIN, biggest * 0.25):
            for y, x in part:
                out[y, x] = 0
    return out


def fill_holes(a):
    """바깥(테두리)과 이어지지 않은 투명 픽셀 = 칼이 몸 위를 지나던 자리의 구멍 → 위·왼쪽 이웃 색으로 채운다"""
    out = a.copy()
    h, w = a.shape[:2]
    empty = out[..., 3] == 0
    outside = np.zeros_like(empty)
    stack = [(y, x) for y in range(h) for x in (0, w - 1) if empty[y, x]] + [(y, x) for x in range(w) for y in (0, h - 1) if empty[y, x]]
    for y, x in stack:
        outside[y, x] = True
    while stack:
        y, x = stack.pop()
        for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
            if 0 <= ny < h and 0 <= nx < w and empty[ny, nx] and not outside[ny, nx]:
                outside[ny, nx] = True
                stack.append((ny, nx))
    holes = empty & ~outside
    for _ in range(6):
        ys, xs = np.nonzero(holes)
        if not len(ys):
            break
        for y, x in zip(ys, xs):
            nbs = [(ny, nx) for ny, nx in ((y - 1, x), (y, x - 1), (y + 1, x), (y, x + 1)) if 0 <= ny < h and 0 <= nx < w and out[ny, nx, 3] > 0]
            if not nbs:
                continue
            # 어둡지 않은 이웃 색 우선 — 외곽선 색으로 메우면 머리카락 속에 검은 줄이 생겼다
            bright = [n for n in nbs if int(out[n][:3].astype(int).sum()) >= 150]
            out[y, x] = out[(bright or nbs)[0]]
            holes[y, x] = False
    return out


def _shift_or(m, r):
    o = m.copy()
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            o |= np.roll(np.roll(m, dy, axis=0), dx, axis=1)
    return o


def detach_thin(a, steel):
    """칼·잔상 자리 둘레(2px)에서 본체에 가늘게(≤2px) 붙은 부분을 떼어 낸다 — 잔상 테두리를 머리카락 어두운 색으로 그려
    색으로 안 걸리고 머리카락과 한 점으로 이어져 남았다(내려베기 타격). 1px 깎기 → 가장 큰 덩어리 → 다시 1px 늘리기, 칼 자리 밖은 건드리지 않는다"""
    m = a[..., 3] > 0
    env = _shift_or(steel, 2)
    er = m & ~_shift_or(~m, 1)
    parts = components(er)
    if not parts:
        return a
    big = max(parts, key=len)
    core = np.zeros_like(m)
    for y, x in big:
        core[y, x] = True
    keep = _shift_or(core, 1) & m
    out = a.copy()
    out[env & m & ~keep] = 0
    return out


def recolor_inner_outline(a, steel):
    """칼 자리 바로 옆(1px)의 어두운 픽셀 중 몸 안쪽(이웃 3면 이상 불투명)에 있는 것 = 머리카락·몸 위에 그려졌던 칼 외곽선 → 밝은 이웃 색으로
    (올려베기에서 머리카락 속에 검은 줄로 남았다). 실루엣 가장자리의 몸 외곽선은 이웃이 2면 이하라 그대로"""
    out = a.copy()
    env = _shift_or(steel, 1)
    h, w = out.shape[:2]
    for _ in range(2):
        alpha = out[..., 3] > 0
        dark = alpha & (out[..., :3].astype(int).sum(axis=2) < 150)
        ys, xs = np.nonzero(dark & env)
        for y, x in zip(ys, xs):
            nbs = [(ny, nx) for ny, nx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)) if 0 <= ny < h and 0 <= nx < w and alpha[ny, nx]]
            bright = [n for n in nbs if int(out[n][:3].astype(int).sum()) >= 150]
            if len(nbs) >= 3 and bright:
                out[y, x] = out[bright[0]]
    return out


def strip_weapon(a, blade_len=None):
    """칼·잔상 픽셀을 지우고(몸에 난 구멍은 주변 색으로 메움) 손잡이(칼에 가장 가까운 주먹)와 칼날 각도(도, 0 = 앞, 90 = 위)를 돌려준다"""
    st = steel_mask(a)
    grip, angle = blade_fit(a, st, blade_len or BLADE_LEN)
    out = a.copy()
    out[st] = 0
    # 칼 외곽선 잔여물: 8방향 이웃에 색 있는(어둡지 않은) 몸 픽셀이 하나도 없는 어두운 픽셀을 반복해서 지운다 — 몸 외곽선은 늘 몸 색에 닿아 있다
    for _ in range(8):
        alpha = out[..., 3] > 0
        dark = alpha & (out[..., :3].astype(int).sum(axis=2) < 150)
        colored = alpha & ~dark
        cnt = np.zeros(alpha.shape, int)
        for dy2 in (-1, 0, 1):
            for dx2 in (-1, 0, 1):
                if dy2 or dx2:
                    cnt += np.roll(np.roll(colored, dy2, axis=0), dx2, axis=1)
        lone = dark & (cnt == 0)
        if not lone.any():
            break
        out[lone] = 0
    out = detach_thin(out, st)
    out = keep_main(out)
    out = fill_holes(out)
    out = recolor_inner_outline(out, st)
    # 구멍 메우기: 지운 픽셀 중 상하좌우 이웃 3개 이상이 불투명이면 몸 안쪽 → 이웃 색으로
    for _ in range(3):
        alpha = out[..., 3] > 0
        fill = []
        for y, x in zip(*np.nonzero(st & ~alpha)):
            nb = [(y + dy2, x + dx2) for dy2, dx2 in ((1, 0), (-1, 0), (0, 1), (0, -1)) if 0 <= y + dy2 < 64 and 0 <= x + dx2 < 64 and alpha[y + dy2, x + dx2]]
            if len(nb) >= 3:
                fill.append((y, x, out[nb[0]]))
        for y, x, c in fill:
            out[y, x] = c
    return out, grip, angle


FIST_R = 2  # 주먹 레이어: 손 좌표 둘레 이 반경 안의 피부색 + 그 테두리 외곽선


def fist(a, hand):
    """무기 위에 다시 그릴 주먹 — 손 점에 닿은 피부 덩어리(주먹 상자 안) + 둘레 한 겹(8방향).
    둘레 칸은 원래 그림이 비었거나 어두우면 검은 외곽선, 아니면 원래 색 — 예전엔 상자 안 피부·기존 외곽선 조각만 떠서
    칼 위에 손이 부서진 조각처럼 보였다(10-26 밤 사용자: 손이 다 깨졌다)."""
    out = np.zeros_like(a)
    if hand is None:
        return out
    hx, hy = hand
    sk = near(a, SKIN) & (a[..., 3] > 0)
    h, w = sk.shape
    box = np.zeros_like(sk)
    box[max(0, hy - FIST_R - 1):min(h, hy + 2 + GRIP_UP), max(0, hx - FIST_R):min(w, hx + FIST_R + 1)] = True
    cand = sk & box
    ys, xs = np.nonzero(cand)
    if len(xs) == 0:
        return out
    i0 = int(((xs - hx) ** 2 + (ys - hy) ** 2).argmin())
    keep = np.zeros_like(sk)
    stack = [(int(ys[i0]), int(xs[i0]))]
    while stack:
        y, x = stack.pop()
        if keep[y, x] or not cand[y, x]:
            continue
        keep[y, x] = True
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w:
                stack.append((ny, nx))
    ring = np.zeros_like(keep)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            ring |= np.roll(np.roll(keep, dy, axis=0), dx, axis=1)
    ring &= ~keep
    out[keep] = a[keep]
    dark = (a[..., 3] == 0) | (a[..., :3].astype(int).sum(axis=2) < 120)
    out[ring & dark] = (0, 0, 0, 255)
    out[ring & ~dark] = a[ring & ~dark]
    return out


def smooth_hands(frames):
    """앞뒤 프레임은 가까운데 혼자 5px 넘게 튄 손 좌표는 이웃 평균으로 — 칼이 한 프레임만 다른 팔로 옮겨 붙어 보였다(10-27 사용자)."""
    for k in range(1, len(frames) - 1):
        p, h, n = frames[k - 1]["hand"], frames[k]["hand"], frames[k + 1]["hand"]
        d = lambda u, v: abs(u[0] - v[0]) + abs(u[1] - v[1])
        if d(h, p) > 5 and d(h, n) > 5 and d(p, n) <= 4:
            frames[k]["hand"] = [(p[0] + n[0]) // 2, (p[1] + n[1]) // 2]


# 가로 맞춤(10-27 원인 검토 1·2번): 생성기는 런지를 '앞발 고정·뒷발을 뒤로'로 그려 공격 중 몸이 최대 10px 뒤로 물러났다.
# "front" = 앞발(바닥 3줄의 가장 오른쪽 픽셀)을 대기 앞발 x 에 둔다 → 찌르면 머리가 앞으로 나간다.
# ("head", d) = 머리(위 18줄 가운데) x 를 대기 머리 + d 에 둔다 → 피격은 3px 만 젖힌다(넉백은 물리가 한다).
IDLE_FRONT_X, IDLE_HEAD_X = 47, 39.5
ALIGN_X = {"Hurt": ("head", -3)}  # 공격은 스켈레톤 자세에 위치가 들어 있다(10-27)

# 콤보 1·2타는 다음 타로 이어지게 끝을 대기에 묶지 않고, 회복(End) 클립은 시작을 묶지 않는다
ALIGN_ANCHOR = {"Stab": "start", "SwingDown": "start", "StabEnd": "end", "SwingDownEnd": "end"}
# 콤보 2·3타는 앞 타가 끝난 머리 위치에서 시작(대기로 5~6px 뒤로 돌아갔다 다시 나가며 튀었다)
START_HEAD = {"SwingDown": 44.0, "SwingUp": 45.0}


def head_x(a):
    m = a[..., 3] > 0
    ys, xs = np.nonzero(m)
    return float(xs[ys < ys.min() + 18].mean())


def plan_x(raws, clip):
    """앞발 맞춤 뒤 머리가 가장 앞으로 나간 프레임 다음부터는 대기 머리 위치까지 고르게 돌아오게 한다 —
    회복 마지막 프레임에서 대기로 6px 한 번에 돌아오며 튀었다(10-27)."""
    sx = [align_x(r, clip) for r in raws]
    if ALIGN_X.get(clip) != "front":
        return sx
    hx = [head_x(r) + d for r, d in zip(raws, sx)]
    # 머리 이동은 프레임당 3px 이하(대기에서 출발, 대기로 도착) — 한 프레임에 6px 씩 튀던 것을 막는다
    want = [min(max(h, IDLE_HEAD_X - 2), IDLE_HEAD_X + 6) for h in hx]
    anchor = ALIGN_ANCHOR.get(clip, "both")
    if anchor in ("both", "start"):
        cur = START_HEAD.get(clip, IDLE_HEAD_X)
        for k in range(len(want)):
            cur = cur + max(-3.0, min(3.0, want[k] - cur))
            want[k] = cur
    if anchor in ("both", "end"):
        cur = IDLE_HEAD_X
        for k in range(len(want) - 1, -1, -1):
            cur = cur + max(-3.0, min(3.0, want[k] - cur))
            want[k] = cur
    for k in range(len(raws)):
        sx[k] = int(round(want[k] - head_x(raws[k])))
    return sx


def align_x(a, clip):
    mode = ALIGN_X.get(clip)
    if mode is None:
        return 0
    m = a[..., 3] > 0
    ys, xs = np.nonzero(m)
    if not len(xs):
        return 0
    if mode == "front":
        bot = ys.max()
        return int(IDLE_FRONT_X - xs[ys >= bot - 2].max())
    top = ys.min()
    return int(round(IDLE_HEAD_X + mode[1] - xs[ys < top + 18].mean()))


REST_ANGLE = -20.0  # 리그 대기 칼 각도(PixelRig.CelBladeAngle)


def fist_center(a, g, r=9):
    """칼날 끝(손잡이 쪽)에서 가장 가까운 피부 덩어리 = 주먹의 가운데. 손잡이(어두운 색)는 칼로 안 읽혀 칼날 끝이 주먹에서
    6~8px 떨어져 있었고, 그 점을 손으로 쓰면 주먹 레이어가 비어(0~9px, 대기는 42px) 손이 작아지거나 사라져 보였다(10-27 사용자)."""
    sk = near(a, SKIN) & (a[..., 3] > 0)
    sc = np.nonzero(near(a, SCARF).any(axis=1))[0]
    top = sc.min() if len(sc) else 40
    # 얼굴 피부(스카프 위에 가운데가 있는 가장 큰 덩어리)는 제외 — 칼이 가슴 높이일 때 턱을 주먹으로 잡았다.
    # 줄로 자르면 머리 위로 든 주먹(올려베기)까지 지워져서 덩어리 단위로 뺀다
    if g[1] >= top:
        sk[:top + 2] = False  # 칼 손이 스카프 아래면 얼굴·턱 줄을 통째로 뺀다
    parts = [q for q in components(sk) if len(q) >= 2]
    face = [q for q in parts if np.mean([y for y, _ in q]) < top and len(q) >= 12]
    if face:  # 머리 위로 든 주먹(올려베기)은 줄로 자르면 지워져서 얼굴 덩어리만 뺀다
        f0 = max(face, key=len)
        parts = [q for q in parts if q is not f0]
    best, bd = None, r * r
    for q in parts:
        arr = np.array(q)
        d = (((arr - [g[1], g[0]]) ** 2).sum(axis=1)).min()
        if d <= bd:
            best, bd = arr, d
    if best is None:
        return g
    # 덩어리 전체(팔뚝까지)의 가운데는 팔꿈치 쪽으로 끌려간다 — 칼날 끝에 가장 가까운 점에서 3px 안만 주먹으로 본다
    dd = ((best - [g[1], g[0]]) ** 2).sum(axis=1)
    near_pts = best[dd <= (np.sqrt(bd) + 3) ** 2]
    return [int(round(near_pts[:, 1].mean())), int(round(near_pts[:, 0].mean()))]


def grip_to_fist(a, g, angle, reach=11):
    """칼날 시작점에서 칼 반대 방향(손잡이 쪽)으로 따라가 처음 만나는 피부 = 주먹. 그 둘레 3px 피부의 가운데를 손으로 쓴다.
    칼날 시작점을 손으로 쓰면 칼이 주먹보다 6~8px 앞에 붙어 손이 프레임마다 움직여 보였다(10-27 사용자 2번 규칙)."""
    if angle is None:
        return g
    sk = near(a, SKIN) & (a[..., 3] > 0)
    ux, uy = -np.cos(np.radians(angle)), np.sin(np.radians(angle))  # 손잡이 방향(그림 좌표, y 아래)
    h, w = sk.shape
    for t in range(0, reach + 1):
        x, y = int(round(g[0] + ux * t)), int(round(g[1] + uy * t))
        if 0 <= x < w and 0 <= y < h and sk[y, x]:
            ys, xs = np.nonzero(sk[max(0, y - 3):y + 4, max(0, x - 3):x + 4])
            return [int(round(xs.mean())) + max(0, x - 3), int(round(ys.mean())) + max(0, y - 3)]
    return g


_COMMON_FIST = None


def common_fist(hand):
    """공격 셀 주먹 = 대기 주먹 한 장을 손 좌표로 옮겨 찍는다(10-27 사용자 1번 규칙: 비슷한 자세면 손 크기가 같아야 한다).
    초안마다 주먹을 다르게 그려 칼 위에 다시 그리는 주먹이 0~37px 로 들쭉날쭉했다(대기 42px)."""
    global _COMMON_FIST
    if _COMMON_FIST is None:
        _COMMON_FIST = np.array(Image.open(os.path.join(OUT, "Idle_0_fist.png")).convert("RGBA"))
    out = np.zeros_like(_COMMON_FIST)
    if hand is None:
        return out
    dx, dy = hand[0] - IDLE_HAND[0], hand[1] - IDLE_HAND[1]
    ys, xs = np.nonzero(_COMMON_FIST[..., 3] > 0)
    ok = (ys + dy >= 0) & (ys + dy < 64) & (xs + dx >= 0) & (xs + dx < 64)
    out[ys[ok] + dy, xs[ok] + dx] = _COMMON_FIST[ys[ok], xs[ok]]
    return out


# 머리 고정(10-27 사용자 1·4번 규칙): 초안마다 머리카락 덩어리 크기가 달라(대기 681px → 공격 347~689px) 머리가 출렁였다.
# 공격·피격 셀은 대기 머리(스카프 윗줄 위)를 머리 위치(위 18줄 가운데·스카프 높이)에 맞춰 찍는다. 머리 앞의 팔·주먹·칼은 위에 다시 그린다.
HEAD_LOCK = {"Stab", "StabEnd", "SwingDown", "SwingDownEnd", "SwingUp", "Hurt"}
HAIR = [(104, 65, 57), (83, 44, 45), (55, 26, 32), (23, 11, 13), (40, 20, 24), (0, 0, 0)]
_IDLE_HEAD = None


def lock_head(a):
    global _IDLE_HEAD
    if _IDLE_HEAD is None:
        idle = np.array(Image.open(os.path.join(OUT, "Idle_0.png")).convert("RGBA"))
        it = int(np.nonzero(near(idle, SCARF).any(axis=1))[0].min())
        hd = idle[:it].copy()
        hm = hd[..., 3] > 0
        ys, xs = np.nonzero(hm)
        _IDLE_HEAD = (hd, hm, it, xs[ys < ys.min() + 18].mean())
    hd, hm, it, hcx = _IDLE_HEAD
    sc = np.nonzero(near(a, SCARF).any(axis=1))[0]
    if not len(sc):
        return a
    t = int(sc.min())
    ys, xs = np.nonzero(a[:t, ..., 3] > 0)
    if not len(ys):
        return a
    dx, dy = int(round(xs[ys < ys.min() + 18].mean() - hcx)), t - it
    region = np.zeros(a.shape[:2], bool)
    region[max(0, t - it - 2):t, :] = True
    op = a[..., 3] > 0
    hair = region & op & near(a, HAIR, 8)
    sk = near(a, SKIN) & op & region
    parts = components(sk)
    face = np.zeros_like(sk)
    for y, x in (max(parts, key=len) if parts else []):
        face[y, x] = True
    keep = region & op & ~hair & ~face
    out = a.copy()
    out[hair | face] = 0
    yy, xx = np.nonzero(hm)
    ny, nx = yy + dy, xx + dx
    ok = (ny >= 0) & (ny < 64) & (nx >= 0) & (nx < 64)
    out[ny[ok], nx[ok]] = hd[yy[ok], xx[ok]]
    out[keep] = a[keep]
    return out


def fill_angles(frames):
    """칼을 못 읽은 프레임(첫·끝 프레임, 스미어)은 이웃 프레임 각도를 이어 받는다 — 리그 기본 규칙 각도(-20°/210°)로 떨어지면 칼이 순간이동했다(10-27)."""
    known = [k for k, f in enumerate(frames) if f["angle"] > -998]
    if not known:
        return
    for k, f in enumerate(frames):
        if f["angle"] > -998:
            continue
        lo = max([j for j in known if j < k], default=None)
        hi = min([j for j in known if j > k], default=None)
        # 앞·뒤 끝은 대기 칼 각도(REST_ANGLE)로 이어진다 — 마지막 각도를 그대로 들고 있으면 손이 내려와도 칼이 위를 향했다
        a0, i0 = (frames[lo]["angle"], lo) if lo is not None else (REST_ANGLE, -1)
        a1, i1 = (frames[hi]["angle"], hi) if hi is not None else (REST_ANGLE, len(frames))
        d = (a1 - a0 + 180) % 360 - 180
        f["angle"] = a0 + d * (k - i0) / (i1 - i0)


def main():
    os.makedirs(OUT, exist_ok=True)
    pal = palette()
    ref = lock(Image.open(os.path.join(SRC, "walk_loop", "0.png")), pal)
    sy = np.nonzero(near(ref, SCARF).any(axis=1))[0]
    scarf_top_ref = int(sy.min())
    meta = {}
    for clip, spec in CLIPS.items():
        folder, idx, frame, loop, align = spec[:5]
        weapon = len(spec) > 5 and spec[5]
        blade_len = spec[5] if len(spec) > 5 and not isinstance(spec[5], bool) else BLADE_LEN
        frames = []
        last_h, last_b = list(IDLE_HAND), list(IDLE_HAND_B)  # 대기 손에서 시작 — 동작 첫 프레임부터 같은 팔
        xplan = None
        if clip in ALIGN_X:
            pre = []
            for i in idx:
                r = np.array(Image.open(os.path.join(SRC, i + ".png") if isinstance(i, str) else os.path.join(SRC, folder, f"{i}.png")).convert("RGBA"))
                pre.append(strip_weapon(r, blade_len)[0] if weapon else r)
            xplan = plan_x(pre, clip)
        for k, i in enumerate(idx):
            raw = np.array(Image.open(os.path.join(SRC, i + ".png") if isinstance(i, str) else os.path.join(SRC, folder, f"{i}.png")).convert("RGBA"))  # "폴더/번호" = 여러 생성 구간 이어 붙이기(키 자세 방식)
            angle, grip = None, None
            if weapon:
                # 칼은 팔레트 고정 전에 지운다 — 고정이 먼저면 강철 색이 주인공 색으로 바뀌어 칼을 못 찾았다
                raw, grip, angle = strip_weapon(raw, blade_len)
            a = lock(Image.fromarray(raw, "RGBA"), pal)
            if not weapon:
                # 몸에서 떨어진 작은 조각(시전 반짝이·총구 불꽃·속도선 — 생성 그림의 이펙트)은 지운다. 이펙트는 따로 붙인다
                parts = components(a[..., 3] > 0)
                if parts:
                    big = max(len(q) for q in parts)
                    for q in parts:
                        if len(q) < max(9, big * 0.04):
                            for y, x in q:
                                a[y, x] = 0
                f = fill_holes(a)
                if ((f[..., 3] > 0) & (a[..., 3] == 0)).sum() <= 4:
                    a = f  # 몸 안 1~4px 투명 구멍(생성 그림의 잡티 — 배경이 비쳐 픽셀이 깨져 보였다)만 메운다. 큰 틈은 의도된 모양일 수 있어 둔다
            if align == "const" and k == 0:
                # 루프 전체에서 가장 낮은 발을 땅(54행)에 — 첫 프레임 기준이면 다른 프레임 발이 땅을 1px 파고들었다
                ref_idx = CONST_REF.get(folder, idx)  # 같은 초안 폴더를 쓰는 클립끼리는 같은 높이(매달리기 ↔ 오르기 전환 때 5px 튀었다)
                lows = [int(np.nonzero(np.array(Image.open(os.path.join(SRC, folder, f"{j}.png")).convert("RGBA"))[..., 3].any(axis=1))[0].max()) for j in ref_idx]
                const_dy = (FEET_ROW - 1) - max(lows)
                dy = const_dy
            elif align == "feet":
                ys = np.nonzero(a[..., 3].any(axis=1))[0]
                dy = (FEET_ROW - 1) - int(ys.max())
            elif align == "const":
                # 반복 동작은 첫 프레임 높이로 고정 — 프레임마다 발을 맞추면 원래 그림의 체공 출렁임이 사라지고 엉뚱한 데서 1px 튀었다
                dy = const_dy
            else:
                s = np.nonzero(near(a, SCARF).any(axis=1))[0]
                dy = scarf_top_ref - int(s.min())
            a = shift(a, dy)
            sx = XSHIFT.get(clip, [0] * len(idx))[k] + (xplan[k] if xplan else 0)
            a = np.roll(a, sx, axis=1)
            if clip in HEAD_LOCK:
                a = lock_head(a)
            h, b = hands(a)
            if h and b and h != b and not weapon:
                # 무기 손 = 직전 프레임 무기 손에 더 가까운 손(10-26 밤 사용자: 점프 중 무기 위치가 이상) — 프레임마다 '피부가 가장 많은 손'을
                # 고르면 팔을 벌린 점프·착지에서 앞팔·뒤팔이 번갈아 뽑혀 칼이 몸 앞뒤로 순간 이동하며 덜그럭거렸다
                d = lambda p: (p[0] - last_h[0]) ** 2 + (p[1] - last_h[1]) ** 2
                if d(b) < d(h):
                    h, b = b, h
            if not h and not weapon:
                h = hand_near(a, last_h)  # 다리를 접은 공중·착지 프레임은 손이 엉덩이 줄 아래로 내려가 못 찾았다 — 직전 손 근처 피부
            if weapon and grip is not None:
                h = fist_center(a, [grip[0] + sx, grip[1] + dy], r=14)
            if (clip, k) in HAND_FIX:
                h = list(HAND_FIX[(clip, k)])
            h = list(h) if h else last_h
            b = list(b) if b else last_b
            last_h, last_b = h, b
            name = f"{clip}_{k}"
            Image.fromarray(a, "RGBA").save(os.path.join(OUT, name + ".png"))
            if weapon:
                # 초안의 주먹(뭉개진 피부·외곽선)을 공용 주먹 자리에서 지운다 — 남겨 두면 공용 주먹·칼과 겹쳐 손이 뭉개져 보였다(10-27 사용자 캡처)
                fl = common_fist(h)
                fm = fl[..., 3] > 0
                grow = fm.copy()
                for oy in (-1, 0, 1):
                    for ox in (-1, 0, 1):
                        grow |= np.roll(np.roll(fm, oy, axis=0), ox, axis=1)
                drop = grow & (near(a, SKIN) | (a[..., :3].astype(int).sum(axis=2) < 60)) & (a[..., 3] > 0)
                a[drop & ~fm] = 0
                a[fm] = fl[fm]
                Image.fromarray(a, "RGBA").save(os.path.join(OUT, name + ".png"))
            else:
                fl = fist(a, h)
            Image.fromarray(fl, "RGBA").save(os.path.join(OUT, name + "_fist.png"))
            if clip in SKEL_HAND:
                h = [SKEL_HAND[clip][k][0] + sx, SKEL_HAND[clip][k][1] + dy]
                angle = float(SKEL_ANGLE[clip][k])
            angle = ANGLE_FIX.get((clip, k), angle)
            fr = {"png": name, "hand": h, "handB": b, "dy": dy, "sx": sx, "time": TIMES.get(clip, [0.0] * len(idx))[k], "angle": angle if angle is not None else -999.0}  # -999 = 리그 규칙(손 위치로)
            frames.append(fr)
        if weapon:
            fill_angles(frames)
            smooth_hands(frames)
        meta[clip] = {"frame": frame, "loop": loop, "frames": frames}
        print(clip, len(frames), [f["dy"] for f in frames], [f["hand"] for f in frames])
    meta["Idle"] = synth_idle(meta)
    with open(os.path.join(OUT, "cel.json"), "w") as fp:
        json.dump(meta, fp, indent=1)


if __name__ == "__main__":
    main()
