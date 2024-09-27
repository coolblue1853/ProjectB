using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class RabbitHole : MonoBehaviour
{
    public GameObject rabbit;
    public int rabbitCount;

    public float CycleFloat;
    public GameObject[] rabbitActive;
    public IObjectPool<GameObject> Pool { get; private set; }
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, rabbitCount, rabbitCount);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < rabbitCount; i++)
        {
            EnemyHealth enemy = CreatePooledItem().GetComponent<EnemyHealth>();
            enemy.Pool.Release(enemy.gameObject);
        }

    }
    // 생성
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(rabbit);
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
        rabbitActive = new GameObject[rabbitCount];
      // Invoke("CreatRabbit", 1f);
        InvokeRepeating("CycleCheck", 1f, CycleFloat);
    }

    public void DestroyRabbit()
    {
        for (int i = 0; i < rabbitCount; i++)
        {
            if (rabbitActive[i] != null)
            {
                EnemyHealth eh = rabbitActive[i].GetComponent<EnemyHealth>();
                eh.DisaperByTime();

            }

        }

    }
    void CycleCheck()
    {
        for (int i = 0; i < rabbitCount; i++)
        {
            if (rabbitActive[i] == null)
            {
                float zPosition = Random.Range(-1f, 1f);
                float xPosition = Random.Range(-1f, 1f);

              //  GameObject r = Instantiate(rabbit, this.transform.position + new Vector3(xPosition, 0, zPosition), this.transform.rotation);
                //rabbitActive[i] = r;
                var enemyObject = Pool.Get();
                enemyObject.transform.position = this.transform.position + new Vector3(xPosition, 0, zPosition);
                rabbitActive[i] = enemyObject;
                RabbitReset RH = enemyObject.GetComponent<RabbitReset>(); 
                RH.rabbitHole = this.GetComponent<RabbitHole>();
            }

        }
    }


    public void ResetRabbit()
    {
        Invoke("ResetR", 2);
    }

    private void ResetR()
    {
        for (int i = 0; i < rabbitCount; i++)
        {
            if (rabbitActive[i] == null)
            {
                float xPosition = Random.Range(-1f, 1f);
                var enemyObject = Pool.Get();
                enemyObject.transform.position = this.transform.position + new Vector3(xPosition, 0);
                rabbitActive[i] = enemyObject;

                RabbitReset RH = enemyObject.GetComponent<RabbitReset>();
                RH.rabbitHole = this.GetComponent<RabbitHole>();
            }
        }
    }

}
