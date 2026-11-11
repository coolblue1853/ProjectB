using UnityEngine;

/// <summary>
/// 인벤 칸 우클릭(REWORK-2 U25). 규칙은 QuickUseRules: 창고 열림=이동(Chest.QuickMove), 상점 열림=판매 수량 창,
/// 그 외 소모품=사용·장비=착용. 키보드·패드는 커서+Z/A 상세창 → 한 번 더 누르기가 같은 동작이라 이 경로는 마우스 전용이다.
/// 기존 키 경로 함수(TryConsume·EquipFromInventory·SellItemUI)를 그대로 부르므로 규칙·저장이 같다.
/// </summary>
public static class InventoryQuickUse
{
    /// <summary>우클릭을 처리했으면 true</summary>
    public static bool Run(InventoryManager inv, ItemCheck item)
    {
        if (inv == null || item == null || item.itemData == null || QuantityPopup.IsBlocking)
        {
            return false;
        }
        bool shop = inv.ShopGameObject != null && inv.ShopGameObject.activeSelf;
        bool inInventory = inv.Service.IndexOf(item) >= 0;
        QuickUseRules.Action action = QuickUseRules.Decide(inv.chest != null, shop, inInventory, item.itemData.type);
        if (action != QuickUseRules.Action.Move && !QuickUseRules.StateAllows(inv.state))
        {
            return false; // 창고 이동은 예전부터 상태와 무관, 나머지는 모달·교체 상태에서 막는다
        }
        switch (action)
        {
            case QuickUseRules.Action.Move:
                return inv.chest.QuickMove(item);
            case QuickUseRules.Action.Sell:
                inv.ChangeCusor(item.transform.parent.gameObject);
                inv.BoxContentChecker(item.gameObject); // 상세창을 띄워 두어 취소하면 키보드와 같은 상태로 돌아온다
                inv.detail = item;
                inv.SellItemUI();
                return true;
            case QuickUseRules.Action.Use:
                inv.ChangeCusor(item.transform.parent.gameObject);
                inv.TryConsume(item);
                return true;
            case QuickUseRules.Action.Equip:
                inv.ChangeCusor(item.transform.parent.gameObject);
                inv.EquipFromInventory(item);
                inv.DetailOff();
                return true;
            default:
                return false;
        }
    }
}
