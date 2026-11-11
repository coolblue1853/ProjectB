int act = EnemyHealth.Active.Count;
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
string s = "active=" + act + " pc=" + pc.transform.position;
int i = 0;
foreach (var e in EnemyHealth.Active) { if (i++ < 4) { s += " | " + e.name + " lv=" + e.Level + " " + e.transform.position + " bar=" + (e.hpBar != null); } }
return s;
