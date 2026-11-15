#!/usr/bin/env bash
# 자동 플레이 봇 한 번: 세이브 백업 → 플레이 → (시험 준비) 사냥터로 → PlayBot.Run → N분 → 정지 → 플레이 끔 → 세이브 원복·md5 → 요약·밸런스 md
# 끝나면 <세션>_summary.md · _balance.md · _report.md(막힘·예외·사망·사람 대비 한 장, Tools/playrec_report.py) 를 만든다. 손 시험은 Logs/playrec_test/ (Tools/playtest.sh)
# 사용: [PREP_LEVEL="<레벨> <골드>"] [PREP_RECALL=<개수>] [PREP_BAG="<남길 물약> <남길 빈칸> [무기약화 0|1]"] bash Tools/bot_run.sh <분> [goal=hunt] [시작 맵=Lv1_NearForest_SideRoad] [순회 맵(쉼표, 기본 PlayBot.DefaultMaps, "-" = 이동 없음)]
# 기록은 Logs/playrec_bot/<세션>.jsonl (src=bot). 사람 기록 Logs/playrec/ 은 건드리지 않는다. 설계: .claude-rules/Game/PlayBotDesign.md 10절
set -u
cd "$(dirname "$0")/.."
MIN=${1:?사용: bot_run.sh <분> [goal] [맵]}
GOAL=${2:-hunt}
MAP=${3:-Lv1_NearForest_SideRoad}
MAPS=${4:-}
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
if [ -n "${PREP_BAG:-}" ]; then # 시험 준비: PREP_BAG="<남길 물약 수> <남길 빈칸 수> [1=착용 무기를 약한 것으로 바꿔 장착 시험]" (마을 업무 시험용, 봇 시작 전만)
  set -- $PREP_BAG
  echo "가방 준비: $(probe bot_prep_bag "s/__POT__/${1:-2}/g; s/__FREE__/${2:-3}/g; s/__WEAKEN__/${3:-0}/g")"
fi
if [ -n "${PREP_LEVEL:-}" ]; then # 시험 준비: PREP_LEVEL="<레벨> <골드>" — 낮은 레벨 상태(세이브는 끝나고 원복)
  set -- $PREP_LEVEL
  echo "레벨 준비: $(probe bot_prep_level "s/__LV__/${1:-3}/g; s/__GOLD__/${2:-300}/g")"
fi
if [ -n "${PREP_RECALL:-}" ]; then # 시험 준비: PREP_RECALL=<개수> — 귀환 주문서를 가방에 넣고 퀵슬롯 3번에 등록(귀환 주문서 실사용 확인)
  echo "귀환 주문서 준비: $(probe bot_prep_recall "s/__N__/${PREP_RECALL}/g")"
fi
r=$(probe bot_start "s/__MIN__/$((MIN + 1))/; s/__GOAL__/$GOAL/; s/__MAPS__/$MAPS/")
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
python -X utf8 Tools/playrec_report.py "$REC" --status "$r" >/dev/null || echo "보고서 생성 실패"
echo "기록: $REC"
echo "요약: ${REC%.jsonl}_summary.md"
echo "밸런스: ${REC%.jsonl}_balance.md"
echo "보고서: ${REC%.jsonl}_report.md"
echo "최종 상태: $r"
