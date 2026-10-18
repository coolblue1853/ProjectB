// B-130: 근교 원본 지형 계열 맵의 맵 끝 낙사 방지 — 보이지 않는 벽(Wall 레이어 BoxCollider2D)을 지형 끝에 딱 붙여 세운다. 에디트 모드, 재실행 안전(기존 EdgeWall_* 를 지우고 다시 만든다)
// 플레이 측정(2026-10-19, 끝에서 바깥 방향키 3초): 오른쪽 끝(지형 x -136.8)은 샛길·일반 숲·산적 성채·깊은 숲·탑 근교 모두 낙사,
// 왼쪽 끝(x -436.8)은 깊은 숲·탑 근교 낙사(탑 근교의 기존 벽 x -438 은 지형 끝과 1.2 유닛 틈이 있어 그 사이로 떨어졌다). 샛길 계열 왼쪽(-752.8)·늪지·Lv2 는 지형 벽이 있다
// 맵을 다시 빌드(build_deep_forest·build_tower_outskirts 등)한 뒤에는 이 프로브를 다시 돌린다
var sb = new System.Text.StringBuilder();
var mapRoot = GameObject.Find("Map").transform;
int wallLayer = LayerMask.NameToLayer("Wall");
// 오른쪽: 마지막 발판(높이 -4.6)이 x -138.85 에서 끝나고 그 너머 2 유닛은 깊이 10 의 구덩이(-14.6) → 벽이 구덩이까지 메워 갇히지 않게 한다
const float RightLedgeEnd = -138.85f, RightOuter = -135.8f, LeftEdgeNear = -436.8f, Bottom = -40f, Top = 90f;
string[] rightMaps = { "Lv1_NearForest_SideRoad", "Lv1_NormalForest", "Lv1_BanditFortress", "Lv1_DeepForest", "Lv1_TowerOutskirts" };
string[] leftMaps = { "Lv1_DeepForest", "Lv1_TowerOutskirts" };
System.Action<Transform, string, float, float> make = (map, wallName, x, w) => {
    var old = map.Find(wallName);
    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var go = new GameObject(wallName);
    go.transform.SetParent(map, false);
    go.layer = wallLayer;
    go.transform.position = new Vector3(x, (Bottom + Top) * 0.5f, 0f);
    var box = go.AddComponent<BoxCollider2D>();
    box.size = new Vector2(w, Top - Bottom);
    sb.Append(map.name + "/" + wallName + " x" + x.ToString("F1") + "; ");
};
foreach (var n in rightMaps)
{
    var m = mapRoot.Find(n);
    if (m == null) { sb.Append("missing " + n + "; "); continue; }
    make(m, "EdgeWall_Right", (RightLedgeEnd + RightOuter) * 0.5f, RightOuter - RightLedgeEnd);
}
foreach (var n in leftMaps)
{
    var m = mapRoot.Find(n);
    if (m == null) { sb.Append("missing " + n + "; "); continue; }
    make(m, "EdgeWall_Left", LeftEdgeNear - 0.5f, 1f);
}
var sc = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sc);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sc);
return sb.ToString();
