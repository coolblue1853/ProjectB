// PT-BOT 현재 상태: 끝났으면 summary 첫 줄, 아니면 run.log 마지막 줄
if (System.IO.File.Exists("Logs/ptbot/summary.txt")) return "DONE " + System.IO.File.ReadAllLines("Logs/ptbot/summary.txt")[0];
var l = System.IO.File.Exists("Logs/ptbot/run.log") ? System.IO.File.ReadAllLines("Logs/ptbot/run.log") : new string[0];
return "RUNNING " + (l.Length > 0 ? l[l.Length - 1] : "-") + " | timeScale=" + Time.timeScale + " play=" + UnityEditor.EditorApplication.isPlaying;
