"""새 마스터 모션(10-29 사용자 추가 결정) 졸라맨 제안 GIF — 생성 0. 왼쪽 = 레퍼런스(림버스 캡처 키 포즈, 저장소 밖), 오른쪽 = 제안.
R8 방패 밀치기 · R9 메치기 · R10 당기기. 림버스식 완급: 준비를 길게 잡고(0.2~0.3초) → 0.07초 한 장으로 터지고 → 타격 자세 유지 → 제자리 회복.
키 = (골반(x, y), 상체 기울기, 칼팔 위·아래, 반대팔 위·아래, 다리 4, 칼 각도) — skel_pose.pose 와 같다. 초록 손 = 반대손(방패·잡기), 빨강 = 칼손.
사용: python Tools/pixelfx/stick_new.py <출력 폴더>  → <동작>_stick.gif (레퍼런스 칸은 C:/Users/user/LimbusRef 가 있을 때만)
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from skel_keys import IDLEK, run  # noqa: E402
from stick_prev import FONT, draw  # noqa: E402

LIMBUS = "C:/Users/user/LimbusRef/cap"
IDLE_AT = lambda x: ((x, 44),) + IDLEK[1:]  # noqa: E731 — 제자리 회복(골반을 끌어오지 않는다)
LUNGE = (-140, -140, -40, -95)
CROUCH = (-125, -95, -60, -100)
WIDE_LOW = (-130, -105, -50, -95)

MOTIONS = {
    # R8 방패 밀치기(로지온 S2 어깨 돌진·싱클레어 S2 밀어붙이기): 방패를 가슴 앞에 붙여 웅크림 → 앞발 내딛으며 상체째 밀어붙임(팔은 끝까지 펴지 않는다, 몸이 민다)
    "R8_ShieldBash": {
        "ref": ("10901_Rodion_Base", "Rodion_Base_S2"),
        "keys": [IDLEK, [
            (((39.5, 45), -8, -140, -120, -35, 60) + CROUCH + (-150,), 2),      # 준비: 방패 가슴 앞, 칼은 뒤 아래, 무게 뒤
            (((39.5, 45), -9, -141, -121, -36, 59) + CROUCH + (-150,), 1),      # 준비 유지(림버스 0.3초)
            (((46, 44), 24, -150, -150, 5, 25) + LUNGE + (-155,), 1),           # 0.07초: 앞발 내딛고 상체째 밀어붙임
            (((47.5, 44), 28, -152, -152, 8, 18) + LUNGE + (-155,), 2),         # 밀친 자세 유지
        ], [(((47.5, 44), 10, -130, -110, -50, 30) + WIDE_LOW + (-120,), 2), (IDLE_AT(47.5), 2)]],
        "times": [50, 140, 130, 70, 90, 110, 80, 80, 90, 110],
    },
    # R9 메치기: 앞으로 낮게 잡음 → 상체를 뒤로 젖히며 두 팔로 머리 위까지 들어 올림(유지) → 0.07초에 앞 아래로 내리꽂음 → 낮은 자세 유지
    "R9_Slam": {
        "ref": None,
        "keys": [IDLEK, [
            (((43, 44), 22, -20, -10, -25, -15) + (-118, -118, -62, -90) + (-15,), 2),   # 잡음: 두 손 앞 낮게
            (((41, 44), -16, 105, 120, 110, 125) + WIDE_LOW + (110,), 2),               # 들어 올림: 몸 뒤로 젖힘, 두 팔 머리 위
            (((41, 44), -17, 106, 121, 111, 126) + WIDE_LOW + (110,), 1),               # 들어 올린 채 유지
            (((45, 46), 36, -55, -70, -50, -65) + LUNGE + (-70,), 1),                    # 0.07초: 앞 아래로 내리꽂음
            (((45.5, 46), 38, -58, -72, -53, -68) + LUNGE + (-72,), 2),                  # 낮은 자세 유지
        ], [(((45.5, 45), 15, -100, -80, -90, -70) + WIDE_LOW + (-60,), 2), (IDLE_AT(45.5), 2)]],
        "times": [50, 90, 110, 120, 140, 70, 100, 110, 90, 90, 100, 110],
    },
    # R10 당기기(사슬·갈고리·낫 걸기): 팔을 앞으로 곧게 던짐 → 걸린 채 유지 → 0.07초에 팔꿈치를 뒤로 확 당기며 상체 뒤로·뒷다리로 버팀 → 유지
    "R10_Pull": {
        "ref": None,
        "keys": [IDLEK, [
            (((44, 44), 16, 4, 4) + (-85, -40) + LUNGE + (2,), 2),                 # 던짐: 칼팔 앞으로 곧게
            (((44.5, 44), 17, 3, 3) + (-85, -40) + LUNGE + (2,), 1),               # 걸린 채 유지
            (((40, 44), -18, -150, 25) + (-100, -60) + (-150, -115, -55, -70) + (175,), 1),   # 0.07초: 팔꿈치 뒤로 당김, 상체 뒤로, 앞다리 버팀
            (((39.5, 44), -20, -152, 28) + (-102, -62) + (-150, -115, -55, -70) + (176,), 2),  # 당긴 자세 유지
        ], [(((40, 44), -5, -120, -60) + (-90, -50) + WIDE_LOW + (-60,), 2), (IDLE_AT(40), 2)]],
        "times": [50, 100, 130, 70, 110, 90, 90, 80, 90, 110],
    },
}


def frames(keys):
    start, main_segs, end_segs = keys
    a, last = run(start, main_segs)
    b, _ = run(last, end_segs)
    return [((f[0][0],) + tuple(f[1:10]), f[10]) for f in a + b]


def ref_frames(ref, n):
    """레퍼런스 클립 프레임을 제안 프레임 수에 맞춰 고르게 뽑는다(없으면 빈 칸)."""
    if ref is None or not os.path.isdir(f"{LIMBUS}/{ref[0]}/{ref[1]}"):
        return [None] * n
    d = f"{LIMBUS}/{ref[0]}/{ref[1]}"
    fs = sorted(f for f in os.listdir(d) if f.endswith(".png"))
    return [f"{d}/{fs[min(len(fs) - 1, int(i * len(fs) / n))]}" for i in range(n)]


def main():
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    for name, m in MOTIONS.items():
        fr = frames(m["keys"])
        assert len(fr) == len(m["times"]), (name, len(fr))
        refs = ref_frames(m["ref"], len(fr))
        gif = []
        for i, ((k, ang), rp) in enumerate(zip(fr, refs)):
            b, _, _ = draw(k, ang, scale=4)
            c = Image.new("RGB", (b.width * 2 + 8, b.height + 30), (255, 255, 255))
            if rp:
                r = Image.open(rp).convert("RGB")
                r = r.crop((r.width // 2 - 200, 60, r.width // 2 + 160, r.height - 60)).resize((b.width, b.height))
                c.paste(r, (0, 30))
            else:
                ImageDraw.Draw(c).text((10, 120), "레퍼런스 없음\n(일반 문법)", fill=(120, 120, 120), font=FONT)
            c.paste(b, (b.width + 8, 30))
            g = ImageDraw.Draw(c)
            g.text((6, 2), "레퍼런스 " + (m["ref"][1] if m["ref"] else "-"), fill=(0, 90, 160), font=FONT)
            g.text((b.width + 14, 2), f"제안 {name} {i}  {m['times'][i] / 1000:.2f}s", fill=(180, 30, 30), font=FONT)
            gif.append(c)
        gif[0].save(f"{out}/{name}_stick.gif", save_all=True, append_images=gif[1:], duration=m["times"], loop=0)
        slow = [t * 3 for t in m["times"]]
        gif[0].save(f"{out}/{name}_stick_slow.gif", save_all=True, append_images=gif[1:], duration=slow, loop=0)
        print(name, len(gif))


if __name__ == "__main__":
    main()
