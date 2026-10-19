"""GPT 이미지(마젠타 배경, 한 줄 N포즈) → 게임 규격 몹 시트 (ArtStyle.md 2절 3단계).
사용: python Tools/pixelfx/gpt_sheet_to_frames.py <입력.png> <몹 폴더 이름> [프레임 크기=64]
- 마젠타·분홍 테두리 제거 → 열 단위로 포즈 분리 → 각 포즈를 바닥 중앙 정렬로 정사각 프레임에 배치
- 포즈 순서 가정(프롬프트와 동일): 0 대기, 1 숨쉬기, 2 달리기(뻗음), 3 달리기(모음), 4 공격, 5 피격
- 출력: Assets/Art/Monsters/<이름>/Idle.png · Run.png · Attack1.png · Take Hit.png · Death.png"""
import os
import sys
from PIL import Image

BODY_W = 0  # 대기 자세 폭(긴 공격 포즈 정렬용)
WING_FIX = False  # 반투명 날개 몹: 인자에 wingfix
KEEP_PINK = False  # 분홍 몸 색(혀·꽃)을 쓰는 몹은 인자 4번째에 keeppink
ROOT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Art", "Monsters")


def is_bg(r, g, b, a):
    if a < 40:
        return True
    # 마젠타 계열(R·B 높고 G 낮음) — 안티앨리어싱 섞인 분홍 테두리까지
    if r > 150 and b > 120 and g < 110 and abs(r - b) < 120:
        return True
    # 분홍 테두리(마젠타가 몸 색과 섞임): R 높고 B 가 G 보다 높다 — 눈 같은 주황 포인트(B 낮음)는 남긴다
    if KEEP_PINK is False and r > 190 and b > 80 and b > g:
        return True
    # 어두운 보라 테두리(배경과 섞인 외곽 픽셀): R·B 가 G 보다 확실히 높다
    # (붉은 몸통 몹은 wingfix 로 이 규칙을 끈다 — 배경은 브라우저 단계에서 이미 지워졌다)
    return not WING_FIX and r > 70 and b > 60 and g < r * 0.55 and g < b * 0.65


def clean(im):
    im = im.convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if WING_FIX and r > 140 and b > 110 and g < r * 0.8 and (r + g + b) > 390:
                # 반투명 날개가 마젠타와 섞인 분홍 → 회보라 연기빛 반투명
                v = (r + g + b) // 3
                px[x, y] = (int(v * 0.85), int(v * 0.85), v, 170)
            elif is_bg(r, g, b, a):
                px[x, y] = (0, 0, 0, 0)
            else:
                px[x, y] = (r, g, b, 255)
    # 외톨이 픽셀 제거(이웃 불투명 1개 이하)
    src = im.copy().load()
    for y in range(im.height):
        for x in range(im.width):
            if src[x, y][3] == 0:
                continue
            n = 0
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < im.width and 0 <= ny < im.height and src[nx, ny][3]:
                    n += 1
            if n <= 1:
                px[x, y] = (0, 0, 0, 0)
    return im


def drop_edge_bits(crop):
    """좌우 가장자리에 걸친 작은 덩어리(옆 포즈에서 넘어온 조각)를 지운다"""
    px, w, h = crop.load(), crop.width, crop.height
    seen, comps = set(), []
    for sy in range(h):
        for sx in range(w):
            if (sx, sy) in seen or not px[sx, sy][3]:
                continue
            stack, comp = [(sx, sy)], []
            seen.add((sx, sy))
            while stack:
                x, y = stack.pop()
                comp.append((x, y))
                for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1), (x + 1, y + 1), (x - 1, y - 1), (x + 1, y - 1), (x - 1, y + 1)):
                    if 0 <= nx < w and 0 <= ny < h and (nx, ny) not in seen and px[nx, ny][3]:
                        seen.add((nx, ny))
                        stack.append((nx, ny))
            comps.append(comp)
    if not comps:
        return crop
    big = max(len(c) for c in comps)
    for c in comps:
        edge = any(x == 0 or x == w - 1 for x, _ in c)
        span = max(x for x, _ in c) - min(x for x, _ in c) + 1
        if edge and len(c) < big * 0.3 and span < w * 0.25:  # 길게 뻗은 혀·무기 끝은 남긴다
            for x, y in c:
                px[x, y] = (0, 0, 0, 0)
    return crop


