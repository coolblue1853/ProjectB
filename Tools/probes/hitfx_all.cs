// 피격 플래시·국소 히트스톱 전수 확인(플레이 모드): 현재 활성 몬스터를 종류별 1마리씩 1 피해로 타격 → 같은 프레임 플래시 값·애니 속도·셰이더
var seen = new System.Collections.Generic.HashSet<string>();
var sb = new System.Text.StringBuilder();
int ok = 0, bad = 0;
foreach (var e in new System.Collections.Generic.List<EnemyHealth>(EnemyHealth.Active))
{
    if (e == null || !e.isActiveAndEnabled || !e.IsMonster || e.nowHP <= 5) continue;
    string n = e.name.Replace("_BD(Clone)", "");
    if (!seen.Add(n)) continue;
    e.damage2Enemy(new int[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f);
    var fx = e.GetComponent<SpriteHitFx>();
    bool good = fx != null && fx.IsReady && fx.CurrentFlashBlend > 0.5f && (e.GetComponent<Animator>() == null || fx.CurrentAnimSpeed == 0f);
    if (good) ok++; else bad++;
    sb.Append(n + (good ? " ok" : " BAD(blend " + (fx ? fx.CurrentFlashBlend : -1) + " anim " + (fx ? fx.CurrentAnimSpeed : -1) + " " + e.GetComponent<SpriteRenderer>().sharedMaterial.shader.name + ")") + "; ");
}
return "ok " + ok + " bad " + bad + " | " + sb;
