/// <summary>
/// 기본 무기 평타 이펙트 고르기 (GameFeelPlan 10.7). 평타 애니 이름 → 새 도트 궤적(PixelLab vfx_workbench 제작) + 무기 테마 색.
/// 근접 무기(기존 swingVfx 가 Slash 계열)만 바꾸고, 지팡이 마법 등은 기존 이펙트 그대로 둔다.
/// </summary>
public static class WeaponFx
{
    public const bool Enabled = true;

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
