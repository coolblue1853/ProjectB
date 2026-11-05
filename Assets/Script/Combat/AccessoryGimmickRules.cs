using UnityEngine;

/// <summary>
/// 산적 대장 보상 장신구 기믹 이름 키 (Floor1Completion.md 15절). 장착 여부는 equipGear[슬롯,0](ItemSheet name)으로 조회한다.
/// </summary>
public static class AccessoryGimmick
{
    public const string PlunderMedal = "BanditPlunderMedal";
    public const string LastStandRing = "ChiefsLastStandRing";

    /// <summary>오기 반지 쿨다운 런타임 상태 (세이브 안 함)</summary>
    public static LastStandRules LastStand = new LastStandRules();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        LastStand = new LastStandRules();
    }

    /// <summary>장비 배열 [슬롯,0]=아이템 이름 중 name 이 있는가 (순수, 테스트용)</summary>
    public static bool IsEquipped(string[,] gear, string name)
    {
        if (gear == null || string.IsNullOrEmpty(name))
        {
            return false;
        }
        for (int i = 0; i < gear.GetLength(0); i++)
        {
            if (gear[i, 0] == name)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>지금 세이브 데이터의 장비에 name 이 있는가</summary>
    public static bool IsEquipped(string name)
    {
        return SaveManager.instance != null && SaveManager.instance.datas != null && IsEquipped(SaveManager.instance.datas.equipGear, name);
    }
}

/// <summary>약탈의 훈장: 몹 처치 골드 드롭 8% 확률로 x1.5 (결정 11-05, 구 x3). 상태 없음.</summary>
public static class PlunderRules
{
    public const float Chance = 0.08f;
    public const float GoldMul = 1.5f;

    /// <summary>roll = 0~1 난수. 발동 여부</summary>
    public static bool Roll(float roll)
    {
        return roll < Chance;
    }

    /// <summary>장착·발동 시 골드 x1.5 내림 (장착 안 했거나 미발동이면 그대로)</summary>
    public static int Apply(int gold, bool equipped, float roll)
    {
        return equipped && gold > 0 && Roll(roll) ? Mathf.FloorToInt(gold * GoldMul) : gold;
    }
}

/// <summary>대장의 오기 반지: 치사 피해를 받으면 HP 1 로 버티고 2초 무적, 재사용 180초. 쿨다운은 런타임 값(세이브 안 함).</summary>
public sealed class LastStandRules
{
    public const float Cooldown = 180f;
    public const float InvulnSeconds = 2f;

    float readyAt;

    /// <summary>남은 쿨다운(초). 0 이면 사용 가능</summary>
    public float Remaining(float now)
    {
        return Mathf.Max(0f, readyAt - now);
    }

    public bool IsReady(float now)
    {
        return now >= readyAt;
    }

    /// <summary>
    /// 피해가 치사(damage >= hp)이고 쿨다운이 끝났으면 발동: 쿨다운 시작, 피해를 hp-1 로 줄여 반환(true).
    /// 치사가 아니거나 쿨다운 중이면 피해 그대로(false). hp 가 0 이하면 이미 죽었으므로 발동 안 함
    /// </summary>
    public bool TryRescue(int hp, ref int damage, float now)
    {
        if (hp <= 0 || damage < hp || !IsReady(now))
        {
            return false;
        }
        damage = hp - 1;
        readyAt = now + Cooldown;
        return true;
    }

    public void Reset()
    {
        readyAt = 0f;
    }
}
