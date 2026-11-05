// PV9 장신구 슬롯 세이브/로드 왕복: 해금 mask 유지 + 폐기 장신구 재지급 없음
var o = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
ExtraEquip.Load();
ExtraEquipRewards.OnFloorBossFirstKill(1); ExtraEquipRewards.OnFloorBossFirstKill(2);
int m0 = ES3.Load<int>(SlotRules.SaveKeyUnlock);
o.Append("mask after unlock=" + m0 + " earring=" + ExtraEquip.IsUnlocked(EquipSlot.Earring) + "\n");
ExtraEquip.Load();
int m1 = ES3.Load<int>(SlotRules.SaveKeyUnlock);
o.Append("mask after Load=" + m1 + " same=" + (m0 == m1) + " blocked=" + ExtraEquip.SaveBlocked + " unlockedStill=" + ExtraEquip.IsUnlocked(EquipSlot.Earring) + "\n");
// 폐기 장신구를 칸 0 에 심고 로드 2회
string stale = "TitleWanderer";
int c0 = inv.CountItem(stale);
var enc = SlotRules.EncodeStack(new ItemStack(stale, 1, 0, 0));
ES3.Save(SlotRules.SaveKeyExtra, new string[]{ enc, "", "", "" });
ExtraEquip.Load();
int c1 = inv.CountItem(stale);
var ids1 = ES3.Load<string[]>(SlotRules.SaveKeyExtra);
ExtraEquip.Load();
int c2 = inv.CountItem(stale);
o.Append("stale count " + c0 + " -> " + c1 + " -> " + c2 + " (재지급 없으면 마지막 둘 같음) key0='" + ids1[0] + "' mask=" + ES3.Load<int>(SlotRules.SaveKeyUnlock) + "\n");
// 읽기 실패 보호
ES3.Save(SlotRules.SaveKeyUnlock, m0);
return o.ToString();
