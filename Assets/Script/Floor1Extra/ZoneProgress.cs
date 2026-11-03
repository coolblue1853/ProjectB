using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 1층계 구역 수호자 안내·구역 완료율 (Floor1Completion.md §14-7, L6).
/// 구역 입장 알림은 수호자 이름 + 목표 한 줄만, 개수 정보는 도감 구역 완료율 탭에 둔다.
/// 구역·수호자 매핑 데이터 파일이 없어 최소 상수 표(<see cref="Zones"/>)로 둔다(미니보스 id 는 Resources/MiniBosses.csv 와 일치).
/// 계산 함수는 순수(정적)이고, 런타임 수집(<see cref="Snapshot"/>)만 씬·세이브를 읽는다.
/// </summary>
public static class ZoneProgress
{
    public sealed class Zone
    {
        public string Map;
        public string ZoneKo, ZoneEn;
        public string MiniId;
        public string GuardKo, GuardEn;
        public string GoalKo, GoalEn;
        public string SetPrefix;
        public string SetKo;
        public int Elites;
    }

    public static readonly Zone[] Zones =
    {
        new Zone { Map = "Lv1_NormalForest", ZoneKo = "숲", ZoneEn = "Forest", MiniId = "AlphaWolf_BD", GuardKo = "우두머리 늑대", GuardEn = "Alpha Wolf", GoalKo = "숲 왼쪽 끝에 있다", GoalEn = "at the far left", SetPrefix = "Wolf", SetKo = "늑대", Elites = 1 },
        new Zone { Map = "Lv1_DeepForest", ZoneKo = "깊은 숲", ZoneEn = "Deep Forest", MiniId = "OldBear_BD", GuardKo = "늙은 곰", GuardEn = "Old Bear", GoalKo = "깊은 곳에 있다", GoalEn = "deep within", SetPrefix = "Grove", SetKo = "숲", Elites = 1 },
        new Zone { Map = "Lv1_Swamp", ZoneKo = "늪", ZoneEn = "Swamp", MiniId = "OldCroc_BD", GuardKo = "늙은 악어", GuardEn = "Old Croc", GoalKo = "늪 아래쪽에 있다", GoalEn = "in the low bog", SetPrefix = "Bog", SetKo = "늪", Elites = 1 },
        new Zone { Map = "Lv1_BanditFortress", ZoneKo = "성채", ZoneEn = "Fortress", MiniId = "BanditLieutenant_BD", GuardKo = "산적 부관", GuardEn = "Bandit Lieutenant", GoalKo = "성채 왼쪽 끝에 있다", GoalEn = "at the far left", SetPrefix = "Raider", SetKo = "약탈자", Elites = 1 },
        new Zone { Map = "Lv1_TowerOutskirts", ZoneKo = "탑 근교", ZoneEn = "Tower Outskirts", MiniId = "ElderGolem_BD", GuardKo = "원로 골렘", GuardEn = "Elder Golem", GoalKo = "벌판 가운데 있다", GoalEn = "in the open field", SetPrefix = "Stone", SetKo = "돌", Elites = 1 },
    };

    public static readonly string[] SetParts = { "Cap", "HandArmor", "LegArmor", "Shoes", "Armor" };
    public const string RecipeResource = "ArmorCraftRecipes";
    public const int SetPartTotal = 5;

    /// <summary>현재 구역 맵 이름(ZoneBanner 가 맵 진입 때 갱신). 수호자 없는 맵이면 표에서 못 찾는다</summary>
    public static string CurrentMap { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        CurrentMap = null;
        s_chests = null;
        s_nodes = null;
        s_recipes = null;
    }

    public static void SetCurrent(string map)
    {
        CurrentMap = map;
    }

    public static Zone Find(string map)
    {
        if (string.IsNullOrEmpty(map))
        {
            return null;
        }
        foreach (Zone z in Zones)
        {
            if (z.Map == map)
            {
                return z;
            }
        }
        return null;
    }

    // ---- 순수 계산 ----

    public static int Percent(int done, int total)
    {
        if (total <= 0)
        {
            return 0;
        }
        return Mathf.Clamp(done * 100 / total, 0, 100);
    }

    /// <summary>구역 입장 한 줄: "깊은 숲의 수호자 — 늙은 곰: 깊은 곳에 있다" / 처치 후 "… 처치 완료"</summary>
    public static string GuardianLine(Zone z, bool defeated, bool korean)
    {
        if (z == null)
        {
            return "";
        }
        if (korean)
        {
            return z.ZoneKo + "의 수호자 — " + z.GuardKo + (defeated ? " 처치 완료" : ": " + z.GoalKo);
        }
        return z.ZoneEn + " Guardian - " + z.GuardEn + (defeated ? " defeated" : ": " + z.GoalEn);
    }

    /// <summary>세트 완성 부위 수 근사: 장착 수 + 가방의 세트 부위 수(최대 5)</summary>
    public static int OwnedParts(int equipped, int inBag)
    {
        return Mathf.Clamp(equipped + inBag, 0, SetParts.Length);
    }

    /// <summary>다음 만들 부위의 재료 보유/필요 합. 세트가 이미 5부위면 false</summary>
    public static bool NextPartMaterials(IList<SelfCraftPages.Recipe> recipes, string prefix, int ownedParts, Func<string, int> have, out int got, out int need)
    {
        got = 0;
        need = 0;
        if (recipes == null || ownedParts >= SetParts.Length)
        {
            return false;
        }
        string result = prefix + SetParts[Mathf.Max(0, ownedParts)];
        foreach (SelfCraftPages.Recipe r in recipes)
        {
            if (r.Result != result)
            {
                continue;
            }
            for (int i = 0; i < r.Ids.Count; i++)
            {
                need += r.Counts[i];
                got += Mathf.Min(have(r.Ids[i]), r.Counts[i]);
            }
            return need > 0;
        }
        return false;
    }

