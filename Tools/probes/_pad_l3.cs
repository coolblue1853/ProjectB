// AUTO-126: 109 _l3InUi 가상 패드 확인(플레이 모드). L3 누름/UI 열림(InputLock 토큰)/LT 취소 시나리오 + CountdownTimer.Tick 음수 dt. 결과 Logs/pad_l3/result.txt
var sb = new System.Text.StringBuilder();
var oldPad = UnityEngine.InputSystem.Gamepad.current; bool created = oldPad == null;
var pad = created ? UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>() : oldPad; pad.MakeCurrent();
System.IDisposable lockTok = null; int tapSeen = 0, holdSeen = 0; int lastFrame = -1;
System.Func<bool, bool, UnityEngine.InputSystem.LowLevel.GamepadState> mk = (l3, lt) => { var s = new UnityEngine.InputSystem.LowLevel.GamepadState(); if (l3) s = s.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.LeftStick); if (lt) s.leftTrigger = 1f; return s; };
var steps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, System.Action>>();
System.Action<int, System.Action> add = (n, a) => steps.Add(new System.Collections.Generic.KeyValuePair<int, System.Action>(n, a));
System.Action<string> rec = (lbl) => sb.AppendLine(lbl + " L3InUi=" + PadFieldInput.L3InUi + " uiOpen=" + DatabaseManager.isOpenUI + " tapSeen=" + tapSeen + " holdSeen=" + holdSeen);
add(5, () => { tapSeen = 0; holdSeen = 0; UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(true, false)); });
add(6, () => rec("S1 UI닫힘 L3누름중"));
add(1, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(false, false)));
add(5, () => rec("S1 뗌 후(Tap 기대 1)"));
add(1, () => { lockTok = PlayerStatus.Acquire(StatusFlag.InputLock, "probe_l3"); tapSeen = 0; holdSeen = 0; });
add(3, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(true, false)));
add(6, () => rec("S2 UI열림 L3누름중(InUi true 기대)"));
add(1, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(false, false)));
add(5, () => { rec("S2 뗌 후"); tapSeen = 0; holdSeen = 0; });
add(1, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(true, false)));
add(4, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(true, true)));
add(6, () => rec("S3 L3 누른 채 LT 더함(InUi false·취소 기대)"));
add(1, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(false, false)));
add(5, () => { rec("S3 뗌 후(Tap 0 기대)"); if (lockTok != null) { lockTok.Dispose(); lockTok = null; } tapSeen = 0; holdSeen = 0; });
add(3, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(true, false)));
add(6, () => rec("S4 UI닫힘 다음 L3 누름중(InUi false 기대)"));
add(1, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, mk(false, false)));
add(5, () => { rec("S4 뗌 후(Tap 1 기대)");
  var ct = new CountdownTimer(); ct.Start(1f); ct.Tick(-5f); float r1 = ct.Remaining; ct.Tick(0.4f); sb.AppendLine("CountdownTimer Tick(-5) remaining=" + r1 + " (1.0 기대) Tick(0.4)=" + ct.Remaining + " (0.6 기대)");
  var drv = GameObject.Find("PadFieldInputDriver"); sb.AppendLine("drivers=" + UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).Count(m => m.GetType().Name == "Driver" && m.gameObject.name == "PadFieldInputDriver")); });
int si = 0, wait = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
  if (Time.frameCount == lastFrame) return; lastFrame = Time.frameCount;
  if (PadFieldInput.L3 == PadGesture.Tap) tapSeen++; else if (PadFieldInput.L3 == PadGesture.Hold) holdSeen++;
  if (si < steps.Count) { if (wait > 0) { wait--; return; } var st = steps[si++]; st.Value(); wait = si < steps.Count ? steps[si].Key : 0; return; }
  UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); if (lockTok != null) lockTok.Dispose(); if (created) UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);
  System.IO.Directory.CreateDirectory("Logs/pad_l3"); System.IO.File.WriteAllText("Logs/pad_l3/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); if (lockTok != null) lockTok.Dispose(); if (created) UnityEngine.InputSystem.InputSystem.RemoveDevice(pad); System.IO.Directory.CreateDirectory("Logs/pad_l3"); System.IO.File.WriteAllText("Logs/pad_l3/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
