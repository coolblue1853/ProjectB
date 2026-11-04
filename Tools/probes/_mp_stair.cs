// 계단 점프: 탭(1틱 C) vs 홀드(0.35초 C) 로 SideRoad 계단을 걸어 올라가는 진행량 비교 → Logs/playtest/mapphys/stair.txt
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
var kb = UnityEngine.InputSystem.Keyboard.current;
var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder();
int mode = -1; int state = 0; double until = 0; float segStart = 0; float nextJ = 0; float holdUntil = 0; float nextTick = 0; float stallX = 0, stallT = 0;
System.Action<System.Collections.Generic.List<UnityEngine.InputSystem.Key>> push = k => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(k.ToArray()));
UnityEditor.EditorApplication.CallbackFunction upd = null;
Time.timeScale = 3f;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup;
    PlayerStatus.AcquireFor(StatusFlag.Invincible, 30f, "mp"); PlayerHealthManager.Instance.HpUp(PlayerHealthManager.Instance.fullHP);
    if (now < until) return;
    if (state == 0)
    {
        mode++; push(new System.Collections.Generic.List<UnityEngine.InputSystem.Key>());
        if (mode >= 2) { Time.timeScale = 1f; System.IO.File.WriteAllText("Logs/playtest/mapphys/stair.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
        MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
        until = now + 3.0; state = 1; return;
    }
    if (state == 1)
    {
        pc.transform.position = new Vector3(-745f, 0f, 0f);
        until = now + 1.0; state = 2; segStart = Time.time; nextJ = 0; holdUntil = 0; nextTick = 0; stallX = pc.transform.position.x; stallT = Time.time;
        sb.AppendLine("== mode " + (mode == 0 ? "TAP" : "HOLD") + " start " + pc.transform.position); return;
    }
    if (state == 2)
    {
        if (Time.time < nextTick) return; nextTick = Time.time + 0.1f;
        var k = new System.Collections.Generic.List<UnityEngine.InputSystem.Key>(); k.Add(UnityEngine.InputSystem.Key.RightArrow);
        bool press = false;
        if (Time.time >= nextJ) { press = true; nextJ = Time.time + 1.0f; if (mode == 1) holdUntil = Time.time + 0.35f; }
        if (press || (mode == 1 && Time.time < holdUntil)) k.Add(UnityEngine.InputSystem.Key.C);
        push(k);
        float t = Time.time - segStart;
        if (((int)(t * 10)) % 50 == 0 && Time.time - stallT > 0.4f) { sb.AppendLine("t=" + t.ToString("F1") + " pos " + pc.transform.position.ToString("F1")); stallT = Time.time; }
        if (t > 40f || pc.transform.position.x > -640f) { sb.AppendLine("end t=" + t.ToString("F1") + " pos " + pc.transform.position.ToString("F1")); state = 0; until = now + 0.5; }
    }
};
UnityEditor.EditorApplication.update += upd;
return "started";
