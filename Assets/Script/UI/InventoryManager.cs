using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class InventoryManager : MonoBehaviour
{
    public bool deletInventory;
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
    //int detailX = 200, detailY = 12;
    public GameObject detailPos;
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
        cusorImage = cusor.GetComponent<Image>(); ;
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
        if (chest != null)
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
    public void SaveInventory()
    {
        SaveManager.instance.datas.money = DatabaseManager.money;
        string[,,] saveInven = new string[5, maxHor * maxVer, 4]; // 몇번째인벤, 칸, 이름
        for(int i =0; i < 5; i++)
        {
            int childCount = inventoryUI[i].transform.childCount;
            if(childCount != 0 )
            {
                for (int j = 0; j < childCount; j++)
                {
                    GameObject box = inventoryUI[i].transform.GetChild(j).gameObject;
                    Debug.Log(box.transform.childCount);
                    if (box.transform.childCount > 0 && box.transform.GetChild(0).GetComponent<ItemCheck>() != null)
                    {
                        ItemCheck item = box.transform.GetChild(0).GetComponent<ItemCheck>();
                        saveInven[i, j, 0] = item.name;
                        saveInven[i, j, 1] = item.nowStack.ToString();
                        saveInven[i, j, 2] = item.tear.ToString();
                        saveInven[i, j, 3] = item.upgrade.ToString();

                    }
                    else
                    {
                        saveInven[i, j, 0] = "non";
                    }

                }
            }

        }
        SaveManager.instance.datas.invenItem = saveInven;
        SaveManager.instance.DataSave();
    }
    public void LoadInventory()
    {
        if(SaveManager.instance.datas.invenItem == null)
        {
            SaveManager.instance.datas.money = DatabaseManager.money;
            SaveInventory();
        }
        else
        {
      DatabaseManager.money = SaveManager.instance.datas.money;
            LoadEquipItem();
            for (int i = 0; i < 5; i++)
            {
                int childCount = inventoryUI[i].transform.childCount;
                if (childCount != 0)
                {
                    for (int j = 0; j < childCount; j++)
                    {
                        // 현재에는 단순히 아이템만  이름만 확인해서 가져오지만, 여기에 수정해서 현재 스택 수치, 강화수치 이런것도 가져와야함
                        if (SaveManager.instance.datas.invenItem[i, j, 0] != "non" && SaveManager.instance.datas.invenItem[i, j, 0]!= null)
                        {
                            inventoryArray[j, i] = 1;
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[i], j);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(SaveManager.instance.datas.invenItem[i, j, 0]);
                            check.nowStack = int.Parse(SaveManager.instance.datas.invenItem[i, j, 1]);
                            check.tear = int.Parse(SaveManager.instance.datas.invenItem[i, j, 2]);
                            check.upgrade = int.Parse(SaveManager.instance.datas.invenItem[i, j, 3]);

                            DatabaseManager.PlusInventoryDict(SaveManager.instance.datas.invenItem[i, j, 0], check.nowStack);
                        }
                    
                    }
                }

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
        for (int j = 0; j < 5; j++)
        {
            int count = inventoryUI[j].transform.childCount;
            for (int i = 0; i < count; i++)
            {
                GameObject boxCheck = inventoryUI[j].transform.GetChild(i).gameObject;

                if (boxCheck.transform.childCount == 0)
                {
                    isCreat = true;

                    break;
                }

            }
            if (isCreat)
            {
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
        if (chest == null)
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

        CreatItemSelected(item.name, nowBox, output);
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
                if (beforBox.transform.childCount > 0)
                {
                    beforitem = beforBox.transform.GetChild(0).gameObject;
                    beforitem.transform.SetParent(afterBox.transform);
                    beforitem.transform.position = afterBox.transform.position;
                }

            }
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
            changeCusor.SetActive(false);
            if (chest != null)
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
        for (int i = 0; i < 5; i++)
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
        if(deletInventory == false)
        {
            LoadInventory();
        }
        else
        {

             SaveManager.instance.datas.headGear = new string[] { "", "",""};
            SaveManager.instance.datas.bodyGear = new string[] { "", "", "" };
            SaveManager.instance.datas.legGear = new string[] { "", "", "" };
            SaveManager.instance.datas.shoseGear = new string[] { "", "", "" };
            SaveManager.instance.datas.handGear = new string[] { "", "", "" };
            SaveManager.instance.datas.weaponGear = new string[] { "", "", "" };
            SaveManager.instance.datas.necklesGear = new string[] { "", "", "" };
            SaveManager.instance.datas.ringGear = new string[] { "", "", "" };
            SaveInventory();

        }

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
            else if (i < (maxHor * maxVer) * 2)
            {
                if (maxBoxCusor != 2)
                {
                    maxBoxCusor = 2;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[1].transform);
            }
            else if (i < (maxHor * maxVer) * 3)
            {
                if (maxBoxCusor != 3)
                {
                    maxBoxCusor = 3;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[2].transform);
            }
            else if (i < (maxHor * maxVer) * 4)
            {
                if (maxBoxCusor != 4)
                {
                    maxBoxCusor = 4;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[3].transform);
            }
            else if (i < (maxHor * maxVer) * 5)
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
        else if (state == "BuyDetail")
        {
            ShopUI.CloseBuyUI();
            state = "ShopCusorOn";
            ShopUI.state = "";
            ShopUI.cusor.SetActive(true);
            ShopUI.isMoveCusor = true;
            ShopUI.DetailOff();
        }
        else if (ShopUI.isBuyDetail == true)
        {
            state = "ShopCusorOn";
            ShopUI.state = "";
            ShopUI.cusor.SetActive(true);
            ShopUI.isMoveCusor = true;
            ShopUI.DetailOff();

        }

        else if (state == "detail")
        {
            if (chest == null)
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
        else if (state == "Equipment")
        {
            OnlyDetailObOff();
        }
        else if (state == "change")
        {
            if (chest == null)
            {
                state = "";
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
            SaveInventory();
            inventory.SetActive(false);
            DatabaseManager.isOpenUI = false;

            if (ShopGameObject.activeSelf == true)
            {
                InventoryManager.instance.state = "";
                ShopUI.CloseShop();
            }
            if (chest != null)
            {
                chest.CloseChest();
                chest = null;
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
        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryUI[0].activeSelf == false && inventoryBox[0].activeSelf == true)
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
        if (nowEquipBox == handBox)
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
                if (chest != null)
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
            else if (ShopGameObject.activeSelf == true)
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


    GameObject SetEquipBox(ItemCheck nowItem = null)
    {
        if(nowItem != null)
        {
            detail = nowItem;
        }

        if (detail.equipArea == "Hand")
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


        }  // 메인 무장이 없으면 메인 무장 먼저, 있으면 사이드 웨폰에다가 무기를 넣어야함
       
        else if (detail.equipArea == "Necklace")
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
    void UnUseEquipment(ItemCheck nowItem = null)
    {
        GameObject equipBox;
        if (nowItem != null)
        {
            detail = nowItem;
            equipBox = SetEquipBox(nowItem);
        }
        else
        {
            equipBox = SetEquipBox();
        }
         equipBox = SetEquipBox();
        GameObject nowEquipItem = detail.gameObject;


        EquipBoxCheck equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();

        if (CheckBoxCanCreatAll() == true)
        {
            Sequence waitSequence = DOTween.Sequence()
          //  .AppendCallback(() => equipBoxCheck.isSetArray = false)
            .OnComplete(() => equipBoxCheck.DeletPrefab(detail, detail.equipArea, false));
            equipBoxCheck.SaveEquipItem(detail, false);
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

    void LoadEquipItem()
    {

        if(SaveManager.instance.datas.headGear != null)
        {
            if (SaveManager.instance.datas.headGear[0] != "")
            {

                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck head = equip.GetComponent<ItemCheck>();
                head.SetItem(SaveManager.instance.datas.headGear[0]);
                head.tear = int.Parse(SaveManager.instance.datas.headGear[1]);
                head.upgrade = int.Parse(SaveManager.instance.datas.headGear[2]);
                UseEquipment(head);
            }
        }

        if (SaveManager.instance.datas.bodyGear != null)
        {
            if (SaveManager.instance.datas.bodyGear[0] != "")
            {


                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck body = equip.GetComponent<ItemCheck>();
                body.SetItem(SaveManager.instance.datas.bodyGear[0]);
                body.tear = int.Parse(SaveManager.instance.datas.bodyGear[1]);
                body.upgrade = int.Parse(SaveManager.instance.datas.bodyGear[2]);
                UseEquipment(body);
            }

        }
        if (SaveManager.instance.datas.legGear != null)
        {
            if (SaveManager.instance.datas.legGear[0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck leg = equip.GetComponent<ItemCheck>();
                leg.SetItem(SaveManager.instance.datas.legGear[0]);
                leg.tear = int.Parse(SaveManager.instance.datas.legGear[1]);
                leg.upgrade = int.Parse(SaveManager.instance.datas.legGear[2]);
                UseEquipment(leg);
            }
        }
        if (SaveManager.instance.datas.shoseGear != null)
        {
            if (SaveManager.instance.datas.shoseGear[0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck shose = equip.GetComponent<ItemCheck>();
                shose.SetItem(SaveManager.instance.datas.shoseGear[0]);
                shose.tear = int.Parse(SaveManager.instance.datas.shoseGear[1]);
                shose.upgrade = int.Parse(SaveManager.instance.datas.shoseGear[2]);
                UseEquipment(shose);
            }
        }
        if (SaveManager.instance.datas.handGear != null)
        {
            if (SaveManager.instance.datas.handGear[0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck hand = equip.GetComponent<ItemCheck>();
                hand.SetItem(SaveManager.instance.datas.handGear[0]);
                hand.tear = int.Parse(SaveManager.instance.datas.handGear[1]);
                hand.upgrade = int.Parse(SaveManager.instance.datas.handGear[2]);
                UseEquipment(hand);
            }
        }
        if (SaveManager.instance.datas.weaponGear != null)
        {
            if (SaveManager.instance.datas.weaponGear[0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck weapon = equip.GetComponent<ItemCheck>();
                weapon.SetItem(SaveManager.instance.datas.weaponGear[0]);
                weapon.tear = int.Parse(SaveManager.instance.datas.weaponGear[1]);
                weapon.upgrade = int.Parse(SaveManager.instance.datas.weaponGear[2]);
                UseEquipment(weapon);
            }
        }
        if (SaveManager.instance.datas.ringGear != null)
        {
            if (SaveManager.instance.datas.ringGear[0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck ring = equip.GetComponent<ItemCheck>();
                ring.SetItem(SaveManager.instance.datas.ringGear[0]);
                ring.tear = int.Parse(SaveManager.instance.datas.ringGear[1]);
                ring.upgrade = int.Parse(SaveManager.instance.datas.ringGear[2]);
                UseEquipment(ring);
            }
        }
        if (SaveManager.instance.datas.necklesGear != null)
        {
            if (SaveManager.instance.datas.necklesGear[0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck necklace = equip.GetComponent<ItemCheck>();
                necklace.SetItem(SaveManager.instance.datas.necklesGear[0]);
                necklace.tear = int.Parse(SaveManager.instance.datas.necklesGear[1]);
                necklace.upgrade = int.Parse(SaveManager.instance.datas.necklesGear[2]);
                UseEquipment(necklace);
            }
        }

        Invoke("ResetInventory", 0.4f);
    }

    void ResetInventory()
    {
        sequence = DOTween.Sequence()
        .AppendCallback(() => inventory.SetActive(true))
         .AppendInterval(waitTime)
         .AppendCallback(() => inventory.SetActive(false));

        // 여기가 이제 로딩 끝나고 카메라가 밝아지는시점.

    }

    void UseEquipment(ItemCheck nowItem = null)
    {
        GameObject equipBox;
        if (nowItem != null)
        {
            detail = nowItem;
             equipBox = SetEquipBox(nowItem);
        }
        else
        {
             equipBox = SetEquipBox();
        }
    
        AttackManager attackManager = player.GetComponent<AttackManager>();
        GameObject nowEquipItem = detail.gameObject;

        EquipBoxCheck equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();

        if (detail.equipArea != "Weapon")
        {
            if (equipBox.transform.childCount == 0) // 무기가 아니고 방어구라면
            {
                nowEquipItem.transform.SetParent(equipBox.transform);
                //     Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
                equipBoxCheck.SaveEquipItem(detail, true);
            }
            else
            {
                // 아이템을 해체한는 부분
                DetechItem(detail.equipArea);
                equipBoxCheck.DeletPrefab(detail, detail.equipArea);

                nowEquipItem.transform.SetParent(equipBox.transform);
                //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
                equipBoxCheck.ActivePrefab(detail.equipArea);
                equipBoxCheck.SaveEquipItem(detail, true);
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
                equipBoxCheck.SaveEquipItem(detail, true);
                //  equipBoxCheck.EquipMainWeapon();
            }
            else
            {

                // 아이템을 해체한는 부분
                DetechItem(detail.equipArea);
                equipBoxCheck.DeletPrefab(detail, detail.equipArea);
                nowEquipItem.transform.SetParent(equipBox.transform);
                //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.equipArea, detail.tfName);
                equipBoxCheck.ActivePrefab(detail.equipArea);
                equipBoxCheck.SaveEquipItem(detail, true);
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
        DatabaseManager.MinusInventoryDict(detail.name, (int)sellSlider.value);
        if (detail.nowStack == 0)
        {
            Destroy(detail.gameObject);
        }
        DatabaseManager.money += totalSellPrice;
        sellUIGameObject.SetActive(false);
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        skillDetailUi.SetActive(false);
        Sequence seq = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => state = "");

        SaveInventory();


    }
    public TextMeshProUGUI myMoney;
    private void Update()
    {

        verticalInput = (verticalCheck.ReadValue<float>());
        horizontalInput = (horizontalCheck.ReadValue<float>());
        if (inventory.activeSelf == true)
        {
            if (myMoney.text != DatabaseManager.money.ToString())
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
            if (selectAction.triggered && state == "detail" &&detail.type == "Consum" && ShopGameObject.activeSelf == false)
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
                else if (state == "detail")
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
                if (state == "I2CMove")
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
                else if (state == "I2CMove")
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
        if ((state == "" || state == "detail" || state == "chestOpen" || state == "I2CMove") && inventory.activeSelf == true)
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

        if (Input.GetKeyDown(KeyCode.F3))
        {
            //   CreatItem("LeafAdae");
            //   CreatItem("LeafStaff");
            //  CreatItem("LeafLegArmor"); CreatItem("LeafHandArmor"); CreatItem("LeafShoes"); CreatItem("LeafArmor"); CreatItem("LeafCap");
            //CreatItem("Stinky Rat Meat Stew");
            CreatItem("Ominous Black Liquid"); CreatItem("Wood"); CreatItem("T0_Ore");
            CreatItem("Blackened Branch"); CreatItem("Eerie Cloth Scrap");
            //
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
          //  CreatItem("ScareSide");
            CreatItem("Cap"); CreatItem("ScareSide");
            //   CreatItem("ScareClow");
            //   CreatItem("ScareStaff");
            //   CreatItem("SacreGreatSword");
            //  CreatItem("RatCheifDagger");
            //  CreatItem("DoubleDagger");
            //  CreatItem("RustedSword");
            //  CreatItem("RustedSwordandShield");
            // CreatItem("Leather Knuckles");
            //  CreatItem("Wood Spear");
            //  CreatItem("LeafStaff");
            //  CreatItem("LeafAdae");




        }
        if (Input.GetKeyDown(KeyCode.F4))

        {
            CreatItem("ScareSide");
            CreatItem("Four-Leaf Clover Ring"); CreatItem("Rabbit's Foot");
            CreatItem("Cap");
            CreatItem("Armor");
            CreatItem("LegArmor");
            CreatItem("HandArmor");
            CreatItem("Shoes");

            //CreatItem("RatCap");
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
                SaveInventory();
                inventory.SetActive(false);
                CloseCheck();

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
    public bool checkRepeat = false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;


    public void ActiveConsum()
    {

        GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        if (gameObject.transform.childCount > 0)
        {
            ItemCheck item = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            if (item.type == "Consum")
            {
                item.ConsumItemActive();
                DatabaseManager.MinusInventoryDict(item.name,1);
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
                if (cusorCount[nowBox] + 1 < nowBoxMax && (cusorCount[nowBox] + 1) % maxHor != 0)
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
                if (cusorCount[nowBox] + 1 < nowBoxMax && (cusorCount[nowBox] + 1) % maxHor != 0)
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
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.tear.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.rarity);
                miscDetail.transform.position = detailPos.transform.position;



                miscDetail.SetActive(true);

            }
            if (detail.type == "Consum")
            {
                Transform consum = consumDetail.gameObject.transform;
                consum.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
                consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
                consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.price).ToString();
                consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.tear.ToString();
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.rarity);
                SetConsumEffect(consum.GetChild(7).gameObject, detail);
   

                consum.transform.position = detailPos.transform.position;
                consumDetail.SetActive(true);

            }
            if (detail.type == "Equip")
            {
                Transform equip = equipDetail.gameObject.transform;

                string folderPath = detail.equipArea + "/";

                // 리소스 폴더 내의 equipName을 로드합니다.
                GameObject prefab = Resources.Load<GameObject>(folderPath + detail.name);
                equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
                equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type + " : " + detail.equipArea;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.price).ToString();
                equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.tear.ToString();
                equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.rarity);
                SetEffectDetail(equip.GetChild(8).gameObject, detail);
                if (detail.equipArea != "Weapon") // 방어구라면
                {
                    Equipment equipment = prefab.GetComponent<Equipment>();
                    SetArmorDetail(equip.GetChild(7).gameObject, equipment);
                }
                else // 무기인 경우
                {
                    Weapon weapon = prefab.GetComponent<Weapon>();
                    SetWeaponDetail(equip.GetChild(7).gameObject, weapon);
                    CheckSkillDetail(weapon);
                }


                equip.transform.position = detailPos.transform.position;
                equipDetail.SetActive(true);

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
            string folderPath = detail.equipArea + "/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + detail.name);
            Transform equip = equipDetail.gameObject.transform;
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type + " : " + detail.equipArea;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.rarity);
            SetEffectDetail(equip.GetChild(8).gameObject, detail);
            if (detail.equipArea != "Weapon") // 방어구라면
            {
                Equipment equipment = prefab.GetComponent<Equipment>();
                SetArmorDetail(equip.GetChild(7).gameObject, equipment);
            }
            else // 무기인 경우
            {
                Weapon weapon = prefab.GetComponent<Weapon>();
                SetWeaponDetail(equip.GetChild(7).gameObject, weapon);
                CheckSkillDetail(weapon);
            }


            equip.transform.position = detailPos.transform.position;
            equipDetail.SetActive(true);

            equip.transform.position = detailPos.transform.position;
            equipDetail.SetActive(true);
            // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);

        }
    }

    public GameObject skillDetailUi;
    public void CheckSkillDetail(Weapon weapon) // 기술 체크.
    {
        skillDetailUi.SetActive(true);
        for (int i =0; i < 4; i++) // 기술 갯수가 늘어나면 여기 변경해 주어야함.
        {
            GameObject skillSlot = skillDetailUi.transform.GetChild(i).gameObject;
            if (weapon.skill[i] != null)
            {
                skillSlot.SetActive(true);
                Skill skill = weapon.skill[i];


                skillSlot.transform.GetChild(0).GetComponent<Image>().sprite = skill.skillImage;
                skillSlot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = skill.skillName;
                skillSlot.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = skill.useStemina.ToString();
                skillSlot.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = skill.SkillCoolTime.ToString();
                skillSlot.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = skill.skillDetail;

                //  equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type + " : " + detail.equipArea;
                // equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            }
            else
            {
                skillSlot.SetActive(false);
            }
        }
    }

    public void SetEffectDetail(GameObject textObject, ItemCheck nowItem)
    {
        // 방어구의 세트효과
        string allSetStr = "";
        if (nowItem.setName != "") allSetStr = "< " + nowItem.setName + " >\n";
        SetItem setItem = new SetItem();
        for (int i = 0; i < 8; i++)
        {
            string effecStr = setItem.SetEffectStr(nowItem.tfName, i);
            if (effecStr != "")
            {
                string noParentheses = effecStr.Replace("(", "").Replace(")", " ");
                string[] box = noParentheses.Split("/");
                allSetStr += "(" + i + ") ";
                for (int k =0; k < box.Length; k++)
                {
                    // Step 1: Remove the parentheses
                    string[] checkEffect = box[k].Split();
                    if (checkEffect[2] == "skillHitCount")
                    {
                        allSetStr +=  checkEffect[0] + checkEffect[1] + " " + "["+checkEffect[3].Replace("_", " ") +"]"+ " count";
    
                    }
                    else if (checkEffect[2] == "bulletCount")
                    {
                        allSetStr += checkEffect[0] + checkEffect[1] + " " + "[" + checkEffect[3].Replace("_", " ") + "]" + " count";
                    }
                    else if (checkEffect[2] == "coolDown")
                    {
                        allSetStr += "-" + checkEffect[1] + "% CoolDown " + "[" + checkEffect[3].Replace("_", " ") + "]" ;
                    }
                    else
                    {
                        allSetStr +=  checkEffect[0]+" "+ checkEffect[1];
                    }

                    if (k != box.Length - 1)
                    {
                        allSetStr += ",\n";
                    }
                    else
                    {
                        allSetStr += "\n";
                    }


                }

            }
        }

        textObject.GetComponent<TextMeshProUGUI>().text = allSetStr;
    }
    public void SetConsumEffect(GameObject textObject, ItemCheck nowItem)
    {
        string allSetStr = "";
        string[] effect = nowItem.effectOb.Split("/");
        string[] effectPower = nowItem.effectPow.Split("/");

        for (int i = 0; i < effect.Length; i++)
        {
            string[] effectStr = effect[i].Split();
            string[] effectPowerDetail = effectPower[i].Split("_");


            if(effectPowerDetail.Length < 2)
            {
                allSetStr += effectStr[1] + CapitalizeFirstLetter(effectStr[0]) + " " + effectPowerDetail[0] + "\n";
            }
            else // 2번째 있다면 그건 시간
            {
                allSetStr += effectStr[1] + CapitalizeFirstLetter(effectStr[0]) + " " + effectPowerDetail[0] + " : "+ effectPowerDetail[1]+" Sec" + "\n";
            }

        }
        textObject.GetComponent<TextMeshProUGUI>().text = allSetStr;
    }
    public void SetWeaponDetail(GameObject textObject, Weapon weapon)
    {
        // 방어구의 기본적인 스탯수치
        string basicStr = "";
        basicStr += weapon.minDmg + "~" + weapon.maxDmg + " Dmg\n";


        if (weapon.critPer != 0)
        {

            if (weapon.critPer > 0) basicStr += "+" + weapon.critPer.ToString() + " Critical Chance\n";
            else basicStr += "-" + weapon.critPer.ToString() + " Critical Chance\n";
        }
        if (weapon.critDmg != 0)
        {

            if (weapon.critDmg > 0) basicStr += "+" + weapon.critDmg.ToString() + " Critical Dmg\n";
            else basicStr += "-" + weapon.critDmg.ToString() + " Critical Dmg\n";
        }
        if (weapon.incDmg != 0)
        {

            if (weapon.incDmg > 0) basicStr += "+" + weapon.incDmg.ToString() + " Dmg Increase\n";
            else basicStr += "-" + weapon.incDmg.ToString() + " Dmg Decrease\n";
        }
        if (weapon.ignDef != 0)
        {

            if (weapon.ignDef > 0) basicStr += "+" + weapon.ignDef.ToString() + " Armor Penetration\n";
            else basicStr += "-" + weapon.ignDef.ToString() + " Armor Penetration\n";
        }
        if (weapon.skillDmg != 0)
        {

            if (weapon.skillDmg > 0) basicStr += "+" + weapon.skillDmg.ToString() + " Skill Dmg\n";
            else basicStr += "-" + weapon.skillDmg.ToString() + " Skill Dmg\n";
        }
        if (weapon.addDmg != 0)
        {

            if (weapon.addDmg > 0) basicStr += "+" + weapon.addDmg.ToString() + " Additional Dmg\n";
            else basicStr += "-" + weapon.addDmg.ToString() + " Additional Dmg\n";
        }
        textObject.GetComponent<TextMeshProUGUI>().text = basicStr;
    }
    public static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // 첫 번째 문자를 대문자로 바꾸고 나머지 부분은 그대로 둡니다.
        return char.ToUpper(input[0]) + input.Substring(1);
    }
    public void SetArmorDetail(GameObject textObject, Equipment equipment)
    {
        // 방어구의 기본적인 스탯수치
        string basicStr = "";
        if (equipment.basicDmg != 0)
        {
            if (equipment.basicDmg > 0) basicStr += "+" + equipment.basicDmg.ToString() + " Basic Dmg\n";
            else basicStr += "-" + equipment.basicDmg.ToString() + " Basic Dmg\n";
        }
        if (equipment.armor != 0)
        {
            if (equipment.armor > 0) basicStr += "+" + equipment.armor.ToString() + " Def\n";
            else basicStr += "-" + equipment.armor.ToString() + " Def\n";
        }
        if (equipment.hp != 0)
        {
            if (equipment.hp > 0) basicStr += "+" + equipment.hp.ToString() + " Hp\n";
            else basicStr += "-" + equipment.hp.ToString() + " Hp\n";
        }
        if (equipment.critical != 0)
        {
            if (equipment.critical > 0) basicStr += "+" + equipment.critical.ToString() + " Critical Rate\n";
            else basicStr += "-" + equipment.critical.ToString() + " Critical Rate\n";
        }
        if (equipment.dropRate != 0)
        {
            if (equipment.dropRate > 0) basicStr += "+" + equipment.dropRate.ToString() + " Drop Rate\n";
            else basicStr += "-" + equipment.dropRate.ToString() + " Drop Rate\n";
        }
        if (equipment.moveSpeed != 0)
        {
            if (equipment.moveSpeed > 0) basicStr += "+" + equipment.moveSpeed.ToString() + " Movement Speed\n";
            else basicStr += "-" + equipment.moveSpeed.ToString() + " Movement Speed\n";
        }
        if (equipment.attSpeed != 0)
        {
            if (equipment.attSpeed > 0) basicStr += "+" + equipment.attSpeed.ToString() + " Attack Speed\n";
            else basicStr += "-" + equipment.attSpeed.ToString() + " Attack Speed\n";
        }
        if (equipment.criticalDmg != 0)
        {
            if (equipment.criticalDmg > 0) basicStr += "+" + equipment.criticalDmg.ToString() + " Critical Dmg\n";
            else basicStr += "-" + equipment.criticalDmg.ToString() + " Critical Dmg\n";
        }
        if (equipment.coolDownSkill.Length != 0)
        {
            for(int i =0; i < equipment.coolDownSkill.Length; i++)
            {
                basicStr += "-" + equipment.coolDownSkill[i].coolDownCount.ToString() + "% Cooldown " + equipment.coolDownSkill[i].skillName;
        }
        }
        if (equipment.incDmg != 0)
        {
            if (equipment.incDmg > 0) basicStr += "+" + equipment.incDmg.ToString() + "% Dmg\n";
            else basicStr += "-" + equipment.incDmg.ToString() + "% Dmg\n";
        }
        if (equipment.addIncomingDmg != 0)
        {
            if (equipment.addIncomingDmg > 0) basicStr += "+" + equipment.addIncomingDmg.ToString() + "% Incoming Dmg\n";
            else basicStr += "-" + equipment.addIncomingDmg.ToString() + "% Incoming Dmg\n";
        }
        if (equipment.isBleeding == true)
        {
            basicStr += "+"+equipment.bleedingPerCent + "% Chance to Cause Bleeding\n";
        }
        if (equipment.bleedingDmgPer != 0)
        {
            basicStr += "+" + equipment.bleedingDmgPer + "% Dmg to Bleeding Enemies\n";
        }
        if (equipment.poisonDmg != 0)
        {
            basicStr += "+" + equipment.poisonDmg + " Posion Dmg\n";
        }
        textObject.GetComponent<TextMeshProUGUI>().text = basicStr;
    }
    public string SetRarity(string rarerity)
    {
        switch (rarerity)
        {
            case ("C"):
                return "Common";
                break;
        
        }

        return "";
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
        SaveInventory();
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

        skillDetailUi.SetActive(false);
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
    }
    void OnlyDetailObOff()
    {
        skillDetailUi.SetActive(false);
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
        changeCusor.SetActive(false); skillDetailUi.SetActive(false);
        state = "";
        miscDetail.SetActive(false);    
      consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        divideUI.SetActive(false);

    }


}
