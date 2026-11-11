// CI-P2·P3 프로브 A·B·C: 무기 타입별 앞키 유지/미유지 3타 X 이동, 캔슬 열림 시각 → Logs/ci/move.txt. 플레이 모드 필요(ES3 백업은 호출자). 가상 패드 스틱 우측 유지(input_bg_on 선행)
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance; var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder(); var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
PlayerHealthManager.Instance.fullHP = 99999; PlayerHealthManager.Instance.nowHp = 99999;
MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
var pad = UnityEngine.InputSystem.Gamepad.current; bool created = pad == null; if (created) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
System.Action<float> stick = (x) => { var s = new UnityEngine.InputSystem.LowLevel.GamepadState(); s.leftStick = new Vector2(x, 0f); UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, s); };
System.Action<string> eq = (name) => {
  if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
  foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
    string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
    if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
    old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
  var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
  box.equipPrefab = null; var d = db.LoadItemData(db.FindItemDataIndex(name)); box.LoadPrefab(name, "Weapon", d.tfName); box.ActivePrefab("Weapon"); };
string[] names = { "PoisonSword", "DoubleDagger", "SacreGreatSword", "RampartSword" };
int ji = -1, wi = 0, step = 0, phase = 0, held = 1; double due = UnityEditor.EditorApplication.timeSinceStartup + 2.5; float tAtk = 0f, x0 = 0f, tOpen = -1f, xMark = 0f, vMax = 0f; bool marked = false; Weapon w = null; int tries = 0;
System.Func<string, object> fld = n => w.GetType().GetField(n, BF).GetValue(w);
UnityEditor.EditorApplication.CallbackFunction upd = null;
System.Action<string> flush = (t) => { System.IO.Directory.CreateDirectory("Logs/ci"); System.IO.File.WriteAllText("Logs/ci/move.txt", t); };
upd = () => { try {
 double now = UnityEditor.EditorApplication.timeSinceStartup; PlayerHealthManager.Instance.nowHp = 99999;
 if (phase == 0 && now >= due) {
  ji++; if (ji >= names.Length * 2) { phase = 9; return; }
  held = ji % 2 == 0 ? 1 : 0; if (ji % 2 == 0) { wi = ji / 2; eq(names[wi]); }
  phase = 1; due = now + 2.0; step = 0; return; }
 if (phase == 1 && now >= due) {
  foreach (var en in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { en.gameObject.SetActive(false); }
  w = Weapon.Equipped; if (w == null) { sb.AppendLine(names[wi] + " not equipped"); phase = 0; due = now + 1; return; }
  sb.AppendLine("WEAPON x=" + pc.transform.position.x.ToString("0.0") + " " + names[wi] + " held=" + held + " kind=" + w.Kind + " stopMove=" + fld("isWeaponStopMove") + " profile=" + (WeaponMoveProfiles.ActiveProfileOf(w) != null));
  phase = 2; return; }
 if (phase == 2) {
  if (!(bool)fld("isAttackWait")) { return; }
  stick(held == 1 ? 1f : 0f); tAtk = Time.time; x0 = pc.transform.position.x; xMark = 0f; marked = false; vMax = 0f; tOpen = -1f; tries = 0; w.MeleeAttack(); phase = 3; return; }
 if (phase == 3) {
  bool started = (float)fld("_attackWaitStartedAt") >= tAtk - 0.01f;
  if (!started) { if (++tries > 120) { sb.AppendLine("  step" + (step + 1) + " not started"); phase = 0; } return; }
  float total = (float)fld("_attackWaitTotal"); float hit = (float)fld("_attackHitDelay"); float el = Time.time - (float)fld("_attackWaitStartedAt");
  float lt = AttackMoveRules.LungeTime(hit);
  vMax = Mathf.Max(vMax, Mathf.Abs(pc.GetComponent<Rigidbody2D>().linearVelocity.x));
  if (!marked && el >= lt + 0.02f) { xMark = pc.transform.position.x - x0; marked = true; }
  if (tOpen < 0f && held == 1 && el > lt + 0.02f && Mathf.Abs(pc.GetComponent<Rigidbody2D>().linearVelocity.x) > 1f && !w.OwnsMoveLockInRecovery) { tOpen = el; }
  if (el > total + 0.05f || tOpen >= 0f) {
   sb.AppendLine("  step" + (step + 1) + " conbo=" + fld("nowConboCount") + " total=" + total.ToString("0.000") + " hit=" + hit.ToString("0.000") + " lungeT=" + lt.ToString("0.000") + " dxAtLungeEnd=" + xMark.ToString("0.00") + " dxEnd=" + (pc.transform.position.x - x0).ToString("0.00") + " vMax=" + vMax.ToString("0.00") + " lockEndedAt=" + (tOpen < 0f ? "never" : tOpen.ToString("0.000")) + " ratio=" + (tOpen < 0f ? "-" : (tOpen / total).ToString("0.00")));
   stick(0f); step++; if (step >= 3) { phase = 0; due = now + 2.0; step = 0; } else { phase = 4; due = now + 0.05; } } return; }
 if (phase == 4 && now >= due) { phase = 2; return; }
 if (phase == 9) { stick(0f); if (created) UnityEngine.InputSystem.InputSystem.RemoveDevice(pad); flush(sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); stick(0f); flush(sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
