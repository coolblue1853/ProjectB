// 수량 창·버리기·나누기 키 경로 확인 → Logs/playtest/qty.txt
var pc = PlayerController.instance; if (pc == null) return "no player";
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
if (!GameObject.Find("Map").transform.Find("Town").gameObject.activeInHierarchy) MapInfo.GoToMap("Town", pc.gameObject);
var inv = InventoryManager.instance; var kb = UnityEngine.InputSystem.Keyboard.current;
var sb = new System.Text.StringBuilder();
System.Action<string> log = s => sb.AppendLine(s);
var steps = new System.Collections.Generic.List<System.Func<string>>();
System.Func<UnityEngine.InputSystem.Key, System.Func<string>> tap = k => () => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(k)); return null; };
System.Func<string> rel = () => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); return null; };
System.Func<string, System.Func<string>> chk = name => () => name + ": open=" + QuantityPopup.IsOpen + " val=" + QuantityPopup.CurrentValue + " state=" + inv.state + " invOpen=" + inv.inventory.activeSelf + " rocks=" + inv.CountItem("Rock");
steps.Add(() => { while (inv.CountItem("Rock") > 0) { inv.Service.Remove("Rock", inv.CountItem("Rock")); } for (int i = 0; i < 30; i++) inv.CreatItem("Rock"); inv.inventory.SetActive(true); UiLock.Set("InvGroup", true); inv.cusor.SetActive(true); inv.state = ""; { var m0 = inv.Service.ScanModel(); int per = inv.maxHor * inv.maxVer; for (int q = 0; q < m0.Capacity; q++) { if (!m0[q].IsEmpty && m0[q].Id == "Rock") { inv.nowBox = q / per; inv.cusorCount[inv.nowBox] = q % per; break; } } } return "setup rocks=" + inv.CountItem("Rock") + " cursorItem=" + (inv.CursorItem() != null ? inv.CursorItem().name + "x" + inv.CursorItem().nowStack : "null"); });
System.Action<UnityEngine.InputSystem.Key> T = k => { steps.Add(tap(k)); steps.Add(null); steps.Add(rel); steps.Add(null); };
steps.Add(null);
T(UnityEngine.InputSystem.Key.Delete); steps.Add(chk("Delete-open"));
T(UnityEngine.InputSystem.Key.Digit5); steps.Add(chk("digit5"));
T(UnityEngine.InputSystem.Key.UpArrow); steps.Add(chk("up(+1)"));
T(UnityEngine.InputSystem.Key.RightArrow); steps.Add(chk("right(+10)"));
T(UnityEngine.InputSystem.Key.LeftArrow); steps.Add(chk("left(-10)"));
T(UnityEngine.InputSystem.Key.DownArrow); steps.Add(chk("down(-1)"));
T(UnityEngine.InputSystem.Key.C); steps.Add(chk("C(max)"));
T(UnityEngine.InputSystem.Key.Backspace); steps.Add(chk("backspace"));
T(UnityEngine.InputSystem.Key.Escape); steps.Add(chk("Esc(cancel)"));
T(UnityEngine.InputSystem.Key.Delete); T(UnityEngine.InputSystem.Key.Digit7); T(UnityEngine.InputSystem.Key.Enter); steps.Add(chk("drop7-confirm"));
T(UnityEngine.InputSystem.Key.C); steps.Add(chk("divide-open"));
T(UnityEngine.InputSystem.Key.Enter); steps.Add(chk("divide-confirm"));
steps.Add(() => { int stacks = 0; var m = inv.Service.ScanModel(); for (int q = 0; q < m.Capacity; q++) { var s = m[q]; if (!s.IsEmpty && s.Id == "Rock") stacks++; } return "rock stacks=" + stacks; });
int idx = 0; double next = UnityEditor.EditorApplication.timeSinceStartup + 3;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < next) return;
    next = now + 0.12;
    if (idx >= steps.Count) { System.IO.File.WriteAllText("Logs/playtest/qty.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
    var st = steps[idx++]; if (st == null) return;
    string r = null; try { r = st(); } catch (System.Exception e) { r = "EX " + e.Message; }
    if (r != null) log(r);
};
UnityEditor.EditorApplication.update += upd; return "started " + steps.Count;
