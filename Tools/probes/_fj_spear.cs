// AUTO-94/110(함정 생성·소멸·상한 3 추가): 투창 착지 판정 — 바닥(열린 지면)/벽(사거리 안)/허공(공중)/사거리 밖 벽 4경우. ClearDistance·_flight·실제 착지 시각 → Logs/fj_spear/result.txt. 플레이 모드, 세이브는 호출자가 백업/복원
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
var steps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<float, System.Action>>();
System.Action<float, System.Action> add = (dt, a) => steps.Add(new System.Collections.Generic.KeyValuePair<float, System.Action>(dt, a));
Vector3 home = Vector3.zero; Vector3 wallPos = Vector3.zero, farPos = Vector3.zero, openPos = Vector3.zero; bool hasWall = false, hasFar = false, hasOpen = false;
float throwT = 0f, landT = -1f; bool tracking = false; ThrownSpearGimmick tg = null;
System.Func<ThrownSpearGimmick, float> clearOf = (g) => (float)typeof(ThrownSpearGimmick).GetMethod("ClearDistance", bf).Invoke(g, null);
add(2.5f, () => { // 장착 확인 + 스캔
  var w = Weapon.Equipped; tg = w != null ? w.GetComponent<ThrownSpearGimmick>() : null; sb.AppendLine("equipped=" + (w ? w.name : "null") + " gimmick=" + (tg != null));
  home = pc.transform.position; sb.AppendLine("home=" + home + " scaleX=" + tg.transform.root.localScale.x + " root=" + tg.transform.root.name + " range=" + (float)typeof(ThrownSpearGimmick).GetField("_throwRange", bf).GetValue(tg) + " speed=" + (float)typeof(ThrownSpearGimmick).GetField("_throwSpeed", bf).GetValue(tg));
  var root = tg.transform.root; var rb0 = root.GetComponent<Rigidbody2D>(); if (rb0 != null) { rb0.bodyType = RigidbodyType2D.Kinematic; rb0.linearVelocity = Vector2.zero; } Vector3 p0 = root.position; int cnt = 0; string scan = "";
  for (int dx = -60; dx <= 60; dx += 1) {
    Vector3 p = new Vector3(home.x + dx, home.y + 3f, home.z); float gy = GroundProbe.Y(p); if (float.IsNaN(gy) || gy < -500f) continue;
    p.y = gy + 0.6f; root.position = p; Physics2D.SyncTransforms(); float c = clearOf(tg); bool hit = c < 5.99f || c > 6.01f; // range 와 같으면 막힘 없음
    if (c < 5.99f && c > 2f && !hasWall) { hasWall = true; wallPos = p; var hh = Physics2D.Raycast(p, root.localScale.x < 0f ? Vector2.left : Vector2.right, 6f, LayerMask.GetMask("Ground", "Wall")); scan += " wall@" + p.x.ToString("0.0") + ":" + c.ToString("0.00") + "(" + (hh.collider != null ? hh.collider.name : "?") + ")"; }
    if (!hit && !hasOpen) { hasOpen = true; openPos = p; scan += " open@" + p.x.ToString("0.0"); }
    cnt++; }
  root.position = p0; sb.AppendLine("scan positions=" + cnt + scan); });
// 사거리 밖 벽: 왼쪽 방향으로 재스캔은 생략 — 임시 벽 콜라이더를 만들어 확인
add(0.1f, () => { var root = tg.transform.root; float dir = root.localScale.x < 0f ? -1f : 1f; Vector3 p = openPos != Vector3.zero ? openPos : home;
  int wl = LayerMask.NameToLayer("Wall"); var wallGo = new GameObject("TmpFarWall"); wallGo.layer = wl; wallGo.name = "TmpFarWall"; wallGo.transform.position = p + new Vector3(dir * 9f, 0f, 0f);
  var bc = wallGo.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1f, 6f); Physics2D.SyncTransforms(); farPos = p; hasFar = true; sb.AppendLine("tmp far wall at +9 (Physics2D.SyncTransforms)  (layer Wall=" + wl + ") from " + p); });
