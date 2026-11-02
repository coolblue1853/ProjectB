---
name: ds03-hero-recipe
description: 11-01 사용자 합격 — 던전 슬래셔 그림체 주인공 ds03 + 걷기 walk_6101 생성 레시피, 걷기가 새 주인공 모션 합격 기준
metadata:
  type: project
---
ds03(Nana style_image, color_palette false, 남청록 코트·배낭 탐험가) + pixminimax 8장 첫·끝 고정 걷기가 "한 번에 잘 나옴"(사용자 11-01). 걷기 = 합격 기준. 상세 레시피는 `.claude-rules/Game/DungeonSlasherReference.md` "확정 레시피".
**Why:** 사용자가 드문 성공이라 방법을 남기라고 함. 1시간 테스트 분량 주인공 모션을 이 방식으로 바꾸라고 지시(사용량 다 써도 됨, 허락 없이 자동).
**How to apply:** 새 주인공 모션은 ds03 URL을 첫 프레임으로 pixminimax, 보간 없음, 0.1초/장. [[dungeon-slasher-reference]] [[pixellab-budget]]
