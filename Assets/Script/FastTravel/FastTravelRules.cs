using System.Collections.Generic;

/// <summary>빠른 이동(마차) 순수 규칙 (FastTravel.md §3~4). 요금 공식·이동 금지 판정·세이브 문자열.</summary>
public static class FastTravelRules
{
    public const int BaseFee = 50;
    public const int PerStep = 25;
    public const int SecondFloorMul = 2;
    public const float CombatLockSeconds = 3f;

    public enum Block { None, Dead, Boss, Combat, Locked, NoGold }

    /// <summary>이동 직후 재진입 가드 시간(초). 확인 팝업 연타·이중 입력으로 두 번 결제되는 것을 막는다</summary>
    public const float TravelCooldown = 1.5f;

    /// <summary>직전 이동 시각(now 와 같은 시계)으로부터 가드 시간 안이면 true</summary>
    public static bool InCooldown(float now, float lastTravelAt)
    {
        return now - lastTravelAt < TravelCooldown;
    }

    /// <summary>fee = (50 + 25 x |순번차|), 2층계 이상 도착은 x2 (임시 가정 Q-FT2)</summary>
    public static int Fee(int fromOrder, int toOrder, int toFloor)
    {
        int steps = fromOrder > toOrder ? fromOrder - toOrder : toOrder - fromOrder;
        int fee = BaseFee + PerStep * steps;
        return toFloor >= 2 ? fee * SecondFloorMul : fee;
    }

    /// <summary>이동 가능 여부. 사망·잠금·보스·전투 직후·골드 순으로 첫 사유를 돌려준다 (secondsSinceHurt 음수 = 피격 기록 없음)</summary>
    public static Block Check(bool dead, bool bossNear, float secondsSinceHurt, bool open, int gold, int fee)
    {
        if (dead) { return Block.Dead; }
        if (!open) { return Block.Locked; }
        if (bossNear) { return Block.Boss; }
        if (secondsSinceHurt >= 0f && secondsSinceHurt < CombatLockSeconds) { return Block.Combat; }
        if (gold < fee) { return Block.NoGold; }
        return Block.None;
    }

    public static string Reason(Block b)
    {
        switch (b)
        {
            case Block.Dead: return "지금은 이동할 수 없습니다";
            case Block.Boss: return "보스가 가까워 이동할 수 없습니다";
            case Block.Combat: return "전투 직후에는 이동할 수 없습니다";
            case Block.Locked: return "먼저 그 지역의 마차 퀘스트를 깨야 합니다";
            case Block.NoGold: return "골드가 부족합니다";
            default: return "";
        }
    }

    /// <summary>열린 정류장 id 들을 ES3 문자열("맵;맵;…")로. 중복 제거, 순서는 입력 순</summary>
    public static string Join(IEnumerable<string> ids)
    {
        var seen = new HashSet<string>();
        var list = new List<string>();
        foreach (string id in ids)
        {
            if (!string.IsNullOrEmpty(id) && seen.Add(id))
            {
                list.Add(id);
            }
        }
        return string.Join(";", list);
    }

    public static List<string> Split(string text)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return list;
        }
        foreach (string part in text.Split(';'))
        {
            string t = part.Trim();
            if (t.Length > 0 && !list.Contains(t))
            {
                list.Add(t);
            }
        }
        return list;
    }
}
