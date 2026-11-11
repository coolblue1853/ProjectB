var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
bool ok = MapInfo.GoToMap("Town", pc.gameObject);
return "goto Town " + ok;
