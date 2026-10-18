// 테스트용 생존 유지(시간 만료형): __SEC__ 초 동안 0.2초마다 플레이어 HP 를 채우고 0.5초짜리 무적 토큰을 새로 건다.
// 긴 무적 잠금 하나 대신 짧은 토큰을 이어 붙인다(RuntimeMonitor 의 8초 초과 잠금 경고 방지). 끝나면 자연히 풀린다
int n = Mathf.Max(1, Mathf.RoundToInt(__SEC__f / 0.2f));
var seq = DG.Tweening.DOTween.Sequence();
DG.Tweening.TweenSettingsExtensions.AppendInterval(seq, 0.2f);
DG.Tweening.TweenSettingsExtensions.AppendCallback(seq, () => {
    var ph = PlayerHealthManager.Instance; if (ph != null) ph.nowHp = ph.fullHP;
    PlayerStatus.AcquireFor(StatusFlag.Invincible, 0.5f, "probe.keep_hp");
});
DG.Tweening.TweenSettingsExtensions.SetLoops(seq, n);
return "keep hp " + __SEC__ + "s";
