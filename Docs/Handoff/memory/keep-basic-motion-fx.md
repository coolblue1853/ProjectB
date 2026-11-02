---
name: keep-basic-motion-fx
description: 10-28 사용자 — 지금 기본 동작·칼 3타 동작·스미어 이펙트 v7은 무슨 일이 있어도 보존(태그 keep/basic-motion-fx-v7)
metadata:
  type: feedback
---

현재 기본 동작(대기·이동·점프 셀), 기본 칼 공격 3동작(찌르기 v6·내려베기 v13·올려베기 v18), 스미어 이펙트 v7(림버스식 타원 고리·바늘 찌르기 + SmearAura·SmearCut 받침)은 **무슨 일이 있어도 남겨 둔다**. 기준점: git 태그 `keep/basic-motion-fx-v7`(c6003a5d 이후 HEAD, 원격에도 푸시), 로컬 사본 `C:\Users\user\ProjectBKeep\basic-motion-fx-v7\`(Cel·PixelFx·gen_smear·skel 키).

**Why:** 10-28 "지금 있는 기본동작이랑 공격 3동작이랑 3 이펙트는 뭐가 어찌되어도 남겨두렴 이펙트는 정말로 마음에 들거든".
**How to apply:** 림버스 분석으로 동작·이펙트를 "강화"할 때도 이 파일들을 덮어쓰지 말고 새 이름(v8 등)·새 클립으로 만들어 비교하고, 사용자가 고르기 전에는 교체하지 않는다. 덮어써야 하면 먼저 묻는다. [[limbus-fx-reference]] [[stick-first-pipeline]]
