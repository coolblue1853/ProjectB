# 무기 스킬 순차 해금·성장 설계 (WSKILL-1)

> 상태: 설계안(2026-11-06). **코드 수정 없음, 구현은 사용자 확인 후.** 임시 가정은 `Docs/OpenQuestions.md` WSKILL 항목.
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

| 단계 | 내용 | 검증 |
|---|---|---|
| P0 | `WeaponMasteryRules` + 테스트(곡선·해금표) | EditMode 테스트 통과 |
| P1 | `Data`·`Service`·ES3 + `LevelSystem` 연결(해금 **적용 안 함**, 숙련만 누적) | 프로브: 몹 처치 후 장착 무기 숙련 EXP 증가, 재시작 후 유지 |
| P2 | 슬롯 잠금(`AttackManager`·`Skill.BlockReason`) + 구세이브 마이그레이션 | 프로브: Lv1 무기에서 2번 슬롯 입력 거부·안내, 레벨 오르면 사용 |
| P3 | HUD 잠금 표시 + 인벤 툴팁 + 토스트 | 플레이 스크린샷(낮 시간대) |
| P4 | Lv5 보너스·따라잡기·StartMastery + 수치 조정 | 플레이 후 곡선 재조정 |
| P5(선택) | 간이 스킬 창, 숙련 그룹 | - |

P1 까지는 플레이 영향이 없으므로 안전하게 먼저 넣을 수 있다.

## 10. 임시 가정 요약
`Docs/OpenQuestions.md` WSKILL 항목 참고: B 채택, 키별 숙련, Lv 1~5, 곡선 400/1100/3000/7500, 등급 계수, Lv5 보너스 +5%, 구세이브 Lv3 초기화, 잠금 시 소모 없음.
