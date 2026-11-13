/// <summary>
/// 마우스 우클릭·호버 규칙(순수, REWORK-2 S2 U25·U26). 어떤 동작을 할지, 툴팁을 어디에 놓을지만 계산한다.
/// 화면 조작(InventoryQuickUse·HoverTip)은 이 규칙만 호출하므로 EditMode 에서 그대로 테스트한다.
/// </summary>
public static class QuickUseRules
{
    public enum Action
    {
        None,
        Move,  // 창고 ↔ 가방 빠른 이동
        Use,   // 소모품 사용
        Equip, // 장비 착용
        Sell   // 상점 판매(수량 창)
    }

    /// <summary>우클릭 한 번이 할 일. 창고 열림=이동, 상점 열림=판매, 그 외 소모품=사용·장비=착용. 가방 밖(장비칸 등)은 없음</summary>
    public static Action Decide(bool chestOpen, bool shopOpen, bool inInventory, string itemType)
    {
        if (chestOpen)
        {
            return Action.Move;
        }
        if (!inInventory)
        {
            return Action.None;
        }
        if (shopOpen)
        {
            return Action.Sell;
        }
        switch (itemType)
        {
            case "Consum": return Action.Use;
            case "Equip": return Action.Equip;
            default: return Action.None;
        }
    }

    /// <summary>
    /// 장비칸에 낀 아이템을 마우스로 해제할지(REWORK-0 2차, 메이플: 착용품 더블클릭 해제). 우클릭 또는 왼쪽 더블클릭.
    /// 창고·상점이 열려 있거나 모달 상태면 하지 않는다. 무기 해제 금지는 InventoryManager.CanUnequip 이 따로 막는다.
    /// </summary>
    public static bool ShouldUnequipWorn(bool worn, bool rightClick, int clickCount, bool chestOpen, bool shopOpen, string state)
    {
        if (!worn || chestOpen || shopOpen)
        {
            return false;
        }
        if (!rightClick && clickCount < 2)
        {
            return false;
        }
        return StateAllows(state) || state == "Equipment";
    }

    /// <summary>우클릭을 받아도 되는 인벤 상태(상세·커서·상점 커서). 수량 창·나누기·교체 등 모달 상태는 막는다</summary>
    public static bool StateAllows(string state)
    {
        return state == "" || state == "detail" || state == "ShopCusorOn" || state == "chestOpen";
    }

    /// <summary>왼쪽 클릭(커서 이동)이 흐름을 깨는 모달 상태(판매·구매·나누기·버리기 수량 창)</summary>
    public static bool IsModal(string state)
    {
        return state == "Sell" || state == "Buy" || state == "BuyDetail" || state == "divide" || state == "drop";
    }

    /// <summary>
    /// 툴팁 왼쪽 아래 좌표(캔버스 좌표, 원점 왼쪽 아래). 기본은 마우스 오른쪽 아래,
    /// 화면 밖으로 나가면 반대쪽으로 뒤집고 그래도 넘치면 화면 안으로 민다.
    /// </summary>
    public static void Place(float mouseX, float mouseY, float width, float height, float canvasW, float canvasH, float gap,
        out float x, out float y)
    {
        x = mouseX + gap;
        if (x + width > canvasW)
        {
            x = mouseX - gap - width;
        }
        y = mouseY - gap - height;
        if (y < 0f)
        {
            y = mouseY + gap;
        }
        if (x + width > canvasW)
        {
            x = canvasW - width;
        }
        if (x < 0f)
        {
            x = 0f;
        }
        if (y + height > canvasH)
        {
            y = canvasH - height;
        }
        if (y < 0f)
        {
            y = 0f;
        }
    }
}
