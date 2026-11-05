# 계보 장비 규칙 정합 점검 (11-05, AUTO-26)

규칙(`LineageGear.md` §0, `TowerWorld.md` 12절): 무기·세트는 드롭 몹의 스킬을 복제한다. 점검 방법: 실 `ItemSheet.csv` 무기(tfName 있는 Weapon) → `Resources/Weapon/*.prefab` 의 `skillName` 추출 → `SetItem.csv` 4·6세트 `coolDown <스킬>` 키와 대조(공백=`_`). 몹 스킬 열은 `LineageGear.md`·`Floor2Plan.md` §5 설계 문서 기준(코드 미확인). 수정 없음, 불일치만 ID 로 기록.

## 1. 1층계 (실 데이터)

| 세트 | 몹 → 몹 스킬 (문서) | 무기 · 스킬 키 (프리팹) | 세트 coolDown 키 | 일치 |
|---|---|---|---|---|
| Bog Stalker `BogArmor` | 개구리 혀 당기기 / 악어 물어 흔들기 / 꽃게 집게 연타 | FrogTongueWhip: Tongue_Lash·Leap_Slam / CrocJawAxe: Jaw_Shake·Death_Roll / PetalPincer: Pincer_Flurry·Shell_Back | Tongue_Lash, Jaw_Shake, Pincer_Flurry | 예 |
| Gloom Swarm `GloomArmor` | 모기 흡혈 돌진 / 슬라임 분열 | BloodstingRapier: Bloodsting / SlimeMace: Slime_Smash | Bloodsting, Slime_Smash | 예 |
| Outlaw `OutlawArmor` | 단검병 기습 / 궁수 사격 | CutthroatDagger: Backstab / BanditShortbow: Triple_Shot | Backstab, Triple_Shot | 예 |
| Iron Raider `RaiderArmor` | 방패병 밀치기 | RaiderBuckler: Shield_Shove·Counter_Stance | Shield_Shove, Counter_Stance | 예 |
| Stoneborn `StoneArmor` | 소형/중형/대형 골렘 | PebbleGauntlet: Rolling_Charge / QuakestoneMaul: Aftershock / MonolithShield: Boulder_Toss | 위 3종 | 예 |
| Rampart Guard `RampartArmor` | 성벽 기사 / 미늘창 파수병 | RampartSword: Shield_Rush / SentryHalberd: Halberd_Sweep | Shield_Rush, Halberd_Sweep | 예 |
| Skybound `SkyArmor` | 가고일 (구름 정령은 무기 없음, 설계대로) | GargoyleWing: Stone_Feather·Gargoyle_Dive | 위 2종 | 예 |
| Cliffwalker's Vow `CliffArmor` | 절벽 몹(문서 공백) | CliffStaff: Starfall·Cliff_Quake / CliffBlade: Cyclone_Edge·Talon_Dive·Skybreaker | 5종 전부 | 예(몹 열 문서 공백) |
| Pack Hunter `WolfArmor` | 늑대·멧돼지(문서 공백) | FangDagger: Howl_of_the_Pack / TuskKnuckles: Boar_Charge | Howl_of_the_Pack, Boar_Charge | 예(몹 열 문서 공백) |
| Night Lantern `WispArmor` | 도깨비불·껍질(문서 공백) | WispLantern: Wisp_Bolt·Ghostlight_Ring / ShellShield: Shell_Guard | 3종 | 예(몹 열 문서 공백) |
| Ancient Grove `GroveArmor` | 곰·엔트(문서 공백) | BearClaw: Mauling_Rush / RootStaff: Ancient_Roots | 2종 | 예(몹 열 문서 공백) |
| Will-o'-Fae `FaeArmor` | 요정(문서 공백) | PhantomScythe: Phantom_Glide / FaeWand: Fairy_Dust_Bolt | 2종 | 예(몹 열 문서 공백) |

결과: 세트 4·6 `coolDown` 키 31개 중 무기 프리팹에 없는 키 **0개**. 소속 세트 키(tfName)도 모든 무기 행이 `SetItem` 에 존재.

## 2. 2층계 초안 (`Docs/Floor2Data/*_draft.csv`, 무기 프리팹 없음)

