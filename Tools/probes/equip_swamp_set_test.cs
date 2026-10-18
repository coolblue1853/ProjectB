// 늪지 세트 6부위(방어구 5 + 계보 무기) 장착 → 세트 단계·스탯 확인 (플레이 모드). __SET__ = Bog | Gloom
string key = "__SET__";
string tf = key + "Armor";
string weapon = key == "Bog" ? "FrogTongueWhip" : "SlimeMace";
string[] cds = key == "Bog" ? new[]{ "Tongue Lash", "Jaw Shake", "Pincer Flurry" } : new[]{ "Bloodsting", "Slime Smash" };
int inc0 = DatabaseManager.incDmg, poi0 = DatabaseManager.addPoisonDmg, bl0 = DatabaseManager.bleedingAddDmg;
var inv = InventoryManager.instance;
int mv0 = (int)DatabaseManager.SpeedBuff; int def0 = DatabaseManager.playerDef, crit0 = DatabaseManager.playerCritRate, cd0 = DatabaseManager.playerCritDmgRate, atk0 = DatabaseManager.attackSpeedBuff;
var parts = new[]{ new[]{key + "Cap","Head","2"}, new[]{key + "Armor","Chest","5"}, new[]{key + "LegArmor","Leg","8"}, new[]{key + "HandArmor","Hand","6"}, new[]{key + "Shoes","Shoes","9"}, new[]{weapon,"Weapon","7"} };
var sb = new System.Text.StringBuilder();
foreach (var p in parts)
{
    var box = inv.equipBox[int.Parse(p[2])].GetComponent<EquipBoxCheck>();
    var data = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(p[0]));
    box.LoadPrefab(p[0], p[1], data.tfName);
    int stack; DatabaseManager.setsEffectStack.TryGetValue(tf, out stack);
    sb.Append(p[0] + "(" + data.tfName + ")→stack" + stack + " ");
}
sb.AppendLine();
foreach (var c in cds) { int v; DatabaseManager.skillCoolDown.TryGetValue(c, out v); sb.Append("cool[" + c + "]=" + v + " "); }
sb.Append("Move " + mv0 + "→" + DatabaseManager.SpeedBuff + " Def " + def0 + "→" + DatabaseManager.playerDef + " Crit " + crit0 + "→" + DatabaseManager.playerCritRate + " CritDmg " + cd0 + "→" + DatabaseManager.playerCritDmgRate + " AtkSpd " + atk0 + "→" + DatabaseManager.attackSpeedBuff + " Inc " + inc0 + "→" + DatabaseManager.incDmg + " Poison " + poi0 + "→" + DatabaseManager.addPoisonDmg + " Bleed " + bl0 + "→" + DatabaseManager.bleedingAddDmg);
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
sb.Append(" weapon=" + (w ? w.name + " skills=" + string.Join(",", System.Array.ConvertAll(w.GetComponentsInChildren<Skill>(true), s => s.skillName)) : "none"));
return sb.ToString();
