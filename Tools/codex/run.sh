#!/usr/bin/env bash
# Codex 에 구현 작업 맡기기: bash Tools/codex/run.sh <이름> <지시서.md> [추론 강도 low|medium|high|xhigh]
# 결과: Logs/codex/<이름>.last.md(마지막 보고), <이름>.log(전체 로그). 남은 사용량은 Tools/codex/usage.sh
cd "$(dirname "$0")/../.."
CODEX="${CODEX_BIN:-/c/Users/user/AppData/Local/OpenAI/Codex/bin/22cfb1d302613eae/codex.exe}"
name="$1"; brief="$2"; effort="${3:-auto}"
# auto: 남은 사용량과 리셋 시점으로 강도 결정(usage.sh 규칙)
[ "$effort" = "auto" ] && effort=$(bash Tools/codex/usage.sh --effort)
echo "effort=$effort"
mkdir -p Logs/codex
# 한도 근접이면 실행하지 않고 75 로 끝낸다 → 큐가 멈추고 Claude 가 직접 작업(사용자 10-31)
if [ "$(bash Tools/codex/usage.sh --limit)" = "LIMIT" ]; then echo "LIMIT $name"; exit 75; fi
# 샌드박스는 %LOCALAPPDATA%\uv 캐시 잠금을 막는다 → 작업 폴더 안 캐시로(u.sh 의 uv run 용)
export UV_CACHE_DIR="$PWD/Logs/uv-cache"
export PATH="/c/Users/user/AppData/Roaming/Python/Python314/Scripts:$PATH"
# 작업 폴더 쓰기 + 네트워크(unity-mcp localhost) + 세이브 폴더만 허용. 커밋 금지 등은 AGENTS.md 로 막는다.
"$CODEX" exec -s workspace-write -c sandbox_workspace_write.network_access=true \
  -c model_reasoning_effort="$effort" \
  --add-dir "$USERPROFILE/AppData/LocalLow/DefaultCompany/ProjectB" -C "$PWD" \
  -o "Logs/codex/$name.last.md" - < "$brief" > "Logs/codex/$name.log" 2>&1
rc=$?
# 실행 중 한도에 걸린 경우
grep -qi "usage limit\|rate limit reached\|hit your usage limit\|out of credits" "Logs/codex/$name.log" && { echo "LIMIT $name"; exit 75; }
echo "exit=$rc report=Logs/codex/$name.last.md"
