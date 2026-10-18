// B-124 검증(플레이, 늪지 밤): 플레이어를 모기 스포너에 두고 12초 동안 MosquitoBite 판정을 감시 → 물기마다 주인 모기의 0.3초 이동량(돌진) 기록, 첫 물기 스크린샷 Logs/playtest/mosquito_bite.png. 결과 pb_mq
var log = new System.Text.StringBuilder();
System.AppDomain.CurrentDomain.SetData("pb_mq", log);
var map = GameObject.Find("Map").transform.Find("Lv1_Swamp");
var pc = PlayerController.instance;
foreach (var s in map.GetComponentsInChildren<EnemySpawner>(true))
{
    if (s.name == "Mosquito1" && s.enemyPositon != null && s.enemyPositon.transform.childCount > 0)
    {
        pc.transform.position = s.enemyPositon.transform.GetChild(0).position + new Vector3(0f, 1f, 0f);
    }
}
// 가장 가까운 모기에서 3유닛 떨어져 선다(돌진 거리 확인)
EnemyHealth nearest = null; float best = 1e9f;
foreach (var e in EnemyHealth.Active) { if (e != null && e.name.StartsWith("SwampMosquito")) { float d = Mathf.Abs(e.transform.position.x - pc.transform.position.x); if (d < best) { best = d; nearest = e; } } }
if (nearest != null) { pc.transform.position = new Vector3(nearest.transform.position.x - 3f, nearest.transform.position.y + 0.5f, pc.transform.position.z); }
var seen = new System.Collections.Generic.HashSet<int>();
int shots = 0;
for (int k = 0; k < 240; k++)
{
    DG.Tweening.DOVirtual.DelayedCall(k * 0.05f, () => {
        var ph = PlayerHealthManager.Instance; if (ph != null) { ph.nowHp = ph.fullHP; }
        var now = new System.Collections.Generic.HashSet<int>();
        foreach (var o in UnityEngine.Object.FindObjectsByType<EnemyLungeBite>(FindObjectsSortMode.None)) { now.Add(o.GetInstanceID()); }
        seen.IntersectWith(now); // 반환된 판정은 다음 등장 때 다시 센다
        foreach (var o in UnityEngine.Object.FindObjectsByType<EnemyLungeBite>(FindObjectsSortMode.None))
        {
            var t = o.transform.parent;
            if (t == null || !seen.Add(o.GetInstanceID())) { continue; }
            float x0 = t.position.x, px = pc.transform.position.x;
            DG.Tweening.DOVirtual.DelayedCall(0.3f, () => log.Append("bite by " + t.name + " gap " + (px - x0).ToString("F1") + " moved " + (t.position.x - x0).ToString("F2") + "; "), false);
            if (shots++ == 0) { DG.Tweening.DOVirtual.DelayedCall(0.1f, () => ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/playtest/mosquito_bite.png")), false); }
        }
    }, false);
}
DG.Tweening.DOVirtual.DelayedCall(12.5f, () => log.Append("DONE"), false);
return "watching";
