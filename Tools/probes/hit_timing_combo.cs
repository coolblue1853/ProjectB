// 녹슨 검 3타 콤보 — 가장 가까운 몹 앞에서 평타 가능해질 때마다 눌러 3번, 각 타의 시작 → HP 감소 시각(초)과 단계 기록. 먼저 to_spawner.cs. 결과 AppDomain pb_hit
var inv = InventoryManager.instance; var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>(); var pc = PlayerController.instance;
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude)) { if (e.nowHP <= 0 || e.isBossMob || !e.IsMonster) continue; float dd = Vector2.Distance(e.transform.position, pc.transform.position); if (dd < best) { best = dd; t = e; } }
if (t == null) return "no enemy";
pc.transform.position = t.transform.position + new Vector3(-1.1f, 0.2f, 0f);
pc.transform.localScale = new Vector3(Mathf.Abs(pc.transform.localScale.x), pc.transform.localScale.y, 1f);
var log = new System.Text.StringBuilder(); System.AppDomain.CurrentDomain.SetData("pb_hit", log);
float hp = 0f, t0 = 0f; int hits = 0; bool waiting = false;
DG.Tweening.DOVirtual.DelayedCall(0.2f, () => { if (amg.equipWeapon == null && Weapon.Equipped != null) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); } }, true);
for (int k = 0; k < 120; k++)
{
    DG.Tweening.DOVirtual.DelayedCall(0.6f + k * 0.02f, () =>
    {
        if (t == null || Weapon.Equipped == null) return;
        if (waiting && t.nowHP < hp) { log.Append("타" + hits + " hit@" + (Time.unscaledTime - t0).ToString("0.00") + " "); waiting = false; }
        if (waiting && Time.unscaledTime - t0 > 0.6f) { log.Append("타" + hits + " miss "); waiting = false; }
        if (!waiting && hits < 3 && Weapon.Equipped.CanMeleeNow) { pc.transform.position = t.transform.position + new Vector3(-1.1f, 0.2f, 0f); hp = t.nowHP; t0 = Time.unscaledTime; Weapon.Equipped.MeleeAttack(); hits++; waiting = true; log.Append("[" + hits + "단계 " + Weapon.Equipped.nowConboCount + "] "); }
    }, true);
}
return "scheduled vs " + t.name;
