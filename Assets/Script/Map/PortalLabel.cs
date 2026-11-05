using TMPro;
using UnityEngine;

/// <summary>
/// 포털 위 안내(11-02): 목적지 구역 이름을 늘 작게 띄우고, 플레이어가 서 있으면 "↑" 힌트로 밝아진다. 봉인된 포털은 잠금 표시.
/// 같은 맵 안 이동 포털(샛길 좌우 끝)은 "건너편"으로 적는다.
/// </summary>
public sealed class PortalLabel : MonoBehaviour
{
    private const float Height = 2.4f;
    private static readonly Color Idle = new Color(0.85f, 0.85f, 0.78f, 0.7f);
    private static readonly Color Near = new Color(1f, 0.92f, 0.55f, 1f);

    private Portal _portal;
    private PortalSeal _seal;
    private TextMeshPro _text;
    private string _title;
    private bool _near, _sealed;

    public static void Attach(Portal portal, PortalSeal seal)
    {
        if (portal == null || portal.GetComponentInChildren<PortalLabel>(true) != null)
        {
            return;
        }
        var go = new GameObject("PortalLabel");
        go.transform.SetParent(portal.transform, false);
        go.AddComponent<PortalLabel>().Init(portal, seal);
    }

    private void Init(Portal portal, PortalSeal seal)
    {
        _portal = portal;
        _seal = seal;
        _title = ResolveTitle(portal);
        _text = gameObject.AddComponent<TextMeshPro>();
        _text.font = UiTheme.Get().pixelFont;
        _text.fontSize = 2.2f;
        _text.alignment = TextAlignmentOptions.Center;
        _text.rectTransform.sizeDelta = new Vector2(5f, 1.5f);
        _text.outlineWidth = 0.2f;
        _text.outlineColor = Color.black;
        _text.sortingOrder = 501;
        var sr = portal.GetComponentInChildren<SpriteRenderer>(true);
        _text.sortingLayerID = sr != null ? sr.sortingLayerID : 0;
        transform.position = portal.transform.position + Vector3.up * Height;
        _near = true; // 첫 갱신 강제
        Refresh(false);
    }

    private static string ResolveTitle(Portal portal)
    {
        if (!string.IsNullOrEmpty(portal.labelOverride))
        {
            return Loc.T(portal.labelOverride);
        }
        MapInfo dest = portal.openMap != null ? portal.openMap.GetComponent<MapInfo>() : null;
        if (dest == null && portal.destination != null)
        {
            dest = portal.destination.GetComponentInParent<MapInfo>();
        }
        MapInfo here = portal.GetComponentInParent<MapInfo>();
        if (dest == null || dest == here)
        {
            return Loc.T("Other side");
        }
        return string.IsNullOrEmpty(dest.displayName) ? dest.name : Loc.T(dest.displayName);
    }

    private void LateUpdate()
    {
        bool near = _portal != null && _portal.player != null && !DatabaseManager.isOpenUI;
        bool locked = _seal != null && !_seal.IsOpen;
        if (near != _near || locked != _sealed)
        {
            Refresh(near);
        }
    }

    private void Refresh(bool near)
    {
        _near = near;
        _sealed = _seal != null && !_seal.IsOpen;
        string line = _sealed ? "[봉인] " + _title : _title;
        _text.text = near && !_sealed ? "↑ " + line : line;
        _text.color = _sealed ? new Color(0.9f, 0.45f, 0.4f, near ? 1f : 0.7f) : (near ? Near : Idle);
    }
}
