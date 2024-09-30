using UnityEngine;

public class DropManager : MonoBehaviour
{
    public LayerMask collisionLayer; // �浹 üũ�� ���� ���̾�
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

                    Vector2 direction = new Vector2(1, 0);
                    Vector2 currentPosition = dropPosition;
                    // ��ǥ ��ġ������ ��θ� �˻�
                    RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, offsetX+0.7f, collisionLayer);

                    if (hit.collider == null)
                    {
                        offsetX += rayLength;  // ���� �������� ��ġ�� ���������� �ű�
                    }
                    else
                    {

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
