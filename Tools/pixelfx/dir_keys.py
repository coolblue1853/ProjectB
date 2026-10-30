"""몸 방향이 바뀌는 프레임(정면·뒷모습·3/4)의 골격 키(10-29 밤, 회전 베기·돌려차기 — 측면 좌우 반전 폐기 후).
방향 기준 그림의 추정 골격(dirs/skel.json)에서 손목·발목·골반을 옮기고 팔꿈치·무릎은 중간점에서 꺾어 만든다(64px 좌표).
출력: keys json(동작 이름 → kp·hand·angle·times) — m41_submit <json> <이름> <문구> <seed> <방향> 으로 보낸다.
사용: python Tools/pixelfx/dir_keys.py <출력.json>
"""
import json
import math
import os
import sys

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
SKEL = json.load(open(os.path.join(ROOT, "Docs", "ArtCompare", "CharacterGen", "dirs", "skel.json"), encoding="utf-8"))
# 측면(east) = 주인공 측면 원본 골격(m41/first_frame.json) — m41_submit 에 방향 인자를 주지 않으면 측면으로 생성(10-30 품질 6회차)
# 측면에서 RIGHT = 몸 뒤쪽(화면 왼쪽) 팔·다리, LEFT = 앞쪽(화면 오른쪽)
SKEL["side"] = json.load(open(os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel", "m41", "first_frame.json"), encoding="utf-8"))["first_frame_keypoints"]

# 프레임 = (방향, {관절: (x, y)}, 무기 각도) — 0 = 화면 오른쪽, 90 = 위. 'shift' 는 몸 전체 이동
# 회전 베기(양손 무기): 측면 장전(기존 양손 수평 셀) → 3/4 뒤(무기 뒤에 끌림) → 뒷모습(무기 화면 왼쪽 수평) → 정면(무기 화면 오른쪽 수평) → 3/4 앞(앞으로 휩쓸며) → 측면 타격(기존 셀)
MOTIONS = {
    # 두 손은 한 손잡이(1~2px 차이). 짧은 팔로 몸 옆까지 쥐려면 먼 쪽 어깨를 몸 가운데로 비튼다(허리 비틀기)
    "D_SpinNE": [
        ("ne", {"RIGHT SHOULDER": (37, 37), "RIGHT ARM": (29, 44), "LEFT ARM": (28, 45), "LEFT LEG": (28, 52), "RIGHT LEG": (42, 52), "shift": (0, 1)}, 200),
        ("ne", {"RIGHT SHOULDER": (36, 37), "RIGHT ARM": (27, 42), "LEFT ARM": (26, 43), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 1)}, 185),
        ("ne", {"RIGHT SHOULDER": (35, 37), "RIGHT ARM": (26, 41), "LEFT ARM": (25, 42), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 1)}, 175),
    ],
    "D_SpinN": [
        ("north", {"RIGHT SHOULDER": (35, 37), "RIGHT ARM": (27, 40), "LEFT ARM": (26, 41), "LEFT LEG": (25, 53), "RIGHT LEG": (44, 53), "shift": (-1, 1)}, 180),
        ("north", {"RIGHT SHOULDER": (34, 37), "RIGHT ARM": (25, 39), "LEFT ARM": (24, 40), "LEFT LEG": (25, 53), "RIGHT LEG": (44, 53), "shift": (-1, 1)}, 175),
        ("north", {"RIGHT SHOULDER": (37, 36), "RIGHT ARM": (31, 36), "LEFT ARM": (30, 37), "LEFT LEG": (25, 53), "RIGHT LEG": (44, 53), "shift": (0, 1)}, 160),
    ],
    "D_SpinS": [
        ("south", {"LEFT SHOULDER": (31, 36), "RIGHT ARM": (43, 40), "LEFT ARM": (42, 41), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (1, 1)}, 0),
        ("south", {"LEFT SHOULDER": (32, 36), "RIGHT ARM": (45, 39), "LEFT ARM": (44, 40), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (1, 1)}, 5),
        ("south", {"LEFT SHOULDER": (31, 37), "RIGHT ARM": (42, 44), "LEFT ARM": (41, 45), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (1, 1)}, -20),
    ],
    "D_SpinSE": [
        ("se", {"RIGHT SHOULDER": (39, 36), "RIGHT ARM": (50, 41), "LEFT ARM": (51, 41), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 1)}, -5),
        ("se", {"RIGHT SHOULDER": (40, 36), "RIGHT ARM": (51, 40), "LEFT ARM": (52, 40), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 1)}, 0),
        ("se", {"RIGHT SHOULDER": (39, 37), "RIGHT ARM": (49, 44), "LEFT ARM": (50, 44), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 1)}, -15),
    ],
    # 돌려차기: 측면 → 3/4 뒤(뒤돌며 무릎 듦) → 뒷모습(무릎 옆으로 높이) → 측면 차기(기존 돌려차기 셀 뻗음)
    "D_KickNE": [
        ("ne", {"RIGHT ARM": (46, 40), "LEFT ARM": (24, 42), "RIGHT LEG": (44, 51), "shift": (0, 0)}, 0),
        ("ne", {"RIGHT ARM": (47, 38), "LEFT ARM": (22, 40), "RIGHT LEG": (48, 47), "shift": (-1, 0)}, 0),
        ("ne", {"RIGHT ARM": (47, 37), "LEFT ARM": (21, 39), "RIGHT LEG": (53, 43), "shift": (-1, -1)}, 0),
    ],
    "D_KickN": [
        ("north", {"RIGHT ARM": (48, 39), "LEFT ARM": (22, 41), "RIGHT LEG": (50, 47), "LEFT LEG": (33, 53), "shift": (-1, 0)}, 0),
        ("north", {"RIGHT ARM": (49, 37), "LEFT ARM": (21, 40), "RIGHT LEG": (55, 42), "LEFT LEG": (33, 53), "shift": (-2, -1)}, 0),
        ("north", {"RIGHT ARM": (48, 36), "LEFT ARM": (20, 40), "RIGHT LEG": (57, 38), "LEFT LEG": (33, 53), "shift": (-2, -1)}, 0),
    ],
    # 로프 오르기(뒷모습, 메이플식): 줄은 몸 가운데(x≈35). 손을 번갈아 머리 옆까지, 같은 쪽 무릎을 들어 디딤 — 4장 반복
    "D_RopeN": [
        ("north", {"RIGHT ARM": (37, 29), "LEFT ARM": (33, 35), "RIGHT LEG": (38, 49), "LEFT LEG": (32, 53)}, 0),
        ("north", {"RIGHT ARM": (37, 31), "LEFT ARM": (33, 32), "RIGHT LEG": (38, 51), "LEFT LEG": (32, 51), "shift": (0, -1)}, 0),
        ("north", {"RIGHT ARM": (37, 35), "LEFT ARM": (33, 29), "RIGHT LEG": (38, 53), "LEFT LEG": (32, 49)}, 0),
        ("north", {"RIGHT ARM": (37, 32), "LEFT ARM": (33, 31), "RIGHT LEG": (38, 51), "LEFT LEG": (32, 51), "shift": (0, -1)}, 0),
    ],
    # 10-29 밤 2차(GPT 방향 혼합 판정 §19): 측면 셀의 장전·유지 키를 방향 프레임으로 바꿔 끼운다(각 3장 = 2생성)
    "D_IaidoNE": [   # 발도 장전: 등을 보이며 낮게 웅크려 왼 허리 칼자루를 쥠
        ("ne", {"RIGHT SHOULDER": (37, 40), "RIGHT ARM": (32, 44), "LEFT ARM": (30, 45), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52), "shift": (1, 2)}, -120),
        ("ne", {"RIGHT SHOULDER": (37, 42), "RIGHT ARM": (31, 46), "LEFT ARM": (29, 47), "LEFT LEG": (27, 52), "RIGHT LEG": (44, 52), "shift": (1, 4)}, -125),
        ("ne", {"RIGHT SHOULDER": (39, 41), "RIGHT ARM": (36, 44), "LEFT ARM": (29, 46), "LEFT LEG": (27, 52), "RIGHT LEG": (44, 52), "shift": (2, 3)}, -60),
    ],
    "D_SheatheSE": [  # 납도: 3/4 앞으로 돌아서며 칼을 턱 옆에 세움 → 왼 허리 칼집으로
        ("se", {"RIGHT ARM": (37, 31), "LEFT ARM": (42, 45)}, 90),
        ("se", {"RIGHT SHOULDER": (37, 36), "RIGHT ARM": (40, 41), "LEFT ARM": (43, 45)}, 160),
        ("se", {"RIGHT SHOULDER": (37, 36), "RIGHT ARM": (41, 43), "LEFT ARM": (43, 44), "shift": (0, -1)}, 175),
    ],
    "D_ThrowNE": [   # 던지기 장전: 등 돌려 던지는 손을 귀 뒤로
        ("ne", {"RIGHT ARM": (42, 30), "LEFT ARM": (28, 41), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52)}, 110),
        ("ne", {"RIGHT ARM": (41, 29), "LEFT ARM": (27, 40), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52), "shift": (-1, 0)}, 120),
        ("ne", {"RIGHT ARM": (43, 32), "LEFT ARM": (28, 41), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52)}, 90),
    ],
    "D_UpperNE": [   # 양손 올려치기 장전: 등 돌려 무기를 뒤 아래로 낮게 끌기
        ("ne", {"RIGHT SHOULDER": (37, 39), "RIGHT ARM": (31, 47), "LEFT ARM": (30, 47), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 2)}, -140),
        ("ne", {"RIGHT SHOULDER": (37, 40), "RIGHT ARM": (30, 48), "LEFT ARM": (29, 48), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 3)}, -145),
        ("ne", {"RIGHT SHOULDER": (38, 39), "RIGHT ARM": (34, 46), "LEFT ARM": (33, 46), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (0, 2)}, -110),
    ],
    "D_PullS": [     # 당기기: 정면으로 버티며 두 손을 가슴으로
        ("south", {"RIGHT ARM": (36, 40), "LEFT ARM": (34, 40), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (-1, 0)}, 5),
        ("south", {"RIGHT ARM": (34, 41), "LEFT ARM": (32, 41), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (-2, 1)}, 10),
        ("south", {"RIGHT ARM": (35, 42), "LEFT ARM": (33, 42), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (-2, 1)}, 15),
    ],
    "D_RaiseS": [    # 양손 들기: 정면, 손은 머리 양옆(머리 위로 겹치면 팔이 사라진다 — GPT)
        ("south", {"RIGHT ARM": (41, 33), "LEFT ARM": (23, 33)}, 60),
        ("south", {"RIGHT ARM": (42, 30), "LEFT ARM": (22, 30), "shift": (0, -1)}, 85),
        ("south", {"RIGHT ARM": (43, 29), "LEFT ARM": (21, 29), "LEFT LEG": (26, 52), "RIGHT LEG": (38, 52), "shift": (0, -1)}, 90),
    ],
    "D_BashNE": [    # 방패 밀치기 장전: 등 돌려 방패를 가슴에 웅크림
        ("ne", {"RIGHT ARM": (40, 44), "LEFT ARM": (30, 40), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52), "shift": (-1, 1)}, -100),
        ("ne", {"RIGHT ARM": (40, 45), "LEFT ARM": (30, 41), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52), "shift": (-2, 2)}, -100),
        ("ne", {"RIGHT ARM": (41, 44), "LEFT ARM": (33, 39), "LEFT LEG": (28, 52), "RIGHT LEG": (43, 52), "shift": (0, 1)}, -95),
    ],
    "D_SweepNE": [   # 하단 쓸기 장전: 등 돌려 창을 뒤로 낮게
        ("ne", {"RIGHT SHOULDER": (37, 38), "RIGHT ARM": (31, 44), "LEFT ARM": (29, 45), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 2)}, 165),
        ("ne", {"RIGHT SHOULDER": (37, 39), "RIGHT ARM": (30, 46), "LEFT ARM": (28, 46), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 3)}, 170),
        ("ne", {"RIGHT SHOULDER": (38, 39), "RIGHT ARM": (33, 47), "LEFT ARM": (31, 47), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (0, 3)}, 200),
    ],
    "D_VictoryS": [  # 승리 포즈(직업 소개): 정면, 무기를 귀 옆에 세우고 반대손 허리
        ("south", {"RIGHT ARM": (40, 40), "LEFT ARM": (25, 44)}, 45),
        ("south", {"RIGHT ARM": (42, 33), "LEFT ARM": (25, 44), "LEFT LEG": (25, 52), "RIGHT LEG": (39, 52)}, 80),
        ("south", {"RIGHT ARM": (42, 31), "LEFT ARM": (25, 44), "LEFT LEG": (25, 52), "RIGHT LEG": (39, 52), "shift": (0, -1)}, 90),
    ],
    "D_PunchNE": [   # 원투의 투 장전: 골반을 돌려 등을 보이며 뒷주먹을 가까운 어깨 옆에 장전, 앞손은 가드
        ("ne", {"RIGHT ARM": (43, 40), "LEFT ARM": (36, 36), "LEFT LEG": (29, 52), "RIGHT LEG": (42, 52), "shift": (-1, 0)}, 0),
        ("ne", {"RIGHT ARM": (44, 41), "LEFT ARM": (35, 35), "LEFT LEG": (29, 52), "RIGHT LEG": (43, 52), "shift": (-2, 1)}, 0),
        ("ne", {"RIGHT ARM": (44, 38), "LEFT ARM": (36, 35), "LEFT LEG": (29, 52), "RIGHT LEG": (43, 52), "shift": (-1, 0)}, 0),
    ],
    "D_PunchSE": [   # 뒷주먹이 가슴 앞을 지나 나가는 순간(3/4 앞): 어깨 비틀어 앞으로, 앞손은 턱 가드
        ("se", {"RIGHT SHOULDER": (37, 36), "RIGHT ARM": (41, 38), "LEFT ARM": (43, 33), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (1, 0)}, 0),
        ("se", {"RIGHT SHOULDER": (38, 36), "RIGHT ARM": (44, 37), "LEFT ARM": (43, 33), "RIGHT LEG": (31, 52), "LEFT LEG": (47, 52), "shift": (2, 0)}, 0),
        ("se", {"RIGHT SHOULDER": (39, 36), "RIGHT ARM": (46, 37), "LEFT ARM": (44, 34), "RIGHT LEG": (31, 52), "LEFT LEG": (48, 52), "shift": (2, 0)}, 0),
    ],
    "D_GunSE": [     # 검 든 채 사격(해적): 3/4 앞, 앞손(권총) 어깨 높이로 앞, 검손은 반대 허리 뒤로 빼 두 무기 분리(GPT 리뷰)
        ("se", {"LEFT ARM": (50, 38), "RIGHT ARM": (31, 44), "RIGHT LEG": (31, 52), "LEFT LEG": (48, 52)}, 200),
        ("se", {"LEFT ARM": (49, 36), "RIGHT ARM": (31, 44), "RIGHT LEG": (31, 52), "LEFT LEG": (48, 52), "shift": (-1, 0)}, 200),
        ("se", {"LEFT ARM": (51, 38), "RIGHT ARM": (32, 45), "RIGHT LEG": (31, 52), "LEFT LEG": (48, 52), "shift": (1, 0)}, 205),
    ],
    # 빠진 동작 후보(MotionCoverage N3 춤 9직업·N2 악기 9직업) — 정면/3/4 앞이 읽기 좋다
    "D_DanceS": [   # 춤·스텝 4장 반복: 팔 벌림 ↔ 위로, 발 교차
        ("south", {"RIGHT ARM": (44, 40), "LEFT ARM": (20, 40), "LEFT LEG": (26, 52), "RIGHT LEG": (38, 52)}, 0),
        ("south", {"RIGHT ARM": (43, 31), "LEFT ARM": (21, 31), "LEFT LEG": (33, 52), "RIGHT LEG": (37, 49), "shift": (1, -1)}, 0),
        ("south", {"RIGHT ARM": (44, 40), "LEFT ARM": (20, 40), "LEFT LEG": (26, 52), "RIGHT LEG": (38, 52), "shift": (-1, 0)}, 0),
        ("south", {"RIGHT ARM": (43, 31), "LEFT ARM": (21, 31), "LEFT LEG": (27, 49), "RIGHT LEG": (31, 52), "shift": (-1, -1)}, 0),
    ],
    "D_LuteSE": [   # 악기 연주(류트·기타): 3/4 앞, 앞손은 목(줄감개) 쪽, 뒷손은 배 앞에서 튕김 — 3장 반복
        ("se", {"LEFT ARM": (49, 38), "RIGHT ARM": (39, 44)}, 30),
        ("se", {"LEFT ARM": (49, 38), "RIGHT ARM": (40, 41), "shift": (0, 1)}, 30),
        ("se", {"LEFT ARM": (49, 39), "RIGHT ARM": (38, 45)}, 30),
    ],
    # 한손 베기 공용(돌진 횡베기·커틀러스 수평, GPT 2차 리뷰 "골반·어깨 회전 부족"): 3/4 뒤 감기 → (측면 타격) → 3/4 앞 팔로스루
    "D_CoilNE": [
        ("ne", {"RIGHT SHOULDER": (37, 39), "RIGHT ARM": (31, 44), "LEFT ARM": (37, 40), "LEFT LEG": (27, 52), "RIGHT LEG": (43, 52), "shift": (-1, 2)}, 190),
        ("ne", {"RIGHT SHOULDER": (37, 40), "RIGHT ARM": (30, 45), "LEFT ARM": (37, 41), "LEFT LEG": (27, 52), "RIGHT LEG": (44, 52), "shift": (-1, 3)}, 195),
        ("ne", {"RIGHT SHOULDER": (38, 39), "RIGHT ARM": (33, 43), "LEFT ARM": (38, 40), "LEFT LEG": (27, 52), "RIGHT LEG": (44, 52), "shift": (0, 2)}, 175),
    ],
    "D_FollowSE": [
        ("se", {"RIGHT SHOULDER": (39, 37), "RIGHT ARM": (47, 41), "LEFT ARM": (43, 45), "RIGHT LEG": (30, 52), "LEFT LEG": (49, 52), "shift": (2, 2)}, -10),
        ("se", {"RIGHT SHOULDER": (39, 38), "RIGHT ARM": (47, 43), "LEFT ARM": (43, 46), "RIGHT LEG": (30, 52), "LEFT LEG": (49, 52), "shift": (2, 3)}, -20),
        ("se", {"RIGHT SHOULDER": (38, 37), "RIGHT ARM": (45, 43), "LEFT ARM": (42, 45), "RIGHT LEG": (31, 52), "LEFT LEG": (48, 52), "shift": (1, 2)}, -25),
    ],
    "D_CrossS": [    # 교차 베기(쌍검) 정면 X(GPT D3): 두 손 머리 양옆 위로 엇갈려 → 가슴 앞에서 X 로 내려 벰 → 양옆 아래로 벌림
        ("south", {"RIGHT ARM": (37, 30), "LEFT ARM": (27, 30), "LEFT LEG": (25, 52), "RIGHT LEG": (39, 52), "shift": (0, -1)}, 135),
        ("south", {"RIGHT ARM": (29, 41), "LEFT ARM": (35, 41), "LEFT LEG": (24, 52), "RIGHT LEG": (40, 52), "shift": (0, 2)}, -135),
        ("south", {"RIGHT ARM": (42, 45), "LEFT ARM": (22, 45), "LEFT LEG": (24, 52), "RIGHT LEG": (40, 52), "shift": (0, 2)}, -40),
    ],
    "D_ChargeS": [   # 모으기(마도학자): 정면, 두 팔을 가슴 앞에 엇갈려 웅크림 → 아래로 활짝 펼침(시전·소환과 몸 자세가 달라 보이게 — GPT 2차)
        ("south", {"RIGHT ARM": (29, 38), "LEFT ARM": (35, 38), "LEFT LEG": (26, 52), "RIGHT LEG": (38, 52), "shift": (0, 2)}, 120),
        ("south", {"RIGHT ARM": (30, 37), "LEFT ARM": (34, 37), "LEFT LEG": (26, 52), "RIGHT LEG": (38, 52), "shift": (0, 3)}, 120),
        ("south", {"RIGHT ARM": (44, 43), "LEFT ARM": (20, 43), "LEFT LEG": (24, 52), "RIGHT LEG": (40, 52), "shift": (0, -1)}, 60),
    ],
    "D_GetupS": [    # 넘어짐 → 일어남(GPT D1): 정면으로 몸을 틀어 한쪽 무릎 꿇고 한 손 땅 짚기 → 밀며 일어섬
        ("south", {"LEFT ARM": (24, 50), "RIGHT ARM": (38, 46), "LEFT LEG": (26, 52), "RIGHT LEG": (40, 52), "shift": (-1, 6)}, -20),
        ("south", {"LEFT ARM": (25, 49), "RIGHT ARM": (39, 44), "LEFT LEG": (26, 52), "RIGHT LEG": (40, 52), "shift": (0, 4)}, -20),
        ("south", {"LEFT ARM": (26, 45), "RIGHT ARM": (40, 43), "LEFT LEG": (27, 52), "RIGHT LEG": (38, 52), "shift": (0, 2)}, -20),
    ],
    "D_PalmSE": [    # 장풍(N6): 3/4 앞, 두 손을 허리에 모아 웅크림 → 두 손바닥을 가슴 높이로 함께 앞으로
        ("se", {"RIGHT SHOULDER": (37, 37), "RIGHT ARM": (41, 45), "LEFT ARM": (44, 45), "RIGHT LEG": (30, 52), "LEFT LEG": (47, 52), "shift": (-1, 3)}, -20),
        ("se", {"RIGHT SHOULDER": (39, 36), "RIGHT ARM": (48, 40), "LEFT ARM": (51, 39), "RIGHT LEG": (30, 52), "LEFT LEG": (49, 52), "shift": (2, 1)}, -20),
        ("se", {"RIGHT SHOULDER": (40, 36), "RIGHT ARM": (49, 39), "LEFT ARM": (52, 38), "RIGHT LEG": (30, 52), "LEFT LEG": (50, 52), "shift": (3, 1)}, -20),
    ],
    # 10-30 품질 루프(GPT): 돌려차기 = 뒷모습 다음 3/4 앞에서 골반을 틀며 차는 발을 골반 높이로 오른쪽 끝까지, 상체는 반대(왼)로 기울임
    "D_KickSE": [   # 2판: 3/4 앞에선 화면 오른쪽 다리 = LEFT(추정 골격) — 그 다리로 오른쪽 골반 높이까지
        ("se", {"RIGHT ARM": (32, 38), "LEFT ARM": (41, 37), "RIGHT LEG": (34, 52), "LEFT LEG": (51, 43), "shift": (-3, 0)}, 0),
        ("se", {"RIGHT ARM": (31, 37), "LEFT ARM": (40, 36), "RIGHT LEG": (34, 52), "LEFT LEG": (52, 41), "shift": (-4, 0)}, 0),
        ("se", {"RIGHT ARM": (32, 39), "LEFT ARM": (41, 38), "RIGHT LEG": (35, 52), "LEFT LEG": (50, 46), "shift": (-2, 0)}, 0),
    ],
    # 장풍 2판: 더 깊게 웅크려 두 손 허리 뒤 → 앞발 크게 내딛으며 가슴 앞 최대로
    "D_PalmSE2": [
        ("se", {"RIGHT SHOULDER": (36, 38), "RIGHT ARM": (33, 46), "LEFT ARM": (37, 47), "RIGHT LEG": (29, 52), "LEFT LEG": (46, 52), "shift": (-2, 4)}, -20),
        ("se", {"RIGHT SHOULDER": (40, 36), "RIGHT ARM": (49, 39), "LEFT ARM": (52, 38), "RIGHT LEG": (28, 52), "LEFT LEG": (52, 52), "shift": (3, 2)}, -20),
        ("se", {"RIGHT SHOULDER": (41, 36), "RIGHT ARM": (50, 39), "LEFT ARM": (53, 38), "RIGHT LEG": (28, 52), "LEFT LEG": (53, 52), "shift": (4, 2)}, -20),
    ],
    "D_GrabSE": [    # 10-30 품질 루프(GPT 잡기 5.0): 3/4 앞 — 두 팔 벌려 웅크림 → 앞으로 모아 움켜쥠 → 갈비뼈로 당기며 젖힘
        ("se", {"RIGHT ARM": (31, 38), "LEFT ARM": (51, 37), "RIGHT LEG": (31, 52), "LEFT LEG": (48, 52), "shift": (0, 2)}, -20),
        ("se", {"RIGHT SHOULDER": (38, 36), "RIGHT ARM": (47, 40), "LEFT ARM": (50, 40), "RIGHT LEG": (30, 52), "LEFT LEG": (50, 52), "shift": (2, 2)}, -20),
        ("se", {"RIGHT SHOULDER": (37, 37), "RIGHT ARM": (42, 42), "LEFT ARM": (45, 42), "RIGHT LEG": (29, 52), "LEFT LEG": (47, 52), "shift": (-2, 1)}, -20),
    ],
    "D_PlaceSE": [   # 10-30 품질 루프(GPT 놓기 5.5): 3/4 앞 — 배 앞에 들고 → 골반 크게 낮춰 손을 앞발 옆 땅까지 → 손 떼고 회수
        ("se", {"RIGHT ARM": (42, 44), "LEFT ARM": (45, 44), "RIGHT LEG": (32, 52), "LEFT LEG": (47, 52), "shift": (0, 2)}, -20),
        ("se", {"RIGHT ARM": (47, 51), "LEFT ARM": (50, 51), "RIGHT LEG": (31, 52), "LEFT LEG": (49, 52), "shift": (2, 6)}, -20),
        ("se", {"RIGHT ARM": (44, 47), "LEFT ARM": (46, 47), "RIGHT LEG": (31, 52), "LEFT LEG": (49, 52), "shift": (1, 5)}, -20),
    ],
    "D_BowNE": [     # 10-30 품질 루프(GPT 활 5.5 "시위를 턱까지 당겼다 놓는 차이가 없다"): 3/4 뒤 — 활 든 손 앞으로 뻗고 당기는 손 턱 옆까지 → 놓으며 뒤로 튕김
        ("ne", {"LEFT ARM": (50, 36), "RIGHT ARM": (44, 37), "LEFT LEG": (30, 52), "RIGHT LEG": (42, 52)}, 0),
        ("ne", {"LEFT ARM": (51, 35), "RIGHT ARM": (40, 34), "LEFT LEG": (30, 52), "RIGHT LEG": (42, 52), "shift": (-1, 0)}, 0),
        ("ne", {"LEFT ARM": (51, 35), "RIGHT ARM": (36, 33), "LEFT LEG": (30, 52), "RIGHT LEG": (42, 52), "shift": (-1, 0)}, 0),
    ],
    # 10-30 품질 루프 4회차(GPT 장풍 5.5 "타격 때 두 팔·어깨가 덜 펼쳐짐"): 측면 쪽으로 몸을 열며 두 손바닥을 위아래로 벌려 크게 내밂
    "D_PalmE": [
        ("se", {"RIGHT SHOULDER": (41, 36), "RIGHT ARM": (50, 35), "LEFT ARM": (53, 42), "RIGHT LEG": (27, 52), "LEFT LEG": (53, 52), "shift": (4, 2)}, -20),
        ("se", {"RIGHT SHOULDER": (42, 36), "RIGHT ARM": (51, 34), "LEFT ARM": (54, 42), "RIGHT LEG": (27, 52), "LEFT LEG": (54, 52), "shift": (5, 2)}, -20),
        ("se", {"RIGHT SHOULDER": (41, 37), "RIGHT ARM": (49, 36), "LEFT ARM": (52, 43), "RIGHT LEG": (28, 52), "LEFT LEG": (52, 52), "shift": (4, 3)}, -20),
    ],
    # 메치기 들기 2판(GPT 5.5 "들어 올리는 높이·무게"): 정면 — 무릎 굽혀 버티며 두 손을 머리 위로 높이(무거운 것) → 뒤로 젖힘
    "D_LiftS": [
        ("south", {"RIGHT ARM": (40, 30), "LEFT ARM": (24, 30), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (0, 3)}, 90),
        ("south", {"RIGHT ARM": (39, 28), "LEFT ARM": (25, 28), "LEFT LEG": (23, 52), "RIGHT LEG": (41, 52), "shift": (0, 1)}, 90),
        ("south", {"RIGHT ARM": (38, 29), "LEFT ARM": (26, 29), "LEFT LEG": (24, 52), "RIGHT LEG": (40, 52), "shift": (0, 0)}, 90),
    ],
    # 품질 5회차: 메치기 내리꽂는 순간(3/4 앞) — 두 손을 앞 땅까지 내리꽂고 골반 깊게(GPT "하강 폭이 작다")
    "D_SlamSE": [
        ("se", {"RIGHT ARM": (46, 49), "LEFT ARM": (49, 49), "RIGHT LEG": (29, 52), "LEFT LEG": (50, 52), "shift": (2, 5)}, -60),
        ("se", {"RIGHT ARM": (48, 52), "LEFT ARM": (51, 52), "RIGHT LEG": (29, 52), "LEFT LEG": (51, 52), "shift": (3, 7)}, -70),
        ("se", {"RIGHT ARM": (46, 50), "LEFT ARM": (49, 50), "RIGHT LEG": (29, 52), "LEFT LEG": (50, 52), "shift": (2, 6)}, -60),
    ],
    # 장풍 방출 뒤 반동(3/4 앞) — 두 팔 뻗은 채 상체가 뒤로 밀림(GPT "방출 반동이 약함")
    "D_PalmRecoil": [
        ("se", {"RIGHT SHOULDER": (39, 36), "RIGHT ARM": (47, 36), "LEFT ARM": (50, 42), "RIGHT LEG": (28, 52), "LEFT LEG": (51, 52), "shift": (0, 2)}, -20),
        ("se", {"RIGHT SHOULDER": (38, 36), "RIGHT ARM": (45, 37), "LEFT ARM": (48, 42), "RIGHT LEG": (28, 52), "LEFT LEG": (50, 52), "shift": (-2, 1)}, -20),
        ("se", {"RIGHT SHOULDER": (37, 36), "RIGHT ARM": (43, 40), "LEFT ARM": (46, 43), "RIGHT LEG": (29, 52), "LEFT LEG": (49, 52), "shift": (-2, 1)}, -20),
    ],
    # 품질 6회차(GPT 장풍 6.5 "방출 때 SE 비틀기 → E 측면에서 양손을 어깨보다 7~8px 앞으로, 뒷발로 밀기"): 측면 방출 → 유지 → 반동
    "E_PalmSide": [
        ("side", {"RIGHT SHOULDER": (42, 33), "RIGHT ARM": (52, 34), "LEFT ARM": (54, 37), "RIGHT LEG": (30, 53), "LEFT LEG": (51, 53), "shift": (3, 1)}, -20),
        ("side", {"RIGHT SHOULDER": (43, 33), "RIGHT ARM": (53, 33), "LEFT ARM": (55, 37), "RIGHT LEG": (29, 53), "LEFT LEG": (52, 53), "shift": (4, 1)}, -20),
        ("side", {"RIGHT SHOULDER": (40, 33), "RIGHT ARM": (49, 35), "LEFT ARM": (51, 38), "RIGHT LEG": (30, 53), "LEFT LEG": (50, 53), "shift": (1, 1)}, -20),
    ],
    # 품질 6회차(GPT 메치기 6.5 "들 때 상체 15° 젖힘 후 급격히 숙임"): 측면 — 두 손 머리 뒤 위로 들고 상체 젖힘 → 더 젖힘 → 앞으로 넘기기 시작
    "E_SlamBack": [
        ("side", {"RIGHT ARM": (36, 22), "LEFT ARM": (39, 21), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (-3, 1)}, 110),
        ("side", {"RIGHT ARM": (34, 23), "LEFT ARM": (37, 22), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (-4, 1)}, 120),
        ("side", {"RIGHT ARM": (43, 23), "LEFT ARM": (46, 23), "RIGHT LEG": (33, 53), "LEFT LEG": (49, 53), "shift": (1, 2)}, 80),
    ],
    # 품질 6회차(GPT 앞차기 6.0 "무릎을 배 앞까지, 발목 오른쪽 7px, 즉시 회수"): 측면 — 앞다리 무릎 배 높이 장전 → 발바닥 앞으로 뻗음 → 회수
    "E_FrontKick": [
        ("side", {"RIGHT ARM": (40, 38), "LEFT ARM": (47, 36), "LEFT KNEE": (49, 41), "LEFT LEG": (47, 47), "RIGHT LEG": (37, 53), "shift": (-2, 0)}, -20),
        ("side", {"RIGHT ARM": (39, 38), "LEFT ARM": (46, 37), "LEFT LEG": (55, 42), "RIGHT LEG": (36, 53), "shift": (-3, 0)}, -20),
        ("side", {"RIGHT ARM": (40, 38), "LEFT ARM": (47, 37), "LEFT KNEE": (48, 44), "LEFT LEG": (46, 50), "RIGHT LEG": (37, 53), "shift": (-1, 0)}, -20),
    ],
    # 품질 7회차(GPT 앞차기 6.4 "무릎 더 높이·발목 더 멀리·지지 무릎 굽힘"): 골반을 앞으로 내밀고 상체 뒤로
    "E_FrontKick2": [
        ("side", {"RIGHT ARM": (40, 38), "LEFT ARM": (47, 36), "LEFT HIP": (44, 43), "LEFT KNEE": (49, 39), "LEFT LEG": (48, 46), "RIGHT KNEE": (39, 49), "RIGHT LEG": (37, 53), "shift": (-2, 1)}, -20),
        ("side", {"RIGHT ARM": (38, 39), "LEFT ARM": (46, 37), "LEFT HIP": (45, 43), "LEFT LEG": (54, 41), "RIGHT KNEE": (38, 49), "RIGHT LEG": (36, 53), "shift": (-3, 1)}, -20),
        ("side", {"RIGHT ARM": (40, 38), "LEFT ARM": (47, 37), "LEFT KNEE": (49, 42), "LEFT LEG": (46, 48), "RIGHT LEG": (37, 53), "shift": (-1, 1)}, -20),
    ],
    # 품질 7회차(GPT 돌려차기 6.0 "앞차기처럼 읽힘 — SE 타격 뒤 E 팔로스루"): 측면 — 발을 골반보다 높이 휘둘러 지나감 → 흘려 내림 → 착지 준비, 상체 크게 젖힘
    "E_RoundFollow": [
        ("side", {"RIGHT ARM": (32, 38), "LEFT ARM": (44, 36), "LEFT HIP": (45, 43), "LEFT LEG": (53, 37), "RIGHT LEG": (36, 53), "shift": (-4, 1)}, -20),
        ("side", {"RIGHT ARM": (33, 39), "LEFT ARM": (45, 37), "LEFT HIP": (45, 43), "LEFT LEG": (53, 43), "RIGHT LEG": (36, 53), "shift": (-3, 1)}, -20),
        ("side", {"RIGHT ARM": (36, 40), "LEFT ARM": (46, 38), "LEFT KNEE": (48, 46), "LEFT LEG": (49, 52), "RIGHT LEG": (36, 53), "shift": (-1, 1)}, -20),
    ],
    # 품질 7회차(GPT 쌍권총 6.4 "두 총이 같은 높이·같은 타이밍"): 측면 — 주손(RIGHT) 어깨 높이로 뻗어 쏨(왼손 낮게) → 왼손 뻗어 쏨(주손 당겨 낮게) → 둘 다 앞, 높이 차 유지
    "E_DualFire": [
        ("side", {"RIGHT SHOULDER": (41, 33), "RIGHT ARM": (50, 33), "LEFT ARM": (47, 38), "RIGHT LEG": (35, 53), "LEFT LEG": (47, 53), "shift": (-1, 0)}, 0),
        ("side", {"RIGHT SHOULDER": (39, 33), "RIGHT ARM": (44, 38), "LEFT ARM": (53, 32), "RIGHT LEG": (35, 53), "LEFT LEG": (47, 53), "shift": (1, 0)}, 0),
        ("side", {"RIGHT SHOULDER": (40, 33), "RIGHT ARM": (48, 34), "LEFT ARM": (50, 37), "RIGHT LEG": (35, 53), "LEFT LEG": (47, 53)}, 0),
    ],
    # 품질 8회차(GPT 한손 수평 6.3 "무겁지도 빠르지도 않음"): 측면 — 검손 머리 옆 뒤로 짧게 장전 → 앞발 내딛으며 어깨 높이로 8px 앞 수평 타격 → 손목 따라 흘림
    "E_OneSide": [
        ("side", {"RIGHT SHOULDER": (36, 33), "RIGHT ARM": (34, 29), "LEFT ARM": (46, 39), "RIGHT LEG": (35, 53), "LEFT LEG": (46, 53), "shift": (-1, 1)}, 170),
        ("side", {"RIGHT SHOULDER": (41, 33), "RIGHT ARM": (50, 35), "LEFT ARM": (42, 39), "RIGHT LEG": (32, 53), "LEFT LEG": (50, 53), "shift": (2, 1)}, 0),
        ("side", {"RIGHT SHOULDER": (41, 34), "RIGHT ARM": (48, 39), "LEFT ARM": (41, 39), "RIGHT LEG": (32, 53), "LEFT LEG": (50, 53), "shift": (2, 2)}, -25),
    ],
    # 품질 10회차(GPT 검+권총 6.0 "두 무기가 몸 앞에서 겹쳐 교차해 든 것처럼"): 측면 — 권총손(LEFT, 앞) 어깨 높이로 8px 앞, 검손(RIGHT) 뒤 허리 아래, 앞발 내딛음 → 사격 → 반동
    "E_GunE": [
        ("side", {"LEFT ARM": (51, 34), "RIGHT ARM": (34, 42), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (0, 1)}, 200),
        ("side", {"LEFT ARM": (53, 33), "RIGHT ARM": (34, 41), "RIGHT LEG": (33, 53), "LEFT LEG": (50, 53), "shift": (1, 1)}, 200),
        ("side", {"LEFT ARM": (50, 31), "RIGHT ARM": (34, 41), "RIGHT LEG": (33, 53), "LEFT LEG": (50, 53), "shift": (-1, 1)}, 205),
    ],
    # 품질 10회차(GPT 활 6.5 "발사 순간 활손과 시위손 분리 부족"): 측면 — 활손(LEFT) 8px 앞 고정, 시위손(RIGHT) 턱 옆까지 당김 → 놓으며 뒤로 튕김 → 정리
    "E_BowE": [
        ("side", {"LEFT ARM": (53, 33), "RIGHT ARM": (40, 30), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (0, 1)}, 0),
        ("side", {"LEFT ARM": (53, 33), "RIGHT ARM": (32, 31), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (-1, 1)}, 0),
        ("side", {"LEFT ARM": (51, 35), "RIGHT ARM": (35, 37), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (-1, 1)}, 0),
    ],
    # 품질 10회차(GPT 잡기 6.5 "잡는 순간의 고정과 끌어당기는 힘"): 측면 — 두 손 어깨 높이로 뻗음 → 내딛으며 앞 아래에서 움켜쥠 → 골반 뒤로 빼며 당김
    "E_GrabE": [
        ("side", {"RIGHT SHOULDER": (40, 33), "RIGHT ARM": (48, 34), "LEFT ARM": (51, 33), "RIGHT LEG": (34, 53), "LEFT LEG": (48, 53), "shift": (1, 2)}, -20),
        ("side", {"RIGHT SHOULDER": (41, 34), "RIGHT ARM": (49, 37), "LEFT ARM": (52, 36), "RIGHT LEG": (33, 53), "LEFT LEG": (51, 53), "shift": (2, 2)}, -20),
        ("side", {"RIGHT SHOULDER": (38, 33), "RIGHT ARM": (43, 38), "LEFT ARM": (46, 37), "RIGHT LEG": (31, 53), "LEFT LEG": (47, 53), "shift": (-2, 1)}, -20),
    ],
    # 품질 11회차(GPT 메치기 6.8 "급강하 때 골반 하강·손 낙하 거리가 더 커야"): 3/4 앞 — 머리 높이에서 넘기는 전환 → 골반 깊게(10px) 떨어뜨리며 두 손 앞 땅까지 → 최저점 버팀
    "D_SlamSE2": [
        ("se", {"RIGHT ARM": (44, 30), "LEFT ARM": (47, 30), "RIGHT LEG": (30, 52), "LEFT LEG": (49, 52), "shift": (1, 2)}, 60),
        ("se", {"RIGHT ARM": (48, 53), "LEFT ARM": (51, 53), "RIGHT LEG": (28, 52), "LEFT LEG": (52, 52), "shift": (3, 10)}, -75),
        ("se", {"RIGHT ARM": (47, 52), "LEFT ARM": (50, 52), "RIGHT LEG": (28, 52), "LEFT LEG": (52, 52), "shift": (2, 9)}, -65),
    ],
    # 품질 12회차(GPT 장풍 6.8 "방출 키 팔·어깨 전진이 작다"): 측면 — 골반·어깨 2px 더 앞, 두 손바닥 각 어깨 앞 8px(뻗기 직전 → 최대 → 상체 2px 후퇴)
    "E_PalmSide2": [
        ("side", {"RIGHT SHOULDER": (42, 33), "RIGHT ARM": (48, 34), "LEFT ARM": (51, 36), "RIGHT HIP": (40, 45), "LEFT HIP": (45, 45), "RIGHT LEG": (29, 53), "LEFT LEG": (52, 53), "shift": (4, 1)}, -20),
        ("side", {"RIGHT SHOULDER": (44, 33), "RIGHT ARM": (52, 33), "LEFT ARM": (56, 35), "RIGHT HIP": (41, 45), "LEFT HIP": (46, 45), "RIGHT LEG": (28, 53), "LEFT LEG": (53, 53), "shift": (6, 1)}, -20),
        ("side", {"RIGHT SHOULDER": (42, 33), "RIGHT ARM": (50, 34), "LEFT ARM": (54, 36), "RIGHT HIP": (40, 45), "LEFT HIP": (45, 45), "RIGHT LEG": (28, 53), "LEFT LEG": (53, 53), "shift": (4, 1)}, -20),
    ],
    # 품질 12회차(GPT 놓기 6.5 "손에서 물건을 내려놓는 순간이 불분명"): 3/4 앞 — 골반 낮춰 물건손 앞 아래 → 골반 더 낮춰 손을 앞 땅에 붙임 → 손 떼어 가슴 앞으로(골반 낮게 유지)
    "D_PlaceSE2": [
        ("se", {"RIGHT ARM": (45, 46), "LEFT ARM": (48, 46), "RIGHT LEG": (31, 52), "LEFT LEG": (50, 52), "shift": (1, 4)}, -20),
        ("se", {"RIGHT ARM": (48, 52), "LEFT ARM": (51, 52), "RIGHT LEG": (30, 52), "LEFT LEG": (51, 52), "shift": (2, 7)}, -20),
        ("se", {"RIGHT ARM": (42, 40), "LEFT ARM": (45, 40), "RIGHT LEG": (30, 52), "LEFT LEG": (51, 52), "shift": (0, 5)}, -20),
    ],
    # 품질 17회차(GPT 대검 올려치기 6.8 "올려치기가 아니라 내려베기로 읽힘"): 측면 양손 — 칼끝 아래(-75°) 낮은 장전 → 가슴 높이 상승 타격(35°) → 머리 위 앞 상단에서 멈춤(110°)
    "E_UpperE": [
        ("side", {"RIGHT SHOULDER": (40, 34), "RIGHT ARM": (42, 41), "LEFT ARM": (45, 41), "RIGHT LEG": (32, 53), "LEFT LEG": (48, 53), "shift": (-1, 3)}, -75),
        ("side", {"RIGHT SHOULDER": (41, 33), "RIGHT ARM": (48, 32), "LEFT ARM": (50, 33), "RIGHT LEG": (32, 53), "LEFT LEG": (50, 53), "shift": (2, 0)}, 35),
        ("side", {"RIGHT SHOULDER": (41, 32), "RIGHT ARM": (44, 25), "LEFT ARM": (47, 26), "RIGHT LEG": (33, 53), "LEFT LEG": (50, 53), "shift": (2, -1)}, 110),
    ],
}
LIMB = {"ARM": ("SHOULDER", "ELBOW"), "LEG": ("HIP", "KNEE")}


