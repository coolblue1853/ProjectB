using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab; // 생성되는 Box 인스턴스
    public GameObject itemPrefab; // 생성되는 Box 인스턴스
    public GameObject[] inventoryUI;
    public GameObject[] inventoryBox;

    public GameObject cusor; // 인벤토리 커서
    public GameObject changeCusor; // 인벤토리 커서
    int[] cusorCount = new int[5]; // 인벤토리 커서
     
    public int maxBoxNum;
    int[,] inventoryArray;

    public int nowBox;  // 이것으로 배열의 값을 읽어오면 됨. 즉, nowBox가 2일때의 15번재는 30 +15 -> 45번째 칸에 있는 아이템이라는 말이 됨.
    public GameObject inventory;
    public GameObject miscDetail;
    int detailX = 150, detailY =12;
    static public InventoryManager instance;
    public string state;
    private void Awake()
    {
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
            inventoryUI[i].SetActive(false);
        }
    }
    public void OpenBox(int num) 
    {
        ResetInventoryBox();
        inventoryUI[num].SetActive(true);
        GameObject insPositon = GetNthChildGameObject(inventoryUI[num], cusorCount[num]);
        cusor.transform.position = insPositon.transform.position;
        nowBox = num;
    }

    public bool CheckBoxCanCreat(int num)
    {
        bool isCreat = false;
        for (int i = 0; i < 30; i++)
        {
            if(inventoryArray[i, num] == 0 && num * 30+ i <maxBoxNum )
            {
                isCreat = true;
  
                break;
            }

        }
        if(isCreat == true)
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
    void ActiveChangeCursor()
    {

        if (state == "detail")
        {
            beforeCusorInt = cusorCount[nowBox];
            beforBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            changeCusor.transform.position = cusor.transform.position;
            changeCusor.SetActive(true);
            OnlyDetailObOff();
            state = "change";

        }
        else if (state == "change")
        {
            afterBox = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
         
            if(afterBox.transform.childCount != 0)
            {
                GameObject changeItem = afterBox.transform.GetChild(0).gameObject;
                changeItem.transform.SetParent(beforBox.transform);
                changeItem.transform.position = beforBox.transform.position;
            }
            else
            {
                ResetArray(nowBox, beforeCusorInt);
            }
            GameObject beforitem = beforBox.transform.GetChild(0).gameObject;
            beforitem.transform.SetParent(afterBox.transform);
            beforitem.transform.position = afterBox.transform.position;
            MoveItem(cusorCount[nowBox], nowBox );

            GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
            cusor.transform.position = insPositon.transform.position;
            changeCusor.SetActive(false);
            state = "";

        }
    }
    void BoxChange()
    {
         if (Input.GetKeyDown(KeyCode.Alpha2) && nowBox != 1)
        {
            GameObject itemBox = GetNthChildGameObject(inventoryUI[nowBox], beforeCusorInt);
            GameObject item = itemBox.transform.GetChild(0).gameObject;
            ItemCheck itemCheck = item.transform.GetComponent<ItemCheck>();
            int siblingParentIndex = inventoryUI[1].transform.GetSiblingIndex();

            Debug.Log(siblingParentIndex);
            if (nowBox != siblingParentIndex)
            {
                if (CheckBoxCanCreat(siblingParentIndex))
                {
                    ResetArray(nowBox, beforeCusorInt);
                    CreatItemSelected(itemCheck.name, siblingParentIndex, itemCheck.nowStack);
                    Destroy(item.gameObject);
                    cusorCount[nowBox] = beforeCusorInt;
                    changeCusor.SetActive(false);
                    state = "";
                }
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
        inventoryUI[0].SetActive(true);
    }

    void MakeInventoryBox()
    {
        for (int i = 0; i < maxBoxNum; i++)
        {
            if (i < 30)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[0].transform);
            }
            else if (i < 60)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[1].transform);
            }
            else if (i < 90)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[2].transform);
            }
            else if (i < 120)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[3].transform);
            }
            else if (i < 150)
            {
                Instantiate(inventoryBoxPrefab, inventoryUI[4].transform);
            }

        }
    }
    void CloseCheck()
    {
        if(state == "detail")
        {
            state = "";
            DetailOff();
        }
        else if(state == "change")
        {
             state = "";
            BoxContentChecker();
            cusorCount[nowBox] = beforeCusorInt;
            changeCusor.SetActive(false);
        }
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
    private void Update()
    {

        if(state == "")
        {
            BoxOpen();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCheck();
        }
        CusorChecker();
        ChangeCusorChecker();
        if (Input.GetKeyDown(KeyCode.X) && inventory.activeSelf == true)
        {
            ActiveChangeCursor();
        }
        if(state == "change" && inventory.activeSelf == true)
        {
            BoxChange();
        }

        if (inventory.activeSelf== true)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                BoxContentChecker();
            }


        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            CreatItem("Wood");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CreatItem("Rock");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventory.activeSelf == true)
            {
                inventory.SetActive(false);
            }
            else
            {
                inventory.SetActive(true);
            }
        }
    }
    void CusorChecker()
    {
        if(inventory.activeSelf == true && state != "change")
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] + 1 < nowBoxMax)
                {
                    cusorCount[nowBox] += 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] -= nowBoxMax-1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }

            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] - 1 >=0)
                {
                    cusorCount[nowBox] -= 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] + 6 < nowBoxMax)
                {
                    cusorCount[nowBox] += 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = cusorCount[nowBox] % 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                DetailOff();
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] - 6 >= 0)
                {
                    cusorCount[nowBox] -= 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    while (cusorCount[nowBox] +6 < nowBoxMax)
                    {
                        cusorCount[nowBox] +=6;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    cusor.transform.position = insPositon.transform.position;
                }
            }


        }
    }
    void ChangeCusorChecker()
    {
        if (inventory.activeSelf == true && state == "change")
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
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
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] - 1 >= 0)
                {
                    cusorCount[nowBox] -= 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = nowBoxMax - 1;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] + 6 < nowBoxMax)
                {
                    cusorCount[nowBox] += 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    cusorCount[nowBox] = cusorCount[nowBox] % 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int nowBoxMax = 0;
                if ((nowBox + 1) * 30 < maxBoxNum)
                {
                    nowBoxMax = 30;
                }
                else
                {
                    nowBoxMax = ((nowBox + 1) * 30) - maxBoxNum;
                }
                if (cusorCount[nowBox] - 6 >= 0)
                {
                    cusorCount[nowBox] -= 6;
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
                else
                {
                    while (cusorCount[nowBox] + 6 < nowBoxMax)
                    {
                        cusorCount[nowBox] += 6;
                    }

                    GameObject insPositon = GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]);
                    changeCusor.transform.position = insPositon.transform.position;
                }
            }


        }
    }
    void BoxContentChecker() // Z키 클릭시 인벤토리창
    {
        if(inventoryArray[cusorCount[nowBox], nowBox] == 1 && state == "")
        {
            state = "detail";
            //inventoryArray[cusorCount[nowBox], nowBox] = 0;     현재 커서가 있는 칸 , 현재 박스
            //  RemoveFirstChild(GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));  현재 커서가 있는 박스에 접근할때 필요한 숫자들.

            GameObject gameObject = (GetNthChildGameObject(inventoryUI[nowBox], cusorCount[nowBox]));
            //Debug.Log(gameObject.transform.GetChild(0));
            ItemCheck detail = gameObject.transform.GetChild(0).GetComponent<ItemCheck>();
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
                miscDetail.transform.localPosition = new Vector2 (cusor.transform.localPosition.x- detailX, detailY);
                miscDetail.SetActive(true);
               // Debug.Log(detail.name + " " + detail.type + " " + detail.description + " " + detail.price + " " + detail.weight + " " + detail.acqPath);
            }
        }


    }

    public void CreatItem(string itemName)
    {
        if(CheckStack(itemName) == false)
        {
            bool isCreate = false;
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 30; i++)
                {
                    if (maxBoxNum > (j * 30) + i)
                    {
                        if (inventoryArray[i, j] == 0)
                        {
                            inventoryArray[i, j] = 1; // i번째 위치한 인벤토리 창 열기.
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


    }


    bool CheckStack(string itemName)
    {


        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 30; i++)
            {
                if (maxBoxNum > (j * 30) + i)
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

        for (int i = 0; i < 30; i++)
        {
            if (maxBoxNum > (boxNum * 30) + i)
            {
                if (inventoryArray[i, boxNum] == 0)
                {
                    inventoryArray[i, boxNum] = 1; // i번째 위치한 인벤토리 창 열기.
                    GameObject insPositon = GetNthChildGameObject(inventoryUI[boxNum], i);
                    GameObject item = Instantiate(itemPrefab, insPositon.transform.position, Quaternion.identity, insPositon.transform);
                    ItemCheck check = item.GetComponent<ItemCheck>();

                    check.SetItem(itemName);
                    check.nowStack = Stack;
                    break;
                }
            }
        }

    }
    public void TestDelet(int uiNum, int boxNum)
    {
        inventoryArray[boxNum, uiNum] = 0;
        RemoveFirstChild(GetNthChildGameObject(inventoryUI[uiNum], boxNum));
    }
    public void ResetArray(int uiNum, int boxNum)
    {
        inventoryArray[boxNum, uiNum] = 0;
    }



    public void MoveItem(int uiNum, int boxNum)
    {
        inventoryArray[uiNum , boxNum] = 1;
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

    void DetailOff()
    {
        state = "";
        miscDetail.SetActive(false);
    }
    void OnlyDetailObOff()
    {
        miscDetail.SetActive(false);
    }
}
