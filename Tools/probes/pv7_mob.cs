// PV7: 몹 둔화·석화 연쇄·화상·장판, 반사 자세, 플레이어 수면 → Logs/pv7/mob.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder(); var phm = PlayerHealthManager.Instance;
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.1; EnemyHealth foe = null; int h0 = 0, p0 = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
 if (st == 0) { pc.transform.position = new Vector3(-384, 9, pc.transform.position.z); until = now + 8; st = 1; return; }
 if (st == 1) { foe = EnemyHealth.Active.FirstOrDefault(e => e.isActiveAndEnabled && !e.IsElite && !e.isBossMob && e.IsMonster);
   sb.AppendLine("foe=" + (foe ? foe.name : "null")); if (foe == null) { st = 9; return; }
   foe.nowHP = 99999; phm.fullHP = 9999; phm.nowHp = 9999; 
   sb.AppendLine("slow 전 mul=" + foe.MoveSpeedMul); foe.ApplySlow(0.5f, 1.5f); sb.AppendLine("slow 중 mul=" + foe.MoveSpeedMul); until = now + 1.8; st = 2; return; }
 if (st == 2) { sb.AppendLine("slow 1.8초 뒤 mul=" + foe.MoveSpeedMul); foe.ApplyPetrify(0.5f); sb.AppendLine("석화1 bt.enabled=" + foe.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>().enabled + " stun=" + foe.isStun); until = now + 0.9; st = 3; return; }
 if (st == 3) { sb.AppendLine("석화1 끝 bt.enabled=" + foe.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>().enabled + " stun=" + foe.isStun); foe.ApplyPetrify(0.5f); sb.AppendLine("석화2(면역 중) bt.enabled=" + foe.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>().enabled + " stun=" + foe.isStun); until = now + 3.5; st = 4; return; }
 if (st == 4) { sb.AppendLine("면역 3초 뒤 석화3"); foe.ApplyPetrify(0.5f); sb.AppendLine("석화3 bt.enabled=" + foe.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>().enabled); until = now + 1.0; st = 5; return; }
 if (st == 5) { foe.nowHP = 99999; h0 = foe.nowHP; foe.ApplyBurn(1000); until = now + 4.6; st = 6; return; }
 if (st == 6) { sb.AppendLine("화상 4.6초 HP 손실=" + (h0 - foe.nowHP) + " (틱 150×4=600 기대)"); foe.nowHP = 99999; h0 = foe.nowHP; EnemyDamageZone.Spawn(foe.transform.position, 3f, 50, 2f, 0.5f, 0.6f, 1f); until = now + 2.6; st = 7; return; }
 if (st == 7) { sb.AppendLine("장판 2초 HP 손실=" + (h0 - foe.nowHP) + " slowmul=" + foe.MoveSpeedMul + " (50×4~5 기대)"); foe.BeginReflect(3f); foe.nowHP = 99999; phm.nowHp = 9999; p0 = phm.nowHp; until = now + 2.2; st = 71; return; }
 if (st == 71) { p0 = phm.nowHp; foe.damage2Enemy(new int[]{1000}, 0f, 0f, Vector2.right, pc.transform.position.x, false, false, 0f, 1f); until = now + 0.3; st = 8; return; }
 if (st == 8) { sb.AppendLine("반사 자세 중 타격 플레이어 손실=" + (p0 - phm.nowHp) + " 몹 HP 손실=" + (99999 - foe.nowHP)); until = now + 2.5; st = 81; return; }
 if (st == 81) { sb.AppendLine("sleep Apply=" + PlayerSleep.Apply(2f) + " IsSleeping=" + PlayerSleep.IsSleeping + " InputLock=" + PlayerStatus.Has(StatusFlag.InputLock)); until = now + 0.5; st = 82; return; }
 if (st == 82) { phm.nowHp = 9999; phm.damage2Player(10, 0.3f, 3f, Vector2.right, phm.transform.position.x - 1, false); until = now + 0.3; st = 83; return; }
 if (st == 83) { sb.AppendLine("피격 후 IsSleeping=" + PlayerSleep.IsSleeping + " InputLock=" + PlayerStatus.Has(StatusFlag.InputLock)); PlayerSleep.Apply(1f); until = now + 1.5; st = 84; return; }
 if (st == 84) { sb.AppendLine("1초 경과 IsSleeping=" + PlayerSleep.IsSleeping + " InputLock=" + PlayerStatus.Has(StatusFlag.InputLock)); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.Directory.CreateDirectory("Logs/pv7"); System.IO.File.WriteAllText("Logs/pv7/mob.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
