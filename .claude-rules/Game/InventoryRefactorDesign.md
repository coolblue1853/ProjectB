# 인벤토리 리팩터 설계 (D2~D6)

> 상태: 설계안(2026-10-18, 코드 역산). 구현 전이다. 줄 번호는 커밋 928178f1 기준이다.
> 관련: `Game/ItemSpec.md`(데이터·세이브), `Game/QualityRoadmap.md` D절, `Game/TechDebt.md`.
> 원칙: **단계마다 컴파일·플레이가 되는 상태**를 유지한다. 외부에서 쓰는 API(`CreatItem`, `CountItem`, `HasRoomFor`, `DeletItemByName`, `SaveInventory`)는 마지막 단계까지 시그니처를 유지한다.

---

## 1. 현재 구조

### 1.1 책임 지도

| 클래스 | 줄 수 | 실제로 하는 일 |
|---|---|---|
| `InventoryManager` | 2207 | 싱글턴. 입력 13종, 커서 3종(일반·교체·페이지), 문자열 `state` FSM, 슬롯 생성, 획득·삭제·스택, 장착·해제, 판매, 소비, 나누기, 창고 이동, 툴팁 3종 텍스트 생성, 세이브·로드, 디버그 F1~F6 |
| `Chest` | 1027 | InventoryManager의 **복붙본**(커서·state·나누기·교체·툴팁). 창고 세이브, 창고↔인벤 이동 |
| `ItemCheck` | 194 | 아이템 GameObject. `itemData`(행 복사본) + `nowStack`. 소비 효과 실행. Update마다 수량 텍스트 갱신. 상점·제작 표시도 겸함 |
| `ItemDrag` | 464 | 마우스 드래그. 부모 Transform을 바꿔 **직접** 이동·교환·합치기. 장비칸 해제 일부 |
| `BoxCheck` | 136 | 슬롯마다 Update에서 `childCount`를 보고 `inventoryArray[slot,page]`를 0/1로 맞춤 |
| `EquipBoxCheck` | 287 | 장비칸. 프리팹 생성, 스탯을 `DatabaseManager` static에 가감, 세트 카운트, 장착 세이브. Update에서 `childCount` 폴링으로 활성화 |
| `DivideSlider` / `ChestDivideSlider` | 143 / 144 | 나누기 수량 UI. `InventoryManager.state == "divide"`를 읽음 |
| `SetItem` | 204 | SetItem.csv 로더 겸 효과 적용기. 매번 `new SetItem()`(MonoBehaviour)으로 호출됨 |
| `DatabaseManager` | 275 | CSV 행 목록, 세트 카운트 Dict, 수량 캐시 Dict, 전역 스탯 static |
| `ShopManager` / `CraftingManager` | 496 / 706 | 각자 커서·state. `InventoryManager.state`에 문자열을 직접 씀 |
| `SaveManager` | 83 | ES3 키 `Datas` 단일 객체 |

### 1.2 진실은 어디에 있나 — 세 군데

```
[UI 계층]  inventoryUI[page] / Box(slot) / Item(ItemCheck: itemData, nowStack)   ← 실제 진실
    │ Update 폴링(BoxCheck, 1프레임 지연)
    ▼
[inventoryArray[slot,page] 0/1]   ← 빈칸 판정 일부가 이것만 봄 (CheckBoxCanCreat, BoxContentChecker)
    │ LateUpdate 0.25s 재계산 (RebuildItemStackCache)
    ▼
[DatabaseManager.inventoryItemStack]   ← 제작·상점 표시용 캐시
[EquipBoxCheck 자식 + equipItemCheck[10] + SaveManager.datas.equipGear]  ← 장비 진실도 3중
[DatabaseManager.setsEffectStack]   ← 세트 장착 수(증감식, 누적 오차 위험)
```

- 데이터를 옮기는 방법이 **GameObject 재부모화**다. 부모가 바뀌는 경로마다 세이브·캐시·장비 효과를 따로 맞춰야 하고, 빠진 경로가 버그가 된다(B-004~006이 전부 이 유형).
- 생성 경로가 4개다. `CreatItem`(스택 우선, 세이브), `CreatItemSelected`(페이지 지정, 세이브·배열 갱신 없음), `Chest.CreatItem`(현재 페이지만), `LoadInventory`.
- 최근 수정(`CountItem`, `RebuildItemStackCache`, `HasRoomFor`, `CreatItem→bool`)으로 **읽기**는 슬롯 직접 집계로 통일됐다. **쓰기**는 아직 흩어져 있다.

### 1.3 남은 버그·위험 (TechDebt 미등재분)

