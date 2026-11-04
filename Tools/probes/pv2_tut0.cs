FirstHints.DebugReset();
var t = typeof(FirstHints); var inst = (FirstHints)t.GetField("s_instance", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
t.GetMethod("TryStartTutorial", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(inst, null);
return "started seen=" + FirstHints.SeenCount;
