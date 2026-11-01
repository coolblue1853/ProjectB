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
| 15 | upgrade | int | 강화 단계 +0~+10 (CSV 기본 0, 아이템 인스턴스마다 ES3 저장 — `EnhanceRules`, ProgressionDesign 3절) |
| 16 | setName | string | 세트 표시 이름 |
| 17 | WeaponType | string | 무기 계열(10-31, `WeaponKind` 열거형 이름: Sword·Greatsword·Scythe·Dagger·Knuckle·Claw·Spear·Adae·Staff·Axe·Mace·Whip·Dual·Bow·Bulwark …). 무기 행만 채움, 나머지 빈칸. 프리팹 `Weapon._kind` 가 None 이 아니면 일치해야 한다(DataIntegrity). 툴팁 "Weapon (활)" 표시 |

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
- **무기 칸은 항상 찬다(2026-10-26):** 맨손 공격이 없으므로 무기 칸이 비면(새 게임·세이브 로드) 기본 무기 `RustedSword`(ItemSheet id 41)를 자동 장착한다(`InventoryManager.EnsureDefaultWeapon`). 인벤토리에 녹슨 검이 있으면 새로 만들지 않고 그것을 꺼내 장착한다(강화 유지, 복제 방지). 무기는 **해제할 수 없고 교체만** 된다(`InventoryManager.CanUnequip` — 장비창 Z 해제·끌어서 빼기 모두 막음). 예전 1회 지급 플래그 ES3 `starter.weapon.given` 은 더 이상 읽지 않는다.
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
- RatArmor·ScareArmor 효과는 `SetItem.csv`에 정의되어 있다(아래 제안안과 수치가 다름 — CSV가 기준).

**Lv2 세트 Cliffwalker's Vow (`CliffArmor`, 6부위 = 방어구 5 + Cliff 무기):**

| 키 | 효과 |
|---|---|
| CliffArmor_2 | Def +15, Crit +5 |
| CliffArmor_4 | IncDmg +10, Cyclone Edge 쿨감 20, Starfall 쿨감 20 |
| CliffArmor_6 | CritDmg +20, AtkSpeed +10, Skybreaker·Cliff Quake·Talon Dive 쿨감 30 |

- 무기 CliffBlade(16~24, 치명 10, 치피 20 / 스킬: Gale Slash, Cyclone Edge, Skybreaker, Talon Dive), CliffStaff(14~24, 스킬뎀 20 / Arcane Bolt, Starfall, Cliff Quake), 유니크 AbyssStaff(22~34, 스킬뎀 30, 최종뎀 10)
- 스킬 연출은 Scare 무기 복제 상태(임시) → G8(Magic Slashes FX)에서 교체. skillHitCount·bulletCount 강화는 스킬 방식(다단·원형) 확정 후 추가
- 생성·재생성: `Tools/probes/build_lv2_items.cs` (Editor/Dev/Lv2ItemBuilder). 아이콘·텍스처는 기존 에셋 색조 변환 임시본(`Tools/make_lv2_icons.py`)

**(구) 제안 (미반영, Q-102):**

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

### 창고 편의·안전 규칙 (2026-11-01)

