# 무기 스킬 쿨타임 점검 (11-17, 읽기 전용)

> 범위: 무기 프리팹 `Assets/Resources/Weapon/*.prefab` 의 `Skill` 컴포넌트 전수(53종, 스킬 147개). 정적 grep 기준이며 Unity 에디터에서 값을 실측하지 않았다(미검증). 코드·에셋 수정 없음.

## 1. 쿨타임 소스 위치

| 단계 | 위치 | 내용 |
|---|---|---|
| 원천 값 | `Assets/Resources/Weapon/<무기>.prefab` 의 `Skill` 컴포넌트 `SkillCoolTime:` | 스킬 슬롯별 기본 쿨(초). 필드 정의: `Assets/Script/Weapon/Skill.cs:18` |
| 쿨 시작 | `Assets/Script/Weapon/Skill.cs:151` | `skillCooldown.cooldownTime[num] = SkillCoolTime` (무기 장착 시 Skill.cs:148 에서 전 스킬 쿨 시작) |
| 쿨 감소 적용 | `Assets/Script/UI/SkillCooldown.cs:210` | `CooldownMath.Apply(cooldownTime, cooldownPower)`, `cooldownPower` = `SkillEffectService.Query(skillName, kind).cooldownPct` (SkillCooldown.cs:208) |
| 쿨감 수식 | `Assets/Script/Combat/Effects/SkillMods.cs:57-65` | `base*(1-pct/100)`, 하한 `minCooldown` 기본 0 → 쿨감 100% 이상이면 0 (W-35 와 같은 문제) |
| 쿨감 데이터 | `Assets/Resources/EquipEffects.csv` `CooldownPct` / `CooldownRefund`, `ItemSheet.csv` 효과 설명 | 장비 쪽 쿨감·환급. 기본 쿨 값은 아님 |
| 세트 옵션 | `Assets/Resources/SetItem.csv` (`coolDown` 문자열) | 텍스트 표기. 실제 적용 경로는 SetEffectApplier 쪽, 본 문서 범위 밖 |
| 스킬 UI | `Assets/Script/UI/InventoryManager.cs:2186`, `TooltipBuilder.cs:200` | 툴팁·슬롯 텍스트 표시만 |

쿨타임 값을 바꾸려면: **프리팹 `SkillCoolTime` 값을 바꾼다**(Unity 에디터에서 프리팹 Skill 컴포넌트 수정 권장, YAML 직접 수정은 에디터 닫은 상태에서만). 장기로는 WeaponReview W-20 대로 WeaponData SO 로 이관 후 일괄 조정.

## 2. 요약

- 무기 스킬 보유 프리팹: **53종**, 스킬 슬롯 **147개** (ItemSheet 에 없는 프리팹 0건).
- 쿨타임 **0**: **0개**.
- 쿨타임 **0.1 (사실상 0, 테스트 값 의심)**: **70개** (전체의 약 48%). WeaponReview W-20 "12종 중 다수 0.1초" 와 일치.
- 의도된 0 후보: **0개**. 0 으로 둘 만한 평타 대체형·연사형은 현재 값이 없다.
- 연사형 0.1 유지 검토(다단 히트 이름 기준, 0 아님): Vicious Assault(DoubleDagger), Rising Roots(LeafStaff), Night Hunt(ScareClow), Swift Assault(RatCheifDagger). 기획 판단 필요: 0.1 을 유지할지 3~5초 연사 쿨로 바꿀지.
- 1층계 태그: F1WeaponMotionAudit.md 본문 또는 Floor1WeaponTrials.csv 에 이름이 나온 무기 기준(정확한 1층계 40종 목록은 아님).

## 3. 제안값 규칙 (메이플·던파 초반 기준, 추정)

| 역할 | 판정 근거(정적) | 제안 쿨(초) |
|---|---|---|
| 이동기 | 스킬 이름에 Step·Stride·Dash·Charge·Back·Haste·Teleport 등 | 2~4 (표 3s) |
| 약한 스킬 | useStemina 1 | 3~5 (표 4s) |
| 강한 스킬 | useStemina 5 이상 | 8~15 (표 8s) |
| 홀드 스킬 | isHoldSkill=1 (홀드 시간 별도) | 8 (홀드 시간은 유지) |

