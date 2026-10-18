// 산적 대장 보스 검증(플레이 모드, 성채 낮, 보스 근처): ① HP 를 55% 깎아 2페이즈 → 부하 단검병 2명 소환(BossMinionCall) 확인 + 스크린샷
// ② 2.5초 뒤 처치 → 확정 보상 드롭 수 + 스크린샷. 결과는 pb_boss (Logs/_r.cs 로 읽음). 세이브 백업 후 실행(보스 처치 ES3 키가 생긴다)
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_boss", log);
EnemyHealth boss = null;
foreach (var e in EnemyHealth.Active) if (e != null && e.isBossMob && e.name.StartsWith("BanditChief")) boss = e;
if (boss == null) return "no boss";
System.Func<int> minions = () => { int n = 0; foreach (var e in EnemyHealth.Active) if (e != null && e.enemyNum == -1 && e.name.StartsWith("BanditCutthroat") && e.nowHP > 0) n++; return n; };
log.Append("boss hp " + boss.nowHP + "/" + boss.maxHP + " minions " + minions() + "; ");
boss.onlyDamage2Enemy(Mathf.RoundToInt(boss.maxHP * 0.55f));
var ph = boss.GetComponent<BossPhase>();
log.Append("after hit hp " + boss.nowHP + " phase " + (ph != null ? ph.Phase.ToString() : "none") + " minions " + minions() + "; ");
DG.Tweening.DOVirtual.DelayedCall(0.8f, () => {
    log.Append("0.8s minions " + minions() + " @");
    foreach (var e in EnemyHealth.Active) if (e != null && e.enemyNum == -1) log.Append(" " + e.transform.position.ToString("F1"));
    log.Append("; ");
    ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/bandit_boss_phase2.png"));
}, false);
DG.Tweening.DOVirtual.DelayedCall(2.5f, () => {
    int drops0 = UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude).Length;
    boss.onlyDamage2Enemy(999999);
    DG.Tweening.DOVirtual.DelayedCall(1.5f, () => {
        var names = new System.Collections.Generic.List<string>();
        foreach (var d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude)) names.Add(d.name.Replace("(Clone)", ""));
        log.Append("killed active=" + boss.gameObject.activeSelf + " drops " + drops0 + "→" + names.Count + " [" + string.Join(",", names.ToArray()) + "] minions " + minions() + "; ");
        ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/bandit_boss_kill.png"));
        log.Append("DONE");
    }, false);
}, false);
return "scheduled";
