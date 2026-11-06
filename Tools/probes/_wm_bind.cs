var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var f = typeof(AttackManager).GetField("skillAction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var arr = (UnityEngine.InputSystem.InputAction[])f.GetValue(am);
var sb = new System.Text.StringBuilder();
for (int i = 0; i < arr.Length; i++) { sb.Append(i + ":"); foreach (var b in arr[i].bindings) sb.Append(b.path + ","); sb.Append(" | "); }
return sb.ToString() + " kbEnabled=" + UnityEngine.InputSystem.Keyboard.current.enabled + " staggered=" + PlayerController.instance.isAttacked + " timeScale=" + UnityEngine.Time.timeScale;
