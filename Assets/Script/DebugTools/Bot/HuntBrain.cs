using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사냥 판단(B1 → B2): 목표 고르기(사람 비율) → 무기 사거리 맞추기(BotRange) → 평타·스킬(쿨·HUD 확인, BotSkill) → 공격 예고 회피(BotAvoid) → 포션(BotPotion).
/// 멀면 대시로 붙고, 처치 직후 드롭은 사람 비율대로 먼저 줍는다. 적이 없으면 드롭 → 돌아다님.
/// 맵 순회: ctx.Maps 가 2장 이상이면 한 맵에 MapStay 초 넘게 있었거나 목표 없이 IdleTravel 초가 지나면 다음 맵으로 포털을 타고 간다(왕복).
/// 비율·임계는 프로필 target.* · avoid.* · potion.hpPct · press.Skill*.
/// </summary>
public sealed class HuntBrain : IBotBrain
{
    public const float AttackDy = 1.2f;
    public const float DropRange = 8f;
    public const float SameFloorDy = 2f;
    public const float UiCloseAfter = 1.5f;
    public const float IgnoreSeconds = 10f;  // 진척 없는(못 닿는) 목표는 이 시간 동안 고르지 않는다
    public const float MapStay = 120f;
    public const float IdleTravel = 20f;
    public const float TravelRetry = 30f;
    public const float TravelGiveUp = 90f;   // 한 번의 맵 이동이 이보다 오래 걸리면 잠시 사냥으로 돌아간다
    public const float ReturnGiveUp = 360f;  // 마을 귀환 전체 제한: 깊은 숲 동쪽 끝에서 마을까지 걸어서 약 250초(11-17 90분 런 — 180초는 길을 가던 중 접혔다)
    public const float DashMin = 3.5f;
    public const float DashMax = 9f;
    public const float DashGap = 1.2f;
    public const float SkillCheck = 0.5f;
    public const float SkillFailSeconds = 5f;
    public const float SwingWindow = 1.0f;
    public const float TownPathGrace = 15f;
    public const float DropUnderDx = 0.8f;
    public const float DropUnderDy = 1.5f;
    public const float DropUnderSeconds = 1.5f;
    public const float DropBlockSeconds = 30f;
    /// <summary>같은 드롭을 이만큼 쫓아도 못 줍는 드롭(닿지 않는 발판·벽 너머)은 무시 목록에 넣는다</summary>
    public const float DropChaseSeconds = 10f;
    public const float DropIgnoreSeconds = 60f;
    /// <summary>드롭 쪽 이동에 길이 없는 상태가 이만큼 이어지면 그 드롭을 무시한다(제자리에 서서 기다리지 않는다)</summary>
    public const float DropNoPathSeconds = 1.5f;
    public const float DropSameRadius = 1.5f;
    /// <summary>세로로 이만큼 넘게 떨어진 드롭은 닿지 않는다(월드 밖으로 떨어지는 드롭을 쫓다 몹 속에서 죽던 DeepForest x≈-219 사망, 11-17)</summary>
    public const float DropMaxDy = 12f;

    private readonly IBotNavigator _nav;
    private readonly DirectNavigator _wanderNav = new DirectNavigator(); // 돌아다님은 가짜 목표로 길찾기 상태를 흐리지 않게 따로
    private float _travelSince = -1f;
    private readonly BotRange _range = new BotRange();
    private readonly BotAvoid _avoid = new BotAvoid();
    private readonly BotPotion _potion = new BotPotion();
    private readonly BotErrand _errand;
    private readonly BotQuest _quest;
    private readonly BotSkillInvest _skill = new BotSkillInvest();
    private string[] _questMaps;
    private readonly bool[] _skillOn = new bool[4];
    private readonly float[] _skillFail = new float[4];
    private readonly string[] _quick = new string[4];
    private int _targetUid;
    private int _targetHp;
    private float _targetDist;
    private bool _goDrop;
    private int _wanderDir = 1;
    private float _uiSince = -1f;
    private float _lastBack;
    private readonly BotStuck _progress = new BotStuck();
    private readonly BotStall _dropStall = new BotStall();
    private int _killsSeen;
    private readonly Dictionary<int, float> _ignore = new Dictionary<int, float>();
    private readonly HashSet<int> _ignoreNow = new HashSet<int>();
    private float _lastDash = -10f;
    private int _skillSlot;
    private float _skillAt;
    private float _swingAt = -1f;
    private int _swingHp;
    private string _mapName;
    private float _mapSince;
    private float _lastTarget;
    private Vector2 _lastFightPos;
    private bool _hasFightPos;
    private string _fightMap;
    private float _homeBlockedUntil;
    private bool _homing;
    private float _targetSeen;
    private float _seekBlockedUntil;
    private const float SeekAfter = 10f;
    private const float SeekArrive = 5f;
    private const float HomeStart = 6f;
    private const float HomeArrive = 2f;
    private const float HomeAfter = 6f; // 적이 안 보인 지 이만큼 지나면 마지막으로 싸운 자리로 돌아간다(몹 없는 구덩이·막다른 길에서 못 나오던 막힘, 11-17)
    private int _circIdx;
    private int _circDir = 1;
    private float _travelBlockedUntil;
    private float _townFailSince = -1f;
    private float _dropNearSince = -1f;
    private float _dropBlockUntil;
    private float _chaseX, _chaseY, _chaseSince = -1f;
    private readonly System.Collections.Generic.List<Vector3> _ignoredDrops = new System.Collections.Generic.List<Vector3>(); // x, y, 만료 시각
    private float _lastNoneKindLog = -100f;

