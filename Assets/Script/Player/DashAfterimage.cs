using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대시 잔상: AnyPortrait 캐릭터(MeshRenderer 여러 개, 정점 수 ~130) 또는 픽셀 리그(SpriteRenderer 부위)의 현재 모습을 복사해 반투명 청색 실루엣으로 남기고 흐리게 지운다.
/// 잔상 객체·Mesh·리스트를 모두 풀링해서 대시 중에도 할당이 거의 없다.
/// 사용: DashAfterimage.Begin(portraitRoot, duration)
/// </summary>
public class DashAfterimage : MonoBehaviour
{
    const float Interval = 0.04f, FadeTime = 0.25f;
    static readonly Color Tint = new Color(0.45f, 0.8f, 1f, 0.55f);
    static readonly int ColorId = Shader.PropertyToID("_Color");

    class Part
    {
        public Transform t;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public Mesh mesh;
    }

    class Ghost
    {
        public GameObject root;
        public readonly List<Part> parts = new List<Part>();
        public readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>(); // 픽셀 리그 부위는 SpriteRenderer 로 그대로 복사
        public float born;
    }

    static Material ghostMaterial;
    static Material ghostSpriteMaterial;
    static readonly List<Vector3> verts = new List<Vector3>();
    static readonly List<Vector2> uvs = new List<Vector2>();
    static readonly List<int> tris = new List<int>();

    Transform source;
    readonly List<MeshRenderer> sourceRenderers = new List<MeshRenderer>();
    readonly List<SpriteRenderer> sourceSprites = new List<SpriteRenderer>(); // 픽셀 리그(PixelRig) 부위
    readonly Stack<Ghost> pool = new Stack<Ghost>();
    readonly List<Ghost> live = new List<Ghost>();
    MaterialPropertyBlock block; // 필드 초기화에서 생성 불가(Unity 제약) → Awake
    float emitUntil, nextEmit;
    int orderShift;
    PixelRig pixelRig;

    public static void Begin(Component portrait, float duration)
    {
        if (portrait == null)
        {
            return;
        }
        var host = portrait.GetComponent<DashAfterimage>();
        if (host == null)
        {
            host = portrait.gameObject.AddComponent<DashAfterimage>();
            host.source = portrait.transform;
        }
        host.emitUntil = Time.time + duration;
        host.nextEmit = 0f;
    }

