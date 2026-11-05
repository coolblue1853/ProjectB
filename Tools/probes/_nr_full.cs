// AUTO-96: NightRaid 끝까지 — 밤→시작→웨이브(스폰 시각)→처치→보상(골드·EXP·Potion)→종료 후 스폰 정지·낮 복귀 → Logs/nr_full/result.txt. 플레이 모드, 세이브는 호출자가 백업/복원
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder(); var inv = InventoryManager.instance;
phm.fullHP = 99999; phm.nowHp = 99999;
new SROptions().Night();
bool ok = MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject); sb.AppendLine("goto=" + ok);
var tc = TimeChange.instance; var nr = NightRaid.Instance;
double stT = UnityEditor.EditorApplication.timeSinceStartup + 2.0; int st = 0; float t0 = 0f; int lastCount = 0, lastWave = 0, maxAlive = 0; bool lastActive = false; float doneAt = -1f;
int gold0 = 0, exp0 = 0, pot0 = 0; var seen = new System.Collections.Generic.Dictionary<EnemyHealth, float>();
UnityEditor.EditorApplication.CallbackFunction upd = null;
System.Func<int> pots = () => inv.Service.ScanModel().Count("Potion");
System.Action<string> L = s => sb.AppendLine("t=" + (Time.time - t0).ToString("0.00") + " " + s);
upd = () => { try {
 double now = UnityEditor.EditorApplication.timeSinceStartup; phm.nowHp = 99999;
 if (st == 0 && now >= stT) {
  var rbp = pc.GetComponent<Rigidbody2D>(); if (rbp) rbp.bodyType = RigidbodyType2D.Kinematic;
  gold0 = DatabaseManager.money; exp0 = LevelSystem.Exp; pot0 = pots(); t0 = Time.time;
  L("night=" + !tc.isDaytime + " nightNo=" + nr.Night + " todayMap=" + nr.TodayMap + " lvl=" + LevelSystem.Level + " gold0=" + gold0 + " exp0=" + exp0 + " potion0=" + pot0);
  bool b = nr.DebugBegin("Lv1_NearForest_SideRoad"); L("DebugBegin=" + b + " active=" + nr.IsActive + " wave=" + nr.Wave); lastActive = nr.IsActive; lastWave = nr.Wave; st = 1; stT = now + 200; return; }
 if (st == 1) {
  if (nr.Wave != lastWave) { L("WAVE " + nr.Wave); lastWave = nr.Wave; }
  foreach (var e in nr.Raiders) if (e != null && !seen.ContainsKey(e)) { seen[e] = Time.time; L("SPAWN#" + seen.Count + " wave=" + nr.Wave + " alive=" + nr.Alive + " x=" + e.transform.position.x.ToString("0.0")); }
  maxAlive = Mathf.Max(maxAlive, nr.Alive);
  foreach (var e in new System.Collections.Generic.List<EnemyHealth>(nr.Raiders)) if (e != null && !e.IsInPool && seen.ContainsKey(e) && Time.time - seen[e] > 1f) { e.ReleaseEnemy(true, true); L("KILL kills=" + nr.Kills); }
  if (lastActive && !nr.IsActive) { doneAt = Time.time; L("END active=false kills=" + nr.Kills + " spawnedTotal=" + seen.Count + " maxAlive=" + maxAlive + " dGold=" + (DatabaseManager.money - gold0) + " dExp=" + (LevelSystem.Exp - exp0) + " lvl=" + LevelSystem.Level + " dPotion=" + (pots() - pot0)); st = 2; stT = now + 8; }
  else if (now >= stT) { L("TIMEOUT active=" + nr.IsActive); st = 3; }
  lastActive = nr.IsActive; return; }
 if (st == 2) {
  int before = seen.Count; foreach (var e in nr.Raiders) if (e != null && !seen.ContainsKey(e)) seen[e] = Time.time;
  if (now >= stT) { L("AFTER 8s: newSpawns=" + (seen.Count - before) + " raiders=" + nr.Raiders.Count + " alive=" + nr.Alive + " active=" + nr.IsActive + " isDay=" + tc.isDaytime);
   new SROptions().Day(); tc.AddTime(tc.totalTime * 0.3f); st = 4; stT = now + 4; } return; }
 if (st == 4 && now >= stT) { L("DAY? isDay=" + tc.isDaytime + " active=" + nr.IsActive + " raiders=" + nr.Raiders.Count); st = 3; }
 if (st == 3) {
  var rbp = pc.GetComponent<Rigidbody2D>(); if (rbp) rbp.bodyType = RigidbodyType2D.Dynamic;
  System.IO.Directory.CreateDirectory("Logs/nr_full"); System.IO.File.WriteAllText("Logs/nr_full/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/nr_full"); System.IO.File.WriteAllText("Logs/nr_full/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
