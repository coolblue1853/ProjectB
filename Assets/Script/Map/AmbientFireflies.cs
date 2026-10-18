using UnityEngine;

/// <summary>
/// 숲 분위기 연출 (1층계 "요정여왕의 방" 레퍼런스의 반딧불): 밤에만 맵 영역 안에서 노란 빛 입자가 천천히 떠다닌다.
/// MapInfo.fireflies 가 켜진 맵에 MapInfo 가 자동 부착. 파티클 1개, 밤 전환은 TimeChange.isDaytime 을 폴링(0.5초).
/// </summary>
public class AmbientFireflies : MonoBehaviour
{
    ParticleSystem ps;
    float areaY;
    float nextCheck;
    bool on;

    public static void Attach(MapInfo map, Bounds area)
    {
        if (map.GetComponentInChildren<AmbientFireflies>() != null)
        {
            return;
        }
        var go = new GameObject("Fireflies");
        go.transform.SetParent(map.transform, false);
        go.transform.position = area.center;
        var f = go.AddComponent<AmbientFireflies>();
        f.Build(area);
    }

    void Build(Bounds area)
    {
        ps = gameObject.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        main.startColor = new Color(1f, 0.95f, 0.45f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 120;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate; // 방출 중심이 화면 밖이어도 멈추지 않게
        var emission = ps.emission;
        emission.rateOverTime = 14f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(44f, Mathf.Max(4f, area.size.y * 0.6f), 0.1f); // 화면 폭 정도, 플레이어를 따라감
        areaY = area.center.y;
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        vel.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.35f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.3f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }); // 깜빡임
        col.color = grad;
        var r = GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = Resources.Load<Material>("FX/M_PixelFx");
        r.sortingOrder = 60;
    }

    void Update()
    {
        if (PlayerController.instance != null)
        {
            transform.position = new Vector3(PlayerController.instance.transform.position.x, areaY, 0f);
        }
        if (Time.time < nextCheck)
        {
            return;
        }
        nextCheck = Time.time + 0.5f;
        bool night = TimeChange.instance != null && !TimeChange.instance.isDaytime;
        if (night == on)
        {
            return;
        }
        on = night;
        if (on)
        {
            ps.Play();
        }
        else
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
