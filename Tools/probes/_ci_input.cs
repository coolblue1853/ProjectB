// CI-P5·P6 프로브 E·F·H: 가상 패드. E 스틱 복귀 직후 X(0.05초 뒤→스틱 방향, 0.12초 뒤→현재 방향), F X 홀드 연타(콤보 마지막 타 뒤 중단), H A+X 같은 프레임(점프 먼저) → Logs/ci/input.txt. 플레이 모드·input_bg_on 선행, ES3 백업은 호출자
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance; var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder(); var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
PlayerHealthManager.Instance.fullHP = 99999; PlayerHealthManager.Instance.nowHp = 99999;
MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
var pad = UnityEngine.InputSystem.Gamepad.current; bool created = pad == null; if (created) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
System.Action<float, bool, bool> set = (x, west, south) => { var s = new UnityEngine.InputSystem.LowLevel.GamepadState(); s.leftStick = new Vector2(x, 0f); if (west) s = s.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.West); if (south) s = s.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.South); UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, s); };
System.Action<string> eq = (name) => {
  if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
  foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
    string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
    if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
    old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
  var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
  box.equipPrefab = null; var d = db.LoadItemData(db.FindItemDataIndex(name)); box.LoadPrefab(name, "Weapon", d.tfName); box.ActivePrefab("Weapon"); };
System.Action<float> face = (sgn) => { var ls = pc.transform.localScale; pc.transform.localScale = new Vector3(sgn * Mathf.Abs(ls.x), ls.y, ls.z); };
Weapon w = null; float lastStart = -99f; int starts = 0;
System.Func<string, object> fld = n => w.GetType().GetField(n, BF).GetValue(w);
// 시나리오 = (대기 초, 동작) 목록. 매 프레임 시작 횟수 집계
var steps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<float, System.Action>>();
System.Action<float, System.Action> add = (t, a) => steps.Add(new System.Collections.Generic.KeyValuePair<float, System.Action>(t, a));
float yStart = 0f, yAtAtk = -99f; bool watchY = false;
add(3f, () => { eq("PoisonSword"); });
add(2.5f, () => { w = Weapon.Equipped; foreach (var en in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { en.gameObject.SetActive(false); } sb.AppendLine("weapon=" + (w != null ? w.name : "null") + " max=" + (w != null ? w.maxComboCount : -1)); face(-1f); set(1f, false, false); });
add(0.05f, () => set(0f, false, false)); // 스틱 복귀
add(0.05f, () => { face(-1f); set(0f, true, false); });    // 0.05초 뒤 X(걸어서 돌아선 방향을 왼쪽으로 되돌려 공격 시작 보정만 보이게)
add(0.15f, () => { sb.AppendLine("E1 스틱→중립 0.05초 뒤 X: facing=" + (pc.transform.localScale.x > 0 ? "오른쪽(기대)" : "왼쪽(실패)")); set(0f, false, false); });
add(2.0f, () => { face(-1f); set(1f, false, false); });
add(0.05f, () => set(0f, false, false));
add(0.12f, () => { face(-1f); set(0f, true, false); });    // 0.12초 뒤 X
add(0.15f, () => { sb.AppendLine("E2 스틱→중립 0.12초 뒤 X: facing=" + (pc.transform.localScale.x > 0 ? "오른쪽(실패)" : "왼쪽(기대)")); set(0f, false, false); });
add(2.5f, () => { starts = 0; set(0f, true, false); });                       // F: X 홀드
add(2.0f, () => { sb.AppendLine("F1 X 2초 홀드 시작 횟수=" + starts + " (기대 = max " + w.maxComboCount + ")"); set(0f, false, false); });
add(1.0f, () => { starts = 0; set(0f, true, false); });
add(0.1f, () => set(0f, false, false));
add(1.0f, () => { sb.AppendLine("F2 놓았다 새로 눌러 탭 1회 시작 횟수=" + starts + " (기대 1)"); });
add(1.5f, () => { yStart = pc.transform.position.y; yAtAtk = -99f; watchY = true; set(0f, true, true); }); // H: A+X 같은 프레임
add(0.1f, () => set(0f, false, false));
add(0.6f, () => { watchY = false; sb.AppendLine("H A+X 동시: y0=" + yStart.ToString("0.00") + " 공격 시작 시 vy=" + yAtAtk.ToString("0.00") + " (기대 vy>0 = 점프 먼저, 공중 평타)"); });
int si = 0; double due = Time.time + steps[0].Key; int lastFrame = -1;
System.Action<string> flush = (t) => { System.IO.Directory.CreateDirectory("Logs/ci"); System.IO.File.WriteAllText("Logs/ci/input.txt", t); };
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
  PlayerHealthManager.Instance.nowHp = 99999; if (Time.frameCount == lastFrame) return; lastFrame = Time.frameCount;
  if (w != null) { float st = (float)fld("_attackWaitStartedAt"); if (st != lastStart) { lastStart = st; starts++; if (watchY && yAtAtk < -90f) yAtAtk = pc.GetComponent<Rigidbody2D>().linearVelocity.y; } if (false) { sb.AppendLine("  f" + Time.frameCount + " y=" + pc.transform.position.y.ToString("0.000") + " defer=" + pc.DeferAttackForJump() + " starts=" + starts + " vy=" + pc.GetComponent<Rigidbody2D>().linearVelocity.y.ToString("0.00")); } }
  double now = Time.time;
  if (si < steps.Count) { if (now < due) return; steps[si].Value(); si++; if (si < steps.Count) due = now + steps[si].Key; return; }
  set(0f, false, false); if (created) UnityEngine.InputSystem.InputSystem.RemoveDevice(pad); flush(sb.ToString()); UnityEditor.EditorApplication.update -= upd;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); set(0f, false, false); flush(sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
