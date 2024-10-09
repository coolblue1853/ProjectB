using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DropManager : MonoBehaviour
{
    [System.Serializable]
    private class ObjectInfo
    {
        // ������Ʈ �̸�
        public string objectName;
        // ������Ʈ Ǯ���� ������ ������Ʈ
        //public GameObject perfab;
        // ��� �̸� ���� �س�������
        public int count;


    }

    public LayerMask collisionLayer; // �浹 üũ�� ���� ���̾�
    public GameObject itemPrefab;
    // �����۰� Ȯ���� ���� ����ü

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
            goDic.Add(objectInfos[idx].objectName, itemPrefab);
            ojbectPoolDic.Add(objectInfos[idx].objectName, pool);

            // �̸� ������Ʈ ���� �س���
            for (int i = 0; i < objectInfos[idx].count; i++)
            {
                objectName = objectInfos[idx].objectName;
                PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
                poolAbleGo.pool.Release(poolAbleGo.gameObject);
            }
        }

        isReady = true;
    }

    // ����
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(goDic[objectName]);
        poolGo.GetComponent<PoolAble>().pool = ojbectPoolDic[objectName];
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
    public float impulseForce = 5f;
    float rayLength = 0.5f;

    public void DropItems(Vector3 dropPosition)
    {
      float offsetX = 0f;  // ������ ���� ������ ���� ������

        // ������ ���� �����Ͽ� Ȯ���� ���� �������� ����߸�
        foreach (DropItem dropItem in dropItems)
        {
            if (Random.value <= dropItem.dropProbability * ( 1+ (float)DatabaseManager.playerDropRate / 100 ))
            {
                // ������ �̸��� ������� �������� ã�Ƽ� ����
                if (itemPrefab != null)
                {
                   // Debug.Log(dropItem.itemName);

                    Vector3 newItemPosition = dropPosition + new Vector3(offsetX, dropItem.offsetY, 0f);
                    var newItem =  GetGo(dropItem.itemName);
                    newItem.transform.position = newItemPosition;

                    Rigidbody2D rb = newItem.GetComponent<Rigidbody2D>();

                    // Rigidbody�� �����ϴ��� Ȯ���մϴ�.
                    if (rb != null)
                    {
                        rb.gravityScale = 1f; // �߷� �������� 1�� ������ ����
                        Vector3 impulseDirection = new Vector2(0, 1);
                        rb.AddForce(impulseDirection * impulseForce, ForceMode2D.Impulse);
                    }
                    DropItemCheck itemCheck = newItem.GetComponent<DropItemCheck>();
                    itemCheck.SetItem(dropItem.itemName);
                    // �߰����� ������ ������ �ʿ��ϴٸ� ���⼭ ó��
        
                    Vector2 direction = new Vector2(1, 0);
                    Vector2 currentPosition = dropPosition;
                    // ��ǥ ��ġ������ ��θ� �˻�
                    RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, offsetX+0.7f, collisionLayer);

                    if (hit.collider == null)
                    {
                        offsetX += rayLength;  // ���� �������� ��ġ�� ���������� �ű�
                    }
         
                }
                else
                {
                    Debug.LogError("�������� ã�� �� �����ϴ�: " + dropItem.itemName);
                }
            }
        }
    }

}
