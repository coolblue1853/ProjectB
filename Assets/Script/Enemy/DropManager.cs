using UnityEngine;

public class DropManager : MonoBehaviour
{
    public GameObject itemPrefab;
    // �����۰� Ȯ���� ���� ����ü
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

        float offsetX = 0f;  // ������ ���� ������ ���� ������

        // ������ ���� �����Ͽ� Ȯ���� ���� �������� ����߸�
        foreach (DropItem dropItem in dropItems)
        {
            if (Random.value <= dropItem.dropProbability)
            {
                // ������ �̸��� ������� �������� ã�Ƽ� ����
                if (itemPrefab != null)
                {

                    Vector3 newItemPosition = dropPosition + new Vector3(offsetX, 0f, 0f);
                    GameObject newItem = Instantiate(itemPrefab, newItemPosition, Quaternion.identity);
                    DropItemCheck itemCheck = newItem.GetComponent<DropItemCheck>();
                    itemCheck.SetItem(dropItem.itemName);
                    // �߰����� ������ ������ �ʿ��ϴٸ� ���⼭ ó��

                    offsetX += 1f;  // ���� �������� ��ġ�� ���������� �ű�
                }
                else
                {
                    Debug.LogError("�������� ã�� �� �����ϴ�: " + dropItem.itemName);
                }
            }
        }
    }

}
