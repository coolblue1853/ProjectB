#!/usr/bin/env bash
# EditMode 테스트 배치 실행(Unity 에디터가 꺼져 있을 때): bash Tools/runtests.sh [필터] → Logs/editmode_results.xml
cd "$(dirname "$0")/.."
"/c/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe" -batchmode -nographics -projectPath "$(pwd -W)" -runTests -testPlatform EditMode ${1:+-testFilter "$1"} -testResults "$(pwd -W)/Logs/editmode_results.xml" -logFile Logs/editmode.log
echo "exit $?"; grep -o 'result="[A-Za-z]*" .*total="[0-9]*" passed="[0-9]*" failed="[0-9]*"' Logs/editmode_results.xml | head -1
