// 마스터 모션 스미어 등록(재실행 안전): gen_motion_fx.py 의 Px_MSmear<n> → PixelFxClip(PPU 36, 셀과 같은 도트 크기) → VfxLibrary(MSmear<n>) 항목 추가·갱신. 먼저 python Tools/pixelfx/gen_motion_fx.py
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
var sb = new System.Text.StringBuilder();
var src = lib.entries.Find(x => x.id == VfxId.WSmearDown);
for (int n = 0; n < MotionFxTable.SlotCount; n++)
{
    string name = "Px_MSmear" + n;
    var id = VfxId.MSmear0 + n;
    sb.Append(ProjectB.EditorTools.PixelVfxBaker.ImportSheet(name, 36f) + "; ");
    var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/" + name + ".asset");
    if (clip == null) return "없음: " + name;
    var e = lib.entries.Find(x => x.id == id);
    if (e == null)
    {
        e = new VfxLibrary.Entry { id = id, prefab = src != null ? src.prefab : null, euler = Vector3.zero, scale = 1f };
        lib.entries.Add(e);
    }
    e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
    e.sortingOrder = src != null ? src.sortingOrder : 210;
}
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return sb.ToString();
