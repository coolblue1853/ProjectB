// PV7: 반사 자세 재측정 + 수면 자연 종료 → Logs/pv7/reflect.txt
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
var foe = EnemyHealth.Active.FirstOrDefault(e => e.IsMonster && !e.isBossMob && !e.IsElite);
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 4; int p0 = 0, m0 = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
 if (st == 0) { sb.AppendLine("sleep 대기 후 Sleeping=" + PlayerSleep.IsSleeping); foe.nowHP = 999999; phm.fullHP = 9999; phm.nowHp = 9999; foe.BeginReflect(5f); p0 = phm.nowHp; m0 = foe.nowHP;
   foe.damage2Enemy(new int[]{20000,20000}, 0f, 0f, Vector2.right, pc.transform.position.x, false, false, 0f, 1f); until = now + 0.3; st = 1; return; }
 if (st == 1) { sb.AppendLine("반사 중: 몹 손실=" + (m0 - foe.nowHP) + " 플레이어 손실=" + (p0 - phm.nowHp)); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.File.WriteAllText("Logs/pv7/reflect.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
