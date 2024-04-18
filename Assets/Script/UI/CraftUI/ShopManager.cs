using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ShopManager : MonoBehaviour
{
    //-105

    int detailX = -105;
    int detailY = 22;

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
        price.text =(nowShopItem.price).ToString();
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
    void SetNeedDetail()
    {
        if (detail.type == "Misc")
        {
            Transform misc = miscDetail.gameObject.transform;
            misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.name;
            misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
            misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
            misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
            misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = detail.acqPath;
            miscDetail.transform.localPosition = new Vector2(detailX, detailY);
            miscDetail.SetActive(true);
            consumDetail.SetActive(false);
            equipDetail.SetActive(false);
        }
        if (detail.type == "Consum")
        {
            Transform consum = consumDetail.gameObject.transform;
            consum.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.name;
            consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
            consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
            consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
            string[] effectString = detail.effectOb.Split();
            consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = effectString[0] + " : " + effectString[1] + detail.effectPow;
            consumDetail.transform.localPosition = new Vector2(detailX, detailY);
            miscDetail.SetActive(false);
            consumDetail.SetActive(true);
            equipDetail.SetActive(false);
        }
        if (detail.type == "Equip")
        {
            Transform equip = equipDetail.gameObject.transform;
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.name;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = detail.acqPath;
            equipDetail.transform.localPosition = new Vector2(detailX, detailY);
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
    // Update is called once per frame
    void Update()
    {



        if(ShopUI.activeSelf == true && InventoryManager.instance.state == "ShopCusorOn")
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
            if (cusor.activeSelf == true && isMoveCusor == true)
            {
                verticalInput = (verticalCheck.ReadValue<float>());
                horizontalInput = (horizontalCheck.ReadValue<float>());
                MoveCusor();
            }

            if (cusor.activeSelf == true && craftCusor.activeSelf == false && selectAction.triggered == true)
            {
                Invoke("ChangeisCarft", 0.1f);
                cusor.SetActive(false);
                craftCusor.SetActive(true);
            }
        }





        // 아래는 UI를 키고 끄는 역할
        if (ShopUI.activeSelf == false && Input.GetKeyDown(KeyCode.P) && DatabaseManager.isOpenUI == false)
        {
            InventoryManager.instance.inventoryUI[0].SetActive(true);
            InventoryManager.instance.nowBox = 0;
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
    bool isMoveCusor = false;
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
            }
            else
            {
                InventoryManager.instance.checkRepeat = false;
                InventoryManager.instance.ExCheckMove();
                InventoryManager.instance.CusorChest2Inven();
                cusor.SetActive(false);
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
