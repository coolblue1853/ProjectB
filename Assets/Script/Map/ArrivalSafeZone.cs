using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 포털·맵 이동 도착점 안전지대. 도착점 반경 안에서는 몹을 스폰하지 않는다(보스 포함).
/// 포털이 켜질 때 목적지 좌표를 등록하고, 스포너가 스폰 위치를 고를 때 <see cref="PickIndex"/> 로 걸러낸다.
/// </summary>
public static class ArrivalSafeZone
{
    /// <summary>가로 반경(유닛). 화면 반폭보다 넓게 — 도착하자마자 보이는 곳에 몹이 없도록</summary>
    public const float RadiusX = 14f;
    /// <summary>세로 반경(유닛)</summary>
    public const float RadiusY = 9f;

    private static readonly List<Vector2> s_points = new List<Vector2>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_points.Clear();
    }

    /// <summary>도착점 등록(근접한 점은 중복 등록하지 않는다)</summary>
    public static void Register(Vector2 point)
    {
        foreach (Vector2 p in s_points)
        {
            if ((p - point).sqrMagnitude < 0.25f)
            {
                return;
            }
        }
        s_points.Add(point);
    }

    public static bool IsInside(Vector2 pos)
    {
        return IsInside(pos, s_points);
    }

    /// <summary>pos 가 points 중 어느 한 점의 타원 영역(RadiusX×RadiusY) 안인가</summary>
    public static bool IsInside(Vector2 pos, IReadOnlyList<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            float dx = (pos.x - points[i].x) / RadiusX;
            float dy = (pos.y - points[i].y) / RadiusY;
            if (dx * dx + dy * dy < 1f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 후보 좌표 중 안전지대 밖인 첫 인덱스. preferred 부터 순환하며 찾고, 전부 안이면 -1(이번 주기는 스폰하지 않는다)
    /// </summary>
    public static int PickIndex(IReadOnlyList<Vector2> candidates, int preferred, IReadOnlyList<Vector2> zones)
    {
        int n = candidates.Count;
        for (int k = 0; k < n; k++)
        {
            int i = (preferred + k) % n;
            if (!IsInside(candidates[i], zones))
            {
                return i;
            }
        }
        return -1;
    }

    public static IReadOnlyList<Vector2> Points => s_points;
}

/// <summary>보스 등장 배너 조건. 맵 이동 중에 켜지는 보스나 멀리 있는 보스는 배너를 띄우지 않는다(GUARD-BANNER)</summary>
public static class BossIntroRules
{
    /// <summary>플레이어와 보스 거리 한계(유닛). 이보다 멀면 '마주친' 것이 아니다</summary>
    public const float MaxDistance = 20f;

    public static bool ShouldPlay(bool inTransit, float distance)
    {
        return !inTransit && distance <= MaxDistance;
    }
}
