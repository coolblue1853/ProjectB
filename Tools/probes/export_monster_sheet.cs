// Enemy_BD 프리팹의 EnemyHealth 값을 Resources/MonsterSheet.csv 로 내보낸다 (C5 1단계). 이미 있으면 새 프리팹 행만 추가
var path = "Assets/Resources/MonsterSheet.csv";
var rows = new System.Collections.Generic.List<string>();
var existing = new System.Collections.Generic.HashSet<string>();
if (System.IO.File.Exists(path)) { foreach (var l in System.IO.File.ReadAllLines(path)) { rows.Add(l); var c = l.Split(','); if (c.Length > 0) existing.Add(c[0]); } }
else rows.Add("id,displayName,floor,zone,maxHP,enemyDef,isBoss,superArmor,hpHeight,moveSpeed,dropRate,note");
int added = 0;
foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[]{ "Assets/Prefab/Enemy_BD" })) {
  var pf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
  var eh = pf.GetComponent<EnemyHealth>(); if (eh == null || existing.Contains(pf.name)) continue;
  var data = Resources.Load<BestiaryData>("UI/BestiaryData"); string disp = pf.name, zone = "";
  if (data != null) { var e = data.entries.Find(x => x.id == pf.name); if (e != null) { disp = e.displayName; zone = e.region; } }
  int floor = pf.name.StartsWith("Cliff") || pf.name.StartsWith("Spore") || pf.name.StartsWith("Gaze") || pf.name.StartsWith("Death") || pf.name.StartsWith("Abyss") ? 2 : 1;
  rows.Add(string.Join(",", new[]{ pf.name, disp, floor.ToString(), zone.Replace(",", " "), eh.maxHP.ToString(), eh.enemyDef.ToString(), eh.isBossMob ? "1" : "0", eh.isSuperArmor ? "1" : "0", eh.hpHeight.ToString("0.##"), "", "", "" }));
  added++;
}
System.IO.File.WriteAllLines(path, rows, new System.Text.UTF8Encoding(false));
AssetDatabase.ImportAsset(path);
return "rows=" + (rows.Count - 1) + " added=" + added;
