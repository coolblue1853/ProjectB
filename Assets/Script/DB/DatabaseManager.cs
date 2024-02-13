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

    public void LoadAllItemData() // ��Ÿ ������
    {


        ItemDataList = ItemSheet.Data.DataList;
        /*  ���ͳݿ��� ������ �޾ƿ��� ���.
        UnityGoogleSheet.LoadFromGoogle<int, ItemSheet.Data>((list, map) => {
            // list���� �ε��� �����Ͱ� ��� ����
            ItemDataList = list;

            if (ItemDataList.Count > 0)
            {
                Debug.Log("�����͸� ���������� �ҷ��Խ��ϴ�.");
            }
            else
            {
                Debug.Log("�����Ͱ� �����ϴ�.");
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
            Debug.Log("�����͸� �ҷ����� ���߰ų� ��ȿ���� ���� �ε����Դϴ�.");
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
                    // �Է��� ���� ��ġ�ϴ� �����͸� ã���� �ش� �ε��� ��ȯ
                    return i;
                }
            }

            // ��ġ�ϴ� �����Ͱ� ������ -1 ��ȯ
            return -1;
        }
        else
        {
            // ������ ����Ʈ�� ��������� -1 ��ȯ
            return -1;
        }
    }


}

