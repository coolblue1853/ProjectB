using UnityEngine;

/// <summary>봇이 누를 메뉴 키(진짜 키 입력으로 바뀐다). Inventory = I, Refill = H(상점 물약 충전), Junk = J(잡템 일괄 판매). 숫자 키는 퀵슬롯(아이템 사용)과 겹쳐 쓰지 않는다 — 가방 페이지는 ← 로 칸 왼쪽 끝을 지나 페이지 고르기(boxChange)로 바꾼다</summary>
public enum BotKey { None, Up, Down, Left, Right, Select, Back, Inventory, Refill, Junk }

/// <summary>메뉴 상태 읽기값(화면에 보이는 것 = 열린 창·커서). 게임 쪽 읽기는 BotErrand 가 채운다</summary>
public struct BotMenuView
{
    public bool InvOpen;
    public bool ShopOpen;
    public bool NpcMenuOpen;
    /// <summary>수량 창·확인 창이 열려 있다(모달)</summary>
    public bool Popup;
    /// <summary>InventoryManager.state: "" 가방 칸 · "detail" 상세 · "ShopCusorOn" 상점 진열 · "boxChange" 가방 페이지 고르기 …</summary>
    public string State;
    public int Page;
    public int Cell;
    public int BoxCursor;
    public int NpcCursor;
    /// <summary>NPC 선택지에서 상점 줄(없으면 -1)</summary>
    public int NpcRow;
}

/// <summary>
/// 메뉴 조작 판단(순수): 지금 보이는 상태에서 목표 가방 칸까지 가려면 무슨 키를 한 번 누를지.
/// 가방 칸 5x5(페이지 25칸). 상점 진열에서는 →로 가방 쪽으로 넘어가 페이지 고르기(boxChange)를 거친다.
/// </summary>
public static class BotMenuNav
{
    public const int Hor = 5;
    public const int PerPage = 25;

    /// <summary>가방 flat 칸 번호 → 페이지</summary>
    public static int PageOf(int index)
    {
        return index / PerPage;
    }

    public static int CellOf(int index)
    {
        return index % PerPage;
    }

    /// <summary>
    /// 목표 칸(page, cell)의 아이템을 "고르고 확정"하기까지 누를 키 한 번. 상세창·수량창은 확정(Select)을 계속 눌러 끝까지 간다.
    /// 목표에 닿은 칸에서 Select → 상세 → Select(장착 또는 판매 수량창) → 수량창 Select(판매 확인). 관측으로 결과를 확인하는 것은 호출자 몫.
    /// </summary>
    public static BotKey ToItem(BotMenuView v, int page, int cell)
    {
        if (v.Popup)
        {
            return BotKey.Select;
        }
        switch (v.State)
        {
            case "detail":
                return BotKey.Select;
            case "boxChange":
                if (v.BoxCursor < page)
                {
                    return BotKey.Down;
                }
                return v.BoxCursor > page ? BotKey.Up : BotKey.Select;
            case "ShopCusorOn":
                return v.ShopOpen ? BotKey.Right : BotKey.None; // 진열 → 가방(페이지 고르기로 들어간다)
            case "Equipment":
                return BotKey.Right; // 장비 칸에서 가방으로 돌아온다
            case "Sell":
            case "Buy":
            case "BuyDetail":
                return BotKey.None; // 수량 창이 닫히는 사이의 전환 상태: 기다린다
            case "":
            case null:
                break;
            default:
                return BotKey.Back; // 모르는 상태(교체·나누기 등)는 한 단계 빠져나온다
        }
        if (!v.InvOpen)
        {
            return BotKey.None;
        }
        if (v.Page != page)
        {
            return BotKey.Left; // 칸 왼쪽 끝에서 ← 를 한 번 더 누르면 페이지 고르기(boxChange)로 들어간다
        }
        if (v.Cell == cell)
        {
            return BotKey.Select;
        }
        int col = v.Cell % Hor, tcol = cell % Hor;
        if (col != tcol)
        {
            return col < tcol ? BotKey.Right : BotKey.Left;
        }
        return v.Cell / Hor < cell / Hor ? BotKey.Down : BotKey.Up;
    }

    /// <summary>NPC 선택지에서 상점 줄로: 커서가 위/아래면 한 칸, 같으면 Select. 상점 줄이 없으면 Back</summary>
    public static BotKey ToNpcShop(BotMenuView v)
    {
        if (!v.NpcMenuOpen)
        {
            return BotKey.None;
        }
        if (v.NpcRow < 0)
        {
            return BotKey.Back;
        }
        if (v.NpcCursor < v.NpcRow)
        {
            return BotKey.Down;
        }
        return v.NpcCursor > v.NpcRow ? BotKey.Up : BotKey.Select;
    }
}

