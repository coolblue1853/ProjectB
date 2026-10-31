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
    int _saveBatchDepth;
    bool _saveBatchPending;
    bool _saveBatchChestPending;

    /// <summary>저장을 미루는 중인지(드래그 중·여러 개 구매 중)</summary>
    public bool IsSaveBatching => _saveBatchDepth > 0;

    /// <summary>여러 개를 연달아 넣을 때(상점 N개 구매·디버그 생성) 저장을 미룬다. 반드시 EndSaveBatch 와 짝으로 부른다(B-022)</summary>
    public void BeginSaveBatch()
    {
        _saveBatchDepth++;
    }

    /// <summary>BeginSaveBatch 이후 저장 요청이 있었으면 한 번만 저장한다</summary>
    public void EndSaveBatch()
    {
        if (_saveBatchDepth > 0)
        {
            _saveBatchDepth--;
        }
        if (_saveBatchDepth == 0 && (_saveBatchPending || _saveBatchChestPending))
        {
            bool withChest = _saveBatchChestPending;
            _saveBatchPending = false;
            _saveBatchChestPending = false;
            if (withChest && chest != null && PagesAlive(chest.inventoryUI))
            {
                chest.WriteChestArray(); // 미뤄 둔 창고 저장도 같은 시점 스캔으로, 디스크 쓰기는 아래 1회(B-015)
            }
            SaveInventory();
        }
    }

    /// <summary>
    /// 종료·씬 해제 중에 끊긴 배치를 저장 없이 푼다. 페이지가 이미 파괴돼 빈 인벤이 저장되는 것을 막는다
    /// (디스크에는 배치 전 상태가 남아 있어 손실 없음).
    /// </summary>
    public void AbandonSaveBatch()
    {
        if (_saveBatchDepth > 0)
        {
            _saveBatchDepth--;
        }
        if (_saveBatchDepth == 0)
        {
            _saveBatchPending = false;
            _saveBatchChestPending = false;
        }
    }

    /// <summary>창고 저장 요청. 배치 중이면 미루고 true(Chest.SaveChest 가 디스크에 바로 쓰지 않게)</summary>
    public bool DeferChestSave()
    {
        if (_saveBatchDepth == 0)
        {
            return false;
        }
        _saveBatchChestPending = true;
        return true;
    }

    public void SaveInventory()
    {
        if (_saveBatchDepth > 0)
        {
            _saveBatchPending = true;
            return;
        }
        if (!PagesAlive(inventoryUI))
        {
            // 종료·씬 해제 중에 페이지가 이미 파괴됐다 → 스캔하면 빈 인벤이 저장된다. 디스크의 이전 상태를 지킨다
            GameLog.Warn(GameLog.Cat.Save, "인벤 페이지가 없어 저장을 건너뜀(종료 중)");
            return;
        }
        SaveManager.instance.datas.money = DatabaseManager.money;
        // Phase 2 브리지: UI 슬롯 → 모델 → 세이브 배열 (SaveMapper)
        string[,,] saveInven = Service.BuildSaveArray(); // 열린 칸 밖 아이템도 보존
        SaveManager.instance.datas.invenItem = saveInven;
        SaveManager.instance.DataSave();
    }

    /// <summary>스캔 저장 전에 페이지 오브젝트가 모두 살아 있는지(파괴된 페이지를 건너뛰면 빈 칸으로 저장됨)</summary>
    public static bool PagesAlive(GameObject[] pages)
    {
        if (pages == null || SaveManager.instance == null)
        {
            return false;
        }
        foreach (GameObject p in pages)
        {
            if (p == null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>인벤토리 칸 → 칸 이동(키보드 교환·드래그 놓기). 성공하면 저장</summary>
    public bool MoveInInventory(Transform fromBox, Transform toBox)
    {
        if (!Service.MoveSlot(fromBox, toBox))
        {
            return false;
        }
        SaveInventory();
        return true;
    }

    /// <summary>인벤과 (열려 있으면) 창고를 세이브 데이터에 쓰고 디스크에는 한 번만 쓴다 — 둘 사이 이동 뒤 한쪽만 저장되는 복제·소실 방지(B-015)</summary>
    public void SaveInventoryAndChest()
    {
        if (DeferChestSave())
        {
            _saveBatchPending = true; // 배치 중: 둘 다 배치 끝에 같은 시점 스캔으로 저장
            return;
        }
        if (chest != null && PagesAlive(chest.inventoryUI))
        {
            chest.WriteChestArray();
        }
        SaveInventory();
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
        EnsureDefaultWeapon();
    }

    /// <summary>무기 칸이 비면 자동 장착하는 기본 무기(ItemSheet id 41 "RustedSword"). 맨손 공격이 없어 무기가 없으면 진행이 막힌다.</summary>
    public const string DefaultWeapon = "RustedSword";

    /// <summary>
    /// 무기 칸이 비어 있으면 기본 무기를 장착한다(새 게임·세이브 로드 때 호출, 2026-10-26 사용자 요청 — 예전 1회 지급 플래그 대체).
    /// 무기 해제 자체를 막으므로(CanUnequip) 빈 칸은 새 게임이나 예전 세이브에서만 생긴다.
    /// 인벤토리에 기본 무기가 있으면 그것을 꺼내 장착해(강화 유지) 무한 복제를 막는다.
    /// </summary>
    private void EnsureDefaultWeapon()
    {
        if (equipBox[7].transform.childCount > 0)
        {
            return;
        }
        GameObject go = Instantiate(itemPrefab, transform.position, Quaternion.identity, transform);
        ItemCheck item = go.GetComponent<ItemCheck>();
        item.SetItem(DefaultWeapon);
        if (item.itemData == null)
        {
            Destroy(go);
            return;
        }
        ItemData reuse = TakeOneFromInventory(DefaultWeapon); // 데이터가 확인된 뒤에 빼야 실패해도 인벤토리 검이 사라지지 않는다
        if (reuse != null)
        {
            item.SetItem(DefaultWeapon, reuse);
        }
        UseEquipment(item);
        equipBox[7].GetComponent<EquipBoxCheck>().ActivePrefab("Weapon"); // 장비창을 열지 않아도 바로 공격할 수 있게 연결
        SaveInventory();
        GameLog.Info(GameLog.Cat.Item, "기본 무기 장착: " + DefaultWeapon + (reuse != null ? " (인벤토리에서 꺼냄)" : " (새로 지급)"));
    }

    /// <summary>인벤토리에서 itemName 1개를 빼고 그 데이터 사본(티어·강화)을 돌려준다. 없으면 null.</summary>
    private ItemData TakeOneFromInventory(string itemName)
    {
        foreach (ItemCheck check in EnumerateInventoryItems())
        {
            if (check.name == itemName && check.itemData != null)
            {
                ItemData data = check.itemData.Clone();
                return Service.RemoveAt(check, 1) ? data : null;
            }
        }
        return null;
    }

    /// <summary>장비 칸에서 빼도 되는지. 무기는 빈손이 되지 않게 해제를 막는다(다른 무기로 교체만 가능).</summary>
    public static bool CanUnequip(ItemCheck item)
    {
        return item == null || item.itemData == null || item.itemData.equipArea != "Weapon";
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
        ItemCheck itemCheck = itemBox.transform.GetChild(0).GetComponent<ItemCheck>();

        InventoryAlpha boxCheck = inventoryUI[num].GetComponent<InventoryAlpha>();
        int siblingParentIndex = boxCheck.siblingIndex;
        if (nowBox != siblingParentIndex)
        {
            // 빈칸 판정은 모델이 한다(옛 inventoryArray 사전 검사 제거, B-219). 못 옮기면 상태 유지
            if (Service.MoveToPage(itemCheck, siblingParentIndex))
            {
                SaveInventory();
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

            // 창고에 들어간 수량만큼 인벤 쪽은 모델 명령으로 뺀다(3c-3, 예전엔 창고 코드가 인벤 nowStack 을 직접 깎았다)
            int count = itemCheck.nowStack;
            int moved = 0;
            for (int i = 0; i < count; i++)
            {
                if (!chest.CreatItem(itemCheck.name, itemCheck.itemData))
                {
                    break; // 창고가 가득 참
                }
                moved++;
            }
            if (moved > 0 && !Service.RemoveAt(itemCheck, moved))
            {
                GameLog.Warn(GameLog.Cat.Item, "창고 이동 후 인벤 차감 실패: " + itemCheck.name + " x" + moved);
            }
            // 양쪽을 함께 저장 (한쪽만 저장된 상태로 종료되면 복제·소실, B-015)
            SaveInventoryAndChest();
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
        if (output > 0 && output < item.nowStack && Service.SplitAt(item, output, nowBox))
        {
            SaveInventory();
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
            afterBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            // 이동·합치기(넘침은 원래 칸)·교환을 모델 명령 하나로(3b)
            if (afterBox != null && beforBox != afterBox)
            {
                MoveInInventory(beforBox.transform, afterBox.transform);
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
    /// <summary>페이지 숫자키 1~5 (New Input System, B-021)</summary>
    static bool DigitPressed(Key key)
    {
        Keyboard kb = Keyboard.current;
        return kb != null && kb[key].wasPressedThisFrame;
    }
    void BoxOpen()
    {
        Keyboard shiftKb = Keyboard.current;
        if (shiftKb != null && shiftKb.shiftKey.isPressed)
        {
            return; // Shift+숫자는 퀵슬롯 등록(QuickSlotBar)이라 페이지를 넘기지 않는다
        }
        if (DigitPressed(Key.Digit1) && nowBox != 0 && inventoryBox[0].activeSelf == true)
        {
            OpenBox(0);
        }
        else if (DigitPressed(Key.Digit2) && nowBox != 1 && inventoryBox[1].activeSelf == true)
        {
            OpenBox(1);
        }
        else if (DigitPressed(Key.Digit3) && nowBox != 2 && inventoryBox[2].activeSelf == true)
        {
            OpenBox(2);
        }
        else if (DigitPressed(Key.Digit4) && nowBox != 3 && inventoryBox[3].activeSelf == true)
        {
            OpenBox(3);
        }
        else if (DigitPressed(Key.Digit5) && nowBox != 4 && inventoryBox[4].activeSelf == true)
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
        if (!CanUnequip(detail))
        {
            return; // 무기는 해제하지 않는다 — 맨손 공격이 없어 빈손이 되면 진행이 막힌다
        }
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
                if (item.itemData == null) // 아이템 시트에서 사라진 이름(손상·구버전 세이브)
                {
                    Debug.LogWarning("[PB][Save] 알 수 없는 장비 무시: " + SaveManager.instance.datas.equipGear[i, 0]);
                    Destroy(equip);
                    continue;
                }
                // 손상 세이브에서 크래시하지 않게 TryParse, 실패하면 시트 기본값 유지(B-020)
                if (int.TryParse(SaveManager.instance.datas.equipGear[i, 1], out int tier))
                {
                    item.itemData.tear = tier;
                }
                if (int.TryParse(SaveManager.instance.datas.equipGear[i, 2], out int upgrade))
                {
                    item.itemData.upgrade = upgrade;
                }
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

    /// <summary>드래그로 인벤 칸 → 장비칸에 놓았을 때(키보드 장착과 같은 경로)</summary>
    public void EquipFromInventory(ItemCheck item)
    {
        UseEquipment(item);
    }

    /// <summary>장착할 장비칸(부위별). 없는 부위면 null</summary>
    public GameObject EquipBoxFor(ItemCheck item)
    {
        ItemCheck keep = detail;
        GameObject box = SetEquipBox(item);
        detail = keep;
        return box;
    }

    /// <summary>
    /// 장착. 인벤 칸에서 꺼내는 경우 인벤 쪽은 모델 명령(TakeOut)으로, 교체되는 장비는 비운 그 칸에 PutIn(3c).
    /// 인벤 밖(로드·기본 무기)에서 오는 아이템은 인벤을 건드리지 않는다.
    /// </summary>
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
        string area = detail.itemData.equipArea;
        EquipBoxCheck equipBoxCheck = equipBox.GetComponent<EquipBoxCheck>();
        // 지금 칸에 실제로 꽂혀 있는 장비(장착 목록이 낡았으면 칸의 자식). 무기 프리팹이 아직 활성화 전이어도
        // (equipWeapon == null) 칸에 무기가 있으면 교체로 본다 — 예전엔 한 칸에 무기 둘이 들어갔다
        ItemCheck beforeItem = CheckNowEquipment(area);
        if ((beforeItem == null || beforeItem.transform.parent != equipBox.transform) && equipBox.transform.childCount > 0)
        {
            beforeItem = equipBox.transform.GetChild(0).GetComponent<ItemCheck>();
        }
        bool occupied = beforeItem != null && beforeItem != detail && beforeItem.transform.parent == equipBox.transform;
        int fromIndex = Service.IndexOf(detail);
        if (fromIndex >= 0)
        {
            if (!Service.TakeOut(detail, equipBox.transform))
            {
                GameLog.Warn(GameLog.Cat.Item, "장착 실패(인벤 칸에서 꺼내지 못함): " + detail.name);
                return;
            }
        }
        else
        {
            detail.transform.SetParent(equipBox.transform);
            detail.transform.position = equipBox.transform.position;
        }

        if (occupied)
        {
            // 교체: 원래 장비는 방금 비운 인벤 칸으로(인벤 밖에서 온 장착이면 커서 칸으로 — 옛 경로)
            if (fromIndex < 0)
            {
                DetechItem(area); // 인벤 밖에서 온 장착(로드·기본 무기): 옛 경로
            }
            else if (!Service.PutIn(fromIndex, beforeItem))
            {
                // 방금 비운 칸이라 거의 없음. 커서 칸(DetechItem)은 드래그 때 엉뚱한 칸일 수 있어 쓰지 않고, 사본을 넣고 원본은 지운다
                GameLog.Warn(GameLog.Cat.Item, "교체 장비를 원래 칸에 못 넣음 → 사본 생성: " + beforeItem.name);
                CreatItem(beforeItem.name, false, beforeItem.itemData.Clone());
                beforeItem.transform.SetParent(null, false);
                Destroy(beforeItem.gameObject);
            }
            equipBoxCheck.DeletPrefab(beforeItem, area);
        }
        if (area == "Weapon")
        {
            skillCooldown.ResetCoolTime(); // 장착이 확정된 뒤에만
        }
        equipBoxCheck.LoadPrefab(detail.itemData.name, area, detail.itemData.tfName);
        if (occupied)
        {
            equipBoxCheck.ActivePrefab(area);
        }
        equipBoxCheck.SaveEquipItem(detail, true);
        CheckNowEquipItem(area);
        if (fromIndex >= 0)
        {
            SaveInventory(); // 인벤·장비를 한 번에 저장(SaveEquipItem 은 datas 에만 씀)
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
        if (detail == null || !ItemTradeRules.CanSell(detail.name))
        {
            Sfx.Play(SfxId.CraftFail, 0.4f);
            return;
        }
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
        sellSlider.onValueChanged.RemoveListener(OnSliderValueChanged); // 열 때마다 누적되지 않게(B-018)
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
        if (detail == null || !ItemTradeRules.CanSell(detail.name))
        {
            Sfx.Play(SfxId.CraftFail, 0.4f);
            return;
        }
        int sellCount = (int)sellSlider.value;
        // 되사기 기록용(칸에서 빼면 오브젝트가 파괴될 수 있어 먼저 읽는다)
        string soldId = detail != null ? detail.name : null;
        int soldTier = detail != null && detail.itemData != null ? detail.itemData.tear : 0;
        int soldUpgrade = detail != null && detail.itemData != null ? detail.itemData.upgrade : 0;
        // 칸에서 실제로 뺀 경우에만 돈을 준다(인벤토리 칸이 아니거나 수량이 모자라면 판매 취소)
        if (Service.RemoveAt(detail, sellCount))
        {
            DatabaseManager.money += totalSellPrice;
            Buyback.Record(soldId, sellCount, soldTier, soldUpgrade, totalSellPrice);
        }
        else
        {
            GameLog.Warn(GameLog.Cat.Item, "판매 실패(인벤토리 칸이 아니거나 수량 부족): " + (detail != null ? detail.name : "null") + " x" + sellCount);
        }
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
                if ((rightInventoryAction.triggered || (checkRepeat == false && horizontalInput == 1)) && sellSlider.value < sellSlider.maxValue)
                {
                    RightMove();
                }
                else if ((leftInventoryAction.triggered || (checkRepeat == false && horizontalInput == -1)) && sellSlider.value > 1)
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugSpawnKeys(); // F1~F6 테스트 아이템 생성은 에디터·개발 빌드에만(B-021)
#endif

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>디버그: F1~F6 로 테스트 아이템 묶음 생성(New Input System, 저장 1회)</summary>
    void DebugSpawnKeys()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !(kb.f1Key.wasPressedThisFrame || kb.f2Key.wasPressedThisFrame || kb.f3Key.wasPressedThisFrame
            || kb.f4Key.wasPressedThisFrame || kb.f5Key.wasPressedThisFrame || kb.f6Key.wasPressedThisFrame))
        {
            return;
        }
        BeginSaveBatch();
        try
        {
            if (kb[Key.F1].wasPressedThisFrame)
            {
                CreatItem("LeafStaff"); CreatItem("LeafAdae");
                CreatItem("ScareSide"); CreatItem("ScareStaff"); CreatItem("ScareClow");
                CreatItem("PoisonSword"); CreatItem("DoubleDagger"); CreatItem("RatCheifDagger");
                CreatItem("RustedSwordandShield");
                CreatItem("RustedSword"); CreatItem("SacreGreatSword");
                CreatItem("Leather Knuckles"); CreatItem("Wood Spear");
            }
            if (kb[Key.F3].wasPressedThisFrame)
            {
                CreatItem("LeafCap"); CreatItem("LeafArmor"); CreatItem("LeafLegArmor");
                CreatItem("LeafHandArmor"); CreatItem("LeafShoes");
                CreatItem("LeafStaff");
                CreatItem("LeafAdae");
            }
            if (kb[Key.F2].wasPressedThisFrame)
            {


                CreatItem("ScareCap"); CreatItem("ScareLegArmor"); CreatItem("ScareArmor");
                CreatItem("ScareHandArmor"); CreatItem("ScareShoes");
            }
            if (kb[Key.F4].wasPressedThisFrame)
            {
                CreatItem("RatCap");
                CreatItem("RatArmor");
                CreatItem("RatLegArmor");
                CreatItem("RatHandArmor");
                CreatItem("RatShoes");

            }
            if (kb[Key.F5].wasPressedThisFrame)
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

            if (kb[Key.F6].wasPressedThisFrame)
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
        }
        finally
        {
            EndSaveBatch();
        }
    }
#endif

    public bool checkRepeat = false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;


    public void ActiveConsum()
    {
        TryConsume(CursorItem());
    }

    /// <summary>열린 인벤토리에서 커서가 가리키는 아이템. 창이 닫혔거나 빈칸이면 null (퀵슬롯 등록용)</summary>
    public ItemCheck CursorItem()
    {
        if (inventory == null || !inventory.activeSelf || inventoryUI == null || nowBox < 0 || nowBox >= inventoryUI.Length)
        {
            return null;
        }
        GameObject box = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
        if (box == null || box.transform.childCount == 0)
        {
            return null;
        }
        return box.transform.GetChild(0).GetComponent<ItemCheck>();
    }

    /// <summary>
    /// 소비 아이템 1개 사용(인벤 소비 키·퀵슬롯 공용). 공유 쿨다운(ConsumeCooldown) 중이면 실패.
    /// 칸에서 1개를 먼저 빼고(InventoryService.RemoveAt) 성공했을 때만 저장 → 효과를 준다. 사용했으면 true.
    /// </summary>
    public bool TryConsume(ItemCheck item)
    {
        if (item == null || item.itemData == null || item.itemData.type != "Consum")
        {
            return false;
        }
        string group = ConsumeCooldown.GroupOf(item.itemData.effectOb);
        if (!ConsumeCooldown.Shared.IsReady(group, Time.time))
        {
            Sfx.Play(SfxId.CraftFail, 0.4f);
            return false;
        }
        if (item.itemData.effectOb != null && item.itemData.effectOb.StartsWith("recall"))
        {
            // 귀환 주문서: 시전이 끝나 이동이 성사될 때 RemoveOne 으로 소비한다(취소하면 소모 없음)
            if (!RecallCast.Begin(item.itemData.name))
            {
                Sfx.Play(SfxId.CraftFail, 0.4f);
                return false;
            }
            ConsumeCooldown.Shared.Start(group, Time.time);
            return true;
        }
        // 0개가 되어도 Destroy 는 프레임 끝이라 이 프레임 안의 ApplyConsumeEffect 는 itemData 를 읽을 수 있다.
        if (!Service.RemoveAt(item, 1))
        {
            GameLog.Warn(GameLog.Cat.Item, "소비 실패(인벤토리 칸이 아님): " + item.name);
            return false;
        }
        ConsumeCooldown.Shared.Start(group, Time.time);
        SaveInventory(); // 효과(CSV 파싱)에서 예외가 나도 차감은 저장되게 먼저 저장
        item.ApplyConsumeEffect();
        return true;
    }

    /// <summary>인벤토리에서 itemName 스택 1개를 빼고 저장한다(귀환 주문서 시전 완료용). 없으면 false</summary>
    public bool RemoveOne(string itemName)
    {
        foreach (ItemCheck check in EnumerateInventoryItems())
        {
            if (check.name == itemName && Service.RemoveAt(check, 1))
            {
                SaveInventory();
                return true;
            }
        }
        return false;
    }

    /// <summary>인벤토리에서 itemName 인 첫 소비 아이템 스택을 1개 사용한다(퀵슬롯). 없거나 쿨다운이면 false</summary>
    public bool ConsumeFirst(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return false;
        }
        ItemCheck found = null;
        foreach (ItemCheck check in EnumerateInventoryItems())
        {
            if (check.name == itemName && check.itemData != null && check.itemData.type == "Consum")
            {
                found = check;
                break;
            }
        }
        return TryConsume(found);
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
        GameObject prefab = GearPrefabCache.Get(d.equipArea, detail.name);
        ItemCheck worn = compare ? EquippedFor(d.equipArea) : null;
        GameObject wornPrefab = worn != null ? GearPrefabCache.Get(worn.itemData.equipArea, worn.name) : null;
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
            stats.text = TooltipBuilder.ArmorBlock(GearPrefabCache.Comp<Equipment>(prefab), d.upgrade, wornPrefab != null ? wornPrefab.GetComponent<Equipment>() : null, wornUp);
        }
        else
        {
            Weapon weapon = GearPrefabCache.Comp<Weapon>(prefab);
            stats.text = TooltipBuilder.WeaponBlock(weapon, d.upgrade, wornPrefab != null ? wornPrefab.GetComponent<Weapon>() : null, wornUp);
            CheckSkillDetail(weapon);
        }
        equip.transform.position = detailPos.transform.position;
        equipDetail.SetActive(true);
    }

    /// <summary>같은 부위에 착용 중인 아이템 (없으면 null). 비교 패널(ComparePanel)도 쓴다</summary>
    public ItemCheck EquippedFor(string equipArea)
    {
        int i = EquipBoxIndex(equipArea);
        GameObject box = i >= 0 && equipBox != null && i < equipBox.Length ? equipBox[i] : null;
        return box != null && box.transform.childCount > 0 ? box.transform.GetChild(0).GetComponent<ItemCheck>() : null;
    }

    /// <summary>부위 → equipBox 칸 번호(SetEquipBox 와 같은 표). 모르는 부위면 -1</summary>
    static int EquipBoxIndex(string equipArea)
    {
        switch (equipArea)
        {
            case "Ring": return 1;
            case "Head": return 2;
            case "Necklace": return 3;
            case "Chest": return 5;
            case "Hand": return 6;
            case "Weapon": return 7;
            case "Leg": return 8;
            case "Shoes": return 9;
            default: return -1;
        }
    }
    public GameObject skillDetailUi;
    public void CheckSkillDetail(Weapon weapon) // 기술 체크.
    {
        if (weapon == null || weapon.skill == null)
        {
            skillDetailUi.SetActive(false); // 프리팹 없는 무기(B-017)
            return;
        }
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
    /// <summary>
    /// 인벤토리 밖(드래그 중·창고)에서 온 Stack개짜리 아이템을 지정 페이지의 첫 빈칸에 놓는다(InventoryService.PutInPage).
    /// 빈칸이 없으면 false(호출자는 원본을 지우지 말 것). 저장은 호출자가 한다.
    /// </summary>
    public bool CreatItemSelected(string itemName, int boxNum, int Stack = 1, ItemData data = null, AddSource? source = null)
    {
        var stack = new ItemStack(itemName, Stack, data != null ? data.tear : 0, data != null ? data.upgrade : 0);
        return Service.PutInPage(boxNum, stack, data, source);
    }
    /// <summary>
    /// 드래그로 페이지 탭(인벤·창고)에 놓아 CreatItemSelected 가 성공한 뒤: 끌던 원본을 지우고 인벤·창고를 한 번에 저장한다(B-015).
    /// Phase 4 에서 SlotContainer.Transfer + 원자 저장으로 대체할 임시 창구다.
    /// </summary>
    public void CommitDragToPage(GameObject dragged)
    {
        dragged.transform.SetParent(null, false); // Destroy 는 프레임 끝이라 먼저 떼어 같은 프레임 스캔에 안 잡히게
        Destroy(dragged);
        SaveInventoryAndChest();
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
