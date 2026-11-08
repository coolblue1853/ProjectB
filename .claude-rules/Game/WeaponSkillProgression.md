# 무기 스킬 순차 해금·성장 설계 (WSKILL-1)

> 상태: B 채택(사용자 선택, 임시 가정 WS-1~3). **P0~P3 구현 완료, P4 부분 완료(2026-11-06)**. 이행 상태는 9절 표. 임시 가정은 `Docs/OpenQuestions.md` WSKILL 항목.
> 읽은 것: `WeaponReview.md`(1.1·1.3), `ProgressionDesign.md`, `WeaponTypes.md`(4절), `LineageGear.md`, 코드 `Weapon.cs`·`AttackManager.cs`·`Skill.cs`·`LevelRules.cs`·`LevelSystem.cs`.

## 0. 추천 (결론)

**(B) 무기 숙련도 채택. 단 "무기 키별 숙련 레벨 → 스킬 슬롯 해금" 최소형으로 시작한다.**
- 그 무기를 장착한 채 얻은 EXP(몹 처치 EXP와 같은 값)가 그 무기의 숙련 EXP로 쌓인다. 플레이어 레벨 곡선은 건드리지 않는다.
- 숙련 Lv로 스킬 슬롯이 하나씩 열린다(평타는 처음부터).
- 이유: 세트 파밍·계보 장비는 "무기를 갈아 끼우는 것"이 핵심인데, (A)는 레벨만 오르면 모든 무기의 스킬이 한꺼번에 열려 무기 간 차이가 없다. (B)는 새 무기마다 성장이 있어 파밍 보상감이 산다.

## 1. 현황 (코드 확인)

| 항목 | 현재 | 위치 |
|---|---|---|
| 스킬 보관 | `Weapon.skill = new Skill[4]` (프리팹에 박힌 `Skill` 컴포넌트, 슬롯 0~3 = A/S/D/F) | `Weapon.cs:60` |
| 발동 | `AttackManager` 가 슬롯 i 마다 `equipWeapon.skill[i]` 가 null 이 아니면 입력 처리 → `Weapon.ActiveWeaponSkill(i)` → `Skill.ActiveSkill` | `AttackManager.cs:205-236`, `Weapon.cs:193-203` |
| 해금 개념 | **없다.** 무기를 장착하면 null 이 아닌 슬롯은 전부 즉시 사용 가능. 레벨·숙련 조건 코드 없음(`reqLevel`·`unlock` grep 0건) | - |
| 차단 사유 | `Skill.BlockReason(i)` = 쿨다운·스태미나 2종 뿐 | `AttackManager.cs:218` |
| 무기 데이터 | ItemSheet 무기 행 **54개**(Weapon 문자열 54 hit). 스킬 수치·이름은 ItemSheet 가 아니라 `Resources/Weapon/<키>.prefab` 에 있음 | `WeaponReview.md` 0절 |
| 스킬 수 | 무기당 0~4개(대부분 2~4개). 복제 무기(CliffBlade 등)는 같은 스킬 | `WeaponReview.md` 1.1 |
| 레벨·EXP | `LevelRules`(순수: `AddExp`, `ExpToNext`, 만렙 70, Lv1~15 표 500..24500), `LevelSystem`(상태·`Changed`/`LeveledUp` 이벤트) | `Level/` |
| 스킬 UI | `SkillCooldown`(HUD 슬롯 아이콘·쿨), `CombatHud`(스태미나·쿨 안내). 스킬 창·스킬 트리 UI 없음 | `UI/` |

핵심: 해금 상태를 끼울 자리는 **`AttackManager` 슬롯 루프(205행 부근)와 `Skill.BlockReason`** 두 곳이다. 프리팹은 건드리지 않아도 된다.

## 2. 비교

### 2.1 (A) 플레이어 레벨 연동 (마크식)

플레이어 Lv 가 오르면 스킬 슬롯이 열린다(예: Lv5 S, Lv10 D, Lv15 F). 스킬 레벨은 "스킬 포인트" 또는 레벨 자동 성장.

| 항목 | 평가 |
|---|---|
| 장점 | 구현 최소(`LevelSystem.Level` 비교 한 줄). 저장 필드 불필요. 기존 EXP 곡선·사망 페널티 재사용. 이해하기 쉬움 |
| 단점 | 레벨 N 이후에는 **모든 무기 스킬이 전부 열려 있음** → 새 무기를 주워도 성장 차이 없음. 무기 54종의 스킬 차이가 해금 구간에서만 의미 |
| 세트·계보 | 무기 교체 비용이 0이라 계보 무기(몹 스킬 복사) 수집 동기가 "수치"뿐. 낮은 레벨에 얻은 희귀 무기의 스킬은 못 쓰는 상태로 대기(레벨 게이트) → 파밍 보상 지연 |
| ES3 | 필드 추가 없음(최선) |
| UI | 슬롯 잠금 표시만 |
| 규모 | 매우 작음(0.5일) |
| 위험 | 레벨 곡선(Lv15 이후 외삽 임시값)과 해금이 엮여 밸런스 조정이 서로를 흔든다 |

