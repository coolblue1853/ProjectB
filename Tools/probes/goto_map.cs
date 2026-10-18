// 플레이어를 맵 __MAP__ 의 입구 포털로 보낸다(MapInfo.GoToMap: 다른 맵 끄기·스포너 켜기). 예: Lv1_DeepForest
// 참고: 비활성 맵의 포털에 TravelNow 를 직접 부르면 Portal.Awake 가 안 돌아 PoolCheck 가 비어 스포너가 안 켜진다 → 이 경로를 쓴다
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
return "goto __MAP__ " + ok + " pos " + pc.transform.position;
