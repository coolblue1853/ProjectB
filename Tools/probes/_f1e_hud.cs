NpcQuestLog.SeedRecord("Q101", 2, 1); NpcQuestLog.SeedRecord("Q102", 2, 8);
var so = new SROptions(); while (LevelSystem.Level < 3) so.LevelUp();
bool a = NpcQuestLog.Accept("Q104"); bool b = NpcQuestLog.Accept("Q106");
var all = QuestService.ActiveAll(); var tc = TimeChange.instance;
string txt = NpcQuestTracker.BuildText(all, NpcQuestTrack.Tracked(all), "", false, QuestNightHint.SecondsUntilNightNow());
var d104 = NpcQuestCatalog.Find("Q104");
string det = QuestWindowText.Detail(d104, QuestListRules.Tab.Active, NpcQuestStatus.Active);
return "acc=" + a + b + " day=" + tc.isDaytime + " lock=" + tc.LockDaytime + " v=" + tc.value + " secs=" + QuestNightHint.SecondsUntilNightNow() + "\n" + txt + "\n---detail\n" + det;
