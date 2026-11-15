#!/usr/bin/env bash
# 자동 플레이 봇 한 번: 세이브 백업 → 플레이 → (시험 준비) 사냥터로 → PlayBot.Run → N분 → 정지 → 플레이 끔 → 세이브 원복·md5 → 요약·밸런스 md
# 사용: bash Tools/bot_run.sh <분> [goal=hunt] [맵=Lv1_NearForest_SideRoad]
# 기록은 Logs/playrec_bot/<세션>.jsonl (src=bot). 사람 기록 Logs/playrec/ 은 건드리지 않는다. 설계: .claude-rules/Game/PlayBotDesign.md 10절
set -u
cd "$(dirname "$0")/.."
MIN=${1:?사용: bot_run.sh <분> [goal] [맵]}
GOAL=${2:-hunt}
MAP=${3:-Lv1_NearForest_SideRoad}
SAVE_DIR=/c/Users/user/AppData/LocalLow/DefaultCompany/ProjectB
BK=Logs/playrec_bot/save_backup_$(date +%Y%m%d_%H%M%S)
TMP=$(mktemp -d)
mkdir -p "$BK"
cp -p "$SAVE_DIR"/*.es3* "$BK"/
(cd "$SAVE_DIR" && md5sum *.es3*) > "$BK/md5.txt"
echo "세이브 백업: $BK"

probe() { # probe <이름> [sed 식]
  sed "${2:-s/^//}" "Tools/probes/$1.cs" > "$TMP/$1.cs"
  timeout 90 bash Tools/u.sh cs "$TMP/$1.cs"
}

restore() {
  timeout 60 bash Tools/u.sh call manage_editor '{"action":"stop"}' >/dev/null 2>&1
  sleep 3
  cp -p "$BK"/*.es3* "$SAVE_DIR"/
  if (cd "$SAVE_DIR" && md5sum -c --quiet "$OLDPWD/$BK/md5.txt"); then echo "세이브 원복 md5 일치"; else echo "세이브 원복 md5 불일치!"; fi
  rm -rf "$TMP"
}
trap restore EXIT

probe bot_pending_source "s/__SRC__/bot/g"
timeout 60 bash Tools/u.sh call manage_editor '{"action":"play"}' | head -c 120; echo
for i in $(seq 1 20); do
  sleep 4
  r=$(probe bot_prep_map "s/__MAP__/$MAP/g")
  echo "준비: $r"
  case "$r" in *already*) break ;; esac
done
sleep 3
r=$(probe bot_start "s/__MIN__/$((MIN + 1))/; s/__GOAL__/$GOAL/")
echo "시작: $r"
case "$r" in *start=True*) ;; *) echo "봇 시작 실패"; exit 1 ;; esac
END=$(( $(date +%s) + MIN * 60 ))
while [ "$(date +%s)" -lt "$END" ]; do
  left=$(( END - $(date +%s) ))
  sleep $(( left < 60 ? left : 60 ))
  echo "상태: $(probe bot_status)"
done
r=$(probe bot_stop)
echo "정지: $r"
REC=$(echo "$r" | sed -n 's/.*rec=\(.*\.jsonl\).*/\1/p' | tr '\\' '/')
timeout 60 bash Tools/u.sh call read_console '{"action":"get","types":["error"],"count":"20","format":"plain"}' | head -c 1500; echo
trap - EXIT
restore
if [ -z "$REC" ] || [ ! -f "$REC" ]; then echo "기록 파일 없음: $REC"; exit 1; fi
python -X utf8 Tools/playrec_summary.py "$REC" >/dev/null
python -X utf8 Tools/playrec_balance.py "$REC" -o "${REC%.jsonl}_balance.md" >/dev/null
echo "기록: $REC"
echo "요약: ${REC%.jsonl}_summary.md"
echo "밸런스: ${REC%.jsonl}_balance.md"
