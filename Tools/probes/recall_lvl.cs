var best = EnemyHealth.Active; 
string r = "";
EnemyHealth e = null;
foreach (var x in EnemyHealth.Active) { if (x != null && x.IsMonster && x.Level > 0 && !x.isBossMob) { e = x; break; } }
if (e == null) return "no enemy";
PlayerController.instance.transform.position = e.transform.position + new Vector3(-2f, 0.5f, 0f);
return e.name + " lv=" + e.Level;
