// W1 마을 NPC 3명 (SPUM 유닛 + TalkNpc). 에디트 모드, 재실행 시 다시 만든다. 외형: 기본 유닛(말 없는 0번)에 머리·수염·옷을 바꿔 끼우고 투구는 뗀다
var sb = new System.Text.StringBuilder();
var town = GameObject.Find("Map").transform.Find("Town");
var board = town.Find("QuestBoard");
var bsr = board.GetComponent<SpriteRenderer>();
string[] prefabs = { "Assets/SPUM/Resources/Addons/Legacy/2_Prefab/SPUM_20250915183854408.prefab", "Assets/SPUM/Resources/Addons/Legacy/2_Prefab/SPUM_20250915184739548.prefab" };
string SP = "Assets/SPUM/Resources/Addons/Legacy/0_Unit/0_Sprite/";
// name, conv, x, hair, beard(-), cloth, weapon(1/0)
var defs = new string[][]
{
    new[]{ "NPC_Elder", StoryDialogues.Elder, "-396.6", "Hair_6", "FaceHair_3", "Cloth_7", "0", "D8D8E0" },
    new[]{ "NPC_Smith", StoryDialogues.Smith, "-401.2", "Hair_2", "FaceHair_1", "Cloth_3", "1", "6B3E22" },
    new[]{ "NPC_Merchant", StoryDialogues.Merchant, "-409.0", "Hair_8", "-", "Cloth_10", "0", "9C3B2A" },
};
System.Func<string, string, Sprite> Spr = (file, sub) => { foreach (var o in AssetDatabase.LoadAllAssetsAtPath(SP + file + ".png")) if (o is Sprite && (sub == null || o.name == sub)) return (Sprite)o; return null; };
foreach (var row in defs)
{
    string dName = row[0], dConv = row[1], dHair = row[3], dBeard = row[4] == "-" ? null : row[4], dCloth = row[5]; float dX = float.Parse(row[2], System.Globalization.CultureInfo.InvariantCulture); bool dWeapon = row[6] == "1"; Color hairCol; ColorUtility.TryParseHtmlString("#" + row[7], out hairCol);
    var old = town.Find(dName);
    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    Transform t = null;
    if (t == null)
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[0]);
        var unit = (GameObject)PrefabUtility.InstantiatePrefab(src);
        var go = new GameObject(dName);
        go.transform.SetParent(town, false);
        unit.transform.SetParent(go.transform, false);
        unit.transform.localPosition = Vector3.zero;
        var sg = go.AddComponent<UnityEngine.Rendering.SortingGroup>();
        sg.sortingLayerID = bsr.sortingLayerID; sg.sortingOrder = bsr.sortingOrder;
        foreach (var sr in unit.GetComponentsInChildren<SpriteRenderer>(true))
        {
            string slot = sr.name.Trim();
            if (slot.Contains("Helmet")) sr.sprite = null;
            else if (slot == "7_Hair") { sr.sprite = Spr("0_Hair/" + dHair, null); sr.color = hairCol; }
            else if (slot == "6_FaceHair") { sr.sprite = dBeard != null ? Spr("1_FaceHair/" + dBeard, null) : null; sr.color = hairCol; }
            else if (slot == "ClothBody") sr.sprite = Spr("2_Cloth/" + dCloth, "Body");
            else if (slot == "21_LCArm") sr.sprite = Spr("2_Cloth/" + dCloth, "Left");
            else if (slot == "-19_RCArm") sr.sprite = Spr("2_Cloth/" + dCloth, "Right");
            else if (slot.EndsWith("_Weapon") && !dWeapon) sr.sprite = null;
        }
        var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(2.2f, 2f); col.offset = new Vector2(0f, 0.8f);
        go.AddComponent<TalkNpc>();
        t = go.transform;
        sb.Append("created " + dName + "; ");
    }
    t.position = new Vector3(dX, board.position.y, 0f);
    t.localScale = new Vector3(1.4f, 1.4f, 1f);
    var npc = t.GetComponent<TalkNpc>();
    var so = new SerializedObject(npc); so.FindProperty("_conversation").stringValue = dConv; so.FindProperty("_promptHeight").floatValue = 1.5f; so.ApplyModifiedPropertiesWithoutUndo();
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(town.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(town.gameObject.scene);
return sb.ToString() + "saved";
