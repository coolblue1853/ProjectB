using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab; // �����Ǵ� Box �ν��Ͻ�
    public GameObject itemPrefab; // �����Ǵ� Box �ν��Ͻ�
    public GameObject inventoryUI; // �ν��Ͻ��� ������ ��ġ.
    public int maxBoxNum;
    int[,] inventoryArray;

    private void Start()
    {
        for(int i = 0; i < maxBoxNum; i++)
        {
            Instantiate(inventoryBoxPrefab,inventoryUI.transform);
        }
        inventoryArray = new int[maxBoxNum, 1];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            testCreat();
        }
    }
    public void testCreat()
    {
        for (int i = 0; i < maxBoxNum; i++)
        {
            if(inventoryArray[i,0] == 0)
            {
                inventoryArray[i, 0] = 1; // i��° ��ġ�� �κ��丮 â ����.
                GameObject insPositon = GetNthChildGameObject(inventoryUI, i);
                Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                break;
            }

        }
    }



    GameObject GetNthChildGameObject(GameObject parent, int n)
    {
        if (parent != null && n >= 0 && n < parent.transform.childCount)
        {
            Transform childTransform = parent.transform.GetChild(n);

            if (childTransform != null)
            {
                GameObject childGameObject = childTransform.gameObject;
                return childGameObject;
            }
        }

        // Ư�� ������Ʈ�� n��° �ڽ��� �������� �ʴ� ��� �Ǵ� parent�� null�� ���
        return null;
    }
}
