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


    public void OpenBox1() 
    {
        inventoryUI[0].SetActive(true);
        inventoryUI[1].SetActive(false);
        inventoryUI[2].SetActive(false);
        inventoryUI[3].SetActive(false);
        inventoryUI[4].SetActive(false);
        GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = 1;
    }
    public void OpenBox2()
    {
        inventoryUI[0].SetActive(false);
        inventoryUI[1].SetActive(true);
        inventoryUI[2].SetActive(false);
        inventoryUI[3].SetActive(false);
        inventoryUI[4].SetActive(false);
        GameObject insPositon = GetNthChildGameObject(inventoryUI[1], cusorCount[1]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = 2;
    }
    public GameObject Inventory;

    private void Awake()
    {


    }
    private void Start()
    {
        Inventory.SetActive(true);
        makeInventoryBox();
        inventoryArray = new int[maxBoxNum, 5];
        Invoke("ActiveFalse", 0.000000001f); // �κ��丮 ���ε�, �� ������ ������ GridLayoutGroup�� �����۵����� ����.
    }

    void ActiveFalse()
    {
        cusorCount[0] = 0;
        cusorCount[1] = 0;
        cusorCount[2] = 0;
        cusorCount[3] = 0;
        cusorCount[4] = 0;
        GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
        cusor.transform.position = insPositon.transform.position;
        Inventory.SetActive(false); 
        inventoryUI[1].SetActive(false);
        inventoryUI[2].SetActive(false);
        inventoryUI[3].SetActive(false);
        inventoryUI[4].SetActive(false);
    }

    void makeInventoryBox()
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
            else if (i < 50)
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
        if(Inventory.activeSelf == true && inventoryUI[0].activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && (cusorCount[0] < maxBoxNum-1 && cusorCount[0] < 29))
            {
                cusorCount[0] += 1;
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
                cusor.transform.position = insPositon.transform.position;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && (cusorCount[0] == maxBoxNum - 1 || cusorCount[0] == 29))
            {
                cusorCount[0] = 0;
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
                cusor.transform.position = insPositon.transform.position;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && (cusorCount[0] > 0))
            {
                cusorCount[0] -= 1;
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
                cusor.transform.position = insPositon.transform.position;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && cusorCount[0] == 0)
            {
                if(maxBoxNum < 29)
                {
                    cusorCount[0] = maxBoxNum-1;
                }
                else
                {
                    cusorCount[0] = 29;
                }
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
                cusor.transform.position = insPositon.transform.position;
            }
        }
        if (Inventory.activeSelf == true && inventoryUI[1].activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && (cusorCount[1] < maxBoxNum - 1 && cusorCount[1] < 29))
            {
                cusorCount[1] += 1;
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[1]);
                cusor.transform.position = insPositon.transform.position;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && (cusorCount[1] == maxBoxNum - 1 || cusorCount[1] == 29))
            {
                cusorCount[1] = 0;
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[1]);
                cusor.transform.position = insPositon.transform.position;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && (cusorCount[1] > 0))
            {
                cusorCount[1] -= 1;
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[1]);
                cusor.transform.position = insPositon.transform.position;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && cusorCount[1] == 0)
            {
                if (maxBoxNum < 29)
                {
                    cusorCount[1] = maxBoxNum - 1;
                }
                else
                {
                    cusorCount[1] = 29;
                }
                GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[1]);
                cusor.transform.position = insPositon.transform.position;
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
