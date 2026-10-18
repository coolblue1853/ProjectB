using System.Collections.Generic;
using UnityEngine;

/// <summary>보스 확정 보상 정의 (Resources/Boss/&lt;보스 프리팹 이름&gt;.asset) — ProgressionDesign 2절</summary>
[CreateAssetMenu(menuName = "ProjectB/Boss Reward")]
public class BossReward : ScriptableObject
{
    [Header("Presentation (G10)")]
    public string displayName;
    [Tooltip("보스 HP바 초상 (RPG Monsters Icon)")]
    public Sprite portrait;

    [Tooltip("확정 1개를 고르는 세트 부위·무기 후보")]
    public string[] setPieces;
    public string uniqueItem;
    [Range(0f, 1f)] public float uniqueChance = 0.08f;
    [Tooltip("마지막 고유템 뒤 이 횟수째 처치에서 반드시 고유템")]
    public int pity = 12;
    public int stoneMin = 3, stoneMax = 5;
    public int firstKillStones = 5;
    [Tooltip("층계 보스면 처치 시 이 탐험가 칭호로 승급 (0 = 없음)")]
    public int grantsRank;
    [Tooltip("첫 처치 시 해금되는 아티팩트")]
    public ArtifactId firstKillArtifact;
}

/// <summary>보스 고유템 천장 판정 (순수 — 테스트 대상)</summary>
public static class BossLootRules
{
    /// <param name="killsSinceUnique">이번 처치를 포함해 마지막 고유템 이후 처치 수</param>
    public static bool ShouldDropUnique(int killsSinceUnique, float roll, float chance, int pity)
    {
        return (pity > 0 && killsSinceUnique >= pity) || roll < chance;
    }
}

/// <summary>보스 처치 보상 굴림 + ES3 기록(처치 수·천장·첫 처치)</summary>
public static class BossLoot
{
    public static List<string> Roll(string bossName, BossReward r)
    {
        var loot = new List<string>();
        string killKey = "bossSinceUnique." + bossName, firstKey = "bossFirstKill." + bossName;
        int since = (ES3.KeyExists(killKey) ? ES3.Load<int>(killKey) : 0) + 1;

        if (r.setPieces != null && r.setPieces.Length > 0)
        {
            loot.Add(r.setPieces[Random.Range(0, r.setPieces.Length)]);
        }
        int stones = Random.Range(r.stoneMin, r.stoneMax + 1);
        if (!ES3.KeyExists(firstKey))
        {
            stones += r.firstKillStones;
            ES3.Save(firstKey, true);
            ArtifactSystem.Unlock(r.firstKillArtifact);
            GameLog.Info(GameLog.Cat.Item, "보스 첫 처치 보너스: " + bossName + " 강화석 +" + r.firstKillStones);
        }
        for (int i = 0; i < stones; i++)
        {
            loot.Add(EnhanceRules.StoneName);
        }
        bool unique = !string.IsNullOrEmpty(r.uniqueItem) && BossLootRules.ShouldDropUnique(since, Random.value, r.uniqueChance, r.pity);
        if (unique)
        {
            loot.Add(r.uniqueItem);
            since = 0;
        }
        ES3.Save(killKey, since);
        if (r.grantsRank > 0 && AscentLoad.GrantRank(r.grantsRank))
        {
            PickupLog.Report("Rank up: " + AscentLoad.RankName(r.grantsRank) + " Explorer", 1, "U");
        }
        BossBanner.Show(string.IsNullOrEmpty(r.displayName) ? bossName : r.displayName, unique ? r.uniqueItem : null); // G2 승리 연출
        GameLog.Info(GameLog.Cat.Item, "보스 보상 " + bossName + ": " + string.Join(", ", loot) + (unique ? " (고유템!)" : " (천장까지 " + (r.pity - since) + ")"));
        return loot;
    }
}
