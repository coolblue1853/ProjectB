// 일반 숲 스포너 밀도 조정: 늑대·멧돼지 스포너 enemyCount 절반(최소 2) — 신규 지역 진입 직후 몰려 죽는 문제
var m = GameObject.Find("Map").transform.Find("Lv1_NormalForest"); var sb = new System.Text.StringBuilder();
foreach (var s in m.GetComponentsInChildren<EnemySpawner>(true))
{
    if (!(s.name.StartsWith("Rabbit") || s.name.StartsWith("Mole"))) continue;
    int before = s.enemyCount; s.enemyCount = Mathf.Max(2, before / 2); EditorUtility.SetDirty(s);
    sb.Append(s.name + " " + before + "->" + s.enemyCount + "; ");
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(m.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(m.gameObject.scene);
return sb.ToString();
