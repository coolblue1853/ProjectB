using UnityEngine;

/// <summary>
/// 무기 기믹 임시 HUD 공통 그리기. 3종(석궁·투창·마도서)이 같은 좌표를 쓴다. OnGUI 안에서 호출.
/// 그래픽 확정 시 교체 예정(임시 HUD, 그래픽 보류).
/// </summary>
public static class GimmickHud
{
    private static readonly Rect Area = new Rect(20, 120, 220, 24);

    public static void Draw(string text)
    {
        GUI.Label(Area, text);
    }
}
