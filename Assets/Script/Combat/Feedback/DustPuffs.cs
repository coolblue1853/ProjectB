using UnityEngine;

/// <summary>
/// 발밑 먼지: 대시 시작(뒤쪽으로 퍼짐)·착지(양옆으로 퍼짐). 흙색 입자가 천천히 떠오르며 사라진다.
/// HitSparks와 같은 방식으로 파티클 시스템 1개에 Emit만 한다.
/// </summary>
public static class DustPuffs
{
    static ParticleSystem ps;
    static readonly Color DustColor = new Color(0.82f, 0.76f, 0.66f, 0.85f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ps = null;
    }

    /// <param name="facing">대시 방향(+1 오른쪽, -1 왼쪽). 먼지는 반대쪽으로 퍼진다</param>
    public static void Dash(Vector3 feet, float facing)
    {
        Emit(feet, -Mathf.Sign(facing), 0f, 8, 3.5f);
    }

    /// <param name="strength">0~1 (낙하 속도에 비례)</param>
    public static void Land(Vector3 feet, float strength)
    {
        int count = Mathf.RoundToInt(Mathf.Lerp(4f, 12f, Mathf.Clamp01(strength)));
        Emit(feet, -1f, 0f, count / 2, 2.5f);
        Emit(feet, 1f, 0f, count / 2, 2.5f);
    }

    static void Emit(Vector3 feet, float dirX, float dirY, int count, float speed)
    {
        if (ps == null)
        {
            ps = EmitParticles.Create("[DustPuffs]", -0.05f, 200, 150, 1.4f);
        }
        var ep = new ParticleSystem.EmitParams();
        for (int i = 0; i < count; i++)
        {
            ep.position = feet + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0f, 0.1f), 0f);
            ep.velocity = new Vector3(dirX * Random.Range(0.4f, 1f) * speed, Random.Range(0.2f, 0.9f) + dirY, 0f);
            ep.startColor = DustColor;
            ep.startSize = Random.Range(0.07f, 0.13f);
            ep.startLifetime = Random.Range(0.3f, 0.5f);
            ps.Emit(ep, 1);
        }
    }
}
