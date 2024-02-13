using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;

public class Item
{
    public string name { get; set; }
    public string type { get; set; }
    public string description { get; set; }
    public int price { get; set; }
    public int weight { get; set; }
    public string acqPath { get; set; }
    public int maxStack { get; set; }
    public string effectOb { get; set; }
    public int effectPow { get; set; }
    public string equipArea { get; set; }

}
public class DatabaseManager : MonoBehaviour
{
    public static bool weaponStopMove = false;
    public static bool isOpenUI = false;




    private List<ItemSheet.Data> ItemDataList;

    static public DatabaseManager instance;
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
        UnityGoogleSheet.LoadAllData();
        LoadAllItemData();
    }

    public void LoadAllItemData() // 기타 아이템
    {


        ItemDataList = ItemSheet.Data.DataList;
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

    public Item LoadItemData(int itemNum)
    {
        if (ItemDataList != null && itemNum >= 0 && itemNum < ItemDataList.Count)
        {
            Item newItem = new Item
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
                equipArea = ItemDataList[itemNum].EquipArea,
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

