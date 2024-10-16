using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
public class CraftingManager : MonoBehaviour
{
    //-105

    public GameObject detailPos;
    public GameObject[] LV1;
    public GameObject nowLine;

    public CraftItemCheck nowCraftItem;
    public NeedItem nowNeedItem;
    public TextMeshProUGUI name;
    public TextMeshProUGUI type;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;
    public TextMeshProUGUI tear;
    public TextMeshProUGUI rarity;


    public bool isSelfCraft;
    public bool isCraftWeapon;
    public Image image;

    public GameObject needMaterail;
    public GameObject itemPrefab; // 생성되는 Box 인스턴스
                                  // Start is called before the first frame update

    int nowLv = 0;
    int line = 0;
    int childCount = 0;
    public GameObject CraftUI;
    public GameObject needMaterailUI;
    public GameObject cusor;
    public GameObject craftCusor;
    public GameObject[] LV1craftBox;
    int nowPage = 0;
    public GameObject skillDetailUi;

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
        nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(0).GetComponent<CraftItemCheck>();
        nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(0).GetComponent<NeedItem>();
        if (isSelfCraft)
        {
           // SetNeedItem();
          //  SetDetail();
        }

        // Invoke("SetDetail", 1f); // 일단 1초뒤에 하도록 해서 딜레이 체크를 했는데 이거는 확인해야할듯

    }

    void SetDetail()
    {
        if(nowCraftItem.itemData == null)
        {
            nowCraftItem.SetItem(nowCraftItem.transform.name);
        }
        name.text = nowCraftItem.itemData.itemNameT;
        type.text = nowCraftItem.itemData.type;
        description.text = nowCraftItem.itemData.description;
        price.text = "Price : " + (nowCraftItem.itemData.price).ToString();
        tear.text = "T" + nowCraftItem.itemData.tear.ToString();
        rarity.text = InventoryManager.instance.SetRarity(nowCraftItem.itemData.rarity);

        Image changeImage = nowCraftItem.image;
        image.sprite = changeImage.sprite;
    }

    public void Craft()
    {
        if(NeedCountCheck() == true)
        {
            Debug.Log("제작 성공");
            for (int i = 0; i < nowNeedItem.needItem.Count; i++)
            {
                for(int j = 0; j < needItemList[i].needCount; j++)
                {
                    InventoryManager.instance.DeletItemByName(needItemList[i].itemData.name, 1);
                }

            }
            Invoke("CreatItem", 0.2f);
        }
        else
        {
            Debug.Log("제작 실패");
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

    public void ResetDetail()
    {

        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
        equipDetail.SetActive(false);
        skillDetailUi.SetActive(false);
    }
    void SetNeedDetail(bool isCraftMain = false)
    {
        if (isCraftMain)
        {
            detail = nowCraftItem.transform.GetComponent<ItemCheck>();
        }
        else
        {
            detail = needMaterail.transform.GetChild(detailCount).transform.GetComponent<ItemCheck>();
        }

        if (detail.itemData.type == "Misc")
        {
            Transform misc = miscDetail.gameObject.transform;
            misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
            misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type;
            misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
            misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
            misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
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
            consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type;
            consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
            consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
            consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
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

            string folderPath = detail.itemData.equipArea + "/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + detail.itemData.name);
            equip.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
            equip.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.itemData.itemNameT;
            equip.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.itemData.type + " : " + detail.itemData.equipArea;
            equip.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.itemData.description;
            equip.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Price : " + (detail.itemData.price).ToString();
            equip.GetChild(5).GetComponent<TextMeshProUGUI>().text = "T" + detail.itemData.tear.ToString();
            equip.GetChild(6).GetComponent<TextMeshProUGUI>().text = InventoryManager.instance.SetRarity(detail.itemData.rarity);
            InventoryManager.instance.SetEffectDetail(equip.GetChild(8).gameObject, detail);
            if (detail.itemData.equipArea != "Weapon") // 방어구라면
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
       GameObject detailOb = needMaterail.transform.GetChild(detailCount).gameObject;
       detailCusor.transform.position = new Vector2(detailOb.transform.position.x, detailOb.transform.position.y);

        if (rightInventoryAction.triggered == true)
        {
            if (needMaterailUI.transform.GetChild(0).childCount > detailCount + 1 && itemDataPrefab[detailCount + 1].gameObject.activeSelf == true)
            {
                 detailCount += 1;
                SetNeedDetail();
            }
        }
        if (leftInventoryAction.triggered == true&& isLeftCraft == false)
        {
            if (0 < detailCount)
            {
                detailCount -= 1;
                SetNeedDetail();
            }
        }
        if(downInventoryAction.triggered == true)
        {
            if (detailCount < 4 && needMaterail.transform.childCount > detailCount+ 4 && itemDataPrefab[detailCount + 4].gameObject.activeSelf == true) // 현재 0 1 2 3   4 가 있을때 
            {
                Debug.Log("작동1");
                detailCount += 4;
                SetNeedDetail();
            }
            else
            {
                miscDetail.SetActive(false);
                consumDetail.SetActive(false);
                equipDetail.SetActive(false);
                skillDetailUi.SetActive(false);
                Invoke("ChangeCheckDetail", 0.1f);
                craftCusor.SetActive(true);
                detailCusor.SetActive(false);
                cusor.SetActive(false);
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
            miscDetail.SetActive(false);
            consumDetail.SetActive(false);
            equipDetail.SetActive(false);
            skillDetailUi.SetActive(false);
            Invoke("ChangeCheckDetail", 0.1f);
            craftCusor.SetActive(true);
            detailCusor.SetActive(false);
            cusor.SetActive(false);
        }
    }
    bool isLeftCraft = false;
    void ChangeCheckDetail()
    {
        isCheckDetail = false;
    }
    // Update is called once per frame
    void Update()
    {



        if(CraftUI.activeSelf == true)
        {
            if(isCheckDetail == true)
            {
                MoveDetailCusor();
            }


                if (cusor.activeSelf == false && craftCusor.activeSelf == true)
            {
                if (selectAction.triggered == true && isCraft == true)
                {
                    ResetDetail();
                    Craft();
                }
                if ((backAction.triggered == true && isCheckDetail == false) || leftInventoryAction.triggered == true)
                {
                    isLeftCraft = true;
                    Sequence seq = DOTween.Sequence()
                     .AppendInterval(waitTime)
                     .AppendCallback(() => isLeftCraft = false);
                    ResetDetail();
                    Invoke("ChangeisCarftFalse",0.1f);

                    cusor.SetActive(true);
                    craftCusor.SetActive(false);
                }
                if(upInventoryAction.triggered == true)
                {
                    detailCount = 0;
                    GameObject detailOb = needMaterail.transform.GetChild(detailCount).gameObject;
                    detailCusor.transform.position = new Vector2(detailOb.transform.position.x, detailOb.transform.position.y);
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
                ResetDetail();
                Invoke("ChangeisCarft", 0.1f);
                cusor.SetActive(false);
                craftCusor.SetActive(true);
            }
        }

        if (isSelfCraft == true&& CraftUI.activeSelf == false && openCraftUiAction.triggered && DatabaseManager.isOpenUI == false)
        {
            DeleteAllChildren(needMaterail);
            SetNeedItem();
            SetDetail();
            Invoke("ChangeisMoveCusor", 0.2f);
            CraftUI.SetActive(true);
            needMaterailUI.SetActive(true);
            DatabaseManager.isOpenUI = true;
        }

        else if (CraftUI.activeSelf == true && backAction.triggered && cusor.activeSelf == true && isCheckDetail == false && isCraft ==false)
        {
            PlayerController.instance.DashWait2F();
            ResetDetail();
            isMoveCusor = false;
            CraftUI.SetActive(false);
            needMaterailUI.SetActive(false);
            DatabaseManager.isOpenUI = false;

        }

         if (CraftUI.activeSelf == true && changeAction.triggered && cusor.activeSelf == true ) // 만들려는 아이템의디테일창을 키는 것
        {
            SetNeedDetail(true);
        }

        if (name.text == "")
        {
            SetDetail();
        }

    }
    bool isCraft = false;

    public void WeaponOpen()
    {
          if (isSelfCraft == false && isCraftWeapon == true && CraftUI.activeSelf == false && DatabaseManager.isOpenUI == false) // 무기 제작인
        {
            PlayerController.instance.uiDashWait = true;
            DeleteAllChildren(needMaterail);
            SetNeedItem();
            SetDetail();
            Invoke("ChangeisMoveCusor", 0.2f);
            CraftUI.SetActive(true);
            needMaterailUI.SetActive(true);
            DatabaseManager.isOpenUI = true;
        }
    }
    public void EquipOpen()
    {
         if (isSelfCraft == false && isCraftWeapon == false && CraftUI.activeSelf == false && DatabaseManager.isOpenUI == false) // 장비 제작인
        {
            PlayerController.instance.uiDashWait = true;
            DeleteAllChildren(needMaterail);
            SetNeedItem();
            SetDetail();
            Invoke("ChangeisMoveCusor", 0.2f);
            CraftUI.SetActive(true);
            needMaterailUI.SetActive(true);
            DatabaseManager.isOpenUI = true;
        }
    }
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
        checkRepeat = false;
    }

    public void MoveCusorByMouse(int inline, int inchild)
    {
        line = inline;
        childCount = inchild;
        cusor.transform.position = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).position; // lv 상자의 line 줄의 count 위치.
        DeleteAllChildren(needMaterail);
        nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
        nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
        SetNeedItem();
        SetDetail();
    }
    void MoveCusor()
    {
        cusor.transform.position = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).position; // lv 상자의 line 줄의 count 위치.
        if ((rightInventoryAction.triggered) || (checkRepeat == false && horizontalInput == 1))
        {
      
            ResetDetail();
            if (LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.childCount > childCount + 1)
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                childCount += 1;
                DeleteAllChildren(needMaterail);
                nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
                nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
                SetNeedItem();
                SetDetail();
            }
        }
        if (((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1)) && isLeftCraft == false)
        {
            ResetDetail();
            if (childCount > 0)
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                childCount -= 1;
                DeleteAllChildren(needMaterail);
                nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
                nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
                SetNeedItem();
                SetDetail();
            }
        }
        if (((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1)) && LV1craftBox[nowLv].transform.GetChild(nowPage).transform.childCount > line+1)
        {
            ResetDetail();
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
            DeleteAllChildren(needMaterail);
            nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
            nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            SetNeedItem();
            SetDetail();
        }
        if (((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1)) && 0 < line)
        {
            ResetDetail();
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
            DeleteAllChildren(needMaterail);
            nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
            nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            SetNeedItem();
            SetDetail();
        }



        if (rightSholderAction.triggered && LV1craftBox[nowLv].transform.childCount > nowPage + 1)
        {
            ResetDetail();
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(false);
            line = 0;
            childCount = 0;
            nowPage += 1;
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(true);
            DeleteAllChildren(needMaterail);
            nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
            nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            SetNeedItem();
            SetDetail();

        }
        if (leftSholderAction.triggered && 0 < nowPage)
        {
            ResetDetail();
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(false);
            line = 0;
            childCount = 0;
            nowPage -= 1;
            LV1craftBox[nowLv].transform.GetChild(nowPage).transform.gameObject.SetActive(true);
            DeleteAllChildren(needMaterail);
            nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
            nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
            SetNeedItem();
            SetDetail();

        }

    }

    //DatabaseManager.inventoryItemStack[name]+"/" + needCount
    bool NeedCountCheck()
    {
        for (int i = 0; i < nowNeedItem.needItem.Count; i++)
        {
            if(DatabaseManager.inventoryItemStack.ContainsKey(needItemList[i].itemData.name) == true)
            {
                if (DatabaseManager.inventoryItemStack[needItemList[i].itemData.name] < needItemList[i].needCount)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


        }

        return true;
    }

    ItemCheck[] needItemList;
   public ItemCheck[] itemDataPrefab;
    void SetNeedItem()
    {
        needItemList = new ItemCheck[nowNeedItem.needItem.Count];
        for (int i =0; i < nowNeedItem.needItem.Count; i++)
        {
            //GameObject item = Instantiate(itemPrefab, needMaterail.transform.position, Quaternion.identity, needMaterail.transform);
            GameObject item = itemDataPrefab[i].gameObject;
            item.SetActive(true);
            ItemCheck itemCheck = item.GetComponent<ItemCheck>();
            needItemList[i] = itemCheck;
            itemCheck.SetItem(nowNeedItem.needItem[i]);
            itemCheck.needCount = nowNeedItem.needItemAmount[i];
        }
    }


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
            child.SetActive(false);
        }
    }
}
