// 무기 계열 1차 플레이 검증(플레이 모드, 산적 성채 낮). __W__ = 무기 이름, __M__ = bow|mace|axe|dual|whip|reg
// 1회차: 무기 장착(Weapon.Start 이후 다시 실행). 2회차: 가장 가까운 몬스터 앞에 서서 기믹 수치를 기록 → AppDomain "pb_wt"
string wn = "__W__"; string mode = "__M__";
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_wt", log);
if (Weapon.Equipped == null || !Weapon.Equipped.name.StartsWith(wn))
{
    var d0 = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
    box.LoadPrefab(wn, "Weapon", d0.tfName);
    return "equipping " + wn + " (Weapon.Start 이후 다시 실행)";
}
if (amg.equipWeapon != Weapon.Equipped) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); }
var pc = PlayerController.instance;
EnemyHealth target = null; float best = 1e9f;
foreach (var e in EnemyHealth.Active) { if (!e.IsMonster || e.isBossMob || Mathf.Abs(e.transform.position.y - pc.transform.position.y) > 1.5f) continue; float dd = Mathf.Abs(e.transform.position.x - pc.transform.position.x); if (dd < best) { best = dd; target = e; } }
if (target == null) { foreach (var e in EnemyHealth.Active) { if (!e.IsMonster || e.isBossMob) continue; target = e; break; } }
if (target == null) return "no target";
pc.transform.position = new Vector3(target.transform.position.x - (mode == "bow" ? 7f : 1.2f), target.transform.position.y + 0.3f, pc.transform.position.z);
var sc = pc.transform.localScale; sc.x = Mathf.Abs(sc.x); pc.transform.localScale = sc;
var w = Weapon.Equipped;
System.Action<float, System.Action> at = (t, a) => DG.Tweening.DOVirtual.DelayedCall(t, () => { try { a(); } catch (System.Exception ex) { log.Append("EX " + ex.Message + "; "); } }, false);
System.Func<string, string> hitboxes = prefix => {
    var s = new System.Text.StringBuilder();
    foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None))
        if (o.name.StartsWith(prefix)) s.Append(o.name.Replace("(Clone)", "") + "{ratio" + o.dmgRatio.ToString("F2") + " sx" + o.transform.lossyScale.x.ToString("F2") + " stiff" + o.stiffnessTime.ToString("F2") + " ign" + (o.damageArr != null ? o.damageArr[5] : -1) + " mod" + o.HasHitModifier + "} ");
    return s.ToString(); };
var ph = PlayerHealthManager.Instance; ph.nowHp = ph.fullHP;
log.Append(w.name + " kind" + w.Kind + " target " + target.name + " def" + target.enemyDef + " hp" + target.nowHP + " gap " + (target.transform.position.x - pc.transform.position.x).ToString("F1") + " | ");
int hp0 = target.nowHP;
if (mode == "bow")
{
    var ch = w.Charge; log.Append("charge " + (ch != null) + " | ");
    float[] held = { 0.1f, 0.5f, 1.1f }; var seen = new System.Collections.Generic.HashSet<int>();
    for (int i = 0; i < 3; i++)
    {
        int k = i;
        at(0.2f + k * 1.6f, () => { ph.nowHp = ph.fullHP; ch.BeginHold(Time.time - held[k]); int tier = ch.EndHold(Time.time); log.Append("held" + held[k] + " tier" + tier + " can" + w.CanMeleeNow + " "); w.MeleeAttack(); });
        for (int sc2 = 1; sc2 <= 6; sc2++) at(0.2f + k * 1.6f + sc2 * 0.08f, () => {
            foreach (var a in UnityEngine.Object.FindObjectsByType<StraightArrow>(FindObjectsSortMode.None)) if (seen.Add(a.GetInstanceID()))
            {
                var t = typeof(StraightArrow); var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var dm = (int[])t.GetField("_damage", bf).GetValue(a); int pt = (int)t.GetField("_pierceTotal", bf).GetValue(a);
                log.Append("arrow " + a.name.Replace("(Clone)", "") + " dmg" + dm[0] + "~" + dm[1] + " pierce" + pt + "; ");
            }
        });
    }
}
else if (mode == "reg" || mode == "mace" || mode == "axe")
{
    int n = mode == "reg" ? 3 : 2;
    for (int i = 0; i < n; i++)
    {
        int k = i;
        at(0.2f + k * 0.75f, () => { ph.nowHp = ph.fullHP; w.MeleeAttack(); });
        at(0.5f + k * 0.75f, () => { log.Append("s" + w.nowConboCount + "[" + hitboxes("") + "] "); });
    }
    if (mode == "axe") at(2.0f, () => { foreach (var e in EnemyHealth.Active) if (e.IsMonster) { log.Append(e.name.Replace("(Clone)", "") + " def" + e.enemyDef + " cleave" + WeaponGimmickRules.CleaveMultiplier(e.enemyDef).ToString("F2") + "; "); } });
}
else if (mode == "dual")
{
    for (int i = 0; i < 4; i++)
    {
        int k = i;
        at(0.2f + k * 0.45f, () => { ph.nowHp = ph.fullHP; w.MeleeAttack(); });
        at(0.5f + k * 0.45f, () => { log.Append("s" + w.nowConboCount + "[" + hitboxes("TwinFang") + "] "); });
    }
}
else if (mode == "whip")
{
    at(0.2f, () => { w.MeleeAttack(); });
    at(0.5f, () => {
        DamageObject hb = null;
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (o.name.StartsWith("VineLash")) hb = o;
        if (hb == null) { log.Append("no hitbox"); return; }
        var bc = hb.GetComponent<BoxCollider2D>(); var b = bc.bounds; var tc = target.GetComponent<Collider2D>();
        log.Append("hb mod" + hb.HasHitModifier + " bounds x" + b.min.x.ToString("F2") + ".." + b.max.x.ToString("F2") + " len" + b.size.x.ToString("F2") + " | ");
        var home = target.transform.position;
        foreach (float f in new[]{ 0.05f, 0.5f, 0.9f })
        {
            float cx = b.min.x + f * b.size.x + tc.bounds.extents.x;
            target.transform.position = new Vector3(cx, home.y, home.z); Physics2D.SyncTransforms();
            log.Append("frac" + f + " mul" + Weapon.Equipped.GetComponent<TipBonus>().HitMultiplier(hb, target, tc).ToString("F2") + "; ");
        }
        target.transform.position = home;
    });
}
at(3.0f + (mode == "bow" ? 3.0f : 0f), () => { log.Append("DONE dmgToTarget " + (hp0 - target.nowHP) + " hp " + target.nowHP); });
return "scheduled " + mode + " vs " + target.name;
