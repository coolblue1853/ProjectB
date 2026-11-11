using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ShopManager : MonoBehaviour
{
    public GameObject detailPos;
    public GameObject skillDetailUi;
    public GameObject[] LV1;
    public GameObject nowLine;

    public ItemCheck nowShopItem;
    public TextMeshProUGUI name;
    public TextMeshProUGUI type;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;
    public TextMeshProUGUI weight;
    public TextMeshProUGUI acqPath;
    public TextMeshProUGUI effectOb;
    public TextMeshProUGUI effectPow;
    public TextMeshProUGUI equipArea;
    public Image image;
    public GameObject itemPrefab; // 생성되는 Box 인스턴스

    int nowLv = 0;
    int line = 0;
    int childCount = 0;
    public GameObject ShopUI;
    public GameObject needMaterailUI;
    public GameObject cusor;
    public GameObject craftCusor;
    public GameObject[] LV1craftBox;
    int nowPage = 0;
    private TextMeshProUGUI _stockPageLabel;
    public GameObject Inventory;

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
    InputAction leftSholderAction;
    InputAction rightSholderAction;
    InputAction openCraftUiAction;
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
        rightSholderAction.Enable();
        leftSholderAction.Enable();
        openCraftUiAction.Enable();
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
        rightSholderAction.Disable();
        leftSholderAction.Disable();
        openCraftUiAction.Disable();
    }
    private void Awake()
    {
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
        leftSholderAction = action.UI.NextLeftPage;
        rightSholderAction = action.UI.NextRightPage;
        openCraftUiAction = action.UI.OpenCraft;
    }
    void Start()
    {
        nowLv = 0;
        line = 0;
        nowPage = 0;
        childCount = 0;
        nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(0).GetComponent<ItemCheck>();
    }
    void SetDetail()
    {
        UpdateStockPageLabel();
        name.text = nowShopItem.itemData.itemNameT; // 내부 이름이 아니라 표시 이름(현지화)
        type.text = ItemText.TypeLine(nowShopItem.itemData, false);
        description.text = ItemText.Description(nowShopItem.itemData);
        price.text =(nowShopItem.buyprice).ToString();
        weight.text = nowShopItem.itemData.weight.ToString();
        acqPath.text = ItemText.AcqPath(nowShopItem.itemData);
        Image changeImage = nowShopItem.transform.GetChild(0).GetComponent<Image>();
        image.sprite = changeImage.sprite;
    }
    public GameObject miscDetail;
    public GameObject consumDetail;
    public GameObject equipDetail;
    ItemCheck detail;
    public bool isBuyDetail;
    public void CusorContinuousInputCheck()
    {
        checkRepeat = true;
        sequence.Kill();
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
    }
    void SetNeedDetail()
    {
        isBuyDetail = true;
        if (nowShopItem.itemData.type == "Misc")
        {
            Transform misc = miscDetail.gameObject.transform;
            misc.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.itemNameT;
            misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(nowShopItem.itemData, false);
            misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(nowShopItem.itemData);
            misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (nowShopItem.buyprice).ToString();
            misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.itemData.tear.ToString();
            misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.itemData.rarity);
            miscDetail.transform.position = detailPos.transform.position;

            miscDetail.SetActive(true);
            consumDetail.SetActive(false);
            equipDetail.SetActive(false);
            skillDetailUi.SetActive(false);
        }
        if (nowShopItem.itemData.type == "Consum")
        {
            Transform consum = consumDetail.gameObject.transform;
            consum.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.itemNameT;
            consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(nowShopItem.itemData, false);
            consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(nowShopItem.itemData);
            consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (nowShopItem.buyprice).ToString();
            consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.itemData.tear.ToString();
            consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.itemData.rarity);
            InventoryManager.instance.SetConsumEffect(consum.GetChild(7).gameObject, nowShopItem);

            consum.transform.position = detailPos.transform.position;
            miscDetail.SetActive(false);
            consumDetail.SetActive(true);
            equipDetail.SetActive(false);
            skillDetailUi.SetActive(false);
        }
        if (nowShopItem.itemData.type == "Equip")
        {
            Transform equip = equipDetail.gameObject.transform;
            GameObject prefab = GearPrefabCache.Get(nowShopItem.itemData.equipArea, nowShopItem.name);
            equip.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = ItemText.TypeLine(nowShopItem.itemData, true);
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(nowShopItem.itemData);
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = ItemText.Description(nowShopItem.itemData);
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = Loc.T("Price : ") + (nowShopItem.buyprice).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.itemData.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.itemData.rarity);
            InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, nowShopItem);
            if (nowShopItem.itemData.equipArea != "Weapon") // 방어구라면
            {
                Equipment equipment = GearPrefabCache.Comp<Equipment>(prefab);
                InventoryManager.instance.SetArmorDetail(equip.GetChild(7).gameObject, equipment);
            }
            else // 무기인 경우
            {
                Weapon weapon = GearPrefabCache.Comp<Weapon>(prefab);
                InventoryManager.instance.SetWeaponDetail(equip.GetChild(7).gameObject, weapon);
                InventoryManager.instance.CheckSkillDetail(weapon);
            }
            equip.transform.position = detailPos.transform.position;
            miscDetail.SetActive(false);
            consumDetail.SetActive(false);
            equipDetail.SetActive(true);
        }
    }
    public GameObject detailCusor;
    bool isCheckDetail;
    int detailCount = 0;

   public bool leftKeyCheck = false;
    public void ChangeCusor()
    {
        cusor.SetActive(true);
        checkRepeat = true;
        sequence = DOTween.Sequence()
       .AppendInterval(waitTime)
       .OnComplete(() => ResetCheckRepeat());
    }
    public int currentSellValue = 0; // 현재 가지고 있는 int 수
    public GameObject sellUIGameObject;
    public TextMeshProUGUI sellText;
    public Slider sellSlider; // Unity Inspector에서 Slider를 할당
    public void SellItemUI()
    {
        state = "Buy";
        ItemCheck target = nowShopItem;
        int unit = target.buyprice;
        int max = Mathf.Max(1, BuyLimit(target)); // 살 수 없어도 max 가 min 보다 작아지지 않게 1, 실제 구매 수는 BuyItem 이 다시 자른다(B-018)
        QuantityPopup.Open(new QuantityRequest
        {
            Title = "구매",
            ItemName = target.itemData.itemNameT,
            Min = 1,
            Max = max,
            Initial = 1,
            ConfirmLabel = "구매",
            Detail = n => "합계 " + QuantityRules.Total(unit, n).ToString("N0") + " 골드 (보유 " + DatabaseManager.money.ToString("N0") + ")",
            OnConfirm = BuyItem,
            OnCancel = CancelBuyQuantity
        });
    }

    /// <summary>수량 창 취소: 상품 상세로 돌아간다(예전 Back 경로와 같은 상태)</summary>
    void CancelBuyQuantity()
    {
        InventoryManager.instance.state = "ShopCusorOn";
        state = "";
        cusor.SetActive(true);
        isMoveCusor = true;
        DetailOff();
    }
    /// <summary>가진 돈으로 살 수 있는 최대 개수(최대 maxStack). 가격 0 이하면 maxStack (0 나누기 방지, B-018)</summary>
    public static int MaxAffordable(int maxStack, int price, int money)
    {
        if (price <= 0)
        {
            return Mathf.Max(0, maxStack);
        }
        return Mathf.Clamp(money / price, 0, Mathf.Max(0, maxStack));
    }

    /// <summary>가방에 더 넣을 수 있는 개수(같은 스택 여유 + 빈칸 × 스택 한도)</summary>
    public static int BagSpaceFor(string id)
    {
        InventoryManager inv = InventoryManager.instance;
        return inv == null ? 0 : inv.Service.ScanModel().SpaceFor(new ItemStack(id, 1));
    }

    /// <summary>골드·가방 여유·스택 한도를 모두 따진 한 번에 살 수 있는 개수</summary>
    public static int BuyLimit(ItemCheck item)
    {
        return ShopRules.MaxBuyable(item.itemData.maxStack, item.buyprice, DatabaseManager.money, BagSpaceFor(item.name));
    }

    /// <summary>살 수 없는 이유를 알린다(골드 부족 / 가방 가득). 살 수 있으면 true</summary>
    public static bool CanBuyOne(ItemCheck item)
    {
        switch (ShopRules.CheckBuy(item.buyprice, DatabaseManager.money, BagSpaceFor(item.name)))
        {
            case ShopRules.BuyBlock.NoGold:
                PickupLog.Warn("골드 부족 (필요 " + item.buyprice.ToString("N0") + "G / 보유 " + DatabaseManager.money.ToString("N0") + "G)");
                return false;
            case ShopRules.BuyBlock.BagFull:
                PickupLog.ReportFull();
                return false;
            default:
                return true;
        }
    }

    void BuyItem(int requested)
    {
        int count = Mathf.Min(requested, BuyLimit(nowShopItem));
        int bought = 0, spent = 0;
        InventoryManager.instance.BeginSaveBatch(); // N개 구매 = 저장 1회(B-022)
        try
        {
            for (int i = 0; i < count; i++)
            {
                if (DatabaseManager.money < nowShopItem.buyprice) // 외상 구매 방지(B-018)
                {
                    break;
                }
                if (InventoryManager.instance.CheckBoxCanCreatAll() == true || InventoryManager.instance.OnlyCheckStack(nowShopItem.name) == true)
                {
                    if (InventoryManager.instance.CreatItem(nowShopItem.name, false, null, AddSource.Shop))
                    {
                        DatabaseManager.money -= nowShopItem.buyprice; // 실제로 들어간 것만 돈을 뺀다
                        bought++;
                        spent += nowShopItem.buyprice;
                    }
                }
            }
            ReportBought(nowShopItem, bought, spent, requested);
            InventoryManager.instance.SaveInventory(); // 하나도 못 샀어도 돈·인벤을 한 번 저장(배치 안이라 표시만)
        }
        finally
        {
            InventoryManager.instance.EndSaveBatch(); // 예외가 나도 배치가 잠긴 채 남지 않게
        }
        InventoryManager.instance.state = "ShopCusorOn";
        sellUIGameObject.SetActive(false);
        state = "";
        cusor.SetActive(true);
        isMoveCusor = true;
        DetailOff();
    }
    /// <summary>구매 결과 안내: 산 개수·쓴 골드, 요청보다 적게 샀으면 이유(가방/골드)</summary>
    public static void ReportBought(ItemCheck item, int bought, int spent, int requested)
    {
        if (bought > 0)
        {
            PickupLog.Report(item.itemData.itemNameT, bought, item.itemData.rarity);
            Sfx.Play(SfxId.Money);
            GameLog.Info(GameLog.Cat.Item, "구매: " + item.name + " x" + bought + " (-" + spent + "G)");
        }
        if (bought < requested)
        {
            if (bought == 0 && DatabaseManager.money < item.buyprice)
            {
                PickupLog.Warn("골드 부족");
            }
            else
            {
                PickupLog.Warn("가방이 가득 차 " + bought + "/" + requested + "개만 샀다");
            }
        }
    }

    public void CloseBuyUI()
    {
        InventoryManager.instance.state = "ShopCusorOn";
        sellUIGameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (QuantityPopup.IsBlocking)
        {
            return; // 수량 창이 모달
        }
        if (ShopUI.activeSelf == true && InventoryManager.instance.state == "ShopCusorOn")
        {
            if (cusor.activeSelf == false && craftCusor.activeSelf == true)
            {
      
                if (upInventoryAction.triggered == true)
                {
                    detailCount = 0;
                    craftCusor.SetActive(false);
                    detailCusor.SetActive(true);

                    isCheckDetail = true;
                    SetNeedDetail();
                }
            }
            if (cusor.activeSelf == true && isMoveCusor == true&& state == "")
            {
                verticalInput = (verticalCheck.ReadValue<float>());
                horizontalInput = (horizontalCheck.ReadValue<float>());
                MoveCusor();
            }

            if (cusor.activeSelf == true && craftCusor.activeSelf == false && selectAction.triggered == true && isBuyDetail ==false)
            {
                //InventoryManager.instance.state = "BuyDetail";
                isCheckDetail = true;
                SetNeedDetail();
            }
            else if(isBuyDetail == true && selectAction.triggered == true)
            {
                if (CanBuyOne(nowShopItem))
                {
                    InventoryManager.instance.state = "BuyDetail";
                    SellItemUI();
                }
                else
                {
                    Sfx.Play(SfxId.CraftFail, 0.4f); // 이유는 CanBuyOne 이 알렸다
                }
            }
            if (name.text == "")
            {
                SetDetail();
            }
        }

    }
    public void OpenShop()
    {
        if (ShopUI.activeSelf == false&& DatabaseManager.isOpenUI == false)
        {
            InventoryManager.instance.inventoryUI[0].SetActive(true);
            InventoryManager.instance.nowBox = 0;
            InventoryManager.instance.state = "";
            InventoryManager.instance.boxCusor = 0;
            InventoryManager.instance.ResetBoxOrigin();
            InventoryManager.instance.cusor.SetActive(true);
            Invoke("ChangeisMoveCusor", 0.2f);
            ShopUI.SetActive(true);
            ShopStockExtender.RefreshFor(this); // 층계가 오른 뒤 처음 열 때 새 품목을 진열
            AttachMouse();
            EnsureCloseButton();
            ResetStockSelection();
            InventoryManager.instance.state = "ShopCusorOn";
            InventoryManager.instance.cusor.SetActive(false);
            cusor.SetActive(true);
            Inventory.SetActive(true);
            UiLock.Set("InvGroup", true);
        }
    }
    public void ResetStockSelection()
    {
        nowLv = 0;
        nowPage = 0;
        line = 0;
        childCount = 0;
        state = "";
        if (LV1craftBox == null || LV1craftBox.Length == 0 || LV1craftBox[0] == null)
        {
            return;
        }
        Transform box = LV1craftBox[0].transform;
        if (box.childCount == 0 || box.GetChild(0).childCount == 0 || box.GetChild(0).GetChild(0).childCount == 0)
        {
            return;
        }
        for (int i = 0; i < box.childCount; i++)
        {
            box.GetChild(i).gameObject.SetActive(i == 0);
        }
        nowShopItem = box.GetChild(0).GetChild(0).GetChild(0).GetComponent<ItemCheck>();
        cusor.transform.position = nowShopItem.transform.position;
        DetailOff();
        if (nowShopItem.itemData != null)
        {
            SetDetail();
        }
    }

    public string state = "";
    public void BuildStockPageLabel()
    {
        if (_stockPageLabel != null)
        {
            return;
        }
        Transform box = LV1craftBox[0].transform;
        var go = new GameObject("TownStockPageLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(box.parent, false);
        _stockPageLabel = go.GetComponent<TextMeshProUGUI>();
        _stockPageLabel.font = UiTheme.Get().pixelFont;
        _stockPageLabel.fontSize = 20f;
        _stockPageLabel.alignment = TextAlignmentOptions.Center;
        _stockPageLabel.textWrappingMode = TextWrappingModes.NoWrap;
        _stockPageLabel.raycastTarget = false;
        RectTransform rt = _stockPageLabel.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.position = box.position;
        rt.anchoredPosition += new Vector2(140f, -340f);
        rt.sizeDelta = new Vector2(560f, 30f);
        UpdateStockPageLabel();
    }

    private void UpdateStockPageLabel()
    {
        if (_stockPageLabel != null)
        {
            _stockPageLabel.text = ShopStockExtender.PageName(nowPage) +
                "  " + (nowPage + 1) + " / " + LV1craftBox[0].transform.childCount + "   Q · E / LB · RB";
        }
    }

    public void DetailOff()
    {
        isBuyDetail = false;
        skillDetailUi.SetActive(false);
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
    }
    public void CloseShop()
    {
        cusor.SetActive(false);
        isMoveCusor = false;
        ShopUI.SetActive(false);
        needMaterailUI.SetActive(false);
        UiLock.Set("InvGroup", false);
    }

    bool isCraft = false;
    void ChangeisCarft()
    {
        isCraft = true;
    }
    void ChangeisCarftFalse()
    {
        isCraft = false;
    }
    public bool isMoveCusor = false;
    void ChangeisMoveCusor()
    {
        isMoveCusor = true;
    }
    float verticalInput;
    float horizontalInput;
    public bool checkRepeat = false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;
    void ResetCheckRepeat()
    {
        leftKeyCheck = false;
        checkRepeat = false;
    }

    public void MoveCusorByMouse(int inline, int inchild)
    {
        line = inline;
        childCount = inchild;
        cusor.transform.position = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).position; // lv 상자의 line 줄의 count 위치.
        nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
        SetDetail();
    }
    void MoveCusor()
    {
        cusor.transform.position = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).position; // lv 상자의 line 줄의 count 위치.
        if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
        {
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.childCount > childCount + 1)
            {
                CusorContinuousInputCheck();
                childCount += 1;
                nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
                SetDetail();
                DetailOff();
            }
            else
            {
                InventoryManager.instance.checkRepeat = false;
                InventoryManager.instance.CusorChest2Inven();
                cusor.SetActive(false);
                DetailOff();
            }
        }
        if (((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1)) && leftKeyCheck == false)
        {
            if (childCount > 0)
            {
                CusorContinuousInputCheck();
                childCount -= 1;
                nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
                SetDetail();
                DetailOff();
            }
        }
        if (((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1)) && LV1craftBox[nowLv].transform.GetChild(nowPage).transform.childCount > line+1)
        {
            CusorContinuousInputCheck();
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line+1).transform.childCount < childCount+1)  //. 1, 2번째줄의 자식카운트 = 2 > 내카운트 2+1
            {
                childCount = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line+1).transform.childCount - 1;
            }
            line += 1;
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
            SetDetail();
            DetailOff();
        }
        if (((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1)) && 0 < line)
        {
            CusorContinuousInputCheck();
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line-1).transform.childCount < childCount +1) //  저게 2     지금이 3
            {
                childCount = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line-1).transform.childCount - 1;
            }
            line -= 1;
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
            SetDetail();
            DetailOff();
        }
        if (rightSholderAction.triggered)
        {
            ChangePage(1);
        }
        if (leftSholderAction.triggered)
        {
            ChangePage(-1);
        }
    }

    /// <summary>진열 페이지를 delta(±1)만큼 넘긴다. 끝이면 무시(키보드 LB·RB, 마우스 휠 공용)</summary>
    void ChangePage(int delta)
    {
        int next = nowPage + delta;
        if (next < 0 || next >= LV1craftBox[nowLv].transform.childCount)
        {
            return;
        }
        LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(false);
        line = 0;
        childCount = 0;
        nowPage = next;
        DetailOff();
        LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(true);
        nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
        SetDetail();
    }

    // ───────────── 마우스 (REWORK-2 S2 U27) ─────────────

    /// <summary>진열된 모든 칸에 마우스 입력을 붙인다(재고가 늘 수 있어 열 때마다 확인)</summary>
    void AttachMouse()
    {
        if (LV1craftBox == null || LV1craftBox.Length == 0 || LV1craftBox[0] == null)
        {
            return;
        }
        foreach (ItemCheck it in LV1craftBox[0].GetComponentsInChildren<ItemCheck>(true))
        {
            if (it.GetComponent<ShopSlotMouse>() == null)
            {
                it.gameObject.AddComponent<ShopSlotMouse>().Init(this, it);
            }
        }
    }

    /// <summary>지금 보이는 페이지의 칸이면 줄·열 번호를 돌려준다</summary>
    bool TryLocate(ItemCheck item, out int ln, out int ch)
    {
        ln = ch = -1;
        if (item == null || item.transform.parent == null || item.transform.parent.parent == null
            || LV1craftBox[nowLv].transform.GetChild(nowPage) != item.transform.parent.parent)
        {
            return false;
        }
        ln = item.transform.parent.GetSiblingIndex();
        ch = item.transform.GetSiblingIndex();
        return true;
    }

    /// <summary>마우스를 칸에 올림: 상점 쪽에 포커스가 있을 때만 커서를 옮긴다(가방을 조작 중이면 건드리지 않음)</summary>
    public void HoverSlot(ItemCheck item)
    {
        int ln, ch;
        if (QuantityPopup.IsBlocking || state != "" || InventoryManager.instance.state != "ShopCusorOn" || nowShopItem == item
            || !TryLocate(item, out ln, out ch))
        {
            return;
        }
        MoveCusorByMouse(ln, ch);
        DetailOff();
    }

    /// <summary>호버 툴팁 문구. 상세창이 이미 그 칸을 보여 주면 비운다</summary>
    public string TipText(ItemCheck item)
    {
        if (item == null || item.itemData == null || (isBuyDetail && nowShopItem == item))
        {
            return "";
        }
        return ItemTipText.Build(item, item.buyprice, false);
    }

    /// <summary>
    /// 칸 클릭. 왼쪽=선택(상세창, 키보드 Z 한 번), 이미 선택·상세가 열린 칸을 다시 누르거나 오른쪽 클릭=구매 수량 창(Z 두 번).
    /// 가방 쪽에 포커스가 있었으면 상점으로 되돌린다.
    /// </summary>
    public void PickByMouse(ItemCheck item, bool buy)
    {
        int ln, ch;
        InventoryManager inv = InventoryManager.instance;
        if (QuantityPopup.IsBlocking || !ShopUI.activeSelf || state != "" || !TryLocate(item, out ln, out ch))
        {
            return;
        }
        if (inv.state != "ShopCusorOn")
        {
            inv.DetailOff();
            inv.state = "ShopCusorOn";
            inv.cusor.SetActive(false);
            cusor.SetActive(true);
            isMoveCusor = true;
        }
        bool again = nowShopItem == item && isBuyDetail;
        MoveCusorByMouse(ln, ch);
        if (!again)
        {
            DetailOff();
            SetNeedDetail();
        }
        if (!buy && !again)
        {
            return;
        }
        if (CanBuyOne(nowShopItem))
        {
            inv.state = "BuyDetail";
            SellItemUI();
        }
        else
        {
            Sfx.Play(SfxId.CraftFail, 0.4f); // 이유는 CanBuyOne 이 알렸다
        }
    }

    /// <summary>마우스 휠: 진열 페이지 넘기기</summary>
    public void PageByMouse(int delta)
    {
        if (QuantityPopup.IsBlocking || state != "" || !ShopUI.activeSelf)
        {
            return;
        }
        ChangePage(delta);
    }

    private GameObject _closeButton;

    /// <summary>우상단 닫기 X(없으면 한 번 만든다). 누르면 Esc 와 같이 인벤·상점을 함께 닫는다</summary>
    void EnsureCloseButton()
    {
        if (_closeButton != null || ShopUI == null)
        {
            return;
        }
        UiTheme theme = UiTheme.Get();
        Image img = theme.NewPanel("CloseX", ShopUI.transform, true);
        img.raycastTarget = true;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-8f, -8f);
        rt.sizeDelta = new Vector2(52f, 52f);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            if (!QuantityPopup.IsBlocking && InventoryManager.instance != null)
            {
                InventoryManager.instance.CloseAllWindows();
            }
        });
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(rt, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            label.font = theme.pixelFont;
        }
        label.fontSize = 30f;
        label.raycastTarget = false;
        label.alignment = TextAlignmentOptions.Center;
        label.text = "X";
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        _closeButton = img.gameObject;
        rt.SetAsLastSibling();
    }
}
