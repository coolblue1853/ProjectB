using UnityEngine;

/// <summary>무기 계열 기믹 수치 규칙 (WeaponTypes.md 2절, 순수 함수 — EditMode 테스트 대상)</summary>
public static class WeaponGimmickRules
{
    // 활: 차지 샷
    public const float ChargeTier1Time = 0.35f;
    public const float ChargeTier2Time = 1.0f;
    public const float ChargeTier1Mul = 1.25f;
    public const float ChargeTier2Mul = 1.6f;
    public const int ChargeTier2Pierce = 1;
    /// <summary>평타가 시작된 뒤 이 시간 안에 나온 첫 평타 화살이 차지를 받는다(초)</summary>
    public const float ChargeValidTime = 0.8f;

    // 둔기: 충격
    public const float MaceStagger = 0.3f;
    public const int MaceIgnoreDef = 20;

    // 도끼: 쪼개기
    public const int ArmoredDefThreshold = 15;
    public const float CleaveMul = 1.2f;

    // 쌍검: 교차 마무리
    public const float FinisherDamageMul = 1.5f;
    public const float FinisherSizeMul = 1.2f;

    // 채찍: 끝 판정
    public const float WhipTipStart = 0.7f;
    public const float WhipNearEnd = 0.3f;
    public const float WhipTipMul = 1.4f;
    public const float WhipNearMul = 0.8f;
    public const float WhipLengthMul = 2.2f;

    /// <summary>누른 시간 → 차지 단계 0/1/2</summary>
    public static int ChargeTier(float held)
    {
        return held >= ChargeTier2Time ? 2 : (held >= ChargeTier1Time ? 1 : 0);
    }

    public static float ChargeDamageMul(int tier)
    {
        return tier >= 2 ? ChargeTier2Mul : (tier == 1 ? ChargeTier1Mul : 1f);
    }

    public static int ChargePierce(int tier)
    {
        return tier >= 2 ? ChargeTier2Pierce : 0;
    }

    /// <summary>방어 무시 % 를 더한다(0~100 로 제한)</summary>
    public static int AddIgnoreDef(int current, int add)
    {
        return Mathf.Clamp(current + add, 0, 100);
    }

    /// <summary>방어력이 높은 몹(기사·방패병·골렘)인가 — 몹 태그가 없어 방어력 임계값으로 판단</summary>
    public static bool IsArmored(int enemyDef)
    {
        return enemyDef >= ArmoredDefThreshold;
    }

    public static float CleaveMultiplier(int enemyDef)
    {
        return IsArmored(enemyDef) ? CleaveMul : 1f;
    }

    /// <summary>마지막 콤보 단계인가 (최대 단계가 2 미만이면 마무리 없음)</summary>
    public static bool IsFinisher(int step, int maxStep)
    {
        return maxStep >= 2 && step == maxStep;
    }

    /// <summary>채찍: 판정 상자 근접 끝에서 맞은 지점까지의 비율(0..1) → 피해 배율</summary>
    public static float WhipMultiplier(float t)
    {
        if (t >= WhipTipStart)
        {
            return WhipTipMul;
        }
        return t < WhipNearEnd ? WhipNearMul : 1f;
    }

    /// <summary>ItemSheet WeaponType 문자열 → 열거형. 빈 문자열이면 None(true), 모르는 이름이면 false</summary>
    public static bool TryParseKind(string text, out WeaponKind kind)
    {
        kind = WeaponKind.None;
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }
        return System.Enum.TryParse(text.Trim(), false, out kind);
    }
}
