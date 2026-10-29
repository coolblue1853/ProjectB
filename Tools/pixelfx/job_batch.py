"""직업 외형 셀 생성(10-30 사용자 "직업 4개는 통과한 외형으로, 필요한 자세만"): 주인공 키(측면)를 그대로 쓰고 첫 프레임만 직업 측면 원본(dirs/uploads.json job_*)으로 바꿔 보낸다.
받은 셀은 register.json 에 'J<직업>_<셀>' 이름 + palette(직업 원본)로 등록 → build_cel 이 직업 색으로 고정.
사용: python Tools/pixelfx/job_batch.py <받을 폴더(m41/jobs)> <시드> 직업:셀:keys파일:동작이름 ...
"""
import json
import os
import subprocess
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))
M41 = os.path.join(HERE, "..", "..", "Docs", "ArtCompare", "Motion", "cel", "m41")
ACTION = "attack motion, bare empty fists, holding nothing"


def run(*a):
    return subprocess.run([sys.executable, *a], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout


def main():
    folder, seed = sys.argv[1], int(sys.argv[2])
    items = [x.split(":") for x in sys.argv[3:]]
    jobs = []
    for job, cel, kf, motion in items:
        keys = os.path.join(M41, kf)
        while True:
            out = run(os.path.join(HERE, "m41_submit.py"), keys, motion, ACTION, str(seed), "job_" + job)
            if "rate limit" in out or "error" in out.lower():
                time.sleep(60)
                continue
            break
        n = len(json.load(open(keys, encoding="utf-8"))[motion]["kp"])
        jobs.append((job, cel, keys, motion, out.split()[-1], n))
        print("보냄", job, cel, jobs[-1][4], flush=True)
        seed += 1
    time.sleep(200)
    left = list(jobs)
    while left:
        for it in list(left):
            job, cel, keys, motion, jid, n = it
            clip = f"J{job.upper()}_{cel}"
            out = run(os.path.join(HERE, "m41_fetch.py"), jid, str(n), clip, f"{folder}/{job}_{cel.lower()}", keys, motion)
            if "frames" in out:
                reg_p = os.path.join(M41, "register.json")
                reg = json.load(open(reg_p, encoding="utf-8"))
                reg[clip]["palette"] = f"../../CharacterGen/jobs/side_{job}.png"
                json.dump(reg, open(reg_p, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
                print("받음", clip, flush=True)
                left.remove(it)
        if left:
            time.sleep(60)
    print("완료", len(jobs))


if __name__ == "__main__":
    main()
