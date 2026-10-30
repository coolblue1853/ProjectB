// excavation_setup/excavation_drive 기록 읽기 + 지금 상태(진행·잠금·프롬프트·지점 쿨타임)
var sb = new System.Text.StringBuilder();
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[ExcProbe]") || g.name.StartsWith("[ExcSetup]")) sb.Append(g.name + "\n");
var node = ExcavationSite.Current;
var s = node != null ? node.Session : null;
sb.Append("now: " + (s == null ? "idle" : "dig cursor=" + s.Cursor + " done=" + s.Done + " " + s.Result + " t=" + s.Elapsed.ToString("F1")));
sb.Append(" lock=" + UiLock.IsHeld(ExcavationSite.Lock) + " isOpenUI=" + DatabaseManager.isOpenUI);
foreach (var n in UnityEngine.Object.FindObjectsByType<ExcavationSite>(FindObjectsSortMode.None)) { var pr = n.transform.Find("SpotPrompt"); sb.Append("\n " + n.transform.parent.name + "/" + n.name + "@" + n.transform.position.ToString("F1") + " ready=" + n.IsReady + " prompt=" + (pr != null && pr.gameObject.activeSelf)); }
var pc = PlayerController.instance;
if (pc != null) sb.Append("\nplayer " + pc.transform.position.ToString("F1"));
return sb.ToString();
