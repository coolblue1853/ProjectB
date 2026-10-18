

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGS;

public class Item
{


}
public class DatabaseManager : MonoBehaviour
{
    // 상태 플래그는 PlayerStatus 토큰의 facade다. 대입은 레거시 토큰 1개만 켜고 끈다 (B-114, PlayerStateDesign Phase 1)
    public static bool isSuperArmor { get => PlayerStatus.Has(StatusFlag.SuperArmor); set => PlayerStatus.SetLegacy(StatusFlag.SuperArmor, value); }
    public static int attackSpeedBuff = 0;
    public static int addbasicDmg = 0;
    public static int SpeedBuff = 0;
    public static int playerDef = 0;
    public static int playerCritRate = 0;
    public static int playerCritDmgRate = 0;
    public static int addPoisonDmg = 0;
    public static int incDmg = 0; // 증뎀
    public static int incIncomingDmg; // 받는 피해 증가
    public static int bleedingAddDmg = 0; // 출혈시 증뎀
    public static int playerDropRate= 0;
    public static int hitCount = 0; // 공격 오브젝트의 타수
    public static bool isInvincibility { get => PlayerStatus.Has(StatusFlag.Invincible); set => PlayerStatus.SetLegacy(StatusFlag.Invincible, value); }
    public static bool weaponStopMove { get => PlayerStatus.Has(StatusFlag.MoveLock); set => PlayerStatus.SetLegacy(StatusFlag.MoveLock, value); }
    public static bool checkAttackLadder { get => PlayerStatus.Has(StatusFlag.LadderLock); set => PlayerStatus.SetLegacy(StatusFlag.LadderLock, value); }
    public static bool isOpenUI { get => PlayerStatus.Has(StatusFlag.InputLock); set => PlayerStatus.SetLegacy(StatusFlag.InputLock, value); }
    public static bool isUsePortal = false;
    public static Dictionary<string, int> inventoryItemStack = new Dictionary<string, int>();
    public static Dictionary<string, int> setsEffectStack = new Dictionary<string, int>();
    public static Dictionary<string, int> skillCoolDown = new Dictionary<string, int>();
    public static Dictionary<string, int> skillHitCount = new Dictionary<string, int>();
    public static Dictionary<string, int> skillBulletCount = new Dictionary<string, int>();
    public static Dictionary<string, int[]> bleedingEquipment = new Dictionary<string, int[]>();
    public static Dictionary<string, float> bleedingEquipmentInterval = new Dictionary<string, float>();
    ItemDatabase itemDatabase;
    SetItem setDatabase;
    public static int money;


    /// <summary>세트 부위 장착 수 증가 → 정확히 단계 수에 도달하면 해당 세트 효과 발동</summary>
    public static void PlusSetDict(string name, int count)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        setsEffectStack.TryGetValue(name, out int now);
        setsEffectStack[name] = now + count;
        SetEffectActvie(name, setsEffectStack[name]);
    }

    /// <summary>세트 부위 장착 수 감소 → 줄기 전 단계의 효과 해제</summary>
    public static void MinusSetDict(string name, int count)
    {
        if (string.IsNullOrEmpty(name) || !setsEffectStack.TryGetValue(name, out int now))
        {
            return; // 세트가 아니거나 카운트되지 않은 장비 (B-112)
        }
        SetEffectDisactvie(name, now);
        now -= count;
        if (now <= 0)
        {
            setsEffectStack.Remove(name);
        }
        else
        {
            setsEffectStack[name] = now;
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
        if (inventoryItemStack.ContainsKey(name))
        {
            inventoryItemStack[name] -= count;
            // 만약 값이 0이 되면 해당 키를 제거할 수 있음
            if (inventoryItemStack[name] == 0)
            {
                inventoryItemStack.Remove(name);
            }
        }


    }






    static public DatabaseManager instance;
    private void Awake()
    {
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

    public void LoadAllItemData() // 기타 아이템
    {
        ItemDataList = itemDatabase.items;
        setsDataList = setDatabase.items;
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
            Debug.Log("데이터를 불러오지 못했거나 유효하지 않은 인덱스입니다.");
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
      //      Debug.Log("데이터를 불러오지 못했거나 유효하지 않은 인덱스입니다.");
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

    public int FindSetsDataIndex(string targetValue)
    {
        if (setsDataList != null)
        {
            for (int i = 0; i < setsDataList.Count; i++)
            {
                if (setsDataList[i].setName == targetValue)
                {
                    // 입력한 값과 일치하는 데이터를 찾으면 해당 인덱스 반환
                    return i;
                }
            }

//            Debug.Log("찾지 못했습니다");
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