def split_poses(im, want, cuts=None):
    if cuts:  # 수동 경계(포즈끼리 열이 겹칠 때): cut=a,b,c,d,e
        xs = [0] + cuts + [im.width]
        return [drop_edge_bits(im.crop((xs[i], 0, xs[i + 1], im.height))).crop() for i in range(want)]
    cols = [any(im.getpixel((x, y))[3] for y in range(im.height)) for x in range(im.width)]
    runs, start = [], None
    for x, on in enumerate(cols + [False]):
        if on and start is None:
            start = x
        elif not on and start is not None:
            runs.append((start, x))
            start = None
    # 포즈끼리 붙어 덜 쪼개졌으면(뿌리·이펙트가 맞닿음) 균등 폭으로 나눈다
    if len(runs) < want:
        step = im.width / want
        dens = [sum(1 for y in range(im.height) if im.getpixel((x, y))[3]) for x in range(im.width)]
        cuts = [0]
        for i in range(1, want):  # 경계 근처에서 가장 비어 있는 열을 자른다
            c = int(i * step)
            lo, hi = max(1, c - int(step * 0.25)), min(im.width - 1, c + int(step * 0.25))
            cuts.append(min(range(lo, hi), key=lambda x: (dens[x], abs(x - c))))
        cuts.append(im.width)
        runs = [(cuts[i], cuts[i + 1]) for i in range(want)]
    # 너무 잘게 쪼개졌으면(꼬리 끝 등) 가까운 조각끼리 합친다
    while len(runs) > want:
        gaps = [(runs[i + 1][0] - runs[i][1], i) for i in range(len(runs) - 1)]
        _, i = min(gaps)
        runs[i:i + 2] = [(runs[i][0], runs[i + 1][1])]
    poses = []
    for x0, x1 in runs:
        crop = drop_edge_bits(im.crop((x0, 0, x1, im.height)))
        bbox = crop.getbbox()
        poses.append(crop.crop(bbox))
    return poses


def frame(pose, size, dx=0, dy=0):
    f = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    if pose.width > size - 2 and pose.height <= size - 2 and BODY_W:
        # 앞으로 길게 뻗은 공격(혀·팔): 축소하면 크기가 튀므로 몸을 대기 자세 위치에 두고 넘친 끝만 자른다
        x = max(0, (size - BODY_W) // 2 + dx)
        pose = pose.crop((0, 0, min(pose.width, size - x), pose.height))
    else:
        scale = min(1.0, (size - 2) / max(pose.width, pose.height))
        if scale < 1.0:
            pose = pose.resize((max(1, int(pose.width * scale)), max(1, int(pose.height * scale))), Image.NEAREST)
        x = (size - pose.width) // 2 + dx
    y = size - 2 - pose.height + dy  # 발은 아래에서 2px 위
    f.alpha_composite(pose, (x, y))
    return f


def tint(im, k):
    im = im.copy()
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a:
                px[x, y] = (int(r + (255 - r) * k), int(g + (255 - g) * k), int(b + (255 - b) * k), a)
    return im


def fade(im, a):
    im = im.copy()
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, al = px[x, y]
            if al:
                px[x, y] = (r, g, b, int(al * a))
    return im


def sheet(frames, size):
    s = Image.new("RGBA", (size * len(frames), size), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        s.paste(f, (i * size, 0))
    return s


def main():
    src, name = sys.argv[1], sys.argv[2]
    size = int(sys.argv[3]) if len(sys.argv) > 3 else 64
    global KEEP_PINK
    KEEP_PINK = "keeppink" in sys.argv[4:]
    global WING_FIX
    WING_FIX = "wingfix" in sys.argv[4:]
    cuts = next(([int(v) for v in a[4:].split(",")] for a in sys.argv[4:] if a.startswith("cut=")), None)
    poses = split_poses(clean(Image.open(src)), 6, cuts)
    poses = [p.crop(p.getbbox()) for p in poses]
    if len(poses) < 6:
        print("poses found:", len(poses))
        return 1
    idle0, idle1, run0, run1, atk, hurt = poses[:6]
    global BODY_W
    BODY_W = idle0.width
    out = os.path.join(ROOT, name)
    os.makedirs(out, exist_ok=True)
    F = lambda p, dx=0, dy=0: frame(p, size, dx, dy)
    sheet([F(idle0), F(idle1), F(idle0), F(idle1, dy=0)], size).save(os.path.join(out, "Idle.png"))
    sheet([F(run0), F(run0, dy=-1), F(run1), F(run1, dy=-1), F(run0, dy=-2), F(run1)], size).save(os.path.join(out, "Run.png"))
    sheet([F(idle1), F(idle1, dx=-2), F(run1, dx=-3), F(atk, dx=3), F(atk, dx=4), F(idle0, dx=1)], size).save(os.path.join(out, "Attack1.png"))
    sheet([tint(F(hurt, dx=-2), 0.7), F(hurt, dx=-2), F(idle0)], size).save(os.path.join(out, "Take Hit.png"))
    death = []
    for i in range(5):
        k = i / 4
        f = F(hurt).rotate(-k * 80, resample=Image.NEAREST, center=(size // 2, size - 2))
        death.append(fade(f, 1 - k * 0.85))
    sheet(death, size).save(os.path.join(out, "Death.png"))
    print("ok", name, [p.size for p in poses[:6]])
    return 0


if __name__ == "__main__":
    sys.exit(main())
