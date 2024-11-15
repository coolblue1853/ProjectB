using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class EnemySpawner : MonoBehaviour
{

    [System.Serializable]
    private class ObjectInfo
    {
        // 오브젝트 이름
        public string objectName;
        // 오브젝트 풀에서 관리할 오브젝트
        public GameObject perfab;
        // 몇개를 미리 생성 해놓을건지
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
    // 오브젝트풀 매니저 준비 완료표시
    public bool isReady { get; private set; }

    [SerializeField]
    private ObjectInfo[] objectInfos = null;

    // 생성할 오브젝트의 key값지정을 위한 변수
    private string objectName;

    // 오브젝트풀들을 관리할 딕셔너리
    private Dictionary<string, IObjectPool<GameObject>> ojbectPoolDict = new Dictionary<string, IObjectPool<GameObject>>();

    // 오브젝트풀에서 오브젝트를 새로 생성할때 사용할 딕셔너리
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

            // 미리 오브젝트 생성 해놓기
            for (int i = 0; i < objectInfos[idx].count; i++)
            {
                objectName = objectInfos[idx].objectName;
                PoolAble poolAble = CreatePooledItem().GetComponent<PoolAble>();
                poolAble.pool.Release(poolAble.gameObject);
            }
        }

        isReady = true;
    }

    // 생성
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(newPoolDict[objectName]);
        poolGo.GetComponent<PoolAble>().pool = ojbectPoolDict[objectName];
        return poolGo;
    }

    // 대여
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // 반환
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }

    public GameObject GetOb(string goName)
    {
        objectName = goName;

        if (newPoolDict.ContainsKey(goName) == false)
        {
            Debug.LogFormat(this.name+":"+"{0} 오브젝트풀에 등록되지 않은 오브젝트입니다.", goName);
            return null;
        }

        return ojbectPoolDict[goName].Get();
    }
    // Start is called before the first frame update
    void Start()
    {
        // 적 슬롯 및 포지션 배열 초기화
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
                // 오브젝트 풀이 비워진 경우 예외 처리
                if (enemyObject == null)
                {
                    Debug.LogWarning("적을 생성할 수 없습니다. 오브젝트 풀이 비워졌습니다.");
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
                enemyHealth.OnReleasedToPool -= OnEnemyReleased; // 중복 등록 방지
                enemyHealth.OnReleasedToPool += OnEnemyReleased; // 이벤트 등록
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
    bool isPoolCleared = false; // 초기화 시 false로 설정

    public void ClearAllPools()
    {
        CancelInvoke("CycleCheck");
        activeOnce = true;
        // 1단계: 모든 활성화된 오브젝트를 풀로 반환하고 비활성화
        foreach (var poolEntry in ojbectPoolDict)
        {
            var pool = poolEntry.Value as ObjectPool<GameObject>; // ObjectPool로 캐스팅

            if (pool != null)
            {
                // 활성화된 오브젝트를 풀로 반환
                while (pool.CountActive > 0)
                {
                    GameObject activeObj = pool.Get(); // 활성화된 오브젝트 가져오기
                    if (activeObj != null)
                    {
                        PoolAble poolAble = activeObj.GetComponent<PoolAble>();
                        if (poolAble != null)
                        {
                            poolAble.pool.Release(activeObj); // 활성화된 오브젝트를 풀에 반환
                            activeObj.SetActive(false); // 비활성화
                        }
                    }
                }
            }
        }

        // 2단계: 적 오브젝트를 모두 릴리즈
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] != null)
            {
                EnemyHealth enemyHealth = enemySlot[i].GetComponent<EnemyHealth>();
                enemyHealth.ForceEnemyRelease(); // 적 오브젝트 강제 릴리즈
            }
        }

        GameObject deadBodyObject = GetOb("dead"); // "dead" 오브젝트를 풀에서 가져옴
        if (deadBodyObject != null)
        {
            PoolAble poolAble = deadBodyObject.GetComponent<PoolAble>();
            if (poolAble != null)
            {
                poolAble.pool.Release(deadBodyObject); // 풀로 반환
                deadBodyObject.SetActive(false); // 비활성화
            }
        }

        // 3단계: 모든 풀을 클리어
        foreach (var poolEntry in ojbectPoolDict)
        {
            var pool = poolEntry.Value as ObjectPool<GameObject>; // ObjectPool로 캐스팅

            if (pool != null)
            {
                // 풀에 비활성화된 오브젝트 삭제
                while (pool.CountInactive > 0)
                {
                    GameObject inactiveObj = pool.Get(); // 비활성화된 오브젝트 가져오기
                    if (inactiveObj != null)
                    {
                        Destroy(inactiveObj); // 비활성화된 오브젝트 삭제
                    }
                }

                // 풀 상태 초기화
                pool.Clear();
            }
        }

        // 4단계: 딕셔너리 초기화
        ojbectPoolDict.Clear();
        newPoolDict.Clear();

    }



}