| ID(제안) | 등급 | 내용 | 위치 |
|---|---|---|---|
| B-008 | P0 | 창고→인벤, **인벤에 빈칸이 없을 때** 분기: `CheckStack`이 1개를 공짜로 추가(창고 차감 없음)하고, while 안에서 `nowStack -= 1` 후 `CreatItem(isC2I:true)`가 또 `chest.itemCheck.nowStack -= 1` → 창고 2개 소모당 인벤 1개. 0 이하 스택은 Destroy되지 않음. 커서 칸이 비어 있으면 이전 `itemCheck`를 씀(stale/NRE) | `Chest.cs:378-394`, `InventoryManager.cs:1896-1899` |
| B-009 | P0 | 나누기: 원본을 먼저 줄이고 `CreatItemSelected`가 빈칸을 못 찾아도 복구하지 않음 → 페이지가 가득 차면 나눈 수량 소실 | `InventoryManager.cs:361-368`, `Chest.cs:488-495` |
| B-010 | P1 | `CreatItemSelected`가 `childCount`를 보지 않고 `inventoryArray`도 1로 만들지 않음 → 같은 프레임 두 번 호출 시 한 칸에 2개 겹침 | `InventoryManager.cs:2113-2132`, `Chest.cs:755-774` |
| B-011 | P1 | 목걸이 교체 시 부위 문자열 오타 `"Neckles"` → `moveItem` null → NRE | `InventoryManager.cs:975, 981` (`ItemDrag.cs:398`도 동일, 현재 죽은 경로) |
| B-012 | P1 | 장비칸에서 드래그해 **차 있는** 인벤 칸에 놓으면 교환 분기로 인벤 아이템이 장비칸에 들어감(부위 검사 없음). 이후 `EquipBoxCheck.Update`가 자식을 감지해 `ActivePrefab` → 잘못된 프리팹 로드/에러 | `ItemDrag.cs:154-192`, `EquipBoxCheck.cs:101-109, 276-282` |
| B-013 | P1 | 장비칸에서 페이지 탭(UpperBox)으로 드롭하면 해제 처리 없이 Destroy → 스탯·프리팹 잔존, `equipGear` 세이브 잔존(재접속 시 부활) | `ItemDrag.cs:206-221` |
| B-014 | P1 | **티어·강화 소실**: 이름만 넘겨 재생성하는 경로들. 해제(`CreatItem(detail.name)`), 페이지 이동, 나누기, 창고 이동. 스택 합치기도 이름만 비교 | `InventoryManager.cs:297, 812, 2126`, `Chest.cs:367, 685, 768, 973`, `ItemDrag.cs:163, 218, 246`, `InventoryManager.cs:399, 2098` |
| B-015 | P1 | 창고→인벤 이동은 아이템마다 인벤을 즉시 저장(`CreatItem`→`SaveInventory`)하지만 창고는 닫을 때만 저장 → 창고를 연 채 강제 종료하면 복제 | `InventoryManager.cs:1902`, `Chest.cs:665` |
| B-016 | P2 | 툴팁 음수 부호 중복: `(v > 0 ? "+" : "-") + v` → `--5` | `InventoryManager.cs:1671-1708, 1730-1807` |
| B-017 | P2 | 툴팁을 열 때마다 `Resources.Load` 하고 결과 null 체크 없음(프리팹 없는 장비 NRE) | `InventoryManager.cs:1467, 1479, 1507, 1520` |
| B-018 | P2 | 상점 구매 슬라이더: 돈 < 가격이면 max 0 < min 1 → 1개 외상 구매 가능성. `buyprice` 0이면 0 나누기. 리스너 누적(B-208) | `ShopManager.cs:262-263, 272-279`, `InventoryManager.cs:1023` |
| B-019 | P2 | 연산자 우선순위: `a.triggered \|\| (… ) && 범위검사` → triggered면 범위 무시(B-208 구체 위치) | `InventoryManager.cs:1064, 1068`, `DivideSlider.cs:106, 110`, `ShopManager.cs:298` |
| B-020 | P2 | 세이브 로드 `int.Parse` → 손상 세이브에서 크래시 | `InventoryManager.cs:215-217, 824-825`, `Chest.cs:178-180` |
| B-021 | P3 | 레거시 Input: 페이지 숫자키, **디버그 F1~F6 아이템 생성(빌드에 치트 남음)** | `InventoryManager.cs:604-620, 1169-1224`, `ItemDrag.cs:147` 외 |
| B-022 | P3 | 획득·구매 1개마다 ES3 전체 저장(N개 구매 = N회 디스크 쓰기) | `InventoryManager.cs:1902`, `ShopManager.cs:272-278` |
| B-023 | P3 | `ItemCheck.name` 필드가 `Object.name`을 가림. 매 프레임 텍스트·SetActive 갱신 | `ItemCheck.cs:15, 121-135` |
| — | 위험 | `BoxFullCheck()`는 이름과 반대로 **빈칸이 있으면 true** | `InventoryManager.cs:1849` |
| — | 위험 | `UnUseEquipment`가 빈 DOTween 시퀀스 OnComplete 타이밍에 의존해 장비칸 자식을 Destroy | `InventoryManager.cs:809-810`, `EquipBoxCheck.cs:224-230` |
| — | 위험 | "창고 열림"이 모드가 아니라 **문맥**인데 `state`에 섞여 있음. `chest != null ? "chestOpen" : ""` 패턴 9곳. Shop·Chest가 `InventoryManager.state`에 문자열을 직접 씀 | 다수 |

> 이 표는 TechDebt에 옮겨 적는다(P0/P1은 Phase 0에서 현 구조 그대로 먼저 고친다).

---

## 2. 목표 구조

```
            ┌──────────── 순수 C# (EditMode 테스트 대상, UnityEngine 의존 최소) ────────────┐
 CSV ──▶ ItemCatalog(IItemCatalog) ──▶ SlotContainer ◀── InventoryService(명령: Add/Remove/Move/Split/Transfer/Sort)
                                         ▲   │ OnSlotChanged(index) / OnItemAdded(stack, source)
 Datas(ES3) ◀──▶ SaveMapper ─────────────┘   │
                                         EquipmentModel ──▶ SetBonusEvaluator ──▶ SetBonusDiff
            └───────────────────────────────────────────────────────────────────────────────┘
                     │ 이벤트                         │ 이벤트
            ┌────────▼─────────┐            ┌─────────▼──────────┐
            │ ContainerView     │            │ EquipmentApplier    │  프리팹 생성, 스탯 가감, 텍스처
            │ (SlotView × N)    │            │ (EquipBoxCheck 후신) │
            └────────▲─────────┘            └────────────────────┘
            InventoryInput(enum FSM) / ItemDrag → InventoryService 명령만 호출
```

### 2.1 데이터 타입 (`Assets/Script/Inventory/Model/`)

```csharp
public readonly struct ItemStack            // 값 타입. 빈칸은 ItemStack.Empty(id == null)
{
    public readonly string Id; public readonly int Count, Tier, Upgrade;
    public bool IsEmpty => Id == null || Count <= 0;
    public bool CanStackWith(ItemStack o) => Id == o.Id && Tier == o.Tier && Upgrade == o.Upgrade;
    public ItemStack WithCount(int c);
}
public interface IItemCatalog { ItemDef Get(string id); bool Exists(string id); }   // 테스트는 FakeCatalog
public sealed class ItemDef { Id, Type, MaxStack, EquipArea, SetKey(tfName), Rarity, Price, DisplayName, SetName, ... }
```

- `ItemDef`는 기존 `ItemData`를 감싼 읽기 전용 뷰다. 인스턴스 값(Tier/Upgrade/Count)은 `ItemStack`에만 둔다. `ItemCheck.itemData.tear` 같은 인스턴스별 행 복사는 없앤다.
- 합치기 조건은 **id + tier + upgrade 동일**이다(B-014 근본 해결).

