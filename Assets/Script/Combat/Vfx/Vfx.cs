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
        // 테마 모드(PixelSlash2D): 램프 색 + 수명 끝 디졸브
        public bool themed;
        public MaterialPropertyBlock block;
    }

    static Material pixelMaterial;
    static Material slashMaterial;
    static readonly int[] RampIds = { Shader.PropertyToID("_C0"), Shader.PropertyToID("_C1"), Shader.PropertyToID("_C2"), Shader.PropertyToID("_C3"), Shader.PropertyToID("_C4") };
    static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    const float DissolveFrom = 0.6f; // 수명의 이 비율부터 끝까지 0→1 로 흩어진다

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
    /// <param name="ignoreFrameCap">보스 처치처럼 빠지면 안 되는 연출만 true (프레임당 상한 무시)</param>
    public static bool Play(VfxId id, Vector3 position, bool facingLeft = false, float scaleMul = 1f, bool ignoreFrameCap = false)
    {
        if (id == VfxId.None || !EnsureInit())
        {
            return false;
        }
        // 예전 3D 구움 베기·치명 X자는 노이즈가 많아 상업 기준 미달 → 같은 색 테마의 새 도트 궤적·별로 대체(GameFeelPlan 10.7)
        if (WeaponFx.Enabled && TryUpgradeLegacy(id, out VfxId nid, out PixelRampId ramp, out float mul, out bool flipY))
        {
            return PlayThemed(nid, position, facingLeft, scaleMul * mul, ramp, flipY, ignoreFrameCap);
        }
        if (frame != Time.frameCount)
        {
            frame = Time.frameCount;
            spawnedThisFrame = 0;
        }
        if (spawnedThisFrame >= MaxPerFrame && !ignoreFrameCap)
        {
            return false;
        }
        VfxLibrary.Entry entry = library.Get(id);
        bool hasPixel = entry != null && entry.pixel != null && entry.usePixel;
        if (entry == null || (entry.prefab == null && !hasPixel))
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

    /// <summary>
    /// 회색조 도트 이펙트를 무기 테마 색으로 재생한다(PixelSlash2D 셰이더). flipY = 올려베기처럼 위아래 반전.
    /// </summary>
    public static bool PlayThemed(VfxId id, Vector3 position, bool facingLeft, float scaleMul, PixelRampId ramp, bool flipY = false, bool ignoreFrameCap = false)
    {
        int before = active.Count;
        if (!Play(id, position, facingLeft, scaleMul, ignoreFrameCap) || active.Count == before)
        {
            return false;
        }
        Live live = active[active.Count - 1];
        if (live.sprite == null)
        {
            return true;
        }
        if (slashMaterial == null)
        {
            slashMaterial = Resources.Load<Material>("FX/M_PixelSlash");
        }
        if (slashMaterial != null)
        {
            live.sprite.sharedMaterial = slashMaterial;
        }
        live.sprite.flipY = flipY;
        live.themed = true;
        if (live.block == null)
        {
            live.block = new MaterialPropertyBlock();
        }
        Color[] c = PixelRamp.Get(ramp);
        for (int i = 0; i < RampIds.Length; i++)
        {
            live.block.SetColor(RampIds[i], c[i]);
        }
        live.block.SetFloat(DissolveId, 0f);
        live.sprite.SetPropertyBlock(live.block);
        return true;
    }

    /// <summary>예전 베기 7종·치명 X자 → 새 도트 이펙트(id·색·크기·상하반전)</summary>
    public static bool TryUpgradeLegacy(VfxId id, out VfxId to, out PixelRampId ramp, out float mul, out bool flipY)
    {
        to = VfxId.WSlashSide;
        ramp = PixelRamp.FromSwing(id);
        mul = 1f;
        flipY = false;
        switch (id)
        {
            case VfxId.SlashWhite:
            case VfxId.SlashRed:
            case VfxId.SlashBlue:
                return true;
            case VfxId.SlashGold:
                to = VfxId.WSlashDown;
                return true;
            case VfxId.SlashGoldWide:
                mul = 1.5f;
                return true;
            case VfxId.SlashGreenBig:
                to = VfxId.WSlashDown;
                mul = 1.5f;
                return true;
            case VfxId.CrackRed:
                to = VfxId.WSlashDown;
                flipY = true;
                return true;
            case VfxId.CritCross:
                to = VfxId.WHitStar;
                ramp = PixelRampId.Gold;
                mul = 2f;
                return true;
            default:
                return false;
        }
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
        if (live.themed)
        {
            live.themed = false;
            live.sprite.flipY = false;
            live.sprite.SetPropertyBlock(null);
            if (pixelMaterial != null)
            {
                live.sprite.sharedMaterial = pixelMaterial;
            }
        }
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
                    if (live.themed)
                    {
                        float life = (now - live.started) / Mathf.Max(live.releaseAt - live.started, 0.0001f);
                        live.block.SetFloat(DissolveId, Mathf.Clamp01((life - DissolveFrom) / (1f - DissolveFrom)));
                        live.sprite.SetPropertyBlock(live.block);
                    }
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
