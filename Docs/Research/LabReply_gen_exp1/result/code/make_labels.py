"""정답 라벨 생성: python make_labels.py <기준선 regions 폴더> <gt_ops.json> <출력 labels 폴더>"""
import json, os, sys
import numpy as np
from PIL import Image

def main():
    reg, ops_path, out = sys.argv[1:4]
    ops = json.load(open(ops_path, encoding="utf-8"))
    os.makedirs(out, exist_ok=True)
    for f, lst in ops.items():
        if f.startswith("_"):
            continue
        lab = np.array(Image.open(os.path.join(reg, f + "_region.png")))
        alpha = lab > 0
        for op in lst:
            L, x0, y0, x1, y1 = op[:5]
            sub = lab[y0:y1 + 1, x0:x1 + 1]
            m = alpha[y0:y1 + 1, x0:x1 + 1].copy()
            if len(op) > 5:
                m &= np.isin(sub, op[5])
            sub[m] = L
        Image.fromarray(lab.astype(np.uint8), "L").save(os.path.join(out, f + "_region.png"))
    print("labels", len([k for k in ops if not k.startswith("_")]))

if __name__ == "__main__":
    main()
