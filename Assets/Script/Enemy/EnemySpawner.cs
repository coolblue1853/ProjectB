using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class EnemySpawner : MonoBehaviour
{

    [System.Serializable]
    private class ObjectInfo
    {
        // ������Ʈ �̸�
        public string objectName;
        // ������Ʈ Ǯ���� ������ ������Ʈ
        public GameObject perfab;
        // ��� �̸� ���� �س�������
        public int count;
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
    List<GameObject> nowActiveList = new List<GameObject>();
    private void OnTriggerStay2D(Collider2D collision)
    {
     if(collision.tag == "Player"&& isInPlayer == false)
        {
            isInPlayer = true;
        }   
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" )
        {
            isInPlayer = false;
        }
        if (collision.tag == "Enemy")
        {
            if(collision.gameObject.activeSelf != false)
            {
                EnemyHealth enemyHealth = collision.transform.GetComponent<EnemyHealth>();
                if (enemyHealth.canDisapear)
                    enemyHealth.DisaperByTime();
            }

        }
    }
    private void Awake()
    {
      dropManager = this.GetComponent<DropManager>();
    }
    private void OnEnable()
    {
        Init();
    }
    // ������ƮǮ �Ŵ��� �غ� �Ϸ�ǥ��
    public bool isReady { get; private set; }

    [SerializeField]
    private ObjectInfo[] objectInfos = null;

    // ������ ������Ʈ�� key�������� ���� ����
    private string objectName;

    // ������ƮǮ���� ������ ��ųʸ�
    private Dictionary<string, IObjectPool<GameObject>> ojbectPoolDict = new Dictionary<string, IObjectPool<GameObject>>();

    // ������ƮǮ���� ������Ʈ�� ���� �����Ҷ� ����� ��ųʸ�
    private Dictionary<string, GameObject> newPoolDict = new Dictionary<string, GameObject>();
    private void Init()
    {
        isReady = false;

        for (int idx = 0; idx < objectInfos.Length; idx++)
        {
            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
            OnDestroyPoolObject, true, objectInfos[idx].count, objectInfos[idx].count);
            if (newPoolDict.ContainsKey(objectInfos[idx].objectName))
                return;

            newPoolDict.Add(objectInfos[idx].objectName, objectInfos[idx].perfab);
            ojbectPoolDict.Add(objectInfos[idx].objectName, pool);

            // �̸� ������Ʈ ���� �س���
            for (int i = 0; i < objectInfos[idx].count; i++)
            {
                objectName = objectInfos[idx].objectName;
                PoolAble poolAble = CreatePooledItem().GetComponent<PoolAble>();
                poolAble.pool.Release(poolAble.gameObject);
            }
        }

        isReady = true;
    }

    // ����
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(newPoolDict[objectName]);
        poolGo.GetComponent<PoolAble>().pool = ojbectPoolDict[objectName];
        return poolGo;
    }

    // �뿩
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // ��ȯ
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // ����
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }

    public GameObject GetOb(string goName)
    {
        objectName = goName;

        if (newPoolDict.ContainsKey(goName) == false)
        {
            Debug.LogFormat(this.name+":"+"{0} ������ƮǮ�� ��ϵ��� ���� ������Ʈ�Դϴ�.", goName);
            return null;
        }

        return ojbectPoolDict[goName].Get();
    }
    // Start is called before the first frame update
    void Start()
    {
        // �� ���� �� ������ �迭 �ʱ�ȭ
        enemySlot = new GameObject[enemyCount];
        enemyPositionArray = new GameObject[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            enemyPositionArray[i] = enemyPositon.transform.GetChild(i).gameObject;
        }
    }
    private void OnEnemyReleased(int num, int force, Vector3 pos, bool isDeadBody = true, bool isDrop = true)
    {
        enemySlot[num] = null;
        if(isDrop)
            dropManager.DropItems(pos);
        if (isDeadBody == true)
        {
            var deadbody = GetOb("dead");
            if (deadbody != null)
            {
                deadbody.transform.position = pos;
                DeadBody body = deadbody.GetComponent<DeadBody>();
                body.Force2DeadBody(force);
            }
        }
    }

    void CycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] == null)
            {
                var enemyObject = GetOb("enemy");
                EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
                enemyHealth.enemySpowner = this;
                // ������Ʈ Ǯ�� ����� ��� ���� ó��
                if (enemyObject == null)
                {
                    Debug.LogWarning("���� ������ �� �����ϴ�. ������Ʈ Ǯ�� ��������ϴ�.");
                    return;
                }

                if (isRandSpawn == true)
                {
                    float zPosition = Random.Range(-1f, 1f);
                    float xPosition = Random.Range(-1f, 1f);
                    int x = Random.Range(0, enemyCount);
                    enemyObject.transform.position = enemyPositionArray[x].transform.position + new Vector3(xPosition, 0, zPosition);
                }
                else
                {
                    float zPosition = Random.Range(-1f, 1f);
                    float xPosition = Random.Range(-1f, 1f);
                    enemyObject.transform.position = enemyPositionArray[i].transform.position + new Vector3(xPosition, 0, zPosition);
                }

                enemyHealth.enemyNum = i;
                enemyHealth.OnReleasedToPool -= OnEnemyReleased; // �ߺ� ��� ����
                enemyHealth.OnReleasedToPool += OnEnemyReleased; // �̺�Ʈ ���
                enemySlot[i] = enemyObject;
            }
        }
    }

    void DeCycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] != null)
            {
                EnemyHealth enemyHealth = enemySlot[i].GetComponent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.DisaperByTime();
                }
            }
        }
    }

    void Update()
    {

        if (isInPlayer == true)
        {
            once = false;
            if (TimeChange.instance.isDaytime == true && isDaytimeSpowner == true && activeOnce == true)
            {
                StartInvokeDaytime();
            }
            else if (TimeChange.instance.isDaytime == false && isDaytimeSpowner == true && activeOnce == false)
            {
                CancelInvoke("CycleCheck");
                EndInvokeDaytime();
            }
            if (TimeChange.instance.isDaytime == false && isDaytimeSpowner == false && activeOnce == true)
            {
                StartInvokeDaytime();
            }
            else if (TimeChange.instance.isDaytime == true && isDaytimeSpowner == false && activeOnce == false)
            {
                CancelInvoke("CycleCheck");
                EndInvokeDaytime();
            }
        }
        else if(isInPlayer == false && once == false)
        {
            once = true;
            CancelInvoke("CycleCheck");
            EndInvokeDaytime();
        }
    }
    bool once = false;

    bool activeOnce = true;
    void StartInvokeDaytime()
    {
        activeOnce = false;
        InvokeRepeating("CycleCheck", 1f, cycleFloat);
    }
    void EndInvokeDaytime()
    {
       activeOnce = true;
        DeCycleCheck();
    }
    bool isPoolCleared = false; // �ʱ�ȭ �� false�� ����

    public void ClearAllPools()
    {
        CancelInvoke("CycleCheck");
        activeOnce = true;
        // 1�ܰ�: ��� Ȱ��ȭ�� ������Ʈ�� Ǯ�� ��ȯ�ϰ� ��Ȱ��ȭ
        foreach (var poolEntry in ojbectPoolDict)
        {
            var pool = poolEntry.Value as ObjectPool<GameObject>; // ObjectPool�� ĳ����

            if (pool != null)
            {
                // Ȱ��ȭ�� ������Ʈ�� Ǯ�� ��ȯ
                while (pool.CountActive > 0)
                {
                    GameObject activeObj = pool.Get(); // Ȱ��ȭ�� ������Ʈ ��������
                    if (activeObj != null)
                    {
                        PoolAble poolAble = activeObj.GetComponent<PoolAble>();
                        if (poolAble != null)
                        {
                            poolAble.pool.Release(activeObj); // Ȱ��ȭ�� ������Ʈ�� Ǯ�� ��ȯ
                            activeObj.SetActive(false); // ��Ȱ��ȭ
                        }
                    }
                }
            }
        }

        // 2�ܰ�: �� ������Ʈ�� ��� ������
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] != null)
            {
                EnemyHealth enemyHealth = enemySlot[i].GetComponent<EnemyHealth>();
                enemyHealth.ForceEnemyRelease(); // �� ������Ʈ ���� ������
            }
        }

        GameObject deadBodyObject = GetOb("dead"); // "dead" ������Ʈ�� Ǯ���� ������
        if (deadBodyObject != null)
        {
            PoolAble poolAble = deadBodyObject.GetComponent<PoolAble>();
            if (poolAble != null)
            {
                poolAble.pool.Release(deadBodyObject); // Ǯ�� ��ȯ
                deadBodyObject.SetActive(false); // ��Ȱ��ȭ
            }
        }

        // 3�ܰ�: ��� Ǯ�� Ŭ����
        foreach (var poolEntry in ojbectPoolDict)
        {
            var pool = poolEntry.Value as ObjectPool<GameObject>; // ObjectPool�� ĳ����

            if (pool != null)
            {
                // Ǯ�� ��Ȱ��ȭ�� ������Ʈ ����
                while (pool.CountInactive > 0)
                {
                    GameObject inactiveObj = pool.Get(); // ��Ȱ��ȭ�� ������Ʈ ��������
                    if (inactiveObj != null)
                    {
                        Destroy(inactiveObj); // ��Ȱ��ȭ�� ������Ʈ ����
                    }
                }

                // Ǯ ���� �ʱ�ȭ
                pool.Clear();
            }
        }

        // 4�ܰ�: ��ųʸ� �ʱ�ȭ
        ojbectPoolDict.Clear();
        newPoolDict.Clear();

    }



}
