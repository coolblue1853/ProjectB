using UnityEngine;

/// <summary>
/// 어비스 안개 두 겹 (ArtStyle.md 머리말: 안개 낀 깊이). 메인 카메라에 붙어 따라다니며 배경 사이에 끼운다.
/// 먼 안개: 산(-17)과 나무(-16) 사이 — 정렬 -16 에 z 를 뒤로 밀어 나무 뒤에 그린다. 가까운 안개: 나무와 풀(-2) 사이 -4.
/// 텍스처는 Tools/pixelfx/gen_fog.py (24 PPU, 가로 이음매 없음). Sprite-Lit 기본 재질이라 낮밤 조명을 따른다.
/// </summary>
public class AbyssFog : MonoBehaviour
{
    public const bool Enabled = true;
    const float TileWidth = 20f; // 480px / 24 PPU

    struct LayerDef
    {
        public string Texture; public int Order; public float Z; public float Y; public float Parallax; public float Drift;
        public LayerDef(string tex, int order, float z, float y, float parallax, float drift)
        {
            Texture = tex; Order = order; Z = z; Y = y; Parallax = parallax; Drift = drift;
        }
    }

    static readonly LayerDef[] Layers =
    {
        new LayerDef("Atmosphere/FogFar", -16, 2f, -0.6f, 0.15f, 0.12f),
        new LayerDef("Atmosphere/FogNear", -4, 1f, -2.1f, 0.35f, 0.25f),
    };

    [SerializeField] private Transform[] _layers;
    [SerializeField] private float[] _parallax;
    [SerializeField] private float[] _drift;
    private Transform _cam;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        Camera cam = Camera.main;
        if (!Enabled || cam == null || cam.GetComponentInChildren<AbyssFog>() != null)
        {
            return;
        }
        GameObject root = new GameObject("AbyssFog");
        root.transform.SetParent(cam.transform, false);
        root.AddComponent<AbyssFog>().Build(cam.transform);
    }

    void Build(Transform cam)
    {
        _cam = cam;
        _layers = new Transform[Layers.Length];
        _parallax = new float[Layers.Length];
        _drift = new float[Layers.Length];
        for (int i = 0; i < Layers.Length; i++)
        {
            LayerDef def = Layers[i];
            Sprite sprite = Resources.Load<Sprite>(def.Texture);
            if (sprite == null)
            {
                Debug.LogWarning("[PB] AbyssFog 텍스처 없음: " + def.Texture);
                continue;
            }
            GameObject go = new GameObject(sprite.name);
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(TileWidth * 3f, sprite.bounds.size.y);
            sr.sortingOrder = def.Order;
            // 카메라(z=-5) 기준 로컬 z: 같은 정렬 순서끼리는 멀수록 먼저 그려진다
            go.transform.localPosition = new Vector3(0f, def.Y, 5f + def.Z);
            _layers[i] = go.transform;
            _parallax[i] = def.Parallax;
            _drift[i] = def.Drift;
        }
    }

    void LateUpdate()
    {
        if (_cam == null)
        {
            return;
        }
        float camX = _cam.position.x;
        for (int i = 0; i < _layers.Length; i++)
        {
            if (_layers[i] == null)
            {
                continue;
            }
            // 월드에서 (1-시차) 만큼 카메라를 따라가고 천천히 흐른다 → 카메라 기준 로컬 오프셋
            float offset = -(camX * _parallax[i]) + Time.time * _drift[i];
            float wrapped = Mathf.Repeat(offset, TileWidth) - TileWidth * 0.5f;
            Vector3 p = _layers[i].localPosition;
            p.x = wrapped;
            _layers[i].localPosition = p;
        }
    }
}
