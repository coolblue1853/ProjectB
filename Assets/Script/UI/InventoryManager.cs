using System.Text;
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

    public GameObject[] equipBox; // 1: 반지, 2: 모자, 3: 목걸이, 5: 갑옷, 6: 장갑, 7: 무기, 8:바지, 9:신발
    public ItemCheck[] equipItemCheck = new ItemCheck[10];

    // 나중에 퍼블릭 해체;
    public GameObject beforBox;
    public GameObject afterBox;
    int beforeCusorInt;

    int equipCusor; // 장비창 커서 번호

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
        cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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
        // Phase 2 브리지: UI 슬롯 → 모델 → 세이브 배열 (SaveMapper)
        string[,,] saveInven = Service.BuildSaveArray(); // 열린 칸 밖 아이템도 보존
        SaveManager.instance.datas.invenItem = saveInven;
        SaveManager.instance.DataSave();
    }
    public void LoadInventory()
    {
        if (SaveManager.instance.datas.invenItem == null)
        {
            SaveManager.instance.datas.money = DatabaseManager.money;
            SaveInventory();
        }
        else
        {
            DatabaseManager.money = SaveManager.instance.datas.money;
            LoadEquipItem();
            Service.LoadFrom(SaveManager.instance.datas.invenItem);
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
                if (CreatItemSelected(itemCheck.name, siblingParentIndex, itemCheck.nowStack, itemCheck.itemData))
                {
                    Destroy(item.gameObject);
                }
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

            int count = itemCheck.nowStack;
            for (int i = 0; i < count; i++)
            {
                int before = itemCheck.nowStack;
                chest.CreatItem(itemCheck.name, itemCheck.itemData); // 성공하면 itemCheck.nowStack이 1 줄어든다
                if (itemCheck.nowStack == before)
                {
                    break; // 창고가 가득 참
                }
            }
            if (itemCheck.nowStack <= 0)
            {
                Destroy(item.gameObject);
            }
            // 양쪽을 함께 저장 (한쪽만 저장된 상태로 종료되면 복제·소실, B-015)
            SaveInventory();
            chest.SaveChest();
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
        // 새 칸을 만든 경우에만 원본을 줄인다 (빈칸이 없으면 나눈 수량이 사라지던 문제, B-009)
        if (output > 0 && output < item.nowStack && CreatItemSelected(item.name, nowBox, output, item.itemData))
        {
            item.nowStack -= output;
        }
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
                if (afterItemCheck.name == beforeItemCheck.name && (afterItemCheck.nowStack != afterItemCheck.itemData.maxStack && beforeItemCheck.nowStack != beforeItemCheck.itemData.maxStack))
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
        if (deletInventory == false)
        {
            LoadInventory();
        }
        else
        {
            for (int i = 1; i < equipItemCheck.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    SaveManager.instance.datas.equipGear[i, j] = "";
                }
            }
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
            UiLock.Set("InvGroup", false);
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

    void MoveCusor2Inventory() // 장비창 => 인벤토리로 커서를 옮기는 함수
    {
        OnlyDetailObOff();
        nowEquipBox = null;
        state = "boxChange";
        boxCusor = 0;
        GameObject insPositon = inventoryBox[boxCusor];
        cusor.transform.position = insPositon.transform.position;
    }
    void ChangeEquipCusor(int cusorNum)
    {
        OnlyDetailObOff();
        nowEquipBox = equipBox[cusorNum];
        cusor.transform.position = nowEquipBox.transform.position;
    }
    void EquipmentCusorManage()
    {
        if (leftInventoryAction.triggered && (equipCusor != 5 && equipCusor % 3 != 1))
        {
            equipCusor -= 1;
            ChangeEquipCusor(equipCusor);
        }
        else if (rightInventoryAction.triggered)
        {
            if (equipCusor % 3 != 0) equipCusor += 1;
            else MoveCusor2Inventory();
            ChangeEquipCusor(equipCusor);
        }
        else if (upInventoryAction.triggered)
        {
            if (equipCusor == 7) equipCusor = 5;
            else if (equipCusor > 3) equipCusor -= 3;
            ChangeEquipCusor(equipCusor);
        }
        else if (downInventoryAction.triggered)
        {
            if (equipCusor == 1) equipCusor = 5;
            else if (equipCusor < 7) equipCusor += 3;
            ChangeEquipCusor(equipCusor);
        }
    }
    public GameObject nowEquipBox;
    public GameObject ShopGameObject;
    public ShopManager ShopUI;

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
        if ((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1))
        {
            CusorContinuousInputCheck();
            if (0 < boxCusor) boxCusor -= 1;
            else boxCusor = maxBoxCusor - 1;
            GameObject insPositon = inventoryBox[boxCusor];

            if (state == "boxChange") cusor.transform.position = insPositon.transform.position;
            else if (state == "itemBoxChange") changeCusor.transform.position = insPositon.transform.position;
        }
        if ((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1))
        {
            CusorContinuousInputCheck();

            if (maxBoxCusor - 1 > boxCusor) boxCusor += 1;
            else boxCusor = 0;
            GameObject insPositon = inventoryBox[boxCusor];

            if (state == "boxChange") cusor.transform.position = insPositon.transform.position;
            else if (state == "itemBoxChange") changeCusor.transform.position = insPositon.transform.position;
        }
        if (rightInventoryAction.triggered && checkRepeat == false)
        {
            CusorContinuousInputCheck();
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);

            if (state == "boxChange") cusor.transform.position = insPositon.transform.position;
            else if (state == "itemBoxChange") changeCusor.transform.position = insPositon.transform.position;

            if (state == "boxChange")
            {
                if (chest != null) state = "chestOpen";
                else state = "";
            }
            else if (state == "itemBoxChange") state = "change";
        }
        if (leftInventoryAction.triggered && checkRepeat == false)
        {
            if (chest != null && state != "itemBoxChange") //창고 접촉
            {
                cusor.SetActive(false);
                state = "InChestMove";
                chest.isCusorChest = true;
                chest.CusorMove2Chest();
            }
            else if (ShopGameObject.activeSelf == true) // 상점 접촉
            {
                ShopUI.leftKeyCheck = true;
                cusor.SetActive(false);
                state = "ShopCusorOn";
                ShopUI.ChangeCusor();
            }
            else // 그외 장비창 접촉
            {
                changeCusor.SetActive(false);
                equipCusor = 6;
                nowEquipBox = equipBox[equipCusor];
                cusor.transform.position = nowEquipBox.transform.position;
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
        cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        state = "boxChange";
        boxCusor = 0;
        GameObject insPositon = inventoryBox[boxCusor];
        cusor.transform.position = insPositon.transform.position;
    }


    GameObject SetEquipBox(ItemCheck nowItem = null)
    {
        if (nowItem != null) detail = nowItem;

        if (detail.itemData.equipArea == "Ring") return equipBox[1];
        else if (detail.itemData.equipArea == "Head") return equipBox[2];
        else if (detail.itemData.equipArea == "Necklace") return equipBox[3];
        else if (detail.itemData.equipArea == "Chest") return equipBox[5];
        else if (detail.itemData.equipArea == "Hand") return equipBox[6];
        else if (detail.itemData.equipArea == "Weapon") return equipBox[7];
        else if (detail.itemData.equipArea == "Leg") return equipBox[8];
        else if (detail.itemData.equipArea == "Shoes") return equipBox[9];
        else return null;
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
            .OnComplete(() => equipBoxCheck.DeletPrefab(detail, detail.itemData.equipArea, false));
            equipBoxCheck.SaveEquipItem(detail, false);
            CreatItem(detail.name, false, detail.itemData.Clone()); // 해제 시 강화 유지 (B-014)
        }
    }
    void LoadEquipItem()
    {
        for (int i = 1; i < equipItemCheck.Length; i++)
        {
            if (SaveManager.instance.datas.equipGear[i, 0] != null && SaveManager.instance.datas.equipGear[i, 0] != "")
            {
                GameObject equip = Instantiate(itemPrefab, this.transform.position, Quaternion.identity, this.transform);
                ItemCheck item = equip.GetComponent<ItemCheck>();
                item.SetItem(SaveManager.instance.datas.equipGear[i, 0]);
                item.itemData.tear = int.Parse(SaveManager.instance.datas.equipGear[i, 1]);
                item.itemData.upgrade = int.Parse(SaveManager.instance.datas.equipGear[i, 2]);
                UseEquipment(item);
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

        // 여기가 로딩 끝나고 카메라가 밝아지는시점.

    }
    public ItemCheck CheckNowEquipment(string equipArea)
    {
        switch (equipArea)
        {
            case ("Ring"):
                return equipItemCheck[1];
            case ("Head"):
                return equipItemCheck[2];
            case ("Necklace"):
                return equipItemCheck[3];
            case ("Chest"):
                return equipItemCheck[5];
            case ("Hand"):
                return equipItemCheck[6];
            case ("Weapon"):
                return equipItemCheck[7];
            case ("Leg"):
                return equipItemCheck[8];
            case ("Shoes"):
                return equipItemCheck[9];
        }
        return null;
    }
    public void CheckNowEquipItem(string equipArea)
    {
        switch (equipArea)
        {
            case ("Ring"):
                equipItemCheck[1] = detail;
                break;
            case ("Head"):
                equipItemCheck[2] = detail;
                break;
            case ("Necklace"):
                equipItemCheck[3] = detail;
                break;
            case ("Chest"):
                equipItemCheck[5] = detail;
                break;
            case ("Hand"):
                equipItemCheck[6] = detail;
                break;
            case ("Weapon"):
                equipItemCheck[7] = detail;
                break;
            case ("Leg"):
                equipItemCheck[8] = detail;
                break;
            case ("Shoes"):
                equipItemCheck[9] = detail;
                break;
        }
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
        ItemCheck beforeItem = CheckNowEquipment(detail.itemData.equipArea);
        AttackManager attackManager = player.GetComponent<AttackManager>();
        GameObject nowEquipItem = detail.gameObject;

        EquipBoxCheck equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();

        if (detail.itemData.equipArea != "Weapon")
        {
            if (equipBox.transform.childCount == 0) // 무기가 아니고 방어구라면
            {
                CheckNowEquipItem(detail.itemData.equipArea);
                nowEquipItem.transform.SetParent(equipBox.transform);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.itemData.name, detail.itemData.equipArea, detail.itemData.tfName);
                equipBoxCheck.SaveEquipItem(detail, true);
            }
            else
            {
                // 아이템을 해체한는 부분
                DetechItem(detail.itemData.equipArea);
                equipBoxCheck.DeletPrefab(beforeItem, beforeItem.itemData.equipArea);

                nowEquipItem.transform.SetParent(equipBox.transform);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.name, detail.itemData.equipArea, detail.itemData.tfName);
                equipBoxCheck.ActivePrefab(detail.itemData.equipArea);
                equipBoxCheck.SaveEquipItem(detail, true);
                CheckNowEquipItem(detail.itemData.equipArea);
            }
        }
        else
        {
            skillCooldown.ResetCoolTime();
            if (attackManager.equipWeapon == null)
            {
                CheckNowEquipItem(detail.itemData.equipArea);
                nowEquipItem.transform.SetParent(equipBox.transform);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.itemData.name, detail.itemData.equipArea, detail.itemData.tfName);
                equipBoxCheck.SaveEquipItem(detail, true);

            }
            else
            {
                // 아이템을 해체한는 부분
                DetechItem(detail.itemData.equipArea);
                equipBoxCheck.DeletPrefab(beforeItem, beforeItem.itemData.equipArea);
                nowEquipItem.transform.SetParent(equipBox.transform);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.itemData.name, detail.itemData.equipArea, detail.itemData.tfName);
                equipBoxCheck.ActivePrefab(detail.itemData.equipArea);
                equipBoxCheck.SaveEquipItem(detail, true);
                CheckNowEquipItem(detail.itemData.equipArea);
            }
        }
    }
    void MoveEquipItem2Inventory(GameObject moveItem, GameObject equipChangeBox)
    {
        moveItem.transform.SetParent(equipChangeBox.transform);
        moveItem.transform.position = equipChangeBox.transform.position;
    }
    public void DetechItem(string equipArea)
    {
        GameObject equipChangeBox = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        GameObject moveItem = null;
        if (equipArea == "Ring") moveItem = equipBox[1].transform.GetChild(0).gameObject;
        else if (equipArea == "Head") moveItem = equipBox[2].transform.GetChild(0).gameObject;
        else if (equipArea == "Necklace") moveItem = equipBox[3].transform.GetChild(0).gameObject;
        else if (equipArea == "Chest") moveItem = equipBox[5].transform.GetChild(0).gameObject;
        else if (equipArea == "Hand") moveItem = equipBox[6].transform.GetChild(0).gameObject;
        else if (equipArea == "Weapon") moveItem = equipBox[7].transform.GetChild(0).gameObject;
        else if (equipArea == "Leg") moveItem = equipBox[8].transform.GetChild(0).gameObject;
        else if (equipArea == "Shoes") moveItem = equipBox[9].transform.GetChild(0).gameObject;
        MoveEquipItem2Inventory(moveItem, equipChangeBox);
    }


    // 상점 판매 변수
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
        totalSellPrice = (int)(sellSlider.value * detail.itemData.price);
        sellText.text = Loc.F("Sell {0} {1}EA,  Price : {2}", detail.itemData.itemNameT, sellSlider.value, totalSellPrice);
    }
    void OnSliderValueChanged(float value)
    {
        output = Mathf.RoundToInt(value);

    }
    void RightMove()
    {
        CusorContinuousInputCheck();
        sellSlider.value += 1;
        totalSellPrice = (int)(sellSlider.value * detail.itemData.price);
        sellText.text = Loc.F("Sell {0} {1}EA,  Price : {2}", detail.itemData.itemNameT, sellSlider.value, totalSellPrice);
    }
    void LeftMove()
    {
        CusorContinuousInputCheck();
        sellSlider.value -= 1;
        totalSellPrice = (int)(sellSlider.value * detail.itemData.price);
        sellText.text = Loc.F("Sell {0} {1}EA,  Price : {2}", detail.itemData.itemNameT, sellSlider.value, totalSellPrice);
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
        int sellCount = (int)sellSlider.value;
        if (!Service.RemoveAt(detail, sellCount))
        {
            // 인벤토리 칸 밖(예외 경로)은 기존 방식
            detail.nowStack -= sellCount;
            if (detail.nowStack <= 0)
            {
                Destroy(detail.gameObject);
            }
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
    /// <summary>정렬 키(D4): 키보드 R, 게임패드 R3 (Y 는 나누기에 이미 쓰고 있다)</summary>
    static bool SortPressed()
    {
        Keyboard kb = Keyboard.current;
        Gamepad gp = Gamepad.current;
        return (kb != null && kb.rKey.wasPressedThisFrame) || (gp != null && gp.rightStickButton.wasPressedThisFrame);
    }

    /// <summary>현재 페이지만 정렬하고 저장한다</summary>
    public void SortCurrentPage()
    {
        Service.SortPage(nowBox);
        SaveInventory();
        Sfx.Play(SfxId.UiMove);
    }

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
                    }
                }
            }
            if (selectAction.triggered && state == "detail" && detail.itemData.type == "Consum" && ShopGameObject.activeSelf == false)
            {
                ActiveConsum();
            }
            if (state == "")
            {
                BoxOpen();
            }
            if ((state == "" || state == "chestOpen") && SortPressed())
            {
                SortCurrentPage();
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
                    cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CreatItem("LeafStaff"); CreatItem("LeafAdae");
            CreatItem("ScareSide"); CreatItem("ScareStaff"); CreatItem("ScareClow");
            CreatItem("PoisonSword"); CreatItem("DoubleDagger"); CreatItem("RatCheifDagger");
            CreatItem("RustedSwordandShield");
            CreatItem("RustedSword"); CreatItem("SacreGreatSword");
            CreatItem("Leather Knuckles"); CreatItem("Wood Spear");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            CreatItem("LeafCap"); CreatItem("LeafArmor"); CreatItem("LeafLegArmor");
            CreatItem("LeafHandArmor"); CreatItem("LeafShoes");
            CreatItem("LeafStaff");
            CreatItem("LeafAdae");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {

    
            CreatItem("ScareCap"); CreatItem("ScareLegArmor"); CreatItem("ScareArmor");
            CreatItem("ScareHandArmor"); CreatItem("ScareShoes");
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            CreatItem("RatCap");
            CreatItem("RatArmor");
            CreatItem("RatLegArmor");
            CreatItem("RatHandArmor");
            CreatItem("RatShoes");

        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreatItem("Cap");
            CreatItem("Armor");
            CreatItem("LegArmor");
            CreatItem("HandArmor");
            CreatItem("Shoes");
            CreatItem("RustedSwordandShield");
            CreatItem("RustedSword");
            CreatItem("Leather Knuckles"); CreatItem("Wood Spear");
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            CreatItem("Rabbit Meat");
            CreatItem("Wood");
            CreatItem("T0_Ore");
            CreatItem("GreenHurb");
            CreatItem("Small Tuber");
            CreatItem("Fresh Green Leaves");
            CreatItem("Sturdy Roots");
            CreatItem("Sturdy Leaf");
            CreatItem("Sharp Petal");
        }

        if (openInventoryAction.triggered)
        {
            if (inventory.activeSelf == true && ShopGameObject.activeSelf == false)
            {
                UiLock.Set("InvGroup", false);
                SaveInventory();
                inventory.SetActive(false);
                CloseCheck();

                state = "";
            }
            else if (inventory.activeSelf == false && DatabaseManager.isOpenUI == false)
            {
                UiLock.Set("InvGroup", true);
                inventory.SetActive(true);
                Sfx.Play(SfxId.UiOpen, 0.5f);
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
            if (item.itemData.type == "Consum")
            {
                item.ApplyConsumeEffect();
                if (!Service.RemoveAt(item, 1))
                {
                    // 인벤토리 칸 밖(예외 경로): 수량만 직접 차감 (효과는 위에서 이미 적용)
                    item.nowStack -= 1;
                    if (item.nowStack <= 0)
                    {
                        Destroy(item.gameObject);
                    }
                }
                else
                {
                    SaveInventory();
                }
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
        CusorContinuousInputCheck();
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
                CusorContinuousInputCheck();
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum) nowBoxMax = (maxHor * maxVer);
                else nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));

                if (cusorCount[nowBox] + 1 < nowBoxMax && (cusorCount[nowBox] + 1) % maxHor != 0)
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
                    cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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

                if (cusorCount[nowBox] - maxHor >= 0)
                {
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
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
                CusorContinuousInputCheck();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum) nowBoxMax = (maxHor * maxVer);
                else nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));

                if (cusorCount[nowBox] + 1 < nowBoxMax && (cusorCount[nowBox] + 1) % maxHor != 0)
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
                detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }
            if (detail.itemData.type == "Misc")
            {
                Transform misc = miscDetail.gameObject.transform;
                misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(detail.itemData, false);
                misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (detail.itemData.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = Loc.T(SetRarity(detail.itemData.rarity));
                miscDetail.transform.position = detailPos.transform.position;
                miscDetail.SetActive(true);
            }
            if (detail.itemData.type == "Consum")
            {
                Transform consum = consumDetail.gameObject.transform;
                consum.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(detail.itemData, false);
                consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
                consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (detail.itemData.price).ToString();
                consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = Loc.T(SetRarity(detail.itemData.rarity));
                SetConsumEffect(consum.GetChild(7).gameObject, detail);
                consum.transform.position = detailPos.transform.position;
                consumDetail.SetActive(true);

            }
            if (detail.itemData.type == "Equip")
            {
                DrawEquipDetail(true);
            }
        }
        else if (state == "Equipment")
        {
            detail = ob != null ? ob.transform.GetComponent<ItemCheck>() : nowEquipBox.transform.GetChild(0).GetComponent<ItemCheck>();
            DrawEquipDetail(false);
        }
    }

    /// <summary>장비 상세(D3): 등급색 이름·강화 반영 스탯·착용품 비교(compare 이고 같은 부위 착용품이 있을 때)·세트 진행도</summary>
    void DrawEquipDetail(bool compare)
    {
        Transform equip = equipDetail.gameObject.transform;
        ItemData d = detail.itemData;
        GameObject prefab = Resources.Load<GameObject>(d.equipArea + "/" + detail.name);
        ItemCheck worn = compare ? EquippedFor(d.equipArea) : null;
        GameObject wornPrefab = worn != null ? Resources.Load<GameObject>(worn.itemData.equipArea + "/" + worn.name) : null;
        int wornUp = worn != null ? worn.itemData.upgrade : 0;
        equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
        equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = TooltipBuilder.Name(d.itemNameT, d.rarity, d.upgrade);
        equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(d, true);
        equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(d);
        equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + d.price;
        equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + d.tear;
        equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = "<color=" + TooltipBuilder.RarityColor(d.rarity) + ">" + TooltipBuilder.RarityName(d.rarity) + "</color>";
        SetEffectDetail(equip.GetChild(8).gameObject, detail);
        var stats = equip.GetChild(7).GetComponent<TextMeshProUGUI>();
        if (d.equipArea != "Weapon")
        {
            stats.text = TooltipBuilder.ArmorBlock(prefab.GetComponent<Equipment>(), d.upgrade, wornPrefab != null ? wornPrefab.GetComponent<Equipment>() : null, wornUp);
        }
        else
        {
            Weapon weapon = prefab.GetComponent<Weapon>();
            stats.text = TooltipBuilder.WeaponBlock(weapon, d.upgrade, wornPrefab != null ? wornPrefab.GetComponent<Weapon>() : null, wornUp);
            CheckSkillDetail(weapon);
        }
        equip.transform.position = detailPos.transform.position;
        equipDetail.SetActive(true);
    }

    /// <summary>같은 부위에 착용 중인 아이템 (없으면 null)</summary>
    ItemCheck EquippedFor(string equipArea)
    {
        ItemCheck saved = detail;
        GameObject box = SetEquipBox();
        detail = saved;
        return box != null && box.transform.childCount > 0 ? box.transform.GetChild(0).GetComponent<ItemCheck>() : null;
    }
    public GameObject skillDetailUi;
    public void CheckSkillDetail(Weapon weapon) // 기술 체크.
    {
        skillDetailUi.SetActive(true);
        for (int i = 0; i < 4; i++) // 기술 갯수가 늘어나면 여기 변경해 주어야함.
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
            }
            else
            {
                skillSlot.SetActive(false);
            }
        }
    }
    /// <summary>세트 효과(D3): "< 세트 > (착용/전체)" + 도달 단계 세트색, 미도달 회색</summary>
    public void SetEffectDetail(GameObject textObject, ItemCheck nowItem)
    {
        ItemData d = nowItem.itemData;
        var tiers = new List<KeyValuePair<int, string>>();
        var setItem = new SetItem();
        for (int i = 0; i < 8; i++)
        {
            string effect = setItem.SetEffectStr(d.tfName, i);
            if (!string.IsNullOrEmpty(effect))
            {
                tiers.Add(new KeyValuePair<int, string>(i, TooltipBuilder.FormatSetEffect(effect)));
            }
        }
        int worn = 0;
        if (!string.IsNullOrEmpty(d.tfName))
        {
            DatabaseManager.setsEffectStack.TryGetValue(d.tfName, out worn);
        }
        string setName = string.IsNullOrEmpty(d.setName) ? null : Loc.T(d.setName);
        textObject.GetComponent<TextMeshProUGUI>().text = TooltipBuilder.SetBlock(setName, worn, SetPartCount(d.tfName), tiers);
    }

    static readonly Dictionary<string, int> setPartCache = new Dictionary<string, int>();

    /// <summary>세트 전체 부위 수 = 같은 세트 키(tfName)를 가진 장비의 서로 다른 부위 수 (무기 여러 종은 1부위)</summary>
    static int SetPartCount(string tfName)
    {
        if (string.IsNullOrEmpty(tfName) || DatabaseManager.instance == null || DatabaseManager.instance.Items == null)
        {
            return 0;
        }
        if (setPartCache.TryGetValue(tfName, out int n))
        {
            return n;
        }
        var areas = new HashSet<string>();
        foreach (ItemData item in DatabaseManager.instance.Items)
        {
            if (item.type == "Equip" && item.tfName == tfName)
            {
                areas.Add(item.equipArea);
            }
        }
        setPartCache[tfName] = areas.Count;
        return areas.Count;
    }

    public void SetConsumEffect(GameObject textObject, ItemCheck nowItem)
    {
        StringBuilder allSetStr = new StringBuilder();
        string[] effect = nowItem.itemData.effectOb.Split("/");
        string[] effectPower = nowItem.itemData.effectPow.Split("/");

        for (int i = 0; i < effect.Length; i++)
        {
            string[] effectStr = effect[i].Split();
            string[] effectPowerDetail = effectPower[i].Split("_");

            if (effectPowerDetail.Length < 2)
            {
                allSetStr.Append(effectStr[1])
                         .Append(CapitalizeFirstLetter(effectStr[0]))
                         .Append(" ")
                         .Append(effectPowerDetail[0])
                         .Append("\n");
            }
            else // 2번째 값이 있다면 시간 정보
            {
                allSetStr.Append(effectStr[1])
                         .Append(CapitalizeFirstLetter(effectStr[0]))
                         .Append(" ")
                         .Append(effectPowerDetail[0])
                         .Append(" : ")
                         .Append(effectPowerDetail[1])
                         .Append(" ").Append(Loc.T("Sec"))
                         .Append("\n");
            }
        }

        textObject.GetComponent<TextMeshProUGUI>().text = allSetStr.ToString();
    }

    public void SetWeaponDetail(GameObject textObject, Weapon weapon, int upgrade = 0)
    {
        textObject.GetComponent<TextMeshProUGUI>().text = TooltipBuilder.WeaponBlock(weapon, upgrade, null, 0);
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
    public void SetArmorDetail(GameObject textObject, Equipment equipment, int upgrade = 0)
    {
        textObject.GetComponent<TextMeshProUGUI>().text = TooltipBuilder.ArmorBlock(equipment, upgrade, null, 0);
    }

    public string SetRarity(string rarity)
    {
        switch (rarity)
        {
            case "C":
                return "Common";
            case "R":
                return "Rare";
            case "U":
                return "Unique";
            default:
                return "";
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
    /// <summary>
    /// 아이템 1개를 인벤토리에 넣는다 (덜 찬 스택 우선, 없으면 첫 빈칸).
    /// 넣지 못하면 false. isC2I면 넣은 경우에만 창고 원본 수량을 줄인다(가득 찼을 때 소실 방지).
    /// </summary>
    InventoryService service;
    /// <summary>Phase 3a 쓰기 명령 (모델 → 바뀐 칸만 뷰 반영)</summary>
    public InventoryService Service => service ??= new InventoryService(this);

    public bool CreatItem(string itemName, bool isC2I = false, ItemData itemData = null, AddSource source = AddSource.Other)
    {
        bool placed = Service.Add(itemName, 1, isC2I ? AddSource.Chest : source, itemData);
        if (placed && isC2I)
        {
            chest.itemCheck.nowStack -= 1;
        }
        if (!placed)
        {
            GameLog.Info(GameLog.Cat.Item, "인벤토리 가득: " + itemName + " 넣지 못함", this);
        }
        if (placed)
        {
            SaveInventory();
        }
        return placed;
    }

    /// <summary>장착 목록에서 해당 아이템 참조를 지운다 (드래그로 해제했을 때)</summary>
    public void ForgetEquippedItem(ItemCheck item)
    {
        for (int i = 0; i < equipItemCheck.Length; i++)
        {
            if (equipItemCheck[i] == item)
            {
                equipItemCheck[i] = null;
            }
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
        if (ShopGameObject.activeSelf == true)
        {
            ShopUI.cusor.SetActive(false);
        }
        cusorCount[nowBox] = ob.transform.GetSiblingIndex();
        cusor.transform.position = ob.transform.position;
        cusor.SetActive(true);
        if (chest != null)
        {
            cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
            chest.changeCusor.SetActive(false);
            chest.cusor.SetActive(false);
            chest.isCusorChest = false;
            state = "chestOpen";
        }
        else
        {
            cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
            state = "";
        }
    }
    /// <summary>인벤토리(창고·장비 제외)에 실제로 놓인 아이템 슬롯을 모두 순회한다.</summary>
    public IEnumerable<ItemCheck> EnumerateInventoryItems()
    {
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum <= (j * (maxHor * maxVer)) + i)
                {
                    continue;
                }
                GameObject slot = GetNthChildGameObject(inventoryUI[j], i);
                if (slot == null || slot.transform.childCount == 0)
                {
                    continue;
                }
                ItemCheck check = slot.transform.GetChild(0).GetComponent<ItemCheck>();
                if (check != null && check.nowStack > 0)
                {
                    yield return check;
                }
            }
        }
    }

    /// <summary>인벤토리에 실제로 있는 아이템 수량 (슬롯을 직접 센다 — 캐시를 믿지 않는다)</summary>
    public int CountItem(string itemName)
    {
        int total = 0;
        foreach (ItemCheck check in EnumerateInventoryItems())
        {
            if (check.name == itemName)
            {
                total += check.nowStack;
            }
        }
        return total;
    }

    /// <summary>
    /// UI 표시용 수량 캐시(DatabaseManager.inventoryItemStack)를 실제 슬롯 기준으로 다시 만든다.
    /// 드래그·창고 이동·장착 등 캐시를 갱신하지 않는 경로가 많아서, 슬롯을 단일 진실 원천으로 삼는다.
    /// </summary>
    public void RebuildItemStackCache()
    {
        DatabaseManager.inventoryItemStack.Clear();
        foreach (ItemCheck check in EnumerateInventoryItems())
        {
            DatabaseManager.inventoryItemStack.TryGetValue(check.name, out int n);
            DatabaseManager.inventoryItemStack[check.name] = n + check.nowStack;
        }
    }

    /// <summary>itemName 1개를 새로 넣을 공간이 있는지 (빈 칸 또는 덜 찬 스택)</summary>
    public bool HasRoomFor(string itemName) => CheckBoxCanCreatAll() || OnlyCheckStack(itemName);

    float stackCacheTimer;
    private void LateUpdate()
    {
        // 표시용 캐시를 0.25초마다 슬롯 기준으로 재계산 (매 프레임 비용 회피)
        stackCacheTimer -= Time.unscaledDeltaTime;
        if (stackCacheTimer <= 0f)
        {
            stackCacheTimer = 0.25f;
            RebuildItemStackCache();
        }
    }

    /// <summary>앞 칸부터 count 개를 지운다. 가진 것보다 많으면 가진 만큼만(기존 동작 유지)</summary>
    public void DeletItemByName(string itemName, int count = 1)
    {
        int take = Mathf.Min(count, CountItem(itemName));
        if (take > 0 && Service.Remove(itemName, take))
        {
            SaveInventory(); // 차감도 즉시 저장 (이전엔 다음 획득 때까지 세이브에 남아 있었다)
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
                            if (check.itemData.maxStack > check.nowStack)
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
                    if (insPositon.transform.childCount > 0)
                    {
                        GameObject item = insPositon.transform.GetChild(0).gameObject;
                        ItemCheck check = item.GetComponent<ItemCheck>();
                        if (check.name == itemName)
                        {
                            if (check.itemData.maxStack > check.nowStack)
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
    /// <summary>지정 페이지의 첫 빈칸에 Stack개짜리 아이템을 만든다. 빈칸이 없으면 false(호출자는 원본을 줄이지 말 것).</summary>
    public bool CreatItemSelected(string itemName, int boxNum, int Stack = 1, ItemData data = null)
    {
        for (int i = 0; i < (maxHor * maxVer); i++)
        {
            if (maxBoxNum > (boxNum * (maxHor * maxVer)) + i)
            {
                GameObject insPositon = GetNthChildGameObject(inventoryUI[boxNum], i);
                if (inventoryArray[i, boxNum] == 0 && insPositon.transform.childCount == 0)
                {
                    inventoryArray[i, boxNum] = 1; // 같은 프레임 중복 배치 방지 (B-010)
                    GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                    ItemCheck check = item.GetComponent<ItemCheck>();
                    float scaleFactor = 0.8f; // 크기를 조절할 비율
                    item.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    check.SetItem(itemName, data != null ? data.Clone() : null); // 티어·강화 보존 (B-014)
                    check.nowStack = Stack;
                    return true;
                }
            }
        }
        return false;
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
        if ((state == "chestOpen" || chest != null) && state != "I2CMove")
        {
            cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
            state = "chestOpen";
        }
        else if (state != "chestOpen" && state != "I2CMove")
        {
            cusorImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
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
