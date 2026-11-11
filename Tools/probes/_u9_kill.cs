// BUG-U9 계측: 활성 몹 전부를 일반 사망/강제 반환 번갈아 처리
int n = 0;
var list = new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active);
foreach (var e in list)
{
    if (e == null || !e.isActiveAndEnabled) { continue; }
    if (n % 2 == 0) { e.ReleaseEnemy(true, true); } else { e.ForceEnemyRelease(); }
    n++;
}
return "released " + n;
