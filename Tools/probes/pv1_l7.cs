// PV1 (3) L7 신규 몹 6종 스폰 비율·드롭. 결과 Logs/pv1/l7.log
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/l7.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
var pc = PlayerController.instance; var rb = pc.GetComponent<Rigidbody2D>();
Time.timeScale = 1f; Application.runInBackground = true;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
if (TimeChange.instance != null) { TimeChange.instance.LockDaytime = true; }
string[][] tgt = new string[][] {
    new[] { "Lv1_NormalForest", "Rabbit2_2", "ForestHunter_BD", "0.3" }, new[] { "Lv1_NormalForest", "Mole3", "VineFairy_BD", "0.3" },
    new[] { "Lv1_DeepForest", "Ent1", "ForestOgre_BD", "0.4" }, new[] { "Lv1_BanditFortress", "Archer2", "BanditSentry_BD", "0.35" },
    new[] { "Lv1_BanditFortress", "Cutthroat3", "BanditWoodcutter_BD", "0.35" }, new[] { "Lv1_TowerOutskirts", "GolemSmall2", "RuinWarden_BD", "0.3" } };
int ti = -1, st = 0; float st0 = 0; EnemySpawner sp = null; string curMap = "";
var baseline = new System.Collections.Generic.HashSet<int>();
var dropTally = new System.Collections.Generic.Dictionary<string, int>();
UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; return; }
    try
    {
        PlayerStatus.AcquireFor(StatusFlag.Invincible, 3f, "pv1");
        float t = Time.time - st0;
        if (st == 0)
        {
            ti++; if (ti >= tgt.Length) { L("DONE"); UnityEditor.EditorApplication.update -= tick; return; }
            var T = tgt[ti]; curMap = T[0];
            bool ok = MapInfo.GoToMap(T[0], pc.gameObject); sp = null;
            var map = GameObject.Find("Map").transform.Find(T[0]);
            foreach (var s in map.GetComponentsInChildren<EnemySpawner>(true)) { if (s.name == T[1]) sp = s; }
            L("== " + T[0] + "/" + T[1] + " expect " + T[2] + "@" + T[3] + " goto=" + ok + " spawner=" + (sp != null) + (sp != null ? " day=" + sp.isDaytimeSpowner + " enabled=" + sp.isActiveAndEnabled : ""));
            if (sp == null) { return; }
            var col = sp.GetComponent<Collider2D>(); var c = col != null ? (Vector3)col.bounds.center : sp.transform.position;
            pc.transform.position = new Vector3(c.x, c.y + 0.5f, 0f); rb.linearVelocity = Vector2.zero;
            st = 1; st0 = Time.time; return;
        }
        if (st == 1)
        {
            if (t < 30f) { return; }
            var T = tgt[ti]; int total = 0, nw = 0; var byId = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var e in EnemyHealth.Active)
            {
                if (e == null || !e.gameObject.activeInHierarchy || e.enemySpowner != sp) continue;
                total++; string id = string.IsNullOrEmpty(e.MonsterId) ? "(host)" + e.name : e.MonsterId; byId[id] = (byId.ContainsKey(id) ? byId[id] : 0) + 1; if (e.MonsterId == T[2]) nw++;
            }
            var sb = new System.Text.StringBuilder(); foreach (var kv in byId) sb.Append(kv.Key + "x" + kv.Value + " ");
            L("  SPAWN after " + t.ToString("F0") + "s total=" + total + " new=" + nw + " ratio=" + (total > 0 ? ((float)nw / total).ToString("F2", ic) : "-") + " | " + sb);
            // 추가 드롭 표
            var ex = ExtraDrops.For(T[2]); var exs = ""; if (ex != null) { foreach (var en in ex) exs += en.Item + "@" + en.Probability.ToString("F2", ic) + (en.QuestOnly ? "(quest)" : "") + " "; }
            L("  EXTRA-DROPS(table) " + (ex == null ? "none" : exs));
            // 처치 → 드롭 집계
            baseline.Clear(); foreach (var cc in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (cc.gameObject.activeInHierarchy) baseline.Add(cc.GetInstanceID()); }
            int killed = 0; var list = new System.Collections.Generic.List<EnemyHealth>();
            foreach (var e in EnemyHealth.Active) { if (e != null && e.gameObject.activeInHierarchy && e.MonsterId == T[2]) list.Add(e); }
            foreach (var e in list) { e.ReleaseEnemy(true, true); killed++; }
            L("  KILLED new-mobs " + killed + " lv=" + (list.Count > 0 ? list[0].Level.ToString() : "-") + " hp=" + (list.Count > 0 ? list[0].maxHP.ToString() : "-") + " contact=" + (list.Count > 0 ? list[0].ContactDamage.ToString() : "-"));
            st = 2; st0 = Time.time; return;
        }
        if (st == 2)
        {
            if (t < 1.5f) { return; }
            var d = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var cc in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (!cc.gameObject.activeInHierarchy || baseline.Contains(cc.GetInstanceID())) continue; d[cc.name] = (d.ContainsKey(cc.name) ? d[cc.name] : 0) + Mathf.Max(1, cc.nowStack); }
            var sb = new System.Text.StringBuilder(); foreach (var kv in d) sb.Append(kv.Key + "x" + kv.Value + " ");
            L("  DROPS " + sb);
            st = 0; return;
        }
    }
    catch (System.Exception ex) { L("EXCEPTION st=" + st + " " + ex.Message + "\n" + ex.StackTrace); UnityEditor.EditorApplication.update -= tick; }
};
UnityEditor.EditorApplication.update += tick;
return "pv1_l7 started";
