// U34 낙사 복귀 + U32 중재 확인 → Logs/playtest/u34.txt
var pc = PlayerController.instance;
if (pc == null) return "no player (play mode?)";
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
MapInfo.GoToMap("Town", pc.gameObject);
PlayerStatus.AcquireFor(StatusFlag.Invincible, 120f, "u34probe");
var sb = new System.Text.StringBuilder();
double until = UnityEditor.EditorApplication.timeSinceStartup + 3; int st = 0; Vector3 start = Vector3.zero; float bottom = 0; double t0 = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
    if (st == 0) { start = pc.transform.position; bottom = MapInfo.CurrentBottom; sb.AppendLine("start=" + start + " bottom=" + bottom + " ground=" + pc.isGround); st = 1; until = now + 1.5; return; }
    if (st == 1) { pc.transform.position = new Vector3(start.x, bottom - 10f, start.z); var rb = pc.GetComponent<Rigidbody2D>(); rb.position = pc.transform.position; sb.AppendLine("teleported y=" + (bottom - 10f)); st = 2; t0 = now; until = now + 0.5; return; }
    if (st == 2) { sb.AppendLine("after " + (now - t0).ToString("F1") + "s pos=" + pc.transform.position + " recovered=" + (pc.transform.position.y > bottom)); System.IO.File.WriteAllText("Logs/playtest/u34.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
};
UnityEditor.EditorApplication.update += upd;
return "started";
