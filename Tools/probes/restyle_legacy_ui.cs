// 기존 씬 UI(인벤토리·창고·상점·상세)의 기본 UISprite 패널 → Art/UI/Frame, 슬롯(Box/EquipBox) → Art/UI/Slot 으로 교체 (에디트 모드, 씬 저장). 재실행 안전
var frame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frame.png");
var slot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Slot.png");
int panels = 0, slots = 0;
foreach (var cn in new[]{ "InvenCanvus", "ChestUICanvus", "ShopCanvus", "DetailCanvus" }) {
  var c = GameObject.Find(cn); if (c == null) continue;
  foreach (var im in c.GetComponentsInChildren<UnityEngine.UI.Image>(true)) {
    if (im.sprite == null) continue;
    string n = im.sprite.name;
    if (n == "UISprite" && im.type == UnityEngine.UI.Image.Type.Sliced) {
      im.sprite = frame; im.pixelsPerUnitMultiplier = 0.5f; im.color = new Color(1f, 1f, 1f, Mathf.Max(0.9f, im.color.a)); EditorUtility.SetDirty(im); PrefabUtility.RecordPrefabInstancePropertyModifications(im); panels++;
    } else if (n == "Box" || n == "EquipBox") {
      im.sprite = slot; im.type = UnityEngine.UI.Image.Type.Sliced; im.pixelsPerUnitMultiplier = 0.5f; im.color = Color.white; EditorUtility.SetDirty(im); PrefabUtility.RecordPrefabInstancePropertyModifications(im); slots++;
    }
  }
}
var sc = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sc); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sc);
return "panels=" + panels + " slots=" + slots;
