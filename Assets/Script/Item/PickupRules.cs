/// <summary>줍기 필터 설정 (BasicFeatures.md 4절, ES3 settings.pickup)</summary>
public enum PickupFilter
{
    All = 0,
    SkipCommonMaterials = 1,
}

/// <summary>
/// 줍기 규칙(순수 함수, BasicFeatures.md 4절): 필터 판정과 자석 이동 계산.
/// 장비·소비·R/U 등급은 필터와 상관없이 항상 줍는다. 거르는 것은 일반(C) 재료(Misc)뿐.
/// </summary>
public static class PickupRules
{
    public const float MagnetRadius = 2.2f;     // 이 거리 안이면 끌려오기 시작
    public const float MagnetReleaseRadius = 4.5f; // 끌려오다 이보다 멀어지면(순간이동 등) 멈춤
    public const float MagnetAccel = 30f;       // 유닛/초²
    public const float MagnetMaxSpeed = 14f;    // 유닛/초

    public static bool ShouldPickUp(string type, string rarity, PickupFilter filter)
    {
        if (filter == PickupFilter.SkipCommonMaterials)
        {
            bool common = string.IsNullOrEmpty(rarity) || rarity == "C";
            return !(type == "Misc" && common);
        }
        return true;
    }

    public static PickupFilter Next(PickupFilter f) => f == PickupFilter.All ? PickupFilter.SkipCommonMaterials : PickupFilter.All;

    /// <summary>자석 상태 갱신: 범위 안에 들어오면 켜지고, 해제 반경 밖으로 나가면 꺼진다</summary>
    public static bool UpdateMagnet(bool active, float sqrDistance)
    {
        if (active)
        {
            return sqrDistance <= MagnetReleaseRadius * MagnetReleaseRadius;
        }
        return sqrDistance <= MagnetRadius * MagnetRadius;
    }

    /// <summary>가속 후 속도(최대 속도 제한)</summary>
    public static float Accelerate(float speed, float dt)
    {
        float v = speed + MagnetAccel * dt;
        return v > MagnetMaxSpeed ? MagnetMaxSpeed : v;
    }
}
