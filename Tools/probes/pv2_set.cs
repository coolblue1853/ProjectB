var o = new System.Text.StringBuilder();
NpcQuestWindow.Toggle();
GameSettings.SetVolume(0.4f);
o.Append("vol=" + GameSettings.Volume + " AL=" + AudioListener.volume + " saved=" + ES3.Load<float>("settings.volume", SaveSlots.SharedFile) + "\n");
PauseMenu.OpenNow();
o.Append("pauseOpen=" + PauseMenu.IsOpen + "\n");
return o.ToString();
