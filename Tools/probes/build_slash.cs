// 베기 궤적 v2 등록(재실행 안전): gen_slash.py 의 Px_WSlashDown·Px_WSlashSide → PixelFxClip 재슬라이스 → VfxLibrary 수명 갱신. 먼저 python Tools/pixelfx/gen_slash.py
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
var sb = new System.Text.StringBuilder();
foreach (var pair in new[] { ("Px_WSlashDown", VfxId.WSlashDown), ("Px_WSlashSide", VfxId.WSlashSide) })
{
    sb.Append(ProjectB.EditorTools.PixelVfxBaker.ImportSheet(pair.Item1) + "; ");
    var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/" + pair.Item1 + ".asset");
    var e = lib.entries.Find(x => x.id == pair.Item2);
    if (clip == null || e == null) return "없음: " + pair.Item1;
    e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
}
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return sb.ToString();
