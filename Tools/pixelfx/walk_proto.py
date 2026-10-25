"""걷기·달리기 포즈 시안(Unity 없이): 리그 부위 PNG 를 포즈별 오프셋으로 합성해 확대 스트립·GIF 로 본다.

값은 PixelRigBuilder 와 같은 단위(아트 픽셀, 오른쪽·위 +). 좋은 값을 찾으면 빌더 Cycle 로 옮긴다.
bend: 다리를 든 포즈에서 신발(아래 3줄)만 앞으로 1px·위로 1px — 무릎이 굽은 것처럼 보인다.
사용: python Tools/pixelfx/walk_proto.py [walk|run]
"""
import sys

from PIL import Image

D = "Docs/ArtCompare/CharacterGen/hero_adventurer/parts/"
P = {n: Image.open(D + n + ".png").convert("RGBA") for n in ["pack", "leg_r", "leg_l", "arm_r", "torso", "head", "arm_l"]}
BOOT_TOP = 52  # 신발 시작 줄(원본 50~54 중 아래 3줄)


def bent(img, fwd, up):
    """신발 줄만 따로 옮긴 다리(무릎 굽힘)"""
    w, h = img.size
    top = img.crop((0, 0, w, BOOT_TOP))
    boot = img.crop((0, BOOT_TOP, w, h))
    out = Image.new("RGBA", (w, h))
    out.alpha_composite(top, (0, 0))
    out.alpha_composite(boot, (fwd, BOOT_TOP - up))
    return out


def shear(img, y0, y1, dx, lift=0):
    """y0 줄은 고정, y1 줄은 dx 만큼 — 줄마다 비례해 민다(붙은 쪽 고정 회전 흉내). lift: 아래쪽 줄을 위로 접는다(무릎 굽힘)"""
    w, h = img.size
    out = Image.new("RGBA", (w, h))
    src = img.load()
    dst = out.load()
    for y in range(h):
        t = 0 if y <= y0 else min(1, (y - y0) / max(1, y1 - y0))
        sx = round(dx * t)
        ty = y - (round(lift * t) if y > y0 else 0)
        for x in range(w):
            if src[x, y][3] and 0 <= x + sx < w and 0 <= ty < h:
                dst[x + sx, ty] = src[x, y]
    return out


def joints(img, rows, fold=None, fold_dx=0):
    """마디별 정수 오프셋: rows = [(시작 줄, dx)] (시작 줄부터 다음 항목 전까지 같은 dx). fold = 지울 줄(무릎 접기) — 그 아래 줄은 1 위로·fold_dx 앞으로"""
    w, h = img.size
    out = Image.new("RGBA", (w, h))
    src = img.load()
    dst = out.load()
    for y in range(h):
        if fold is not None and y == fold:
            continue
        dx = 0
        for y0, d in rows:
            if y >= y0:
                dx = d
        ty = y
        if fold is not None and y > fold:
            ty, dx = y - 1, dx + fold_dx
        for x in range(w):
            if src[x, y][3] and 0 <= x + dx < w:
                dst[x + dx, ty] = src[x, y]
    return out


def seal(img):
    """마디를 옮겨 빈 외곽: 색 픽셀(검정 아님)과 상하좌우로 닿는 빈칸을 검정으로 메운다"""
    w, h = img.size
    src = img.load()
    out = img.copy()
    dst = out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3]:
                continue
            for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] and src[nx, ny][:3] != (0, 0, 0):
                    dst[x, y] = (0, 0, 0, 255)
                    break
    return out


def leg_img(name, dx, lift):
    """dx: 발 앞뒤(무릎은 절반), lift: 1 = 무릎 접어 1 들기, 2 = 접고 1 더 띄움"""
    k = int(dx / 2) if abs(dx) > 1 else 0
    img = joints(P[name], [(51, k), (52, dx)], fold=51 if lift else None, fold_dx=1 if lift else 0)
    if lift > 1:
        img = joints(img, [(51, 0)])
        img2 = Image.new("RGBA", img.size)
        img2.alpha_composite(img.crop((0, 51, 64, 64)), (0, 50))
        img2.alpha_composite(img.crop((0, 0, 64, 51)), (0, 0))
        img = img2
    return img


