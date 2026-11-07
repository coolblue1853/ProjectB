"""PixelLab 문장 방식(pixminimax)이 그림에 같이 그려 넣는 이펙트(흰 호)를 지우는 도구 (11-07 올려베기 M1-a 에서 확정, 사용자 합격).
자동 색 필터만으로는 머리카락(크림색)까지 지워져 게임에 못 쓴다 → **칸마다 지울 영역(zone)을 눈으로 정해 JSON 에 적는다.**
사용: python Tools/pixelfx/erase_fx.py <spec.json>
spec 형식:
{
 "src": "입력 폴더(f0.png…)", "dst": "출력 폴더",
 "colors": [[248,248,248],[233,219,197],…],   # 이펙트색(없으면 흰색+회색 자동 + 이 목록)
 "frames": {
   "4": {"zones": [[x0,y0,x1,y1],…],            # 이 사각형 안의 이펙트색만 지운다(양끝 포함)
         "patch_from": {"frame": "4", "rows": [41,48], "xs": [20,46]},   # (선택) 지운/빈 곳을 다른 칸 같은 줄에서 복사
         "paint": [[x,y,[r,g,b]],…]}            # (선택) 직접 칠할 픽셀
 }
}
순서: ① patch_from(이펙트/빈 곳에만 복사) → ② zones 지우기 → ③ paint. 반드시 결과를 8배 확대해서 눈으로 확인한다."""
import json, os, sys
from PIL import Image


def gray(c):
    return abs(c[0] - c[1]) < 6 and abs(c[1] - c[2]) < 6 and c[0] > 90


def main(spec_path):
    sp = json.load(open(spec_path, encoding="utf8"))
    src, dst = sp["src"], sp["dst"]
    os.makedirs(dst, exist_ok=True)
    cols = {tuple(c) for c in sp.get("colors", [])}

    def eff(p):
        return p[3] > 0 and (p[:3] in cols or gray(p[:3]))

    ims = {}
    names = sorted(f for f in os.listdir(src) if f.endswith(".png"))
    for f in names:
        ims[f[1:-4]] = Image.open(os.path.join(src, f)).convert("RGBA")  # f0.png → "0"
    out = {k: v.copy() for k, v in ims.items()}
    for k, cfg in sp["frames"].items():
        im = out[k]
        px = im.load()
        pf = cfg.get("patch_from")
        if pf:
            ref = out[pf["frame"]].load()
            for y in range(pf["rows"][0], pf["rows"][1] + 1):
                for x in range(pf["xs"][0], pf["xs"][1] + 1):
                    if (px[x, y][3] == 0 or eff(px[x, y])) and ref[x, y][3] > 0 and not eff(ref[x, y]):
                        px[x, y] = ref[x, y]
        n = 0
        for z in cfg.get("zones", []):
            for y in range(z[1], z[3] + 1):
                for x in range(z[0], z[2] + 1):
                    if eff(px[x, y]):
                        px[x, y] = (0, 0, 0, 0)
                        n += 1
        for x, y, c in cfg.get("paint", []):
            px[x, y] = (c[0], c[1], c[2], 255)
        print("frame", k, "erased", n)
    for k, im in out.items():
        im.save(os.path.join(dst, "f%s.png" % k))
    print("ok", dst)


if __name__ == "__main__":
    main(sys.argv[1])
