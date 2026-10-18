using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 풀링되는 일회성 파티클 이펙트 (Resources/FX/VfxLibrary.asset).
/// 끝난 인스턴스는 비활성 큐로 돌아가고 다시 꺼낼 때 Clear+Play 하므로 전투 중 Instantiate/Destroy가 없다.
/// 광역 타격 폭주를 막기 위해 프레임당 MaxPerFrame개까지만 생성한다.
/// </summary>
public static class Vfx
{
    const int MaxPerFrame = 6;

    class Live
    {
        public GameObject go;
        public ParticleSystem[] systems;
        public float releaseAt;
        public VfxId id;
        // 도트 모드
        public SpriteRenderer sprite;
        public PixelFxClip clip;
        public float started;
    }

    static Material pixelMaterial;

    static VfxLibrary library;
    static Transform root;
    static readonly Dictionary<VfxId, Stack<Live>> pools = new Dictionary<VfxId, Stack<Live>>();
    static readonly List<Live> active = new List<Live>();
    static int frame = -1, spawnedThisFrame;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        library = null;
        root = null;
        pools.Clear();
        active.Clear();
        frame = -1;
    }

    public static int ActiveCount => active.Count;

    /// <param name="facingLeft">true면 좌우 반전 (Y축 180도)</param>
    public static bool Play(VfxId id, Vector3 position, bool facingLeft = false, float scaleMul = 1f)
    {
        if (id == VfxId.None || !EnsureInit())
        {
            return false;
        }
        if (frame != Time.frameCount)
        {
            frame = Time.frameCount;
            spawnedThisFrame = 0;
        }
        if (spawnedThisFrame >= MaxPerFrame)
        {
            return false;
        }
        VfxLibrary.Entry entry = library.Get(id);
        if (entry == null || entry.prefab == null)
        {
            GameLog.Warn(GameLog.Cat.Combat, "VFX 미등록: " + id);
            return false;
        }
        spawnedThisFrame++;

        if (entry.pixel != null && entry.usePixel && entry.pixel.frames != null && entry.pixel.frames.Length > 0)
        {
            PlayPixel(id, entry, position, facingLeft, scaleMul);
            return true;
        }

        Live live = Rent(id, entry);
        Transform t = live.go.transform;
        t.position = new Vector3(position.x, position.y, 0f);
        t.rotation = (facingLeft ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity) * Quaternion.Euler(entry.euler);
        t.localScale = Vector3.one * entry.scale * scaleMul;
        live.go.SetActive(true);
        foreach (ParticleSystem ps in live.systems)
        {
            ps.Clear(false);
            ps.Play(false);
        }
        live.releaseAt = Time.time + entry.lifetime;
        active.Add(live);
        return true;
    }

    /// <summary>도트 이펙트: SpriteRenderer 1개로 프레임을 넘긴다. 좌우는 flipX, 크기는 베이크 PPU 그대로(scaleMul만)</summary>
    static void PlayPixel(VfxId id, VfxLibrary.Entry entry, Vector3 position, bool facingLeft, float scaleMul)
    {
        Live live;
        if (pools.TryGetValue(id, out Stack<Live> stack) && stack.Count > 0)
        {
            live = stack.Pop();
        }
        else
        {
            var go = new GameObject("PXFX_" + id);
            go.transform.SetParent(root, false);
            var sr = go.AddComponent<SpriteRenderer>();
            if (pixelMaterial == null)
            {
                pixelMaterial = Resources.Load<Material>("FX/M_PixelFx");
            }
            if (pixelMaterial != null)
            {
                sr.sharedMaterial = pixelMaterial; // Unlit — 밤에도 원색
            }
            sr.sortingOrder = entry.sortingOrder;
            live = new Live { go = go, systems = new ParticleSystem[0], id = id, sprite = sr };
            GameLog.Info(GameLog.Cat.Combat, "도트 VFX 풀 생성: " + id);
        }
        live.clip = entry.pixel;
        live.started = Time.time;
        live.sprite.flipX = facingLeft;
        live.sprite.sprite = entry.pixel.frames[0];
        Transform t = live.go.transform;
        t.position = new Vector3(position.x, position.y, 0f);
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one * scaleMul;
        live.go.SetActive(true);
        live.releaseAt = Time.time + entry.pixel.Duration;
        active.Add(live);
    }

    static Live Rent(VfxId id, VfxLibrary.Entry entry)
    {
        if (pools.TryGetValue(id, out Stack<Live> stack) && stack.Count > 0)
        {
            return stack.Pop();
        }
        GameObject go = Object.Instantiate(entry.prefab, root);
        go.name = "VFX_" + id;
        foreach (ParticleSystemRenderer r in go.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.sortingOrder = entry.sortingOrder;
        }
        var systems = go.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy; // 루트 스케일로 전체 크기 조절
            main.playOnAwake = false;
        }
        GameLog.Info(GameLog.Cat.Combat, "VFX 풀 생성: " + id);
        return new Live { go = go, systems = systems, id = id };
    }

    static void Tick()
    {
        float now = Time.time;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Live live = active[i];
            if (live.go == null)
            {
                active.RemoveAt(i);
                continue;
            }
            if (now < live.releaseAt)
            {
                if (live.clip != null)
                {
                    int f = Mathf.Min(live.clip.frames.Length - 1, (int)((now - live.started) * live.clip.fps));
                    live.sprite.sprite = live.clip.frames[f];
                }
                continue;
            }
            live.go.SetActive(false);
            active.RemoveAt(i);
            if (!pools.TryGetValue(live.id, out Stack<Live> stack))
            {
                stack = new Stack<Live>();
                pools[live.id] = stack;
            }
            stack.Push(live);
        }
    }

    static bool EnsureInit()
    {
        if (root != null)
        {
            return library != null;
        }
        library = Resources.Load<VfxLibrary>("FX/VfxLibrary");
        var go = new GameObject("[Vfx]");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<Ticker>();
        root = go.transform;
        if (library == null)
        {
            GameLog.Warn(GameLog.Cat.Combat, "Resources/FX/VfxLibrary 없음");
        }
        return library != null;
    }

    class Ticker : MonoBehaviour
    {
        void Update()
        {
            Tick();
        }
    }
}
