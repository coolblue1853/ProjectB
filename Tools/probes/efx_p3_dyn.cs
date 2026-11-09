// REWORK-12 P3: 동적 규칙 확인 — Homing(조준 변화)·Return(속도 반전)·OnHitSpawn(폭발·둔화). 로그 AppDomain "efx3"
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("efx3", log);
var pc = PlayerController.instance;
var db = DatabaseManager.instance;
log.Append("LoadItemData(LeafAdae).effectIds=" + db.LoadItemData(db.FindItemDataIndex("LeafAdae")).effectIds + " BearClaw=" + db.LoadItemData(db.FindItemDataIndex("BearClaw")).effectIds + "\n");
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab");
System.Func<float, float, EnemyHealth> spawn = (dx, dy) => {
    float gy = GroundProbe.Y(pc.transform.position + Vector3.right * dx);
    var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + dx, gy + 0.6f + dy, 0f), Quaternion.identity);
    var e = go.GetComponent<EnemyHealth>(); e.maxHP = 100000; e.nowHP = 100000;
    return e;
};
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => {
    var d = db.LoadItemData(db.FindItemDataIndex("LeafAdae"));
    box.LoadPrefab("LeafAdae", "Weapon", d.tfName);
    amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
}, false);
// Homing: 높은 곳(+7, +5)에 적. 유도 없음/있음 각각 0.7초 뒤 탄 속도 y 평균
System.Func<string, float, string> shot = (id, wait) => "";
float t = 1.0f;
foreach (string id0 in new[] { "", "EF_DEV_Seek" })
{
    string id = id0;
    EnemyHealth[] tgt = new EnemyHealth[1];
    DG.Tweening.DOVirtual.DelayedCall(t, () => {
        tgt[0] = spawn(12f, 6f);
        SkillEffectService.Detach("Dev:Rule"); if (id.Length > 0) SkillEffectService.AttachIds("Dev:Rule", id, "R");
        PlayerHealthManager.Instance.nowStemina = 999;
        var w = Weapon.Equipped; w.skill[0].skillCooldown.ResetCoolTime(); w.skill[0].isButtonDownSkill = true; w.ActiveWeaponSkill(0);
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 0.5f, () => {
        float sy = 0; int n = 0; int above = 0; float cs = 0; var tp = tgt[0].transform.position;
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) { if (string.IsNullOrEmpty(o.skillName)) continue; var v = o.GetComponent<Rigidbody2D>().linearVelocity; sy += v.y; n++; cs += Vector2.Dot(v.normalized, ((Vector2)(tp - o.transform.position)).normalized); if (v.y > 0.5f) above++; }
        log.Append("HOMING id=" + (id.Length == 0 ? "none" : id) + " n=" + n + " avgVy=" + (n > 0 ? (sy / n).ToString("F2") : "-") + " vyPositive=" + above + " meanCosToTarget=" + (n > 0 ? (cs / n).ToString("F2") : "-") + " tgt=" + tp.ToString("F1") + "\n");
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
        UnityEngine.Object.Destroy(tgt[0].gameObject);
    }, false);
    t += 1.5f;
}
// Return: 적 없이 발사 → 0.3초·1.3초 vx 부호
DG.Tweening.DOVirtual.DelayedCall(t, () => {
    SkillEffectService.Detach("Dev:Rule"); SkillEffectService.AttachIds("Dev:Rule", "EF_DEV_Boomerang", "R");
    PlayerHealthManager.Instance.nowStemina = 999;
    var w = Weapon.Equipped; w.skill[0].skillCooldown.ResetCoolTime(); w.skill[0].isButtonDownSkill = true; w.ActiveWeaponSkill(0);
}, false);
foreach (float dt in new[] { 0.4f, 0.9f, 1.4f })
{
    float tt = dt;
    DG.Tweening.DOVirtual.DelayedCall(t + tt, () => {
        var sb = new System.Text.StringBuilder(); int n = 0;
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) { if (string.IsNullOrEmpty(o.skillName)) continue; var v = o.GetComponent<Rigidbody2D>().linearVelocity; sb.Append(v.x.ToString("F1") + " "); n++; }
        log.Append("RETURN t+" + tt + " n=" + n + " vx=" + sb + "\n");
    }, false);
}
t += 2.0f;
DG.Tweening.DOVirtual.DelayedCall(t, () => {
    foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
    SkillEffectService.Detach("Dev:Rule");
    // OnHitSpawn: 직접 리스너 호출
    var e1 = spawn(8f, 0f); var e2 = spawn(8.8f, 0f); var e3 = spawn(14f, 0f);
    var go = new GameObject("blast_test"); var d = go.AddComponent<DamageObject>(); d.SetDamge(new int[] { 100, 100, 0 });
    var ex = go.AddComponent<RuleOnHit>(); ex.Init(d, RuleOnHit.Kind.Explode, 60f, 1.5f);
    var sl = go.AddComponent<RuleOnHit>(); sl.Init(d, RuleOnHit.Kind.Slow, 30f, 1f);
    int h1 = e1.nowHP, h2 = e2.nowHP, h3 = e3.nowHP;
    ex.OnSkillHit(d, e1); sl.OnSkillHit(d, e1);
    DG.Tweening.DOVirtual.DelayedCall(0.2f, () => {
        log.Append("BLAST e1(center) dHP=" + (h1 - e1.nowHP) + " e2(near) dHP=" + (h2 - e2.nowHP) + " e3(far) dHP=" + (h3 - e3.nowHP) + " e1.slowMul=" + e1.MoveSpeedMul.ToString("F2") + " e2.slowMul=" + e2.MoveSpeedMul.ToString("F2") + "\n");
        UnityEngine.Object.Destroy(e1.gameObject); UnityEngine.Object.Destroy(e2.gameObject); UnityEngine.Object.Destroy(e3.gameObject); UnityEngine.Object.Destroy(go);
    }, false);
}, false);
t += 1.0f;
DG.Tweening.DOVirtual.DelayedCall(t, () => log.Append("DONE active=" + SkillEffectService.ActiveCount), false);
return "scheduled " + t;
