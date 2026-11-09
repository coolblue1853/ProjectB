using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가산(Additive) 혼합 이펙트 밝기 예산 (ConventionAudit U22, TASK-FX-1). 가산 레이어는 겹칠수록 더해져 화면이 하얗게 타므로,
/// 같은 시간대에 살아 있는 가산 오라(SmearAura: 글로우·바람 호·불꽃·칼선)의 수로 밝기 배율을 깎고, 너무 많으면 새 오라의 가산 부분을 끈다.
/// - 소프트 상한 SoftLayers(2): 동시 2개까지는 원래 밝기. 그 이상이면 배율 = SoftLayers / 개수(합산 밝기가 2개분을 넘지 않는다).
/// - 하드 상한 HardLayers(6): 그 이상은 가산 부분을 아예 그리지 않는다(스프라이트 스미어 자체는 그대로).
/// 만료는 시간 기준(Time.time + 수명)이라 풀 반환·비활성 훅이 필요 없다. 수치는 상수 — 바꾸면 이 파일 한 곳만 고친다.
/// </summary>
public static class AdditiveBudget
{
    public const int SoftLayers = 2;
    public const int HardLayers = 6;

    static readonly List<float> s_ends = new List<float>(16);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        s_ends.Clear();
    }

    /// <summary>현재 살아 있는 가산 오라 수(만료 정리 포함)</summary>
    public static int Active
    {
        get
        {
            Prune();
            return s_ends.Count;
        }
    }

    /// <summary>순수 계산: 동시 개수(자기 포함) → 밝기 배율 0~1. 하드 상한 초과 시 0</summary>
    public static float FactorFor(int count)
    {
        if (count > HardLayers)
        {
            return 0f;
        }
        return count <= SoftLayers ? 1f : (float)SoftLayers / count;
    }

    /// <summary>가산 오라 하나를 lifetime 초 동안 등록하고 그 오라가 쓸 밝기 배율을 돌려준다(0 이면 등록하지 않는다)</summary>
    public static float Enter(float lifetime)
    {
        Prune();
        float f = FactorFor(s_ends.Count + 1);
        if (f > 0f)
        {
            s_ends.Add(Time.time + Mathf.Max(0.05f, lifetime));
        }
        return f;
    }

    static void Prune()
    {
        float now = Time.time;
        for (int i = s_ends.Count - 1; i >= 0; i--)
        {
            if (s_ends[i] <= now)
            {
                s_ends.RemoveAt(i);
            }
        }
    }
}
