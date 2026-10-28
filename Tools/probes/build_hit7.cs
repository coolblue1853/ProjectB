// 타격 이펙트 v7 등록(재실행 안전): gen_hit.py 의 Px_WHit7 → PixelFxClip(24 PPU) → VfxLibrary(WHit7) 항목 추가·갱신. 먼저 python Tools/pixelfx/gen_hit.py
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
string r = ProjectB.EditorTools.PixelVfxBaker.ImportSheet("Px_WHit7");
var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/Px_WHit7.asset");
if (clip == null) return "없음: " + r;
var e = lib.entries.Find(x => x.id == VfxId.WHit7);
if (e == null)
{
    var src = lib.entries.Find(x => x.id == VfxId.WHitStar);
    e = new VfxLibrary.Entry { id = VfxId.WHit7, prefab = src != null ? src.prefab : null, euler = Vector3.zero, scale = 1f, sortingOrder = src != null ? src.sortingOrder : 210 };
    lib.entries.Add(e);
}
e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return r + " lifetime=" + e.lifetime;
