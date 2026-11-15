// 시험 준비(봇 행동 아님, 봇 실행 전에만): 플레이어를 __MAP__ 로 보낸다. 봇 실행 중에는 쓰지 않는다
var pc = PlayerController.instance;
if (pc == null) return "no player";
if (PlayBot.IsRunning) return "bot running - refused";
if (BgmDirector.CurrentMap == "__MAP__") return "already __MAP__ x=" + pc.transform.position.x.ToString("F1");
return "goto=" + MapInfo.GoToMap("__MAP__", pc.gameObject) + " from=" + BgmDirector.CurrentMap;