    void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        float now = Time.time;
        if (now < emitUntil && now >= nextEmit)
        {
            nextEmit = now + Interval;
            Snapshot();
        }
        for (int i = live.Count - 1; i >= 0; i--)
        {
            Ghost g = live[i];
            float k = (now - g.born) / FadeTime;
            if (k >= 1f)
            {
                g.root.SetActive(false);
                live.RemoveAt(i);
                pool.Push(g);
                continue;
            }
            var c = new Color(Tint.r, Tint.g, Tint.b, Tint.a * (1f - k));
            block.SetColor(ColorId, c);
            foreach (Part p in g.parts)
            {
                p.renderer.SetPropertyBlock(block);
            }
            foreach (SpriteRenderer sr in g.sprites)
            {
                sr.color = c;
            }
        }
    }

    void Snapshot()
    {
        EnsureMaterial();
        source.GetComponentsInChildren(false, sourceRenderers);
        source.GetComponentsInChildren(false, sourceSprites);
        // 픽셀 리그를 쓰면 숨겨 둔 예전 AnyPortrait 메쉬는 빼야 한다(안 보이게 둔 메쉬까지 복사해 큰 파란 사각형 잔상이 찍혔다, 10-26)
        if (pixelRig == null)
        {
            pixelRig = source.GetComponentInChildren<PixelRig>();
        }
        if (pixelRig != null && pixelRig.isActiveAndEnabled)
        {
            sourceRenderers.Clear();
        }
        int minOrder = int.MaxValue, maxOrder = int.MinValue;
        foreach (MeshRenderer r in sourceRenderers)
        {
            minOrder = Mathf.Min(minOrder, r.sortingOrder);
            maxOrder = Mathf.Max(maxOrder, r.sortingOrder);
        }
        foreach (SpriteRenderer r in sourceSprites)
        {
            minOrder = Mathf.Min(minOrder, r.sortingOrder);
            maxOrder = Mathf.Max(maxOrder, r.sortingOrder);
        }
        orderShift = maxOrder - minOrder + 1; // 잔상 전체를 캐릭터 뒤로

        Ghost g = pool.Count > 0 ? pool.Pop() : new Ghost { root = new GameObject("DashGhost") };
        int used = 0;
        foreach (MeshRenderer src in sourceRenderers)
        {
            if (!src.enabled || !src.TryGetComponent(out MeshFilter mf) || mf.sharedMesh == null)
            {
                continue;
            }
            Part p = used < g.parts.Count ? g.parts[used] : AddPart(g);
            used++;
            CopyMesh(mf.sharedMesh, p.mesh);
            p.t.SetPositionAndRotation(src.transform.position, src.transform.rotation);
            p.t.localScale = src.transform.lossyScale;
            p.renderer.sortingLayerID = src.sortingLayerID;
            p.renderer.sortingOrder = src.sortingOrder - orderShift;
            Texture tex = src.sharedMaterial != null ? src.sharedMaterial.mainTexture : null;
            if (tex != null)
            {
                block.SetTexture("_MainTex", tex);
            }
            block.SetColor(ColorId, Tint);
            p.renderer.SetPropertyBlock(block);
            p.t.gameObject.SetActive(true);
        }
        // 픽셀 리그 부위: 메시로 옮기면 64×64 부위 사각형이 투명도 없이 칠해져 큰 파란 사각형이 찍혔다(10-26) → SpriteRenderer 로 그림 그대로
        int usedSprites = 0;
        foreach (SpriteRenderer src in sourceSprites)
        {
            if (!src.enabled || src.sprite == null)
            {
                continue;
            }
            SpriteRenderer sr = usedSprites < g.sprites.Count ? g.sprites[usedSprites] : AddSpritePart(g);
            usedSprites++;
            sr.sprite = src.sprite;
            sr.flipX = src.flipX;
            sr.flipY = src.flipY;
            sr.color = Tint;
            sr.sortingLayerID = src.sortingLayerID;
            sr.sortingOrder = src.sortingOrder - orderShift;
            sr.transform.SetPositionAndRotation(src.transform.position, src.transform.rotation);
            sr.transform.localScale = src.transform.lossyScale;
            sr.gameObject.SetActive(true);
        }
        for (int i = used; i < g.parts.Count; i++)
        {
            g.parts[i].t.gameObject.SetActive(false);
        }
        for (int i = usedSprites; i < g.sprites.Count; i++)
        {
            g.sprites[i].gameObject.SetActive(false);
        }
        g.born = Time.time;
        g.root.SetActive(true);
        live.Add(g);
    }

    static Part AddPart(Ghost g)
    {
        var go = new GameObject("Part");
        go.transform.SetParent(g.root.transform, false);
        var p = new Part
        {
            t = go.transform,
            filter = go.AddComponent<MeshFilter>(),
            renderer = go.AddComponent<MeshRenderer>(),
            mesh = new Mesh { name = "GhostMesh" },
        };
        p.mesh.MarkDynamic();
        p.filter.sharedMesh = p.mesh;
        p.renderer.sharedMaterial = ghostMaterial;
        g.parts.Add(p);
        return p;
    }

    static SpriteRenderer AddSpritePart(Ghost g)
    {
        var go = new GameObject("SpritePart");
        go.transform.SetParent(g.root.transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        if (ghostSpriteMaterial != null)
        {
            sr.sharedMaterial = ghostSpriteMaterial;
        }
        g.sprites.Add(sr);
        return sr;
    }

    static void CopyMesh(Mesh src, Mesh dst)
    {
        dst.Clear();
        src.GetVertices(verts);
        src.GetUVs(0, uvs);
        src.GetTriangles(tris, 0);
        dst.SetVertices(verts);
        dst.SetUVs(0, uvs);
        dst.SetTriangles(tris, 0);
    }

    static void EnsureMaterial()
    {
        if (ghostMaterial == null)
        {
            ghostMaterial = new Material(Shader.Find("Sprites/Default")) { name = "DashGhost" };
        }
        if (ghostSpriteMaterial == null)
        {
            // 잔상은 조명을 받지 않는다(밤에도 같은 청색). 셰이더가 없으면 SpriteRenderer 기본 머티리얼
            Shader unlit = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlit != null)
            {
                ghostSpriteMaterial = new Material(unlit) { name = "DashGhostSprite" };
            }
        }
    }

    void OnDisable()
    {
        foreach (Ghost g in live)
        {
            g.root.SetActive(false);
            pool.Push(g);
        }
        live.Clear();
    }

    void OnDestroy()
    {
        live.ForEach(pool.Push);
        foreach (Ghost g in pool)
        {
            foreach (Part p in g.parts)
            {
                Destroy(p.mesh);
            }
            Destroy(g.root);
        }
    }
}
