# 2층계 데이터 병합 사전 점검 (11-05, AUTO-73)

병합 전 현재 상태 점검. 읽기·python 스캔만, CSV 수정 없음. 통과 = 이번 점검에서 확인, 미확인 = 이번 점검 범위 밖(테스트 실행 필요). 절차 원본은 `MigrationPlan.md` §3.

주의: 문서의 "Floor2Preview/*.csv" 실체는 `Assets/Resources/Floor2Preview/`(로더가 읽는 사본, 파일명 `_draft` 없음)이고, `Docs/Floor2Data/*_draft.csv` 는 별도 사본이다. 병합 원본은 둘 중 하나로 정해야 한다(아래 #8).

## 1. 점검 표

| # | 항목 | 결과 | 근거 | 재대조(AUTO-89, 11-05) |
|---|---|---|---|---|
| 1 | 몹 헤더 열 순서 | 통과 | 18열 이름·순서 `MonsterSheet.csv` 와 일치 (초안·Resources 사본 모두) | 유지 |
| 2 | 아이템 헤더 열 순서 | 통과 | 19열 일치. 실 `ItemSheet.csv` 는 1행 타입 행, 2행 헤더, 19번째 열 `category` 포함(원시 줄 확인). 초안 `category` 값은 전부 빈칸 | 유지 |
| 3 | 세트 헤더 | 통과 | `setName,effect` 2열, 실 파일 1행 타입·2행 헤더 | 유지 |
| 4 | 드롭 헤더 | 통과 | 초안 `monsterId,item,probability,condition` 4열, 실 파일 동일 4열 | 유지 |
| 5 | 몹 id 중복 | 통과 | 초안 21행 id 중복 0, 실 63행과 겹침 0 | 유지 |
| 6 | 아이템 index·이름 중복 | 통과 | 초안 268~312(45행) 연속, 초안 내 중복 0, 실 최대 267(268행)과 겹침 0, 이름 겹침 0 | 유지(Resources 사본 행수 아이템 46·세트 13·드롭 29, 헤더 포함) |
| 7 | 세트 키 중복·연결 | 통과 | 초안 12행이 실 키와 겹침 0. 아이템 `tfName` 4종(Garden·MoonStone·Order·Drake Armor) 모두 `_2/_4/_6` 키 존재 | 유지 |
| 8 | Docs 초안 ↔ Resources 사본 일치 | **불일치** | 아이템 45·세트 12·드롭 28 행은 동일. 몹 21행 중 **7행이 다름**: Docs 사본은 보스 7행 `killKey` 가 빈칸, Resources 사본은 `killKey=id` 채움. `MigrationPlan.md` §2b 는 "이미 채워짐"이라 적었으나 Docs 사본에는 해당 없음 → 수정 후보: 병합은 Resources 사본을 원본으로 삼거나 Docs 사본 7행을 채워 동기화 | **불일치 유지**: 몹 22행(헤더 포함) 중 7행 다름, 아이템·세트·드롭 0행 다름 |
| 9 | 보스 `killKey==id` | 통과 | Resources 사본 보스 7행(StatueQueen·Lancer/Sword/Archer/MageCaptain·KnightCommander·CloudseaDrake) 모두 `killKey=id`. `On_BossRowsHaveKillKey` 가 이 사본을 검증 | 유지 |
| 10 | 드롭의 몹 참조 | 통과 | 28행 몹 id 전부 실 또는 초안 몹에 존재(미해결 0) | 유지 |
| 11 | 드롭의 아이템 참조 | 통과 | 28행 아이템 이름 전부 실 또는 초안 `name` 에 존재(미해결 0) | 유지 |
| 12 | 드롭 (몹, 아이템) 쌍 중복 | 통과 | 실 파일과 겹침 0, 초안 내 중복 0 | 유지 |
| 13 | 병합 로더 기본 OFF | 통과(정적) | `Floor2Data.Enabled` 기본 false, 로더는 OFF 면 원문 반환. 실서비스 코드에서 ON 으로 켜는 곳 0 은 `ProductionCode_NeverEnablesSwitch` 가 검사 | 유지 |
| 14 | EditMode 테스트 실제 통과 | 미확인 | 이번 점검은 테스트 미실행(Unity 사용 금지). 아래 §2 로 병합 직전 실행 | 미확인 유지(테스트 미실행) |
| 15 | 아이콘·프리팹 | 미확인 | 그래픽 별도(Q-F2M). `DataIntegrity.md` 에 누락 목록 기록 예정 | 미확인 유지 |

불일치 1건(#8)이 병합 전 정리 대상이다. 나머지 데이터 정합은 모두 통과.

## 2. 필요 테스트와 실행 명령

| 테스트 | 무엇을 막나 |
|---|---|
| `Floor2DataTests` | 기본 OFF 원문 보존, ON 시 행 추가, id 중복, 보스 `killKey`, 세트 `tfName` 연결, 헤더 일치(`RealHeaders_MatchBase`), 스위치 오용 |
| `Floor2LinkTests` | 드롭의 몹·아이템 실존, 세트 키 전 단계, 계보 무기 드롭·4세트 쿨다운 스킬, 몹 수치 범위 |
| `Floor2BalanceTests` | 기존 몹 중복·구역 대역, 초안 퀘스트 스키마·예산, 레벨 25 보상 타이밍 |
| `Floor2MobBehaviorRulesTests` | 석상 기상·자세 주기 경계(규칙 클래스만) |
| `DataIntegrityTests`·`MonsterTableTests`·`MiniBossDataTests` | 실 CSV 전체 id·참조·중간보스 `killKey` |

실행(에디터가 꺼진 상태): `bash Tools/runtests.sh Floor2` 후 `bash Tools/runtests.sh DataIntegrity` 등 클래스명 필터, 결과 `Logs/editmode_results.xml`. 에디터가 켜져 있으면 unity-mcp `run_tests`(EditMode, 같은 이름 필터). 병합 후에는 필터 없이 전체 EditMode 1회.

## 3. 아직 막는 것

| 막는 것 | 상태 | 병합 영향 |
|---|---|---|
| 몹 행동(석상 기상·자세·천칭 반사 등) | 규칙 클래스 `Combat/Rules/Floor2MobBehaviorRules.cs` 와 테스트만 있고 몹 컴포넌트가 호출하지 않음 | 데이터 병합과 무관. 플래그 ON 전까지 몹 미등장이라 영향 0 |
| 방어·반사·둔화 등 신규 시스템 | `Floor2Plan.md` §5.5 표의 "신규 필요" 항목 | 병합과 무관, ON 전 완성 필요 |
| 그래픽(스프라이트·프리팹·아이콘) | 사용자 승인 대기 | 임시 가정 F2M: 데이터만 먼저 병합 허용, ON 은 승인 후 |
| 계보 정합 | `Docs/Playtest/LineageAudit.md` 후보 | 병합 전 확인 권장, 블로커 아님 |

## 4. 병합해도 되는 최소 범위 제안

1. 먼저 #8 정리(Resources 사본을 원본으로 확정하거나 Docs 사본 보스 7행 `killKey` 채움).
2. §2 테스트를 병합 전 한 번 실행해 현재 녹색을 확인.
3. `MigrationPlan.md` §3.2 순서대로 몹 → 아이템 → 세트 → 드롭, 한 단계 한 커밋. 몹 병합 때 보스 7행 `killKey=id`.
4. `Floor2Data.Enabled` 는 건드리지 않는다. 데이터가 실 CSV 에 들어가면 플래그와 무관하게 로드되므로, 병합 직후 몹 존(Statue Garden 등)이 맵에 배치되지 않은 것(2층계 맵 미연결)과 상점·드롭 테이블에 신규 아이템이 노출되지 않는지를 §3.3 플레이 프로브로 확인한다.
5. 위험: 실 CSV 병합은 로더 플래그를 우회한다. 플래그 OFF 로 막던 노출(드롭 테이블 `MonsterExtraDrops` 의 신규 몹 행은 신규 몹이 스폰되지 않으면 무해, 신규 아이템 index 는 기존 세이브 불변)을 병합 후에도 지키는지는 미검증이다.

## 5. 수정 후보 (불일치)

- #8: `Docs/Floor2Data/Floor2Monsters.csv` 보스 7행 `killKey` 를 `Assets/Resources/Floor2Preview/Floor2Monsters.csv` 와 같게 채우고, `MigrationPlan.md` §2b 에 "원본 = Resources 사본" 한 줄 추가.

## 6. 병합 가능한 최소 범위 (11-05 재대조 확정)

- 실 `ItemSheet.csv` 최대 index 는 여전히 267, 신규 초안 268~312 와 겹침 없음. 데이터 단계 불일치는 #8 한 건뿐이다.
- 최소 범위 = (1) #8 해소(Docs 몹 사본 보스 7행 `killKey` 채우기 또는 Resources 사본을 원본으로 확정) → (2) §2 테스트 실행 → (3) 몹 → 아이템 → 세트 → 드롭 순으로 단계별 병합. 그래픽·몹 행동·신규 시스템은 병합 블로커 아님.
- 후속 (b): 병합 후 신규 아이템 이름의 한글 표시 `LocItems.cs` 누락 여부는 병합 후 재확인(`LocPerfAudit.md` §10 은 코드 문자열 대상).

