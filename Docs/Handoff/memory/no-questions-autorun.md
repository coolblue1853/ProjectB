---
name: no-questions-autorun
description: "10-26 user — \"질문하지 말고 효율적인 방법 찾아서 알아서 돌려\" — don't end turns with decision questions; pick the efficient option, log assumptions, keep going"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-26T03:21:50.169Z
---

User (10-26, after I ended a turn asking "인벤토리 연결할까요?"): "질문하지말고 효율적으로 할수있는방법찾아서 알아서 돌리라고".

**Why:** User is often away and finds per-step decision prompts wasteful; they already set direction (cel motion → DNF equipment layers → mobs/weapon anims/effects).

**How to apply:** Choose the cheapest effective approach yourself (prefer zero-generation tricks like splitting existing layers before new PixelLab calls), record assumptions in Docs/OpenQuestions.md, and continue to the next item. Don't use wrap_up asks for routine next steps; only stop for the usage cap (see [[usage-budget]]). Related: [[no-stopping]], [[questions-batching]].
