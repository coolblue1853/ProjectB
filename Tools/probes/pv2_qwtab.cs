var w = UnityEngine.Object.FindFirstObjectByType<NpcQuestWindow>(FindObjectsInactive.Include);
var f = typeof(NpcQuestWindow).GetField("_tab", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
f.SetValue(w, (QuestListRules.Tab)__T__);
typeof(NpcQuestWindow).GetMethod("Redraw", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.Invoke(w, null);
return "tab=" + f.GetValue(w);
