"""직업 4개 최종본 영상(10-29 밤): Logs/playtest/final/<번호>/f_###.png(capture_final_jobs.cs) → 직업 이름 자막, 2배, 직업별 GIF + 합본 mp4.
사용: python Tools/pixelfx/final_jobs_reel.py <출력 폴더> [chars]
"""
import os
import subprocess
import sys
import tempfile

from PIL import Image, ImageDraw, ImageFont

SRC = "Logs/playtest/final"
FONT = ImageFont.truetype("C:/Windows/Fonts/malgunbd.ttf", 22)
SUB = ImageFont.truetype("C:/Windows/Fonts/malgun.ttf", 15)
JOBS = [("사무라이", "카타나 · 발도 → 돌진 횡베기 → 내려베기 → 납도 → 승리"),
        ("해적 선장", "커틀러스+권총 · 한손 수평 → 검 든 채 사격 → 찌르기 → 백스텝 → 승리"),
        ("심해 닻 전사", "닻 · 내려찍기 → 회전 베기 → 올려치기 → 사슬 당기기 → 승리"),
        ("마도학자", "눈알 지팡이+마도서 · 시전 → 모으기 → 소환 → 승리")]


CHARS = [("블러드 헌터", "커틀러스+권총 · 한손 수평 → 검 든 채 사격 → 백스텝"),
         ("사령술사", "지팡이 · 시전 → 모으기 → 소환"),
         ("기계공학자", "해머 · 내려찍기 → 폭탄 던지기 → 포탑 설치"),
         ("암살자", "쌍단검 · 교차 베기 → 투척 → 백스텝(연막)")]


def main():
    global SRC, JOBS
    out = sys.argv[1]
    if len(sys.argv) > 2 and sys.argv[2] == "chars":  # 사용자 지정 외형 4직업(capture_job_chars.cs)
        SRC, JOBS = "Logs/playtest/final_chars", CHARS
    os.makedirs(out, exist_ok=True)
    tmp = tempfile.mkdtemp()
    n = 0
    for gi, d in enumerate(sorted(os.listdir(SRC))):
        files = sorted(f for f in os.listdir(os.path.join(SRC, d)) if f.endswith(".png"))
        frames = []
        for f in files:
            im = Image.open(os.path.join(SRC, d, f)).convert("RGB").resize((800, 520), Image.NEAREST)
            dr = ImageDraw.Draw(im)
            name, sub = JOBS[gi] if gi < len(JOBS) else (d, "")
            dr.text((16, 12), name, font=FONT, fill=(255, 230, 140))
            dr.text((16, 44), sub, font=SUB, fill=(200, 205, 215))
            frames.append(im)
            im.save(os.path.join(tmp, f"{n:05}.png"))
            n += 1
        small = [f.resize((400, 260), Image.NEAREST) for f in frames]
        small[0].save(os.path.join(out, f"{gi:02}_{JOBS[gi][0] if gi < len(JOBS) else d}.gif"), save_all=True, append_images=small[1:], duration=33, loop=0)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-framerate", "30", "-i", os.path.join(tmp, "%05d.png"),
                    "-pix_fmt", "yuv420p", os.path.join(out, "final_jobs_4.mp4")], check=True)
    print(n, "frames →", out)


if __name__ == "__main__":
    main()
