// PT-BOT 캠페인: 퀘스트 흐름대로 Lv1 -> 졸업(Q604) 자동 통주. 설정 Logs/ptbot/config.txt, 로그 Logs/ptbot/run.log, 결과 Logs/ptbot/summary.txt, 중지 Logs/ptbot/stop.
// 계획기: 완료 가능 > 수락 가능 > 진행 중 > 레벨 부족(사냥터 그라인드). 목표별 작업: talk / kill(몹·보스) / elite(미니보스) / collect(사냥 후 부족분 치트) / cheat(제작·방문·강화).
// 기존 ptbot_run 의 입력·전투·대화 코드를 재사용(가상 키 + Weapon.MeleeAttack). 보스·미니보스는 공격 애니(isAttack) 때 후퇴.
var cfg = new System.Collections.Generic.Dictionary<string, string>();
foreach (var ln in System.IO.File.ReadAllLines("Logs/ptbot/config.txt")) { int eq = ln.IndexOf('='); if (eq > 0 && !ln.StartsWith("#")) cfg[ln.Substring(0, eq).Trim()] = ln.Substring(eq + 1).Trim(); }
var ic = System.Globalization.CultureInfo.InvariantCulture;
System.Func<string, string, string> C = (k, d) => cfg.ContainsKey(k) ? cfg[k] : d;
System.Func<string, float, float> CF = (k, d) => { float v; return float.TryParse(C(k, ""), System.Globalization.NumberStyles.Float, ic, out v) ? v : d; };
float totalSec = CF("totalRealSec", 3000f), logEvery = CF("logEvery", 10f), stuckSec = CF("stuckSec", 6f), potionPct = CF("potionPct", 0.5f);
float ts = Mathf.Clamp(CF("timescale", 3f), 0.5f, 4f);
float huntTimeout = CF("huntTimeout", 420f), moveCostSec = CF("moveCostSec", 45f);
bool god = C("god", "0") == "1";
var skipSet = new System.Collections.Generic.HashSet<string>(C("skip", "Q104,Q610,Q501,Q502,Q503,Q504,Q505,Q506,Q507,Q508,Q509,Q701,Q702,Q703,Q704").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
string finishQuest = C("finishQuest", "Q604");
string logPath = "Logs/ptbot/run.log", sumPath = "Logs/ptbot/summary.txt", stopPath = "Logs/ptbot/stop";
System.IO.File.WriteAllText(logPath, "");

var pc = PlayerController.instance; var hm = PlayerHealthManager.Instance; var inv = InventoryManager.instance;
var iset = UnityEngine.InputSystem.InputSystem.settings;
iset.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
iset.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var kb = UnityEngine.InputSystem.Keyboard.current;
UnityEngine.InputSystem.InputSystem.EnableDevice(kb);
Application.runInBackground = true;
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
var tc = TimeChange.instance;
System.Action<bool> setDay = (day) =>
{
    if (tc == null) return;
    if (day) { tc.LockDaytime = true; if (!tc.isDaytime) { float tg = (tc.endNighttime + 0.05f) * tc.totalTime; float ad = tg - tc.value * tc.totalTime; if (ad < 0) ad += tc.totalTime; tc.AddTime(ad); } }
    else { tc.LockDaytime = false; if (tc.isDaytime) { float tg = (tc.startNighttime + 0.05f) * tc.totalTime; float ad = tg - tc.value * tc.totalTime; if (ad < 0) ad += tc.totalTime; tc.AddTime(ad); } }
};
setDay(true);
Time.timeScale = ts;

// ---- 상태 ----
var K = UnityEngine.InputSystem.Key.X;
int kills = 0, deaths = 0, stucks = 0, potions = 0, errors = 0, bossKills = 0, eliteKills = 0, mapMoves = 0, talks = 0, cheats = 0;
float cheatModelMin = 0f;
var errSet = new System.Collections.Generic.List<string>();
var mapSec = new System.Collections.Generic.Dictionary<string, float>(); var mapKills = new System.Collections.Generic.Dictionary<string, int>(); var mapDeaths = new System.Collections.Generic.Dictionary<string, int>();
var qStart = new System.Collections.Generic.Dictionary<string, float>(); var qLines = new System.Collections.Generic.List<string>();
var bossFails = new System.Collections.Generic.Dictionary<string, int>();
var npcBlock = new System.Collections.Generic.Dictionary<string, float>();
var talkTries = new System.Collections.Generic.Dictionary<string, int>();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
float realStart = Time.realtimeSinceStartup, gameStart = Time.time;
System.Action<string> Log = (m) => { System.IO.File.AppendAllText(logPath, "[" + (Time.time - gameStart).ToString("F0", ic) + "s] " + m + "\n"); };
var maps = new System.Collections.Generic.List<MapInfo>(UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None));
System.Func<string> curMap = () => { foreach (var m in maps) { if (m != null && m.gameObject.activeInHierarchy) return m.name; } return ""; };
System.Func<string> Snap = () => "map=" + curMap() + " pos=" + pc.transform.position.x.ToString("F0", ic) + "," + pc.transform.position.y.ToString("F0", ic) + " hp=" + hm.nowHp + "/" + hm.fullHP + " lv=" + LevelSystem.Level + " exp=" + LevelSystem.Exp + " gold=" + DatabaseManager.money + " k=" + kills + " d=" + deaths;
System.Action<EnemyHealth> onKilled = (e) => { kills++; string m = curMap(); mapKills[m] = (mapKills.ContainsKey(m) ? mapKills[m] : 0) + 1; if (e != null && e.isBossMob) { bossKills++; Log("BOSS_KILLED " + e.name); } else if (e != null && (e.IsElite || e.IsMiniBoss)) { eliteKills++; Log("ELITE_KILLED " + e.name); } };
System.Action onDied = () => { deaths++; string m = curMap(); mapDeaths[m] = (mapDeaths.ContainsKey(m) ? mapDeaths[m] : 0) + 1; Log("DEATH " + Snap()); };
UnityEngine.Application.LogCallback onLog = (msg, st, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) { errors++; string k = msg.Split('\n')[0]; if (k.Length > 200) k = k.Substring(0, 200); if (errSet.Count < 40 && !errSet.Contains(k)) { errSet.Add(k); Log("CONSOLE_ERROR " + k); } } };
EnemyHealth.Killed += onKilled; PlayerHealthManager.PlayerDied += onDied; Application.logMessageReceived += onLog;

