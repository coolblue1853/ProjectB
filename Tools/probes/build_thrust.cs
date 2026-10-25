// 찌르기 궤적 v2 등록(재실행 안전): gen_thrust.py 의 Px_WThrust 시트 → PixelFxClip 재슬라이스 → VfxLibrary(WHitStar) 수명 갱신. 먼저 python Tools/pixelfx/gen_hit_impact.py
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
string r = ProjectB.EditorTools.PixelVfxBaker.ImportSheet("Px_WThrust");
var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/Px_WThrust.asset");
var e = lib.entries.Find(x => x.id == VfxId.WThrust);
if (clip == null || e == null) return "없음: " + r;
e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return r + " lifetime=" + e.lifetime;
