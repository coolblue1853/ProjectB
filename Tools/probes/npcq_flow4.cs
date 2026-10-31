var before = NpcQuestLog.Serialize();
NpcQuestLog.ClearAll();
string cleared = NpcQuestLog.Serialize();
NpcQuestLog.LoadFromDisk();
string after = NpcQuestLog.Serialize();
NpcQuestWindow.Toggle();
return "before=" + before + " | cleared='" + cleared + "' | reloaded=" + after + " eq=" + (before == after) + " windowOpen=" + NpcQuestWindow.IsOpen;
