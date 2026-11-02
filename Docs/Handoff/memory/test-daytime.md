---
name: test-daytime
description: 10-25 사용자 지시 — 인게임 시각 확인(이펙트·캐릭터 캡처·플레이 테스트)은 낮 시간대로 전환한 뒤 한다
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-25T10:47:54.944Z
---

인게임에서 그림·이펙트를 확인하거나 스크린샷을 찍을 때는 **먼저 낮 시간대로 바꾸고** 테스트한다.

**Why:** 사용자(10-25): "이거 테스트할 때에는 낮 시간대로 하고 하렴". 밤·숲 조명에서는 캐릭터와 이펙트가 어둡게 보여, 픽셀 리그 첫 캡처가 어두워 판단하기 어려웠다.

**How to apply:** 플레이 확인 프로브 전에 낮/밤 시스템(WorldSpec 낮/밤)을 낮으로 강제하는 단계를 넣는다. 밤 연출 자체를 검증할 때만 예외. Related: [[no-focus-steal]].
