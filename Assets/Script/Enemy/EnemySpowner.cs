using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class EnemySpowner : MonoBehaviour
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
        dropManager = this.GetComponent<DropManager>();
    }
    // 오브젝트풀 매니저 준비 완료표시
    public bool isReady { get; private set; }

    [SerializeField]
    private ObjectInfo[] objectInfos = null;

    // 생성할 오브젝트의 key값지정을 위한 변수
    private string objectName;

    // 오브젝트풀들을 관리할 딕셔너리
    private Dictionary<string, IObjectPool<GameObject>> ojbectPoolDic = new Dictionary<string, IObjectPool<GameObject>>();

    // 오브젝트풀에서 오브젝트를 새로 생성할때 사용할 딕셔너리
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
                Debug.LogFormat("{0} 이미 등록된 오브젝트입니다.", objectInfos[idx].objectName);
                return;
            }

            goDic.Add(objectInfos[idx].objectName, objectInfos[idx].perfab);
            ojbectPoolDic.Add(objectInfos[idx].objectName, pool);

            // 미리 오브젝트 생성 해놓기
            for (int i = 0; i < objectInfos[idx].count; i++)
            {
                objectName = objectInfos[idx].objectName;
                PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
                poolAbleGo.Pool.Release(poolAbleGo.gameObject);
            }
        }

        Debug.Log("오브젝트풀링 준비 완료");
        isReady = true;
    }

    // 생성
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(goDic[objectName]);
        poolGo.GetComponent<PoolAble>().Pool = ojbectPoolDic[objectName];
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

    public GameObject GetGo(string goName)
    {
        objectName = goName;

        if (goDic.ContainsKey(goName) == false)
        {
            Debug.LogFormat("{0} 오브젝트풀에 등록되지 않은 오브젝트입니다.", goName);
            return null;
        }

        return ojbectPoolDic[goName].Get();
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
    private void OnEnemyReleased(int num, int force, Vector3 pos, bool isDeadBody = true)
    {
        enemySlot[num] = null;
        dropManager.DropItems(pos);
        if (isDeadBody == true)
        {
            var deadbody = GetGo("dead");
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
        if (isPoolCleared)
        {
            Debug.LogWarning("오브젝트 풀이 비워져 적을 생성할 수 없습니다.");
            return;
        }
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] == null)
            {
                var enemyObject = GetGo("enemy");

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

                // 이벤트 핸들러 등록
                EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
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
        Debug.Log("인보크 종료");
       // ClearAllPools ();
       activeOnce = true;
        DeCycleCheck();
    }
    bool isPoolCleared = false; // 초기화 시 false로 설정

public void ClearAllPools()
{
    foreach (var pool in ojbectPoolDic.Values)
    {
        pool.Clear(); // 각 풀을 비움
    }

    ojbectPoolDic.Clear(); // 딕셔너리도 비워줍니다.
    goDic.Clear();         // goDic도 비워줍니다.
    isPoolCleared = true;  // 오브젝트 풀이 비워졌음을 표시
}

}
