# hunt1 독립 리뷰 (2026-10-31)

리뷰 범위는 미커밋 hunt1 파일 23개와 이 리뷰 보고서다. 다른 작업의 씬·폰트·FX·ES3Defaults·아트·Tools/keep_going.sh·.claude 변경은 검사/수정 대상에서 제외했다. 커밋·push·stash·reset·브랜치 전환 없음.

## 발견

| 심각도 | 위치 | 발견 및 조치 |
|---|---|---|
| [필수] P2 | Assets/Script/Enemy/EnemyHealth.cs:610 (수정 전 607), BestiaryData.cs:83 | Kill 메시지를 지역 수치 키로 바꾸었으나 기존 도감 항목과 bestiary.kills 저장 키는 프리팹 이름이다. 4개 지역 변종의 처치가 기존 도감에 반영되지 않는다. KillMessageId로 기존 프리팹 키를 유지하도록 수정. EXP·골드·몸박은 지역 MonsterId 유지. 변종 4개 회귀 테스트 추가. |
| [필수] P2 | Assets/Editor/Dev/HuntingDensityAudit.cs:14, Docs/Balance/Floor1HuntingAudit.md:7 | 발판 중심만 조회한 최대값 31~44를 전체 동시 활성 정원 상한으로 서술했다. 트리거 경계 전수 점 겹침 검사는 샛길·일반 숲·산적 성채 낮/밤 모두 58이다. 예를 들어 일반 숲 (-695, 0)에서 Flowermole1 28 + Mole2 14 + Mole3 16 = 58이다. 계산 CSV 열을 anchorPeakActiveSlots로 명시하고 두 설계 문서·감사 문서의 범위를 정정. 플레이어 콜라이더 크기와 이탈 후 페이드 잔존은 별도다. |

새 diff의 초기화/풀 재사용/정예 해제 경로, Update 할당, 이벤트 중복 구독 해제, 종료 시 풀 반환, 필드·ES3 키 호환, CSV 열/프로필 키를 대조했다. 추가로 확정된 null·풀 이중 반환·DOTween·세이브 필드 변경 버그는 발견하지 않았다. 기존 코드 전체에 대한 무결성 보증은 아니다.

## 품질 의견 (수치·아트 추가 변경 없음)

- 낮 일반 숲에서 좁은 발판에 늑대가 뭉쳐 주인공 실루엣을 가린다. 밀도가 늘면 찾아다니는 공백은 줄지만, 이동·피격·대상 판독이 어려워지는 구간이 생긴다. 초반의 작고 소박한 적이라는 인상도 약해진다. 화면: Logs/playtest/hunt_review_daytime.png.
- 대표 +0 평타의 5.6~24.5초 TTK와 문지기 모델 18.87분은 설계 목표 일반몹 1~3초 및 답답하지 않은 공격 감각과 거리가 있다. 장비·스킬 미포함 모델이므로 HP를 임의로 바꾸지 않았다.
- HP 자동 보충·무회피 결과를 실제 물약 소비로 쓰면 안 된다. 경제 목표 15~30개/시간 및 골드의 10~20%는 아직 정상 회피·방어구·실제 소비 경로로 검증되지 않았다.
- 이번 변경은 아트 제작이 없어 유아틱한 신규 표현은 확인되지 않는다. 기존 숲 배경의 낮은 채도·안개는 방향과 맞는다.

## 검증

- EditMode: 수정 전 366/366, 도감 키 회귀 테스트 4건 추가 후 370/370. 최종 재컴파일 뒤에도 370/370, 실패/스킵 0.
- 낮밤 14/14 조합·스포너 92개·92처치, 도감 카운트 증가 92/92. 프로필/HP/슬롯 반환·재충전·EXP 실패 0. EXP 기대/실제 6,191/6,191, 직접 골드 발생 합 1,915G. 해당 검사는 리젠 콜백 직접 호출이며 별도 600초 검사와 구분한다. Logs/playtest/hunt_review_live.csv.
- 발판 중심 기대 밀도 613지점 범위 이탈 0. 7맵×낮밤 14조합 경계 점 겹침 검사. 일반 숲 (-695,0)에서 콜백 직접 호출 없이 자연 트리거/리젠으로 정원 58·실제 점유 58 확인. Logs/hunt1-review/trigger-maxima.csv, overlap-runtime.txt.
- 일반 숲 낮 600.00게임초 연속 평타: 시간 배율 1, 자동 접근·매 프레임 HP 보충·회피 없음, 실제 리젠 틱 사용. 86처치, 적 HP 피해 15,805, EXP 2,254, 직접 골드 발생 1,012G, HP 손실 17,206, Lv4→5. HP220 중형 물약 78.2개/10분(469.3개/시간) 등가이며 실제 소비량 아님. 방어 3·추가 기본공격 2는 종료 시 값이다. 기존 30초 표본과 레벨/정예/행동 분포가 다르므로 기존 보정 계수 입력 CSV는 덮어쓰지 않았다. Logs/playtest/hunt_review_600.csv.
- 낮 화면 직접 확인: Logs/playtest/hunt_review_daytime.png. 600초 검사 및 14조합 검사 뒤 플레이 중 콘솔 에러 0. 활성 객체 29개인 사냥터에서 직접 플레이 정지. 종료 후 최종 콘솔 에러 0, playing=False / changingPlay=False 확인. SaveFile.es3 백업 복원 1/1, 추가 es3 없음, MD5 F1CBD3EDF7E845CACE5442F6641C5B2F 전후 일치(Logs/hunt1-review/save-restored.json).

## 남은 미검증

- Lv1→15 실제 전체 퀘스트·수집·장비 확보·NPC 왕복 동선과 층계 문지기 실전 시간.
- 정상 회피/게임패드/하드웨어 조작 및 실제 물약 소비·가격 대비 수입.
- 모든 사냥터의 600초 연속 플레이, 실제 화면 밀도 분포, 다양한 해상도와 광역·스킬·강화 포함 효율.
- 제품 빌드의 장시간 GC/프레임 성능. 프로브·최소화 에디터 수치를 빌드 성능으로 일반화하지 않는다.

## 커밋할 파일 (24개)

수정 8개:
- .claude-rules/Game/ClassicMapleDesign.md
- .claude-rules/Game/EnemySpec.md
- Assets/Resources/MonsterSheet.csv
- Assets/Script/Enemy/EnemyHealth.cs
- Assets/Script/Enemy/EnemySpawner.cs
- Assets/Script/Enemy/MonsterTable.cs
- Assets/Script/Player/Level/LevelSystem.cs
- Docs/OpenQuestions.md

신규 16개:
- Assets/Editor/Dev/HuntingAudit.cs 및 .meta
- Assets/Editor/Dev/HuntingDensityAudit.cs 및 .meta
- Assets/Editor/Tests/HuntingRulesTests.cs 및 .meta
- Assets/Resources/HuntingSpawnsFloor1.csv 및 .meta
- Assets/Script/Enemy/HuntingRules.cs 및 .meta
- Assets/Script/Enemy/HuntingSpawnTable.cs 및 .meta
- Docs/Balance/Floor1HuntingBaseline.csv
- Docs/Balance/Floor1HuntingMeleeSample.csv
- Docs/Balance/Floor1HuntingAudit.md
- Docs/Balance/Floor1HuntingReview.md

커밋 제외: Tools/probes/hunt_*.cs (이번 hunt_review_*.cs 포함), Logs/**, Assets/Screenshots/** 및 hunt1 외 작업의 모든 변경. 검증 세이브 백업은 Logs/hunt1-review/save-backup/에 보관한다.
