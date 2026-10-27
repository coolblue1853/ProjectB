"""스컬식 베기 스미어(꽉 찬 초승달 / 얇은 호)를 픽셀로 그린다(생성 0).
스컬 리틀 본 AttackA·B 기준: 꽉 찬 면 1프레임(흰색 2단 명암, 바깥 두껍고 끝이 가늘다) → 얇은 호 1프레임(2~3px) → 사라짐.
각도는 수학 기준(0=오른쪽, 90=위, 반시계 +). 캔버스 좌표는 위가 0.
사용: from smear import crescent, thin_arc
"""
import math

from PIL import Image

WHITE = (240, 240, 236, 255)
SHADE = (196, 198, 204, 255)
EDGE = (150, 152, 162, 255)


def _ang_in(a, a0, a1):
    """a 가 a0 에서 a1 로(반시계, a1 > a0) 가는 구간 안이면 0~1 진행도."""
    span = a1 - a0
    t = (a - a0) % 360
    return t / span if t <= span else None


def crescent(size, cx, cy, r, a0, a1, thick=9, ccw=True):
    """꽉 찬 초승달. a0→a1 로 휘두른다(ccw=False 면 시계 방향). 진행 끝쪽이 가장 두껍고 시작 쪽은 0으로 가늘어진다."""
    img = Image.new("RGBA", size)
    px = img.load()
    lo, hi = (a0, a1) if ccw else (a1, a0)
    if hi < lo:
        hi += 360
    for y in range(size[1]):
        for x in range(size[0]):
            dx, dy = x + .5 - cx, cy - (y + .5)
            d = math.hypot(dx, dy)
            a = math.degrees(math.atan2(dy, dx))
            t = _ang_in(a, lo, hi)
            if t is None:
                continue
            prog = t if ccw else 1 - t  # 0=시작, 1=끝(칼이 지금 있는 곳)
            w = thick * min(1.0, prog * 1.6) * (1 - max(0, prog - .9) * 4)
            inner = r - w
            if inner <= d <= r:
                f = (d - inner) / max(1, w)
                px[x, y] = EDGE if f > .85 else (WHITE if f > .35 else SHADE)
    return img


def thin_arc(size, cx, cy, r, a0, a1, width=2, ccw=True):
    """얇은 호 잔상. 끝쪽 2~3px, 시작 쪽으로 1px 로 가늘어진다."""
    img = Image.new("RGBA", size)
    px = img.load()
    lo, hi = (a0, a1) if ccw else (a1, a0)
    if hi < lo:
        hi += 360
    for y in range(size[1]):
        for x in range(size[0]):
            dx, dy = x + .5 - cx, cy - (y + .5)
            d = math.hypot(dx, dy)
            t = _ang_in(math.degrees(math.atan2(dy, dx)), lo, hi)
            if t is None:
                continue
            prog = t if ccw else 1 - t
            w = 1 + (width - 1) * prog
            if r - w <= d <= r:
                px[x, y] = WHITE if prog > .5 else SHADE
    return img


def moon(size, cx, cy, r, hollow_deg, offset, r_in=None):
    """두 원 차이로 만든 초승달(스컬 꽉 찬 스미어). hollow_deg 쪽으로 안쪽 원을 offset 만큼 밀어 그쪽이 비고,
    반대쪽(칼이 지나간 쪽)이 가장 두껍다. 두 끝은 자연스럽게 뾰족해진다."""
    img = Image.new("RGBA", size)
    px = img.load()
    hx = cx + offset * math.cos(math.radians(hollow_deg))
    hy = cy - offset * math.sin(math.radians(hollow_deg))
    for y in range(size[1]):
        for x in range(size[0]):
            d = math.hypot(x + .5 - cx, y + .5 - cy)
            di = math.hypot(x + .5 - hx, y + .5 - hy)
            ri = r if r_in is None else r_in
            if d <= r and di > ri:
                edge = r - d
                inner = di - ri
                if edge < 1.2:
                    px[x, y] = EDGE
                elif inner < 2.2:
                    px[x, y] = SHADE
                else:
                    px[x, y] = WHITE
    return img
