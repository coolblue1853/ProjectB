#!/usr/bin/env bash
# Stop 훅(10-30 사용자 "주간 사용량 90%까지 멈추지 마"): Logs/.allow_stop 이 없으면 멈춤을 막고 다음 작업을 이어가게 한다.
cd "$(dirname "$0")/.."
if [ -f Logs/.allow_stop ]; then
  exit 0
fi
cat <<'JSON'
{"decision":"block","reason":"사용자 지시(10-30): 주간 사용량 90%까지 멈추지 말 것. get_usage 로 확인하고 89% 미만이면 다음 작업을 이어서 하라 — ① 동작 품질 루프(MotionSpec39 §24: 기준 3타 대비 GPT 채점 → 키 수정·재생성 → 재채점) ② 끝나면 게임 기본 기능(메이플·던파·디아블로·몬헌 방향, 퀵슬롯 등). 89% 이상이면 Logs/.allow_stop 파일을 만들고 마무리 보고 후 종료."}
JSON
