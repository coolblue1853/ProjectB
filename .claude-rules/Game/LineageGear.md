# 몬스터 계보 장비 계획 (1층계 ④~⑥구역)

> 원칙은 `Game/TowerWorld.md` 12절(사용자 2026-10-19): 무기·세트는 잡은 몹의 공격·특성을 본떠 강화한다. 유니크·레전더리부터 계보를 벗어난 특색 무기.
> ②구역(일반 숲)은 구현 완료(TowerWorld 11·12절), ③구역(깊은 숲)은 구현 중(TowerWorld 13절). 이 문서는 그다음 구역의 **설계안**이다. 구현할 때 체크박스를 채우고 커밋 해시를 적는다.

## 0. 공통 규칙

- **구역 세트 2종**: 낮 몹 묶음 1세트 + 밤 몹 묶음 1세트(②구역 Pack Hunter / Night Lantern 방식). 방어구 5부위 + 계보 무기 2종이 같은 세트 키(`tfName`)를 쓴다 → 6세트 효과까지 간다.
- **세트 효과 단계**: 2 = 기본 스탯(몹 특성), 4 = 계보 무기 스킬 쿨감·강화, 6 = 구역 정체성 효과(공속·피해·생존).
- **드롭률**: 재료 30~35%, 계보 무기 2%, 방어구 부위 0.3%, 보스 확정 보상은 `BossReward`.
- **아트**: 몹·아이콘은 코드 도트(`Tools/pixelfx`), 방어구 착용 텍스처는 Leaf 아틀라스 색 변환(임시) → 정식 도트는 후속.
- **구현 경로 재사용**: 소환 `SummonRunner`/`SummonSkill`, 유도 `HomingOrb`, 공전 `OrbitWisps`, 판정 개조는 원본 판정 프리팹을 복제해서 바꾼다(공유 프리팹 직접 수정 금지).

## 1. ④ 늪지 (부하 30) — ✅ 1차 구현 (2026-10-19, TowerWorld 14절)

| 몹 | 공격(몹) | 계보 무기 | 무기 스킬 | 비고 |
|---|---|---|---|---|
| 개구리 | 혀 당기기, 점프 압사 | **Frog Tongue Whip**(채찍형, Spear 기반) | A 혀 끌어당기기(적을 앞으로), S 도약 내려찍기 | 당기기는 음수 넉백 |
| 모기 | 흡혈 돌진(비행) | **Bloodsting Rapier**(Dagger 기반) | A 흡혈 찌르기(피해 일부 회복), S 3연 돌진 | `HpHeal` 연동 |
| 슬라임 | 분열 | **Slime Mace**(GreatSword 기반) | A 내려찍기 시 작은 슬라임 소환 2(`SummonRunner` 변형: 튀는 슬라임) | |
| 꽃게 | 집게 연타·방어 | **Petal Pincer**(Knuckles 기반) | A 집게 연타, S 방어 자세(Shell Guard 재사용) | |
| 악어 | 물어 흔들기 | **Croc Jaw Axe**(Side 기반) | A 물어 흔들기(다단 3), S 돌진 회전 | |
| 보스 **거대 슬라임** | 분열·점프 | 유니크 **Primordial Gel** | 처치 시 아군 슬라임 소환(확률) | `BossReward` |