### 2.2 (B) 무기 숙련도 (추천)

| 항목 | 평가 |
|---|---|
| 장점 | 무기마다 성장 서사("이 칼에 익숙해진다"). 갈아 끼울 때 선택 비용 발생 → 세트 파밍·계보 수집이 성장 루프와 맞물림. 낮은 레벨 무기를 계속 쓸 이유(숙련 만렙 보너스) 생김. 플레이어 레벨 곡선과 독립이라 밸런스 분리 |
| 단점 | 세이브 필드·UI 추가 필요. 새 무기 습득 직후 약하게 느껴질 수 있음(→ 4절 완화). 무기 54종 × 숙련 데이터가 늘어나 "전부 키우기" 피로 |
| 세트·계보 | 계보 무기(몹 스킬 복사)는 해금 순서를 "몹이 쓰는 순서"로 짤 수 있어 `LineageGear.md` 의 컨셉과 일치. 세트 효과(장비 N부위)는 숙련과 독립 — 충돌 없음 |
| ES3 | 신규 키만 추가(기존 키 불변, 5절) |
| UI | 인벤 툴팁에 숙련 Lv, 스킬 슬롯 잠금 표시, 간이 스킬 목록 창(6절) |
| 규모 | 중간(2~3일, 7절) |
| 위험 | 8절 |

### 2.3 하이브리드(참고)
(A)로 "스킬 슬롯 수 상한"을 Lv 로 열고 (B)로 슬롯 내 순서 해금: 구현·설명이 복잡해져 **채택하지 않는다**.

## 3. 추천안 상세 (B 최소형)

### 3.1 규칙

1. 숙련 키 = **무기 ItemSheet 키**(예 `RustedSword`). 강화(+N)·개체 차이와 무관하게 키별 1개. 드롭을 다시 주워도 유지.
2. 숙련 EXP 획득 = `LevelSystem` 이 EXP 를 얻을 때 **같은 양**(`LevelRules.KillExp` 결과)을 현재 장착 무기에 더한다. 퀘스트·기타 EXP 도 같은 경로면 동일. 만렙(플레이어 Lv70)이어도 숙련은 쌓인다.
3. 숙련 Lv 1~5. Lv1 = 평타 + 첫 스킬. Lv5 = 모든 스킬 + 소규모 보너스(데미지 +5%, 임시).
4. 스킬 해금은 **스킬 개수 n 별 해금 표**(3.2). 잠긴 슬롯 키를 누르면 "숙련 Lv N 에 해금" 안내(`CombatHud` 알림 재사용), 쿨·스태미나 소모 없음.
5. 사망 시 숙련 EXP 손실 없음(플레이어 EXP 만 `ExpAfterDeath`).
6. 새 무기 따라잡기: 같은 `WeaponType` 계열에서 이미 숙련 Lv3 이상인 무기가 있으면 신규 무기의 **숙련 EXP 획득 x2**(Lv3 도달 시까지). 임시.

### 3.2 해금 순서 예 (n = 스킬 수)

| n | Lv1 | Lv2 | Lv3 | Lv4 | Lv5 |
|---|---|---|---|---|---|
| 1 | S1 | - | - | - | +5% 보너스 |
| 2 | S1 | - | S2 | - | +5% |
| 3 | S1 | S2 | - | S3 | +5% |
| 4 | S1 | S2 | S3 | S4 | +5% |

- 해금 순서 = **슬롯 번호 순**(프리팹 `skill[0..3]`) 기본값. 순서를 바꾸고 싶은 무기만 CSV 에서 덮어쓴다(5.2 `UnlockOrder`).
- 예(RustedSwordandShield, n=3): Lv1 Fierce Thrust, Lv2 Shield Charge, Lv4 Hold the Line(홀드 스킬은 난이도가 높아 후반).
- 예(ScareSide, n=4): Lv1 Sinister Slash, Lv2 Nightmare Eruption, Lv3 Whirling Malice, Lv4 Dark Harvest.
- 스킬 0개 무기(PickAxe 류, W-01)는 해당 없음.

### 3.3 숙련 EXP 곡선 (초안)

"다음 숙련 Lv 까지 필요 EXP" = 기준표 × 무기 계수.

| 현재 숙련 Lv | 1→2 | 2→3 | 3→4 | 4→5 | 누적 |
|---|---|---|---|---|---|
| 기준 EXP | 400 | 1100 | 3000 | 7500 | 12000 |

