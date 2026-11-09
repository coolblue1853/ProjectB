"""키프레임 사이 중간 프레임 보간(데드셀식 흉내 실험, 11-06). 부위 분리 없이 광류(DIS)로 두 프레임 사이를 워핑하고 팔레트에 맞춰 도트로 되돌린다.
사용: from interp_flow import tween ; tween(imgA, imgB, t) → RGBA 64x64. 결과는 gif 로 먼저 사용자 검토 (게임 적용 X)."""
import cv2
import numpy as np
from PIL import Image

UP = 4


def _prep(im):
    a = np.array(im.convert("RGBA"))
    bg = np.full(a.shape[:2] + (3,), 128, np.uint8)
    al = a[..., 3:4] / 255.0
    rgb = (a[..., :3] * al + bg * (1 - al)).astype(np.uint8)
    return a, rgb


def _warp(rgba, flow, t):
    H, W = rgba.shape[:2]
    gx, gy = np.meshgrid(np.arange(W), np.arange(H))
    mx = (gx - flow[..., 0] * t).astype(np.float32)
    my = (gy - flow[..., 1] * t).astype(np.float32)
    return cv2.remap(rgba, mx, my, cv2.INTER_NEAREST, borderMode=cv2.BORDER_CONSTANT, borderValue=(0, 0, 0, 0))


def tween(A, B, t):
    a, ra = _prep(A)
    b, rb = _prep(B)
    ua = cv2.resize(ra, None, fx=UP, fy=UP, interpolation=cv2.INTER_NEAREST)
    ub = cv2.resize(rb, None, fx=UP, fy=UP, interpolation=cv2.INTER_NEAREST)
    ga, gb = cv2.cvtColor(ua, cv2.COLOR_RGB2GRAY), cv2.cvtColor(ub, cv2.COLOR_RGB2GRAY)
    dis = cv2.DISOpticalFlow_create(cv2.DISOPTICAL_FLOW_PRESET_MEDIUM)
    f_ab = dis.calc(ga, gb, None)  # A→B (업스케일 좌표)
    f_ba = dis.calc(gb, ga, None)
    fa = cv2.resize(f_ab, (64, 64), interpolation=cv2.INTER_AREA) / UP
    fb = cv2.resize(f_ba, (64, 64), interpolation=cv2.INTER_AREA) / UP
    # 중간 시점 t: A 를 앞으로(t), B 를 뒤로(1-t) 보내 겹친다
    wa = _warp(a, -fb * 0 + fa, 0.0)  # placeholder 구조 유지
    # 정방향 워핑 대신 역방향 샘플링: 중간 프레임 픽셀 p 는 A 의 p - t*F_ba'... 근사로 양쪽 모두 역샘플
    wa = _warp(a, fb * -1.0, t)   # A 쪽: B→A 흐름의 부호 반전으로 t 만큼 당김
    wb = _warp(b, fa * -1.0, 1 - t)
    A_al, B_al = wa[..., 3] > 127, wb[..., 3] > 127
    out = np.zeros_like(a)
    pick_a = (A_al & ~B_al) | (A_al & B_al & (t < 0.5))
    pick_b = (B_al & ~A_al) | (A_al & B_al & (t >= 0.5))
    out[pick_a] = wa[pick_a]
    out[pick_b] = wb[pick_b]
    out[..., 3] = np.where(pick_a | pick_b, 255, 0)
    return Image.fromarray(out, "RGBA")
