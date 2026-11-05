var pc = PlayerController.instance;
var kb = UnityEngine.InputSystem.Keyboard.current;
if (kb == null) return "NO KEYBOARD";
float y0 = pc.transform.position.y, maxY = y0, t0 = Time.time; bool released = false;
System.IO.File.WriteAllText("Logs/pv4/tapjump.txt", "start y=" + y0 + " grounded=" + pc.isGround + "\n");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.C));
UnityEngine.Events.UnityAction tick = null;
tick = () => {
  if (!EditorApplication.isPlaying) { Application.onBeforeRender -= tick; return; }
  maxY = Mathf.Max(maxY, pc.transform.position.y);
  if (!released && Time.time - t0 > 0.1f) { released = true; UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); }
  if (Time.time - t0 > 1.5f) { System.IO.File.AppendAllText("Logs/pv4/tapjump.txt", "rise=" + (maxY - y0) + "\n"); Application.onBeforeRender -= tick; }
};
Application.onBeforeRender += tick;
return "tap started";
