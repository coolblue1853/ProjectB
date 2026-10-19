// 한글 도트 폰트 TMP 에셋(Galmuri11 Bold, OFL): RASTER·12px·동적 아틀라스·Point 필터, ThaleahFat 폴백으로 연결(Q-407). 재실행 안전
string ttfPath = "Assets/ThirdParty/Font/Galmuri/Galmuri11-Bold.ttf";
string outPath = "Assets/ThirdParty/Font/Galmuri/Galmuri11Bold_TMP.asset";
var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
if (font == null) return "no ttf";
var fa = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(outPath);
if (fa == null)
{
    fa = TMPro.TMP_FontAsset.CreateFontAsset(font, 12, 2, UnityEngine.TextCore.LowLevel.GlyphRenderMode.RASTER, 1024, 1024, TMPro.AtlasPopulationMode.Dynamic, true);
    fa.name = "Galmuri11Bold_TMP";
    AssetDatabase.CreateAsset(fa, outPath);
    fa.atlasTextures[0].name = "Galmuri11Bold Atlas";
    AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
    fa.material.name = "Galmuri11Bold Material";
    AssetDatabase.AddObjectToAsset(fa.material, fa);
}
fa.atlasTextures[0].filterMode = FilterMode.Point;
fa.isMultiAtlasTexturesEnabled = true;
fa.ClearFontAssetData(true); // 저장소에는 빈 아틀라스로 — 실행 중 필요한 글자만 동적으로 채운다

var so = new SerializedObject(fa); var cp = so.FindProperty("m_ClearDynamicDataOnBuild"); if (cp != null) { cp.boolValue = true; so.ApplyModifiedPropertiesWithoutUndo(); }

string missing = "";
EditorUtility.SetDirty(fa);
var thaleah = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ThirdParty/Font/ThaleahFat_TMP.asset");
if (thaleah != null)
{
    if (thaleah.fallbackFontAssetTable == null) thaleah.fallbackFontAssetTable = new System.Collections.Generic.List<TMPro.TMP_FontAsset>();
    if (!thaleah.fallbackFontAssetTable.Contains(fa)) thaleah.fallbackFontAssetTable.Add(fa);
    EditorUtility.SetDirty(thaleah);
}
AssetDatabase.SaveAssets();
return "galmuri tmp ok missing=[" + missing + "] fallbackOf thaleah=" + (thaleah != null && thaleah.fallbackFontAssetTable.Contains(fa)) + " glyphs=" + fa.characterTable.Count;
