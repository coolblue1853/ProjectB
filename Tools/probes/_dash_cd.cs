// AUTO-95: TempMobDash 재돌진 쿨 간격 — 플레이어 옆 4칸에 대시 몹을 두고 돌진 시작(IsDashing 상승) 시각 6회 기록 → Logs/dash_cd/result.txt. 플레이 모드, 세이브는 호출자가 백업/복원
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
phm.fullHP = 99999; phm.nowHp = 99999;
new SROptions().Night();
bool ok = MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject); sb.AppendLine("goto=" + ok);
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/Scarecrow_BD.prefab");
double t0 = UnityEditor.EditorApplication.timeSinceStartup; int st = 0; double stT = t0 + 1.5; GameObject mob = null; TempMobDash dash = null;
var starts = new System.Collections.Generic.List<float>(); var crouchStarts = new System.Collections.Generic.List<float>(); bool wasDash = false, wasCrouch = false; float simStart = 0f; int repos = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
 double now = UnityEditor.EditorApplication.timeSinceStartup;
 if (st == 0 && now >= stT) {
  var rbp = pc.GetComponent<Rigidbody2D>(); if (rbp) rbp.bodyType = RigidbodyType2D.Kinematic;
  float gy = GroundProbe.Y(pc.transform.position + Vector3.right * 4f);
  mob = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + 4f, gy + 0.6f, 0f), Quaternion.identity);
  var e = mob.GetComponent<EnemyHealth>(); e.maxHP = 100000; e.nowHP = 100000;
  var rb = mob.GetComponent<Rigidbody2D>(); if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
  dash = mob.AddComponent<TempMobDash>(); simStart = Time.time; st = 1; stT = now + 240; return; }
 if (st == 1) {
  if (mob == null) { sb.AppendLine("mob destroyed"); st = 2; stT = now; return; }
  phm.nowHp = 99999;
  bool d = dash.IsDashing, c = dash.CrouchAmount > 0.01f;
  if (d && !wasDash) starts.Add(Time.time - simStart);
  if (c && !wasCrouch && !d) crouchStarts.Add(Time.time - simStart);
  wasDash = d; wasCrouch = c;
  float dx = Mathf.Abs(mob.transform.position.x - pc.transform.position.x);
  if (!d && !c && dx < 2.3f) { float gy = GroundProbe.Y(pc.transform.position + Vector3.right * 4f); mob.transform.position = new Vector3(pc.transform.position.x + 4f, gy + 0.6f, 0f); Physics2D.SyncTransforms(); repos++; }
  if (starts.Count >= 6 || now >= stT) st = 2; return; }
 if (st == 2) {
  sb.AppendLine("dashStarts(rel)=" + string.Join(",", starts.ConvertAll(s => s.ToString("0.00"))));
  sb.AppendLine("crouchStarts(rel)=" + string.Join(",", crouchStarts.ConvertAll(s => s.ToString("0.00"))));
  var iv = new System.Collections.Generic.List<string>(); for (int i = 1; i < starts.Count; i++) iv.Add((starts[i] - starts[i - 1]).ToString("0.00"));
  sb.AppendLine("dashIntervals=" + string.Join(",", iv) + " repos=" + repos + " expect>=" + (TempMobTraits.CrouchTime + TempMobTraits.DashTime + TempMobTraits.DashCooldown));
  if (mob != null) UnityEngine.Object.Destroy(mob);
  var rbp = pc.GetComponent<Rigidbody2D>(); if (rbp) rbp.bodyType = RigidbodyType2D.Dynamic;
  System.IO.Directory.CreateDirectory("Logs/dash_cd"); System.IO.File.WriteAllText("Logs/dash_cd/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/dash_cd"); System.IO.File.WriteAllText("Logs/dash_cd/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