// ---- 색인: 몹 -> 스포너, 보스/미니보스 ----
var mobSp = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<EnemySpawner>>();
foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    string pn = (sp.MonsterProfile ?? "").Split('@')[0]; if (string.IsNullOrEmpty(pn) || pn == "Rock" || pn == "tree" || pn == "GreenHurb") continue;
    if (sp.GetComponentInParent<MapInfo>(true) == null) continue;
    if (!mobSp.ContainsKey(pn)) mobSp[pn] = new System.Collections.Generic.List<EnemySpawner>(); mobSp[pn].Add(sp);
}
var bossSet = new System.Collections.Generic.HashSet<string> { "AncientEnt_BD", "GiantSlime_BD", "Gatekeeper_BD" };
System.Func<EnemySpawner, string> spMap = (sp) => sp.GetComponentInParent<MapInfo>(true).name;
System.Func<EnemySpawner, Vector3> spPos = (sp) => { var col = sp.GetComponent<Collider2D>(); return col != null ? (Vector3)col.bounds.center : sp.transform.position; };
System.Func<int, string> levelMap = (lv) => lv <= 3 ? "Lv1_NearForest_SideRoad" : lv <= 6 ? "Lv1_NormalForest" : lv <= 8 ? "Lv1_DeepForest" : lv <= 12 ? "Lv1_Swamp" : "Lv1_TowerOutskirts";

// ---- 작업 상태 ----
string tKind = "", tMap = "", tMob = "", tNpc = "", tQ = ""; float tStart = 0f; bool tArrived = false; float tNoTarget = -1f;
System.Collections.Generic.List<EnemySpawner> tSp = new System.Collections.Generic.List<EnemySpawner>(); int wpIdx = 0;
MiniBossTable.Row tRow = null; bool tWantDay = true; float tGraceUntil = 0f; int tDeaths0 = 0;
float nextTick = 0f, nextLog = 0f, nextSample = 0f, nextSkill = 0f, nextPotion = 0f, nextJump = 0f, retreatUntil = 0f, nextRestock = 0f, nextEquip = 0f, lastT = Time.time, phaseUntil = 0f, lastTalkAt = 0f;
int skillIdx = 0, prevHp = 0, unstuckStage = 0; Vector3 samplePos = Vector3.zero; float stillSince = 0f; bool talkActive = false; bool pendingEnter = false;
bool finished = false; string qLast = "";
Portal pitP = null; Vector3 bossPos = Vector3.zero; float bossStill = 0f; float bossLogAt = 0f, travelFightSince = -1f, ignoreMobsUntil = 0f; bool talkSeen = false; float talkStartT = 0f; float walkSecTotal = 0f; int hopCount = 0, walkTps = 0, tpFallbacks = 0;
Portal hopP = null; float hopStart = 0f; Vector3 tvPos = Vector3.zero; float tvStill = 0f, tvNextSample = 0f; int tvStage = 0;
float arrMonUntil = 0f, arrLogAt = 0f; string arrMap = ""; int arrPrevHp = 0;
var portals = new System.Collections.Generic.List<Portal>();
foreach (var po in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (po.closeMap != null && po.openMap != null && po.destination != null) portals.Add(po); }
var extraDrop = new System.Collections.Generic.Dictionary<string, string>();
try { var ta = Resources.Load<TextAsset>("MonsterExtraDrops"); foreach (var ln in ta.text.Split((char)10)) { var c = ln.Trim().Split(','); if (c.Length >= 4 && c[3].Trim() == "quest" && !extraDrop.ContainsKey(c[1])) extraDrop[c[1]] = c[0]; } } catch (System.Exception) { }
var triedEquip = new System.Collections.Generic.HashSet<int>();
var keyList = new System.Collections.Generic.List<UnityEngine.InputSystem.Key>();
System.Action flush = () => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(keyList.ToArray())); keyList.Clear(); };
System.Action release = () => { keyList.Clear(); UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); };
var aniParam = new System.Collections.Generic.Dictionary<int, bool>();

