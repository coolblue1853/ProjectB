// 강화 창을 +10 CliffBlade 로 열고 단조 패널을 띄운다(스크린샷용). forge_run 뒤에 실행하면 +11 이 된다.
var inv = InventoryManager.instance;
ForgeState.UnlockMask = 15;
ItemCheck blade = null;
foreach (var c in inv.EnumerateInventoryItems()) if (c.itemData.name == "CliffBlade") blade = c;
if (blade == null) { inv.CreatItem("CliffBlade"); foreach (var c in inv.EnumerateInventoryItems()) if (c.itemData.name == "CliffBlade") blade = c; }
for (int i = 0; i < 30; i++) inv.CreatItem("Enhance Stone");
DatabaseManager.money += 20000;
if (blade.itemData.upgrade < 11) inv.Service.SetUpgrade(blade, 12);
ForgeState.Pity = 3;
EnhanceWindow.Open();
var w = UnityEngine.Object.FindObjectOfType<EnhanceWindow>();
var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var items = (System.Collections.Generic.List<ItemCheck>)typeof(EnhanceWindow).GetField("items", bf).GetValue(w);
typeof(EnhanceWindow).GetField("cursor", bf).SetValue(w, items.IndexOf(blade));
typeof(EnhanceWindow).GetMethod("Redraw", bf).Invoke(w, null);
return "opened lv=" + blade.itemData.upgrade + " idx=" + items.IndexOf(blade);
