"""전체 마스터 모션 합본 영상: Logs/playtest/all/<번호>_<동작>/f_###.png(capture_all_motions.cs) → 동작마다 이름·무기 자막, 2배 확대, 30fps mp4.
사용: python Tools/pixelfx/motion_reel.py <출력.mp4>
"""
import os
import subprocess
import sys
import tempfile

from PIL import Image, ImageDraw, ImageFont

SRC = "Logs/playtest/all"
FONT = ImageFont.truetype("C:/Windows/Fonts/malgun.ttf", 18)
NAMES = {
    "Idle": "대기", "Walk": "걷기", "Run": "달리기", "Dash": "대시", "JumpCrouch": "점프 준비", "Land": "착지", "RopeUp": "로프 오르기",
    "Hurt": "피격", "Death": "사망", "Stab": "A3 찌르기(기본)", "SwingDown": "A1 내려베기(기본)", "SwingUp": "A2 올려베기(기본)",
    "GreatDown": "A4 양손 내려찍기 · 대검", "GreatSide": "A5 양손 수평 · 대검", "GreatSideOne": "A5′ 한손 수평 · 장검",
    "DaggerFlurry": "A6 연속 찌르기 · 쌍단검", "DaggerCross": "A7 교차 베기 · 쌍단검", "AirSlash": "A8 공중 베기", "Lunge": "A9 돌진 · 창",
    "Spin": "A10 회전 베기 · 대검", "Cast": "A11 시전 · 지팡이", "Throw": "A12 던지기 · 투척 단검", "Bow": "A13 사격 · 활",
    "Guard": "A14 막기 · 검+방패", "Charge": "A15 모으기 · 지팡이", "LeapSlam": "A16 도약 내려찍기 · 해머", "Backstep": "A17 백스텝 · 쌍단검",
    "Punch": "A18 주먹 · 너클", "Iaido": "A19 발도 · 카타나", "ShoulderAim": "R1 견착 · 대포", "Raise": "R2 양손 들기 · 지팡이",
    "Place": "R6 웅크려 놓기", "Kick": "R7 발차기", "Grab": "R3 잡기", "Ride": "R4 탈것 · 창", "Knockdown": "R5 넘어짐",
    "ShieldBash": "R8 방패 밀치기", "Slam": "R9 메치기", "Pull": "R10 당기기 · 사슬낫",
}


def main():
    out = sys.argv[1]
    tmp = tempfile.mkdtemp()
    n = 0
    for d in sorted(os.listdir(SRC)):
        anim = d.split("_", 1)[1]
        fs = sorted(f for f in os.listdir(f"{SRC}/{d}") if f.endswith(".png"))
        for f in fs + [fs[-1]] * 8:  # 끝 자세 잠깐 유지
            im = Image.open(f"{SRC}/{d}/{f}").convert("RGB").crop((0, 60, 256, 240)).resize((512, 360), Image.NEAREST)
            g = ImageDraw.Draw(im)
            g.text((10, 8), NAMES.get(anim, anim), fill=(255, 240, 160), font=FONT)
            im.save(f"{tmp}/{n:05d}.png")
            n += 1
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-framerate", "30", "-i", f"{tmp}/%05d.png", "-pix_fmt", "yuv420p", "-vf", "scale=1024:720:flags=neighbor", out], check=True)
    print(out, n, "frames")


if __name__ == "__main__":
    main()
