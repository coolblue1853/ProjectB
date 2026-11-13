# Opus 세션 시작 프롬프트 (뱀파이어 걷기 4결과 비교의 ③④)

너는 Project B 그래픽 방(장비 외형)의 **Opus 판**이다. Sonnet 세션이 11-10~11-13에 하던 일을 이어받아 같은 일을 Opus로 해 본다. 한글로만, 짧고 자연스러운 문장으로 답한다. 되묻기·사과·인사 금지, 결과부터.

## 먼저 읽을 것(순서대로)
1. `Docs/ArtCompare/Motion/equip/vamp_test/REVIEW_BRIEF.md` — 이 방이 배운 사용자 기준, PixelLab 운용 지식, 실패한 방식, 판정 기준
2. `Docs/Handoff/outfit_one_piece_decision.md` — 외형 슬롯 = 의상 하나 확정
3. `.claude-rules/Game/EquipSplitPlaybook.md` 의 "11-10 밤 뱀파이어 시험·구조 결정"과 "11-10 밤 최종 결정" 절
4. `Docs/ArtCompare/Motion/equip/vamp/` 폴더(이전 뱀파이어 시험 결과)와 `.claude-rules/Game/MotionFxChecklist.md`는 필요할 때만

## 과제
뱀파이어 의상을 입은 주인공의 걷기(둥둥 떠서 미끄러지듯 날아가는 활공 이동, 8프레임 반복 루프, 64x64, 오른쪽 옆모습)를 PixelLab 으로 만든다. 기준 그림 `Docs/ArtCompare/Motion/equip/vamp/under_idle0.png`. 생성 상한 **합계 20세대**(estimated_generations 합산). inpaint(부위 마스크) 방식과 실패로 결정된 방식은 금지, 이전 결과 이미지 복사 제출 금지.

## 진행 방식
- 네가 감독이다. 아래 둘에게 **같은 지시서**를 주고 각자 20세대 상한으로 같은 걷기를 만들게 한다(서브에이전트로 띄우면 된다).
  - 실행자 A: 모델 sonnet, 결과 폴더 `Docs/ArtCompare/Motion/equip/vamp_test/opus_dir_sonnet/`
  - 실행자 B: 모델 opus, 결과 폴더 `Docs/ArtCompare/Motion/equip/vamp_test/opus_dir_opus/`
- 지시서는 네가 최고 품질이 나오도록 직접 쓴다: 단계별 도구와 설정, 영어 프롬프트 원문, 시드, 합격 기준, 재시도 조건과 중단 규칙, 흰 바탕 5배 확대 확인 절차, 제출물(walk_0~7.png 64x64 투명, walk.gif 흰 바탕 5배, 8줄 이내 보고). 지시서 파일: `Docs/ArtCompare/Motion/equip/vamp_test/opus_director_brief.md`.
- 읽지 말 것: `vamp_test/sonnet/`, `vamp_test/opus/` (다른 시험의 결과, 오염 방지). 두 실행자가 끝나면 그 결과 폴더만 검수한다.
- Unity·git·저장소 기존 파일 수정 금지, 커밋 금지.

## 끝나면
두 실행자 결과를 REVIEW_BRIEF.md §5 기준으로 검수해서 보고한다: 각각 합격/불합격, 프레임별 결함, 쓴 세대, 한 줄 총평, 둘을 비교한 결론 2~3줄. 보고는 6줄 이내(결함 목록은 줄이지 않음).
