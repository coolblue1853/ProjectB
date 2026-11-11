// 창 열기(일반/입구) 후 텍스트 덤프. 인자 없음: 일반 모드 Toggle
if (NpcQuestWindow.IsOpen) NpcQuestWindow.Toggle();
NpcQuestWindow.Toggle();
var sb = new System.Text.StringBuilder("IsOpen=" + NpcQuestWindow.IsOpen + "\n");
var w = UnityEngine.Object.FindFirstObjectByType<NpcQuestWindow>(FindObjectsInactive.Include);
foreach (var tm in w.GetComponentsInChildren<TMPro.TMP_Text>(false)) if (!string.IsNullOrEmpty(tm.text)) sb.AppendLine("[" + tm.name + "] " + tm.text.Replace("\n", " | "));
return sb.ToString();
