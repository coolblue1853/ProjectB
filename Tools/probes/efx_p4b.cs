// REWORK-12 P4b: Chain 기준선 대비 + CooldownRefund(Kill/Hit). 로그 "efx4"
var pc = PlayerController.instance;
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("efx4", log);
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab");
var list = new System.Collections.Generic.List<EnemyHealth>();
System.Func<float, int, EnemyHealth> spawn = (dx, hp) => {
    float gy = GroundProbe.Y(pc.transform.position + Vector3.right * dx);
    var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pc.transform.position.x + dx, gy + 0.6f, 0f), Quaternion.identity);
    var e = go.GetComponent<EnemyHealth>(); e.maxHP = hp; e.nowHP = hp; list.Add(e);
    return e;
};
System.Action clear = () => { foreach (var e in list) if (e != null) UnityEngine.Object.Destroy(e.gameObject); list.Clear(); foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject); };
System.Action cast = () => {
    var w = Weapon.Equipped;
    PlayerHealthManager.Instance.nowStemina = 999;
    w.skill[0].skillCooldown.ResetCoolTime();
    w.skill[0].isButtonDownSkill = true;
    try { w.ActiveWeaponSkill(0); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); }
};
System.Action<string> loss = (tag) => { log.Append("## " + tag + " hp loss: "); foreach (var e in list) log.Append((100000 - e.nowHP) + " "); log.Append("\n"); };
float t = 0.5f;
foreach (string id0 in new[] { "", "EF_DEV_Chain" })
{
    string id = id0;
    DG.Tweening.DOVirtual.DelayedCall(t, () => {
        clear(); SkillEffectService.Detach("Dev:Rule"); if (id.Length > 0) SkillEffectService.AttachIds("Dev:Rule", id, "R");
        spawn(4f, 100000); spawn(7f, 100000); spawn(9f, 100000); spawn(11f, 100000);
        cast();
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 1.4f, () => loss(id.Length == 0 ? "base" : "chain"), false);
    t += 1.8f;
}
foreach (string id0 in new[] { "EF_DEV_RefundKill", "EF_DEV_RefundHit" })
{
    string id = id0;
    DG.Tweening.DOVirtual.DelayedCall(t, () => {
        clear(); SkillEffectService.Detach("Dev:Rule"); SkillEffectService.AttachIds("Dev:Rule", id, "R");
        spawn(id.EndsWith("Kill") ? 6f : 4f, id.EndsWith("Kill") ? 1 : 100000);
        cast();
        var cd = SkillCooldown.instance; cd.isCooldown[0] = false; cd.cooldownTime[0] = 20f;
        cd.UseSkill(0, "Scattering_Leaves", Weapon.Equipped.Kind);
        log.Append("## " + id + " remain0=" + cd.Remaining(0).ToString("F2"));
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 0.3f, () => {
        var cd = SkillCooldown.instance;
        log.Append(" remainAt0.3=" + cd.Remaining(0).ToString("F2"));
        if (id.EndsWith("Kill") && list[0] != null) list[0].damage2Enemy(new int[] { 5, 5, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, true, 0f, 1f);
        log.Append(" afterEvent=" + cd.Remaining(0).ToString("F2") + "\n");
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 1.5f, () => log.Append("   later(" + id + ")=" + SkillCooldown.instance.Remaining(0).ToString("F2") + "\n"), false);
    t += 1.8f;
}
DG.Tweening.DOVirtual.DelayedCall(t, () => { SkillEffectService.Detach("Dev:Rule"); clear(); log.Append("DONE"); }, false);
return "scheduled " + t;
