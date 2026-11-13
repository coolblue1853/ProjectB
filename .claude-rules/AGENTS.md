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
| `Game/MotionTheory.md` | **모션 백서**: 합격/실패 조건·모션 이론·PixelLab 운용 규칙·판정 기준·100생성 연구 계획 |
| `Game/MobBgPlan.md` | 몬스터·배경·NPC·방어구 2주 계획: 현황·합격 기준·PixelLab 예산·평일 위임 규칙 |
| `Game/MobRules.md` | 몬스터 규칙: 던전 슬래셔 통계(프레임·속도·크기)·크기 등급·생성 절차·한 종 비용 |
| `Game/BgTilePlan.md` | 맵·배경·타일맵 계획: 현황·합격 기준·타일/원경/소품 방법·순서·평일 위임 |
| `Game/MotionRootCause.md` | 공격 모션이 이상하게 나오는 원인 검토 |
| `Game/DungeonSlasherReference.md` | 던전 슬래셔 레퍼런스 |
| `Game/BackgroundRefPlan.md` | 던전 슬래셔 기반 배경 후보·1~4층계 대응·재제작 방법(적용은 검토 후) |
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
| `Game/CombatInputRedesign.md` | 전투 입력 재설계(REWORK-4): 무기 타입별 공격 중 이동 모드·패드 버퍼·콤보/캔슬 규칙·P0~P8·프로브 |
| `Game/EquipSlotExpansion.md` | 비외형 장신구 신규 슬롯(귀걸이·팔찌·부적·마법석): 잠금·층계별 해금·폐기 정리·ES3·SLOT-R 큐 |
| `Docs/Floor2Data/MigrationPlan.md` *(저장소 루트)* | 2층계 초안 CSV 이행 계획·재대조·롤백 |
| `Docs/Playtest/LineageAudit.md` *(저장소 루트)* | 계보 장비(몹 스킬↔무기↔세트) 정합 점검과 수정 후보 |
| `Docs/Playtest/LocPerfAudit.md` *(저장소 루트)* | Loc 누락 문자열·Update 할당/탐색 점검과 수정 후보 |
| `Docs/Playtest/ProbeInventory.md` *(저장소 루트)* | 미추적 `_`접두 프로브 170개 정리 목록(유지/통합/삭제 후보) |
| `Docs/Playtest/SaveMigrationAudit.md` *(저장소 루트)* | ES3 키 전수·구세이브 로드 시나리오·예외 처리 위치·필요 테스트 |
| `Game/Durability.md` | 던파식 내구도·수리 도입 검토(추천: 보류) |
| `Game/WeaponSkillProgression.md` | 무기 스킬 순차 해금·성장: 레벨 연동 vs 무기 숙련도 비교, 추천 B·곡선·이행안 |
| `Game/EquipEffectDesign.md` | 장비·세트 효과 설계(REWORK-12): 스킬 수정자·규칙 대체 프레임워크, 등급별 효과 표, 1층계 효과 초안, P0~P8 |
| `Game/QuestSystemDesign.md` | 퀘스트 통합 설계(REWORK-5): 3계통 비교·공통 데이터 모델·Q 창·추적 HUD·입구 통일·U7 정책·P0~P5 |
| `Game/QuestDesign.md` | 퀘스트 콘텐츠(REWORK-6): 메이플 퀘스트 구성 조사·유형 3종 정의·대사 단계 형식·층계별 메인 줄기·사이드 사슬·재분류표 |
| `Game/Cooking.md` | 요리·일괄 제작(REWORK-11): 현재 구조 진단·마을 요리대·수량 선택 일괄·한 판 등급 전체 적용·대장장이 일괄 제작 통일 |
| `Game/EquipSplitPlaybook.md` | 장비 부위 분리 노하우: 속옷 기반 몸+5부위 쌓기·영역 지도·배낭/랜턴=상의·절차·함정 |
| `Game/FloorTransition.md` | 층계 전환 규칙: 이동 조건·칭호 부하·포털/귀환·안전지대·구현 갭 FT-* |
| `Game/FastTravel.md` | 빠른 이동(REWORK-14): 수레(마차) 형제 NPC 정류장·퀘스트 해금·골드 이동 규칙·ES3 `ft.stops`·3경로 입력·히든 포털·P0~P5(설계만) |
| `Game/EarlyHourPlan.md` | 초반 30분·1시간 시간표(REWORK-9): 필수 경험·변화 간격·막힘/단조 구간·퀘스트 CSV 조정 R-1~14·검증 시나리오 |
| `Game/LevelDesignRef.md` | 레벨 디자인 참고(REWORK-7·10): 메랜 DB 레벨 구간별 몹 HP·EXP·맵당 개체 수와 우리 1~2층계 대조·튀는 구간 L1~L7·수정안 |

