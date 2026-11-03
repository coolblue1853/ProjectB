using UnityEngine;

/// <summary>
/// 구역 미니보스 한 마리의 소환·재리젠 담당 (Floor1Completion.md 4-4, MiniBossTable 한 행 = 사이트 하나).
/// 씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다(NestHuntPlacer 와 같은 방식). 맵이 켜져 있고 플레이어가 가까우면
/// 같은 구역 일반 몹 프리팹(baseMonster)을 스포너 풀 밖에서 직접 만들어 MonsterSheet 미니보스 행을 입히고 정예 표식(금빛, 1.5배)을 붙인다.
/// 풀 밖 개체라 반환하면 파괴된다(PoolAble: pool 없음). 처치하면 확정 드롭 + 확률 드롭(MonsterExtraDrops) + 골드, 재처치 감소는 보스와 같은 규칙.
/// 낮/밤: baseMonster 스포너의 시간대에만 존재한다(바뀌면 서서히 사라지고 다음 해당 시간대에 다시 나온다).
/// </summary>
public class MiniBossSite : MonoBehaviour
{
    private const float SpawnRange = 45f;    // 이 안에 플레이어가 있으면 소환
    private const float VanishRetry = 30f;   // 처치 못 하고 사라졌을 때 다시 나오기까지
    private const float CheckInterval = 0.5f;

    private MiniBossTable.Row _row;
    private EnemySpawner[] _spawners;
    private EnemySpawner _spawner;
    private EnemyHealth _enemy;
    private float _readyAt;
    private float _nextCheck;

    public bool Alive => _enemy != null && !_enemy.IsInPool;
    public EnemyHealth Enemy => _enemy;
    public MiniBossTable.Row Row => _row;

    public void Init(MiniBossTable.Row row)
    {
        _row = row;
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            EnemyHealth e = _enemy;
            e.OnReleasedToPool -= OnReleased;
            _enemy = null;
            if (!e.IsInPool)
            {
                e.ForceEnemyRelease(); // 맵을 떠남: 드롭 없이 정리(이 개체는 풀 밖이라 파괴된다)
            }
        }
    }

    private void Update()
    {
        if (_row == null || Time.time < _nextCheck)
        {
            return;
        }
        _nextCheck = Time.time + CheckInterval;
        if (Alive)
        {
            if (_spawner != null && !IsMyTime(_spawner))
            {
                _enemy.DisaperByTime();
            }
            return;
        }
        if (Time.time < _readyAt || !FindSpawner())
        {
            return;
        }
        PlayerController player = PlayerController.instance;
        if (player == null || Mathf.Abs(player.transform.position.x - _row.X) > SpawnRange)
        {
            return;
        }
        Spawn();
    }

    private static bool IsMyTime(EnemySpawner s)
    {
        return TimeChange.instance == null || TimeChange.instance.isDaytime == s.isDaytimeSpowner;
    }

    /// <summary>이 맵에서 baseMonster 프리팹을 쓰고 지금 시간대에 맞는 스포너를 찾는다</summary>
    private bool FindSpawner()
    {
        if (_spawners == null)
        {
            _spawners = transform.parent != null ? transform.parent.GetComponentsInChildren<EnemySpawner>(true) : new EnemySpawner[0];
        }
        _spawner = null;
        foreach (EnemySpawner s in _spawners)
        {
            GameObject prefab = s.EnemyPrefab;
            if (prefab != null && prefab.name == _row.BaseMonster && s.isReady && IsMyTime(s))
            {
                _spawner = s;
                return true;
            }
        }
        return false;
    }

    private void Spawn()
    {
        GameObject prefab = _spawner.EnemyPrefab;
        float z = _spawner.enemyPositionArray != null && _spawner.enemyPositionArray.Length > 0 && _spawner.enemyPositionArray[0] != null
            ? _spawner.enemyPositionArray[0].transform.position.z : _spawner.transform.position.z;
        var pos = new Vector3(_row.X, _row.Y + 0.5f, z);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        if (!go.TryGetComponent(out EnemyHealth e))
        {
            Destroy(go);
            return;
        }
        e.enemySpowner = _spawner;
        e.enemyNum = -1;
        e.spawnPosition = pos;
        e.hasSpawnPosition = true;
        e.ApplySpawnProfile(_row.Id);
        e.PromoteElite(true, EnemyHealth.MiniBossScaleMul);
        e.OnReleasedToPool += OnReleased;
        _enemy = e;
        GameLog.Info(GameLog.Cat.Spawn, "미니보스 소환 " + _row.Id + " HP " + e.maxHP + " @" + pos.ToString("F1"), this);
    }

    private void OnReleased(int num, int force, Vector3 pos, bool isDeadBody, bool isDrop)
    {
        if (_enemy != null)
        {
            _enemy.OnReleasedToPool -= OnReleased;
        }
        EnemySpawner sp = _spawner;
        if (isDrop && sp != null && sp.dropManager != null)
        {
            float mult = BossRetryRules.Multiplier(BossRetry.KillsToday(_row.Id));
            sp.dropManager.DropGuaranteed(MiniBossTable.BuildLoot(_row, mult), pos);
            sp.dropManager.DropGold(_row.Id, true, false, pos);
            if (mult >= 1f || Random.value < mult)
            {
                sp.dropManager.DropExtra(_row.Id, pos);
            }
            BossRetry.Record(_row.Id);
            if (mult < 1f)
            {
                PickupLog.Notice("같은 날 다시 쓰러뜨려 미니보스 보상이 " + BossRetryRules.Describe(mult) + "로 줄었다");
            }
            _readyAt = Time.time + _row.RespawnSec;
        }
        else
        {
            _readyAt = Time.time + VanishRetry;
        }
        if (isDeadBody && sp != null)
        {
            GameObject body = sp.GetOb("dead");
            if (body != null)
            {
                body.transform.position = pos;
                body.GetComponent<DeadBody>().Force2DeadBody(force);
            }
        }
        _enemy = null;
        GameLog.Info(GameLog.Cat.Spawn, "미니보스 반환 " + _row.Id + " drop=" + isDrop, this);
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }
}

/// <summary>MiniBosses.csv 의 행마다 해당 맵 오브젝트 아래에 사이트를 만든다 (씬 파일 수정 없음)</summary>
public static class MiniBossPlacer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MiniBossTable.Row row in MiniBossTable.All())
        {
            MapInfo map = null;
            foreach (MapInfo m in maps)
            {
                if (m.name == row.Map)
                {
                    map = m;
                    break;
                }
            }
            if (map == null)
            {
                Debug.LogWarning("[PB][MiniBoss] 맵 없음: " + row.Map);
                continue;
            }
            var go = new GameObject("MiniBossSite_" + row.Id);
            go.transform.SetParent(map.transform, false);
            go.AddComponent<MiniBossSite>().Init(row);
        }
    }
}
