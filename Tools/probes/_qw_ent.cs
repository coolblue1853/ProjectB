if (NpcQuestWindow.IsOpen) NpcQuestWindow.Toggle();
var ok = QuestEntrance.Open(QuestEntrance.Board());
var w = UnityEngine.Object.FindFirstObjectByType<NpcQuestWindow>(FindObjectsInactive.Include);
var sb = new System.Text.StringBuilder("open=" + ok + " entrance=" + (NpcQuestWindow.EntranceFilter != null) + "\n");
foreach (var tm in w.GetComponentsInChildren<TMPro.TMP_Text>(false)) if (!string.IsNullOrEmpty(tm.text) && tm.name != "Hint") sb.AppendLine("[" + tm.name + "] " + tm.text.Replace("\n", " | "));
UnityEngine.ScreenCapture.CaptureScreenshot("Logs/playtest/qw_entrance.png");
return sb.ToString();