def build(view, edits):
    base = {q["label"]: [q["x"] * 64, q["y"] * 64] for q in SKEL[view]}
    dx, dy = edits.get("shift", (0, 0))
    upper = {"NOSE", "NECK", "RIGHT EYE", "LEFT EYE", "RIGHT EAR", "LEFT EAR", "RIGHT SHOULDER", "LEFT SHOULDER"}
    for k in base:  # 상체만 기울이고(이동량 전부), 골반은 반만
        f = 1.0 if k in upper else (0.5 if "HIP" in k else 0)
        base[k][0] += dx * f
        base[k][1] += dy * f
    for k, v in edits.items():
        if k == "shift":
            continue
        base[k] = list(v)
    # 팔·다리 길이는 기준 그림 골격 그대로(10-29 밤 사용자 "팔이 고무인간이냐" — 손목을 원래 팔 길이 두 배로 잡아 생성기가 팔을 늘렸다).
    # 목표가 닿는 거리보다 멀면 그 방향으로 최대 거리까지만, 팔꿈치·무릎은 두 마디 길이로 IK(팔은 아래로, 무릎은 앞으로 굽힘)
    orig = {q["label"]: (q["x"] * 64, q["y"] * 64) for q in SKEL[view]}
    for side in ("RIGHT", "LEFT"):
        for end, (root, mid) in LIMB.items():
            r = base[f"{side} {root}"]
            o_r, o_m, o_e = orig[f"{side} {root}"], orig[f"{side} {mid}"], orig[f"{side} {end}"]
            l1 = max(2.5, math.dist(o_r, o_m))
            l2 = max(2.5, math.dist(o_m, o_e))
            e = base[f"{side} {end}"]
            d = math.dist(r, e)
            reach = (l1 + l2) * 0.97
            if d > reach:
                e = [r[0] + (e[0] - r[0]) * reach / d, r[1] + (e[1] - r[1]) * reach / d]
                base[f"{side} {end}"] = e
                d = reach
            if f"{side} {mid}" in edits:
                continue
            d = max(d, 0.5)
            a = (l1 * l1 - l2 * l2 + d * d) / (2 * d)
            h = math.sqrt(max(0.0, l1 * l1 - a * a))
            ux, uy = (e[0] - r[0]) / d, (e[1] - r[1]) / d
            px, py = r[0] + ux * a, r[1] + uy * a
            nx, ny = -uy, ux  # 두 굽힘 후보 중 팔은 아래(y 큰 쪽), 다리는 무릎이 몸 앞(x 큰 쪽)
            c1, c2 = (px + nx * h, py + ny * h), (px - nx * h, py - ny * h)
            if end == "ARM":
                base[f"{side} {mid}"] = list(max(c1, c2, key=lambda c: c[1]))
            else:
                base[f"{side} {mid}"] = list(max(c1, c2, key=lambda c: c[0]))
    return [{"label": k, "x": round(min(max(v[0], 0), 63) / 64, 3), "y": round(min(max(v[1], 0), 63) / 64, 3)} for k, v in base.items()]


def main():
    out = {}
    for name, fr in MOTIONS.items():
        kps = [build(v, e) for v, e, _ in fr]
        out[name] = {"kp": kps, "view": fr[0][0], "angle": [a for _, _, a in fr], "times": [40] * len(fr),
                     "hand": [[int(round(p["x"] * 64)), int(round(p["y"] * 64))] for k in kps for p in k if p["label"] == "RIGHT ARM"]}
    json.dump(out, open(sys.argv[1], "w"), indent=0)
    print(list(out))


if __name__ == "__main__":
    main()
