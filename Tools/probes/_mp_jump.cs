// 제자리 점프 높이 측정: 풀홀드 / 탭(0.1초) / 탭(0.05초) → Logs/playtest/mapphys/jump.txt
var kb = UnityEngine.InputSystem.Keyboard.current; var pc = PlayerController.instance;
var rb = pc.GetComponent<Rigidbody2D>();
var sb = new System.Text.StringBuilder();
float[] holds = { 0.6f, 0.35f, 0.1f, 0.05f }; int i = -1; int st = 0; double until = 0; float baseY = 0, maxY = 0, t0 = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
System.Action<bool> c = on => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, on ? new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.C) : new UnityEngine.InputSystem.LowLevel.KeyboardState());
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup;
    if (st == 0) { if (now < until) return; i++; if (i >= holds.Length) { sb.AppendLine("jumpForce=" + pc.jumpForce + " gs=" + rb.gravityScale); System.IO.File.WriteAllText("Logs/playtest/mapphys/jump.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
        MapInfo.GoToMap("Lv1_NearForest", pc.gameObject); until = now + 3; st = 1; return; }
    if (st == 1) { if (now < until) return; pc.transform.position = new Vector3(-380f, 0f, 0f); until = now + 2; st = 2; return; }
    if (st == 2) { if (now < until) return; baseY = pc.transform.position.y; maxY = baseY; t0 = Time.time; c(true); st = 3; return; }
    if (st == 3) { maxY = Mathf.Max(maxY, pc.transform.position.y); if (Time.time - t0 >= holds[i]) { c(false); st = 4; } return; }
    if (st == 4) { maxY = Mathf.Max(maxY, pc.transform.position.y); if (Time.time - t0 > 1.5f) { sb.AppendLine("hold " + holds[i] + "s -> rise " + (maxY - baseY).ToString("F2")); st = 0; until = now + 0.5; } }
};
UnityEditor.EditorApplication.update += upd; return "started";
