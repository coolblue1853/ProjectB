// 녹슨 검 찌르기 1회 — 가장 가까운 몹 바로 앞에서 휘두르고 HP 가 줄어든 시각(공격 시작 기준)을 기록
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var pc = PlayerController.instance;
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude)) { if (e.nowHP <= 0 || e.isBossMob || !e.IsMonster) continue; float dd = Vector2.Distance(e.transform.position, pc.transform.position); if (dd < best) { best = dd; t = e; } }
if (t == null) return "no enemy";
pc.transform.position = t.transform.position + new Vector3(-1.1f, 0.2f, 0f);
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
var log = new System.Text.StringBuilder(); System.AppDomain.CurrentDomain.SetData("pb_hit", log);
float hp0 = 0f, t0 = 0f;
DG.Tweening.DOVirtual.DelayedCall(0.2f, () => { if (amg.equipWeapon == null && Weapon.Equipped != null) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); } }, true);
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => { hp0 = t.nowHP; t0 = Time.unscaledTime; Weapon.Equipped.MeleeAttack(); log.Append("start hp=" + hp0 + " "); }, true);
for (int k = 0; k < 40; k++) { DG.Tweening.DOVirtual.DelayedCall(0.6f + k * 0.02f, () => { if (t != null && t.nowHP < hp0 && !log.ToString().Contains("hit@")) log.Append("hit@" + (Time.unscaledTime - t0).ToString("0.00") + " "); }, true); }
return "scheduled vs " + t.name;
