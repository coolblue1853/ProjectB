// Wolf(Pack Hunter) 세트 6부위(방어구 5 + 무기) 장착 → 세트 단계·스탯 확인
var inv = InventoryManager.instance;
int mv0 = (int)DatabaseManager.SpeedBuff; int def0 = DatabaseManager.playerDef, crit0 = DatabaseManager.playerCritRate, cd0 = DatabaseManager.playerCritDmgRate, atk0 = DatabaseManager.attackSpeedBuff;
var parts = new[]{ new[]{"WolfCap","Head","2"}, new[]{"WolfArmor","Chest","5"}, new[]{"WolfLegArmor","Leg","8"}, new[]{"WolfHandArmor","Hand","6"}, new[]{"WolfShoes","Shoes","9"}, new[]{"FangDagger","Weapon","7"} };
var sb = new System.Text.StringBuilder();
foreach (var p in parts)
{
    var box = inv.equipBox[int.Parse(p[2])].GetComponent<EquipBoxCheck>();
    var data = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(p[0]));
    box.LoadPrefab(p[0], p[1], data.tfName);
    int stack; DatabaseManager.setsEffectStack.TryGetValue("WolfArmor", out stack);
    sb.Append(p[0] + "→stack" + stack + " ");
}
int cdSky; DatabaseManager.skillCoolDown.TryGetValue("Howl of the Pack", out cdSky);
int cdCyc; DatabaseManager.skillCoolDown.TryGetValue("Boar Charge", out cdCyc);
sb.AppendLine();
sb.Append("Move " + mv0 + "→" + DatabaseManager.SpeedBuff + " Def " + def0 + "→" + DatabaseManager.playerDef + " Crit " + crit0 + "→" + DatabaseManager.playerCritRate + " CritDmg " + cd0 + "→" + DatabaseManager.playerCritDmgRate + " AtkSpd " + atk0 + "→" + DatabaseManager.attackSpeedBuff + " cool[Howl]=" + cdSky + " cool[BoarCharge]=" + cdCyc);
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
sb.Append(" weapon=" + (w ? w.name + " skills=" + string.Join(",", System.Array.ConvertAll(w.GetComponentsInChildren<Skill>(true), s => s.skillName)) : "none"));
return sb.ToString();
