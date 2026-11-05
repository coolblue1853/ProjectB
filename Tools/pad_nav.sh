#!/usr/bin/env bash
# AUTO-117: 패드/창 동작 하나 실행 후 상태 출력. 사용: bash Tools/pad_nav.sh <ACT> [횟수]
cd "$(dirname "$0")/.."
n=${2:-1}
for i in $(seq 1 $n); do
  sed "s/__ACT__/$1/g" Tools/probes/_pad_nav_windows.cs > "${TMPDIR:-/tmp}/pn_act.cs"
  bash Tools/u.sh cs "${TMPDIR:-/tmp}/pn_act.cs" >/dev/null
  sleep 0.45
done
sed "s/__ACT__/STATE/" Tools/probes/_pad_nav_windows.cs > "${TMPDIR:-/tmp}/pn_st.cs"
echo "$1 x$n -> $(bash Tools/u.sh cs "${TMPDIR:-/tmp}/pn_st.cs" | head -c 400)"
