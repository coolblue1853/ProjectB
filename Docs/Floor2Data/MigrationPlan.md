# 2층계 데이터 Resources 이행 계획 (11-05, AUTO-21)

초안 CSV(`Docs/Floor2Data/*`, AUTO-8)를 실 CSV(`Assets/Resources/`)와 재대조한 결과와 병합 절차. **데이터 이동은 아직 없다**(이행은 AUTO-22 스위치 뒤). 대조는 python 1회 스캔(읽기만).

## 1. 스키마 재대조

| 초안 | 실 CSV | 헤더 | 행 | ID 충돌 | 비고 |
|---|---|---|---|---|---|
| `Floor2Monsters.csv` | `MonsterSheet.csv` (64줄, floor 1·2 존재) | 18열 완전 일치 | 21 | 0 | 신규 21행은 `floor=2`, 구역 Statue Garden·Knight Citadel·Dragon Nest |
| `Floor2ItemSheet_draft.csv` | `ItemSheet.csv` (269줄) | 19열 이름 일치. **실 파일은 1행이 타입 행**(`int,string,...`), 초안은 헤더만 | 45 | 0 (index 268~312, 실 최대 267, 이름 충돌 0) | 병합 시 타입 행 복사 금지, 데이터 행만 이어붙임 |
| `Floor2SetItem_draft.csv` | `SetItem.csv` (46줄, 1행 타입·2행 헤더) | 2열 일치 | 12 | 0 | 키 = 아이템 `tfName` + `_2/_4/_6` |
| `Floor2ExtraDrops_draft.csv` | `MonsterExtraDrops.csv` (25줄) | 4열(`condition` 선택) | 28 | — | 몹·아이템 모두 실존/초안 내 존재, 미해결 참조 0 |
| `Floor2Zones.csv`·`Floor2Cliff_T6.csv` | (참고용) | — | — | — | 병합 대상 아님 |

불일치: **없음**(컬럼 추가로 어긋난 곳 0). 단 아래 3건은 병합 전 확인 사항.

## 2. 병합 전 확인 3건

| # | 항목 | 내용 | 조치 |
|---|---|---|---|
| a | `minFloor` | `MonsterSheet`·`ItemSheet`·`SetItem` 에 `minFloor` 열이 **없다**(`BagExpansion.csv`·`ShopStock` 만 사용). 몹은 `floor` 열(=2)과 구역으로 구분 | 2층계 상점/가방 행을 더할 때만 `minFloor=2` 설정. 몹·아이템 행은 변경 없음 |
| b | 계보 키(`killKey`) | 초안 21행 전부 `killKey` 빈칸. 실 CSV 는 보스·정예 14행이 `killKey=id`(`MiniBossDataTests` 가 중간보스 `killKey==id` 를 요구) | 병합 때 `isBoss=1` 7행(StatueQueen·Lancer/Sword/Archer/MageCaptain·KnightCommander·CloudseaDrake)에 `killKey=id` 채움. 일반 몹은 빈칸 유지 |
| c | 세트 이름 | 아이템 `setName`(표시명 Garden Warden 등)과 `SetItem` 키(`GardenArmor_N`)는 `tfName` 으로 연결됨. 실 구조와 동일 | 변경 없음. 4세트(Garden·MoonStone·Order·Drake)×(2/4/6)=12행 모두 키 존재 |

## 3. 병합 순서와 롤백 (11-05, AUTO-34 확정 — 체크리스트)

순서: 몹 → 아이템 → 세트 → 드롭. 앞 단계가 뒤 단계의 참조 대상(드롭이 몹·아이템을, 아이템이 세트 키를 가리킴)이다. 전제: AUTO-33(`killKey`·`tfName` 테스트) 완료. 기본 OFF 플래그(AUTO-22)는 건드리지 않는다.

### 3.1 병합 전
- [ ] `git status` 로 대상 4개 CSV(`MonsterSheet`·`ItemSheet`·`SetItem`·`MonsterExtraDrops`)가 깨끗한지 확인
- [ ] 기준 커밋 해시를 기록(롤백 지점): `git rev-parse --short HEAD`
- [ ] 초안 4개(`Docs/Floor2Data/*`)가 §1 행 수(21/45/12/28)와 같은지 확인

### 3.2 병합 (한 단계 = 한 커밋)
- [ ] 몹: `MonsterSheet.csv` 끝에 21행 추가, 보스 7행 `killKey=id` 채움 → 커밋 `(추가) 2층계 몹 21행`
- [ ] 아이템: `ItemSheet.csv` 끝에 45행 추가(타입 행 제외, index 268~312 연속) → 커밋
- [ ] 세트: `SetItem.csv` 끝에 12행 추가 → 커밋
- [ ] 드롭: `MonsterExtraDrops.csv` 끝에 28행 추가 → 커밋
- [ ] 줄바꿈·BOM 은 기존 파일 방식 유지, 끝 개행 확인

### 3.3 병합 직후 확인
- [ ] EditMode 테스트: `DataIntegrityTests`·`MonsterTableTests`·`MiniBossDataTests`·`Floor2DataTests`·`Floor2BalanceTests` 통과(ID 중복 0, 참조 실존)
- [ ] `refresh_unity` 컴파일 에러 0, 콘솔 신규 에러 0
- [ ] 플레이 프로브 1회(`Tools/playtest.sh`): 1층계 시작→몹 처치→드롭이 정상, 2층계 몹은 플래그 OFF 라 미등장
- [ ] 아이콘·프리팹 누락 보고서는 예상됨(그래픽 별도) — 목록을 `Docs/Playtest/DataIntegrity.md` 에 기록
- [ ] 플래그(AUTO-22) OFF 상태로 먼저 커밋·push 해 플레이 영향 0 을 확인한 뒤에만 ON

### 3.4 실패 시 롤백
- [ ] 단계별 커밋이므로 문제 단계만 `git revert <해시>` (이후 단계가 참조하면 역순으로 revert: 드롭→세트→아이템→몹)
- [ ] 전체 철회: `git checkout <3.1 해시> -- <4개 CSV>` 후 커밋. 초안은 `Docs/Floor2Data/` 에 그대로 남음
- [ ] 플래그 OFF 로 즉시 완화 가능(데이터가 있어도 로드 안 함)
- [ ] 세이브 영향 없음(신규 아이템 index 만 추가, 기존 index 불변)

### 3.5 보류 조건
- 그래픽(몹 스프라이트·프리팹·아이콘) 없이 병합해도 되는지는 사용자 판단 → `Docs/OpenQuestions.md` Q-F2M 임시 가정: 플래그 OFF 이므로 데이터만 먼저 병합 허용, ON 은 그래픽 승인 후.
