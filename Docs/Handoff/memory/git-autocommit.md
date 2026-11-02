---
name: git-autocommit
description: User authorized auto commit+push per completed feature in Project B
metadata:
  node_type: memory
  type: feedback
  originSessionId: 7f303872-6cc7-4749-abaf-5659414b9850
  modified: 2026-10-18T00:21:10.569Z
---

When a feature-sized unit of work is done in Project B, commit it without asking. Shell push works (GCM login done 2026-10-18), so push right after each commit.

**Why:** User wants autonomous progress; reviews history in Fork.
**How to apply:** One commit per feature, Korean message style like existing ones ("(수정) ...", "(추가) ..."), push to origin. Related: [[questions-batching]]
