# 장비 특수 효과·세트 효과 설계 (REWORK-12, 11-09)

> 근거: 사용자 11-08 — 게임 핵심 즐거움 = 장비 파밍 → 무기 스킬 강화 → **스킬 로직 자체 변경(디아블로식, 제한 없음)**. 앞으로 만드는 모든 무기·방어구·악세서리는 등급이 높을수록 특이한 효과를 가진다.
> 상태: P0~P8 구현 완료(신규 장신구 제외)(11-09~11; P4 = Chain·OnKillCast·Proc·CooldownRefund, P2 = effectIds 로드·피해/범위/지속/탄속/스태미나 훅, P3 = Rule 1차 FireMode·Homing·Pierce/Return·OnHitSpawn(Explode/Slow) + PierceAdd·SummonAdd 훅), 신규 장신구·팔찌 미착수. 수치는 전부 임시 가정(`Docs/OpenQuestions.md` Q-EFX1~6). 읽은 것: `LineageGear.md`, `ItemSpec.md` §1~3, `EquipSlotExpansion.md`, `Floor1Completion.md` §5, `WeaponSkillProgression.md` V.2~V.3b, 코드(아래 §1).
> 공통 규칙: 모든 UI는 마우스·키보드·게임패드 3경로(§7). 기본 3타·스미어 v7 은 절대 건드리지 않는다.

## 1. 현황 진단

### 1.1 효과 종류별 구현 여부

| 효과 종류 | 구현 | 위치(시그니처) | 한계 |
|---|---|---|---|
| 정수 스탯(Def·Crit·CritDmg·IncDmg·BasicDmg·Poison·BleedDmg·AtkSpeed·MoveSpeed·DropRate) | 완료 | `SetEffectApplier.Apply(string effect, bool apply, string group)` → `PlayerStats.Add(StatModifier)`(그룹 일괄 해제) | 세트만. 일부는 레거시 `DatabaseManager.*` 직접 가산 |
| HP | 완료 | `SetEffectApplier.ApplyEntry` → `PlayerHealthManager.EquipmentActiveTrue/False` | 모디파이어 그룹 밖 |
| 스킬 쿨감(%, 스킬 이름별) | 완료 | 저장: `DatabaseManager.skillCoolDown`(Dictionary<string,int>), 방어구 프리팹 `Equipment.coolDownSkill[]`(`EquipBoxCheck.cs` ~174~193), 세트 `coolDown`. **사용: `SkillCooldown.cs:196-201`** `cooldownTime*(1-p/100f)` | 상한 없음, 합산 한 줄. 스킬 이름 키만(태그 없음), 출처 구분 불가 |
| 스킬 타격 수 +N | 완료 | `DatabaseManager.skillHitCount`, 사용 `Skill.cs:192`(`allObjectMaxCount`) | 이름 키만 |
| 스킬 탄 수 +N | 완료 | `DatabaseManager.skillBulletCount`, 사용 `Skill.cs:249-254`(`allBullet`) | 이름 키만 |
| 스킬 범위·투사체 속도·지속시간·데미지 배율·스태미나 소모·관통·유도·분열 | **없음** | - | 훅 신설 필요(§2.4) |
| 스킬 규칙 대체(발사 방식·대상·판정·연쇄) | **없음**(스킬별로 프리팹 복제/개조로만 가능) | `Skill.ActiveSkill(int)`, `Skill.RoundSpawn(GameObject, GameObject)`, `Skill.CanActivate`, `Skill.BlockReason`, `OnHoldRelease(int)` | 장비가 스킬 로직을 바꾸는 경로 없음 |
| 무기 고유 기믹 | 완료(타입별 6종) | `WeaponGimmick`(`OnAttackStart(Weapon,int)`, `OnHitboxSpawned(Weapon,int,DamageObject)`, `HitMultiplier(DamageObject,EnemyHealth,Collider2D)`, `IHitModifier`), 프리팹 컴포넌트 | 평타 중심, 스킬 불관여, 프리팹 하드와이어 |
| 무기 몹 스킬 부가(흡혈·처치 소환·반격) | 완료 | `LifeStealHit`, `KillSummonProc`, `CounterBuff`, `ShieldGuard`(무기 프리팹 컴포넌트) | 아이템 데이터로 켜고 끌 수 없음(프리팹 고정) |
| 세트 판정 | 완료 | `SetBonusEvaluator`(`CountPieces`·`Diff`), `SetItem.CheckSetEffect(string name,int level)`/`CheckSetDisEffect`, `SetSwapPreview`(교체 미리보기). CSV `SetItem.csv`(키 `{tfName}_{수}`, 2열 효과 문자열) | 문자열 문법 = 스탯 + 스킬 3종뿐. 기본 8칸만 판정(신규 4칸 제외) |
| 비외형 장신구 | 스탯만 | `ExtraEquip`(`TryHandleEquip(ItemCheck)`·`Unequip`·`ApplyStats()`), `ExtraEquipStats.csv`(critical,criticalDmg,attSpeed,moveSpeed,dropRate,def,incDmg) | 스킬 효과 불가 |
| 기믹 장신구 | 이름 하드코딩 2종 | `AccessoryGimmick.IsEquipped(string name)`, `PlunderMedal`·`LastStandRing` 상수 | 아이템마다 코드 추가 필요 |
| 아티팩트 | 액티브 4종 고정 | `ArtifactSystem.Defs`(HealingDew·GuardianShell·Hourglass·StarBurst, 쿨 고정), `TryUseNow()` | 장비 효과와 무관, 쿨 수정 불가 |
| 이동기 | 별개 | `Player/Teleport.cs`(텔레포트 스킬) | 쿨감 훅 없음 → 태그 `Dash`로 편입 |
| 숙련·공용 트리 | 설계 완료/P2까지 | `SkillTreeService.Mod(stat, kind)`(WSKILL-V2 P2), 상한 `SkillTreeRules.Cap(stat)` | P3 이후 미연결 → 본 설계와 같은 훅에 합류 |

### 1.2 진단 결론

1. 스킬 수정 훅은 **이름 키 3종(쿨·타격 수·탄 수)** 뿐이고 출처·상한·조건이 없다. 효과는 "세트 4/6 쿨감" 위주라 체감이 단조롭다(LineageGear §4: 세트 쿨감 11/12 세트가 같은 패턴).
2. 스킬 규칙을 바꾸는 장비 효과는 0. 무기 기믹·`KillSummonProc` 등은 프리팹 고정이라 파밍으로 "다른 스킬 방식"을 얻을 수 없다.
3. 등급(`rarity` C/R/U)과 효과량이 연결돼 있지 않다. 유니크도 세트 밖 특색 무기 한 개일 뿐.
4. 새 시스템은 레거시를 **대체하지 않고 흡수**한다: 기존 3종 Dictionary·세트 문자열 문법·`Equipment.coolDownSkill[]`은 그대로 동작, 새 서비스가 최종 질의 한 곳에서 합산·상한을 건다.

## 2. 효과 프레임워크

### 2.1 개념

- **효과(Effect)** = id 하나. 한 효과는 1개 이상의 **행(Entry)** 을 가진다. 행은 `Modifier`(수치 조정) 또는 `Rule`(규칙 대체) 중 하나.
- 효과를 가진 **출처(Source)**: 아이템(무기·방어구·장신구, `ItemSheet` 컬럼), 세트 단계(`SetItem.csv`), 아티팩트(후속). 모두 같은 효과 id 를 참조한다 → 세트 2/4/6, 악세서리, 무기 고유 효과가 같은 코드 경로.
- **스킬 선택자(Selector)**: 효과가 어떤 스킬에 적용되는지. 스킬 이름(`Skill.skillName`, 공백 → `_`), 태그(`tag:Projectile`), 무기 종류(`kind:Staff`), `all`. 쉼표 불가(CSV)이므로 `|`로 OR.
- **스킬 태그**: 신규 `SkillTags.csv`(`skillName,tags`, 프리팹 무수정). 태그 어휘(초안): `Melee` `Projectile` `Summon` `Ground` `Dash` `Buff` `Hold` `Magic` `AoE` `Basic`(평타). 무기 종류는 `Weapon._kind` 에서 자동. 태그 미등록 스킬은 `kind:` 와 이름 선택자만 가능(오류 아님).

