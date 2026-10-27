"""마스터 모션 38개마다 스컬 라이브러리에서 같은 계열 애니를 찾아 참고 시트를 만든다(참고 전용, 저장소 밖에 저장).
사용: python Tools/skulref/m38_refs.py C:/Users/user/SkulRef
결과: <root>/compare/m38/NN_이름.png(후보 띠 최대 6개), 표준출력에 마크다운 표(모션 | 후보 | 프레임 수)
"""
import json
import os
import re
import sys

from PIL import Image, ImageDraw, ImageFont

# (번호, 이름, 애니 이름 정규식, 우선 소분류) — 머리(player_head)를 먼저 본다
M38 = [
    (1, "대기", r"^idle$", "player_head"), (2, "걷기", r"^walk$", "player_head"), (3, "달리기", r"^(run|dash)$", "player_head"),
    (4, "제동", r"^(dash_end|run_end|move_end)$", None), (5, "점프 웅크림", r"^jump$", "player_head"), (6, "공중", r"^fallrepeat$", "player_head"),
    (7, "착지", r"^(jumpattack_land|land|landing)$", None), (8, "대시", r"^dash$", "player_head"), (9, "피격", r"^hit\d*$", None),
    (10, "사망", r"^(dead|die)$", None), (11, "로프 매달리기", r"^(hang|climb)", None), (12, "로프 오르기", r"^(climb)", None),
    (13, "A1 내려베기", r"^(attacka|verticaldownslash_attack|slash_vertical(_a_active)?|chop_attack)$", "player_head"),
    (14, "A2 올려베기", r"^(attackb|verticalupslash_attack|risingslash_basicattack_completed|upper_attack)$", "player_head"),
    (15, "A3 찌르기", r"^(pierce_attack_skill|thrust_attack|rapidthrust_attack|stab)", None),
    (16, "A4 양손 내려찍기", r"^(slam_attack|heavenearthslam_attack|stomp_attack|smash)", None),
    (17, "A5 양손 수평 휘두르기", r"^(slash_horizontal(_a_active)?|horizontalslash_attack|swing_attack)$", None),
    (18, "A6 연속 찌르기", r"^(rapidthrust_attack|ranbu_attack|flurry)", None), (19, "A7 교차 베기", r"^(slash_cross_attack|cross)", None),
    (20, "A8 공중 내려치기", r"^jumpattack$", "player_head"), (21, "A9 돌진 찌르기", r"^(rush|dashattack|tackle|shieldcharge|lunge)", None),
    (22, "A10 회전 베기", r"^(cyclone|spinattack|buddhacyclone|whirlwind)", None), (23, "A11 시전", r"^casting", None),
    (24, "A12 던지기", r"^(throw|drawcard_front_attack|skill1)$", "player_head"), (25, "A13 활", r"^(fastshot_attack|slowshot_attack|chargeattack_completed)", None),
    (26, "A14 막기", r"^(guard|parry)$", None), (27, "A15 모으기", r"^(chargeattack_charging|concentration|charge_ready)", None),
    (28, "A16 도약 내려찍기", r"^(madrush_land|jumpattack_land|stomp_a_attack|heavenearthslam_attack)", None), (29, "A17 백스텝", r"^backstep$", None),
    (30, "A18 주먹 3타", r"^(punch_attack|hook_attack|bodyblow_attack|attacka)$", None), (31, "A19 발도", r"^(draw_attack|ilseom_attack|iaido)", None),
    (32, "R1 어깨 겨눔", r"^(fire_attack|revolver|aim)", None), (33, "R2 양손 들기", r"^(skill_ready|casting_ready|buff_ready)", None),
    (34, "R3 잡기", r"^(grab_forward|dashgrab_attack|grab_attack|naturesgrab_completed_grab)", None), (35, "R4 앉기(탈것)", r"^(hellbike|ride|sit)", None),
    (36, "R5 넘어짐·일어남", r"^(groggy_ready|down|knockdown|reborn1)", None), (37, "R6 웅크려 놓기", r"^(interaction|place|get)", None),
    (38, "R7 발차기", r"^kick(_attack)?$", None),
]


def main():
    root = sys.argv[1]
    lib = os.path.join(root, "lib")
    ix = [e for e in json.load(open(os.path.join(lib, "index.json"), encoding="utf-8")) if e["cat"] == "character"]
    out = os.path.join(root, "compare", "m38")
    os.makedirs(out, exist_ok=True)
    font = ImageFont.truetype("C:/Windows/Fonts/malgunbd.ttf", 14)
    print("| # | 모션 | 스컬 후보(묶음/애니, 프레임) |\n|---|---|---|")
    for no, name, rx, pref in M38:
        r = re.compile(rx, re.I)
        hits = [e for e in ix if r.match(e["anim"])]
        hits.sort(key=lambda e: (e["sub"] != (pref or e["sub"]), e["sub"] != "player_head", -min(e["frames"], 8)))
        seen, pick = set(), []
        for e in hits:
            key = e["group"].split("_")[0] + e["anim"]
            if key in seen:
                continue
            seen.add(key)
            pick.append(e)
            if len(pick) == 6:
                break
        print(f"| {no} | {name} | " + ", ".join(f"{e['group']}/{e['anim']}({e['frames']})" for e in pick) + " |")
        if not pick:
            continue
        ims = [(f"{e['group']}/{e['anim']} x{e['frames']}", Image.open(os.path.join(lib, e["file"])).convert("RGBA")) for e in pick]
        w = max(i.width for _, i in ims)
        sheet = Image.new("RGBA", (min(w, 2400), sum(i.height + 18 for _, i in ims) + 24), (36, 36, 40, 255))
        d = ImageDraw.Draw(sheet)
        d.text((4, 2), f"{no:02d} {name}", font=font, fill=(255, 255, 255))
        y = 24
        for t, i in ims:
            d.text((4, y), t, font=font, fill=(255, 230, 120))
            sheet.alpha_composite(i.crop((0, 0, min(i.width, 2400), i.height)), (0, y + 16))
            y += i.height + 18
        sheet.save(os.path.join(out, f"{no:02d}_{name.replace(' ', '_')}.png"))


if __name__ == "__main__":
    main()
