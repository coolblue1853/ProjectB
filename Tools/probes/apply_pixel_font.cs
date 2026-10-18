// G7: UI 폰트를 ThaleahFat 픽셀 폰트로 통일 (에디트 모드). 씬 TMP·레거시 Text + Assets/Prefab·Resources 프리팹 TMP + TMP 기본 폰트. 한글 텍스트 없음 확인됨(2026-10-18)
var tmpFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ThirdParty/Font/ThaleahFat_TMP.asset");
var ttf = AssetDatabase.LoadAssetAtPath<Font>("Assets/ThirdParty/Font/ThaleahFat_TTF.ttf");
int scene = 0, legacy = 0, prefabs = 0;
foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
  if (t.font == null || t.font.name.StartsWith("LiberationSans")) { Undo.RecordObject(t, "font"); t.font = tmpFont; EditorUtility.SetDirty(t); scene++; }
foreach (var t in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
  { t.font = ttf; EditorUtility.SetDirty(t); PrefabUtility.RecordPrefabInstancePropertyModifications(t); legacy++; }
foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[]{ "Assets/Prefab", "Assets/Resources" })) {
  var path = AssetDatabase.GUIDToAssetPath(g);
  var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
  bool need = false;
  foreach (var t in pf.GetComponentsInChildren<TMPro.TMP_Text>(true)) if (t.font == null || t.font.name.StartsWith("LiberationSans")) need = true;
  if (!need) continue;
  var root = PrefabUtility.LoadPrefabContents(path);
  foreach (var t in root.GetComponentsInChildren<TMPro.TMP_Text>(true)) if (t.font == null || t.font.name.StartsWith("LiberationSans")) t.font = tmpFont;
  PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root); prefabs++;
}
var settings = Resources.Load<TMPro.TMP_Settings>("TMP Settings");
if (settings != null) { var so = new SerializedObject(settings); so.FindProperty("m_defaultFontAsset").objectReferenceValue = tmpFont; so.ApplyModifiedPropertiesWithoutUndo(); }
var sc = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sc); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sc);
AssetDatabase.SaveAssets();
return "sceneTMP=" + scene + " legacy=" + legacy + " prefabs=" + prefabs + " defaultFont=" + (settings != null);
// (apply_pixel_font 이후) 레거시 Text 는 같은 크기에서 작게 보여 1.5배 — Logs/_lfs.cs 와 동일 처리
