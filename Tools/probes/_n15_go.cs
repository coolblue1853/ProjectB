var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
bool ok = MapInfo.GoToMap("Lv1_TowerOutskirts", pc.gameObject);
return "goto Lv1_TowerOutskirts " + ok;
