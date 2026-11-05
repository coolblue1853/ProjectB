// PV11 #9: 보스 근처 Die() 경로 EXP/골드 확인 → Logs/pv11/die.txt
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
if (NpcQuestWindow.IsOpen) NpcQuestWindow.Toggle();
phm.fullHP = 99999; phm.nowHp = 99999;
bool ok = MapInfo.GoToMap("Lv1_TowerOutskirts", pc.gameObject);
double until = UnityEditor.EditorApplication.timeSinceStartup + 7; int st = 0;
var die = typeof(PlayerHealthManager).GetMethod("Die", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { if (UnityEditor.EditorApplication.timeSinceStartup < until) return;
 try {
  if (st == 0) {
   EnemyHealth boss = null; foreach (var e in EnemyHealth.Active) if (e.isBossMob && e.nowHP > 0) boss = e;
   sb.AppendLine("goto=" + ok + " boss=" + (boss ? boss.name : "none") + " activeBoss=" + EnemyHealth.ActiveBossCount);
   if (boss == null) { System.IO.File.WriteAllText("Logs/pv11/die.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
   var bt = boss.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>(); if (bt != null) bt.DisableBehavior();
   pc.transform.position = new Vector3(boss.transform.position.x - 3f, boss.transform.position.y, pc.transform.position.z);
   LevelSystem.SetState(20, 1000); DatabaseManager.money = 10000;
   sb.AppendLine("near=" + BgmDirector.BossNearPlayer() + " Lv=" + LevelSystem.Level + " exp0=" + LevelSystem.Exp + " money0=" + DatabaseManager.money);
   die.Invoke(phm, null);
   sb.AppendLine("A(Lv20 boss near) exp=" + LevelSystem.Exp + " lastLoss=" + LevelSystem.LastDeathLoss + " money=" + DatabaseManager.money);
   until = UnityEditor.EditorApplication.timeSinceStartup + 6; st = 1; return; }
  if (st == 1) {
   sb.AppendLine("after respawn: dead=" + phm.IsDead + " near=" + BgmDirector.BossNearPlayer());
   LevelSystem.SetState(20, 1000);
   die.Invoke(phm, null);
   sb.AppendLine("B(Lv20 no boss) exp=" + LevelSystem.Exp + " lastLoss=" + LevelSystem.LastDeathLoss + " money=" + DatabaseManager.money);
   until = UnityEditor.EditorApplication.timeSinceStartup + 6; st = 2; return; }
  if (st == 2) {
   LevelSystem.SetState(8, 100);
   die.Invoke(phm, null);
   sb.AppendLine("C(Lv8) exp=" + LevelSystem.Exp + " lastLoss=" + LevelSystem.LastDeathLoss + " money=" + DatabaseManager.money);
   System.IO.File.WriteAllText("Logs/pv11/die.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.File.WriteAllText("Logs/pv11/die.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
