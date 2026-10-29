"""방향 기준 그림(정면·뒷모습·3/4)의 골격을 PixelLab estimate-skeleton 으로 읽어 저장한다(0.1 생성/장). 회전 베기·돌려차기처럼 몸 방향(yaw)이 바뀌는 동작용.
인증은 m41_submit 과 같은 MCP 설정 헤더(출력하지 않음).
사용: python Tools/pixelfx/dir_skel.py <png>... → Docs/ArtCompare/CharacterGen/dirs/skel.json 에 파일 이름별로 추가
"""
import base64
import json
import os
import sys
import urllib.request

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from m41_submit import server  # noqa: E402

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
OUT = os.path.join(ROOT, "Docs", "ArtCompare", "CharacterGen", "dirs", "skel.json")


def estimate(path, headers):
    body = json.dumps({"image": {"type": "base64", "base64": base64.b64encode(open(path, "rb").read()).decode()}}).encode()
    h = {"Content-Type": "application/json"}
    h.update({k: v for k, v in headers.items() if k.lower() == "authorization"})
    req = urllib.request.Request("https://api.pixellab.ai/v2/estimate-skeleton", body, h, method="POST")
    with urllib.request.urlopen(req, timeout=120) as r:
        return json.loads(r.read().decode("utf-8"))["keypoints"]


def main():
    _, headers = server()
    res = json.load(open(OUT, encoding="utf-8")) if os.path.exists(OUT) else {}
    for p in sys.argv[1:]:
        kp = estimate(p, headers)
        res[os.path.splitext(os.path.basename(p))[0]] = kp
        print(os.path.basename(p), " ".join(f"{q['label'][:9]}({q['x']*64:.0f},{q['y']*64:.0f})" for q in kp))
    json.dump(res, open(OUT, "w", encoding="utf-8"), indent=1)


if __name__ == "__main__":
    main()
