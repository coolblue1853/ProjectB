// 발걸음 먼지 등록(재실행 안전): gen_smoke_v2.py 의 Px_StepDust 시트 → PixelFxClip → VfxLibrary(StepDust). 먼저 python Tools/pixelfx/gen_smoke_v2.py(또는 step_dust 만)
var lib = AssetDatabase.LoadAssetAtPath<VfxLibrary>("Assets/Resources/FX/VfxLibrary.asset");
string r = ProjectB.EditorTools.PixelVfxBaker.ImportSheet("Px_StepDust");
var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>(ProjectB.EditorTools.PixelVfxBaker.OutDir + "/Px_StepDust.asset");
if (clip == null) return "clip 없음: " + r;
var e = lib.entries.Find(x => x.id == VfxId.StepDust);
if (e == null) { e = new VfxLibrary.Entry { id = VfxId.StepDust }; lib.entries.Add(e); }
e.pixel = clip; e.usePixel = true; e.lifetime = clip.Duration;
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return r + " lifetime=" + e.lifetime;
