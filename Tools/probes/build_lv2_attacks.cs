// Lv2 전용 공격 프리팹 복제(피해 = Lv2ContentPlan 3.3) + Lv2 스포너 objectInfos 교체(키 유지 → BT 무변경) + 씬 저장. 에디트 모드에서 실행
const string Dir = "Assets/Prefab/Enemy/EnemyDamge/Lv2";
if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Prefab/Enemy/EnemyDamge", "Lv2");
// 스포너 이름 접두 → (키, 새 이름, 피해)
var plan = new System.Collections.Generic.Dictionary<string, object[][]> {
  { "Shroom", new[]{ new object[]{ "leaf", "L2_ShroomSpore", 40 } } },
  { "Goblin", new[]{ new object[]{ "stab", "L2_GoblinSlash", 55 } } },
  { "Eye",    new[]{ new object[]{ "flower", "L2_EyeGaze", 45 } } },
  { "Skel",   new[]{ new object[]{ "stab", "L2_SkelSlash", 70 } } },
  { "Elite",  new[]{ new object[]{ "clow", "L2_BringerScythe", 110 }, new object[]{ "fire", "L2_BringerFire", 60 } } },
  { "Boss",   new[]{ new object[]{ "clow", "L2_WizardSlash", 130 }, new object[]{ "fire", "L2_WizardOrb", 60 } } },
};
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var sb = new System.Text.StringBuilder();
var lv2 = GameObject.Find("Map").transform.Find("Lv2_MagicCliffs");
var made = new System.Collections.Generic.Dictionary<string, GameObject>();
foreach (var sp in lv2.GetComponentsInChildren<EnemySpawner>(true)) {
  object[][] rows = null;
  foreach (var kv in plan) if (sp.name.Contains(kv.Key)) rows = kv.Value;
  if (rows == null) continue;
  var so = new SerializedObject(sp);
  var arr = so.FindProperty("objectInfos");
  for (int i = 0; i < arr.arraySize; i++) {
    var el = arr.GetArrayElementAtIndex(i);
    var key = el.FindPropertyRelative("objectName").stringValue;
    foreach (var r in rows) {
      if (key != (string)r[0]) continue;
      var name = (string)r[1];
      GameObject copy;
      if (!made.TryGetValue(name, out copy)) {
        var src = (GameObject)el.FindPropertyRelative("perfab").objectReferenceValue;
        var srcPath = AssetDatabase.GetAssetPath(src);
        var dst = Dir + "/" + name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(dst) == null) AssetDatabase.CopyAsset(srcPath, dst);
        var root = PrefabUtility.LoadPrefabContents(dst);
        foreach (var d in root.GetComponentsInChildren<EnemyDamageObject>(true)) d.damage = (int)r[2];
        PrefabUtility.SaveAsPrefabAsset(root, dst); PrefabUtility.UnloadPrefabContents(root);
        copy = AssetDatabase.LoadAssetAtPath<GameObject>(dst); made[name] = copy;
      }
      el.FindPropertyRelative("perfab").objectReferenceValue = copy;
      sb.Append(sp.name + "." + key + "→" + name + "(" + r[2] + ") ");
    }
  }
  so.ApplyModifiedPropertiesWithoutUndo();
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(lv2.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(lv2.gameObject.scene);
return sb.ToString();
// 참고: Lv2MapBuilder로 맵을 다시 만들면 스포너가 새로 생기므로 이 프로브를 다시 실행하고, 정예 스포너 eliteChance=0 도 다시 맞춘다.
