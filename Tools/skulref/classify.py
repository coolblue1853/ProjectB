"""스컬 추출 스프라이트(참고 전용, 저장소 밖)를 아틀라스 묶음 + 애니 단위로 세세히 분류한다.
사용: python Tools/skulref/classify.py C:/Users/user/SkulRef
입력: <root>/meta.json(meta.py 결과), <root>/spr/...(assetName_pathID 로 내보낸 png)
결과: <root>/lib/
  character/<player_head|enemy|boss|hero_enemy|npc>/<묶음>/  애니별 띠 png + _sheet.png(애니 대표 프레임)
  effect/<속성·형태>/<출처>__<애니>.png
  background/<소분류>/..., ui/<소분류>/..., item/<소분류>/..., unknown/...
  index.json(애니마다 분류·묶음·프레임 수·크기·피벗·PPU), summary.md(분류별 개수·예시)
주의: 스컬 그림은 참고용이다. 저장소·게임에 넣지 않는다.
"""
import json
import os
import re
import sys
from collections import Counter, defaultdict

from PIL import Image, ImageDraw

R = lambda p: re.compile(p, re.I)
FRAME = re.compile(r"^(.*?)(?:_(\d+))?$")

# (대분류, 소분류, 이름 정규식) — 위에서부터 먼저 맞는 것을 쓴다.
RULES = [
    ("ui", "icon_skill", R(r"skill.*icon|icon.*skill")),
    ("ui", "icon_item_frame", R(r"item.*(icon|frame)|quintessence|^frame_")),
    ("ui", "icon_misc", R(r"icon")),
    ("ui", "portrait", R(r"portrait|illust|standing|_face$|^face_")),
    ("ui", "hud_gauge", R(r"gauge|healthbar|hp_?bar|cooldown|hud|_bar$|^bar_")),
    ("ui", "button_key", R(r"button|btn|keyguide|keyboard|gamepad|stick")),
    ("ui", "window_frame", R(r"frame|window|panel|popup|tooltip|slot|dialog|speech|bubble_?ui")),
    ("ui", "font_number", R(r"font|digit|damage_?text")),
    ("ui", "title_loading", R(r"title|logo|menu|tutorial|loading|credit|ending")),
    ("item", "food_consumable", R(r"burger|cake|egg|chicken|soup|meat|bread|potion|amanitas|special|dish|food")),
    ("item", "drop_currency", R(r"gold|coin|gem|quartz|bone$|^bone|bonechip|heart|money|reward")),
    ("effect", "slash_arc", R(r"slash|swing|cut|blade|crescent|claw|scratch|ranbu|iaido|ilseom|draw_")),
    ("effect", "thrust_pierce", R(r"thrust|stab|pierce|lance|spear|sting|lunge")),
    ("effect", "impact_hit", R(r"impact|hit|strike|smash|stomp|crash|slam|punch|landing")),
    ("effect", "explosion", R(r"explo|blast|bomb|burst|boom|meteor")),
    ("effect", "projectile", R(r"projectile|bullet|arrow|shot|missile|orb|ball|throw|bolt|needle|muzzle")),
    ("effect", "beam_laser", R(r"beam|laser|ray")),
    ("effect", "electric", R(r"electric|thunder|lightning|spark|volt")),
    ("effect", "fire", R(r"fire|flame|burn|ember|lava|magma|bonfire")),
    ("effect", "ice", R(r"ice|frost|glacial|freeze|snow|cold")),
    ("effect", "water", R(r"aqua|water|wave|tide|splash|flood|rain")),
    ("effect", "poison_curse", R(r"poison|venom|toxic|curse|virus|bleed")),
    ("effect", "dark_soul", R(r"dark|shadow|abyss|void|ghost|soul|spirit|lich|death")),
    ("effect", "holy_light", R(r"holy|light|judg|halo|angel|divine|grace|bless")),
    ("effect", "nature", R(r"leaf|vine|wood|root|thorn|growth|flower|petal|nature")),
    ("effect", "aura_buff", R(r"aura|buff|heal|shield|barrier|glow|awaken|enhance|charge|charging|power|rage")),
    ("effect", "smoke_dust", R(r"smoke|dust|puff|mist|fog|steam|cloud")),
    ("effect", "debris_blood", R(r"debris|piece|fragment|shard|chip|blood|gib|parts?\d")),
    ("effect", "sign_warning", R(r"sign|warning|alert|danger|target|mark|exclam|question")),
    ("effect", "portal_summon", R(r"portal|summon|spawn|appear|teleport|warp|gate|intro")),
    ("effect", "trail_wind", R(r"trail|afterimage|outline|line|streak|wind|gust|tornado|whirl|cyclone")),
    ("effect", "chain_rope", R(r"chain|rope|hook|grab")),
    ("effect", "generic", R(r"effect|fx|vfx|particle|flash|activate|loop")),
    ("background", "tile_ground", R(r"tile|ground|floor|platform|terrain|brick|stair|step")),
    ("background", "prop_nature", R(r"tree|grass|bush|rock|stone|mushroom|crystal|moss")),
    ("background", "prop_structure", R(r"wall|pillar|column|statue|door|bridge|fence|roof|house|castle|tower|ruin|throne|entrance|gargoyle")),
    ("background", "prop_object", R(r"chest|box|barrel|crate|torch|lamp|candle|banner|flag|cage|table|chair|shelf|book|altar|coffin|grave|curtain")),
    ("background", "layer_sky", R(r"background|^bg|_bg|sky|moon|sun|parallax|layer")),
    ("background", "misc", R(r"deco|prop|map|stage|room|level")),
]

