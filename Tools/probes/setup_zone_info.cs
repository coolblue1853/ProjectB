// 맵 루트에 MapInfo(층계·영문 구역명) 설정 — TowerWorld.md 4절 (에디트 모드, 씬 저장)
var map = GameObject.Find("Map").transform;
var rows = new object[][] {
  new object[]{ "Town", 0, "Village at the Tower's Foot", 0 },
  new object[]{ "Lv1_NearForest", 1, "Forest Outskirts", 0, true },
  new object[]{ "Lv1_NearForest_SideRoad", 1, "Outskirts - Side Road", 5, true },
  new object[]{ "Lv2_MagicCliffs", 2, "Undercloud Cliffs", 25 },
};
var sb = new System.Text.StringBuilder();
foreach (var r in rows) {
  var t = map.Find((string)r[0]); if (t == null) { sb.Append("없음 " + r[0] + "; "); continue; }
  var info = t.GetComponent<MapInfo>(); if (info == null) info = t.gameObject.AddComponent<MapInfo>();
  info.floor = (int)r[1]; info.zoneTitle = (string)r[2]; info.ascentLoad = (int)r[3]; info.fireflies = r.Length > 4 && (bool)r[4]; if (string.IsNullOrEmpty(info.displayName)) info.displayName = (string)r[2];
  EditorUtility.SetDirty(info); sb.Append(r[0] + "=F" + r[1] + " " + r[2] + " load=" + info.ascentLoad + "; ");
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return sb.ToString();
