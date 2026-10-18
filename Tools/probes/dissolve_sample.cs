// 처치 후 시체 디졸브 값을 0.5/1.5/1.9초에 Debug.Log로 샘플링 (read_console로 확인)
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 0 && d < best) { best = d; t = e; } }
if (t == null) return "no enemy";
t.onlyDamage2Enemy(99999);
DeadBody body = null;
foreach (var b in UnityEngine.Object.FindObjectsByType<DeadBody>(FindObjectsInactive.Exclude)) body = b;
if (body == null) return "no body";
foreach (var at in new[]{ 0.5f, 1.5f, 1.9f })
{
    float when = at;
    DG.Tweening.DOVirtual.DelayedCall(when, () => {
        var r = body.transform.GetChild(0).GetComponent<SpriteRenderer>();
        Debug.Log("[probe] dissolve t=" + when + " fade=" + r.material.GetFloat("_FadeAmount").ToString("F2") + " shader=" + r.sharedMaterial.shader.name);
    }, false); // 게임 시간 기준 (최소화 시 실시간과 다름)
}
return "sampling " + body.name;
