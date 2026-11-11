// REWORK-12 P4: Chain 전이·OnKillCast·Proc·CooldownRefund 확인. 로그 AppDomain "efx4"
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("efx4", log);
var pc = PlayerController.instance;
var db = DatabaseManager.instance;
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab");
var list = new System.Collections.Generic.List<EnemyHealth>();
System.Func<float, int, EnemyHealth> spawn = (dx, hp) => {
    float gy = GroundProbe.Y(pc.transform.position + Vector3.right * dx);
    var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + dx, gy + 0.6f, 0f), Quaternion.identity);
    var e = go.GetComponent<EnemyHealth>(); e.maxHP = hp; e.nowHP = hp; list.Add(e);
    return e;
};
System.Action clear = () => { foreach (var e in list) if (e != null) UnityEngine.Object.Destroy(e.gameObject); list.Clear(); };
System.Func<int> freeCount = () => { int n = 0; foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) n++; return n; };
System.Action cast = () => {
    var w = Weapon.Equipped;
    PlayerHealthManager.Instance.nowStemina = 999;
    w.skill[0].skillCooldown.ResetCoolTime();
    w.skill[0].isButtonDownSkill = true;
    try { w.ActiveWeaponSkill(0); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); }
};
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => {
    var d = db.LoadItemData(db.FindItemDataIndex("LeafAdae"));
    box.LoadPrefab("LeafAdae", "Weapon", d.tfName);
    amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
}, false);
// 1) Chain: +4 +7 +9 +11 에 적(HP 100000). 첫 탄이 +4 를 맞히면 +7(3칸)·+9(5칸) 에 감쇠 피해, +11 은 범위 밖
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => {
    clear(); SkillEffectService.Detach("Dev:Rule"); SkillEffectService.AttachIds("Dev:Rule", "EF_DEV_Chain", "R");
    spawn(4f, 100000); spawn(7f, 100000); spawn(9f, 100000); spawn(11f, 100000);
    cast();
}, false);
DG.Tweening.DOVirtual.DelayedCall(2.2f, () => {
    log.Append("## Chain hp loss: ");
    foreach (var e in list) log.Append((100000 - e.nowHP) + " ");
    log.Append("\n");
    clear();
    // 2) Proc 확정(100%, 내부쿨1s): 시전 직후 판정 수 비교
    SkillEffectService.Detach("Dev:Rule"); SkillEffectService.AttachIds("Dev:Rule", "EF_DEV_Proc", "R");
    foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
}, false);
DG.Tweening.DOVirtual.DelayedCall(2.4f, () => { log.Append("## Proc before=" + freeCount() + " "); cast(); }, false);
DG.Tweening.DOVirtual.DelayedCall(2.45f, () => { log.Append("after(탄수 기본+Proc)=" + freeCount() + "\n"); }, false);
// 3) OnKillCast: 적 2마리 HP1, 0.1초 간격 처치 → 첫 처치만 발동(내부쿨 1s). 판정 수 증가
DG.Tweening.DOVirtual.DelayedCall(3.5f, () => {
    foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
    SkillEffectService.Detach("Dev:Rule"); SkillEffectService.AttachIds("Dev:Rule", "EF_DEV_KillCast", "R");
    var a = spawn(3f, 1); var b = spawn(3.5f, 1);
    log.Append("## KillCast before=" + freeCount() + " ");
    a.damage2Enemy(new int[] { 5, 5, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, true, 0f, 1f);
}, false);
DG.Tweening.DOVirtual.DelayedCall(3.6f, () => {
    log.Append("afterKill1=" + freeCount() + " ");
    list[1].damage2Enemy(new int[] { 5, 5, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, true, 0f, 1f);
}, false);
DG.Tweening.DOVirtual.DelayedCall(3.7f, () => { log.Append("afterKill2(내부쿨 중이면 동일)=" + freeCount() + "\n"); }, false);
// 4) CooldownRefund(Kill 2초): 시전 후 쿨 남음 → 처치 → 남음 감소
DG.Tweening.DOVirtual.DelayedCall(5.0f, () => {
    foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
    clear();
    SkillEffectService.Detach("Dev:Rule"); SkillEffectService.AttachIds("Dev:Rule", "EF_DEV_RefundKill", "R");
    var w = Weapon.Equipped; var s0 = w.skill[0];
    spawn(3f, 1);
    cast();
}, false);
DG.Tweening.DOVirtual.DelayedCall(5.3f, () => {
    var w = Weapon.Equipped; int slot = w.skill[0].ActiveSlot;
    log.Append("## Refund slot=" + slot + " remainBefore=" + SkillCooldown.instance.Remaining(slot).ToString("F2"));
    if (list.Count > 0 && list[0] != null) list[0].damage2Enemy(new int[] { 5, 5, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, true, 0f, 1f);
    log.Append(" after=" + SkillCooldown.instance.Remaining(slot).ToString("F2") + "\n");
}, false);
DG.Tweening.DOVirtual.DelayedCall(6.0f, () => { SkillEffectService.Detach("Dev:Rule"); clear(); log.Append("DONE active=" + SkillEffectService.ActiveCount); }, false);
return "scheduled";
