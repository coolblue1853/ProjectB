// AUTO-49 가상 패드 주입: L3 짧게/길게, SELECT 짧게(아티팩트)/길게(퀘스트), LT+SELECT(도감), LT+START(지도). 결과 Logs/pv49/pad.txt
var sb = new System.Text.StringBuilder(); var inv = InventoryManager.instance;
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; phm.fullHP = 9999; phm.nowHp = 100;
inv.CreatItem("Potion"); inv.CreatItem("Potion"); inv.CreatItem("Potion"); inv.CreatItem("Potion");
ItemCheck potion = null; foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "Potion") { potion = c; break; } }
QuickSlotBar.AssignFrom(0, potion);
ArtifactSystem.Unlock(ArtifactId.HealingDew); ArtifactSystem.Unlock(ArtifactId.GuardianShell);
var pad = UnityEngine.InputSystem.Gamepad.current; if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
var bestiary = UnityEngine.Object.FindFirstObjectByType<BestiaryWindow>(FindObjectsInactive.Include); var wp = UnityEngine.Object.FindFirstObjectByType<WaypointMap>(FindObjectsInactive.Include);
System.Func<object, string, bool> fld = (o, n) => o != null && (bool)o.GetType().GetField(n, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(o);
System.Func<string> snap = () => "potion=" + inv.CountItem("Potion") + " art=" + ArtifactSystem.Equipped + " quest=" + NpcQuestWindow.IsOpen + " bestiary=" + fld(bestiary, "open") + " map=" + fld(wp, "open") + " invOpen=" + inv.inventory.activeSelf + " ui=" + DatabaseManager.isOpenUI;
System.Action<UnityEngine.InputSystem.LowLevel.GamepadButton[], float> noop = null;
// 단계: (라벨, 눌릴 버튼들, 유지 초). 각 단계 뒤 1.5초 쉬고 스냅샷, 열린 창은 닫는다
var steps = new System.Collections.Generic.List<object[]> {
 new object[]{"L3 짧게", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.LeftStick}, 0.15f},
 new object[]{"L3 짧게(재)", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.LeftStick}, 0.15f},
 new object[]{"L3 길게", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.LeftStick}, 0.8f},
 new object[]{"SELECT 짧게", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.Select}, 0.15f},
 new object[]{"SELECT 길게", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.Select}, 1.1f},
 new object[]{"R3 (퀘스트 아님)", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.RightStick}, 0.15f},
 new object[]{"LT+SELECT", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.Select}, 0.2f},
 new object[]{"LT+START", new[]{UnityEngine.InputSystem.LowLevel.GamepadButton.Start}, 0.2f},
};
int idx = -1, phase = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.5; string before = "";
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
  if (phase == 0) { idx++; if (idx >= steps.Count) { sb.AppendLine("end " + snap()); System.IO.Directory.CreateDirectory("Logs/pv49"); System.IO.File.WriteAllText("Logs/pv49/pad.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); return; }
    before = snap(); var st = steps[idx]; var s = new UnityEngine.InputSystem.LowLevel.GamepadState();
    foreach (var b in (UnityEngine.InputSystem.LowLevel.GamepadButton[])st[1]) s = s.WithButton(b);
    string lb = (string)st[0]; if (lb.StartsWith("LT")) s.leftTrigger = 1f;
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, s); until = now + (float)st[2]; phase = 1; return; }
  if (phase == 1) { var st = steps[idx]; string lb = (string)st[0]; UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); until = now + 1.2; phase = 2; return; }
  if (phase == 2) { sb.AppendLine(steps[idx][0] + ": " + before + "  =>  " + snap());
    if (NpcQuestWindow.IsOpen) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.East)); }
    until = now + 0.4; phase = 3; return; }
  if (phase == 3) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); if (fld(bestiary, "open") || fld(wp, "open")) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.East)); } until = now + 0.5; phase = 4; return; }
  if (phase == 4) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); until = now + 0.5; phase = 0; return; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/pv49"); System.IO.File.WriteAllText("Logs/pv49/pad.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
