"""올려베기 3칸 졸라맨(사용자 11-06 레퍼런스: 분홍 한복 여검사 3프레임 — 칼 앞 위 → 낮게 쓸어 올림 → 팔 위로 곧게, 칼 머리 뒤).
오른쪽 방향 기준. 사용: python Tools/pixelfx/up_keys8.py → Logs/motion_review/up8_keys.json, up8_cmp.png"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import up_keys2 as u  # noqa: E402
import up_keys3 as k3  # noqa: E402
from PIL import Image  # noqa: E402

ROOT = u.ROOT
# (골반 낮춤, 상체 앞기울기, 오른팔 위°, 오른팔 아래°, 왼팔 위°, 왼팔 아래°, 앞발 x, 앞발 들림, 뒷발 x, 뒷발 들림)
KEYS = [
    (5.5, 1.0, 245, 135, 240, 130, 45, 0.0, 18, 0.0),   # A 깊이 앉아 두 손을 어깨 뒤로 접음, 칼 뒤 위(140°)
    (6.5, 8.0, 325, 335, 320, 330, 45, 0.0, 22, 0.0),   # B 상체 크게 숙이며 두 팔 앞 아래로 뻗음, 칼 아래 뒤로(190°)
    (0.5, -1.0, 68, 70, 72, 74, 38, 0.0, 27, 0.0),      # C 서며 두 팔 위 앞으로 곧게, 칼 앞 위(40°)
]
BLADE = [140, 190, 40]

if __name__ == "__main__":
    Ps = [k3.pose(k) for k in KEYS]
    json.dump([u.to_kp(P) for P in Ps], open(os.path.join(ROOT, "Logs/motion_review/up8_keys.json"), "w"))
    ref = Image.open(os.path.join(ROOT, "Logs/motion_review/up8_ref.png")).convert("RGB")
    W, H = ref.size
    boxes = [(30, 20, 200, 180), (420, 60, 610, 200), (790, 0, 1000, 180)]
    sheet = Image.new("RGB", (3 * 400, 800), (255, 255, 255))
    for i, b in enumerate(boxes):
        c = ref.crop(b)
        c.thumbnail((400, 400))
        sheet.paste(c, (i * 400, 0))
        k3.BL = 16.0
        sheet.paste(k3.draw_blade(Ps[i], BLADE[i]).resize((400, 400)), (i * 400, 400))
    sheet.save(os.path.join(ROOT, "Logs/motion_review/up8_cmp.png"))
    print("ok")
