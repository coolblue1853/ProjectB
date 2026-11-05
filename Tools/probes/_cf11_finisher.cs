// AUTO-125: 마무리 타 StopFinisher 0.10 경계 — 기본 칼(RustedSword) nowConboCount 1/2/3 + 스킬/치명 조합으로 HitFeedback.OnEnemyHit 호출 후 적 Animator.speed==0 지속 시간 측정. 결과 Logs/cf11_finisher/result.txt
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>(); var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var db = DatabaseManager.instance; var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder();
if (am != null && am.equipWeapon != null) { am.UnEquipMainWeaopon(); am.equipWeapon = null; }
foreach (var old in UnityEngine.Object.FindObjectsByType<Weapon>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  string on = old.name.Replace("(Clone)", "").Trim(); int ix = db.FindItemDataIndex(on);
  if (ix >= 0) { var od = db.LoadItemData(ix); if (od != null && !string.IsNullOrEmpty(od.tfName)) DatabaseManager.MinusSetDict(od.tfName, 1); }
  old.gameObject.SetActive(false); UnityEngine.Object.Destroy(old.gameObject); }
var wf = typeof(EquipBoxCheck).GetField("weapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if (wf != null) wf.SetValue(box, null);
box.equipPrefab = null; var d0 = db.LoadItemData(db.FindItemDataIndex("RustedSword")); box.LoadPrefab("RustedSword", "Weapon", d0.tfName); box.ActivePrefab("Weapon");
bool go = MapInfo.GoToMap("Lv1_NormalForest", pc.gameObject); sb.AppendLine("goto=" + go);
var steps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<float, System.Action>>();
System.Action<float, System.Action> add = (dt, a) => steps.Add(new System.Collections.Generic.KeyValuePair<float, System.Action>(dt, a));
EnemyHealth tgt = null; SpriteHitFx fx = null; Animator anim = null; Weapon w = null;
string curLabel = null; float curStart = -1f, curZeroStart = -1f, curEnd = -1f, expect = 0f; bool wasZero = false; int frames = 0;
var rows = new System.Collections.Generic.List<string>();
add(4f, () => { var sps = UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None); if (sps.Length > 0) { var sp = sps[sps.Length / 2]; pc.transform.position = sp.transform.position + Vector3.up * 2f; sb.AppendLine("tp to spawner " + sp.name + " of " + sps.Length + " day=" + TimeChange.instance.isDaytime); } });
add(10f, () => { w = Weapon.Equipped; sb.AppendLine("equipped=" + (w ? w.name : "null") + " anims=" + (w != null && w.attackAnimName != null ? w.attackAnimName.Length : -1) + " maxCombo=" + (w ? w.maxComboCount : -1));
  var home = pc.transform.position; float best = 1e9f; foreach (var e in new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active)) { if (e != null && e.IsMonster && e.GetComponent<Animator>() != null && e.GetComponent<SpriteHitFx>() != null) { float d = Vector2.Distance(e.transform.position, home); if (d < best) { best = d; tgt = e; } } }
  sb.AppendLine("active=" + EnemyHealth.Active.Count + " target=" + (tgt ? tgt.name : "NONE")); if (tgt != null) { anim = tgt.GetComponent<Animator>(); sb.AppendLine("tgtActive=" + tgt.isActiveAndEnabled + " boss=" + tgt.isBossMob + " fx=" + (tgt.GetComponent<SpriteHitFx>() != null)); tgt.maxHP = 1000000; tgt.nowHP = 1000000; } });
System.Action<string, int, bool, bool, float> hit = (label, step, crit, skill, exp) => {
  w.nowConboCount = step; curLabel = label; expect = exp; curZeroStart = -1f; curEnd = -1f; wasZero = false; frames = 0;
  curStart = Time.unscaledTime; HitFeedback.OnEnemyHit(tgt, crit, skill, false); bool zeroNow = anim.speed == 0f; if (zeroNow) { curZeroStart = Time.unscaledTime; wasZero = true; }
  sb.AppendLine(label + " step=" + step + "/" + w.attackAnimName.Length + " crit=" + crit + " skill=" + skill + " speedAfterCall=" + anim.speed + " expect=" + exp.ToString("0.00")); };
System.Action fin = () => { if (curLabel == null) return; float dur = curEnd > 0f && curZeroStart > 0f ? curEnd - curZeroStart : -1f; sb.AppendLine("  -> " + curLabel + " measured=" + dur.ToString("0.000") + " expect=" + expect.ToString("0.00") + " frames=" + frames + " restoredSpeed=" + anim.speed); curLabel = null; };
float[] gaps = { 0.6f };
add(0.2f, () => hit("T1 basic", 1, false, false, 0.07f)); add(0.7f, () => { fin(); hit("T2 basic", 2, false, false, 0.07f); });
add(0.7f, () => { fin(); hit("T3 finisher", 3, false, false, 0.10f); });
add(0.7f, () => { fin(); hit("T3 finisher+crit", 3, true, false, 0.13f); });
add(0.7f, () => { fin(); hit("T1 basic+crit", 1, true, false, 0.10f); });
add(0.7f, () => { fin(); hit("T3 skill", 3, false, true, 0.07f); });
add(0.7f, () => { fin(); hit("T3 skill+crit", 3, true, true, 0.10f); });
add(0.7f, () => { fin(); w.nowConboCount = 0; if (tgt != null) { tgt.maxHP = 100; tgt.nowHP = 100; } });
int si = 0; float due = Time.time;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { try {
  if (curLabel != null && anim != null) { frames++; bool z = anim.speed == 0f; if (z && !wasZero) { wasZero = true; if (curZeroStart < 0f) curZeroStart = Time.unscaledTime; } if (!z && wasZero && curEnd < 0f) curEnd = Time.unscaledTime; }
  if (si < steps.Count) { if (Time.time < due) return; var st = steps[si++]; st.Value(); due = Time.time + (si < steps.Count ? steps[si].Key : 0f); return; }
  System.IO.Directory.CreateDirectory("Logs/cf11_finisher"); System.IO.File.WriteAllText("Logs/cf11_finisher/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd;
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/cf11_finisher"); System.IO.File.WriteAllText("Logs/cf11_finisher/result.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
