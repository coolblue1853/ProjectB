// B-114 검증: 무적 스킬대시(0.5s) 직후 일반 대시 → 대시 종료 뒤에도 스킬 무적 유지되는지, 0.6s 후 전부 해제되는지 (게임시간 로그)
var pc = PlayerController.instance;
pc.SkillDash(0.5f, 1f, 0f, true, false);
typeof(PlayerController).GetMethod("Dash", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(pc, null);
string start = PlayerStatus.Dump().Replace("\n", " | ");
foreach (var at in new[]{ 0.3f, 0.7f })
{
    float when = at;
    DG.Tweening.DOVirtual.DelayedCall(when, () => Debug.Log("[probe] t=" + when + " inv=" + PlayerStatus.Has(StatusFlag.Invincible) + " armor=" + PlayerStatus.Has(StatusFlag.SuperArmor) + " :: " + PlayerStatus.Dump().Replace("\n", " | ")), false);
}
return "start: " + start;