    public static string SetMaterialLine(Zone z, int got, int need, bool korean)
    {
        return korean ? "세트 재료 " + got + "/" + need + " (" + z.SetKo + ")" : "Set mats " + got + "/" + need + " (" + z.SetPrefix + ")";
    }

    public struct Counts
    {
        public int ChestDone, ChestTotal, Colonies, Elites, MiniDone, MiniTotal, SetParts;
    }

    /// <summary>완료율 = 진행이 저장되는 항목(상자·미니보스·세트 부위)의 달성/전체. 군락·정예는 저장 기록이 없어 개수만 표시한다</summary>
    public static int CompletionPercent(Counts c)
    {
        return Percent(c.ChestDone + c.MiniDone + c.SetParts, c.ChestTotal + c.MiniTotal + SetPartTotal);
    }

    public static string ZoneRow(Zone z, Counts c, bool korean)
    {
        var sb = new StringBuilder();
        if (korean)
        {
            sb.Append("<color=#FFC640>").Append(z.ZoneKo).Append("</color>  ").Append(CompletionPercent(c)).Append("%\n");
            sb.Append("  상자 ").Append(c.ChestDone).Append('/').Append(c.ChestTotal)
              .Append(" · 군락 ").Append(c.Colonies).Append("곳 · 정예 ").Append(c.Elites)
              .Append(" · 미니보스 ").Append(c.MiniDone).Append('/').Append(c.MiniTotal)
              .Append(" · 세트 ").Append(c.SetParts).Append('/').Append(SetPartTotal);
        }
        else
        {
            sb.Append("<color=#FFC640>").Append(z.ZoneEn).Append("</color>  ").Append(CompletionPercent(c)).Append("%\n");
            sb.Append("  Chests ").Append(c.ChestDone).Append('/').Append(c.ChestTotal)
              .Append(" · Colonies ").Append(c.Colonies).Append(" · Elite ").Append(c.Elites)
              .Append(" · Mini-boss ").Append(c.MiniDone).Append('/').Append(c.MiniTotal)
              .Append(" · Set ").Append(c.SetParts).Append('/').Append(SetPartTotal);
        }
        return sb.ToString();
    }

    // ---- 런타임 수집 ----

    private static List<TreasureDef> s_chests;
    private static List<RareNodeDef> s_nodes;
    private static List<SelfCraftPages.Recipe> s_recipes;

    public static bool IsDefeated(Zone z)
    {
        return z != null && BestiaryRecord.Kills(z.MiniId) > 0;
    }

    private static int OwnedPartsNow(Zone z)
    {
        DatabaseManager.setsEffectStack.TryGetValue(z.SetPrefix + "Armor", out int equipped);
        int inBag = 0;
        InventoryManager inv = InventoryManager.instance;
        if (inv != null)
        {
            foreach (string part in SetParts)
            {
                if (inv.CountItem(z.SetPrefix + part) > 0)
                {
                    inBag++;
                }
            }
        }
        return OwnedParts(equipped, inBag);
    }

    public static Counts Snapshot(Zone z)
    {
        if (s_chests == null)
        {
            TextAsset t = Resources.Load<TextAsset>("Floor1Treasures");
            s_chests = t != null ? TreasureLedger.Parse(t.text) : new List<TreasureDef>();
        }
        if (s_nodes == null)
        {
            TextAsset t = Resources.Load<TextAsset>("Floor1RareNodes");
            s_nodes = t != null ? RareColonyRules.Parse(t.text) : new List<RareNodeDef>();
        }
        var c = new Counts { Elites = z.Elites, SetParts = OwnedPartsNow(z), MiniTotal = 1 };
        foreach (TreasureDef d in s_chests)
        {
            if (d.Map == z.Map)
            {
                c.ChestTotal++;
                if (TreasureLedger.IsOpened(d.Id))
                {
                    c.ChestDone++;
                }
            }
        }
        foreach (RareNodeDef d in s_nodes)
        {
            if (d.Map == z.Map)
            {
                c.Colonies++;
            }
        }
        c.MiniDone = IsDefeated(z) ? 1 : 0;
        return c;
    }

    /// <summary>추적 HUD 한 줄: 현재 구역 세트의 다음 부위 재료 n/m. 해당 없으면 빈 문자열</summary>
    public static string TrackerLine()
    {
        Zone z = Find(CurrentMap);
        InventoryManager inv = InventoryManager.instance;
        if (z == null || inv == null)
        {
            return "";
        }
        if (s_recipes == null)
        {
            TextAsset t = Resources.Load<TextAsset>(RecipeResource);
            s_recipes = SelfCraftPages.Parse(t != null ? t.text : "");
        }
        if (!NextPartMaterials(s_recipes, z.SetPrefix, OwnedPartsNow(z), inv.CountItem, out int got, out int need))
        {
            return "";
        }
        return "<color=#9FD4A0>" + SetMaterialLine(z, got, need, Loc.IsKorean) + "</color>\n";
    }

    /// <summary>도감 구역 완료율 탭 본문</summary>
    public static string TabText()
    {
        var sb = new StringBuilder();
        foreach (Zone z in Zones)
        {
            sb.Append(ZoneRow(z, Snapshot(z), Loc.IsKorean)).Append("\n\n");
        }
        return sb.ToString();
    }
}
