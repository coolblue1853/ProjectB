// 지도 가장자리에서 걸어 떨어져 낙사 복구되는지 → Logs/playtest/f1nav3/fall.txt
var kb = UnityEngine.InputSystem.Keyboard.current;
var pc = PlayerController.instance;
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
// map, startX, key, label
var cases = new object[][] {
 new object[]{"Lv1_Swamp", -416f, UnityEngine.InputSystem.Key.LeftArrow, "swamp-L"},
 new object[]{"Lv1_Swamp", -186f, UnityEngine.InputSystem.Key.RightArrow, "swamp-R"},
 new object[]{"Lv1_BanditFortress", -425f, UnityEngine.InputSystem.Key.LeftArrow, "fort-L"},
 new object[]{"Lv1_BanditFortress", -145f, UnityEngine.InputSystem.Key.RightArrow, "fort-R"},
 new object[]{"Lv1_DeepForest", -430f, UnityEngine.InputSystem.Key.LeftArrow, "deep-L"},
 new object[]{"Lv1_TowerOutskirts", -430f, UnityEngine.InputSystem.Key.LeftArrow, "tower-L"},
};
var sb = new System.Text.StringBuilder();
int ci = -1, st = 0; double until = 0, t0 = 0; float minY = 999, startY = 0, startX = 0; bool resc = false;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
    if (st == 0) { ci++; if (ci >= cases.Length) { System.IO.File.WriteAllText("Logs/playtest/f1nav3/fall.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
        MapInfo.GoToMap((string)cases[ci][0], pc.gameObject); PlayerStatus.AcquireFor(StatusFlag.Invincible, 120f, "nav3probe"); st = 1; until = now + 3.5; return; }
    if (st == 1) { pc.transform.position = new Vector3((float)cases[ci][1], pc.transform.position.y, 158.5f); startY = pc.transform.position.y; startX = (float)cases[ci][1]; minY = 999; resc = false; st = 2; until = now + 1.5; return; }
    if (st == 2) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState((UnityEngine.InputSystem.Key)cases[ci][2])); st = 3; t0 = now; until = now + 0.1; return; }
    if (st == 3) {
        float y = pc.transform.position.y; minY = Mathf.Min(minY, y);
        if (y > startY - 1f && minY < startY - 14f) resc = true;
        if (now - t0 > 20 || resc) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
            sb.AppendLine((string)cases[ci][3] + " rescued=" + resc + " t=" + (now - t0).ToString("F1") + " start=(" + startX + "," + startY.ToString("F1") + ") now=" + pc.transform.position.ToString("F1") + " minY=" + minY.ToString("F1"));
            st = 0; until = now + 1.0; return; }
        until = now + 0.5;
    }
};
UnityEditor.EditorApplication.update += upd;
return "started";
