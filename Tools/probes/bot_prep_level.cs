// 시험 준비(봇 행동 아님, 봇 실행 전에만): 레벨·경험치·골드를 낮춰 초반 상태를 만든다. __LV__ = 레벨, __GOLD__ = 골드. 세이브는 실행 스크립트(bot_run.sh)가 백업·원복한다
if (PlayBot.IsRunning) return "bot running - refused";
LevelSystem.SetState(__LV__, 0);
DatabaseManager.money = __GOLD__;
return "level=" + LevelSystem.Level + " gold=" + DatabaseManager.money;
