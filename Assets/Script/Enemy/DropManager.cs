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
    private void OnEnable()
    {
        Init();
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


    public bool isReady { get; private set; }

    [SerializeField]
    private ObjectInfo[] objectInfos = null; // (구) 아이템별 풀 설정 — 직렬화 호환용으로만 유지

    // 모든 DropManager가 같은 드롭 프리팹을 쓰므로 프리팹당 공유 풀 1개만 둔다.
    // (기존: 스포너마다 × 아이템 이름마다 풀을 만들어 맵 진입 시 수백~천 개를 미리 생성 → 프리즈)
    const int SharedWarmCount = 12;
    static readonly Dictionary<GameObject, ObjectPool<GameObject>> sharedPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
    readonly HashSet<GameObject> myActiveDrops = new HashSet<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetShared() => sharedPools.Clear();

    private void Init()
    {
        if (itemPrefab == null)
        {
            return;
        }
        if (!sharedPools.TryGetValue(itemPrefab, out ObjectPool<GameObject> pool) || pool == null)
        {
            GameObject prefab = itemPrefab;
            ObjectPool<GameObject> created = null;
            created = new ObjectPool<GameObject>(
                () => { var go = Instantiate(prefab); go.GetComponent<PoolAble>().pool = created; return go; },
                go => go.SetActive(true),
                go => go.SetActive(false),
                go => { if (go != null) Destroy(go); },
                collectionCheck: true, defaultCapacity: SharedWarmCount, maxSize: 200);
            sharedPools[prefab] = created;
            var warm = new List<GameObject>(SharedWarmCount);
            for (int i = 0; i < SharedWarmCount; i++) warm.Add(created.Get());
            foreach (var go in warm) go.GetComponent<PoolAble>().ReleaseObject(); // 반환은 ReleaseObject 한 곳 (B-141)
            GameLog.Info(GameLog.Cat.Pool, "드롭 공유 풀 생성: " + prefab.name);
        }
        isReady = true;
    }

    /// <summary>드롭 오브젝트 하나를 꺼낸다(아이템 종류는 호출자가 SetItem으로 지정)</summary>
    public GameObject GetGo(string goName)
    {
        if (itemPrefab == null || !sharedPools.TryGetValue(itemPrefab, out ObjectPool<GameObject> pool))
        {
            Init();
            if (itemPrefab == null || !sharedPools.TryGetValue(itemPrefab, out pool))
            {
                return null;
            }
        }
        GameObject go = pool.Get();
        myActiveDrops.Add(go);
        if (go.TryGetComponent(out DropItemCheck check))
        {
            check.owner = this; // 공유 풀이므로 누가 떨어뜨렸는지 기록 (맵 이탈 정리용)
        }
        return go;
    }

    public float impulseForce = 5f;
    float rayLength = 0.5f;

    public void DropItems(Vector3 dropPosition)
    {
        float offsetX = 0f; // 아이템 간격
        foreach (DropItem dropItem in dropItems)
        {
            if (Random.value <= dropItem.dropProbability * (1 + (float)DatabaseManager.playerDropRate / 100))
            {
                if (DropOne(dropItem.itemName, dropPosition, dropItem.offsetY, ref offsetX))
                {
                    GameLog.Info(GameLog.Cat.Item, "드롭 " + dropItem.itemName + " (확률 " + (dropItem.dropProbability * 100f).ToString("F2") + "%)", this);
                }
            }
        }
    }

    /// <summary>확정 드롭 (정예·보스 보상). 확률표와 별개로 반드시 떨어뜨린다</summary>
    public void DropGuaranteed(IEnumerable<string> itemNames, Vector3 dropPosition)
    {
        float offsetX = 0.3f;
        foreach (string itemName in itemNames)
        {
            if (DropOne(itemName, dropPosition, 0.3f, ref offsetX))
            {
                GameLog.Info(GameLog.Cat.Item, "확정 드롭 " + itemName, this);
            }
        }
    }

    /// <summary>드롭 테이블 전체 (도감 표시용): 아이템 이름과 확률</summary>
    public List<(string name, float chance)> AllDrops()
    {
        var result = new List<(string, float)>();
        if (dropItems != null)
        {
            foreach (DropItem d in dropItems)
            {
                result.Add((d.itemName, d.dropProbability));
            }
        }
        return result;
    }

    /// <summary>이 드롭 테이블의 장비 아이템 이름들 (정예 확정 장비 후보)</summary>
    public List<string> EquipmentNames()
    {
        var result = new List<string>();
        if (dropItems == null)
        {
            return result;
        }
        foreach (DropItem d in dropItems)
        {
            int idx = DatabaseManager.instance.FindItemDataIndex(d.itemName);
            if (idx >= 0 && DatabaseManager.instance.LoadItemData(idx).type == "Equip" && !result.Contains(d.itemName))
            {
                result.Add(d.itemName);
            }
        }
        return result;
    }

    bool DropOne(string itemName, Vector3 dropPosition, float offsetY, ref float offsetX)
    {
        if (itemPrefab == null)
        {
            return false;
        }
        var newItem = GetGo(itemName);
        if (newItem == null)
        {
            GameLog.Anomaly(GameLog.Cat.Item, name + ": 드롭 풀에서 꺼내지 못함 " + itemName, this);
            return false;
        }
        newItem.transform.position = dropPosition + new Vector3(offsetX, offsetY, 0f);
        if (newItem.TryGetComponent(out Rigidbody2D rb))
        {
            rb.gravityScale = 1f;
            rb.AddForce(Vector2.up * impulseForce, ForceMode2D.Impulse);
        }
        newItem.GetComponent<DropItemCheck>().SetItem(itemName);
        // 오른쪽이 막혀 있지 않으면 다음 아이템을 옆으로
        RaycastHit2D hit = Physics2D.Raycast(dropPosition, Vector2.right, offsetX + 0.7f, collisionLayer);
        if (hit.collider == null)
        {
            offsetX += rayLength;
        }
        return true;
    }
    /// <summary>맵을 떠날 때: 이 스포너가 떨어뜨려 아직 바닥에 있는 드롭을 반환한다 (풀은 유지)</summary>
    public void ClearAll()
    {
        int n = 0;
        foreach (GameObject go in myActiveDrops)
        {
            if (go != null && go.activeSelf && go.TryGetComponent(out DropItemCheck p) && p.owner == this)
            {
                p.ReleaseObject();
                n++;
            }
        }
        myActiveDrops.Clear();
        if (n > 0)
        {
            GameLog.Info(GameLog.Cat.Pool, name + ": 바닥 드롭 " + n + "개 반환", this);
        }
    }
}