System.Action<string, Vector3, bool> cas = (label, pos, air) => {
  var root = tg.transform.root; root.position = pos + (air ? Vector3.up * 4f : Vector3.zero); Physics2D.SyncTransforms();
  var rb = root.GetComponent<Rigidbody2D>(); if (rb != null && air) rb.linearVelocity = Vector2.zero;
  float clear = clearOf(tg); var w = Weapon.Equipped; string before = tg.HudText; w.MeleeAttack();
  float fl = (float)typeof(ThrownSpearGimmick).GetField("_flight", bf).GetValue(tg); float expect = ThrownSpear.FlightTime(clear, 6f, 18f);
  float dir = root.localScale.x < 0f ? -1f : 1f; throwT = Time.time; landT = -1f; tracking = true;
  sb.AppendLine(label + " pos=" + root.position.ToString("0.00") + " clear=" + clear.ToString("0.000") + " expectFlight=" + expect.ToString("0.000") + " _flight=" + fl.ToString("0.000") + " landingPoint~" + (root.position + new Vector3(dir * Mathf.Min(clear, 6f), 0f, 0f)).ToString("0.00") + " hud " + before + "->" + tg.HudText); };
System.Action<string> res = (label) => { sb.AppendLine(label + " landDt=" + (landT < 0 ? "none" : (landT - throwT).ToString("0.000")) + " state=" + tg.Spear.Current + " trap=" + tg.Spear.TrapActive + " pos(now)=" + tg.transform.root.position.ToString("0.00") + " | traps live=" + SpearTrap.LiveCount + " " + string.Join(",", System.Linq.Enumerable.Select(UnityEngine.Object.FindObjectsByType<SpearTrap>(FindObjectsSortMode.None), t => t.transform.position.ToString("0.0") + "@expire+" + (t.ExpireTime - Time.time).ToString("0.0")))); tracking = false; tg.Retrieve(); };
add(0.1f, () => { cas("CASE1 ground-open", hasOpen ? openPos : home, false); });
add(1.2f, () => res("CASE1"));
add(0.3f, () => { if (hasWall) cas("CASE2 wall-in-range", wallPos, false); else sb.AppendLine("CASE2 skipped (no wall found)"); });
add(1.2f, () => { if (hasWall) res("CASE2"); });
add(0.3f, () => { cas("CASE3 air", hasOpen ? openPos : home, true); });
add(1.2f, () => res("CASE3"));
add(0.3f, () => { if (hasFar) cas("CASE4 wall-beyond-range(9)", farPos, false); });
add(1.2f, () => { if (hasFar) res("CASE4"); var f = GameObject.Find("TmpFarWall"); if (f != null) UnityEngine.Object.Destroy(f); tg.transform.root.position = home; var rbe = tg.transform.root.GetComponent<Rigidbody2D>(); if (rbe != null) rbe.bodyType = RigidbodyType2D.Dynamic; });
add(5f, () => { sb.AppendLine("after 5s traps live=" + SpearTrap.LiveCount + " activeObjs=" + UnityEngine.Object.FindObjectsByType<SpearTrap>(FindObjectsSortMode.None).Length); });
int si = 0; float due = Time.time;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
  if (tracking && landT < 0f && tg != null && tg.Spear.Current != ThrownSpear.State.InFlight) landT = Time.time;
  if (si < steps.Count) { if (Time.time < due) return; var st = steps[si++]; st.Value(); due = Time.time + (si < steps.Count ? steps[si].Key : 0f); return; }
  System.IO.Directory.CreateDirectory("Logs/fj_spear"); System.IO.File.WriteAllText("Logs/fj_spear/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/fj_spear"); System.IO.File.WriteAllText("Logs/fj_spear/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
