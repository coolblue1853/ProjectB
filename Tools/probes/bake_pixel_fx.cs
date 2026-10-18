// Magic Slashes FX → 도트 이펙트 8종 베이크 후 VfxLibrary 에 연결 (PixelVfxBaker). 재실행 가능
var P = "Assets/ThirdParty/MagicSlashesFX/Prefabs/EffectPrefabs/";
var R = ProjectB.EditorTools.PixelVfxBaker.Ramps;
var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/FX/M_PixelFx.mat");
if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")); AssetDatabase.CreateAsset(mat, "Assets/Resources/FX/M_PixelFx.mat"); }
var rows = new object[][] {
  new object[]{ VfxId.CritCross, "Eff10_Slash", 0f, 0.4f, 2.5f, "gold", 1, 0.6f, 0.12f },
  new object[]{ VfxId.SlashWhite, "Eff8", -90f, 0.3f, 2.5f, "white", 1, 0.5f, 0.06f },
  new object[]{ VfxId.SlashRed, "Eff1_Right", -90f, 0.35f, 2.5f, "red", 1, 0.6f, 0.06f },
  new object[]{ VfxId.SlashBlue, "Eff3_Right", -90f, 0.35f, 2.5f, "blue" },
  new object[]{ VfxId.SlashGold, "Eff9_Right", -90f, 0.35f, 2.5f, "gold" },
  new object[]{ VfxId.SlashGoldWide, "Eff12", -90f, 0.35f, 3f, "gold" },
  new object[]{ VfxId.SlashGreenBig, "Eff7_Slash", -90f, 0.2f, 4f, "green", 0, 0.4f, 0.06f },
  new object[]{ VfxId.CrackRed, "Eff1_Crack", 0f, 1f, 3f, "red", 1, 0.6f, 0.06f },
};
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
var sb = new System.Text.StringBuilder();
foreach (var r in rows) {
  var id = (VfxId)r[0]; var name = "Px_" + id;
  var spec = new ProjectB.EditorTools.PixelVfxBaker.Spec { name = name, prefabPath = P + (string)r[1] + ".prefab", euler = new Vector3((float)r[2], 0, 0), scale = (float)r[3], halfExtent = (float)r[4], ramp = R[(string)r[5]] };
  if (r.Length > 6) { spec.dilate = (int)r[6]; spec.lifetime = (float)r[7]; spec.startTime = (float)r[8]; }
  sb.AppendLine(ProjectB.EditorTools.PixelVfxBaker.Bake(spec));
  var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/" + name + ".asset");
  foreach (var e in lib.entries) if (e.id == id) { e.pixel = clip; e.usePixel = true; }
}
EditorUtility.SetDirty(lib); AssetDatabase.SaveAssets();
return sb.ToString();
