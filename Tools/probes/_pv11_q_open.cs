LevelSystem.SetState(20, 0);
if (!NpcQuestWindow.IsOpen) NpcQuestWindow.Toggle();
return "toggled open=" + NpcQuestWindow.IsOpen;
