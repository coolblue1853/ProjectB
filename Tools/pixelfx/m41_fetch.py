"""PixelLab 스켈레톤 생성 결과를 받아 셀 원본 폴더에 저장하고 build_cel 등록(m41/register.json)을 갱신한다.
사용: python Tools/pixelfx/m41_fetch.py <job_id> <프레임 수> <클립 이름> <폴더(m41/…)> <keys json> <keys 속 동작 이름>
"""
import json
import os
import sys
import urllib.request

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
SRC = os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel")


def main():
    job, n, clip, folder, keys, motion = sys.argv[1], int(sys.argv[2]), sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6]
    d = os.path.join(SRC, folder)
    os.makedirs(d, exist_ok=True)
    for i in range(n):
        url = f"https://api.pixellab.ai/mcp/images/{job}/download?index={i}"
        urllib.request.urlretrieve(url, os.path.join(d, f"{i}.png"))
    k = json.load(open(keys, encoding="utf-8"))[motion]
    reg_p = os.path.join(SRC, "m41", "register.json")
    reg = json.load(open(reg_p, encoding="utf-8")) if os.path.exists(reg_p) else {}
    hand_b = [[int(round(q["x"] * 64)), int(round(q["y"] * 64))] for f in k["kp"] for q in f if q["label"] == "LEFT ARM"]  # 앞손(방패·보조손)
    reg[clip] = {"frames": [f"{folder}/{i}" for i in range(n)], "hand": k["hand"], "handB": hand_b, "angle": k["angle"],
                 "times": [round(t / 1000, 3) for t in k["times"]], "job": job, "motion": motion}
    json.dump(reg, open(reg_p, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
    print(clip, n, "frames →", d)


if __name__ == "__main__":
    main()
