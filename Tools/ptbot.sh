#!/usr/bin/env bash
# PT-BOT: 에디터 플레이 모드 자동 플레이 봇. 사용: bash Tools/ptbot.sh [옵션 key=value ...]
#  zones=콤마목록(기본 Lv1_NearForest_SideRoad) zoneSec=구역당 게임초(120) totalRealSec=실시간 상한(280) timescale=0.5~4(2)
#  quests=0|1 boss=0|1 god=0|1 night=0|1 givePotions=N(5) weapon=이름|none(none=현재 장비 유지) lv=시작레벨(1) logEvery=5 stuckSec=6 potionPct=0.45
# 흐름: 세이브 백업(orig 1회 + run_시각) → 플레이 → 무기/레벨 준비 → 봇 실행·폴링 → 정지 → 세이브 복원 → 보고서 Logs/ptbot/report.md. 형식은 Docs/Playtest/PtBot.md.
set -u
cd "$(dirname "$0")/.."
# 원본 복원 전용: bash Tools/ptbot.sh restore-orig  (Logs/pt_save/orig -> 세이브 폴더)
if [ "${1:-}" = "restore-orig" ]; then
  [ -n "$(ls -A Logs/pt_save/orig 2>/dev/null)" ] || { echo "no orig backup"; exit 1; }
  cp -f Logs/pt_save/orig/* "/c/Users/user/AppData/LocalLow/DefaultCompany/ProjectB"/ && echo "orig restored"; exit 0
fi
U="bash Tools/u.sh"
OUT=Logs/ptbot; mkdir -p $OUT
SAVEDIR="/c/Users/user/AppData/LocalLow/DefaultCompany/ProjectB"
declare -A O=( [zones]=Lv1_NearForest_SideRoad [zoneSec]=120 [totalRealSec]=280 [timescale]=2 [quests]=0 [boss]=0 [god]=0 [night]=0 [givePotions]=5 [weapon]=none [lv]=1 [logEvery]=5 [stuckSec]=6 [potionPct]=0.45 )
for a in "$@"; do O[${a%%=*}]=${a#*=}; done
{ for k in "${!O[@]}"; do echo "$k=${O[$k]}"; done; } > $OUT/config.txt
rm -f $OUT/stop $OUT/summary.txt $OUT/run.log
# 세이브 백업: 최초 원본(Logs/pt_save/orig, 이미 있으면 덮어쓰지 않음) + 실행별 사본(Logs/pt_save/run_시각). 복원은 이번 실행 시작 사본.
SAVEBK=Logs/pt_save; ORIG=$SAVEBK/orig; RUN=$SAVEBK/run_$(date +%Y%m%d_%H%M%S)
mkdir -p $ORIG $RUN
[ -z "$(ls -A $ORIG 2>/dev/null)" ] && cp -p "$SAVEDIR"/*.es3* $ORIG/ 2>/dev/null
cp -p "$SAVEDIR"/*.es3* $RUN/ 2>/dev/null
restore() {
  $U call manage_editor '{"action":"stop"}' >/dev/null 2>&1; sleep 3
  cp -f $RUN/* "$SAVEDIR"/ 2>/dev/null
  echo "save restored from $RUN"
}
trap restore EXIT
$U call manage_scene '{"action":"load","path":"Assets/Scenes/SampleScene.unity"}' >/dev/null 2>&1
$U call read_console '{"action":"clear"}' >/dev/null 2>&1
$U call manage_editor '{"action":"play"}' | head -c 80; echo
sleep 14
$U cs Tools/probes/dismiss_title.cs | head -c 80; echo
sed -e "s/__W__/${O[weapon]}/" -e "s/__LV__/${O[lv]}/" Tools/probes/ptbot_prep.cs > $OUT/prep.cs
$U cs $OUT/prep.cs | head -c 200; echo
$U cs Tools/probes/ptbot_run.cs | head -c 300; echo
LIMIT=$(( ${O[totalRealSec]} + 60 ))
for i in $(seq 1 $((LIMIT/5))); do
  sleep 5
  [ -f $OUT/summary.txt ] && break
done
[ -f $OUT/summary.txt ] || { $U cs Tools/probes/ptbot_stop.cs >/dev/null; sleep 4; }
$U call read_console '{"action":"get","types":["error"],"count":"100","format":"plain"}' > $OUT/console_errors.json 2>/dev/null
python Tools/ptbot_report.py $OUT > $OUT/report.md 2>&1
cat $OUT/report.md | head -60
