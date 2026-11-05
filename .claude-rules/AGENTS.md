# Project B 규칙 인덱스

- 모든 대화·문서는 **한글**로 쓴다.
- 이 파일은 **짧은 인덱스**다(매 세션 로드). 상세 설명은 `Framework/DocIndexFull.md`(온디맨드)에 있다. 새 문서는 여기에 짧게, 상세는 DocIndexFull 에도 추가.
- 절약이 대원칙: `Framework/TokenEconomy.md`.
- 이 파일은 **인덱스 전용**이다. 항목마다 "어느 문서를 읽을지" 판단할 한 줄만 적고, 항목은 **백틱**으로 적는다(링크 금지).
- 문서는 `Framework/`(범용 규약) 또는 `Game/`(게임 고유)에 둔다. 한 문서는 500줄 이내를 권장한다.

## Project Overview

**Project B** — 메이플식 횡스크롤 2D 액션 + 던파·디아식 장비 세트 파밍 RPG (개인 프로젝트).

- **Unity:** 6000.4.0f1 / URP 2D / PC(키보드·게임패드)
- **데이터:** 로컬 CSV(`Assets/Resources`) + Easy Save 3. 서버 없음.
- **에디터 자동화:** unity-mcp(`Tools/u.sh`, `Tools/playtest.sh`, `Tools/probes/*.cs`)

## 프레임워크 규약

| 파일 | 설명 |
|------|------|
| `Framework/TokenEconomy.md` | **절약 대원칙·세션 담당자 운영** |
| `Framework/AutoRun.md` | 자동 진행 `/loop` 틱 절차(5시간 한도·주간 페이스) |
| `Framework/Dispatch.md` | 업무 생성·전달 규칙(중복 방지, 큐 원본, SendMessage) |
| `Framework/ContextEconomy.md` | 컨텍스트 절약 |
| `Framework/CodeStyle.md` | 코드 스타일 |
| `Framework/EditorAutomation.md` | unity-mcp로 컴파일 |

## 게임 기획 / 스펙

| 파일 | 설명 |
|------|------|
| `Game/TowerWorld.md` | TO THE BEYOND 세계관 |
| `Game/LineageGear.md` | 몬스터 계보 장비 설계안 |
| `Game/ClassicMapleDesign.md` | 게임 진행 구조 = 클래식 메이플 |
| `Game/GameOverview.md` | 게임 개요 |
| `Game/CombatSpec.md` | 이동·입력·스탯·데미지 공식·콤보 |
| `Game/ItemSpec.md` | ItemSheet/SetItem CSV 스키마 |
| `Game/EnemySpec.md` | 몬스터 스탯 |
| `Game/WorldSpec.md` | 낮/밤·맵·포털·카메라 |
| `Game/ArtStyle.md` | 도트 아트 스타일 가이드 |
| `Game/CharacterPixelRules.md` | 캐릭터 도트 생성 규칙 |
| `Game/AssetInventory.md` | 보유 에셋스토어 86종 분류와 도입 현황 |
| `Game/AssetIntegrationPlan.md` | 사용자 지정 필수 에셋 |
| `Game/TechDebt.md` | 버그·기술 부채 백로그 |
| `Game/QualityRoadmap.md` | 게임 방향·퀄리티 로드맵 |
| `Game/MotionSet.md` | 주인공 마스터 모션 목록 |
| `Game/JobCoverage.md` | 직업 200종 |
| `Game/SkulReference.md` | 전투 모션·이펙트 레퍼런스(스컬) |
| `Game/MotionFxChecklist.md` | 모션·이펙트 작업 전 필독 |
| `Game/MotionFxLessons.md` | 모션·이펙트 실패 교훈 |
| `Game/MotionFxReview.md` | 3타 품질 검토 절차서 |
| `Game/MotionPipeline.md` | 모션 제작 절차 |
| `Game/MotionRootCause.md` | 공격 모션이 이상하게 나오는 원인 검토 |
| `Game/DungeonSlasherReference.md` | 던전 슬래셔 레퍼런스 |
| `Game/LimbusReference.md` | 이펙트 레퍼런스(림버스) |
| `Game/MotionSpec39.md` | 마스터 모션 39 규격 |
| `Game/MotionReferenceMaple.md` | 비슷한 환경 레퍼런스 = 메이플 몸 동작 34개 |
| `Game/MotionReferenceSurvey.md` | 메이플 외 레퍼런스 조사 |
| `Game/MotionGapSurvey.md` | 빠진 동작 조사 |
| `Game/MotionCoverage.md` | 무기×직업 모션 커버리지 |
| `Game/TransformSpec.md` | 변신형·몸 부속 설계 |
| `Game/SkulMotionMap.md` | 마스터 모션 38 × 스컬 참고 애니 대응표 |
| `Game/SkulAssetLibrary.md` | 스컬 원본 추출 라이브러리 |
| `Game/GameFeelPlan.md` | 타격감 구현 계획 |
| `Game/InventoryRefactorDesign.md` | 인벤토리 리팩터 설계 |
| `Game/WeaponReview.md` | 무기 평타·스킬 감사 |
| `Game/PixelRigDesign.md` | 새 주인공 픽셀 리그 |
| `Game/PlayerStateDesign.md` | 플레이어 FSM·상태 토큰 |
| `Game/ProgressionDesign.md` | 성장 루프 |
| `Game/Lv2ContentPlan.md` | Lv2 "마법 절벽" 기획 |
| `Game/CelEquipmentPlan.md` | 셀 애니 × 장비 외형 계획 |
| `Game/Floor2Plan.md` | 2층계 Undercloud 설계 |
| `Game/BasicFeatures.md` | 장르 기본 기능 점검 |
| `Game/MiniGames.md` | 미니게임 |
| `Game/FloorCoreContent.md` | 1~4층계 본무대 확정 |
| `Game/PixelLabEconomy.md` | PixelLab 사용량 절약 |
| `Game/WeaponTypes.md` | 무기 타입 확장 |
| `Game/Floor1Completion.md` | 1층계 완성 계획: 최단 클리어 모델·할 것 설계·세트/무기 표 |
| `Game/CombatFeelPlan.md` | 전투 조작감 개선: 진단·메이플+던파 조합 선택·CF 항목 |
| `Game/EquipSlotExpansion.md` | 비외형 장신구 신규 슬롯(귀걸이·팔찌·부적·마법석): 잠금·층계별 해금·폐기 정리·ES3·SLOT-R 큐 |
| `Docs/Floor2Data/MigrationPlan.md` *(저장소 루트)* | 2층계 초안 CSV 이행 계획·재대조·롤백 |
| `Docs/Playtest/LineageAudit.md` *(저장소 루트)* | 계보 장비(몹 스킬↔무기↔세트) 정합 점검과 수정 후보 |
| `Docs/Playtest/LocPerfAudit.md` *(저장소 루트)* | Loc 누락 문자열·Update 할당/탐색 점검과 수정 후보 |
| `Game/Durability.md` | 던파식 내구도·수리 도입 검토(추천: 보류) |
| `Game/FloorTransition.md` | 층계 전환 규칙: 이동 조건·칭호 부하·포털/귀환·안전지대·구현 갭 FT-* |

## 운영

| 파일 | 설명 |
|------|------|
| `Docs/OpenQuestions.md` *(저장소 루트)* | 사용자 확인이 필요한 기획 질문과 임시 가정 |
| `Docs/MorningQuestions.md` *(저장소 루트)* | 아침에 한 번에 답할 질문 모음(번호·추천) |
| `Docs/AutomationLog.md` *(저장소 루트)* | 예약 자동 실행 기록과 사용량 예산 규칙 |
