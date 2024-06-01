using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class InventoryManager : MonoBehaviour
{
    public GameObject player;
    public GameObject inventoryBoxPrefab; // 생성되는 Box 인스턴스
    public GameObject itemPrefab; // 생성되는 Box 인스턴스
    public GameObject[] inventoryUI;
    public GameObject[] inventoryUIBack;
    public GameObject[] inventoryBox;
    Image cusorImage;
    public GameObject cusor; // 인벤토리 커서
    public GameObject changeCusor; // 인벤토리 커서
    public int[] cusorCount = new int[5]; // 인벤토리 커서
    public int boxCusor = 0; // 박스 이동시 커서 int
    int maxBoxCusor = 0; // 최대 이동 가능 박스 커서
    public int maxBoxNum;
    public int[,] inventoryArray;

    public int nowBox;  // 이것으로 배열의 값을 읽어오면 됨. 즉, nowBox가 2일때의 15번재는 30 +15 -> 45번째 칸에 있는 아이템이라는 말이 됨.
    public GameObject inventory;
    public GameObject miscDetail;
    public GameObject consumDetail;
    public GameObject equipDetail;
    int detailX = 150, detailY =12;
    int equipDetailX = 150;
    static public InventoryManager instance;
    public string state;
    public GameObject divideUI;
    public DivideSlider divideSlider;

    KeyAction action;
    InputAction openInventoryAction;
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
    InputAction consumAction;

    public int maxHor = 5;
    public int maxVer = 5;

    public GameObject handBox;
    public GameObject chestBox;
    public GameObject necklesBox;
    public GameObject headBox;
    public GameObject ringBox;
    public GameObject legBox;
    public GameObject shoesBox;
    public GameObject weaponBox;

    private void OnEnable()
    {

        openInventoryAction.Enable();
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
        consumAction.Enable();
    }
    private void OnDisable()
    {

        openInventoryAction.Disable();
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
        consumAction.Disable();
    }
    private void Awake()
    {
        cusorImage  = cusor.GetComponent<Image>(); ;
        action = new KeyAction();
        openInventoryAction = action.UI.OpenInventory;
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
        consumAction = action.UI.ActiveConsum;

        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        string state = "";
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
        if(chest != null)
        {
            state = "chestOpen";
        }
        else
        {
            state = "";
        }
        cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
        ResetInventoryBox();
        inventoryUI[num].transform.SetAsLastSibling();
        InventoryAlpha inventoryAlpha = inventoryUI[num].GetComponent<InventoryAlpha>();
        inventoryAlpha.ChangeAlpha();
        GameObject insPositon = GetNthChildGameObject(inventoryUI[num], cusorCount[num]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = num;
    }

    public bool CheckBoxCanCreat(int num)
    {
        bool isCreat = false;
        for (int i = 0; i < maxHor * maxVer; i++)
        {
            if (inventoryArray[i, num] == 0 && num * maxHor * maxVer + i < maxBoxNum)
            {
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
    public bool CheckBoxCanCreatAll()
    {
        bool isCreat = false;
        for (int j = 0; j <5; j++)
        {
            for (int i = 0; i < maxHor * maxVer; i++)
            {
                if (inventoryArray[i, j] == 0 && j * maxHor * maxVer + i < maxBoxNum)
                {
                    isCreat = true;

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
    private void Start()
    {
        inventory.SetActive(true);
        MakeInventoryBox();
        inventoryArray = new int[maxBoxNum, 5];
        Invoke("ActiveFalse", 0.000000001f); // 인벤토리 리로드, 이 과정이 없으면 GridLayoutGroup이 정상작동하지 않음.

    }

    // 나중에 퍼블릭 해체;
    public GameObject beforBox;
    public GameObject afterBox;
    int beforeCusorInt;

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
                CreatItemSelected(itemCheck.name, siblingParentIndex, itemCheck.nowStack);
                Destroy(item.gameObject);
                cusorCount[nowBox] = beforeCusorInt;
                changeCusor.SetActive(false);
                state = "";
            }
        }
    }

    public int nowChestNum;
    public ItemCheck itemCheck;
  void I2CMove()
    {

           GameObject itemBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        if (itemBox.transform.childCount > 0)
        {
            GameObject item = itemBox.transform.GetChild(0).gameObject;
             itemCheck = item.transform.GetComponent<ItemCheck>();
            //int siblingParentIndex = inventoryUI[num].transform.GetSiblingIndex();

            int count = itemCheck.nowStack;

            for (int i = 0; i < count; i++)
            {
                  chest.CreatItem(itemCheck.name);
                DatabaseManager.MinusInventoryDict(itemCheck.name, 1);

            }

            if (itemCheck.nowStack <= 0)
            {
                Destroy(item.gameObject);

            }


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
    public void CloseDivide()
    {
        if(chest == null)
        {
            divideSlider.gameObject.SetActive(false);
            state = "";
        }
        else
        {
            divideSlider.gameObject.SetActive(false);
            state = "chestOpen";
        }
    }
    public void DownNowStack(int output)
    {

        GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        ItemCheck item = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
        item.nowStack -= output;

        CreatItemSelected(item.name,nowBox, output);
    }

    void ActiveChangeCursor()
    {


        if (state == "" || state == "detail" || state == "chestOpen")
        {
            beforBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            if(beforBox.transform.childCount != 0)
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
                if (afterItemCheck.name == beforeItemCheck.name && (afterItemCheck.nowStack != afterItemCheck.maxStack && beforeItemCheck.nowStack != beforeItemCheck.maxStack ))
                {
                    isSame = true; // 같다면 가능한 만큼 스택을 합친다.

                    if(afterItemCheck.nowStack + beforeItemCheck.nowStack <= afterItemCheck.maxStack)
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
            
            if(isSame == false)
            {
                if(beforBox.transform.childCount > 0)
                {
                    beforitem = beforBox.transform.GetChild(0).gameObject;
                    beforitem.transform.SetParent(afterBox.transform);
                    beforitem.transform.position = afterBox.transform.position;
                }

            }
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
            changeCusor.SetActive(false);
            if(chest != null)
            {
                state = "chestOpen";
            }
            else
            {
                state = "";
            }


        }
    }


    void ActiveFalse()
    {
        for(int i = 0; i < 5; i++)
        {
            cusorCount[i] = 0;
        }
        GameObject insPositon = GetNthChildGameObject(inventoryUI[0], cusorCount[0]);
        cusor.transform.position = insPositon.transform.position;
        inventory.SetActive(false);
        ResetInventoryBox();
        inventoryUI[0].transform.SetAsLastSibling();
        InventoryAlpha inventoryAlpha = inventoryUI[0].GetComponent<InventoryAlpha>();
        inventoryAlpha.A21();
    }

    void MakeInventoryBox()
    {
        
        for (int i = 0; i < maxBoxNum; i++)
        {
            if (i < (maxHor * maxVer))
            {
                if (maxBoxCusor != 1)
                {
                    maxBoxCusor = 1;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[0].transform);
            }
            else if (i < (maxHor * maxVer)*2)
            {
                if (maxBoxCusor != 2)
                {
                    maxBoxCusor = 2;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[1].transform);
            }
            else if (i < (maxHor * maxVer)*3)
            {
                if (maxBoxCusor != 3)
                {
                    maxBoxCusor = 3;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[2].transform);
            }
            else if (i < (maxHor * maxVer)*4)
            {
                if (maxBoxCusor != 4)
                {
                    maxBoxCusor = 4;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[3].transform);
            }
            else if (i < (maxHor * maxVer)*5)
            {
                if (maxBoxCusor != 5)
                {
                    maxBoxCusor = 5;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[4].transform);
            }

        }
    }
    void CloseCheck()
    {

        if (state == "Sell")
        {
            state = "detail";
            sellUIGameObject.SetActive(false);
        }
        else if(state == "BuyDetail")
        {
            ShopUI.CloseBuyUI();
        }
        else if (ShopUI.isBuyDetail == true)
        {
            state = "ShopCusorOn";
            ShopUI.state = "";
            ShopUI.cusor.SetActive(true);
            ShopUI.isMoveCusor = true;
            ShopUI.DetailOff();

        }
        
        else if(state == "detail")
        {
            if(chest == null)
            {
                state = "";
                DetailOff();
            }
            else
            {
                state = "chestOpen";
                DetailOff();
            }

        }
       else if(state == "Equipment")
        {
            OnlyDetailObOff();
        }
        else if(state == "change")
        {
            if (chest == null)
            {
                state = "";
                //  BoxContentChecker();
                DetailOff();
                cusorCount[nowBox] = beforeCusorInt;
                changeCusor.SetActive(false);
            }
            else
            {
                state = "chestOpen";
                DetailOff();
                cusorCount[nowBox] = beforeCusorInt;
                changeCusor.SetActive(false);
            }

        }
       else if (state == "divide")
        {
            CloseDivide();
        }
        else
        {
            inventory.SetActive(false);
            DatabaseManager.isOpenUI = false;

            if(ShopGameObject.activeSelf == true)
            {
                InventoryManager.instance.state = "";
                ShopUI.CloseShop();
            }
        }
    }
    public void ExCheckMove()
    {
        checkRepeat = true;
        sequence.Kill();

        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());

    }
    void BoxOpen()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)&& inventoryUI[0].activeSelf == false && inventoryBox[0].activeSelf == true)
        {
            OpenBox(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryUI[1].activeSelf == false && inventoryBox[1].activeSelf == true)
        {
            OpenBox(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryUI[2].activeSelf == false && inventoryBox[2].activeSelf == true)
        {
            OpenBox(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && inventoryUI[3].activeSelf == false && inventoryBox[3].activeSelf == true)
        {
            OpenBox(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && inventoryUI[4].activeSelf == false && inventoryBox[4].activeSelf == true)
        {
            OpenBox(4);
        }


    }

    void EquipmentCusorManage()
    {
        if(nowEquipBox == handBox)
        {
            if (leftInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = chestBox.gameObject;
                nowEquipBox = chestBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (downInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = shoesBox.gameObject;
                nowEquipBox = shoesBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (upInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = necklesBox.gameObject;
                nowEquipBox = necklesBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                nowEquipBox = null;
                state = "boxChange";
                boxCusor = 0;
                GameObject insPositon = inventoryBox[boxCusor];
                cusor.transform.position = insPositon.transform.position;
            }
        }
       else if (nowEquipBox == necklesBox)
        {
            if (leftInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = headBox.gameObject;
                nowEquipBox = headBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (downInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = handBox.gameObject;
                nowEquipBox = handBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                nowEquipBox = null;
                state = "boxChange";
                boxCusor = 0;
                GameObject insPositon = inventoryBox[boxCusor];
                cusor.transform.position = insPositon.transform.position;
            }
        }
        else if (nowEquipBox == shoesBox)
        {
            if (leftInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = legBox.gameObject;
                nowEquipBox = legBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (upInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = handBox.gameObject;
                nowEquipBox = handBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                nowEquipBox = null;
                state = "boxChange";
                boxCusor = 0;
                GameObject insPositon = inventoryBox[boxCusor];
                cusor.transform.position = insPositon.transform.position;
            }
        }
        else if (nowEquipBox == chestBox)
        {

             if (downInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = legBox.gameObject;
                nowEquipBox = legBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (upInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = headBox.gameObject;
                nowEquipBox = headBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = handBox.gameObject;
                nowEquipBox = handBox;
                cusor.transform.position = insPositon.transform.position;
            }
        }
        else if (nowEquipBox == legBox)
        {
            if (leftInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = weaponBox.gameObject;
                nowEquipBox = weaponBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (upInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = chestBox.gameObject;
                nowEquipBox = chestBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = shoesBox.gameObject;
                nowEquipBox = shoesBox;
                cusor.transform.position = insPositon.transform.position;
            }
        }
        else if (nowEquipBox == headBox)
        {
            if (leftInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = ringBox.gameObject;
                nowEquipBox = ringBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (downInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = chestBox.gameObject;
                nowEquipBox = chestBox;
                cusor.transform.position = insPositon.transform.position;
            }
            else if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = necklesBox.gameObject;
                nowEquipBox = necklesBox;
                cusor.transform.position = insPositon.transform.position;
            }
        }
        else if (nowEquipBox == ringBox)
        {
        if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = headBox.gameObject;
                nowEquipBox = headBox;
                cusor.transform.position = insPositon.transform.position;
            }

        }
        else if (nowEquipBox == weaponBox)
        {
            if (rightInventoryAction.triggered)
            {
                OnlyDetailObOff();
                GameObject insPositon = legBox.gameObject;
                nowEquipBox = legBox;
                cusor.transform.position = insPositon.transform.position;
            }

        }
    }
   public GameObject nowEquipBox;
    public GameObject ShopGameObject;
    public ShopManager ShopUI;
    void BoxChangeByKey()
    {
        if ((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1))
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
        if ((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1))
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
        if (rightInventoryAction.triggered && checkRepeat == false)
       {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
             if (state == "boxChange")
            {

                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }

             if (state == "boxChange")
            {
                if (chest != null )
                {
                    state = "chestOpen";
                }
                else
                {
                    state = "";
                }
            }
            else if (state == "itemBoxChange")
            {
                state = "change";
            }
        }
        if (leftInventoryAction.triggered && checkRepeat == false)
        {
            if (chest != null && state != "itemBoxChange")
            {
                cusor.SetActive(false);
                state = "InChestMove";
                chest.isCusorChest = true;
                chest.CusorMove2Chest();
            }
            else if(ShopGameObject.activeSelf == true)
            {
                Debug.Log("상점인");
                ShopUI.leftKeyCheck = true;
                cusor.SetActive(false);
                state = "ShopCusorOn";
                ShopUI.ChangeCusor();
            }
            else
            {

                changeCusor.SetActive(false);
                GameObject insPositon = handBox.gameObject;
                nowEquipBox = handBox;
                cusor.transform.position = insPositon.transform.position;
                state = "Equipment";
            }

        }
        if (selectAction.triggered&& state == "boxChange")
        {
            ResetInventoryBox();
            nowBox = boxCusor;
            OpenBox(nowBox);
            state = "";
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
        }
        if (changeAction.triggered && state == "itemBoxChange" && boxCusor!=nowBox)    
        {
            BoxChange(boxCusor);
            state = "";
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
        }
    }

    public string stick = "";
    float verticalInput;
    float horizontalInput;

    public void CusorChest2Inven()
    {
        cusor.SetActive(true);
        cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
        state = "boxChange";
        boxCusor = 0;
        GameObject insPositon = inventoryBox[boxCusor];
        cusor.transform.position = insPositon.transform.position;
    }
    

    GameObject SetEquipBox()
    {
        if(detail.equipArea == "Hand")
        {
            return handBox;
        }
        else if (detail.equipArea == "Chest")
        {
            return chestBox;
        }
        else if (detail.equipArea == "Weapon")
        {
            return weaponBox;
            /*
            if (nowEquipBox != null)
            {

                if (nowEquipBox.name == "Weapon")
                {
                    return weaponBox;
                }
                else
                {
                    Debug.Log("Null");
                    return null;
                }
            }
            else
            {
                return weaponBox;
            }
            */

        }  // 메인 무장이 없으면 메인 무장 먼저, 있으면 사이드 웨폰에다가 무기를 넣어야함
        else if (detail.equipArea == "Neckles")
        {
            return necklesBox;
        }
        else if (detail.equipArea == "Head")
        {
            return headBox;
        }
        else if (detail.equipArea == "Ring")
        {
            return ringBox;

        }
        else if (detail.equipArea == "Leg")
        {
            return legBox;
        }
        else if (detail.equipArea == "Shoes")
        {
            return shoesBox;
        }
        else
        {
            return null;
        }

    }
    public SkillCooldown skillCooldown;
    void UnUseEquipment()
    {
        GameObject equipBox = SetEquipBox();
        GameObject nowEquipItem = detail.gameObject;


        EquipBoxCheck equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();

        if(CheckBoxCanCreatAll() == true)
        {
            Sequence waitSequence = DOTween.Sequence()
            .AppendCallback(() => equipBoxCheck.isSetArray = false)
            .OnComplete(() => equipBoxCheck.DeletPrefab(detail.equipArea, false));

          //  skillCooldown.DeletLeftSkill();
            CreatItem(detail.name);
            //  DetechItem(detail.equipArea);

        }


        /*
        nowEquipItem.transform.SetParent(equipBox.transform);
        nowEquipItem.transform.position = equipBox.transform.position;
        equipBoxCheck.LoadPrefab(detail.name, detail.equipArea);
        equipBoxCheck.ActivePrefab(detail.equipArea);
        */
    }
    public EquipBoxCheck sideWeaponBoxCheck;
    void UseEquipment()
    {
        AttackManager attackManager = player.GetComponent<AttackManager>();

        GameObject equipBox = SetEquipBox();
        GameObject nowEquipItem = detail.gameObject;

      EquipBoxCheck equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();

        if(detail.equipArea != "Weapon")
        {
            if (equipBox.transform.childCount == 0) // 무기가 아니고 방어구라면
            {
                nowEquipItem.transform.SetParent(equipBox.transform);
                //     Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
            }
            else
            {
                // 아이템을 해체한는 부분
                DetechItem(detail.equipArea);
                equipBoxCheck.DeletPrefab(detail.equipArea);

                nowEquipItem.transform.SetParent(equipBox.transform);
                //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
                equipBoxCheck.ActivePrefab(detail.equipArea);
            }
        }
        else
        {
            skillCooldown.ResetCoolTime();
            if (attackManager.equipWeapon == null)
            {

                nowEquipItem.transform.SetParent(equipBox.transform);
                nowEquipItem.transform.position = equipBox.transform.position;
                 equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
              //  equipBoxCheck.EquipMainWeapon();
            }
            else
            {

                // 아이템을 해체한는 부분
                DetechItem(detail.equipArea);
                equipBoxCheck.DeletPrefab(detail.equipArea);

                nowEquipItem.transform.SetParent(equipBox.transform);
                //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
                equipBoxCheck.ActivePrefab(detail.equipArea);
            }


        }


    }
    public void DetechItem(string equipArea)
    {
        GameObject equipChangeBox = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        if (equipArea == "Weapon") // 이거도 부 무장도 빼질 수 있도록 변경해 주어야 함
        {
            GameObject moveItem = weaponBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
        else if (equipArea == "Head")
        {
            GameObject moveItem = headBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;

        }
        else if (equipArea == "Chest")  
        {
            GameObject moveItem = chestBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
        else if (equipArea == "Hand")
        {
            GameObject moveItem = handBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
        else if (equipArea == "Neckles")
        {
            GameObject moveItem = necklesBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
        else if (equipArea == "Ring")
        {
            GameObject moveItem = ringBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
        else if (equipArea == "Leg")
        {
            GameObject moveItem = legBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
        else if (equipArea == "Shoes")
        {
            GameObject moveItem = shoesBox.transform.GetChild(0).gameObject;
            moveItem.transform.SetParent(equipChangeBox.transform);
            moveItem.transform.position = equipChangeBox.transform.position;
        }
    }
    public int currentSellValue = 0; // 현재 가지고 있는 int 수
    public GameObject sellUIGameObject;
    int totalSellPrice;
    public TextMeshProUGUI sellText;
    public Slider sellSlider; // Unity Inspector에서 Slider를 할당
    int output;
    public void SellItemUI()
    {
        InitializeSlider();
        state = "Sell";
        sellUIGameObject.SetActive(true);
        totalSellPrice = (int)(sellSlider.value * detail.price);
        sellText.text = "Sell " + detail.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void OnSliderValueChanged(float value)
    {
        output = Mathf.RoundToInt(value);

    }
    void RightMove()
    {
        checkRepeat = true;
        sequence.Kill();
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        sellSlider.value += 1;
        totalSellPrice = (int)(sellSlider.value * detail.price);
        sellText.text = "Sell " + detail.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void LeftMove()
    {
        checkRepeat = true;
        sequence.Kill();
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        sellSlider.value -= 1;
        totalSellPrice = (int)(sellSlider.value * detail.price);
        sellText.text = "Sell " + detail.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }

    void InitializeSlider()
    {
        sellSlider.onValueChanged.AddListener(OnSliderValueChanged);
        sellSlider.maxValue = detail.nowStack;
        // 슬라이더의 최소값과 최대값 설정
        sellSlider.minValue = 1;
        // 슬라이더의 현재 값 설정
        sellSlider.value = detail.nowStack;
        output = 1;
    }
    void SellItem()
    {
        detail.nowStack -= (int)sellSlider.value;
        if (detail.nowStack == 0)
        {
            Destroy(detail.gameObject);
        }
        DatabaseManager.money += totalSellPrice;
        sellUIGameObject.SetActive(false);
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        Sequence seq = DOTween.Sequence()
.AppendInterval(waitTime)
.OnComplete(() => state = "");

  

    }
    public TextMeshProUGUI myMoney;
    private void Update()
    {

       


        verticalInput =(verticalCheck.ReadValue<float>());
        horizontalInput = (horizontalCheck.ReadValue<float>());
        if (inventory.activeSelf == true)
        {
            if(myMoney.text != DatabaseManager.money.ToString())
            {
                myMoney.text = DatabaseManager.money.ToString();
            }


            if (state == "Sell")
            {
                if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1) && sellSlider.value < sellSlider.maxValue)
                {
                    RightMove();
                }
                else if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1) && sellSlider.value > 1)
                {
                    LeftMove();
                };
                if (selectAction.triggered)
                {
                    SellItem();
                }


            }
            if (state == "Equipment")
            {
                EquipmentCusorManage();
                if (selectAction.triggered && nowEquipBox.transform.childCount != 0)
                {
                    if (equipDetail.activeSelf == true)
                    {
                        UnUseEquipment();
                        CloseCheck();
                    }
                    else
                    {
                        BoxContentChecker();
                        // CloseCheck();

                    }

                }
  
            }
            if (consumAction.triggered && state == "")
            {
                ActiveConsum();
            }
            if (state == "")
            {
                BoxOpen();

            }
            if (selectAction.triggered)
            {
                if (state == "" || state == "chestOpen")
                {
                    BoxContentChecker();


                }
                else if(state == "detail")
                {

                    if (equipDetail.activeSelf == true && chest == null && ShopGameObject.activeSelf == false)
                    {

                        UseEquipment();
                        CloseCheck();
                    }
                    else if (ShopGameObject.activeSelf == true)
                    {
                        // 판매 함수
                        SellItemUI();
                    }

                    else
                    {
                        CloseCheck();

                    }

                }
                if(state == "I2CMove")
                {
                    I2CMove();
                }
            }
            if (chestMoveAction.triggered)
            {
                if (state == "chestOpen")
                {
                    state = "I2CMove"; // 인벤토리에서 창고로 물건 이동
                    cusorImage.color = new Color(23f / 255f, 123f / 255f, 161f / 255f);

                }
                else if(state == "I2CMove")
                {
                    state = "chestOpen";
                    cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
                }
            }
        }
        if (backAction.triggered)
        {
            CloseCheck();
        }
        if((state =="" || state =="detail"|| state  == "chestOpen"|| state == "I2CMove") && inventory.activeSelf== true)
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
        if(state == "change")
        {
            ChangeCusorChecker();
        }
        if ((state == "boxChange" || state == "itemBoxChange") && inventory.activeSelf == true)
        {
            BoxChangeByKey();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {

            CreatItem("RatCheifDagger");
            //CreatItem("PoisonSword");
            //CreatItem("Wood Spear");
            // CreatItem("Leather Knuckles");
            //CreatItem("PickAxe");
            //
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            CreatItem("DoubleDagger");
            CreatItem("RustedSword");
            CreatItem("RustedSwordandShield");
            CreatItem("Leather Knuckles");
             CreatItem("Wood Spear");
            CreatItem("LeafStaff");
            CreatItem("LeafAdae");
            //  CreatItem("ScareSide");
            //CreatItem("ScareStaff");
            //CreatItem("SacreGreatSword");
            //CreatItem("RatCheifDagger");
            // CreatItem("PoisonSword");
            // CreatItem("DoubleDagger");

            //  CreatItem("Necklace");


        }
        if (Input.GetKeyDown(KeyCode.F4))

        {
            // CreatItem("Wood");

              CreatItem("ScareCap");
              CreatItem("ScareArmor");
              CreatItem("ScareLegArmor");
             CreatItem("ScareHandArmor");
             CreatItem("ScareShoes");

           // CreatItem("RatCap");
         //   CreatItem("RatArmor");
         //   CreatItem("RatLegArmor");
          //  CreatItem("RatHandArmor");
          //  CreatItem("RatShoes");

            //CreatItem("Potion");

        }
        if (openInventoryAction.triggered)
        {
            if (inventory.activeSelf == true && ShopGameObject.activeSelf == false)
            {
                DatabaseManager.isOpenUI = false;
                inventory.SetActive(false);
                CloseCheck();
                if (chest != null)
                {
                    chest.CloseChest();
                    chest = null;
                }
                state = "";
            }
            else if (inventory.activeSelf == false && DatabaseManager.isOpenUI == false)
            {
                DatabaseManager.isOpenUI = true;
                inventory.SetActive(true);
                cusor.SetActive(true);
                  instance.nowBox = 0;
                 instance.boxCusor = 0;
                instance.ResetBoxOrigin();
            }
        }
    }
    public bool checkRepeat= false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;
 

    public void ActiveConsum()
    {
        GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        if(gameObject.transform.childCount > 0)
        {
            ItemCheck item = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            if (item.type == "Consum")
            {
                item.ConsumItemActive();
            }
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

    void DoubleCheck()
    {
        DoubleCheckController = false;
    }

    void CusorChecker()
    {
        if (inventory.activeSelf == true && state != "change")
        {

            if ((rightInventoryAction.triggered  ) || (checkRepeat == false && horizontalInput == 1))
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
                if (cusorCount[nowBox] + 1 < nowBoxMax&&( cusorCount[nowBox] + 1) % maxHor != 0)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {/*
                    cusorCount[nowBox] -= nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                    */
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
                if (cusorCount[nowBox] % maxHor == 0)
                {
                    cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
                    state = "boxChange";
                    boxCusor = 0;
                    GameObject insPositon = inventoryBox[boxCusor];
                    cusor.transform.position = insPositon.transform.position;
                   
                }
                else
                {
                    cusorCount[nowBox] -= 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                  
                }


            }
             if ((downInventoryAction.triggered && DoubleCheckController == false) ||( checkRepeat == false && verticalInput == -1))
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
                 //   Debug.Log(cusorCount[nowBox]);
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                /*
                 else
                 {
                     cusorCount[nowBox] = cusorCount[nowBox] % maxHor;
                     GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                     cusor.transform.position = insPositon.transform.position;
                 }
                  */

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
                /*
               
                else
                {
                    while (cusorCount[nowBox] + maxHor < nowBoxMax)
                    {
                        cusorCount[nowBox] += maxHor;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                */

            }
        }
    }

    void ResetCheckRepeat()
    {
        checkRepeat = false;
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
                if (cusorCount[nowBox] + 1 < nowBoxMax && (cusorCount[nowBox] +1)% maxHor != 0)
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
                {
                    state = "itemBoxChange";
                    boxCusor = 0;
                    GameObject insPositon = inventoryBox[boxCusor];
                    changeCusor.transform.position = insPositon.transform.position;
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
                    Debug.Log(nowBoxMax);

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
            }
  
            }


        }
    }
   public ItemCheck detail;
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
                //Debug.Log(gameObject.transform.GetChild(0));
                detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }

            if (detail.type == "Misc")
            {
                Transform misc = miscDetail.gameObject.transform;
                misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
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
                // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);
            }
            if (detail.type == "Consum")
            {
                Transform consum = consumDetail.gameObject.transform;
                consum.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
                consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
                consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
                consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
                string[] effectString = detail.effectOb.Split();
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = effectString[0] + " : " + effectString[1] + detail.effectPow;
                if (ob != null)
                {
                    consumDetail.transform.localPosition = new Vector2(ob.transform.localPosition.x - detailX, detailY);
                }
                else
                {
                    consumDetail.transform.localPosition = new Vector2(cusor.transform.localPosition.x - detailX, detailY);
                }
                consumDetail.SetActive(true);
                // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);
            }
            if (detail.type == "Equip")
            {
                Transform equip = equipDetail.gameObject.transform;
                equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
                equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
                equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
                equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = detail.acqPath;
                if (ob != null)
                {
                    equipDetail.transform.localPosition = new Vector2(ob.transform.localPosition.x - detailX, detailY);
                }
                else
                {
                    equipDetail.transform.localPosition = new Vector2(cusor.transform.localPosition.x - detailX, detailY);
                }
                equipDetail.SetActive(true);
                // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);
            }
        }
        else if (state == "Equipment")
        {
            // state = "detail";

            if (ob != null)
            {
                detail = ob.transform.GetComponent<ItemCheck>();
            }
            else
            {
                GameObject gameObject = (nowEquipBox);
                //Debug.Log(gameObject.transform.GetChild(0));
                detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }

            Transform equip = equipDetail.gameObject.transform;
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = detail.acqPath;
            if (ob != null)
            {
                equipDetail.transform.localPosition = new Vector2(ob.transform.localPosition.x + equipDetailX, detailY);
            }
            else
            {
                equipDetail.transform.localPosition = new Vector2(cusor.transform.localPosition.x + equipDetailX, detailY);
            }
            equipDetail.SetActive(true);
            // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);

        }
    }
    public bool BoxFullCheck()
    {
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum > (j * (maxHor * maxVer)) + i)
                {
                    if (inventoryArray[i, j] == 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }





public void CreatItem(string itemName, bool isC2I = false)
    {
        if(CheckStack(itemName) == false)
        {
            bool isCreate = false;
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < (maxHor * maxVer); i++)
                {
                    if (maxBoxNum > (j * (maxHor * maxVer)) + i)
                    {
                        if (inventoryArray[i, j] == 0)
                        {
                            inventoryArray[i, j] = 1;
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);
                            DatabaseManager.PlusInventoryDict(itemName, 1);
                            isCreate = true;
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

        if (isC2I == true)
        {
            chest.itemCheck.nowStack -= 1;
        }

    }
    public void ExchangeItem(GameObject nowBoxOb, GameObject afterBoxOb)
    {


        GameObject changeItem = afterBoxOb.transform.GetChild(0).gameObject;
        Debug.Log(changeItem); 
        changeItem.transform.SetParent(nowBoxOb.transform);
        changeItem.transform.position = nowBoxOb.transform.position;


        GameObject beforitem = nowBoxOb.transform.GetChild(0).gameObject;
        Debug.Log(beforitem);
        beforitem.transform.SetParent(afterBoxOb.transform);
        beforitem.transform.position = afterBoxOb.transform.position;


        GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        cusor.transform.position = insPositon.transform.position;
        changeCusor.SetActive(false);
        state = "";
    }
    public void ChangeCusor(GameObject ob)
    {
        if(ShopGameObject.activeSelf == true)
        {
            ShopUI.cusor.SetActive(false);
        }
        cusorCount[nowBox] = ob.transform.GetSiblingIndex();
        cusor.transform.position = ob.transform.position;
        cusor.SetActive(true);
        if(chest != null)
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            chest.changeCusor.SetActive(false);
            chest.cusor.SetActive(false);
            chest.isCusorChest = false;
            state = "chestOpen";
        }
        else
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            state = "";
        }
        
    }
   public  void DeletItemByName(string itemName, int count = 1)
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
                        if (check.name == itemName && count >0)
                        {
                            if (check.nowStack>0)
                            {
                                check.nowStack -= 1;
                                count -= 1;
                                DatabaseManager.MinusInventoryDict(itemName, 1);
                            }

                        }
                        if (check.nowStack <= 0)
                        {
                            Destroy(item);
                            inventoryArray[i, j] = 0;
                        }
                    }

                }
            }
        }

  
    }
    public bool OnlyCheckStack(string itemName)
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
                                return true;
                            }

                        }
                    }

                }
            }
        }
        return false;
    }


    public bool CheckStack(string itemName)
    {


        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum > (j * (maxHor * maxVer)) + i)
                {
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                    if(insPositon.transform.childCount > 0)
                    {
                        GameObject item = insPositon.transform.GetChild(0).gameObject;
                        ItemCheck check = item.GetComponent<ItemCheck>();
                        if (check.name == itemName)
                        {
                            if (check.maxStack > check.nowStack)
                            {
                                check.nowStack += 1;
                                DatabaseManager.PlusInventoryDict(itemName, 1);
                                return true;
                            }

                        }
                    }

                }
            }
        }
        return false;
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
    public void TestDelet(int uiNum, int boxNum)
    {

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

    public void DetailOff()
    {
        if ((state == "chestOpen" || chest != null)&& state != "I2CMove")
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            state = "chestOpen";
        }
        else if (state != "chestOpen" && state != "I2CMove")
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            state = "";
        }


        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
    }
    void OnlyDetailObOff()
    {
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
    }

    public Chest chest;
    public void CheckNowChest(Chest input)
    {
        state = "chestOpen";
        chest = input;
    }


    public void ResetBoxOrigin()
    {
        GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        cusor.transform.position = insPositon.transform.position;
    }

    public void DragReset()
    {
        changeCusor.SetActive(false);
        state = "";
        miscDetail.SetActive(false);    
      consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        divideUI.SetActive(false);

    }


}
