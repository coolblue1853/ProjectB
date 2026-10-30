// tracking_setup/tracking_drive/tracking_kill 기록 + 지금 상태(진행·잠금·지점 쿨타임·추적 정예)
var sb = new System.Text.StringBuilder();
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[TrkProbe]") || g.name.StartsWith("[TrkSetup]") || g.name.StartsWith("[TrkKill]")) sb.Append(g.name + "\n");
var node = TrackingSpot.Current;
var s = node != null ? node.Session : null;
sb.Append("now: " + (s == null ? "idle" : s.Phase + " stage=" + s.Stage + " mist=" + s.Mistakes + " " + s.Result));
sb.Append(" lock=" + UiLock.IsHeld(TrackingSpot.Lock) + " isOpenUI=" + DatabaseManager.isOpenUI);
foreach (var n in UnityEngine.Object.FindObjectsByType<TrackingSpot>(FindObjectsSortMode.None)) { var pr = n.transform.Find("SpotPrompt"); sb.Append("\n " + n.transform.parent.name + "/" + n.name + "@" + n.transform.position.ToString("F1") + " ready=" + n.IsReady + " prompt=" + (pr != null && pr.gameObject.activeSelf)); }
var t = EnemySpawner.TrackedTarget;
sb.Append("\ntracked=" + (t == null ? "-" : t.name + "@" + t.transform.position.ToString("F1") + " elite=" + t.IsElite + " hp=" + t.nowHP + " stun=" + t.isStun + " pool=" + t.IsInPool) + " has=" + EnemySpawner.HasTrackedTarget);
var pc = PlayerController.instance;
if (pc != null) sb.Append("\nplayer " + pc.transform.position.ToString("F1"));
return sb.ToString();
