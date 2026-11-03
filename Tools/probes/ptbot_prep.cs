// PT-BOT 준비: 무기 장착 + 레벨 목표(__LV__) + 물약 지급. 세이브 백업은 ptbot.sh 가 한다.
var inv = InventoryManager.instance;
string w = "__W__";
int lv = __LV__;
var so = new SROptions();
while (LevelSystem.Level < lv) so.LevelUp();
if (w != "none")
{
    if (inv.CountItem(w) < 1) inv.CreatItem(w);
    ItemCheck it = null;
    foreach (var c in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (c.itemData != null && c.itemData.name == w && c.transform.IsChildOf(inv.transform)) { it = c; break; }
    if (it != null) typeof(InventoryManager).GetMethod("UseEquipment", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(inv, new object[] { it });
}
var eq = inv.EquippedFor("Weapon");
return "lv=" + LevelSystem.Level + " weapon=" + (eq != null ? eq.itemData.name : "none");
