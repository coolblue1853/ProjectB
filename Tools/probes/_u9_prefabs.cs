string s = ""; int n = 0, bad = 0;
foreach (var g in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab" }))
{
    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
    var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
    var e = go != null ? go.GetComponent<EnemyHealth>() : null;
    if (e == null) { continue; }
    n++;
    if (e.hpBar != null && (e.hpObject == null || e.hpCanvas == null))
    { bad++; s += path + " hpObject=" + (e.hpObject != null) + " hpCanvas=" + (e.hpCanvas != null) + " boss=" + e.isBossMob + "\n"; }
}
return "prefabs=" + n + " bad=" + bad + "\n" + s;