- 구현 체크: [x] 몹 6종(개구리·꽃게·악어·모기·슬라임·거대 슬라임) [x] Frog Tongue Whip [x] Bloodsting Rapier [x] Slime Mace [x] Petal Pincer(A 연타만, Shell Guard ⏳) [x] Croc Jaw Axe [x] Primordial Gel(`KillSummonProc`) [x] 세트 Bog Stalker·Gloom Swarm [x] 맵 `Lv1_Swamp`·드롭·보스 보상. 커밋: 99d660a2(맵·몹), 이후 장비 커밋은 TowerWorld 14절 참조.
- 변경: Gloom Swarm 2세트는 `LifeSteal` 신규 키 대신 HP 80·치명 5(흡혈은 Bloodsting 무기 `LifeStealHit` 이 맡음), 4세트 "소환 슬라임 +1" 대신 Bloodsting·Slime Smash 쿨감(소환 수 세트 효과는 4절 4번 시스템 대기). Bog 6세트 "출혈"은 `BleedDmg` 15.
- 세트: **Bog Stalker**(낮: 개구리·악어·꽃게) 2: 이동 6·방어 8 / 4: 혀·턱 쿨 −30 / 6: 공속 10·출혈. **Gloom Swarm**(밤·습지: 모기·슬라임) 2: 흡혈 소량(공격 시 HP 1%) — 신규 효과 키 `LifeSteal` 필요 / 4: 소환 슬라임 +1 / 6: 독 피해.

## 2. ⑤ 산적 성채 (부하 30, 분기) — ✅ 1차 구현 (2026-10-19, TowerWorld 15절)

| 몹 | 공격 | 계보 무기 | 무기 스킬 |
|---|---|---|---|
| 산적 단검병 | 기습 연타 | **Cutthroat Dagger** | A 뒤로 돌아 찌르기(백어택 치명↑) |
| 산적 궁수 | 원거리 사격 | **Bandit Shortbow**(신규 원거리 무기 타입) | A 3연사, S 독화살 비 — 활 계열이 없으므로 `Weapon` 원거리 판정(탄 이동) 신규 필요 |
| 산적 방패병 | 방패 밀치기 | **Raider Buckler**(Sword&Shield) | A 방패 밀치기, S 반격 자세 |
| 보스 **산적 대장** | 휘두르기·부하 호출 | 유니크 **Chief's War Banner** | 부하(산적 그림자) 소환 |

- 인간형 아트는 SPUM 금지 → 마을 NPC와 같은 도트 양식(24×32·8배)으로 코드 생성하거나 보유 Martial Hero / Medieval King Pack 2(다운로드 캐시 확인 필요) 사용.
- 세트: **Outlaw**(단검·궁수) 2: 치명 6 / 4: 백어택 치명피해 +25(신규 효과 키 `BackstabDmg`) / 6: 공속 12. **Iron Raider**(방패·대장) 2: 방어 15 / 4: 반격 피해 / 6: HP 15%.
- 구현 체크: [x] 몹 5종(단검병·궁수·방패병·횃불 순찰병(밤)·산적 대장) — 코드 도트(SPUM 미사용) [x] 궁수 탄 직선 화살 `BanditArrow` [x] 보스 판정 교체(ChiefCleave·ChiefAxe) [x] 보스 2페이즈 부하 호출(`BossMinionCall`) [x] Cutthroat Dagger(Backstab) [x] Bandit Shortbow(원거리 `StraightArrow` 평타·Triple Shot·Poison Volley) [x] Raider Buckler(Shield Shove·Counter Stance = Shell Guard 재사용) [x] Chief's War Banner(Rally the Gang = `SummonRunner`) [x] 세트 Outlaw·Iron Raider [x] 맵 `Lv1_BanditFortress`·포털(깊은 숲 분기)·드롭·보스 보상. 커밋은 TowerWorld 15절 참조.
- 변경: 신규 효과 키 `BackstabDmg` 대신 Backstab 판정 자체가 백어택 보너스(치명 +40%·치피 +30%)를 갖고, Outlaw 4세트는 CritDmg 25 + 쿨감. Iron Raider "반격 피해"는 Shield Shove·Counter Stance 쿨감 + 피해 8, "HP 15%" 는 Hp 150(정수 키). Iron Raider 무기는 Raider Buckler 1종(대장 고유템은 세트 밖).

## 3. ⑥ 탑 근교 (부하 40, 1층계 마지막) — ✅ 1차 구현 (2026-10-19, TowerWorld 16절)

