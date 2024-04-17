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
    public float impulseForce = 5f;
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
                    Debug.Log(dropItem.itemName);

                    Vector3 newItemPosition = dropPosition + new Vector3(offsetX, 1f, 0f);
                    GameObject newItem = Instantiate(itemPrefab, newItemPosition, Quaternion.identity);
                    Rigidbody2D rb = newItem.GetComponent<Rigidbody2D>();

                    // Rigidbody�� �����ϴ��� Ȯ���մϴ�.
                    if (rb != null)
                    {
                        // ������Ʈ�� ���޽��� �����մϴ�. (0�� ������, 1�� ��, 0�� �չ����� ��Ÿ���ϴ�.)
                        Vector3 impulseDirection = new Vector2(0, 1); // �� �������� ���� ���Ϸ��� (0, 1, 0)�� ����մϴ�.
                        rb.AddForce(impulseDirection * impulseForce, ForceMode2D.Impulse);
                    }
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
