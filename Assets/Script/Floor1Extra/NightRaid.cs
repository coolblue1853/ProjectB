using System.Collections.Generic;
using UnityEngine;

/// <summary>밤 습격 한 구역 (Resources/Floor1Raids.csv: map,minLevel,waves,perWave,gold,exp,item,itemCount)</summary>
public sealed class NightRaidDef
{
    public string Map;
    public int MinLevel;
    public int Waves;
    public int PerWave;
    public Floor1Data.Reward Reward;

    public int TotalMobs => Waves * PerWave;
}

/// <summary>밤 습격 순수 규칙 (테스트 대상)</summary>
public static class NightRaidRules
{
    /// <summary>웨이브 하나당 제한 시간(초). 넘으면 습격을 놓친다</summary>
    public const float SecondsPerWave = 40f;
    public const float WaveGap = 2.5f;
    public const float SpawnInterval = 0.7f;
    /// <summary>몹이 나타나는 거리(플레이어 양옆, 유닛)</summary>
    public const float SpawnNear = 11f;
    public const float SpawnFar = 17f;

    public static List<NightRaidDef> Parse(string text)
    {
        var list = new List<NightRaidDef>();
        foreach (string[] c in Floor1Data.Rows(text))
        {
            if (c.Length < 8 || c[0].Trim().Length == 0)
            {
                continue;
            }
            list.Add(new NightRaidDef
            {
                Map = c[0].Trim(),
                MinLevel = Floor1Data.Int(c[1]),
                Waves = Mathf.Max(1, Floor1Data.Int(c[2])),
                PerWave = Mathf.Max(1, Floor1Data.Int(c[3])),
                Reward = new Floor1Data.Reward
                {
                    Gold = Floor1Data.Int(c[4]),
                    Exp = Floor1Data.Int(c[5]),
                    Item = c[6].Trim(),
                    Count = Floor1Data.Int(c[7]),
                    Label = "Raid",
                },
            });
        }
        return list;
    }

    /// <summary>그날 밤의 습격 구역: 레벨 조건을 채운 곳 중 밤 번호 시드로 하나 (같은 밤·같은 조건이면 항상 같다). 없으면 null</summary>
    public static NightRaidDef Pick(IReadOnlyList<NightRaidDef> defs, int night, int level)
    {
        var pool = new List<NightRaidDef>();
        foreach (NightRaidDef d in defs)
        {
            if (d.MinLevel <= level)
            {
                pool.Add(d);
            }
        }
        if (pool.Count == 0)
        {
            return null;
        }
        var rng = new System.Random(night * 104729 + 7);
        return pool[rng.Next(pool.Count)];
    }

    public static float Timeout(NightRaidDef d)
    {
        return d.Waves * SecondsPerWave;
    }
}

/// <summary>
/// 밤 습격 이벤트(FloorCoreContent·사용자 11-02 "콘텐츠 임의 추가"): 밤이 되면 구역 하나가 정해지고(알림), 그 구역에 있는 동안 짧은 몹 웨이브가 몰려온다.
/// 웨이브(2~3)마다 그 구역 밤 몹이 플레이어 양옆에서 나타나고, 전부 쓰러뜨리면 소량 보상(골드·EXP·재료). 구역을 떠나거나 시간이 지나면 놓친다.
/// 한 밤에 한 번. 세이브 없음(밤 번호는 실행 중 세션 기준). 끄기: Floor1Content.csv 의 raid, 수치: Floor1Raids.csv.
/// </summary>
public class NightRaid : MonoBehaviour
{
    private enum Phase { Idle, Pending, Active, Done }

    private static NightRaid s_instance;

    private List<NightRaidDef> _defs = new List<NightRaidDef>();
    private Phase _phase = Phase.Idle;
    private NightRaidDef _today;
    private int _night;
    private string _map = "";
    private bool _init;
    private bool _wasDay = true;
    private int _wave;
    private int _spawned;
    private int _alive;
    private int _kills;
    private Cooldown _nextSpawn;
    private float _waveStartAt;
    private Cooldown _deadline;
    private readonly List<EnemyHealth> _raiders = new List<EnemyHealth>();
    private EnemySpawner[] _spawners;
    private int _spawnerCursor;
    private float _failedSpawnSince = -1f;
    private Cooldown _nextBegin;
    private int _epoch; // 취소할 때마다 올려, 이전 습격 몹의 늦은 콜백이 새 습격 집계를 건드리지 않게 한다
    private int _groundMask = -1;

