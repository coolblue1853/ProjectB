---
name: no-codex
description: 11-02 사용자: Codex 사용 금지 — 구현·검증·리뷰는 Claude(builder 에이전트 등)로만
metadata:
  type: feedback
---

Codex CLI(Tools/codex/run.sh·queue_wait.sh·review.sh)를 쓰지 않는다. 구현·검증·리뷰는 Claude builder(sonnet)·reviewer 에이전트나 직접 작업으로 한다. [[codex-delegation]]·[[codex-claude-alternate]] 는 폐기.

**Why:** 11-02 사용자 "이제 코덱스도 사용하면 안 돼"(계정 전환 시점).
**How to apply:** 지시서(Logs/codex/*.md)는 builder 에이전트에 "그대로 수행"으로 맡긴다. 사용자 소유 Codex 데스크톱 앱 프로세스는 끄지 않는다. Claude 사용량 상한은 [[usage-budget]].
