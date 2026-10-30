// 단조 패널 시작(리플렉션으로 StartForge). 스크린샷 직전에 실행.
var w = UnityEngine.Object.FindObjectOfType<EnhanceWindow>();
var m = typeof(EnhanceWindow).GetMethod("StartForge", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
m.Invoke(w, null);
return "forge started";