### 2.2 `SlotContainer` — 인벤토리·창고 공용

```csharp
public sealed class SlotContainer
{
    public SlotContainer(IItemCatalog catalog, int capacity, int unlocked);
    public int Capacity { get; }  public int Unlocked { get; set; }     // 15 → 25 확장은 Unlocked만 바꾼다
    public ItemStack this[int index] { get; }
    public event Action<int> OnSlotChanged;                             // 변경된 칸마다 1회
    public int  Count(string id);
    public int  SpaceFor(ItemStack s);                                  // 넣을 수 있는 최대 수량
    public int  TryAdd(ItemStack s);          // 덜 찬 스택 → 첫 빈칸 순. 넣은 수량 반환. 0이면 상태 불변
    public bool TryAddAll(ItemStack s);       // 전부 들어갈 때만 넣음(원자적)
    public bool TryRemove(string id, int n);  // 부족하면 false, 상태 불변. 뒤 칸부터? → 앞 칸부터(현행 유지)
    public bool Move(int from, int to);       // 빈칸=이동, 합칠 수 있으면 합치고 넘침은 원위치, 아니면 교환
    public bool Split(int index, int amount, int preferPage = -1);  // 1 ≤ amount < count, 빈칸 없으면 false(B-009)
    public void Sort(IComparer<ItemStack> cmp);                         // 병합 후 정렬
    public static int Transfer(SlotContainer src, int index, SlotContainer dst, int amount); // 옮긴 수량만 원본 차감
}
```

- **모든 실패는 상태 불변 + false/0**이다. 이벤트는 성공한 변경에만 발생한다.
- 인벤토리 = `new SlotContainer(cat, 5*25, unlocked: 15)`, 창고 = `new SlotContainer(cat, 5*30, 150)`. 페이지는 뷰 개념이다(`index = page * perPage + slot`).

### 2.3 `InventoryService` (MonoBehaviour 파사드, 기존 `InventoryManager` 자리)

- `Inventory`, `Chest`, `Equipment` 모델을 소유하고 명령 메서드만 노출한다.
- 기존 API를 래핑한다: `CreatItem(name)` → `TryAdd(new ItemStack(name,1,0,0))`, `CountItem` → `Inventory.Count`, `HasRoomFor` → `SpaceFor > 0`, `DeletItemByName` → `TryRemove`.
- 세이브는 **dirty 플래그 + 디바운스(0.5s) + 즉시 저장 지점(창 닫기·포털·종료)**. 인벤·창고·장비를 **한 번의 `DataSave`**로 쓴다(B-015, B-022).
- `OnItemAdded(ItemStack, AddSource{Pickup, Craft, Shop, Chest, Unequip, Load})` → 픽업 로그(D6)가 구독한다.

### 2.4 뷰 바인딩 — 기존 GameObject 재사용

- 슬롯 박스(`inventoryBoxPrefab` 인스턴스)에 `SlotView { int Index; Bind(ItemStack, ItemDef) }`를 붙인다. 아이템 GameObject(`itemPrefab`)는 **아이콘 표시 전용**이 되고, 슬롯마다 1개를 미리 만들어 켜고 끈다(Instantiate/Destroy 제거).
- `ContainerView`가 `OnSlotChanged(i)`를 구독해 해당 `SlotView`만 갱신한다. 수량 텍스트는 바뀔 때만 쓴다(B-023).
- `BoxCheck.Update` 폴링, `inventoryArray`, `inventoryItemStack` 캐시는 마지막 단계에서 삭제한다. 제작·상점 표시는 `Count(id)`를 직접 호출한다.
- `ItemDrag`는 드롭 시 **재부모화하지 않고** `Service.Move/Transfer/Equip`를 호출한 뒤 드래그 아이콘을 원위치시킨다. 결과는 이벤트로 그려진다(B-012/013 구조적 해결).

### 2.5 입력 — enum FSM (`InventoryInput`)

문맥과 모드를 분리한다.

```csharp
[Flags] enum UiContext { None=0, Chest=1, Shop=2 }            // 열린 창
enum UiFocus { Inventory, Equipment, PageTabs, Chest, Shop }   // 커서가 있는 패널
enum UiMode  { Browse, Detail, Swap, SwapPageTabs, Divide, Sell, Buy, MoveToChest, MoveToInventory }
```

| 현재 문자열 | 목표 |
|---|---|
| `""`, `"chestOpen"` | `Mode=Browse` (+ `Context.Chest`) |
| `"detail"` / `"Equipment"` | `Mode=Detail` / `Focus=Equipment` |
| `"change"` / `"itemBoxChange"` | `Mode=Swap` / `Mode=SwapPageTabs` |
| `"boxChange"` | `Focus=PageTabs` |
| `"divide"` / `"Sell"` / `"BuyDetail"` | `Mode=Divide` / `Sell` / `Buy` |
| `"I2CMove"` / `"InChestMove"` | `Mode=MoveToChest` / `Focus=Chest` |
| `"ShopCusorOn"` | `Focus=Shop` |

- 전이는 `Enter(mode)` / `Back()` 한 곳에서만 한다. `Back()`이 "chest면 chestOpen, 아니면 ''" 중복 9곳을 대체한다.
- Chest·Shop·DivideSlider는 `state` 문자열 대신 `Input.Mode == UiMode.Divide` 같은 읽기와 `Input.Enter/Back` 호출만 한다.
- 커서 이동(그리드 경계, 페이지 탭, 장비칸 3×3 이동표)은 `GridCursor` 순수 클래스로 분리해 인벤·창고가 공유한다(Chest 복붙 제거).
- 레거시 `Input.GetKeyDown`을 없애고 페이지 숫자키는 `KeyAction`에 추가한다. 디버그 아이템 생성은 `#if UNITY_EDITOR`의 Dev 메뉴(`Assets/Editor/Dev`)로 옮긴다.

### 2.6 장비 모델 · 세트 평가기

