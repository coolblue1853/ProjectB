// B-141 검증: 이동·공격 시퀀스가 돌고 있는 몹을 강제 반환한 뒤 그 시퀀스가 끊겼는지(반환 전 참조로) 본다.
// 몹 자식으로 붙은 판정(EnemyDamageObject)이 있으면 2.5초 뒤 풀로 돌아갔는지도 로그로 남긴다("[B141] dmg ...").
// 반환: "moving=N stillActive=N attachedDmg=N"  (stillActive 0 이어야 함)
int moving = 0, still = 0, attached = 0;
var seqs = new System.Collections.Generic.List<DG.Tweening.Sequence>();
var dmgs = new System.Collections.Generic.List<EnemyDamageObject>();
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
{
    if (!e.isActiveAndEnabled || e.GetComponent<BossPhase>() != null)
    {
        continue;
    }
    var bt = e.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
    bool hasSeq = bt != null && bt.sequence != null && DG.Tweening.TweenExtensions.IsActive(bt.sequence);
    var childDmg = e.GetComponentsInChildren<EnemyDamageObject>(false);
    if (!hasSeq && childDmg.Length == 0)
    {
        continue;
    }
    if (hasSeq)
    {
        moving++;
        seqs.Add(bt.sequence);
    }
    foreach (var d in childDmg)
    {
        dmgs.Add(d);
        attached++;
    }
    e.ForceEnemyRelease();
}
foreach (var s in seqs)
{
    if (DG.Tweening.TweenExtensions.IsActive(s))
    {
        still++;
    }
}
DG.Tweening.DOVirtual.DelayedCall(2.5f, () =>
{
    int back = 0, stuck = 0;
    foreach (var d in dmgs)
    {
        if (d == null) { continue; }
        if (d.IsInPool && d.transform.parent == null) { back++; } else { stuck++; Debug.Log("[B141] dmg stuck " + d.name + " active=" + d.gameObject.activeInHierarchy + " parent=" + (d.transform.parent == null ? "null" : d.transform.parent.name)); }
    }
    Debug.Log("[B141] dmg returned=" + back + " stuck=" + stuck);
}, false);
return "moving=" + moving + " stillActive=" + still + " attachedDmg=" + attached;
