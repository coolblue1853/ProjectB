using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 카테고리별 개발용 로그. `PB_DEBUG` 심볼이 있을 때만 컴파일된다([Conditional]) → 정식 빌드에서는 호출 자체가 사라져 비용 0.
/// 형식: "[PB][Category] message" — read_console의 filter_text로 카테고리별 수집이 쉽다.
/// 경고(Warn)·이상(Anomaly)은 심볼과 무관하게 항상 남긴다(문제 추적용).
/// </summary>
public static class GameLog
{
    public enum Cat { Combat, Pool, Spawn, Item, Save, Player, AI, Map, Audio, Perf, Monitor }

    /// <summary>카테고리별 on/off (기본 전부 켬). 너무 시끄러우면 프로브에서 끈다.</summary>
    public static readonly bool[] Enabled = { true, true, true, true, true, true, true, true, true, true, true };

    [Conditional("PB_DEBUG")]
    public static void Info(Cat cat, string message, Object context = null)
    {
        if (!Enabled[(int)cat])
        {
            return;
        }
        Debug.Log("[PB][" + cat + "] " + message, context);
    }

    public static void Warn(Cat cat, string message, Object context = null)
    {
        Debug.LogWarning("[PB][" + cat + "] " + message, context);
    }

    /// <summary>논리적으로 일어나면 안 되는 상황. 에러로 남겨 자동 테스트에서 실패로 잡히게 한다.</summary>
    public static void Anomaly(Cat cat, string message, Object context = null)
    {
        Debug.LogError("[PB][" + cat + "][ANOMALY] " + message, context);
    }
}
