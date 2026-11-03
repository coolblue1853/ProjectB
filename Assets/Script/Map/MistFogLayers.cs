using UnityEngine;

/// <summary>
/// 안개 늪(Lv1_MistSwamp) 흰 안개 2겹. 기존 Resources/Atmosphere 안개 스프라이트(AbyssFog 와 같은 것)를 알파 0.25 로
/// 맵 가로 폭만큼 깔고 서로 다른 속도로 천천히 흘린다. 맵 자식이라 맵이 꺼지면 같이 멈춘다.
/// </summary>
public sealed class MistFogLayers : MonoBehaviour
{
    private const float Alpha = 0.25f;
    private const float TileWidth = 20f; // 480px / 24 PPU

    private struct Layer
    {
        public string Texture;
        public float Y, Drift;
        public int Order;
    }

    private static readonly Layer[] Layers =
    {
        new Layer { Texture = "Atmosphere/FogFar", Y = 2.2f, Drift = 0.35f, Order = 18 },
        new Layer { Texture = "Atmosphere/FogNear", Y = 0.6f, Drift = -0.55f, Order = 19 },
    };

    private Transform[] _layers;
    private float[] _baseX;
    private float[] _drift;

    public static void Attach(MapInfo map)
    {
        if (map.GetComponentInChildren<MistFogLayers>(true) != null || !map.TryGetTileXRange(out float left, out float right))
        {
            return;
        }
        var go = new GameObject("MistFog");
        go.transform.SetParent(map.transform, false);
        // 지면 y: 맵 바닥 윗면(MapInfo.cameraBottom + 8) 기준
        float ground = map.cameraBottom + 8f;
        go.AddComponent<MistFogLayers>().Build(left, right, ground, map.transform.position.z);
    }

    private void Build(float left, float right, float groundY, float z)
    {
        _layers = new Transform[Layers.Length];
        _baseX = new float[Layers.Length];
        _drift = new float[Layers.Length];
        float width = right - left + TileWidth * 2f;
        for (int i = 0; i < Layers.Length; i++)
        {
            Sprite sprite = Resources.Load<Sprite>(Layers[i].Texture);
            if (sprite == null)
            {
                continue;
            }
            var go = new GameObject(sprite.name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(width, sprite.bounds.size.y);
            sr.sortingOrder = Layers[i].Order;
            sr.color = new Color(1f, 1f, 1f, Alpha);
            _baseX[i] = (left + right) * 0.5f;
            go.transform.position = new Vector3(_baseX[i], groundY + Layers[i].Y, z);
            _layers[i] = go.transform;
            _drift[i] = Layers[i].Drift;
        }
    }

    private void Update()
    {
        for (int i = 0; _layers != null && i < _layers.Length; i++)
        {
            if (_layers[i] == null)
            {
                continue;
            }
            float off = Mathf.Repeat(Time.time * _drift[i], TileWidth);
            Vector3 p = _layers[i].position;
            p.x = _baseX[i] + off - TileWidth * 0.5f;
            _layers[i].position = p;
        }
    }
}
