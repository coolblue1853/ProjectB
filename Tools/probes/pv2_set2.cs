var t = typeof(PauseMenu); var bf = System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var inst = (PauseMenu)t.GetField("instance", System.Reflection.BindingFlags.Static|bf).GetValue(null);
t.GetField("settingsMode", bf).SetValue(inst, true);
t.GetMethod("Redraw", bf).Invoke(inst, null);
return "settingsMode ok";
