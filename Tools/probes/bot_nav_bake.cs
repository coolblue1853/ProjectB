// 봇 이동 그래프 굽기(에디터, 플레이 아닐 때): Map 아래 맵마다 Assets/Bot/Nav/<맵>.json
if (Application.isPlaying) return "play mode - stop first";
return BotNavBaker.BakeAll();
