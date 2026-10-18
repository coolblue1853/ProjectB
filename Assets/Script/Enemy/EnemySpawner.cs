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

    private bool isSpawning;

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
            // 영역 밖으로 나간 몬스터는 서서히 사라진다
            if (collision.TryGetComponent(out EnemyHealth enemyHealth) && enemyHealth.canDisapear)
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

            // 미리 생성(워밍업)
            var warm = new List<GameObject>(info.count);
            for (int i = 0; i < info.count; i++)
            {
                warm.Add(pool.Get());
            }
            foreach (GameObject go in warm)
            {
                pool.Release(go);
            }
        }

        isReady = true;
    }

    private GameObject CreatePooledItem(GameObject prefab, IObjectPool<GameObject> pool)
    {
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
    /// 맵을 떠날 때 호출. 활동 중인 몬스터·공격·시체를 모두 반환하고 풀을 파괴한다.
    /// 다시 활성화되면 <see cref="Init"/>에서 새로 만든다.
    /// </summary>
    public void ClearAllPools()
    {
        CancelInvoke(nameof(CycleCheck));
        isSpawning = false;

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

        foreach (ObjectPool<GameObject> pool in pools.Values)
        {
            pool.Clear(); // 비활성 오브젝트 파괴 (OnDestroyPoolObject)
        }
        pools.Clear();
        prefabs.Clear();
        isReady = false;
    }
    #endregion

    #region Spawn
    private bool IsMyTime()
    {
        return TimeChange.instance != null && TimeChange.instance.isDaytime == isDaytimeSpowner;
    }

    private void OnEnemyReleased(int num, int force, Vector3 pos, bool isDeadBody = true, bool isDrop = true)
    {
        if (enemySlot != null && num >= 0 && num < enemySlot.Length)
        {
            enemySlot[num] = null;
        }
        if (isDrop && dropManager != null)
        {
            dropManager.DropItems(pos);
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

            enemyHealth.enemyNum = i;
            enemyHealth.OnReleasedToPool -= OnEnemyReleased; // 중복 등록 방지
            enemyHealth.OnReleasedToPool += OnEnemyReleased;
            enemySlot[i] = enemyObject;
        }
    }

    /// <summary>플레이어가 떠났거나 시간대가 바뀌면 남은 몬스터를 서서히 사라지게 한다.</summary>
    private void DeCycleCheck()
    {
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
