using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>사냥 추정용 순수 계산. 실제 600초 입력 재생이 아닌 평균 피해·리젠 기반 모델이다.</summary>
public static class HuntingRules
{
    public sealed class Camp
    {
        public int Count;
        public float Cycle;
        public double KillSeconds;
        public double Exp;
        public double Gold;
        public double Sale;
        public double HpLoss;
    }

    public struct Result
    {
        public int Kills;
        public double Exp;
        public double Gold;
        public double Sale;
        public double HpLoss;
    }

    /// <summary>실제 타격 공식의 평균. 치명타·기본 피해 난수를 독립 적분하고, 추가타는 실제 코드처럼 부하 적용 전 값을 쓴다.</summary>
    public static double ExpectedHit(int[] damage, DamageCalculator.PlayerOffense offense,
        int defense, float ratio, float levelMul, float outgoingMul)
    {
        double sum = 0;
        float critChance = Mathf.Clamp01((damage[DamageCalculator.CritRate] + offense.critRate) / 100f);
        for (int i = 0; i < 100; i++)
        {
            float roll = (i + 0.5f) / 100f;
            var normal = DamageCalculator.Calculate(damage, offense, defense, false, false, ratio, roll, 100f, levelMul);
            var crit = DamageCalculator.Calculate(damage, offense, defense, false, false, ratio, roll, -1f, levelMul);
            double n = Mathf.Max(1, Mathf.RoundToInt(normal.damage * outgoingMul)) + normal.extraDamage;
            double c = Mathf.Max(1, Mathf.RoundToInt(crit.damage * outgoingMul)) + crit.extraDamage;
            sum += n * (1f - critChance) + c * critChance;
        }
        return sum / 100;
    }

    public static double GoldPerKill(int level, float eliteChance)
    {
        float p = Mathf.Clamp01(eliteChance);
        return level <= 0 ? 0 : (4d * level + 2) * ((1 - p) * GoldDropRules.NormalChance + p * GoldDropRules.EliteMul);
    }

    /// <summary>1920×1080·현재 카메라(20×11.25u), 발 위 2u 중심. 스폰 x 지터 ±1의 화면 안 확률.</summary>
    public static double VisibleChance(Vector2 spawn, Vector2 player)
    {
        if (Mathf.Abs(spawn.y - player.y - 2f) > 5.625f)
        {
            return 0;
        }
        return Mathf.Max(0, Mathf.Min(spawn.x + 1f, player.x + 10f)
            - Mathf.Max(spawn.x - 1f, player.x - 10f)) / 2d;
    }

    /// <summary>죽은 슬롯은 죽은 뒤 cycle초가 아니라 다음 전역 리젠 틱에서 채워진다. 첫 틱은 1초.</summary>
    public static double RefillAfter(double deathTime, double cycle)
    {
        return 1 + (Math.Floor(Math.Max(0, deathTime - 1) / cycle) + 1) * cycle;
    }

