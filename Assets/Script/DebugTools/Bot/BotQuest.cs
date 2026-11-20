using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 봇 퀘스트 따라가기(B3 축소판). 퀘스트 상태는 읽기만 하고(QuestService.StatusOf·ActiveAll·NpcQuestTrack.Tracked), 받기·보고는 진짜 입력으로만 한다:
/// NPC 로 걸어가 ↑ → 선택지에서 "퀘스트 이야기" 줄로 ↑↓·Select → 대화 중 Select 로 넘기고 선택지 첫 줄("수락한다")을 고른다. 완료 함수(QuestService.Report 등)는 부르지 않는다.
/// 사냥 방향: 진행 중 처치 퀘스트의 목표 몹이 나오는 맵(HuntMap)과 몹 id(IsTarget) 를 HuntBrain 에 알려 준다.
/// 처치형·"수락한 NPC 와 대화"형만 다룬다(수집·제작·방문은 안 받는다). 규칙은 BotQuestRules(순수). 설계 PlayBotDesign.md 15절.
/// </summary>
public sealed class BotQuest
{
    public const float EvalInterval = 1f;
    public const float JobTimeout = 150f;
    public const float AbortCooldown = 120f;
    public const float AfterJobCooldown = 2f;
    public const float NearDx = 0.7f;
    public const int MaxTalkTries = 6;
    public const int FocusQuest = -5;

    private enum Phase { None, Go, Talk }

    private readonly IBotNavigator _nav;
    private readonly List<BotQuestInfo> _infos = new List<BotQuestInfo>();
    private readonly Dictionary<string, HashSet<string>> _mapMobs = new Dictionary<string, HashSet<string>>();
    private readonly Dictionary<string, int> _fails = new Dictionary<string, int>();
    private readonly Dictionary<string, float> _blockedQuests = new Dictionary<string, float>();
    private HashSet<string> _huntTargets;
    private readonly List<SpawnPt> _spawns = new List<SpawnPt>();

    private struct SpawnPt
    {
        public string Map;
        public HashSet<string> Mobs;
        public Vector2 Pos;
    }
    private BotQuestJob _job;
    private Phase _phase;
    private float _phaseSince;
    private float _nextEval;
    private float _blockedUntil;
    private float _nextKey;
    private float _talkAt;
    private int _talkTries;
    private bool _scanned;
    private int _doneAtStart;
    private NpcQuestGiver _npc;
    private string _npcMap;
    private float _npcRetryAt;

    public string HuntMap { get; private set; }
    public int Accepted { get; private set; }
    public int Reported { get; private set; }
    public int Aborts { get; private set; }
    public string Phased => _phase == Phase.None ? "-" : _job.Kind + ":" + _job.QuestId + ":" + _phase;
    public bool Busy => _phase != Phase.None;

    /// <summary>봇 시작 뒤 완료된 퀘스트 수(시작 때 완료 수와 지금의 차)</summary>
    public int Completed => Mathf.Max(0, CountDone() - _doneAtStart);

    public BotQuest(IBotNavigator nav)
    {
        _nav = nav;
    }

    public void Begin(BotContext ctx)
    {
        _phase = Phase.None;
        _doneAtStart = CountDone();
        Accepted = 0;
        Reported = 0;
        Aborts = 0;
        HuntMap = null;
        _huntTargets = null;
        _fails.Clear();
        _blockedQuests.Clear();
        _blockedUntil = ctx.Now + 3f;
        _scanned = false;
    }

    /// <summary>마을 귀환 업무가 끼어든 동안은 퀘스트 일의 제한 시간을 멈춘다</summary>
    public void Hold(float now)
    {
        if (_phase != Phase.None)
        {
            _phaseSince = now;
        }
    }

