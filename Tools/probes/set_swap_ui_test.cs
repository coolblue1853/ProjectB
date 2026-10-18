// B-128 재현(플레이 모드): 실제 UI 장착 경로 InventoryManager.UseEquipment(ItemCheck)(우클릭·Enter 장착과 같은 메서드, 리플렉션 호출)로
// 같은 세트 키 무기 2종(__A__ ↔ __B__)과 같은 부위 방어구(__C__)를 0.4초 간격으로 번갈아 장착 → 매번 세트 스택(__TF__)·플레이어 밑 무기/장비 개수를 기록.
// __OPEN__=1 이면 인벤토리를 연 상태(실제 UI), 0 이면 닫힌 상태. 결과 pb_swap (Logs/_r.cs). 세이브 백업 후 실행(장착 정보가 저장된다)
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_swap", log);
var inv = InventoryManager.instance;
var use = typeof(InventoryManager).GetMethod("UseEquipment", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
string tf = "__TF__";
System.Func<string> state = () => {
    int st; DatabaseManager.setsEffectStack.TryGetValue(tf, out st);
    return "stack" + st + " weapons" + am.GetComponentsInChildren<Weapon>(true).Length + " equips" + am.GetComponentsInChildren<Equipment>(true).Length;
};
System.Action<string> equip = (n) => {
    GameObject go = UnityEngine.Object.Instantiate(inv.itemPrefab, inv.transform.position, Quaternion.identity, inv.transform);
    var ic = go.GetComponent<ItemCheck>();
    ic.SetItem(n);
    use.Invoke(inv, new object[]{ ic });
};
if ("__OPEN__" == "1") inv.inventory.SetActive(true); // 실제 UI 는 인벤토리를 연 채로 장착한다(EquipBoxCheck.Update 가 돌아야 활성화)
log.Append("start open=" + inv.inventory.activeSelf + " " + state() + "; ");
string[] seq = { "__A__", "__B__", "__A__", "__B__", "__C__", "__C__", "__C__" };
for (int i = 0; i < seq.Length; i++)
{
    string n = seq[i];
    DG.Tweening.DOVirtual.DelayedCall(0.4f * (i + 1), () => {
        try { equip(n); } catch (System.Exception ex) { log.Append("EX " + (ex.InnerException ?? ex).Message + " "); }
        DG.Tweening.DOVirtual.DelayedCall(0.2f, () => log.Append(n + "→" + state() + "; "), false);
    }, false);
}
DG.Tweening.DOVirtual.DelayedCall(0.4f * (seq.Length + 2), () => log.Append("DONE"), false);
return "scheduled " + state();