- 근거: 플레이어 Lv1→2 필요 500, Lv1~10 누적 약 29k(5000+6000 포함 전) — 첫 무기가 첫 레벨업 직전(400)에 Lv2 를 얻고, Lv5 는 플레이어 Lv10 전후에 도달하도록 맞춘 값이다. 1층계(Lv~15) 동안 기본 무기 1~2종은 만렙, 신규 계보 무기는 중간까지.
- 무기 계수(임시): ItemSheet 등급 기준 일반 1.0 / 고급 1.5 / 희귀 2.5 / 영웅 4.0. 높은 등급 무기는 EXP 단가가 커진 후반에 얻으므로 임계값도 키운다.
- 초기값은 플레이 후 조정. 곡선은 코드에 직접 박지 않고 순수 클래스 상수 + 테스트로 둔다(`LevelRules` 와 같은 방식).

## 4. 위험 완화: 새 무기가 약해 보이는 문제

- Lv1 에서도 첫 스킬은 즉시 쓸 수 있다(스킬 0개 상태 없음).
- 3.1-6 따라잡기 x2.
- 보스·정예 드롭 희귀 무기는 시작 숙련 Lv2 로 지급(`StartMasteryLevel` 컬럼, 기본 1). 임시.

## 5. 데이터·세이브

### 5.1 ES3 (필드 추가만)

| 키 | 타입 | 설명 |
|---|---|---|
| `weaponMastery` | `Dictionary<string, WeaponMasteryData>` 또는 `string[] keys` + `int[] exps` | 무기 키 → 누적 숙련 EXP(레벨은 EXP 에서 계산, 저장 안 함 → 곡선 조정 시 세이브 재계산) |

- 기존 세이브에 키 없음 → 빈 사전. **구세이브 마이그레이션 문제**: 이미 모든 스킬을 쓰던 세이브가 잠기는 것을 막기 위해, 로드 시 `weaponMastery` 키가 **없으면** 현재 인벤·장착 무기에 한해 숙련 EXP 를 Lv3 상당(1500)으로 초기화(1회, 플래그 `weaponMasteryMigrated`). 임시.
  - **구현 변경(P2):** Lv3(1500)은 스킬 3·4번(해금 Lv4)이 잠겨 기존 플레이어가 스킬을 잃는다. 그래서 대상 무기(인벤·창고·장착 중 `Resources/Weapon/<키>` 가 있는 것)를 **만렙 EXP 12000(Lv5)** 로 시작시킨다(`WeaponMasteryRules.LegacyStartExp`). 새로 얻는 무기는 0부터.
  - 저장: ES3 키 `weaponMastery`(`WeaponMasterySave{keys[],exps[]}`), `weaponMasteryMigrated`(bool). `Datas` 는 불변. 새 게임(세이브 파일 없음)은 마이그레이션 없이 플래그만 기록.
- 기존 키 이름·타입 변경 없음(`CodeStyle.md` §3 준수).

### 5.2 CSV (맨 뒤 컬럼 추가, `ItemSpec.md` 갱신)
- ItemSheet 맨 뒤에 `MasteryScale`(float, 빈칸 = 등급표 사용), `StartMastery`(int, 빈칸 = 1), `UnlockOrder`(문자열 `0|1|3|2`, 빈칸 = 슬롯 순)을 **선택** 컬럼으로 둔다. 1차 구현은 컬럼 없이 코드 기본값만으로 동작해야 한다.

## 6. UI 영향

| 위치 | 변경 |
|---|---|
| HUD 스킬 슬롯(`SkillCooldown`) | 잠긴 슬롯은 어둡게 + 자물쇠/"Lv N" 표시 |
| 인벤 무기 툴팁(`InventoryManager` 툴팁) | "숙련 Lv 3 (1200/3000)" 한 줄 + 스킬 해금 상태 줄. `InventoryManager` 는 2000줄이므로 **분리된 `WeaponMasteryTooltip` 정적 헬퍼**로 문자열만 만든다 |
| 스킬 창 | 1차는 만들지 않는다. 2차로 코드 생성 간이 창(장착 무기 스킬 목록·해금 Lv·숙련 바). 단축키 임시 K |
| 알림 | 숙련 Lv 상승·스킬 해금 토스트(기존 알림 경로) |
| 로컬라이즈 | 신규 문자열은 Loc 누락 점검(`Docs/Playtest/LocPerfAudit.md`) 기준으로 I2/Loc 에 등록 |

## 7. 구현 규모 (파일·클래스)

