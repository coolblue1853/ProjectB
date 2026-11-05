// FJ-6: 석궁 탄수·마도서 장 HUD 플레이 확인(+투창 HUD). 장착별 HudText 전후 값 → Logs/fj6/result.txt. 플레이 모드에서 실행, 세이브는 호출자가 백업/복원
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance;
var sb = new System.Text.StringBuilder();
sb.AppendLine("Loc=" + Loc.Current);
string[] keys = { "Reloading {0}s", "Ammo {0}/{1}", "Spear held", "Spear in flight", "Spear planted (trap)", "Spear planted", "Fire", "Frost", "Lightning", "Page: {0}" };
foreach (var k in keys) sb.AppendLine("LOC '" + k + "' -> '" + Loc.T(k) + "'");
System.Action<string> eq = (name) => {
  if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
  foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
    string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
    if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
    old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
  var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
  box.equipPrefab = null; var d = db.LoadItemData(db.FindItemDataIndex(name)); box.LoadPrefab(name, "Weapon", d.tfName); box.ActivePrefab("Weapon"); };
var steps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<float, System.Action>>();
System.Action<float, System.Action> add = (dt, a) => steps.Add(new System.Collections.Generic.KeyValuePair<float, System.Action>(dt, a));
add(0f, () => { eq("SentryCrossbow"); });
add(1.5f, () => { var w = Weapon.Equipped; var g = w != null ? w.GetComponent<AmmoGimmick>() : null; sb.AppendLine("CROSSBOW equipped=" + (w ? w.name : "null") + " gimmick=" + (g != null) + " hud=" + (g != null ? g.HudText : "-")); });
for (int i = 1; i <= 8; i++) { int n = i; add(0.8f, () => { var w = Weapon.Equipped; var g = w.GetComponent<AmmoGimmick>(); string b = g.HudText; w.MeleeAttack(); sb.AppendLine("CROSSBOW shot" + n + " before='" + b + "' after='" + g.HudText + "'"); }); }
add(0.5f, () => { var g = Weapon.Equipped.GetComponent<AmmoGimmick>(); sb.AppendLine("CROSSBOW later='" + g.HudText + "'"); });
add(3.5f, () => { var g = Weapon.Equipped.GetComponent<AmmoGimmick>(); sb.AppendLine("CROSSBOW +3.5s='" + g.HudText + "'"); });
add(0.1f, () => { eq("FairysGrimoire"); });
add(1.5f, () => { var w = Weapon.Equipped; var g = w != null ? w.GetComponent<TomePageGimmick>() : null; sb.AppendLine("GRIMOIRE equipped=" + (w ? w.name : "null") + " gimmick=" + (g != null) + " hud=" + (g != null ? g.HudText : "-")); });
for (int i = 1; i <= 3; i++) { int n = i; add(i == 1 ? 0.2f : 3.6f, () => { var g = Weapon.Equipped.GetComponent<TomePageGimmick>(); string b = g.HudText; bool ok = g.Tome.TrySwitch(); sb.AppendLine("GRIMOIRE switch" + n + " ok=" + ok + " before='" + b + "' after='" + g.HudText + "'"); }); }
add(0.2f, () => { var g = Weapon.Equipped.GetComponent<TomePageGimmick>(); bool ok = g.Tome.TrySwitch(); sb.AppendLine("GRIMOIRE switch-in-cooldown ok=" + ok + " hud='" + g.HudText + "'"); });
add(0.1f, () => { eq("HuntersJavelin"); });
add(1.5f, () => { var w = Weapon.Equipped; var g = w != null ? w.GetComponent<ThrownSpearGimmick>() : null; sb.AppendLine("JAVELIN equipped=" + (w ? w.name : "null") + " gimmick=" + (g != null) + " hud=" + (g != null ? g.HudText : "-")); });
add(0.1f, () => { var w = Weapon.Equipped; var g = w.GetComponent<ThrownSpearGimmick>(); string b = g.HudText; w.MeleeAttack(); sb.AppendLine("JAVELIN throw before='" + b + "' after='" + g.HudText + "'"); });
add(1.0f, () => { var g = Weapon.Equipped.GetComponent<ThrownSpearGimmick>(); sb.AppendLine("JAVELIN +1s='" + g.HudText + "'"); });
add(5.5f, () => { var g = Weapon.Equipped.GetComponent<ThrownSpearGimmick>(); sb.AppendLine("JAVELIN +6.5s='" + g.HudText + "'"); });
int si = 0; float due = Time.time; float guard = Time.time + 120f;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
  if (si < steps.Count) { if (Time.time < due) return; var st = steps[si++]; st.Value(); due = Time.time + (si < steps.Count ? steps[si].Key : 0f); return; }
  System.IO.Directory.CreateDirectory("Logs/fj6"); System.IO.File.WriteAllText("Logs/fj6/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/fj6"); System.IO.File.WriteAllText("Logs/fj6/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
