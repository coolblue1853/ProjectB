"""스컬 스프라이트 덤프(.txt)에서 아틀라스·크기·피벗을 뽑아 묶음(그룹)을 만든다(참고 전용).
사용: python Tools/skulref/meta.py C:/Users/user/SkulRef/sprdump C:/Users/user/SkulRef/meta.json
그룹 키: 같은 파일 안 m_SpriteAtlas PathID(없으면 텍스처 PathID) = 캐릭터·이펙트 한 벌
"""
import json
import os
import re
import sys

NAME = re.compile(r'm_Name = "(.*)"')
FLOAT = lambda k: re.compile(r"Rectf m_Rect.*?float " + k + r" = ([-\d.E]+)", re.S)
PIVOT = re.compile(r"Vector2f m_Pivot\s+float x = ([-\d.E]+)\s+float y = ([-\d.E]+)")
PPU = re.compile(r"m_PixelsToUnits = ([-\d.E]+)")
ATLAS = re.compile(r"PPtr<SpriteAtlas> m_SpriteAtlas\s+int m_FileID = (-?\d+)\s+SInt64 m_PathID = (-?\d+)")
TEX = re.compile(r"PPtr<Texture2D> texture\s+int m_FileID = (-?\d+)\s+SInt64 m_PathID = (-?\d+)")
RECT = re.compile(r"Rectf m_Rect\s+float x = [-\d.E]+\s+float y = [-\d.E]+\s+float width = ([-\d.E]+)\s+float height = ([-\d.E]+)")


def main():
    src, out = sys.argv[1], sys.argv[2]
    rows = []
    for root, _, files in os.walk(src):
        rel = os.path.relpath(root, src).replace(os.sep, "/")
        for f in files:
            if not f.endswith(".txt"):
                continue
            pid = f.rsplit(" @", 1)[-1][:-4]
            t = open(os.path.join(root, f), encoding="utf-8", errors="replace").read()
            if t.lstrip().startswith("{"):  # 타입트리 없는 파일은 JSON 덤프
                d = json.loads(t)
                a, rc, pv = d.get("m_SpriteAtlas", {}), d.get("m_Rect", {}), d.get("m_Pivot", {})
                tx = d.get("m_RD", {}).get("texture", {})
                akey = f"A{a.get('m_FileID')}:{a['m_PathID']}" if a.get("m_PathID") else (f"T{tx.get('m_FileID')}:{tx['m_PathID']}" if tx.get("m_PathID") else "none")
                rows.append({"dir": rel, "pid": pid, "name": d.get("m_Name", f[:-4]),
                             "group": rel.split("/")[0].split(".")[0][:12] + "|" + rel.split("/")[-1][:16] + "|" + akey,
                             "w": rc.get("width", 0), "h": rc.get("height", 0), "px": pv.get("X", .5), "py": pv.get("Y", .5),
                             "ppu": d.get("m_PixelsToUnits", 0)})
                continue
            t = t[:6000]
            m = NAME.search(t)
            a, tx, r, pv, pp = ATLAS.search(t), TEX.search(t), RECT.search(t), PIVOT.search(t), PPU.search(t)
            akey = f"A{a.group(1)}:{a.group(2)}" if a and a.group(2) != "0" else (f"T{tx.group(1)}:{tx.group(2)}" if tx else "none")
            rows.append({
                "dir": rel, "pid": pid, "name": m.group(1) if m else f[:-4],
                "group": rel.split("/")[0].split(".")[0][:12] + "|" + rel.split("/")[-1][:16] + "|" + akey,
                "w": float(r.group(1)) if r else 0, "h": float(r.group(2)) if r else 0,
                "px": float(pv.group(1)) if pv else .5, "py": float(pv.group(2)) if pv else .5,
                "ppu": float(pp.group(1)) if pp else 0,
            })
    json.dump(rows, open(out, "w", encoding="utf-8"), ensure_ascii=False)
    groups = {r["group"] for r in rows}
    print(len(rows), "sprites,", len(groups), "groups")


if __name__ == "__main__":
    main()
