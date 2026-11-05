// 소환수·궤도·유도탄이 몹에게 실제 타격을 주는지 → Logs/pv11/atk.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder();
var foe = EnemyHealth.Active.FirstOrDefault(e => e.isActiveAndEnabled && !e.IsElite && !e.isBossMob && e.IsMonster);
if (foe == null) return "no foe";
string[] paths = { "Weapon/DamageObject/WispLantern/WispRing", "Weapon/DamageObject/WispLantern/WispRing", "Weapon/DamageObject/WispLantern/WispRing" };
int st = 0; double until = 0; int hp0 = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
  if (st < 3) {
   var pf = Resources.Load<GameObject>(paths[st]); if (pf == null) { sb.AppendLine(paths[st] + " null"); st++; return; }
   foe.nowHP = 99999; hp0 = foe.nowHP;
   foe.transform.position = pc.transform.position + new Vector3(1.2f, 0.8f, 0);
   var rb = foe.GetComponent<Rigidbody2D>(); if (rb) rb.linearVelocity = Vector2.zero;
   UnityEngine.Object.Instantiate(pf, pc.transform.position + new Vector3(0, 0.4f, 0), Quaternion.identity);
   until = now + 3; st += 10; return; }
  if (st >= 10) { int i = st - 10; sb.AppendLine(paths[i] + " foeHP " + hp0 + " -> " + foe.nowHP + " (피해 " + (hp0 - foe.nowHP) + ")"); st = i + 1; return; }
  st = 9;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 3 || st == 9) { System.IO.File.WriteAllText("Logs/pv11/atk.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 99; } };
UnityEditor.EditorApplication.update += upd; return "started";
