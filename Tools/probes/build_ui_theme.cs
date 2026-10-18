// Resources/UI/UiTheme.asset 생성/갱신 (픽셀 폰트 + Art/UI 프레임 9-slice)
foreach (var n in new[]{ "Frame", "FrameLight", "Divider" }) {
  var path = "Assets/Art/UI/" + n + ".png"; var imp = (TextureImporter)AssetImporter.GetAtPath(path);
  imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single; imp.filterMode = FilterMode.Point;
  imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.spritePixelsPerUnit = 100;
  imp.spriteBorder = n == "Divider" ? Vector4.zero : new Vector4(8, 8, 8, 8); imp.SaveAndReimport();
}
if (!AssetDatabase.IsValidFolder("Assets/Resources/UI")) AssetDatabase.CreateFolder("Assets/Resources", "UI");
var t = AssetDatabase.LoadAssetAtPath<UiTheme>("Assets/Resources/UI/UiTheme.asset");
if (t == null) { t = ScriptableObject.CreateInstance<UiTheme>(); AssetDatabase.CreateAsset(t, "Assets/Resources/UI/UiTheme.asset"); }
t.pixelFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ThirdParty/Font/ThaleahFat_TMP.asset");
t.frame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frame.png");
t.frameLight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/FrameLight.png");
t.divider = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Divider.png");
EditorUtility.SetDirty(t); AssetDatabase.SaveAssets();
return "UiTheme font=" + (t.pixelFont != null) + " frame=" + (t.frame != null) + " light=" + (t.frameLight != null);
