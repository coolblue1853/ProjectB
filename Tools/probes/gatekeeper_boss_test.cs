// 탑의 문지기 검증(플레이 모드, 탑 근교 낮, 보스 근처, 조우 대사 뒤): ① HP 55% → 2페이즈·소형 골렘 부하 2 ② HP 80% → 3페이즈
// ③ 처치 → 확정 보상·칭호·탑 입구 포털 개방, 배너 순서(처치 → 승급 → 탑 입구) 스크린샷 3장. 결과 pb_gate (Logs/_r.cs). 세이브 백업 후 실행(처치·칭호 ES3 키가 생긴다)
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_gate", log);
EnemyHealth boss = null;
foreach (var e in EnemyHealth.Active) if (e != null && e.isBossMob && e.name.StartsWith("Gatekeeper")) boss = e;
if (boss == null) return "no boss";
var ph = boss.GetComponent<BossPhase>();
var gate = GameObject.Find("Map").transform.Find("Lv1_TowerOutskirts/Portal/Portal_ToTower");
System.Func<int> minions = () => { int n = 0; foreach (var e in EnemyHealth.Active) if (e != null && e.enemyNum == -1 && e.gameObject.activeSelf) n++; return n; };
log.Append("start hp " + boss.nowHP + "/" + boss.maxHP + " rank " + AscentLoad.Rank + " gate " + (gate != null && gate.gameObject.activeSelf) + "; ");
boss.onlyDamage2Enemy(Mathf.RoundToInt(boss.maxHP * 0.55f));
log.Append("55% phase " + (ph != null ? ph.Phase.ToString() : "none") + " minions " + minions() + "; ");
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => {
    log.Append("1s minions " + minions() + "; ");
    ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/gate_phase2.png"));
}, false);
DG.Tweening.DOVirtual.DelayedCall(3.5f, () => {
    boss.onlyDamage2Enemy(Mathf.RoundToInt(boss.maxHP * 0.25f));
    log.Append("80% phase " + (ph != null ? ph.Phase.ToString() : "none") + " hp " + boss.nowHP + "; ");
}, false);
DG.Tweening.DOVirtual.DelayedCall(6.0f, () => {
    boss.onlyDamage2Enemy(999999);
    DG.Tweening.DOVirtual.DelayedCall(0.6f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/gate_kill_banner1.png")), false);
    DG.Tweening.DOVirtual.DelayedCall(3.6f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/gate_kill_banner2.png")), false);
    DG.Tweening.DOVirtual.DelayedCall(6.0f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/gate_kill_banner3.png")), false);
    DG.Tweening.DOVirtual.DelayedCall(2.0f, () => {
        var names = new System.Collections.Generic.List<string>();
        foreach (var d in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude)) names.Add(d.name.Replace("(Clone)", ""));
        log.Append("killed active=" + boss.gameObject.activeSelf + " drops [" + string.Join(",", names.ToArray()) + "] rank " + AscentLoad.Rank + " gate " + (gate != null && gate.gameObject.activeSelf) + " firstKey " + ES3.KeyExists("bossFirstKill.Gatekeeper_BD") + "; ");
    }, false);
    DG.Tweening.DOVirtual.DelayedCall(4.0f, () => log.Append("4s minions " + minions() + "; DONE"), false);
}, false);
return "scheduled";
