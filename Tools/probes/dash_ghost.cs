// 대시를 직접 실행하고 0.15초 뒤 정지 → 잔상 스크린샷. GC 측정용으로 대시 전후 할당량도 기록
var pc = PlayerController.instance;
var m = typeof(PlayerController).GetMethod("Dash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
long before = System.GC.GetTotalMemory(false);
m.Invoke(pc, null);
DG.Tweening.DOVirtual.DelayedCall(__KEY__f, () => Time.timeScale = 0f, false);
return "dash from " + pc.transform.position + " facing=" + pc.transform.localScale.x;
