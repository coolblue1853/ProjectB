using UnityEngine;

/// <summary>
/// 적중 스파크: 도트 느낌의 작은 사각 입자. 파티클 시스템 1개(월드 공간)에 위치·색만 바꿔 Emit하므로 개체 생성이 없다.
/// 일반 = 흰 입자 6개, 치명 = 주황 입자 12개(더 빠르고 큼), 처치 = 입자 16개.
/// </summary>
public static class HitSparks
{
    static ParticleSystem ps;
    static int frame = -1, emittedThisFrame;
    const int MaxBurstsPerFrame = 6; // 광역 타격 시 파티클 폭주 방지

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ps = null;
        frame = -1;
    }

    public static void Emit(Vector3 position, bool isCrit, bool killed)
    {
        if (!Ensure())
        {
            return;
        }
        if (frame != Time.frameCount)
        {
            frame = Time.frameCount;
            emittedThisFrame = 0;
        }
        if (++emittedThisFrame > MaxBurstsPerFrame)
        {
            return;
        }
        int count = killed ? 16 : isCrit ? 12 : 6;
        Color color = isCrit ? new Color(1f, 0.6f, 0.15f) : killed ? new Color(1f, 0.9f, 0.6f) : Color.white;
        float speed = isCrit || killed ? 7f : 4.5f;
        float size = isCrit ? 0.2f : 0.14f;
        var ep = new ParticleSystem.EmitParams();
        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            ep.position = position;
            ep.velocity = new Vector3(dir.x, dir.y * 0.7f + 0.3f, 0f) * Random.Range(0.5f, 1f) * speed;
            ep.startColor = color;
            ep.startSize = size * Random.Range(0.7f, 1.2f);
            ep.startLifetime = Random.Range(0.12f, 0.25f);
            ps.Emit(ep, 1);
        }
    }

    static bool Ensure()
    {
        if (ps != null)
        {
            return true;
        }
        var go = new GameObject("[HitSparks]");
        Object.DontDestroyOnLoad(go);
        ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 1.5f;
        main.maxParticles = 300;
        var emission = ps.emission;
        emission.enabled = false;
        var shape = ps.shape;
        shape.enabled = false;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
        var sizeOver = ps.sizeOverLifetime;
        sizeOver.enabled = true;
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 200;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        ps.Play();
        return true;
    }
}
