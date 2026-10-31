/// <summary>세이브 아이템 키를 유지한 채 진행 필수 아이템의 판매를 막는다.</summary>
public static class ItemTradeRules
{
    public static bool CanSell(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && itemId != "BroochResonance";
    }
}
