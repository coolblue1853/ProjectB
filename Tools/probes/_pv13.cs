// PV13: Cooldown 교체(AUTO-83/84) 동작 확인 — ContactDamage 간격 / TempMobDash 쿨 / NightRaid 시작·스폰 → Logs/pv13/result.txt
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
phm.fullHP = 99999; phm.nowHp = 99999;
new SROptions().Night();
bool ok = MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
sb.AppendLine("goto=" + ok);
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/Scarecrow_BD.prefab");
double t0 = UnityEditor.EditorApplication.timeSinceStartup; int st = 0; double stT = t0 + 1.5; GameObject mob = null; float lastHp = 0; var hits = new System.Collections.Generic.List<double>();
var dashStarts = new System.Collections.Generic.List<double>(); float lastX = 0; double lastT = 0; bool inDash = false; NightRaid nr = null; int maxAlive = 0; int rkills = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
 double now = UnityEditor.EditorApplication.timeSinceStartup;
 if (st == 0 && now >= stT) { // 몸박
  mob = UnityEngine.Object.Instantiate(prefab, pc.transform.position + Vector3.up * 0.2f, Quaternion.identity); var e = mob.GetComponent<EnemyHealth>(); e.maxHP = 100000; e.nowHP = 100000;
  var rb = mob.GetComponent<Rigidbody2D>(); if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
  lastHp = phm.nowHp; st = 1; stT = now + 3.5; return; }
 if (st == 1) { if (phm.nowHp < lastHp) { hits.Add(now); lastHp = phm.nowHp; } else if (phm.nowHp > lastHp) lastHp = phm.nowHp;
  if (now >= stT) { sb.AppendLine("CONTACT hits=" + hits.Count + " times(rel)=" + string.Join(",", hits.ConvertAll(h => (h - (stT - 3.5)).ToString("0.00"))) + " noDamgeTime=" + phm.noDamgeTime);
   UnityEngine.Object.Destroy(mob); phm.nowHp = 99999;
   float gy = GroundProbe.Y(pc.transform.position + Vector3.right * 4f);
   mob = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + 4f, gy + 0.6f, 0f), Quaternion.identity); var e = mob.GetComponent<EnemyHealth>(); e.maxHP = 100000; e.nowHP = 100000;
   mob.AddComponent<TempMobDash>(); lastX = mob.transform.position.x; lastT = now; st = 2; stT = now + 9; } return; }
 if (st == 2) { if (mob == null) { sb.AppendLine("DASH mob destroyed"); st = 3; stT = now; return; }
  float x = mob.transform.position.x; float v = Mathf.Abs(x - lastX) / Mathf.Max(0.001f, (float)(now - lastT)); lastX = x; lastT = now;
  if (v > 6f && !inDash) { inDash = true; dashStarts.Add(now - (stT - 9)); } else if (v < 2f) inDash = false;
  if (now >= stT) { sb.AppendLine("DASH starts(rel)=" + string.Join(",", dashStarts.ConvertAll(d => d.ToString("0.0"))) + " cooldown=" + TempMobTraits.DashCooldown + " (기대: 연속 시작 간격 >= 3.5+)");
   UnityEngine.Object.Destroy(mob); st = 3; stT = now; } return; }
 if (st == 3) { nr = NightRaid.Instance; bool b = nr != null && nr.DebugBegin("Lv1_NearForest_SideRoad"); sb.AppendLine("RAID begin=" + b + " nr=" + (nr != null)); st = 4; stT = now + 6; return; }
 if (st == 4) { if (nr != null) { maxAlive = Mathf.Max(maxAlive, nr.Alive); rkills = nr.Kills; }
  if (now >= stT) { sb.AppendLine("RAID active=" + (nr != null && nr.IsActive) + " maxAlive=" + maxAlive + " wave=" + (nr != null ? nr.Wave : -1) + " kills=" + rkills);
   System.IO.Directory.CreateDirectory("Logs/pv13"); System.IO.File.WriteAllText("Logs/pv13/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/pv13"); System.IO.File.WriteAllText("Logs/pv13/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
