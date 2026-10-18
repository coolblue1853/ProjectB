#!/usr/bin/env bash
# unity_mcp_call.py 단축 실행: bash Tools/u.sh call <tool> '<json>' | cs <file> | list | schema <tool>
cd "$(dirname "$0")/.."
export PATH="$PATH:/c/Users/user/.local/bin"
exec uv run -q --with mcp python Tools/unity_mcp_call.py "$@" 2>/dev/null
