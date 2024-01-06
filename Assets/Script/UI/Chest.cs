using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Chest : MonoBehaviour
{
    public GameObject inventoryOb;
    public GameObject chestOb;
    bool isChestActive;
    public GameObject itemPrefab;
    public GameObject[] inventoryUI;
    public GameObject[] inventoryBox;

    public GameObject cusor; // �κ��丮 Ŀ��
    public GameObject changeCusor; // �κ��丮 Ŀ��
    int[] cusorCount = new int[5]; // �κ��丮 Ŀ��
    int boxCusor = 0; // �ڽ� �̵��� Ŀ�� int
    int maxBoxCusor = 0; // �ִ� �̵� ���� �ڽ� Ŀ��
    public int maxBoxNum;
    int[,] inventoryArray;

    public int nowBox;  // �̰����� �迭�� ���� �о���� ��. ��, nowBox�� 2�϶��� 15����� 30 +15 -> 45��° ĭ�� �ִ� �������̶�� ���� ��.
    public GameObject inventory;
    public GameObject miscDetail;
    int detailX = 150, detailY = 12;
    static public InventoryManager instance;
    public string state;
    public GameObject divideUI;
    public DivideSlider divideSlider;



    public int maxHor = 5;
    public int maxVer = 5;
    KeyAction action;
    InputAction upAction;
    private void OnEnable()
    {
        upAction.Enable();

    }


    private void OnDisable()
    {
        upAction.Disable();

    }
    private void Awake()
    {
        ResetInventoryBox();
        inventoryUI[0].SetActive(true);
           inventoryArray = new int[maxBoxNum, 5];
        action = new KeyAction();
        upAction = action.UI.Up;

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            isChestActive = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            isChestActive = false;
        }
    }
    private void Update()
    {
        if (isChestActive == true&& upAction.triggered)
        {
            DatabaseManager.isOpenUI = true;
            InventoryManager.instance.CheckNowChest(this);
            inventoryOb.SetActive(true);
            chestOb.SetActive(true);

        }
    }

    public void CreatItem(string itemName)
    {
        bool isCreate = false;

        if (CheckStack(itemName) == false)
        {

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < (maxHor * maxVer); i++)
                {
                    if (maxBoxNum > (j * (maxHor * maxVer)) + i)
                    {
                        if (inventoryArray[i, j] == 0)
                        {
                            inventoryArray[i, j] = 1; // i��° ��ġ�� �κ��丮 â ����.
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            float scaleFactor = 0.8f; // ũ�⸦ ������ ����
                            item.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            isCreate = true;
                            InventoryManager.instance.itemCheck.nowStack -= 1;
                            break;
                        }
                    }
                }
                if (isCreate == true)
                {
                    break;
                }
            }
        }

    }
    bool CheckStack(string itemName, int stack =1)
    {
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum > (j * (maxHor * maxVer)) + i)
                {
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                    if (insPositon.transform.childCount > 0)
                    {
                        GameObject item = insPositon.transform.GetChild(0).gameObject;
                        ItemCheck check = item.GetComponent<ItemCheck>();
                        if (check.name == itemName)
                        {
                            if (check.maxStack > check.nowStack)
                            {
                                InventoryManager.instance.itemCheck.nowStack -= 1;
                                check.nowStack += stack;
                                return true;
                            }
 

                        }
                    }

                }
            }
        }
        return false;
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
    public void ResetInventoryBox()
    {
        for (int i = 0; i < 5; i++)
        {
            inventoryUI[i].SetActive(false);
        }
    }
    public void CreatItemSelected(string itemName, int boxNum, int Stack = 1)
    {
        //boxNum -= 1;

        for (int i = 0; i < (maxHor * maxVer); i++)
        {
            if (maxBoxNum > (boxNum * (maxHor * maxVer)) + i)
            {
                if (inventoryArray[i, boxNum] == 0)
                {

                    inventoryArray[i, boxNum] = 1; // i��° ��ġ�� �κ��丮 â ����.
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[boxNum], i);
                    GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                    ItemCheck check = item.GetComponent<ItemCheck>();

                    check.SetItem(itemName);
                    check.nowStack = Stack;
                    break;
                }
            }
        }

    }
    public bool CheckBoxCanCreat()
    {
        bool isCreat = false;
        for(int j = 0; j < 5; j++)
        {
            for (int i = 0; i < maxHor * maxVer; i++)
            {
                if (inventoryArray[i, j] == 0 && j * maxHor * maxVer + i < maxBoxNum)
                {
                    isCreat = true;
                    InventoryManager.instance.nowChestNum = j;
                    break;
                }

            }
        }

        if (isCreat == true)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

}