    private static int CountDone()
    {
        int n = 0;
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (NpcQuestLog.IsDone(d.Id))
            {
                n++;
            }
        }
        return n;
    }

    /// <summary>이 몹 id 가 지금 따라가는 처치 퀘스트의 목표인가(목표가 없으면 항상 거짓)</summary>
    public bool IsTarget(string monsterId)
    {
        return _huntTargets != null && monsterId != null && _huntTargets.Contains(monsterId);
    }

    public bool HasHuntTargets => _huntTargets != null;

    /// <summary>지금 맵에서 목표 몹이 나오는 스포너 중 from 에서 가장 가까운 자리(없으면 false). 목표 몹이 안 보일 때 그리로 찾아간다</summary>
    public bool TryFindSpawn(Vector2 from, out Vector2 pos)
    {
        pos = default(Vector2);
        if (_huntTargets == null)
        {
            return false;
        }
        string map = BgmDirector.CurrentMap;
        float best = float.MaxValue;
        bool found = false;
        for (int i = 0; i < _spawns.Count; i++)
        {
            SpawnPt s = _spawns[i];
            if (s.Map != map || !_huntTargets.Overlaps(s.Mobs))
            {
                continue;
            }
            float d = Mathf.Abs(s.Pos.x - from.x) + Mathf.Abs(s.Pos.y - from.y);
            if (d < best)
            {
                best = d;
                pos = s.Pos;
                found = true;
            }
        }
        return found;
    }

    // ───── 읽기 ─────

    private void Scan()
    {
        _scanned = true;
        _mapMobs.Clear();
        _spawns.Clear();
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < maps.Length; i++)
        {
            var set = new HashSet<string>();
            EnemySpawner[] sp = maps[i].GetComponentsInChildren<EnemySpawner>(true);
            for (int s = 0; s < sp.Length; s++)
            {
                var one = new HashSet<string>();
                AddSpawner(one, maps[i].name, sp[s]);
                set.UnionWith(one);
                _spawns.Add(new SpawnPt { Map = maps[i].name, Mobs = one, Pos = sp[s].transform.position });
            }
            _mapMobs[maps[i].name] = set;
        }
        Debug.Log("[PB][Bot] 퀘스트: 맵별 몹 목록 " + _mapMobs.Count + "맵");
    }

    private static void AddSpawner(HashSet<string> set, string map, EnemySpawner sp)
    {
        GameObject prefab = sp.EnemyPrefab; // 슬롯은 지금 떠 있는 개체뿐이라 새 게임 첫 스캔에는 비어 있다(11-19: Q102 꽃두더지 HuntMap 이 null 이라 봇이 퀘스트를 못 받았다) — 풀 프리팹을 먼저 본다
        if (prefab != null)
        {
            set.Add(ExtraDrops.MonsterId(prefab.name));
        }
        if (sp.enemySlot != null)
        {
            for (int k = 0; k < sp.enemySlot.Length; k++)
            {
                if (sp.enemySlot[k] != null)
                {
                    set.Add(ExtraDrops.MonsterId(sp.enemySlot[k].name));
                }
            }
        }
        HuntingSpawnTable.Row row = HuntingSpawnTable.Get(map, sp.name);
        if (row != null && !string.IsNullOrEmpty(row.Profile))
        {
            foreach (string part in row.Profile.Split('|', ',', ';', '+'))
            {
                int at = part.IndexOf('@');
                string id = (at >= 0 ? part.Substring(0, at) : part).Trim();
                if (id.Length > 0)
                {
                    set.Add(id);
                }
            }
        }
    }

    private void Read(float now)
    {
        _infos.Clear();
        List<NpcQuestDef> active = QuestService.ActiveAll();
        IList<NpcQuestDef> tracked = NpcQuestTrack.Tracked(active);
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            NpcQuestStatus st = QuestService.StatusOf(d);
            if (st != NpcQuestStatus.Available && st != NpcQuestStatus.Active && st != NpcQuestStatus.Ready)
            {
                continue;
            }
            if (_blockedQuests.TryGetValue(d.Id, out float until) && now < until)
            {
                continue;
            }
            string[] targets = d.Objective == NpcQuestObjective.Kill ? BotQuestRules.Split(d.Target) : null;
            _infos.Add(new BotQuestInfo
            {
                Id = d.Id, Status = st, Objective = d.Objective, Giver = d.Giver, ReportNpc = d.ReportNpc, Kind = d.Kind, Source = d.Source,
                Target = d.Target, Targets = targets,
                HuntMap = targets != null ? BotQuestRules.PickHuntMap(targets, _mapMobs, BgmDirector.CurrentMap) : null,
                Tracked = tracked != null && tracked.Contains(d),
                Have = d.Objective == NpcQuestObjective.Kill ? QuestService.Progress(d) : 0,
                Need = d.Count,
            });
        }
    }

    // ───── 판단 ─────

    /// <summary>1초마다: 퀘스트를 다시 읽어 사냥 방향을 갱신하고, 쉬는 중이면 새 일을 정한다. 새 일을 시작했으면 true</summary>
    public bool Evaluate(BotContext ctx)
    {
        if (ctx.Now < _nextEval)
        {
            return false;
        }
        _nextEval = ctx.Now + EvalInterval;
        if (!_scanned || _mapMobs.Count == 0)
        {
            Scan();
        }
        Read(ctx.Now);
        int h = BotQuestRules.PickHunt(_infos, TimeChange.instance == null || TimeChange.instance.isDaytime == false);
        HuntMap = h >= 0 ? _infos[h].HuntMap : null;
        _huntTargets = h >= 0 ? new HashSet<string>(_infos[h].Targets) : null;
        if (_phase != Phase.None || ctx.Now < _blockedUntil)
        {
            return false;
        }
        BotQuestJob job = BotQuestRules.PickJob(_infos, BgmDirector.CurrentMap);
        if (job.Kind == BotQuestJobKind.None)
        {
            return false;
        }
        _job = job;
        _talkTries = 0;
        Enter(Phase.Go, ctx.Now);
        Debug.Log("[PB][Bot] 퀘스트 일 시작 " + job.Kind + " " + job.QuestId + " npc=" + job.Npc + " map=" + job.Map + " lv=" + LevelSystem.Level);
        return true;
    }

    private void Enter(Phase p, float now)
    {
        _phase = p;
        _phaseSince = now;
    }

    private void Finish(BotContext ctx, bool ok, string why)
    {
        if (ok)
        {
            if (_job.Kind == BotQuestJobKind.Report) { Reported++; } else { Accepted++; }
            _blockedUntil = ctx.Now + AfterJobCooldown;
            Debug.Log("[PB][Bot] 퀘스트 일 끝 " + _job.Kind + " " + _job.QuestId + " " + why);
        }
        else
        {
            Aborts++;
            _fails.TryGetValue(_job.QuestId, out int n);
            _fails[_job.QuestId] = n + 1;
            if (n + 1 >= 2)
            {
                _blockedQuests[_job.QuestId] = ctx.Now + 600f; // 두 번 실패한 퀘스트는 10분 접는다(가방 가득·대화 실패 등)
            }
            _blockedUntil = ctx.Now + AbortCooldown;
            Debug.LogWarning("[PB][Bot] 퀘스트 일 중단 " + _job.Kind + " " + _job.QuestId + " " + why + " phase=" + _phase + " map=" + BgmDirector.CurrentMap);
        }
        _phase = Phase.None;
    }

    private bool JobDone()
    {
        NpcQuestDef d = NpcQuestCatalog.Find(_job.QuestId);
        NpcQuestStatus st = QuestService.StatusOf(d);
        return _job.Kind == BotQuestJobKind.Report ? st == NpcQuestStatus.Done : st != NpcQuestStatus.Available;
    }

    // ───── 실행 ─────

    /// <summary>일 중 한 틱. 이 틱의 조작을 맡았으면 true(intent 를 채움). foeNear = 가까이 적(싸우러 양보)</summary>
    public bool Run(BotContext ctx, bool foeNear, ref BotIntent it)
    {
        if (_phase == Phase.None)
        {
            return false;
        }
        if (JobDone() && !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive && !NpcMenu.IsOpen)
        {
            Finish(ctx, true, "state=" + QuestService.StatusOf(NpcQuestCatalog.Find(_job.QuestId)));
            return false;
        }
        if (ctx.Now - _phaseSince > (_phase == Phase.Go ? JobTimeout : 40f))
        {
            Finish(ctx, false, "시간 초과");
            return false;
        }
        BotIntent q = it;
        bool mine = true;
        if (_phase == Phase.Go)
        {
            mine = DoGo(ctx, foeNear, ref q);
        }
        else
        {
            DoTalk(ctx, ref q);
        }
        if (mine)
        {
            q.Focus = FocusQuest; // 양보(false)일 때는 사냥 쪽 intent 를 오염시키지 않는다
            q.Note = _job.Kind == BotQuestJobKind.Report ? "quest:report" : "quest:accept";
            it = q;
        }
        return mine;
    }

    private bool DoGo(BotContext ctx, bool foeNear, ref BotIntent it)
    {
        if (ctx.Obs.UiOpen)
        {
            return false; // 다른 창이 열려 있으면 CloseUi 가 닫게 양보
        }
        if (BgmDirector.CurrentMap != _job.Map)
        {
            if (foeNear)
            {
                return false;
            }
            if (!_nav.TravelTo(ctx, _job.Map, ref it))
            {
                return false; // 길 없음·막힘: 잠시 사냥, 시간 초과로 접힌다
            }
            return true;
        }
        NpcQuestGiver npc = FindNpc();
        if (npc == null)
        {
            Finish(ctx, false, "NPC 없음 " + _job.Npc);
            return false;
        }
        Vector3 p = npc.transform.position;
        float dx = p.x - ctx.Obs.X;
        if (Mathf.Abs(dx) <= NearDx && Mathf.Abs(p.y - ctx.Obs.Y) < 3f)
        {
            it.MoveX = 0;
            Enter(Phase.Talk, ctx.Now);
            _talkAt = ctx.Now + 0.4f;
            return true;
        }
        _nav.Steer(ctx, dx, p.y - ctx.Obs.Y, ref it);
        return true;
    }

    private void DoTalk(BotContext ctx, ref BotIntent it)
    {
        bool conv = PixelCrushers.DialogueSystem.DialogueManager.isConversationActive;
        if (conv)
        {
            _talkTries = 0;
            Press(ctx, BotKey.Select, ref it, 0.35f); // 타이핑 중이면 끝까지 표시, 다음 줄, 선택지는 첫 줄
            return;
        }
        if (NpcMenu.IsOpen)
        {
            BotKey k = BotQuestRules.ToNpcQuest(true, NpcMenu.Cursor, NpcMenu.RowOf(NpcMenuRules.Kind.Quest));
            Press(ctx, k, ref it);
            return;
        }
        if (ctx.Now < _talkAt)
        {
            return;
        }
        NpcQuestGiver npc = FindNpc();
        if (npc != null && Mathf.Abs(npc.transform.position.x - ctx.Obs.X) > NearDx * 2f)
        {
            Enter(Phase.Go, ctx.Now); // 밀려났다
            return;
        }
        if (_talkTries++ >= MaxTalkTries)
        {
            Finish(ctx, false, "NPC 말 걸기 실패");
            return;
        }
        _talkAt = ctx.Now + 1.4f;
        it.Menu = BotKey.Up; // NPC 상호작용 = ↑
    }

    private bool Press(BotContext ctx, BotKey key, ref BotIntent it, float extraWait = 0f)
    {
        if (key == BotKey.None || ctx.Now < _nextKey)
        {
            return false;
        }
        it.Menu = key;
        float gap = ctx.Profile.Sample("menu.focusInterval", ctx.Rng, 0.25f);
        _nextKey = ctx.Now + Mathf.Clamp(gap, 0.22f, 0.9f) + extraWait;
        return true;
    }

    private NpcQuestGiver FindNpc()
    {
        string map = BgmDirector.CurrentMap + "|" + _job.Npc;
        if (_npcMap == map && (_npc != null || Time.time < _npcRetryAt))
        {
            return _npc;
        }
        _npcRetryAt = Time.time + 1f;
        _npcMap = map;
        _npc = null;
        NpcQuestGiver[] all = Object.FindObjectsByType<NpcQuestGiver>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].isActiveAndEnabled && all[i].NpcId == _job.Npc)
            {
                _npc = all[i];
                break;
            }
        }
        return _npc;
    }
}
