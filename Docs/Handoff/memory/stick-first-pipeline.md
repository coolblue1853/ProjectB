---
name: stick-first-pipeline
description: 10-27 사용자 확정 — 모션은 레퍼런스를 졸라맨으로 따라 그려 비교·승인 후 고정 길이 스켈레톤 보간으로 PixelLab 1회
metadata:
  type: feedback
---

모션(공격·동작)을 만들 때 레퍼런스를 가져오면 바로 PixelLab을 돌리지 않는다. 순서: 몸 전체가 보이는 레퍼런스 → 관절 읽기 → **레퍼런스 | 지금 | 수정안 3칸 졸라맨 GIF**(stick_gif.py)로 사용자 승인 → skel_keys 보간(고정 관절 길이) → animate_with_skeleton_v3 1회 → 칼은 코드로 손목에 붙임. 절차 문서: `.claude-rules/Game/MotionPipeline.md`.

**Why:** 사용자: "지금 보간 방법이 가장 좋게 잘 나온다", "래퍼런스 가져왔을 때 무턱대고 픽셀랩 돌리는 게 아니라 이렇게 해서 비용 절감". v5~v8 에서 생성만 반복해 PixelLab 예산을 썼다.
**How to apply:** 레퍼런스 분석은 칼 든 팔만이 아니라 몸 전체(머리·허리·반대손·두 다리)를 프레임마다 읽는다(10-27 사용자 "몸 전체를 분석해야 잘 나온다, 잊지 말고"). 레퍼런스는 횡스크롤 게임(던파류) 우선. 자세 수정 요청이 오면 먼저 졸라맨 GIF를 만들어 보여 준다. 레퍼런스는 프레임이 충분히 많은(동작 흐름이 보이는) 것을 고른다. [[pixellab-budget]] [[body-consistency-rule]] [[weapon-hand-rule]]

이펙트(스미어)도 같은 원칙: 레퍼런스 → 졸라맨 위 면 계획 승인 → PixelLab vfx_workbench(무료) 부품 → gen_smear 로 칼끝 경로 원에 맞춤 → 느린 미리보기 → 인게임 캡처. 절차: MotionPipeline.md §7.
