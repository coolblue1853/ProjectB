// 1층계 숲 맵(Lv1 두 맵) 바닥 위에 식생 장식 배치 (에디트 모드, 씬 저장). 재실행 시 기존 Vegetation 삭제 후 재배치. 콜라이더 없음, 정렬 = 캐릭터 뒤(-2)/앞(풀 일부 +6)
var names = new[]{ "Grass1", "Grass2", "GrassLight", "MushroomRed", "MushroomBrown", "FlowerWhite", "FlowerPink", "OddTree" };
AssetDatabase.Refresh();
foreach (var n in names) { AssetDatabase.ImportAsset("Assets/Art/Vegetation/" + n + ".png", ImportAssetOptions.ForceSynchronousImport); var imp = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Vegetation/" + n + ".png"); imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single; imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.spritePixelsPerUnit = 24; var st = new TextureImporterSettings(); imp.ReadTextureSettings(st); st.spriteAlignment = (int)SpriteAlignment.BottomCenter; imp.SetTextureSettings(st); imp.SaveAndReimport(); }
var sprites = new System.Collections.Generic.Dictionary<string, Sprite>(); foreach (var n in names) sprites[n] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Vegetation/" + n + ".png");
var rng = new System.Random(11); var map = GameObject.Find("Map").transform; int placed = 0; var layer = LayerMask.GetMask("Ground");
foreach (var mapName in new[]{ "Lv1_NearForest", "Lv1_NearForest_SideRoad" }) {
  var m = map.Find(mapName); if (m == null) continue;
  var old = m.Find("Vegetation"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
  var root = new GameObject("Vegetation"); root.transform.SetParent(m, false);
  var spawners = m.GetComponentsInChildren<EnemySpawner>(true); if (spawners.Length == 0) continue;
  float minX = float.MaxValue, maxX = float.MinValue, y0 = 0f;
  foreach (var sp in spawners) { minX = Mathf.Min(minX, sp.transform.position.x); maxX = Mathf.Max(maxX, sp.transform.position.x); y0 = sp.transform.position.y; }
  for (float x = minX; x < maxX; x += 1.6f + (float)rng.NextDouble() * 2.2f) {
    var hit = Physics2D.Raycast(new Vector2(x, y0 + 6f), Vector2.down, 30f, layer); if (hit.collider == null) continue;
    double roll = rng.NextDouble();
    string n = roll < 0.55 ? (roll < 0.2 ? "GrassLight" : roll < 0.38 ? "Grass1" : "Grass2") : roll < 0.7 ? (rng.NextDouble() < 0.5 ? "MushroomRed" : "MushroomBrown") : roll < 0.9 ? (rng.NextDouble() < 0.5 ? "FlowerWhite" : "FlowerPink") : "OddTree";
    var go = new GameObject(n); go.transform.SetParent(root.transform, false); go.transform.position = new Vector3(x, hit.point.y - 0.02f, 0f);
    var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprites[n]; sr.sortingOrder = n == "OddTree" ? -3 : (n.StartsWith("Grass") && rng.NextDouble() < 0.4 ? 6 : -2);
    if (rng.NextDouble() < 0.5) sr.flipX = true; placed++;
  }
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return "placed " + placed;
