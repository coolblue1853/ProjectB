// PT-BOT 본체: 플레이 모드에서 1층계를 자동으로 플레이하는 봇. 설정은 Logs/ptbot/config.txt(key=value), 로그는 Logs/ptbot/run.log, 결과는 Logs/ptbot/summary.txt.
// 중지: Logs/ptbot/stop 파일 생성(ptbot_stop.cs). 키 입력은 가상 키보드(백그라운드 허용 필요 — 이 프로브가 켠다, 복구는 input_bg_off.cs).
// 기존 프로브 재사용: 구역 이동(MapInfo.GoToMap, _f1nav2), 걷기·점프(_f1nav2_walk), 대화 진행(firsthour_talks), 키 전투(f1fr_boss), 스포너 영역 이동(tp_spawner).
var cfg = new System.Collections.Generic.Dictionary<string, string>();
foreach (var ln in System.IO.File.ReadAllLines("Logs/ptbot/config.txt")) { int eq = ln.IndexOf('='); if (eq > 0 && !ln.StartsWith("#")) cfg[ln.Substring(0, eq).Trim()] = ln.Substring(eq + 1).Trim(); }
var ic = System.Globalization.CultureInfo.InvariantCulture;
System.Func<string, string, string> C = (k, d) => cfg.ContainsKey(k) ? cfg[k] : d;
System.Func<string, float, float> CF = (k, d) => { float v; return float.TryParse(C(k, ""), System.Globalization.NumberStyles.Float, ic, out v) ? v : d; };
string[] zones = C("zones", "Lv1_NearForest_SideRoad").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
float zoneSec = CF("zoneSec", 120f), totalSec = CF("totalRealSec", 280f), logEvery = CF("logEvery", 5f), stuckSec = CF("stuckSec", 6f), potionPct = CF("potionPct", 0.45f);
float ts = Mathf.Clamp(CF("timescale", 2f), 0.5f, 4f);
bool doQuests = C("quests", "0") == "1", doBoss = C("boss", "0") == "1", god = C("god", "0") == "1", wantNight = C("night", "0") == "1";
int givePotions = (int)CF("givePotions", 0);
string logPath = "Logs/ptbot/run.log", sumPath = "Logs/ptbot/summary.txt", stopPath = "Logs/ptbot/stop";

System.IO.File.WriteAllText(logPath, "");

var pc = PlayerController.instance; var hm = PlayerHealthManager.Instance; var inv = InventoryManager.instance;
// 가상 키 입력 허용(input_bg_on 과 동일)
var iset = UnityEngine.InputSystem.InputSystem.settings;
iset.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
iset.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var kb = UnityEngine.InputSystem.Keyboard.current;
UnityEngine.InputSystem.InputSystem.EnableDevice(kb);
Application.runInBackground = true;
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
try { TitleScreen.DismissNow(); } catch (System.Exception) { }
// 낮 설정(낮 고정 켜기) — night=1 이면 밤
var tc = TimeChange.instance;
if (tc != null) { if (wantNight) { tc.LockDaytime = false; float tg = (tc.startNighttime + 0.05f) * tc.totalTime; float ad = tg - tc.value * tc.totalTime; if (ad < 0) ad += tc.totalTime; tc.AddTime(ad); } else { tc.LockDaytime = true; float tg = (tc.endNighttime + 0.05f) * tc.totalTime; float ad = tg - tc.value * tc.totalTime; if (ad < 0) ad += tc.totalTime; tc.AddTime(ad); } }
if (givePotions > 0)
{   // 가방이 가득 차면 잡템 한 칸을 비워 물약을 넣는다(세이브는 ptbot.sh 가 복원)
    bool gp = inv.CreatItem("Small HP Potion");
    if (!gp)
    {
        foreach (var ic0 in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (ic0.itemData == null || !ic0.transform.IsChildOf(inv.transform) || ic0.itemData.type == "Equip" || ic0.itemData.type == "Consum") continue;
            inv.Service.RemoveAt(ic0, Mathf.Max(1, ic0.nowStack)); gp = inv.CreatItem("Small HP Potion"); if (gp) break;
        }
    }
    for (int i = 1; i < givePotions && gp; i++) inv.CreatItem("Small HP Potion");
    System.IO.File.AppendAllText(logPath, "SETUP potions=" + inv.CountItem("Small HP Potion") + "\n");
}
Time.timeScale = ts;