/// <summary>귀환·장착·판매 판단 규칙(순수)</summary>
public static class BotErrandRules
{
    public const int FallbackBagFree = 3;
    public const int FallbackPotions = 2;
    /// <summary>사람 표본이 너무 크거나 작을 때 자르는 범위(가방 여유 칸·포션 수). 사람 표본 중앙값은 11칸·15개라 그대로 쓰면 1분마다 귀환한다</summary>
    public const int BagFreeMin = 3, BagFreeMax = 8, PotionsMin = 2, PotionsMax = 8;

    public static int ClampBagFree(float sample)
    {
        return Mathf.Clamp(Mathf.RoundToInt(sample), BagFreeMin, BagFreeMax);
    }

    public static int ClampPotions(float sample)
    {
        return Mathf.Clamp(Mathf.RoundToInt(sample), PotionsMin, PotionsMax);
    }

    /// <summary>가방 여유 칸 ≤ 문턱이거나 포션 수 ≤ 문턱이면 마을로 간다</summary>
    public static bool ShouldReturn(int bagFree, int potions, int bagThreshold, int potionThreshold)
    {
        return bagFree <= bagThreshold || potions <= potionThreshold;
    }

    public enum Reason { None, Bag, Potions }

    public static Reason WhyReturn(int bagFree, int potions, int bagThreshold, int potionThreshold)
    {
        if (potions <= potionThreshold)
        {
            return Reason.Potions;
        }
        return bagFree <= bagThreshold ? Reason.Bag : Reason.None;
    }

    /// <summary>같은 부위에서 후보가 착용품보다 점수가 높으면 장착(점수 음수 = 모름 → 장착하지 않음). 빈 부위는 점수가 0 보다 크면 장착</summary>
    public static bool IsUpgrade(float candidate, float worn, bool slotEmpty)
    {
        if (candidate <= 0f)
        {
            return false;
        }
        return slotEmpty || candidate > worn + 0.01f;
    }

    /// <summary>
    /// 팔 장비인가: 폐기 장비는 판다. 장착 가능 부위면 착용품(빈 칸 아님)보다 점수가 높지 않을 때만 판다. 점수를 모르면(음수) 팔지 않는다.
    /// 잠긴 것·장착 불가 부위(귀걸이 등 따로 다루는 칸)는 팔지 않는다.
    /// </summary>
    public static bool ShouldSell(bool locked, bool retired, bool equippableArea, float score, float worn, bool slotEmpty)
    {
        if (locked)
        {
            return false;
        }
        if (retired)
        {
            return true;
        }
        if (!equippableArea || slotEmpty || score < 0f)
        {
            return false;
        }
        return score <= worn + 0.01f;
    }

    /// <summary>봇이 장착을 다루는 부위(InventoryManager.SetEquipBox 가 처리하는 칸)</summary>
    public static bool IsEquippableArea(string area)
    {
        return area == "Weapon" || area == "Chest" || area == "Ring" || area == "Necklace";
    }

    /// <summary>물약 구매 수: 가진 골드의 pct 비율로 살 수 있는 수, 스택 한도 남은 수로 자른다. 1개도 못 사면 0</summary>
    public static int PotionBuyAmount(int gold, int price, int have, int maxStack, float pct)
    {
        if (price <= 0 || gold <= 0)
        {
            return 0;
        }
        int byGold = Mathf.FloorToInt(gold * Mathf.Clamp01(pct) / price);
        int room = Mathf.Max(0, maxStack - have);
        return Mathf.Max(0, Mathf.Min(byGold, room));
    }

    /// <summary>수량 창에서 목표 수량까지: 위 +1, 아래 -1, 오른쪽 +10, 왼쪽 -10. 같으면 확정</summary>
    public static BotKey ToQuantity(int current, int target)
    {
        int d = target - current;
        if (d == 0)
        {
            return BotKey.Select;
        }
        if (d >= 10)
        {
            return BotKey.Right;
        }
        if (d <= -10)
        {
            return BotKey.Left;
        }
        return d > 0 ? BotKey.Up : BotKey.Down;
    }

    /// <summary>상점 진열 격자에서 현재 칸(line,col) → 목표 칸까지 키 한 번(같으면 Select)</summary>
    public static BotKey ToStockCell(int line, int col, int tline, int tcol)
    {
        if (line != tline)
        {
            return line < tline ? BotKey.Down : BotKey.Up;
        }
        if (col != tcol)
        {
            return col < tcol ? BotKey.Right : BotKey.Left;
        }
        return BotKey.Select;
    }
}
