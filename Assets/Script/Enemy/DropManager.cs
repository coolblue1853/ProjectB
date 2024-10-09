using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DropManager : MonoBehaviour
{
    [System.Serializable]
    private class ObjectInfo
    {
        // 오브젝트 이름
        public string objectName;
        // 오브젝트 풀에서 관리할 오브젝트
        //public GameObject perfab;
        // 몇개를 미리 생성 해놓을건지
        public int count;


    }

    public LayerMask collisionLayer; // 충돌 체크를 위한 레이어
    public GameObject itemPrefab;
    // 아이템과 확률을 담을 구조체

    [System.Serializable]
    public struct DropItem
    {

        public string itemName;
        [Range(0f, 1f)] public float dropProbability;
        public float offsetY;
    }

    [SerializeField]
    private DropItem[] dropItems;
    private void Awake()
    {
        Init();
    }

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
            goDic.Add(objectInfos[idx].objectName, itemPrefab);
            ojbectPoolDic.Add(objectInfos[idx].objectName, pool);

            // 미리 오브젝트 생성 해놓기
            for (int i = 0; i < objectInfos[idx].count; i++)
            {
                objectName = objectInfos[idx].objectName;
                PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
                poolAbleGo.pool.Release(poolAbleGo.gameObject);
            }
        }

        isReady = true;
    }

    // 생성
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(goDic[objectName]);
        poolGo.GetComponent<PoolAble>().pool = ojbectPoolDic[objectName];
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
    public float impulseForce = 5f;
    float rayLength = 0.5f;

    public void DropItems(Vector3 dropPosition)
    {
      float offsetX = 0f;  // 아이템 간격 조절을 위한 오프셋

        // 랜덤한 값을 생성하여 확률에 따라 아이템을 떨어뜨림
        foreach (DropItem dropItem in dropItems)
        {
            if (Random.value <= dropItem.dropProbability * ( 1+ (float)DatabaseManager.playerDropRate / 100 ))
            {
                // 아이템 이름을 기반으로 프리팹을 찾아서 생성
                if (itemPrefab != null)
                {
                   // Debug.Log(dropItem.itemName);

                    Vector3 newItemPosition = dropPosition + new Vector3(offsetX, dropItem.offsetY, 0f);
                    var newItem =  GetGo(dropItem.itemName);
                    newItem.transform.position = newItemPosition;

                    Rigidbody2D rb = newItem.GetComponent<Rigidbody2D>();

                    // Rigidbody가 존재하는지 확인합니다.
                    if (rb != null)
                    {
                        rb.gravityScale = 1f; // 중력 스케일을 1로 강제로 설정
                        Vector3 impulseDirection = new Vector2(0, 1);
                        rb.AddForce(impulseDirection * impulseForce, ForceMode2D.Impulse);
                    }
                    DropItemCheck itemCheck = newItem.GetComponent<DropItemCheck>();
                    itemCheck.SetItem(dropItem.itemName);
                    // 추가적인 아이템 설정이 필요하다면 여기서 처리
        
                    Vector2 direction = new Vector2(1, 0);
                    Vector2 currentPosition = dropPosition;
                    // 목표 위치까지의 경로를 검사
                    RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, offsetX+0.7f, collisionLayer);

                    if (hit.collider == null)
                    {
                        offsetX += rayLength;  // 다음 아이템의 위치를 오른쪽으로 옮김
                    }
         
                }
                else
                {
                    Debug.LogError("프리팹을 찾을 수 없습니다: " + dropItem.itemName);
                }
            }
        }
    }

}
