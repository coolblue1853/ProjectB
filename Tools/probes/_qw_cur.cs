// 창 커서/탭 지정 후 캡처: 커서 1, 탭 0~2 는 _qw_tab 인자 대신 파일 Logs/playtest/qw_args.txt "tab cursor name"
var a = System.IO.File.ReadAllText("Logs/playtest/qw_args.txt").Trim().Split(' ');
var w = UnityEngine.Object.FindFirstObjectByType<NpcQuestWindow>(FindObjectsInactive.Include);
var T = typeof(NpcQuestWindow); var f = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
T.GetField("_tab", f).SetValue(w, (QuestListRules.Tab)int.Parse(a[0]));
T.GetField("_cursor", f).SetValue(w, int.Parse(a[1]));
T.GetMethod("Redraw", f).Invoke(w, null);
var sb = new System.Text.StringBuilder();
foreach (var tm in w.GetComponentsInChildren<TMPro.TMP_Text>(false)) if (!string.IsNullOrEmpty(tm.text) && (tm.name == "Detail" || tm.name == "RewardText" || tm.name == "Guide")) sb.AppendLine("[" + tm.name + "] " + tm.text.Replace("\n", " | "));
UnityEngine.ScreenCapture.CaptureScreenshot("Logs/playtest/" + a[2] + ".png");
return sb.ToString();
