/// <summary>
/// % 단위 정수 스탯을 배율로 바꾸는 공용 헬퍼. 정수 나눗셈(x/100 == 0) 버그를 막기 위해 % 계산은 여기로 모은다.
/// </summary>
public static class StatMath
{
    /// <summary>정수 퍼센트를 비율로 (25 → 0.25)</summary>
    public static float Pct(int percent) => percent / 100f;

    /// <summary>공격 속도 배율. 시간 값은 이 값으로 나누고, 속도 값은 곱한다.</summary>
    public static float AttackSpeedMul => 1f + DatabaseManager.attackSpeedBuff / 100f;

    /// <summary>이동 속도 배율</summary>
    public static float MoveSpeedMul => 1f + DatabaseManager.SpeedBuff / 100f;

    /// <summary>받는 피해 배율</summary>
    public static float IncomingDamageMul => 1f + DatabaseManager.incIncomingDmg / 100f;
}
