using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class InventoryManager : MonoBehaviour
{
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
    int detailX = 150, detailY =12;
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
        cusorImage  = cusor.GetComponent<Image>(); ;
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
        if(chest != null)
        {
            state = "chestOpen";
        }
        else
        {
            state = "";
        }
        cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
        ResetInventoryBox();
        inventoryUI[num].transform.SetAsLastSibling();
        InventoryAlpha inventoryAlpha = inventoryUI[num].GetComponent<InventoryAlpha>();
        inventoryAlpha.ChangeAlpha();
        GameObject insPositon = GetNthChildGameObject(inventoryUI[num], cusorCount[num]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = num;
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
    private void Start()
    {
        inventory.SetActive(true);
        MakeInventoryBox();
        inventoryArray = new int[maxBoxNum, 5];
        Invoke("ActiveFalse", 0.000000001f); // 인벤토리 리로드, 이 과정이 없으면 GridLayoutGroup이 정상작동하지 않음.

    }

    // 나중에 퍼블릭 해체;
    public GameObject beforBox;
    public GameObject afterBox;
    int beforeCusorInt;

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
            //int siblingParentIndex = inventoryUI[num].transform.GetSiblingIndex();

            int count = itemCheck.nowStack;

            for (int i = 0; i < count; i++)
            {
                  chest.CreatItem(itemCheck.name);
   
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
        if(chest == null)
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
        CreatItemSelected(item.name,nowBox, output);
    }

    void ActiveChangeCursor()
    {

        if (state == "" || state == "detail" || state == "chestOpen")
        {
            beforBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            if(beforBox.transform.childCount != 0)
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
                if (afterItemCheck.name == beforeItemCheck.name && (afterItemCheck.nowStack != afterItemCheck.maxStack && beforeItemCheck.nowStack != beforeItemCheck.maxStack ))
                {
                    isSame = true; // 같다면 가능한 만큼 스택을 합친다.

                    if(afterItemCheck.nowStack + beforeItemCheck.nowStack <= afterItemCheck.maxStack)
                    {
                        afterItemCheck.nowStack += beforeItemCheck.nowStack;
                        Destroy(beforitem);
                    }
                    else
                    {
                        beforeItemCheck.nowStack -= (afterItemCheck.maxStack - afterItemCheck.nowStack);
                        afterItemCheck.nowStack = afterItemCheck.maxStack;

                    }
                }
                else
                {

                    changeItem = afterBox.transform.GetChild(0).gameObject;
                    changeItem.transform.SetParent(beforBox.transform);
                    changeItem.transform.position = beforBox.transform.position;
                }
            }
            else
            {
            }
            
            if(isSame == false)
            {
                if(beforBox.transform.childCount > 0)
                {
                    beforitem = beforBox.transform.GetChild(0).gameObject;
                    beforitem.transform.SetParent(afterBox.transform);
                    beforitem.transform.position = afterBox.transform.position;
                }

            }
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
            changeCusor.SetActive(false);
            if(chest != null)
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
        for(int i = 0; i < 5; i++)
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
            else if (i < (maxHor * maxVer)*2)
            {
                if (maxBoxCusor != 2)
                {
                    maxBoxCusor = 2;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[1].transform);
            }
            else if (i < (maxHor * maxVer)*3)
            {
                if (maxBoxCusor != 3)
                {
                    maxBoxCusor = 3;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[2].transform);
            }
            else if (i < (maxHor * maxVer)*4)
            {
                if (maxBoxCusor != 4)
                {
                    maxBoxCusor = 4;
                }
                Instantiate(inventoryBoxPrefab, inventoryUI[3].transform);
            }
            else if (i < (maxHor * maxVer)*5)
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
        if(state == "detail")
        {
            if(chest == null)
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
        else if(state == "change")
        {
            if (chest == null)
            {
                state = "";
                //  BoxContentChecker();
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
        if (state == "divide")
        {
            CloseDivide();
        }
    }
    public void ExCheckMove()
    {
        checkRepeat = true;
        sequence.Kill();

        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());

    }
    void BoxOpen()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)&& inventoryUI[0].activeSelf == false && inventoryBox[0].activeSelf == true)
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

    void BoxChangeByKey()
    {
        if ((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1))
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());

            if (0 < boxCusor)
            {
                boxCusor -= 1;
            }
            else
            {
                boxCusor = maxBoxCusor - 1;
            }
            GameObject insPositon = inventoryBox[boxCusor];
            if (state == "boxChange")
            {
                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }
        }
        if ((downInventoryAction.triggered) || (checkRepeat == false && verticalInput == -1))
        {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());

            if (maxBoxCusor - 1 > boxCusor)
            {
                boxCusor += 1;
            }
            else
            {
                boxCusor = 0;
            }
            GameObject insPositon = inventoryBox[boxCusor];
            if (state == "boxChange")
            {
                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }
        }
        if (rightInventoryAction.triggered && checkRepeat == false)
       {
            checkRepeat = true;
            sequence.Kill();

            sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .OnComplete(() => ResetCheckRepeat());
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
             if (state == "boxChange")
            {
                Debug.Log("작동중");
                cusor.transform.position = insPositon.transform.position;
            }
            else if (state == "itemBoxChange")
            {
                changeCusor.transform.position = insPositon.transform.position;
            }

             if (state == "boxChange")
            {
                if (chest != null )
                {
                    state = "chestOpen";
                }
                else
                {
                    state = "";
                }
            }
            else if (state == "itemBoxChange")
            {
                state = "change";
            }
        }
        if (leftInventoryAction.triggered && checkRepeat == false)
        {
            if (chest != null && state != "itemBoxChange")
            {
                cusor.SetActive(false);
                state = "InChestMove";
                chest.isCusorChest = true;
                chest.CusorMove2Chest();
            }

        }
        if (selectAction.triggered&& state == "boxChange")
        {
            ResetInventoryBox();
            nowBox = boxCusor;
            OpenBox(nowBox);
            state = "";
            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
        }
        if (changeAction.triggered && state == "itemBoxChange" && boxCusor!=nowBox)    
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
        cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
        state = "boxChange";
        boxCusor = 0;
        GameObject insPositon = inventoryBox[boxCusor];
        cusor.transform.position = insPositon.transform.position;
    }
    private void Update()
    {
        verticalInput =(verticalCheck.ReadValue<float>());
        horizontalInput = (horizontalCheck.ReadValue<float>());
        if (inventory.activeSelf == true)
        {
            if (consumAction.triggered && state == "")
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
                else if(state == "detail")
                {
                    CloseCheck();


                }
                if(state == "I2CMove")
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
                else if(state == "I2CMove")
                {
                    state = "chestOpen";
                    cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
                }
            }
        }
        if (backAction.triggered)
        {
            CloseCheck();
        }
        if((state =="" || state =="detail"|| state  == "chestOpen"|| state == "I2CMove") && inventory.activeSelf== true)
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
        if(state == "change")
        {
            ChangeCusorChecker();
        }
        if ((state == "boxChange" || state == "itemBoxChange") && inventory.activeSelf == true)
        {
            BoxChangeByKey();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {

       
            CreatItem("Armor");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
          //  CreatItem("PickAxe");
            CreatItem("HighArmor");

        }
        if (openInventoryAction.triggered)
        {
            if (inventory.activeSelf == true)
            {
                DatabaseManager.isOpenUI = false;
                inventory.SetActive(false);
                if(chest != null)
                {
                    chest.CloseChest();
                    chest = null;
                }
                state = "";
            }
            else
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
    public bool checkRepeat= false;
    float waitTime = 0.18f;
    public Sequence sequence;
    Sequence dcSequence;
    public bool DoubleCheckController = false;
 

    public void ActiveConsum()
    {
        GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
        if(gameObject.transform.childCount > 0)
        {
            ItemCheck item = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            if (item.type == "Consum")
            {
                item.ConsumItemActive();
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
        checkRepeat = true;
        sequence.Kill();

        sequence = DOTween.Sequence()
        .AppendInterval(waitTime * 2)
        .OnComplete(() => ResetCheckRepeat());

    }

    void DoubleCheck()
    {
        DoubleCheckController = false;
    }

    void CusorChecker()
    {
        if (inventory.activeSelf == true && state != "change")
        {

            if ((rightInventoryAction.triggered  ) || (checkRepeat == false && horizontalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill(); 

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());


                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] + 1 < nowBoxMax)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] -= nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }

            }
             if ((leftInventoryAction.triggered && DoubleCheckController == false) || (checkRepeat == false && horizontalInput == -1))
            {

                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] % maxHor == 0)
                {
                    cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
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
             if ((downInventoryAction.triggered && DoubleCheckController == false) ||( checkRepeat == false && verticalInput == -1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);

                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                    Debug.Log(nowBoxMax);
                }
                if (cusorCount[nowBox] + maxHor < nowBoxMax)
                {
                    cusorCount[nowBox] += maxHor;
                 //   Debug.Log(cusorCount[nowBox]);
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = cusorCount[nowBox] % maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
             if ((upInventoryAction.triggered && DoubleCheckController == false) || (checkRepeat == false && verticalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                    Debug.Log(nowBoxMax);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                    Debug.Log(nowBoxMax);
                }
                if (cusorCount[nowBox] - maxHor >= 0)
                {
                    cusorCount[nowBox] -= maxHor;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    while (cusorCount[nowBox] + maxHor < nowBoxMax)
                    {
                        cusorCount[nowBox] += maxHor;
                    }

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
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] + 1 < nowBoxMax)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] -= nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }

            }
            if ((leftInventoryAction.triggered) || (checkRepeat == false && horizontalInput == -1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
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
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());
                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] + maxVer < nowBoxMax)
                {
                    cusorCount[nowBox] += maxVer;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = cusorCount[nowBox] % maxVer;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }
            if ((upInventoryAction.triggered) || (checkRepeat == false && verticalInput == 1))
            {
                checkRepeat = true;
                sequence.Kill();

                sequence = DOTween.Sequence()
                .AppendInterval(waitTime)
                .OnComplete(() => ResetCheckRepeat());

                int nowBoxMax = 0;
                if ((nowBox + 1) * (maxHor * maxVer) < maxBoxNum)
                {
                    nowBoxMax = (maxHor * maxVer);
                }
                else
                {
                    nowBoxMax = (maxBoxNum) - ((nowBox * (maxHor * maxVer)));
                }
                if (cusorCount[nowBox] - maxVer >= 0)
                {
                    cusorCount[nowBox] -= maxVer;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    while (cusorCount[nowBox] + maxVer < nowBoxMax)
                    {
                        cusorCount[nowBox] += maxVer;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }


        }
    }
    public void BoxContentChecker(GameObject ob = null) // Z키 클릭시 인벤토리창
    {
        if (inventoryArray[cusorCount[nowBox], nowBox] == 1 && (state == "" || state == "chestOpen"))
        {
            ItemCheck detail;
            state = "detail";

            if(ob != null)
            {
                detail = ob.transform.GetComponent<ItemCheck>();
            }
            else
            {
                GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
                //Debug.Log(gameObject.transform.GetChild(0));
                 detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
            }

            if (detail.type == "Misc")
            {
                Transform misc =  miscDetail.gameObject.transform;
                misc.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                misc.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.name;
                misc.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
                misc.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                misc.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
                misc.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
                misc.GetChild(6).GetComponent<TextMeshProUGUI>().text = detail.acqPath;
                if(ob != null)
                {
                    miscDetail.transform.localPosition = new Vector2(ob.transform.localPosition.x - detailX, detailY);
                }
                else
                {
                    miscDetail.transform.localPosition = new Vector2(cusor.transform.localPosition.x - detailX, detailY);
                }
                miscDetail.SetActive(true);
               // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);
            }
            if (detail.type == "Consum")
            {
                   Transform  consum = consumDetail.gameObject.transform;
                consum.GetChild(0).GetComponent<Image>().sprite = detail.image.sprite;
                consum.GetChild(1).GetComponent<TextMeshProUGUI>().text = detail.name;
                consum.GetChild(2).GetComponent<TextMeshProUGUI>().text = detail.type;
                consum.GetChild(3).GetComponent<TextMeshProUGUI>().text = detail.description;
                consum.GetChild(4).GetComponent<TextMeshProUGUI>().text = (detail.price).ToString();
                consum.GetChild(5).GetComponent<TextMeshProUGUI>().text = detail.weight.ToString();
                string[] effectString = detail.effectOb.Split();
                consum.GetChild(6).GetComponent<TextMeshProUGUI>().text = effectString[0] + " : "+ effectString[1] + detail.effectPow;
                if (ob != null)
                {
                    consumDetail.transform.localPosition = new Vector2(ob.transform.localPosition.x - detailX, detailY);
                }
                else
                {
                    consumDetail.transform.localPosition = new Vector2(cusor.transform.localPosition.x - detailX, detailY);
                }
                consumDetail.SetActive(true);
                // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);
            }
        }


    }

    public void CreatItem(string itemName, bool isC2I = false)
    {
        if(CheckStack(itemName) == false)
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

                            GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                            GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                            ItemCheck check = item.GetComponent<ItemCheck>();
                            check.SetItem(itemName);

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
        Debug.Log("체크중1");
        state = "";
    }
    public void ChangeCusor(GameObject ob)
    {
        cusorCount[nowBox] = ob.transform.GetSiblingIndex();
        cusor.transform.position = ob.transform.position;
        cusor.SetActive(true);
        if(chest != null)
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            chest.changeCusor.SetActive(false);
            chest.cusor.SetActive(false);
            chest.isCusorChest = false;
            state = "chestOpen";
        }
        else
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            state = "";
        }

    }


    bool CheckStack(string itemName)
    {


        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < (maxHor * maxVer); i++)
            {
                if (maxBoxNum > (j * (maxHor * maxVer)) + i)
                {
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[j], i);
                    if(insPositon.transform.childCount > 0)
                    {
                        GameObject item = insPositon.transform.GetChild(0).gameObject;
                        ItemCheck check = item.GetComponent<ItemCheck>();
                        if (check.name == itemName)
                        {
                            if (check.maxStack > check.nowStack)
                            {
                                check.nowStack += 1;
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
                    float scaleFactor = 0.8f; // 크기를 조절할 비율
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
        if ((state == "chestOpen" || chest != null)&& state != "I2CMove")
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            state = "chestOpen";
        }
        else if (state != "chestOpen" && state != "I2CMove")
        {
            cusorImage.color = new Color(161f / 255f, 22f / 255f, 22f / 255f);
            state = "";
        }

        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
    }
    void OnlyDetailObOff()
    {
        miscDetail.SetActive(false);
        consumDetail.SetActive(false);
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
        changeCusor.SetActive(false);
        state = "";
    }
}
