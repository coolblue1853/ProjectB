---
name: codex-claude-alternate
description: 11-02 사용자: Codex 큐가 LIMIT 로 멈추면 기다리지 말고 즉시 Claude(builder 에이전트)로 이어 가고, 풀리면 다시 Codex — 스스로 감시해 번갈아
metadata:
  type: feedback
---

Codex 큐 감시는 "DONE 줄"만 보면 안 된다. LIMIT 대기(queue_wait 가 5분마다 잠듦)도 감지해서, 걸리면 바로 큐를 끄고 남은 지시서를 Claude builder 에이전트로 돌린다. Codex 한도가 풀리면(usage.sh --limit 가 LIMIT 아님) 다시 Codex 로.

**Why:** 11-02 Codex 93% LIMIT 로 03:05~08:36 5시간 반을 아무 일 없이 기다렸다. 사용자 "알아서 체크해서 번갈아가면서 일하라니까".
**How to apply:** 감시 루프에 `bash Tools/codex/usage.sh --limit` 확인을 넣는다. Claude 주간 상한 10%는 그대로([[usage-budget]]). [[codex-delegation]]
