using UnityEngine;

/// <summary>
/// 포자등 두꺼비 특성(MonsterSheet trait=spore): 맞으면 제자리에 포자 구름을 남기고, 안에 선 플레이어에게 작은 지속 피해를 준다.
/// 구름은 개체당 하나를 재사용한다. 쿨다운 동안 다시 맞아도 새로 만들지 않는다.
/// </summary>
public sealed class SporeCloud : MonoBehaviour
{
    const float Duration = 2.5f, Cooldown = 3.5f, TickInterval = 0.8f, Radius = 1.3f;
    const int TickDamage = 5;
    static Sprite s_sprite;

    EnemyHealth _enemy;
    Transform _cloud;
    SpriteRenderer _cloudSr;
    float _until, _nextAllowed, _nextTick;
    PlayerHealthManager _hp;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        s_sprite = null;
    }

    void Awake()
    {
        _enemy = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        if (_enemy != null)
        {
            _enemy.OnDamaged += OnDamaged;
        }
        _until = 0f;
        _nextAllowed = 0f;
    }

    void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnDamaged -= OnDamaged;
        }
        if (_cloud != null)
        {
            _cloud.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (_cloud != null)
        {
            Destroy(_cloud.gameObject);
        }
    }

    void OnDamaged(object sender, System.EventArgs e)
    {
        if (Time.time < _nextAllowed || _enemy.nowHP <= 0)
        {
            return;
        }
        _nextAllowed = Time.time + Cooldown;
        _until = Time.time + Duration;
        _nextTick = Time.time + 0.3f;
        EnsureCloud();
        _cloud.position = transform.position + Vector3.up * 0.2f;
        _cloud.gameObject.SetActive(true);
    }

    void EnsureCloud()
    {
        if (_cloud != null)
        {
            return;
        }
        var go = new GameObject("SporeCloud");
        _cloud = go.transform; // 부모 없음: 두꺼비가 움직이거나 뒤집혀도 제자리
        _cloud.localScale = Vector3.one * (Radius * 2f);
        _cloudSr = go.AddComponent<SpriteRenderer>();
        _cloudSr.sprite = CloudSprite();
        if (TryGetComponent(out SpriteRenderer host))
        {
            _cloudSr.sortingLayerID = host.sortingLayerID;
            _cloudSr.sortingOrder = host.sortingOrder + 1;
        }
        go.SetActive(false);
    }

    void Update()
    {
        if (_cloud == null || !_cloud.gameObject.activeSelf)
        {
            return;
        }
        float left = _until - Time.time;
        if (left <= 0f)
        {
            _cloud.gameObject.SetActive(false);
            return;
        }
        float pulse = 0.9f + 0.1f * Mathf.Sin(Time.time * 6f);
        _cloudSr.color = new Color(0.55f, 0.78f, 0.35f, 0.38f * Mathf.Clamp01(left / 0.6f) * pulse);
        if (Time.time < _nextTick)
        {
            return;
        }
        _nextTick = Time.time + TickInterval;
        PlayerController pc = PlayerController.instance;
        if (pc == null)
        {
            return;
        }
        if (_hp == null)
        {
            _hp = PlayerHealthManager.Instance;
        }
        Vector3 d = pc.transform.position - _cloud.position;
        d.z = 0f;
        if (_hp != null && _hp.CanBeHit && d.sqrMagnitude <= Radius * Radius)
        {
            _hp.damage2Player(TickDamage, 0f, 0f, Vector2.zero, _cloud.position.x, false, 0.25f);
        }
    }

    static Sprite CloudSprite()
    {
        if (s_sprite != null)
        {
            return s_sprite;
        }
        const int n = 32;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "SporeCloudTex" };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f - n * 0.5f) / (n * 0.5f);
                float dy = (y + 0.5f - n * 0.5f) / (n * 0.5f);
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r >= 1f)
                {
                    px[y * n + x] = new Color32(0, 0, 0, 0);
                }
                else if (r > 0.75f)
                {
                    px[y * n + x] = new Color32(255, 255, 255, 255);
                }
                else
                {
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(((x / 4 + y / 4) % 2 == 0) ? 170 : 110));
                }
            }
        }
        tex.SetPixels32(px);
        tex.Apply(false, true);
        s_sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        return s_sprite;
    }
}
