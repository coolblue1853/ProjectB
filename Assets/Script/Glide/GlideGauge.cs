using UnityEngine;

/// <summary>활공 남은 시간 막대(플레이어 머리 위 작은 도트 막대). 활공 중이거나 시간이 줄어 있을 때만 보인다</summary>
public class GlideGauge : MonoBehaviour
{
    private const float Width = 1.6f;
    private Transform _fill;
    private SpriteRenderer _fillSr;
    private SpriteRenderer _bg;
    private static Sprite _px;

    public static GlideGauge Create()
    {
        var go = new GameObject("GlideGauge");
        var g = go.AddComponent<GlideGauge>();
        g.Build();
        return g;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _px = null;
    }

    private static Sprite Pixel()
    {
        if (_px == null)
        {
            Color32[] px = { new Color32(255, 255, 255, 255) };
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels32(px);
            tex.Apply();
            _px = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }
        return _px;
    }

    private void Build()
    {
        var bg = new GameObject("Bg");
        bg.transform.SetParent(transform, false);
        _bg = bg.AddComponent<SpriteRenderer>();
        _bg.sprite = Pixel();
        _bg.color = new Color(0f, 0f, 0f, 0.6f);
        _bg.sortingOrder = 90;
        bg.transform.localScale = new Vector3(Width + 0.16f, 0.3f, 1f);
        bg.transform.localPosition = new Vector3(-Width * 0.5f - 0.08f, 0f, 0f);
        var fill = new GameObject("Fill");
        fill.transform.SetParent(transform, false);
        _fillSr = fill.AddComponent<SpriteRenderer>();
        _fillSr.sprite = Pixel();
        _fillSr.sortingOrder = 91;
        _fill = fill.transform;
        _fill.localPosition = new Vector3(-Width * 0.5f, 0f, 0f);
    }

    public void Set(bool show, Vector3 playerPos, float ratio)
    {
        if (_bg.enabled != show)
        {
            _bg.enabled = show;
            _fillSr.enabled = show;
        }
        if (!show)
        {
            return;
        }
        transform.position = new Vector3(playerPos.x, playerPos.y + 3.4f, playerPos.z);
        ratio = Mathf.Clamp01(ratio);
        _fill.localScale = new Vector3(Width * ratio, 0.18f, 1f);
        _fillSr.color = ratio > 0.25f ? new Color(0.7f, 0.95f, 1f, 1f) : new Color(1f, 0.55f, 0.4f, 1f);
    }
}
