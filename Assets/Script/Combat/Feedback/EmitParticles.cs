using UnityEngine;

/// <summary>
/// 코드로 만드는 단순 파티클 시스템(월드 공간, 자동 방출 없음). EmitParams로 위치·색·속도를 바꿔 쏘는 일회성 연출용.
/// HitSparks(적중 스파크)·DustPuffs(대시·착지 먼지)가 공유한다.
/// </summary>
public static class EmitParticles
{
    public static ParticleSystem Create(string name, float gravity, int maxParticles, int sortingOrder, float endSize)
    {
        var go = new GameObject(name);
        Object.DontDestroyOnLoad(go);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = gravity;
        main.maxParticles = maxParticles;
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
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, endSize));
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = sortingOrder;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        ps.Play();
        return ps;
    }
}
