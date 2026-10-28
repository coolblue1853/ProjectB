/// <summary>
/// 기본 무기 평타 이펙트 고르기 (GameFeelPlan 10.7). 평타 애니 이름 → 새 도트 궤적(PixelLab vfx_workbench 제작) + 무기 테마 색.
/// 근접 무기(기존 swingVfx 가 Slash 계열)만 바꾸고, 지팡이 마법 등은 기존 이펙트 그대로 둔다.
/// </summary>
public static class WeaponFx
{
    public const bool Enabled = true;
    /// <summary>공격 타격 포즈 시각(초) — 궤적 이펙트·휘두르는 소리를 이때 띄운다. 셀 공격(0.04초 × 7프레임)은 3번째 프레임에 뻗는데 궤적이 0.1초 늦게 보여(인게임 0.04초 간격 촬영) 0.04초로(10-26, 예전 리그 0.07)</summary>
    public const float StrikeDelay = 0.04f;

    /// <summary>기본 칼 3타 셀(10-28 확정)에서 칼이 지나가는 프레임 — 궤적을 이 프레임 시작에 띄운다.
    /// 고정 0.04초면 준비 동작(0.16~0.19초) 중에 궤적이 먼저 나왔다. 찌르기 v6 3번(뻗기 시작), 내려베기 v13e 3번(머리 위 → 앞), 올려베기 v18 3번(몸 아래 → 앞)</summary>
    static readonly System.Collections.Generic.Dictionary<string, int> StrikeFrame = new System.Collections.Generic.Dictionary<string, int>
    {
        { "Stab", 3 }, { "SwingDown", 3 }, { "SwingUp", 3 },
    };

    /// <summary>궤적 위치 보정(유닛, 앞 +x·위 +y). 올려베기는 3타 판정 기준점이 몸에서 멀어(약 1유닛 앞) 궤적이 칼과 떨어져 보였다 —
    /// 칼이 몸 앞에서 머리 위로 지나가므로 내려베기 궤적과 같은 거리(약 0.6유닛)로 당기고 조금 올린다(10-28 인게임 캡처)</summary>
    public static UnityEngine.Vector2 FxOffset(string anim)
    {
        return !string.IsNullOrEmpty(anim) && PixelRig.MapName(anim) == "SwingUp" ? new UnityEngine.Vector2(-0.4f, 0.2f) : UnityEngine.Vector2.zero;
    }

    /// <summary>궤적·휘두르는 소리 지연(초, 속도 1) — 리그가 셀 시간을 알면 베는 프레임 시작, 아니면 StrikeDelay</summary>
    public static float StrikeDelayFor(string anim, CharacterRig rig)
    {
        if (rig != null && !string.IsNullOrEmpty(anim) && StrikeFrame.TryGetValue(PixelRig.MapName(anim), out int f))
        {
            float t = rig.FrameStartTime(anim, f);
            if (t >= 0f)
            {
                return t;
            }
        }
        return StrikeDelay;
    }

    /// <summary>근접 평타면 새 궤적 id·상하 반전 여부를 돌려준다</summary>
    public static bool TryMelee(string anim, VfxId swing, out VfxId id, out bool flipY)
    {
        id = VfxId.None;
        flipY = false;
        if (!Enabled || string.IsNullOrEmpty(anim) || !IsMeleeSwing(swing))
        {
            return false;
        }
        switch (PixelRig.MapName(anim))
        {
            case "Stab":
                id = VfxId.WThrust;
                return true;
            case "SwingSide":
                id = VfxId.WSlashSide;
                return true;
            case "SwingUp":
                id = VfxId.WSlashDown;
                flipY = true;
                return true;
            case "SwingDown":
                id = VfxId.WSlashDown;
                return true;
            default:
                return false;
        }
    }

    public static bool IsMeleeSwing(VfxId swing)
    {
        return swing >= VfxId.SlashWhite && swing <= VfxId.CrackRed;
    }
}