- 보관·꺼내기는 `SlotContainer.Transfer`로 **실제로 들어간 수량만** 원본에서 뺀다. 현재 페이지가 찼어도 전체 창고 페이지의 덜 찬 스택·빈칸을 쓴다. 남은 수량은 원래 컨테이너에 보존하고 획득 로그와 같은 안내로 가득 참을 알린다.
- 합치기 조건은 **아이템 ID + 티어 + 강화 동일**이다. 창고 키보드 교환·드래그·정렬도 같은 조건을 쓴다.
- 인벤 쪽 커서에서 **G / 패드 RT**: 선택한 재료·소비품과 같은 ID를 인벤 전체 페이지에서 모두 보관한다. **T / 패드 SELECT**: 모든 재료(`Misc`)만 보관한다. 장비는 개별 이동한다.
- 창고 쪽 커서에서 **R / R3**: 전체 창고를 스택 병합 후 **종류(장비→소비→재료) → 등급(L→U→R→C) → 이름 → 티어·강화 내림차순**으로 정렬한다. 인벤 쪽 R/R3는 기존 현재 페이지 정렬이다.
- **Tab / LB**로 꺼내기 모드를 켠 뒤 **C / Y**: 기존 `ChestDivideSlider`로 1~스택 전체 수량을 고르고 Enter/A로 꺼낸다. 일반 모드 C/Y는 기존 창고 안 나누기다. ESC/B로 수량 선택을 취소한다.
- 창고 하단에 사용/전체 칸 수와 조작 안내를 표시한다(기존 `UiTheme.pixelFont` 재사용). 정렬·일괄 보관은 상세/교환/나누기/드래그 중 실행하지 않는다.
- 이동 완료 직후 `invenItem`과 `chestItem`을 같은 `Datas`에 반영한 뒤 **`DataSave` 1회**로 저장한다. 기존 ES3 필드·배열 형식은 변경하지 않는다. 드래그 중에는 기존 저장 배치를 유지하고 드롭 끝에 양쪽 저장을 요청한다.
- 검증(11-01 재개): Unity EditMode 460/460, 플레이 모델·양쪽 디스크 대조 5,521회, 키보드·가상 패드 입력 11건 통과. 낮 창고·수량 선택 캡처를 직접 검토했다. 마지막 슬롯 줄과 겹치던 안내는 페이지·커서 84% 배율로 하단 66px 공간을 확보했다. 수량 창의 실행 버튼은 꺼내기/나누기로 구분한다. 창고 페이지 간 이동도 원본을 즉시 분리하고 저장한다. 상세·한계는 `Docs/Playtest/ChestReview.md`.

## 5. 제작 · 상점

**제작:**
- 레시피는 씬 오브젝트(NeedItem 컴포넌트)에 있다. 오브젝트 이름이 결과물이고, needItem과 needItemAmount 리스트가 재료다.
- 제작 UI는 3종이다: 자가 제작, 무기 대장장이, 방어구 대장장이.
- 재료 패턴은 `T0_Ore + Wood + 몬스터 재료`이고 수량은 3~20개다.
- Lv2 레시피(2026-10-18): 방어구 대장장이 6(Cliff 방어구 5 + Eye of the Cliffs), 무기 대장장이 3(CliffBlade, CliffStaff, AbyssStaff). 재생성: `Tools/probes/add_lv2_recipes.cs`. Bringer's Sigil Ring 은 10-30 부터 `Resources/ArmorCraftRecipes.csv` 추가 3페이지(BasicFeatures.md 15절). 자가 제작은 10-30 부터 `Resources/SelfCraftRecipes.csv` 추가 페이지(BasicFeatures.md 10절): Roasted Fox Skewer·Grilled Forest Carp·Amber Trout Soup.
- 개선 방향: 레시피를 CSV(`CraftRecipe.csv`)로 옮겨 데이터로 관리한다.

**상점:**
- 구매가 = `int(price × 1.5)`, 판매가 = price이다.
- 품목은 씬에서 `ItemCheck.isShopItem`으로 표시된 오브젝트다.

## 5.1 강화 (대장장이 ↓) — 상세는 `ProgressionDesign.md` 3절

- 재료: `Enhance Stone`(ItemSheet 105, R등급) + 골드. 성공률 100→30%, +6부터 실패 시 −1, 2연속 실패 뒤 확정 성공, 파괴 없음(Q-302).
- 효과: 무기 데미지 단계당 +4%(+5·+10에 +10%씩), 방어구·장신구 HP·기본공격·방어 단계당 +5%(+5·+10에 +10%씩). 방어구 방어력은 ×0.5만 `playerDef`에 반영(B-212).
- 강화석 획득: 정예 확정 1~2개, 보스 확정 3~6개(+첫 처치 5).

## 6. 세이브 — Easy Save 3 (`SaveFile.es3`, 키 `Datas`)

- **저장 항목:**
  - money, level, exp, playerPos, time, mapName(구역 식별자; 구 세이브 미지정이면 활성 맵·높이로 추정)
  - invenItem[5,25,4] / chestItem[5,30,4]: 칸마다 이름·수량·티어·강화
  - equipGear[10,3]
- **별도 키(같은 슬롯 파일):** `quickslot.items`(string[4], 퀵슬롯 등록 아이템 이름, 빈칸 "") — BasicFeatures.md 3절
- **사용하지 않는 필드:** level, camPos
- **저장 시점:** 획득·매매·인벤토리 닫기·창고 닫기·포털 이동·창고 이동 직후(양쪽 동시)·창고 정렬, 그리고 디버그 Space 키 🐞

