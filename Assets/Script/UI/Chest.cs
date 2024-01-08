using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class Chest : MonoBehaviour
{
    Image cusorImage;
    public GameObject inventoryOb;
    public GameObject chestOb;
    bool isChestActive;
    public GameObject itemPrefab;
    public GameObject[] inventoryUI;
    public GameObject[] inventoryBox;

    public GameObject cusor; // 인벤토리 커서
    public GameObject changeCusor; // 인벤토리 커서
    int[] cusorCount = new int[5]; // 인벤토리 커서
    int boxCusor = 0; // 박스 이동시 커서 int
    int maxBoxCusor = 0; // 최대 이동 가능 박스 커서
    public int maxBoxNum;
    public int[,] inventoryArray;

    public int nowBox;  // 이것으로 배열의 값을 읽어오면 됨. 즉, nowBox가 2일때의 15번재는 30 +15 -> 45번째 칸에 있는 아이템이라는 말이 됨.
    public GameObject inventory;
    public GameObject miscDetail;
    int detailX = 150, detailY = 12;
    static public InventoryManager instance;
    public string state;
    public GameObject divideUI;
    public DivideSlider divideSlider;

    public bool isCusorChest = false;

    public int maxHor = 5;
    public int maxVer = 5;
    KeyAction action;
    InputAction upAction;
    InputAction leftInventoryAction;
    InputAction rightInventoryAction;
    InputAction upInventoryAction;
    InputAction downInventoryAction;
    InputAction verticalCheck;
    InputAction horizontalCheck;

    bool checkRepeat = false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;
    private void OnEnable()
    {
        upAction.Enable();
        leftInventoryAction.Enable();
        rightInventoryAction.Enable();
        upInventoryAction.Enable();
        downInventoryAction.Enable();
        verticalCheck.Enable();
        horizontalCheck.Enable();
    }


    private void OnDisable()
    {
        upAction.Disable();
        leftInventoryAction.Disable();
        rightInventoryAction.Disable();
        upInventoryAction.Disable();
        downInventoryAction.Disable();
        verticalCheck.Disable();
        horizontalCheck.Disable();
    }
    private void Awake()
    {
        ResetInventoryBox();
        inventoryUI[0].SetActive(true);
        inventoryArray = new int[maxBoxNum, 5];
        action = new KeyAction();
        upAction = action.UI.Up;
        leftInventoryAction = action.UI.LeftInventory;
        rightInventoryAction = action.UI.RightInventory;
        upInventoryAction = action.UI.UPInventory;
        downInventoryAction = action.UI.DownInventory;
        verticalCheck = action.UI.verticalCheck;
        horizontalCheck = action.UI.horizontalCheck;
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

        if (isChestActive == true&& upAction.triggered && DatabaseManager.isOpenUI == false)
        {

            Debug.Log("작동중");
            InventoryManager.instance.RepeatCheck();
            DatabaseManager.isOpenUI = true;
            InventoryManager.instance.CheckNowChest(this);
            inventoryOb.SetActive(true);
            chestOb.SetActive(true);

        }
        if(isCusorChest == true)
        {
            CusorChecker();
        }
    }

    public void CusorMove2Chest()
    {
        isChestActive = true;
        cusor.SetActive(true);
        GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        cusor.transform.position = insPositon.transform.position;
    }
    public string stick = "";
    float verticalInput;
    float horizontalInput;
    void CusorChecker()
    {
        if (inventory.activeSelf == true && isChestActive == true)
        {

            if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());


                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * (maxHor * maxVer)) - maxBoxNum;
                }
                if (((cusorCount[nowBox]+1) % (maxHor)) ==0 && cusorCount[nowBox] !=0)
                {
                    InventoryManager.instance.checkRepeat = false;
                    InventoryManager.instance.ExCheckMove();
                    cusor.SetActive(false);
                    isChestActive = false;
                    InventoryManager.instance.CusorChest2Inven();
                }
                else
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }

            }
            if ((leftInventoryAction.triggered && DoubleCheckController == false) || (checkRepeat == false && horizontalInput == -1))
            {

                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {                        
                    nowBoxMax = ((nowBox + 1) * (maxHor * maxVer)) - maxBoxNum;
                }
                if (cusorCount[nowBox]  == 0)
                {
                    cusorCount[nowBox] = nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] -= 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }


            }
            if ((downInventoryAction.triggered && DoubleCheckController == false) || (checkRepeat == false && verticalInput == -1))
            {

                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * (maxHor * maxVer)) - maxBoxNum;
                }
                if (cusorCount[nowBox] + maxHor < nowBoxMax)
                {
                    cusorCount[nowBox] += maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = cusorCount[nowBox] % maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
            if ((upInventoryAction.triggered && DoubleCheckController == false) || (checkRepeat == false && verticalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * (maxHor * maxVer)) - maxBoxNum;
                }
                if (cusorCount[nowBox] - maxHor >= 0)
                {
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    while (cusorCount[nowBox] + maxHor < nowBoxMax)
                    {
                        cusorCount[nowBox] += maxHor;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
        }
    }
        public void DetailOff()
        {
            if (state != "chestOpen" && state != "I2CMove")
            {
                state = "";
            }

            miscDetail.SetActive(false);
        }
        public void CloseChest()
    {
        chestOb.SetActive(false);
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
                            //inventoryArray[i, j] = 1; // i번째 위치한 인벤토리 창 열기.
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            float scaleFactor = 0.8f; // 크기를 조절할 비율
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

        // 특정 오브젝트의 n번째 자식이 존재하지 않는 경우 또는 parent가 null인 경우
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

                    inventoryArray[i, boxNum] = 1; // i번째 위치한 인벤토리 창 열기.
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
        public void RepeatCheck()
        {
            DoubleCheckController = true;
            dcSequence.Kill();
            dcSequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => DoubleCheck());
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime * 2)
            .OnComplete(() => ResetCheckRepeat());

        }
        void ResetCheckRepeat()
        {
            checkRepeat = false;
        }
        void DoubleCheck()
        {
            DoubleCheckController = false;
        }
    }