// ---- 상태 ----
var K = UnityEngine.InputSystem.Key.X;
int kills = 0, deaths = 0, stucks = 0, potions = 0, errors = 0, dodges = 0, talks = 0, bossKills = 0, eliteKills = 0;
var errSet = new System.Collections.Generic.List<string>();
var zs = new System.Collections.Generic.List<string>();   // 구역 결과 줄
int zi = -1; string zone = ""; string phase = "init"; float phaseUntil = 0f, zoneStart = 0f, nextTick = 0f, nextLog = 0f, nextSample = 0f, nextSkill = 0f, nextPotion = 0f, nextJump = 0f, dodgeUntil = 0f, lastKillT = -1f, noTargetSince = -1f, firstKillT = -1f;
int zKills = 0, zDeaths = 0, zStucks = 0, zPotions = 0, zElite = 0, zBoss = 0, zLv0 = 1, skillIdx = 0, prevHp = 0, wpIdx = 0, zTalks = 0, unstuckStage = 0;
long zGold0 = 0; int zExp0 = 0;
Vector3 samplePos = Vector3.zero; float stillSince = 0f;
bool noPotionLogged = false, started = false, talkStarted = false; int talkIdx = 0;
var talkTried = new System.Collections.Generic.Dictionary<string, int>();
System.Collections.Generic.List<string> talkTodo = null;
float realStart = Time.realtimeSinceStartup, gameStart = Time.time;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
System.Action<string> Log = (m) => { System.IO.File.AppendAllText(logPath, "[" + (Time.time - gameStart).ToString("F1", ic) + "s] " + m + "\n"); };
System.Func<string> Snap = () => "zone=" + zone + " pos=" + pc.transform.position.x.ToString("F1", ic) + "," + pc.transform.position.y.ToString("F1", ic) + " hp=" + hm.nowHp + "/" + hm.fullHP + " lv=" + LevelSystem.Level + " exp=" + LevelSystem.Exp + " gold=" + DatabaseManager.money + " kills=" + kills + " deaths=" + deaths + " stucks=" + stucks + " potions=" + potions;

System.Action<EnemyHealth> onKilled = (e) => { kills++; zKills++; lastKillT = Time.time; if (firstKillT < 0) firstKillT = Time.time - zoneStart; if (e != null && e.isBossMob) { bossKills++; zBoss++; } else if (e != null && e.IsElite) { eliteKills++; zElite++; } };
System.Action onDied = () => { deaths++; zDeaths++; Log("DEATH " + Snap()); };
UnityEngine.Application.LogCallback onLog = (msg, st, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) { errors++; string k = msg.Split('\n')[0]; if (k.Length > 200) k = k.Substring(0, 200); if (errSet.Count < 30 && !errSet.Contains(k)) { errSet.Add(k); Log("CONSOLE_ERROR " + k); } } };
EnemyHealth.Killed += onKilled; PlayerHealthManager.PlayerDied += onDied; Application.logMessageReceived += onLog;

System.Action<string> setKeys = null;
var keyList = new System.Collections.Generic.List<UnityEngine.InputSystem.Key>();
System.Action flush = () => { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(keyList.ToArray())); keyList.Clear(); };
System.Action release = () => { keyList.Clear(); UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); };