정적 규칙이라 개별 스킬 성격과 어긋날 수 있다. 특히 "강함"으로 찍힌 Mauling Rush 류 돌진 공격은 기획 확인 필요. 값 확정은 플레이 검증 뒤.

## 4. 전체 표

열: 무기 | ItemSheet id | 1층계 태그 | 슬롯(skillNum) | 스킬 | 현재 쿨(s) | 판정 | useStemina | 역할(추정) | 제안 쿨(s)

| 무기 | id | 1층계 | 슬롯 | 스킬 | 현재 | 판정 | 스태미나 | 역할 | 제안 |
|---|---|---|---|---|---|---|---|---|---|
| BearClaw | 124 | 1층계 | 0 | Mauling Rush | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| BearClaw | 124 | 1층계 | 1 | Rending Upswipe | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| BearClaw | 124 | 1층계 | 2 | Bear Roar | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| RootStaff | 125 | 1층계 | 0 | Root Step | 4 | OK | 5 | 이동기 | 3 |
| RootStaff | 125 | 1층계 | 1 | Ancient Roots | 2 | OK | 1 | 약함 | 4 |
| RootStaff | 125 | 1층계 | 2 | Sap Mend | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| FrogTongueWhip | 143 | 1층계 | 0 | Tongue Lash | 2 | OK | 5 | 강함 | 8 |
| FrogTongueWhip | 143 | 1층계 | 1 | Leap Slam | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| CrocJawAxe | 144 | 1층계 | 0 | Jaw Shake | 3 | OK | 5 | 강함 | 8 |
| CrocJawAxe | 144 | 1층계 | 1 | Bog Eruption | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| CrocJawAxe | 144 | 1층계 | 3 | Death Roll | 5 | OK | 1 | 약함 | 4 |
| CrocJawAxe | 144 | 1층계 | 2 | Tail Whirl | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| Wood Spear | 16 | 1층계 | 0 | Double Strike | 2 | OK | 5 | 강함 | 8 |
| Wood Spear | 16 | 1층계 | 1 | Charge Forward! | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| CutthroatDagger | 164 | 1층계 | 0 | Backstab | 3 | OK | 5 | 이동기 | 3 |
| CutthroatDagger | 164 | 1층계 | 1 | Shadow Step | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| CutthroatDagger | 164 | 1층계 | 2 | Cutpurse Stride | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| LeafStaff | 22 | 1층계 | 0 | Leaf Teleport | 4 | OK | 5 | 이동기 | 3 |
| LeafStaff | 22 | 1층계 | 1 | Rising Roots | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| LeafStaff | 22 | 1층계 | 2 | Nature's Healing | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| LeafAdae | 23 | 1층계 | 0 | Scattering Leaves | 1 | OK | 5 | 강함 | 8 |
| LeafAdae | 23 | 1층계 | 1 | Haste | 20 | OK | 1 | 이동기 | 3 |
| LeafAdae | 23 | 1층계 | 2 | Falling Leaf Step | 1 | OK | 1 | 이동기 | 3 |
| HuntersLongbow | 241 | 1층계 | 0 | Hunter's Volley | 2 | OK | 5 | 강함 | 8 |
| HuntersLongbow | 241 | 1층계 | 1 | Venom Rain | 8 | OK | 1 | 약함 | 4 |
| GnarledClub | 242 | 1층계 | 1 | Club Smash | 4 | OK | 1 | 약함 | 4 |
| GnarledClub | 242 | 1층계 | 2 | Root Tremor | 7 | OK | 5 | 강함 | 8 |
| TwinFang | 244 | 1층계 | 0 | Cross Fang | 3 | OK | 5 | 강함 | 8 |
| TwinFang | 244 | 1층계 | 1 | Turn and Thrust | 4 | OK | 1 | 약함 | 4 |
| VineLash | 245 | 1층계 | 0 | Vine Pull | 2 | OK | 5 | 강함 | 8 |
| VineLash | 245 | 1층계 | 1 | Pounce Lash | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| PoisonSword | 30 | 1층계 | 0 | Vile Ambush | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| PoisonSword | 30 | 1층계 | 1 | This is Our Turf | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| ScareSide | 39 | 1층계 | 0 | Sinister Slash | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| ScareSide | 39 | 1층계 | 1 | Nightmare Eruption | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| ScareSide | 39 | 1층계 | 3 | Dark Harvest | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| ScareSide | 39 | 1층계 | 2 | Whirling Malice | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| ScareClow | 40 | 1층계 | 0 | Night Hunt | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| ScareClow | 40 | 1층계 | 1 | Rising Dread | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| ScareClow | 40 | 1층계 | 2 | Encroaching Darkness | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| RustedSword | 41 | 1층계 | 0 | Smash Down | 4 | OK | 5 | 강함 | 8 |
| RustedSword | 41 | 1층계 | 1 | Slash Attck | 3 | OK | 1 | 약함 | 4 |
| CliffStaff | 100 | - | 0 | Arcane Bolt | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| CliffStaff | 100 | - | 1 | Starfall | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| CliffStaff | 100 | - | 2 | Cliff Quake | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| CliffBlade | 101 | - | 0 | Gale Slash | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| CliffBlade | 101 | - | 1 | Cyclone Edge | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| CliffBlade | 101 | - | 3 | Talon Dive | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| CliffBlade | 101 | - | 2 | Skybreaker | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| AbyssStaff | 102 | - | 0 | Abyss Bolt | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| AbyssStaff | 102 | - | 1 | Abyss Rain | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| AbyssStaff | 102 | - | 2 | Abyss Quake | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| FangDagger | 106 | - | 0 | Fang Rush | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| FangDagger | 106 | - | 1 | Pack Step | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| FangDagger | 106 | - | 2 | Hunter's Stride | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| FangDagger | 106 | - | 0 | Howl of the Pack | 8 | OK | 5 | 강함 | 8 |
| TuskKnuckles | 111 | - | 0 | Boar Charge | 1 | OK | 5 | 이동기 | 3 |
| TuskKnuckles | 111 | - | 1 | Hoof Step | 1 | OK | 1 | 이동기 | 3 |
| TuskKnuckles | 111 | - | 2 | Tusk Uppercut | 1 | OK | 1 | 약함 | 4 |
| WispLantern | 112 | - | 0 | Wisp Bolt | 1.2 | OK | 5 | 강함 | 8 |
| WispLantern | 112 | - | 1 | Ghostlight Ring | 10 | OK | 1 | 약함 | 4 |
| WispLantern | 112 | - | 2 | Wailing Light | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| ShellShield | 113 | - | 0 | Shell Pierce | 3 | OK | 5 | 강함 | 8 |
| ShellShield | 113 | - | 1 | Shell Bash | 5 | OK | 1 | 약함 | 4 |
| ShellShield | 113 | - | 2 | Shell Guard | 6 | OK | 1 | 홀드 | 8 |
| HeartwoodGreatsword | 126 | - | 0 | Sap Drain | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| HeartwoodGreatsword | 126 | - | 1 | Grove Cleave | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| HeartwoodGreatsword | 126 | - | 2 | Ancient Wrath | 9 | OK | 1 | 약함 | 4 |
| ToxicPickAxe | 13 | - | 0 |  | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| ToxicPickAxe | 13 | - | 0 |  | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| PhantomScythe | 131 | - | 0 | Phantom Reap | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| PhantomScythe | 131 | - | 1 | Spectral Eruption | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| PhantomScythe | 131 | - | 3 | Phantom Glide | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| PhantomScythe | 131 | - | 2 | Wraith Whirl | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| FaeWand | 132 | - | 0 | Fairy Dust Bolt | 1.2 | OK | 5 | 강함 | 8 |
| FaeWand | 132 | - | 1 | Fae Blessing | 12 | OK | 1 | 약함 | 4 |
| FaeWand | 132 | - | 2 | Starlight Lance | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| PetalPincer | 145 | - | 0 | Crab Scuttle | 1 | OK | 5 | 이동기 | 3 |
| PetalPincer | 145 | - | 1 | Shell Back | 1 | OK | 1 | 이동기 | 3 |
| PetalPincer | 145 | - | 2 | Pincer Flurry | 2 | OK | 1 | 약함 | 4 |
| BloodstingRapier | 146 | - | 0 | Bloodsting | 1.5 | OK | 5 | 강함 | 8 |
| BloodstingRapier | 146 | - | 1 | Swarm Dart | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| BloodstingRapier | 146 | - | 2 | Swarm Call | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| SlimeMace | 147 | - | 0 | Gel Gather | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| SlimeMace | 147 | - | 1 | Slime Smash | 6 | OK | 1 | 약함 | 4 |
| PrimordialGel | 148 | - | 0 | Primordial Pull | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| PrimordialGel | 148 | - | 1 | Gel Tsunami | 7 | OK | 1 | 약함 | 4 |
| Leather Knuckles | 15 | - | 0 | Ducking Dash | 1 | OK | 5 | 이동기 | 3 |
| Leather Knuckles | 15 | - | 1 | Sway Back | 1 | OK | 1 | 이동기 | 3 |
| Leather Knuckles | 15 | - | 2 | Uppercut | 1 | OK | 1 | 약함 | 4 |
| BanditShortbow | 165 | - | 0 | Triple Shot | 1.5 | OK | 5 | 강함 | 8 |
| BanditShortbow | 165 | - | 1 | Poison Volley | 8 | OK | 1 | 약함 | 4 |
| BanditShortbow | 165 | - | 2 | Pitch Shot | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| RaiderBuckler | 166 | - | 0 | Raider Thrust | 3 | OK | 5 | 강함 | 8 |
| RaiderBuckler | 166 | - | 1 | Shield Shove | 4 | OK | 1 | 약함 | 4 |
| RaiderBuckler | 166 | - | 2 | Counter Stance | 6 | OK | 1 | 홀드 | 8 |
| ChiefsWarBanner | 167 | - | 0 | Banner Thrust | 2 | OK | 5 | 강함 | 8 |
| ChiefsWarBanner | 167 | - | 1 | Raid Charge | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| ChiefsWarBanner | 167 | - | 0 | Rally the Gang | 9 | OK | 5 | 강함 | 8 |
| PebbleGauntlet | 182 | - | 0 | Rolling Charge | 2.5 | OK | 5 | 이동기 | 3 |
| PebbleGauntlet | 182 | - | 1 | Pebble Hop | 1 | OK | 1 | 이동기 | 3 |
| PebbleGauntlet | 182 | - | 2 | Rubble Uppercut | 1 | OK | 1 | 약함 | 4 |
| QuakestoneMaul | 183 | - | 0 | Tremor Pull | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| QuakestoneMaul | 183 | - | 1 | Quake Slam | 3 | OK | 1 | 약함 | 4 |
| QuakestoneMaul | 183 | - | 2 | Aftershock | 6 | OK | 5 | 강함 | 8 |
| MonolithShield | 184 | - | 0 | Boulder Toss | 2.5 | OK | 5 | 강함 | 8 |
| MonolithShield | 184 | - | 1 | Granite Bash | 5 | OK | 1 | 약함 | 4 |
| MonolithShield | 184 | - | 2 | Monolith Stance | 6 | OK | 1 | 홀드 | 8 |
| GatekeepersOath | 185 | - | 0 | Oath Bind | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| GatekeepersOath | 185 | - | 1 | Gate Cleave | 4 | OK | 1 | 약함 | 4 |
| GatekeepersOath | 185 | - | 0 | Stone Sentinels | 10 | OK | 5 | 강함 | 8 |
| RampartSword | 195 | - | 0 | Shield Rush | 3 | OK | 5 | 강함 | 8 |
| RampartSword | 195 | - | 1 | Knight's Cleave | 3 | OK | 1 | 약함 | 4 |
| SentryHalberd | 196 | - | 0 | Halberd Sweep | 2.5 | OK | 5 | 강함 | 8 |
| SentryHalberd | 196 | - | 1 | Crescent Reap | 3.5 | OK | 1 | 약함 | 4 |
| GargoyleWing | 197 | - | 0 | Stone Feather | 1.4 | OK | 5 | 강함 | 8 |
| GargoyleWing | 197 | - | 1 | Petrify Bolt | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| GargoyleWing | 197 | - | 2 | Gargoyle Dive | 9 | OK | 5 | 강함 | 8 |
| SkyfallTalon | 198 | - | 0 | Talon Storm | 2.5 | OK | 5 | 강함 | 8 |
| SkyfallTalon | 198 | - | 1 | Skyfall Rend | 3 | OK | 1 | 약함 | 4 |
| SkyfallTalon | 198 | - | 2 | Gargoyle Host | 10 | OK | 1 | 약함 | 4 |
| PickAxe | 2 | - | 0 |  | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| PickAxe | 2 | - | 0 |  | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| WoodcuttersAxe | 243 | - | 0 | Timber Chop | 3 | OK | 5 | 이동기 | 3 |
| WoodcuttersAxe | 243 | - | 3 | Log Roller | 5 | OK | 1 | 약함 | 4 |
| DoubleDagger | 29 | - | 0 | Vicious Assault | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| DoubleDagger | 29 | - | 1 | Double Step | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| DoubleDagger | 29 | - | 2 | Sly Stride | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| RatCheifDagger | 31 | - | 0 | Swift Assault | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| RatCheifDagger | 31 | - | 1 | Ground Sweep | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| RatCheifDagger | 31 | - | 2 | Rally Now! | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| SentryCrossbow | 313 | - | 0 | Triple Shot | 1.5 | OK | 5 | 강함 | 8 |
| SentryCrossbow | 313 | - | 1 | Poison Volley | 8 | OK | 1 | 약함 | 4 |
| SentryCrossbow | 313 | - | 2 | Pitch Shot | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| HuntersJavelin | 314 | - | 0 | Double Strike | 2 | OK | 5 | 강함 | 8 |
| HuntersJavelin | 314 | - | 1 | Charge Forward! | 0.1 | 0.1 테스트값 의심 | 1 | 이동기 | 3 |
| FairysGrimoire | 315 | - | 0 | Fairy Dust Bolt | 1.2 | OK | 5 | 강함 | 8 |
| FairysGrimoire | 315 | - | 1 | Fae Blessing | 12 | OK | 1 | 약함 | 4 |
| FairysGrimoire | 315 | - | 2 | Starlight Lance | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| SacreGreatSword | 37 | - | 0 | Gathering Nightmare | 0.1 | 0.1 테스트값 의심 | 5 | 홀드 | 8 |
| SacreGreatSword | 37 | - | 1 | Approaching Nightmare | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| ScareStaff | 38 | - | 0 | Manifest Darkness | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| ScareStaff | 38 | - | 1 | Dream Eater | 0.1 | 0.1 테스트값 의심 | 1 | 약함 | 4 |
| ScareStaff | 38 | - | 2 | Creeping Nightmare | 0.1 | 0.1 테스트값 의심 | 5 | 강함 | 8 |
| RustedSwordandShield | 42 | - | 0 | Fierce Thrust | 3 | OK | 5 | 강함 | 8 |
| RustedSwordandShield | 42 | - | 1 | Shield Charge | 5 | OK | 1 | 이동기 | 3 |
| RustedSwordandShield | 42 | - | 2 | Hold the Line | 1 | OK | 1 | 홀드 | 8 |

## 결정 (11-17 총괄, 추천대로)
- 0.1초 70개는 테스트값으로 보고 위 표 제안 쿨로 바꾼다. 연사형 4개도 던파처럼 쿨 0 근처를 두지 않고 표 제안값(4~8초)을 쓴다.
- 적용은 귀환 길찾기 builder 가 끝난 뒤 builder 가 한다(프리팹 `SkillCoolTime` 바이트 보존 수정 → 봇 실행으로 분당 스킬 사용·처치 속도 확인).
