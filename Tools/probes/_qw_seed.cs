// REWORK-5 P2~P4 검증: 퀘스트 6건 수락 + 창/입구 텍스트 덤프 → Logs/playtest/qw_seed.txt
var sb = new System.Text.StringBuilder();
if (NpcQuestWindow.IsOpen) NpcQuestWindow.Toggle();
NpcQuestLog.ClearAll(); NpcQuestLog.LevelGetter = () => 20;
var avail = QuestService.Query(new QuestFilter { Tab = QuestListRules.Tab.Available }).Items;
int n = 0;
foreach (var d in new System.Collections.Generic.List<NpcQuestDef>(avail)) { if (n >= 6) break; if (QuestService.CanAccept(d, out _) && QuestService.Accept(d.Id)) n++; }
sb.AppendLine("accepted=" + n + " active=" + QuestService.ActiveAll().Count + " tracked=" + NpcQuestTrack.Tracked(QuestService.ActiveAll()).Count + " max=" + NpcQuestTrack.Max);
sb.AppendLine("HUD:\n" + NpcQuestTracker.BuildText(QuestService.ActiveAll(), NpcQuestTrack.Tracked(QuestService.ActiveAll()), "", NpcQuestTrack.HudFolded));
System.IO.Directory.CreateDirectory("Logs/playtest"); System.IO.File.WriteAllText("Logs/playtest/qw_seed.txt", sb.ToString());
return sb.ToString();
