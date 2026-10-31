#!/usr/bin/env bash
# Codex 크레딧이 돌아올 때까지 30분마다 아주 작은 실행으로 확인: bash Tools/codex/wait_credits.sh
cd "$(dirname "$0")/../.."
C="${CODEX_BIN:-/c/Users/user/AppData/Local/OpenAI/Codex/bin/22cfb1d302613eae/codex.exe}"
while :; do
  out=$("$C" exec -s read-only "Reply with just: ok" 2>&1)
  if echo "$out" | grep -qi "out of credits\|usage limit\|credits_depleted"; then sleep 1800; continue; fi
  echo "$out" | grep -q "^ok" && { echo "CREDITS OK $(date)"; exit 0; }
  sleep 1800
done
