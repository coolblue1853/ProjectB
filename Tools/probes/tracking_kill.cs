// 추적 정예 처치(MiniGames.md ⑥, 플레이 중): 플레이어를 정예 옆으로 옮기고 HP 를 넘는 피해로 사망 처리 → 0.3초 뒤 새 바닥 드롭 목록을 "[TrkKill] ..." 에 기록
// __MODE__ = kill(처치) / expire(제한 시간 즉시 만료 → 보상 없이 사라짐)
var t = EnemySpawner.TrackedTarget;
if (t == null || t.IsInPool) return "no tracked";
foreach (var g in UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsSortMode.None)) if (g.name.StartsWith("[TrkKill]")) UnityEngine.Object.Destroy(g);
var log = new UnityEngine.GameObject("[TrkKill] __MODE__ " + t.gameObject.name + " elite=" + t.IsElite);
var before = new System.Collections.Generic.HashSet<DropItemCheck>(UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsSortMode.None));
if ("__MODE__" == "expire")
{
    typeof(EnemySpawner).GetField("trackedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(t.enemySpowner, 0f);
}
else
{
    t.onlyDamage2Enemy(t.nowHP + 1);
}
DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(0.3f, () =>
{
    var sb = new System.Text.StringBuilder(" drops:");
    foreach (var d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsSortMode.None)) if (!before.Contains(d) && d.itemData != null) sb.Append(" " + d.itemData.name);
    log.name += sb + " has=" + EnemySpawner.HasTrackedTarget;
}), true);
DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(2.5f, () => { log.name += " later.has=" + EnemySpawner.HasTrackedTarget + " pool=" + t.IsInPool; }), true);
return "ok " + t.gameObject.name;
