using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 미니맵에 점으로 표시할 곳(1층계 보강: 사냥 의뢰 게시대·희귀 군락). 켜져 있고(OnEnable) Visible 이 참일 때만 MinimapHud 가 1초마다 점을 놓는다.
/// 맵 오브젝트의 자식이라 다른 맵에 있으면 MinimapHud 가 영역 밖으로 보고 그리지 않는다.
/// </summary>
public class MinimapMarker : MonoBehaviour
{
    private static readonly List<MinimapMarker> s_all = new List<MinimapMarker>();

    [SerializeField] private Color _color = Color.white;
    [SerializeField] private float _size = 7f;

    /// <summary>null 이면 항상 표시. 참을 돌려줄 때만 점이 보인다</summary>
    public System.Func<bool> Visible;

    public Color DotColor => _color;
    public float DotSize => _size;
    public static IReadOnlyList<MinimapMarker> All => s_all;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_all.Clear();
    }

    public void Init(Color color, float size, System.Func<bool> visible)
    {
        _color = color;
        _size = size;
        Visible = visible;
    }

    public bool IsShown => Visible == null || Visible();

    private void OnEnable()
    {
        if (!s_all.Contains(this))
        {
            s_all.Add(this);
        }
    }

    private void OnDisable()
    {
        s_all.Remove(this);
    }
}
