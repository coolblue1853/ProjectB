// PV5: 포털 도착 무적 2초 만료 + 초반 3맵 도착 지점 몹 밀집 → Logs/pv5/arrive.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder();
var visited = new System.Collections.Generic.HashSet<string>(); int hop = 0; int st = 0; double until = 0; double t0 = 0; string cur = "Town"; bool[] sampled = new bool[3]; Portal pick = null;
System.Action<string> snap = tag => {
  Vector2 p = pc.transform.position; int tot=0, w14=0, inz=0; float minD=999;
  foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { tot++; float d=Vector2.Distance(e.transform.position,p); if(d<minD)minD=d; if(d<14)w14++; if(ArrivalSafeZone.IsInside(e.transform.position))inz++; }
  int cand=0, candIn=0;
  foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { var mi = s.GetComponentInParent<MapInfo>(); if (mi==null) continue; var pts = SpawnPointTable.Get(mi.name, s.name); if (pts==null) continue; foreach (var q in pts) { cand++; if (ArrivalSafeZone.IsInside(q)) candIn++; } }
  sb.AppendLine("  [" + tag + "] inv=" + PlayerStatus.Has(StatusFlag.Invincible) + " enemies total=" + tot + " within14=" + w14 + " inZone=" + inz + " nearest=" + (minD>900?"-":minD.ToString("F1")) + " spawnPts=" + cand + " ptsInZone(차단 대상)=" + candIn + " zones=" + ArrivalSafeZone.Points.Count);
};
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
  double now = UnityEditor.EditorApplication.timeSinceStartup;
  if (st == 0) { string[] tg = { "Lv1_NearForest_SideRoad", "Lv1_NearForest", "Lv1_NormalForest" };
    if (hop >= 3) { System.IO.File.WriteAllText("Logs/pv5/arrive.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
    pick = null; foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (p.openMap != null && p.closeMap != null && p.openMap.name == tg[hop]) { pick = p; break; }
    if (pick == null) { sb.AppendLine("no portal to " + tg[hop]); hop++; return; }
    MapInfo.GoToMap(pick.closeMap.name, pc.gameObject); until = now + 4; st = 2; return; }
  if (st == 2) { if (now < until) return; sb.AppendLine("hop" + hop + " " + pick.closeMap.name + " -> " + pick.openMap.name); bool ok = pick.Use(pc.gameObject); sb.AppendLine("  use=" + ok); t0 = now; sampled[0]=sampled[1]=sampled[2]=false; st = 1; return; }
  if (st == 1) { double d = now - t0;
    if (!sampled[0] && d >= 1.0) { sampled[0]=true; snap("t+1.0s"); }
    if (!sampled[1] && d >= 1.8) { sampled[1]=true; snap("t+1.8s"); }
    if (!sampled[2] && d >= 3.0) { sampled[2]=true; snap("t+3.0s"); hop++; st = 0; } }
};
UnityEditor.EditorApplication.update += upd; return "started";
