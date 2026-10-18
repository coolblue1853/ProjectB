// Cliff 세트 6부위(방어구 5 + 무기) 장착 → 세트 단계·스탯 확인
var inv = InventoryManager.instance;
int def0 = DatabaseManager.playerDef, crit0 = DatabaseManager.playerCritRate, cd0 = DatabaseManager.playerCritDmgRate, atk0 = DatabaseManager.attackSpeedBuff;
var parts = new[]{ new[]{"CliffCap","Head","2"}, new[]{"CliffArmor","Chest","5"}, new[]{"CliffLegArmor","Leg","8"}, new[]{"CliffHandArmor","Hand","6"}, new[]{"CliffShoes","Shoes","9"}, new[]{"CliffBlade","Weapon","7"} };
var sb = new System.Text.StringBuilder();
foreach (var p in parts)
{
    var box = inv.equipBox[int.Parse(p[2])].GetComponent<EquipBoxCheck>();
    var data = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(p[0]));
    box.LoadPrefab(p[0], p[1], data.tfName);
    int stack; DatabaseManager.setsEffectStack.TryGetValue("CliffArmor", out stack);
    sb.Append(p[0] + "→stack" + stack + " ");
}
int cdSky; DatabaseManager.skillCoolDown.TryGetValue("Skybreaker", out cdSky);
int cdCyc; DatabaseManager.skillCoolDown.TryGetValue("Cyclone Edge", out cdCyc);
sb.AppendLine();
sb.Append("Def " + def0 + "→" + DatabaseManager.playerDef + " Crit " + crit0 + "→" + DatabaseManager.playerCritRate + " CritDmg " + cd0 + "→" + DatabaseManager.playerCritDmgRate + " AtkSpd " + atk0 + "→" + DatabaseManager.attackSpeedBuff + " cool[Skybreaker]=" + cdSky + " cool[Cyclone Edge]=" + cdCyc);
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
sb.Append(" weapon=" + (w ? w.name + " skills=" + string.Join(",", System.Array.ConvertAll(w.GetComponentsInChildren<Skill>(true), s => s.skillName)) : "none"));
return sb.ToString();
