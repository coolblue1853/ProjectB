---
name: no-stopping
description: "User wants continuous autonomous work — never end a turn with \"say continue\" or a question; keep chaining tasks until they say 종료"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-19T03:18:06.043Z
---

Do not end turns with offers like "계속이라고 하시면 이어가겠습니다" or any question. Keep picking the next item from projectb-progress and doing it until the user says 종료.

**Why:** 2026-10-18 the user said "물어 보지 말고 계속 하라니까" after I closed a turn with a summary + "say continue".

2026-10-19 재지시: "나한테 질문하지 말고 알아서 하라고" — 아트 색감 후보 A/B/C를 고르라고 물은 직후. **취향 선택(시안 비교)도 직접 고른다.** 비교 시트는 남기되 고른 이유를 한 줄로 적고 적용한다. wrap_up에 ask/버튼을 넣지 않는다.

**How to apply:** After each commit, immediately start the next queued item; give short progress lines in Korean instead of a closing summary. Related: [[usage-budget]], [[projectb-progress]], [[questions-batching]]
