// 줍기 자석·필터 검증(BasicFeatures.md 4절, 플레이 중). __PHASE__ = drop | check
// drop: 플레이어 오른쪽 1.8유닛에 Wood·Potion 을 떨어뜨림(필터 __FILTER__ 적용) / check: 수량·남은 드롭 보고
var inv = InventoryManager.instance;
var pc = PlayerController.instance;
if (inv == null || pc == null) return "missing";
string phase = "__PHASE__";
if (phase == "drop")
{
    GameSettings.SetPickup((PickupFilter)__FILTER__);
    DropManager dm = UnityEngine.Object.FindAnyObjectByType<DropManager>(UnityEngine.FindObjectsInactive.Include);
    if (dm == null) return "no DropManager";
    UnityEngine.PlayerPrefs.SetInt("pb_probe_wood", inv.CountItem("Wood"));
    UnityEngine.PlayerPrefs.SetInt("pb_probe_potion", inv.CountItem("Potion"));
    dm.DropGuaranteed(new System.Collections.Generic.List<string> { "Wood", "Potion" }, pc.transform.position + UnityEngine.Vector3.right * 1.8f + UnityEngine.Vector3.up * 0.5f);
    return "dropped filter=" + GameSettings.Pickup + " wood=" + inv.CountItem("Wood") + " potion=" + inv.CountItem("Potion");
}
int w0 = UnityEngine.PlayerPrefs.GetInt("pb_probe_wood"), p0 = UnityEngine.PlayerPrefs.GetInt("pb_probe_potion");
UnityEngine.PlayerPrefs.DeleteKey("pb_probe_wood"); UnityEngine.PlayerPrefs.DeleteKey("pb_probe_potion");
int left = 0;
foreach (DropItemCheck d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(UnityEngine.FindObjectsSortMode.None))
{
    if (UnityEngine.Vector2.Distance(d.transform.position, pc.transform.position) < 6f) left++;
}
return "filter=" + GameSettings.Pickup + " wood " + w0 + "->" + inv.CountItem("Wood")
    + " potion " + p0 + "->" + inv.CountItem("Potion") + " dropsNearby=" + left;
