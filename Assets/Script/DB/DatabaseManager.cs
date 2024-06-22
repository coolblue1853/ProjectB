

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;

public class Item
{


}
public class DatabaseManager : MonoBehaviour
{
    public static bool isSuperArmor = false;
    public static int attackSpeedBuff = 0;
    public static int SpeedBuff = 0;
    public static int playerDef = 0;
    public static int playerCritRate = 0;
    public static int playerDropRate= 0;
    public static int hitCount = 0; // ���� ������Ʈ�� Ÿ��
    public static bool isInvincibility = false;
    public static bool weaponStopMove = false;
    public static bool checkAttackLadder = false;
    public static bool isOpenUI = false;
    public static bool isUsePortal = false;
    public static Dictionary<string, int> inventoryItemStack = new Dictionary<string, int>();
    public static Dictionary<string, int> setsEffectStack = new Dictionary<string, int>();
    ItemDatabase itemDatabase;
    SetItem setDatabase;
    public static int money;


    public static void PlusSetDict(string name, int count)
    {

        bool isContain = false;
        foreach (var value in setsEffectStack)
        {
            if (value.Key == name)
            {
                isContain = true;
                break;
            }
        }
        if (isContain == false)
        {
            setsEffectStack[name] = count;
        }
        else
        {
            setsEffectStack[name] += count;
        }

        SetEffectActvie(name, setsEffectStack[name]);

    }
    public static void MinusSetDict(string name, int count)
    {
        SetEffectDisactvie(name, setsEffectStack[name]);
        setsEffectStack[name] -= count;
        // ���� ���� 0�� �Ǹ� �ش� Ű�� ������ �� ����
        if (setsEffectStack[name] == 0)
        {
            setsEffectStack.Remove(name);
        }

    }


    public static void SetEffectActvie(string name, int level)
    {
        SetItem setItem = new SetItem();
        setItem.CheckSetEffect(name, level);
    }
    public static void SetEffectDisactvie(string name, int level)
    {
        SetItem setItem = new SetItem();
     setItem.CheckSetDisEffect(name, level);
    }


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
            inventoryItemStack[name] = count;
        }
        else
        {
            inventoryItemStack[name] += count;
        }
    }

    public static void MinusInventoryDict(string name, int count)
    {
        inventoryItemStack[name] -= count;
        // ���� ���� 0�� �Ǹ� �ش� Ű�� ������ �� ����
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
        setDatabase = this.GetComponent<SetItem>();
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

    public void LoadAllItemData() // ��Ÿ ������
    {
        ItemDataList = itemDatabase.items;
        setsDataList = setDatabase.items;
        //    ItemDataList = ItemSheet.Data.DataList;

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
                tfName = ItemDataList[itemNum].tfName,
                itemNameT = ItemDataList[itemNum].itemNameT,
                tear = ItemDataList[itemNum].tear,
                rarity = ItemDataList[itemNum].rarity,
                upgrade = ItemDataList[itemNum].upgrade,
                setName = ItemDataList[itemNum].setName,
            };
            return newItem;


        }
        else
        {
            Debug.Log("�����͸� �ҷ����� ���߰ų� ��ȿ���� ���� �ε����Դϴ�.");
            return null;
        }
    }
    private List<SetData> setsDataList;
    public SetData LoadSetsData(int itemNum)
    {
        if (setsDataList != null && itemNum >= 0 && itemNum < setsDataList.Count)
        {
            SetData newItem = new SetData
            {
                setName = setsDataList[itemNum].setName,
                effect = setsDataList[itemNum].effect,
    
            };
            return newItem;


        }
        else
        {
      //      Debug.Log("�����͸� �ҷ����� ���߰ų� ��ȿ���� ���� �ε����Դϴ�.");
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

            Debug.Log("ã�� ���߽��ϴ�");
            // ��ġ�ϴ� �����Ͱ� ������ -1 ��ȯ
            return -1;
        }
        else
        {
            // ������ ����Ʈ�� ��������� -1 ��ȯ
            return -1;
        }
    }

    public int FindSetsDataIndex(string targetValue)
    {
        if (setsDataList != null)
        {
            for (int i = 0; i < setsDataList.Count; i++)
            {
                if (setsDataList[i].setName == targetValue)
                {
                    // �Է��� ���� ��ġ�ϴ� �����͸� ã���� �ش� �ε��� ��ȯ
                    return i;
                }
            }

//            Debug.Log("ã�� ���߽��ϴ�");
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


