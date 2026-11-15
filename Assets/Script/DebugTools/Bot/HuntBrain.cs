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
    public const float DashMin = 3.5f;
    public const float DashMax = 9f;
    public const float DashGap = 1.2f;
    public const float SkillCheck = 0.5f;
    public const float SkillFailSeconds = 5f;
    public const float SwingWindow = 1.0f;

    private readonly IBotNavigator _nav;
    private readonly DirectNavigator _wanderNav = new DirectNavigator(); // 돌아다님은 가짜 목표로 길찾기 상태를 흐리지 않게 따로
    private float _travelSince = -1f;
    private readonly BotRange _range = new BotRange();
    private readonly BotAvoid _avoid = new BotAvoid();
    private readonly BotPotion _potion = new BotPotion();
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
    private int _circIdx;
    private int _circDir = 1;
    private float _travelBlockedUntil;

    public string LastNote { get; private set; } = "";
    public bool NoProgress { get; private set; }
    public int Dashes { get; private set; }
    public int Skills { get; private set; }
    /// <summary>회피 동작(뒤·앞·점프·대시)을 낸 횟수(반응 하나당 1, 판단 틱 수 아님)</summary>
    public int Avoids => _avoid.Moves;
    public int Potions => _potion.Uses;
    public int AvoidWindows => _avoid.Windows;
    public float Range => _range.Range;

    public HuntBrain(IBotNavigator nav)
    {
        _nav = nav;
    }

    public void Begin(BotContext ctx)
    {
        _targetUid = 0;
        _goDrop = false;
        _nav.Reset();
        _progress.Reset(ctx.Now, ctx.Obs.X);
        _killsSeen = ctx.Kills;
        _mapName = null;
        _lastTarget = ctx.Now;
    }

    public BotIntent Decide(BotContext ctx)
    {
        PlayObservation o = ctx.Obs;
        BotIntent it = default(BotIntent);
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
        if (_goDrop && o.Drop.Found && Mathf.Abs(o.Drop.Dx) < DropRange)
        {
            return Steer(ctx, o.Drop.Dx, o.Drop.Dy, BotIntent.FocusDrop, "drop", ref it);
        }
        _goDrop = false;
        if (Travel(ctx, ref it))
        {
            return it;
        }
        int idx = FindTarget(ctx);
        if (idx < 0)
        {
            if (o.Drop.Found && Mathf.Abs(o.Drop.Dx) < DropRange)
            {
                return Steer(ctx, o.Drop.Dx, o.Drop.Dy, BotIntent.FocusDrop, "drop", ref it);
            }
            return Wander(ctx, ref it);
        }
        _lastTarget = ctx.Now;
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

    // ───── 전투 ─────

    private void Fight(BotContext ctx, PlayObsEnemies.Foe f, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        Weapon w = Weapon.Equipped;
        // 구 무기 프리팹은 _kind 가 비어 있어 ItemSheet WeaponType 으로 보충한다(WeaponMoveProfiles 와 같은 규칙). 안 읽으면 사거리가 1.3 기본값에 머문다
        WeaponKind kind = w == null ? WeaponKind.None : (w.Kind != WeaponKind.None ? w.Kind : WeaponMasteryService.KindOf(w.MasteryKey));
        _range.SetKind(kind);
        float range = _range.Range;
        int side = f.Dx >= 0f ? 1 : -1;
        CheckSkillFail(ctx);
        bool sameFloor = Mathf.Abs(f.Dy) <= SameFloorDy * 0.75f;
        if (sameFloor && Mathf.Abs(f.Dx) <= Mathf.Max(range + 0.5f, 3f) && o.Face == side && !o.UiOpen)
        {
            TrySkill(ctx, ref it);
        }
        if (Mathf.Abs(f.Dx) <= range && Mathf.Abs(f.Dy) <= AttackDy)
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
        _lastTarget = ctx.Now;
        _travelBlockedUntil = 0f;
        _travelSince = -1f;
        _ignore.Clear();
        int i = ctx.Maps != null ? System.Array.IndexOf(ctx.Maps, map) : -1;
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

    private bool Travel(BotContext ctx, ref BotIntent it)
    {
        string[] maps = ctx.Maps;
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
            Debug.LogWarning("[PB][Bot] 맵 이동 경로 없음 " + _mapName + " -> " + maps[next]);
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

    private BotIntent Steer(BotContext ctx, float dx, float dy, int focus, string note, ref BotIntent it)
    {
        it.Focus = focus;
        it.Note = note;
        _nav.Steer(ctx, dx, dy, ref it);
        NoProgress = _progress.NoProgress(ctx.Now);
        LastNote = note;
        return it;
    }

    private bool CloseUi(BotContext ctx, ref BotIntent it)
    {
        if (!ctx.Obs.UiOpen)
        {
            _uiSince = -1f;
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
            if (foes.Foes[i].Uid == _targetUid && Alive(foes.Foes[i]) && !_ignoreNow.Contains(_targetUid))
            {
                return i;
            }
        }
        return Choose(foes, ctx.Rng.NextDouble(), ctx.Profile, _ignoreNow);
    }

    private static bool Alive(PlayObsEnemies.Foe f)
    {
        return f.State != 5 && f.HpPercent > 0;
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

    private BotIntent Wander(BotContext ctx, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        float edge = _wanderDir > 0 ? o.Terrain.EdgeR : o.Terrain.EdgeL;
        if ((o.Grounded && edge >= 0f && edge < 0.6f && o.Terrain.DropF > 2f) || _nav.BlockedAhead(ctx, _wanderDir))
        {
            _wanderDir = -_wanderDir; // 낭떠러지·막힌 끝 앞에서 돌아선다
        }
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
