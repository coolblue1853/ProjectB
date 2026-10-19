// Phase 10c: 버프 Id 모디파이어 — 스킬 버프 2개 중첩 → 하나 만료 전 파괴 → 다른 하나 만료(DestoryBuff) → 0. __KEY__ = start | check
var sb = new System.Text.StringBuilder();
if ("__KEY__" == "start")
{
    var a = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Weapon/DamageObject/Buff/SpeedBuffOb"));
    var b = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Weapon/DamageObject/Buff/SpeedBuffOb"));
    var food = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("EatDefBuff")).GetComponent<PlayerBuff>();
    food.buffPower = 5; food.buffTime = 60; food.ActiveBuff();
    DG.Tweening.DOVirtual.DelayedCall(0.5f, () => { sb.Length = 0; System.AppDomain.CurrentDomain.SetData("pb_buff", "after0.5s " + PlayerStats.Dump()); UnityEngine.Object.Destroy(a); food.DestoryBuff(); }, false);
    DG.Tweening.DOVirtual.DelayedCall(1.0f, () => { System.AppDomain.CurrentDomain.SetData("pb_buff2", "after destroy a+food " + PlayerStats.Dump()); b.GetComponent<PlayerBuff>().DestoryBuff(); }, false);
    DG.Tweening.DOVirtual.DelayedCall(1.5f, () => { System.AppDomain.CurrentDomain.SetData("pb_buff3", "after expire b " + PlayerStats.Dump()); }, false);
    return "started " + PlayerStats.Dump();
}
return System.AppDomain.CurrentDomain.GetData("pb_buff") + "\n" + System.AppDomain.CurrentDomain.GetData("pb_buff2") + "\n" + System.AppDomain.CurrentDomain.GetData("pb_buff3");
