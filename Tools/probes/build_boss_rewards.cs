// ⚠ Q-415(2026-10-19) 이후 몹 HP/방어·무기 피해·칭호(grantsRank)는 MonsterSheet.csv · apply_zone_tier_weapons.cs · Resources/Boss 가 기준이다. 이 프로브를 다시 돌리면 옛 값으로 되돌아가므로, 재실행 뒤 apply_zone_tier_weapons.cs 를 다시 돌리고 grantsRank(엔트 1·문지기 2·AbyssWizard 3)를 확인할 것.
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
  r.grantsRank = (string)d[0] == "PassiveTerror_BD" ? 1 : (string)d[0] == "AbyssWizard_BD" ? 2 : 0; // 임시 층계 보스 승급
  r.firstKillArtifact = (string)d[0] == "PassiveTerror_BD" ? ArtifactId.GuardianShell : ArtifactId.Hourglass;
  r.portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/RPGMonstersIcon/default size/no shadow/" + d[6] + ".png");
  if (r.portrait == null) sb.Append("[초상 없음 " + d[6] + "] ");
  EditorUtility.SetDirty(r); sb.Append(d[0] + " ");
  foreach (var n in r.setPieces) if (DatabaseManager.instance != null && DatabaseManager.instance.FindItemDataIndex(n) < 0) sb.Append("[없는 아이템 " + n + "] ");
}
AssetDatabase.SaveAssets();
return "boss rewards: " + sb;
