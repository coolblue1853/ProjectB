# 아이템 · 장비 · 세트 스펙

> 상태: 코드·데이터 역산(2026-10-18). 데이터는 로컬 CSV(`Assets/Resources/*.csv`)이며 서버는 쓰지 않는다.

## 1. ItemSheet.csv

1행은 타입, 2행은 헤더다. 조회 키는 `name`이다(`index`는 쓰지 않음).

| # | 컬럼 | 타입 | 설명 |
|---|---|---|---|
| 0 | index | int | 행 번호(미사용) |
| 1 | **name** | string | 고유 키. 프리팹 `Resources/{equipArea}/{name}`, 아이콘 `Resources/Item/{name}`, 세이브 키 |
| 2 | type | string | `Misc` / `Consum` / `Equip` |
| 3 | description | string | 설명. 쉼표 사용 불가 |
| 4 | price | int | 기준가. 판매가 = price × 수량, 구매가 = price × 1.5 |
| 5 | weight | int | 표시만 함 |
| 6 | acqPath | string | 획득 경로 텍스트(표시만 함) |
| 7 | maxStack | int | 슬롯당 최대 수량 |
| 8 | effectOb | string | 소비 효과 키. `"stat sign"`을 `/`로 구분. 키: hp, fullness, stemina, light, def |
| 9 | effectPow | string | 효과 수치. `/`는 effectOb 순서에 대응, `_`는 보조값(def `5_180` = 수치_초) |
| 10 | equipArea | string | Weapon/Head/Chest/Leg/Hand/Shoes/Ring/Necklace, 장비가 아니면 `-` |
| 11 | tfName | string | **세트 키** 겸 방어구 텍스처 이름 `Resources/Sprite/{tfName}` |
| 12 | itemNameT | string | 표시 이름 |
| 13 | tear | int | 티어(전부 0) |
| 14 | rarity | string | C=Common, R=Rare, U=Unique |
| 15 | upgrade | int | 강화(전부 0, 세이브만 함) |
| 16 | setName | string | 세트 표시 이름 |

**현황:**
- 유효 행 81개: 장비 48(무기 15, 방어구 5부위 × 4~5, 반지 6, 목걸이 7), Misc 23, Consum 9, 테스트 행 일부
- placeholder 101행(`name='-'`)도 로드된다.

> 편집: Unity 메뉴 **ProjectB/데이터 편집기**(Odin). 검증 리포트에서 아이콘·프리팹 누락, 등급 누락, 이름 중복, 세트 키 불일치를 확인한다. EditMode `DataIntegrityTests`가 같은 검증을 자동으로 돈다.
> 2026-10-18: 등급이 비어 있던 14개 행을 채웠다(목걸이 2·반지 1은 R, Necklace of Silent Screams는 U, 나머지 C). 아이콘 누락 9개(Rock, PickAxe, test4, Potion, Food, HighArmor, Necklace, Ring, ToxicPickAxe)는 대부분 테스트·미사용 아이템이다.

## 2. 장비 슬롯

| 슬롯 번호 | 1 | 2 | 3 | 5 | 6 | 7 | 8 | 9 |
|---|---|---|---|---|---|---|---|---|
| 부위 | Ring | Head | Necklace | Chest | Hand | Weapon | Leg | Shoes |

- 0번과 4번은 쓰지 않는다.
- 장착하면 프리팹을 플레이어 아래에 만들고, AnyPortrait 메시 텍스처를 `tfName`으로 바꾼다.
- **방어구 스탯(Equipment):** armor, hp, basicDmg, critical, criticalDmg, incDmg, dropRate, attSpeed, moveSpeed, 출혈(확률/피해/간격/횟수/증뎀), poisonDmg, addIncomingDmg, coolDownSkill[]
- **무기 스탯(Weapon):** min/maxDmg, 치명, 치피, 증뎀, 방무, 스킬뎀, 최종뎀, 추가타, skill[4]

## 3. 세트 효과 — SetItem.csv

**형식:**
- 키는 `{tfName}_{장착수}`이다.
- 효과 문법은 `(sign)값 스탯 [스킬명]`이고, 여러 효과는 `/`로 구분한다. 스킬명의 `_`는 공백으로 바뀐다.
- 지원 스탯: Def, Hp, skillHitCount, bulletCount, coolDown

| 키 | 효과 |
|---|---|
| RabbitArmor_2 | Def +10 |
| RabbitArmor_5 | Hp +100 |
| LeafArmor_2 | Rising Roots 타격 +1, Scattering Leaves 탄 +2 |
| LeafArmor_5 | Rising Roots 타격 +2, Haste 쿨감 50 |

**발동 규칙:**
- 장착 수가 정의된 단계와 **정확히 같아지면** 그 단계를 켠다.
- 단계는 누적된다(5셋이면 2셋 효과 + 5셋 효과).
- 해제하면 줄기 전 단계를 끈다.

**세트 구성:**
- RabbitArmor는 방어구 5부위 + 무기(RustedSwordandShield)로 6부위다(Q-102).
- LeafArmor는 5부위다.
- RatArmor와 ScareArmor는 효과 정의가 없다.

**제안 (미반영, Q-102):**

| 키 | 효과 |
|---|---|
| RatArmor_2 | 독 피해 +3 |
| RatArmor_5 | 출혈 확률 +10% / 출혈 증뎀 +10% |
| ScareArmor_2 | 치명 +5% |
| ScareArmor_5 | 밤 동안 최종뎀 +15% (밤 몬스터 파밍용) |

## 4. 인벤토리 · 창고

- **인벤토리:** 설계는 5페이지 × 25칸이지만 씬 설정(maxBoxNum=15)상 **실제로는 15칸, 1페이지**다.
- **획득:** 같은 이름의 덜 찬 스택에 먼저 넣고, 없으면 첫 빈칸에 넣는다. 획득할 때마다 저장한다.
- **창고:** 6×5 × 5페이지 = 150칸.
- **나누기/합치기:** DivideSlider, 드래그로 한다.

## 5. 제작 · 상점

**제작:**
- 레시피는 씬 오브젝트(NeedItem 컴포넌트)에 있다. 오브젝트 이름이 결과물이고, needItem과 needItemAmount 리스트가 재료다.
- 제작 UI는 3종이다: 자가 제작, 무기 대장장이, 방어구 대장장이.
- 재료 패턴은 `T0_Ore + Wood + 몬스터 재료`이고 수량은 3~20개다.
- 개선 방향: 레시피를 CSV(`CraftRecipe.csv`)로 옮겨 데이터로 관리한다.

**상점:**
- 구매가 = `int(price × 1.5)`, 판매가 = price이다.
- 품목은 씬에서 `ItemCheck.isShopItem`으로 표시된 오브젝트다.

## 6. 세이브 — Easy Save 3 (`SaveFile.es3`, 키 `Datas`)

- **저장 항목:**
  - money, playerPos, time
  - invenItem[5,25,4] / chestItem[5,30,4]: 칸마다 이름·수량·티어·강화
  - equipGear[10,3]
- **사용하지 않는 필드:** level, camPos
- **저장 시점:** 획득·매매·인벤토리 닫기·창고 닫기·포털 이동, 그리고 디버그 Space 키 🐞

## 7. 드롭률

- 드롭 판정은 `Random.value ≤ p × (1 + dropRate/100)`이다.
- 몬스터별 드롭 테이블은 `Game/EnemySpec.md` 참조.
