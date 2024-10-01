using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class Chest : MonoBehaviour
{
    public bool deletChest = false;
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
    public GameObject skillDetailUi;
    public GameObject detailPos;
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
    public void SaveChest()
    {
        string[,,] saveChest = new string[5, 30, 4]; // 몇번째인벤, 칸, 이름
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                GameObject box = inventoryUI[i].transform.GetChild(j).gameObject;
                if (box.transform.childCount > 0 && box.transform.GetChild(0).GetComponent<ItemCheck>() != null)
                {
                    ItemCheck item = box.transform.GetChild(0).GetComponent<ItemCheck>();
                    saveChest[i, j, 0] = item.itemData.name;
                    saveChest[i, j, 1] = item.nowStack.ToString();
                    saveChest[i, j, 2] = item.itemData.tear.ToString();
                    saveChest[i, j, 3] = item.itemData.upgrade.ToString();
                }
                else
                {
                    saveChest[i, j, 0] = "non";
                }
            }
        }
        SaveManager.instance.datas.chestItem = saveChest;
        SaveManager.instance.DataSave();
    }
    public void LoadChest()
    {
        if (SaveManager.instance.datas.chestItem == null)
        {
            SaveChest();
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                int childCount = inventoryUI[i].transform.childCount;
                if (childCount != 0)
                {
                    for (int j = 0; j < childCount; j++)
                    {
                        if (SaveManager.instance.datas.chestItem[i, j, 0] != "non" && SaveManager.instance.datas.chestItem[i, j, 0] != null)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[i], j);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            float scaleFactor = 0.8f; // 크기를 조절할 비율
                            item.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(SaveManager.instance.datas.chestItem[i, j, 0]);
                            check.nowStack = int.Parse(SaveManager.instance.datas.chestItem[i, j, 1]);
                            check.itemData.tear = int.Parse(SaveManager.instance.datas.chestItem[i, j, 2]);
                            check.itemData.upgrade = int.Parse(SaveManager.instance.datas.chestItem[i, j, 3]);
                        }
                    }
                }
            }
        }
    }
    public void CusorContinuousInputCheck()
    {
        checkRepeat = true;
        sequence.Kill();
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
    }
    void BoxChangeByKey()
    {
        if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
        {
            CusorContinuousInputCheck();
            if (maxBoxCusor - 1 > boxCusor) boxCusor += 1;
            else boxCusor = 0;
            GameObject insPositon = inventoryBox[boxCusor];
            if (state == "boxChange") cusor.transform.position = insPositon.transform.position;
            else if (state == "itemBoxChange") changeCusor.transform.position = insPositon.transform.position;
        }
        if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1))
        {
            CusorContinuousInputCheck();
            if (0 < boxCusor) boxCusor -= 1;
            else boxCusor = maxBoxCusor - 1;
            GameObject insPositon = inventoryBox[boxCusor];
            if (state == "boxChange") cusor.transform.position = insPositon.transform.position;
            else if (state == "itemBoxChange") changeCusor.transform.position = insPositon.transform.position;
        }
        if (downInventoryAction.triggered )
        {
            CusorContinuousInputCheck();
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            if (state == "boxChange") cusor.transform.position = insPositon.transform.position;
            else if (state == "itemBoxChange") changeCusor.transform.position = insPositon.transform.position;
            if (state == "boxChange") state = "";
            else if (state == "itemBoxChange") state = "change";
        }
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
        Invoke("LoadSave", 1f);
    }
    
    void LoadSave()
    {
        if (deletChest == false)
        {
            LoadChest();
        }
        else
        {
            SaveChest();
        }
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
            ResetChest();
        }
        if(isCusorChest == true)
        {
            if(InventoryManager.instance.state == "InChestMove")
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
                        cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
                    }
                }
                if (inventory.activeSelf == true)
                {
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
    }
    GameObject item;
   public  ItemCheck itemCheck ;
   void C2IMove()
    {
        if( InventoryManager.instance.BoxFullCheck() == true) // 빈공간이 있다면
        {
            GameObject itemBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            if (itemBox.transform.childCount > 0)
            {
                item = itemBox.transform.GetChild(0).gameObject;
                itemCheck = item.transform.GetComponent<ItemCheck>();
                int count = itemCheck.nowStack;

                for (int i = 0; i < count; i++)
                {
                    InventoryManager.instance.CreatItem(itemCheck.itemData.name, true);
                }
                if (itemCheck.nowStack <= 0)
                    Destroy(item.gameObject);
            }
        }
        else
        {
            GameObject itemBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);

            if (itemBox.transform.childCount > 0)
            {
                item = itemBox.transform.GetChild(0).gameObject;
                itemCheck = item.transform.GetComponent<ItemCheck>();
            }
            if (InventoryManager.instance.CheckStack(itemCheck.itemData.name) == true) // 빈공간이없다면
            {
                if (itemBox.transform.childCount > 0)
                {
                    while (InventoryManager.instance.OnlyCheckStack(itemCheck.itemData.name) == true)
                    {
                        itemCheck.nowStack -= 1;
                        InventoryManager.instance.CreatItem(itemCheck.itemData.name, true);
                    }
                }
            }
        }
    }
    void OnlyDetailObOff()
    {
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        skillDetailUi.SetActive(false);
    }
    public GameObject consumDetail;
    public GameObject equipDetail;
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
                if (afterItemCheck.itemData.name == beforeItemCheck.itemData.name && (afterItemCheck.nowStack != afterItemCheck.itemData.maxStack && beforeItemCheck.nowStack != beforeItemCheck.itemData.maxStack))
                {
                    isSame = true; // 같다면 가능한 만큼 스택을 합친다.

                    if (afterItemCheck.nowStack + beforeItemCheck.nowStack <= afterItemCheck.itemData.maxStack)
                    {
                        afterItemCheck.nowStack += beforeItemCheck.nowStack;
                        Destroy(beforitem);
                    }
                    else
                    {
                        beforeItemCheck.nowStack -= (afterItemCheck.itemData.maxStack - afterItemCheck.nowStack);
                        afterItemCheck.nowStack = afterItemCheck.itemData.maxStack;
                    }
                }
                else
                {
                    changeItem = afterBox.transform.GetChild(0).gameObject;
                    changeItem.transform.SetParent(beforBox.transform);
                    changeItem.transform.position = beforBox.transform.position;
                }
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
        CreatItemSelected(item.itemData.name, nowBox, output);
    }
    void ChangeCusorChecker()
    {
        if (inventory.activeSelf == true && state == "change")
        {
            if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
            {
                CusorContinuousInputCheck();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum) nowBoxMax = (maxHor * maxVer);
                else nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                if (cusorCount[nowBox] + 1 < nowBoxMax)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }
            if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1))
            {
                CusorContinuousInputCheck();
                if (cusorCount[nowBox] % maxHor == 0)
                {
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
                CusorContinuousInputCheck();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum) nowBoxMax = (maxHor * maxVer);
                else nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                if (cusorCount[nowBox] + maxHor < nowBoxMax)
                {
                    cusorCount[nowBox] += maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }
            if ((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1))
            {
                CusorContinuousInputCheck();
                if (cusorCount[nowBox] - maxHor >= 0)
                {
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
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
        state = "";
        CusorContinuousInputCheck();
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
                CusorContinuousInputCheck();
                DetailOff();
                if (((cusorCount[nowBox]+1) % (maxHor)) ==0 && cusorCount[nowBox] !=0)
                {
                    InventoryManager.instance.checkRepeat = false;
                    InventoryManager.instance.CusorContinuousInputCheck();
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

                CusorContinuousInputCheck();
                DetailOff();
                if (cusorCount[nowBox] % maxHor == 0)
                {
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

                CusorContinuousInputCheck();
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum) nowBoxMax = (maxHor * maxVer);
                else nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                if (cusorCount[nowBox] + maxHor < nowBoxMax)
                {
                    cusorCount[nowBox] += maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
            if ((upInventoryAction.triggered && DoubleCheckController == false) || (checkRepeat == false && verticalInput == 1))
            {
                CusorContinuousInputCheck();
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum) nowBoxMax = (maxHor * maxVer);
                else nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                if (cusorCount[nowBox] - maxHor >= 0)
                {
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    state = "boxChange";
                    boxCusor = nowBox;
                    cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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
            cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
            state = "";
        }
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        skillDetailUi.SetActive(false);
    }


    public void CloseChest()
    {
        SaveChest();
        chestOb.SetActive(false);
    }
    public void CreatItem(string itemName)
    {
        bool isCreate = false;
        if (CheckStack(itemName) == false)
        {
            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum > (nowBox * (maxHor * maxVer)) + i)
                {
                    if (inventoryArray[i, nowBox] == 0)
                    {
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
                    if (check.itemData.name == itemName)
                    {
                        if (check.itemData.maxStack > check.nowStack)
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
        state = ""; 
        cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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
        for (int i = 0; i < (maxHor * maxVer); i++)
        {
            if (maxBoxNum > (boxNum * (maxHor * maxVer)) + i)
            {
                if (inventoryArray[i, boxNum] == 0)
                {
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
    ItemCheck detail;
    public void BoxContentChecker(GameObject ob = null) // Z키 클릭시 인벤토리창
    {
        if (inventoryArray[cusorCount[nowBox], nowBox] == 1 && (state == "" || state == "chestOpen"))
        {
            state = "detail";
            if (ob != null)
            {
                detail = ob.transform.GetComponent<ItemCheck>();
            }
            else
            {
                GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
                detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }
            if (detail.itemData.type == "Misc")
            {
                Transform misc = miscDetail.gameObject.transform;
                misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type;
                misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
                miscDetail.transform.position = detailPos.transform.position;

                miscDetail.SetActive(true);
                consumDetail.SetActive(false);
                equipDetail.SetActive(false);
                skillDetailUi.SetActive(false);
            }
            if (detail.itemData.type == "Consum")
            {
                Transform consum = consumDetail.gameObject.transform;
                consum.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type;
                consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
                consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
                consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
                InventoryManager.instance.SetConsumEffect(consum.GetChild(7).gameObject, detail);

                consum.transform.position = detailPos.transform.position;
                miscDetail.SetActive(false);
                consumDetail.SetActive(true);
                equipDetail.SetActive(false);
                skillDetailUi.SetActive(false);
            }
            if (detail.itemData.type == "Equip")
            {
                Transform equip = equipDetail.gameObject.transform;
                string folderPath = detail.itemData.equipArea + "/";
                // 리소스 폴더 내의 equipName을 로드합니다.
                GameObject prefab = Resources.Load<GameObject>(folderPath + detail.itemData.name);
                equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type + " : " + detail.itemData.equipArea;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
                equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
                equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
                InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, detail);
                if (detail.itemData.equipArea != "Weapon") // 방어구라면
                {
                    Equipment equipment = prefab.GetComponent<Equipment>();
                    InventoryManager.instance.SetArmorDetail(equip.GetChild(7).gameObject, equipment);
                }
                else // 무기인 경우
                {
                    Weapon weapon = prefab.GetComponent<Weapon>();
                    InventoryManager.instance.SetWeaponDetail(equip.GetChild(7).gameObject, weapon);
                    InventoryManager.instance.CheckSkillDetail(weapon);
                }

                equip.transform.position = detailPos.transform.position;
                miscDetail.SetActive(false);
                consumDetail.SetActive(false);
                equipDetail.SetActive(true);
            }
        }

        else if (state == "Equipment")
        {
            Transform equip = equipDetail.gameObject.transform;
            string folderPath = detail.itemData.equipArea + "/";
            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + detail.itemData.name);
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type + " : " + detail.itemData.equipArea;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
            InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, detail);
            if (detail.itemData.equipArea != "Weapon") // 방어구라면
            {
                Equipment equipment = prefab.GetComponent<Equipment>();
                InventoryManager.instance.SetArmorDetail(equip.GetChild(7).gameObject, equipment);
            }
            else // 무기인 경우
            {
                Weapon weapon = prefab.GetComponent<Weapon>();
                InventoryManager.instance.SetWeaponDetail(equip.GetChild(7).gameObject, weapon);
                InventoryManager.instance.CheckSkillDetail(weapon);
            }
            equip.transform.position = detailPos.transform.position;
            miscDetail.SetActive(false);
            consumDetail.SetActive(false);
            equipDetail.SetActive(true);
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
        else
        {
            state = "";
            CloseChest();
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
        CusorContinuousInputCheck();
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
        if (nowBox != siblingParentIndex)
        {
            if (CheckBoxCanCreat(siblingParentIndex))
            {
                CreatItemSelected(itemCheck.itemData.name, siblingParentIndex, itemCheck.nowStack);
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
        cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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
