// gear1 검증(플레이 모드): 퀘스트 보상 경로 + 세트 2/4/6 스탯 → 상대 DPS 배율
var sb = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
// A. 퀘스트 보상으로 장비 지급 (NpcQuestRewards.Apply = 실제 지급 경로)
foreach (var id in new[]{"Q102","Q201","Q202","Q204","Q301","Q302","Q304","Q401","Q402"})
{
    var d = NpcQuestCatalog.Find(id);
    bool fit = NpcQuestRewards.CanReceive(d);
    NpcQuestRewards.Apply(d);
    sb.Append(id + " Lv" + d.MinLevel + " reward=" + d.RewardItem + " fit=" + fit + " inBag=" + inv.Service.ScanModel().Count(d.RewardItem) + "\n");
}
// B. 세트 스탯
System.Func<string,string,int,string,string> eq = (name, area, slot, dummy) => {
    var box = inv.equipBox[slot].GetComponent<EquipBoxCheck>();
    var data = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(name));
    box.LoadPrefab(name, area, data.tfName);
    return name;
};
System.Func<string> stat = () => "crit" + DatabaseManager.playerCritRate + " cd" + DatabaseManager.playerCritDmgRate + " inc" + DatabaseManager.incDmg + " atk" + DatabaseManager.attackSpeedBuff + " mv" + DatabaseManager.SpeedBuff + " def" + DatabaseManager.playerDef;
var sets = new[]{
  new[]{"Wolf","FangDagger","WolfCap","WolfArmor","WolfLegArmor","WolfHandArmor","WolfShoes"},
  new[]{"Grove","BearClaw","GroveCap","GroveArmor","GroveLegArmor","GroveHandArmor","GroveShoes"},
  new[]{"Bog","CrocJawAxe","BogCap","BogArmor","BogLegArmor","BogHandArmor","BogShoes"} };
string[] areas = {"Weapon","Head","Chest","Leg","Hand","Shoes"}; int[] slots = {7,2,5,8,6,9};
foreach (var s in sets)
{
    // 리셋: 기본 장비
    string[] basic = {"RustedSword","Cap","Armor","LegArmor","HandArmor","Shoes"};
    for (int i = 0; i < 6; i++) eq(basic[i], areas[i], slots[i], "");
    var w = Resources.Load<GameObject>("Weapon/" + s[1]).GetComponent<Weapon>();
    sb.Append("== " + s[0] + " weapon " + s[1] + " avg" + (w.minDmg + w.maxDmg) / 2f + " wcrit" + w.critPer + "/" + w.critDmg + " winc" + w.incDmg + "\n");
    sb.Append("  basic(rusted): " + stat() + "\n");
    int n = 0;
    foreach (int i in new[]{0,1,2,3,4,5})
    {
        eq(s[1 + i], areas[i], slots[i], ""); n++;
        sb.Append("  +" + s[1 + i] + " (" + n + "): " + stat() + "\n");
    }
}
for (int i = 0; i < 6; i++) eq(new[]{"RustedSword","Cap","Armor","LegArmor","HandArmor","Shoes"}[i], areas[i], slots[i], "");
sb.Append("reset: " + stat());
var lb = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var ld = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("LeafAdae")); lb.LoadPrefab("LeafAdae","Weapon",ld.tfName);
var ew = UnityEngine.Object.FindFirstObjectByType<Weapon>(); sb.Append(" equip LeafAdae -> " + (ew ? ew.name + " skills=" + string.Join(",", System.Array.ConvertAll(ew.GetComponentsInChildren<Skill>(true), z => z.skillName)) : "none"));
return sb.ToString();
