// 시험 준비(봇 행동 아님, 봇 실행 전에만): 마을 업무가 일어나게 가방을 만든다. __POT__ = 남길 소형 물약 수, __FREE__ = 남길 빈칸 수.
// __WEAKEN__ = 1 이면 착용 무기를 가방의 Wood Spear 로 바꿔 둔다(장착 시험: 봇이 원래 무기를 다시 끼는지). 더 좋을 수 있는 장비 몇 개와 잡템을 채운다. 세이브는 실행 스크립트(bot_run.sh)가 백업·원복한다
var inv = InventoryManager.instance;
if (inv == null) return "no inventory";
if (PlayBot.IsRunning) return "bot running - refused";
var svc = inv.Service;
if (__WEAKEN__ == 1)
{
    ItemCheck worn = inv.EquippedFor("Weapon");
    if (worn != null && worn.name != "Wood Spear")
    {
        inv.UnequipWorn(worn);
        var mm = svc.ScanModel();
        int per = inv.maxHor * inv.maxVer;
        for (int i = 0; i < mm.Unlocked; i++)
        {
            if (mm[i].Id == "Wood Spear")
            {
                ItemCheck spear = inv.inventoryUI[i / per].transform.GetChild(i % per).GetChild(0).GetComponent<ItemCheck>();
                inv.EquipFromInventory(spear);
                break;
            }
        }
    }
}
int have = inv.CountItem("Small HP Potion");
if (have > __POT__) svc.Remove("Small HP Potion", have - __POT__);
foreach (string id in new[] { "SacreGreatSword", "HeartwoodGreatsword", "WolfArmor", "GroveArmor", "RootStaff" })
{
    if (inv.CountItem(id) == 0) svc.Add(id, 1, AddSource.Other);
}
string[] junk = { "Small Bone", "Round Bone", "Disgusting Rat Meat", "Filthy Rat Hide", "Tough Rat Hide", "Ominous Black Liquid", "Blackened Branch", "Eerie Cloth Scrap", "Gloomy Cloth Scrap", "Malice-Infused Branch", "Fine Rabbit Hide" };
int free = 0;
var m = svc.ScanModel();
for (int i = 0; i < m.Unlocked; i++) if (m[i].IsEmpty) free++;
for (int k = 0; k < junk.Length && free > __FREE__; k++)
{
    if (inv.CountItem(junk[k]) == 0 && svc.Add(junk[k], 1, AddSource.Other)) free--;
}
m = svc.ScanModel();
free = 0;
for (int i = 0; i < m.Unlocked; i++) if (m[i].IsEmpty) free++;
return "potions=" + inv.CountItem("Small HP Potion") + " free=" + free;
