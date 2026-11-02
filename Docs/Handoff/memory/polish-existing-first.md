---
name: polish-existing-first
description: "2026-10-19 user directive — improve existing art (character, town, backgrounds, mobs, weapon/item icons) before making new content; Claude picks the order"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-19T01:44:59.779Z
---

새 콘텐츠보다 **이미 있는 것**(주인공·마을·배경·몹·무기/아이템 아이콘)의 퀄리티를 먼저 끌어올린다. 순서와 방법은 Claude가 알아서 정한다.

**Why:** 2026-10-19 사용자: "너가 적당히 알아서해줘. 새로운것들 보다 기존에 있던거 … 우선해서 발전시켜주렴". 숲 늑대 이끼 리디자인(d36f1212)을 본 직후의 지시.

**How to apply:** 2층계 같은 신규 맵·몹 작업보다 폴리싱을 우선한다. PixelLab 생성 수가 적으니(trial, 하루 +5) 코드 폴리싱(`Tools/pixelfx/polish_mob_sheets.py` 류)을 먼저 하고, 생성은 가장 눈에 띄는 대상에 쓴다. 관련: [[monster-art-pipeline]], [[no-stopping]], [[projectb-progress]]
