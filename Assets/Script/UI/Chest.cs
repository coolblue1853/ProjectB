using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class Chest : MonoBehaviour
{
    public Image cusorImage;
    public GameObject inventoryOb;
    public GameObject chestOb;
    public bool isChestClose;
    public  bool isChestActive;
    public GameObject itemPrefab;
    public GameObject[] inventoryUI;
    public GameObject[] inventoryBox;

    public GameObject cusor; // 인벤토리 커서
    public GameObject changeCusor; // 인벤토리 커서
    int[] cusorCount = new int[5]; // 인벤토리 커서
    int boxCusor = 0; // 박스 이동시 커서 int
    int maxBoxCusor = 5; // 최대 이동 가능 박스 커서
    public int maxBoxNum;
    public int[,] inventoryArray;

    public int nowBox;  // 이것으로 배열의 값을 읽어오면 됨. 즉, nowBox가 2일때의 15번재는 30 +15 -> 45번째 칸에 있는 아이템이라는 말이 됨.
    public GameObject inventory;
    public GameObject miscDetail;
    int detailX = -100  , detailY = 12;
    static public InventoryManager instance;
    public string state;
    public GameObject divideUI;
    public ChestDivideSlider divideSlider;

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

    InputAction selectAction;
    InputAction changeAction;
    InputAction divideAction;
    InputAction backAction;
    InputAction chestMoveAction;

    bool checkRepeat = false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;

    public GameObject beforBox;
    public GameObject afterBox;
    int beforeCusorInt;
    private void OnEnable()
    {
        upAction.Enable();
        leftInventoryAction.Enable();
        rightInventoryAction.Enable();
        upInventoryAction.Enable();
        downInventoryAction.Enable();
        verticalCheck.Enable();
        horizontalCheck.Enable();
        selectAction.Enable();
        changeAction.Enable();
        divideAction.Enable();
        backAction.Enable();
        chestMoveAction.Enable();
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
        selectAction.Disable();
        changeAction.Disable();
        divideAction.Disable();
        backAction.Disable();
        chestMoveAction.Disable();
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

        selectAction = action.UI.Select;
        changeAction = action.UI.Change;
        divideAction = action.UI.Divide;
        backAction = action.UI.Back;
        chestMoveAction = action.UI.ChestMoveActive;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            isChestClose = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            isChestClose = false;
        }
    }
    void BoxChangeByKey()
    {
        if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());

            if (maxBoxCusor - 1 > boxCusor)
            {
                boxCusor += 1;
            }
            else
            {
                boxCusor = 0;
            }
            GameObject insPositon = inventoryBox[boxCusor];
            if (state == "boxChange")
            {
                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }
        }
        if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1))
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());

            if (0 < boxCusor)
            {
                boxCusor -= 1;
            }
            else
            {
                boxCusor = maxBoxCusor - 1;
            }

            GameObject insPositon = inventoryBox[boxCusor];
            if (state == "boxChange")
            {
                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }
         
        }
        
        if (downInventoryAction.triggered )
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            if (state == "boxChange")
            {
                Debug.Log("작동중");
                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }

            if (state == "boxChange")
            {
                state = "chestOpen";
            }
            else if (state == "itemBoxChange")
            {
                state = "change";
            }
        }
        /*
        if (leftInventoryAction.triggered && checkRepeat == false)
        {


        }
        */


        if (selectAction.triggered && state == "boxChange")
        {
            ResetInventoryBox();
            nowBox = boxCusor;
            OpenBox(nowBox);
            state = "";
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
        }
        if (changeAction.triggered && state == "itemBoxChange" && boxCusor != nowBox)
        {
            BoxChange(boxCusor);
            state = "";
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
        }
    }
    void ResetChest()
    {
        ResetInventoryBox();
        //InventoryManager.instance.ResetInventoryBox();

        cusor.SetActive(false);
        inventoryUI[0].SetActive(true);
        InventoryManager.instance.inventoryUI[0].SetActive(true);
        InventoryManager.instance.nowBox = 0;
        InventoryManager.instance.boxCusor = 0;
        InventoryManager.instance.ResetBoxOrigin();
        nowBox = 0;
        boxCusor = 0;

        InventoryManager.instance.cusor.SetActive(true);

        InventoryManager.instance.state = "chestOpen";
    }
    private void Start()
    {

        inventoryUI[0].transform.SetAsLastSibling();
        InventoryAlpha inventoryAlpha = inventoryUI[0].GetComponent<InventoryAlpha>();
        inventoryAlpha.A21();
    }
    private void Update()
    {

        if (isChestClose == true&& upAction.triggered && DatabaseManager.isOpenUI == false)
        {
            state = ""; 
            InventoryManager.instance.RepeatCheck();
            DatabaseManager.isOpenUI = true;
            InventoryManager.instance.CheckNowChest(this);
            inventoryOb.SetActive(true);
            chestOb.SetActive(true);

           // ResetChest();
        }
        if(isCusorChest == true)
        {
            if (chestMoveAction.triggered)
            {
                if (state == "")
                {
                    state = "C2IMove"; // 인벤토리에서 창고로 물건 이동
                    cusorImage.color = new Color(23f / 255f, 123f / 255f, 161f / 255f);

                }
                else if (state == "C2IMove")
                {
                    state = "";
                    cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
                }
            }
            if (inventory.activeSelf == true)
            {
                if (state == "")
                {
           //         BoxOpen();

                }
                if (selectAction.triggered)
                {
                    if (state == "")
                    {

                        BoxContentChecker();
                    }
                    else if (state == "detail")
                    {
                        CloseCheck();
                    }
                    if (state == "C2IMove")
                   {
                          C2IMove();
                   }
                }
             
            }
            if (backAction.triggered)
            {
                CloseCheck();
            }
            if ((state == "" || state == "detail" || state == "chestOpen" || state == "C2IMove") && inventory.activeSelf == true)
            {
                CusorChecker();

                if (divideAction.triggered)
                {
            OpenDivide();
                }
            }

            if (changeAction.triggered && inventory.activeSelf == true)
            {
               ActiveChangeCursor();
            }
            if (state == "change")
            {
                ChangeCusorChecker();
            }
            if ((state == "boxChange" || state == "itemBoxChange") && inventory.activeSelf == true)
            {
                BoxChangeByKey();
            }

            verticalInput = (verticalCheck.ReadValue<float>());
            horizontalInput = (horizontalCheck.ReadValue<float>());

        }
    }
    GameObject item;
   public  ItemCheck itemCheck ;
   void C2IMove()
    {

        GameObject itemBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        if (itemBox.transform.childCount > 0)
        {

             item = itemBox.transform.GetChild(0).gameObject;
              itemCheck = item.transform.GetComponent<ItemCheck>();
            //int siblingParentIndex = inventoryUI[num].transform.GetSiblingIndex();

            int count = itemCheck.nowStack;

            for (int i = 0; i < count; i++)
            {

                InventoryManager.instance.CreatItem(itemCheck.name, true) ;
            }

            if (itemCheck.nowStack <= 0)
            {
                Destroy(item.gameObject);
            }
        }
    }
    void OnlyDetailObOff()
    {
        miscDetail.SetActive(false);
    }
    void ActiveChangeCursor()
    {

        if (state == "" || state == "detail" || state == "chestOpen")
        {
            beforBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            if (beforBox.transform.childCount != 0)
            {
                beforeCusorInt = cusorCount[nowBox];

                changeCusor.transform.position = cusor.transform.position;
                changeCusor.SetActive(true);
                OnlyDetailObOff();
                state = "change";
            }


        }
        else if (state == "change")
        {
            bool isSame = false;
            afterBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            GameObject changeItem;
            GameObject beforitem;
            ItemCheck beforeItemCheck;
            ItemCheck afterItemCheck;
            if (afterBox.transform.childCount != 0 && beforBox != afterBox)
            {
                changeItem = afterBox.transform.GetChild(0).gameObject;
                afterItemCheck = changeItem.GetComponent<ItemCheck>();
                beforitem = beforBox.transform.GetChild(0).gameObject;
                beforeItemCheck = beforitem.GetComponent<ItemCheck>();
                if (afterItemCheck.name == beforeItemCheck.name && (afterItemCheck.nowStack != afterItemCheck.maxStack && beforeItemCheck.nowStack != beforeItemCheck.maxStack))
                {
                    isSame = true; // 같다면 가능한 만큼 스택을 합친다.

                    if (afterItemCheck.nowStack + beforeItemCheck.nowStack <= afterItemCheck.maxStack)
                    {
                        afterItemCheck.nowStack += beforeItemCheck.nowStack;
                        Destroy(beforitem);
                    }
                    else
                    {
                        beforeItemCheck.nowStack -= (afterItemCheck.maxStack - afterItemCheck.nowStack);
                        afterItemCheck.nowStack = afterItemCheck.maxStack;

                    }
                }
                else
                {

                    changeItem = afterBox.transform.GetChild(0).gameObject;
                    changeItem.transform.SetParent(beforBox.transform);
                    changeItem.transform.position = beforBox.transform.position;
                }
            }
            else
            {
            }

            if (isSame == false)
            {
                beforitem = beforBox.transform.GetChild(0).gameObject;
                beforitem.transform.SetParent(afterBox.transform);
                beforitem.transform.position = afterBox.transform.position;
            }
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
            changeCusor.SetActive(false);
            state = "";

        }
    }
    void OpenDivide()
    {
        beforBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        if (beforBox.transform.childCount != 0)
        {
            DetailOff();
            GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
            ItemCheck nowDivideItem = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            if (nowDivideItem.nowStack > 1)
            {
                state = "divide";
                divideSlider.currentValue = nowDivideItem.nowStack;
                divideSlider.gameObject.SetActive(true);
                divideSlider.itemCheck = nowDivideItem;
                divideSlider.ResetData();
            }

        }

    }
    public void DownNowStack(int output)
    {

        GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        ItemCheck item = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();

        item.nowStack -= output;
        CreatItemSelected(item.name, nowBox, output);
    }
    void ChangeCusorChecker()
    {
        if (inventory.activeSelf == true && state == "change")
        {
            if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] + 1 < nowBoxMax)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {/*
                    cusorCount[nowBox] -= nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                    */
                }

            }
            if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] % maxHor == 0)
                {/*
                    state = "itemBoxChange";
                    boxCusor = 0;
                    GameObject insPositon = inventoryBox[boxCusor];
                    changeCusor.transform.position = insPositon.transform.position;
                     */
                }
                else
                {
                    cusorCount[nowBox] -= 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                   
                }

            }
            if ((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] + maxHor < nowBoxMax)
                {
                    cusorCount[nowBox] += maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {/*
                    cusorCount[nowBox] = cusorCount[nowBox] % maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                    */
                }
            }
            if ((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] - maxHor >= 0)
                {
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {/*
                  
                    while (cusorCount[nowBox] + maxHor < nowBoxMax)
                    {
                        cusorCount[nowBox] += maxHor;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                    */
                    state = "itemBoxChange";
                    boxCusor = nowBox;
                    GameObject insPositon = inventoryBox[boxCusor];
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }


        }
    }
    public void CusorMove2Chest()
    {
        checkRepeat = true;
        sequence.Kill();
        state = "";
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        isChestActive = true;
        cusor.SetActive(true);
        InventoryManager.instance.changeCusor.SetActive(false);
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
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
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
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] %maxHor == 0)
                { /*
                    cusorCount[nowBox] = nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                       */
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
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] + maxHor < nowBoxMax)
                {
                    cusorCount[nowBox] += maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    /*
                    cusorCount[nowBox] = cusorCount[nowBox] % maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                    */
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
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] - maxHor >= 0)
                {
   
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
       

                }
                else
                {/*
                    while (cusorCount[nowBox] + maxHor < nowBoxMax)
                    {
                        cusorCount[nowBox] += maxHor;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                    */
                    state = "boxChange";
                    boxCusor = nowBox;
                    GameObject insPositon = inventoryBox[boxCusor];
                    cusor.transform.position = insPositon.transform.position;

                }

            }
        }
    }
        public void DetailOff()
        {
            if (state != "chestOpen" && state != "C2IMove")
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
             
            }

            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum > (nowBox * (maxHor * maxVer)) + i)
                {
                    if (inventoryArray[i, nowBox] == 0)
                    {
                        //inventoryArray[i, j] = 1; // i번째 위치한 인벤토리 창 열기.
                        GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], i);
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
  
            }
        }

    }
    bool CheckStack(string itemName, int stack =1)
    {
        for (int i = 0; i < (maxHor * maxVer); i++)
        {
            if (maxBoxNum > (nowBox * (maxHor * maxVer)) + i)
            {
                GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], i);
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
            inventoryUI[i].SetActive(true);
            InventoryAlpha inventoryAlpha = inventoryUI[i].GetComponent<InventoryAlpha>();
            inventoryAlpha.A20();
        }

    }
    public void OpenBox(int num)
    {
        ResetInventoryBox();
        inventoryUI[num].transform.SetAsLastSibling();
        InventoryAlpha inventoryAlpha = inventoryUI[num].GetComponent<InventoryAlpha>();
        inventoryAlpha.ChangeAlpha();
        GameObject insPositon = GetNthChildGameObject(inventoryUI[num], cusorCount[num]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = num;
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

                    //inventoryArray[i, boxNum] = 1; // i번째 위치한 인벤토리 창 열기.
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[boxNum], i);
                    GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                    ItemCheck check = item.GetComponent<ItemCheck>();
                    float scaleFactor = 0.8f; // 크기를 조절할 비율
                    item.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    check.SetItem(itemName);
                    check.nowStack = Stack;
                    break;
                }
            }
        }

    }
    public void BoxContentChecker(GameObject ob = null) // Z키 클릭시 인벤토리창
    {

        if (inventoryArray[cusorCount[nowBox], nowBox] == 1 && state == "")
        {
            Debug.Log("세부 확인");
            ItemCheck detail;
            state = "detail";

            if (ob != null)
            {
                detail = ob.transform.GetComponent<ItemCheck>();
            }
            else
            {
                Debug.Log("세부 확인2");
                GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
                //Debug.Log(gameObject.transform.GetChild(0));
                detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }

            if (detail.type == "Misc")
            {
                Transform misc = miscDetail.gameObject.transform;
                misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.name;
                misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
                misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = detail.acqPath;
                if (ob != null)
                {
                    miscDetail.transform.localPosition = new Vector2(ob.transform.localPosition.x - detailX, detailY);
                }
                else
                {
                    miscDetail.transform.localPosition = new Vector2(cusor.transform.localPosition.x - detailX, detailY);
                }
                miscDetail.SetActive(true);
            }
        }


    }
    void CloseCheck()
    {
        if (state == "detail")
        {
            state = "";
            DetailOff();
        }
        else if (state == "change")
        {
            state = "";
            cusorCount[nowBox] = beforeCusorInt;
            changeCusor.SetActive(false);
        }
        else if (state == "divide")
        {
            CloseDivide();
        }
    }
    public void CloseDivide()
    {
        divideSlider.gameObject.SetActive(false);
        state = "";
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
    void BoxChange(int num)
    {
        changeCusor.SetActive(false);
        GameObject itemBox = GetNthChildGameObject(inventoryUI[nowBox], beforeCusorInt);
        GameObject item = itemBox.transform.GetChild(0).gameObject;
        ItemCheck itemCheck = item.transform.GetComponent<ItemCheck>();

        InventoryAlpha boxCheck = inventoryUI[num].GetComponent<InventoryAlpha>();
        int siblingParentIndex = boxCheck.siblingIndex;
        Debug.Log("이동할 박스 : "+siblingParentIndex);
        Debug.Log("현재 박수 : "+nowBox);

        Debug.Log("작동중1");
        if (nowBox != siblingParentIndex)
        {
            Debug.Log("작동중2");
            if (CheckBoxCanCreat(siblingParentIndex))
            {
                Debug.Log("작동중3");
                CreatItemSelected(itemCheck.name, siblingParentIndex, itemCheck.nowStack);
                Destroy(item.gameObject);
                cusorCount[nowBox] = beforeCusorInt;
                changeCusor.SetActive(false);
                state = "";
            }
        }
    }

    public bool CheckBoxCanCreat(int num)
    {
        bool isCreat = false;
        for (int i = 0; i < maxHor * maxVer; i++)
        {
            if (inventoryArray[i, num] == 0 && num * maxHor * maxVer + i < maxBoxNum)
            {
                Debug.Log(i);
                isCreat = true;

                break;
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

    public void ChangeCusor(GameObject ob)
    {
        isChestActive = true;
        isCusorChest = true;
        cusorCount[nowBox] = ob.transform.GetSiblingIndex();
        cusor.transform.position = ob.transform.position;
        changeCusor.SetActive(false);
        InventoryManager.instance.CloseDivide();
        state = "";
        isChestActive = true;
        cusor.SetActive(true);
        InventoryManager.instance.state = "InChestMove";
        InventoryManager.instance.changeCusor.SetActive(false);
        InventoryManager.instance.cusor.SetActive(false);


    }

    public void DragReset()
    {
        divideUI.SetActive(false);
        changeCusor.SetActive(false);
        state = "";
    }

}
