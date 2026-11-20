#!/usr/bin/env bash
# 자동 플레이 봇 한 번: 세이브 백업 → 플레이 → (시험 준비) 사냥터로 → PlayBot.Run → N분 → 정지 → 플레이 끔 → 세이브 원복·md5 → 요약·밸런스 md
# 끝나면 <세션>_summary.md · _balance.md · _report.md(막힘·예외·사망·사람 대비 한 장, Tools/playrec_report.py) 를 만든다. 손 시험은 Logs/playrec_test/ (Tools/playtest.sh)
# 사용: [PREP_FRESH=1(새 게임: 세이브 폴더를 비우고 게임 자신의 새 게임 경로로 시작, 시간대 기본 day, 끝나면 원복·md5)] [PREP_LEVEL="<레벨> <골드>"] [PREP_TIME=day|night] [PREP_RECALL=<개수>] [PREP_BAG="<남길 물약> <남길 빈칸> [무기약화 0|1]"] bash Tools/bot_run.sh <분> [goal=hunt] [시작 맵=Lv1_NearForest_SideRoad] [순회 맵(쉼표, 기본 PlayBot.DefaultMaps, "-" = 이동 없음)]
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
ROOT=$(pwd)
FRESH=${PREP_FRESH:-}
if [ -n "$FRESH" ]; then # 사용자가 플레이 중이면(사람 기록이 2분 안에 갱신) 세이브를 건드리지 않는다. 직전 실행이 원복 못 한 백업(RESTORE_ME)도 알린다
  if [ -n "$(find Logs/playrec -maxdepth 1 -name '*.jsonl' -mmin -2 2>/dev/null | head -1)" ]; then echo "사용자 플레이 중(Logs/playrec 최근 갱신) - 중단"; exit 1; fi
  for d in Logs/playrec_bot/save_backup_*/RESTORE_ME; do [ -e "$d" ] && echo "경고: 원복되지 않은 백업 있음: $(dirname "$d")"; done
fi
mkdir -p "$BK"
cp -p "$SAVE_DIR"/*.es3* "$BK"/
(cd "$SAVE_DIR" && md5sum *.es3*) > "$BK/md5.txt"
echo "세이브 백업: $BK"
if [ "$(ls "$SAVE_DIR"/*.es3* 2>/dev/null | wc -l)" != "$(ls "$BK"/*.es3* 2>/dev/null | wc -l)" ]; then echo "백업 파일 수가 원본과 다름 - 중단"; exit 1; fi
if [ -n "$FRESH" ]; then # 새 게임: 백업 md5 가 맞는지 확인한 뒤에만 비운다(백업이 틀리면 중단)
  if ! (cd "$BK" && md5sum -c --quiet md5.txt); then echo "백업 md5 불일치 - 중단(세이브는 그대로)"; rm -rf "$TMP"; exit 1; fi
  PREP_TIME=${PREP_TIME:-day} # R5: 시간대 미지정이면 낮 고정
fi

probe() { # probe <이름> [sed 식]
  sed "${2:-s/^//}" "Tools/probes/$1.cs" > "$TMP/$1.cs"
  timeout 90 bash Tools/u.sh cs "$TMP/$1.cs"
}

restore() {
  timeout 60 bash Tools/u.sh call manage_editor '{"action":"stop"}' >/dev/null 2>&1
  sleep 3
  if [ -n "$FRESH" ]; then rm -f "${SAVE_DIR:?}"/*.es3*; fi # 새 게임이 만든 파일(.bac 포함)을 지운 뒤 원본만 되돌린다
  cp -p "$BK"/*.es3* "$SAVE_DIR"/
  if (cd "$SAVE_DIR" && md5sum -c --quiet "$ROOT/$BK/md5.txt"); then echo "세이브 원복 md5 일치"; rm -f "$BK/RESTORE_ME"; else echo "세이브 원복 md5 불일치!"; fi
  rm -rf "$TMP"
}
trap restore EXIT
if [ -n "$FRESH" ]; then
  timeout 60 bash Tools/u.sh call manage_editor '{"action":"stop"}' >/dev/null 2>&1 # 플레이 중이면 끈 뒤(끄는 저장이 끝난 뒤) 비운다
  sleep 3
  : > "$BK/RESTORE_ME" # 원복 전에 죽으면 이 표식이 남는다(원본은 이 백업 폴더, 다음 실행이 경고)
  rm -f "${SAVE_DIR:?}"/*.es3*
  echo "새 게임: 세이브 폴더 비움($(ls "$SAVE_DIR"/*.es3* 2>/dev/null | wc -l)개 남음)"
fi

probe bot_pending_source "s/__SRC__/bot/g"
timeout 60 bash Tools/u.sh call manage_editor '{"action":"play"}' | head -c 120; echo
MAP_OK=
for i in $(seq 1 20); do
  sleep 4
  r=$(probe bot_prep_map "s/__MAP__/$MAP/g")
  echo "준비: $r"
  case "$r" in *already*) MAP_OK=1; break ;; esac
done
if [ -z "$MAP_OK" ]; then echo "맵 준비 실패($MAP) - 경고 없이 진행하지 않고 중단"; exit 1; fi # R3: trap 이 세이브를 원복한다
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
if [ -n "${PREP_NOQUEST:-}" ]; then # 시험 준비: PREP_NOQUEST=1 — 진행 중 퀘스트 비움(퀘스트 받기 경로 시험)
  echo "퀘스트 비움: $(probe bot_prep_noquest)"
fi
if [ -n "${PREP_TIME:-}" ]; then # 시험 준비: PREP_TIME=day|night — 낮/밤 고정(런타임만, 밸런스 비교용)
  echo "시간 고정: $(probe bot_prep_time "s/__MODE__/${PREP_TIME}/g")"
fi
COND=$(probe bot_start_cond | tr -d '\r' | tail -1)
echo "시작 조건: $COND"
printf '%s\n' "$COND" > "$BK/start_cond.txt"
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
python -X utf8 Tools/playrec_report.py "$REC" --status "$r" --prep "fresh=${FRESH:-0} time=${PREP_TIME:-save} level=${PREP_LEVEL:--}" --cond "$COND" >/dev/null || echo "보고서 생성 실패"
echo "기록: $REC"
echo "요약: ${REC%.jsonl}_summary.md"
echo "밸런스: ${REC%.jsonl}_balance.md"
echo "보고서: ${REC%.jsonl}_report.md"
echo "최종 상태: $r"
