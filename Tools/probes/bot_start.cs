// 봇 시작(PlayBot.Run). __MIN__ = 실제 시간 분, __GOAL__ = hunt, __MAPS__ = 순회 맵(쉼표, 빈 문자열 = 기본, "-" = 이동 없음). 플레이 모드에서만
return "start=" + PlayBot.Run(__MIN__f, "__GOAL__", "__MAPS__") + " " + PlayBot.Status() + " rec=" + PlayRecorder.FilePath;
