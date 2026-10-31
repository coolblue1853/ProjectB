// 골드 드롭 검증: 활성 몹 최대 40마리 처치 → 드롭 횟수·총액·바닥 더미 수 (수명 20초 이내 두 번째 프로브로 줍기)
int n = 0, normal = 0; var lvSum = 0;
foreach (var e in new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active))
{
    if (n >= 40) break;
    if (!e.IsMonster || e.isBossMob || e.nowHP <= 0) continue;
    var r = MonsterTable.Get(e.name); lvSum += r != null && r.level.HasValue ? r.level.Value : 0;
    if (!e.IsElite) normal++;
    e.onlyDamage2Enemy(999999); n++;
}
int piles = 0, onGround = 0;
foreach (var d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude)) if (d.goldAmount > 0) { piles++; }
return "killed=" + n + " normal=" + normal + " avgLv=" + (n > 0 ? (float)lvSum / n : 0) + " goldDrops(mobs)=" + DropManager.GoldDroppedCount + " goldTotal=" + DropManager.GoldDroppedTotal + " activePiles=" + piles + " money=" + DatabaseManager.money;
