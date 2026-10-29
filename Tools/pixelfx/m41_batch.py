"""keys json 의 동작들을 PixelLab 에 차례로 보내고(동시 한도면 기다렸다 재시도) 끝나면 받아 등록까지(m41_submit + m41_fetch).
사용: python Tools/pixelfx/m41_batch.py <keys json> <받을 폴더(m41/…)> <시드 시작> 이름=셀=동작문구 ...
"""
import json
import os
import subprocess
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))


def run(*a):
    return subprocess.run([sys.executable, *a], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout


def main():
    keys, folder, seed = sys.argv[1], sys.argv[2], int(sys.argv[3])
    items = [x.split("=", 2) for x in sys.argv[4:]]
    jobs = {}
    pending = list(items)
    while pending:
        name, cel, action = pending[0]
        out = run(os.path.join(HERE, "m41_submit.py"), keys, name, action, str(seed))
        if "rate limit" in out or "error" in out.lower():
            time.sleep(60)
            continue
        jobs[name] = (cel, out.split()[-1], len(json.load(open(keys, encoding="utf-8"))[name]["kp"]))
        print("보냄", name, jobs[name][1], flush=True)
        pending.pop(0)
        seed += 1
    time.sleep(240)
    left = dict(jobs)
    while left:
        for name, (cel, job, n) in list(left.items()):
            out = run(os.path.join(HERE, "m41_fetch.py"), job, str(n), cel, f"{folder}/{cel.lower()}", keys, name)
            if "frames" in out:
                print("받음", name, cel, flush=True)
                del left[name]
        if left:
            time.sleep(60)
    print("완료", len(jobs))


if __name__ == "__main__":
    main()
