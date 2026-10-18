#!/usr/bin/env bash
# unity_mcp_call.py 단축 실행: bash Tools/u.sh call <tool> '<json>' | cs <file> | list | schema <tool>
# manage_editor play 뒤에는 타이틀 오버레이를 자동 해제한다(자동화 프로브가 입력 잠금에 막히지 않게).
cd "$(dirname "$0")/.."
export PATH="$PATH:/c/Users/user/.local/bin"
run() { uv run -q --with mcp python Tools/unity_mcp_call.py "$@" 2>/dev/null; }
if [ "$1" = "call" ] && [ "$2" = "manage_editor" ] && [[ "$3" == *'"play"'* ]]; then
  run "$@"; rc=$?
  ( for i in 1 2 3 4 5 6; do sleep 2; run cs Tools/probes/dismiss_title.cs 2>/dev/null | grep -q "title dismissed" && break; done ) &
  exit $rc
fi
run "$@"
