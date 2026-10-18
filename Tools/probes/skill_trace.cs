// 무기 __W__ 장착 후 슬롯 __S__ 스킬 발동 → 0.1초 간격 1.6초 동안 판정 이름·무적·HP 추적. 결과 pb_trace (_r 로 GetData)
string wn = "__W__"; int slot = __S__;
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_trace", log);
if (Weapon.Equipped == null || !Weapon.Equipped.name.StartsWith(wn))
{
    var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
    box.LoadPrefab(wn, "Weapon", d.tfName);
    amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
}
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => {
    var ph = PlayerHealthManager.Instance; ph.nowHp = 40; // 회복 스킬 확인용으로 HP 를 낮춘다 PlayerHealthManager.Instance.nowStemina = 999;
    var w = Weapon.Equipped;
    log.Append(w.name + " " + w.minDmg + "~" + w.maxDmg + " slot" + slot + "=" + w.skill[slot].skillName + " hp" + ph.nowHp + " pos" + PlayerController.instance.transform.position.x.ToString("F1") + " | ");
    w.skill[slot].isButtonDownSkill = true; // 입력 없이 발동 (CanActivate 조건)
    try { w.ActiveWeaponSkill(slot); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); }
    for (int k = 1; k <= 16; k++)
    {
        float tt = k * 0.1f;
        DG.Tweening.DOVirtual.DelayedCall(tt, () => {
            var names = new System.Collections.Generic.List<string>();
            foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) names.Add(o.name.Replace("(Clone)", ""));
            int orbs = UnityEngine.Object.FindObjectsByType<HomingOrb>(FindObjectsSortMode.None).Length;
            log.Append(tt.ToString("F1") + ":" + string.Join(",", names.ToArray()) + (orbs > 0 ? " orb" + orbs : "") + (PlayerStatus.Has(StatusFlag.Invincible) ? " INV" : "") + " hp" + ph.nowHp + " x" + PlayerController.instance.transform.position.x.ToString("F1") + "; ");
        }, false);
    }
    DG.Tweening.DOVirtual.DelayedCall(1.7f, () => log.Append("DONE"), false);
}, false);
return "scheduled";
