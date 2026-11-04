var o = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
o.Append("start unlocked=" + BagExpansion.Unlocked + " canUseShop=" + Buyback.CanUse() + " money=" + DatabaseManager.money + "\n");
int origMoney = DatabaseManager.money;
DatabaseManager.money = 0; BagExpansion.Describe(); bool b0 = BagExpansion.TryBuy();
o.Append("no gold TryBuy=" + b0 + " unlocked=" + BagExpansion.Unlocked + "\n");
DatabaseManager.money = 100000;
for (int i = 0; i < 4; i++) { int before = DatabaseManager.money; bool b = BagExpansion.TryBuy(); o.Append("buy" + i + "=" + b + " unlocked=" + BagExpansion.Unlocked + " spent=" + (before - DatabaseManager.money) + "\n"); }
o.Append("save.bagUnlocked=" + SaveManager.instance.datas.bagUnlocked + "\n");
DatabaseManager.money = origMoney;
return o.ToString();
