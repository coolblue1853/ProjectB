using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 영역형 몬스터 스포너.
/// 플레이어가 영역 안에 있고 시간대(낮/밤)가 맞으면 <see cref="cycleFloat"/>초마다 빈 슬롯을 채운다.
/// 몬스터·공격·시체를 키별 ObjectPool로 관리하며, 맵을 떠날 때 <see cref="ClearAllPools"/>로 모두 정리한다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    private class ObjectInfo
    {
        public string objectName;   // 풀 키 ("enemy", "dead", 공격 이름 등)
        public GameObject perfab;   // 풀에서 관리할 프리팹
        public int count;           // 미리 생성할 개수
    }

    public int enemyCount;
    public float cycleFloat;
    public GameObject[] enemySlot;
    public GameObject enemyPositon;
    public GameObject[] enemyPositionArray;
    public DropManager dropManager;
    public bool isDaytimeSpowner;
    public bool isRandSpawn = true;
    [Tooltip("스폰 시 정예(중간보스)로 승격될 확률 (ProgressionDesign 1절). 보스 프리팹은 승격 안 함")]
    [Range(0f, 1f)] public float eliteChance = 0.04f;
    public bool isInPlayer = false;

    [SerializeField]
    private ObjectInfo[] objectInfos = null;

    public bool isReady { get; private set; }

    private const string EnemyKey = "enemy";
    private const string DeadBodyKey = "dead";

    private readonly Dictionary<string, ObjectPool<GameObject>> pools = new Dictionary<string, ObjectPool<GameObject>>();
    private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    // ObjectPool은 대여 중인 오브젝트 목록을 제공하지 않으므로 정리용으로 직접 추적한다.
    private readonly HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    // 슬롯 밖 보스 부하(SpawnMinion). 보스 처치·시간대 전환 시 DismissMinions 로 정리 (B-127)
    private readonly List<EnemyHealth> minions = new List<EnemyHealth>();

    private bool isSpawning;

    // 흔적 추적(MiniGames.md ⑥)으로 부른 정예: 슬롯 밖, 한 번에 하나(TrackedTarget). 처치 때 정예 확정 보상 + 추적 보너스
    private EnemyHealth tracked;
    private float trackedUntil;
    private readonly List<string> trackedBonus = new List<string>();

    /// <summary>지금 살아 있는 추적 정예(모든 스포너 통틀어 하나, 없으면 null)</summary>
    public static EnemyHealth TrackedTarget { get; private set; }

    /// <summary>살아 있는 추적 정예가 있는가(풀에 돌아간 참조는 없는 것으로 본다)</summary>
    public static bool HasTrackedTarget => TrackedTarget != null && !TrackedTarget.IsInPool;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        TrackedTarget = null;
    }

    #region Unity
    private void Awake()
    {
        dropManager = GetComponent<DropManager>();
    }

    private void OnEnable()
    {
        Init();
    }

    private void Start()
    {
        enemySlot = new GameObject[enemyCount];
        enemyPositionArray = new GameObject[enemyCount];
        for (int i = 0; i < enemyCount; i++)
        {
            enemyPositionArray[i] = enemyPositon.transform.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        if (tracked != null && Time.time >= trackedUntil)
        {
            tracked.DisaperByTime(); // 제한 시간 안에 못 잡으면 보상 없이 사라진다
            trackedUntil = float.MaxValue;
        }
        bool shouldSpawn = isInPlayer && IsMyTime();
        if (shouldSpawn && !isSpawning)
        {
            isSpawning = true;
            InvokeRepeating(nameof(CycleCheck), 1f, cycleFloat);
        }
        else if (!shouldSpawn && isSpawning)
        {
            isSpawning = false;
            CancelInvoke(nameof(CycleCheck));
            DeCycleCheck();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isInPlayer && collision.CompareTag("Player"))
        {
            isInPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInPlayer = false;
        }
        else if (collision.CompareTag("Enemy") && collision.gameObject.activeSelf)
        {
            // 이 스포너 영역 밖으로 나간 '자기' 몬스터만 서서히 사라진다.
            // 다른 스포너 영역과 겹친 곳에서 경계를 넘을 때 남의 몬스터(보스 포함)가 사라지던 문제 (B-117)
            if (collision.TryGetComponent(out EnemyHealth enemyHealth) && enemyHealth.canDisapear && enemyHealth.enemySpowner == this)
            {
                enemyHealth.DisaperByTime();
            }
        }
    }
    #endregion

    #region Pool
    private void Init()
    {
        if (isReady)
        {
            return;
        }

        foreach (ObjectInfo info in objectInfos)
        {
            if (prefabs.ContainsKey(info.objectName))
            {
                continue;
            }
            string key = info.objectName;
            GameObject prefab = info.perfab;
            ObjectPool<GameObject> pool = null;
            pool = new ObjectPool<GameObject>(
                () => CreatePooledItem(prefab, pool),
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: info.count,
                maxSize: Mathf.Max(info.count * 2, info.count + 4));
            prefabs.Add(key, prefab);
            pools.Add(key, pool);

            pendingWarmup.Add(new KeyValuePair<ObjectPool<GameObject>, int>(pool, info.count));
        }

        isReady = true;
        if (pendingWarmup.Count > 0 && isActiveAndEnabled)
        {
            StartCoroutine(WarmUp());
        }
    }

    const int WarmupPerFrame = 3;
    readonly List<KeyValuePair<ObjectPool<GameObject>, int>> pendingWarmup = new List<KeyValuePair<ObjectPool<GameObject>, int>>();

    /// <summary>
    /// 워밍업을 여러 프레임에 나눈다. 맵 진입 시 스포너 20여 개가 한 프레임에 수백 개를 Instantiate해서
    /// 1초 넘게 멈추던 문제(soak 테스트 측정) 완화. 첫 스폰은 1초 뒤라 그 전에 대부분 끝난다.
    /// </summary>
    System.Collections.IEnumerator WarmUp()
    {
        var temp = new List<GameObject>(8);
        while (pendingWarmup.Count > 0)
        {
            var entry = pendingWarmup[0];
            pendingWarmup.RemoveAt(0);
            int remaining = entry.Value - entry.Key.CountInactive - entry.Key.CountActive;
            while (remaining > 0)
            {
                int n = Mathf.Min(WarmupPerFrame, remaining);
                temp.Clear();
                for (int i = 0; i < n; i++)
                {
                    temp.Add(entry.Key.Get());
                }
                foreach (GameObject go in temp)
                {
                    go.GetComponent<PoolAble>().ReleaseObject(); // 반환은 ReleaseObject 한 곳 (B-141)
                }
                remaining -= n;
                yield return null;
            }
        }
    }

    private GameObject CreatePooledItem(GameObject prefab, IObjectPool<GameObject> pool)
    {
        if (isReady)
        {
            // 워밍업 이후의 생성 = 풀이 모자라 늘어남 (count 튜닝 신호)
            GameLog.Info(GameLog.Cat.Pool, name + ": 풀 확장 생성 " + prefab.name, this);
        }
        GameObject go = Instantiate(prefab);
        go.GetComponent<PoolAble>().pool = pool;
        return go;
    }

    private void OnTakeFromPool(GameObject go)
    {
        go.SetActive(true);
        activeObjects.Add(go);
    }

    private void OnReturnedToPool(GameObject go)
    {
        go.SetActive(false);
        activeObjects.Remove(go);
    }

    private void OnDestroyPoolObject(GameObject go)
    {
        activeObjects.Remove(go);
        Destroy(go);
    }

    /// <summary>키에 해당하는 풀에서 오브젝트를 꺼낸다. 등록되지 않은 키면 null.</summary>
    public GameObject GetOb(string goName)
    {
        if (!pools.TryGetValue(goName, out ObjectPool<GameObject> pool))
        {
            Debug.LogWarning($"{name}: '{goName}'은(는) 오브젝트풀에 등록되지 않은 오브젝트입니다.");
            return null;
        }
        return pool.Get();
    }

    /// <summary>
    /// 맵을 떠날 때 호출. 활동 중인 몬스터·공격·시체를 모두 풀로 반환한다.
    /// 풀 자체는 파괴하지 않는다 — 다시 들어올 때 재생성(수백 개 Instantiate)으로 생기던 1초대 프리즈 제거.
    /// </summary>
    public void ClearAllPools()
    {
        GameLog.Info(GameLog.Cat.Pool, name + ": 맵 이탈 정리 — 활성 " + activeObjects.Count + "개 반환 (풀 유지 " + pools.Count + "종)", this);
        CancelInvoke(nameof(CycleCheck));
        isSpawning = false;
        if (tracked != null)
        {
            tracked.ForceEnemyRelease(); // 이벤트로 추적 상태가 비워진다
        }

        if (enemySlot != null)
        {
            for (int i = 0; i < enemySlot.Length; i++)
            {
                if (enemySlot[i] != null && enemySlot[i].TryGetComponent(out EnemyHealth enemyHealth))
                {
                    enemyHealth.ForceEnemyRelease(); // 이벤트로 슬롯이 비워진다
                }
                enemySlot[i] = null;
            }
        }

        // 복사본을 돌면서 반환 (반환 시 activeObjects가 수정됨)
        foreach (GameObject go in new List<GameObject>(activeObjects))
        {
            if (go != null && go.TryGetComponent(out PoolAble poolAble))
            {
                poolAble.ReleaseObject();
            }
        }
        activeObjects.Clear();
    }

    void OnDestroy()
    {
        foreach (ObjectPool<GameObject> pool in pools.Values)
        {
            pool.Clear();
        }
    }
    #endregion

    #region Spawn
    private bool IsMyTime()
    {
        return TimeChange.instance != null && TimeChange.instance.isDaytime == isDaytimeSpowner;
    }

    /// <summary>정예 확정 보상: 강화석 1~2 + 이 스포너 드롭표의 장비 1개(없으면 강화석 1개 추가)</summary>
    List<string> EliteLoot()
    {
        var loot = new List<string> { EnhanceRules.StoneName };
        if (Random.value < 0.5f)
        {
            loot.Add(EnhanceRules.StoneName);
        }
        List<string> equips = dropManager.EquipmentNames();
        loot.Add(equips.Count > 0 ? equips[Random.Range(0, equips.Count)] : EnhanceRules.StoneName);
        return loot;
    }

    private void OnEnemyReleased(int num, int force, Vector3 pos, bool isDeadBody = true, bool isDrop = true)
    {
        // 슬롯을 비우기 전에 정예 여부 확인 (OnDisable 에서 Demote 되기 전에 이벤트가 먼저 온다)
        bool wasElite = enemySlot != null && num >= 0 && num < enemySlot.Length && enemySlot[num] != null
            && enemySlot[num].TryGetComponent(out EnemyHealth releasedEnemy) && releasedEnemy.IsElite;
        if (wasElite && isDrop && dropManager != null)
        {
            dropManager.DropGuaranteed(EliteLoot(), pos);
        }
        GameObject released = enemySlot != null && num >= 0 && num < enemySlot.Length ? enemySlot[num] : null;
        if (isDrop && dropManager != null && released != null && released.TryGetComponent(out EnemyHealth boss) && boss.isBossMob)
        {
            string bossName = released.name.Replace("(Clone)", "").Trim();
            var reward = Resources.Load<BossReward>("Boss/" + bossName);
            if (reward != null)
            {
                dropManager.DropGuaranteed(BossLoot.Roll(bossName, reward), pos);
            }
            else
            {
                GameLog.Warn(GameLog.Cat.Item, "보스 보상 정의 없음: Resources/Boss/" + bossName);
            }
        }
        if (enemySlot != null && num >= 0 && num < enemySlot.Length)
        {
            enemySlot[num] = null;
        }
        else
        {
            GameLog.Anomaly(GameLog.Cat.Spawn, name + ": 잘못된 슬롯 번호 반환 " + num, this);
        }
        GameLog.Info(GameLog.Cat.Spawn, name + ": 슬롯 " + num + " 비움 (drop=" + isDrop + ", body=" + isDeadBody + ")", this);
        if (isDrop && dropManager != null)
        {
            dropManager.DropItems(pos);
            if (released != null)
            {
                bool isBoss = released.TryGetComponent(out EnemyHealth bossHp) && bossHp.isBossMob;
                dropManager.DropGold(ExtraDrops.MonsterId(released.name), wasElite, isBoss, pos);
                dropManager.DropExtra(ExtraDrops.MonsterId(released.name), pos); // 계보 무기 추가 드롭(MonsterExtraDrops.csv)
            }
        }
        if (isDeadBody)
        {
            GameObject deadbody = GetOb(DeadBodyKey);
            if (deadbody != null)
            {
                deadbody.transform.position = pos;
                deadbody.GetComponent<DeadBody>().Force2DeadBody(force);
            }
        }
    }

    private void CycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // 비어 있거나, 다른 경로로 반환되어 비활성화된 슬롯은 다시 채운다
            if (enemySlot[i] != null && enemySlot[i].activeSelf)
            {
                continue;
            }

            GameObject enemyObject = GetOb(EnemyKey);
            if (enemyObject == null)
            {
                return;
            }

            EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
            enemyHealth.enemySpowner = this;

            int posIndex = isRandSpawn ? Random.Range(0, enemyCount) : i;
            var jitter = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            enemyObject.transform.position = enemyPositionArray[posIndex].transform.position + jitter;
            enemyHealth.spawnPosition = enemyObject.transform.position;
            enemyHealth.hasSpawnPosition = true;

            enemyHealth.enemyNum = i;
            enemyHealth.OnReleasedToPool -= OnEnemyReleased; // 중복 등록 방지
            enemyHealth.OnReleasedToPool -= OnMinionReleased; // 부하로 쓰였던 오브젝트면 부하 경로 해제
            enemyHealth.OnReleasedToPool -= OnTrackedReleased;
            enemyHealth.OnReleasedToPool += OnEnemyReleased;
            enemySlot[i] = enemyObject;
            if (Random.value < eliteChance)
            {
                enemyHealth.PromoteElite();
            }
            GameLog.Info(GameLog.Cat.Spawn, name + ": 슬롯 " + i + " 스폰 " + enemyObject.name + " @" + enemyObject.transform.position.ToString("F1"), this);
        }
    }

    /// <summary>
    /// 보스 부하 소환 (BossMinionCall): 슬롯 밖에서 풀 key 몬스터를 꺼내 pos 에 둔다.
    /// 슬롯 번호는 -1, 드롭 없음, 죽으면 "minionDead" 풀 시체. 몬스터 BT 가 이 스포너 풀에서 공격을 꺼내므로 부하의 공격 키도 이 스포너에 있어야 한다.
    /// </summary>
    public GameObject SpawnMinion(string key, Vector3 pos)
    {
        if (!pools.ContainsKey(key))
        {
            GameLog.Warn(GameLog.Cat.Spawn, name + ": 부하 풀 없음 " + key);
            return null;
        }
        GameObject go = GetOb(key);
        if (go == null || !go.TryGetComponent(out EnemyHealth enemyHealth))
        {
            return go;
        }
        enemyHealth.enemySpowner = this;
        enemyHealth.enemyNum = -1;
        go.transform.position = pos;
        enemyHealth.spawnPosition = pos;
        enemyHealth.hasSpawnPosition = true;
        enemyHealth.OnReleasedToPool -= OnEnemyReleased;
        enemyHealth.OnReleasedToPool -= OnMinionReleased;
        enemyHealth.OnReleasedToPool -= OnTrackedReleased;
        enemyHealth.OnReleasedToPool += OnMinionReleased;
        if (!minions.Contains(enemyHealth))
        {
            minions.Add(enemyHealth);
        }
        GameLog.Info(GameLog.Cat.Spawn, name + ": 부하 소환 " + go.name + " @" + pos.ToString("F1"), this);
        return go;
    }

    private void OnMinionReleased(int num, int force, Vector3 pos, bool isDeadBody, bool isDrop)
    {
        minions.RemoveAll(m => m == null || m.IsInPool);
        if (!isDeadBody || !pools.ContainsKey(MinionDeadKey))
        {
            return;
        }
        GameObject deadbody = GetOb(MinionDeadKey);
        if (deadbody != null && deadbody.TryGetComponent(out DeadBody body))
        {
            deadbody.transform.position = pos;
            body.Force2DeadBody(force);
        }
    }

    /// <summary>
    /// 살아 있는 보스 부하를 모두 서서히 사라지게 한다(드롭 없음). 보스 처치(BossMinionCall)·시간대 전환(DeCycleCheck)에서 부른다 (B-127).
    /// </summary>
    public void DismissMinions()
    {
        for (int i = minions.Count - 1; i >= 0; i--)
        {
            EnemyHealth m = minions[i];
            if (m == null || m.IsInPool)
            {
                minions.RemoveAt(i);
                continue;
            }
            m.DisaperByTime();
        }
    }

    /// <summary>이 스포너의 일반 몹 프리팹(풀 키 "enemy", 없으면 null). 흔적 추적 대상 고르기용</summary>
    public GameObject EnemyPrefab
    {
        get
        {
            if (objectInfos == null)
            {
                return null;
            }
            foreach (ObjectInfo info in objectInfos)
            {
                if (info.objectName == EnemyKey)
                {
                    return info.perfab;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 흔적 추적 정예 소환(MiniGames.md ⑥): 슬롯 밖에서 일반 몹을 꺼내 pos 에 두고 정예로 승격, stunSeconds 동안 덫에 묶는다.
    /// lifetime 초 안에 못 잡으면 사라진다(보상 없음). 처치하면 정예 확정 보상 + bonusLoot(계보 재료 등)를 떨어뜨린다.
    /// 이미 추적 정예가 있거나 꺼낼 수 없으면 null.
    /// </summary>
    public EnemyHealth SpawnTracked(Vector3 pos, IEnumerable<string> bonusLoot, float stunSeconds, float lifetime)
    {
        if (HasTrackedTarget || !pools.ContainsKey(EnemyKey))
        {
            return null;
        }
        GameObject go = GetOb(EnemyKey);
        if (go == null || !go.TryGetComponent(out EnemyHealth enemyHealth))
        {
            return null;
        }
        enemyHealth.enemySpowner = this;
        enemyHealth.enemyNum = -1;
        go.transform.position = pos;
        enemyHealth.spawnPosition = pos;
        enemyHealth.hasSpawnPosition = true;
        enemyHealth.OnReleasedToPool -= OnEnemyReleased;
        enemyHealth.OnReleasedToPool -= OnMinionReleased;
        enemyHealth.OnReleasedToPool -= OnTrackedReleased;
        enemyHealth.OnReleasedToPool += OnTrackedReleased;
        enemyHealth.PromoteElite();
        enemyHealth.Snare(stunSeconds);
        tracked = enemyHealth;
        TrackedTarget = enemyHealth;
        trackedUntil = Time.time + lifetime;
        trackedBonus.Clear();
        trackedBonus.AddRange(bonusLoot);
        GameLog.Info(GameLog.Cat.Spawn, name + ": 추적 정예 소환 " + go.name + " @" + pos.ToString("F1") + " 보너스 " + string.Join(",", trackedBonus), this);
        return enemyHealth;
    }

    private void OnTrackedReleased(int num, int force, Vector3 pos, bool isDeadBody, bool isDrop)
    {
        if (tracked != null)
        {
            tracked.OnReleasedToPool -= OnTrackedReleased;
        }
        if (isDrop && dropManager != null)
        {
            List<string> loot = EliteLoot();
            loot.AddRange(trackedBonus);
            dropManager.DropGuaranteed(loot, pos);
            dropManager.DropItems(pos);
            if (tracked != null)
            {
                dropManager.DropGold(ExtraDrops.MonsterId(tracked.name), true, false, pos);
                dropManager.DropExtra(ExtraDrops.MonsterId(tracked.name), pos);
            }
        }
        if (isDeadBody)
        {
            GameObject deadbody = GetOb(DeadBodyKey);
            if (deadbody != null)
            {
                deadbody.transform.position = pos;
                deadbody.GetComponent<DeadBody>().Force2DeadBody(force);
            }
        }
        GameLog.Info(GameLog.Cat.Spawn, name + ": 추적 정예 반환 (drop=" + isDrop + ")", this);
        if (TrackedTarget == tracked)
        {
            TrackedTarget = null;
        }
        tracked = null;
        trackedBonus.Clear();
    }

    /// <summary>보스 부하 풀 키 (BossMinionCall 기본값, build_bandit_fortress.cs 가 같은 이름으로 등록). 부하 BT 의 공격 키(예: "stab")도 같은 스포너에 있어야 한다</summary>
    public const string MinionKey = "minion";
    public const string MinionDeadKey = "minionDead";

    /// <summary>플레이어가 떠났거나 시간대가 바뀌면 남은 몬스터를 서서히 사라지게 한다.</summary>
    private void DeCycleCheck()
    {
        DismissMinions();
        if (tracked != null)
        {
            tracked.DisaperByTime();
        }
        if (enemySlot == null)
        {
            return;
        }
        foreach (GameObject slot in enemySlot)
        {
            if (slot != null && slot.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.DisaperByTime();
            }
        }
    }
    #endregion
}
