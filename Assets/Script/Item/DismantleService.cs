using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 분해 실행(BasicFeatures.md 8절): 인벤 칸의 장비 1개 → DismantleRules 재료. 모든 변경은 InventoryService 명령으로.
/// 재료를 먼저 넣어 보고(자리가 없으면 넣은 것을 되돌리고 실패), 그다음 장비를 뺀다 — 실패해도 장비·재료 총량이 보존된다.
/// 한계: 가방이 꽉 찼으면 장비를 빼서 생길 칸을 미리 쓰지 못해 실패한다(Q-434). 롤백은 OnItemAdded 를 되돌리지 않는다(지금 구독자 없음).
/// </summary>
public static class DismantleService
{
    public enum Check { Ok, NotEquipment, NotInInventory, NoRoom }

    public static Check CanTry(InventoryManager inv, ItemCheck item)
    {
        if (item == null || item.itemData == null || item.itemData.type != "Equip")
        {
            return Check.NotEquipment;
        }
        if (inv == null || inv.Service.IndexOf(item) < 0)
        {
            return Check.NotInInventory; // 착용칸·창고는 대상 아님
        }
        return Check.Ok;
    }

    public static Check TryDismantle(InventoryManager inv, ItemCheck item, out List<KeyValuePair<string, int>> outputs)
    {
        outputs = null;
        Check check = CanTry(inv, item);
        if (check != Check.Ok)
        {
            return check;
        }
        ItemData d = item.itemData;
        outputs = DismantleRules.Outputs(d.rarity, d.tear, d.upgrade);
        InventoryService svc = inv.Service;
        var added = new List<KeyValuePair<string, int>>();
        foreach (KeyValuePair<string, int> o in outputs)
        {
            if (!svc.Add(o.Key, o.Value, AddSource.Dismantle))
            {
                Rollback(svc, added);
                return Check.NoRoom;
            }
            added.Add(o);
        }
        string name = item.name;
        if (!svc.RemoveAt(item, 1))
        {
            Rollback(svc, added);
            inv.SaveInventory();
            return Check.NotInInventory;
        }
        inv.SaveInventory();
        GameLog.Info(GameLog.Cat.Item, "분해: " + name + " +" + d.upgrade + " (" + d.rarity + ")");
        return Check.Ok;
    }

    private static void Rollback(InventoryService svc, List<KeyValuePair<string, int>> added)
    {
        foreach (KeyValuePair<string, int> a in added)
        {
            if (!svc.Remove(a.Key, a.Value))
            {
                Debug.LogError("[PB][Item] 분해 롤백 실패(재료 복제 가능): " + a.Key + " x" + a.Value);
            }
        }
    }
}
