"""올려베기 옆모습 키 자세 v9(11-07, 모션 연구 M1). 옆모습 하나, 낮은 예비(칼 뒤 아래) → 몸이 솟으며 칼이 앞 위로 호 → 끝에서 몸 전체가 펴짐.
up_keys3 의 pose/draw_blade 를 재사용. 출력: Logs/motion_review/up9_side_sheet.png (칼 호·체중 표시 포함)."""
import json, math, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import up_keys3 as k3
import up_keys2 as u
from PIL import Image, ImageDraw

# (골반 낮춤, 상체 앞기울기, 오른팔 위°, 오른팔 아래°, 왼팔 위°, 왼팔 아래°, 앞발 x, 앞발 들림, 뒷발 x, 뒷발 들림)
KEYS = [
    (4.5, 4.0, 240, 205, 20, 55, 45, 0.0, 21, 0.0),    # 예비: 깊이 웅크림, 칼손 뒤 엉덩이 옆, 칼 뒤 아래
    (3.5, 3.0, 300, 335, 150, 200, 45, 0.0, 22, 0.0),  # 스윙 중: 몸이 솟기 시작, 칼손 앞 아래, 칼 앞
    (1.5, -1.0, 40, 65, 200, 235, 45, 0.0, 22, 1.5),   # 타격: 다리 펴짐, 팔 앞 위, 칼 앞 위, 뒷발 뒤꿈치 듦
    (0.0, -4.0, 80, 105, 205, 240, 44, 0.0, 23, 2.5),  # 끝: 몸 전체 펴짐·살짝 젖힘, 칼 머리 위 뒤로 넘어감
]
BLADE = [200, 345, 62, 118]
S = 8
if __name__ == "__main__":
    k3.BLADE = BLADE
    Ps = [k3.pose(k) for k in KEYS]
    json.dump([u.to_kp(P) for P in Ps], open("Logs/motion_review/up9_side_keys.json", "w"))
    n = len(Ps)
    W = 300
    sheet = Image.new("RGB", (n * W, W + 30), (30, 30, 36))
    tips = []
    for i, P in enumerate(Ps):
        im = k3.draw_blade(P, BLADE[i], S).resize((W, W))
        w = P["RIGHT ARM"]
        tip = (w[0] + k3.BL * math.cos(math.radians(BLADE[i])), w[1] - k3.BL * math.sin(math.radians(BLADE[i])))
        tips.append(tip)
        sheet.paste(im, (i * W, 30))
    d = ImageDraw.Draw(sheet)
    sc = W / (64.0)  # 그림 좌표 → 시트(스틱 캔버스 = 64 가정)
    for i, t in enumerate(tips):
        d.text((i * W + 6, 6), ["1 예비", "2 스윙", "3 타격", "4 끝"][i], fill=(255, 255, 255))
    # 칼끝 호(모든 칸에 같은 위치로 겹쳐 그림 = 첫 칸 좌표계)
    for i in range(n):
        pts = [(i * W + t[0] * sc, 30 + t[1] * sc) for t in tips]
        d.line(pts, fill=(80, 200, 255), width=2)
        for p in pts:
            d.ellipse([p[0] - 3, p[1] - 3, p[0] + 3, p[1] + 3], outline=(80, 200, 255))
        # 체중(앞발 쪽 비율) 표시
        f = Ps[i]["LEFT LEG"][0] if "LEFT LEG" in Ps[i] else 0
    sheet.save("Logs/motion_review/up9_side_sheet.png")
    fr = [k3.draw_blade(P, BLADE[i], S).resize((384, 384)) for i, P in enumerate(Ps)]
    fr[0].save("Logs/motion_review/up9_side.gif", save_all=True, append_images=fr[1:], duration=[200, 100, 200, 400], loop=0)
    print("ok", n, [tuple(round(v, 1) for v in t) for t in tips])