    public static NightRaid Instance => s_instance;
    public bool IsActive => _phase == Phase.Active;
    public string TodayMap => _today != null ? _today.Map : "";
    public int Night => _night;
    public int Kills => _kills;
    public int Wave => _wave;
    public IReadOnlyList<EnemyHealth> Raiders => _raiders;
    public int Alive => _alive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (s_instance != null || !Floor1Data.On("raid"))
        {
            return;
        }
        TextAsset csv = Resources.Load<TextAsset>("Floor1Raids");
        if (csv == null)
        {
            return;
        }
        var go = new GameObject("[NightRaid]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<NightRaid>();
        s_instance._defs = NightRaidRules.Parse(csv.text);
    }

    private void OnEnable()
    {
        Portal.MapChanged += OnMapChanged;
    }

    private void OnDisable()
    {
        Portal.MapChanged -= OnMapChanged;
    }

    private void OnMapChanged(GameObject map)
    {
        _map = map != null ? map.name : "";
        if (_phase == Phase.Active && _map != _today.Map)
        {
            Fail("습격 구역을 떠나 습격을 놓쳤다");
        }
    }

    private void Update()
    {
        TimeChange tc = TimeChange.instance;
        if (tc == null)
        {
            return;
        }
        if (!_init)
        {
            _init = true;
            _wasDay = tc.isDaytime;
            _map = CurrentMapName();
            if (!tc.isDaytime)
            {
                OnNightStart(); // 밤에 시작했으면 오늘 밤 구역을 바로 정한다
            }
        }
        if (_wasDay && !tc.isDaytime)
        {
            OnNightStart();
        }
        else if (!_wasDay && tc.isDaytime)
        {
            OnDayStart();
        }
        _wasDay = tc.isDaytime;
        if (_phase == Phase.Pending && _map == _today.Map && PlayerController.instance != null)
        {
            if (_nextBegin.Ready(Time.time))
            {
                _nextBegin.Arm(Time.time, 0.5f); // 밤 스포너가 준비될 때까지 0.5초마다만 찾는다
                Begin();
            }
        }
        else if (_phase == Phase.Active)
        {
            Tick();
        }
    }

    private static string CurrentMapName()
    {
        GameObject root = GameObject.Find("Map");
        PlayerController player = PlayerController.instance;
        if (root == null || player == null)
        {
            return "";
        }
        string best = "";
        float bestDy = float.MaxValue;
        foreach (Transform m in root.transform)
        {
            float dy = Mathf.Abs(m.position.y - player.transform.position.y);
            if (m.gameObject.activeInHierarchy && m.GetComponent<MapInfo>() != null && dy < bestDy)
            {
                bestDy = dy;
                best = m.name;
            }
        }
        return best;
    }

    private void OnNightStart()
    {
        _night++;
        Cancel();
        _today = NightRaidRules.Pick(_defs, _night, LevelSystem.Level);
        _phase = _today != null ? Phase.Pending : Phase.Idle;
        if (_today != null)
        {
            PickupLog.Notice("밤이 깊었다. " + ZoneName(_today.Map) + " 쪽에서 몬스터 무리가 움직이는 기척이 난다");
        }
    }

    private void OnDayStart()
    {
        if (_phase == Phase.Active)
        {
            Fail("날이 밝아 습격이 물러갔다");
        }
        _phase = Phase.Idle;
        _today = null;
    }

    private static string ZoneName(string map)
    {
        GameObject root = GameObject.Find("Map");
        Transform t = root != null ? root.transform.Find(map) : null;
        MapInfo info = t != null ? t.GetComponent<MapInfo>() : null;
        return info != null && !string.IsNullOrEmpty(info.displayName) ? Loc.T(info.displayName) : map;
    }

    /// <summary>검증용: 지금 있는 구역에서 바로 습격 시작(그 구역 정의가 있어야 한다). 시작했으면 true</summary>
    public bool DebugBegin(string map)
    {
        NightRaidDef d = _defs.Find(x => x.Map == map);
        if (d == null)
        {
            return false;
        }
        Cancel();
        _today = d;
        _map = map;
        Begin();
        return _phase == Phase.Active;
    }

    private void Begin()
    {
        EnemySpawner[] spawners = FindRaidSpawners();
        if (spawners.Length == 0)
        {
            return; // 밤 스포너가 아직 준비 중이다: 다음 프레임에 다시 시도
        }
        _spawners = spawners;
        _phase = Phase.Active;
        _wave = 0;
        _kills = 0;
        _alive = 0;
        _raiders.Clear();
        _failedSpawnSince = -1f;
        _deadline.Arm(Time.time, NightRaidRules.Timeout(_today));
        PickupLog.Notice("몬스터 무리의 습격! 전부 쓰러뜨려라 (" + _today.Waves + "차례)");
        StartWave();
    }

