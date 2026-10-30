// 활공 검증 준비(플레이 중): 성벽 맵으로 이동, 체력 채움, 낮 고정 확인
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var phm0 = PlayerHealthManager.Instance; if (phm0 != null) phm0.nowHp = phm0.fullHP;
bool ok = MapInfo.GoToMap("Lv2_BrokenRampart", pc.gameObject);
return "goto " + ok + " pos " + pc.transform.position + " unlocked " + GlideMotor.Unlocked + " motor " + (GlideMotor.Instance != null);