| 파일 | 내용 | 종류 |
|---|---|---|
| `Script/Player/Mastery/WeaponMasteryRules.cs` | 순수 정적: `ExpToNext(lv, scale)`, `LevelFromExp`, `IsUnlocked(slot, skillCount, lv, order)`, `Bonus(lv)` | 신규, EditMode 테스트 |
| `Script/Player/Mastery/WeaponMasteryData.cs` | 키별 EXP 보관·ES3 저장/로드·`Changed`/`LevelUp` 이벤트 | 신규 |
| `Script/Player/Mastery/WeaponMasteryService.cs` | EXP 수신(`LevelSystem` 연결), 장착 무기 키 조회, 따라잡기 계수 | 신규 |
| `Script/Player/Level/LevelSystem.cs` | EXP 획득 직후 `WeaponMasteryService.AddForEquipped(gain)` 호출 한 줄 | 수정 |
| `Script/Player/AttackManager.cs` | 슬롯 루프에서 `WeaponMasteryService.IsSlotUnlocked(equipWeapon, i)` false 면 `continue` + 안내 | 수정(소) |
| `Script/Weapon/Skill.cs` | `BlockReason` 에 `Locked` 추가(`SkillBlock` enum) | 수정(소) |
| `Script/Player/Weapon.cs` | 숙련 보너스를 `SetEnhance` 와 같은 방식으로 데미지 배율에 곱(강화와 합산 순서는 곱셈) | 수정(소) |
| `Script/UI/SkillCooldown.cs`·`CombatHud.cs` | 잠금 표시·안내 | 수정(소) |
| `Script/UI/WeaponMasteryTooltip.cs` | 툴팁 문자열 헬퍼 | 신규 |
| `Editor/Tests/WeaponMasteryRulesTests.cs` | 곡선·해금표·구세이브 마이그레이션 | 신규 |
| `Docs/Handoff/queue.md` 큐 | 단계별 항목 | 후속 |

예상: 신규 5파일(약 400줄) + 수정 5파일(각 수십 줄). 2~3일.

## 8. 위험

| 위험 | 완화 |
|---|---|
| 구세이브가 잠긴다(스킬을 쓰던 사용자) | 5.1 마이그레이션 플래그 |
| 무기 54종 전부 키우기 피로 | 따라잡기 x2, 스타트 Lv, Lv5 가 12000 EXP(전체 Lv10 수준)로 낮음 |
| 숙련 EXP 가 "플레이어 EXP 와 같은 양"이라 후반 인플레 → 곡선 무의미 | 등급 계수 + 후속에 지수 상한. 6절 이후 Lv 곡선 조정 시 재계산 가능하도록 레벨 비저장 |
| 해금이 홀드·고난도 스킬 순서와 맞지 않음 | `UnlockOrder` 덮어쓰기 |
| 복제 무기(CliffBlade=ScareSide)가 숙련을 따로 먹음 | 키별 분리가 의도. 문제면 `MasteryGroup` 컬럼으로 공유(후속) |
| 스킬 0개/레거시 무기 | 해금 판정 제외 |
| `AttackManager` 슬롯 루프·입력 버퍼와의 충돌(W-09, W-15 등 기존 이슈) | 잠금은 루프 최상단 `continue` 로 버퍼 이전에 처리, 버퍼 건드리지 않음 |

## 9. 단계적 이행안

| 단계 | 내용 | 검증 | 상태 |
|---|---|---|---|
| P0 | `WeaponMasteryRules` + 테스트(곡선·해금표) | EditMode 테스트 통과 | 완료 11-06 (13/13) |
| P1 | `Data`·`Service`·ES3 + `LevelSystem` 연결(숙련만 누적) | 프로브: 몹 처치 후 숙련 EXP 증가, 재시작 후 유지 | 완료 11-06 (BanditArcher +38=플레이어 EXP, 재시작 유지) |
| P2 | 슬롯 잠금(`AttackManager`·`Skill.BlockReason`=`SkillBlock.Locked`) + 구세이브 마이그레이션 | 프로브: Lv1 무기에서 2번 슬롯 입력 거부·쿨·스태미나 불변, Lv3 에서 발동 | 완료 11-06 (EditMode 914/914) |
| P3 | HUD 잠금 표시 + 인벤 툴팁 + 토스트 | 플레이 스크린샷(낮 시간대) | 완료 11-06 (EditMode 914/914, `Logs/playtest/wsp3_*.png`: 잠긴 슬롯 어둡게+"LV 3", NotifyLocked 화면 표시, 해금 토스트. 툴팁은 문자열만 프로브 확인·시각 미검증) |
| P4 | Lv5 보너스·따라잡기·StartMastery·등급 계수 + 수치 조정 | 플레이 후 곡선 재조정 | 부분 완료 11-06: Lv5 +5%(`Weapon.SetDmgArray` 곱, LevelUp 이벤트로 갱신), 따라잡기 x2(ItemSheet WeaponType 기준 계열, `GainMultiplier`), StartMastery(ItemSheet 20번째 선택 컬럼 파싱, 첫 EXP 획득 시 적용, 컬럼 값 미기입). 프로브: 38→76 EXP, 800/1000→840/1050, EditMode 16/16. 등급 계수·곡선 재조정 미착수 |
| P5(선택) | 간이 스킬 창, 숙련 그룹 | - | 미착수 |

P1 까지는 플레이 영향이 없으므로 안전하게 먼저 넣을 수 있다.

## 10. 임시 가정 요약
`Docs/OpenQuestions.md` WSKILL 항목 참고: B 채택, 키별 숙련, Lv 1~5, 곡선 400/1100/3000/7500, 등급 계수, Lv5 보너스 +5%, 구세이브 Lv3 초기화, 잠금 시 소모 없음.

---

