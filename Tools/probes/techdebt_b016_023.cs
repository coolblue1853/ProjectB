// TechDebt B-017~023 회귀 프로브(플레이 모드). 세이브를 바꾸므로 실행 전후 SaveFile.es3 백업·복원할 것.
// __ITEM__ = 상점 테스트 아이템(예: Wood = 겹침 가능, Cap = 1개)
// B-018 상점 외상·0 나누기, B-017 프리팹 캐시·null 툴팁, B-020 알 수 없는 이름, B-022 배치 저장, B-023 수량 텍스트 갱신
var sb = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

// B-018: 살 수 있는 개수
sb.Append("afford(99,0,100)=" + ShopManager.MaxAffordable(99, 0, 100));
sb.Append(" afford(99,15,10)=" + ShopManager.MaxAffordable(99, 15, 10));
sb.Append(" afford(10,15,1000)=" + ShopManager.MaxAffordable(10, 15, 1000));

// B-017: 캐시·null 안전
sb.Append(" | missing=" + (GearPrefabCache.Get("Weapon", "__NoSuchGear__") == null));
sb.Append(" sword=" + (GearPrefabCache.Get("Weapon", "RustedSword") != null));
sb.Append(" sameRef=" + ((object)GearPrefabCache.Get("Weapon", "RustedSword") == (object)GearPrefabCache.Get("Weapon", "RustedSword")));
sb.Append(" nullBlock='" + TooltipBuilder.WeaponBlock(null, 0, null, 0) + TooltipBuilder.ArmorBlock(null, 0, null, 0) + "'");

// B-020: 시트에 없는 이름이면 예외 없이 itemData null
var go = UnityEngine.Object.Instantiate(inv.itemPrefab, inv.transform);
var ic = go.GetComponent<ItemCheck>();
ic.itemData = null;
ic.SetItem("__NoSuchItem__");
sb.Append(" | unknownItemData=" + (ic.itemData == null));
UnityEngine.Object.Destroy(go);

// B-022: 배치 중 저장 미룸
var pendingField = typeof(InventoryManager).GetField("_saveBatchPending", flags);
int wood0 = inv.CountItem("Wood");
inv.BeginSaveBatch();
inv.CreatItem("Wood"); inv.CreatItem("Wood"); inv.CreatItem("Wood");
bool pendingIn = (bool)pendingField.GetValue(inv);
inv.EndSaveBatch();
bool pendingOut = (bool)pendingField.GetValue(inv);
sb.Append(" | batch pendingIn=" + pendingIn + " pendingOut=" + pendingOut + " wood " + wood0 + "->" + inv.CountItem("Wood"));

// B-018: 돈이 모자랄 때 구매 → 외상 없음
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(UnityEngine.FindObjectsInactive.Include);
if (shop != null)
{
    var sgo = UnityEngine.Object.Instantiate(inv.itemPrefab, inv.transform);
    var si = sgo.GetComponent<ItemCheck>();
    si.SetItem("__ITEM__");
    si.buyprice = 50;
    shop.nowShopItem = si;
    int moneyBefore = DatabaseManager.money;
    DatabaseManager.money = 10;
    int cap0 = inv.CountItem("__ITEM__");
    typeof(ShopManager).GetMethod("InitializeSlider", flags).Invoke(shop, null);
    float sliderMax = shop.sellSlider.maxValue;
    shop.sellSlider.value = 5;
    typeof(ShopManager).GetMethod("BuyItem", flags).Invoke(shop, null);
    sb.Append(" | poor: max=" + sliderMax + " money=" + DatabaseManager.money + " cap " + cap0 + "->" + inv.CountItem("__ITEM__"));
    // 두 개 값만 있는 경우: 3개 요청 → 2개만
    DatabaseManager.money = 100;
    typeof(ShopManager).GetMethod("InitializeSlider", flags).Invoke(shop, null);
    sliderMax = shop.sellSlider.maxValue;
    shop.sellSlider.value = 3;
    typeof(ShopManager).GetMethod("BuyItem", flags).Invoke(shop, null);
    sb.Append(" | 100g: max=" + sliderMax + " money=" + DatabaseManager.money + " cap->" + inv.CountItem("__ITEM__"));
    // 가격 0: 0 나누기 없음
    si.buyprice = 0;
    typeof(ShopManager).GetMethod("InitializeSlider", flags).Invoke(shop, null);
    sb.Append(" | free max=" + shop.sellSlider.maxValue);
    DatabaseManager.money = moneyBefore;
    shop.nowShopItem = null;
    UnityEngine.Object.Destroy(sgo);
}
else
{
    sb.Append(" | shop=null");
}

// B-023: 수량 텍스트는 바뀔 때만
ItemCheck any = null;
foreach (var c in UnityEngine.Object.FindObjectsByType<ItemCheck>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
{
    if (c.stackText != null && !c.isCraftItem && !c.isShopItem && c.itemData != null && c.itemData.name == "Wood")
    {
        any = c;
        break;
    }
}
if (any != null)
{
    sb.Append(" | woodText='" + any.stackText.text + "' stack=" + any.nowStack + " textActive=" + any.stackText.gameObject.activeSelf);
}
return sb.ToString();
