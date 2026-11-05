// PV9: 화상 재적용 틱 중첩 안 됨 → Logs/pv9/burn.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder(); var phm = PlayerHealthManager.Instance;
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.1; EnemyHealth foe = null; int h0 = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
 if (st == 0) { pc.transform.position = new Vector3(-384, 9, pc.transform.position.z); until = now + 8; st = 1; return; }
 if (st == 1) { foe = EnemyHealth.Active.FirstOrDefault(e => e.isActiveAndEnabled && !e.IsElite && !e.isBossMob && e.IsMonster);
   sb.AppendLine("foe=" + (foe ? foe.name : "null")); if (foe == null) { st = 9; return; }
   phm.fullHP = 9999; phm.nowHp = 9999; foe.nowHP = 99999; h0 = foe.nowHP;
   foe.ApplyBurn(1000); until = now + 0.6; st = 2; return; }
 if (st == 2) { foe.ApplyBurn(1000); foe.ApplyBurn(1000); sb.AppendLine("재적용 2회 후 PosionAttack 수=" + foe.GetComponentsInChildren<PosionAttack>(true).Length + " (자식) / " + foe.GetComponents<PosionAttack>().Length + " (본체)"); until = now + 5.0; st = 3; return; }
 if (st == 3) { sb.AppendLine("화상 총 HP 손실=" + (h0 - foe.nowHP) + " (중첩 없으면 ~600~750, 3중첩이면 ~1800)"); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.File.WriteAllText("Logs/pv9/burn.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