```csharp
public enum EquipSlot { Ring=1, Head=2, Necklace=3, Chest=5, Hand=6, Weapon=7, Leg=8, Shoes=9 }  // 세이브 인덱스 그대로
public sealed class EquipmentModel
{
    public ItemStack this[EquipSlot s] { get; }
    public event Action<EquipSlot, ItemStack /*old*/, ItemStack /*new*/> OnChanged;
    public static bool TryGetSlot(ItemDef def, out EquipSlot slot);     // "Necklace" 문자열 비교는 여기 한 곳(B-011)
}
// InventoryService.Equip(invIndex): 부위 검사 → 기존 장비를 그 칸으로(교환) → OnChanged
// InventoryService.Unequip(slot): 인벤 SpaceFor 확인 → 실패면 거절(티어·강화 보존)
```

```csharp
public static class SetBonusEvaluator   // 순수 함수
{
    // 장착 아이템의 SetKey별 개수
    public static Dictionary<string,int> CountPieces(IEnumerable<ItemDef> equipped);
    // 정의된 단계 중 count 이하인 것 전부(누적 규칙): LeafArmor 5개 → {2,5}
    public static IReadOnlyList<int> ActiveTiers(string setKey, int count, SetTable table);
    // 이전/이후 활성 단계 비교 → 적용할 것·해제할 것
    public static SetBonusDiff Diff(IReadOnlyDictionary<string,int> before, IReadOnlyDictionary<string,int> after, SetTable table);
}
public sealed class SetTable { /* "RabbitArmor_2" → List<SetEffect>; 세트별 정의 단계, 세트별 총 부위 수(ItemSheet에서 SetKey 행 수) */ }
public readonly struct SetEffect { char Sign; int Value; string Stat; string Skill; }   // SetEffectParser가 생성
```

- 현재 `PlusSetDict/MinusSetDict`(증감 카운터)는 "정확히 같아질 때" 규칙과 누적 해제 순서에 의존한다. 평가기는 **매번 장착 상태 전체에서 다시 계산**하고 차이만 적용하므로 카운터 오차가 생기지 않는다. 결과는 `ItemSpec.md` 3절 규칙과 같다(누적, 6부위 RabbitArmor는 5단계까지).
- `EquipmentApplier`(EquipBoxCheck 후신)는 `OnChanged`에서 프리팹 생성·파괴·텍스처·스탯 가감을 하고, `SetBonusDiff`를 받아 `SetItem.ActiveSetEffect`의 적용부를 호출한다. 스탯 static 정리는 로드맵 B5(Modifier)에서 한다.
- 평가 결과(`세트별 count`, `활성 단계`)는 툴팁·장비창 세트 표시(D5)가 그대로 읽는다.

### 2.7 세이브 DTO 매핑 — `Datas` 하위 호환

`SaveMapper`(순수)가 기존 필드를 **그대로** 읽고 쓴다. 필드 이름·차원은 바꾸지 않는다.

| 필드 | 형식 | 매핑 |
|---|---|---|
| `invenItem` | `string[5,25,4]` = [page, slot, {name, count, tier, upgrade}] | `index = page*25 + slot`. 빈칸 = `"non"` 또는 null. 저장 시 빈칸은 `"non"` |
| `chestItem` | `string[5,30,4]` | `index = page*30 + slot` |
| `equipGear` | `string[10,3]` = [EquipSlot, {name, tier, upgrade}] | 0·4번은 항상 비움. 빈칸 = null 또는 `""` |
| `money` | int | 그대로 |
| *(추가)* `saveVersion` | int, 기본 0 | 0 = 레거시. 1 = 모델 저장. ES3는 없는 필드를 기본값으로 둔다 |

- **파싱은 TryParse**다. 수량 ≤ 0 → 빈칸, tier/upgrade 실패 → 0(B-020).
- **CSV에 없는 id**는 버리지 않는다. 원문 문자열을 `ItemStack`에 그대로 보존(`Unknown` 표시)해서 다시 저장해도 사라지지 않게 한다.
- 잠긴 칸(Unlocked 밖)에 있는 세이브 아이템은 첫 빈칸으로 옮기고, 자리가 없으면 원래 칸에 둔 채 경고 로그를 남긴다(소실 금지).
- 15 → 25 확장은 배열 폭이 이미 25라서 세이브 변경이 필요 없다.

---

## 3. UI 스펙

### 3.1 디아식 툴팁 (D3)

`TooltipBuilder.Build(ctx) → List<TooltipLine{ Text, Color, Style }>`(순수). 뷰는 TMP 한 개에 리치 텍스트로 그린다(현재 고정 자식 인덱스 `GetChild(0..8)` 방식 폐기).

| 순서 | 줄 | 규칙 |
|---|---|---|
| 1 | 이름 | 등급색. 강화 시 `+3 ` 접두. 세트 아이템이면 이름은 등급색, 2줄의 세트명은 세트색 |
| 2 | 등급 · 부위 · 티어 | `희귀 장갑  T0` / 무기는 `subType`(예: 단검) |
| 3 | 주 스탯 | 무기: `10 ~ 20 공격력`(크게). 방어구: `방어 +5`(크게) |
| 4 | 부가 스탯 | 파란색. 0인 스탯 생략. 부호는 값에서 한 번만(B-016). 출혈·독·쿨감 포함 |
| 5 | 비교 | 같은 부위 착용품이 있고 이 아이템이 미착용일 때: 각 줄 끝에 `▲+3`(초록)/`▼-2`(빨강). 착용품에만 있는 스탯은 `-X 스탯` 빨강 줄로 추가. 무기는 평균 피해 `(min+max)/2` 비교 |
| 6 | 무기 스킬 | 최대 4칸: 아이콘·이름·스태미나·쿨타임. 세트로 강화되는 스킬은 `쿨 12s → 6s`, `타격 +1 (세트)`를 초록으로 표시(게임 방향: 세트가 무기 스킬을 강화) |
| 7 | 세트 | `< Nature's Reward > (2/5)` — 세트색. 부위 목록: 착용 부위 흰색, 미착용 회색. 단계: `(2) 효과` 활성은 세트색, 비활성은 회색 |
| 8 | 설명 | 기울임, 회색. `flavor`가 있으면 그것, 없으면 `description` |
| 9 | 하단 | `판매 100G` / 상점에서는 `구매 150G`. 스택이면 `수량 12/99` |

