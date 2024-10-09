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
        name.text = nowShopItem.name;
        type.text = nowShopItem.itemData.type;
        description.text = nowShopItem.itemData.description;
        price.text =(nowShopItem.buyprice).ToString();
        weight.text = nowShopItem.itemData.weight.ToString();
        acqPath.text = nowShopItem.itemData.acqPath;
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
            misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.type;
            misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.description;
            misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (nowShopItem.itemData.price).ToString();
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
            consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.type;
            consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.description;
            consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (nowShopItem.itemData.price).ToString();
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
            string folderPath = nowShopItem.itemData.equipArea + "/";
            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + nowShopItem.name);
            equip.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.type + " : " + nowShopItem.itemData.equipArea;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.description;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemData.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (nowShopItem.itemData.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.itemData.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.itemData.rarity);
            InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, nowShopItem);
            if (nowShopItem.itemData.equipArea != "Weapon") // 방어구라면
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
    int totalSellPrice;
    public TextMeshProUGUI sellText;
    public Slider sellSlider; // Unity Inspector에서 Slider를 할당
    int output;
    public void SellItemUI()
    {
        InitializeSlider();
        state = "Buy";
        sellUIGameObject.SetActive(true);
        totalSellPrice = (int)(sellSlider.value * nowShopItem.buyprice);
        sellText.text = "Buy " + nowShopItem.itemData.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void OnSliderValueChanged(float value)
    {
        output = Mathf.RoundToInt(value);
    }
    void RightMove()
    {
        CusorContinuousInputCheck();
        sellSlider.value += 1;
        totalSellPrice = (int)(sellSlider.value * nowShopItem.buyprice);
        sellText.text = "Buy " + nowShopItem.itemData.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void LeftMove()
    {
        CusorContinuousInputCheck();
        sellSlider.value -= 1;
        totalSellPrice = (int)(sellSlider.value * nowShopItem.buyprice);
        sellText.text = "Buy " + nowShopItem.itemData.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void InitializeSlider()
    {
        sellSlider.onValueChanged.AddListener(OnSliderValueChanged);
        sellSlider.maxValue = Mathf.Min(nowShopItem.itemData.maxStack, DatabaseManager.money / nowShopItem.buyprice);
        // 슬라이더의 최소값과 최대값 설정
        sellSlider.minValue = 1;
        // 슬라이더의 현재 값 설정
        sellSlider.value = 1;
        output = 1;
    }
    void BuyItem()
    {
        for(int i =0; i < sellSlider.value; i++)
        {
            if (InventoryManager.instance.CheckBoxCanCreatAll() == true || InventoryManager.instance.OnlyCheckStack(nowShopItem.name) == true)
            {
                DatabaseManager.money -= nowShopItem.buyprice;
                InventoryManager.instance.CreatItem(nowShopItem.name);
            }
        }
        InventoryManager.instance.state = "ShopCusorOn";
        sellUIGameObject.SetActive(false);
        state = "";
        cusor.SetActive(true);
        isMoveCusor = true;
        DetailOff();
        InventoryManager.instance.SaveInventory();
    }
    public void CloseBuyUI()
    {
        InventoryManager.instance.state = "ShopCusorOn";
        sellUIGameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (InventoryManager.instance.state == "BuyDetail")
        {
            if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1) && sellSlider.value < sellSlider.maxValue)
            {
                RightMove();
            }
            else if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1) && sellSlider.value > 1)
            {
                LeftMove();
            }
            if (selectAction.triggered && DatabaseManager.money >= nowShopItem.buyprice)
            {
                BuyItem();
            }
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
            else if(isBuyDetail == true && selectAction.triggered == true && DatabaseManager.money >= nowShopItem.buyprice)
            {
                InventoryManager.instance.state = "BuyDetail";
                SellItemUI();
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
            Inventory.SetActive(true);
            DatabaseManager.isOpenUI = true;
        }
    }
    public string state = "";
    public void DetailOff()
    {
        isBuyDetail = false;
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
        DatabaseManager.isOpenUI = false;
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
        if (rightSholderAction.triggered && LV1craftBox[nowLv].transform.childCount > nowPage + 1)
        {
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(false);
            line = 0;
            childCount = 0;
            nowPage += 1;
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(true);
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
            SetDetail();
        }
        if (leftSholderAction.triggered && 0 < nowPage)
        {
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(false);
            line = 0;
            childCount = 0;
            nowPage -= 1;
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(true);
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
            SetDetail();
        }
    }
}
