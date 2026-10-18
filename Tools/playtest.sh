#!/usr/bin/env bash
# 플레이 테스트 한 사이클: 씬 로드 → 콘솔 비우기 → Play → N초 대기 → 스크린샷 → 콘솔 수집 → Stop
# 사용: bash Tools/playtest.sh [대기초=8] [출력이름=playtest]
set -u
cd "$(dirname "$0")/.."
export PATH="$PATH:/c/Users/user/.local/bin"
WAIT=${1:-8}
NAME=${2:-playtest}
C="uv run -q --with mcp python Tools/unity_mcp_call.py"
mkdir -p Logs/playtest

$C call manage_scene '{"action":"load","path":"Assets/Scenes/SampleScene.unity"}' 2>/dev/null | head -c 300; echo
$C call read_console '{"action":"clear"}' >/dev/null 2>&1
$C call manage_editor '{"action":"play"}' 2>/dev/null | head -c 300; echo
sleep "$WAIT"
$C call manage_camera '{"action":"screenshot","properties":{"include_image":true}}' "Logs/playtest/$NAME.png" 2>/dev/null | grep -v '"data"' | head -c 400; echo
$C call read_console '{"action":"get","types":["error","warning"],"count":"100","format":"plain"}' 2>/dev/null > "Logs/playtest/$NAME.console.json"
$C call manage_editor '{"action":"stop"}' 2>/dev/null | head -c 200; echo
python - "Logs/playtest/$NAME.console.json" <<'EOF'
import json, sys, collections
raw = open(sys.argv[1], encoding="utf-8").read()
try:
    d = json.loads(raw)
except Exception:
    print(raw[:2000]); sys.exit()
items = d.get("data") or []
c = collections.Counter(i.strip().split("\n")[0][:220] for i in items if isinstance(i, str))
print(f"console entries: {len(items)} (unique {len(c)})")
for k, v in c.most_common(40):
    print(f"{v:3d}x {k}")
EOF
