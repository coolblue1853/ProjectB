#!/usr/bin/env bash
# AUTO-115: 구간별 이동+처치 점검. 사용: bash Tools/f1_fullrun.sh 맵 시작Lv 끝Lv  (플레이 모드 중)
cd "$(dirname "$0")/.."
T=$(mktemp -d)
for ph in goto spawn kill; do
  sed "s/__MAP__/$1/g; s/__LO__/$2/g; s/__HI__/$3/g; s/__PH__/$ph/g" Tools/probes/_f1_fullrun.cs > "$T/$ph.cs"
done
bash Tools/u.sh cs "$T/goto.cs" | head -c 300; echo
sleep 3; bash Tools/u.sh cs "$T/spawn.cs" | head -c 200; echo; sleep ${SPAWNWAIT:-10}
bash Tools/u.sh cs "$T/kill.cs" | head -c 1500; echo
