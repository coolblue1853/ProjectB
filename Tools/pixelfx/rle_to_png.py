"""브라우저에서 팔레트+RLE 텍스트로 옮긴 GPT 이미지(JSON: w, pal, rows)를 PNG 로 되돌린다.
rows: 줄마다 팔레트 문자(공백 '.'). 반복은 두 형식을 모두 읽는다:
  새 형식 '`<개수>`<문자>' / 옛 형식 '`<개수><문자>'(문자가 숫자면 모호 → 줄 너비 w 에 맞는 해석을 백트래킹으로 찾는다)
사용: python Tools/pixelfx/rle_to_png.py <입력.json> <출력.png>"""
import json
import sys
from PIL import Image

A = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&()*+,-./:;<=>?@[]^_{|}~"


def expand(row, w):
    """백트래킹으로 길이 w 가 되는 전개를 찾는다"""
    sys.setrecursionlimit(10000)
    memo = {}

    def go(i, used):
        if i == len(row):
            return "" if used == w else None
        if used > w:
            return None
        key = (i, used)
        if key in memo:
            return memo[key]
        res = None
        if row[i] == "`":
            j = i + 1
            while j < len(row) and row[j].isdigit():
                j += 1
            digits = row[i + 1:j]
            if j < len(row) and row[j] == "`":  # 새 형식
                n = int(digits)
                rest = go(j + 2, used + n)
                if rest is not None:
                    res = row[j + 1] * n + rest
            else:
                # 옛 형식: 숫자 일부가 반복 문자일 수 있다 → 긴 개수부터 시도
                for k in range(len(digits), 0, -1):
                    n = int(digits[:k])
                    if k < len(digits):
                        ch, nxt = digits[k], i + 1 + k + 1
                    else:
                        if j >= len(row):
                            continue
                        ch, nxt = row[j], j + 1
                    rest = go(nxt, used + n)
                    if rest is not None:
                        res = ch * n + rest
                        break
        else:
            rest = go(i + 1, used + 1)
            if rest is not None:
                res = row[i] + rest
        memo[key] = res
        return res

    out = go(0, 0)
    if out is None:
        raise ValueError("row decode failed")
    return out


def main():
    data = json.load(open(sys.argv[1], encoding="utf-8"))
    w = data["w"]
    pal = [tuple(int(v) for v in c.split(",")) for c in data["pal"]]
    rows = [expand(r, w) for r in data["rows"].split("\n")]
    im = Image.new("RGBA", (w, len(rows)), (0, 0, 0, 0))
    px = im.load()
    for y, r in enumerate(rows):
        for x, ch in enumerate(r):
            if ch != ".":
                px[x, y] = pal[A.index(ch)] + (255,)
    im.save(sys.argv[2])
    print("ok", im.size, len(pal), "colors")


if __name__ == "__main__":
    main()
