// AUTO-97: CF-12 _moveCancelFraction 실플레이 측정 — 무기별 3타 각각 공격 시작→CanMoveCancelNow 가 참이 되기까지 지연(초)·후딜 총길이·hitDelay → Logs/cf12/result.txt. 값 조정 없음. 플레이 모드, 세이브는 호출자가 백업/복원
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance; var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder(); var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
PlayerHealthManager.Instance.fullHP = 99999; PlayerHealthManager.Instance.nowHp = 99999;
MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
System.Action<string> eq = (name) => {
  if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
  foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
    string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
    if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
    old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
  var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
  box.equipPrefab = null; var d = db.LoadItemData(db.FindItemDataIndex(name)); box.LoadPrefab(name, "Weapon", d.tfName); box.ActivePrefab("Weapon"); };
string[] names = { "RustedSword", "DoubleDagger", "HeartwoodGreatsword", "QuakestoneMaul" };
int wi = -1, step = 0; int phase = 0; double due = UnityEditor.EditorApplication.timeSinceStartup + 2.5; float tAtk = 0f; float tOpen = -1f; Weapon w = null; float frac = 0f; int tries = 0;
System.Func<string, object> fld = n => w.GetType().GetField(n, BF).GetValue(w);
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
 double now = UnityEditor.EditorApplication.timeSinceStartup; PlayerHealthManager.Instance.nowHp = 99999;
 if (phase == 0 && now >= due) { // 다음 무기 장착
  wi++; if (wi >= names.Length) { phase = 9; return; }
  eq(names[wi]); phase = 1; due = now + 2.0; step = 0; return; }
 if (phase == 1 && now >= due) {
  w = Weapon.Equipped; if (w == null) { sb.AppendLine(names[wi] + " not equipped"); phase = 0; return; }
  frac = (float)fld("_moveCancelFraction");
  sb.AppendLine("WEAPON " + names[wi] + " equipped=" + w.name + " _moveCancelFraction=" + frac.ToString("0.00") + " floorRatio=" + MovementUnlockRules.CancelStartRatio(MovementUnlocks.Instance != null ? MovementUnlocks.Instance.HighestFloor : 1).ToString("0.00") + " stopMove=" + fld("isWeaponStopMove"));
  phase = 2; return; }
 if (phase == 2) { // 공격 시작: 후딜(대기)이 끝나 isAttackWait 일 때만
  if (!(bool)fld("isAttackWait")) return;
  tAtk = Time.time; w.MeleeAttack(); tOpen = -1f; tries = 0; phase = 3; return; }
 if (phase == 3) {
  bool started = (float)fld("_attackWaitStartedAt") >= tAtk - 0.01f;
  if (!started) { if (++tries > 120) { sb.AppendLine("  step" + (step + 1) + " 공격이 시작되지 않음"); phase = 0; } return; }
  if (tOpen < 0f && w.CanMoveCancelNow) tOpen = Time.time - tAtk;
  float total = (float)fld("_attackWaitTotal"); float hit = (float)fld("_attackHitDelay");
  if (Time.time - tAtk > total + 0.05f) {
   sb.AppendLine("  step" + (step + 1) + " conbo=" + fld("nowConboCount") + " total=" + total.ToString("0.000") + " hitDelay=" + hit.ToString("0.000") + " cancelOpenAfter=" + (tOpen < 0f ? "never" : tOpen.ToString("0.000")) + " ratio=" + (tOpen < 0f ? "-" : (tOpen / total).ToString("0.00")));
   step++; if (step >= 3) { phase = 0; due = now + 1.5; } else { phase = 2; } } return; }
 if (phase == 9) { System.IO.Directory.CreateDirectory("Logs/cf12"); System.IO.File.WriteAllText("Logs/cf12/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/cf12"); System.IO.File.WriteAllText("Logs/cf12/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