    public string LastNote { get; private set; } = "";
    public bool NoProgress { get; private set; }
    public int Dashes { get; private set; }
    public int Skills { get; private set; }
    /// <summary>회피 동작(뒤·앞·점프·대시)을 낸 횟수(반응 하나당 1, 판단 틱 수 아님)</summary>
    public int Avoids => _avoid.Moves;
    public int Potions => _potion.Uses;
    public int AvoidWindows => _avoid.Windows;
    public float Range => _range.Range;
    public BotErrand Errand => _errand;
    public BotQuest Quest => _quest;
    public BotSkillInvest Skill => _skill;

    public HuntBrain(IBotNavigator nav)
    {
        _nav = nav;
        _errand = new BotErrand(nav);
        _quest = new BotQuest(nav);
    }

    public void Begin(BotContext ctx)
    {
        _targetUid = 0;
        _goDrop = false;
        _chaseSince = -1f;
        _ignoredDrops.Clear();
        _dropStall.Clear();
        _nav.Reset();
        _progress.Reset(ctx.Now, ctx.Obs.X);
        _killsSeen = ctx.Kills;
        _mapName = null;
        _hasFightPos = false;
        _homing = false;
        _homeBlockedUntil = 0f;
        _seekBlockedUntil = 0f;
        _targetSeen = ctx.Now;
        _lastTarget = ctx.Now;
        _errand.Begin(ctx);
        _quest.Begin(ctx);
        _skill.Begin(ctx);
        _questMaps = null;
    }

    public BotIntent Decide(BotContext ctx)
    {
        PlayObservation o = ctx.Obs;
        BotIntent it = default(BotIntent);
        if (_errand.Run(ctx, ref it))
        {
            LastNote = it.Note;
            return it; // 마을 업무 중: 창을 직접 연다(CloseUi 가 닫지 않게 먼저)
        }
        if (_errand.Returning)
        {
            _quest.Hold(ctx.Now);
        }
        else
        {
            _quest.Evaluate(ctx);
            if (_quest.Run(ctx, NearFoe(o, 3f), ref it))
            {
                LastNote = it.Note;
                return it; // 퀘스트 받기·보고 중: 대화창을 직접 연다(CloseUi 가 닫지 않게 먼저)
            }
        }
        if (!_errand.Returning && !_quest.Busy && _skill.Run(ctx, NearFoe(o, 3f), ref it))
        {
            LastNote = it.Note;
            return it; // 스킬 투자 중: 스킬창을 직접 연다(CloseUi 가 닫지 않게 먼저)
        }
        if (CloseUi(ctx, ref it))
        {
            return it;
        }
        TrackMap(ctx);
        BotHud.QuickSlots(_quick);
        it.Quick = _potion.Decide(o.Hp, o.HpMax, ctx.Now, _quick, ctx.Profile, ctx.Rng);
        if (ctx.Kills != _killsSeen)
        {
            _killsSeen = ctx.Kills;
            _progress.Progress(ctx.Now);
            _targetUid = 0;
            _swingAt = -1f;
            _goDrop = o.Drop.Found && ctx.Rng.NextDouble() < ctx.Profile.RatioOr("target.dropFirst", 0f);
        }
        BotAvoid.React react = _avoid.Update(o.Foes, ctx.Now, ctx.Profile, ctx.Rng);
        bool dropOk = DropOk(ctx);
        if (_goDrop && dropOk)
        {
            return SteerDrop(ctx, ref it);
        }
        _goDrop = false;
        if (ReturnToTown(ctx, ref it))
        {
            return it;
        }
        if (!_quest.Busy && !_errand.Returning && Travel(ctx, ref it)) // 퀘스트 일(마을 보고 등)·귀환이 맵 이동을 맡는 동안엔 순회·Lv 맵 이동을 쉰다 — 둘이 반대 포털로 번갈아 끌던 계단 진동(11-17)
        {
            return it;
        }
        if (SeekQuestMob(ctx, ref it))
        {
            return it;
        }
        int idx = FindTarget(ctx);
        if (idx < 0)
        {
            if (dropOk)
            {
                return SteerDrop(ctx, ref it);
            }
            return Wander(ctx, ref it);
        }
        _lastTarget = ctx.Now;
        if (o.Grounded && !o.OnRope)
        {
            _lastFightPos = new Vector2(o.X, o.Y); // 땅에 서서 싸운 자리만(공중·로프 위 좌표는 길찾기 목표로 못 쓴다)
            _hasFightPos = true;
            _fightMap = _mapName;
        }
        PlayObsEnemies.Foe f = o.Foes.Foes[idx];
        TrackProgress(ctx, f);
        if (NoProgress)
        {
            Ignore(ctx, f.Uid);
        }
        it.Focus = f.Uid;
        Fight(ctx, f, ref it);
        ApplyAvoid(react, ref it);
        LastNote = it.Note;
        return it;
    }