UnityEditor.EditorApplication.CallbackFunction tick = null;
System.Action<string> finish = (why) =>
{
    UnityEditor.EditorApplication.update -= tick; release();
    EnemyHealth.Killed -= onKilled; PlayerHealthManager.PlayerDied -= onDied; Application.logMessageReceived -= onLog;
    Time.timeScale = 1f;
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("END=" + why);
    sb.AppendLine("realSec=" + (Time.realtimeSinceStartup - realStart).ToString("F0", ic) + " gameSec=" + (Time.time - gameStart).ToString("F0", ic) + " timescale=" + ts.ToString("F1", ic));
    sb.AppendLine("final=" + Snap());
    sb.AppendLine("totals kills=" + kills + " elite=" + eliteKills + " boss=" + bossKills + " deaths=" + deaths + " stucks=" + stucks + " potions=" + potions + " talks=" + talks + " dodges=" + dodges + " consoleErrors=" + errors);
    foreach (var z in zs) sb.AppendLine("ZONE " + z);
    foreach (var e in errSet) sb.AppendLine("ERR " + e);
    System.IO.File.WriteAllText(sumPath, sb.ToString());
    Log("FINISH " + why);
};
System.Action closeZone = () =>
{
    if (zone == "") return;
    zs.Add(zone + " | gameSec=" + (Time.time - zoneStart).ToString("F0", ic) + " kills=" + zKills + " elite=" + zElite + " boss=" + zBoss + " firstKillSec=" + (firstKillT < 0 ? "-" : firstKillT.ToString("F0", ic)) + " deaths=" + zDeaths + " stucks=" + zStucks + " potions=" + zPotions + " talks=" + zTalks + " lv=" + zLv0 + "->" + LevelSystem.Level + " exp=" + zExp0 + "->" + LevelSystem.Exp + " gold=" + zGold0 + "->" + DatabaseManager.money);
};
System.Func<bool> nextZone = () =>
{
    closeZone(); zi++;
    if (zi >= zones.Length) return false;
    zone = zones[zi]; zKills = zDeaths = zStucks = zPotions = zElite = zBoss = zTalks = 0; firstKillT = -1f; zLv0 = LevelSystem.Level; zExp0 = LevelSystem.Exp; zGold0 = DatabaseManager.money; wpIdx = 0; talkTodo = null; talkTried.Clear();
    bool ok = MapInfo.GoToMap(zone, pc.gameObject);
    Log("ZONE_ENTER " + zone + " ok=" + ok + " " + Snap());
    phase = "enter"; phaseUntil = Time.time + 2.5f; zoneStart = Time.time; stillSince = Time.time; samplePos = pc.transform.position;
    return true;
};
System.Func<EnemySpawner[]> zoneSpawners = () =>
{
    var list = new System.Collections.Generic.List<EnemySpawner>();
    var mapRoot = GameObject.Find("Map");
    var map = mapRoot != null ? mapRoot.transform.Find(zone) : null;
    if (map == null) return list.ToArray();
    foreach (var sp in map.GetComponentsInChildren<EnemySpawner>(false)) { if (sp.isActiveAndEnabled && sp.enemyPositon != null) list.Add(sp); }
    return list.ToArray();
};
System.Func<EnemyHealth, bool> isBossish = (e) => e.isBossMob || e.IsMiniBoss;
System.Func<EnemyHealth> pickTarget = () =>
{
    EnemyHealth best = null; float bd = 1e9f; bool bestBoss = false;
    foreach (var e in EnemyHealth.Active)
    {
        if (e == null || !e.gameObject.activeInHierarchy || e.nowHP <= 0) continue;
        bool bossish = isBossish(e) || e.IsElite;
        if (isBossish(e) && !doBoss) continue;                       // 보스/미니보스는 boss=1 일 때만 도전
        float dx = e.transform.position.x - pc.transform.position.x, dy = e.transform.position.y - pc.transform.position.y;
        if (Mathf.Abs(dy) > 10f || Mathf.Abs(dx) > 40f) continue;
        float d = Mathf.Abs(dx) + Mathf.Abs(dy) * 2f;
        if (doBoss && bossish && !bestBoss) { best = e; bd = d; bestBoss = true; continue; }
        if (bestBoss && !bossish) continue;
        if (d < bd) { bd = d; best = e; bestBoss = bossish && doBoss; }
    }
    return best;
};
System.Func<bool> advanceDialogue = () =>
{
    if (!PixelCrushers.DialogueSystem.DialogueManager.isConversationActive) return false;
    var ui = UnityEngine.Object.FindFirstObjectByType<PixelDialogueUI>();
    if (ui == null) { PixelCrushers.DialogueSystem.DialogueManager.StopConversation(); return true; }
    if (typeof(PixelDialogueUI).GetField("responses", flags).GetValue(ui) != null) typeof(PixelDialogueUI).GetMethod("UpdateResponses", flags).Invoke(ui, new object[] { true });
    else { var tw = typeof(PixelDialogueUI).GetField("typewriter", flags).GetValue(ui) as Febucci.UI.TextAnimatorPlayer; if (tw != null) tw.SkipTypewriter(); PixelCrushers.DialogueSystem.DialogueManager.instance.conversationView.HandleContinueButtonClick(); }
    return true;
};

