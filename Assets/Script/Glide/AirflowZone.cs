using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기류 구역(FloorCoreContent.md 2절). 위치 = 아래 가운데, 크기 = 폭×높이. 효과는 GlideMotor 가 위치 폴링으로 적용한다(콜라이더 없음).
/// 표시는 코드 스프라이트 흐름선(상승 위로, 하강 아래로, 순풍·역풍 옆으로). 맵이 꺼지면 같이 꺼져 폴링 대상에서 빠진다.
/// </summary>
public class AirflowZone : MonoBehaviour
{
    private const int LineCount = 12;
    private static readonly List<AirflowZone> _all = new List<AirflowZone>();
    private static Sprite _line;

    private AirflowKind _kind;
    private float _width = 6f;
    private float _height = 12f;
    private Transform[] _lines;
    private SpriteRenderer[] _renderers;

    public static IReadOnlyList<AirflowZone> All => _all;
    public AirflowKind Kind => _kind;
    public float Width => _width;
    public float Height => _height;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _all.Clear();
        _line = null;
    }

    public static AirflowZone Create(Transform parent, Vector3 worldPos, AirflowKind kind, float width, float height)
    {
        var go = new GameObject("Airflow_" + kind);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        var zone = go.AddComponent<AirflowZone>();
        zone.Init(kind, width, height);
        return zone;
    }

    public void Init(AirflowKind kind, float width, float height)
    {
        _kind = kind;
        _width = width;
        _height = height;
        BuildLines();
    }

    private void OnEnable()
    {
        _all.Add(this);
    }

    private void OnDisable()
    {
        _all.Remove(this);
    }

    public bool Contains(Vector2 p)
    {
        Vector3 o = transform.position;
        return Mathf.Abs(p.x - o.x) <= _width * 0.5f && p.y >= o.y && p.y <= o.y + _height;
    }

    private bool Horizontal => _kind == AirflowKind.Tail || _kind == AirflowKind.Head;

    private void BuildLines()
    {
        Color c = TintOf(_kind);
        _lines = new Transform[LineCount];
        _renderers = new SpriteRenderer[LineCount];
        for (int i = 0; i < LineCount; i++)
        {
            var go = new GameObject("Line");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LineSprite();
            sr.color = c;
            sr.sortingOrder = 18;
            if (Horizontal)
            {
                go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            _lines[i] = go.transform;
            _renderers[i] = sr;
        }
    }

    private static Color TintOf(AirflowKind kind)
    {
        switch (kind)
        {
            case AirflowKind.UpStrong: return new Color(0.75f, 1f, 1f, 0.8f);
            case AirflowKind.Down: return new Color(0.7f, 0.55f, 0.95f, 0.75f);
            case AirflowKind.Tail: return new Color(0.9f, 1f, 0.8f, 0.7f);
            case AirflowKind.Head: return new Color(1f, 0.85f, 0.7f, 0.7f);
            default: return new Color(0.7f, 0.92f, 1f, 0.6f);
        }
    }

    /// <summary>1×8 흰 선(PPU 16), 아래 가운데 기준</summary>
    private static Sprite LineSprite()
    {
        if (_line != null)
        {
            return _line;
        }
        Color32[] px = MiniGameSprites.Blank(8);
        for (int y = 0; y < 8; y++)
        {
            px[y] = new Color32(255, 255, 255, (byte)(y < 2 || y > 5 ? 150 : 255));
        }
        _line = MiniGameSprites.MakeSprite(px, 1, 8);
        return _line;
    }

    private void Update()
    {
        if (_lines == null)
        {
            return;
        }
        float speed = _kind == AirflowKind.UpStrong ? 7f : 4.5f;
        float dir = _kind == AirflowKind.Down || _kind == AirflowKind.Head ? -1f : 1f;
        float span = Horizontal ? _width : _height;
        float cross = Horizontal ? _height : _width;
        for (int i = 0; i < LineCount; i++)
        {
            float phase = Mathf.Repeat(i / (float)LineCount + Time.time * speed * dir / span + i * 0.37f, 1f);
            float lane = (Mathf.Repeat(i * 0.6180339f, 1f) - 0.5f) * cross * 0.9f;
            float along = phase * span;
            Vector3 lp = Horizontal ? new Vector3(along - _width * 0.5f, lane + _height * 0.5f, 0f) : new Vector3(lane, along, 0f);
            _lines[i].localPosition = lp;
            Color c = _renderers[i].color;
            c.a = TintOf(_kind).a * Mathf.Sin(phase * Mathf.PI);
            _renderers[i].color = c;
        }
    }
}
