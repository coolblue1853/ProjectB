using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤 버리기(REWORK-2 U24). 인벤 창 밖으로 끌어 놓기(마우스) / Delete 키 / 상세창에서 RT(패드) 가 같은 확인 창(QuantityPopup)을 연다.
/// 확인하면 칸에서 수량을 빼고 저장한다(되돌릴 수 없다). 잠금·진행 필수·대여 아이템은 막는다.
/// </summary>
public static class InventoryDrop
{
    private static readonly List<RaycastResult> Hits = new List<RaycastResult>();

    /// <summary>이 아이템을 버릴 수 있는지(순수 판정). 이유는 reason 으로</summary>
    public static bool CanDiscard(string id, bool locked, out string reason)
    {
        reason = null;
        if (string.IsNullOrEmpty(id))
        {
            reason = "버릴 아이템이 없다";
            return false;
        }
        if (locked)
        {
            reason = "잠긴 아이템은 버릴 수 없다 (L 해제)";
            return false;
        }
        if (!ItemTradeRules.CanSell(id))
        {
            reason = "이 아이템은 버릴 수 없다";
            return false;
        }
        return true;
    }

    /// <summary>수량 창을 연다. 열었으면 true</summary>
    public static bool Request(InventoryManager inv, ItemCheck item)
    {
        if (inv == null || item == null || item.itemData == null || QuantityPopup.IsBlocking)
        {
            return false;
        }
        bool shop = inv.ShopGameObject != null && inv.ShopGameObject.activeSelf;
        if (shop || inv.chest != null || inv.Service.IndexOf(item) < 0)
        {
            return false; // 상점에선 판매, 창고가 열려 있으면 창고로 옮긴다
        }
        string reason;
        if (!CanDiscard(item.name, item.itemData.isLocked, out reason))
        {
            Sfx.Play(SfxId.CraftFail, 0.4f);
            PickupLog.Warn(reason);
            return false;
        }
        int stack = Mathf.Max(1, item.nowStack);
        string label = item.itemData.itemNameT;
        inv.state = "drop";
        QuantityPopup.Open(new QuantityRequest
        {
            Title = "버리기",
            ItemName = label,
            Min = 1,
            Max = stack,
            Initial = 1,
            ConfirmLabel = "버리기",
            Detail = n => n + "개를 버립니다. 되돌릴 수 없습니다",
            OnConfirm = n => Discard(inv, item, label, n),
            OnCancel = () => Finish(inv)
        });
        return true;
    }

    private static void Discard(InventoryManager inv, ItemCheck item, string label, int count)
    {
        string id = item != null ? item.name : null;
        if (item == null || item.itemData == null || item.itemData.isLocked || inv.Service.IndexOf(item) < 0)
        {
            Sfx.Play(SfxId.CraftFail, 0.4f);
            Finish(inv);
            return;
        }
        int have = item.nowStack;
        if (inv.Service.RemoveAt(item, Mathf.Min(count, have)))
        {
            inv.SaveInventory();
            PickupLog.Notice(label + " " + Mathf.Min(count, have) + "개를 버렸다");
            GameLog.Info(GameLog.Cat.Item, "버리기: " + id + " x" + Mathf.Min(count, have));
        }
        Finish(inv);
    }

    private static void Finish(InventoryManager inv)
    {
        if (inv == null)
        {
            return;
        }
        inv.state = "";
        inv.DetailOff();
    }

    /// <summary>화면 좌표가 인벤·창고·상점 창 위인지. 아무 UI 도 없는 곳(월드)이면 false</summary>
    public static bool IsOverInventoryWindow(InventoryManager inv, Vector2 screenPos)
    {
        if (inv == null || EventSystem.current == null)
        {
            return true; // 모르면 창 안으로 본다(실수로 버리기 창이 뜨지 않게)
        }
        var data = new PointerEventData(EventSystem.current) { position = screenPos };
        Hits.Clear();
        EventSystem.current.RaycastAll(data, Hits);
        foreach (RaycastResult hit in Hits)
        {
            Transform t = hit.gameObject.transform;
            if (inv.inventory != null && t.IsChildOf(inv.inventory.transform))
            {
                return true;
            }
            if (inv.ShopGameObject != null && t.IsChildOf(inv.ShopGameObject.transform))
            {
                return true;
            }
            if (inv.chest != null && inv.chest.chestOb != null && t.IsChildOf(inv.chest.chestOb.transform))
            {
                return true;
            }
        }
        return Hits.Count > 0 && AnyNonDragHit();
    }

    /// <summary>다른 UI(퀵슬롯·HUD 등) 위에 놓은 경우는 버리기로 보지 않는다. 끌고 있는 아이템 자신(dragCanvas)만 맞았으면 월드로 본다</summary>
    private static bool AnyNonDragHit()
    {
        foreach (RaycastResult hit in Hits)
        {
            if (!hit.gameObject.CompareTag("dragCanvas") && hit.gameObject.GetComponentInParent<ItemDrag>() == null)
            {
                return true;
            }
        }
        return false;
    }
}
