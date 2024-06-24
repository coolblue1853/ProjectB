using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ShopManager : MonoBehaviour
{
    //-105
    public GameObject detailPos;
    int detailX = -105;
    int detailY = 22;
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
                                  // Start is called before the first frame update

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
     //   SetNeedItem(); 
        // Invoke("SetDetail", 1f); // 일단 1초뒤에 하도록 해서 딜레이 체크를 했는데 이거는 확인해야할듯

    }

    void SetDetail()
    {
        name.text = nowShopItem.name;
        type.text = nowShopItem.type;
        description.text = nowShopItem.description;
        price.text =(nowShopItem.Buyprice).ToString();
        weight.text = nowShopItem.weight.ToString();
        acqPath.text = nowShopItem.acqPath;
        Image changeImage = nowShopItem.transform.GetChild(0).GetComponent<Image>();
        image.sprite = changeImage.sprite;
        if (type.text == "Consum")
        {
          //  effectOb.text = nowCraftItem.effectOb;
         //   effectPow.text = nowCraftItem.effectPow.ToString();
        }
        if (type.text == "Equip")
        {
          //  equipArea.text = nowCraftItem.equipArea;
        }

    }


    void CreatItem()
    {
        InventoryManager.instance.CreatItem(name.text);
    }
    public GameObject miscDetail;
    public GameObject consumDetail;
    public GameObject equipDetail;
    ItemCheck detail;
    public bool isBuyDetail;
    void SetNeedDetail()
    {
        //nowShopItem.type 
        isBuyDetail = true;
        if (nowShopItem.type == "Misc")
        {
            Transform misc = miscDetail.gameObject.transform;
            misc.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemNameT;
            misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = nowShopItem.type;
            misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.description;
            misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (nowShopItem.price).ToString();
            misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.tear.ToString();
            misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.rarity);
            miscDetail.transform.position = detailPos.transform.position;


            miscDetail.SetActive(true);
            consumDetail.SetActive(false);
            equipDetail.SetActive(false);
            skillDetailUi.SetActive(false);
        }
        if (nowShopItem.type == "Consum")
        {
            Transform consum = consumDetail.gameObject.transform;
            consum.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemNameT;
            consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = nowShopItem.type;
            consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.description;
            consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (nowShopItem.price).ToString();
            consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.tear.ToString();
            consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.rarity);
            InventoryManager.instance.SetConsumEffect(consum.GetChild(7).gameObject, nowShopItem);


            consum.transform.position = detailPos.transform.position;
            miscDetail.SetActive(false);
            consumDetail.SetActive(true);
            equipDetail.SetActive(false);
            skillDetailUi.SetActive(false);
        }
        if (nowShopItem.type == "Equip")
        {
            Transform equip = equipDetail.gameObject.transform;

            string folderPath = nowShopItem.equipArea + "/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + nowShopItem.name);
            equip.GetChild(0).GetComponent<Image>().sprite = nowShopItem.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = nowShopItem.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = nowShopItem.type + " : " + nowShopItem.equipArea;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.description;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = nowShopItem.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (nowShopItem.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + nowShopItem.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(nowShopItem.rarity);
            InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, nowShopItem);
            if (nowShopItem.equipArea != "Weapon") // 방어구라면
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
    void MoveDetailCusor()
    {
    //   GameObject detailOb = needMaterail.transform.GetChild(detailCount).gameObject;
     //  detailCusor.transform.position = new Vector2(detailOb.transform.position.x, detailOb.transform.position.y);

        if (rightInventoryAction.triggered == true)
        {
            if (needMaterailUI.transform.GetChild(0).childCount > detailCount + 1)
            {
                 detailCount += 1;
                SetNeedDetail();
            }
        }
        if (leftInventoryAction.triggered == true)
        {
            if (0 < detailCount)
            {
                detailCount -= 1;
                SetNeedDetail();
            }
        }
        if(downInventoryAction.triggered == true)
        {
            if(detailCount < 4) // 현재 0 1 2 3   4 가 있을때 
            {
                detailCount += 4;
                SetNeedDetail();
            }
            else
            {
                if (detailCount >= 4 )
                {
                    miscDetail.SetActive(false);
                    consumDetail.SetActive(false);
                    equipDetail.SetActive(false);
                    Invoke("ChangeCheckDetail", 0.1f);
                    craftCusor.SetActive(true);
                    detailCusor.SetActive(false);
                    cusor.SetActive(false);
                }

            }

        }
        if (upInventoryAction.triggered == true)
        {
            if (detailCount >= 4 ) // 현재 0 1 2 3   4 가 있을때 
            {
                detailCount -= 4;
                SetNeedDetail();
            }
        }

        if(backAction.triggered == true)
        {
          //  miscDetail.SetActive(false);
          //  consumDetail.SetActive(false);
          //  equipDetail.SetActive(false);
          //  Invoke("ChangeCheckDetail", 0.1f);
           // craftCusor.SetActive(true);
          ////  detailCusor.SetActive(false);
           // cusor.SetActive(false);
        }
    }
    void ChangeCheckDetail()
    {
        isCheckDetail = false;
    }

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
        totalSellPrice = (int)(sellSlider.value * nowShopItem.Buyprice);
        sellText.text = "Buy " + nowShopItem.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void OnSliderValueChanged(float value)
    {
        output = Mathf.RoundToInt(value);

    }
    void RightMove()
    {
        Debug.Log("작동");
        checkRepeat = true;
        sequence.Kill();
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        sellSlider.value += 1;
        totalSellPrice = (int)(sellSlider.value * nowShopItem.Buyprice);
        sellText.text = "Buy " + nowShopItem.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }
    void LeftMove()
    {
        checkRepeat = true;
        sequence.Kill();
        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        sellSlider.value -= 1;
        totalSellPrice = (int)(sellSlider.value * nowShopItem.Buyprice);
        sellText.text = "Buy " + nowShopItem.itemNameT + " " + sellSlider.value + "EA,  Price : " + totalSellPrice;
    }

    void InitializeSlider()
    {
        sellSlider.onValueChanged.AddListener(OnSliderValueChanged);
        sellSlider.maxValue = Mathf.Min(nowShopItem.maxStack, DatabaseManager.money / nowShopItem.Buyprice);
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
                DatabaseManager.money -= nowShopItem.Buyprice;
                InventoryManager.instance.CreatItem(nowShopItem.name);
            }
        }


        InventoryManager.instance.state = "ShopCusorOn";
        sellUIGameObject.SetActive(false);
        state = "";
        cusor.SetActive(true);
        isMoveCusor = true;
        DetailOff();
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
            };
            if (selectAction.triggered && DatabaseManager.money >= nowShopItem.Buyprice)
            {
                BuyItem();
            }
        }

        if (ShopUI.activeSelf == true && InventoryManager.instance.state == "ShopCusorOn")
        {
            if(isCheckDetail == true)
            {
               // MoveDetailCusor();
            }


                if (cusor.activeSelf == false && craftCusor.activeSelf == true)
            {
                if (selectAction.triggered == true && isCraft == true)
                {
                    //Craft();
                }
                if ((backAction.triggered == true && isCheckDetail == false) || leftInventoryAction.triggered == true)
                {
                 //   Invoke("ChangeisCarftFalse",0.1f);

                  //  cusor.SetActive(true);
                   // craftCusor.SetActive(false);
                }
                if(upInventoryAction.triggered == true)
                {
                    detailCount = 0;
                  //  GameObject detailOb = needMaterail.transform.GetChild(detailCount).gameObject;
                  //  detailCusor.transform.position = new Vector2(detailOb.transform.position.x, detailOb.transform.position.y);
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
            else if(isBuyDetail == true && selectAction.triggered == true && DatabaseManager.money >= nowShopItem.Buyprice)
            {
                InventoryManager.instance.state = "BuyDetail";
                SellItemUI();
            }
        }





        // 아래는 UI를 키고 끄는 역할
        if (ShopUI.activeSelf == false && Input.GetKeyDown(KeyCode.P) && DatabaseManager.isOpenUI == false)
        {
            InventoryManager.instance.inventoryUI[0].SetActive(true);
            InventoryManager.instance.nowBox = 0;
            InventoryManager.instance.state = "";
            InventoryManager.instance.boxCusor = 0;
            InventoryManager.instance.ResetBoxOrigin();
            InventoryManager.instance.cusor.SetActive(true);
            ResetShopUi();
            Invoke("ChangeisMoveCusor", 0.2f);
            ShopUI.SetActive(true);
            Inventory.SetActive(true);

          //  needMaterailUI.SetActive(true);
            DatabaseManager.isOpenUI = true;
        }
        else if (ShopUI.activeSelf == true && (backAction.triggered ||Input.GetKeyDown(KeyCode.P))&& InventoryManager.instance.state != "Sell" )
        {

        }


        if (name.text == "")
        {
           // cusor.SetActive(false);
            SetDetail();
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
    void ResetShopUi()
    {

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
       // DeleteAllChildren(needMaterail);
        nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
     //   nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
        //SetNeedItem();
        SetDetail();
    }
    void MoveCusor()
    {
        cusor.transform.position = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).position; // lv 상자의 line 줄의 count 위치.
        if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
        {
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.childCount > childCount + 1)
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                childCount += 1;
               // DeleteAllChildren(needMaterail);
                nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
              //  nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
                //SetNeedItem();
                SetDetail();
                DetailOff();
            }
            else
            {
                InventoryManager.instance.checkRepeat = false;
                InventoryManager.instance.ExCheckMove();
                InventoryManager.instance.CusorChest2Inven();
                cusor.SetActive(false);
                DetailOff();
            }
        }
        if (((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1)) && leftKeyCheck == false)
        {
            if (childCount > 0)
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                childCount -= 1;
              //  DeleteAllChildren(needMaterail);
                nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
                //nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
                //SetNeedItem();
                SetDetail();
                DetailOff();
            }
        }
        if (((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1)) && LV1craftBox[nowLv].transform.GetChild(nowPage).transform.childCount > line+1)
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line+1).transform.childCount < childCount+1)  //. 1, 2번째줄의 자식카운트 = 2 > 내카운트 2+1
            {
                childCount = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line+1).transform.childCount - 1;
            }
            line += 1;
         //   DeleteAllChildren(needMaterail);
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
            //nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            //SetNeedItem();
            SetDetail();
            DetailOff();
        }
        if (((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1)) && 0 < line)
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line-1).transform.childCount < childCount +1) //  저게 2     지금이 3
            {
                childCount = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line-1).transform.childCount - 1;
            }
            line -= 1;
           // DeleteAllChildren(needMaterail);
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
           // nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            //SetNeedItem();
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
         //   DeleteAllChildren(needMaterail);
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
        //    nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            //SetNeedItem();
            SetDetail();

        }
        if (leftSholderAction.triggered && 0 < nowPage)
        {
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(false);
            line = 0;
            childCount = 0;
            nowPage -= 1;
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(true);
         //   DeleteAllChildren(needMaterail);
            nowShopItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<ItemCheck>();
        //    nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            //SetNeedItem();
            SetDetail();

        }

    }

    //DatabaseManager.inventoryItemStack[name]+"/" + needCount




    void DeleteAllChildren(GameObject parent)
    {
        // 부모 GameObject의 자식 GameObject를 모두 가져와서 배열에 저장
        GameObject[] children = new GameObject[parent.transform.childCount];
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            children[i] = parent.transform.GetChild(i).gameObject;
        }

        // 배열에 저장된 모든 자식 GameObject를 삭제
        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }
}
