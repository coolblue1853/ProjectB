// AUTO-115: 1층계 구간별 자동 점검(플레이 모드). 이 파일은 템플릿 — 토큰 __MAP__ __LO__ __HI__ __PH__ 를 치환해 실행(Tools/f1_fullrun.sh)
// PH=goto: 맵 이동+레벨 설정 / PH=spawn: 스포너 위로 이동·CycleCheck / PH=kill: 대표 몹 최대 10마리 실제 처치(damage2Enemy 반복) → 레벨·EXP·골드·모델 시간
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
string PH = "__PH__";
if (PH == "goto")
{
    var prop = typeof(LevelSystem).GetProperty("Level");
    prop.SetValue(null, __LO__); typeof(LevelSystem).GetProperty("Exp").SetValue(null, 0);
    bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
    return "goto __MAP__ " + ok + " lv " + LevelSystem.Level + " pos " + pc.transform.position;
}
if (PH == "spawn")
{
    var sps = new System.Collections.Generic.List<EnemySpawner>(UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude));
    if (sps.Count > 0)
    {
        var s0 = sps[sps.Count / 2]; var col = s0.GetComponent<Collider2D>(); var sp = col != null ? (Vector3)col.bounds.center : s0.transform.position;
        pc.transform.position = new Vector3(sp.x, sp.y, 0);
        foreach (var s in sps) s.SendMessage("CycleCheck", SendMessageOptions.DontRequireReceiver);
    }
    return "spawners " + sps.Count + " pos " + pc.transform.position;
}
var cands = new System.Collections.Generic.List<EnemyHealth>();
foreach (var e in EnemyHealth.Active) { if (e != null && e.isActiveAndEnabled && e.IsMonster && !e.isBossMob && e.nowHP > 0) cands.Add(e); }
cands.Sort((a, b) => (a.transform.position - pc.transform.position).sqrMagnitude.CompareTo((b.transform.position - pc.transform.position).sqrMagnitude));
var perName = new System.Collections.Generic.Dictionary<string, int>();
int lv0 = LevelSystem.Level, ex0 = LevelSystem.Exp, m0 = DatabaseManager.money;
var off = DamageCalculator.PlayerOffense.FromDatabase();
int[] arr = Weapon.Equipped != null ? Weapon.Equipped.damgeArray : new int[10];
int kills = 0, hitsTotal = 0; float simSec = 0f; var sb = new System.Text.StringBuilder();
float approach = 3f, perHit = 0.4f;
foreach (var e in cands)
{
    if (kills >= 10) break;
    string n = e.name.Replace("(Clone)", "");
    int c; perName.TryGetValue(n, out c); if (c >= 3) continue; perName[n] = c + 1;
    int per = DamageCalculator.Calculate(arr, off, e.enemyDef, false, false, 1f, 0.99f, 99f, DamageCalculator.LevelGapMul(e.Level, LevelSystem.Level)).damage;
    if (per < 1) per = 1;
    int hits = Mathf.CeilToInt(e.nowHP / (float)per);
    int guard = 0;
    while (e != null && e.nowHP > 0 && guard++ < 400) e.damage2Enemy(arr, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f);
    kills++; hitsTotal += hits; simSec += approach + hits * perHit;
    sb.Append(n + "(HP" + (e != null ? e.maxHP : 0) + ",def" + e.enemyDef + ",hit" + per + "x" + hits + ") ");
}
int gained = 0; // 레벨업 걸치면 누적 계산
int lv1 = LevelSystem.Level; for (int l = lv0; l < lv1; l++) gained += LevelRules.ExpToNext(l);
gained += LevelSystem.Exp - ex0;
float expPerKill = kills > 0 ? gained / (float)kills : 0f;
float secPerKill = kills > 0 ? simSec / kills : 0f;
int need = 0; for (int l = __LO__; l < __HI__; l++) need += LevelRules.ExpToNext(l);
float killsNeed = expPerKill > 0 ? need / expPerKill : -1f;
return "MAP __MAP__ cands " + cands.Count + " kills " + kills + " | lv " + lv0 + "->" + lv1 + " exp+" + gained + " gold+" + (DatabaseManager.money - m0) + " | exp/kill " + expPerKill.ToString("F1") + " sec/kill(model) " + secPerKill.ToString("F1") + " | need(Lv__LO__->__HI__) " + need + " kills~" + killsNeed.ToString("F0") + " min(k1)~" + (killsNeed * secPerKill / 60f).ToString("F0") + " | " + sb;
