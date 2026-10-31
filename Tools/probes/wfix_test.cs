// 무기 1차 리뷰 수정 검증(플레이). __W__ = HuntersLongbow, __M__ = bow | whip. 결과는 wtype_log.cs 로 읽는다
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
    return "equipping " + wn;
}
if (amg.equipWeapon != Weapon.Equipped) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); }
var pc = PlayerController.instance; var w = Weapon.Equipped; var ph = PlayerHealthManager.Instance; ph.nowHp = ph.fullHP;
EnemyHealth target = null; float best = 1e9f;
foreach (var e in EnemyHealth.Active) { if (!e.IsMonster || e.isBossMob) continue; float dd = Mathf.Abs(e.transform.position.x - pc.transform.position.x); if (dd < best) { best = dd; target = e; } }
if (target == null) return "no target";
System.Action<float, System.Action> at = (t, a) => DG.Tweening.DOVirtual.DelayedCall(t, () => { try { a(); } catch (System.Exception ex) { log.Append("EX " + ex.Message + "; "); } }, false);
for (int i = 0; i < 60; i++) at(i * 0.1f, () => { ph.nowHp = ph.fullHP; }); // 죽지 않게 계속 채운다
var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
if (mode == "bow")
{
    var ch = w.Charge; var seen = new System.Collections.Generic.HashSet<int>();
    // 위치는 옮기지 않는다(벼랑 위 낙하 방지) - 가까운 몹 쪽을 본다
    var sc = pc.transform.localScale; sc.x = (target.transform.position.x >= pc.transform.position.x ? 1f : -1f) * Mathf.Abs(sc.x); pc.transform.localScale = sc;
    log.Append("target " + target.name + " gap " + (target.transform.position.x - pc.transform.position.x).ToString("F1") + "; ");
    System.Action<string> arrows = tag => { foreach (var a in UnityEngine.Object.FindObjectsByType<StraightArrow>(FindObjectsSortMode.None)) if (seen.Add(a.GetInstanceID())) { var t = typeof(StraightArrow); var dm = (int[])t.GetField("_damage", bf).GetValue(a); int pt = (int)t.GetField("_pierceTotal", bf).GetValue(a); log.Append(tag + " arrow dmg" + dm[0] + "~" + dm[1] + " pierce" + pt + "; "); } };
    // 1) 릴리스 후 재입력해도 단계 유지: 2단계 놓음 -> 바로 짧게 한 번 더 -> 발사 = 2단계여야
    at(0.2f, () => { ch.BeginHold(Time.time - 1.1f); int t1 = ch.EndHold(Time.time); ch.BeginHold(Time.time); int t2 = ch.EndHold(Time.time); log.Append("T1 release tier" + t1 + " then reinput tier" + t2 + " pending" + ch.ReleasedTier + "; "); w.MeleeAttack(); });
    for (int i = 1; i <= 8; i++) at(0.2f + i * 0.08f, () => arrows("T1"));
    // 2) 후딜 중 놓은 차지가 0.4초 안에 자동 발사: 평타 직후 2단계 놓고 버퍼에 넣는다
    at(1.6f, () => { log.Append("T2 first can" + w.CanMeleeNow + "; "); w.MeleeAttack(); });
    at(1.85f, () => { log.Append("T2 can" + w.CanMeleeNow + "; "); ch.BeginHold(Time.time - 1.1f); ch.EndHold(Time.time); var am = typeof(AttackManager).GetProperty("chargeBuffer", bf); ((InputBuffer)am.GetValue(amg)).Press(Time.time); });
    for (int i = 1; i <= 40; i++) at(1.6f + i * 0.04f, () => arrows("T2"));
    var sf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static; var ct = typeof(ChargeShot); float lastVU = -1f;
    for (int i = 0; i <= 60; i++) at(1.6f + i * 0.03f, () => { float vu = (float)ct.GetField("s_validUntil", sf).GetValue(null); if (vu != lastVU && vu > 0f) { lastVU = vu; log.Append("[attackStart mul" + ((float)ct.GetField("s_mul", sf).GetValue(null)).ToString("F2") + " pierce" + (int)ct.GetField("s_pierce", sf).GetValue(null) + " @" + Time.time.ToString("F2") + "] "); } });
    bool lastCan = false; for (int i = 0; i <= 40; i++) at(1.6f + i * 0.04f, () => { if (w.CanMeleeNow != lastCan) { lastCan = w.CanMeleeNow; log.Append("[can" + lastCan + "@" + Time.time.ToString("F2") + "] "); } });
    at(3.4f, () => { log.Append("T2 can" + w.CanMeleeNow + "; "); });
    // 3) 무기 교체(OnDisable) 후 남은 충전 소비 불가
    at(4.0f, () => { ch.BeginHold(Time.time - 1.1f); ch.EndHold(Time.time); w.MeleeAttack(); });
    at(4.05f, () => { ch.enabled = false; float m; int p; bool ok = ChargeShot.TryConsume(out m, out p); log.Append("T3 consumeAfterDisable " + ok + "; "); ch.enabled = true; });
    at(4.5f, () => { log.Append("DONE preDelay " + w.isHasDelayTime); });
    return "scheduled bow";
}
else
{
    // 채찍 왼쪽 방향 끝 판정
    var hbHome = target.transform.position;
    pc.transform.position = new Vector3(target.transform.position.x + 1.2f, target.transform.position.y + 0.3f, pc.transform.position.z);
    var sc = pc.transform.localScale; sc.x = -Mathf.Abs(sc.x); pc.transform.localScale = sc;
    at(0.2f, () => { w.MeleeAttack(); });
    at(0.5f, () => {
        DamageObject hb = null;
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (o.name.StartsWith("VineLash")) hb = o;
        if (hb == null) { log.Append("no hitbox"); return; }
        var b = hb.GetComponent<BoxCollider2D>().bounds; var tc = target.GetComponent<Collider2D>();
        log.Append("hb mod" + hb.HasHitModifier + " bounds x" + b.min.x.ToString("F2") + ".." + b.max.x.ToString("F2") + " | ");
        foreach (float f in new[] { 0.05f, 0.5f, 0.9f })
        {
            float cx = b.max.x - f * b.size.x - tc.bounds.extents.x;
            target.transform.position = new Vector3(cx, hbHome.y, hbHome.z); Physics2D.SyncTransforms();
            log.Append("L frac" + f + " mul" + w.GetComponent<TipBonus>().HitMultiplier(hb, target, tc).ToString("F2") + "; ");
        }
        target.transform.position = hbHome;
    });
    return "scheduled whip left";
}
