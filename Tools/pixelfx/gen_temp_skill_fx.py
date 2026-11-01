# 첫 1시간 테스트용 임시 무기 스킬 FX(11-01). Docs/Playtest/TempFxSlots.md 주문표를 절차 생성으로 그린다.
# 출력: Assets/Art/Temp/FX/<ID>_f4.png (가로 4프레임, 투명, 오른쪽을 보는 원본) + Logs/tempfx/preview.png
# 초반용이라 작고 소박하게: 색 3개(어두운/중간/밝은 뼈색), 외곽선 없음, 프레임마다 줄어들며 사라진다.
import math, os, random, sys
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Assets", "Art", "Temp", "FX")
PREV = os.path.join(ROOT, "Logs", "tempfx")

def hexc(h):
    return tuple(int(h[i:i + 2], 16) for i in (1, 3, 5)) + (255,)

STONE = [hexc("#796A59"), hexc("#82979D"), hexc("#D6CEAD")]
MOSS = [hexc("#687B51"), hexc("#527B78"), hexc("#D6CEAD")]

# ID: (모양, 팔레트, 폭)
SPEC = {
    "Smash_Down": ("slam", STONE, 64), "Slash_Attck": ("arc", STONE, 64),
    "Scattering_Leaves": ("leaves", MOSS, 64), "Haste": ("rise", MOSS, 48),
    "Falling_Leaf_Step": ("leafstep", MOSS, 48), "Fang_Rush": ("fangs", STONE, 64),
    "Pack_Step": ("dust", STONE, 48), "Hunter's_Stride": ("wind", STONE, 48),
    "Howl_of_the_Pack": ("ripple", STONE, 64), "Mauling_Rush": ("claw3", STONE, 64),
    "Rending_Upswipe": ("upclaw", STONE, 64), "Bear_Roar": ("ripple", STONE, 48),
    "Root_Step": ("dust", MOSS, 64), "Ancient_Roots": ("roots", MOSS, 64),
    "Sap_Mend": ("sap", MOSS, 48), "Jaw_Shake": ("jaw", MOSS, 64),
    "Bog_Eruption": ("mud", MOSS, 64), "Death_Roll": ("roll", MOSS, 64),
    "Tail_Whirl": ("whirl", MOSS, 64), "Boar_Charge": ("dust_push", STONE, 64),
    "Hoof_Step": ("dust", STONE, 64), "Tusk_Uppercut": ("tusk", STONE, 64),
    "Tongue_Lash": ("tongue", MOSS, 64), "Leap_Slam": ("slam", MOSS, 64),
    "Crab_Scuttle": ("dust_side", MOSS, 64), "Shell_Back": ("dust_back", MOSS, 64),
    "Pincer_Flurry": ("pincer", MOSS, 64),
}
H = 48
FR = 4
FADE = [1.0, 0.72, 0.45, 0.2]  # 프레임별 남는 비율

class Canvas:
    def __init__(self, w):
        self.w = w
        self.img = Image.new("RGBA", (w, H), (0, 0, 0, 0))
        self.px = self.img.load()

    def dot(self, x, y, c):
        x, y = int(round(x)), int(round(y))
        if 0 <= x < self.w and 0 <= y < H:
            self.px[x, y] = c

    def disc(self, x, y, r, c):
        ri = int(math.ceil(r))
        for dy in range(-ri, ri + 1):
            for dx in range(-ri, ri + 1):
                if dx * dx + dy * dy <= r * r + 0.3:
                    self.dot(x + dx, y + dy, c)

    def line(self, x0, y0, x1, y1, t0, t1, cols):
        # 두께가 t0→t1 로 변하는 선, 가운데는 밝은 색
        n = int(max(abs(x1 - x0), abs(y1 - y0)) * 2) + 1
        for i in range(n + 1):
            k = i / n
            x, y = x0 + (x1 - x0) * k, y0 + (y1 - y0) * k
            t = t0 + (t1 - t0) * k
            if t <= 0.05:
                continue
            self.disc(x, y, t / 2, cols[0])
        for i in range(n + 1):
            k = i / n
            t = t0 + (t1 - t0) * k
            if t >= 2.2:
                self.disc(x0 + (x1 - x0) * k, y0 + (y1 - y0) * k, max(0.4, t / 2 - 1), cols[1])
            if t >= 3.5:
                self.dot(x0 + (x1 - x0) * k, y0 + (y1 - y0) * k, cols[2])

    def arc(self, cx, cy, rx, ry, a0, a1, thick, cols, tilt=0.0):
        # a0→a1 로 진행하는 호, 머리(a1) 쪽이 두껍고 꼬리로 갈수록 가늘어진다
        steps = int(abs(a1 - a0) * max(rx, ry) / 20) + 8
        pts = []
        for i in range(steps + 1):
            k = i / steps
            a = math.radians(a0 + (a1 - a0) * k)
            x, y = math.cos(a) * rx, math.sin(a) * ry
            ct, st = math.cos(tilt), math.sin(tilt)
            pts.append((cx + x * ct - y * st, cy + x * st + y * ct, thick * (k ** 0.8)))
        for (x0, y0, t0), (x1, y1, t1) in zip(pts, pts[1:]):
            self.line(x0, y0, x1, y1, t0, t1, cols)

