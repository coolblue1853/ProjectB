#!/usr/bin/env bash
# MAP-SCAN 배치모드 실행(Unity 에디터가 꺼져 있을 때): bash Tools/mapscan.sh → Logs/mapscan.txt
cd "$(dirname "$0")/.."
"/c/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe" -batchmode -nographics -quit -projectPath "$(pwd -W)" -executeMethod MapScan.${1:-Run} -logFile Logs/mapscan_editor.log
echo "exit $?"; tail -3 Logs/mapscan.txt 2>/dev/null | head -3
