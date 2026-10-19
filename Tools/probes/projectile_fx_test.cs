// ProjectileFx 확인(플레이 모드): 소환수·투사체 프리팹을 플레이어 앞에 직접 생성 → 등장 이펙트 수(Vfx.ActiveCount)·트레일 유무 → 0.3초 뒤 스크린샷 Logs/playtest/projfx.png
var p = PlayerController.instance.transform.position;
string[] names = { "FangDagger/WolfSpirit", "ChiefsWarBanner/BanditShadow", "Outskirts/StoneSentinel", "SlimeMace/SlimeHopper", "FaeWand/FaeOrb", "WispLantern/WispOrb" };
var sb = new System.Text.StringBuilder();
int v0 = Vfx.ActiveCount;
for (int i = 0; i < names.Length; i++) {
    var pf = Resources.Load<GameObject>("Weapon/DamageObject/" + names[i]);
    if (pf == null) { sb.Append(names[i] + " ? "); continue; }
    var go = UnityEngine.Object.Instantiate(pf, p + new Vector3(1.5f + i * 0.6f, 0.8f, 0f), Quaternion.identity);
    sb.Append(pf.name + (go.GetComponent<ProjectileFx>() != null ? " fx" : " -") + (go.GetComponent<TrailRenderer>() != null ? "+trail " : " "));
}
sb.Append("| vfx +" + (Vfx.ActiveCount - v0));
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/projfx.png")), false);
return sb.ToString();
