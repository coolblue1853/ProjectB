using UnityEngine;

/// <summary>NPC 퀘스트 보상 지급: EXP(LevelSystem)·골드·아이템 (QuestBoard.GrantReward 와 같은 방식)</summary>
public static class NpcQuestRewards
{
    public const string InventoryFullHint = "보상을 받을 가방 공간이 부족합니다.\nI에서 장비를 착용하거나 마을 왼쪽 상인에게 판매한 뒤 다시 보고하세요.";
    public const string ShopInventoryFullHint = "보상 공간이 모자라요. 상점에서 남는 장비·재료를 팔고 다시 와요.\n요리용 토끼고기 2·약초 1·나무 1은 남겨 둬요.";

    public static string InventoryFullHintFor(string npcId)
    {
        switch (npcId)
        {
            case "elder": return "보상을 담을 자리가 없군. I에서 장비를 착용해 보게.\n마을 왼쪽 상인에게 남는 장비·재료를 팔고 다시 오게.";
            case "ruen": return "가방이 찼어. I에서 장비부터 착용해.\n마을 왼쪽 상인에게 남는 장비·재료를 팔고 돌아와.";
            case "sena": return "보상 넣을 곳이 없네요. I에서 장비를 착용해 보세요.\n마을 왼쪽 상인에게 남는 장비·재료를 팔고 오셔도 돼요.";
            case "eve": return "가방부터 비워야겠네. I에서 장비를 착용해 봐.\n마을 왼쪽 상인에게 남는 장비·재료를 팔고 돌아와.";
            case "merchant": return ShopInventoryFullHint;
            case "smith": return "보상 넣을 자리가 없다. I에서 장비부터 착용해.\n마을 왼쪽 상인에게 남는 장비·재료를 팔고 와.";
            default: return InventoryFullHint;
        }
    }

    public static bool CanReceive(NpcQuestDef d)
    {
        if (d == null)
        {
            return false;
        }
        if (string.IsNullOrEmpty(d.RewardItem) || d.RewardCount <= 0)
        {
            return true;
        }
        InventoryManager inv = InventoryManager.instance;
        return inv != null && CanFitReward(d, inv.Service.ScanModel(), CsvItemCatalog.Shared);
    }

    /// <summary>수집 재료 회수 후의 공간을 복사본에서 검사한다. 실제 인벤토리는 바꾸지 않는다.</summary>
    public static bool CanFitReward(NpcQuestDef d, SlotContainer inventory, IItemCatalog catalog)
    {
        var copy = new SlotContainer(catalog, inventory.Capacity, inventory.Unlocked);
        for (int i = 0; i < inventory.Unlocked; i++)
        {
            if (!inventory[i].IsEmpty)
            {
                copy.PutAt(i, inventory[i]);
            }
        }
        if (d.Objective == NpcQuestObjective.Collect && !d.IsRecordsTarget && !copy.TryRemove(d.Target, d.Count))
        {
            return false;
        }
        return copy.TryAddAll(new ItemStack(d.RewardItem, d.RewardCount));
    }

    public static void Apply(NpcQuestDef d)
    {
        if (d == null)
        {
            return;
        }
        DatabaseManager.money += d.Gold;
        LevelSystem.GainExp(d.Exp, d.Id);
        Transform player = PlayerController.instance != null ? PlayerController.instance.transform : null;
        if (d.Gold > 0 && player != null)
        {
            HitFeedback.SpawnText(null, player.position + Vector3.up, "+" + d.Gold + " G", HitFeedback.TextKind.Gold);
        }
        InventoryManager inv = InventoryManager.instance;
        if (inv != null && !string.IsNullOrEmpty(d.RewardItem) && d.RewardCount > 0)
        {
            int given = 0;
            for (int i = 0; i < d.RewardCount; i++)
            {
                if (inv.CreatItem(d.RewardItem))
                {
                    given++;
                }
            }
            if (given < d.RewardCount)
            {
                GameLog.Warn(GameLog.Cat.Item, "NPC 퀘스트 보상 일부 못 받음(가방 가득): " + d.RewardItem + " " + given + "/" + d.RewardCount);
            }
            if (given > 0)
            {
                PickupLog.Report(d.RewardItem, given, "R");
            }
        }
        if (inv != null)
        {
            inv.RebuildItemStackCache();
            inv.SaveInventory();
        }
        Sfx.Play(SfxId.Victory, 0.6f);
    }
}
