// AUTO-117: 도감·퀘스트 창 패드 탐색 점검(플레이 모드). 템플릿 — __ACT__ 치환해 Tools/pad_nav.sh 가 실행.
// ACT: STATE(상태만) / OPENB(도감 토글) / OPENQ(퀘스트 토글) / South,North,West,East,DpadUp,DpadDown,DpadLeft,DpadRight,LeftShoulder,RightShoulder,Start,Select,StickRight,StickLeft,StickUp,StickDown
string act = "__ACT__";
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;
System.Func<System.Type, object, string, object> G = (t, o, n) => { var f = t.GetField(n, BF); return f != null ? f.GetValue(o) : "?"; };
var bt = typeof(BestiaryWindow); var qt = typeof(NpcQuestWindow);
var bi = bt.GetField("instance", BF).GetValue(null); var qi = qt.GetField("s_instance", BF).GetValue(null);
if (act == "OPENB") { BestiaryWindow.Toggle(); }
else if (act == "OPENQ") { NpcQuestWindow.Toggle(); }
else if (act != "STATE")
{
    var pad = UnityEngine.InputSystem.Gamepad.current; if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
    var st = new UnityEngine.InputSystem.LowLevel.GamepadState();
    if (act.StartsWith("Stick")) { var v = act == "StickRight" ? Vector2.right : act == "StickLeft" ? Vector2.left : act == "StickUp" ? Vector2.up : Vector2.down; st.leftStick = v; }
    else st = st.WithButton((UnityEngine.InputSystem.LowLevel.GamepadButton)System.Enum.Parse(typeof(UnityEngine.InputSystem.LowLevel.GamepadButton), act));
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, st);
    DG.Tweening.DOVirtual.DelayedCall(0.12f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()), true);
    return "pad " + act;
}
string s = "Bestiary open=" + G(bt, bi, "open") + " cursor=" + G(bt, bi, "cursor") + " zoneTab=" + G(bt, bi, "zoneTab");
s += " | Quest open=" + G(qt, qi, "_open") + " tab=" + G(qt, qi, "_tab") + " cursor=" + G(qt, qi, "_cursor") + " count=" + G(qt, qi, "_count");
var rows = G(qt, qi, "_rows") as System.Collections.IList; s += " rows=" + (rows != null ? rows.Count : -1);
s += " | UiLock=" + (DatabaseManager.isOpenUI) + " EventSystem.sel=" + (UnityEngine.EventSystems.EventSystem.current != null ? (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name : "null") : "noES");
return s;
