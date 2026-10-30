using System;
using System.Collections.Generic;

public enum DigResult { None, Success, OutOfPicks, Collapsed, TimedOut, Cancelled }

/// <summary>유물 한 개를 캐서 나온 보상(추첨 결과). Def.Id 가 ExcavationRules.RecordId 면 아버지의 기록</summary>
public readonly struct DigFind
{
    public readonly GatherDef Def;
    public readonly int Count;

    public DigFind(GatherDef def, int count)
    {
        Def = def;
        Count = count;
    }
}

/// <summary>
/// 발굴 한 판(순수 상태 머신). 커서 이동(Move)·발굴(Dig)·틱(Tick)만 받는다. 보상 지급은 ExcavationSite 몫.
/// 유물을 캐는 즉시 보상을 추첨해 Finds 에 쌓는다(실패·중단해도 이미 캔 유물은 유지).
/// </summary>
public sealed class ExcavationSession
{
    private readonly GatherDef[] _table;
    private readonly Func<float> _random;
    private readonly DigContent[] _cells;
    private readonly bool[] _dug;
    private readonly List<DigFind> _finds = new List<DigFind>();

    public DigSpec Spec { get; }
    public int Size => Spec.Size;
    public int Cursor { get; private set; }
    public int PicksLeft { get; private set; }
    public int Collapse { get; private set; }
    public int RelicsFound { get; private set; }
    public bool Done { get; private set; }
    public DigResult Result { get; private set; }
    /// <summary>시작 뒤 경과(결과가 나면 결과 뒤 경과)</summary>
    public float Elapsed { get; private set; }
    /// <summary>마지막으로 판 칸(-1 = 아직 없음)과 그 내용</summary>
    public int LastDug { get; private set; } = -1;
    public IReadOnlyList<DigFind> Finds => _finds;

    public ExcavationSession(DigSpec spec, GatherDef[] table, Func<float> random)
    {
        Spec = spec;
        _table = table;
        _random = random;
        _cells = ExcavationRules.Generate(spec, random);
        _dug = new bool[_cells.Length];
        PicksLeft = spec.Picks;
        Cursor = ExcavationRules.Center(spec.Size);
        _dug[Cursor] = true; // 가운데 칸은 공짜로 드러난다(곡괭이 소모 없음)
    }

    public bool IsDug(int index)
    {
        return _dug[index];
    }

    /// <summary>판 칸이거나 위험 칸이면 내용을 보여 준다(위험 칸은 파기 전에도 붉은 균열로 보인다)</summary>
    public DigContent ContentIfVisible(int index)
    {
        return _dug[index] || _cells[index] == DigContent.Hazard ? _cells[index] : DigContent.Empty;
    }

    /// <summary>판 칸의 상하좌우 유물 수(안 판 칸은 -1)</summary>
    public int HintAt(int index)
    {
        return _dug[index] ? ExcavationRules.Hint(_cells, Size, index) : -1;
    }

    /// <summary>테스트·프로브용: 실제 내용(숨김 무시)</summary>
    public DigContent ContentAt(int index)
    {
        return _cells[index];
    }

    public void Tick(float dt)
    {
        Elapsed += dt;
        if (!Done && Elapsed >= Spec.TimeLimit)
        {
            Finish(DigResult.TimedOut);
        }
    }

    /// <summary>커서 이동(판 밖으로 나가지 않음)</summary>
    public void Move(int dx, int dy)
    {
        if (Done)
        {
            return;
        }
        int x = Clamp(Cursor % Size + dx, 0, Size - 1);
        int y = Clamp(Cursor / Size + dy, 0, Size - 1);
        Cursor = y * Size + x;
    }

    /// <summary>커서 칸을 판다. 이미 판 칸이면 아무 일도 없다(곡괭이 소모 없음). 팠으면 true</summary>
    public bool Dig()
    {
        if (Done || _dug[Cursor] || PicksLeft <= 0)
        {
            return false;
        }
        _dug[Cursor] = true;
        LastDug = Cursor;
        PicksLeft--;
        switch (_cells[Cursor])
        {
            case DigContent.Relic:
                RelicsFound++;
                GatherDef def = _table[GatherRules.Roll(_table, _random(), 0f, 0f)];
                _finds.Add(new DigFind(def, GatherRules.RollCount(def, _random())));
                break;
            case DigContent.Hazard:
                Collapse += ExcavationRules.CollapsePerHazard;
                break;
        }
        if (RelicsFound >= Spec.Relics)
        {
            Finish(DigResult.Success);
        }
        else if (Collapse >= ExcavationRules.CollapseLimit)
        {
            Finish(DigResult.Collapsed);
        }
        else if (PicksLeft <= 0)
        {
            Finish(DigResult.OutOfPicks);
        }
        return true;
    }

    /// <summary>도중 중단(ESC·피격·사망·이탈). 결과가 났으면 무시</summary>
    public void Cancel()
    {
        if (!Done)
        {
            Finish(DigResult.Cancelled);
        }
    }

    private void Finish(DigResult result)
    {
        Result = result;
        Done = true;
        Elapsed = 0f;
    }

    private static int Clamp(int v, int min, int max)
    {
        return v < min ? min : (v > max ? max : v);
    }
}
