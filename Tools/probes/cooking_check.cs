// cooking_setup/cooking_drive 기록 읽기 + 지금 상태(진행·잠금·프롬프트·화로 쿨타임·요리 버프)
var sb = new System.Text.StringBuilder();
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[CookProbe]") || g.name.StartsWith("[CookSetup]")) sb.Append(g.name + "\n");
var node = CookingStove.Current;
var s = node != null ? node.Session : null;
sb.Append("now: " + (s == null ? "idle" : "phase=" + s.Phase + " grade=" + s.Grade + " heat=" + s.Heat.ToString("F0") + " round=" + s.Round + " t=" + s.Elapsed.ToString("F1")));
sb.Append(" lock=" + UiLock.IsHeld(CookingStove.Lock) + " isOpenUI=" + DatabaseManager.isOpenUI + " timeScale=" + UnityEngine.Time.timeScale);
foreach (var n in UnityEngine.Object.FindObjectsByType<CookingStove>(FindObjectsSortMode.None)) { var pr = n.transform.Find("SpotPrompt"); sb.Append("\n " + n.transform.parent.name + "/" + n.name + "@" + n.transform.position.ToString("F1") + " ready=" + n.IsReady + " prompt=" + (pr != null && pr.gameObject.activeSelf)); }
sb.Append("\nbuff=" + FoodBuff.Kind + " power=" + FoodBuff.Power + " remaining=" + FoodBuff.Remaining.ToString("F0") + " incDmg=" + DatabaseManager.incDmg + " deficit=" + AscentLoad.Deficit + " load=" + AscentLoad.CurrentLoad);
var pc = PlayerController.instance;
if (pc != null) sb.Append("\nplayer " + pc.transform.position.ToString("F1"));
return sb.ToString();