    private EnemySpawner[] FindRaidSpawners()
    {
        GameObject root = GameObject.Find("Map");
        Transform map = root != null ? root.transform.Find(_today.Map) : null;
        var list = new List<EnemySpawner>();
        if (map == null)
        {
            return list.ToArray();
        }
        foreach (EnemySpawner s in map.GetComponentsInChildren<EnemySpawner>(false))
        {
            GameObject prefab = s.isReady && !s.isDaytimeSpowner ? s.EnemyPrefab : null;
            if (prefab != null && prefab.TryGetComponent(out EnemyHealth eh) && !eh.isBossMob)
            {
                list.Add(s);
            }
        }
        return list.ToArray();
    }

    private void StartWave()
    {
        _wave++;
        _spawned = 0;
        _waveStartAt = Time.time;
        _nextSpawn.Arm(Time.time, 1f);
        if (_wave > 1)
        {
            PickupLog.Notice("습격 " + _wave + "/" + _today.Waves + "차례");
        }
    }

    private void Tick()
    {
        if (_deadline.Ready(Time.time))
        {
            Fail("시간이 지나 습격을 놓쳤다");
            return;
        }
        if (_spawned < _today.PerWave)
        {
            if (_nextSpawn.Ready(Time.time))
            {
                _nextSpawn.Arm(Time.time, NightRaidRules.SpawnInterval);
                TrySpawn();
            }
            return;
        }
        if (_alive > 0 || Time.time < _waveStartAt + NightRaidRules.WaveGap)
        {
            return;
        }
        if (_wave >= _today.Waves)
        {
            Complete();
        }
        else
        {
            StartWave();
        }
    }

    private void TrySpawn()
    {
        PlayerController player = PlayerController.instance;
        if (player == null || _spawners == null || _spawners.Length == 0)
        {
            return;
        }
        Vector3 at;
        if (!FindSpawnPoint(player.transform.position, _spawned % 2 == 0 ? 1f : -1f, out at))
        {
            if (_failedSpawnSince < 0f)
            {
                _failedSpawnSince = Time.time;
            }
            else if (Time.time - _failedSpawnSince > 8f)
            {
                _spawned++; // 자리를 못 찾는 구간: 마리 수를 채운 것으로 쳐서 습격이 멈추지 않게
                _failedSpawnSince = Time.time;
            }
            return;
        }
        _failedSpawnSince = -1f;
        EnemySpawner spawner = _spawners[_spawnerCursor++ % _spawners.Length];
        int epoch = _epoch;
        EnemyHealth e = spawner.SpawnRaider(at, (who, killed) => OnRaiderDown(epoch, who, killed));
        if (e == null)
        {
            return;
        }
        _spawned++;
        _alive++;
        _raiders.Add(e);
    }

    /// <summary>플레이어 side 쪽으로 SpawnNear~SpawnFar 떨어진 지면 위 한 점. 지면이 없거나 너무 멀면 false</summary>
    private bool FindSpawnPoint(Vector3 from, float side, out Vector3 point)
    {
        if (_groundMask < 0)
        {
            _groundMask = LayerMask.GetMask("Ground");
        }
        point = default(Vector3);
        float x = from.x + side * Random.Range(NightRaidRules.SpawnNear, NightRaidRules.SpawnFar);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, from.y + 6f), Vector2.down, 14f, _groundMask);
        if (hit.collider == null)
        {
            return false;
        }
        point = new Vector3(x, hit.point.y + 1f, from.z);
        return true;
    }

    private void OnRaiderDown(int epoch, EnemyHealth who, bool killed)
    {
        if (epoch != _epoch)
        {
            return;
        }
        _raiders.Remove(who);
        _alive = Mathf.Max(0, _alive - 1);
        if (killed)
        {
            _kills++;
        }
    }

    private void Complete()
    {
        _phase = Phase.Done;
        _raiders.Clear();
        Floor1Data.Reward r = _today.Reward;
        if (!r.CanReceive())
        {
            r = new Floor1Data.Reward { Gold = r.Gold, Exp = r.Exp, Label = r.Label };
            PickupLog.Warn("가방 공간이 부족해 습격 재료 보상은 받지 못했다");
        }
        r.Apply();
        PickupLog.Notice("습격 격퇴! " + r.Describe());
    }

    private void Fail(string message)
    {
        PickupLog.Warn(message);
        Cancel();
        _phase = Phase.Done;
    }

    /// <summary>남은 습격 몹을 서서히 사라지게 하고 진행을 비운다</summary>
    private void Cancel()
    {
        _epoch++;
        foreach (EnemyHealth e in new List<EnemyHealth>(_raiders))
        {
            if (e != null && !e.IsInPool)
            {
                e.DisaperByTime();
            }
        }
        _raiders.Clear();
        _alive = 0;
        _spawned = 0;
    }
}
