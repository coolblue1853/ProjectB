// PT-BOT 중지 신호(봇 틱이 다음 프레임에 종료 요약을 쓴다)
System.IO.Directory.CreateDirectory("Logs/ptbot");
System.IO.File.WriteAllText("Logs/ptbot/stop", "stop");
return "ptbot stop requested";
