using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정예 둥지 추적 한 곳(맵당 하나, FloorCoreContent.md 1절 ②). 표식 4개(흔적 3 + 둥지)를 거느리고 순서·제한 시간·소환·쿨타임을 맡는다.
/// 첫 흔적을 살피면 50초 시계가 돌고, 흔적 3개를 차례로 따라가면 둥지 표식이 나타난다. 둥지를 살피면 이 맵·지금 시간대의
/// 일반 몹 하나가 정예로 확정 출현(EnemySpawner.SpawnTracked, 추가 보너스 없이 기존 정예 보상). 시간 초과·맵 이탈은 실패.
/// 쿨타임: 성공 10분·실패 3분(맵별, 세션 한정). 흔적 추적 정예와 합쳐 추적 정예는 한 번에 하나.
/// </summary>
public class NestHuntSite : MonoBehaviour
{
    private const float ResultShow = 3.5f;

    private readonly NestMark[] _marks = new NestMark[NestHuntRules.Marks];
    private readonly List<EnemySpawner> _candidates = new List<EnemySpawner>();
    private EnemySpawner[] _spawners;
    private int _cachedDay = -1;
    private float _readyAt;
    private float _resultUntil;

    /// <summary>HUD 가 읽는 가장 최근 사이트(진행 중이거나 결과 표시 중)</summary>
    public static NestHuntSite Latest { get; private set; }

    public NestHuntState State { get; private set; } = new NestHuntState();
    /// <summary>끝난 판의 결과 문구용: 출현한 정예 이름(실패면 null)</summary>
    public string SummonedName { get; private set; }
    public bool ShowResult => State.Finished && Time.time < _resultUntil;
    public bool Active => State.Running || ShowResult;
    /// <summary>쿨타임 남은 초(0이면 가능)</summary>
    public float CooldownLeft => Mathf.Max(0f, _readyAt - Time.time);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Latest = null;
    }

    /// <summary>표식이 Init 에서 자기를 등록한다</summary>
    public void Register(NestMark mark)
    {
        _marks[mark.Index] = mark;
    }

    public NestMark Mark(int index)
    {
        return _marks[index];
    }

    private void OnDisable()
    {
        if (State.Running)
        {
            State.Cancel();
            _readyAt = Time.time + NestHuntRules.FailCooldown;
        }
        State = new NestHuntState();
        SummonedName = null;
        if (Latest == this)
        {
            Latest = null;
        }
    }

    private void Update()
    {
        if (State.Running)
        {
            State.Tick(Time.deltaTime);
            if (State.Phase == NestPhase.TimedOut)
            {
                Fail();
            }
        }
        else if (State.Finished && Time.time >= _resultUntil)
        {
            State = new NestHuntState(); // 결과 표시가 끝나면 처음 상태로
            SummonedName = null;
        }
    }

    /// <summary>index 번 표식을 지금 살필 수 있는가. 0번은 쿨타임·추적 정예·후보 몹까지 본다</summary>
    public bool CanInvestigate(int index)
    {
        if (!State.CanInvestigate(index))
        {
            return false;
        }
        return index > 0 || (Time.time >= _readyAt && !EnemySpawner.HasTrackedTarget && RefreshCandidates() > 0);
    }

    /// <summary>표식을 그릴지: 시작 표식은 늘, 나머지는 차례가 됐을 때만</summary>
    public bool IsVisible(int index)
    {
        return index == 0 || (State.Running && State.Found == index);
    }

    /// <summary>표식이 살피기를 마쳤다고 알린다</summary>
    public void OnInvestigated(int index)
    {
        if (!State.Investigate(index))
        {
            return;
        }
        Latest = this;
        GameLog.Info(GameLog.Cat.Spawn, "NestHunt: mark " + index + " found=" + State.Found + " t=" + State.Elapsed.ToString("F1"));
        if (State.Phase == NestPhase.Success)
        {
            Summon();
        }
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }

    /// <summary>제한 시간을 다 쓴 것으로 만든다(프로브용)</summary>
    public void ForceTimeout()
    {
        State.Tick(NestHuntRules.TimeLimit);
        if (State.Phase == NestPhase.TimedOut)
        {
            Fail();
        }
    }

    private void Fail()
    {
        SummonedName = null;
        _readyAt = Time.time + NestHuntRules.FailCooldown;
        _resultUntil = Time.time + ResultShow;
        GameLog.Info(GameLog.Cat.Spawn, "NestHunt: 실패 " + State.Phase + " found=" + State.Found);
    }

    /// <summary>둥지에서 이 맵·지금 시간대의 일반 몹 하나를 정예로 확정 소환한다</summary>
    private void Summon()
    {
        _resultUntil = Time.time + ResultShow;
        _readyAt = Time.time + NestHuntRules.SuccessCooldown;
        SummonedName = null;
        if (RefreshCandidates() == 0)
        {
            return;
        }
        EnemySpawner spawner = _candidates[TrackingRules.Pick(_candidates.Count, Random.value)];
        Vector3 pos = _marks[NestHuntRules.Trails].Position;
        EnemyHealth e = spawner.SpawnTracked(pos, new string[0], 0f, NestHuntRules.EliteLifetime);
        if (e != null)
        {
            SummonedName = MonsterTable.DisplayName(spawner.EnemyPrefab.name);
        }
        GameLog.Info(GameLog.Cat.Spawn, "NestHunt: 둥지 소환 " + spawner.EnemyPrefab.name + " summoned=" + (e != null) + " t=" + State.Elapsed.ToString("F1"));
    }

    /// <summary>이 맵·지금 시간대의 소환 후보 스포너(시간대가 바뀔 때만 다시 계산). 흔적 추적과 같은 기준</summary>
    private int RefreshCandidates()
    {
        bool day = TimeChange.instance == null || TimeChange.instance.isDaytime;
        int key = day ? 1 : 0;
        if (key == _cachedDay)
        {
            return _candidates.Count;
        }
        if (DatabaseManager.instance == null)
        {
            return 0;
        }
        _cachedDay = key;
        _candidates.Clear();
        if (_spawners == null)
        {
            _spawners = transform.parent != null ? transform.parent.GetComponentsInChildren<EnemySpawner>(true) : new EnemySpawner[0];
        }
        foreach (EnemySpawner s in _spawners)
        {
            if (s.isDaytimeSpowner == day && TrackingSpot.IsHuntable(s))
            {
                _candidates.Add(s);
            }
        }
        return _candidates.Count;
    }
}
