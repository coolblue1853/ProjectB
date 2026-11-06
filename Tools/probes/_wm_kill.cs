var sb = new System.Text.StringBuilder();
// 숙련 초기화(Lv1) 후 실제 몹 1마리 처치
WeaponMasteryService.Data.LoadFrom(new WeaponMasterySave());
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am.equipWeapon;
sb.Append("before=" + WeaponMasteryService.Data.GetExp(w.MasteryKey) + " expP=" + LevelSystem.Exp + "/Lv" + LevelSystem.Level);
EnemyHealth target = null;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { var r = MonsterTable.Get(e.MonsterId); if (e.nowHP > 0 && r != null && r.exp.HasValue && r.exp.Value > 0) { target = e; break; } }
if (target == null) { return sb.Append(" no enemy").ToString(); }
sb.Append(" enemy=" + target.name);
target.onlyDamage2Enemy(99999999);
sb.Append(" after=" + WeaponMasteryService.Data.GetExp(w.MasteryKey) + " expP=" + LevelSystem.Exp + "/Lv" + LevelSystem.Level);
SaveManager.instance.DataSave();
return sb.ToString();
