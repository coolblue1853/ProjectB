---
name: monster-art-pipeline
description: "How to source new monster sprites when owned assets lack them — Pinterest reference search, then GPT image generation, always pixel art"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 7f303872-6cc7-4749-abaf-5659414b9850
  modified: 2026-10-19T01:30:33.476Z
---

When a needed monster has no owned asset, search Pinterest (kr.pinterest.com) for similar creatures, use the found image as a reference, and generate the sprite with GPT image generation (ChatGPT in the browser). The result **must be pixel art** (dot style), then clean it up to project spec (Tools/pixelfx, 24 PPU, 4–8 frame sheets) and build the prefab.

**Why:** User said on 2026-10-18 (example pin: https://kr.pinterest.com/pin/92675704824160944/): "몹 같은거 없어서 추가하고싶을때에는 핀터레스트에 비슷한거 검색해서 가져와서 레퍼런스로 이미지 생성해서 gpt로 가져와줘 단 도트여야한다는건 잊지말고".

**How to apply:** For floor-1 zones ②–⑥ monsters (Q-405) and any new enemy without an owned sprite: Pinterest reference → GPT pixel-art generation → pixel cleanup → MonsterBuilder. Only use references for inspiration; don't copy them. Do it in the built-in browser; never enter credentials. Related: [[projectb-progress]], [[usage-budget]]

2026-10-19: ChatGPT is not signed in on either browser, so I draw sprites in code (Tools/pixelfx/gen_forest_mobs.py, gen_elder.py). User rejected SPUM ("너무 안 어울린다") — never use SPUM; NPCs follow the existing town NPC style (24×32 logical px, 8× scale, PPU 100).

2026-10-19 확장: 몹뿐 아니라 **캐릭터 외형·배경·타일셋**도 핀터레스트에서 예쁜 도트를 찾아 레퍼런스로 삼는다(사용자 지시). ChatGPT 로그인 완료(앱 내장 브라우저). PixelLab MCP 연결은 사용자 결정 대기 — 연결되면 그쪽 우선(애니메이션 시트 생성).

2026-10-19: PixelLab MCP 연결됨 — trial 플랜, 총 40생성(늑대·단검수 비교에 2 사용 후 19 남음), 동시 작업 1개(병렬 호출 시 rate limit). pixflux 64×64 side 결과: 깨끗한 격자·적은 색(늑대 6색)이지만 사람형은 3/4 정면으로 틀어짐. 비교 시트 `Docs/ArtCompare/`.
2026-10-19 오전: 숲 늑대를 PixelLab 이끼 늑대로 교체(d36f1212), 남은 생성 약 11. 사용자는 "더 판타지스러운"(등에 풀·이끼) 방향을 원함. 절차는 ArtStyle.md 2절 PixelLab 경로. 요정·모기는 다음 재생성 후보.
2026-10-19 12:55: PixelLab trial 40생성 모두 소진(밤마다 +5 보너스). 사용자 지시로 플레이어를 PixelLab 캐릭터(측면, 금발 포니테일 탐험가·청록 브로치) + 프레임 스프라이트로 교체 예정 — 충전 후 create_character(view side, 4방향, size 48) → animate_character 템플릿 east만(1생성씩). 요정 정령 시안(1c892f20)은 사용자가 "기존 것 강화"를 원해 미적용.
