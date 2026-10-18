// 깊은 숲 계보 무기 3종 스킬 발동 검증: 장착 → 슬롯마다 ActiveWeaponSkill → 0.9초 뒤 그 스킬 이름의 판정(DamageObject) 수. 결과는 pb_dfw (_read: GetData)
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_dfw", log);
float t = 0f;
foreach (var wn0 in new[]{ "BearClaw", "RootStaff", "HeartwoodGreatsword" })
{
    string wn = wn0;
    DG.Tweening.DOVirtual.DelayedCall(t, () => {
        var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
        box.LoadPrefab(wn, "Weapon", d.tfName);
        amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
        log.Append(wn + " [" + Weapon.Equipped.minDmg + "~" + Weapon.Equipped.maxDmg + "]: ");
    }, false);
    for (int k = 0; k < 4; k++)
    {
        int slot = k;
        DG.Tweening.DOVirtual.DelayedCall(t + 0.4f + slot * 1.6f, () => {
            PlayerHealthManager.Instance.nowStemina = 999;
            var w = Weapon.Equipped; if (w == null || slot >= w.skill.Length || w.skill[slot] == null) return;
            try { w.ActiveWeaponSkill(slot); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); }
        }, false);
        DG.Tweening.DOVirtual.DelayedCall(t + 1.5f + slot * 1.6f, () => {
            var w = Weapon.Equipped; if (w == null || slot >= w.skill.Length || w.skill[slot] == null) return;
            string sn = w.skill[slot].skillName; int n = 0;
            foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (o.skillName == sn) n++;
            log.Append(slot + ":" + sn + "=" + n + " cd=" + SkillCooldown.instance.isCooldown[slot] + "; ");
        }, false);
    }
    t += 7f;
}
DG.Tweening.DOVirtual.DelayedCall(t, () => log.Append("DONE"), false);
return "scheduled ~" + t + "s";
