// 늪지 계보 무기 검증(플레이 모드, 늪지 낮): 무기 __W__ 장착 → 가장 가까운 몬스터 __GAP__유닛 왼쪽에 서서 슬롯 __S__ 발동 →
// 1.2초 뒤 대상 x 변화(당기기)·HP(흡혈)·소환물(SummonRunner) 수·판정 이름 기록. 결과는 pb_sw (Logs/_r.cs 로 읽음)
string wn = "__W__"; int slot = __S__;
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_sw", log);
if (Weapon.Equipped == null || !Weapon.Equipped.name.StartsWith(wn))
{
    var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
    box.LoadPrefab(wn, "Weapon", d.tfName);
    return "equipping " + wn + " (Weapon.Start 이후 다시 실행)";
}
if (amg.equipWeapon != Weapon.Equipped) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); }
var pc = PlayerController.instance;
EnemyHealth target = null; float best = 1e9f;
foreach (var e in EnemyHealth.Active) { if (!e.IsMonster || e.isBossMob) continue; float dd = Mathf.Abs(e.transform.position.x - pc.transform.position.x); if (dd < best) { best = dd; target = e; } }
if (target == null) return "no target";
pc.transform.position = new Vector3(target.transform.position.x - __GAP__f, target.transform.position.y + 0.3f, pc.transform.position.z);
var sc = pc.transform.localScale; sc.x = Mathf.Abs(sc.x); pc.transform.localScale = sc;
DG.Tweening.DOVirtual.DelayedCall(0.5f, () => {
    var ph = PlayerHealthManager.Instance; ph.nowHp = Mathf.Max(40, ph.fullHP / 2);
    var w = Weapon.Equipped;
    float x0 = target.transform.position.x, px = pc.transform.position.x; int ehp0 = target.nowHP;
    log.Append(w.name + " " + w.minDmg + "~" + w.maxDmg + " slot" + slot + "=" + w.skill[slot].skillName + " target " + target.name + " gap " + (x0 - px).ToString("F1") + " | ");
    w.skill[slot].isButtonDownSkill = true;
    try { w.ActiveWeaponSkill(slot); } catch (System.Exception ex) { log.Append("EX " + ex.Message + "; "); }
    for (int k = 1; k <= 6; k++)
    {
        float tt = k * 0.2f;
        DG.Tweening.DOVirtual.DelayedCall(tt, () => {
            var names = new System.Collections.Generic.List<string>();
            foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!names.Contains(o.name)) names.Add(o.name.Replace("(Clone)", ""));
            int runners = UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsSortMode.None).Length;
            log.Append(tt.ToString("F1") + ": [" + string.Join(",", names.ToArray()) + "] run" + runners + " hp" + ph.nowHp + " gap " + (target.transform.position.x - pc.transform.position.x).ToString("F1") + " ehp " + target.nowHP + "; ");
        }, false);
    }
    DG.Tweening.DOVirtual.DelayedCall(1.3f, () => log.Append("DONE dx " + (target.transform.position.x - x0).ToString("F2") + " dmg " + (ehp0 - target.nowHP)), false);
}, false);
return "scheduled vs " + target.name;
