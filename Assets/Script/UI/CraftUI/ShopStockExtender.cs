using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 진열을 ShopStock.csv(Page,Row,ItemId,MinFloor,PageName)로 만든다. 씬의 기존 진열 슬롯을 템플릿으로 복제해 페이지·줄·칸을 채운다.
/// 도달한 층계(MovementUnlocks.HighestFloor)가 MinFloor 이상인 품목만 진열하고, 상점을 열 때 층계가 달라졌으면 다시 만든다.
/// </summary>
public sealed class ShopStockExtender : MonoBehaviour
{
    private const float RowGap = 110f;

    private static ShopStockExtender _active;
    private static readonly List<string> s_pageNames = new List<string>();
    private static readonly HashSet<string> s_ids = new HashSet<string>();

    // 1층계 기준 진열 줄(ShopStock.csv). 예전 하드코딩 배열과 같은 이름 — 품목 검증 테스트가 쓴다
    public static string[] ExtraItems => Row(0, 0);
    public static string[] Supplies => Row(0, 1);
    public static string[] Weapons => Row(1, 0);
    public static string[] Armor => Row(1, 1);

    private static string[] Row(int page, int row)
    {
        List<List<List<string>>> layout = ShopStockTable.Parse(LoadCsv()).Layout(1, null, out _);
        return page < layout.Count && row < layout[page].Count ? layout[page][row].ToArray() : new string[0];
    }

    private ShopManager _shop;
    private Transform _box;
    private GameObject _pageTemplate, _rowTemplate, _slotTemplate;
    private Vector2 _rowOrigin;
    private int _builtFloor = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _active = null;
        s_pageNames.Clear();
        s_ids.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        new GameObject("[ShopStockExtender]").AddComponent<ShopStockExtender>();
    }

    private void OnDestroy()
    {
        if (_active == this)
        {
            _active = null;
        }
    }

    private IEnumerator Start()
    {
        while (DatabaseManager.instance == null || DatabaseManager.instance.FindItemDataIndex("Small HP Potion") < 0)
        {
            yield return null;
        }
        ShopManager shop = FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
        if (shop != null && shop.LV1craftBox != null && shop.LV1craftBox.Length > 0 && shop.LV1craftBox[0] != null
            && CaptureTemplates(shop))
        {
            _active = this;
            Rebuild();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>표시용 페이지 이름(CSV PageName). 없으면 "상점"</summary>
    public static bool Sells(string id)
    {
        return s_ids.Contains(id);
    }

    public static string PageName(int page)
    {
        return page >= 0 && page < s_pageNames.Count && !string.IsNullOrEmpty(s_pageNames[page]) ? s_pageNames[page] : "상점";
    }

    /// <summary>상점을 열 때: 층계가 바뀌었으면 진열을 다시 만든다</summary>
    public static void RefreshFor(ShopManager shop)
    {
        if (_active != null && _active._shop == shop && CurrentFloor() != _active._builtFloor)
        {
            _active.Rebuild();
        }
    }

    private static int CurrentFloor()
    {
        return MovementUnlocks.Instance != null ? MovementUnlocks.Instance.HighestFloor : 1;
    }

    private bool CaptureTemplates(ShopManager shop)
    {
        Transform box = shop.LV1craftBox[0].transform;
        if (box.childCount == 0 || box.GetChild(0).childCount == 0 || box.GetChild(0).GetChild(0).childCount == 0)
        {
            return false;
        }
        Transform page = box.GetChild(0);
        Transform row = page.GetChild(0);
        Transform slot = row.GetChild(0);
        _shop = shop;
        _box = box;
        _rowOrigin = ((RectTransform)row).anchoredPosition;
        _slotTemplate = MakeTemplate(slot.gameObject);
        _rowTemplate = MakeTemplate(row.gameObject);
        ClearChildren(_rowTemplate.transform);
        _pageTemplate = MakeTemplate(page.gameObject);
        ClearChildren(_pageTemplate.transform);
        return true;
    }

    private GameObject MakeTemplate(GameObject source)
    {
        GameObject copy = Instantiate(source, transform);
        copy.name = source.name;
        copy.SetActive(false);
        return copy;
    }

    private static void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Transform old = t.GetChild(i);
            old.gameObject.SetActive(false);
            old.SetParent(null);
            Destroy(old.gameObject);
        }
    }

    private void Rebuild()
    {
        int floor = CurrentFloor();
        ShopStockTable table = ShopStockTable.Parse(LoadCsv());
        List<List<List<string>>> layout = table.Layout(floor, id => DatabaseManager.instance.FindItemDataIndex(id) >= 0, out List<string> names);
        if (layout.Count == 0)
        {
            GameLog.Warn(GameLog.Cat.Item, "ShopStock.csv 에 진열 품목이 없어 기존 진열을 유지");
            _builtFloor = floor;
            return;
        }
        ClearChildren(_box);
        s_pageNames.Clear();
        s_ids.Clear();
        s_pageNames.AddRange(names);
        for (int p = 0; p < layout.Count; p++)
        {
            Transform page = Instantiate(_pageTemplate, _box).transform;
            page.name = p == 0 ? "TownStock" : "TownStock" + p;
            page.gameObject.SetActive(p == 0);
            for (int r = 0; r < layout[p].Count; r++)
            {
                MakeRow(page, layout[p][r], r);
            }
        }
        _builtFloor = floor;
        _shop.BuildStockPageLabel();
        _shop.ResetStockSelection();
    }

    private static string LoadCsv()
    {
        TextAsset csv = Resources.Load<TextAsset>("ShopStock");
        return csv != null ? csv.text : "";
    }

    private void MakeRow(Transform page, List<string> items, int index)
    {
        RectTransform row = Instantiate(_rowTemplate, page).GetComponent<RectTransform>();
        row.name = "TownLine" + index;
        row.gameObject.SetActive(true);
        row.anchoredPosition = _rowOrigin + Vector2.down * (RowGap * index);
        foreach (string id in items)
        {
            s_ids.Add(id);
            GameObject slot = Instantiate(_slotTemplate, row);
            slot.name = id;
            slot.SetActive(true);
            ItemCheck check = slot.GetComponent<ItemCheck>();
            check.isShopItem = true;
            check.SetItem(id);
            if (check.Cost != null)
            {
                check.Cost.text = check.buyprice.ToString();
            }
        }
    }
}
