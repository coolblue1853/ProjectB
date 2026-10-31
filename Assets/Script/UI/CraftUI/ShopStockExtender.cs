using System.Collections;
using UnityEngine;

/// <summary>기존 슬롯으로 보급품·기본 장비 두 페이지를 구성한다.</summary>
public sealed class ShopStockExtender : MonoBehaviour
{
    public static readonly string[] ExtraItems = { "Small HP Potion", "Medium HP Potion", "Large HP Potion", "Recall Scroll" };
    public static readonly string[] Supplies = { "Emergency Rations", "Torch" };
    public static readonly string[] Weapons = { "RustedSword", "Leather Knuckles", "Wood Spear" };
    public static readonly string[] Armor = { "Cap", "Armor", "HandArmor", "LegArmor", "Shoes" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        new GameObject("[ShopStockExtender]").AddComponent<ShopStockExtender>();
    }

    private IEnumerator Start()
    {
        while (DatabaseManager.instance == null || DatabaseManager.instance.FindItemDataIndex("Small HP Potion") < 0)
        {
            yield return null;
        }
        ShopManager shop = FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
        if (shop != null && shop.LV1craftBox != null && shop.LV1craftBox.Length > 0 && shop.LV1craftBox[0] != null)
        {
            Build(shop);
        }
        Destroy(gameObject);
    }

    private static void Build(ShopManager shop)
    {
        Transform box = shop.LV1craftBox[0].transform;
        if (box.childCount == 0 || box.Find("TownEquipment") != null)
        {
            return;
        }
        Transform page = box.GetChild(0);
        if (page.childCount == 0 || page.GetChild(0).childCount == 0)
        {
            return;
        }
        Transform source = page.GetChild(0);
        GameObject template = source.GetChild(0).gameObject;
        Transform equipment = Instantiate(page.gameObject, box).transform;
        equipment.name = "TownEquipment";
        // 원본을 살려 둔 상태로 두 페이지를 만들고 마지막에 원본 줄을 제거한다.
        ReplaceRows(equipment, template, source, Weapons, Armor);
        ReplaceRows(page, template, source, ExtraItems, Supplies);
        equipment.gameObject.SetActive(false);
        shop.BuildStockPageLabel();
        shop.ResetStockSelection();
    }

    private static void ReplaceRows(Transform page, GameObject template, Transform source, string[] first, string[] second)
    {
        Transform row1 = MakeRow(page, template, source, first, 0);
        Transform row2 = MakeRow(page, template, source, second, 1);
        for (int i = page.childCount - 1; i >= 0; i--)
        {
            Transform old = page.GetChild(i);
            if (old != row1 && old != row2)
            {
                old.gameObject.SetActive(false);
                old.SetParent(null);
                Destroy(old.gameObject);
            }
        }
    }

    private static Transform MakeRow(Transform page, GameObject template, Transform source, string[] items, int index)
    {
        RectTransform row = Instantiate(source.gameObject, page).GetComponent<RectTransform>();
        row.name = "TownLine" + index;
        for (int i = row.childCount - 1; i >= 0; i--)
        {
            Transform old = row.GetChild(i);
            old.gameObject.SetActive(false);
            old.SetParent(null);
            Destroy(old.gameObject);
        }
        row.anchoredPosition = ((RectTransform)source).anchoredPosition + Vector2.down * (110f * index);
        foreach (string id in items)
        {
            GameObject slot = Instantiate(template, row);
            slot.name = id;
            ItemCheck check = slot.GetComponent<ItemCheck>();
            check.SetItem(id);
            check.isShopItem = true;
            if (check.Cost != null)
            {
                check.Cost.text = check.buyprice.ToString();
            }
        }
        return row;
    }
}
