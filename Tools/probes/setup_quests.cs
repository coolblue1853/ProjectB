// G11 씬 구성 (에디트 모드): Quest Machine 프리팹 + 플레이어 QuestJournal + 마을 퀘스트 게시판. 재실행 안전
var sb = new System.Text.StringBuilder();
// 스프라이트 임포트 설정 (24 PPU, 바닥 중앙 피벗, 포인트)
foreach (var n in new[]{ "QuestBoard", "QuestMark" }) {
  var path = "Assets/Art/NPC/" + n + ".png";
  var imp = (TextureImporter)AssetImporter.GetAtPath(path);
  imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single; imp.filterMode = FilterMode.Point;
  imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.spritePixelsPerUnit = 24;
  var st = new TextureImporterSettings(); imp.ReadTextureSettings(st); st.spriteAlignment = (int)SpriteAlignment.BottomCenter; imp.SetTextureSettings(st);
  imp.SaveAndReimport();
}
var qmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Plugins/Pixel Crushers/Quest Machine/Prefabs/Quest Machine.prefab");
if (UnityEngine.Object.FindFirstObjectByType<PixelCrushers.QuestMachine.QuestMachineConfiguration>(FindObjectsInactive.Include) == null) {
  var qm = (GameObject)PrefabUtility.InstantiatePrefab(qmPrefab); sb.Append("QuestMachine 추가; ");
}
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
if (pc.GetComponent<PixelCrushers.QuestMachine.QuestJournal>() == null) { pc.gameObject.AddComponent<PixelCrushers.QuestMachine.QuestJournal>(); sb.Append("QuestJournal 추가; "); }
var town = GameObject.Find("Map").transform.Find("Town");
var board = town.Find("QuestBoard");
if (board == null) {
  var go = new GameObject("QuestBoard"); go.transform.SetParent(town, false);
  var smith = town.Find("EquipSmith");
  go.transform.position = new Vector3(-393.5f, smith.position.y - 0.9f, 0f);
  var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NPC/QuestBoard.png");
  var smithSr = smith.GetComponentInChildren<SpriteRenderer>(); if (smithSr != null) { sr.sortingLayerID = smithSr.sortingLayerID; sr.sortingOrder = smithSr.sortingOrder; sr.sharedMaterial = smithSr.sharedMaterial; }
  var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(2.6f, 2.4f); col.offset = new Vector2(0f, 0.9f);
  var giver = go.AddComponent<PixelCrushers.QuestMachine.QuestGiver>();
  giver.id = new PixelCrushers.StringField("QuestBoard"); giver.displayName = new PixelCrushers.StringField("Quest Board");
  var markGo = new GameObject("QuestMark"); markGo.transform.SetParent(go.transform, false); markGo.transform.localPosition = new Vector3(0f, 1.95f, 0f);
  var msr = markGo.AddComponent<SpriteRenderer>(); msr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NPC/QuestMark.png"); msr.sortingLayerID = sr.sortingLayerID; msr.sortingOrder = sr.sortingOrder + 1;
  var qbComp = go.AddComponent<QuestBoard>();
  var so = new SerializedObject(qbComp); so.FindProperty("_questMark").objectReferenceValue = msr; so.ApplyModifiedPropertiesWithoutUndo();
  sb.Append("QuestBoard 추가 @" + go.transform.position + "; ");
}
var giverC = town.Find("QuestBoard").GetComponent<PixelCrushers.QuestMachine.QuestGiver>();
giverC.image = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NPC/QuestBoard.png"); EditorUtility.SetDirty(giverC); // 대화창 초상
var es = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include);
sb.Append("EventSystem=" + (es != null ? es.name + "/" + es.currentInputModule : "없음") + "; ");
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(town.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(town.gameObject.scene);
return sb.ToString();
