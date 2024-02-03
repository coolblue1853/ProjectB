using UnityEngine;

public class DropManager : MonoBehaviour
{
    public GameObject itemPrefab;
    // 아이템과 확률을 담을 구조체
    [System.Serializable]
    public struct DropItem
    {
        public string itemName;
        [Range(0f, 1f)] public float dropProbability;
    }

    [SerializeField]
    private DropItem[] dropItems;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            DropItems(this.transform.position);
        }
    }
    public void DropItems(Vector3 dropPosition)
    {

        float offsetX = 0f;  // 아이템 간격 조절을 위한 오프셋

        // 랜덤한 값을 생성하여 확률에 따라 아이템을 떨어뜨림
        foreach (DropItem dropItem in dropItems)
        {
            if (Random.value <= dropItem.dropProbability)
            {
                // 아이템 이름을 기반으로 프리팹을 찾아서 생성
                if (itemPrefab != null)
                {

                    Vector3 newItemPosition = dropPosition + new Vector3(offsetX, 0f, 0f);
                    GameObject newItem = Instantiate(itemPrefab, newItemPosition, Quaternion.identity);
                    DropItemCheck itemCheck = newItem.GetComponent<DropItemCheck>();
                    itemCheck.SetItem(dropItem.itemName);
                    // 추가적인 아이템 설정이 필요하다면 여기서 처리

                    offsetX += 1f;  // 다음 아이템의 위치를 오른쪽으로 옮김
                }
                else
                {
                    Debug.LogError("프리팹을 찾을 수 없습니다: " + dropItem.itemName);
                }
            }
        }
    }

}
