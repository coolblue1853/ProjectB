---
name: usage-budget
description: Claude/GPT usage limits the user set for automated Project B work
metadata:
  node_type: memory
  type: project
  originSessionId: 7f303872-6cc7-4749-abaf-5659414b9850
  modified: 2026-10-18T00:21:01.557Z
---

- Claude (this project): unlimited until 2026-10-18 21:00 (weekly reset). After that, on weekdays, Project B may use at most 10% of the weekly Claude usage.
- 2026-10-18 20:45 override: user said "멈추지 말고 계속해 9시 구간넘어가도 일단 멈추지말고" — keep working past the 21:00 reset in this session (Sunday night included) until the user says 종료 or tells otherwise. Still check get_usage periodically and log in AutomationLog.
- 2026-11-01: user allowed Claude weekly up to **100%** (95% stop lifted).
- 2026-10-27: user raised the cap 10→15→20→**50%** weekly all-models (stop ~44%).
- 2026-10-25 (Sun night, user going to sleep, "멈추지말고 계속"): hard cap — never let "Weekly · all models" exceed 10% (was 2% at the start). Check get_usage between work items; at ~9% wrap up, log, and stop.
- GPT Plus image generation: user lifted the cap on 2026-10-18 (was 70%).
- Unity Asset Store purchased assets (https://assetstore.unity.com/account/assets) may be imported; user expects few pixel-art ones fit.

**Why:** User is automating long-running improvement of Project B and has other projects sharing the quota.
- 2026-11-02: Codex 사용 금지([[no-codex]]), 상한 15%.
**How to apply:** User wants continuous in-session work (fix → next fix, no stopping), NOT a scheduled routine (task projectb-auto-improve is disabled). After the 2026-10-18 21:00 reset, check get_usage between work items and stop for the day at ~2%/weekday, 10%/week; log in Docs/AutomationLog.md. Related: [[questions-batching]]

10-28: 사용자 "상한 올려줌" → 50%로(+5%, 버튼 문구 기준). 49% 근처에서 멈춤.

10-28 저녁: 사용자 "상한 올려줌"(두 번째) → 55%. 54% 근처에서 멈춤.

10-28 밤: 사용자 "멈추지 말고 작업해 65퍼까지는 허용" → **65%**. 64% 근처에서 멈춤.

- 10-29 오후 사용자: 한도 70%로 상향(69%쯤 멈춤).
- 10-29 밤 사용자: 한도 80%로 상향(79%쯤 멈춤).
- 10-30 사용자: **한도 90%**, "턴이 끝나든 뭐든 멈추지 마" — Stop 훅(.claude/settings.local.json, Logs/.allow_stop 있으면 허용)과 10분 간격 재개 예약으로 자동 계속. 89%쯤 Logs/.allow_stop 만들고 마무리.
- 10-30 밤 사용자: 잠깐 멈춤 요청 + **한도 100%**로 상향(그래도 GPT 로 되는 건 GPT). 재개하면 Logs/.allow_stop 삭제 후 Stop 훅 다시 작동.

- **11-01 밤(주간 리셋 직후) 사용자: Claude 주간 상한 10%**(all models, 9%쯤 정리·중단). 작업은 **Codex 우선**(`Tools/codex/run.sh`), Codex 가 바닥나면(LIMIT) 그때 Claude 로 직접. Claude 는 지시서·확인·커밋 위주.
- 11-02 낮 사용자: **Claude 주간 상한 15%**로 상향(14%쯤 정리·중단).