def draw(shape, pal, w, f, rnd):
    c = Canvas(w)
    s = FADE[f]
    dark, mid, light = pal
    cols = [dark, mid, light]
    cx, cy = w / 2, H / 2
    if shape == "arc":
        c.arc(cx - 6, cy + 2, 22, 15, 200, 200 + 140 * (0.6 + 0.4 * s), 6 * s, cols, -0.25)
    elif shape == "roll":
        c.arc(cx, cy, 20, 13, 150, 150 + 200 * (0.5 + 0.5 * s), 5 * s, cols, 0.35)
    elif shape == "whirl":
        for k in (0, 180):
            c.arc(cx, cy + 6, 22 * (1.1 - 0.15 * s), 7, k, k + 150 * s + 20, 4 * s, cols)
    elif shape in ("slam", "jaw"):
        # 땅을 찍은 자리: 낮은 쐐기 두 갈래 + 위로 튀는 파편 3~4개
        gy = H - 8
        for d in (-1, 1):
            ln = 12 + 10 * (1 - s)
            c.line(cx + d * 2, gy, cx + d * ln, gy - 1, 5 * s + 1, 0.6, cols)
            if shape == "jaw":
                c.line(cx + d * (10 - 4 * (1 - s)), gy - 14, cx + d * 3, gy - 3, 0.8, 4 * s + 0.6, cols)
        if f < 2:
            c.line(cx, gy - 12 + f * 6, cx, gy, 1, 5 - f * 2, cols)
        for i in range(4):
            ang = math.radians(-150 + i * 40 + rnd.uniform(-8, 8))
            dist = 6 + 14 * (1 - s)
            x, y = cx + math.cos(ang) * dist, gy - 2 + math.sin(ang) * dist * 0.8
            c.disc(x, y, 1.2 * s + 0.5, mid if i % 2 else dark)
            if s > 0.5:
                c.dot(x, y - 1, light)
    elif shape in ("dust", "dust_push", "dust_side", "dust_back", "mud"):
        # 둥근 먼지 덩이 4~5개: 커지며 옅어진다. 아래 어두운 색, 위 중간색, 첫 프레임만 밝은 점
        gy = H - 7
        dirx = {"dust_push": 1, "dust_back": -1}.get(shape, 0)
        n = 5 if shape != "mud" else 6
        grow = 1 - s
        for i in range(n):
            spread = 1.6 if shape == "dust_side" else 1.0
            ox = ((i - (n - 1) / 2) * 7 * spread) * (0.7 + 0.6 * grow) + dirx * 12 * grow
            oy = -abs(i - (n - 1) / 2) * 0.8 - grow * (5 if shape != "mud" else 12) * (0.6 + 0.4 * (i % 2))
            r = 2.2 + 1.8 * grow * (1 if f < 3 else 0.6) + (i % 2) * 0.6
            if f == 3:
                r *= 0.6
            c.disc(cx + ox, gy + oy, r, dark)
            c.disc(cx + ox - 0.6, gy + oy - 0.8, max(0.6, r - 1.3), mid)
            if f == 0 and i % 2 == 0:
                c.dot(cx + ox - 1, gy + oy - r + 1, light)
        if shape == "dust_push" and f < 3:
            for k in range(2):
                y = gy - 8 - k * 6
                c.line(cx - 20 + f * 4, y, cx + 6 - k * 4, y, 0.4, 2.2 * s, cols)
    elif shape in ("fangs", "claw3", "pincer", "wind"):
        rows = {"fangs": [-6, 6], "claw3": [-8, 0, 8], "pincer": [-5, 5], "wind": [-6, 0, 6]}[shape]
        for j, oy in enumerate(rows):
            x1 = w - 6 - (0 if shape != "wind" else 6)
            x0 = x1 - (40 if shape != "wind" else 30) * (0.5 + 0.5 * s)
            tilt = 0 if shape not in ("claw3", "pincer") else (4 if j % 2 else -4)
            if shape == "pincer":
                c.line(x0, cy + oy - tilt, x1, cy - oy * 0.3, 0.5, 4 * s, cols)
            else:
                c.line(x0, cy + oy - tilt, x1, cy + oy + tilt * 0.5, 0.5, (4.5 if shape != "wind" else 2) * s, cols)
    elif shape in ("rise", "upclaw", "tusk", "roots"):
        gy = H - 4
        xs = {"rise": [-10, 0, 10], "upclaw": [-6, 0, 6], "tusk": [0], "roots": [-14, -4, 6, 15]}[shape]
        for i, ox in enumerate(xs):
            top = 6 + (1 - s) * 10 + (i % 2) * 6
            if shape == "upclaw":
                c.line(cx + ox - 10, gy - 4, cx + ox + 6, top, 0.5, 4 * s, cols)
            elif shape == "tusk":
                c.line(cx - 14, gy - 2, cx + 6, top, 1, 6 * s + 1, cols)
            elif shape == "roots":
                ht = (gy - top) * (0.4 + 0.6 * (1 - abs(f - 1.2) / 3))
                y = gy
                x = cx + ox
                while y > gy - ht:
                    nx = x + rnd.choice((-1, 0, 1))
                    c.line(x, y, nx, y - 3, 2.6 * s + 0.6, 2 * s + 0.4, cols)
                    x, y = nx, y - 3
            else:
                c.line(cx + ox, gy, cx + ox, top + 10, 0.5, 2 * s, cols)
    elif shape == "ripple":
        for k in range(2):
            r = 6 + f * 5 + k * 6
            if r > w / 2 - 2:
                continue
            for d in (1, -1):
                a0 = -40 if d == 1 else 140
                c.arc(cx, cy, r, r * 0.9, a0, a0 + 80, 2.6 * s + 0.4, cols)
    elif shape in ("leaves", "leafstep", "sap"):
        n = {"leaves": 9, "leafstep": 6, "sap": 7}[shape]
        for i in range(n):
            bx = rnd.uniform(-1, 1)
            by = rnd.uniform(-1, 1)
            spread = 6 + 16 * (1 - s) if shape == "leaves" else 10
            x = cx + bx * spread + (8 * (1 - s) if shape == "leaves" else 0)
            y = cy + by * 10 + (10 if shape == "leafstep" else 0) + (f * 3 if shape != "sap" else -f * 3)
            if shape == "sap":
                c.disc(x, y, 1.6 * s + 0.4, mid)
                c.dot(x - 0.5, y - 0.5, light)
            else:
                # 잎: 2~3px 사선 조각
                d = 1 if (i + f) % 2 else -1
                c.line(x - 1.5, y - d, x + 1.5, y + d, 2.2 * s + 0.6, 1.2 * s + 0.4, [dark, mid, light])
    elif shape == "tongue":
        x0 = 8
        ln = (w - 16) * (0.55 + 0.45 * (1 - abs(f - 1) / 3))
        prev = None
        for i in range(int(ln)):
            y = cy + math.sin(i / 6 + f) * 3 * s
            if prev:
                c.line(prev[0], prev[1], x0 + i, y, 3 * s, 3 * s, cols)
            prev = (x0 + i, y)
        if prev:
            c.disc(prev[0], prev[1], 2.5 * s + 0.6, mid)
    else:
        raise SystemExit("모양 없음: " + shape)
    return c.img

def main():
    only = sys.argv[1:]
    os.makedirs(OUT, exist_ok=True)
    os.makedirs(PREV, exist_ok=True)
    rows = []
    for sid, (shape, pal, w) in SPEC.items():
        if only and sid not in only:
            continue
        sheet = Image.new("RGBA", (w * FR, H), (0, 0, 0, 0))
        for f in range(FR):
            rnd = random.Random(sum(map(ord, sid)) * 7 + f * 0)
            sheet.paste(draw(shape, pal, w, f, rnd), (f * w, 0))
        sheet.save(os.path.join(OUT, sid + "_f4.png"))
        rows.append((sid, sheet))
    # 미리보기: 3배, 어두운 바탕
    sc = 3
    pw = max(s.width for _, s in rows) * sc
    prev = Image.new("RGBA", (pw, len(rows) * (H * sc + 4)), (30, 32, 36, 255))
    for i, (_, s) in enumerate(rows):
        big = s.resize((s.width * sc, H * sc), Image.NEAREST)
        prev.alpha_composite(big, (0, i * (H * sc + 4)))
    prev.save(os.path.join(PREV, "preview.png"))
    print(len(rows), "장 생성 →", OUT)

if __name__ == "__main__":
    main()
