// 기본 칼 3타 스미어 등록(재실행 안전): gen_smear.py 의 Px_WSmear* → PixelFxClip(PPU 36, 셀과 같은 도트 크기) → VfxLibrary 항목 추가·갱신. 먼저 python Tools/pixelfx/gen_smear.py
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
var sb = new System.Text.StringBuilder();
var src = lib.entries.Find(x => x.id == VfxId.WSlashDown);
foreach (var pair in new[] { ("Px_WSmearDown", VfxId.WSmearDown), ("Px_WSmearUp", VfxId.WSmearUp), ("Px_WSmearStab", VfxId.WSmearStab) })
{
    sb.Append(ProjectB.EditorTools.PixelVfxBaker.ImportSheet(pair.Item1, 36f) + "; ");
    var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/" + pair.Item1 + ".asset");
    if (clip == null) return "없음: " + pair.Item1;
    var e = lib.entries.Find(x => x.id == pair.Item2);
    if (e == null)
    {
        e = new VfxLibrary.Entry { id = pair.Item2, prefab = src != null ? src.prefab : null, euler = Vector3.zero, scale = 1f };
        lib.entries.Add(e);
    }
    e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
    e.sortingOrder = src != null ? src.sortingOrder : 210; // 스컬처럼 몸 위에 덮는다
}
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return sb.ToString();
