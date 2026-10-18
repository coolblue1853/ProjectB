// Resources/Boss/<보스>.asset 생성 (ProgressionDesign 2절)
var defs = new object[][] {
  new object[]{ "PassiveTerror_BD", new[]{ "ScareCap","ScareArmor","ScareLegArmor","ScareHandArmor","ScareShoes","ScareStaff" }, "Necklace of Silent Screams", 3, 5, "Passive Terror", "1 Undead/undead_112" },
  new object[]{ "AbyssWizard_BD", new[]{ "CliffCap","CliffArmor","CliffLegArmor","CliffHandArmor","CliffShoes","CliffBlade","CliffStaff" }, "AbyssStaff", 4, 6, "Abyss Wizard", "1 Undead/undead_110" },
};
var sb = new System.Text.StringBuilder();
foreach (var d in defs) {
  var path = "Assets/Resources/Boss/" + d[0] + ".asset";
  var r = AssetDatabase.LoadAssetAtPath<BossReward>(path);
  if (r == null) { r = ScriptableObject.CreateInstance<BossReward>(); AssetDatabase.CreateAsset(r, path); }
  r.setPieces = (string[])d[1]; r.uniqueItem = (string)d[2]; r.stoneMin = (int)d[3]; r.stoneMax = (int)d[4];
  r.displayName = (string)d[5];
  r.portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/RPGMonstersIcon/default size/no shadow/" + d[6] + ".png");
  if (r.portrait == null) sb.Append("[초상 없음 " + d[6] + "] ");
  EditorUtility.SetDirty(r); sb.Append(d[0] + " ");
  foreach (var n in r.setPieces) if (DatabaseManager.instance != null && DatabaseManager.instance.FindItemDataIndex(n) < 0) sb.Append("[없는 아이템 " + n + "] ");
}
AssetDatabase.SaveAssets();
return "boss rewards: " + sb;
