using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>무기 시연대 한 단계 (Resources/Floor1WeaponTrials.csv: stage,minLevel,weapons(|로 구분),note)</summary>
public sealed class WeaponTrialDef
{
    public int Stage;
    public int MinLevel;
    public string[] Weapons;
}

/// <summary>무기 시연대 순수 규칙 (Floor1Completion R4). 테스트 대상</summary>
public static class WeaponTrialRules
{
    public const int NotStarted = 0;
    public const int Rented = 1;
    public const int Done = 2;

    public static List<WeaponTrialDef> Parse(string text)
    {
        var list = new List<WeaponTrialDef>();
        foreach (string[] c in Floor1Data.Rows(text))
        {
            if (c.Length < 3 || c[0].Trim().Length == 0)
            {
                continue;
            }
            string[] w = c[2].Split('|');
            for (int i = 0; i < w.Length; i++)
            {
                w[i] = w[i].Trim();
            }
            list.Add(new WeaponTrialDef { Stage = Floor1Data.Int(c[0]), MinLevel = Floor1Data.Int(c[1]), Weapons = w });
        }
        list.Sort((a, b) => a.Stage.CompareTo(b.Stage));
        return list;
    }

    /// <summary>지금 대화할 시연대 단계: 레벨을 채웠고 아직 끝나지 않은 가장 낮은 단계. 없으면 null</summary>
    public static WeaponTrialDef Next(IReadOnlyList<WeaponTrialDef> defs, int level, Func<int, int> stateOf)
    {
        foreach (WeaponTrialDef d in defs)
        {
            if (d.MinLevel <= level && stateOf(d.Stage) != Done)
            {
                return d;
            }
        }
        return null;
    }

    public static bool Contains(WeaponTrialDef d, string weapon)
    {
        return d != null && Array.IndexOf(d.Weapons, weapon) >= 0;
    }

    /// <summary>이 아이템이 지금 대여 중인가: 대여 중(Rented)인 단계의 무기 목록에 있으면 true</summary>
    public static bool IsRentedId(IReadOnlyList<WeaponTrialDef> defs, string id, Func<int, int> stateOf)
    {
        if (defs == null || string.IsNullOrEmpty(id))
        {
            return false;
        }
        foreach (WeaponTrialDef d in defs)
        {
            if (Contains(d, id) && stateOf(d.Stage) == Rented)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>고르기를 막아야 하나: 착용 중인 무기가 이번 시연 무기이고 고른 것이 아닐 때(돌려받을 수 없어 두 자루가 남는다)</summary>
    public static bool PickBlocked(WeaponTrialDef d, string chosen, string equipped)
    {
        return !string.IsNullOrEmpty(equipped) && equipped != chosen && Contains(d, equipped);
    }
}

/// <summary>
/// 마을 대장장이 무기 시연대(Lv5·Lv10). 계보 무기 3종을 가방에 빌려주고(대여), 써 본 뒤 하나만 남긴다(보상).
/// 상태는 ES3 정수 "WeaponTrial.S{stage}" (0 미시작/1 대여 중/2 완료). 대화는 StoryDialogues.WeaponTrial.cs, Lua 함수 WeaponTrialRent/Pick.
/// </summary>
public static class WeaponTrial
{
    private static List<WeaponTrialDef> s_defs;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_defs = null;
        s_rentCacheFrame = -1;
        s_rented.Clear();
    }

    public static IReadOnlyList<WeaponTrialDef> Defs
    {
        get
        {
            if (s_defs == null)
            {
                TextAsset csv = Resources.Load<TextAsset>("Floor1WeaponTrials");
                s_defs = WeaponTrialRules.Parse(csv != null ? csv.text : "");
            }
            return s_defs;
        }
    }

    public static string Key(int stage)
    {
        return "WeaponTrial.S" + stage;
    }

    public static int StateOf(int stage)
    {
        return ES3.KeyExists(Key(stage)) ? ES3.Load<int>(Key(stage)) : WeaponTrialRules.NotStarted;
    }

    // 판매·분해 검사가 목록을 훑을 때마다 ES3 를 읽지 않도록 프레임당 한 번만 계산한다(Rent/Pick 이 바로 무효화)
    private static int s_rentCacheFrame = -1;
    private static readonly HashSet<string> s_rented = new HashSet<string>();

    /// <summary>대여 중인 무기인가 (대여 중에는 판매·분해 불가, ItemTradeRules). 정산(Pick) 뒤 고른 무기는 풀린다</summary>
    public static bool IsRented(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }
        if (s_rentCacheFrame != Time.frameCount)
        {
            s_rentCacheFrame = Time.frameCount;
            s_rented.Clear();
            foreach (WeaponTrialDef d in Defs)
            {
                if (StateOf(d.Stage) != WeaponTrialRules.Rented)
                {
                    continue;
                }
                foreach (string w in d.Weapons)
                {
                    s_rented.Add(w);
                }
            }
        }
        return s_rented.Contains(itemId);
    }

