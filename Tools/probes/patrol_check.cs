// 활성 몬스터들의 스폰 위치 대비 현재 거리(리쉬 동작 확인)
var sb = new System.Text.StringBuilder();
float maxDist = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{
    if (!e.hasSpawnPosition) continue;
    float d = Mathf.Abs(e.transform.position.x - e.spawnPosition.x);
    maxDist = Mathf.Max(maxDist, d);
    sb.Append(e.name.Replace("_BD(Clone)", "") + ":" + d.ToString("F1") + " ");
}
return "maxDistFromSpawn=" + maxDist.ToString("F1") + " | " + sb;
