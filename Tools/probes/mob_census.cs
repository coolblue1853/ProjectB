// 활성 몬스터를 이름별로 센다(플레이 모드): 보스 HP·위치 포함. 구역 스폰 검증용
var cnt = new System.Collections.Generic.SortedDictionary<string, int>();
var sb = new System.Text.StringBuilder();
foreach (var e in EnemyHealth.Active)
{
    if (e == null || !e.isActiveAndEnabled) continue;
    string n = e.name.Replace("_BD(Clone)", "");
    int c; cnt.TryGetValue(n, out c); cnt[n] = c + 1;
    if (e.isBossMob) sb.Append("[boss " + n + " hp " + e.nowHP + "/" + e.maxHP + " @" + e.transform.position.ToString("F0") + "] ");
}
foreach (var kv in cnt) sb.Append(kv.Key + "=" + kv.Value + " ");
var pc = PlayerController.instance;
return sb.ToString() + "| player " + pc.transform.position.ToString("F0") + " day=" + TimeChange.instance.isDaytime;
