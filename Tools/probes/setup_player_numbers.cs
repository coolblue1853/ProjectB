// 플레이어 회복·피격 숫자 프리팹 연결 (에디트 모드)
var ph = UnityEngine.Object.FindFirstObjectByType<PlayerHealthManager>(FindObjectsInactive.Include);
var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/EnemyDamge.prefab").GetComponent<DamageNumbersPro.DamageNumber>();
var so = new SerializedObject(ph); so.FindProperty("_numberPrefab").objectReferenceValue = pf; so.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ph.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(ph.gameObject.scene);
return "assigned " + (pf != null);
