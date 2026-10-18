// ② 일반 숲은 보스가 없는 구역인데, 복제 원본(샛길)의 야간 보스 스포너를 SetActive(false) 로만 꺼 둬서
// 포털·웨이포인트 진입 때 PoolCheck.AbleSpwaner 가 모든 스포너를 다시 켜 PassiveTerror 가 나왔다(2026-10-19 회귀 소크) → 스포너를 지운다
var sb = new System.Text.StringBuilder();
var m = GameObject.Find("Map").transform.Find("Lv1_NormalForest");
foreach (var sp in m.GetComponentsInChildren<EnemySpawner>(true))
{
    if (!sp.name.StartsWith("Boss")) continue;
    sb.Append("removed " + sp.transform.parent.name + "/" + sp.name + "; ");
    UnityEngine.Object.DestroyImmediate(sp.gameObject);
}
var sc = m.gameObject.scene;
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sc);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sc);
return sb.Length == 0 ? "none" : sb.ToString();
