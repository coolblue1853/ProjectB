using UnityEngine;

/// <summary>구역 분위기 입자 종류 (MapInfo.ambience). 정수로 직렬화되므로 새 값은 끝에만 추가한다</summary>
public enum AmbienceKind
{
    None,
    DeepForest,  // 떨어지는 잎 + 떠오르는 포자
    Swamp,       // 바닥 안개 + 늪 포자
    Bandit,      // 떠오르는 불씨
    TowerRunes,  // 청록 룬 조각
}

/// <summary>
/// 구역 분위기 입자 (1층계 폴리싱): 플레이어를 따라가는 화면 폭 띠에서 저비용 파티클 1~2개를 돌린다.
/// AmbientFireflies 와 같은 방식(맵 자식 → 맵이 꺼지면 같이 멈춤, World 시뮬레이션, 파티클 상한 ≤ 60/시스템).
/// 텍스처는 Resources/FX/Px_Ambient(16x16 칸 6x1 격자: 잎A·잎B·포자·불씨·룬·안개, Tools/pixelfx/gen_polish_fx.py)를 _MainTex 로 쓰는 M_AmbientUnlit·M_AmbientLit.
/// </summary>
public class ZoneAmbience : MonoBehaviour
{
    private const int LeafA = 0, LeafB = 1, Spore = 2, Ember = 3, Rune = 4, Fog = 5, Tiles = 6;
    private const float BandWidth = 44f;

    private static Material s_unlit, s_lit;

    private float _yOffset;

    public static void Attach(MapInfo map, AmbienceKind kind)
    {
        if (kind == AmbienceKind.None || map.GetComponentInChildren<ZoneAmbience>(true) != null || !LoadAssets())
        {
            return;
        }
        var go = new GameObject("Ambience_" + kind);
        go.transform.SetParent(map.transform, false);
        var z = go.AddComponent<ZoneAmbience>();
        z.Build(kind);
    }

    private static bool LoadAssets()
    {
        if (s_unlit == null)
        {
            // 두 머티리얼 모두 _MainTex = Px_Ambient 시트(6x1 격자) — build_polish_fx.cs 가 만든다
            s_unlit = Resources.Load<Material>("FX/M_AmbientUnlit");
            s_lit = Resources.Load<Material>("FX/M_AmbientLit");
            if (s_unlit == null || s_lit == null)
            {
                GameLog.Warn(GameLog.Cat.Map, "Resources/FX/M_AmbientUnlit·M_AmbientLit 없음 — 분위기 입자 생략");
                s_unlit = null;
                return false;
            }
        }
        return true;
    }

    private void Build(AmbienceKind kind)
    {
        SetOffset(kind);
        Follow();
        switch (kind)
        {
            case AmbienceKind.DeepForest:
                Falling(Child("Leaves"), new[] { LeafA, LeafB }, 4f, 30);
                Drifting(Child("Spores"), new[] { Spore }, 4f, 30, new Vector2(0.1f, 0.16f), new Vector2(0.05f, 0.35f), new Color(0.6f, 0.9f, 0.5f), 400, false);
                break;
            case AmbienceKind.Swamp:
                FogBank(Child("Fog"), 2.5f, 24);
                Drifting(Child("Motes"), new[] { Spore }, 3f, 24, new Vector2(0.1f, 0.16f), new Vector2(0.05f, 0.25f), new Color(0.75f, 0.9f, 0.5f), 400, false);
                break;
            case AmbienceKind.Bandit:
                Drifting(Child("Embers"), new[] { Ember }, 8f, 50, new Vector2(0.1f, 0.18f), new Vector2(0.5f, 1.3f), new Color(1f, 0.62f, 0.3f), 400, true);
                break;
            case AmbienceKind.TowerRunes:
                Drifting(Child("Runes"), new[] { Rune }, 4f, 30, new Vector2(0.18f, 0.3f), new Vector2(0.1f, 0.45f), new Color(0.7f, 1f, 1f), 400, true);
                break;
        }
    }

    private void SetOffset(AmbienceKind kind)
    {
        _yOffset = kind == AmbienceKind.DeepForest ? 3f : kind == AmbienceKind.Bandit ? 1f : kind == AmbienceKind.TowerRunes ? 2f : 0f;
    }

    private ParticleSystem Child(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }

    private static void Common(ParticleSystem ps, int[] frames, float rate, int max, Material mat, int order, Vector3 box)
    {
        var main = ps.main;
        main.loop = true;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = max;
        main.cullingMode = ParticleSystemCullingMode.Automatic; // 띠가 플레이어를 따라가므로 화면 밖 정지로 충분
        var emission = ps.emission;
        emission.rateOverTime = rate;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = box;
        var sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = Tiles;
        sheet.numTilesY = 1;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.startFrame = new ParticleSystem.MinMaxCurve(frames[0], frames[frames.Length - 1] + 0.999f); // 연속된 칸 중 무작위
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.sortingOrder = order;
    }

    private static Gradient FadeInOut(Color c, float peak)
    {
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(peak, 0.2f), new GradientAlphaKey(peak, 0.75f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    /// <summary>위에서 흔들리며 떨어지는 잎</summary>
    private static void Falling(ParticleSystem ps, int[] frames, float rate, int max)
    {
        Common(ps, frames, rate, max, s_lit, 300, new Vector3(BandWidth, 1f, 0.1f));
        ps.transform.localPosition = new Vector3(0f, 6f, 0f);
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.28f, 0.42f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.6f, 0.2f);
        vel.y = new ParticleSystem.MinMaxCurve(-1.1f, -0.6f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        noise.frequency = 0.4f;
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = FadeInOut(Color.white, 1f);
        ps.Play();
    }

    /// <summary>천천히 떠오르는 작은 입자(포자·불씨·룬). flicker = 깜빡임</summary>
    private static void Drifting(ParticleSystem ps, int[] frames, float rate, int max, Vector2 size, Vector2 rise, Color tint, int order, bool flicker)
    {
        Common(ps, frames, rate, max, s_unlit, order, new Vector3(BandWidth, 9f, 0.1f));
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
        vel.y = new ParticleSystem.MinMaxCurve(rise.x, rise.y);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.5f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        if (flicker)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(tint, 0f), new GradientColorKey(tint, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0.35f, 0.45f), new GradientAlphaKey(0.9f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
        }
        else
        {
            col.color = FadeInOut(tint, 0.85f);
        }
        ps.Play();
    }

    /// <summary>바닥에 깔려 옆으로 흐르는 반투명 안개 덩어리</summary>
    private static void FogBank(ParticleSystem ps, float rate, int max)
    {
        Common(ps, new[] { Fog }, rate, max, s_lit, 350, new Vector3(BandWidth, 1.2f, 0.1f));
        ps.transform.localPosition = new Vector3(0f, -1.2f, 0f);
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 13f);
        main.startSize = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        vel.y = new ParticleSystem.MinMaxCurve(0f, 0.05f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = FadeInOut(new Color(0.82f, 0.9f, 0.78f), 0.18f);
        ps.Play();
    }

    private void Update()
    {
        Follow();
    }

    private void Follow()
    {
        if (PlayerController.instance == null)
        {
            return;
        }
        Vector3 pp = PlayerController.instance.transform.position;
        transform.position = new Vector3(pp.x, pp.y + _yOffset, 0f);
    }
}
