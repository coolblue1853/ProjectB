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
    public GameObject inventoryBoxPrefab; // �����Ǵ� Box �ν��Ͻ�
    public GameObject itemPrefab; // �����Ǵ� Box �ν��Ͻ�
    public GameObject[] inventoryUI;
    public GameObject[] inventoryUIBack;
    public GameObject[] inventoryBox;
    Image cusorImage;
    public GameObject cusor; // �κ��丮 Ŀ��
    public GameObject changeCusor; // �κ��丮 Ŀ��
    public int[] cusorCount = new int[5]; // �κ��丮 Ŀ��
    public int boxCusor = 0; // �ڽ� �̵��� Ŀ�� int
    int maxBoxCusor = 0; // �ִ� �̵� ���� �ڽ� Ŀ��
    public int maxBoxNum;
    public int[,] inventoryArray;

    public int nowBox;  // �̰����� �迭�� ���� �о���� ��. ��, nowBox�� 2�϶��� 15����� 30 +15 -> 45��° ĭ�� �ִ� �������̶�� ���� ��.
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

    public GameObject[] equipBox; // 1: ����, 2: ����, 3: �����, 5: ����, 6: �尩, 7: ����, 8:����, 9:�Ź�
    public ItemCheck[] equipItemCheck = new ItemCheck[10];

    // ���߿� �ۺ� ��ü;
    public GameObject beforBox;
    public GameObject afterBox;
    int beforeCusorInt;

    int equipCusor; // ���â Ŀ�� ��ȣ

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
        string[,,] saveInven = new string[5, maxHor * maxVer, 4]; // ���°�κ�, ĭ, �̸�
        for (int i = 0; i < 5; i++)
        {
            int childCount = inventoryUI[i].transform.childCount;
            if (childCount != 0)
            {
                for (int j = 0; j < childCount; j++)
                {
                    GameObject box = inventoryUI[i].transform.GetChild(j).gameObject;
                  //  Debug.Log(box.transform.childCount);
                    if (box.transform.childCount > 0 && box.transform.GetChild(0).GetComponent<ItemCheck>() != null)
                    {
                        ItemCheck item = box.transform.GetChild(0).GetComponent<ItemCheck>();
                        saveInven[i, j, 0] = item.itemData.name;
                        saveInven[i, j, 1] = item.nowStack.ToString();
                        saveInven[i, j, 2] = item.itemData.tear.ToString();
                        saveInven[i, j, 3] = item.itemData.upgrade.ToString();
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
        if (SaveManager.instance.datas.invenItem == null)
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
                        // ���翡�� �ܼ��� �����۸�  �̸��� Ȯ���ؼ� ����������, ���⿡ �����ؼ� ���� ���� ��ġ, ��ȭ��ġ �̷��͵� �����;���
                        if (SaveManager.instance.datas.invenItem[i, j, 0] != "non" && SaveManager.instance.datas.invenItem[i, j, 0] != null)
                        {
                            inventoryArray[j, i] = 1;
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[i], j);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(SaveManager.instance.datas.invenItem[i, j, 0]);
                            check.nowStack = int.Parse(SaveManager.instance.datas.invenItem[i, j, 1]);
                            check.itemData.tear = int.Parse(SaveManager.instance.datas.invenItem[i, j, 2]);
                            check.itemData.upgrade = int.Parse(SaveManager.instance.datas.invenItem[i, j, 3]);
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
        Invoke("ActiveFalse", 0.000000001f); // �κ��丮 ���ε�, �� ������ ������ GridLayoutGroup�� �����۵����� ����.
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
                if (afterItemCheck.name == beforeItemCheck.name && (afterItemCheck.nowStack != afterItemCheck.itemData.maxStack && beforeItemCheck.nowStack != beforeItemCheck.itemData.maxStack))
                {
                    isSame = true; // ���ٸ� ������ ��ŭ ������ ��ģ��.

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

    void MoveCusor2Inventory() // ���â => �κ��丮�� Ŀ���� �ű�� �Լ�
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
            if (chest != null && state != "itemBoxChange") //â�� ����
            {
                cusor.SetActive(false);
                state = "InChestMove";
                chest.isCusorChest = true;
                chest.CusorMove2Chest();
            }
            else if (ShopGameObject.activeSelf == true) // ���� ����
            {
                ShopUI.leftKeyCheck = true;
                cusor.SetActive(false);
                state = "ShopCusorOn";
                ShopUI.ChangeCusor();
            }
            else // �׿� ���â ����
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
            CreatItem(detail.name);
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

        // ���Ⱑ �ε� ������ ī�޶� ������½���.

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
            if (equipBox.transform.childCount == 0) // ���Ⱑ �ƴϰ� �����
            {
                CheckNowEquipItem(detail.itemData.equipArea);
                nowEquipItem.transform.SetParent(equipBox.transform);
                nowEquipItem.transform.position = equipBox.transform.position;
                equipBoxCheck.LoadPrefab(detail.itemData.name, detail.itemData.equipArea, detail.itemData.tfName);
                equipBoxCheck.SaveEquipItem(detail, true);
            }
            else
            {
                // �������� ��ü�Ѵ� �κ�
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
                // �������� ��ü�Ѵ� �κ�
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
        else if (equipArea == "Neckles") moveItem = equipBox[3].transform.GetChild(0).gameObject;
        else if (equipArea == "Chest") moveItem = equipBox[5].transform.GetChild(0).gameObject;
        else if (equipArea == "Hand") moveItem = equipBox[6].transform.GetChild(0).gameObject;
        else if (equipArea == "Weapon") moveItem = equipBox[7].transform.GetChild(0).gameObject;
        else if (equipArea == "Leg") moveItem = equipBox[8].transform.GetChild(0).gameObject;
        else if (equipArea == "Shoes") moveItem = equipBox[9].transform.GetChild(0).gameObject;
        MoveEquipItem2Inventory(moveItem, equipChangeBox);
    }


    // ���� �Ǹ� ����
    public int currentSellValue = 0; // ���� ������ �ִ� int ��
    public GameObject sellUIGameObject;
    int totalSellPrice;
    public TextMeshProUGUI sellText;
    public Slider sellSlider; // Unity Inspector���� Slider�� �Ҵ�
    int output;

    public void SellItemUI()
    {
        InitializeSlider();
        state = "Sell";
        sellUIGameObject.SetActive(true);
        totalSellPrice = (int)(sellSlider.value * detail.itemData.price);
        sellText.text = "Sell " + detail.itemData.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
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
        sellText.text = "Sell " + detail.itemData.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void LeftMove()
    {
        CusorContinuousInputCheck();
        sellSlider.value -= 1;
        totalSellPrice = (int)(sellSlider.value * detail.itemData.price);
        sellText.text = "Sell " + detail.itemData.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }

    void InitializeSlider()
    {
        sellSlider.onValueChanged.AddListener(OnSliderValueChanged);
        sellSlider.maxValue = detail.nowStack;
        // �����̴��� �ּҰ��� �ִ밪 ����
        sellSlider.minValue = 1;
        // �����̴��� ���� �� ����
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
                        // �Ǹ� �Լ�
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
                    state = "I2CMove"; // �κ��丮���� â��� ���� �̵�
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
            if (item.itemData.type == "Consum")
            {
                item.ConsumItemActive();
                DatabaseManager.MinusInventoryDict(item.name, 1);
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
    public void BoxContentChecker(GameObject ob = null) // ZŰ Ŭ���� �κ��丮â
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
                misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.  itemData.description;
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.itemData.rarity);
                miscDetail.transform.position = detailPos.transform.position;
                miscDetail.SetActive(true);
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
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.itemData.rarity);
                SetConsumEffect(consum.GetChild(7).gameObject, detail);
                consum.transform.position = detailPos.transform.position;
                consumDetail.SetActive(true);

            }
            if (detail.itemData.type == "Equip")
            {
                Transform equip = equipDetail.gameObject.transform;
                string folderPath = detail.itemData.equipArea + "/";

                // ���ҽ� ���� ���� equipName�� �ε��մϴ�.
                GameObject prefab = Resources.Load<GameObject>(folderPath + detail.name);
                equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type + " : " + detail.itemData.equipArea;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
                equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
                equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.itemData.rarity);
                SetEffectDetail(equip.GetChild(8).gameObject, detail);
                if (detail.itemData.equipArea != "Weapon") // �����
                {
                    Equipment equipment = prefab.GetComponent<Equipment>();
                    SetArmorDetail(equip.GetChild(7).gameObject, equipment);
                }
                else // ������ ���
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
            if (ob != null)
            {
                detail = ob.transform.GetComponent<ItemCheck>();
            }
            else
            {
                GameObject gameObject = (nowEquipBox);
                detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }
            string folderPath = detail.itemData.equipArea + "/";

            // ���ҽ� ���� ���� equipName�� �ε��մϴ�.
            GameObject prefab = Resources.Load<GameObject>(folderPath + detail.name);
            Transform equip = equipDetail.gameObject.transform;
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type + " : " + detail.itemData.equipArea;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = SetRarity(detail.itemData.rarity);
            SetEffectDetail(equip.GetChild(8).gameObject, detail);
            if (detail.itemData.equipArea != "Weapon") // �����
            {
                Equipment equipment = prefab.GetComponent<Equipment>();
                SetArmorDetail(equip.GetChild(7).gameObject, equipment);
            }
            else // ������ ���
            {
                Weapon weapon = prefab.GetComponent<Weapon>();
                SetWeaponDetail(equip.GetChild(7).gameObject, weapon);
                CheckSkillDetail(weapon);
            }
            equip.transform.position = detailPos.transform.position;
            equipDetail.SetActive(true);

            equip.transform.position = detailPos.transform.position;
            equipDetail.SetActive(true);
        }
    }
    public GameObject skillDetailUi;
    public void CheckSkillDetail(Weapon weapon) // ��� üũ.
    {
        skillDetailUi.SetActive(true);
        for (int i = 0; i < 4; i++) // ��� ������ �þ�� ���� ������ �־����.
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
    public void SetEffectDetail(GameObject textObject, ItemCheck nowItem)
    {
        // ���� ��Ʈȿ��
        StringBuilder allSetStr = new StringBuilder();

        if (!string.IsNullOrEmpty(nowItem.itemData.setName))
            allSetStr.Append("< ").Append(nowItem.itemData.setName).Append(" >\n");

        SetItem setItem = new SetItem();

        for (int i = 0; i < 8; i++)
        {
            string effecStr = setItem.SetEffectStr(nowItem.itemData.tfName, i);
            if (!string.IsNullOrEmpty(effecStr))
            {
                string noParentheses = effecStr.Replace("(", "").Replace(")", " ");
                string[] box = noParentheses.Split("/");

                allSetStr.Append("(").Append(i).Append(") ");

                for (int k = 0; k < box.Length; k++)
                {
                    string[] checkEffect = box[k].Split();

                    if (checkEffect[2] == "skillHitCount")
                    {
                        allSetStr.Append(checkEffect[0])
                                 .Append(checkEffect[1])
                                 .Append(" [").Append(checkEffect[3].Replace("_", " "))
                                 .Append("] count");
                    }
                    else if (checkEffect[2] == "bulletCount")
                    {
                        allSetStr.Append(checkEffect[0])
                                 .Append(checkEffect[1])
                                 .Append(" [").Append(checkEffect[3].Replace("_", " "))
                                 .Append("] count");
                    }
                    else if (checkEffect[2] == "coolDown")
                    {
                        allSetStr.Append("-").Append(checkEffect[1])
                                 .Append("% CoolDown [").Append(checkEffect[3].Replace("_", " "))
                                 .Append("]");
                    }
                    else
                    {
                        allSetStr.Append(checkEffect[0])
                                 .Append(" ").Append(checkEffect[1])
                                 .Append(" ").Append(checkEffect[2]);
                    }

                    if (k != box.Length - 1)
                    {
                        allSetStr.Append(",\n");
                    }
                    else
                    {
                        allSetStr.Append("\n");
                    }
                }
            }
        }

        textObject.GetComponent<TextMeshProUGUI>().text = allSetStr.ToString();
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
            else // 2��° ���� �ִٸ� �ð� ����
            {
                allSetStr.Append(effectStr[1])
                         .Append(CapitalizeFirstLetter(effectStr[0]))
                         .Append(" ")
                         .Append(effectPowerDetail[0])
                         .Append(" : ")
                         .Append(effectPowerDetail[1])
                         .Append(" Sec")
                         .Append("\n");
            }
        }

        textObject.GetComponent<TextMeshProUGUI>().text = allSetStr.ToString();
    }

    public void SetWeaponDetail(GameObject textObject, Weapon weapon)
    {
        StringBuilder basicStr = new StringBuilder();

        // ���� �⺻���� ���ȼ�ġ
        basicStr.Append(weapon.minDmg)
                .Append("~")
                .Append(weapon.maxDmg)
                .Append(" Dmg\n");

        if (weapon.critPer != 0)
        {
            basicStr.Append(weapon.critPer > 0 ? "+" : "-")
                    .Append(weapon.critPer)
                    .Append(" Critical Chance\n");
        }

        if (weapon.critDmg != 0)
        {
            basicStr.Append(weapon.critDmg > 0 ? "+" : "-")
                    .Append(weapon.critDmg)
                    .Append(" Critical Dmg\n");
        }

        if (weapon.incDmg != 0)
        {
            basicStr.Append(weapon.incDmg > 0 ? "+" : "-")
                    .Append(weapon.incDmg)
                    .Append(weapon.incDmg > 0 ? " Dmg Increase\n" : " Dmg Decrease\n");
        }

        if (weapon.ignDef != 0)
        {
            basicStr.Append(weapon.ignDef > 0 ? "+" : "-")
                    .Append(weapon.ignDef)
                    .Append(" Armor Penetration\n");
        }

        if (weapon.skillDmg != 0)
        {
            basicStr.Append(weapon.skillDmg > 0 ? "+" : "-")
                    .Append(weapon.skillDmg)
                    .Append(" Skill Dmg\n");
        }

        if (weapon.addDmg != 0)
        {
            basicStr.Append(weapon.addDmg > 0 ? "+" : "-")
                    .Append(weapon.addDmg)
                    .Append(" Additional Dmg\n");
        }

        textObject.GetComponent<TextMeshProUGUI>().text = basicStr.ToString();
    }

    public static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        // ù ��° ���ڸ� �빮�ڷ� �ٲٰ� ������ �κ��� �״�� �Ӵϴ�.
        return char.ToUpper(input[0]) + input.Substring(1);
    }
    public void SetArmorDetail(GameObject textObject, Equipment equipment)
    {
        StringBuilder basicStr = new StringBuilder();

        // ���� �⺻���� ���ȼ�ġ
        if (equipment.basicDmg != 0)
        {
            basicStr.Append(equipment.basicDmg > 0 ? "+" : "-")
                    .Append(equipment.basicDmg)
                    .Append(" Basic Dmg\n");
        }

        if (equipment.armor != 0)
        {
            basicStr.Append(equipment.armor > 0 ? "+" : "-")
                    .Append(equipment.armor)
                    .Append(" Def\n");
        }

        if (equipment.hp != 0)
        {
            basicStr.Append(equipment.hp > 0 ? "+" : "-")
                    .Append(equipment.hp)
                    .Append(" Hp\n");
        }

        if (equipment.critical != 0)
        {
            basicStr.Append(equipment.critical > 0 ? "+" : "-")
                    .Append(equipment.critical)
                    .Append(" Critical Rate\n");
        }

        if (equipment.dropRate != 0)
        {
            basicStr.Append(equipment.dropRate > 0 ? "+" : "-")
                    .Append(equipment.dropRate)
                    .Append(" Drop Rate\n");
        }

        if (equipment.moveSpeed != 0)
        {
            basicStr.Append(equipment.moveSpeed > 0 ? "+" : "-")
                    .Append(equipment.moveSpeed)
                    .Append(" Movement Speed\n");
        }

        if (equipment.attSpeed != 0)
        {
            basicStr.Append(equipment.attSpeed > 0 ? "+" : "-")
                    .Append(equipment.attSpeed)
                    .Append(" Attack Speed\n");
        }

        if (equipment.criticalDmg != 0)
        {
            basicStr.Append(equipment.criticalDmg > 0 ? "+" : "-")
                    .Append(equipment.criticalDmg)
                    .Append(" Critical Dmg\n");
        }

        if (equipment.coolDownSkill.Length != 0)
        {
            for (int i = 0; i < equipment.coolDownSkill.Length; i++)
            {
                basicStr.Append("-")
                        .Append(equipment.coolDownSkill[i].coolDownCount)
                        .Append("% Cooldown ")
                        .Append(equipment.coolDownSkill[i].skillName)
                        .Append("\n");
            }
        }

        if (equipment.incDmg != 0)
        {
            basicStr.Append(equipment.incDmg > 0 ? "+" : "-")
                    .Append(equipment.incDmg)
                    .Append("% Dmg\n");
        }

        if (equipment.addIncomingDmg != 0)
        {
            basicStr.Append(equipment.addIncomingDmg > 0 ? "+" : "-")
                    .Append(equipment.addIncomingDmg)
                    .Append("% Incoming Dmg\n");
        }

        if (equipment.isBleeding)
        {
            basicStr.Append("+")
                    .Append(equipment.bleedingPerCent)
                    .Append("% Chance to Cause Bleeding\n");
        }

        if (equipment.bleedingDmgPer != 0)
        {
            basicStr.Append("+")
                    .Append(equipment.bleedingDmgPer)
                    .Append("% Dmg to Bleeding Enemies\n");
        }

        if (equipment.poisonDmg != 0)
        {
            basicStr.Append("+")
                    .Append(equipment.poisonDmg)
                    .Append(" Poison Dmg\n");
        }

        textObject.GetComponent<TextMeshProUGUI>().text = basicStr.ToString();
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
    public void CreatItem(string itemName, bool isC2I = false)
    {
        if (CheckStack(itemName) == false)
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
    public void DeletItemByName(string itemName, int count = 1)
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
                        if (check.name == itemName && count > 0)
                        {
                            if (check.nowStack > 0)
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
                    float scaleFactor = 0.8f; // ũ�⸦ ������ ����
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
