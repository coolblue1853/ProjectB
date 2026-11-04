var t = typeof(FirstHints); var bf = System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var inst = (FirstHints)t.GetField("s_instance", System.Reflection.BindingFlags.Static|bf).GetValue(null);
t.GetMethod("SkipTutorial", bf).Invoke(inst, null);
return "after skip vis=" + FirstHints.Visible + " seen=" + FirstHints.SeenCount;
