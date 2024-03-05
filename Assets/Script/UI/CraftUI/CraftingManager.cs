using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
public class CraftingManager : MonoBehaviour
{

    public GameObject[] LV1;
    public GameObject nowLine;

    public CraftItemCheck nowCraftItem;
    public NeedItem nowNeedItem;
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

    public GameObject needMaterail;
    public GameObject itemPrefab; // 생성되는 Box 인스턴스
                                  // Start is called before the first frame update

    int nowLv = 0;
    int line = 0;
    int childCount = 0;
    public GameObject CraftUI;
    public GameObject cusor;
    public GameObject[] LV1craftBox;
    int nowPage = 0;


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
    }
    void Start()
    {
        nowLv = 0;
        line = 0;
        nowPage = 0;
        childCount = 0;
        nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(0).GetComponent<CraftItemCheck>();
        nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(0).GetComponent<NeedItem>();
        SetNeedItem();
        // Invoke("SetDetail", 1f); // 일단 1초뒤에 하도록 해서 딜레이 체크를 했는데 이거는 확인해야할듯

    }

    void SetDetail()
    {
        name.text = nowCraftItem.name;
        type.text = nowCraftItem.type;
        description.text = nowCraftItem.description;
        price.text =(nowCraftItem.price).ToString();
        weight.text = nowCraftItem.weight.ToString();
        acqPath.text = nowCraftItem.acqPath;
        Image changeImage = nowCraftItem.transform.GetChild(0).GetComponent<Image>();
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

    public void Craft()
    {
        if(NeedCountCheck() == true)
        {
            Debug.Log("제작 성공");
            for (int i = 0; i < nowNeedItem.needItem.Count; i++)
            {
                InventoryManager.instance.DeletItemByName(needItemList[i].name, needItemList[i].needCount);
            }
            InventoryManager.instance.CreatItem(name.text);
        }
        else
        {
            Debug.Log("제작 실패");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(CraftUI.activeSelf == true)
        {
            verticalInput = (verticalCheck.ReadValue<float>());
            horizontalInput = (horizontalCheck.ReadValue<float>());
            MoveCusor();
        }



        if (name.text == "")
        {
            SetDetail();
        }

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
                DeleteAllChildren(needMaterail);
                nowCraftItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<CraftItemCheck>();
                nowNeedItem = LV1craftBox[nowLv].transform.GetChild(nowPage).transform.GetChild(line).transform.GetChild(childCount).GetComponent<NeedItem>();
                SetNeedItem();
                SetDetail();
            }
        }
        if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1))
        {
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
            if(DatabaseManager.inventoryItemStack.ContainsKey(needItemList[i].name) == true)
            {
                if (DatabaseManager.inventoryItemStack[needItemList[i].name] < needItemList[i].needCount)
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
    void SetNeedItem()
    {
        needItemList = new ItemCheck[nowNeedItem.needItem.Count];
        for (int i =0; i < nowNeedItem.needItem.Count; i++)
        {
            GameObject item = Instantiate(itemPrefab, needMaterail.transform.position, Quaternion.identity, needMaterail.transform);
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
            Destroy(child);
        }
    }
}
