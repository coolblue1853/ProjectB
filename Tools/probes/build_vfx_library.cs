// Resources/FX/VfxLibrary.asset 생성/갱신 (Magic Slashes FX 매핑)
const string D = "Assets/ThirdParty/MagicSlashesFX/Prefabs/EffectPrefabs/";
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
if (lib == null) { lib = ScriptableObject.CreateInstance<VfxLibrary>(); AssetDatabase.CreateAsset(lib, "Assets/Resources/FX/VfxLibrary.asset"); }
lib.entries.Clear();
var rows = new object[][] {
  new object[]{ VfxId.CritCross, "Eff10_Slash", 0f, 0.4f },
  new object[]{ VfxId.SlashWhite, "Eff8", -90f, 0.22f },
  new object[]{ VfxId.SlashRed, "Eff1_Right", -90f, 0.35f },
  new object[]{ VfxId.SlashBlue, "Eff3_Right", -90f, 0.35f },
  new object[]{ VfxId.SlashGold, "Eff9_Right", -90f, 0.35f },
  new object[]{ VfxId.SlashGoldWide, "Eff12", -90f, 0.35f },
  new object[]{ VfxId.SlashGreenBig, "Eff7_Slash", -90f, 0.2f },
  new object[]{ VfxId.CrackRed, "Eff1_Crack", 0f, 0.4f },
};
var sb = new System.Text.StringBuilder();
foreach (var r in rows) {
  var pf = AssetDatabase.LoadAssetAtPath<GameObject>(D + (string)r[1] + ".prefab");
  float life = 0.3f;
  foreach (var ps in pf.GetComponentsInChildren<ParticleSystem>(true)) { var m = ps.main; life = Mathf.Max(life, m.startDelay.constantMax + m.startLifetime.constantMax); }
  life = Mathf.Min(life, 1.5f);
  lib.entries.Add(new VfxLibrary.Entry { id = (VfxId)r[0], prefab = pf, euler = new Vector3((float)r[2], 0f, 0f), scale = (float)r[3], lifetime = life });
  sb.Append(r[0] + "=" + life.ToString("0.00") + " ");
}
EditorUtility.SetDirty(lib); AssetDatabase.SaveAssets();
return sb.ToString();
