// 퀵슬롯 검증(BasicFeatures.md 3절, 플레이 중): 물약 2개 지급 → 1번 칸 등록 → 사용 → 수량 -1·HP 회복·쿨다운 → 즉시 재사용 거절 → ES3 키 확인
var inv = InventoryManager.instance;
var model = QuickSlotBar.Model;
if (inv == null || model == null) return "missing inv=" + (inv != null) + " bar=" + (model != null);
inv.CreatItem("Potion"); inv.CreatItem("Potion");
ItemCheck potion = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "Potion") { potion = c; break; } }
if (potion == null) return "no potion in inventory";
bool assigned = QuickSlotBar.AssignFrom(0, potion);
ItemCheck notConsum = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.itemData != null && c.itemData.type != "Consum") { notConsum = c; break; } }
bool rejected = notConsum == null || !QuickSlotBar.AssignFrom(1, notConsum);
var phm = PlayerHealthManager.Instance;
phm.nowHp = phm.fullHP / 2;
int hpBefore = phm.nowHp;
int before = inv.CountItem("Potion");
bool used1 = QuickSlotBar.Use(0);
int after1 = inv.CountItem("Potion");
int hpAfter = phm.nowHp;
bool used2 = QuickSlotBar.Use(0);
int after2 = inv.CountItem("Potion");
float cd = ConsumeCooldown.Shared.Remaining(ConsumeCooldown.PotionGroup, UnityEngine.Time.time);
string[] saved = ES3.Load<string[]>(QuickSlotBar.SaveKey);
var bar = UnityEngine.GameObject.Find("[QuickSlotBar]");
return "assigned=" + assigned + " rejectNonConsum=" + rejected + " slot0=" + model[0]
    + " count " + before + "->" + after1 + "->" + after2 + " used1=" + used1 + " used2(cd)=" + used2
    + " hp " + hpBefore + "->" + hpAfter + " cdLeft=" + cd.ToString("F1")
    + " saved=[" + string.Join(",", saved) + "] barActive=" + (bar != null && bar.activeInHierarchy);