- 소비: 3~4 대신 효과 줄(`HP +30`, `방어 +5 (180초)`). 재료: `제작 재료`(레시피 CSV 도입 후 "사용처" 추가).
- 비교 패널: Shift(게임패드 LT)를 누르는 동안 착용품 툴팁을 옆에 하나 더 띄운다.
- 색 토큰(한 곳에 상수로): C `#D8D8D8`, R `#5A8CFF`, U `#E0A53C`, 세트 `#3FCF6A`, 비활성 `#7A7A7A`, 상승 `#4CD964`, 하락 `#FF5A5A`.
- 스탯 원천: 장비 프리팹의 `Equipment`/`Weapon` 컴포넌트를 **이름별 1회 로드 후 캐시**한 `StatBlock`(순수 struct)으로 바꿔 쓴다(B-017). 툴팁·비교·정렬이 같은 `StatBlock`을 쓴다.

### 3.2 정렬 · 확장 (D4)

- **정렬 키:** 정렬 버튼 + `KeyAction.UI.Sort`(키보드 R, 게임패드 Y). 현재 페이지만 정렬한다. 창고도 동일하게 쓴다.
- **정렬 순서:** 먼저 같은 스택을 병합한다. 그다음 아래 순서로 비교한다.
  1. 타입: Equip → Consum → Misc
  2. 장비 부위: Weapon, Head, Chest, Hand, Leg, Shoes, Ring, Necklace
  3. 등급: U → R → C
  4. 티어 내림차순, 강화 내림차순
  5. 이름
- `ItemSortComparer`는 순수 클래스이고 테스트 대상이다.
- **15 → 25칸:** 씬의 `maxBoxNum`을 25로 바꾸면 5×5 한 페이지가 된다. 세이브 폭은 이미 25다. 페이지 2~5는 `Unlocked`로 잠가 두고, 나중에 가방 확장 보상으로 연다.
- **페이지 탭:** 잠긴 탭은 자물쇠 아이콘을 표시하고 선택할 수 없다.

### 3.3 픽업 로그 (D6)

> **구현 상태(2026-10-18):** `Assets/Script/UI/PickupLog/` — `PickupLogModel`(순수)·`PickupLog`(뷰, 코드 생성 캔버스)·`UiTheme`(Resources/UI). `DropItemCheck`의 `CreatItem` 결과에 직접 연결. 세트 아이템 강조는 아직 없음(R/U만), 경고 문구는 한글 TMP 폰트가 없어 영어 "Inventory Full"(G7과 함께 정리).

- 좌측 하단에 최대 5줄을 표시한다. 줄 형식은 `[아이콘] 이름 ×N`이고, 이름은 등급색이다.
- 같은 아이템을 1.5초 안에 다시 얻으면 줄을 새로 만들지 않고 기존 줄의 N을 올린다. 그 줄의 수명도 다시 시작한다.
- 수명은 3초다. 마지막 0.5초 동안 페이드한다.
- R/U 등급과 세트 아이템은 강조한다. 수명 5초, 테두리 글로우, 획득음을 쓴다.
- 인벤토리가 가득 차서 획득이 거절되면 빨간 `인벤토리가 가득 찼습니다`를 띄운다. 2초 스로틀을 둔다. 현재 `DropItemCheck`는 이 경우 조용히 무시한다.
- `PickupLogModel`은 순수 클래스다. 시간을 주입받아 병합·만료·최대 줄 수를 처리한다. 뷰는 줄 6개를 풀로 쓴다. DOTween은 `Kill` 후 `SetLink`로 건다.
- 연결은 `Service.OnItemAdded`에서 `source == Pickup`일 때만 한다. 그 전 단계라면 `DropItemCheck.cs:78`의 `CreatItem` 결과에 직접 연결해도 된다.

### 3.4 CSV 추가 컬럼 (ItemSheet.csv **맨 뒤**에만 추가)

| # | 컬럼 | 타입 | 기본값 | 용도 |
|---|---|---|---|---|
| 17 | subType | string | `-` | 무기 계열(Sword/Dagger/Staff/Knuckle/Spear/Scythe/Claw)·방어구 재질. 툴팁 2줄, 정렬 보조 |
| 18 | flavor | string | 빈칸 | 툴팁 기울임 문구. 쉼표 불가(description과 같은 제약) |
| 19 | sortOrder | int | 0 | 같은 키끼리 수동 정렬 우선순위(선택). 0이면 무시 |

- **SetItem.csv:** 맨 뒤에 `displayText`(string, 선택) 컬럼을 추가한다. 비어 있으면 `SetEffectParser` 결과로 문구를 자동 생성한다. 툴팁 7줄에 쓴다.
- 세트 총 부위 수는 컬럼을 추가하지 않고, ItemSheet에서 같은 `tfName` 행 수로 계산한다. 그러면 RabbitArmor = 6이다(Q-102).
- 세트 정의 단계도 SetItem.csv 키에서 유도한다.
- 컬럼을 추가할 때는 `ItemDatabase.LoadItemsFromCSV`에 case 17~19를 추가한다. 빈 값은 기본값으로 처리한다(B-209와 함께).
- `ItemSpec.md` 1절 표도 갱신한다.

---

## 4. 단계별 이행

각 단계는 독립 커밋이다. 끝나면 컴파일, EditMode 테스트, 플레이 프로브를 돌린다. 프로브는 `Tools/probes/*.cs` 형식이다(`inv_test.cs` 참고).

### Phase 0 — 현 구조에서 P0/P1 먼저 고치기 (모델 없이)

