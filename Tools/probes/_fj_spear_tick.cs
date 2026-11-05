// AUTO-124: 투창 SpearTrap 바닥/벽(거리3)/벽(거리0) 실제 틱 횟수·틱 피해·소멸·상한3. 더미=활성 적 1체(HP 임시 증가, 종료 시 복원). 결과 Logs/fj_spear_tick/result.txt
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance; var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder();
if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
  if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
  old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
box.equipPrefab = null; var d0 = db.LoadItemData(db.FindItemDataIndex("HuntersJavelin")); box.LoadPrefab("HuntersJavelin", "Weapon", d0.tfName); box.ActivePrefab("Weapon");
bool go = MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject); sb.AppendLine("goto=" + go);
var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
EnemyHealth dummy = null; int dmgHp = 0, dmgMax = 0; Rigidbody2D drb = null; RigidbodyType2D drbType = RigidbodyType2D.Dynamic;
Vector3 home = Vector3.zero; bool tracking = false; Vector3 dpos = Vector3.zero; int lastHp = 0; float t0 = 0f; var tickTimes = new System.Collections.Generic.List<float>(); int tickDmg = 0; float avgAtk = 0f; ThrownSpearGimmick tg = null;
var steps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<float, System.Action>>();
System.Action<float, System.Action> add = (dt, a) => steps.Add(new System.Collections.Generic.KeyValuePair<float, System.Action>(dt, a));
System.Func<float> clearOf = () => (float)typeof(ThrownSpearGimmick).GetMethod("ClearDistance", bf).Invoke(tg, null);
GameObject tmpWall = null;
add(2.5f, () => {
  var w = Weapon.Equipped; tg = w != null ? w.GetComponent<ThrownSpearGimmick>() : null; sb.AppendLine("equipped=" + (w ? w.name : "null") + " gimmick=" + (tg != null));
  home = pc.transform.position; var dm = w.damgeArray; avgAtk = (dm[0] + dm[1]) * 0.5f; sb.AppendLine("home=" + home + " dmg=" + dm[0] + "~" + dm[1] + " avg=" + avgAtk + " expectTick=" + Mathf.Max(1, Mathf.RoundToInt(avgAtk * 0.3f)));
  float best = 1e9f; foreach (var e in new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active)) { if (e != null && e.isActiveAndEnabled && !e.isBossMob) { float d = Vector2.Distance(e.transform.position, home); if (d < best) { best = d; dummy = e; } } }
  sb.AppendLine("dummy=" + (dummy ? dummy.name : "NONE"));
  if (dummy != null) { dmgHp = dummy.nowHP; dmgMax = dummy.maxHP; dummy.maxHP = 1000000; dummy.nowHP = 1000000; drb = dummy.GetComponent<Rigidbody2D>(); if (drb != null) { drbType = drb.bodyType; drb.bodyType = RigidbodyType2D.Kinematic; drb.linearVelocity = Vector2.zero; } }
  var rb0 = tg.transform.root.GetComponent<Rigidbody2D>(); });
System.Action<string, float> cas = (label, wallOff) => {
  var root = tg.transform.root; float dir = root.localScale.x < 0f ? -1f : 1f;
  foreach (var t in UnityEngine.Object.FindObjectsByType<SpearTrap>(FindObjectsSortMode.None)) t.gameObject.SetActive(false);
  if (tmpWall != null) UnityEngine.Object.Destroy(tmpWall); tmpWall = null;
  root.position = home; Physics2D.SyncTransforms();
  if (wallOff > -90f) { tmpWall = new GameObject("TmpWall"); tmpWall.layer = LayerMask.NameToLayer("Wall"); tmpWall.transform.position = home + new Vector3(dir * (wallOff + 0.5f), 0f, 0f); var bc = tmpWall.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1f, 8f); Physics2D.SyncTransforms(); }
  float clear = clearOf(); tg.Retrieve(); var w = Weapon.Equipped; w.MeleeAttack(); tickTimes.Clear(); t0 = Time.time;
  Vector3 land = root.position + new Vector3(dir * Mathf.Min(clear, 6f), 0f, 0f); var down = Physics2D.Raycast(land, Vector2.down, 6f, LayerMask.GetMask("Ground", "Wall")); if (down.collider != null) land.y = down.point.y;
  dpos = land + Vector3.up * 0.4f; lastHp = 1000000; dummy.nowHP = 1000000; tickDmg = 0; tracking = true;
  sb.AppendLine(label + " clear=" + clear.ToString("0.000") + " landPoint=" + land.ToString("0.00") + " state=" + tg.Spear.Current); };