# V2. 타입별 숙련 + 스킬트리 (사용자 11-07, 1~10절을 대체하는 부분은 아래에 명시)

> 상태: 설계 확정(임시 가정 V2-1~8, `Docs/OpenQuestions.md` Q-WSKILL2). 구현 전. 1~10절 중 **키별 Lv·따라잡기·StartMastery·Lv5 +5%** 는 V2 에서 비활성(코드 유지, V.5). 스킬 슬롯 해금 표(3.2)·잠금 UI(P2·P3)는 그대로 쓰되 기준 레벨만 "무기 키 → 타입"으로 바뀐다.
> 읽은 것: 위 1~10절, `WeaponTypes.md`, `ProgressionDesign.md`, 코드 `WeaponMasteryService/Rules/Data`, `WeaponGimmick.cs`(`WeaponKind` 20값), `Weapon.cs`(critPer·critDmg·`SetDmgArray`).

## V.1 타입별 숙련

| 항목 | 규칙 |
|---|---|
| 키 | `WeaponKind`(Sword, Greatsword, Scythe, Dagger, Knuckle, Claw, Spear, Adae, Staff, Axe, Mace, Whip, Flail, Dual, Tachi, Bow, Crossbow, Tome, Javelin, Bulwark). `None` 제외. 무기 키→종류는 기존 `WeaponMasteryService.KindOf`(ItemSheet `WeaponType`) 재사용. 저장 문자열은 enum 이름(순서 변경에 안전) |
| EXP | 기존과 같다: EXP 획득 시 **같은 양**을 현재 장착 무기의 **타입**에 가산(`AddForEquipped`). 만렙(플레이어 Lv70)이어도 누적, 사망 손실 없음, 무기 없음(맨손)은 가산 없음 |
| 같은 타입 새 무기 | 타입 EXP 를 공유하므로 새 검을 주워도 검 Lv 그대로 이어진다. 따라잡기 x2·StartMastery 는 불필요해져 비활성 |
| 최대 Lv | **10**(기존 5 → 확장) |
| 곡선(Lv→Lv+1) | 1→2 400, 2→3 1100, 3→4 3000, 4→5 7500(기존 재사용, 누적 12000) · 5→6 12000, 6→7 18000, 7→8 26000, 8→9 36000, 9→10 48000. 누적 152000 |
| 근거 | Lv5 까지는 기존 값 그대로라 구세이브·해금 표 호환. 플레이어 EXP Lv1~15 누적 약 17.5만(`LevelRules`)이므로 주력 타입 Lv10 ≈ 플레이어 Lv15 전후(1층계 말). 서브 타입은 Lv4~6 |
| 등급 칭호 5단계 | Lv1~2 **견습** · Lv3~4 **숙련자** · Lv5~6 **전문가** · Lv7~8 **달인** · Lv9~10 **명인**. 한글은 Loc 키 `Mastery.Title.1..5` |
| 해금 연결 | 슬롯 해금 표(3.2)는 변경 없이 **타입 Lv** 로 판정(Lv1~4 안에서 끝). 타입 Lv5+ 는 스킬 해금과 무관하고 트리 포인트만 준다 |
| 포인트 지급 | 타입 Lv2~10 도달마다 1포인트(타입당 최대 9). **저장하지 않는다**: `earned = Lv-1`, `available = earned - spent`. 곡선 조정에도 안전 |

순수 함수(`TypeMasteryRules`): `ExpToNext(lv)`, `LevelFromExp(exp)`, `TitleIndex(lv)`(0~4), `EarnedPoints(lv)`.

## V.2 타입 스킬트리 (공통 템플릿 + 타입별 극의)

모든 타입이 같은 3갈래·8노드 템플릿을 쓴다(19종 개별 설계 대신 템플릿 + 극의 1개만 타입별). 노드 레벨당 비용 1포인트, 극의만 3포인트.

| 갈래 | 노드 ID | 효과(레벨당) | 최대 Lv | 선행 |
|---|---|---|---|---|
| 위력 | `P1` 평타 단련 | 평타 데미지 +2% | 5 | - |
| | `P2` 기술 단련 | 스킬 데미지 +3% | 3 | P1≥3 |
| | `P3` 극의 | 타입 고유 효과 강화(아래 표) | 1(비용 3) | P2≥2 |
| 정밀 | `A1` 급소 | 치명타 확률 +0.5%p | 5 | - |
| | `A2` 치명 | 치명타 피해 +3%p | 5 | A1≥3 |
| 기동 | `M1` 연타 | 공격 속도 +1.5% | 5 | - |
| | `M2` 숙달 | 스킬 쿨다운 -3% | 3 | M1≥3 |
| | `M3` 효율 | 스킬 스태미나 소모 -4% | 3 | M1≥3 |

