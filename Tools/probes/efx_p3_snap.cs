// REWORK-12 P3: 대표 스킬 5종 전후 동일성. 무기 장착 → 슬롯 발동 → 판정 스냅샷(수, 상대위치, up, 피해, 스케일, 속도). 로그 AppDomain "efx3"
var jobs = new string[][] { new[]{"LeafAdae","0"}, new[]{"WispLantern","0"}, new[]{"BearClaw","0"}, new[]{"QuakestoneMaul","0"}, new[]{"RootStaff","1"} };
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("efx3", log);
float t = 0.5f;
foreach (var j in jobs)
{
    string wn = j[0]; int slot = int.Parse(j[1]);
    DG.Tweening.DOVirtual.DelayedCall(t, () => {
        var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
        box.LoadPrefab(wn, "Weapon", d.tfName);
        amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon();
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 0.8f, () => {
        PlayerHealthManager.Instance.nowStemina = 999;
        var w = Weapon.Equipped;
        log.Append("## " + w.name + " slot" + slot + "=" + w.skill[slot].skillName + " block=" + w.skill[slot].BlockReason(slot) + " ");
        w.skill[slot].isButtonDownSkill = true;
        try { w.ActiveWeaponSkill(slot); } catch (System.Exception e) { log.Append("EX " + e.Message + "; "); }
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(t + 1.15f, () => {
        var pp = PlayerController.instance.transform.position;
        var objs = new System.Collections.Generic.List<string>();
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(o.skillName)) continue;
            var rb = o.GetComponent<Rigidbody2D>();
            var v = rb != null ? rb.linearVelocity : Vector2.zero;
            objs.Add(o.skillName + "|" + (o.transform.position.x - pp.x).ToString("F1") + "," + (o.transform.position.y - pp.y).ToString("F1") + "|up" + o.transform.up.x.ToString("F2") + "," + o.transform.up.y.ToString("F2") + "|dmg" + (o.damageArr != null && o.damageArr.Length > 0 ? o.damageArr[0] : -1) + "|sc" + o.transform.localScale.x.ToString("F2") + "|v" + v.x.ToString("F1") + "," + v.y.ToString("F1") + "|max" + o.maxDamagedEnemies);
        }
        objs.Sort();
        log.Append("n=" + objs.Count + " :: " + string.Join(" ; ", objs.ToArray()) + "\n");
        foreach (var o in UnityEngine.Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)) if (!string.IsNullOrEmpty(o.skillName)) UnityEngine.Object.Destroy(o.gameObject);
    }, false);
    t += 2.6f;
}
DG.Tweening.DOVirtual.DelayedCall(t, () => log.Append("DONE"), false);
return "scheduled " + t;
