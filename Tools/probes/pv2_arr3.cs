var pc = PlayerController.instance; var p0 = pc.transform.position; var sb = new System.Text.StringBuilder();
sb.AppendLine("player=" + p0.ToString("F1") + " zones=" + ArrivalSafeZone.Points.Count + " inZone=" + ArrivalSafeZone.IsInside(p0));
int total=0, near=0, inzone=0; float minD=999;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { total++; float d = Vector2.Distance(e.transform.position, p0); if (d<minD) minD=d; if (d<14) near++; if (ArrivalSafeZone.IsInside(e.transform.position)) inzone++; }
sb.AppendLine("enemies total=" + total + " within14=" + near + " inZone=" + inzone + " nearest=" + minD.ToString("F1"));
var phm = UnityEngine.Object.FindFirstObjectByType<PlayerHealthManager>();
sb.AppendLine("hp=" + phm.nowHp + " canBeHit=" + phm.CanBeHit + " inv=" + PlayerStatus.Has(StatusFlag.Invincible));
return sb.ToString();
