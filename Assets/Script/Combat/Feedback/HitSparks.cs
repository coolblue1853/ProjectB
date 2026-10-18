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
        if (ps == null)
        {
            ps = EmitParticles.Create("[HitSparks]", 1.5f, 300, 200, 0.3f);
        }
        return ps != null;
    }
}
