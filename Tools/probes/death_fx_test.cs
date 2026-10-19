// 사망 연출 확인(플레이 모드): 가장 가까운 일반 몬스터(__ELITE__=1 이면 정예 승격 후)를 처치 → 0.25s·0.95s 스크린샷, 연기 재생 수·조각 수 기록 (결과 pb_death)
var pc = PlayerController.instance;
EnemyHealth t = null; float best = 1e9f;
foreach (var e in EnemyHealth.Active) { if (e == null || !e.isActiveAndEnabled || !e.IsMonster || e.isBossMob || e.nowHP <= 0) continue; float d = Mathf.Abs(e.transform.position.x - pc.transform.position.x); if (d < best) { best = d; t = e; } }
if (t == null) return "no mob";
if (__ELITE__ == 1) t.PromoteElite();
pc.transform.position = t.transform.position + new Vector3(-3.5f, 0.5f, 0f);
int before = DeathFx.PlayCount;
string tag = "__TAG__";
System.AppDomain.CurrentDomain.SetData("pb_death", "pending");
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => {
    t.onlyDamage2Enemy(9999999);
    int played = DeathFx.PlayCount - before;
    DG.Tweening.DOVirtual.DelayedCall(0.2f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/death_" + tag + "_a.png")), false);
    DG.Tweening.DOVirtual.DelayedCall(0.95f, () => {
        ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/death_" + tag + "_b.png"));
        System.AppDomain.CurrentDomain.SetData("pb_death", t.name + " deathFx+" + played + " shards " + DeathShards.LiveCount + " vfx " + Vfx.ActiveCount);
    }, false);
}, false);
return "target " + t.name + " d " + best.ToString("F1");
