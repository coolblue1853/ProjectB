// Resources/UI/BestiaryData.asset 생성 — 몬스터 프리팹 이름 · 표시 이름 · 지역 · RPG Monsters Icon
const string I = "Assets/ThirdParty/RPGMonstersIcon/default size/no shadow/";
var rows = new object[][] {
  new object[]{ "Rabbit_BD", "Forest Rabbit", "Forest Road", "2 Jungle/jungle_208" },
  new object[]{ "StoneRabbit_BD", "Stone Rabbit", "Forest Road", "5 Field/field_510" },
  new object[]{ "FlowerMole_BD", "Flower Mole", "Forest Side Road", "2 Jungle/jungle_206" },
  new object[]{ "LeftMole_BD", "Leaf Mole", "Forest Side Road", "2 Jungle/jungle_207" },
  new object[]{ "Scarecrow_BD", "Scarecrow", "Forest Side Road", "4 Forest/forest_404" },
  new object[]{ "RatThief_BD", "Rat Thief", "Forest Side Road", "6 Cave/cave_604" },
  new object[]{ "RatChief_BD", "Rat Chief", "Forest Side Road", "6 Cave/cave_605" },
  new object[]{ "PassiveTerror_BD", "Passive Terror", "Forest Side Road (night)", "1 Undead/undead_112", "boss" },
  new object[]{ "SporeShroom_BD", "Spore Shroom", "Magic Cliffs", "4 Forest/forest_402" },
  new object[]{ "CliffGoblin_BD", "Cliff Goblin", "Magic Cliffs", "2 Jungle/jungle_205" },
  new object[]{ "GazeEye_BD", "Gaze Eye", "Magic Cliffs (night)", "5 Field/field_507" },
  new object[]{ "CliffSkeleton_BD", "Cliff Skeleton", "Magic Cliffs (night)", "1 Undead/undead_103" },
  new object[]{ "DeathBringer_BD", "Death Bringer", "Magic Cliffs (night)", "1 Undead/undead_111", "elite" },
  new object[]{ "AbyssWizard_BD", "Abyss Wizard", "Cliff Altar (night)", "1 Undead/undead_110", "boss" },
};
var path = "Assets/Resources/UI/BestiaryData.asset";
var data = AssetDatabase.LoadAssetAtPath<BestiaryData>(path);
if (data == null) { data = ScriptableObject.CreateInstance<BestiaryData>(); AssetDatabase.CreateAsset(data, path); }
data.entries.Clear(); var miss = new System.Text.StringBuilder();
foreach (var r in rows) {
  var icon = AssetDatabase.LoadAssetAtPath<Sprite>(I + r[3] + ".png");
  if (icon == null) miss.Append(r[3] + " ");
  data.entries.Add(new BestiaryData.Entry { id = (string)r[0], displayName = (string)r[1], region = (string)r[2], icon = icon, isBoss = r.Length > 4 && (string)r[4] == "boss", isElite = r.Length > 4 && (string)r[4] == "elite" });
}
EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
return "bestiary " + data.entries.Count + " missingIcons=[" + miss + "]";
