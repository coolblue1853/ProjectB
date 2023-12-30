using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab; // 생성되는 Box 인스턴스
    public GameObject itemPrefab; // 생성되는 Box 인스턴스
    public GameObject[] inventoryUI;

    public GameObject cusor; // 인벤토리 커서
     int[] cusorCount = new int[5]; // 인벤토리 커서
    public int maxBoxNum;
    int[,] inventoryArray;

    public int nowBox;  // 이것으로 배열의 값을 읽어오면 됨. 즉, nowBox가 2일때의 15번재는 30 +15 -> 45번째 칸에 있는 아이템이라는 말이 됨.


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
        Invoke("ActiveFalse", 0.000000001f); // 인벤토리 리로드, 이 과정이 없으면 GridLayoutGroup이 정상작동하지 않음.
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
                        inventoryArray[i, j] = 1; // i번째 위치한 인벤토리 창 열기.
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

        // 특정 오브젝트의 n번째 자식이 존재하지 않는 경우 또는 parent가 null인 경우
        return null;
    }

    // 첫 번째 자식의 첫 번째 자식을 삭제하는 함수
    void RemoveFirstChild(GameObject parent)
    {
        // parent가 null이 아니고 자식이 적어도 하나 이상 있다면
        if (parent != null && parent.transform.childCount > 0)
        {
            Transform firstChild = parent.transform.GetChild(0);

            // 첫 번째 자식을 삭제
            Destroy(firstChild.gameObject);
        }
    }
}