| 몹 | 공격 | 계보 무기 | 무기 스킬 |
|---|---|---|---|
| 소형 골렘 | 구르기 돌진 | **Pebble Gauntlet** | A 굴러 돌진 |
| 중형 골렘 | 지면 강타 충격파 | **Quakestone Maul** | A 강타 + 지면 충격파(Cliff Quake 재사용 강화) |
| 대형 골렘 | 슈퍼아머 돌진·바위 투척 | **Monolith Shield** | A 바위 투척(포물선), S 슈퍼아머 버프 |
| 보스 **탑의 문지기** | (스토리: 아버지일 가능성, TowerWorld 2절) | 레전더리 **Gatekeeper's Oath** | 특색 무기 — 문지기 패턴을 플레이어용으로 |

- 세트: **Stoneborn** 2: 방어 20 / 4: 슈퍼아머 지속 +1s / 6: 받는 피해 −10%(신규 키 `DmgReduce`). 1층계 졸업 세트.
- 구현 체크: [x] 몹 5종(소형·중형·대형 골렘·유적 정령·탑의 문지기) — 코드 도트 [x] 대형 골렘 바위 `GolemBoulder`·정령 `RuinBolt`·중형 지면 강타 `GolemQuake` [x] 보스 판정 교체(GateCleave·GateShockwave) [x] 보스 3페이즈 + 2페이즈 소형 골렘 호출 [x] 보스 조우 대사(`BossEncounterTalk`)·칭호 1·탑 입구 개방(`TowerGatePortal`) [x] Pebble Gauntlet(Rolling Charge) [x] Quakestone Maul(Quake Slam·Aftershock = Cliff Quake 강화) [x] Monolith Shield(Boulder Toss 포물선·Monolith Stance 슈퍼아머) [x] Gatekeeper's Oath(Gate Cleave 충격파·Stone Sentinels) [x] 세트 Stoneborn [x] 맵 `Lv1_TowerOutskirts`·포털·드롭·보스 보상. 커밋은 TowerWorld 16절 참조.
- 변경: Stoneborn 4세트 "슈퍼아머 지속 +1s" 대신 계보 무기 3종 쿨 −30·피해 8, 6세트 `DmgReduce` 대신 방어 25·HP 180·피해 10(기존 키만). 슈퍼아머는 Monolith Stance(홀드)가 직접 준다. Gatekeeper's Oath 는 등급 표시를 U(고유)로(레전더리 등급 미구현).

## 4. 신규 시스템 필요 목록 (구현 순서)

1. 세트 효과 키 `LifeSteal`, `BackstabDmg`, `DmgReduce` — `SetEffectApplier` + `DatabaseManager` 필드 + 데미지 공식(`Game/CombatSpec.md`) 반영.
2. ✅ 원거리 무기 타입(활): `Weapon` 옵션 대신 평타 판정 복제본(`BowShot`, 근접 판정 끔)이 `SpawnOnStart` 로 `StraightArrow` 를 쏘는 방식 (2026-10-19, Bandit Shortbow).
3. 몬스터 둔화(점액 장판 등) — 몹 이동 속도 배율 토큰.
4. 소환수 수 증가 세트 효과(`SummonCount <스킬>`) — `SummonSkill` 이 스킬 이름을 알아야 한다.

## 5. 1층계 첫 1시간 장비 구성 (2026-10-31)

Lv1~12 보상 무기 6종·3세트(Pack Hunter·Ancient Grove·Bog Stalker)의 입수 시각, 방어구 레시피, 세트 수치 재조정(4세트 +17~20%, 6세트 +26~29%, 6세트가 4+2의 8% 이내)은 `Docs/Balance/Floor1FirstHourGear.md`. NPC 퀘스트 보상(QuestsFloor1.csv)이 첫 대체 무기를 고정 지급한다. 샛길 장비 드롭은 공급원이 아니다.
