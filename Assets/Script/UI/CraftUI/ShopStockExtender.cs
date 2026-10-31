using UnityEngine;

/// <summary>
/// 상점(ShopManager) 판매 목록 확장: 씬은 건드리지 않고 시작 시 기존 줄(Line1)을 복제해 물약 3종·귀환 주문서 줄을 덧붙인다.
/// 가격은 ItemSheet price x1.5(ItemCheck.buyprice). ClassicMapleDesign.md 구현 3단계.
/// </summary>
public sealed class ShopStockExtender : MonoBehaviour
{
    public static readonly string[] ExtraItems = { "Small HP Potion", "Medium HP Potion", "Large HP Potion", "Recall Scroll" };
    private const float LineGap = 110f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        new GameObject("[ShopStockExtender]").AddComponent<ShopStockExtender>();
    }

    private void Start()
    {
        Extend(FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include));
        Destroy(gameObject);
    }

    private static void Extend(ShopManager shop)
    {
        if (shop == null || shop.LV1craftBox == null || shop.LV1craftBox.Length == 0 || shop.LV1craftBox[0].transform.childCount == 0)
        {
            return;
        }
        Transform page = shop.LV1craftBox[0].transform.GetChild(0);
        if (page.childCount == 0 || page.Find("LineExtra") != null)
        {
            return;
        }
        Transform src = page.GetChild(0);
        if (src.childCount == 0)
        {
            return;
        }
        GameObject template = src.GetChild(0).gameObject; // 소비 아이템 슬롯(Emergency Rations) 복제 원본
        RectTransform line = Instantiate(src.gameObject, page).GetComponent<RectTransform>();
        line.name = "LineExtra";
        for (int i = line.childCount - 1; i >= 0; i--)
        {
            Destroy(line.GetChild(i).gameObject);
        }
        line.anchoredPosition = ((RectTransform)src).anchoredPosition + Vector2.down * LineGap;
        foreach (string id in ExtraItems)
        {
            GameObject slot = Instantiate(template, line);
            slot.name = id;
            slot.GetComponent<ItemCheck>().SetItem(id);
        }
    }
}