    /// <summary>각 슬롯을 순서대로 사냥. 캠프 이동·줍기 시간 포함, 사냥 시작 1초 후 초기 스폰, 종료 시각 뒤 처치는 제외.</summary>
    public static Result Simulate(IReadOnlyList<Camp> camps, double seconds, double travelSeconds)
    {
        var result = new Result();
        if (camps == null || camps.Count == 0 || seconds <= 0 || double.IsNaN(seconds)
            || double.IsInfinity(seconds) || travelSeconds < 0 || double.IsNaN(travelSeconds)
            || double.IsInfinity(travelSeconds))
        {
            return result;
        }
        var slots = new List<Camp>();
        int maxCount = 0;
        foreach (Camp camp in camps)
        {
            if (camp.Count <= 0 || camp.Cycle <= 0 || float.IsNaN(camp.Cycle) || float.IsInfinity(camp.Cycle) || camp.KillSeconds <= 0
                || double.IsNaN(camp.KillSeconds) || double.IsInfinity(camp.KillSeconds))
            {
                throw new ArgumentException("사냥 캠프의 수·주기·처치 시간은 양수여야 합니다.");
            }
            maxCount = Math.Max(maxCount, camp.Count);
        }
        // 짧은 표본도 모든 몹 종류를 지나가도록 슬롯을 캠프끼리 교차 배치한다.
        for (int i = 0; i < maxCount; i++)
        {
            foreach (Camp camp in camps)
            {
                if (i < camp.Count)
                {
                    slots.Add(camp);
                }
            }
        }
        var ready = new double[slots.Count];
        for (int i = 0; i < ready.Length; i++)
        {
            ready[i] = 1;
        }
        double time = 0;
        int cursor = 0;
        while (slots.Count > 0)
        {
            Camp camp = slots[cursor];
            time = Math.Max(time + travelSeconds, ready[cursor]) + camp.KillSeconds;
            if (time > seconds)
            {
                break;
            }
            result.Kills++;
            result.Exp += camp.Exp;
            result.Gold += camp.Gold;
            result.Sale += camp.Sale;
            result.HpLoss += camp.HpLoss;
            ready[cursor] = RefillAfter(time, camp.Cycle);
            cursor = (cursor + 1) % slots.Count;
        }
        return result;
    }

    public static double PotionCount(double hpLoss, int heal, int maxHp)
    {
        return hpLoss <= 0 || heal <= 0 || maxHp <= 0 ? 0 : hpLoss / Math.Min(heal, maxHp);
    }

    /// <summary>실제 HP 감소/초를 기준 몹의 단일 대상 DPS와 비교한다. 유효한 피해 표본이 없으면 보정하지 않는다.</summary>
    public static double ObservedDamageFactor(double damage, double seconds, int referenceHp, double referenceTtk)
    {
        double factor = damage * referenceTtk / (seconds * referenceHp);
        return damage <= 0 || seconds <= 0 || referenceHp <= 0 || referenceTtk <= 0
            || double.IsNaN(factor) || double.IsInfinity(factor) ? 1 : factor;
    }

    public static int ExpToReach(int targetLevel)
    {
        int sum = 0;
        for (int lv = 1; lv < targetLevel; lv++)
        {
            sum += LevelRules.ExpToNext(lv);
        }
        return sum;
    }

    /// <summary>퀘스트는 최소 레벨·선행 충족 시 보상을 계산. 실제 수행 시간은 별도 더하고, 목표 레벨 이후 보상은 선반영하지 않는다.</summary>
    public static double LevelingMinutes(int targetLevel, IReadOnlyList<NpcQuestDef> quests,
        Func<int, double> expPerMinute, bool includeBandits)
    {
        int level = 1, exp = 0;
        double minutes = 0;
        var completed = new HashSet<string>();
        while (level < targetLevel)
        {
            bool granted = false;
            foreach (NpcQuestDef quest in quests)
            {
                if (completed.Contains(quest.Id) || quest.MinLevel > level || quest.MinLevel >= targetLevel
                    || (!includeBandits && quest.Id.StartsWith("Q5", StringComparison.Ordinal))
                    || (!string.IsNullOrEmpty(quest.Requires) && !completed.Contains(quest.Requires)))
                {
                    continue;
                }
                completed.Add(quest.Id);
                LevelRules.AddExp(ref level, ref exp, quest.Exp);
                granted = true;
                if (level >= targetLevel)
                {
                    break;
                }
            }
            if (granted || level >= targetLevel)
            {
                continue;
            }
            double rate = expPerMinute(level);
            if (rate <= 0 || double.IsNaN(rate) || double.IsInfinity(rate))
            {
                return double.PositiveInfinity;
            }
            minutes += (LevelRules.ExpToNext(level) - exp) / rate;
            level++;
            exp = 0;
        }
        return minutes;
    }
}
