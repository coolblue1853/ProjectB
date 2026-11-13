using System.Collections.Generic;

/// <summary>세트별 정의 단계 (SetItem.csv 키 "RabbitArmor_2" → 세트 RabbitArmor, 단계 2). 효과 문자열은 SetEffectApplier.Parse 로 해석</summary>
public sealed class SetTierTable
{
    /// <summary>SetItem.csv 세트 단계 상한(P7 11단계). 단계 조회 루프는 이 값을 공유한다</summary>
    public const int MaxTier = 11;

    readonly Dictionary<string, SortedDictionary<int, string>> tiers = new Dictionary<string, SortedDictionary<int, string>>();

    public void Define(string setKey, int tier, string effect)
    {
        if (!tiers.TryGetValue(setKey, out var map))
        {
            map = new SortedDictionary<int, string>();
            tiers[setKey] = map;
        }
        map[tier] = effect;
    }

    public IEnumerable<int> TiersOf(string setKey)
    {
        return tiers.TryGetValue(setKey, out var map) ? map.Keys : (IEnumerable<int>)new int[0];
    }

    public string EffectOf(string setKey, int tier)
    {
        return tiers.TryGetValue(setKey, out var map) && map.TryGetValue(tier, out string e) ? e : null;
    }

    public bool HasSet(string setKey) => tiers.ContainsKey(setKey);
}

public sealed class SetBonusDiff
{
    public readonly List<(string set, int tier)> Apply = new List<(string, int)>();
    public readonly List<(string set, int tier)> Remove = new List<(string, int)>();
    public bool IsEmpty => Apply.Count == 0 && Remove.Count == 0;
}

/// <summary>
/// [v2 예정 11-10] 세트 단계: 시작 5칸(의상·무기·반지·목걸이·귀걸이) 기준 최대 5셋, 세트마다 2·4 또는 3·5 단계. 옛 2/4/6 은 정리·수치 조정.
/// 세트에 세는 것은 장비 슬롯뿐이며 펫·룬·성물(별도 콘텐츠)은 세지 않는다. 해금 슬롯(보조장비·팔찌·허리띠·마법석)의 세트 포함·6셋 이상은 기획 필요(OpenQuestions).
/// 단계가 켜질수록 외형 이펙트가 커진다(예: 2셋 약한 선조 → 4셋 강한 선조, 절차 생성, 몸 위치만 따라감) — 이 평가기는 단계 변화 이벤트만 내보내고 그림은 이펙트 쪽 담당(훅은 구현 예정).
/// 세트 효과 평가 (InventoryRefactorDesign 2.6) — 순수 함수. 장착 상태 전체에서 매번 다시 계산하고 차이만 적용하므로
/// 증감 카운터(PlusSetDict/MinusSetDict)의 누적 오차가 생기지 않는다. 규칙: 정의된 단계 중 개수 이하인 것 전부(누적).
/// </summary>
public static class SetBonusEvaluator
{
    public static Dictionary<string, int> CountPieces(IEnumerable<ItemDef> equipped)
    {
        var counts = new Dictionary<string, int>();
        foreach (ItemDef d in equipped)
        {
            if (d == null || string.IsNullOrEmpty(d.SetKey))
            {
                continue; // 빈 tfName 무시 (B-112)
            }
            counts.TryGetValue(d.SetKey, out int n);
            counts[d.SetKey] = n + 1;
        }
        return counts;
    }

    public static List<int> ActiveTiers(string setKey, int count, SetTierTable table)
    {
        var result = new List<int>();
        foreach (int t in table.TiersOf(setKey))
        {
            if (t <= count)
            {
                result.Add(t);
            }
        }
        return result;
    }

    public static SetBonusDiff Diff(IReadOnlyDictionary<string, int> before, IReadOnlyDictionary<string, int> after, SetTierTable table)
    {
        var diff = new SetBonusDiff();
        var keys = new HashSet<string>();
        foreach (var k in before.Keys) keys.Add(k);
        foreach (var k in after.Keys) keys.Add(k);
        foreach (string set in keys)
        {
            before.TryGetValue(set, out int b);
            after.TryGetValue(set, out int a);
            var was = ActiveTiers(set, b, table);
            var now = ActiveTiers(set, a, table);
            foreach (int t in now)
            {
                if (!was.Contains(t))
                {
                    diff.Apply.Add((set, t));
                }
            }
            // 해제는 높은 단계부터 (적용의 역순)
            for (int i = was.Count - 1; i >= 0; i--)
            {
                if (!now.Contains(was[i]))
                {
                    diff.Remove.Add((set, was[i]));
                }
            }
        }
        return diff;
    }
}
