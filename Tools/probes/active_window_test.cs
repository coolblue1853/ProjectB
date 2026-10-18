// W-07: 평타 판정이 공격 주기×비율 뒤 닫히는지. 결과 pb_aw
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_aw", log);
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Weapon.Equipped.MeleeAttack(), true);
foreach (float t in new[]{ 0.4f, 0.6f, 0.9f })
{
    float tt = t;
    DG.Tweening.DOVirtual.DelayedCall(tt, () =>
    {
        var dm = Weapon.Equipped.GetComponentInChildren<DamageObject>();
        log.Append((tt - 0.3f).ToString("F1") + "s:" + (dm == null ? "none" : "end=" + dm.activeEnd.ToString("F2") + " col=" + dm.GetComponent<BoxCollider2D>().enabled + " hold=" + dm.holdingTime) + "; ");
    }, false);
}
return "scheduled";
