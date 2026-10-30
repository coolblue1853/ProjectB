// 둥지 추적 구동(플레이 중). __MODE__: win(흔적 3 → 둥지 → 정예 소환) / order(순서 어김·쿨타임 확인) / timeout(시간 초과 실패 → 쿨타임) / leave(맵 이탈 취소)
// 결과는 "[NestDrive] ..." 오브젝트 이름에 쌓인다. 마크 앞으로 순간이동 → Begin() → 살피기 0.8초(게임 시간) 뒤 다음.
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
NestHuntSite site = null;
foreach (var n in UnityEngine.Object.FindObjectsByType<NestHuntSite>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (n.gameObject.activeInHierarchy) site = n;
if (site == null) return "no active site";
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[NestDrive]")) UnityEngine.Object.Destroy(g);
var log = new UnityEngine.GameObject("[NestDrive] __MODE__");
var rb = pc.GetComponent<Rigidbody2D>();
System.Action<string> L = s => log.name += " | " + s;
System.Action<int> go = null;

go = i =>
{
    var m = site.Mark(i);
    pc.transform.position = new Vector3(m.transform.position.x, m.transform.position.y + 1.2f, pc.transform.position.z);
    if (rb != null) rb.linearVelocity = Vector2.zero;
    bool begun = m.Begin();
    L("m" + i + " begin=" + begun + " vis=" + site.IsVisible(i));
    DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(3f, () =>
    {
        L("after" + i + " phase=" + site.State.Phase + " found=" + site.State.Found + " t=" + site.State.Elapsed.ToString("F1"));
        if (i < 3) go(i + 1);
        else L("elite=" + (EnemySpawner.TrackedTarget != null && !EnemySpawner.TrackedTarget.IsInPool ? EnemySpawner.TrackedTarget.gameObject.name + " isElite=" + EnemySpawner.TrackedTarget.IsElite : "none") + " name=" + site.SummonedName + " cd=" + site.CooldownLeft.ToString("F0"));
    }), true);
};
if ("__MODE__" == "win") { go(0); }
else if ("__MODE__" == "order")
{
    L("vis1=" + site.IsVisible(1) + " vis3=" + site.IsVisible(3) + " can1=" + site.CanInvestigate(1) + " can3=" + site.CanInvestigate(3));
    L("begin1=" + site.Mark(1).Begin() + " begin3=" + site.Mark(3).Begin());
}
else if ("__MODE__" == "timeout")
{
    var m = site.Mark(0);
    pc.transform.position = new Vector3(m.transform.position.x, m.transform.position.y + 1.2f, pc.transform.position.z);
    L("begin0=" + m.Begin());
    DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(3f, () =>
    {
        L("running=" + site.State.Running + " found=" + site.State.Found);
        site.ForceTimeout();
        L("phase=" + site.State.Phase + " cd=" + site.CooldownLeft.ToString("F0") + " can0=" + site.CanInvestigate(0) + " showResult=" + site.ShowResult);
    }), true);
}
else if ("__MODE__" == "cd")
{
    L("phase=" + site.State.Phase + " cd=" + site.CooldownLeft.ToString("F0") + " can0=" + site.CanInvestigate(0) + " begin0=" + site.Mark(0).Begin());
}
return "ok";
