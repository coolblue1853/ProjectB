using System;
using System.Collections.Generic;

public enum CookPhase { Select, Play, Result }

/// <summary>
/// 요리 한 판(순수 상태 머신). 선택(Select: 재료를 가진 레시피를 좌우로 고름) → 시작(Confirm) → 굽기/끓이기(Play) → 결과(Result).
/// 재료 소모와 보상 지급은 CookingStove 몫. 입력은 매 틱 held(공격 키 누르고 있음)·pressed(이번 프레임에 누름)로 받는다.
/// </summary>
public sealed class CookingSession
{
    private const float Step = 0.05f; // 프레임이 길어도 이 단위로 나눠 계산

    private readonly List<CookRecipe> _recipes;
    private readonly Func<float> _random;
    private readonly List<float> _errors = new List<float>();
    private float _grillClock;
    private bool _roundResolved;
    private bool _windowStarted;
    private float _driftTimer;
    private float _drift;
    private float _inZone;
    private float _overheat;

    public CookPhase Phase { get; private set; }
    public IReadOnlyList<CookRecipe> Recipes => _recipes;
    public int Index { get; private set; }
    public CookRecipe Recipe { get; private set; }
    public CookGrade Grade { get; private set; }
    public bool Cancelled { get; private set; }
    public bool Overheated { get; private set; }
    /// <summary>플레이 시작 뒤 경과(결과가 나면 결과 뒤 경과)</summary>
    public float Elapsed { get; private set; }

    // 굽기 표시용
    public int Round { get; private set; }
    public bool WindowOpen { get; private set; }
    public float GrillCursor { get; private set; }
    public float GrillTarget { get; private set; }
    public IReadOnlyList<float> Errors => _errors;
    public int Misses { get; private set; }
    /// <summary>마지막으로 입력한 회의 오차(-1 = 입력 없이 지나감/아직 없음)</summary>
    public float LastError { get; private set; } = -1f;

    // 끓이기 표시용
    public float Heat { get; private set; }
    public float InZoneSeconds => _inZone;
    public float InZoneRatio => Elapsed > 0f ? _inZone / Math.Min(Elapsed, CookingRules.BoilTime) : 0f;
    public float OverheatTime => _overheat;

    public CookingSession(List<CookRecipe> available, Func<float> random)
    {
        _recipes = available;
        _random = random;
        Phase = CookPhase.Select;
    }

    /// <summary>선택 단계: 목록 좌우 이동(양끝에서 멈춤)</summary>
    public void Select(int dx)
    {
        if (Phase != CookPhase.Select || _recipes.Count == 0)
        {
            return;
        }
        Index = Math.Max(0, Math.Min(_recipes.Count - 1, Index + dx));
    }

    /// <summary>선택 단계: 고른 레시피로 시작. 레시피가 없으면 null</summary>
    public CookRecipe Confirm()
    {
        if (Phase != CookPhase.Select || _recipes.Count == 0)
        {
            return null;
        }
        Recipe = _recipes[Index];
        Phase = CookPhase.Play;
        Elapsed = 0f;
        Heat = CookingRules.HeatStart;
        return Recipe;
    }

    public void Tick(float dt, bool held, bool pressed)
    {
        if (Phase == CookPhase.Result)
        {
            Elapsed += dt;
            return;
        }
        if (Phase != CookPhase.Play)
        {
            return;
        }
        if (Recipe.Method == CookMethod.Grill)
        {
            TickGrill(dt, pressed);
        }
        else
        {
            TickBoil(dt, held);
        }
    }

    /// <summary>도중 중단(ESC·피격·사망·이탈). 결과가 났으면 무시. 시작 뒤 중단이면 재료는 돌려받지 못한다</summary>
    public void Cancel()
    {
        if (Phase != CookPhase.Result)
        {
            Cancelled = true;
            Finish(CookGrade.Fail);
        }
    }

    private void TickGrill(float dt, bool pressed)
    {
        float remaining = dt;
        bool press = pressed;
        while (remaining > 0f && Phase == CookPhase.Play)
        {
            float h = Math.Min(Step, remaining);
            remaining -= h;
            _grillClock += h;
            Elapsed = _grillClock;
            UpdateWindow();
            if (press && WindowOpen)
            {
                Resolve(Math.Abs(GrillCursor - GrillTarget));
            }
            press = false;
            if (_grillClock >= (Round + 1) * CookingRules.GrillRoundTime)
            {
                EndRound();
            }
        }
    }

    private void UpdateWindow()
    {
        float roundEnd = (Round + 1) * CookingRules.GrillRoundTime;
        float windowStart = roundEnd - CookingRules.GrillWindowTime;
        bool open = _grillClock >= windowStart && _grillClock < roundEnd && !_roundResolved;
        if (open && !_windowStarted)
        {
            _windowStarted = true;
            GrillTarget = CookingRules.GrillTargetMin + (CookingRules.GrillTargetMax - CookingRules.GrillTargetMin) * Clamp01(_random());
        }
        WindowOpen = open;
        GrillCursor = open ? CookingRules.PingPong(_grillClock - windowStart, CookingRules.GrillWindowTime) : 0f;
    }

    private void Resolve(float error)
    {
        _errors.Add(error);
        LastError = error;
        _roundResolved = true;
        WindowOpen = false;
    }

    private void EndRound()
    {
        if (!_roundResolved)
        {
            Misses++;
            LastError = -1f;
        }
        Round++;
        _roundResolved = false;
        _windowStarted = false;
        WindowOpen = false;
        if (Round >= CookingRules.GrillRounds)
        {
            Finish(CookingRules.GradeGrill(_errors, Misses));
        }
    }

    private void TickBoil(float dt, bool held)
    {
        float remaining = dt;
        while (remaining > 0f && Phase == CookPhase.Play)
        {
            float h = Math.Min(Step, remaining);
            remaining -= h;
            Elapsed += h;
            _driftTimer -= h;
            if (_driftTimer <= 0f)
            {
                _driftTimer += CookingRules.HeatDriftStep;
                _drift = (Clamp01(_random()) * 2f - 1f) * CookingRules.HeatDriftMax;
            }
            float rate = (held ? CookingRules.HeatUpRate : -CookingRules.HeatDownRate) + _drift;
            Heat = Math.Max(0f, Math.Min(100f, Heat + rate * h));
            if (Heat >= CookingRules.SafeMin && Heat <= CookingRules.SafeMax)
            {
                _inZone += h;
            }
            _overheat = Heat >= CookingRules.OverheatLevel ? _overheat + h : 0f;
            if (_overheat >= CookingRules.OverheatFail)
            {
                Overheated = true;
                Finish(CookGrade.Fail);
            }
            else if (Elapsed >= CookingRules.BoilTime)
            {
                Finish(CookingRules.GradeBoil(_inZone / CookingRules.BoilTime, false));
            }
        }
    }

    private void Finish(CookGrade grade)
    {
        Grade = grade;
        Phase = CookPhase.Result;
        WindowOpen = false;
        Elapsed = 0f;
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : (v > 0.999999f ? 0.999999f : v);
    }
}
