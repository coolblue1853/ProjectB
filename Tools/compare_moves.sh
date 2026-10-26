#!/usr/bin/env bash
# 기본 이동·점프 비교 촬영(Q-423): 낮·칼 장착 후 walk/run/jump 를 0.04초 간격 30컷 → Logs/playtest/cmp_<태그>_<동작>.png
# 사용: bash Tools/compare_moves.sh <태그> [original]   (original 이면 예전 AnyPortrait 주인공으로 되돌려 찍는다)
cd "$(dirname "$0")/.."
TAG=$1; MODE=${2:-}
U="bash Tools/u.sh"
$U call manage_editor '{"action":"play"}' >/dev/null 2>&1
sleep 15
$U cs Tools/probes/set_day.cs >/dev/null 2>&1
$U cs Tools/probes/input_bg_on.cs >/dev/null 2>&1
[ "$MODE" = "original" ] && $U cs Tools/probes/use_original_rig.cs 2>&1 | head -c 80 && echo
$U cs Logs/equip.cs >/dev/null 2>&1
sleep 3
$U cs Tools/probes/set_day.cs >/dev/null 2>&1
for a in walk run jump; do
  sed -e "s/__ACTION__/$a/g" \
      -e 's/int n = action == "combo3" ? 12 : action == "jump" || action == "fall" ? 10 : 8, w = 256;/int n = 30, w = 256;/' \
      -e 's/float step = action == "brake" || action == "turn" ? 0.04f :/float step = 0.04f; float _unused = action == "brake" ? 0.04f :/' \
      -e "s#Logs/playtest/rig_act___ACTION__.png#Logs/playtest/cmp_${TAG}_$a.png#; s#Logs/playtest/rig_act_$a.png#Logs/playtest/cmp_${TAG}_$a.png#" \
      Tools/probes/rig_action_strip.cs > Logs/ra_cmp.cs
  $U cs Logs/ra_cmp.cs 2>&1 | head -c 60; echo
  sleep 6
done
$U call read_console '{"action":"get","types":["error"],"count":"3","format":"plain"}' 2>&1 | head -c 200; echo
$U call manage_editor '{"action":"stop"}' >/dev/null 2>&1