    /// <summary>
    /// 줍으러 갈 드롭이 있나. 발밑(가로 0.8·세로 1.5 안)에 1.5초 넘게 있는데 안 주워지면 가방이 가득 찬 것이라 30초 동안 무시한다
    /// (11-15 NormalForest x≈364~383 "진척 없음 do=drop" 8건의 원인: 가방 빈칸 0 에서 같은 드롭 위를 왔다 갔다 함).
    /// </summary>
    private bool DropOk(BotContext ctx)
    {
        PlayObservation o = ctx.Obs;
        if (!o.Drop.Found || Mathf.Abs(o.Drop.Dx) >= DropRange || Mathf.Abs(o.Drop.Dy) > DropMaxDy || ctx.Now < _dropBlockUntil)
        {
            _dropNearSince = -1f;
            _chaseSince = -1f;
            return false;
        }
        if (IgnoredDrop(ctx, o.X + o.Drop.Dx, o.Y + o.Drop.Dy))
        {
            return false;
        }
        if (Mathf.Abs(o.Drop.Dx) < DropUnderDx && Mathf.Abs(o.Drop.Dy) < DropUnderDy)
        {
            _chaseSince = -1f;
            if (_dropNearSince < 0f)
            {
                _dropNearSince = ctx.Now;
            }
            else if (ctx.Now - _dropNearSince > DropUnderSeconds)
            {
                _dropNearSince = -1f;
                _dropBlockUntil = ctx.Now + DropBlockSeconds;
                Debug.LogWarning("[PB][Bot] 발밑 드롭이 안 주워짐(가방 가득?) — " + DropBlockSeconds + "초 무시 map=" + BgmDirector.CurrentMap + " x=" + o.X.ToString("F1"));
                return false;
            }
        }
        else
        {
            _dropNearSince = -1f;
            if (ChaseTooLong(ctx, o.X + o.Drop.Dx, o.Y + o.Drop.Dy))
            {
                return false;
            }
        }
        return true;
    }

