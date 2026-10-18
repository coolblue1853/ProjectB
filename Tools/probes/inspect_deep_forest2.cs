// 깊은 숲 2차 조사: 보스 스포너 objectInfos, ScareSide·LeafStaff 스킬 구성, 깊은 숲 스포너·드롭 이름
var sb = new System.Text.StringBuilder();
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
var map = GameObject.Find("Map").transform.Find("Lv1_DeepForest");
foreach (var s in map.GetComponentsInChildren<EnemySpawner>(true))
{
    sb.Append(s.name + " [");
    var arr = (System.Array)f.GetValue(s);
    var et = arr.GetType().GetElementType();
    foreach (var o in arr)
    {
        var pf = (GameObject)et.GetField("perfab").GetValue(o);
        sb.Append(et.GetField("objectName").GetValue(o) + "=" + (pf ? pf.name + "(" + AssetDatabase.GetAssetPath(pf) + ")" : "null") + " ");
    }
    var dm = s.GetComponent<DropManager>();
    if (dm != null)
    {
        var so = new SerializedObject(dm); var d = so.FindProperty("dropItems");
        sb.Append("] drops:");
        for (int i = 0; i < d.arraySize; i++) sb.Append(d.GetArrayElementAtIndex(i).FindPropertyRelative("itemName").stringValue + " ");
    }
    sb.AppendLine();
}
foreach (var wn in new[]{ "ScareSide", "LeafStaff", "ScareStaff" })
{
    var w = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Weapon/" + wn + ".prefab");
    sb.Append(wn + ": ");
    foreach (var s in w.GetComponentsInChildren<Skill>(true))
    {
        sb.Append(s.name + "/" + s.skillName + " cd" + s.SkillCoolTime + " pf[");
        if (s.skillprefab != null) foreach (var p in s.skillprefab) sb.Append(p ? p.name + "," : "null,");
        sb.Append("] ");
    }
    sb.AppendLine();
}
return sb.ToString();
