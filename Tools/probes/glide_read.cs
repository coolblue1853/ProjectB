var sb = new System.Text.StringBuilder();
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[GlideRes]")) sb.Append(g.name + "\n");
var m = GlideMotor.Instance; var cs = UnityEngine.Object.FindFirstObjectByType<GlideCourse>();
sb.Append("motor unlocked=" + GlideMotor.Unlocked + " gliding=" + (m != null && m.Gliding) + " grounded=" + (m != null && m.Grounded) + " rem=" + (m != null ? m.Remaining.ToString("F1") : "-") + " max=" + (m != null ? m.MaxDuration.ToString("F1") : "-") + " deficit=" + AscentLoad.Deficit);
if (cs != null) sb.Append("\ncourse running=" + cs.Running + " t=" + cs.Elapsed.ToString("F1") + " falls=" + cs.Falls + " sent=" + cs.Sentences + " start=" + cs.StartPoint + " finish=" + cs.FinishPoint);
sb.Append("\nplayer " + PlayerController.instance.transform.position.ToString("F1"));
return sb.ToString();