tick = () =>
{
    if (!EditorApplication.isPlaying) { UnityEditor.EditorApplication.update -= tick; EnemyHealth.Killed -= onKilled; PlayerHealthManager.PlayerDied -= onDied; Application.logMessageReceived -= onLog; return; }
    if (Time.time < nextTick) return;
    nextTick = Time.time + 0.1f;
    if (System.IO.File.Exists(stopPath)) { finish("stop-file"); return; }
    if (Time.realtimeSinceStartup - realStart > totalSec) { finish("real-time-limit"); return; }
    if (pc == null || hm == null) { finish("player-missing"); return; }
    if (god) { PlayerStatus.AcquireFor(StatusFlag.Invincible, 5f, "ptbot"); }
    if (!started) { started = true; if (!nextZone()) { finish("no-zones"); return; } }
    // 주기 로그
    if (Time.time >= nextLog) { nextLog = Time.time + logEvery; Log("STATE " + phase + " " + Snap()); }
    if (hm.IsDead) { release(); return; }
    if (DatabaseManager.isOpenUI && phase != "quest") { release(); }
    // 물약
    if (hm.nowHp > 0 && hm.nowHp < hm.fullHP * potionPct && Time.time >= nextPotion)
    {
        nextPotion = Time.time + 1.0f; int before = hm.nowHp; bool used = false;
        foreach (var pn in new[] { "Small HP Potion", "Medium HP Potion", "Large HP Potion" }) { if (inv.ConsumeFirst(pn)) { used = true; break; } }
        if (used) { potions++; zPotions++; Log("POTION used hp " + before + "->" + hm.nowHp + "/" + hm.fullHP); }
        else if (!noPotionLogged) { noPotionLogged = true; Log("NO_POTION hp=" + hm.nowHp); }
    }
    // 사망 후 부활: 구역 재진입
    if (phase == "respawnwait") { if (!hm.IsDead && Time.time >= phaseUntil) { MapInfo.GoToMap(zone, pc.gameObject); phase = "enter"; phaseUntil = Time.time + 2.5f; Log("RE_ENTER " + zone); } return; }
    if (deaths > 0 && hm.IsDead) { phase = "respawnwait"; phaseUntil = Time.time + 3.5f; return; }
    if (Time.time < phaseUntil) { return; }

    if (phase == "enter")
    {
        PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
        if (doQuests) { phase = "quest"; talkTodo = null; talkIdx = 0; talkStarted = false; phaseUntil = Time.time; } else { phase = "hunt"; }
        return;
    }
    if (phase == "quest")
    {
        if (advanceDialogue()) { phaseUntil = Time.time + 0.2f; return; }
        if (talkStarted) { talks++; zTalks++; Log("TALK " + talkTodo[talkIdx] + " lv=" + LevelSystem.Level + " exp=" + LevelSystem.Exp + " gold=" + DatabaseManager.money); talkIdx++; talkStarted = false; phaseUntil = Time.time + 0.5f; return; }
        if (talkTodo == null)
        {
            talkTodo = new System.Collections.Generic.List<string>();
            foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(FindObjectsSortMode.None))
            {
                if (!g.gameObject.activeInHierarchy) continue;
                NpcQuestDef qd; var st = NpcQuestLog.BestFor(g.NpcId, out qd);
                if ((st == NpcQuestStatus.Ready || st == NpcQuestStatus.Available) && !talkTodo.Contains(g.NpcId)) talkTodo.Add(g.NpcId);
            }
            Log("QUEST_NPCS " + string.Join(",", talkTodo.ToArray()));
        }
        if (talkIdx >= talkTodo.Count) { phase = "hunt"; return; }
        foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(FindObjectsSortMode.None))
        {
            if (g.NpcId != talkTodo[talkIdx] || !g.gameObject.activeInHierarchy) continue;
            pc.transform.position = new Vector3(g.transform.position.x, g.transform.position.y + 0.86f, 0f);
            talkStarted = NpcQuestGiver.TryTalk(g.gameObject);
            if (!talkStarted) { Log("TALK_FAILED " + talkTodo[talkIdx]); talkIdx++; }
            return;
        }
        Log("TALK_NPC_NOT_FOUND " + talkTodo[talkIdx]); talkIdx++;
        return;
    }
    // ---- hunt ----
    if (Time.time - zoneStart > zoneSec) { Log("ZONE_DONE " + zone + " " + Snap()); release(); if (!nextZone()) { finish("all-zones-done"); } return; }
    var pos = pc.transform.position;
    // 막힘 감지(위치 불변)
    if (Time.time >= nextSample)
    {
        nextSample = Time.time + 1f;
        if ((pos - samplePos).sqrMagnitude > 0.09f) { samplePos = pos; stillSince = Time.time; unstuckStage = 0; }
        else if (Time.time - stillSince > stuckSec)
        {
            // 전투 중(타겟이 사거리 안)이면 정지는 정상
            var tgt0 = pickTarget(); bool fighting = tgt0 != null && Mathf.Abs(tgt0.transform.position.x - pos.x) < 2.6f && Mathf.Abs(tgt0.transform.position.y - pos.y) < 2.5f;
            if (!fighting && zoneSpawners().Length > 0)
            {
                stucks++; zStucks++; stillSince = Time.time; unstuckStage++;
                Log("STUCK stage=" + unstuckStage + " " + Snap() + " target=" + (tgt0 != null ? tgt0.name : "none"));
                if (unstuckStage >= 2)
                {   // 스포너 영역으로 순간이동 후 재개
                    var sps = zoneSpawners(); if (sps.Length > 0) { var sp = sps[wpIdx++ % sps.Length]; var col = sp.GetComponent<Collider2D>(); var p = col != null ? (Vector3)col.bounds.center : sp.transform.position; pc.transform.position = new Vector3(p.x, p.y, 0f); Log("UNSTUCK_TP " + sp.name); unstuckStage = 0; samplePos = pc.transform.position; }
                }
                else { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 0.5f; }
            }
            else stillSince = Time.time;
        }
    }
    // 피격 감지 → 점프 회피
    if (prevHp > 0 && hm.nowHp < prevHp && Time.time > dodgeUntil) { dodgeUntil = Time.time + 1.5f; dodges++; }
    prevHp = hm.nowHp;
    var target = pickTarget();
    var w = Weapon.Equipped;
    if (target == null)
    {
        if (noTargetSince < 0) noTargetSince = Time.time;
        // 스포너 사이를 걷는다. 8초 넘게 몹이 없으면 스포너 영역으로 이동
        var sps2 = zoneSpawners();
        if (sps2.Length > 0)
        {
            var sp2 = sps2[wpIdx % sps2.Length]; var col2 = sp2.GetComponent<Collider2D>(); var wp = col2 != null ? (Vector3)col2.bounds.center : sp2.transform.position;
            float dxw = wp.x - pos.x;
            if (Mathf.Abs(dxw) < 2.5f) { wpIdx++; }
            else { keyList.Add(dxw > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow); if (wp.y - pos.y > 1.5f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1.2f; } }
            if (Time.time - noTargetSince > 8f) { pc.transform.position = new Vector3(wp.x, wp.y, 0f); noTargetSince = Time.time; Log("WAYPOINT_TP " + sp2.name); wpIdx++; }
        }
        flush(); return;
    }
    noTargetSince = -1f;
    Vector3 tp = target.transform.position; float ddx = tp.x - pos.x, ddy = tp.y - pos.y;
    float range = (w != null && w.isOverHand) ? 2.6f : 2.0f;
    bool dodging = Time.time < dodgeUntil && Time.time > dodgeUntil - 1.1f;
    if (dodging)
    {   // 점프 회피: 적 반대로 점프
        keyList.Add(ddx > 0 ? UnityEngine.InputSystem.Key.LeftArrow : UnityEngine.InputSystem.Key.RightArrow);
        if (Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 0.9f; }
        flush(); return;
    }
    if (Mathf.Abs(ddx) > range * 0.8f) keyList.Add(ddx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow);
    else if (Mathf.Abs(ddx) > 0.4f && Time.time - lastKillT > 0.1f) { /* 사거리 안 — 바라보기만 */ }
    if (ddy > 1.6f && Time.time >= nextJump) { keyList.Add(UnityEngine.InputSystem.Key.C); nextJump = Time.time + 1.0f; }
    if (Mathf.Abs(ddx) <= range + 0.6f && Mathf.Abs(ddy) < 3f)
    {
        // 적 방향을 보게 한 걸음 + 공격(평타 콤보)
        keyList.Add(ddx > 0 ? UnityEngine.InputSystem.Key.RightArrow : UnityEngine.InputSystem.Key.LeftArrow);
        if (w != null) { if (w.CanMeleeNow) w.MeleeAttack(); } else keyList.Add(K);
        if (Time.time >= nextSkill && w != null) { nextSkill = Time.time + 1.3f; try { w.ActiveWeaponSkill(skillIdx++ % 4); } catch (System.Exception ex) { Log("SKILL_ERR " + ex.GetType().Name); } }
    }
    flush();
};
UnityEditor.EditorApplication.update += tick;
return "ptbot started zones=" + string.Join(",", zones) + " zoneSec=" + zoneSec + " timescale=" + ts + " quests=" + doQuests + " boss=" + doBoss;
