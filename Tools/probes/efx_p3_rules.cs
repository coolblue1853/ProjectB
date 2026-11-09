// REWORK-12 P3: LeafAdae 장착 → Scattering_Leaves 발동 시 효과별 탄 수·방향·속도 로그. 효과 해제 시 원복 확인. 로그 AppDomain "efx3"
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("efx3", log);
string[] ids = new[] { "", "EF_TEST_LeafFan", "EF_DEV_Ring", "EF_DEV_Parallel", "EF_DEV_Backward", "EF_DEV_Seek", "EF_DEV_Boomerang", "EF_DEV_PierceAdd", "EF_DEV_Blast", "" };
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => {
    var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("LeafAdae"));
    box.LoadPrefab("LeafAdae", "Weapon", d.tfName);
    amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
    log.Append("active after equip=" + SkillEffectService.ActiveCount + " (LeafAdae item effectIds=" + d.effectIds + ")\n");
}, false);
float t = 1.2f;
foreach (var id0 in ids)
{
    string id = id0;
    DG.Tweening.DOVirtual.DelayedCall(t, () => {
        SkillEffectService.Detach("Dev:Rule");
        if (id.Length > 0) SkillEffectService.AttachIds("Dev:Rule", id, "R");
        var w = Weapon.Equipped;
        PlayerHealthManager.Instance.nowStemina = 999;
        w.skill[0].skillCooldown.ResetCoolTime();
        log.Append("## " + id + " block=" + w.skill[0].BlockReason(0) + " ");
        w.skill[0].isButtonDownSkill = true;
        try { w.ActiveWeaponSkill(0); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); }
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 0.35f, () => {
        var pp = PlayerController.instance.transform.position;
        var objs = new System.Collections.Generic.List<string>();
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(o.skillName)) continue;
            var rb = o.GetComponent<Rigidbody2D>();
            var v = rb != null ? rb.linearVelocity : Vector2.zero;
            objs.Add((o.transform.position.x - pp.x).ToString("F1") + "," + (o.transform.position.y - pp.y).ToString("F1") + "|up" + o.transform.up.x.ToString("F2") + "," + o.transform.up.y.ToString("F2") + "|v" + v.x.ToString("F1") + "," + v.y.ToString("F1") + "|max" + o.maxDamagedEnemies + "|hold" + o.holdingTime.ToString("F2") + (o.GetComponent<RuleHoming>() != null ? "|H" : "") + (o.GetComponent<RuleReturn>() != null ? "|R" : "") + (o.GetComponent<RuleOnHit>() != null ? "|X" : ""));
        }
        objs.Sort();
        log.Append("n=" + objs.Count + " :: " + string.Join(" ; ", objs.ToArray()) + "\n");
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
    }, false);
    t += 1.5f;
}
DG.Tweening.DOVirtual.DelayedCall(t, () => { SkillEffectService.Detach("Dev:Rule"); log.Append("DONE active=" + SkillEffectService.ActiveCount); }, false);
return "scheduled " + t;
