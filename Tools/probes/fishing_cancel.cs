// 낚시 중단 검증(플레이 중, input_bg_on 먼저): 가까운 낚시터에서 Begin → 1.5초 뒤 가상 → 키 0.3초 → 결과 Cancelled·잠금 해제 확인(fishing_check.cs)
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
FishingSpot best = null; float bd = 999f;
foreach (var sp in UnityEngine.Object.FindObjectsByType<FishingSpot>(FindObjectsSortMode.None)) { float d = Mathf.Abs(sp.transform.position.x - pc.transform.position.x); if (d < bd) { bd = d; best = sp; } }
if (best == null) return "no spot";
bool began = best.Begin();
var kb = UnityEngine.InputSystem.Keyboard.current;
DG.Tweening.DOVirtual.DelayedCall(1.5f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.RightArrow)), true);
DG.Tweening.DOVirtual.DelayedCall(1.8f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
return "began=" + began + " phase=" + (best.Session != null ? best.Session.Phase.ToString() : "-");