## 운영

| 파일 | 설명 |
|------|------|
| `Docs/Playtest/UpslashDirGen.md` *(저장소 루트)* | 올려베기 방향 전환(②③④) 생성 기록·합격 세트·재생성 절약 교훈 |
| `Docs/Playtest/MotionJudgeLog.md` *(저장소 루트)* | 모션 판정 기록: Claude 판정 vs 사용자 판정 비교·연구 실험 표 |
| `Docs/Playtest/MobJudgeLog.md` *(저장소 루트)* | 몬스터 판정 기록(Claude 판정 먼저 → 사용자 판정, 실패 원인) |
| `Docs/Playtest/ConventionAudit.md` *(저장소 루트)* | 장르 관례 이탈 점검·사용자 제보(창고 I 키 버그 등) 목록 |
| `Docs/Playtest/InputAudit.md` *(저장소 루트)* | 키보드·패드 조작 점검표(충돌·미지정) |
| `Docs/Playtest/DedupAudit.md` *(저장소 루트)* | 중복 코드 정리 후보 8건 |
| `Docs/OpenQuestions.md` *(저장소 루트)* | 사용자 확인이 필요한 기획 질문과 임시 가정 |
| `Docs/MorningQuestions.md` *(저장소 루트)* | 아침에 한 번에 답할 질문 모음(번호·추천) |
| `Docs/AutomationLog.md` *(저장소 루트)* | 예약 자동 실행 기록과 사용량 예산 규칙 |
| `Docs/Floor2Data/Preflight.md` *(저장소 루트)* | 2층계 병합 사전 점검(#8 통과) |
| `Docs/Playtest/SaveMigrationAudit.md` *(저장소 루트)* | 세이브 호환 점검과 권고 |
| `Docs/Playtest/ReviewCode_1105c.md` *(저장소 루트)* | 코드 커밋 정적 리뷰(1105·b·c 같은 형식) |
| `Docs/Playtest/Floor1PlayVerify.md` *(저장소 루트)* | 1층계 플레이 검증 표(미검증 통합) |
| `Docs/Playtest/Floor1Readiness.md` *(저장소 루트)* | 1층계 주말 확인 전 준비도 점검(사용자 결정·코드 작업 목록) |
| `Docs/Playtest/Weekend1Guide.md` *(저장소 루트)* | 1층계 주말 플레이 테스트 가이드(진행 순서·알려진 임시 목록) |
| `Docs/Playtest/UserPlaytest_1111.md` *(저장소 루트)* | 11-11 사용자 플레이 확인 목록(미검증 항목) |
| `Docs/Handoff/outfit_one_piece_decision.md` *(저장소 루트)* | 외형 슬롯=의상 하나·층계별 해금 슬롯·세트 단계 이펙트 결정과 총괄 요청(11-10) |
| `Docs/Handoff/queue.md` *(저장소 루트)* | 작업 큐 원본(현재 상태 `Docs/Handoff/state.md`) |
| 그 밖의 `Docs/Playtest/*`·`Docs/Balance/*`·`Docs/Handoff/*` 문서 | `Framework/DocIndexFull.md` 의 점검·감사·인수인계 절 |
