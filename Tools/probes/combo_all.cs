// 무기 전체 콤보 검증(W-01/02/03): 각 무기를 장착하고 후딜이 끝날 때마다 평타를 눌러 단계 순서를 기록. 결과는 combo_all_read.cs 로 읽는다
var names = new System.Collections.Generic.List<string>();
foreach (var go in Resources.LoadAll<GameObject>("Weapon")) if (go.GetComponent<Weapon>() != null && ("__ONLY__" == "" || ("," + "__ONLY__" + ",").Contains("," + go.name + ","))) names.Add(go.name);
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_combo", log);
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
float t = 0f;
string orig = Weapon.Equipped != null ? Weapon.Equipped.name.Replace("(Clone)", "").Trim() : null;
foreach (var n in names)
{
    string wn = n;
    DG.Tweening.DOVirtual.DelayedCall(t, () =>
    {
        int idx = DatabaseManager.instance.FindItemDataIndex(wn);
        if (idx < 0) { log.Append(wn + ": no item row\n"); return; }
        var data = DatabaseManager.instance.LoadItemData(idx);
        try { box.LoadPrefab(wn, "Weapon", data.tfName); } catch (System.Exception e) { log.Append(wn + ": equip EX " + e.GetType().Name + "\n"); }
    }, true);
    var steps = new System.Text.StringBuilder();
    // 1.6초 동안 0.05초마다: 공격 가능하면 누르고 단계를 기록
    for (int k = 0; k < 40; k++)
    {
        DG.Tweening.DOVirtual.DelayedCall(t + 0.4f + k * 0.05f, () =>
        {
            var w = Weapon.Equipped;
            if (w == null || !w.CanMeleeNow) return;
            try { w.MeleeAttack(); steps.Append(w.nowConboCount); } catch (System.Exception e) { steps.Append("X(" + e.Message + " @ " + e.StackTrace.Split('\n')[0] + ")"); }
        }, true);
    }
    DG.Tweening.DOVirtual.DelayedCall(t + 2.5f, () =>
    {
        var w = Weapon.Equipped;
        log.Append(wn + " max=" + (w ? w.maxComboCount : -1) + " steps=" + steps + "\n");
    }, true);
    t += 2.7f;
}
DG.Tweening.DOVirtual.DelayedCall(t + 0.2f, () =>
{
    if (orig != null) { var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(orig)); box.LoadPrefab(orig, "Weapon", d.tfName); }
    log.Append("DONE restored=" + orig + "\n");
}, true);
return "scheduled " + names.Count + " weapons, ~" + Mathf.CeilToInt(t) + "s";
