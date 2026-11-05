#!/usr/bin/env bash
# AUTO-119: 맵 이동 후 스포너 상태 덤프. 사용: bash Tools/swamp_spawn.sh 맵 [대기초=6]  (플레이 모드 중)
cd "$(dirname "$0")/.."
T=$(mktemp -d)
for ph in goto dump; do sed "s/__MAP__/$1/g; s/__PH__/$ph/g" Tools/probes/_swamp_spawn.cs > "$T/$ph.cs"; done
bash Tools/u.sh cs "$T/goto.cs" | head -c 300; echo
sleep ${2:-6}; bash Tools/u.sh cs "$T/dump.cs" | head -c 6000