- **B-008:** `C2IMove`의 "빈칸 없음" 분기를 `while (OnlyCheckStack) { if (!CreatItem(id, true)) break; }`로 바꾼다. 수동 `nowStack -= 1`과 선행 `CheckStack`은 제거하고, 0 이하 스택은 Destroy한다. 커서 칸이 비어 있으면 return한다.
- **B-009/010:** `CreatItemSelected`가 bool을 반환하게 한다. `childCount == 0`도 함께 검사하고, 배열을 즉시 1로 만든다. `DownNowStack`은 생성에 성공한 다음에만 원본을 차감한다.
- **B-011:** `"Neckles"`를 `"Necklace"`로 바꾼다. 두 파일 모두 고친다.
- **B-012/013:** 장비칸에서 시작한 드래그는 빈 인벤 칸 드롭만 허용한다. 차 있는 칸이나 페이지 탭에 놓으면 원위치시킨다.
- **B-016/019:** 부호 처리와 괄호를 고친다.
- **프로브:** `inv_c2i_full.cs`는 인벤을 가득 채운 뒤 덜 찬 스택으로 창고에서 이동하고, 창고 차감량과 인벤 증가량이 같은지 본다. `inv_divide_full.cs`는 페이지가 가득 찬 상태에서 나누고, 총량이 그대로인지 본다.

### Phase 1 — 순수 모델 + 테스트 (씬 연결 없음)

> **진행(2026-10-18):** ✅ `ItemStack`·`ItemDef`·`IItemCatalog`·`CsvItemCatalog`·`SlotContainer`(TryAdd/TryAddAll/TryRemove/Move/Split/Sort/Transfer, 이벤트) + `SlotContainerTests` 21개(SlotContainer·Transfer 목록 전부). ✅ `EquipSlot`·`EquipmentModel`·`SetBonusEvaluator`·`SetTierTable`(데이터 편집기 `SetTable`과 이름 충돌로 변경) + `SetBonusAndEquipmentTests` 14개. 파서는 기존 `SetEffectApplier.Parse` 재사용. ⏳ `SaveMapper`·`ItemSortComparer`, Phase 2(서비스 연결).

- **추가할 것:** `ItemStack`, `ItemDef`, `IItemCatalog`(+ CSV 구현 `CsvItemCatalog`), `SlotContainer`, `EquipSlot`, `EquipmentModel`, `SetEffectParser`, `SetTable`, `SetBonusEvaluator`, `SaveMapper`, `ItemSortComparer`
- **위치:** `Assets/Script/Inventory/Model/`. asmdef 없이 Assembly-CSharp에 둔다. 그래야 기존 `Assets/Editor/Tests`에서 바로 참조된다.
- **EditMode 테스트** (`Assets/Editor/Tests/`, `FakeCatalog`를 쓴다):
  - **`SlotContainerTests`**
    - 빈 컨테이너에 넣으면 첫 칸에 들어간다.
    - 앞에 빈칸이 있어도 덜 찬 스택에 먼저 넣는다.
    - maxStack을 넘으면 다음 칸으로 넘친다(25 → 10/10/5).
    - 가득 차면 0을 반환하고 상태가 바뀌지 않는다.
    - `TryAddAll`은 일부만 들어갈 수 있으면 아무것도 넣지 않는다.
    - 티어·강화가 다르면 합치지 않는다.
    - `Unlocked` 밖 칸은 쓰지 않는다.
    - 제거가 여러 칸에 걸쳐 차감되고 빈칸을 정리한다.
    - 수량이 부족하면 제거가 실패하고 상태가 바뀌지 않는다.
    - Count가 합계를 낸다.
    - Move는 빈칸으로 이동한다.
    - Move는 다른 아이템끼리 교환한다.
    - Move는 같은 아이템을 전부 합친다.
    - Move는 합치다 넘친 양을 원위치에 남긴다.
    - Move에서 가득 찬 스택끼리는 교환한다.
    - Split은 빈칸에 나눈다.
    - Split은 빈칸이 없으면 실패하고 총량을 보존한다(B-009 회귀).
    - Split amount는 1 이상 count 미만만 허용한다.
    - 성공한 칸만 이벤트를 낸다.
    - 실패하면 이벤트가 없다.
  - **`TransferTests`**
    - 창고로 전부 옮긴다.
    - 대상이 가득 차면 넣은 만큼만 원본에서 차감한다(B-006/008 회귀).
    - 티어·강화를 보존한다(B-014 회귀).
  - **`SetBonusEvaluatorTests`**
    - 1셋은 비활성이다.
    - 2셋은 2단계가 켜진다.
    - 5셋은 2단계와 5단계가 누적으로 켜진다.
    - RabbitArmor 6부위는 5단계까지 켜진다.
    - 5 → 4가 되면 5단계만 해제된다.
    - 빈 tfName은 무시한다(B-112 회귀).
    - 효과 정의가 없는 세트(Rat)는 진행도만 낸다.
    - Diff는 적용·해제 목록을 정확히 낸다.
    - 같은 상태로 두 번 평가하면 Diff가 비어 있다.
  - **`SetEffectParserTests`**
    - `(+)10 Def`를 파싱한다.
    - 스킬명의 `_`를 공백으로 바꾼다.
    - `/`로 구분된 다중 효과를 파싱한다.
    - 잘못된 문법은 예외 없이 건너뛴다.
  - **`EquipmentModelTests`**
    - 빈 슬롯에 장착한다.
    - 교체하면 이전 장비를 반환한다.
    - 부위가 맞지 않으면 거절한다.
    - Necklace가 슬롯 3에 매핑된다.
    - 인벤이 가득 차면 해제를 거절한다.
  - **`SaveMapperTests`**
    - 인벤 왕복(저장 → 로드)이 일치한다.
    - `"non"`과 null을 빈칸으로 읽는다.
    - 손상된 수량은 빈칸으로 처리한다.
    - 모르는 id를 보존한다.
    - equipGear 슬롯 번호를 유지한다.
    - 레거시 고정 데이터(15칸 사용)를 로드한다.
    - 25칸으로 확장해도 기존 위치를 유지한다.
  - **`ItemSortComparerTests`**
    - 타입 → 부위 → 등급 → 티어 → 이름 순으로 정렬한다.
    - 병합 후 정렬한다.

### Phase 2 — 브리지: UI에서 모델 읽기

> **진행(2026-10-18):** ✅ `SaveMapper`(TryParse·원문 보존, `SaveMapperTests` 4개) + `ViewScanner.BuildFrom` + `CsvItemCatalog.Shared`. `SaveInventory`·`Chest.SaveChest`가 스캐너→매퍼로 저장하고, `LoadInventory`·`LoadChest`는 `ReadCell`로 읽는다(int.Parse 크래시 제거, B-020). 프로브 `inv_model_sync.cs`(불일치 0, 저장 왕복 0)·`chest_save_test.cs`(왕복 0, 창고가 비어 있어 아이템 있는 경우는 미검증). `CountItem`·`HasRoomFor`는 이미 슬롯 직접 집계라 유지.

