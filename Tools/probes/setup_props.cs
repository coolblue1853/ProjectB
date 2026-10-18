// 1층계 파괴 가능 오브젝트 배치 (에디트 모드): 시트 임포트(3프레임) → Lv1 맵 바닥 위 x 간격으로 통·상자 교대 배치, 드롭 = 나무/돌/약초. 재실행 시 기존 Props 삭제 후 재배치
var sb = new System.Text.StringBuilder();
foreach (var n in new[]{ "Barrel", "Crate" }) {
  var png = "Assets/Art/Props/" + n + ".png";
  var imp = (TextureImporter)AssetImporter.GetAtPath(png);
  var js = System.IO.File.ReadAllText("Assets/Art/Props/" + n + ".json");
  int mw = int.Parse(System.Text.RegularExpressions.Regex.Match(js, @"""w"":\s*(\d+)").Groups[1].Value), mh = int.Parse(System.Text.RegularExpressions.Regex.Match(js, @"""h"":\s*(\d+)").Groups[1].Value);
  imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple; imp.filterMode = FilterMode.Point;
  imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.spritePixelsPerUnit = 24;
  var metas = new SpriteMetaData[3];
  for (int f = 0; f < 3; f++) metas[f] = new SpriteMetaData { name = n + "_" + f, rect = new Rect(f * mw, 0, mw, mh), alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(0.5f, 0f) };
  imp.spritesheet = metas; imp.SaveAndReimport();
}
var map = GameObject.Find("Map").transform;
int placed = 0;
foreach (var mapName in new[]{ "Lv1_NearForest", "Lv1_NearForest_SideRoad" }) {
  var m = map.Find(mapName); if (m == null) continue;
  var old = m.Find("Props"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
  var root = new GameObject("Props"); root.transform.SetParent(m, false);
  var spawners = m.GetComponentsInChildren<EnemySpawner>(true);
  var dm = spawners.Length > 0 ? spawners[0].GetComponent<DropManager>() : null;
  // 맵 x 범위: 스포너 위치들의 min~max, 바닥 y: 스포너 콜라이더 아래 레이캐스트
  float minX = float.MaxValue, maxX = float.MinValue, y0 = 0f;
  foreach (var sp in spawners) { minX = Mathf.Min(minX, sp.transform.position.x); maxX = Mathf.Max(maxX, sp.transform.position.x); y0 = sp.transform.position.y; }
  if (spawners.Length == 0) continue;
  var layer = LayerMask.GetMask("Ground");
  for (float x = minX + 3f; x < maxX; x += 9f) {
    var hit = Physics2D.Raycast(new Vector2(x, y0 + 6f), Vector2.down, 30f, layer);
    if (hit.collider == null) continue;
    bool barrel = placed % 2 == 0; string n = barrel ? "Barrel" : "Crate";
    var go = new GameObject(n); go.transform.SetParent(root.transform, false); go.transform.position = new Vector3(x, hit.point.y, 0f);
    var sr = go.AddComponent<SpriteRenderer>(); var sprites = new System.Collections.Generic.List<Sprite>();
    foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Props/" + n + ".png")) if (o is Sprite s) sprites.Add(s);
    sprites.Sort((a, b) => a.rect.x.CompareTo(b.rect.x)); sr.sprite = sprites[0]; sr.sortingOrder = 5;
    var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(0.9f, 1f); col.offset = new Vector2(0f, 0.5f);
    var bp = go.AddComponent<BreakableProp>();
    var so = new SerializedObject(bp);
    var fr = so.FindProperty("_frames"); fr.arraySize = 3; for (int i = 0; i < 3; i++) fr.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    so.FindProperty("_dropSource").objectReferenceValue = dm;
    var dr = so.FindProperty("_drops"); dr.arraySize = 3;
    string[] items = { "Wood", "T0_Ore", "GreenHurb" }; float[] ch = { 0.8f, 0.35f, 0.3f };
    for (int i = 0; i < 3; i++) { var e = dr.GetArrayElementAtIndex(i); e.FindPropertyRelative("itemName").stringValue = items[i]; e.FindPropertyRelative("chance").floatValue = ch[i]; }
    so.ApplyModifiedPropertiesWithoutUndo(); placed++;
  }
  sb.Append(mapName + " x[" + minX.ToString("0") + "," + maxX.ToString("0") + "]; ");
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return "placed " + placed + " | " + sb;
