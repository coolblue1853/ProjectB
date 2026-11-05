// CF-11: 히트스톱 복원 확인 — 활성 적의 애니 속도 로그
var sb = new System.Text.StringBuilder();
foreach (var e in EnemyHealth.Active) { if (e == null) continue; var fx = e.GetComponent<SpriteHitFx>(); if (fx != null) sb.Append(e.name + " speed " + fx.CurrentAnimSpeed + "; "); }
return sb.ToString();
