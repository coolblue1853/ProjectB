/// <summary>
/// 기본 무기 평타 이펙트 고르기 (GameFeelPlan 10.7). 평타 애니 이름 → 새 도트 궤적(PixelLab vfx_workbench 제작) + 무기 테마 색.
/// 근접 무기(기존 swingVfx 가 Slash 계열)만 바꾸고, 지팡이 마법 등은 기존 이펙트 그대로 둔다.
/// </summary>
public static class WeaponFx
{
    public const bool Enabled = true;
    /// <summary>공격 타격 포즈 시각(초) — 궤적 이펙트·휘두르는 소리를 이때 띄운다. 셀 공격(0.04초 × 7프레임)은 3번째 프레임에 뻗는데 궤적이 0.1초 늦게 보여(인게임 0.04초 간격 촬영) 0.04초로(10-26, 예전 리그 0.07)</summary>
    public const float StrikeDelay = 0.04f;

    /// <summary>기본 칼 3타 셀(10-28 확정)에서 스미어 면이 나오는 프레임 — fx_plan.py 로 승인받은 면 프레임(찌르기 4, 내려베기 3, 올려베기 4).
    /// 고정 0.04초면 준비 동작(0.16~0.19초) 중에 궤적이 먼저 나왔다</summary>
    static readonly System.Collections.Generic.Dictionary<string, int> StrikeFrame = new System.Collections.Generic.Dictionary<string, int>
    {
        { "Stab", 4 }, { "SwingDown", 3 }, { "SwingUp", 4 },
    };

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
                id = VfxId.WSmearStab;
                return true;
            case "SwingSide":
                id = VfxId.WSlashSide;
                return true;
            case "SwingUp":
                id = VfxId.WSmearUp;
                return true;
            case "SwingDown":
                id = VfxId.WSmearDown;
                return true;
            default:
                return false;
        }
    }

    /// <summary>셀 좌표로 그린 스미어(리그 셀 원점에 크기 1로 띄운다)</summary>
    public static bool IsCelSmear(VfxId id)
    {
        return id == VfxId.WSmearDown || id == VfxId.WSmearUp || id == VfxId.WSmearStab;
    }

    public static bool IsMeleeSwing(VfxId swing)
    {
        return swing >= VfxId.SlashWhite && swing <= VfxId.CrackRed;
    }
}
