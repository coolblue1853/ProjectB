// 아티팩트: 전부 해금 → 각각 장착·사용 → 효과 확인 (HP 회복량, 무적 토큰, timeScale, 주변 적 HP). 쿨다운 무시용 readyAt 리셋
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var sys = UnityEngine.Object.FindFirstObjectByType<ArtifactSystem>();
foreach (var d in ArtifactSystem.Defs) ArtifactSystem.Unlock(d.id);
var eqF = typeof(ArtifactSystem).GetField("equipped", BF); var rdF = typeof(ArtifactSystem).GetField("readyAt", BF);
var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
phm.HpDown(Mathf.RoundToInt(phm.fullHP * 0.5f)); int hp0 = phm.nowHp;
eqF.SetValue(sys, ArtifactId.HealingDew); rdF.SetValue(sys, 0f); ArtifactSystem.TryUseNow(); sb.Append("dew hp " + hp0 + "→" + phm.nowHp + "/" + phm.fullHP + "; ");
eqF.SetValue(sys, ArtifactId.GuardianShell); rdF.SetValue(sys, 0f); ArtifactSystem.TryUseNow(); sb.Append("shell invincible=" + PlayerStatus.Has(StatusFlag.Invincible) + "; ");
eqF.SetValue(sys, ArtifactId.Hourglass); rdF.SetValue(sys, 0f); ArtifactSystem.TryUseNow(); sb.Append("hourglass ts=" + Time.timeScale + "; ");
Time.timeScale = 1f;
eqF.SetValue(sys, ArtifactId.StarBurst); rdF.SetValue(sys, 0f);
int hit = 0; foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) if (Vector2.Distance(e.transform.position, PlayerController.instance.transform.position) <= 4f && e.nowHP > 0) hit++;
ArtifactSystem.TryUseNow(); sb.Append("burst targets=" + hit + " cd=" + ArtifactSystem.CooldownLeft.ToString("0") + "; ");
return sb.ToString();
