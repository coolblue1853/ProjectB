// Resources/UI/UiTheme.asset 생성 (픽셀 폰트 연결)
if (!AssetDatabase.IsValidFolder("Assets/Resources/UI")) AssetDatabase.CreateFolder("Assets/Resources", "UI");
var t = AssetDatabase.LoadAssetAtPath<UiTheme>("Assets/Resources/UI/UiTheme.asset");
if (t == null) { t = ScriptableObject.CreateInstance<UiTheme>(); AssetDatabase.CreateAsset(t, "Assets/Resources/UI/UiTheme.asset"); }
t.pixelFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ThirdParty/Font/ThaleahFat_TMP.asset");
EditorUtility.SetDirty(t); AssetDatabase.SaveAssets();
return "UiTheme font=" + (t.pixelFont != null ? t.pixelFont.name : "null");