## 7. 드롭률

- 드롭 판정은 `Random.value ≤ p × (1 + dropRate/100)`이다.
- 몬스터별 드롭 테이블은 `Game/EnemySpec.md` 참조.

- (10-31) 소비 아이템 추가: Small/Medium/Large HP Potion(246~248, hp +120/220/320)·Recall Scroll(249, effectOb `recall +`). 구매가는 price x1.5. 기존 Potion 은 hp +500(고급 물약). 상세는 `ClassicMapleDesign.md` 12절.

- (10-31) `ArmorCraftRecipes.csv` 에 Wolf·Grove·Bog 방어구 15행 추가(방어구 대장장이 추가 페이지, 재료는 구역 35% 드롭 재료). `QuestsFloor1.csv` rewardItem 에 장비 보상(Q102 LeafAdae 등 9건). 세트 수치는 `Docs/Balance/Floor1FirstHourGear.md` §3.

### 착용 중 강화 (2026-11-01)

- 강화 대상은 **착용 중 장비 + 가방 장비**다. 목록 앞의 `[착용]`으로 구별한다. 장비 이동·해제·복제를 거치지 않는다.
- 가방 장비는 기존 `InventoryService.SetUpgrade`, 착용 장비는 `EquipBoxCheck.RefreshEnhancement`를 사용한다. 무기는 `Weapon.SetEnhance`로 공격 배열을 즉시 갱신하고 방어구는 해당 장비 출처의 모디파이어를 다시 계산한다.
- 최대 HP는 새 적용값과 이전 적용값의 차이만 반영한다. 강화 성공으로 현재 HP를 채우지 않으며, 하락으로 최대 HP가 줄면 현재 HP를 최대치 안으로 제한한다. 같은 결과를 다시 반영해도 스탯이 쌓이지 않는다.
- 세트 소속·장착 수는 강화로 변하지 않는다. 세트 모디파이어/스킬/HP 효과를 해제·재적용하지 않고 유지해 중복 발동과 무료 회복을 막는다.
- `SaveEquipItem`이 `Datas.equipGear[슬롯,2]`를 갱신한 뒤 `SaveInventory`로 골드·가방과 함께 저장한다. 세이브 필드·형식은 그대로다.
- ESC 취소는 BasicFeatures의 `UiLock`/`UiBackFrame` 단일 소비 규칙을 따른다.

### 인스턴스 잠금·새 획득 저장 (2026-11-02)

- 기존 `invenItem`/`chestItem`의 4필드, `equipGear`의 3필드는 그대로 유지한다. Datas에 **inventoryFlags(int[,]) / chestFlags(int[,]) / equipmentFlags(int[10])**만 추가한다. 비트0=잠금, 비트1=미열람 새 획득. 구형 세이브의 null/짧은 배열은 0으로 읽는다. 페이지/슬롯 위치는 기존 아이템 배열과 같다.
- ItemData의 isLocked/isNew와 ItemStack의 IsLocked/IsNew를 ViewScanner·SaveMapper·ItemFlagSave가 연결한다. 병합 조건은 **ID+티어+강화+잠금 동일**. 새 표시가 다른 스택은 합칠 수 있고 결과의 새 표시는 OR이다. WithCount·분할·정렬·장착/해제·창고 왕복은 상태를 보존한다. 직접 장착은 새 표시를 끈다.
- 잠금은 판매/분해 보호이며 소비·제작·강화 재료 예약이 아니다. 창고 화면에서도 가방 커서의 잠금을 바꿀 수 있다. 잠금 장비를 사용·교체·강화할 수 있고 착용품 저장에도 잠금을 쓴다.
- J/LT+L3 상점 일괄 판매는 잠금 해제 Misc만 대상으로 가격합을 long으로 확인하고 int 골드 한도를 검사한다. 1회 확인 뒤 기존 InventoryService.RemoveAt·Buyback.Record·저장 배치를 사용한다. 제작/의뢰 재료를 자동 판별해 보존하지 않으므로 필요한 스택은 먼저 L로 잠근다. 되사기 한도는 기존 12건.
- 새 아이템 점은 첫 열람 페이지를 떠날 때 해제. 재료 핀·가격·물약 공유 쿨다운·줍기 수치는 바꾸지 않는다. 상세 비교에는 실제 교체로 꺼지거나 켜질 세트 단계와 효과를 추가한다.
