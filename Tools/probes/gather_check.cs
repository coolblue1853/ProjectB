// gather_setup/gather_drive 기록 읽기 + 지금 상태(진행·잠금·프롬프트·지점 쿨타임)
var sb = new System.Text.StringBuilder();
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[GatherProbe]") || g.name.StartsWith("[GatherSetup]")) sb.Append(g.name + "\n");
var node = GatherNode.Current;
var s = node != null ? node.Session : null;
sb.Append("now: " + (s == null ? "idle" : s.Kind + " done=" + s.Done + " " + s.Result + " t=" + s.Elapsed.ToString("F1")));
sb.Append(" lock=" + UiLock.IsHeld(GatherNode.LockOwner) + " isOpenUI=" + DatabaseManager.isOpenUI);
foreach (var n in UnityEngine.Object.FindObjectsByType<GatherNode>(FindObjectsSortMode.None)) { var pr = n.transform.Find("GatherPrompt"); sb.Append("\n " + n.transform.parent.name + "/" + n.name + "@" + n.transform.position.ToString("F1") + " ready=" + n.Ready + " prompt=" + (pr != null && pr.gameObject.activeSelf)); }
var pc = PlayerController.instance;
if (pc != null) sb.Append("\nplayer " + pc.transform.position.ToString("F1"));
return sb.ToString();
