// PV11 #6: 퀘스트 창 첫 행(머리글) 상태 보고. __OPEN__ 이 1이면 먼저 연다
var w = UnityEngine.Object.FindFirstObjectByType<NpcQuestWindow>(FindObjectsInactive.Include);
var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var rows = (System.Collections.IList)typeof(NpcQuestWindow).GetField("_rows", bf).GetValue(w);
string s = "open=" + NpcQuestWindow.IsOpen + " tab=" + typeof(NpcQuestWindow).GetField("_tab", bf).GetValue(w) + " cursor=" + typeof(NpcQuestWindow).GetField("_cursor", bf).GetValue(w) + " rows=" + rows.Count;
if (rows.Count > 0) { var r0 = rows[0]; var t = r0.GetType(); string key = (string)t.GetField("Key").GetValue(r0); s += " row0 quest=" + (t.GetField("Quest").GetValue(r0) == null ? "null(header)" : "quest") + " key=" + key + " isFolded=" + QuestFoldRules.IsFolded(key); }
return s;