    private bool IgnoredDrop(BotContext ctx, float x, float y)
    {
        for (int i = _ignoredDrops.Count - 1; i >= 0; i--)
        {
            Vector3 g = _ignoredDrops[i];
            if (ctx.Now >= g.z)
            {
                _ignoredDrops.RemoveAt(i);
            }
            else if (Mathf.Abs(g.x - x) < DropSameRadius && Mathf.Abs(g.y - y) < DropSameRadius)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>같은 자리 드롭을 DropChaseSeconds 넘게 쫓았으면 무시 목록에 넣는다(닿지 않는 드롭)</summary>
    private bool ChaseTooLong(BotContext ctx, float x, float y)
    {
        if (_chaseSince < 0f || Mathf.Abs(_chaseX - x) > DropSameRadius || Mathf.Abs(_chaseY - y) > DropSameRadius)
        {
            _chaseX = x;
            _chaseY = y;
            _chaseSince = ctx.Now;
            return false;
        }
        if (ctx.Now - _chaseSince <= DropChaseSeconds)
        {
            return false;
        }
        _chaseSince = -1f;
        _ignoredDrops.Add(new Vector3(x, y, ctx.Now + DropIgnoreSeconds));
        Debug.LogWarning("[PB][Bot] 닿지 않는 드롭 " + DropIgnoreSeconds + "초 무시 map=" + BgmDirector.CurrentMap + " drop=(" + x.ToString("F1") + "," + y.ToString("F1") + ")");
        return true;
    }

    // ───── 전투 ─────

    private void Fight(BotContext ctx, PlayObsEnemies.Foe f, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        Weapon w = Weapon.Equipped;
        WeaponKind kind = w == null ? WeaponKind.None : w.Kind; // 구 프리팹 _kind 보충은 Weapon.Kind 가 한다
        if (kind != WeaponKind.None)
        {
            _range.SetKind(kind); // 무기를 바꾸는 순간 None 이 잠깐 보이면 기본 1.3 으로 되돌아가던 것을 막는다(11-15 DeepForest 에서 range 1.3 으로 보인 구간의 유력 원인)
        }
        else if (ctx.Now - _lastNoneKindLog > 30f)
        {
            _lastNoneKindLog = ctx.Now;
            Debug.LogWarning("[PB][Bot] 무기 계열 None w=" + (w == null ? "null" : w.name + " mastery=" + w.MasteryKey) + " map=" + BgmDirector.CurrentMap + " range=" + _range.Range.ToString("F1"));
        }
        float range = _range.Range;
        int side = f.Dx >= 0f ? 1 : -1;
        CheckSkillFail(ctx);
        bool sameFloor = Mathf.Abs(f.Dy) <= SameFloorDy * 0.75f;
        if (sameFloor && Mathf.Abs(f.Dx) <= Mathf.Max(range + 0.5f, 3f) && o.Face == side && !o.UiOpen)
        {
            TrySkill(ctx, ref it);
        }
        // 공중에서 다른 발판의 몹: 공격 분기가 이동을 끊어 점프가 제자리로 떨어진다(SideRoad x≈-386 계단) → 착지 전엔 접근을 이어 간다
        bool airOtherFloor = !o.Grounded && Mathf.Abs(f.Dy) > 0.5f;
        if (Mathf.Abs(f.Dx) <= range && Mathf.Abs(f.Dy) <= AttackDy && !airOtherFloor)
        {
            it.MoveX = o.Face != side ? side : 0; // 반대를 보고 있으면 한 걸음 돌아선다
            it.Attack = true;
            it.Note = "attack";
            TrackSwing(ctx, f);
            return;
        }
        _swingAt = -1f;
        bool ok = _nav.Steer(ctx, f.Dx, f.Dy, ref it);
        it.Note = "approach";
        if (!ok)
        {
            Ignore(ctx, f.Uid); // 길 없음·막힘: 다른 목표
        }
        if (sameFloor && Mathf.Abs(f.Dx) >= DashMin && Mathf.Abs(f.Dx) <= DashMax && it.MoveX == side && o.Face == side
            && o.Grounded && ctx.Now - _lastDash > DashGap)
        {
            _lastDash = ctx.Now;
            it.Dash = true;
            Dashes++;
        }
        NoProgress = NoProgress || _progress.NoProgress(ctx.Now);
    }

    private void TrySkill(BotContext ctx, ref BotIntent it)
    {
        if (_skillSlot > 0)
        {
            return; // 앞 스킬 결과 확인 중
        }
        BotHud.SkillSlots(_skillOn);
        int slot = BotSkill.Pick(ctx.Obs.Cooldowns, _skillOn, _skillFail, ctx.Now, ctx.Rng.NextDouble(), ctx.Profile);
        if (slot > 0)
        {
            it.Skill = slot;
            _skillSlot = slot;
            _skillAt = ctx.Now;
            Skills++;
        }
    }

    /// <summary>눌렀는데 쿨이 안 돌았으면(스태미나 부족·동작 중) 그 칸을 잠시 뺀다</summary>
    private void CheckSkillFail(BotContext ctx)
    {
        if (_skillSlot <= 0 || ctx.Now - _skillAt < SkillCheck)
        {
            return;
        }
        if (ctx.Obs.Cooldowns[_skillSlot - 1] <= 0f)
        {
            _skillFail[_skillSlot - 1] = ctx.Now + SkillFailSeconds;
        }
        _skillSlot = 0;
    }

    private void TrackSwing(BotContext ctx, PlayObsEnemies.Foe f)
    {
        if (_swingAt < 0f)
        {
            _swingAt = ctx.Now;
            _swingHp = f.HpPercent;
            return;
        }
        if (ctx.Now - _swingAt >= SwingWindow)
        {
            _range.Swing(f.HpPercent < _swingHp);
            _swingAt = ctx.Now;
            _swingHp = f.HpPercent;
        }
    }

    private void ApplyAvoid(BotAvoid.React react, ref BotIntent it)
    {
        switch (react)
        {
            case BotAvoid.React.Back:
                it.MoveX = -_avoid.FoeDir;
                it.Attack = false;
                break;
            case BotAvoid.React.Forward:
                it.MoveX = _avoid.FoeDir;
                break;
            case BotAvoid.React.Jump:
                it.Jump = true;
                break;
            case BotAvoid.React.Dash:
                it.MoveX = -_avoid.FoeDir;
                it.Dash = true;
                it.Attack = false;
                break;
            default:
                return;
        }
        it.Note = "avoid";
    }

    // ───── 맵 순회 ─────

    private void TrackMap(BotContext ctx)
    {
        string map = BgmDirector.CurrentMap;
        if (map == _mapName)
        {
            return;
        }
        _mapName = map;
        _mapSince = ctx.Now;
        _targetSeen = ctx.Now;
        _lastTarget = ctx.Now;
        _travelBlockedUntil = 0f;
        _travelSince = -1f;
        _ignore.Clear();
        string[] maps = EffMaps(ctx);
        int i = maps != null ? System.Array.IndexOf(maps, map) : -1;
        if (i >= 0)
        {
            _circIdx = i;
        }
    }

    /// <summary>다음 맵: 순회 목록 왕복. 지금 맵이 목록 밖(마을 등)이면 첫 맵</summary>
    public static int NextIndex(int len, int cur, bool inList, ref int dir)
    {
        if (!inList)
        {
            return 0;
        }
        if (cur + dir < 0 || cur + dir >= len)
        {
            dir = -dir;
        }
        return cur + dir;
    }

    private int NextPeek(int len, bool inList)
    {
        int dir = _circDir;
        return NextIndex(len, _circIdx, inList, ref dir);
    }

    /// <summary>귀환 중이면 마을로: 귀환 주문서가 있으면 쓰고, 없으면 포털 길찾기. 근처에 적이 있으면 먼저 싸운다(마을 도착 뒤는 BotErrand.Run)</summary>
    private bool ReturnToTown(BotContext ctx, ref BotIntent it)
    {
        if (!_errand.Evaluate(ctx) || _mapName == BotErrand.Town)
        {
            return false;
        }
        bool foe = NearFoe(ctx.Obs, 3f);
        if (_errand.TryRecall(ctx, foe, ref it))
        {
            return true;
        }
        if (foe)
        {
            return false;
        }
        if (_errand.ReturnSeconds(ctx.Now) > ReturnGiveUp)
        {
            _errand.Abort(ctx, "마을 가는 데 너무 오래 걸림");
            return false;
        }
        it.Focus = BotIntent.FocusWander;
        it.Note = "toTown";
        if (!_nav.TravelTo(ctx, BotErrand.Town, ref it))
        {
            // 간선 금지(막힘 직후 20초) 때문에 잠깐 길이 없을 수 있다: 15초 넘게 계속 없을 때만 접는다
            if (_townFailSince < 0f)
            {
                _townFailSince = ctx.Now;
            }
            else if (ctx.Now - _townFailSince > TownPathGrace)
            {
                _townFailSince = -1f;
                _errand.Abort(ctx, "마을 경로 없음 " + _mapName + " 이유=" + (_nav is GraphNavigator ? ((GraphNavigator)_nav).TravelFail : "-"));
            }
            return false;
        }
        _townFailSince = -1f;
        LastNote = it.Note;
        return true;
    }

    /// <summary>
    /// 사냥 맵 목록. 따라가는 처치 퀘스트가 있으면 그 목표 몹이 나오는 맵 하나(맵 이동이 허용된 실행에서만 — ctx.Maps 가 비면 이동 없음 그대로)
    /// </summary>
    private string[] EffMaps(BotContext ctx)
    {
        if (ctx.Maps == null || ctx.Maps.Length == 0)
        {
            return ctx.Maps;
        }
        string want = _quest.HuntMap;
        if (want == null && ctx.LevelMaps)
        {
            want = BotQuestRules.LevelMap(LevelSystem.Level, ctx.Maps); // 퀘스트 목표 맵이 없으면 Lv 에 맞는 사냥터
        }
        if (want == null)
        {
            return ctx.Maps;
        }
        if (_questMaps == null || _questMaps[0] != want)
        {
            _questMaps = new[] { want };
        }
        return _questMaps;
    }

    private bool Travel(BotContext ctx, ref BotIntent it)
    {
        string[] maps = EffMaps(ctx);
        if (maps == null || maps.Length == 0 || ctx.Now < _travelBlockedUntil)
        {
            return false;
        }
        bool inList = System.Array.IndexOf(maps, _mapName) >= 0;
        bool due = !inList || (maps.Length > 1 && (ctx.Now - _mapSince > MapStay || ctx.Now - _lastTarget > IdleTravel));
        if (!due || (inList && NearFoe(ctx.Obs, 3f)))
        {
            _travelSince = -1f;
            return false;
        }
        if (_travelSince < 0f)
        {
            _travelSince = ctx.Now;
        }
        else if (ctx.Now - _travelSince > TravelGiveUp)
        {
            _travelSince = -1f;
            _travelBlockedUntil = ctx.Now + TravelRetry;
            Debug.LogWarning("[PB][Bot] 맵 이동 시간 초과 " + _mapName + " -> " + maps[NextPeek(maps.Length, inList)]);
            return false;
        }
        int dir = _circDir;
        int next = NextIndex(maps.Length, _circIdx, inList, ref dir);
        it.Focus = BotIntent.FocusWander;
        it.Note = "travel";
        if (!_nav.TravelTo(ctx, maps[next], ref it))
        {
            _travelBlockedUntil = ctx.Now + TravelRetry; // 길 없음·막힘: 잠시 사냥하다 다시 시도
            Debug.LogWarning("[PB][Bot] 맵 이동 경로 없음 " + _mapName + " -> " + maps[next] + " 이유=" + (_nav is GraphNavigator ? ((GraphNavigator)_nav).TravelFail : "-") + " x=" + ctx.Obs.X.ToString("F1"));
            return false;
        }
        _circDir = dir;
        LastNote = it.Note;
        return true;
    }

    private static bool NearFoe(PlayObservation o, float dx)
    {
        for (int i = 0; i < o.Foes.FoeCount; i++)
        {
            PlayObsEnemies.Foe f = o.Foes.Foes[i];
            if (Alive(f) && Mathf.Abs(f.Dx) <= dx && Mathf.Abs(f.Dy) <= SameFloorDy)
            {
                return true;
            }
        }
        return false;
    }

    // ───── 공통 ─────

    private void Ignore(BotContext ctx, int uid)
    {
        _ignore[uid] = ctx.Now + IgnoreSeconds;
        _progress.Progress(ctx.Now);
        _targetUid = 0;
        NoProgress = false;
    }

    /// <summary>드롭 쪽으로 간다. 이동 그래프에 길이 없어(NoPath) 1.5초 넘게 못 가면 그 드롭을 무시 목록에 넣는다</summary>
    private BotIntent SteerDrop(BotContext ctx, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        float dx = o.Drop.Dx;
        float dy = o.Drop.Dy;
        it.Focus = BotIntent.FocusDrop;
        it.Note = "drop";
        bool moved = _nav.Steer(ctx, dx, dy, ref it);
        if (_dropStall.Update(ctx.Now, !moved, DropNoPathSeconds))
        {
            _ignoredDrops.Add(new Vector3(o.X + dx, o.Y + dy, ctx.Now + DropIgnoreSeconds));
            Debug.LogWarning("[PB][Bot] 길 없는 드롭 " + DropIgnoreSeconds + "초 무시 map=" + BgmDirector.CurrentMap + " drop=(" + (o.X + dx).ToString("F1") + "," + (o.Y + dy).ToString("F1") + ")");
        }
        NoProgress = _progress.NoProgress(ctx.Now);
        LastNote = it.Note;
        return it;
    }

    private BotIntent Steer(BotContext ctx, float dx, float dy, int focus, string note, ref BotIntent it)
    {
        it.Focus = focus;
        it.Note = note;
        _nav.Steer(ctx, dx, dy, ref it);
        NoProgress = _progress.NoProgress(ctx.Now);
        LastNote = note;
        return it;
    }

    /// <summary>창이 열린 채 ESC 로 닫아 복구한 횟수(창 열림 1건 = 1회)</summary>
    public int UiRecoveries { get; private set; }
    private bool _uiCounted;

    private bool CloseUi(BotContext ctx, ref BotIntent it)
    {
        if (!ctx.Obs.UiOpen)
        {
            _uiSince = -1f;
            _uiCounted = false;
            return false;
        }
        if (_uiSince < 0f)
        {
            _uiSince = ctx.Now;
        }
        if (ctx.Now - _uiSince > UiCloseAfter && ctx.Now - _lastBack > UiCloseAfter)
        {
            it.Back = true;
            _lastBack = ctx.Now;
            if (!_uiCounted)
            {
                _uiCounted = true; // 창 하나가 열린 한 번(여러 번 ESC 가 필요해도 1회)을 복구 1회로 센다 — 보고서 B5-2
                UiRecoveries++;
                Debug.LogWarning("[PB][Bot] UI 복구(ESC) " + (ctx.Now - _uiSince).ToString("F1") + "s 열려 있음 map=" + _mapName + " 누적=" + UiRecoveries);
            }
        }
        it.Focus = BotIntent.FocusUi;
        it.Note = "ui";
        LastNote = it.Note;
        return true;
    }

    private void TrackProgress(BotContext ctx, PlayObsEnemies.Foe f)
    {
        if (f.Uid != _targetUid)
        {
            _targetUid = f.Uid;
            _targetHp = f.HpPercent;
            _targetDist = f.Dist;
            _progress.Progress(ctx.Now);
            NoProgress = false;
            return;
        }
        if (f.HpPercent < _targetHp || f.Dist < _targetDist - 0.5f)
        {
            _targetHp = f.HpPercent;
            _targetDist = f.Dist;
            _progress.Progress(ctx.Now);
        }
        NoProgress = _progress.NoProgress(ctx.Now);
    }

    /// <summary>살아 있는 현재 목표를 이어 가고, 없으면 사람 비율대로 새로 고른다</summary>
    private int FindTarget(BotContext ctx)
    {
        PlayObsEnemies foes = ctx.Obs.Foes;
        _ignoreNow.Clear();
        if (_ignore.Count > 64)
        {
            _ignore.Clear(); // 풀 재사용 uid 가 쌓이지 않게(만료 판정보다 단순하게 비운다)
        }
        foreach (KeyValuePair<int, float> kv in _ignore)
        {
            if (kv.Value > ctx.Now)
            {
                _ignoreNow.Add(kv.Key);
            }
        }
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (foes.Foes[i].Uid == _targetUid && Alive(foes.Foes[i]) && !_ignoreNow.Contains(_targetUid)
                && (!_quest.HasHuntTargets || _quest.IsTarget(foes.Foes[i].Id) || !AnyTarget(foes)))
            {
                return i;
            }
        }
        PreferQuestTargets(foes);
        return Choose(foes, ctx.Rng.NextDouble(), ctx.Profile, _ignoreNow);
    }

    /// <summary>따라가는 퀘스트의 목표 몹이 쓸 만하게 보이면 다른 몹은 이번 선택에서 뺀다(목표 몹이 없으면 아무거나 — 가만히 있지 않는다)</summary>
    private void PreferQuestTargets(PlayObsEnemies foes)
    {
        if (!_quest.HasHuntTargets)
        {
            return;
        }
        bool any = false;
        for (int i = 0; i < foes.FoeCount && !any; i++)
        {
            any = Usable(foes.Foes[i], _ignoreNow) && _quest.IsTarget(foes.Foes[i].Id);
        }
        if (!any)
        {
            return;
        }
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (!_quest.IsTarget(foes.Foes[i].Id))
            {
                _ignoreNow.Add(foes.Foes[i].Uid);
            }
        }
    }

