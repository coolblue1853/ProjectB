// 봇 정지(사람 기록 세션으로 되돌리지 않는다 — 곧 플레이를 끌 때)
PlayBot.Stop(false);
return PlayBot.Status() + " rec=" + PlayRecorder.FilePath;
