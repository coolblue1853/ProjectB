using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public partial class Chest : MonoBehaviour
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
        if (InventoryManager.instance != null && InventoryManager.instance.DeferChestSave())
        {
            return; // 드래그 중 등 배치 중이면 인벤과 함께 배치 끝에 저장(끌던 아이템이 빠진 창고가 저장되지 않게, B-219)
        }
        if (!InventoryManager.PagesAlive(inventoryUI))
        {
            GameLog.Warn(GameLog.Cat.Save, "창고 페이지가 없어 저장을 건너뜀(종료 중)");
            return;
        }
        WriteChestArray();
        SaveManager.instance.DataSave();
    }

    /// <summary>창고 슬롯 → 세이브 데이터(datas.chestItem)에 쓰기만 한다. 디스크 쓰기는 호출자(인벤과 한 번에 저장할 때, B-015)</summary>
    public void WriteChestArray()
    {
        // Phase 2 브리지: UI 슬롯 → 모델 → 세이브 배열 (SaveMapper)
        SlotContainer model = ViewScanner.BuildFrom(inventoryUI, 30, CsvItemCatalog.Shared);
        SaveManager.instance.datas.chestItem = SaveMapper.ToArray(model, 5, 30);
        SaveManager.instance.datas.chestFlags = ItemFlagSave.ToArray(model, 5, 30);
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
                        ItemStack saved = SaveMapper.ReadCell(SaveManager.instance.datas.chestItem, i, j);
                        if (!saved.IsEmpty)
                        {
                            GameObject insPositon = GetNthChildGameObject(inventoryUI[i], j);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            float scaleFactor = 0.8f; // 크기를 조절할 비율
                            item.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(saved.Id);
                            if (check.itemData == null) // 시트에 없는 이름(손상·구버전 세이브)은 버린다(B-020)
                            {
                                Destroy(item);
                                continue;
                            }
                            check.nowStack = saved.Count;
                            check.itemData.tear = saved.Tier;
                            check.itemData.upgrade = saved.Upgrade;
                            ItemFlagSave.Apply(check.itemData, ItemFlagSave.At(SaveManager.instance.datas.chestFlags, i, j));
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
        if (QuantityPopup.IsBlocking)
        {
            return; // 수량 창이 모달
        }
        UpdateConvenience();
        bool mine = InteractArbiter.Contend(gameObject, transform.position, isChestClose);
        if (mine && upAction.triggered && DatabaseManager.isOpenUI == false)
        {
            state = ""; 
            InventoryManager.instance.RepeatCheck();
            UiLock.Set("InvGroup", true);
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
                            WithdrawSelected(int.MaxValue); // U17: 상세를 연 칸에서 Z 한 번 더 = 꺼내기(Tab 모드를 몰라도 꺼내진다)
                            DetailOff();
                        }
                        if (state == "C2IMove")
                        {
                            C2IMove();
                        }
                    }
                }
                if (UiLock.TryBack("InvGroup", backAction.triggered))
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
   public  ItemCheck itemCheck ;
   /// <summary>
    /// 커서 칸의 창고 아이템을 모델로 옮기고, 실제 이동한 수량만 창고에서 줄인다.
    /// 인벤토리가 가득 차면 남은 수량은 창고에 그대로 둔다 (이중 차감·공짜 생성 버그 B-008 수정).
    /// </summary>
    void C2IMove()
    {
        WithdrawSelected(int.MaxValue);
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
                if (ViewScanner.Read(afterBox.transform).CanStackWith(ViewScanner.Read(beforBox.transform)) && afterItemCheck.nowStack < afterItemCheck.itemData.maxStack)
                {
                    isSame = true; // 같다면 가능한 만큼 스택을 합친다.

                    if (afterItemCheck.nowStack + beforeItemCheck.nowStack <= afterItemCheck.itemData.maxStack)
                    {
                        afterItemCheck.nowStack += beforeItemCheck.nowStack;
                        beforitem.transform.SetParent(null, false);
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
            SaveChest();
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
                _withdrawQuantity = state == "C2IMove";
                state = "divide";
                OpenDivideQuantity(nowDivideItem, _withdrawQuantity);
            }
        }
    }

    /// <summary>창고 나누기/꺼내기 수량 창(U14 공용 창). 꺼내기는 전부(스택 수)까지, 나누기는 1개 남기고까지</summary>
    void OpenDivideQuantity(ItemCheck item, bool withdraw)
    {
        int total = item.nowStack;
        QuantityPopup.Open(new QuantityRequest
        {
            Title = withdraw ? "꺼내기" : "나누기",
            ItemName = item.itemData.itemNameT,
            Min = 1,
            Max = withdraw ? total : total - 1,
            Initial = withdraw ? total : total / 2,
            ConfirmLabel = withdraw ? "꺼내기" : "나누기",
            Detail = n => withdraw ? n + "개를 인벤토리로 (창고에 " + (total - n) + "개 남음)" : n + "개를 새 칸으로 (원래 칸 " + (total - n) + "개)",
            OnConfirm = n =>
            {
                DownNowStack(n);
                DetailOff();
            },
            OnCancel = CloseDivide
        });
    }
    public void DownNowStack(int output)
    {
        if (_withdrawQuantity)
        {
            WithdrawSelected(output);
            _withdrawQuantity = false;
            state = "C2IMove";
            return;
        }
        GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        ItemCheck item = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
        if (output > 0 && output < item.nowStack && CreatItemSelected(item.itemData.name, nowBox, output, item.itemData))
        {
            item.nowStack -= output; // 새 칸을 만든 경우에만 원본 감소 (B-009)
            SaveChest();
        }
        else if (output > 0 && output < item.nowStack)
        {
            PickupLog.Warn("이 페이지가 가득 찼습니다 — 나눌 빈칸이 필요합니다");
        }
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
        InventoryManager.instance.state = "chestOpen";
    }
    /// <summary>창고 전체 페이지에 1개 넣는다(같은 속성의 스택 우선). 인벤 쪽 차감과 저장은 호출자 책임.</summary>
    public bool CreatItem(string itemName, ItemData data = null)
    {
        if (!CsvItemCatalog.Shared.Exists(itemName))
        {
            return false;
        }
        SlotContainer model = ScanModel();
        if (!model.TryAddAll(new ItemStack(itemName, 1, data != null ? data.tear : 0, data != null ? data.upgrade : 0, data != null && data.isLocked, data != null && data.isNew)))
        {
            return false;
        }
        ApplyModel(model);
        return true;
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
                misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(detail.itemData, false);
                misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (detail.itemData.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = TooltipBuilder.RarityName(detail.itemData.rarity);
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
                consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(detail.itemData, false);
                consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
                consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (detail.itemData.price).ToString();
                consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = TooltipBuilder.RarityName(detail.itemData.rarity);
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
                GameObject prefab = GearPrefabCache.Get(detail.itemData.equipArea, detail.itemData.name);
                equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
                equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(detail.itemData, true);
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
                equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
                equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (detail.itemData.price).ToString();
                equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
                equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = TooltipBuilder.RarityName(detail.itemData.rarity);
                InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, detail);
                if (detail.itemData.equipArea != "Weapon") // 방어구라면
                {
                    Equipment equipment = GearPrefabCache.Comp<Equipment>(prefab);
                    InventoryManager.instance.SetArmorDetail(equip.GetChild(7).gameObject, equipment, detail.itemData.upgrade);
                }
                else // 무기인 경우
                {
                    Weapon weapon = GearPrefabCache.Comp<Weapon>(prefab);
                    InventoryManager.instance.SetWeaponDetail(equip.GetChild(7).gameObject, weapon, detail.itemData.upgrade);
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
            GameObject prefab = GearPrefabCache.Get(detail.itemData.equipArea, detail.itemData.name);
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(detail.itemData, true);
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(detail.itemData);
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (detail.itemData.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = TooltipBuilder.RarityName(detail.itemData.rarity);
            InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, detail);
            if (detail.itemData.equipArea != "Weapon") // 방어구라면
            {
                Equipment equipment = GearPrefabCache.Comp<Equipment>(prefab);
                InventoryManager.instance.SetArmorDetail(equip.GetChild(7).gameObject, equipment, detail.itemData.upgrade);
            }
            else // 무기인 경우
            {
                Weapon weapon = GearPrefabCache.Comp<Weapon>(prefab);
                InventoryManager.instance.SetWeaponDetail(equip.GetChild(7).gameObject, weapon, detail.itemData.upgrade);
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
            InventoryManager.instance.CloseAllWindows(); // Esc 는 창고만이 아니라 인벤까지 함께 닫는다(U1)
        }
    }
    public void CloseDivide()
    {
        divideSlider.gameObject.SetActive(false);
        state = _withdrawQuantity ? "C2IMove" : "";
        _withdrawQuantity = false;
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
                if (CreatItemSelected(itemCheck.itemData.name, siblingParentIndex, itemCheck.nowStack, itemCheck.itemData))
                {
                    item.transform.SetParent(null, false);
                    Destroy(item.gameObject);
                    SaveChest();
                }
                cusorCount[nowBox] = beforeCusorInt;
                changeCusor.SetActive(false);
                state = "";
            }
            else
            {
                PickupLog.Warn("이 창고 페이지가 가득 찼습니다");
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
