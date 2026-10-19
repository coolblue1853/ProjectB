using UnityEngine;

/// <summary>지면 높이 찾기 (이펙트 바닥 맞춤·사망 조각 착지): pos 위 1유닛에서 아래로 Ground 레이어 레이캐스트.</summary>
public static class GroundProbe
{
    private const float MaxDistance = 5f;
    private static int s_groundMask = -1;

    /// <summary>pos 아래 지면 높이(+0.05). 못 찾으면 pos.y</summary>
    public static float Y(Vector3 pos)
    {
        if (s_groundMask < 0)
        {
            s_groundMask = LayerMask.GetMask("Ground");
        }
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(pos.x, pos.y + 1f), Vector2.down, MaxDistance, s_groundMask);
        return hit.collider != null ? hit.point.y + 0.05f : pos.y;
    }
}
