using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab; // 생성되는 Box 인스턴스
    public GameObject itemPrefab; // 생성되는 Box 인스턴스
    public GameObject inventoryUI; // 인스턴스를 생성할 위치.
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
                inventoryArray[i, 0] = 1; // i번째 위치한 인벤토리 창 열기.
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

        // 특정 오브젝트의 n번째 자식이 존재하지 않는 경우 또는 parent가 null인 경우
        return null;
    }
}
