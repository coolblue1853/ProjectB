// 보스 공격 판정 감시(플레이어 체력은 계속 채운다): 12초 동안 0.05초 간격으로 활성 EnemyDamageObject 중 __KEY__ 가 이름에 든 것을 기록하고, 종류별 첫 등장 0.12초 뒤 게임 뷰 스크린샷(Logs/playtest/boss_<이름>.png)
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_trace", log);
var seen = new System.Collections.Generic.HashSet<string>();
for (int k = 0; k < 240; k++)
{
    DG.Tweening.DOVirtual.DelayedCall(k * 0.05f, () => {
        var ph = PlayerHealthManager.Instance; if (ph != null) ph.nowHp = ph.fullHP; // 무적 잠금 대신 체력 유지(잠금 12초 초과 경고 방지)
        foreach (var o in UnityEngine.Object.FindObjectsByType<EnemyDamageObject>(FindObjectsSortMode.None))
        {
            if (o.name.IndexOf("__KEY__", System.StringComparison.Ordinal) < 0) continue;
            string n = o.name.Replace("(Clone)", "");
            if (seen.Add(n))
            {
                var sr = o.GetComponent<SpriteRenderer>();
                log.Append(n + " @" + o.transform.position.ToString("F1") + " spr=" + (sr && sr.sprite ? sr.sprite.name : "-") + "; ");
                string file = System.IO.Path.GetFullPath("Logs/playtest/boss_" + n + ".png");
                DG.Tweening.DOVirtual.DelayedCall(0.12f, () => ScreenCapture.CaptureScreenshot(file), false);
            }
        }
    }, false);
}
DG.Tweening.DOVirtual.DelayedCall(12.2f, () => log.Append("DONE"), false);
return "watching";