- 총 용량 = 5+3+3+5+5+5+3+3 = **32** 포인트 대 획득 9 → 28% 만 채운다. 선택이 의미 있다(위력 올인 P1·P2·P3 = 11 > 9 로 불가).
- 타입 트리 효과는 **그 타입 무기를 장착했을 때만** 적용(타입이 다르면 0).
- 검 데미지 증가(사용자 예) = `P1`·`P2`.
- 극의 `P3` 효과(배율은 `WeaponGimmick` 기믹 수치에 곱, 기믹 없는 타입은 "평타 마지막 타 데미지 +15%" 기본): Sword 3타 마무리 +15% · Greatsword/Scythe 차지·홀드 효과 +15% · Dagger 연타 판정 +1회 · Bow/Crossbow 차지 사거리 +15% · Staff/Tome 발사체 데미지 +10% · 나머지 기본 효과. 상세 수치는 구현 때 조정(P7).
- 노드 데이터: 코드 상수 `SkillTreeRules.Nodes`(순수 정적 배열, `LevelRules` 방식). CSV 화는 후속. 노드 ID 는 불변(세이브 키).

## V.3 공용 트리

| 항목 | 규칙 |
|---|---|
| 포인트 | 플레이어 레벨 **1당 1포인트**(Lv2 부터, `earned = Lv-1`, 저장 안 함). Lv15 = 14, Lv70 = 69 |
| 노드 | 비용 1/레벨 | 
| `C1` 치명타 확률 +0.5%p ×10 | `C2` 치명타 피해 +2%p ×10 |
| `C3` 공격 속도 +1% ×10 | `C4` 이동 속도 +1% ×5 |
| `C5` 최대 HP +2% ×10 | `C6` 방어 +1% ×10 |
| `C7` 스태미나 회복 +2% ×5 | 총 용량 60(코드 `SkillTreeRules.Capacity(true)`, 초안의 65 는 합산 오류) |
| 선행 | 없음(공용은 자유 배분. 선택지가 많아 상한으로 제어) |

**합산 상한**(타입 트리 + 공용 트리 + 장비 제외, **트리 출처만**): 치명타 확률 +10%p, 치명타 피해 +40%p, 공격 속도 +25%, 평타·스킬 데미지 합 +30%, 이동 속도 +10%, 쿨 감소 -15%, 스태미나 소모 -20%. 상한은 `SkillTreeRules.Cap(stat)` 로 최종 합산에서 clamp(노드 투자는 막지 않는다 → 초과분은 낭비라 UI 에 "상한" 표시). 장비 수치는 상한과 별개로 기존 공식에 합산한다.

- 스탯 적용 위치(모두 "곱/합산 한 줄"): 데미지 = `Weapon.SetDmgArray`(강화 x 숙련과 같은 곱, `Weapon.cs:114`), 치명타 = `Weapon.cs:117` critPer·critDmg 에 가산, 공속 = 공격 애니/판정 속도 계수(builder 가 `AttackManager`·`PlayerController` 에서 기존 공속 훅을 grep 해 확인, 없으면 Animator speed 에 곱), 쿨·스태미나 = `Skill`, 이동 = `PlayerController.moveSpeed` 사용처, HP·방어 = `PlayerHealthManager`.

## V.3b 공용 트리 개정 (사용자 11-08) — 3분류, 이동기는 장비로

- 공용 트리는 **레벨당 1포인트**(V.3 규칙 유지)로 찍는 **포괄 성장**이다. 무기 숙련 트리(V.2)는 그 무기 종류 한정이고, 공용 트리는 무기와 무관하게 캐릭터 전반을 올린다.
- 트리 창에 **전투 / 생활 / 편의성** 3탭을 둔다(강화창처럼 한 창). 탭별 예시 노드(수치는 설계 단계에서 확정, 전부 임시):

| 탭 | 노드 예시 |
|---|---|
| 전투 | 치명타 확률·치명타 피해·방어도·최대 생명력·공격 속도 |
| 생활 | 채집 속도·채집 추가 드롭·낚시 성공률·요리·제작 성공률·제작 시간 |
| 편의성 | 최대 스태미나·스태미나 회복·이동 속도·자동 줍기 범위·물약 쿨 감소·상점 판매가·가방 확장 비용 할인 |

- **이동기(더블 점프·대시 강화 등)는 트리가 아니라 장비로 나중에 준다.** 트리에는 넣지 않는다.
- V.3 의 기존 7노드(C1~C7)는 전투·편의성 탭의 시작 노드로 재배치한다(C1·C2·C3·C5·C6 = 전투, C4·C7 = 편의성). 생활 탭 노드는 채집·낚시·요리·제작 시스템 훅이 확인되는 것부터 구현(없으면 후속).
- 합산 상한(V.3)은 탭 구분 없이 같은 스탯에 적용. 총 용량은 노드 확정 후 `SkillTreeRules.Capacity` 갱신(포인트 14~69 대비 28~50% 정도 채우는 규모 유지).
- 구현 영향: P4(공용 트리 효과)·P5(스킬창 UI)에 탭 구조와 생활·편의성 노드 반영. P0~P2 코드의 노드 표(`SkillTreeRules`)는 노드 추가·탭 필드 추가로 확장(기존 ES3 키 유지).