System.Func<bool> advanceDialogue = () =>
{
    if (!PixelCrushers.DialogueSystem.DialogueManager.isConversationActive) return false;
    var ui = UnityEngine.Object.FindFirstObjectByType<PixelDialogueUI>();
    if (ui == null) { PixelCrushers.DialogueSystem.DialogueManager.StopConversation(); return true; }
    if (typeof(PixelDialogueUI).GetField("responses", flags).GetValue(ui) != null) typeof(PixelDialogueUI).GetMethod("UpdateResponses", flags).Invoke(ui, new object[] { true });
    else { var tw = typeof(PixelDialogueUI).GetField("typewriter", flags).GetValue(ui) as Febucci.UI.TextAnimatorPlayer; if (tw != null) tw.SkipTypewriter(); PixelCrushers.DialogueSystem.DialogueManager.instance.conversationView.HandleContinueButtonClick(); }
    return true;
};
// 가방: 물약 보충(골드 차감) + 가득이면 잡템 비움
System.Func<bool> makeRoom = () =>
{
    foreach (var ic0 in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
        if (ic0.itemData == null || !ic0.transform.IsChildOf(inv.transform) || ic0.itemData.type == "Equip" || ic0.itemData.type == "Consum") continue;
        if (NpcQuestLog.WantsDrop(ic0.itemData.name)) continue;
        bool kp = false; foreach (var aq in NpcQuestLog.ActiveQuests()) { if (aq.Target == ic0.itemData.name) kp = true; } if (kp) continue;
        inv.Service.RemoveAt(ic0, Mathf.Max(1, ic0.nowStack)); return true;
    }
    return false;
};
System.Action cleanBag = () =>
{
    int removed = 0; var keepItems = new System.Collections.Generic.HashSet<string>();
    foreach (var aq in NpcQuestLog.ActiveQuests()) { keepItems.Add(aq.Target); if (!string.IsNullOrEmpty(aq.RewardItem)) keepItems.Add(aq.RewardItem); }
    foreach (var ic1 in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
        if (ic1.itemData == null || !ic1.transform.IsChildOf(inv.transform) || ic1.itemData.type == "Consum") continue;
        if (ic1.itemData.type != "Equip" && (NpcQuestLog.WantsDrop(ic1.itemData.name) || keepItems.Contains(ic1.itemData.name))) continue;
        inv.Service.RemoveAt(ic1, Mathf.Max(1, ic1.nowStack)); removed++;
    }
    if (removed > 0) Log("CLEAN_BAG removed=" + removed);
};
System.Action restock = () =>
{
    int have = inv.CountItem("Small HP Potion") + inv.CountItem("Medium HP Potion") + inv.CountItem("Large HP Potion");
    if (have >= 8) return;
    string pn = LevelSystem.Level >= 12 ? "Large HP Potion" : LevelSystem.Level >= 7 ? "Medium HP Potion" : "Small HP Potion"; int price = pn[0] == 'L' ? 47 : pn[0] == 'M' ? 30 : 17;
    int bought = 0;
    for (int i = have; i < 12; i++)
    {
        if (DatabaseManager.money < price) break;
        bool ok = inv.CreatItem(pn); if (!ok && makeRoom()) ok = inv.CreatItem(pn); if (!ok) break;
        DatabaseManager.money -= price; bought++;
    }
    if (bought > 0) Log("RESTOCK " + pn + " x" + bought + " gold=" + DatabaseManager.money);
    else if (have == 0) Log("NO_POTION_MONEY gold=" + DatabaseManager.money);
};
// 보상 장비 착용(새 장비는 한 번씩 착용 시도)
foreach (var c0 in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (c0.itemData != null && c0.itemData.type == "Equip") triedEquip.Add(c0.GetInstanceID()); }
System.Action equipNew = () =>
{
    foreach (var c in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
        if (c.itemData == null || c.itemData.type != "Equip" || !c.transform.IsChildOf(inv.transform)) continue;
        if (triedEquip.Contains(c.GetInstanceID())) continue; triedEquip.Add(c.GetInstanceID());
        var worn = inv.EquippedFor(c.itemData.equipArea);
        if (worn != null && worn != c && worn.itemData != null && worn.itemData.name == c.itemData.name) continue;
        try { typeof(InventoryManager).GetMethod("UseEquipment", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(inv, new object[] { c }); Log("EQUIP " + c.itemData.name); } catch (System.Exception ex) { Log("EQUIP_ERR " + c.itemData.name + " " + ex.GetType().Name); }
    }
};
if (C("jumpLv", "") != "")
{
    var so = new SROptions(); int jl = (int)CF("jumpLv", 1f); while (LevelSystem.Level < jl) so.LevelUp();
    foreach (var qid in C("jumpDone", "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)) { NpcQuestLog.Accept(qid); NpcQuestLog.Complete(qid); }
    DatabaseManager.money = (int)CF("jumpGold", 0f);
}
equipNew();
System.Func<NpcQuestDef, bool> isSkipped = (d) => skipSet.Contains(d.Id);
System.Action<NpcQuestDef, string> skipQuest = (d, why) => { if (d != null && skipSet.Add(d.Id)) Log("QSKIP " + d.Id + " " + why + " " + Snap()); };
System.Action<string> setTask = null;
// 맵 이동(순간이동 + 환산 비용)
System.Func<string, bool> goMap = (m) => { bool ok = MapInfo.GoToMap(m, pc.gameObject); mapMoves++; Log("MAP_GO " + m + " ok=" + ok); stillSince = Time.time; samplePos = pc.transform.position; return ok; };
// 가장 좋은 맵 선택(몹)
System.Action<string> pickMobMap = (mob) =>
{
    tSp.Clear(); tMap = ""; if (!mobSp.ContainsKey(mob)) return;
    var cnt = new System.Collections.Generic.Dictionary<string, int>();
    foreach (var sp in mobSp[mob]) { string mn = spMap(sp); cnt[mn] = (cnt.ContainsKey(mn) ? cnt[mn] : 0) + sp.enemyCount; }
    int best = -1; foreach (var kv in cnt) if (kv.Value > best) { best = kv.Value; tMap = kv.Key; }
    foreach (var sp in mobSp[mob]) if (spMap(sp) == tMap) tSp.Add(sp);
    tWantDay = false; foreach (var sp in tSp) if (sp.isDaytimeSpowner) tWantDay = true;
};
System.Action<string> pickGrindMap = (m) =>
{
    tMap = m; tSp.Clear(); tWantDay = true;
    foreach (var kv in mobSp) foreach (var sp in kv.Value) if (spMap(sp) == m && sp.isDaytimeSpowner) tSp.Add(sp);
};

System.Func<Portal, bool> sealedP = (po) => { var se = po.GetComponent<PortalSeal>(); return se != null && !se.IsOpen; };
System.Func<string, string, string> nextHop = (from, to) =>
{
    var prev = new System.Collections.Generic.Dictionary<string, string>(); var qu = new System.Collections.Generic.Queue<string>(); qu.Enqueue(from); prev[from] = "";
    while (qu.Count > 0)
    {
        string cur = qu.Dequeue(); if (cur == to) break;
        foreach (var po in portals) { if (po.closeMap.name != cur || sealedP(po)) continue; string nx = po.openMap.name; if (prev.ContainsKey(nx)) continue; prev[nx] = cur; qu.Enqueue(nx); }
    }
    if (!prev.ContainsKey(to)) return ""; string step = to; while (prev[step] != from && prev[step] != "") step = prev[step]; return step;
};
System.Func<string, int> minLvOf = (m) => m == "Lv1_DeepForest" ? 7 : m == "Lv1_Swamp" ? 9 : m == "Lv1_TowerOutskirts" ? 13 : m == "Lv1_NormalForest" ? 4 : 1;
// 계획기: 다음 작업 선택
System.Action plan = () =>
{
    tKind = ""; tArrived = false; tNoTarget = -1f; wpIdx = 0; tRow = null; tStart = Time.time; tDeaths0 = deaths; lastTalkAt = 0f;
    if (NpcQuestLog.IsDone(finishQuest)) { tKind = "end"; return; }
    // 1) 완료 가능 > 2) 수락 가능: NPC 단위(대화하면 NPC 의 대표 퀘스트가 처리된다)
    string wantNpc = ""; string kind = ""; NpcQuestDef pick = null;
    foreach (var npc in new[] { "elder", "ruen", "sena", "merchant", "smith", "eve" })
    {
        if (npcBlock.ContainsKey(npc) && Time.time < npcBlock[npc]) continue;
        NpcQuestDef q; var st = NpcQuestLog.BestFor(npc, out q);
        if (st == NpcQuestStatus.Ready) { wantNpc = npc; kind = "turnin"; pick = q; break; }
    }
    if (wantNpc == "") foreach (var npc in new[] { "elder", "ruen", "sena", "merchant", "smith", "eve" })
    {
        if (npcBlock.ContainsKey(npc) && Time.time < npcBlock[npc]) continue;
        NpcQuestDef q; var st = NpcQuestLog.BestFor(npc, out q);
        if (st == NpcQuestStatus.Available && !(q != null && skipSet.Contains(q.Id) && q.Id != "Q104")) { wantNpc = npc; kind = "accept"; pick = q; break; }
    }
    if (wantNpc != "")
    {
        string key = wantNpc + (pick != null ? pick.Id : "") + kind; talkTries[key] = (talkTries.ContainsKey(key) ? talkTries[key] : 0) + 1;
        if (talkTries[key] > 2) { npcBlock[wantNpc] = Time.time + 40f; Log("NPC_BLOCK " + wantNpc + " " + key); tKind = "replan"; phaseUntil = Time.time + 1f; return; }
        tKind = "talk"; tNpc = wantNpc; tQ = pick != null ? pick.Id : ""; string gm = ""; foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (g.NpcId == tNpc) { var mi = g.GetComponentInParent<MapInfo>(true); gm = mi != null ? mi.name : ""; } }
        tMap = gm; Log("PLAN talk " + tNpc + " for " + tQ + " (" + kind + ") map=" + gm); return;
    }
    // 3) 진행 중
    NpcQuestDef act = null;
    foreach (var d in NpcQuestLog.ActiveQuests()) { if (!isSkipped(d) && NpcQuestLog.StatusOf(d) == NpcQuestStatus.Active) { act = d; break; } }
    if (act != null)
    {
        tQ = act.Id; tMob = act.Target; if (!qStart.ContainsKey(act.Id)) { qStart[act.Id] = Time.time; }
        switch (act.Objective)
        {
            case NpcQuestObjective.Kill:
                pickMobMap(act.Target); if (tMap == "") { skipQuest(act, "NO_SPAWNER " + act.Target); tKind = "replan"; return; }
                tKind = bossSet.Contains(act.Target) ? "boss" : "hunt";
                if (tKind == "boss")
                {
                    int reqLv = (act.Target == "AncientEnt_BD" ? 9 : act.Target == "GiantSlime_BD" ? 11 : 15) + 2 * (bossFails.ContainsKey(act.Target) ? bossFails[act.Target] : 0);
                    if (LevelSystem.Level < reqLv) { tKind = "grind"; tQ = "boss " + act.Id + " need Lv" + reqLv; tMob = reqLv.ToString(); pickGrindMap(levelMap(LevelSystem.Level)); Log("PLAN grind(for boss " + act.Target + ") map=" + tMap + " needLv=" + reqLv + " " + Snap()); return; }
                }
                break;
            case NpcQuestObjective.KillElite:
            {
                MiniBossTable.Row row = null; string lm = levelMap(act.MinLevel);
                foreach (var r in MiniBossTable.All()) { if (act.Target != "*" ? r.Id == act.Target : r.Map == lm) { row = r; break; } }
                if (row == null) { skipQuest(act, "NO_MINIBOSS " + act.Target); tKind = "replan"; return; }
                tRow = row; tGraceUntil = 0f; tMap = row.Map; tKind = "elite"; pickMobMap(row.BaseMonster); tMap = row.Map; tSp.Clear(); if (mobSp.ContainsKey(row.BaseMonster)) foreach (var sp in mobSp[row.BaseMonster]) if (spMap(sp) == tMap) tSp.Add(sp);
                tWantDay = false; foreach (var sp in tSp) if (sp.isDaytimeSpowner) tWantDay = true; break;
            }
            case NpcQuestObjective.Collect:
                if (act.IsRecordsTarget) { skipQuest(act, "RECORDS"); tKind = "replan"; return; }
                pickGrindMap(levelMap(act.MinLevel)); tKind = "collect"; tMob = "";
                if (extraDrop.ContainsKey(act.Target) && mobSp.ContainsKey(extraDrop[act.Target])) { pickMobMap(extraDrop[act.Target]); tMob = extraDrop[act.Target]; }
                break;
            case NpcQuestObjective.Visit: tKind = "cheat"; break;
            case NpcQuestObjective.Craft: tKind = "cheat"; break;
            case NpcQuestObjective.Enhance: tKind = "cheat"; break;
            case NpcQuestObjective.Equip: tKind = "equipq"; break;
            default: skipQuest(act, "UNSUPPORTED " + act.Objective); tKind = "replan"; return;
        }
        if ((tKind == "hunt" || tKind == "elite" || tKind == "collect") && LevelSystem.Level < minLvOf(tMap))
        {
            int rq = minLvOf(tMap); tKind = "grind"; tQ = "gate " + act.Id + " need Lv" + rq; tMob = rq.ToString(); pickGrindMap(levelMap(LevelSystem.Level)); Log("PLAN grind(gate for " + act.Id + ") map=" + tMap + " needLv=" + rq + " " + Snap()); return;
        }
        if ((tKind == "hunt" || tKind == "elite" || tKind == "collect" || tKind == "boss") && curMap() != "Town" && inv.CountItem("Small HP Potion") + inv.CountItem("Medium HP Potion") + inv.CountItem("Large HP Potion") < 3 && DatabaseManager.money >= 100)
        { tKind = "shop"; tMap = "Town"; Log("PLAN shop (potions low) gold=" + DatabaseManager.money); return; }
        Log("PLAN " + tKind + " " + act.Id + " " + act.Objective + " " + act.Target + " x" + act.Count + " map=" + tMap + (tKind == "hunt" || tKind == "boss" ? " day=" + tWantDay : "")); return;
    }
    // 4) 레벨 부족 — 사냥터 그라인드
    int need = 99; foreach (var d in NpcQuestCatalog.All) { if (isSkipped(d)) continue; var st = NpcQuestLog.StatusOf(d); if (st == NpcQuestStatus.LevelLow && d.MinLevel < need) need = d.MinLevel; }
    if (need == 99) { foreach (var kv in npcBlock) { if (Time.time < kv.Value && kv.Value < Time.time + 200f) { tKind = "replan"; phaseUntil = Time.time + 3f; return; } } tKind = "end"; Log("PLAN end: no actionable quest"); return; }
    tKind = "grind"; tMob = ""; pickGrindMap(levelMap(LevelSystem.Level)); tQ = "need Lv" + need; tMob = need.ToString();
    Log("PLAN grind map=" + tMap + " needLv=" + need + " " + Snap());
};

// 전투 한 틱: 대상 접근·공격·보스 후퇴
System.Func<EnemyHealth, bool> isBossish = (e) => e.isBossMob || e.IsMiniBoss || e.IsElite;
System.Func<EnemyHealth, bool> isAttacking = (e) =>
{
    var an = e.GetComponent<Animator>(); if (an == null) return false;
    int id = an.GetInstanceID(); bool has;
    if (!aniParam.TryGetValue(id, out has)) { has = false; foreach (var p in an.parameters) if (p.name == "isAttack") has = true; aniParam[id] = has; }
    return has && an.GetBool("isAttack");
};
System.Action<EnemyHealth> fight = (target) =>
{
    var pos = pc.transform.position; Vector3 tp = target.transform.position; float ddx = tp.x - pos.x, ddy = tp.y - pos.y;
    var w = Weapon.Equipped; float range = (w != null && w.isOverHand) ? 2.6f : 2.0f;
    bool bossish = isBossish(target);
    if (bossish && Time.time >= retreatUntil + 0.3f && Mathf.Abs(ddx) < 5f && isAttacking(target)) { retreatUntil = Time.time + 1.0f; }
    if (Time.time < retreatUntil)
    {   // 보스 공격 중 후퇴 + 점프
        keyList.Add(ddx > 0 ? UnityEngine.InputSystem.Key.LeftArrow : UnityEngine.InputSystem.Key.RightArrow); keyList.Add(UnityEngine.InputSystem.Key.LeftShift);
        if (Mathf.Abs(ddx) < 3f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 0.8f; }
        flush(); return;
    }
    if (Mathf.Abs(ddx) > range * 0.8f) keyList.Add(ddx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow);
    if (ddy > 1.6f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1.0f; }
    if (Mathf.Abs(ddx) <= range + 0.6f && Mathf.Abs(ddy) < 3f)
    {
        keyList.Add(ddx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow);
        if (w != null) { if (w.CanMeleeNow) w.MeleeAttack(); } else keyList.Add(K);
        if (Time.time >= nextSkill && w != null) { nextSkill = Time.time + 1.3f; try { w.ActiveWeaponSkill(skillIdx++ % 4); } catch (System.Exception ex) { Log("SKILL_ERR " + ex.GetType().Name); } }
    }
    flush();
};
// 대상 고르기: 몹 이름 접두(빈 문자열이면 아무 몹), 보스류 포함 여부
System.Func<string, bool, bool, EnemyHealth> pickT = (prefix, allowBossish, onlyBossish) =>
{
    EnemyHealth best = null; float bd = 1e9f;
    foreach (var e in EnemyHealth.Active)
    {
        if (e == null || !e.gameObject.activeInHierarchy || e.nowHP <= 0) continue;
        bool b = isBossish(e); if (onlyBossish && !b) continue; if (b && !allowBossish) continue;
        if (!string.IsNullOrEmpty(prefix) && !e.name.StartsWith(prefix)) continue;
        float dx = e.transform.position.x - pc.transform.position.x, dy = e.transform.position.y - pc.transform.position.y;
        if (onlyBossish ? (Mathf.Abs(dy) > 40f || Mathf.Abs(dx) > 400f) : (Mathf.Abs(dy) > 10f || Mathf.Abs(dx) > 40f)) continue;
        float d = Mathf.Abs(dx) + Mathf.Abs(dy) * 2f; if (d < bd) { bd = d; best = e; }
    }
    return best;
};
// 걷기: 웨이포인트(스포너) 순회, 8초 못 찾으면 순간이동
System.Action walkWaypoints = () =>
{
    var pos = pc.transform.position; if (tNoTarget < 0) tNoTarget = Time.time;
    if (tSp.Count > 0)
    {
        var wp = spPos(tSp[wpIdx % tSp.Count]); float dxw = wp.x - pos.x;
        if (Mathf.Abs(dxw) < 2.5f) wpIdx++;
        else { keyList.Add(dxw > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow); if (wp.y - pos.y > 1.5f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1.2f; } }
        if (Time.time - tNoTarget > 8f) { pc.transform.position = new Vector3(wp.x, wp.y, 0f); tNoTarget = Time.time; wpIdx++; Log("WAYPOINT_TP " + tSp[(wpIdx - 1) % tSp.Count].name); }
    }
    flush();
};

// 정상 동선 이동: 포털까지 걷기(막히면 포털 옆 순간이동) 후 Use. 도착하면 true
System.Func<string, bool> travelTo = null;
travelTo = (dest) =>
{
    string cm0 = curMap(); if (cm0 == dest) { hopP = null; return true; }
    if (DatabaseManager.isUsePortal) { release(); return false; }
    string nx = cm0 == "" ? "" : nextHop(cm0, dest);
    if (nx == "")
    {   // 경로 없음(봉인 등): 마지막 수단 순간이동
        tpFallbacks++; Log("TELEPORT_FALLBACK " + cm0 + "->" + dest); goMap(dest); phaseUntil = Time.time + 2f; return false;
    }
    Portal p = null; foreach (var po in portals) { if (po.closeMap.name == cm0 && po.openMap.name == nx && !sealedP(po) && po.gameObject.activeInHierarchy) { p = po; break; } }
    if (p == null) { tpFallbacks++; Log("TELEPORT_FALLBACK(no active portal) " + cm0 + "->" + nx); goMap(nx); phaseUntil = Time.time + 2f; return false; }
    var pos0 = pc.transform.position; Vector3 pp = p.transform.position;
    if (hopP != p) { hopP = p; hopStart = Time.time; tvPos = pos0; tvStill = Time.time; tvStage = 0; Log("HOP_START " + cm0 + "->" + nx + " portal=" + p.name + " at=" + pp.x.ToString("F0", ic) + "," + pp.y.ToString("F0", ic) + " from=" + pos0.x.ToString("F0", ic) + "," + pos0.y.ToString("F0", ic)); }
    bool near = Mathf.Abs(pp.x - pos0.x) < 1.3f && Mathf.Abs(pp.y - pos0.y) < 3f;
    if (near)
    {
        release(); bool ok = p.Use(pc.gameObject); hopCount++; walkSecTotal += Time.time - hopStart;
        Log("HOP " + cm0 + "->" + nx + " ok=" + ok + " walked=" + (Time.time - hopStart).ToString("F0", ic) + "s " + Snap());
        hopP = null; phaseUntil = Time.time + 2.2f; arrMap = nx; arrLogAt = Time.time + 2.6f; arrMonUntil = Time.time + 14f; arrPrevHp = hm.nowHp; return false;
    }
    var mob = pickT("", false, false);
    if (Time.time - hopStart > 300f) { pc.transform.position = new Vector3(pp.x - 0.3f, pp.y + 0.3f, 0f); walkTps++; hopStart = Time.time; Log("WALK_TP(timeout) " + cm0 + "->" + nx); release(); return false; }
    if (mob != null && Time.time >= ignoreMobsUntil && Mathf.Abs(mob.transform.position.x - pos0.x) < 3.5f && Mathf.Abs(mob.transform.position.y - pos0.y) < 3f)
    {
        if (travelFightSince < 0f) travelFightSince = Time.time;
        if (Time.time - travelFightSince > 8f) { ignoreMobsUntil = Time.time + 10f; travelFightSince = -1f; Log("TRAVEL_IGNORE_MOB " + mob.name); }
        else { fight(mob); return false; }
    }
    else if (mob == null || Mathf.Abs(mob.transform.position.x - pos0.x) >= 3.5f) travelFightSince = -1f;
    float dx = pp.x - pos0.x;
    keyList.Add(dx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow); keyList.Add(UnityEngine.InputSystem.Key.LeftShift);
    if (pp.y - pos0.y > 1.5f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1.0f; }
    flush();
    if (Time.time >= tvNextSample)
    {
        tvNextSample = Time.time + 1f;
        if ((pos0 - tvPos).sqrMagnitude > 0.25f) { tvPos = pos0; tvStill = Time.time; tvStage = 0; }
        else if (Time.time - tvStill > 2f) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 0.4f; flush(); tvStill = Time.time; tvStage++; }
    }
    if (tvStage >= 3 || Time.time - hopStart > 300f)
    {
        pc.transform.position = new Vector3(pp.x - 0.3f, pp.y + 0.3f, 0f); walkTps++; tvStage = 0; Log("WALK_TP " + cm0 + "->" + nx); release();
    }
    return false;
};
System.Action<string> finish = null;
UnityEditor.EditorApplication.CallbackFunction tick = null;
finish = (why) =>
{
    if (finished) return; finished = true;
    UnityEditor.EditorApplication.update -= tick; release();
    EnemyHealth.Killed -= onKilled; PlayerHealthManager.PlayerDied -= onDied; Application.logMessageReceived -= onLog;
    Time.timeScale = 1f;
    var sb = new System.Text.StringBuilder();
    float gs = Time.time - gameStart;
    sb.AppendLine("END=" + why);
    sb.AppendLine("realSec=" + (Time.realtimeSinceStartup - realStart).ToString("F0", ic) + " gameSec=" + gs.ToString("F0", ic) + " timescale=" + ts.ToString("F1", ic));
    sb.AppendLine("final=" + Snap());
    sb.AppendLine("totals kills=" + kills + " elite=" + eliteKills + " boss=" + bossKills + " deaths=" + deaths + " stucks=" + stucks + " potions=" + potions + " talks=" + talks + " mapMoves=" + mapMoves + " cheats=" + cheats + " hops=" + hopCount + " walkSecInHops=" + walkSecTotal.ToString("F0", ic) + " walkTp=" + walkTps + " tpFallback=" + tpFallbacks + " cheatModelMin=" + cheatModelMin.ToString("F1", ic) + " moveCostSec=" + moveCostSec.ToString("F0", ic) + " consoleErrors=" + errors);
    foreach (var kv in mapSec) sb.AppendLine("MAP " + kv.Key + " gameSec=" + kv.Value.ToString("F0", ic) + " kills=" + (mapKills.ContainsKey(kv.Key) ? mapKills[kv.Key] : 0) + " deaths=" + (mapDeaths.ContainsKey(kv.Key) ? mapDeaths[kv.Key] : 0));
    foreach (var q in qLines) sb.AppendLine("QUEST " + q);
    foreach (var e in errSet) sb.AppendLine("ERR " + e);
    System.IO.File.WriteAllText(sumPath, sb.ToString());
    Log("FINISH " + why);
};

tick = () =>
{
    if (finished) return;
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; EnemyHealth.Killed -= onKilled; PlayerHealthManager.PlayerDied -= onDied; Application.logMessageReceived -= onLog; return; }
    if (Time.time < nextTick) return;
    float dt = Time.time - lastT; lastT = Time.time; if (dt > 1f) dt = 1f;
    nextTick = Time.time + 0.05f;
    if (System.IO.File.Exists(stopPath)) { finish("stop-file"); return; }
    if (Time.realtimeSinceStartup - realStart > totalSec) { finish("real-time-limit"); return; }
    if (pc == null || hm == null) { finish("player-missing"); return; }
    string cm = curMap(); mapSec[cm] = (mapSec.ContainsKey(cm) ? mapSec[cm] : 0f) + dt;
    if (god) PlayerStatus.AcquireFor(StatusFlag.Invincible, 5f, "ptbot");
    if (Time.time >= nextLog) { nextLog = Time.time + logEvery; Log("STATE " + tKind + ":" + tQ + " " + Snap()); }
    // 사망: 키 해제, 부활 대기 후 현재 작업 지도 재진입
    if (hm.IsDead) { release(); pendingEnter = true; tArrived = false; phaseUntil = Time.time + 3f; return; }
    if (pendingEnter) { if (Time.time < phaseUntil) return; pendingEnter = false; }
    if (DatabaseManager.isOpenUI && !talkActive) { release(); }
    // 물약 (보스/정예와 싸울 땐 임계 높게)
    if (hm.nowHp > 0 && hm.nowHp < hm.fullHP * potionPct && Time.time >= nextPotion)
    {
        nextPotion = Time.time + 0.8f; int before = hm.nowHp; int deficit = hm.fullHP - hm.nowHp; bool used = false;
        string[] order = deficit >= 320 ? new[] { "Large HP Potion", "Medium HP Potion", "Small HP Potion" } : deficit >= 220 ? new[] { "Medium HP Potion", "Large HP Potion", "Small HP Potion" } : new[] { "Small HP Potion", "Medium HP Potion", "Large HP Potion" };
        foreach (var pn in order) { if (inv.ConsumeFirst(pn)) { used = true; break; } }
        if (used) { potions++; } else if (Time.time >= nextRestock) { Log("NO_POTION hp=" + hm.nowHp); }
    }
    // 마을에서 물약 보충·장비 착용
    if (Time.time >= nextEquip && cm == "Town") { nextEquip = Time.time + 20f; equipNew(); restock(); }
    // 대화 중
    if (talkActive) { if (advanceDialogue()) { talkSeen = true; return; } if (!talkSeen && Time.time < talkStartT + 1.5f) return; talkActive = false; talks++; Log("TALK_DONE " + tNpc + " " + Snap()); equipNew(); phaseUntil = Time.time + 0.6f; tKind = "replan"; return; }
    if (Time.time < phaseUntil) return;
    // 작업 없음/완료 → 계획
    if (tKind == "" || tKind == "replan")
    {
        // 퀘스트 완료 기록
        foreach (var d in NpcQuestCatalog.All) { if (qStart.ContainsKey(d.Id) && NpcQuestLog.IsDone(d.Id) && !qLines.Exists(s => s.StartsWith(d.Id + " "))) { qLines.Add(d.Id + " doneAt=" + (Time.time - gameStart).ToString("F0", ic) + "s startedAt=" + (qStart[d.Id] - gameStart).ToString("F0", ic) + "s lv=" + LevelSystem.Level + " gold=" + DatabaseManager.money); } }
        plan();
        if (tKind == "end") { finish(NpcQuestLog.IsDone(finishQuest) ? "graduated" : "no-more-quests"); return; }
        return;
    }
    if (tKind == "talk")
    {
        if (tMap == "") { npcBlock[tNpc] = Time.time + 999f; tKind = "replan"; return; }
        if (curMap() != tMap) { travelTo(tMap); return; }
        if (tNpc == "elder" || tNpc == "merchant" || tNpc == "smith" || tNpc == "ruen" || tNpc == "sena") { if (Time.time >= nextRestock) { nextRestock = Time.time + 30f; restock(); } }
        foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(FindObjectsSortMode.None))
        {
            if (g.NpcId != tNpc || !g.gameObject.activeInHierarchy) continue;
            pc.transform.position = new Vector3(g.transform.position.x, g.transform.position.y + 0.86f, 0f);
            equipNew(); cleanBag(); bool ok = NpcQuestGiver.TryTalk(g.gameObject); Log("TALK " + tNpc + " q=" + tQ + " ok=" + ok + " " + Snap());
            if (ok) { talkActive = true; talkSeen = false; talkStartT = Time.time; } else { tKind = "replan"; phaseUntil = Time.time + 1.5f; }
            return;
        }
        npcBlock[tNpc] = Time.time + 60f; Log("NPC_NOT_FOUND " + tNpc); tKind = "replan"; return;
    }
    // ---- 사냥류 공통: 맵 도착, 밤낮 ----
    var dq0 = NpcQuestCatalog.Find(tQ);
    if (arrLogAt > 0f && Time.time >= arrLogAt)
    {
        arrLogAt = 0f; var sbA = new System.Text.StringBuilder("ARRIVE " + arrMap + " pos=" + pc.transform.position.x.ToString("F1", ic) + "," + pc.transform.position.y.ToString("F1", ic) + " lv=" + LevelSystem.Level + " hp=" + hm.nowHp + "/" + hm.fullHP + " enemies:");
        foreach (var e in EnemyHealth.Active) { if (e == null || !e.gameObject.activeInHierarchy) continue; float ex = e.transform.position.x - pc.transform.position.x, ey = e.transform.position.y - pc.transform.position.y; if (Mathf.Abs(ex) < 30f && Mathf.Abs(ey) < 12f) sbA.Append(" " + e.name + (isBossish(e) ? "[B]" : "") + "(" + ex.ToString("F1", ic) + "," + ey.ToString("F1", ic) + ")"); }
        Log(sbA.ToString());
    }
    if (arrMonUntil > 0f) { if (hm.nowHp < arrPrevHp) { string who = ""; foreach (var e in EnemyHealth.Active) { if (e == null || !e.gameObject.activeInHierarchy) continue; float ex = e.transform.position.x - pc.transform.position.x; if (Mathf.Abs(ex) < 8f) who += e.name + "(" + ex.ToString("F1", ic) + ") "; } Log("ARRIVE_HIT " + arrMap + " hp " + arrPrevHp + "->" + hm.nowHp + " near: " + who); arrMonUntil = 0f; } arrPrevHp = hm.nowHp; if (Time.time > arrMonUntil) arrMonUntil = 0f; }
    if (tKind == "shop")
    {
        if (cm != "Town") { travelTo("Town"); return; }
        restock(); equipNew(); tKind = "replan"; return;
    }
    if (tKind == "cheat")
    {   // 제작·방문·강화: 즉시 진행 치트(모델 시간 별도 가산)
        float addMin = dq0.Objective == NpcQuestObjective.Craft ? 5f : dq0.Objective == NpcQuestObjective.Visit ? 2.5f : 2.5f;
        if (dq0.Objective == NpcQuestObjective.Craft) { for (int i = 0; i < dq0.Count; i++) NpcQuestLog.OnCraft(dq0.Target); }
        else if (dq0.Objective == NpcQuestObjective.Visit) { NpcQuestLog.OnVisit(dq0.Target); }
        else if (dq0.Objective == NpcQuestObjective.Enhance) { for (int i = 1; i <= dq0.Count; i++) NpcQuestLog.OnEnhance(dq0.Target, i); }
        cheats++; cheatModelMin += addMin; Log("CHEAT " + dq0.Id + " " + dq0.Objective + " " + dq0.Target + " +" + addMin + "min status=" + NpcQuestLog.StatusOf(dq0));
        if (NpcQuestLog.StatusOf(dq0) != NpcQuestStatus.Ready) skipQuest(dq0, "CHEAT_NO_EFFECT");
        tKind = "replan"; return;
    }
    if (tKind == "equipq")
    {
        equipNew(); phaseUntil = Time.time + 1f;
        if (NpcQuestLog.StatusOf(dq0) == NpcQuestStatus.Ready) { tKind = "replan"; return; }
        if (Time.time - tStart > 20f) { skipQuest(dq0, "EQUIP_NOT_MET"); tKind = "replan"; }
        return;
    }
    if (tKind == "boss" && deaths - tDeaths0 >= 3) { bossFails[tMob] = (bossFails.ContainsKey(tMob) ? bossFails[tMob] : 0) + 1; Log("BOSS_FAIL " + tMob + " fails=" + bossFails[tMob]); release(); tKind = "replan"; return; }
    // 목표 달성 확인
    if (dq0 != null && (tKind == "hunt" || tKind == "boss" || tKind == "elite" || tKind == "collect") && NpcQuestLog.StatusOf(dq0) == NpcQuestStatus.Ready) { Log("QREADY " + dq0.Id + " " + Snap() + " took=" + (Time.time - tStart).ToString("F0", ic) + "s"); release(); tKind = "replan"; return; }
    if (tKind == "grind" && LevelSystem.Level >= int.Parse(tMob)) { Log("GRIND_DONE lv=" + LevelSystem.Level + " took=" + (Time.time - tStart).ToString("F0", ic) + "s"); release(); tKind = "replan"; return; }
    // 시간 초과
    float to = tKind == "boss" ? 900f : tKind == "elite" ? 420f : tKind == "grind" ? 900f : tKind == "collect" ? (tMob != "" ? 240f : 90f) : huntTimeout;
    if (Time.time - tStart > to)
    {
        if (tKind == "collect" && dq0 != null)
        {   // 부족분 치트
            int have = NpcQuestLog.Progress(dq0); for (int i = have; i < dq0.Count; i++) { if (!inv.CreatItem(dq0.Target) && makeRoom()) inv.CreatItem(dq0.Target); } cheats++; cheatModelMin += 2.5f; Log("CHEAT_COLLECT " + dq0.Id + " " + dq0.Target + " had=" + have + "/" + dq0.Count + " +2.5min");
            if (NpcQuestLog.StatusOf(dq0) != NpcQuestStatus.Ready) skipQuest(dq0, "COLLECT_FAIL");
        }
        else if (tKind == "grind") { Log("GRIND_TIMEOUT"); }
        else if (dq0 != null) skipQuest(dq0, "TIMEOUT " + tKind);
        release(); tKind = "replan"; return;
    }
    // 맵 도착
    if (cm != tMap) { travelTo(tMap); return; }
    if (!tArrived) { tArrived = true; samplePos = pc.transform.position; stillSince = Time.time; }
    // 밤낮 유지
    if (tc != null && tc.isDaytime != tWantDay) { setDay(tWantDay); }
    // 막힘 감지
    var pos = pc.transform.position;
    if (Time.time >= nextSample)
    {
        nextSample = Time.time + 1f;
        if ((pos - samplePos).sqrMagnitude > 0.09f) { samplePos = pos; stillSince = Time.time; unstuckStage = 0; }
        else if (Time.time - stillSince > stuckSec)
        {
            var t0 = pickT("", true, false); bool fighting = t0 != null && Mathf.Abs(t0.transform.position.x - pos.x) < 2.6f && Mathf.Abs(t0.transform.position.y - pos.y) < 2.5f;
            if (!fighting && tSp.Count > 0)
            {
                stucks++; stillSince = Time.time; unstuckStage++; Log("STUCK stage=" + unstuckStage + " " + Snap());
                if (unstuckStage >= 2) { var wp = spPos(tSp[wpIdx++ % tSp.Count]); pc.transform.position = new Vector3(wp.x, wp.y, 0f); Log("UNSTUCK_TP"); unstuckStage = 0; samplePos = pc.transform.position; }
                else { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 0.5f; flush(); }
            }
            else stillSince = Time.time;
        }
    }
    // 대상 결정
    EnemyHealth tgt = null;
    if (tKind == "hunt") tgt = pickT(tMob, false, false);
    else if (tKind == "collect" && tMob != "") tgt = pickT(tMob, false, false);
    else if (tKind == "collect" || tKind == "grind") tgt = pickT("", false, false);
    else if (tKind == "boss")
    {
        tgt = pickT(tMob, true, true);
        if (tgt != null && Time.time > bossLogAt) { bossLogAt = Time.time + 20f; Log("BOSS_TARGET " + tgt.name + " hp=" + tgt.nowHP + " dx=" + (tgt.transform.position.x - pos.x).ToString("F1", ic) + " dy=" + (tgt.transform.position.y - pos.y).ToString("F1", ic)); }
    }
    else if (tKind == "elite")
    {
        tgt = pickT(tRow.Id, true, true); if (tgt == null) tgt = pickT("", true, true);
        if (tgt == null && (Mathf.Abs(tRow.X - pos.x) > 6f || Mathf.Abs(tRow.Y - pos.y) > 6f))
        {
            if (tGraceUntil == 0f) tGraceUntil = Time.time + 40f;
            float edx = tRow.X - pos.x; keyList.Add(edx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow); keyList.Add(UnityEngine.InputSystem.Key.LeftShift);
            if (tRow.Y - pos.y > 1.5f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1f; } flush();
            if (Time.time > tGraceUntil) { pc.transform.position = new Vector3(tRow.X, tRow.Y + 1f, 0f); tGraceUntil = Time.time + 40f; Log("ELITE_TP(recovery) " + tRow.Id); }
            return;
        }
    }
    if ((tKind == "boss" || tKind == "elite") && tgt != null && Mathf.Abs(tgt.transform.position.x - pos.x) > 25f && pitP == null)
    {
        if ((pos - bossPos).sqrMagnitude > 1f) { bossPos = pos; bossStill = Time.time; }
        else if (Time.time - bossStill > 4f)
        {
            Portal best = null; float bdist = 1e9f;
            foreach (var po in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsSortMode.None)) { if (po.openMap != null || po.destination == null || !po.gameObject.activeInHierarchy) continue; float dd = Mathf.Abs(po.transform.position.x - pos.x) + Mathf.Abs(po.transform.position.y - pos.y); if (dd < bdist && dd < 80f) { bdist = dd; best = po; } }
            bossStill = Time.time; if (best != null) { pitP = best; Log("PIT_PORTAL " + best.name + " at=" + best.transform.position.x.ToString("F0", ic) + "," + best.transform.position.y.ToString("F0", ic) + " dest=" + best.destination.position.x.ToString("F0", ic) + "," + best.destination.position.y.ToString("F0", ic)); }
        }
    }
    if (pitP != null)
    {
        Vector3 pw = pitP.transform.position; float pdx = pw.x - pos.x;
        if (Mathf.Abs(pdx) < 1.3f && Mathf.Abs(pw.y - pos.y) < 3f) { release(); bool ok2 = pitP.Use(pc.gameObject); Log("PIT_USE ok=" + ok2); pitP = null; bossStill = Time.time; phaseUntil = Time.time + 2f; return; }
        keyList.Add(pdx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow); keyList.Add(UnityEngine.InputSystem.Key.LeftShift);
        if (pw.y - pos.y > 1.5f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1f; }
        flush(); if (Time.time - bossStill > 40f) { pitP = null; } return;
    }
    if (tgt == null) { walkWaypoints(); return; }
    tNoTarget = -1f;
    fight(tgt);
};
UnityEditor.EditorApplication.update += tick;
return "ptbot camp started ts=" + ts + " skip=" + string.Join(",", new System.Collections.Generic.List<string>(skipSet).ToArray()) + " mobs=" + mobSp.Count;
