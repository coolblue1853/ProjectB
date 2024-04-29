using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;

public class Item
{


}
public class DatabaseManager : MonoBehaviour
{
    public static bool isInvincibility = false;
    public static bool weaponStopMove = false;
    public static bool isOpenUI = false;
    public static bool isUsePortal = false;
    public static Dictionary<string, int> inventoryItemStack = new Dictionary<string, int>();
    ItemDatabase itemDatabase;
    public static int money;
    public static void PlusInventoryDict(string name, int count)
    {
        bool isContain = false;
        foreach(var value in inventoryItemStack)
        {
            if(value.Key == name)
            {
                isContain = true;
                break;
            }
        }
        if(isContain == false)
        {
         //   Debug.Log("Add1");
            //inventoryItemStack.Add(name,count);
            inventoryItemStack[name] = count;
        }
        else
        {
         //   Debug.Log("Add2");
            inventoryItemStack[name] += count;
        }
    }

    public static void MinusInventoryDict(string name, int count)
    {
        inventoryItemStack[name] -= count;
        // 만약 값이 0이 되면 해당 키를 제거할 수 있음
        if (inventoryItemStack[name] == 0)
        {
            inventoryItemStack.Remove(name);
        }

    }






    static public DatabaseManager instance;
    private void Awake()
    {
        money += 1800;
        itemDatabase = this.GetComponent<ItemDatabase>();
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
    //    ItemSheet.Data.Load();
       // UnityGoogleSheet.LoadAllData();
        LoadAllItemData();
    }

    public void LoadAllItemData() // 기타 아이템
    {
        ItemDataList = itemDatabase.items;
        //    ItemDataList = ItemSheet.Data.DataList;

        /*  인터넷에서 정보를 받아오는 방법.
        UnityGoogleSheet.LoadFromGoogle<int, ItemSheet.Data>((list, map) => {
            // list에는 로드한 데이터가 들어 있음
            ItemDataList = list;

            if (ItemDataList.Count > 0)
            {
                Debug.Log("데이터를 성공적으로 불러왔습니다.");
            }
            else
            {
                Debug.Log("데이터가 없습니다.");
            }
        }, true);
        */
    }
    private List<ItemData> ItemDataList;
    public ItemData LoadItemData(int itemNum)
    {
        if (ItemDataList != null && itemNum >= 0 && itemNum < ItemDataList.Count)
        {
            ItemData newItem = new ItemData
            {
                name = ItemDataList[itemNum].name,
                type = ItemDataList[itemNum].type,
                description = ItemDataList[itemNum].description,
                price = ItemDataList[itemNum].price,
                weight = ItemDataList[itemNum].weight,
                acqPath = ItemDataList[itemNum].acqPath,
                maxStack = ItemDataList[itemNum].maxStack,
                effectOb = ItemDataList[itemNum].effectOb,
                effectPow = ItemDataList[itemNum].effectPow,
                equipArea = ItemDataList[itemNum].equipArea,
            };
            return newItem;


        }
        else
        {
            Debug.Log("데이터를 불러오지 못했거나 유효하지 않은 인덱스입니다.");
            return null;
        }
    }

    public int FindItemDataIndex(string targetValue)
    {
        if (ItemDataList != null)
        {
            for (int i = 0; i < ItemDataList.Count; i++)
            {
                if (ItemDataList[i].name == targetValue)
                {
                    // 입력한 값과 일치하는 데이터를 찾으면 해당 인덱스 반환
                    return i;
                }
            }

            Debug.Log("찾지 못했습니다");
            // 일치하는 데이터가 없으면 -1 반환
            return -1;
        }
        else
        {
            // 데이터 리스트가 비어있으면 -1 반환
            return -1;
        }
    }


}