    /// <summary>대화 선택지용 한글 아이템 이름 (Lua: WeaponTrialName)</summary>
    public static string ItemName(string id)
    {
        return ItemText.DisplayName(id);
    }

    public static string RentConversation(int stage) => "Trial/" + stage + "/rent";

    public static string PickConversation(int stage) => "Trial/" + stage + "/pick";

    /// <summary>대장장이에게 말을 걸었을 때: 시연대 대화를 연다. 열었으면 true (아니면 기존 상호작용으로)</summary>
    public static bool TryTalk(Transform npc)
    {
        if (!Floor1Data.On("trial"))
        {
            return false;
        }
        WeaponTrialDef d = WeaponTrialRules.Next(Defs, LevelSystem.Level, StateOf);
        if (d == null)
        {
            return false;
        }
        string conv = StateOf(d.Stage) == WeaponTrialRules.Rented ? PickConversation(d.Stage) : RentConversation(d.Stage);
        return DialogueBoot.Talk(conv, npc);
    }

    private static WeaponTrialDef Find(int stage)
    {
        foreach (WeaponTrialDef d in Defs)
        {
            if (d.Stage == stage)
            {
                return d;
            }
        }
        return null;
    }

    /// <summary>Lua: 무기 3종을 가방에 빌려준다. 자리가 모자라면 아무것도 주지 않는다</summary>
    public static void Rent(string stageText)
    {
        WeaponTrialDef d = Find(Floor1Data.Int(stageText));
        InventoryManager inv = InventoryManager.instance;
        if (d == null || inv == null || StateOf(d.Stage) != WeaponTrialRules.NotStarted)
        {
            return;
        }
        var given = new List<string>();
        foreach (string w in d.Weapons)
        {
            if (!inv.CreatItem(w))
            {
                break;
            }
            given.Add(w);
        }
        if (given.Count < d.Weapons.Length)
        {
            foreach (string w in given)
            {
                inv.RemoveOne(w);
            }
            PickupLog.Warn("무기 " + d.Weapons.Length + "자루를 둘 가방 공간이 부족합니다. 비우고 다시 오세요.");
            return;
        }
        ES3.Save(Key(d.Stage), WeaponTrialRules.Rented);
        s_rentCacheFrame = -1;
        inv.RebuildItemStackCache();
        inv.SaveInventory();
        var names = new List<string>();
        foreach (string w in d.Weapons)
        {
            names.Add(ItemName(w));
        }
        PickupLog.Notice("시연 무기 대여: " + string.Join(", ", names) + " - 사냥터에서 써 보고 하나를 고르세요 (대여 중에는 판매·분해 불가)");
    }

    /// <summary>Lua: 고른 무기만 남기고 나머지 대여품을 돌려받는다</summary>
    public static void Pick(string stageText, string chosen)
    {
        WeaponTrialDef d = Find(Floor1Data.Int(stageText));
        InventoryManager inv = InventoryManager.instance;
        if (d == null || inv == null || StateOf(d.Stage) != WeaponTrialRules.Rented || !WeaponTrialRules.Contains(d, chosen))
        {
            return;
        }
        ItemCheck worn = inv.EquippedFor("Weapon");
        string wornName = worn != null ? worn.name : null;
        if (WeaponTrialRules.PickBlocked(d, chosen, wornName))
        {
            PickupLog.Warn("착용 중인 대여 무기는 돌려받을 수 없습니다. 무기를 벗고 다시 고르세요.");
            return;
        }
        foreach (string w in d.Weapons)
        {
            if (w != chosen)
            {
                inv.RemoveOne(w);
            }
        }
        if (inv.CountItem(chosen) == 0 && wornName != chosen)
        {
            inv.CreatItem(chosen); // 대여품을 이미 팔았거나 버린 경우에도 보상은 받는다
        }
        ES3.Save(Key(d.Stage), WeaponTrialRules.Done);
        s_rentCacheFrame = -1;
        inv.RebuildItemStackCache();
        inv.SaveInventory();
        Sfx.Play(SfxId.Victory, 0.6f);
        PickupLog.Notice("시연 보상: " + ItemName(chosen) + " 획득");
    }
}
