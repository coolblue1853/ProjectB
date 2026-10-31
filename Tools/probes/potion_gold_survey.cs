// 1층계 몹 기대 판매 골드(드롭 확률 x ItemSheet price) 조사 — 읽기 전용
var db = DatabaseManager.instance;
var seen = new System.Collections.Generic.HashSet<string>();
var sb = new System.Text.StringBuilder();
foreach (var dm in UnityEngine.Object.FindObjectsByType<DropManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    string n = dm.gameObject.name.Replace("(Clone)", "");
    if (!seen.Add(n)) continue;
    float ev = 0;
    foreach (var dr in dm.AllDrops())
    {
        var d = db != null ? db.LoadItemData(db.FindItemDataIndex(dr.name)) : null;
        if (d != null) ev += dr.chance * d.price;
    }
    sb.AppendLine(n + " ev=" + ev.ToString("F1"));
}
return sb.ToString();
