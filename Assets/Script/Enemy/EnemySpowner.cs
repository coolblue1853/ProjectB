using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class EnemySpowner : MonoBehaviour
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

    public bool isDaytimeSpowner;

    public bool isRandSpawn = true;

    public bool isInPlayer = false;
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
        Init();
    }
    // ������ƮǮ �Ŵ��� �غ� �Ϸ�ǥ��
    public bool isReady { get; private set; }

    [SerializeField]
    private ObjectInfo[] objectInfos = null;

    // ������ ������Ʈ�� key�������� ���� ����
    private string objectName;

    // ������ƮǮ���� ������ ��ųʸ�
    private Dictionary<string, IObjectPool<GameObject>> ojbectPoolDic = new Dictionary<string, IObjectPool<GameObject>>();

    // ������ƮǮ���� ������Ʈ�� ���� �����Ҷ� ����� ��ųʸ�
    private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();
    private void Init()
    {
        isReady = false;

        for (int idx = 0; idx < objectInfos.Length; idx++)
        {
            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
            OnDestroyPoolObject, true, objectInfos[idx].count, objectInfos[idx].count);

            if (goDic.ContainsKey(objectInfos[idx].objectName))
            {
                Debug.LogFormat("{0} �̹� ��ϵ� ������Ʈ�Դϴ�.", objectInfos[idx].objectName);
                return;
            }

            goDic.Add(objectInfos[idx].objectName, objectInfos[idx].perfab);
            ojbectPoolDic.Add(objectInfos[idx].objectName, pool);

            // �̸� ������Ʈ ���� �س���
            for (int i = 0; i < objectInfos[idx].count; i++)
            {
                objectName = objectInfos[idx].objectName;
                PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
                poolAbleGo.Pool.Release(poolAbleGo.gameObject);
            }
        }

        Debug.Log("������ƮǮ�� �غ� �Ϸ�");
        isReady = true;
    }

    // ����
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(goDic[objectName]);
        poolGo.GetComponent<PoolAble>().Pool = ojbectPoolDic[objectName];
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

    public GameObject GetGo(string goName)
    {
        objectName = goName;

        if (goDic.ContainsKey(goName) == false)
        {
            Debug.LogFormat("{0} ������ƮǮ�� ��ϵ��� ���� ������Ʈ�Դϴ�.", goName);
            return null;
        }

        return ojbectPoolDic[goName].Get();
    }
    // Start is called before the first frame update
    void Start()
    {
        enemySlot = new GameObject[enemyCount];
        enemyPositionArray = new GameObject[enemyCount];
        for (int i = 0; i < enemyCount; i++)
        {
            enemyPositionArray[i] = enemyPositon.transform.GetChild(i).gameObject;
        }

    }
    private void OnEnemyReleased(int num)
    {
        enemySlot[num] = null;
        // ���� Ǯ�� ������Ǿ��� �� ������ �۾�
        Debug.Log("Enemy released to pool");
    }

    void CycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] == null)
            {
                var enemyObject = GetGo("enemy");
                if (isRandSpawn == true)
                {
                    float zPosition = Random.Range(-1f, 1f);
                    float xPosition = Random.Range(-1f, 1f);
                    int x = Random.Range(0, enemyCount);
                    enemyObject.transform.position = enemyPositionArray[x].transform.position + new Vector3(xPosition, 0, zPosition);
                    enemySlot[i] = enemyObject;
                }
                else
                {
                    float zPosition = Random.Range(-1f, 1f);
                    float xPosition = Random.Range(-1f, 1f);
                    enemyObject.transform.position = enemyPositionArray[i].transform.position + new Vector3(xPosition, 0, zPosition);
                    enemySlot[i] = enemyObject;
                }
                // �̺�Ʈ �ڵ鷯 ���
                EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
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
}
