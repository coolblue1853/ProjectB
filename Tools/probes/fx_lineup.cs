// 신규 도트 이펙트 6종을 플레이어 앞에 나란히 재생 → 0.18초 뒤 스크린샷 Logs/playtest/fx_lineup.png
var p = PlayerController.instance.transform.position;
VfxId[] ids = { VfxId.SmokeGray, VfxId.SmokePurple, VfxId.DustBrown, VfxId.DustStone, VfxId.GroundShock, VfxId.BurstRed };
float gy = GroundProbe.Y(p);
var sb = new System.Text.StringBuilder();
for (int i = 0; i < ids.Length; i++) {
    var pos = new Vector3(p.x - 7f + i * 2.8f, VfxLibrary.IsGroundFx(ids[i]) ? gy : p.y + 1.2f, 0f);
    sb.Append(ids[i] + "=" + Vfx.Play(ids[i], pos, false, 1.2f) + " ");
}
DG.Tweening.DOVirtual.DelayedCall(0.18f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/fx_lineup.png")), false);
return sb.ToString();
