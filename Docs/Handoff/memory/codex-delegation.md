---
name: codex-delegation
description: 10-31 사용자 결정 — 구현·검증은 로컬 Codex CLI(Tools/codex/run.sh)에 맡기고 Claude는 지시서·커밋만
metadata:
  type: feedback
---

구현·Unity 검증은 Codex CLI 에 맡긴다(`bash Tools/codex/run.sh <이름> <지시서.md> [강도]`, 규칙은 루트 `AGENTS.md`, 사용량 `Tools/codex/usage.sh`). Claude 는 짧은 지시서·보고 확인·커밋만. builder(sonnet) 서브에이전트 대신 이것. 기본 강도 high.

**Why:** Claude 주간 90%에서 11-01 20:00까지 최대한 많은 작업을 하려고(사용자 "codex를 통해서 최대한 줄여보자"). Codex 는 주간 2%만 사용.

**When:** Claude 주간 85%↑ 또는 사용자가 정한 하루 한도 근접 시 전환(사용자 10-31 "앞으로도 이 방법"). 리뷰·품질 확인도 Codex(`Tools/codex/review.sh`).

**Fallback:** Codex 한도(5시간 85%↑·주간 95%↑, run.sh exit 75 LIMIT) 걸리면 Claude 가 직접 작업, 풀리면 다시 Codex. 강도 기본 medium.

**How to apply:** 보고 파일만 읽고 전체 로그는 grep. Unity 하나라 Codex 구현은 순차. `--dangerously-bypass…` 는 권한 거부됨 — workspace-write+network 사용. 관련 [[budget-priority]], [[usage-budget]].
