// B-127 검증(플레이 모드, 보스 근처): 이름이 __BOSS__ 로 시작하는 보스 HP 를 55% 깎아 2페이즈 부하 소환 → 1초 뒤 처치 →
// 2.5초 뒤(DisaperByTime 1.5s) 살아 있는 부하(enemyNum −1) 수가 0 인지. 결과 pb_minion (Logs/_r.cs 로 읽음). 세이브 백업 후 실행
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_minion", log);
EnemyHealth boss = null;
foreach (var e in EnemyHealth.Active) if (e != null && e.isBossMob && e.name.StartsWith("__BOSS__")) boss = e;
if (boss == null) return "no boss";
System.Func<int> minions = () => { int n = 0; foreach (var e in EnemyHealth.Active) if (e != null && e.enemyNum == -1 && e.gameObject.activeSelf) n++; return n; };
boss.onlyDamage2Enemy(Mathf.RoundToInt(boss.maxHP * 0.55f));
log.Append("phase2 minions " + minions() + "; ");
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => {
    log.Append("before kill minions " + minions() + "; ");
    boss.onlyDamage2Enemy(999999);
    DG.Tweening.DOVirtual.DelayedCall(0.5f, () => { log.Append("0.5s after kill minions " + minions() + "; "); }, false);
    DG.Tweening.DOVirtual.DelayedCall(2.5f, () => { log.Append("2.5s after kill minions " + minions() + "; DONE"); }, false);
}, false);
return "scheduled";