BODY = [
    ("01_idle", R(r"^(idle|wait|stand)")), ("02_walk_run", R(r"^(walk|run|move)")), ("03_dash", R(r"^(dash|backstep|rolling|rush)")),
    ("04_jump_fall", R(r"^(jump(?!.*attack)|fall|land)")), ("05_jump_attack", R(r"^(jump.*attack|air)")),
    ("06_attack_basic", R(r"^attack")), ("07_skill", R(r"^(skill|special|ultimate|charge|casting|cast)")),
    ("08_swap", R(r"^(swap|switch|swtich)")), ("09_guard_parry", R(r"^(guard|parry|counter)")),
    ("10_hit_dead", R(r"^(hit|hurt|dead|die|death|stun|down|knock|groggy)")), ("11_named_move", R(r".")),
]


def has(names, p):
    return any(re.match(p, n, re.I) for n in names)


def role(names, nspr):
    if has(names, "idle") and has(names, "walk") and has(names, "dash") and has(names, "jump_?attack"):
        return "player_head"
    if has(names, r"idle|wait") and has(names, r"attack|hit|dead|die|casting"):
        if any(re.match(r"(adventurer|hero|firsthero|darkfirsthero)", n, re.I) for n in names):
            return "hero_enemy"
        if nspr >= 250 or has(names, r"(awaken|phase|\dphase|groggy|intro)"):
            return "boss"
        return "enemy"
    if has(names, r"talk|greeting|npc|interaction") and has(names, "idle"):
        return "npc"
    return None


def rule(name):
    for big, sub, rx in RULES:
        if rx.search(name):
            return big, sub
    return None


def strip(paths, out):
    ims = []
    for p in paths[:24]:
        try:
            ims.append(Image.open(p).convert("RGBA"))
        except OSError:
            pass
    if not ims:
        return
    w = sum(i.width for i in ims) + 2 * (len(ims) - 1)
    img = Image.new("RGBA", (w, max(i.height for i in ims)))
    x = 0
    for i in ims:
        img.alpha_composite(i, (x, img.height - i.height))
        x += i.width + 2
    img.save(out)