    private bool AnyTarget(PlayObsEnemies foes)
    {
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (Alive(foes.Foes[i]) && _quest.IsTarget(foes.Foes[i].Id))
            {
                return true;
            }
        }
        return false;
    }

    private static bool Alive(PlayObsEnemies.Foe f)
    {
        return f.State != 5 && f.HpPercent > 0 && !f.Prop; // 나무·바위 같은 파괴 오브젝트는 몹이 아니다(목표·근접 위협에서 뺀다)
    }

    private static bool Usable(PlayObsEnemies.Foe f, HashSet<int> ignore)
    {
        return Alive(f) && (ignore == null || !ignore.Contains(f.Uid));
    }

    /// <summary>
    /// 순수 선택: 같은 층(|dy|≤2) 후보가 있으면 그 안에서. r &lt; lowestHp 비율 → HP 최저, 그다음 nearest 비율 → 최근접, 나머지 → 두 번째로 가까운 것.
    /// Foes 는 거리순이다. 후보 없으면 -1.
    /// </summary>
    public static int Choose(PlayObsEnemies foes, double r, BotProfile profile, HashSet<int> ignore = null)
    {
        bool anySame = false;
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (Usable(foes.Foes[i], ignore) && Mathf.Abs(foes.Foes[i].Dy) <= SameFloorDy)
            {
                anySame = true;
                break;
            }
        }
        int first = -1, second = -1, lowest = -1;
        for (int i = 0; i < foes.FoeCount; i++)
        {
            PlayObsEnemies.Foe f = foes.Foes[i];
            if (!Usable(f, ignore) || (anySame && Mathf.Abs(f.Dy) > SameFloorDy))
            {
                continue;
            }
            if (first < 0)
            {
                first = i;
            }
            else if (second < 0)
            {
                second = i;
            }
            if (lowest < 0 || f.HpPercent < foes.Foes[lowest].HpPercent)
            {
                lowest = i;
            }
        }
        if (first < 0)
        {
            return -1;
        }
        float pLow = profile.RatioOr("target.lowestHp", 0f);
        float pNear = profile.RatioOr("target.nearest", 1f);
        if (r < pLow)
        {
            return lowest;
        }
        if (r < pLow + pNear || second < 0)
        {
            return first;
        }
        return second;
    }

    /// <summary>
    /// 따라가는 처치 퀘스트의 목표 몹이 한동안 안 보이면 그 몹이 나오는 스포너 자리로 간다(11-17: 이끼껍질 딱정벌레 스포너가 안쪽 구덩이에 있어
    /// 토끼·쥐만 잡다 퀘스트가 안 끝났다). 가까이 적이 있으면 싸우고, 길이 없으면 10초 쉰다.
    /// </summary>
    private bool SeekQuestMob(BotContext ctx, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        if (!_quest.HasHuntTargets)
        {
            return false;
        }
        if (AnyTarget(o.Foes))
        {
            _targetSeen = ctx.Now;
            return false;
        }
        if (ctx.Now - _targetSeen < SeekAfter || ctx.Now < _seekBlockedUntil || NearFoe(o, 3f) || !_quest.TryFindSpawn(new Vector2(o.X, o.Y), out Vector2 sp))
        {
            return false;
        }
        if (Mathf.Abs(sp.x - o.X) + Mathf.Abs(sp.y - o.Y) < SeekArrive)
        {
            return false; // 도착했는데도 안 보이면(리스폰 대기) 근처에서 평소대로
        }
        it.Focus = BotIntent.FocusWander;
        it.Note = "seek";
        if (_nav.Steer(ctx, sp.x - o.X, sp.y - o.Y, ref it))
        {
            LastNote = it.Note;
            return true;
        }
        _seekBlockedUntil = ctx.Now + 10f;
        return false;
    }

    private BotIntent Wander(BotContext ctx, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        float homeDist = _hasFightPos && _fightMap == _mapName ? Mathf.Abs(_lastFightPos.x - o.X) + Mathf.Abs(_lastFightPos.y - o.Y) : 0f;
        if (_homing && homeDist < HomeArrive)
        {
            _homing = false; // 도착(들어가는 6 과 나오는 2 를 달리해 경계에서 떨지 않는다)
        }
        else if (!_homing && ctx.Now > _homeBlockedUntil && ctx.Now - _lastTarget > HomeAfter && homeDist > HomeStart)
        {
            _homing = true;
        }
        if (_homing && ctx.Now > _homeBlockedUntil)
        {
            it.Focus = BotIntent.FocusWander;
            it.Note = "home";
            if (_nav.Steer(ctx, _lastFightPos.x - o.X, _lastFightPos.y - o.Y, ref it))
            {
                LastNote = it.Note;
                return it;
            }
            _homing = false;
            _homeBlockedUntil = ctx.Now + 10f; // 길이 없으면 10초 쉬고 다시 시도(NoPath 막힘 기록이 쌓이지 않게)
        }
        float edge = _wanderDir > 0 ? o.Terrain.EdgeR : o.Terrain.EdgeL;
        // DropF -1 = 끝 너머에 바닥이 없는 구덩이(SideRoad x≈-437, 밤 몹 없을 때 낙사 복귀 반복). 닿는 낮은 단(0~2)만 내려간다
        bool pit = o.Terrain.DropF > 2f || o.Terrain.DropF < 0f;
        if ((o.Grounded && edge >= 0f && edge < 0.6f && pit) || _nav.BlockedAhead(ctx, _wanderDir))
        {
            _wanderDir = -_wanderDir; // 낭떠러지·막힌 끝 앞에서 돌아선다
        }
        BotTrace.MarkEdge("wander", -1);
        it.Focus = BotIntent.FocusWander;
        it.Note = "wander";
        it.MoveX = _wanderDir;
        BotIntent probe = it;
        _wanderNav.Steer(ctx, _wanderDir * 10f, 0f, ref probe);
        if (probe.MoveX != _wanderDir && probe.MoveX != 0)
        {
            _wanderDir = probe.MoveX; // 막혀서 반대로
        }
        it.MoveX = probe.MoveX;
        it.MoveY = probe.MoveY;
        it.Jump = probe.Jump;
        it.JumpHold = probe.JumpHold;
        LastNote = it.Note;
        return it;
    }
}
