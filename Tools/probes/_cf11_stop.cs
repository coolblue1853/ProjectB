// CF-11 적 히트스톱 확인(플레이 모드): 활성 몬스터 1체에 OnEnemyHit 직접 호출 → 애니 속도 0 확인. 복원 확인은 별도 호출로 시간 후 재실행
EnemyHealth t = null;
foreach (var e in new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active)) { if (e != null && e.isActiveAndEnabled && e.GetComponent<Animator>() != null) { t = e; break; } }
if (t == null) return "no target";
float before = t.GetComponent<Animator>().speed;
HitFeedback.OnEnemyHit(t, false, false, false);
var fx = t.GetComponent<SpriteHitFx>();
return "target " + t.name + " before " + before + " after " + fx.CurrentAnimSpeed + " finisher=" + (Weapon.Equipped != null ? Weapon.Equipped.nowConboCount.ToString() + "/" + (Weapon.Equipped.attackAnimName != null ? Weapon.Equipped.attackAnimName.Length : 0).ToString() : "-1");
