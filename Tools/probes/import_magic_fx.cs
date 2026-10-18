// gen_magic.py 산출 시트 12종 → PixelFxClip + VfxLibrary 연결 + 지팡이 테마 재적용
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
var sb = new System.Text.StringBuilder();
foreach (var kind in new[]{ "Burst", "Pillar", "Rune" }) foreach (var tag in new[]{ "Blue", "Purple", "Gold", "Green" }) {
  var name = "Px_" + kind + tag;
  sb.AppendLine(ProjectB.EditorTools.PixelVfxBaker.ImportSheet(name));
  var id = (VfxId)System.Enum.Parse(typeof(VfxId), kind + tag);
  var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/" + name + ".asset");
  var e = lib.entries.Find(x => x.id == id);
  if (e == null) { e = new VfxLibrary.Entry { id = id }; lib.entries.Add(e); }
  e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
}
EditorUtility.SetDirty(lib); AssetDatabase.SaveAssets();
sb.Append(ProjectB.EditorTools.Lv2ItemBuilder.ApplyVfx());
return sb.ToString();
