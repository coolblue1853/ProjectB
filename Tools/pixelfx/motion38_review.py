"""마스터 모션 38개 검토 영상: 게임과 같은 규칙(cel_weapon_sheet.frame — 셀 + 장비 레이어 + 칼 + 주먹)으로 합성해
모션마다 'NN_이름' 꼬리표를 붙인 GIF 와 전체를 이어 붙인 MP4 를 만든다(생성 0).
사용: python Tools/pixelfx/motion38_review.py [장비 id=iron_set]
결과: Docs/ArtCompare/Motion/m38_review/NN_이름.gif, Docs/ArtCompare/Motion/m38_review/all_38.mp4
"""
import json
import os
import subprocess
import sys
import tempfile

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(__file__))
import cel_weapon_sheet as ws  # noqa: E402

# (번호, 표시 이름, 셀 이름) — MotionSet.md 순서. 셀이 없는 모션은 게임과 같은 대체 셀
MOTIONS = [
    ("대기", "Idle"), ("걷기", "Walk"), ("달리기", "Run"), ("제동(대기 공용)", "Idle"), ("점프 웅크림", "JumpCrouch"),
    ("공중", "JumpArc"), ("착지", "Land"), ("대시", "Dash"), ("피격", "Hurt"), ("사망", "Death"),
    ("로프 매달리기", "RopeStay"), ("로프 오르기", "RopeUp"),
    ("A1 내려베기", "SwingDown"), ("A2 올려베기", "SwingUp"), ("A3 찌르기", "Stab"), ("A4 양손 내려찍기", "GreatDown"),
    ("A5 양손 수평 휘두르기", "GreatSide"), ("A6 연속 찌르기", "DaggerFlurry"), ("A7 교차 베기", "DaggerCross"),
    ("A8 공중 내려치기", "AirSlash"), ("A9 돌진 찌르기", "Lunge"), ("A10 회전 베기", "Spin"), ("A11 시전", "Cast"),
    ("A12 던지기", "Throw"), ("A13 활(찌르기 대체)", "Stab"), ("A14 막기", "Guard"), ("A15 모으기", "Charge"),
    ("A16 도약 내려찍기", "LeapSlam"), ("A17 백스텝", "Backstep"), ("A18 주먹 3타", "Punch"), ("A19 발도", "Iaido"),
    ("R1 어깨 겨눔", "ShoulderAim"), ("R2 양손 들기", "Raise"), ("R3 잡기", "Grab"), ("R4 앉기(탈것)", "Ride"),
    ("R5 넘어짐·일어남", "Knockdown"), ("R6 웅크려 놓기", "Place"), ("R7 발차기", "Kick"),
]
S = 4
FPS = 50


def main():
    eq = sys.argv[1] if len(sys.argv) > 1 else "iron_set"
    out = "Docs/ArtCompare/Motion/m38_review"
    os.makedirs(out, exist_ok=True)
    d = json.load(open(ws.P + "Cel/cel.json", encoding="utf-8"))
    sw = Image.open(ws.SWORD).convert("RGBA")
    font = ImageFont.truetype("C:/Windows/Fonts/malgunbd.ttf", 22)
    tmp = tempfile.mkdtemp()
    n = 0
    for no, (label, cel) in enumerate(MOTIONS, 1):
        title = f"{no:02d}_{label}"
        frames = []
        fr = d[cel]["frames"]
        dt = d[cel]["frame"]
        for i in range(len(fr)):
            img = ws.frame(cel, i, d, eq, sw).resize((96 * S, 80 * S), Image.NEAREST)
            canvas = Image.new("RGB", (96 * S, 80 * S + 36), (30, 30, 30))
            canvas.paste(img.convert("RGB"), (0, 36))
            ImageDraw.Draw(canvas).text((8, 5), title, font=font, fill=(255, 230, 120))
            frames.append(canvas)
        hold = max(1, round(dt * FPS))
        loops = max(2, int(1.2 / (dt * len(frames))) + 1)
        frames[0].save(f"{out}/{title.replace(' ', '_')}.gif", save_all=True, append_images=frames[1:], duration=int(dt * 1000), loop=0)
        for _ in range(loops):
            for f in frames:
                for _ in range(hold):
                    f.save(f"{tmp}/{n:05d}.png")
                    n += 1
        for _ in range(10):  # 모션 사이 0.2초 쉼
            frames[-1].save(f"{tmp}/{n:05d}.png")
            n += 1
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-framerate", str(FPS), "-i", f"{tmp}/%05d.png",
                    "-pix_fmt", "yuv420p", f"{out}/all_38.mp4"], check=True)
    print(out, n, "frames")


if __name__ == "__main__":
    main()
