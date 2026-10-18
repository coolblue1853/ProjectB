// W2 Feel: FeelFx.PlayerPop → 플레이어 메시 크기가 잠깐 커졌다 원래대로 돌아오는지 (게임 시간 기준). 결과 pb_feel
var mesh = PlayerController.instance.mainCharacter.transform;
float y0 = mesh.localScale.y;
var log = new System.Text.StringBuilder("base=" + y0.ToString("F3") + "; ");
System.AppDomain.CurrentDomain.SetData("pb_feel", log);
FeelFx.PlayerPop(0.25f);
foreach (float t in new[]{ 0.1f, 0.2f, 0.6f })
{
    float tt = t;
    DG.Tweening.DOVirtual.DelayedCall(tt, () => log.Append(tt.ToString("F1") + "s=" + mesh.localScale.y.ToString("F3") + "; "), false);
}
return "played";
