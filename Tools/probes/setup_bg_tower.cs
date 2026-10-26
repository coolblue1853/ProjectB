// 공용 원경 정리(2026-10-26): LV1_Field 원경 레이어를 점 필터로, 먼 탑(far-tower)을 원경이 있는 모든 맵의 Parrelax 에 추가.
// 탑은 far-clouds 와 같은 크기·틴트·세로 패럴랙스, 가로는 카메라에 고정(무한히 먼 랜드마크). near-clouds 뒤(order -20, z -0.1).
// 에디트 모드, 재실행 안전(기존 far-tower 를 지우고 다시 만든다).
if (EditorApplication.isPlaying) { return "BUSY: play mode"; }
var sb = new System.Text.StringBuilder();
string dir = "Assets/Sprite/LV1_Field/";
foreach (var f in new[]{"far-clouds","near-clouds","far-mountains","mountains","trees","far-tower"})
{
    var imp = (TextureImporter)AssetImporter.GetAtPath(dir + f + ".png");
    bool dirty = imp.filterMode != FilterMode.Point || imp.mipmapEnabled;
    if (f == "far-tower")
    {
        imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 100; imp.alphaIsTransparency = true; imp.textureCompression = TextureImporterCompression.Uncompressed; dirty = true;
    }
    imp.filterMode = FilterMode.Point; imp.mipmapEnabled = false;
    if (dirty) imp.SaveAndReimport();
}
var tower = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "far-tower.png");
var map = GameObject.Find("Map").transform;
foreach (Transform m in map)
{
    var p = m.Find("Parrelax"); if (p == null) continue;
    var clouds = p.Find("far-clouds"); if (clouds == null) continue;
    var old = p.Find("far-tower"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var src = clouds.GetComponent<ParrelaxBacground>();
    var csr = clouds.GetComponent<SpriteRenderer>();
    var go = new GameObject("far-tower"); go.transform.SetParent(p, false);
    go.transform.localScale = clouds.localScale;
    go.transform.position = new Vector3(src.originPoint.transform.position.x + 5.5f, clouds.position.y - 0.3f, -0.1f);
    var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = tower; sr.sharedMaterial = csr.sharedMaterial;
    sr.sortingLayerID = csr.sortingLayerID; sr.sortingOrder = -20; sr.color = csr.color;
    var pb = go.AddComponent<ParrelaxBacground>();
    var so = new SerializedObject(pb);
    var mul = new SerializedObject(src).FindProperty("parallaxEffectMultiplier").vector2Value;
    so.FindProperty("parallaxEffectMultiplier").vector2Value = new Vector2(1f, mul.y);
    so.FindProperty("originPoint").objectReferenceValue = src.originPoint;
    so.ApplyModifiedPropertiesWithoutUndo();
    sb.Append(m.name + " ");
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return "tower added: " + sb;