### 2.2 Modifier 타입

| 필드 | 타입 | 설명 |
|---|---|---|
| id | string | 효과 id (`EF_` 접두 권장). 불변(세이브·툴팁 키) |
| entry | int | 효과 내 행 번호(1~) |
| kind | enum | `Mod` |
| selector | string | §2.1 (`Scattering_Leaves`, `tag:Dash`, `kind:Staff`, `all`) |
| op | enum | 아래 표 |
| value | float | 퍼센트 op 는 % 단위 정수·실수(`/100f` 로 계산, B-101), 개수 op 는 정수 |
| cond | string | 조건(§2.5), 빈칸 = 항상 |
| group | string | 중복 방지 그룹. 같은 group 은 출처가 달라도 **가장 큰 값 1개만** 적용(스택 방지). 빈칸 = 합산 |

| op | 의미 | 훅 위치(신설/기존) | 단위 |
|---|---|---|---|
| `CooldownPct` | 쿨 -% | 기존 `SkillCooldown.cs:196` 를 서비스 질의로 교체 | % |
| `DamagePct` | 스킬 피해 +% (곱) | `Skill` 이 `DamageObject` 생성 직후(`RoundSpawn`/`ActiveMainSkill` 판정 생성부) 피해 배율에 곱 | % |
| `RangePct` | 판정 크기·사거리 +% | 판정 오브젝트 `localScale`·탄 수명 | % |
| `ProjSpeedPct` | 탄 속도 +% | 탄 컴포넌트(`StraightArrow`·`HomingOrb` 등) 속도 | % |
| `DurationPct` | 지속(장판·버프·소환수) +% | `SummonRunner`/버프 시간 | % |
| `ProjectileAdd` | 탄 +N | 기존 `Skill.cs:249-254` 를 서비스 질의로 교체 | 개 |
| `HitCountAdd` | 타격 수 +N | 기존 `Skill.cs:192` 를 서비스 질의로 교체 | 회 |
| `PierceAdd` | 관통 +N | 탄 컴포넌트 | 회 |
| `SummonAdd` | 소환 수 +N | `SummonRunner`/`SummonSkill`(LineageGear §4-4 해결) | 마리 |
| `StaminaPct` | 스태미나 소모 -% | `Skill` 스태미나 차감부(builder grep) | % |
| `ProcChance` | 확률 효과 가산(Rule `Proc` 의 chance) | 서비스 | %p |
| `CastCharge` | 충전 +N(쿨 전 연속 사용) | `SkillCooldown` 타이머 | 회 |

### 2.3 Rule 타입(스킬 규칙 대체) — 디아블로식

Rule 은 **코드에 등록된 핸들러**를 데이터가 고르고 파라미터를 채우는 방식이다(임의 스크립트 금지: 검증·밸런스·세이브 안전). 새 변형이 필요하면 핸들러 1개 + CSV 행.

| 필드 | 타입 | 설명 |
|---|---|---|
| id / entry / selector / cond / group | | Modifier 와 동일 |
| kind | enum | `Rule` |
| rule | enum | 아래 표 |
| p1~p3 | string | 핸들러 파라미터(숫자·스킬 이름·비율) |

| rule | 분류 | 동작 | 파라미터 | 구현 방식 |
|---|---|---|---|---|
| `FireMode` | 발사 방식 | 탄 배치 변경: `Fan`(부채꼴)·`Ring`(원형)·`Parallel`(평행)·`Backward`(뒤로 같이) | p1=모드, p2=각도/간격 | `Skill.RoundSpawn` 의 탄 생성 루프를 `FirePattern` 전략 객체로 위임(기본 = 기존 동작) |
| `Homing` | 대상 | 직선탄 → 가장 가까운 적 유도 | p1=회전속도 | 탄에 `HomingOrb` 계열 컴포넌트 추가 |
| `Retarget` | 대상 | 시전 대상을 `Nearest`/`LowestHp`/`AllInRange` 로 변경 | p1=대상 | 탄 방향·판정 위치 결정부 |
| `OnHitSpawn` | 판정 | 적중 시 작은 폭발·장판·상태이상 부여 | p1=종류(`Explode`·`Bleed`·`Poison`·`Slow`), p2=피해%, p3=범위 | `DamageObject` 적중 이벤트 훅(기존 `IHitModifier` 재사용) |
| `Pierce` / `Return` | 판정 | 관통 / 사거리 끝에서 되돌아옴(부메랑) | p1=횟수 | 탄 컴포넌트 |
| `Chain` | 연쇄 | 적중 시 근처 적 N명에게 같은 판정 전이 | p1=N, p2=거리, p3=감쇠% | 적중 훅 + `Physics2D.OverlapCircle` NonAlloc |
| `OnKillCast` | 연쇄 | 처치 시 지정 스킬 자동 시전(비용·쿨 없음) | p1=스킬 이름, p2=피해% | 적 사망 이벤트(`KillSummonProc` 방식 일반화) |
| `Proc` | 연쇄 | 시전 시 확률로 다른 스킬 추가 시전 | p1=스킬, p2=확률%, p3=내부 쿨(초) | `Skill.ActiveSkill` 직후 |
| `CooldownRefund` | 연쇄 | 조건(처치·적중·크리)마다 쿨 -초 | p1=초, p2=트리거 | `SkillCooldown` |
| `Replace` | 변형 | 스킬 프리팹을 변형판으로 교체(예: 직선 → 차징 광선) | p1=변형 프리팹 키 | `Resources/Weapon/Variants/{p1}`; 공유 프리팹 직접 수정 금지(LineageGear §0) |

- **변형 모양 허용 범위: 제한 없음**(사용자 11-08). 단 핸들러 화이트리스트 + 충돌 규칙(§2.6) + 합산 상한(§2.7)으로 밸런스를 지킨다.
- 신규 핸들러는 인터페이스 `ISkillRule { void OnCast(SkillCtx c); void OnSpawn(SkillCtx c, GameObject proj); void OnHit(SkillCtx c, DamageObject d, EnemyHealth e); void OnKill(SkillCtx c, EnemyHealth e); }` 중 필요한 것만 구현(기본 no-op 추상 클래스 `SkillRuleBase`).

### 2.4 런타임 구조(신규 파일)

| 파일 | 클래스·시그니처 | 역할 |
|---|---|---|
| `Script/Combat/Effects/EffectDef.cs` | `sealed class EffectDef { string id; List<EffectEntry> entries; string nameKey; string descKey; Grade minGrade; }`, `EffectEntry`(§2.2·2.3 필드) | 순수 데이터 |
| `Script/Combat/Effects/EffectTable.cs` | `static EffectTable { Load(); Get(string id); static List<EffectEntry> ParseEntries(string csv) }` | `Resources/EquipEffects.csv` 로드(순수 파서 → EditMode 테스트) |
| `Script/Combat/Effects/SkillEffectService.cs` | `static class SkillEffectService { void Attach(string sourceKey, IEnumerable<string> effectIds); void Detach(string sourceKey); void Recompute(); SkillMods Query(string skillName, WeaponKind kind); }` | 활성 출처 → 합산·상한·충돌 해소 → 스킬별 `SkillMods` 캐시. 장착/해제/세트 변동 때만 재계산(Update 할당 금지) |
| `.../SkillMods.cs` | `struct SkillMods { float cooldownPct, damagePct, rangePct, projSpeedPct, durationPct, staminaPct; int projectileAdd, hitCountAdd, pierceAdd, summonAdd, castCharge; ISkillRule[] rules; }` | 질의 결과(캐시 재사용, 매 시전 `new` 금지) |
| `.../Rules/*.cs` | `FireModeRule`, `HomingRule`, `ChainRule`, `OnKillCastRule`, `ProcRule`, `OnHitSpawnRule`, `ReplaceRule` ... | 핸들러(P3~P5 점진) |
| `.../EffectDescriber.cs` | `static string Describe(EffectEntry e)` / `DescribeEffect(string id)` | §2.8 자동 설명 |
| `.../SkillTagTable.cs` | `static bool HasTag(string skillName, string tag)` | `SkillTags.csv` |

