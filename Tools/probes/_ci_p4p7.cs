// CI-P4·P7 프로브 D·G·I + 정예: 무기별 모드/잠금/후딜/연타 간격, 쥐 두목 정예 확인 → Logs/ci/p4p7.txt. 플레이 모드·input_bg_on 선행, ES3 백업은 호출자
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance; var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder(); var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
PlayerHealthManager.Instance.fullHP = 99999; PlayerHealthManager.Instance.nowHp = 99999;
MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
var pad = UnityEngine.InputSystem.Gamepad.current; bool created = pad == null; if (created) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
System.Action<bool> hold = (west) => { var s = new UnityEngine.InputSystem.LowLevel.GamepadState(); if (west) s = s.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.West); UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, s); };
System.Action<string> eq = (name) => {
  if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
  foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
    string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
    if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
    old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
  var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
  box.equipPrefab = null; var d = db.LoadItemData(db.FindItemDataIndex(name)); box.LoadPrefab(name, "Weapon", d.tfName); box.ActivePrefab("Weapon"); };
string[] names = { "PoisonSword", "RustedSword", "DoubleDagger", "SacreGreatSword", "Wood Spear", "CrocJawAxe", "FrogTongueWhip", "LeafStaff", "Leather Knuckles", "ScareSide", "ScareClow", "LeafAdae" };
int wi = -1, phase = 0, curStep = 0; double due = UnityEditor.EditorApplication.timeSinceStartup + 3.0; Weapon w = null; float holdEnd = 0f, lastSt = -99f; var recs = new System.Collections.Generic.List<string>(); bool lockSeen = false; float prevStart = -99f, prevTotal = 0f;
System.Func<string, object> fld = nm => w.GetType().GetField(nm, BF).GetValue(w);
UnityEditor.EditorApplication.CallbackFunction upd = null;
System.Action<string> flush = (t) => { System.IO.Directory.CreateDirectory("Logs/ci"); System.IO.File.WriteAllText("Logs/ci/p4p7.txt", t); };
upd = () => { try {
 double now = UnityEditor.EditorApplication.timeSinceStartup; PlayerHealthManager.Instance.nowHp = 99999;
 if (phase == 0 && now >= due) { wi++; if (wi >= names.Length) { phase = 9; return; } eq(names[wi]); phase = 1; due = now + 2.5; return; }
 if (phase == 1 && now >= due) {
  foreach (var en in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { en.gameObject.SetActive(false); }
  w = Weapon.Equipped; if (w == null) { sb.AppendLine(names[wi] + " not equipped"); phase = 0; due = now + 1; return; }
  var prof = WeaponMoveProfiles.ActiveProfileOf(w); string modes = ""; if (prof != null) { for (int s = 1; s <= w.maxComboCount; s++) modes += prof.ModeAt(s) + (s < w.maxComboCount ? "/" : ""); }
  sb.AppendLine("WEAPON " + names[wi] + " kind=" + w.Kind + " stopMovePrefab=" + w.isWeaponStopMove + " max=" + w.maxComboCount + " class=" + (prof != null ? prof.Class.ToString() : "-") + " modes=" + modes + " lockStep1=" + WeaponMoveProfiles.LocksMove(w));
  recs.Clear(); lockSeen = false; lastSt = -99f; holdEnd = Time.time + 2.4f; hold(true); phase = 2; return; }
 if (phase == 2) {
  float st = (float)fld("_attackWaitStartedAt");
  if (st != lastSt && st > 0f) { lastSt = st; float total = (float)fld("_attackWaitTotal"); float gap = prevStart > 0 ? st - prevStart : -1f; recs.Add("step" + fld("nowConboCount") + " t=" + st.ToString("0.00") + " total=" + total.ToString("0.000") + " hit=" + ((float)fld("_attackHitDelay")).ToString("0.000") + (gap >= 0 ? " gap=" + gap.ToString("0.000") + " gap/prevTotal=" + (gap / prevTotal).ToString("0.00") : "")); prevStart = st; prevTotal = total; }
  if (w.OwnsMoveLockInRecovery) lockSeen = true;
  if (Time.time > holdEnd) { hold(false); foreach (var r in recs) sb.AppendLine("  " + r); sb.AppendLine("  ownsLockInRecovery=" + lockSeen + " attacks=" + recs.Count); prevStart = -99f; phase = 0; due = now + 1.5; } return; }
 if (phase == 9) { hold(false); if (created) UnityEngine.InputSystem.InputSystem.RemoveDevice(pad); flush(sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { hold(false); sb.AppendLine("ERR " + e); flush(sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