- `ViewScanner.BuildFrom(inventoryUI)`가 현재 슬롯 GameObject로 `SlotContainer`를 만든다.
- 다음 기능을 스캐너 + 모델로 바꾼다.
  - `SaveInventory`, `Chest.SaveChest`: 둘 다 `SaveMapper`를 쓴다.
  - `CountItem`, `HasRoomFor`, `RebuildItemStackCache`
- 진실은 아직 UI다. 동작은 바뀌지 않지만 세이브 형식은 모델 경로로 검증된다.
- **프로브:** `inv_model_sync.cs`는 스캐너 모델과 UI 슬롯을 칸 단위로 비교하고, 불일치 0을 확인한다. `save_roundtrip.cs`는 저장 → `Datas` 복제 → 매퍼 로드를 거쳐 칸 단위로 비교한다.

### Phase 3 — 진실을 모델로 옮기기 (인벤토리)

> **진행(2026-10-18):** 🔶 3a 1차 — `InventoryService`(Add/Remove → 모델 명령 → 바뀐 칸만 `DrawSlot`, 명령 전 `SyncModelFromViews` 브리지, `OnItemAdded(ItemStack, AddSource)` 이벤트). `CreatItem`·`DeletItemByName`이 서비스를 쓴다. 프로브 `inv_test`(기존과 동일 결과)·`inv_model_sync`(불일치 0). 2차: `LoadInventory` → `Service.LoadFrom`(매퍼 → 전 칸 그리기, 열린 칸 밖 세이브 아이템은 경고·보존), `CreatItem(source)`로 `AddSource` 전달(드롭·제작·상점·창고), `DeletItemByName` 즉시 저장. 남은 것: 소비·판매·`CreatItemSelected` 이전, `SlotView` 바인딩, 픽업 로그 구독. ✅ 열린 칸 밖 세이브 아이템은 `hidden`에 보관해 `BuildSaveArray`가 다시 쓴다(프로브 `inv_hidden_keep`). 판매·소비는 `RemoveAt`(aa63a13f). ✅ 3a 마무리(10-30): 소비는 `RemoveAt` 성공 후에만 효과(먼저 빼고 효과), 판매는 `RemoveAt` 성공 시에만 돈 지급(예외 경로의 직접 차감 삭제, 죽은 `ItemCheck.ConsumItemActive` 삭제). `CreatItemSelected` → `Service.PutInPage`(지정 페이지 첫 빈칸에 그대로, 티어·강화 보존, 데이터 사본), 키보드 페이지 이동 `BoxChange` → `Service.MoveToPage`, 나누기 `DownNowStack` → `Service.SplitAt`(지정 페이지 우선, 가득 차면 다른 페이지 빈칸, Q-429). 드래그 페이지 탭 드롭은 성공 시에만 원본 삭제 + 인벤(출발이 창고면 창고도) 저장, 실패 시 원래 칸 복귀. 프로브 `inv_phase3a_paths`(소비 -1·나누기 총량 보존·놓기 t2u3·판매 +돈/초과 판매 거절·배열/뷰 불일치 0). 키보드 페이지 이동·페이지 탭 드래그는 2~5페이지가 잠겨 있어 실제 이동은 미검증. 리뷰 반영: `PutInPage` 는 시트에 없는 이름을 미리 거절(성공 보고 후 원본 삭제 → 소실 방지)·`source` 를 주면 `OnItemAdded` 발행(창고발 = Chest), 페이지 탭 드롭(인벤↔창고 양방향)은 `SaveInventoryAndChest` 로 디스크 1회 저장, 소비는 차감 저장 후 효과. 드래그 임시 구멍은 B-219. 저장은 여전히 스캐너→매퍼(`BuildSaveArray`)다 — 모델 소유 저장은 드래그·키보드 교환이 GameObject 를 직접 옮기는 동안 진실이 UI 라서 3c 이후로 미룬다. 남은 것: `SlotView` 바인딩, 픽업 로그 구독, 창고 쪽 `Chest.CreatItemSelected`(Phase 4).

- **3a. 기본 명령 이전:** `InventoryService`가 모델을 소유한다. `LoadInventory`는 매퍼로 모델을 채우고 뷰를 그린다.
  - `CreatItem`, `DeletItemByName`, 소비, 판매가 모델 명령을 쓴다. 뷰는 `SlotView.Bind`로 그린다.
  - 아직 GameObject를 직접 옮기는 경로(드래그, 키보드 교환, 나누기)는 끝에서 `SyncModelFromViews()`를 호출한다. 이것이 Phase 2 스캐너를 쓰는 **양방향 브리지**다.
- **3b. 키보드 경로 이전:** 키보드 교환·나누기·페이지 이동을 `Move`/`Split`으로 옮긴다.
  - ✅(10-30) 페이지 이동(`MoveToPage`)·나누기(`SplitAt`)는 3a 마무리에서 옮겼다. 남은 것: 같은 페이지 안 키보드 교환(`itemBoxChange`).
- **3c. 드래그 이전:** `ItemDrag`가 명령만 호출하게 한다. 이로써 브리지 호출이 0이 된다.
  - 그다음 `BoxCheck` 폴링, `inventoryArray`, `inventoryItemStack`을 삭제한다. 제작·상점은 `Count`를 직접 쓴다.
- **픽업 로그(D6):** 3a 직후 붙일 수 있다.
- **프로브:**
  - `inv_test.cs` 재실행(기존 결과와 동일해야 한다)
  - `inv_full_reject.cs`: 40개 획득 → 거절 수와 로그 표시 확인
  - 드래그 수동 플레이 체크: 이동, 교환, 합치기, 장비칸에서 끌어내기
  - 스크린샷 비교

### Phase 4 — 창고 공용화

