// 마을 대화 NPC (W1): 장로 1명 — 기존 마을 NPC 도트 양식(gen_elder.py, PPU 100, 8배 픽셀). 에디트 모드, 재실행 안전.
// SPUM 은 사용자 지시(2026-10-19 "너무 안 어울린다")로 쓰지 않는다 → 예전 SPUM NPC 는 지운다.
// 대장장이·상인 대사는 기존 상점 NPC 첫 방문 인사로 붙일 예정(미구현).
var sb = new System.Text.StringBuilder();
var town = GameObject.Find("Map").transform.Find("Town");
foreach (var n in new[]{ "NPC_Elder", "NPC_Smith", "NPC_Merchant" })
{
    var old = town.Find(n);
    if (old != null) { UnityEngine.Object.DestroyImmediate(old.gameObject); sb.Append("removed " + n + "; "); }
}
string path = "Assets/Art/NPC/Elder.png";
var imp = (TextureImporter)AssetImporter.GetAtPath(path);
imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple;
imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.spritePixelsPerUnit = 100;
#pragma warning disable 618
var metas = new SpriteMetaData[2];
for (int i = 0; i < 2; i++) { metas[i].name = "Elder_" + i; metas[i].rect = new Rect(i * 192, 0, 192, 256); metas[i].alignment = (int)SpriteAlignment.BottomCenter; }
imp.spritesheet = metas;
#pragma warning restore 618
imp.SaveAndReimport();
var frames = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Sprite) frames.Add((Sprite)o);
frames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

var smith = town.Find("EquipSmith").GetComponentInChildren<SpriteRenderer>();
var go = new GameObject("NPC_Elder");
go.transform.SetParent(town, false);
go.transform.position = new Vector3(-396.6f, town.Find("QuestBoard").position.y, 0f);
var ls = smith.transform.lossyScale; go.transform.localScale = new Vector3(Mathf.Abs(ls.x), Mathf.Abs(ls.y), 1f);
var sr = go.AddComponent<SpriteRenderer>();
sr.sprite = frames[0]; sr.sortingLayerID = smith.sortingLayerID; sr.sortingOrder = smith.sortingOrder; sr.sharedMaterial = smith.sharedMaterial;
var fb = go.AddComponent<SpriteFlipbook>(); fb.Frames = frames.ToArray();
var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true;
col.size = new Vector2(2.2f / go.transform.localScale.x, 2f / go.transform.localScale.y); col.offset = new Vector2(0f, 1f / go.transform.localScale.y);
var npc = go.AddComponent<TalkNpc>();
var so = new SerializedObject(npc);
so.FindProperty("_conversation").stringValue = StoryDialogues.Elder;
so.FindProperty("_promptHeight").floatValue = 3.0f / go.transform.localScale.y;
so.FindProperty("_portrait").objectReferenceValue = frames[0];
so.FindProperty("_actorName").stringValue = "Village Elder";
so.ApplyModifiedPropertiesWithoutUndo();
sb.Append("elder @" + go.transform.position.ToString("F1") + " scale=" + go.transform.localScale.x + "; ");
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(town.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(town.gameObject.scene);
return sb.ToString();
