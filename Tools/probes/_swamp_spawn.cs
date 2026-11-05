// AUTO-119: 늪 스포너 진단. 플레이 모드 중 실행. 토큰 __MAP__ 치환(Tools/swamp_spawn.sh 아님, sed 직접). PH=goto|dump
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
string PH = "__PH__";
if (PH == "goto") { return "goto " + MapInfo.GoToMap("__MAP__", pc.gameObject) + " pos " + pc.transform.position + " day=" + TimeChange.instance.isDaytime; }
var sb = new System.Text.StringBuilder();
sb.AppendLine("MAP __MAP__ day=" + TimeChange.instance.isDaytime + " pc=" + pc.transform.position);
var sps = UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include);
foreach (var s in sps)
{
    if (!s.gameObject.activeInHierarchy) continue;
    var col = s.GetComponent<Collider2D>();
    var f = typeof(EnemySpawner);
    var spawning = (bool)f.GetField("isSpawning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
    int alive = 0; if (s.enemySlot != null) foreach (var g in s.enemySlot) if (g != null && g.activeSelf) alive++;
    int pts = s.enemyPositionArray != null ? s.enemyPositionArray.Length : -1;
    var prefabs = (System.Collections.IDictionary)f.GetField("prefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
    string enemyPf = prefabs != null && prefabs.Contains("enemy") ? (prefabs["enemy"] != null ? ((GameObject)prefabs["enemy"]).name : "NULLPREFAB") : "nokey";
    string pos0 = pts > 0 && s.enemyPositionArray[0] != null ? s.enemyPositionArray[0].transform.position.ToString("F1") : "-";
    string gnd = "-";
    if (pts > 0 && s.enemyPositionArray[0] != null) { var p = s.enemyPositionArray[0].transform.position; var h = Physics2D.Raycast(p + Vector3.up * 2f, Vector2.down, 30f, LayerMask.GetMask("Ground")); gnd = h.collider != null ? "ground@" + h.point.y.ToString("F1") : "NOGROUND"; }
    sb.AppendLine(s.name + " active=" + s.gameObject.activeInHierarchy + " enabled=" + s.enabled + " daySp=" + s.isDaytimeSpowner + " inPlayer=" + s.isInPlayer + " ready=" + s.isReady + " spawning=" + spawning
        + " cnt=" + s.enemyCount + " cyc=" + s.cycleFloat + " alive=" + alive + " pts=" + pts + " pf=" + enemyPf + " pos0=" + pos0 + " " + gnd
        + " col=" + (col != null ? col.bounds.center.ToString("F0") + "/" + col.bounds.size.ToString("F0") + (col.enabled ? "" : " COLDISABLED") : "NOCOL"));
}
sb.AppendLine("EnemyHealth.Active=" + EnemyHealth.Active.Count);
return sb.ToString();