- `Chest`가 `SlotContainer`와 `GridCursor`를 쓰게 한다. 중복 메서드(커서, 나누기, 교체, 툴팁)를 삭제해 Chest를 300줄 이하로 줄이는 것이 목표다.
- 인벤 → 창고, 창고 → 인벤 이동은 `SlotContainer.Transfer` 한 함수로 처리한다.
- 인벤·창고·장비를 원자적으로 함께 저장한다(B-015).
  - (10-30) 시작점: `InventoryManager.SaveInventoryAndChest()`(창고 `WriteChestArray` + 인벤 저장, 디스크 쓰기 1회). 임시 창구 `CommitDragToPage`(페이지 탭 드롭)는 `Transfer` + 원자 저장으로 대체하고 지운다. 페이지 탭 드롭은 지금 합치지 않고 빈칸에 그대로 놓는다 — `Transfer` 로 바꿀 때 합치기 여부를 정한다.
- **프로브:** `chest_transfer.cs`
  - 양방향으로 이동한다.
  - 대상이 가득 차도 총량이 보존되는지 확인한다.
  - 저장 후 재로드해도 복제·소실이 0인지 확인한다.

### Phase 5 — 장비 모델 + 세트 평가기

- `EquipmentApplier`가 `EquipBoxCheck`의 프리팹, 스탯, 텍스처 처리를 맡는다. Update 폴링은 제거한다.
- 장착·해제는 `Service.Equip/Unequip`만 거친다.
- `DatabaseManager.PlusSetDict/MinusSetDict`는 `SetBonusEvaluator.Diff` 적용으로 바꾼다.
- 장비창에 세트 활성 표시를 붙인다(D5 일부).
- **프로브:** `equip_set_tiers.cs`
  - LeafArmor를 0 → 5개로 장착하면서 `skillHitCount["Rising Roots"]`와 `skillBulletCount`가 단계별 기대값과 일치하는지 본다.
  - 이어서 5 → 0개로 해제했을 때 전부 0으로 돌아오는지 본다.
  - Rabbit 6부위에서 Def·HP가 기대값과 일치하는지 본다.

### Phase 6 — 입력 FSM

- `InventoryInput`(enum 3종)과 `GridCursor`를 도입한다. Chest·Shop·DivideSlider는 문자열 `state` 대신 FSM API를 쓴다.
- 레거시 Input을 제거하고, 디버그 생성 기능은 Editor 메뉴로 옮긴다(B-021).
- 이 단계가 끝나면 `InventoryManager`는 300~400줄짜리 파사드가 된다. Input, View, Tooltip은 각자 클래스로 분리된다.
- **프로브:** `ui_state_walk.cs`
  - 열기 → 상세 → 교환 → 취소 → 나누기 → 취소 → 창고 → 이동모드 → 닫기 순서로 입력을 시뮬레이션한다.
  - 각 단계의 Mode/Focus가 기대값과 같은지 본다.
  - 마지막에 `isOpenUI == false`인지 확인한다.

### Phase 7 — 툴팁(D3) · 정렬/25칸(D4)

> **진행(2026-10-19):** ✅ 정렬 — `ItemSortComparer`(순수) + `SlotContainer.SortRange`(페이지 범위, 잠긴 칸 제외) + `InventoryService.SortPage` + 인벤토리 R 키(게임패드 **R3** — Y 는 이미 나누기라 3.2 의 Y 대신), 정렬 뒤 즉시 저장. `ItemSortComparerTests` 4개 통과, 프로브 `inv_sort.cs`(총량 보존·스택 병합 확인). 정렬 버튼 UI·25칸 확장은 미구현.
> ✅ 툴팁 1차(D3) — `TooltipBuilder`(순수 문구 생성) + `InventoryManager.DrawEquipDetail`(인벤토리·장비창 중복 블록 통합): 등급색 이름·강화 접두 "+N"·강화 배율 반영 스탯·부호 한 번(B-016)·**착용품 비교 ▲/▼**(같은 부위 착용 시, 무기는 평균 피해)·착용품에만 있는 스탯 빨간 줄·**세트 진행도 `(착용/전체)`** + 도달 단계 세트색/미도달 회색, 세트 효과 키(IncDmg 등) 번역. 창고 툴팁도 강화 반영. 레이아웃은 기존 고정 자식 TMP 유지(단일 TMP 재배치·Shift 비교 패널·무기 스킬 세트 강화 표시는 미구현). `TooltipBuilderTests` 7개 통과, 프로브 `tooltip_dump.cs` + 스크린샷. 랭크 HUD 칩이 툴팁 위에 겹치던 문제는 UI 열림 시 숨김으로 해결.

- `StatBlock` 캐시, `TooltipBuilder`(+ `TooltipBuilderTests`)를 만든다.
- **`TooltipBuilderTests`:**
  - 등급색이 맞다.
  - 세트 진행도가 `(2/5)`로 표시된다.
  - 활성 단계와 비활성 단계의 색이 다르다.
  - 비교 delta의 부호와 색이 맞다.
  - 음수 부호가 한 번만 붙는다.
  - 무기 스킬의 쿨 감소가 표시된다.
- CSV 컬럼 17~19를 추가하고, 정렬 키와 `maxBoxNum` 25를 반영한다.
- **프로브:**
  - `tooltip_dump.cs`: LeafArmor 2부위를 착용한 상태에서 LeafShoes 툴팁 텍스트를 덤프해 확인하고, 스크린샷을 찍는다.
  - `inv_sort.cs`: 정렬 후 순서를 검증하고, 총량을 보존하는지 본다.

---

## 5. 결정·가정 (확인 필요 시 `Docs/OpenQuestions.md`로)

- 합치기 키는 id+tier+upgrade다. 지금은 tier/upgrade가 전부 0이라 체감 변화는 없다.
- 디바운스 저장(0.5s)은 "획득할 때마다 저장" 규칙을 대체한다. 강제 종료 시 최대 0.5초분이 유실될 수 있다(복제보다 낫다고 판단).
- 반지 슬롯은 1개를 유지한다. 반지 2칸 여부는 별도 기획 질문이다.
- 툴팁 문구 예시는 한글로 적었지만 현재 UI 텍스트는 영어다. 표시 언어는 별도로 결정한다(Q 등록 대상).
