using UnityEngine;

/// <summary>NPC 퀘스트 보상 지급: EXP(LevelSystem)·골드·아이템 (QuestBoard.GrantReward 와 같은 방식)</summary>
public static class NpcQuestRewards
{
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
                PickupLog.Report(ItemText.DisplayName(d.RewardItem), given, "R");
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