| 세트 | 몹 → 몹 스킬 (Floor2Plan §5) | 초안 무기 | 세트 coolDown 키(초안) | 일치 |
|---|---|---|---|---|
| Garden Warden `GardenArmor` | 천칭 석상 반사 / 정원사 2연 가위·덩굴 묶기 | ScaleBulwark, PruningShears | Scale_Guard, Pruning_Cut | 부분 (덩굴 묶기 S 미반영) |
| Moonlit Stone `MoonStoneArmor` | 날개 석상 급강하 / 달빛 나방 가루 장판 | PetrifiedWing, MoonmothCenser | Wing_Dive, Dust_Field | 예 |
| Order of Four `OrderArmor` | 창기사 돌진 / 검기사 3연 베기·패링 / 궁기사 관통 / 마법기사 유도탄·방벽 | LancersPike, KnightCommanderBlade, LongbowOfTheOrder, SpellguardRod | Pike_Charge, Triple_Slash, Pierce_Arrow, Homing_Orb | 부분 (패링·방벽 S 미반영) |
| Cloudsea Drake `DrakeArmor` | 새끼 용 불씨 연타 / 와이번 활강·꼬리 독침 / 숭배자 화염 구슬 | DrakelingFang, WyvernGlaive, CinderCenser | Ember_Combo, Glide_Slash, Fire_Orbs | 부분 (꼬리 독침 S 미반영) |
| 고유 3종 (Gorgon's Gaze·Fourfold Oath·Cloudsea Breath) | 보스 | tfName 빈칸 | — | 예(세트 없음, 설계대로) |

초안 키는 아직 프리팹이 없어 `skillName` 확인 불가: 프리팹 제작 때 위 키(공백=`_`)를 `skillName` 으로 맞춘다.

## 3. 불일치 · 공백 목록

| ID | 내용 | 영향 |
|---|---|---|
| L1 | 문서 공백: `LineageGear.md` 에 Cliff·Wolf·Wisp·Grove·Fae 5세트의 몹 스킬 열이 없음(§1~3 은 ④~⑥구역만) | 규칙 대조 불가, 값은 무기·세트 키 일치 |
| L2 | 비계보 무기 5종(TwinFang·GnarledClub·VineLash·HuntersLongbow·WoodcuttersAxe)이 세트 키(tfName)를 갖지만 세트 4세트 coolDown 에 스킬 없음 | 세트 수 카운트 포함 여부 확인 필요, 쿨감 혜택 없음 |
| L3 | 초안 Garden·Order·Drake 의 S 슬롯 스킬(덩굴 묶기·패링·방벽·꼬리 독침)이 세트 4세트 키에 없음 | 세트 4 쿨감이 일부 스킬만 적용 |
| L4 | 초안 스킬 키가 `Floor2Plan.md` §5 에는 한글 설명뿐 — 영문 키 표 없음 | 프리팹 제작 때 이름 어긋날 위험 |
| L5 | 문서의 Petal Pincer S "방어 자세(Shell Guard)" 는 ShellShield 쪽에만 존재(PetalPincer 는 Shell_Back) | 문서 표현 불일치(기능 영향 없음) |

## 4. 수정 후보 큐 ID

- LA-1 (문서, P3): `LineageGear.md` 에 Cliff·Wolf·Wisp·Grove·Fae 몹→무기 표 추가 (L1)
- LA-2 (확인, P2): 세트 카운트가 `setName` 인지 `tfName` 인지 확인 후 비계보 무기 5종의 세트 소속 의도 결정 (L2)
- LA-3 (문서, P2): `Floor2Plan.md` §5 에 무기 스킬 영문 키 표 추가, 초안 coolDown 키와 대조 (L3·L4)
- LA-4 (설계, P3): 초안 S 슬롯 스킬 4종을 4세트 coolDown 에 포함할지 결정 (L3)
- LA-5 (문서, P3): `LineageGear.md` Petal Pincer 서술을 실제 키(Shell_Back/Shell_Guard)에 맞춤 (L5)

## 5. 적용 결과 (11-05)
- L1 → `LineageGear.md` §7 추가. L4 → `Floor2Plan.md` §5.6 영문 키 표. L5 → `LineageGear.md` §1 서술을 실제 키(Shell_Back)에 맞춤.
- L2·L3 → 판단 건: `Docs/OpenQuestions.md` Q-LA2/LA3 임시 가정(유지·A 슬롯만). 데이터(CSV) 변경 없음, 세트 키 이름 변경 없음.
- 검증: 데이터 변경이 없어 기존 EditMode 741 통과 상태 그대로(문서만 수정).
