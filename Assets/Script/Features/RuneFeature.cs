using UnityEngine;

/// <summary>
/// 룬 (4층계 해금 · 장비 아님 · EQUIP-SLOT-REFORM v2): 자리만 확보한 뼈대. 내용(특정 스킬 강화)은 기획 확정 전이라 비어 있다.
/// 해금 상태는 ContentFeatures(SlotUnlocks.csv), 진입점(Open)은 아직 안내 문구만 띄운다. 세트 효과에는 들어가지 않는다.
/// </summary>
public static class RuneFeature
{
    public static bool IsUnlocked => ContentFeatures.IsUnlocked(ContentFeatures.Rune);

    /// <summary>진입점 자리. 잠겨 있으면 false. 내용이 정해지면 여기서 창을 연다</summary>
    public static bool Open()
    {
        if (!IsUnlocked)
        {
            return false;
        }
        PickupLog.Notice(Loc.T("RUNE: coming soon"));
        return true;
    }
}
