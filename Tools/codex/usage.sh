#!/usr/bin/env bash
# Codex 남은 사용량과 권장 추론 강도: bash Tools/codex/usage.sh [--effort]
# 최근 세션 기록의 rate_limits(primary=5시간, secondary=주간)를 읽는다.
# 강도 규칙(리셋 시점 기준 남은 양으로 판단):
#   주간 남은% / 주간 남은 시간 비율이 넉넉하고 5시간 창도 여유 → xhigh, 리셋 6시간 안 + 30% 이상 남음 → max
#   5시간 창 70% 이상 사용 또는 주간 페이스 초과 → medium, 주간 90% 이상 → low
# 진행 중인 세션에는 아직 기록이 없을 수 있어, 최근 파일부터 rate_limits 가 있는 것을 찾는다.
line=""
for f in $(ls -t ~/.codex/sessions/*/*/*/*.jsonl 2>/dev/null | head -10); do
  line=$(grep -o '"rate_limits":{"limit_id":"codex".\{0,300\}' "$f" | tail -1)
  [ -n "$line" ] && break
done
p_used=$(echo "$line" | grep -o '"primary":{"used_percent":[0-9.]*' | grep -o '[0-9.]*$')
p_reset=$(echo "$line" | grep -o '"primary":{[^}]*"resets_at":[0-9]*' | grep -o '[0-9]*$')
w_used=$(echo "$line" | grep -o '"secondary":{"used_percent":[0-9.]*' | grep -o '[0-9.]*$')
w_reset=$(echo "$line" | grep -o '"secondary":{[^}]*"resets_at":[0-9]*' | grep -o '[0-9]*$')
now=$(date +%s)
awk -v pu="${p_used:-0}" -v pr="${p_reset:-$now}" -v wu="${w_used:-0}" -v wr="${w_reset:-$now}" -v now="$now" -v mode="$1" '
BEGIN {
  ph = (pr - now) / 3600; wh = (wr - now) / 3600; if (wh < 0.1) wh = 0.1
  # 주간 페이스: 남은 시간 비율만큼 쓸 수 있다고 보고, 남은 양이 그보다 많으면 여유
  allow = 100 - wu; pace = allow / (wh / 168 * 100)
  e = "high"
  if (wu >= 90) e = "low"
  else if (pu >= 70 || pace < 1) e = "medium"
  else if (wh <= 6 && allow >= 30) e = "max"
  else if (pace >= 2 && pu < 50) e = "xhigh"
  if (mode == "--effort") { print e; exit }
  printf "5시간: %.0f%% 사용, 리셋까지 %.1f시간\n", pu, ph
  printf "주간: %.0f%% 사용, 리셋까지 %.1f시간 (여유 배율 %.1f)\n", wu, wh, pace
  printf "권장 강도: %s\n", e
}'