## V.4 초기화(리스펙)

| 규칙 | 값(임시) |
|---|---|
| 단위 | 트리별(타입별 각각 + 공용) 전체 초기화만. 노드 1단계 되돌리기는 없음 |
| 환불 | 투입 포인트 100% 환불. 숙련 EXP·Lv 손실 없음 |
| 비용 | 각 트리의 **첫 1회 무료**. 이후 골드 = `150 × 투입포인트 × min(누적 초기화 횟수, 5)`. (횟수는 트리별 `respec` 카운트) |
| 불가 조건 | 전투 중(`BlockReason` 유사 체크)·보스전은 창 자체를 못 연다 |
| 안전망 | 곡선·노드 개정으로 `spent > earned` 가 되면 로드 시 해당 트리를 무료 자동 초기화 |

골드 소모 API 는 기존 재화 코드(`Datas` 골드)를 builder 가 확인. 없으면 임시로 무료.

## V.5 개별 무기 숙련 비활성 (삭제 금지)

- 정적 플래그 `WeaponMasteryRules.PerWeaponMastery`(**기본 false**, 불 스테이지 개조에서 true 로). 상수가 아니라 `static bool` 이라 런타임·프로브로 토글 가능.
- **계속 동작(비활성 아님)**: 키별 EXP 누적(`Data.Add`/ES3 `weaponMastery` 저장). 이유: 개조 스테이지가 열릴 때 기존 진행도가 살아 있다.
- **플래그 false 시 우회**: ① `DamageBonus(key)` = 1(Lv5 +5% 무효, `Weapon.cs:114` 변경 없음), ② `AddForEquipped` 의 따라잡기 계산·`StartMasteryOf` 첫 획득 승급 건너뜀, ③ `LevelOf(key)`·`IsSlotUnlocked` 는 타입 Lv 를 본다(true 면 기존 키별 Lv).
- 새 함수 `WeaponMasteryService.EffectiveLevel(Weapon w)` 한 곳에서 분기(`PerWeaponMastery ? 키별 : 타입별`). `AttackManager`·`SkillCooldown`·툴팁은 이 함수만 호출하도록 교체(직접 `LevelOf(key)` 호출 금지).
- EditMode 테스트로 두 모드 모두 보호(플래그 true 에서 기존 16개 테스트 그대로 통과해야 한다).

## V.6 ES3·구세이브

신규 키만(기존 `weaponMastery`·`weaponMasteryMigrated`·`Datas` 불변):

| 키 | 타입 | 내용 |
|---|---|---|
| `typeMastery` | `TypeMasterySave{string[] kinds; int[] exps}` | 타입 이름 → 누적 EXP(Lv 비저장) |
| `skillTree` | `SkillTreeSave{string[] nodeIds; int[] levels; string[] respecTrees; int[] respecCounts}` | 노드 ID(`P1`..`C7`, 타입 노드는 `Sword.P1` 형식) → Lv, 트리별 초기화 횟수 |
| `typeMasteryMigrated` | bool | 이행 1회 플래그 |

- 이행(`typeMasteryMigrated` 없음): `weaponMastery` 가 있으면 각 무기 키 → `KindOf` → 타입별 **최대** EXP 로 `typeMastery` 시드(합산 아님: 복제·구세이브 Lv5 초기화가 타입에 중복 인플레하지 않게). `weaponMastery` 없음 + 세이브 있음(P1 이전 세이브)이면 보유 무기 타입을 Lv5(12000) 로 시드(기존 `LegacyStartExp` 재사용, 스킬 잠김 방지). 새 게임은 빈 상태. 포인트는 파생값이라 소급 지급된다.
- 로드 실패는 `GameLog.Warn` 후 빈 상태(기존 패턴).
- 저장 시점: 기존 `SaveManager.DataSave` 에 `TypeMasteryService.Save()`·`SkillTreeService.Save()` 한 줄씩(더티 플래그 패턴 동일).

## V.7 UI 최소안

| 요소 | 방법 |
|---|---|
| 스킬창 | 단축키 K(임시). 코드 생성 창 1장(`SkillTreeWindow`): 상단 탭 [공용][현재 무기 타입], 헤더에 타입명·칭호·Lv·EXP 바·남은 포인트·[초기화]. 본문은 노드 행(이름·Lv/최대·효과 한 줄·[+]) 목록. 선행 미충족 노드는 회색. 합산 상한 도달 줄에 "상한" 표시. 기존 UI 창 열기/닫기·입력 잠금 관례(인벤 창) 재사용 |
| HUD 슬롯 | `SkillCooldown` 잠금 표시 그대로(판정 레벨만 EffectiveLevel) |
| 툴팁 | `WeaponMasteryTooltip` 한 줄을 "검 숙련 Lv3 숙련자 (1200/3000)" 로 교체 |
| 토스트 | 기존 `LevelUp` 이벤트 경로 재사용: 타입 Lv 상승 "검 숙련 Lv4 · 포인트 +1". 포인트 미사용 시 스킬창 아이콘 점(후속) |