def arm_img(name, y0, dx):
    """소매 고정, 팔뚝 1·손 dx"""
    return joints(P[name], [(y0 + 4, int(dx / 2) if abs(dx) > 1 else dx), (y0 + 7, dx)])


def frame(p):
    c = Image.new("RGBA", (64, 64), (150, 160, 170, 255))
    hx, hy = p.get("hips", (0, 0))

    def put(name, x, y, img=None):
        c.alpha_composite(img or P[name], (x, -y))

    pk = p.get("pack", 0)
    put("pack", hx, hy + pk)
    for leg, key in (("leg_r", "B"), ("leg_l", "F")):
        fx, lift = p.get("leg" + key, (0, 0))
        put(leg, 0, 0, seal(leg_img(leg, fx, lift)))
    ax, ay = p.get("armB", (0, 0))
    put("arm_r", hx, hy + ay, seal(arm_img("arm_r", 39, ax)))
    put("torso", hx, hy)
    put("head", hx, hy + p.get("head", 0))
    ax, ay = p.get("armF", (0, 0))
    put("arm_l", hx, hy + ay, seal(arm_img("arm_l", 38, ax)))
    return c.crop((20, 14, 60, 58))


# 8포즈 걷기: 접지 → 내려앉음 → 교차(뒷발 듦) → 올라섬 → 접지(반대) → … 몸은 내려앉음 -1, 올라섬 +1 은 쓰지 않는다(머리 위로 튀었다)
WALK = [  # legX=(발 앞뒤 px, 들기 0~2), armX=(손 앞뒤 px, y). 3/4 정면 구도: 다리는 나란히 → 앞뒤 ±1, 번갈아 들기로 걸음을 읽힌다
    dict(legF=(1, 0), legB=(-1, 0), armF=(-2, 0), armB=(2, 0)),
    dict(hips=(0, -1), legF=(1, 0), legB=(-1, 0), armF=(-1, 0), armB=(1, 0), pack=-1),
    dict(legF=(0, 0), legB=(0, 1), armF=(0, 0), armB=(0, 0), head=-1),
    dict(legF=(-1, 0), legB=(1, 1), armF=(1, 0), armB=(-1, 0)),
    dict(legF=(-1, 0), legB=(1, 0), armF=(2, 0), armB=(-2, 0)),
    dict(hips=(0, -1), legF=(-1, 0), legB=(1, 0), armF=(1, 0), armB=(-1, 0), pack=-1),
    dict(legF=(0, 1), legB=(0, 0), armF=(0, 0), armB=(0, 0), head=-1),
    dict(legF=(1, 1), legB=(-1, 0), armF=(-1, 0), armB=(1, 0)),
]

if __name__ == "__main__":
    poses = WALK
    fr = [frame(p) for p in poses]
    s = 8
    w, h = fr[0].size
    strip = Image.new("RGBA", (w * s * len(fr), h * s))
    for i, f in enumerate(fr):
        strip.paste(f.resize((w * s, h * s), Image.NEAREST), (i * w * s, 0))
    strip.save("Logs/playtest/walk_proto.png")
    lo = Image.new("RGBA", (w * 12 * len(fr), 22 * 12))
    for i, f in enumerate(fr):
        lo.paste(f.crop((0, 22, w, 44)).resize((w * 12, 22 * 12), Image.NEAREST), (i * w * 12, 0))
    lo.save("Logs/playtest/walk_proto_lower.png")
    big = [f.resize((w * 6, h * 6), Image.NEAREST) for f in fr]
    big[0].save("Logs/playtest/walk_proto.gif", save_all=True, append_images=big[1:], duration=90, loop=0)
    print("ok", len(fr))
