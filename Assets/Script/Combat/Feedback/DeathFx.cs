using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몬스터 사망 순간 연출 통일 (1층계 폴리싱): 모든 몹 = 연기 퍼프, 정예 = 금빛 폭발 + 강한 흔들림,
/// 보스 = 금빛 폭발 → 몸 곳곳 연기 4회 → 큰 폭발(Impact 흔들림). 시체 디졸브·파편은 DeadBody/DeathShards 가 맡는다.
/// </summary>
public static class DeathFx
{
    private const float BossBurstScale = 2.2f, BossFinalScale = 3f, BossPuffInterval = 0.12f, BossFinalDelay = 0.6f;
    private const int BossPuffs = 4;

    /// <summary>디버그·프로브용 카운터</summary>
    public static int PlayCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        PlayCount = 0;
    }

    public static void Play(EnemyHealth enemy, Bounds bounds)
    {
        if (enemy == null)
        {
            return;
        }
        PlayCount++;
        Vector3 c = bounds.center;
        float size = Mathf.Clamp(bounds.size.y / 2.2f, 0.7f, 2.2f);
        Vfx.Play(VfxId.SmokeGray, c, false, size);
        if (enemy.isBossMob)
        {
            PlayBoss(c, bounds.extents);
        }
        else if (enemy.IsElite)
        {
            Vfx.Play(VfxId.BurstGold, c, false, 1.3f);
            HitFeedback.Shake(HitFeedback.ShakeTier.Heavy);
        }
    }

    private static void PlayBoss(Vector3 c, Vector3 ext)
    {
        Vfx.Play(VfxId.BurstGold, c, false, BossBurstScale, true);
        for (int i = 0; i < BossPuffs; i++)
        {
            Vector3 p = c + new Vector3(Random.Range(-ext.x, ext.x) * 0.8f, Random.Range(-ext.y, ext.y) * 0.8f, 0f);
            DOVirtual.DelayedCall(BossPuffInterval * (i + 1), () => Vfx.Play(VfxId.SmokeGray, p, false, 1.6f), false);
        }
        DOVirtual.DelayedCall(BossFinalDelay, () =>
        {
            Vfx.Play(VfxId.BurstGold, c, false, BossFinalScale, true);
            HitFeedback.Shake(HitFeedback.ShakeTier.Impact);
        }, false);
    }
}
