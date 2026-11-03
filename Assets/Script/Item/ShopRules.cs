using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>판매 목록 종류 필터(메이플 상점 탭: 장비/소비/기타)</summary>
public enum SellFilter { All, Equip, Consum, Misc }

/// <summary>
/// 상점 규칙(순수 로직, BasicFeatures.md 14절): 구매 가능 수량(골드·가방 여유), 물약 충전 수량, 판매 필터, 판매가 미리보기.
/// UI·인벤토리 의존이 없어 EditMode 테스트로 검증한다.
/// </summary>
public static class ShopRules
{
    public enum BuyBlock { None, NoGold, BagFull }

    /// <summary>한 번에 살 수 있는 최대 개수 = min(스택 한도, 골드로 살 수 있는 수, 가방 여유). 가격 0 이하면 골드 제한 없음</summary>
    public static int MaxBuyable(int maxStack, int price, int money, int space)
    {
        int cap = Math.Max(0, Math.Min(maxStack, space));
        if (price <= 0)
        {
            return cap;
        }
        return Math.Max(0, Math.Min(cap, money / price));
    }

    /// <summary>상점 판매가 = 시트 가격 × 1.5 (소수점 버림). 되팔 때는 시트 가격 그대로</summary>
    public static int BuyPrice(int sheetPrice)
    {
        return (int)(sheetPrice * 1.5f);
    }

    /// <summary>1개도 못 살 때 이유(골드가 먼저, 그다음 가방). 살 수 있으면 None</summary>
    public static BuyBlock CheckBuy(int price, int money, int space)
    {
        if (price > 0 && money < price)
        {
            return BuyBlock.NoGold;
        }
        return space <= 0 ? BuyBlock.BagFull : BuyBlock.None;
    }

    /// <summary>물약 충전: target 까지 모자란 만큼, 골드·가방 여유 안에서. 이미 target 이상이면 0</summary>
    public static int RefillAmount(int have, int target, int price, int money, int space)
    {
        int need = target - have;
        if (need <= 0)
        {
            return 0;
        }
        return MaxBuyable(need, price, money, space);
    }

    /// <summary>충전 목표 수량: 퀵슬롯 물약은 스택 한도(최대 수량)까지</summary>
    public static int RefillTarget(int maxStack)
    {
        return Math.Max(1, maxStack);
    }

    public static bool FilterMatches(SellFilter filter, string type)
    {
        switch (filter)
        {
            case SellFilter.Equip: return type == "Equip";
            case SellFilter.Consum: return type == "Consum";
            case SellFilter.Misc: return type == "Misc";
            default: return true;
        }
    }

    public static SellFilter NextFilter(SellFilter f)
    {
        return (SellFilter)(((int)f + 1) % 4);
    }

    public static string FilterLabel(SellFilter f)
    {
        switch (f)
        {
            case SellFilter.Equip: return "장비";
            case SellFilter.Consum: return "소비";
            case SellFilter.Misc: return "기타";
            default: return "전체";
        }
    }

    /// <summary>판매 총액(오버플로 방지로 long)</summary>
    public static long SaleTotal(int price, int count)
    {
        return (long)Math.Max(0, price) * Math.Max(0, count);
    }
}

/// <summary>
/// 상점 진열 데이터(ShopStock.csv): Page,Row,ItemId,MinFloor,PageName,MapOnly(선택). 도달한 층계가 MinFloor 이상이어야 진열된다.
/// 같은 Page 가 한 페이지, 같은 (Page,Row) 가 한 줄. 순수 파서(층계는 호출자가 넘긴다).
/// </summary>
public sealed class ShopStockTable
{
    public struct Entry
    {
        public int Page, Row, MinFloor;
        public string Id, PageName, MapOnly;
    }

    private readonly List<Entry> _entries = new List<Entry>();

    public IReadOnlyList<Entry> Entries => _entries;

    public static ShopStockTable Parse(string text)
    {
        var table = new ShopStockTable();
        if (string.IsNullOrEmpty(text))
        {
            return table;
        }
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 4 || !int.TryParse(c[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int page) || page < 0
                || !int.TryParse(c[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int row) || row < 0
                || string.IsNullOrWhiteSpace(c[2])
                || !int.TryParse(c[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int minFloor))
            {
                continue;
            }
            table._entries.Add(new Entry
            {
                Page = page, Row = row, Id = c[2].Trim(), MinFloor = Math.Max(1, minFloor),
                PageName = c.Length > 4 ? c[4].Trim() : "",
                MapOnly = c.Length > 5 ? c[5].Trim() : "",
            });
        }
        return table;
    }

    /// <summary>맵 한정 품목 규칙: mapOnly 가 비면 전역(항상 진열), 아니면 현재 맵 이름과 같을 때만</summary>
    public static bool MapAllows(string mapOnly, string map)
    {
        return string.IsNullOrEmpty(mapOnly) || mapOnly == map;
    }

    /// <summary>floor 에서 진열되는 항목을 페이지 → 줄 → 아이템 순으로. exists 가 false 인 이름(시트에 없음)은 뺀다. 비는 페이지·줄은 건너뛰어 번호를 앞으로 당긴다</summary>
    public List<List<List<string>>> Layout(int floor, Func<string, bool> exists, out List<string> pageNames, string map = null)
    {
        pageNames = new List<string>();
        var pages = new SortedDictionary<int, SortedDictionary<int, List<string>>>();
        var names = new Dictionary<int, string>();
        foreach (Entry e in _entries)
        {
            if (e.MinFloor > floor || !MapAllows(e.MapOnly, map) || (exists != null && !exists(e.Id)))
            {
                continue;
            }
            if (!pages.TryGetValue(e.Page, out var rows))
            {
                rows = new SortedDictionary<int, List<string>>();
                pages[e.Page] = rows;
            }
            if (!rows.TryGetValue(e.Row, out var ids))
            {
                ids = new List<string>();
                rows[e.Row] = ids;
            }
            ids.Add(e.Id);
            if (!string.IsNullOrEmpty(e.PageName) && !names.ContainsKey(e.Page))
            {
                names[e.Page] = e.PageName;
            }
        }
        var result = new List<List<List<string>>>();
        foreach (var page in pages)
        {
            var rowList = new List<List<string>>();
            foreach (var row in page.Value)
            {
                rowList.Add(row.Value);
            }
            result.Add(rowList);
            pageNames.Add(names.TryGetValue(page.Key, out string n) ? n : "");
        }
        return result;
    }
}