## V.8 구현 단계 P0~P7

| 단계 | 내용 | 검증 | 건드릴 파일 |
|---|---|---|---|
| P0 | `TypeMasteryRules`(곡선·칭호·포인트) + `SkillTreeRules`(노드표·선행·상한 clamp·리스펙 비용) 순수 클래스 | EditMode: 곡선 누적 152000·Lv 경계·칭호 5단계·포인트 Lv-1·선행/상한/비용 | 신규 `Script/Player/Mastery/TypeMasteryRules.cs`, `SkillTreeRules.cs`, `Editor/Tests/TypeMasteryRulesTests.cs`, `SkillTreeRulesTests.cs` |
| P1 | 타입 EXP 누적·ES3·이행·`PerWeaponMastery` 플래그·`EffectiveLevel` 교체 | 프로브: 검 A 로 처치→검 B 장착 시 같은 Lv 이어짐, 재시작 유지, 구세이브(weaponMastery 만 있는 ES3) 이행 값, 플래그 true 시 기존 16 테스트 통과, 플래그 false 에서 Lv5 보너스 1.0 | 신규 `TypeMasteryData.cs`·`TypeMasteryService.cs`; 수정 `WeaponMasteryService.cs`(분기), `WeaponMasteryRules.cs`(플래그), `AttackManager.cs`·`SkillCooldown.cs`·`WeaponMasteryTooltip.cs`(EffectiveLevel), `SaveManager.cs`(Load/Save 한 줄) |
| P2 | 트리 상태·투자/환불/리스펙·집계기 `SkillTreeService.Mod(stat, kind)` | 프로브: 포인트 소모·선행 거부·리스펙 환불/비용·로드 후 유지·`spent>earned` 자동 초기화 | 신규 `SkillTreeData.cs`·`SkillTreeService.cs`; `SaveManager.cs` |
| P3 | 타입 트리 효과 연결(데미지·치명·공속·쿨·스태미나·극의 1~2종 먼저) | 프로브: P1 레벨별 평타 데미지 숫자 비교(검 Lv2 P1=3 → +6%), 타입 불일치 무기 0 적용, 상한 clamp | `Weapon.cs`, `Skill.cs`, `AttackManager.cs`, `WeaponGimmick.cs`(극의) |
| P4 | 공용 트리 효과(이동·HP·방어·스태미나 회복·공속·치명) | 프로브: 노드별 최종 스탯 변화 로그 | `PlayerController.cs`, `PlayerHealthManager.cs` |
| P5 | 스킬창 UI | 플레이 스크린샷(낮 시간대): 탭 전환·+ 클릭·초기화·상한 표시 | 신규 `UI/SkillTreeWindow.cs`; 입력 등록(K) |
| P6 | 툴팁·토스트·Loc 문구 | 스크린샷 + Loc 누락 점검 | `WeaponMasteryTooltip.cs`, Loc CSV |
| P7 | 수치 조정·극의 전 타입·문서 갱신 | 1층계 플레이 후 곡선·포인트 체감 | 문서, 노드표 |

**진행 상태(11-07)**

| 단계 | 상태 | 비고 |
|---|---|---|
| P0 | 완료·커밋 | EditMode 938 통과. 곡선 누적 152000, 노드 15개(타입 8·공용 7) |
| P1 | 완료(프로브 검증) | `TypeMasteryService`·`PerWeaponMastery=false`·`EffectiveLevel`·구세이브 이행(`Tools/probes/wskill_v2.cs`). 이행은 DB 미로드면 보류 후 첫 사용 때 실행, 보류 중 해금 열림·저장 차단 |
| P2 | 완료(프로브 검증) | `SkillTreeService`(투자·선행 거부·리스펙·`Mod`·`spent>earned` 자동 초기화). 골드 = `DatabaseManager.money`. 전투 중 차단은 UI(P5)에서 |
| P3~P7 | 미착수 | |

P0~P2 는 플레이 영향 없음(안전). P1 은 해금 판정 기준이 바뀌므로 반드시 구세이브 이행 프로브 후 커밋.

## V.9 위험·함정

- 타입 EXP 시드가 "키별 최대"라 구세이브 이행 후 일부 타입이 Lv5 로 시작 — 의도(스킬 잠김 방지). 트리 포인트 4 가 소급 지급되는 것도 허용.
- `KindOf` 가 DB 미로드 시 None 을 돌려준다(캐시 안 함): None 타입은 EXP·트리 모두 무시하고 해금은 열림 처리(기존 방침).
- 공속·방어 스탯 훅은 코드에 이름 확정이 안 된 곳이 있다(builder 가 grep, 없으면 P4 에서 해당 노드를 후속으로 미룬다).
- 노드 ID·타입 이름은 세이브 키이므로 개정 시 추가만 한다(삭제 노드는 로드 시 무시 후 환불).
