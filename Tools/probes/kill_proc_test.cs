// Primordial Gel 처치 소환(KillSummonProc) 확인: 장착 무기가 PrimordialGel 일 때 몬스터 최대 6마리를 damage2Enemy 로 처치 → 소환 슬라임 수
var w = Weapon.Equipped;
var kp = w != null ? w.GetComponent<KillSummonProc>() : null;
int before = UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsSortMode.None).Length;
var dmg = new int[10]; dmg[0] = 999999; dmg[1] = 999999;
var list = new System.Collections.Generic.List<EnemyHealth>();
foreach (var e in EnemyHealth.Active) if (e.IsMonster && !e.isBossMob && e.nowHP > 0) list.Add(e);
int n = 0;
foreach (var e in list) { if (n >= 6) break; e.damage2Enemy(dmg, 0f, 0f, Vector2.zero, e.transform.position.x, false, true, 0f, 1f); n++; }
int after = UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsSortMode.None).Length;
return "weapon " + (w != null ? w.name : "-") + " proc " + (kp != null) + " killed " + n + " runners " + before + "->" + after;