연결점(기존 코드 수정 최소):
- `SetEffectApplier.Apply` 끝에서, 세트 행의 `effectIds` 열이 있으면 `SkillEffectService.Attach("Set:"+key, ids)` / 해제 시 `Detach`. 문자열 문법 파서는 **무변경**(레거시 유지).
- 아이템 장착/해제 지점: 방어구·무기 `EquipBoxCheck`(쿨 `coolDownSkill[]` 가산부 근처), 신규 4칸 `ExtraEquip.TryHandleEquip/Unequip/ApplyStats`, 무기 교체 → 각 지점에서 `Attach("Item:"+slot, itemEffectIds)`/`Detach`. 로드 직후 `Recompute()` 한 번.
- 질의 교체 3곳: `SkillCooldown.cs:196-201`(쿨), `Skill.cs:192`(타격), `Skill.cs:249-254`(탄). 레거시 Dictionary 값은 `Query` 안에서 장비 버킷 `cooldownPct`/`hitCountAdd`/`projectileAdd` 에 **더해** 같은 상한을 통과시킨다.
- 신규 훅: `Skill` 판정 생성부(피해·크기 배율), 탄 컴포넌트(속도·관통·유도), 적 사망 이벤트(`OnKill`), `Skill.ActiveSkill` 직후(`Proc`·`OnCast`), 스태미나 차감부.
- 평타(`Basic` 태그): 같은 서비스로 질의하되 **P6 이후**(기본 3타 보존 원칙, `Weapon.SetDmgArray` 곱에 `damagePct` 한 줄만).

### 2.5 조건(cond) 어휘

`Night`·`Day`(`TimeOfDay`), `HpBelow:30`·`HpAbove:80`(% HP), `Airborne`, `Crouch`, `Moving`, `FirstHit`(스킬 첫 타), `Crit`(치명타 직후), `KillWithin:3`(최근 3초 처치), `Boss`(대상이 보스). 조건은 **질의 시점**에 평가(캐시는 조건 없는 것만, 조건 있는 항목은 `SkillMods` 에 별도 리스트로 시전 때 평가). 1차 구현은 `Night/Day/HpBelow/HpAbove/KillWithin` 5종.

### 2.6 충돌 규칙

1. 같은 `group` : 가장 큰 값 1개만 적용(예: `cd_haste` 그룹이면 세트·무기에서 같은 종류가 와도 중복 금지).
2. Rule 슬롯 제한(스킬 하나당): `FireMode` 1개, `Retarget`/`Homing` 합쳐 1개, `Replace` 1개, `Chain`·`Proc`·`OnHitSpawn`·`OnKillCast` 합쳐 2개.
3. 초과 시 **우선순위**: `grade` 높은 쪽 > 같은 등급이면 장착 순서 마지막. 탈락한 Rule 은 툴팁에서 회색 "(비활성: 같은 종류 효과가 이미 적용됨)".
4. `Replace` 가 있으면 그 변형 프리팹이 `FireMode` 를 이미 포함할 수 있어 `FireMode` 는 무시(변형이 우선). 로그 `GameLog.Info(Cat.Item, ...)` 1줄.
5. 서로 다른 스킬을 가리키는 효과는 충돌 없음. 선택자가 겹치면(이름 + 태그) 둘 다 해당 스킬에 적용(위 규칙으로 다시 정리).
6. Rule 을 가진 효과 출처가 해제되면 즉시 `Recompute`; 진행 중이던 탄·장판은 그대로 끝까지(상태 꼬임 방지).

### 2.7 합산·상한

계산 순서(스킬 하나, 쿨 예): `최종 쿨 = 기본 쿨 × (1 − min(장비합, 60)/100) × (1 − 트리합/100)`, 하한 0.5초. 트리 합은 `SkillTreeRules.Cap`(-15%, V.3 합산 상한)이 별도 적용 → **장비 버킷과 트리 버킷은 곱연산**으로 만나므로 어느 한쪽이 상한을 우회해 폭주하지 않는다(WSKILL V.3 "장비 수치는 상한과 별개" 방침과 일치).

| op | 장비 버킷 합산 상한(스킬 1개당, 임시) | 트리 버킷 상한(기존 V.3) | 결합 |
|---|---|---|---|
| CooldownPct | 60% | 15% | 곱 |
| DamagePct | +100% | 평타·스킬 합 +30% | 곱 |
| RangePct | +50% | - | - |
| ProjSpeedPct | +60% | - | - |
| DurationPct | +100% | - | - |
| StaminaPct | -50% | -20% | 곱 |
| ProjectileAdd | +6 또는 기본의 +100% 중 작은 값 | - | - |
| HitCountAdd | +4 | - | - |
| PierceAdd | +3 | - | - |
| SummonAdd | +3 | - | - |
| Proc 확률 합 | 40%p | - | - |
| CastCharge | +2 | - | - |

- 상한 초과분은 버리되 툴팁·세트 창에 "상한" 표시(트리 UI와 동일 표기). 레거시 Dictionary 값은 장비 버킷에 포함.
- 현행 최댓값 점검: Leaf_5 `Haste` -50, Wolf_4 `Howl` -40 + 방어구 프리팹 `coolDownSkill[]` 가산이 겹치면 60% 상한에 걸릴 수 있다 → P0 프로브에서 현행 모든 세트 조합의 스킬별 합을 덤프해 상한 밑인지 확인(§5).

### 2.8 CSV 컬럼안(기존 컬럼 뒤에만 추가)

| 파일 | 추가 | 설명 |
|---|---|---|
| `ItemSheet.csv` | 컬럼 18 `effectIds`(string) | 효과 id 를 `;` 로 연결(최대 3). 빈칸 = 효과 없음. 무기·방어구·장신구 공통. `ItemSpec.md` §1 표 갱신 |
| `SetItem.csv` | 3열 `effectIds`(string) | 기존 `{tfName}_{수}, effect` 뒤. 효과 문자열(스탯) 과 병행. 비면 기존 동작 |
| `EquipEffects.csv`(신규) | `id,entry,kind,selector,op,value,cond,group,rule,p1,p2,p3,nameKey,descKey,minGrade` | Mod 행은 `rule/p*` 빈칸, Rule 행은 `op/value` 빈칸. 쉼표 금지(`\|`·`;` 사용) |
| `SkillTags.csv`(신규) | `skillName,tags` | tags 는 `;` 구분 |
| `EquipEffectLoc`(Loc 키) | `Efx.<id>.name`, `Efx.<id>.desc`(Rule 전용 선택) | Mod 행은 자동 설명(§2.9)이라 desc 불필요 |

- `DataIntegrityTests` 확장: ① `effectIds` 가 `EquipEffects.csv` 에 존재 ② selector 의 스킬 이름이 실제 `Skill.skillName`/`SkillTags` 에 존재 ③ Rule 의 `rule` 이 핸들러 등록표에 존재 ④ 등급 규칙(§3) 위반 경고 ⑤ 같은 id 의 entry 번호 연속.
- 컬럼은 맨 뒤에만 추가. `ItemSheet` 파서가 헤더 이름 기반인지 열 번호 기반인지 builder 가 먼저 확인(함정).

### 2.9 툴팁·세트 창 자동 설명

