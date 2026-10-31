# goldart1 독립 리뷰 (2026-10-31)

검토 범위는 goldart1의 미커밋 변경 8개 파일이다. ClassicMapleDesign의 §16, OpenQuestions의 Q-449, NpcQuestTests의 요리 목표 입력 1줄만 해당 작업으로 분리했다. 같은 파일에 섞인 이동 규칙·1층계 작업과 씬·폰트·FX·ES3Defaults·.claude·아트 변경은 수정하지 않았다. 커밋하지 않았다.

## 판정과 발견

**확정된 [필수] 버그·규약 위반·문서 불일치 없음. 제품 코드·수치 수정 없음.**

| 심각도 | 파일:줄 | 발견 | 수정 여부 |
|---|---|---|---|
| [권장] 품질 | Assets/Resources/MonsterSheet.csv:41~42 | 동레벨 +0 RampartSword, 레벨 공격 보너스만, 세트·스킬·버프 제외 평타 모델에서 성벽 기사 15.10~16.63초, 미늘창 파수병 21.28~23.42초. 범위는 칭호3/2 차이이며 실제 입력 측정이 아니다. 목표 2~4초를 입증하지 못한다. | 없음. HP·방어는 기존 값이고 §16.3이 미검증 조건으로 명시했다. 후속으로 적정 장비·강화·스킬 구성을 먼저 정해야 한다. |
| [권장] 품질 | Assets/Resources/MonsterSheet.csv:5 | DeathBringer는 기본 HP2500/EXP283이며 +0 CliffBlade 모델 41.06초. 동일 EXP인 응시의 눈은 HP120/1.45초다. 위험 분기의 보상이 적어 회피가 합리적일 수 있다. 정예 승격·재료·고유 드롭의 가치는 별도다. | 없음. 기존 HP를 이번 리뷰에서 임의 변경하지 않는다. |
| [참고] 계획 한계 | .claude-rules/Game/ClassicMapleDesign.md:401 | 247.43분은 분당6처치와 보상 평탄화 모델이다. 미등록 21종·미배치 NPC·활공 중급/상급을 포함하므로 현재 실행 가능한 완주 시간으로 읽으면 안 된다. 원문도 이 한계를 명시한다. | 없음. |
| [참고] 경제 | .claude-rules/Game/ClassicMapleDesign.md:416 | 물약15~30개/h의 골드 비중 4.8~14.7%는 목표10~20%를 일부 밑돈다. 공식 검산은 맞지만 실제 회피·방어·드롭/줍기·소비를 검증한 값이 아니다. | 없음. |

CSV 변경은 기존11행의 level/exp/contactDamage만이며 컬럼·id·세이브 키 변경이 없다. 테스트 diff에는 Update 할당·이벤트 구독·DOTween·풀 반환·직렬화 필드 변경이 없다. 새 테스트는 UTF-8 BOM/CRLF이다. NpcQuestTests의 변경 1줄은 현재 Q203 목표를 사용하며 관련 요리 레시피 검사는 같은 파일의 다른 작업이 담당한다.

## 검증

- 전체 EditMode 최초398/398 통과(3.4317952초), 최종 재컴파일 후401/401 통과(3.3520551초), 모두 실패0·스킵0. 두 실행 사이 다른 작업의 테스트3개가 추가됐다. 최초 요청 및 중간 재실행1회는 플레이 상태 때문에 시작 거부되어 정지 후 재실행했다. 최종 정지 확인 playing=False/changing=False, 오류 기록 보존·콘솔 초기화 후 정지 상태 오류0이다. 이를 앞선 플레이 전체 오류0으로 해석하지 않는다.
- 플레이 모드에서 11종에 ApplySpawnProfile을 적용하고 Level/ContactDamage 및 LevelSystem.OnEnemyKilled의 EXP를 검사: 11/11 일치. 비활성 임시 EnemyHealth와 EXP 처리 함수를 사용한 연결 검사이며 실제 사냥/사망/풀 반환 검사가 아니다. 결과 Logs/goldart1_review/runtime.csv.
- 퀘스트 초안20개·메인6/서브14·EXP181684·골드5400, Lv25 이전 보상132000, Lv15→26 필요EXP726734를 테스트/CSV 집계로 재확인했다. 초안은 Resources에 등록되지 않았다.
- +0 CliffBlade 모델: 절벽 고블린1.94초·버섯2.07초·해골4.00초·눈1.45초·DeathBringer41.06초·마법사146.56초. +0 RampartSword 모델: 가고일10.24~11.28초·구름정령5.44~5.99초·군주472.71~520.15초. 무기 프리팹의 피해·판정배율·타격 수·콤보 시간과 실제 피해 공식의 난수 평균을 사용한다. 접근·피격·빗나감·광역·스킬·장비 보너스는 제외했다.
- 낮 전환 day=True 후 절벽 입구 캡처 확인: Logs/playtest/goldart1_review_day.png. 작은 주인공과 낮은 채도·안개 배경은 소박하고 몽환적인 방향에 부합한다. 적이 없는 입구 한 장이므로 몹/장비의 전체 시각 품질은 판정하지 않는다. 이번 작업에 새 아트는 없다.
- 플레이 환경이 중간에 정지/재진입하며 FirstHourTraversal 무적 잠금 오류가 다시 나타났다. 테스트 이후 첫 플레이의 싱글톤 초기화 실패도 있었고 SampleScene 재로드로 낮 캡처는 가능했다. 이후 콘솔에서 범위 밖 AttackManager.cs:56, ArtifactSystem.cs:139, ShopManager.cs:320, QuickSlotBar.cs:103 예외가 발생했다(수집1000개: 1/333/333/333, 전체 개수 아님). 원본 스택은 Logs/goldart1_review/errors_detailed.json에 보존했다. goldart1 데이터가 원인이라고 확정할 근거는 없으며 범위 밖 코드는 수정하지 않았다. 플레이 전체 오류0을 달성했다고 보고하지 않는다.
- 세이브2개는 작업 전 Logs/goldart1_review/save_backup에 백업했다. 종료 복원 MD5: SaveFile F1CBD3EDF7E845CACE5442F6641C5B2F, SaveSlot3 F254C96976D14750971BAC3033234927.

## 남은 미검증

- 적정 강화·세트·스킬을 포함한 실제2~4초 처치, 안정된 단독 에디터 세션의 실제 몸박·회피·물약 소비·골드 순수입.
- 실제4~5시간 완주, 사망·밤 대기·활공 재시도·퀘스트 보상 지급 시점의 영향.
- 미구현21종·NPC5명·퀘스트/완주 이벤트·4기사 전투 선행 연결.
- 장시간 성능·실제 하드웨어 입력. 이번 계산 모델을 실측으로 대체하지 않는다.

## 커밋할 파일

goldart1 수정5개:
- Assets/Resources/MonsterSheet.csv
- .claude-rules/Game/ClassicMapleDesign.md (§16만)
- Docs/OpenQuestions.md (Q-449만)
- Assets/Editor/Tests/HuntingRulesTests.cs
- Assets/Editor/Tests/NpcQuestTests.cs (요리 입력1줄만)

goldart1 신규3개:
- Docs/Data/floor2_quests_draft.csv
- Assets/Editor/Tests/Floor2BalanceTests.cs
- Assets/Editor/Tests/Floor2BalanceTests.cs.meta

리뷰 신규1개:
- Docs/Balance/Floor2BalanceReview.md

커밋 제외: Tools/probes/floor2_balance_v1.cs, Tools/probes/goldart1_review*.cs, Logs/** 및 다른 작업의 모든 변경. 공유 파일은 해당 hunk만 포함한다.
