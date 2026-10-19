using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단일 스프라이트 시체(MonsterBuilder 로 만든 코드 도트 몹)를 격자 조각으로 부숴 튕기며 디졸브시킨다.
/// 원본 몹의 파츠 시체(조각이 물리로 흩어지며 디졸브)와 사망 연출을 맞추기 위한 것 (1층계 폴리싱).
/// 조각 스프라이트는 원본 스프라이트별로 한 번만 만들어 캐시하고, 조각 오브젝트는 풀로 재사용한다(전투 중 할당 없음).
/// 물리는 Rigidbody 없이 한 Update 에서 적분하고, 바닥은 시작 시 한 번 레이캐스트한 높이에서 튕긴다.
/// </summary>
public class DeathShards : MonoBehaviour
{
    private const int MaxShards = 96;
    private const float Life = 0.95f, DissolveDelay = 0.25f, DissolveDuration = 0.6f, Gravity = 18f;
    private const int SmallGrid = 3, LargeGrid = 4, LargeSpritePixels = 72;

    private struct Piece
    {
        public Sprite sprite;
        public Vector2 offset; // 원본 피벗 기준 로컬 중심(유닛)
    }

    private class Shard
    {
        public GameObject go;
        public SpriteRenderer sr;
        public SpriteHitFx fx;
        public Vector2 velocity;
        public float spin, age, groundY;
        public bool dissolving;
    }

    private static DeathShards s_instance;
    private static readonly Dictionary<Sprite, Piece[]> s_cache = new Dictionary<Sprite, Piece[]>();

    private readonly List<Shard> _free = new List<Shard>();
    private readonly List<Shard> _live = new List<Shard>();
    private int _created;