- `EffectDescriber` 가 행에서 문장을 생성. 템플릿(Loc): `Efx.op.CooldownPct` = "{target} 쿨다운 -{v}%", `DamagePct` = "{target} 피해 +{v}%", `ProjectileAdd` = "{target} 탄 +{v}", … Rule 은 `Efx.rule.<rule>` 템플릿 + 파라미터("부채꼴 {p2}도로 발사", "적중 시 최대 {p1}명에게 연쇄"), 복잡한 변형은 `descKey` 수동 문구가 우선.
- `{target}` : 이름 선택자면 스킬 표시 이름(한글화 Loc), 태그면 "투사체 스킬" 같은 태그 이름, `all` = "모든 스킬".
- 현재값 병기: 세트 창·툴팁에 "Haste 쿨다운 10.0초 → 5.0초" 처럼 **현재 합산 결과 미리보기**(`Query` 재사용). 상한 도달·비활성 Rule 은 회색.
- 기존 `TooltipBuilder`(`Cooldown X` 줄 생성부 ~198, 세트 문자열 해석 ~268-284)에 효과 줄을 추가(레거시 줄 유지). 효과가 있는 아이템은 상단에 "특수 효과" 구획과 등급색 좌측 막대.

## 3. 등급별 효과 개수·강도

등급 코드: 기존 `C`(Common)·`R`(Rare)·`U`(Unique) + **`L`(Legendary) 신설**(Gatekeeper's Oath 가 "U 표시"였던 사유, LineageGear §3). 언커먼은 만들지 않는다(데이터 이행 비용 회피). 큐 문구 "언커먼 이상"은 R 로 대응(Q-EFX1).

### 3.1 효과 슬롯 표(아이템 1개)

| 등급 | 효과 슬롯 | 허용 내용 | 강도 점수 예산(§3.2) | 예 |
|---|---|---|---|---|
| C | 0 | 스탯만. 세트 키가 있으면 세트 효과만 | 0 | 녹슨 검 |
| R | 1 | Mod 1행(소) 또는 상시 Mod 1~2행 | 10 | 잎사귀 스태프: Scattering Leaves 탄 +2 |
| U | 2 | Mod 1~2행(중) + **Rule(소형)** 1개(`Chain`·`Proc`·`OnHitSpawn`·`CooldownRefund`) | 24 | 보스 유니크: 처치 시 소환 |
| L | 3 | Mod(대) + **Rule(변형급)** 1개(`FireMode`·`Homing`·`Replace`·`OnKillCast`) + 고유 보너스 1개 | 45 | Gatekeeper's Oath |

- 방어구·장신구도 같은 슬롯 표. 방어구 R 은 "세트 시너지 효과"(해당 세트 스킬 한정 Mod) 우선.
- 세트 효과는 아이템 슬롯과 **별도**: 세트 단계는 §3.3.
- 장신구(귀걸이·팔찌·부적·마법석)는 스탯 1~2줄(EquipSlotExpansion §3) + R 부터 이 표의 효과. 팔찌는 쿨감·스킬 피해, 부적은 `CooldownRefund`·`OnKillCast`, 귀걸이는 `Crit` 조건 효과, 마법석은 `OnHitSpawn`(상태이상) 성향.

### 3.2 강도 점수(밸런스 도구)

| op/rule | 점수 단위 |
|---|---|
| CooldownPct | 0.5점/% (예 -20% = 10점) |
| DamagePct | 0.4점/% |
| RangePct·ProjSpeedPct·DurationPct | 0.2점/% |
| StaminaPct | 0.15점/% |
| ProjectileAdd·HitCountAdd·PierceAdd·SummonAdd | 4점/개 |
| CastCharge | 8점/회 |
| 조건 있음 | 점수 ×0.6 (조건부는 더 강하게 가능) |
| Rule 소형(`Chain`·`Proc`·`OnHitSpawn`·`CooldownRefund`) | 10~14점 |
| Rule 변형급(`FireMode`·`Homing`·`Retarget`·`Replace`·`OnKillCast`) | 18~30점 |

DataIntegrity 가 아이템별 합계가 등급 예산의 ±30% 안인지 경고(실패 아님, 수동 판정 보조).

### 3.3 세트 단계 강도(방어구 5부위 + 계보 무기)

| 단계 | 내용 | 예산 |
|---|---|---|
| 2 | 스탯(몹 특성, 기존 유지) | 0 |
| 3(선택) | 스킬 Mod 1행 | 10 |
| 4 | 계보 무기 스킬 Mod 2~3행(쿨감 + 수치 1종) | 22 |
| 5 또는 6 | **Rule 1개(변형급)** + 스탯 | 28 |

- 스탯 인플레 금지 원칙 유지(기존 2단계 수치·`Floor1Balance` §8 보존). 세트 4단계의 순수 쿨감 4종 패턴은 **쿨감 1 + 다른 op 1 이상**으로 바꾼다.

### 3.4 신규 아이템 제작 체크리스트(`Floor1Completion`/무기 추가 절차에 합류)

1. 등급 결정 → §3.1 슬롯 수 충족? (`특이 효과: 있음/없음(등급 기준 충족)` 기록)
2. `EquipEffects.csv` 행 + `effectIds` 입력, 강도 점수 합계 확인
3. 선택자 스킬이 해당 무기·세트에 실제로 있는지(`LineageAudit` 방식 대조)
4. Loc 이름·설명 키, 툴팁 스크린샷
5. 프로브: 장착 전후 `SkillEffectService.Query` 수치 로그
6. 기존 아이템 재점검: 등급 대비 효과 없는 아이템 목록 생성 도구(P7)

## 4. 1층계 재설계 초안

선정 6세트: Leaf(샛길)·Wolf(일반 숲 낮)·Wisp(일반 숲 밤)·Grove(깊은 숲 낮)·Bog(늪 낮)·Stone(탑 근교). 나머지(Rabbit·Rat·Scare·Fae·Gloom·Outlaw·Raider·Cliff)는 §4.3 방향만 정하고 후속 큐. 기존 2단계 스탯과 4단계 쿨감 수치는 유지하고 **굵은 글씨 = 새로 추가**한다. 스킬 이름은 `SetItem.csv`·`LineageGear` §7 표 기준.

### 4.1 세트 효과 초안

| 세트 | 2 | 4 (Mod 중심) | 5/6 (Rule 변형급) |
|---|---|---|---|
| **Leaf**(2/5) | 기존: Rising Roots 타격 +1, Scattering Leaves 탄 +2 | (4 없음) | 5: 기존 Rising Roots 타격 +2·Haste 쿨 -50 + **Rule `FireMode` Fan(Scattering Leaves 부채꼴 60도) + `Return`(잎이 사거리 끝에서 되돌아옴 1회)** |
| **Wolf** | 기존 이속 8·치명 8·증뎀 3 | 기존 Howl -40·Boar_Charge -30 + **Howl_of_the_Pack `RangePct` +30, `DurationPct` +30** | 6: 기존 공속 8·HP 18 + **Rule `CooldownRefund`(처치 시 Howl 쿨 -1.5초, 내부 쿨 0.5초) + `Pierce`(Boar_Charge 관통)** |
| **Wisp** | 기존 방어 6·HP 20 | 기존 Ghostlight_Ring -40·Shell_Guard -30 + **Wisp_Bolt `ProjectileAdd` +1** | 6: 기존 방어 8·치피 15·Wisp_Bolt 쿨 -40 + **Rule `Homing`(Wisp_Bolt 유도) + `OnHitSpawn` Slow(적중 시 1초 둔화 30%)** |
| **Grove** | 기존 방어 4·HP 8 | 기존 Ancient_Roots -40·Mauling_Rush -30 + **Ancient_Roots `HitCountAdd` +1, `RangePct` +25** | 6: 기존 공속 6·치피 20·HP 20 + **Rule `OnKillCast`(처치 지점에 Ancient_Roots 자동 발동 50% 피해, 내부 쿨 3초)** |
| **Bog** | 기존 이속 6·방어 4 | 기존 Tongue_Lash·Jaw_Shake·Pincer_Flurry -30 + **Tongue_Lash `RangePct` +25** | 6: 기존 공속 6·출혈피해 15·증뎀 2 + **Rule `Chain`(Tongue_Lash 적중 시 근처 적 2명에게 당기기 전이, 감쇠 30%) + `OnHitSpawn` Bleed** |
| **Stone** | 기존 방어 6·HP 16 | 기존 Rolling_Charge·Aftershock·Boulder_Toss -30·증뎀 8 + **Boulder_Toss `DamagePct` +20** | 6: 기존 방어 8·HP 28·증뎀 10 + **Rule `OnHitSpawn` Explode(Boulder_Toss 착지 충격파 60% 피해) + `Proc`(Quake Slam 12% 확률로 Aftershock 추가 시전)** |

- 점수 확인 예: Wolf 6 = Cooldown Refund 12 + Pierce 10 ≈ 22점(예산 28 이내). Wisp 4 = ProjectileAdd 4 + (쿨감은 기존 레거시, 별도).
- 쿨감이 레거시 경로인 경우 점수 계산 제외(기존 값 보존), 신규 행만 점수.

### 4.2 계보 무기·장신구 효과 초안(등급은 현 `ItemSheet` 기준 builder 확인, 아래는 목표 등급)

| 아이템 | 목표 등급 | 효과 | 유형 |
|---|---|---|---|
| **잎사귀 스태프**(LeafStaff, 상점) | R | Scattering Leaves 탄 +2, 치명 시 잎사귀 1장 추가(`ProjectileAdd`, cond `Crit`) | Mod |
| 루트 스태프(RootStaff, 엔트 보스) | U | Ancient_Roots `HitCountAdd` +2 + **Rule `Chain`(뿌리가 인접 적으로 번짐 2명)** | Mod+Rule |
| 늑대 송곳니 단검(FangDagger) | R | Howl_of_the_Pack 쿨 -20%, `DurationPct` +20 | Mod |
| 도깨비불 랜턴(WispLantern) | U | Wisp_Bolt 탄 +1 + Rule `Proc`(Ghostlight_Ring 10% 추가 시전) | Mod+Rule |
| 개구리 혀 채찍(FrogTongueWhip) | U | Tongue_Lash `RangePct` +30 + Rule `OnHitSpawn` Slow | Mod+Rule |
| 곰 발톱(BearClaw) | R | Mauling_Rush `DamagePct` +20 | Mod |
| 지진석 망치(QuakestoneMaul) | U | Quake_Slam `HitCountAdd` +1 + Rule `Proc`(Aftershock) | Mod+Rule |
| 세트 밖 유니크(Primordial Gel·Chief's War Banner) | U | 기존 `KillSummonProc`·`SummonRunner` 를 효과 id 로 이관: `SummonAdd` +1 + 처치 소환 | Mod+Rule |
| **Gatekeeper's Oath** | L | Gate_Cleave 충격파 `FireMode` Ring(전방위) + `OnKillCast` Stone_Sentinels(처치 시 수호석 1체 20%) + 고유 보너스: 슈퍼아머 발동 시 쿨 -2초 | Mod+Rule(변형급)+고유 |
| **방랑자의 팔찌**(신규, Bracelet 슬롯, 2층계 해금 전 1층계 보상 후보) | R | **텔레포트 쿨 -20%**(`tag:Dash`/`Teleport` 선택자) | Mod |
| 방랑자의 팔찌 상급(신규, U) | U | 텔레포트 쿨 -25% + Rule `OnHitSpawn` Explode(도착 지점 잔상 폭발 40% 피해) | Mod+Rule |
| 사냥꾼 부적(신규, Charm) | U | Rule `CooldownRefund`(처치 시 모든 스킬 쿨 -0.5초, 내부 쿨 1초) | Rule |

- `Player/Teleport.cs` 의 쿨 구조가 `Skill`/`SkillCooldown` 경유인지 builder 가 P1 에서 확인. 아니면 `Teleport` 쿨 계산부 1곳을 `SkillEffectService.Query("Teleport", kind).cooldownPct` 로 질의(선택자 `Teleport` 이름 고정).
- 신규 아이템(팔찌·부적)은 `ExtraEquipStats.csv` 에 보조 스탯 1줄 + `ItemSheet.effectIds`. 아이콘·텍스처는 임시/PixelLab 방 담당(총괄·설계는 그래픽 비담당).

### 4.3 나머지 세트 방향(후속 큐 REWORK-12-F1B)

| 세트 | 방향 |
|---|---|
| Rabbit·Rat·Scare | 입문 세트 — 효과 없음 유지(C). Rat 6=독 `OnHitSpawn`, Scare 밤 조건 `DamagePct`(cond `Night`) 정도만 |
| Fae | Phantom_Glide `Dash` 쿨감·도착 시 `OnHitSpawn` 요정 가루, 6: Fairy_Dust_Bolt `FireMode` Ring |
| Gloom | Bloodsting 적중 `LifeSteal` 계열 Rule, 6: Slime_Smash `OnHitSpawn` 소형 슬라임 `SummonAdd` |
| Outlaw | Backstab 뒤잡기 `DamagePct`(cond), 6: Triple_Shot `FireMode` Parallel |
| Raider | Counter_Stance 성공 시 `CooldownRefund`, 6: 반격 `OnHitSpawn` 충격파 |
| Cliff(Lv2) | Starfall `Retarget`(LowestHp), 6: Cyclone_Edge `FireMode` Ring |

## 5. 구현 단계 P0~P8

| 단계 | 내용 | 검증(프로브·테스트) | 건드릴 파일 |
|---|---|---|---|
| P0 **[완료 11-09]** | 순수 로직: `EffectEntry` 파서·`SkillMods` 합산·상한·충돌 규칙·`EffectDescriber` 템플릿 | EditMode: 상한 clamp(쿨 60·타격 4)·group 최대값·Rule 슬롯 초과 탈락·곱 결합(60%×15%=쿨 0.34배)·파서 오류 행 건너뜀·설명 문장 스냅샷. **현행 전 세트 조합의 스킬별 쿨 합 덤프**로 기존 상한 위반 여부 확인 | 신규 `Combat/Effects/*`, `Editor/Tests/EquipEffect*Tests.cs`, `Resources/EquipEffects.csv`(빈 헤더), `SkillTags.csv` |
| P1 **[완료 11-09]** | 서비스 연결 3곳(쿨·타격·탄)을 `Query` 로 교체 + 레거시 Dictionary 흡수 + `Attach/Detach` 호출 지점(세트·방어구·무기·`ExtraEquip`) | 기존 테스트 전부 통과(수치 불변이 합격 조건). 프로브: 현행 세트 6단계 착용 전후 `SkillCooldown` 쿨 초·탄 수가 **P1 이전과 동일** | `SkillCooldown.cs`, `Skill.cs`, `SetEffectApplier.cs`, `EquipBoxCheck.cs`, `ExtraEquip.cs`, `SetItem.cs`, 무기 교체 지점 |
| P2 **[완료 11-09]** | `ItemSheet.effectIds`·`SetItem.csv effectIds` 로드, `DataIntegrity` 확장, 신규 Mod op(`DamagePct`·`RangePct`·`ProjSpeedPct`·`DurationPct`·`StaminaPct`·`PierceAdd`·`SummonAdd`) 훅 | 프로브: 효과 1건 장착 시 해당 스킬 판정 크기·피해·스태미나 로그 비교(값 ± 허용). 해제 후 원복 | `ItemSheet.csv`·`SetItem.csv`(맨 뒤 열), 파서, `Skill`·탄 컴포넌트·`SummonRunner` |
| P3 **[완료 11-09]** | Rule 핸들러 1차: `FireMode`·`Homing`·`Pierce/Return`·`OnHitSpawn`(Explode/Slow) + `Skill.RoundSpawn` 전략 위임 | 프로브: Leaf 5세트 장착 → Scattering Leaves 탄 방향 각도 분포·되돌아옴 로그, 스크린샷(낮 시간대). 비장착 시 기존과 동일 | `Skill.cs`(RoundSpawn), 신규 `Rules/*`, `StraightArrow`·`HomingOrb` |
| P4 **[완료 11-11]** | Rule 2차: `Chain`·`OnKillCast`·`Proc`·`CooldownRefund`(+ 적 사망 이벤트 훅) | 프로브: 소환 프로브로 적 3마리 배치 → Chain 전이 수·감쇠·`OnKillCast` 발동·내부 쿨 | `EnemyHealth` 사망 이벤트, `KillSummonProc` 일반화, `SkillCooldown` |
| P5 **[완료 11-11]** | `Replace` 변형 프리팹 경로 + 유니크·레전더리 효과 데이터 | 프로브: Gatekeeper's Oath 변형 시전, 공유 프리팹 무변경 확인(프리팹 해시) | `Resources/Weapon/Variants/*`, `EquipEffects.csv` |
| P6 **[완료 11-11, 일부 보류 — 세트 창 11-14 완료(장비창 추가 칸 줄 "세트" 칸 호버·커서 → 착용 세트 요약 팝업 `SetOverviewPopup`, 순수 규칙 `SetOverview`) — 남음: 효과 상세 토글·SetSwapPreview 효과 줄·세트 창 문구 Loc 등록]** | UI: 툴팁 "특수 효과" 구획, 세트 창 효과 목록·미리보기·상한 표시, 평타 `damagePct` 한 줄 | 스크린샷(툴팁·세트 창), 마우스·키보드·패드(가상 패드 프로브) 3경로 확인, `InputAudit.md` 갱신 | `TooltipBuilder.cs`, 세트 창 UI(`SetSwapPreview` 호출 UI), Loc CSV |
| P7 **[완료 11-11, 일부 보류]** | 1층계 데이터 적용(§4) + 기존 아이템 재점검 도구(등급 대비 효과 없는 아이템 목록 → 보강 큐) | 프로브: 6세트 각 단계 `Query` 덤프 vs §4 표, 체감 프로브(쿨·피해 수치), 점수 합계 | `EquipEffects.csv`, `ItemSheet.csv`, `SetItem.csv`, `Docs/Playtest/EquipEffectAudit.md`(신규, 도구 출력) |
| P8 **[완료 11-11, 장신구 11-14]** | 숙련·공용 트리(WSKILL P3·P4)와 합산 결합 확인, 수치 조정, 신규 4칸 장신구·팔찌 추가 | 트리+장비 동시 장착으로 곱 결합 프로브, 1층계 플레이 체감 기록 | 문서, `SkillTreeService` 연결부 |

- **P0·P1 구현 메모(11-09):** `Script/Combat/Effects/*`(EffectEntry·EffectTable·SkillTagTable·SkillMods/EffectCaps/CooldownMath·EffectResolver·SkillEffectService·EffectDescriber), `Editor/Tests/EquipEffectTests.cs`(20건), `Resources/EquipEffects.csv`·`SkillTags.csv`(헤더만). 쿨·타격·탄 3곳은 `Query`로 교체, 레거시 Dictionary는 저장소로 유지하고 Query가 읽어 상한(쿨 60·타격 +4·탄 min(+6,기본)) 통과. 현행 전 세트·방어구 프리팹 합 최대 쿨감 50%라 상한 미달 → 신구 결과 동일(`P1_AllSetsAndEquipment_NewEqualsOld`). Attach/Detach 는 세트·방어구(EquipBoxCheck)·ExtraEquip 에 연결했으나 effectIds(P2) 전이라 id 없음. **P2 전 할 일(리뷰 지적):** 무기 교체 지점 Attach, Query 에 WeaponKind 전달(현재 None → `kind:` 선택자 미작동), 새 게임·로드 진입점 `ClearAll()`+`Recompute()`, 등급(grade) 인자 전달, 활성 행 있을 때 Query 캐시(GC), 쿨 하한 0.5초는 현재 꺼짐(`CooldownMath.Apply` minCooldown), 트리 곱 결합은 SkillTreeService 연결 시(P8).
- **P2 구현 메모(11-09):** `ItemSheet.csv`·`SetItem.csv`·Floor2Preview 머리글에 맨 뒤 `effectIds` 열 추가(ItemData.effectIds 는 머리글 이름 기반 로드). `SkillEffectService.AttachIds/SplitIds/StaminaPct`, `SkillModApplier`(피해·범위(localScale)·지속(holdingTime)·탄속(launchForce·desiredSpeed) 적용, 효과 없으면 SetDamge 와 동일 참조). `Skill` 3개 판정 생성부 + BlockReason/SkillEffect 스태미나 + `UseSkill(kind)`. Query 에 `weapon.Kind` 전달(kind: 선택자 동작). Attach 지점: 무기=`EquipBoxCheck.ActivePrefab`(해제=`DeletPrefab` Detach), 방어구=ActivePrefab, 장신구=`ExtraEquip.ApplyStats`, 세트=`SetEffectApplier.Apply(…, effectIds)`; 등급은 `rarity` 전달. 시험 데이터: `EF_LeafStaff`(Scattering_Leaves 피해+10·범위+10, LeafStaff)·`EF_BearClaw`(Mauling_Rush 피해+20, BearClaw)·`EF_DEV_Test`(all 스태미나20·탄속20·지속20, 어떤 아이템에도 안 붙임). `EquipEffectDataTests` 12건. **이월:** `PierceAdd`·`SummonAdd` 훅(탄 컴포넌트·SummonRunner, P3~P4), 새 게임·로드 진입점 `ClearAll()`, Query 캐시(스태미나만 캐시, 나머지는 시전 시점 호출이라 미적용), 쿨 하한 0.5초.
- **P3 구현 메모(11-09):** `FirePatterns.cs`(`IFirePattern`·`DefaultFirePattern`=기존 RoundSpawn 수식 그대로·`ModeFirePattern` Fan/Ring/Parallel/Backward), `SkillRuleHandlers.cs`(화이트리스트 switch: Homing·Pierce·Return·OnHitSpawn Explode/Slow, 컴포넌트 `RuleHoming`·`RuleReturn`·`RuleOnHit`), `SkillModApplier.Apply` 끝에서 호출(rules·pierceAdd·summonAdd 가 없으면 건너뜀). `DamageObject` 에 `IHitListener`·`ResetStruck()` 추가(적중 직후 알림, 리스너 없으면 기존과 동일), `SpawnOnStart.AddCount`, `SummonSkill.summonCount` 가산(SummonAdd). `Skill.RoundSpawn` 은 전략에 위임. 새 게임·로드 `SkillEffectService.ClearAll()`(play 진입 `RuntimeInitializeOnLoad` + `SaveManager.DataLoad`). 파라미터: FireMode p1=모드 p2=전체각(Fan)·간격(Parallel), Homing p1=회전속도(도/초, 기본 360) p2=탐색 반경, Pierce p1=횟수(≤3), Return p1 무시(수명 2배·절반 지점 반전·재타격), OnHitSpawn p1=Explode(p2=피해%·p3=반경, 중심 적 제외 주변 적)·Slow(p2=감속%·p3=초). 투사체 이동 Rule(Homing·Pierce·Return)은 `isLaunch` 판정만. **발견한 P2 버그 수정:** `DatabaseManager.LoadItemData` 가 `effectIds` 를 복사하지 않아 아이템 효과(EF_LeafStaff·EF_BearClaw)가 실제 장착에서 안 걸렸음 → 복사 추가. **시험 연결:** 잎사귀 아데(LeafAdae, R) → `EF_TEST_LeafFan`(Scattering_Leaves 부채꼴 90도). 주의: `Scattering Leaves`(RoundSpawn)는 LeafStaff 가 아니라 LeafAdae 소속이라 `EF_LeafStaff`(Scattering_Leaves 피해·범위)는 LeafStaff 에서 대상 스킬이 없음(설계 §4.2 표와 불일치, 후속 정리). 개발용 `EF_DEV_*`(Ring·Parallel·Backward·Seek·Boomerang·Pierce·PierceAdd·Blast)는 어떤 아이템에도 안 붙임. 폭발 시각 효과는 미구현(그래픽 비담당, 피해만). 프로브 `Tools/probes/efx_p3_snap.cs`(대표 5종 전후)·`efx_p3_rules.cs`·`efx_p3_dyn.cs`, EditMode `EquipEffectRuleTests` 12건. 미검증: 스크린샷 육안 확인.
- **P4 구현 메모(11-11):** `SkillRuleEvents.cs`(`RuleMath` 순수 계산·`SkillRuleEvents`=`EnemyHealth.Killed`·`Skill.Cast` 구독·`RuleChain`·`RuleHitRefund`). Chain=적중 리스너(p1=N≤5, p2=거리, p3=점프당 감쇠%, 연쇄 피해는 `damage2Enemy` 직접 호출이라 재연쇄 없음). OnKillCast(p1=스킬, p2=피해%, p3=내부쿨)·Proc(p1=스킬, p2=확률%+`ProcChance` 가산, p3=내부쿨)는 **무료 시전 = `Skill.SpawnFree`(첫 판정 프리팹만, 비용·쿨·모션 없음, 규칙 재적용 없음)** 이고 항목별 내부 쿨 최소 0.3초(§8-5). 대상 스킬은 **장착 무기의 스킬**에서 이름으로 찾는다(없으면 `GameLog.Warn`). CooldownRefund(p1=초, p2=Kill|Hit): Kill 은 처치 이벤트, Hit 은 판정 리스너, `Crit` 은 미지원(치명타 이벤트 없음 → 후속). `SkillCooldown.Refund`·`Skill.ActiveSlot`·`Skill.Cast` 추가. 규칙 슬롯 제한은 `Query(...).rules` 를 거치므로 그대로 적용. 시험: `EF_DEV_Chain/KillCast/Proc/RefundKill/RefundHit`(어디에도 안 붙임). 프로브 `efx_p4b.cs`(기준선 대비 연쇄 피해, 환급 -2.0초·-1.0초 확인), EditMode `EquipEffectRule2Tests` 6건. 미검증: 스크린샷(연쇄 시각 효과 없음 — 그래픽 비담당).
- **P5 구현 메모(11-11):** `SkillVariants.cs`(Replace p1 키 → `Resources/Weapon/Variants/{p1}` 로드·캐시, DamageObject 아니거나 없으면 경고 후 원본), `Skill.Variant()` 가 판정 프리팹 생성 지점 10곳을 감싼다(효과 0개면 질의 없이 원본). 변형은 **공유 원본의 복제본**(`GateCleave_Wide` = GatekeepersOathSlashWave 복제, 스케일 1.7·관통 +2 — 전방위 Ring 은 그림이 필요해 보류, 모양은 PixelLab/그래픽 방). 원본 프리팹 무수정(EditMode 로 스케일 확인). 데이터: `EF_GatekeepersOath`(Gate_Cleave 피해+15·쿨-10·Replace Wide·OnKillCast Stone_Sentinels 20%/3초 — 고유 보너스 "슈퍼아머 발동 시 쿨 -2초"는 슈퍼아머 이벤트가 없어 보류), R 무기 `EF_WispLantern`(Wisp_Bolt 탄+1)·`EF_QuakestoneMaul`(Quake_Slam 타격+1)·`EF_FangDagger`(Howl 쿨-15·지속+20, 11.5점). U 목표(RootStaff Chain·Wisp Proc·Frog Slow·Quake Proc)는 `EF_U_*` 로 정의만 하고 **부착 안 함**(아이템이 R 이라 등급 승격 결정 Q-EFX7 필요). 아이템 등급은 U 유지(L 신설은 드롭 연출·정렬 영향이라 보류). 발견: 무기 프리팹 `_kind` 가 None 인 무기(Gatekeeper's Oath 등)는 `kind:` 선택자 불가 → 스킬 이름 선택자 사용. 효과는 슬롯 아이템에서 읽으므로 프로브는 LoadPrefab 한 프레임 뒤(Weapon.Start 에서 Equipped 설정)에 장착·부착. 프로브 `efx_p5.cs`(변형 해석 전후·처치 시 Stone Sentinels), EditMode 3건+아이템 효과 목록 갱신. 미검증: 변형 판정 실제 시전(스킬 슬롯이 숙련 잠금이라 프로브로 못 쏨), 스크린샷.
- **P6 구현 메모(11-11):** `UI/EffectTooltip.cs`(효과 id → 자동 설명 줄, `ForItem`=아이템 툴팁 "특수 효과" 구획, `ForSetTier`=세트 단계 줄(달성=세트색·미달성=회색+"(잠김)"), `Status`=이름 선택자 스킬 Query 로 "(비활성)"(Rule 슬롯 초과)·"(상한)" 문구). `InventoryManager.EquipStatText`(상세창·호버 공용)·`SetEffectText` 에 연결, `TooltipBuilder.SetBlock` 에 `tierExtra` 선택 인자. 평타 `damagePct`: 선택자 `Basic`/`tag:Basic` 조건 없는 DamagePct 만 합산(`EffectResolver.BasicDamagePct`, all 은 평타 제외, 상한 100), `SkillEffectService.BasicDamageMul(isSkill)`(Recompute 때 캐시)을 `DamageObject` 플레이어 타격 2곳에 곱 — Weapon.cs 는 타 세션 수정 중이라 건드리지 않음. `EffectDescriber` 의 OnKillCast·Proc 스킬 이름은 Norm+번역. Loc 29종 `LocUi`+I2 동기화. 새 키 없음(호버 툴팁 재사용 → InputAudit 변경 없음). **미구현(보류):** 효과 상세 토글(Tab/Y)·세트 창 별도 화면(현재 세트 정보는 툴팁 세트 블록만 존재), 교체 미리보기(`SetSwapPreview`)의 효과 줄. 프로브 `efx_p6.cs`(한국어 문구·상태), `efx_p6_show.cs`+스크린샷(HoverTip 렌더 확인), EditMode 3건. 미검증: 실제 아이템 호버, 패드.
- **P7 구현 메모(11-11):** 세트 효과 11단계 데이터 `EF_Set_*`(Leaf5·Wolf4/5·Wisp4/5·Grove4/5·Bog4/5·Stone4/5, CSV 단계 키 기준 = 설계의 '6' 은 5단계)를 `SetItem.csv effectIds` 에 연결. 기존 스탯·레거시 쿨감 무변경. `EffectScore`(§3.2 점수·등급 예산·판정) + `Editor/EquipEffectAudit.cs` → `Docs/Playtest/EquipEffectAudit.md`(아이템 등급 대비 효과 없음 130건 = 보강 큐 자료, 프로브 `efx_p7.cs` 가 재생성). 점수: Leaf5 36(예산 28 초과, 설계 자체가 Rule 2개), Wolf5 24, Wisp5 36, Grove5 24, Stone5 24·24 등. **버그 수정:** `DatabaseManager.LoadSetsData` 가 `effectIds` 를 복사하지 않아 세트 단계 효과가 실제로는 안 걸렸음(P2 누락) → 복사 추가, 프로브 `efx_p7_set.cs` 로 로드·Attach·Detach 확인. **보류/차이:** Bog5 `OnHitSpawn Bleed`(핸들러 없음)·Wisp4 외 일부 설계 문구 생략, Pierce(Boar_Charge)는 `isLaunch` 판정만 적용돼 근접 돌진에는 효과 없음, 아이템 부착(R 보강)은 Q-EFX7. EditMode `EffectScore`·`SetItemCsv_EffectIds_AllExist`.
- **P8 구현 메모(11-11):** 숙련·공용 트리 쿨감과 장비 쿨감을 **곱 결합**으로 바꿨다: `Query` 가 `CooldownMath.CombinedPct(장비%, 트리%)`(각 버킷이 먼저 상한 60·15 통과, 실효 % 하나로 돌려줘 소비자 무변경). 이전엔 단순 합산이라 상한 밖이었음. 장비 60 + 트리 15 = 66%(×0.34, 15초 → 5.1초). 프로브 `efx_p8.cs`(FangDagger 15 × 트리 15 = 27.75% 확인), EditMode 1건. 트리의 피해·스태미나는 이미 별도 경로(`DamageMul`·`StaminaPct`)라 장비와 곱으로 쌓인다. **이월(미착수):** 신규 장신구·팔찌 아이템(방랑자의 팔찌 Teleport 쿨 -20%·사냥꾼 부적) — `Teleport.cs` 에 쿨 구조가 없어 쿨 질의 훅 신설과 팔찌 슬롯(홀수 층계 해금) 확정이 먼저, 큐 REWORK-12-F1B 로 이관. 미검증: 1층계 플레이 체감 기록(수치는 전부 임시값).
- **장신구 이월분 구현 메모(11-14):** 텔레포트는 별도 쿨 구조가 아니라 `Leaf Teleport` 스킬(LeafStaff)이고 `SkillCooldown.UseSkill` → `Query` 를 이미 거친다 → `Teleport.cs` 수정 불필요, `SkillTags.csv` 에 이동기 8종 `Dash`(Leaf Teleport 는 `Teleport;Dash`)만 등록. 신규 317 `WandererBracelet`(Bracelet, R, `EF_WandererBracelet` = `tag:Dash` 쿨 -20%, 공속 +2%)·318 `HunterCharm`(Charm=보조장비, U, `EF_HunterCharm` = all `CooldownRefund` 0.5초 Kill, p3 내부 쿨 1초, 드롭률 +2%). `CooldownRefund(Kill)` 에 p3 내부 쿨 추가(`RuleMath.RefundOnKill`: 처치 한 번에 항목별 한 번 판정, p3 비면 기존 동작). 획득: 부적 = AlphaWolf_BD 10%(Q-EFX8), 팔찌 = 3층계 해금이라 획득처 없음. 상급 팔찌(도착 폭발)는 텔레포트에 적중 판정이 없어 보류. 프로브 `_efx_acc.cs`(DB 로드 effectIds·Leaf Teleport 10초→8초·처치 환급 -0.5/내부 쿨 차단/1.2초 뒤 -0.5), EditMode 4건.
- P0~P1 은 플레이 영향 0이어야 한다(안전). P1 은 반드시 "수치 불변" 프로브 후 커밋.
- 위험 순서: P3 이 가장 크다(탄 생성 루프 개조). 변형은 항상 **효과 없으면 기존 코드 경로 그대로**.

## 6. 세이브 호환

- 효과는 **아이템 데이터(CSV)에서 파생** → ES3 에 효과 상태를 저장하지 않는다. 장착 상태·아이템 키는 기존 키 그대로. 로드 후 `Recompute()` 로 복원.
- 신규 ES3 키 불필요. 런타임 쿨다운 내부값(Proc 내부 쿨 등)은 비저장.
- 기존 필드·키 이름 변경 금지. `ItemSheet.csv`·`SetItem.csv` 는 맨 뒤 컬럼 추가만(빈칸 허용 → 구 행은 효과 없음).
- 효과 id 는 불변. 삭제된 id 를 참조하는 아이템은 `GameLog.Warn` 후 무시(로드 실패 금지).
- 등급 `L` 신설: 알 수 없는 등급 문자열은 `U` 로 취급하지 않고 경고 후 `C`(구 세이브·구 CSV 안전). 인스턴스(강화·잠금)는 ES3 기존 구조 유지.
- 레거시 `skillCoolDown`/`skillHitCount`/`skillBulletCount` 는 런타임 Dictionary 이므로 저장 이슈 없음.

## 7. UI — 마우스·키보드·게임패드 3경로

| 화면 | 마우스 | 키보드 | 게임패드 |
|---|---|---|---|
| 아이템 툴팁(특수 효과 구획) | 호버 | 인벤 커서 이동(방향키) 시 자동 표시 | 십자키/스틱 커서 이동 시 자동 표시 |
| 효과 상세(Rule 설명·비활성 사유 펼침) | 툴팁 클릭 | `Tab`(툴팁 상세 토글, 임시) | 보조 버튼(Y, 임시) |
| 세트 창(단계 목록·미리보기·상한 표시) | 세트 줄 호버/클릭 | 방향키 위/아래로 단계, `Enter` 상세 | 십자키 위/아래, A 상세, B 닫기 |
| 장비 교체 시 세트 변화 미리보기(`SetSwapPreview`) | 호버 | 교체 대상 선택 중 자동 | 같음 |

- 키 충돌 확인은 `Docs/Playtest/InputAudit.md` 표에서 `Tab`·`Y` 사용 여부를 builder 가 점검 후 확정(임시 가정). 3경로는 같은 이벤트(`UiAction`)를 공유해 하나로 구현.
- 색 전용 구분 금지: 활성/비활성은 색 + "(비활성)" 텍스트, 상한은 "상한" 문구.

## 8. 위험·함정

1. `Skill.RoundSpawn` 개조 시 기존 스킬 회귀 — 기본 전략은 기존 코드를 그대로 호출하는 래퍼. 스킬 전수 프로브(`TempFxSlots` 27 슬롯 기준).
2. 공유 판정 프리팹을 직접 수정하면 다른 무기가 함께 바뀜 → 복제 후 변형(LineageGear §0).
3. 장착/해제 이벤트 누락 → 효과가 남는 버그. `Attach/Detach` 는 출처 키 단위 멱등, 로드·씬 전환·무기 교체마다 `Recompute` 일관 호출.
4. `Equipment.coolDownSkill[]` 가 아이템 프리팹 필드라 `EquipBoxCheck` 와 새 서비스에서 이중 가산될 수 있음 → P1 에서 레거시 Dictionary 한 곳만 서비스가 읽도록 확정.
5. Chain·OnKillCast 는 무한 연쇄 위험: 연쇄로 발생한 판정은 `fromRule` 플래그로 재연쇄 금지, 내부 쿨 필수.
6. 조건부 효과를 캐시에 넣으면 상태 변화 후 틀림 → 조건 항목은 시전 시점 평가.
7. 초반 소박 원칙(`early-game-modest`): 1층계 수치는 R 10·U 24 예산 안. 초과 시 낮춘다.
8. 신규 문서·CSV 열이 `ItemSpec.md` 에 반영 안 되면 이후 작업이 어긋남 → P2 완료 때 표 갱신 필수.

## 세트 단계별 외형 이펙트 (11-10 사용자 확정, 구현은 EQUIP-SLOT-REFORM v2 이후)

- 세트 효과는 **추가 이펙트를 주는 형식**이다. 조각이 많이 켜질수록 외형 이펙트가 커져 "점점 완성되는 느낌"을 준다. 예(사용자): 디아블로식 선조 세트 — 2셋이면 약해 보이는 선조가 등 뒤에, 4셋이면 강해 보이는 선조.
- 세트에 세는 것은 장비 슬롯뿐이다(시작 5칸 의상·무기·반지·목걸이·귀걸이, 나중에 홀수 층계 해금 슬롯). 펫·룬·성물(짝수 층계)은 별도 콘텐츠라 세지 않는다. 지금은 5칸 기준 최대 5셋.
- 단계 구조: 세트마다 2·4 또는 3·5 단계. `SetBonusEvaluator` 가 단계 변화(켜짐·꺼짐·상승)를 이벤트로 내보내고, 이펙트 쪽이 단계별 그림을 고른다. 이펙트는 절차 생성(`MotionFxChecklist.md`)이고 몸 위치만 따라가므로 포즈·의상과 무관하다.
- 모션 소유 원칙과 무관: 세트 이펙트는 외형 부속일 뿐 기본·공격 동작을 바꾸지 않는다.
