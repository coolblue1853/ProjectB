---
name: hero-polish-loop
description: "10-25 사용자 지시 — 주인공·기본 공격을 상업 기준으로 질문 없이 계속 개선(이동→점프→무기→이펙트), 끝내라 할 때까지. 남는 시간엔 이펙트 품질, PixelLab 후순위"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-25T11:58:49.675Z
---

주인공(픽셀 리그)과 기본 공격을 상업용 게임 기준(높게 잡고 웹 레퍼런스로 확인하는 발전 모델)으로 **질문 없이 계속** 개선한다.
- 순서: 이동 → 점프 → 무기(쥐는 모습: 손잡이를 주먹에, 칼날은 손 위로) → 이펙트.
- 사용자가 "끝내라"고 할 때까지 멈추지 않는다.
- 남는 시간에는 이펙트 개선(상업용이라 일정선 이상 품질)이나 다른 부분을 고친다.
- **PixelLab 사용은 후순위**: 가능하면 코드·직접 픽셀 작업으로 해결한다.

**Why:** 사용자(10-25): "상업용 게임이니까 기준을 높게 잡고 레퍼런스 검색해서 발전 모델로", "내가 말하지 않아도 개선할 거 개선", "끝내라고 하기 전까지는 계속", "가능하면 픽셀랩 쓰는 건 후순위".
**How to apply:** 기준표는 `.claude-rules/Game/PixelRigDesign.md` 3.5절. 매 단계 낮 인게임 검증(`bash Tools/rig_check.sh`) 후 커밋하고 다음 단계로. Related: [[no-stopping]], [[test-daytime]], [[pixel-character-skill]].
