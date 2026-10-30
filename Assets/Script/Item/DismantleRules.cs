using System.Collections.Generic;

/// <summary>
/// 장비 분해 규칙(순수, BasicFeatures.md 8절, 디아·던파식). 등급·티어·강화 → 받는 재료.
/// C: 광석 1 / R: 광석 1 + 층계 결정 1 / U: 층계 결정 3. 강화 단계 2마다 층계 결정 +1(강화 비용 일부 환급).
/// 광석은 티어 0 이면 T0_Ore, 1 이상이면 T1_Ore. 수치는 Q-434 임시 가정.
/// </summary>
public static class DismantleRules
{
    public const string Ore0 = "T0_Ore";
    public const string Ore1 = "T1_Ore";

    public static List<KeyValuePair<string, int>> Outputs(string rarity, int tier, int upgrade)
    {
        var list = new List<KeyValuePair<string, int>>();
        string ore = tier >= 1 ? Ore1 : Ore0;
        int stones = 0;
        switch (rarity)
        {
            case null:
            case "":
            case "C":
                list.Add(new KeyValuePair<string, int>(ore, 1));
                break;
            case "R":
                list.Add(new KeyValuePair<string, int>(ore, 1));
                stones = 1;
                break;
            default:
                stones = 3; // U 와 그 위(레전더리 등 추가 등급)는 U 이상으로 — 상위 등급이 C 보다 덜 주지 않게
                break;
        }
        stones += upgrade > 0 ? upgrade / 2 : 0;
        if (stones > 0)
        {
            list.Add(new KeyValuePair<string, int>(EnhanceRules.StoneName, stones));
        }
        return list;
    }
}
