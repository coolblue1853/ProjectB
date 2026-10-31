#!/usr/bin/env bash
# 한도에 걸려도 기다렸다 이어 가는 큐: bash Tools/codex/queue_wait.sh <이름…>
# 각 작업: 보고(.last.md)가 이미 있으면 건너뜀 → 실행 → 리뷰. LIMIT(75)면 5시간 창 리셋까지 기다렸다 같은 단계 재시도.
cd "$(dirname "$0")/../.."
wait_limit() { while [ "$(bash Tools/codex/usage.sh --limit)" = "LIMIT" ]; do sleep 300; done; }
step() { # $1=보고 파일, 나머지=명령
  local rep="$1"; shift
  [ -s "$rep" ] && return 0
  while :; do
    wait_limit
    "$@"; rc=$?
    [ $rc = 75 ] && { echo "LIMIT, 대기 후 재시도: $*"; sleep 60; continue; }
    return $rc
  done
}
for n in "$@"; do
  step "Logs/codex/$n.last.md" bash Tools/codex/run.sh "$n" "Logs/codex/$n.md"
  step "Logs/codex/$n.review.last.md" bash Tools/codex/review.sh "$n"
  echo "DONE $n"
done