def contact(anims, out, cell=112, cols=8):
    keys = sorted(anims)
    rows = (len(keys) + cols - 1) // cols
    img = Image.new("RGBA", (cols * cell, rows * (cell + 14)), (36, 36, 40, 255))
    d = ImageDraw.Draw(img)
    for i, k in enumerate(keys):
        fr = anims[k]
        try:
            im = Image.open(fr[len(fr) // 2]).convert("RGBA")
        except OSError:
            continue
        im.thumbnail((cell - 4, cell - 4), Image.NEAREST)
        x, y = (i % cols) * cell, (i // cols) * (cell + 14)
        img.alpha_composite(im, (x + 2, y + 14))
        d.text((x + 2, y + 1), f"{k[:15]} x{len(fr)}", fill=(255, 230, 120))
    img.save(out)


def safe(s):
    return re.sub(r"[^\w\-()]+", "_", s)[:60]


def group_name(gkey, names, r):
    if {"GetSkull", "Reborn1(Castle)", "Wait"} <= names:
        return safe("LittleBone_" + gkey.split("|")[0][:6] + gkey.split(":")[-1][-4:])  # 기본 스컬(리틀 본)
    bundle = gkey.split("|")[0][:6]
    tail = gkey.split(":")[-1][-4:]
    pref = Counter(n.split("_")[0] for n in names if "_" in n and not re.match(r"(attack|skill|jump|fall|dash|idle)", n, re.I))
    tag = pref.most_common(1)[0][0] if pref else ""
    if r == "player_head":
        sk = sorted(n for n in names if not re.match(r"(attack[a-e]?|dash|fall|fallrepeat|fall_repeat|idle|jump|jumpattack|jumpattack_land|walk|swap|switch)$", n, re.I))
        tag = (sk[0].split("_")[0] if sk else "basic")
    return safe(f"{tag}_{bundle}{tail}")


def main():
    root = sys.argv[1]
    rows = json.load(open(os.path.join(root, "meta.json"), encoding="utf-8"))
    lib = os.path.join(root, "lib")
    groups = defaultdict(lambda: defaultdict(list))
    info = {}
    for r in rows:
        m = FRAME.match(r["name"])
        stem, idx = m.group(1), int(m.group(2) or 0)
        p = os.path.join(root, "spr", r["dir"], f"{r['name']} @{r['pid']}.png")
        groups[r["group"]][stem].append((idx, p))
        info.setdefault((r["group"], stem), (r["w"], r["h"], round(r["px"], 2), round(r["py"], 2), r["ppu"]))
    index = []
    for gk, anims in groups.items():
        for s in anims:
            anims[s] = [p for _, p in sorted(anims[s])]
        names = set(anims)
        nspr = sum(len(v) for v in anims.values())
        rl = role(names, nspr) if not gk.endswith("|none") else None
        if rl:
            gname = group_name(gk, names, rl)
            od = os.path.join(lib, "character", rl, gname)
            os.makedirs(od, exist_ok=True)
            contact(anims, os.path.join(od, "_sheet.png"))
            for s, ps in anims.items():
                sub = next(b for b, rx in BODY if rx.match(s))
                fn = f"{sub}__{safe(s)}.png"
                strip(ps, os.path.join(od, fn))
                index.append({"cat": "character", "sub": rl, "group": gname, "anim": s, "body": sub,
                              "frames": len(ps), "size_pivot_ppu": info[(gk, s)], "file": f"character/{rl}/{gname}/{fn}"})
            continue
        src = gk.split("|")[0][:6] + gk.split(":")[-1][-4:]
        for s, ps in anims.items():
            hit = rule(s)
            big, sub = hit if hit else (("effect", "unnamed_anim") if len(ps) >= 3 else ("unknown", "still"))
            od = os.path.join(lib, big, sub)
            os.makedirs(od, exist_ok=True)
            fn = f"{safe(s)}__{src}.png"
            strip(ps, os.path.join(od, fn))
            index.append({"cat": big, "sub": sub, "group": src, "anim": s, "frames": len(ps),
                          "size_pivot_ppu": info[(gk, s)], "file": f"{big}/{sub}/{fn}"})
    json.dump(index, open(os.path.join(lib, "index.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=0)
    agg = defaultdict(list)
    for e in index:
        agg[(e["cat"], e["sub"])].append(e)
    with open(os.path.join(lib, "summary.md"), "w", encoding="utf-8") as o:
        o.write("| 대분류 | 소분류 | 애니 수 | 묶음 수 | 예시 |\n|---|---|---|---|---|\n")
        for (c, s), es in sorted(agg.items()):
            ex = ", ".join(sorted({e["anim"] for e in es})[:6])
            o.write(f"| {c} | {s} | {len(es)} | {len({e['group'] for e in es})} | {ex} |\n")
    print(len(index), "anims,", len(agg), "classes")


if __name__ == "__main__":
    main()
