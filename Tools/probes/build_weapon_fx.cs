// 기본 무기 평타 이펙트 등록 (GameFeelPlan 10.7): PixelLab 시트 4종 → PixelFxClip, M_PixelSlash 재질, VfxLibrary 항목.
// 시트는 Tools/pixelfx/import_pixellab_vfx.py 산출물(Assets/Art/PixelFx/Px_W*.png/json). 다시 실행해도 안전(덮어쓰기).
var t = System.Type.GetType("ProjectB.EditorTools.PixelVfxBaker, Assembly-CSharp-Editor");
var import = t.GetMethod("ImportSheet");
var sb = new System.Text.StringBuilder();
string[] names = { "Px_WSlashDown", "Px_WSlashSide", "Px_WThrust", "Px_WHitStar" };
VfxId[] ids = { VfxId.WSlashDown, VfxId.WSlashSide, VfxId.WThrust, VfxId.WHitStar };
foreach (var n in names) sb.Append(import.Invoke(null, new object[] { n, 24f }) + " | ");

// 셰이더 Tier-2: 실제 컴파일 에러 확인
var shader = Shader.Find("ProjectB/PixelSlash2D");
sb.Append(" shader=" + (shader != null) + " hasError=" + (shader != null && ShaderUtil.ShaderHasError(shader)) + " supported=" + (shader != null && shader.isSupported));

string matPath = "Assets/Resources/FX/M_PixelSlash.mat";
var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
mat.shader = shader;
EditorUtility.SetDirty(mat);

var lib = Resources.Load<VfxLibrary>("FX/VfxLibrary");
for (int i = 0; i < ids.Length; i++)
{
    var clip = AssetDatabase.LoadAssetAtPath<PixelFxClip>("Assets/Art/PixelFx/" + names[i] + ".asset");
    VfxLibrary.Entry e = null;
    foreach (var x in lib.entries) if (x.id == ids[i]) e = x;
    if (e == null) { e = new VfxLibrary.Entry(); e.id = ids[i]; lib.entries.Add(e); }
    e.pixel = clip;
    e.usePixel = true;
    e.prefab = null;
    e.sortingOrder = ids[i] == VfxId.WHitStar ? 215 : 210;
    e.lifetime = clip != null ? clip.Duration : 0.4f;
}
EditorUtility.SetDirty(lib);
AssetDatabase.SaveAssets();
return sb.ToString() + " entries=" + lib.entries.Count;