System.Action<string> res = (label) => { tracking = false; var f = tickTimes; int sum = 0; string tt = ""; foreach (var x in f) tt += x.ToString("0.00") + " ";
  sb.AppendLine(label + " ticks=" + f.Count + " (expect~8) tickDmg=" + tickDmg + " lastTick@" + (f.Count > 0 ? f[f.Count - 1].ToString("0.00") : "-") + " times: " + tt + "| liveNow=" + SpearTrap.LiveCount);
  tg.Retrieve(); if (tmpWall != null) UnityEngine.Object.Destroy(tmpWall); tmpWall = null; };
add(0.2f, () => cas("CASE_FLOOR", -99f));
add(5.0f, () => res("CASE_FLOOR"));
add(0.3f, () => cas("CASE_WALL3", 3f));
add(5.0f, () => res("CASE_WALL3"));
add(0.3f, () => cas("CASE_WALL0", 0f));
add(5.0f, () => res("CASE_WALL0"));
add(0.3f, () => { tg.transform.root.position = home; foreach (var t in UnityEngine.Object.FindObjectsByType<SpearTrap>(FindObjectsSortMode.None)) t.gameObject.SetActive(false);
  var a = SpearTrap.Spawn(home + Vector3.right * 30f, 1f, 1, 4f, 0.5f); var b = SpearTrap.Spawn(home + Vector3.right * 32f, 1f, 1, 4f, 0.5f); var c = SpearTrap.Spawn(home + Vector3.right * 34f, 1f, 1, 4f, 0.5f);
  sb.AppendLine("CAP live3=" + SpearTrap.LiveCount + " a=" + a.gameObject.activeSelf);
  var d = SpearTrap.Spawn(home + Vector3.right * 36f, 1f, 1, 4f, 0.5f); sb.AppendLine("CAP after 4th live=" + SpearTrap.LiveCount + " oldest(a) active=" + a.gameObject.activeSelf + " b=" + b.gameObject.activeSelf + " c=" + c.gameObject.activeSelf + " d=" + d.gameObject.activeSelf); });
add(4.8f, () => { sb.AppendLine("CAP after 4.8s live=" + SpearTrap.LiveCount + " activeObjs=" + System.Linq.Enumerable.Count(UnityEngine.Object.FindObjectsByType<SpearTrap>(FindObjectsSortMode.None), t => t.gameObject.activeSelf));
  if (dummy != null) { dummy.maxHP = dmgMax; dummy.nowHP = dmgHp; if (drb != null) drb.bodyType = drbType; } });
int si = 0; float due = Time.time;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
  if (tracking && dummy != null) { dummy.transform.position = dpos; if (drb != null) drb.linearVelocity = Vector2.zero;
    int d = lastHp - dummy.nowHP; if (d > 0) { int n = tickDmg > 0 ? Mathf.Max(1, d / tickDmg) : 1; if (tickDmg == 0) tickDmg = d; for (int k = 0; k < n; k++) tickTimes.Add(Time.time - t0); lastHp = dummy.nowHP; } }
  if (si < steps.Count) { if (Time.time < due) return; var st = steps[si++]; st.Value(); due = Time.time + (si < steps.Count ? steps[si].Key : 0f); return; }
  System.IO.Directory.CreateDirectory("Logs/fj_spear_tick"); System.IO.File.WriteAllText("Logs/fj_spear_tick/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/fj_spear_tick"); System.IO.File.WriteAllText("Logs/fj_spear_tick/result.txt", sb.ToString()); if (dummy != null) { dummy.maxHP = dmgMax; dummy.nowHP = dmgHp; if (drb != null) drb.bodyType = drbType; } UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
