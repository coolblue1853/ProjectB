using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 새 몹의 임시 단일 그림 연결(Logs/codex/tempmobs.md). MonsterSheet killKey 가 있는 프로필에 쓴다.
/// Temp_ID.png 가 있으면 호스트 프리팹 스프라이트를 그 그림으로 바꾸고(애니메이터 정지) 코드로 움직인다.
/// 없으면 호스트 스프라이트에 tint 색만 곱한다. 그림은 왼쪽을 보므로 flipX 로 기존 몹 규칙(scale.x 양수 = 오른쪽)에 맞춘다.
/// </summary>
public static class TempMobVisual
{
    const string EditorDir = "Assets/Art/Temp/Mobs/";
    const float FitWidth = 0.95f, FitHeight = 1.1f;
    static readonly Dictionary<string, Sprite> s_cache = new Dictionary<string, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        s_cache.Clear();
    }

    public static void Apply(EnemyHealth enemy, MonsterTable.Row row)
    {
        if (!enemy.TryGetComponent(out SpriteRenderer sr))
        {
            return;
        }
        Sprite sprite = Resolve(row.id, enemy);
        if (sprite == null)
        {
            if (row.tint.HasValue)
            {
                sr.color = sr.color * row.tint.Value;
            }
            return;
        }
        if (enemy.TryGetComponent(out Animator anim))
        {
            anim.enabled = false;
        }
        sr.sprite = sprite;
        sr.flipX = true;
        if (!enemy.TryGetComponent(out TempMobMotion _))
        {
            enemy.gameObject.AddComponent<TempMobMotion>();
        }
    }

    /// <summary>호스트 콜라이더에 맞춘 스프라이트. 발바닥을 콜라이더 바닥에, 가로 중심을 콜라이더 중심에 둔다.</summary>
    static Sprite Resolve(string id, EnemyHealth enemy)
    {
        string key = id + "@" + enemy.name.Replace("(Clone)", "");
        if (s_cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }
        Sprite made = null;
        Texture2D tex = LoadTexture(id.Replace("_BD", ""));
        if (tex != null && enemy.TryGetComponent(out BoxCollider2D box))
        {
            float fit = Mathf.Min(FitWidth * box.size.x / tex.width, FitHeight * box.size.y / tex.height); // 로컬 단위/픽셀
            float ppu = 1f / fit;
            float bottom = box.offset.y - box.size.y * 0.5f;
            float pivotY = -bottom * ppu / tex.height;
            float pivotX = 0.5f - box.offset.x * ppu / tex.width;
            made = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(pivotX, pivotY), ppu, 0, SpriteMeshType.FullRect);
            made.name = "Temp_" + id;
        }
        s_cache[key] = made;
        return made;
    }

    static Texture2D LoadTexture(string shortId)
    {
        string name = "Temp_" + shortId;
#if UNITY_EDITOR
        Texture2D t = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(EditorDir + name + ".png");
        if (t != null)
        {
            return t;
        }
#endif
        return Resources.Load<Texture2D>("TempMobs/" + name);
    }
}

/// <summary>단일 그림의 코드 움직임: 숨쉬기(세로 스케일), 이동 방향 기울기. 피격 흰 번쩍임은 SpriteHitFx 가 맡는다.</summary>
[DisallowMultipleComponent]
public sealed class TempMobMotion : MonoBehaviour
{
    const float BreathAmp = 0.035f, BreathSpeed = 3.2f, MaxTilt = 7f, TiltPerSpeed = 3f, TiltSmooth = 12f;

    EnemyHealth _enemy;
    float _breath = 1f, _tilt, _lastX, _phase;
    bool _hasLast;

    void Awake()
    {
        _enemy = GetComponent<EnemyHealth>();
        _phase = Random.value * 6.28f;
    }

    void OnEnable()
    {
        _hasLast = false;
        _breath = 1f;
        _tilt = 0f;
    }

    void OnDisable()
    {
        if (_breath != 1f && _breath > 0f)
        {
            Vector3 s = transform.localScale;
            s.y /= _breath;
            transform.localScale = s;
        }
        _breath = 1f;
        transform.rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }
        float x = transform.position.x;
        float vx = _hasLast ? (x - _lastX) / dt : 0f;
        _lastX = x;
        _hasLast = true;
        float target = Mathf.Clamp(-vx * TiltPerSpeed, -MaxTilt, MaxTilt);
        _tilt = Mathf.Lerp(_tilt, target, 1f - Mathf.Exp(-TiltSmooth * dt));
        transform.rotation = Quaternion.Euler(0f, 0f, _tilt);

        float f = _enemy != null && _enemy.IsElite ? 1f : 1f + BreathAmp * Mathf.Sin(Time.time * BreathSpeed + _phase);
        Vector3 s = transform.localScale;
        s.y = s.y / _breath * f;
        transform.localScale = s;
        _breath = f;
    }
}
