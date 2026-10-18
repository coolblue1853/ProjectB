// 테스트용 생존 유지(시간 만료형): __SEC__ 초 동안 0.2초마다 플레이어 HP 를 채우고 0.5초짜리 무적 토큰을 새로 건다.
// 긴 무적 잠금 하나 대신 짧은 토큰을 이어 붙인다. 8초(40틱)마다 3틱(0.6초) 토큰 갱신을 쉬어 무적 플래그가 잠깐 풀리게 한다
// → RuntimeMonitor 의 "Invincible 12초 초과" 이상 보고가 테스트에서 나지 않는다(B-132). 쉬는 동안도 HP 는 계속 채운다
int n = Mathf.Max(1, Mathf.RoundToInt(__SEC__f / 0.2f));
int[] tick = new int[1];
var seq = DG.Tweening.DOTween.Sequence();
DG.Tweening.TweenSettingsExtensions.AppendInterval(seq, 0.2f);
DG.Tweening.TweenSettingsExtensions.AppendCallback(seq, () => {
    var ph = PlayerHealthManager.Instance; if (ph != null) ph.nowHp = ph.fullHP;
    tick[0]++;
    if (tick[0] % 40 < 37) { PlayerStatus.AcquireFor(StatusFlag.Invincible, 0.5f, "probe.keep_hp"); }
});
DG.Tweening.TweenSettingsExtensions.SetLoops(seq, n);
return "keep hp " + __SEC__ + "s";