    public static int LiveCount => s_instance != null ? s_instance._live.Count : 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
        foreach (Piece[] pieces in s_cache.Values) // 도메인 리로드를 끈 에디터에서 런타임 스프라이트가 남지 않게
        {
            if (pieces == null)
            {
                continue;
            }
            foreach (Piece p in pieces)
            {
                if (p.sprite != null)
                {
                    Destroy(p.sprite);
                }
            }
        }
        s_cache.Clear();
    }

    /// <summary>source 의 현재 스프라이트를 조각내 흩뿌린다. 성공하면 true(호출자가 원본을 숨긴다).</summary>
    public static bool Shatter(SpriteRenderer source, float dirSign)
    {
        if (source == null || source.sprite == null)
        {
            return false;
        }
        Piece[] pieces = GetPieces(source.sprite);
        if (pieces == null)
        {
            return false;
        }
        if (s_instance == null)
        {
            var root = new GameObject("DeathShards");
            s_instance = root.AddComponent<DeathShards>();
        }
        // 조각을 전부 낼 수 없으면 부서지지 않고 기존 디졸브로 (일부만 튀고 나머지가 사라지는 것 방지)
        if (s_instance._free.Count + (MaxShards - s_instance._created) < pieces.Length)
        {
            return false;
        }
        s_instance.Spawn(source, pieces, dirSign);
        return true;
    }

    private static Piece[] GetPieces(Sprite sprite)
    {
        if (s_cache.TryGetValue(sprite, out Piece[] cached))
        {
            return cached;
        }
        Piece[] pieces = null;
        // 타이트 패킹·회전된 아틀라스 스프라이트는 사각형으로 자를 수 없다 → 조각 없이 기존 디졸브만
        if (!sprite.packed || (sprite.packingMode == SpritePackingMode.Rectangle && sprite.packingRotation == SpritePackingRotation.None))
        {
            Rect tr = sprite.textureRect;
            Vector2 trOffset = sprite.textureRectOffset;
            int grid = sprite.rect.height > LargeSpritePixels ? LargeGrid : SmallGrid;
            float cw = tr.width / grid, ch = tr.height / grid;
            pieces = new Piece[grid * grid];
            for (int y = 0; y < grid; y++)
            {
                for (int x = 0; x < grid; x++)
                {
                    var r = new Rect(Mathf.Floor(tr.x + x * cw), Mathf.Floor(tr.y + y * ch), Mathf.Floor(cw), Mathf.Floor(ch));
                    Sprite s = Sprite.Create(sprite.texture, r, new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    s.name = sprite.name + "_shard" + (y * grid + x);
                    Vector2 centerPx = trOffset + new Vector2((x + 0.5f) * cw, (y + 0.5f) * ch);
                    pieces[y * grid + x] = new Piece { sprite = s, offset = (centerPx - sprite.pivot) / sprite.pixelsPerUnit };
                }
            }
        }
        s_cache[sprite] = pieces;
        return pieces;
    }

    private void Spawn(SpriteRenderer source, Piece[] pieces, float dirSign)
    {
        Transform st = source.transform;
        Vector3 scale = st.lossyScale;
        bool flip = (scale.x < 0f) != source.flipX;
        float groundY = GroundProbe.Y(source.bounds.center);
        if (groundY > source.bounds.center.y)
        {
            groundY = source.bounds.min.y;
        }
        foreach (Piece p in pieces)
        {
            Shard s = Rent(); // Shatter 가 여유를 미리 확인했으므로 null 이 아니다
            Vector2 off = p.offset;
            if (source.flipX)
            {
                off.x = -off.x;
            }
            s.go.transform.position = st.TransformPoint(off);
            s.go.transform.rotation = Quaternion.Euler(0f, 0f, st.eulerAngles.z); // 기운 몸체면 조각도 같은 각도에서 시작
            s.go.transform.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);
            s.sr.sprite = p.sprite;
            s.sr.flipX = flip;
            s.sr.color = source.color;
            s.sr.sortingLayerID = source.sortingLayerID;
            s.sr.sortingOrder = source.sortingOrder;
            float side = (flip ? -off.x : off.x);
            s.velocity = new Vector2(dirSign * Random.Range(0.6f, 2.4f) + side * 2.5f, Random.Range(2.5f, 5.5f));
            s.spin = Random.Range(-540f, 540f);
            s.age = 0f;
            s.groundY = groundY;
            s.dissolving = false;
            s.go.SetActive(true);
            _live.Add(s);
        }
    }

    private Shard Rent()
    {
        if (_free.Count > 0)
        {
            Shard s = _free[_free.Count - 1];
            _free.RemoveAt(_free.Count - 1);
            return s;
        }
        if (_created >= MaxShards)
        {
            return null;
        }
        _created++;
        var go = new GameObject("Shard");
        go.transform.SetParent(transform, false);
        go.SetActive(false);
        var sr = go.AddComponent<SpriteRenderer>();
        var fx = go.AddComponent<SpriteHitFx>(); // M_EnemySprite 인스턴스(디졸브)
        return new Shard { go = go, sr = sr, fx = fx };
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = _live.Count - 1; i >= 0; i--)
        {
            Shard s = _live[i];
            s.age += dt;
            if (s.age >= Life)
            {
                s.go.SetActive(false); // SpriteHitFx.OnDisable 가 디졸브 값을 되돌린다
                _live.RemoveAt(i);
                _free.Add(s);
                continue;
            }
            if (!s.dissolving && s.age >= DissolveDelay)
            {
                s.dissolving = true;
                s.fx.Dissolve(DissolveDuration);
            }
            s.velocity.y -= Gravity * dt;
            Transform t = s.go.transform;
            Vector3 p = t.position + (Vector3)(s.velocity * dt);
            if (p.y < s.groundY && s.velocity.y < 0f)
            {
                p.y = s.groundY;
                s.velocity = new Vector2(s.velocity.x * 0.5f, -s.velocity.y * 0.25f);
                s.spin *= 0.5f;
            }
            t.position = p;
            t.Rotate(0f, 0f, s.spin * dt);
        }
    }
}
