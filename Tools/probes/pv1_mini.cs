// PV1 (2) 미니보스 M1~M5 스폰·드롭 + 정예 둥지 3곳(늪·성채·탑) 작동·구역 인장 드롭. 결과 Logs/pv1/mini.log
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/mini.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
bool onlyNest = System.IO.File.Exists("Logs/pv1/onlynest");
var pc = PlayerController.instance; var rb = pc.GetComponent<Rigidbody2D>();
Time.timeScale = 1f; Application.runInBackground = true;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
if (TimeChange.instance != null) { TimeChange.instance.LockDaytime = true; }
L("day=" + (TimeChange.instance == null || TimeChange.instance.isDaytime));
var rows = MiniBossTable.All();
if (System.IO.File.Exists("Logs/pv1/only.txt")) { string oid = System.IO.File.ReadAllText("Logs/pv1/only.txt").Trim(); var rr = new System.Collections.Generic.List<MiniBossTable.Row>(); foreach (var r0 in rows) { if (r0.Id == oid) rr.Add(r0); } rows = rr; }
var nestMaps = new System.Collections.Generic.HashSet<string> { "Lv1_Swamp", "Lv1_BanditFortress", "Lv1_TowerOutskirts" };
var baseline = new System.Collections.Generic.HashSet<int>();
System.Action snapBase = () => { baseline.Clear(); foreach (var c in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (c.gameObject.activeInHierarchy) baseline.Add(c.GetInstanceID()); } };
System.Func<string> newDrops = () =>
{
    var d = new System.Collections.Generic.Dictionary<string, int>();
    foreach (var c in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
        if (!c.gameObject.activeInHierarchy || baseline.Contains(c.GetInstanceID())) continue;
        string k = c.name; d[k] = (d.ContainsKey(k) ? d[k] : 0) + Mathf.Max(1, c.nowStack);
    }
    var sb = new System.Text.StringBuilder(); foreach (var kv in d) sb.Append(kv.Key + "x" + kv.Value + " "); return sb.ToString();
};
bool begun = false; int ri = -1; int st = 0; float st0 = 0; MiniBossTable.Row row = null; MiniBossSite site = null; NestHuntSite nest = null; int mi = 0; bool didNest = false; EnemyHealth tracked = null;
UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; return; }
    try
    {
        float t = Time.time - st0;
        PlayerStatus.AcquireFor(StatusFlag.Invincible, 3f, "pv1");
        if (st == 0)
        {   // 다음 행
            ri++; if (ri >= rows.Count) { L("DONE"); UnityEditor.EditorApplication.update -= tick; return; }
            row = rows[ri]; didNest = false; mi = 0;
            bool ok = MapInfo.GoToMap(row.Map, pc.gameObject);
            pc.transform.position = new Vector3(row.X, row.Y + 1f, 0f); rb.linearVelocity = Vector2.zero;
            L("== " + row.Id + " map=" + row.Map + " goto=" + ok + " pos=(" + row.X + "," + row.Y + ") base=" + row.BaseMonster + " guaranteed=" + string.Join(",", row.Guaranteed.ConvertAll(s => s.Item + ":" + s.Count).ToArray()));
            st = onlyNest ? 4 : 1; st0 = Time.time; site = null; if (onlyNest && !nestMaps.Contains(row.Map)) { st = 0; } return;
        }
        if (st == 1)
        {   // 미니보스 스폰 대기
            if (site == null) { foreach (var s in UnityEngine.Object.FindObjectsByType<MiniBossSite>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { if (s.Row != null && s.Row.Id == row.Id) site = s; } }
            if (site != null && !site.Alive && t > 1f && t < 1.6f) { site.ResetCooldown(); }
            if (site != null && site.Alive)
            {
                var e = site.Enemy; L("  SPAWN after " + t.ToString("F1", ic) + "s name=" + e.name + " HP=" + e.maxHP + " elite=" + e.IsElite + " miniBoss=" + e.IsMiniBoss + " pos=" + e.transform.position.ToString("F1") + " contactDmg=" + e.ContactDamage);
                snapBase(); st = 2; st0 = Time.time; return;
            }
            if (t > 20f) { L("  FAIL 20초 안에 미니보스 안 나옴 site=" + (site != null) + " pcPos=" + pc.transform.position.ToString("F1")); st = 4; st0 = Time.time; }
            return;
        }
        if (st == 2)
        {
            if (t < 1f) { return; }
            if (site.Alive) { site.Enemy.ReleaseEnemy(true, true); }
            st = 3; st0 = Time.time; return;
        }
        if (st == 3)
        {
            if (t > 0.3f && t < 0.5f) { L("  DROPS@0.3 " + newDrops() + " pcPos=" + pc.transform.position.ToString("F1")); return; }
            if (t < 1.5f) { return; }
            L("  DROPS(mini) " + newDrops() + " retryMult=" + BossRetryRules.Multiplier(BossRetry.KillsToday(row.Id)));
            st = 4; st0 = Time.time; return;
        }
        if (st == 4)
        {   // 둥지
            if (!nestMaps.Contains(row.Map)) { st = 0; return; }
            if (nest == null)
            {
                foreach (var n in UnityEngine.Object.FindObjectsByType<NestHuntSite>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { if (n.gameObject.activeInHierarchy) nest = n; }
                if (nest == null) { if (t > 8f) { L("  NEST FAIL 둥지 사이트 없음"); st = 0; } return; }
                nest.ResetCooldown(); snapBase(); mi = 0; st = 5; st0 = Time.time; L("  NEST site=" + nest.transform.parent.name + " bonus=" + string.Join(",", nest.BonusLoot)); return;
            }
            st = 5; st0 = Time.time; return;
        }
        if (st == 5)
        {   // 표식 mi 번 이동·살피기 시작
            if (mi > 3) { st = 7; st0 = Time.time; return; }
            var m = nest.Mark(mi);
            pc.transform.position = new Vector3(m.transform.position.x, m.transform.position.y + 1.2f, 0f); rb.linearVelocity = Vector2.zero;
            begun = false; L("  mark" + mi + " can=" + nest.CanInvestigate(mi) + " vis=" + nest.IsVisible(mi));
            st = 6; st0 = Time.time; return;
        }
        if (st == 6)
        {
            var m = nest.Mark(mi);
            pc.transform.position = new Vector3(m.transform.position.x, m.transform.position.y + 1.2f, 0f); rb.linearVelocity = Vector2.zero;
            if (!begun) { begun = m.Begin(); if (begun) { L("  mark" + mi + " begin ok after " + t.ToString("F1", ic) + "s"); } }
            if (nest.State.Found > mi || nest.State.Phase == NestPhase.Success) { L("  mark" + mi + " done after " + t.ToString("F1", ic) + "s found=" + nest.State.Found + " phase=" + nest.State.Phase); mi++; st = 5; st0 = Time.time; return; }
            if (t > 8f) { L("  NEST FAIL mark" + mi + " 8초 안에 못 끝냄 phase=" + nest.State.Phase + " found=" + nest.State.Found); st = 0; nest = null; }
            return;
        }
        if (st == 7)
        {   // 정예 소환 확인 → 처치 → 인장
            tracked = EnemySpawner.TrackedTarget;
            if (tracked == null || tracked.IsInPool) { if (t > 4f) { L("  NEST FAIL 정예 소환 안 됨 phase=" + nest.State.Phase + " summoned=" + nest.SummonedName); st = 0; nest = null; } return; }
            L("  NEST ELITE " + tracked.name + " elite=" + tracked.IsElite + " HP=" + tracked.maxHP + " summoned=" + nest.SummonedName);
            snapBase(); tracked.ReleaseEnemy(true, true); st = 8; st0 = Time.time; return;
        }
        if (st == 8)
        {
            if (t < 1.5f) { return; }
            string dr = newDrops(); L("  NEST DROPS " + dr + " => seal=" + dr.Contains("ZoneSeal"));
            nest = null; st = 0; return;
        }
    }
    catch (System.Exception ex) { L("EXCEPTION st=" + st + " " + ex.Message + "\n" + ex.StackTrace); UnityEditor.EditorApplication.update -= tick; }
};
UnityEditor.EditorApplication.update += tick;
return "pv1_mini started rows=" + rows.Count;
