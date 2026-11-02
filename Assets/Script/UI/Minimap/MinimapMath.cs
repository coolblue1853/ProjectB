using UnityEngine;

/// <summary>미니맵 좌표 계산(순수, BasicFeatures.md 9절). 테스트 대상</summary>
public static class MinimapMath
{
    /// <summary>월드 좌표 → 0~1 (맵 영역 밖은 0~1 로 자름)</summary>
    public static Vector2 WorldToUv(Vector2 world, Rect area)
    {
        if (area.width <= 0f || area.height <= 0f)
        {
            return new Vector2(0.5f, 0.5f);
        }
        return new Vector2(Mathf.Clamp01((world.x - area.xMin) / area.width), Mathf.Clamp01((world.y - area.yMin) / area.height));
    }

    /// <summary>NPC 점 색: 완료 가능 금색, 수락 가능 연노랑, 레벨 부족 회색, 그 밖(진행 중·없음) 초록</summary>
    public static Color NpcDotColor(NpcQuestStatus s)
    {
        switch (s)
        {
            case NpcQuestStatus.Ready: return new Color(1f, 0.82f, 0.15f);
            case NpcQuestStatus.Available: return new Color(1f, 1f, 0.6f);
            case NpcQuestStatus.LevelLow: return new Color(0.6f, 0.62f, 0.6f);
            default: return new Color(0.45f, 1f, 0.5f);
        }
    }

    /// <summary>NPC 점 크기: 완료 가능 10, 수락 가능 8, 그 밖 6</summary>
    public static float NpcDotSize(NpcQuestStatus s)
    {
        return s == NpcQuestStatus.Ready ? 10f : s == NpcQuestStatus.Available ? 8f : 6f;
    }

    /// <summary>비율을 유지해 maxSize 안에 맞춘 크기(가로·세로 최소 minSide)</summary>
    public static Vector2 Fit(Vector2 content, Vector2 maxSize, float minSide)
    {
        if (content.x <= 0f || content.y <= 0f)
        {
            return maxSize;
        }
        float s = Mathf.Min(maxSize.x / content.x, maxSize.y / content.y);
        return new Vector2(Mathf.Max(minSide, content.x * s), Mathf.Max(minSide, content.y * s));
    }
}
