using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpowner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount;

    public float cycleFloat;
    public GameObject[] enemySlot;
    public GameObject enemyPositon;
    public GameObject[] enemyPositionArray;

    public bool isDaytimeSpowner;

    public bool isRandSpawn = true;
    public IObjectPool<GameObject> Pool { get; private set; }
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
            EnemyHealth eh = collision.transform.GetComponent<EnemyHealth>();
            eh.DisaperByTime();
        }
    }
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, enemyCount, enemyCount);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < enemyCount; i++)
        {
               EnemyHealth enemy = CreatePooledItem().GetComponent<EnemyHealth>();
               enemy.Pool.Release(enemy.gameObject);
        }

    }
    // 생성
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(enemyPrefab);
        poolGo.GetComponent<EnemyHealth>().Pool = this.Pool;
        return poolGo;
    }

    // 사용
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


    void CycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] == null)
            {
                if(isRandSpawn == true)
                {
                    float zPosition = Random.Range(-1f, 1f);
                    float xPosition = Random.Range(-1f, 1f);
                    int x = Random.Range(0, enemyCount);
                  //  GameObject r = Instantiate(enemyPrefab, enemyPositionArray[x].transform.position + new Vector3(xPosition, 0, zPosition), enemyPositionArray[x].transform.rotation);
                    var enemyObject =Pool.Get();
                    enemyObject.transform.position = enemyPositionArray[x].transform.position + new Vector3(xPosition, 0, zPosition);
                    enemySlot[i] = enemyObject;
                }
                else
                {
                    float zPosition = Random.Range(-1f, 1f);
                    float xPosition = Random.Range(-1f, 1f);
                    // GameObject r = Instantiate(enemyPrefab, enemyPositionArray[i].transform.position + new Vector3(xPosition, 0, zPosition), enemyPositionArray[i].transform.rotation);
                    var enemyObject = Pool.Get();
                    enemyObject.transform.position = enemyPositionArray[i].transform.position + new Vector3(xPosition, 0, zPosition);
                    enemySlot[i] = enemyObject;
                }

            }

        }
    }
    void DeCycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] != null)
            {
                EnemyHealth eh = enemySlot[i].GetComponent<EnemyHealth>();
                RabbitHole rH = enemySlot[i].GetComponent<RabbitHole>();
                if (eh != null)
                {
                    eh.DisaperByTime();
                }
                if (rH != null)
                {
                    rH.DestroyRabbit();
                    Pool.Release(enemySlot[i].gameObject);
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
