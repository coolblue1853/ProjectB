// PV1 (4) L1 퀘스트 목표 타입 killElite·equip·enhance·bestiary 진행 갱신. 결과 Logs/pv1/quest.log
var ic = System.Globalization.CultureInfo.InvariantCulture;
string logPath = "Logs/pv1/quest.log"; System.IO.File.WriteAllText(logPath, "");
System.Action<string> L = (m) => { System.IO.File.AppendAllText(logPath, m + "\n"); };
var pc = PlayerController.instance; var rb = pc.GetComponent<Rigidbody2D>(); var inv = InventoryManager.instance;
Time.timeScale = 1f; Application.runInBackground = true;
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
if (TimeChange.instance != null) { TimeChange.instance.LockDaytime = true; }
string orig = NpcQuestLog.Serialize(); L("orig quest state saved: " + orig.Length + " chars");
NpcQuestLog.LevelGetter = () => 99;
// 선행 퀘스트 체인 완료 처리
var ids = new[] { "Q207", "Q308", "Q309", "Q307", "Q409", "Q610" };
var done = new System.Collections.Generic.HashSet<string>();
System.Action<string> mark = null;
mark = (id) => { var d = NpcQuestCatalog.Find(id); if (d == null || string.IsNullOrEmpty(d.Requires) || done.Contains(d.Requires)) return; done.Add(d.Requires); mark(d.Requires); };
foreach (var id in ids) mark(id);
var sbS = new System.Text.StringBuilder(orig); foreach (var dn in done) { if (!NpcQuestLog.IsDone(dn)) { if (sbS.Length > 0) sbS.Append(';'); sbS.Append(dn + ":2:" + NpcQuestCatalog.Find(dn).Count); } }
NpcQuestLog.Deserialize(sbS.ToString());
foreach (var id in ids)
{
    var d = NpcQuestCatalog.Find(id);
    bool acc = NpcQuestLog.Accept(id);
    L("ACCEPT " + id + " (" + d.Objective + " " + d.Target + " x" + d.Count + ") accepted=" + acc + " progress=" + NpcQuestLog.Progress(d) + "/" + d.Count + " status=" + NpcQuestLog.StatusOf(d));
}
System.Func<string, string> P = (id) => { var d = NpcQuestCatalog.Find(id); return id + "=" + NpcQuestLog.Progress(d) + "/" + d.Count + "(" + NpcQuestLog.StatusOf(d) + ")"; };
// ---- B. 착용 ----
{
    var parts = new[] { "GroveCap", "GroveArmor", "GroveLegArmor" };
    foreach (var ic0 in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {   // 가방 칸 3개 비우기(세이브는 PV1 이 복원)
        if (parts.Length == 0) break;
    }
    int freed = 0;
    foreach (var ic1 in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
        if (freed >= 3) break;
        if (ic1.itemData == null || !ic1.transform.IsChildOf(inv.transform) || ic1.itemData.type == "Equip" || ic1.itemData.type == "Consum") continue;
        inv.Service.RemoveAt(ic1, Mathf.Max(1, ic1.nowStack)); freed++;
    }
    L("  freed slots " + freed);
    foreach (var pn in parts)
    {
        var addRes = inv.Service.Add(pn, 1, AddSource.Other); L("  add " + pn + " -> " + addRes);
        ItemCheck it = null; foreach (var c in inv.EnumerateInventoryItems()) { if (c.itemData != null && c.itemData.name == pn) { it = c; break; } }
        if (it == null) { L("EQUIP " + pn + " 인벤에서 못 찾음"); continue; }
        inv.EquipFromInventory(it);
        L("EQUIP " + pn + " -> setsEffectStack[GroveArmor]=" + (DatabaseManager.setsEffectStack.ContainsKey("GroveArmor") ? DatabaseManager.setsEffectStack["GroveArmor"] : 0) + " | " + P("Q307"));
    }
}
// ---- C. 강화 ----
{
    var wp = inv.EquippedFor("Weapon");
    if (wp == null) { L("ENHANCE 무기 미착용"); }
    else
    {
        var box = wp.transform.parent.GetComponent<EquipBoxCheck>();
        wp.itemData.upgrade = 0; box.RefreshEnhancement(wp);
        // Q409 는 수락 시 시드가 이미 잡혔다(수락 전 +N). 진행 갱신만 본다.
        for (int i = 0; i < 40; i++) { inv.CreatItem("Enhance Stone"); }
        DatabaseManager.money += 200000;
        int tries = 0; int lastLv = 0;
        while (tries < 60 && wp.itemData.upgrade < 3)
        {
            EnhanceRules.Result res; int nl;
            if (EnhanceService.CanTry(wp) != EnhanceService.Check.Ok) { L("ENHANCE 시도 불가 " + EnhanceService.CanTry(wp)); break; }
            EnhanceService.TryEnhance(wp, out res, out nl); tries++;
            if (wp.itemData.upgrade != lastLv) { lastLv = wp.itemData.upgrade; L("ENHANCE try#" + tries + " " + res + " weapon +" + lastLv + " | " + P("Q409")); }
        }
        L("ENHANCE END weapon +" + wp.itemData.upgrade + " tries=" + tries + " | " + P("Q409"));
    }
}
// ---- D/E. 정예 처치 + 도감 (틱) ----
int st = 0; float st0 = 0; MiniBossSite site = null; var row = MiniBossTable.All()[0]; int kindsBefore = 0; int kIdx = 0;
var killedKinds = new System.Collections.Generic.HashSet<string>();
UnityEditor.EditorApplication.CallbackFunction tick = null;
System.Action finish = () =>
{
    NpcQuestLog.Deserialize(orig); NpcQuestLog.LevelGetter = () => LevelSystem.Level;
    L("RESTORED quest state; DONE"); UnityEditor.EditorApplication.update -= tick;
};
tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; return; }
    try
    {
        PlayerStatus.AcquireFor(StatusFlag.Invincible, 3f, "pv1");
        float t = Time.time - st0;
        if (st == 0)
        {
            MapInfo.GoToMap(row.Map, pc.gameObject); pc.transform.position = new Vector3(row.X, row.Y + 1f, 0f); rb.linearVelocity = Vector2.zero;
            L("ELITE row=" + row.Id + " before: " + P("Q207") + " " + P("Q308") + " " + P("Q309") + " | bestiary kinds=" + BestiaryRecord.KindsKilled + " " + P("Q610"));
            st = 1; st0 = Time.time; return;
        }
        if (st == 1)
        {
            if (site == null) { foreach (var s in UnityEngine.Object.FindObjectsByType<MiniBossSite>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { if (s.Row != null && s.Row.Id == row.Id) site = s; } }
            if (site != null && !site.Alive && t > 1f && t < 1.6f) { site.ResetCooldown(); }
            if (site != null && site.Alive) { L("  spawned " + site.Enemy.MonsterId + " elite=" + site.Enemy.IsElite); kindsBefore = BestiaryRecord.KindsKilled; site.Enemy.damage2Enemy(new int[] { 9999999, 9999999, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 0f, Vector2.zero, 0f, false, false, 0f, 1f); st = 2; st0 = Time.time; return; }
            if (t > 20f) { L("  FAIL 미니보스 안 나옴"); finish(); }
            return;
        }
        if (st == 2)
        {
            if (t < 1.2f) { return; }
            L("ELITE after kill: " + P("Q207") + " " + P("Q308") + " " + P("Q309") + " | bestiary kinds " + kindsBefore + "->" + BestiaryRecord.KindsKilled + " " + P("Q610"));
            st = 3; st0 = Time.time; return;
        }
        if (st == 3)
        {   // 서로 다른 종류의 일반 몹 처치
            if (t < 1f) { return; }
            if (t > 40f) { L("  종류 수집 시간 초과"); finish(); return; }
            EnemyHealth pick = null;
            foreach (var e in EnemyHealth.Active) { if (e != null && e.gameObject.activeInHierarchy && e.IsMonster && !e.IsElite && !string.IsNullOrEmpty(e.MonsterId) && BestiaryRecord.Kills(e.MonsterId) == 0 && !killedKinds.Contains(e.MonsterId)) { pick = e; break; } }
            if (pick == null)
            {   // 새 종류가 없다 → 이미 많이 죽여 본 구역: 다른 구역으로
                string[] zs = { "Lv1_NearForest_SideRoad", "Lv1_DeepForest", "Lv1_Swamp", "Lv1_BanditFortress", "Lv1_TowerOutskirts", "Lv1_NearForest" };
                if (kIdx < zs.Length && t > 3f) { var map = GameObject.Find("Map").transform.Find(zs[kIdx]); MapInfo.GoToMap(zs[kIdx], pc.gameObject); foreach (var s in map.GetComponentsInChildren<EnemySpawner>(false)) { if (s.isActiveAndEnabled && s.enemyPositon != null) { var col = s.GetComponent<Collider2D>(); var c = col != null ? (Vector3)col.bounds.center : s.transform.position; pc.transform.position = new Vector3(c.x, c.y + 0.5f, 0f); break; } } kIdx++; st0 = Time.time - 0.5f; }
                return;
            }
            int kb = BestiaryRecord.KindsKilled; string mid = pick.MonsterId; killedKinds.Add(mid); pick.damage2Enemy(new int[] { 9999999, 9999999, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 0f, Vector2.zero, 0f, false, false, 0f, 1f);
            st = 4; st0 = Time.time; L("  kill new-kind " + mid + " kinds before=" + kb); return;
        }
        if (st == 4)
        {
            if (t < 0.6f) { return; }
            L("  -> kinds=" + BestiaryRecord.KindsKilled + " " + P("Q610"));
            if (killedKinds.Count >= 3) { finish(); return; }
            st = 3; st0 = Time.time; return;
        }
    }
    catch (System.Exception ex) { L("EXCEPTION st=" + st + " " + ex.Message + "\n" + ex.StackTrace); finish(); }
};
UnityEditor.EditorApplication.update += tick;
return "pv1_quest started";
