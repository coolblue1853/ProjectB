using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab; // �����Ǵ� Box �ν��Ͻ�
    public GameObject itemPrefab; // �����Ǵ� Box �ν��Ͻ�
    public GameObject[] inventoryUI;

    public GameObject cusor; // �κ��丮 Ŀ��
     int[] cusorCount = new int[5]; // �κ��丮 Ŀ��
    public int maxBoxNum;
    int[,] inventoryArray;

    public int nowBox;  // �̰����� �迭�� ���� �о���� ��. ��, nowBox�� 2�϶��� 15����� 30 +15 -> 45��° ĭ�� �ִ� �������̶�� ���� ��.
    public GameObject Inventory;


    public void ResetInventoryBox()
    {
        for (int i = 0; i < 5; i++)
        {
            inventoryUI[i].SetActive(false);
        }
    }
    public void OpenBox(int num) 
    {
        ResetInventoryBox();
        inventoryUI[num].SetActive(true);
        GameObject insPositon = GetNthChildGameObject(inventoryUI[num], cusorCount[num]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = num;
    }


    private void Start()
    {
        Inventory.SetActive(true);
        MakeInventoryBox();
        inventoryArray = new int[maxBoxNum, 5];
        Invoke("ActiveFalse", 0.000000001f); // �κ��丮 ���ε�, �� ������ ������ GridLayoutGroup�� �����۵����� ����.
    }

    void ActiveFalse()
    {
        for(int i = 0; i < 5; i++)
        {
            cusorCount[i] = 0;
        }
        GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
        cusor.transform.position = insPositon.transform.position;
        Inventory.SetActive(false);
        ResetInventoryBox();
        inventoryUI[0].SetActive(true);
    }

    void MakeInventoryBox()
    {
        for (int i = 0; i < maxBoxNum; i++)
        {
            if (i < 30)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[0].transform);
            }
            else if (i < 60)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[1].transform);
            }
            else if (i < 90)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[2].transform);
            }
            else if (i < 120)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[3].transform);
            }
            else if (i < 150)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[4].transform);
            }

        }
    }
    private void Update()
    {
        CusorChecker();

        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestDelet(0,3);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            testCreat("Rock");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (Inventory.activeSelf == true)
            {
                Inventory.SetActive(false);
            }
            else
            {
                Inventory.SetActive(true);
            }
        }
    }
    void CusorChecker()
    {
        if(Inventory.activeSelf == true )
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] + 1 < nowBoxMax)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] -= nowBoxMax-1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }

            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] - 1 >=0)
                {
                    cusorCount[nowBox] -= 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] + 6 < nowBoxMax)
                {
                    cusorCount[nowBox] += 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = cusorCount[nowBox] % 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] - 6 >= 0)
                {
                    cusorCount[nowBox] -= 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    while (cusorCount[nowBox] +6 < nowBoxMax)
                    {
                        cusorCount[nowBox] +=6;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }


        }





    }

    public void testCreat(string itemName)
    {
        bool isCreate = false;
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 30; i++)
            {
              if(maxBoxNum > (j * 30) + i)
                {
                    if (inventoryArray[i, j] == 0)
                    {
                        inventoryArray[i, j] = 1; // i��° ��ġ�� �κ��丮 â ����.
                        if (j == 0)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[0], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            isCreate = true;
                            break;
                        }
                        else if (j == 1)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[1], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            isCreate = true;
                            break;
                        }
                        else if (j == 2)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[2], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            break;
                        }
                        else if (j == 3)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[3], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            break;
                        }
                        else if (j == 4)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[4], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            break;
                        }
                    }
                }
            }
            if (isCreate == true)
            {
                break;
            }
        }

    }


    public void TestDelet(int uiNum, int boxNum)
    {
        inventoryArray[boxNum, uiNum] = 0;
        RemoveFirstChild(GetNthChildGameObject(inventoryUI[uiNum], boxNum));
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

    // ù ��° �ڽ��� ù ��° �ڽ��� �����ϴ� �Լ�
    void RemoveFirstChild(GameObject parent)
    {
        // parent�� null�� �ƴϰ� �ڽ��� ��� �ϳ� �̻� �ִٸ�
        if (parent != null && parent.transform.childCount > 0)
        {
            Transform firstChild = parent.transform.GetChild(0);

            // ù ��° �ڽ��� ����
            Destroy(firstChild.gameObject);
        }
    }
}
