// 1층계 이펙트 폴리싱 설정 (재실행 안전): ① gen_polish_fx.py 도트 시트 6종 → PixelFxClip·VfxLibrary ② 분위기 입자 시트 슬라이스
// ③ 트레일·분위기 머티리얼 ④ 투사체·소환수·뿌리 프리팹에 ProjectileFx ⑤ 계보 무기 스킬 castVfx ⑥ 구역 MapInfo.ambience (씬 저장)
var sb = new System.Text.StringBuilder();
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
foreach (var n in new[]{ "SmokeGray", "SmokePurple", "DustBrown", "DustStone", "GroundShock", "BurstRed" }) {
  sb.Append(ProjectB.EditorTools.PixelVfxBaker.ImportSheet("Px_" + n) + "; ");
  var id = (VfxId)System.Enum.Parse(typeof(VfxId), n);
  var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/Px_" + n + ".asset");
  if (clip == null) return "clip 없음 Px_" + n + " (gen_polish_fx.py 먼저)";
  var e = lib.entries.Find(x => x.id == id);
  if (e == null) { e = new VfxLibrary.Entry { id = id }; lib.entries.Add(e); }
  e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
}
EditorUtility.SetDirty(lib);
// ② 분위기 시트
var slice = typeof(ProjectB.EditorTools.PixelVfxBaker).GetMethod("SliceSheet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
var amb = (Sprite[])slice.Invoke(null, new object[]{ "Assets/Resources/FX/Px_Ambient.png", 6, 16, 16, new Vector2(0.5f, 0.5f), 16f });
sb.Append("ambient " + amb.Length + "; ");
// ③ 머티리얼
var pix = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/FX/M_PixelFx.mat");
if (AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/FX/M_Trail.mat") == null) AssetDatabase.CreateAsset(new Material(pix.shader), "Assets/Resources/FX/M_Trail.mat");
var ambTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/FX/Px_Ambient.png");
foreach (var mn in new[]{ "M_AmbientUnlit", "M_AmbientLit" }) {
  string mp = "Assets/Resources/FX/" + mn + ".mat";
  var m = AssetDatabase.LoadAssetAtPath<Material>(mp);
  if (m == null) { m = new Material(mn == "M_AmbientLit" ? Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default") : pix.shader); AssetDatabase.CreateAsset(m, mp); }
  m.mainTexture = ambTex; EditorUtility.SetDirty(m);
}
sb.Append("mats ok; ");
// ④ ProjectileFx: 경로, spawnFx, endFx, scale, trailTime, trailWidth, r,g,b,a
const string D = "Assets/Resources/Weapon/DamageObject/";
object[][] pf = {
  new object[]{ "FaeWand/FaeOrb", VfxId.None, VfxId.BurstPurple, 0.45f, 0.25f, 0.18f, 1f, 0.6f, 0.85f, 0.8f },
  new object[]{ "WispLantern/WispOrb", VfxId.None, VfxId.BurstBlue, 0.45f, 0.25f, 0.2f, 0.5f, 0.8f, 1f, 0.8f },
  new object[]{ "BanditShortbow/ArrowBasic", VfxId.None, VfxId.None, 1f, 0.12f, 0.08f, 1f, 0.95f, 0.8f, 0.6f },
  new object[]{ "BanditShortbow/ArrowSkill", VfxId.None, VfxId.None, 1f, 0.14f, 0.1f, 1f, 0.9f, 0.7f, 0.7f },
  new object[]{ "BanditShortbow/ArrowRain", VfxId.None, VfxId.None, 1f, 0.12f, 0.08f, 0.6f, 1f, 0.4f, 0.6f },
  new object[]{ "Outskirts/BoulderToss", VfxId.None, VfxId.DustStone, 1f, 0.15f, 0.25f, 0.65f, 0.65f, 0.65f, 0.5f },
  new object[]{ "Outskirts/RollingStone", VfxId.DustStone, VfxId.DustStone, 0.8f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "Outskirts/OathShockwave", VfxId.GroundShock, VfxId.None, 0.8f, 0.2f, 0.3f, 0.5f, 1f, 1f, 0.6f },
  new object[]{ "Rampart/CrescentReapWave", VfxId.None, VfxId.None, 1f, 0.2f, 0.3f, 0.9f, 0.95f, 1f, 0.6f },
  new object[]{ "Rampart/StoneFeatherShot", VfxId.None, VfxId.None, 1f, 0.12f, 0.1f, 0.75f, 0.78f, 0.82f, 0.6f },
  new object[]{ "FangDagger/WolfSpirit", VfxId.SmokeGray, VfxId.SmokeGray, 0.8f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "ChiefsWarBanner/BanditShadow", VfxId.SmokePurple, VfxId.SmokePurple, 0.8f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "Outskirts/StoneSentinel", VfxId.DustStone, VfxId.SmokeGray, 0.8f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "Rampart/GargoyleDiver", VfxId.SmokeGray, VfxId.SmokeGray, 0.7f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "Rampart/GargoyleHostDiver", VfxId.SmokeGray, VfxId.SmokeGray, 0.7f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "SlimeMace/SlimeHopper", VfxId.BurstGreen, VfxId.BurstGreen, 0.45f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "SlimeMace/GelHopper", VfxId.BurstGreen, VfxId.BurstGreen, 0.6f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "RootStaff/DeepRoot", VfxId.DustBrown, VfxId.None, 0.8f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "RootStaff/GiantRoot", VfxId.DustBrown, VfxId.None, 1.3f, 0f, 0f, 1f, 1f, 1f, 0f },
  new object[]{ "QuakestoneMaul/QuakestoneMaulScareStaright", VfxId.GroundShock, VfxId.None, 1f, 0f, 0f, 1f, 1f, 1f, 0f },
};
foreach (var r in pf) {
  string path = D + (string)r[0] + ".prefab";
  var root = PrefabUtility.LoadPrefabContents(path);
  var fx = root.GetComponent<ProjectileFx>(); if (fx == null) fx = root.AddComponent<ProjectileFx>();
  var so = new SerializedObject(fx);
  so.FindProperty("_spawnFx").intValue = (int)(VfxId)r[1];
  so.FindProperty("_endFx").intValue = (int)(VfxId)r[2];
  so.FindProperty("_fxScale").floatValue = (float)r[3];
  so.FindProperty("_trailTime").floatValue = (float)r[4];
  so.FindProperty("_trailWidth").floatValue = (float)r[5];
  so.FindProperty("_trailColor").colorValue = new Color((float)r[6], (float)r[7], (float)r[8], (float)r[9]);
  so.ApplyModifiedPropertiesWithoutUndo();
  PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
}
sb.Append("projectileFx " + pf.Length + "; ");
// ⑤ castVfx: 무기, 스킬 이름, VfxId, 배율
object[][] cv = {
  new object[]{ "FangDagger", "Fang Rush", VfxId.DustBrown, 0.8f }, new object[]{ "FangDagger", "Howl of the Pack", VfxId.BurstBlue, 1f },
  new object[]{ "TuskKnuckles", "Boar Charge", VfxId.DustBrown, 1f }, new object[]{ "TuskKnuckles", "Tusk Uppercut", VfxId.DustBrown, 0.8f },
  new object[]{ "ShellShield", "Shell Bash", VfxId.DustBrown, 0.8f }, new object[]{ "ShellShield", "Shell Guard", VfxId.RunePurple, 0.7f },
  new object[]{ "BearClaw", "Mauling Rush", VfxId.DustBrown, 1f }, new object[]{ "BearClaw", "Rending Upswipe", VfxId.SlashRed, 1f }, new object[]{ "BearClaw", "Bear Roar", VfxId.BurstRed, 1.2f },
  new object[]{ "HeartwoodGreatsword", "Sap Drain", VfxId.RuneGreen, 1f }, new object[]{ "HeartwoodGreatsword", "Grove Cleave", VfxId.SlashGreenBig, 1f },
  new object[]{ "PhantomScythe", "Phantom Reap", VfxId.SmokePurple, 0.9f }, new object[]{ "PhantomScythe", "Spectral Eruption", VfxId.SmokePurple, 1.2f },
  new object[]{ "PhantomScythe", "Wraith Whirl", VfxId.SmokePurple, 1f }, new object[]{ "PhantomScythe", "Phantom Glide", VfxId.SmokePurple, 0.9f },
  new object[]{ "FrogTongueWhip", "Tongue Lash", VfxId.SlashRed, 0.8f }, new object[]{ "FrogTongueWhip", "Leap Slam", VfxId.DustBrown, 1f },
  new object[]{ "CrocJawAxe", "Jaw Shake", VfxId.SlashRed, 0.9f }, new object[]{ "CrocJawAxe", "Bog Eruption", VfxId.DustBrown, 1.2f },
  new object[]{ "CrocJawAxe", "Tail Whirl", VfxId.DustBrown, 1f }, new object[]{ "CrocJawAxe", "Death Roll", VfxId.DustBrown, 1f },
  new object[]{ "PetalPincer", "Crab Scuttle", VfxId.DustBrown, 0.7f }, new object[]{ "PetalPincer", "Pincer Flurry", VfxId.CrackRed, 0.8f },
  new object[]{ "BloodstingRapier", "Bloodsting", VfxId.BurstRed, 0.8f }, new object[]{ "BloodstingRapier", "Swarm Dart", VfxId.SlashRed, 0.9f }, new object[]{ "BloodstingRapier", "Swarm Call", VfxId.BurstRed, 1f },
  new object[]{ "SlimeMace", "Gel Gather", VfxId.RuneGreen, 1f }, new object[]{ "SlimeMace", "Slime Smash", VfxId.BurstGreen, 1f },
  new object[]{ "PrimordialGel", "Primordial Pull", VfxId.RuneGreen, 1.2f }, new object[]{ "PrimordialGel", "Gel Tsunami", VfxId.BurstGreen, 1.4f },
  new object[]{ "CutthroatDagger", "Backstab", VfxId.SmokePurple, 0.9f }, new object[]{ "CutthroatDagger", "Shadow Step", VfxId.SmokePurple, 0.8f },
  new object[]{ "BanditShortbow", "Triple Shot", VfxId.BurstGold, 0.6f }, new object[]{ "BanditShortbow", "Poison Volley", VfxId.BurstGreen, 0.8f }, new object[]{ "BanditShortbow", "Pitch Shot", VfxId.BurstGold, 0.8f },
  new object[]{ "RaiderBuckler", "Shield Shove", VfxId.DustStone, 0.9f }, new object[]{ "RaiderBuckler", "Counter Stance", VfxId.RuneGold, 0.8f },
  new object[]{ "ChiefsWarBanner", "Raid Charge", VfxId.DustBrown, 1f }, new object[]{ "ChiefsWarBanner", "Rally the Gang", VfxId.BurstRed, 1.2f },
  new object[]{ "PebbleGauntlet", "Rolling Charge", VfxId.DustStone, 1f }, new object[]{ "PebbleGauntlet", "Rubble Uppercut", VfxId.DustStone, 0.8f },
  new object[]{ "QuakestoneMaul", "Quake Slam", VfxId.GroundShock, 1f }, new object[]{ "QuakestoneMaul", "Aftershock", VfxId.GroundShock, 1.3f },
  new object[]{ "MonolithShield", "Boulder Toss", VfxId.DustStone, 0.8f }, new object[]{ "MonolithShield", "Granite Bash", VfxId.DustStone, 0.9f }, new object[]{ "MonolithShield", "Monolith Stance", VfxId.RuneGold, 0.9f },
  new object[]{ "GatekeepersOath", "Oath Bind", VfxId.RuneBlue, 1f }, new object[]{ "GatekeepersOath", "Gate Cleave", VfxId.GroundShock, 1.2f }, new object[]{ "GatekeepersOath", "Stone Sentinels", VfxId.RuneBlue, 1f },
};
int set = 0; var miss = new System.Collections.Generic.List<string>();
var byWeapon = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object[]>>();
foreach (var r in cv) { string k = (string)r[0]; if (!byWeapon.ContainsKey(k)) byWeapon[k] = new System.Collections.Generic.List<object[]>(); byWeapon[k].Add(r); }
foreach (var kv in byWeapon) {
  string path = "Assets/Resources/Weapon/" + kv.Key + ".prefab";
  var root = PrefabUtility.LoadPrefabContents(path);
  var skills = root.GetComponentsInChildren<Skill>(true);
  foreach (var r in kv.Value) {
    bool found = false;
    foreach (var s in skills) if (s.skillName == (string)r[1]) { s.castVfx = (VfxId)r[2]; s.castVfxScale = (float)r[3]; found = true; set++; }
    if (!found) miss.Add(kv.Key + "/" + r[1]);
  }
  PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
}
sb.Append("castVfx " + set + " miss [" + string.Join(",", miss.ToArray()) + "]; ");
// ⑥ 구역 분위기
string[][] maps = { new[]{ "Lv1_DeepForest", "DeepForest" }, new[]{ "Lv1_Swamp", "Swamp" }, new[]{ "Lv1_BanditFortress", "Bandit" }, new[]{ "Lv1_TowerOutskirts", "TowerRunes" } };
var mapRoot = GameObject.Find("Map");
if (mapRoot == null) return sb + " Map 없음(SampleScene 열기)";
foreach (var m in maps) {
  var t = mapRoot.transform.Find(m[0]);
  var mi = t != null ? t.GetComponent<MapInfo>() : null;
  if (mi == null) { sb.Append(m[0] + " ?; "); continue; }
  mi.ambience = (AmbienceKind)System.Enum.Parse(typeof(AmbienceKind), m[1]);
  EditorUtility.SetDirty(mi);
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mapRoot.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(mapRoot.scene);
AssetDatabase.SaveAssets();
return sb.ToString();
