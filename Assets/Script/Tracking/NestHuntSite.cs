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

    // R5 소형 던전: 둥지를 찾으면 웨이브 → 정예 → 상자
    private readonly List<EnemyHealth> _raiders = new List<EnemyHealth>();
    private EnemyHealth _elite;
    private System.Action<int, int, Vector3, bool, bool> _eliteHandler;
    private int _wave;
    private int _spawned;
    private int _alive;
    private int _epoch; // 중단할 때마다 올려, 이전 웨이브 몹의 늦은 콜백이 새 집계를 건드리지 않게 한다
    private float _waveStart;
    private float _nextSpawn;
    private float _deadline;

    /// <summary>웨이브·정예 단계(없으면 None). HUD 가 읽는다</summary>
    public NestStage Stage { get; private set; }
    public int Wave => _wave;
    public int WaveAlive => _alive;

    /// <summary>HUD 가 읽는 가장 최근 사이트(진행 중이거나 결과 표시 중)</summary>
    public static NestHuntSite Latest { get; private set; }

    public NestHuntState State { get; private set; } = new NestHuntState();
    /// <summary>끝난 판의 결과 문구용: 출현한 정예 이름(실패면 null)</summary>
    public string SummonedName { get; private set; }
    /// <summary>둥지 정예 처치 확정 추가 드롭(구역 인장). NestHuntPlacer 가 정한다</summary>
    public string[] BonusLoot { get; set; } = new string[0];
    /// <summary>이 둥지가 있는 맵 이름(NestHuntRules.ChestTable 키). NestHuntPlacer 가 정한다</summary>
    public string Zone { get; set; }
    public bool ShowResult => State.Finished && Time.time < _resultUntil;
    public bool Active => State.Running || ShowResult || Stage != NestStage.None;
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
        EndDungeon(true);
        State = new NestHuntState();
        SummonedName = null;
        if (Latest == this)
        {
            Latest = null;
        }
    }

    private void Update()
    {
        if (Stage == NestStage.Wave)
        {
            TickWave();
        }
        if (State.Running)
        {
            State.Tick(Time.deltaTime);
            if (State.Phase == NestPhase.TimedOut)
            {
                Fail();
            }
        }
        else if (State.Finished && Stage != NestStage.Wave && Time.time >= _resultUntil)
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
            BeginWaves();
        }
    }

    /// <summary>둥지에 닿으면 정예 전에 웨이브 몹이 먼저 몰려온다. 소환할 몹이 없으면 곧바로 정예(기존 동작)</summary>
    private void BeginWaves()
    {
        _readyAt = Time.time + NestHuntRules.SuccessCooldown;
        if (RefreshCandidates() == 0)
        {
            Summon();
            return;
        }
        Stage = NestStage.Wave;
        _wave = 0;
        StartWave();
        PickupLog.Notice("둥지에서 몬스터가 몰려온다! " + NestHuntRules.Waves + "차례를 막아내라");
    }

    private void StartWave()
    {
        _wave++;
        _spawned = 0;
        _waveStart = Time.time;
        _nextSpawn = Time.time + 1f;
        _deadline = Time.time + NestHuntRules.WaveTimeLimit;
        if (_wave > 1)
        {
            PickupLog.Notice("둥지 습격 " + _wave + "/" + NestHuntRules.Waves + "차례");
        }
    }

    private void TickWave()
    {
        if (Time.time >= _deadline)
        {
            PickupLog.Warn("시간이 지나 둥지를 놓쳤다");
            _readyAt = Time.time + NestHuntRules.FailCooldown;
            SummonedName = null;
            _resultUntil = Time.time + ResultShow;
            EndDungeon(true);
            return;
        }
        switch (NestHuntRules.Step(_wave, _spawned, _alive, Time.time - _waveStart))
        {
            case NestWaveStep.Spawn:
                if (Time.time >= _nextSpawn)
                {
                    _nextSpawn = Time.time + 0.7f;
                    SpawnWaveMob();
                }
                break;
            case NestWaveStep.NextWave:
                StartWave();
                break;
            case NestWaveStep.Elite:
                Stage = NestStage.None;
                Summon();
                break;
        }
    }

    private void SpawnWaveMob()
    {
        if (RefreshCandidates() == 0)
        {
            _spawned++;
            return;
        }
        EnemySpawner spawner = _candidates[TrackingRules.Pick(_candidates.Count, Random.value)];
        float side = _spawned % 2 == 0 ? 1f : -1f;
        Vector3 pos = _marks[NestHuntRules.Trails].Position + new Vector3(side * Random.Range(NestHuntRules.WaveNear, NestHuntRules.WaveFar), 1f, 0f);
        int epoch = _epoch;
        EnemyHealth e = spawner.SpawnRaider(pos, (who, killed) => OnWaveMobDown(epoch, who));
        _spawned++;
        if (e != null)
        {
            _alive++;
            _raiders.Add(e);
        }
    }

    private void OnWaveMobDown(int epoch, EnemyHealth who)
    {
        if (epoch != _epoch)
        {
            return;
        }
        _raiders.Remove(who);
        _alive = Mathf.Max(0, _alive - 1);
    }

    /// <summary>웨이브·정예 단계를 끝낸다. clear 면 남은 웨이브 몹을 서서히 사라지게 하고 정예 감시도 푼다</summary>
    private void EndDungeon(bool clear)
    {
        _epoch++;
        if (clear)
        {
            foreach (EnemyHealth e in new List<EnemyHealth>(_raiders))
            {
                if (e != null && !e.IsInPool)
                {
                    e.DisaperByTime();
                }
            }
            ReleaseEliteWatch();
        }
        _raiders.Clear();
        _alive = 0;
        _spawned = 0;
        Stage = NestStage.None;
    }

    private void ReleaseEliteWatch()
    {
        if (_elite != null && _eliteHandler != null)
        {
            _elite.OnReleasedToPool -= _eliteHandler;
        }
        _elite = null;
        _eliteHandler = null;
    }

    /// <summary>정예가 쓰러지면 상자 보상(골드·EXP·강화석). 시간이 다 돼 사라진 경우는 보상 없음</summary>
    private void OnEliteReleased(int num, int force, Vector3 at, bool deadBody, bool killed)
    {
        ReleaseEliteWatch();
        Stage = NestStage.None;
        if (!killed)
        {
            return;
        }
        int gold, exp, stones = 1;
        if (NestHuntRules.TryChest(Zone, out NestHuntRules.ChestRow row))
        {
            gold = row.Gold;
            exp = row.Exp;
            stones = row.Stones;
        }
        else
        {
            NestHuntRules.ChestReward(LevelSystem.Level, out gold, out exp);
        }
        var r = new Floor1Data.Reward { Gold = gold, Exp = exp, Item = "Enhance Stone", Count = stones, Label = "NestChest" };
        if (!r.CanReceive())
        {
            r = new Floor1Data.Reward { Gold = gold, Exp = exp, Label = r.Label };
            PickupLog.Warn("가방 공간이 부족해 상자의 강화석은 받지 못했다");
        }
        r.Apply();
        PickupLog.Notice("둥지 상자! " + r.Describe());
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
        EnemyHealth e = spawner.SpawnTracked(pos, BonusLoot, 0f, NestHuntRules.EliteLifetime);
        if (e != null)
        {
            SummonedName = MonsterTable.DisplayName(spawner.EnemyPrefab.name);
            ReleaseEliteWatch();
            _elite = e;
            _eliteHandler = OnEliteReleased;
            e.OnReleasedToPool += _eliteHandler;
            Stage = NestStage.Elite;
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
