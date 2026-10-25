// 타격 이펙트 v2 등록(재실행 안전): gen_hit_impact.py 의 Px_WHitStar 시트 → PixelFxClip 재슬라이스 → VfxLibrary(WHitStar) 수명 갱신. 먼저 python Tools/pixelfx/gen_hit_impact.py
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
string r = ProjectB.EditorTools.PixelVfxBaker.ImportSheet("Px_WHitStar");
var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/Px_WHitStar.asset");
var e = lib.entries.Find(x => x.id == VfxId.WHitStar);
if (clip == null || e == null) return "없음: " + r;
e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return r + " lifetime=" + e.lifetime;
