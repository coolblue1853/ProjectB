using UnityEngine;

/// <summary>
/// 희귀 드롭 빛 기둥 (디아블로식): R = 파랑, U = 금색. 바닥에서 위로 옅어지는 세로 빛 + 떠오르는 작은 입자.
/// 드롭 아이템은 풀링되므로 SetRarity로 켜고 끈다. 텍스처·머티리얼은 전체 공유.
/// </summary>
public class DropLightPillar : MonoBehaviour
{
    const float Height = 3.2f, Width = 0.45f;

    static Sprite beamSprite;
    static Material additive, alphaBlend;

    SpriteRenderer beam;
    ParticleSystem motes;
    float baseAlpha;

    public static void Apply(GameObject drop, string rarity)
    {
        bool show = rarity == "R" || rarity == "U";
        var pillar = drop.GetComponentInChildren<DropLightPillar>(true);
        if (!show)
        {
            if (pillar != null)
            {
                pillar.gameObject.SetActive(false);
            }
            return;
        }
        if (pillar == null)
        {
            var go = new GameObject("LightPillar");
            go.transform.SetParent(drop.transform, false);
            pillar = go.AddComponent<DropLightPillar>();
            pillar.Build();
        }
        pillar.SetColor(rarity == "U" ? new Color(1f, 0.78f, 0.25f) : new Color(0.35f, 0.65f, 1f), rarity == "U" ? 0.75f : 0.6f);
        pillar.gameObject.SetActive(true);
        FeelFx.RareDrop(drop.transform, rarity);
    }

    void Build()
    {
        EnsureShared();
        Vector3 ls = transform.parent.lossyScale;
        transform.localScale = new Vector3(1f / Mathf.Max(0.01f, Mathf.Abs(ls.x)), 1f / Mathf.Max(0.01f, Mathf.Abs(ls.y)), 1f);

        beam = gameObject.AddComponent<SpriteRenderer>();
        beam.sprite = beamSprite;
        beam.sharedMaterial = alphaBlend; // 가산은 밝은 배경(숲)에서 묻혀서 일반 알파로
        beam.drawMode = SpriteDrawMode.Sliced;
        beam.size = new Vector2(Width, Height);
        var itemSr = transform.parent.GetComponent<SpriteRenderer>();
        beam.sortingLayerID = itemSr != null ? itemSr.sortingLayerID : 0;
        beam.sortingOrder = (itemSr != null ? itemSr.sortingOrder : 0) - 1; // 아이템 바로 뒤

        var moteGo = new GameObject("Motes");
        moteGo.transform.SetParent(transform, false);
        motes = moteGo.AddComponent<ParticleSystem>();
        motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = motes.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.3f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 24;
        var emission = motes.emission;
        emission.rateOverTime = 10f;
        var shape = motes.shape;
        shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
        shape.radius = Width * 0.6f;
        var vel = motes.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.y = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var col = motes.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
        var r = moteGo.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = additive;
        r.sortingLayerID = beam.sortingLayerID;
        r.sortingOrder = beam.sortingOrder + 2;
    }

    void SetColor(Color c, float alpha)
    {
        baseAlpha = alpha;
        beam.color = new Color(c.r, c.g, c.b, alpha);
        var main = motes.main;
        main.startColor = c;
    }

    void OnEnable()
    {
        if (motes != null)
        {
            motes.Play();
        }
    }

    void Update()
    {
        // 은은한 맥동 (개체별 위상)
        float a = baseAlpha * (0.8f + 0.2f * Mathf.Sin(Time.time * 3f + transform.position.x));
        Color c = beam.color;
        beam.color = new Color(c.r, c.g, c.b, a);
    }

    static void EnsureShared()
    {
        if (beamSprite != null)
        {
            return;
        }
        // 4x32 세로 그라데이션: 아래 진하고 위로 사라짐, 좌우 가장자리 옅게
        var tex = new Texture2D(4, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        for (int y = 0; y < 32; y++)
        {
            float v = 1f - y / 31f;
            for (int x = 0; x < 4; x++)
            {
                float edge = x == 0 || x == 3 ? 0.35f : 1f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, v * v * edge));
            }
        }
        tex.Apply();
        beamSprite = Sprite.Create(tex, new Rect(0, 0, 4, 32), new Vector2(0.5f, 0f), 16f, 0, SpriteMeshType.FullRect, new Vector4(1, 1, 1, 1));
        alphaBlend = new Material(Shader.Find("Sprites/Default"));
        Shader add = Shader.Find("Legacy Shaders/Particles/Additive");
        additive = add != null ? new Material(add) : alphaBlend;
    }
}
